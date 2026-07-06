using Akka.Actor;

namespace CoctailServer.Actors
{
    public class IngredientActor : ReceiveActor
    {
        private readonly Cache _cache;
        private readonly IActorRef _wordCloudActor;

        public IngredientActor(Cache cache, IActorRef wordCloudActor)
        {
            _cache = cache;
            _wordCloudActor = wordCloudActor;

            Receive<ReplaceCocktailsForLetter>(message =>
            {
                Log.Info($"IngredientActor primio {message.Cocktails.Count} koktela za '{message.Letter}'.");

                var entry = _cache.ReplaceCocktails(
                    message.Letter,
                    message.Cocktails
                );

                Log.Info($"WordCloud se azurira za '{message.Letter}'.");

                _wordCloudActor.Tell(new UpdateWordCloud(
                    entry.Letter,
                    new Dictionary<string, int>(entry.Frequencies),
                    new List<CocktailInfo>(entry.Cocktails),
                    entry.LastUpdated,
                    entry.ExpiresAt
                ));

                Sender.Tell(new ReplaceCompleted(entry.Letter));
            });
            Receive<CheckExpiration>(message =>
            {
                bool expired = _cache.IsExpired(message.Letter);

                Sender.Tell(new ExpirationResult(expired));
            });
        }
    }
}