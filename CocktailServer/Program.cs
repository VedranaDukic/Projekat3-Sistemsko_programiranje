using Akka.Actor;
using CoctailServer;
using CoctailServer.Actors;
using CoctailServer.Services;

Console.OutputEncoding = System.Text.Encoding.UTF8;

Log.Info("Pokretanje aplikacije...");

var actorSystem = ActorSystem.Create("CocktailActorSystem");

var cache = new Cache(durationMinutes: 5);

var wordCloudActor = actorSystem.ActorOf(
    Props.Create(() => new WordCloudActor()),
    "wordCloudActor"
);

var ingredientActor = actorSystem.ActorOf(
    Props.Create(() => new IngredientActor(cache, wordCloudActor)),
    "ingredientActor"
);

var cocktailService = new CocktailService();

var rxWorker =
    new RxWorker(
        cocktailService,
        ingredientActor,
        wordCloudActor
    );

rxWorker.StartPeriodicProcessing();

var server = new Server("http://localhost:8080/", wordCloudActor, rxWorker);

Log.Success("Aplikacija je spremna.");
Log.Info("Otvori u browseru: http://localhost:8080/wordcloud?letter=a");

await server.StartAsync();

await actorSystem.Terminate();