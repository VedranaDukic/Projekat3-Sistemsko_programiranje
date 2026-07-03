using CoctailServer.Models;
using Newtonsoft.Json;

namespace CoctailServer.Services
{
    public class CocktailService
    {
        private readonly HttpClient _httpClient;

        public CocktailService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<List<Cocktail>> GetCocktailsByFirstLetterAsync(string letter)
        {
            letter = Helper.NormalizeLetter(letter);

            string url = $"https://www.thecocktaildb.com/api/json/v1/1/search.php?f={letter}";

            Log.Info($"Poziv eksternog API-ja: {url}");

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                Log.Error($"API greska: {response.StatusCode}");
                return new List<Cocktail>();
            }

            var json = await response.Content.ReadAsStringAsync();

            var cocktailResponse = JsonConvert.DeserializeObject<CocktailResponse>(json);

            return cocktailResponse?.Drinks ?? new List<Cocktail>();
        }
    }
}