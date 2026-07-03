using System.Collections.Generic;
namespace CoctailServer
{
    public record IngredientFound(string Letter, string Ingredient);

    public record UpdateWordCloud(
        string Letter,
        Dictionary<string, int> Frequencies,
        List<CocktailInfo> Cocktails,
        DateTime LastUpdated,
        DateTime ExpiresAt
    );
    public record GetWordCloud(string Letter);

    public record CocktailInfo(string Name, List<string> Ingredients);

    public record WordCloudResponse(
        string Letter,
        Dictionary<string, int> Frequencies,
        List<CocktailInfo> Cocktails,
        DateTime? LastUpdated,
        DateTime? ExpiresAt,
        bool IsExpired,
        string Message
    );
    public record CocktailFound(string Letter, string CocktailName, List<string> Ingredients);
}