using Akka.Actor;
using System.Net;
using System.Text;

namespace CoctailServer
{
    public class Server
    {
        private readonly HttpListener _listener;
        private readonly IActorRef _wordCloudActor;
        private readonly RxWorker _rxWorker;

        public Server(string url, IActorRef wordCloudActor, RxWorker rxWorker)
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add(url);
            _wordCloudActor = wordCloudActor;
            _rxWorker = rxWorker;
        }

        public async Task StartAsync()
        {
            _listener.Start();
            Log.Success("Server je pokrenut na http://localhost:8080/");

            while (true)
            {
                var context = await _listener.GetContextAsync();
                _ = Task.Run(() => HandleRequestAsync(context));
            }
        }

        private async Task HandleRequestAsync(HttpListenerContext context)
        {
            try
            {
                string? letter = context.Request.QueryString["letter"];

                Log.Info($"Primljen zahtev: {context.Request.RawUrl}");

                if (!Helper.IsValidLetter(letter))
                {
                    await SendResponse(context, "<h1>Greska</h1><p>Parametar letter mora biti jedno slovo.</p>", 400);
                    return;
                }

                letter = Helper.NormalizeLetter(letter);

                await _rxWorker.ProcessLetterAsync(letter);
                await Task.Delay(300);

                var response = await _wordCloudActor.Ask<WordCloudResponse>(
                    new GetWordCloud(letter),
                    TimeSpan.FromSeconds(5)
                );

                string html = Helper.GenerateHtml(letter, response);

                await SendResponse(context, html, 200);

                Log.Success($"Zahtev za slovo '{letter}' uspesno obradjen.");
            }
            catch (Exception ex)
            {
                Log.Error($"Greska pri obradi zahteva: {ex.Message}");
                await SendResponse(context, "<h1>Greska na serveru</h1>", 500);
            }
        }

        private async Task SendResponse(HttpListenerContext context, string content, int statusCode)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(content);

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "text/html; charset=utf-8";
            context.Response.ContentLength64 = buffer.Length;

            await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            context.Response.OutputStream.Close();
        }
    }
}