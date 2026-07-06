using Akka.Actor;

namespace CoctailServer.Actors
{
    public class WordCloudActor : ReceiveActor
    {
        private readonly Dictionary<string, WordCloudResponse> _wordClouds = new();

        public WordCloudActor()
        {
            Receive<UpdateWordCloud>(message =>
            {
                Log.Info($"WordCloudActor primio UpdateWordCloud za '{message.Letter}'.");

                _wordClouds[message.Letter] = new WordCloudResponse(
                    message.Letter,
                    message.Frequencies,
                    message.Cocktails,
                    message.LastUpdated,
                    message.ExpiresAt,
                    DateTime.Now > message.ExpiresAt,
                    "Podaci su uspešno ažurirani."
                );
            });

            Receive<GetWordCloud>(message =>
            {
                string letter = Helper.NormalizeLetter(message.Letter);

                if (!_wordClouds.TryGetValue(letter, out var response))
                {
                    Sender.Tell(new WordCloudResponse(
                        letter,
                        new Dictionary<string, int>(),
                        new List<CocktailInfo>(),
                        null,
                        null,
                        true,
                        "Za traženo slovo trenutno nema podataka."
                    ));
                    return;
                }

                bool isExpired = response.ExpiresAt.HasValue &&
                                 DateTime.Now > response.ExpiresAt.Value;

                Sender.Tell(response with
                {
                    IsExpired = isExpired,
                    Message = isExpired
                        ? "Podaci u kešu su istekli."
                        : "Podaci su validni."
                });
            });
        }
    }
}