using Akka.Actor;
using CoctailServer.Services;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace CoctailServer
{
    public class RxWorker
    {
        private readonly CocktailService _cocktailService;
        private readonly IActorRef _ingredientActor;

        private readonly HashSet<string> _trackedLetters = new(); //pamti koja slova treba periodicno da osvezava
        private readonly object _lock = new();

        public RxWorker(CocktailService cocktailService, IActorRef ingredientActor, IActorRef wordCloudActor)
        {
            _cocktailService = cocktailService;
            _ingredientActor = ingredientActor;

        }

        public async Task TrackLetterAsync(string letter) //metoda koju poziva server
        {
            letter = Helper.NormalizeLetter(letter);

            bool isNewLetter = false;

            lock (_lock) //zakljuca HashSet jer u jednom trenutku samo jedna nit moze da koristi HAshSet
            {
                if (!_trackedLetters.Contains(letter))
                {
                    _trackedLetters.Add(letter);
                    isNewLetter = true;
                }
            }

            Log.Info($"Rx prati slovo '{letter}'.");

            if (isNewLetter)
            {
                await ProcessLetterAsync(letter); //ako je slovo novo, odmah zove api, ako nije prvi put, ne radi nista jer ce kansije periodicna obrada sama osvezavati
            }
        }

        public void StartPeriodicProcessing() //na svakih 10sec izvrsava await ProcessLetterAsync(letter);

        {
            Observable
                .Interval(TimeSpan.FromSeconds(10))
                .SubscribeOn(TaskPoolScheduler.Default)
                .Subscribe(async _ =>
                {
                    List<string> letters;

                    lock (_lock)
                    {
                        letters = _trackedLetters.ToList();
                    }

                    foreach (var letter in letters)
                    {
                        var result =
                            await _ingredientActor.Ask<ExpirationResult>(
                                new CheckExpiration(letter),
                                TimeSpan.FromSeconds(2)
                            );

                        if (result.IsExpired)
                        {
                            Log.Info($"Podaci za '{letter}' su istekli.");

                            await ProcessLetterAsync(letter);
                        }
                        else
                        {
                            Log.Info($"Podaci za '{letter}' su jos uvek validni.");
                        }
                    }
                });

            Log.Success("Rx periodicna obrada je pokrenuta.");
        }

        private async Task ProcessLetterAsync(string letter)
        {
            letter = Helper.NormalizeLetter(letter);

            Log.Info($"Rx poziva eksterni API za slovo '{letter}'.");

            var cocktails = await Observable
                .FromAsync(() => _cocktailService.GetCocktailsByFirstLetterAsync(letter)) //poziva metodu iz servisa
                .Select(cocktails => cocktails
                    .Where(c => !string.IsNullOrWhiteSpace(c.Name))
                    .Select(c => new CocktailInfo(c.Name, c.GetIngredients()))
                    .ToList()
                ); //transformise podatke
            Log.Info($"Saljem {cocktails.Count} koktela za slovo '{letter}' IngredientActor-u.");
            await _ingredientActor.Ask<ReplaceCompleted>(
             new ReplaceCocktailsForLetter(letter, cocktails),
             TimeSpan.FromSeconds(5)
            ); //posalje aktoru slovo i koktele i ceka na odgovor

            Log.Success($"Rx obrada zavrsena za slovo '{letter}'.");
        }
    }
}