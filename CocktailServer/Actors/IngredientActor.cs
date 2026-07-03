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

            Receive<CocktailFound>(message =>
            {
                _cache.AddCocktail(
                    message.Letter,
                    message.CocktailName,
                    message.Ingredients
                );
            });

            Receive<IngredientFound>(message =>
            {
                var entry = _cache.AddOrUpdate(message.Letter, message.Ingredient);
                var current = _cache.Get(message.Letter);

                _wordCloudActor.Tell(new UpdateWordCloud(
                    entry.Letter,
                    new Dictionary<string, int>(entry.Frequencies),
                    current?.Cocktails ?? new List<CocktailInfo>(),
                    entry.LastUpdated,
                    entry.ExpiresAt
                ));
            });
        }
    }
}