using Newtonsoft.Json;

namespace CoctailServer.Models
{
    public class CocktailResponse
    {
        [JsonProperty("drinks")]
        public List<Cocktail>? Drinks { get; set; }
    }
}