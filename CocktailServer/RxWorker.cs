using Akka.Actor;
using CoctailServer.Services;
using System.Reactive.Linq;

namespace CoctailServer
{
    public class RxWorker
    {
        private readonly CocktailService _cocktailService;
        private readonly IActorRef _ingredientActor;

        public RxWorker(CocktailService cocktailService, IActorRef ingredientActor)
        {
            _cocktailService = cocktailService;
            _ingredientActor = ingredientActor;
        }

        public async Task ProcessLetterAsync(string letter)
        {
            letter = Helper.NormalizeLetter(letter);

            Log.Info($"Rx obrada pokrenuta za slovo '{letter}'.");

            await Observable
                .FromAsync(() => _cocktailService.GetCocktailsByFirstLetterAsync(letter))
                .SelectMany(cocktails => cocktails)
                .Do(cocktail =>
                {
                    var ingredients = cocktail.GetIngredients();

                    if (!string.IsNullOrWhiteSpace(cocktail.Name))
                    {
                        _ingredientActor.Tell(
                            new CocktailFound(letter, cocktail.Name, ingredients)
                        );
                    }

                    foreach (var ingredient in ingredients)
                    {
                        _ingredientActor.Tell(
                            new IngredientFound(letter, ingredient)
                        );
                    }
                })
                .LastOrDefaultAsync();

            Log.Success($"Rx obrada zavrsena za slovo '{letter}'.");
        }
    }
}