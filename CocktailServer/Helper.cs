using System.Text;

namespace CoctailServer
{
    public static class Helper
    {
        public static bool IsValidLetter(string? letter)
        {
            return !string.IsNullOrWhiteSpace(letter)
                   && letter.Length == 1
                   && char.IsLetter(letter[0]);
        }

        public static string NormalizeLetter(string letter)
        {
            return letter.Trim().ToLower();
        }

        public static string NormalizeIngredient(string ingredient)
        {
            return ingredient.Trim().ToLower();
        }

        public static string GenerateHtml(string letter, WordCloudResponse response)
        {
            var sb = new StringBuilder();

            sb.Append("<html><head><meta charset='UTF-8'><title>Word Cloud</title>");
            sb.Append("<style>");
            sb.Append("body{font-family:Arial;padding:30px;background:#fafafa;color:#222;}");
            sb.Append(".box{background:white;padding:20px;margin-bottom:25px;border-radius:10px;box-shadow:0 2px 8px #ddd;}");
            sb.Append(".cloud span{display:inline-block;margin:12px;}");
            sb.Append("table{border-collapse:collapse;width:100%;}");
            sb.Append("td,th{border-bottom:1px solid #ddd;padding:8px;text-align:left;}");
            sb.Append(".cocktail{margin-bottom:12px;}");
            sb.Append("</style></head><body>");

            sb.Append($"<h1>Word Cloud za slovo: {letter.ToUpper()}</h1>");
            sb.Append($"<p>{response.Message}</p>");

            sb.Append("<div class='box cloud'>");
            sb.Append("<h2>Word Cloud sastojaka</h2>");

            if (response.Frequencies.Count == 0)
            {
                sb.Append("<p>Nema podataka za prikaz.</p>");
            }
            else
            {
                int max = response.Frequencies.Values.Max();

                foreach (var item in response.Frequencies.OrderByDescending(x => x.Value))
                {
                    int fontSize = 14 + (int)(40.0 * item.Value / max);
                    sb.Append($"<span style='font-size:{fontSize}px'>{item.Key.ToUpper()} ({item.Value})</span>");
                }
            }

            sb.Append("</div>");

            sb.Append("<div class='box'>");
            sb.Append($"<h2>Ukupno koktela: {response.Cocktails.Count}</h2>");

            foreach (var cocktail in response.Cocktails.OrderBy(c => c.Name))
            {
                sb.Append("<div class='cocktail'>");
                sb.Append($"<b>{cocktail.Name}</b><br>");
                sb.Append(string.Join(", ", cocktail.Ingredients));
                sb.Append("</div>");
            }

            sb.Append("</div>");

            sb.Append("<div class='box'>");
            sb.Append("<h2>Statistika frekvencija</h2>");
            sb.Append("<table><tr><th>Sastojak</th><th>Broj pojavljivanja</th></tr>");

            foreach (var item in response.Frequencies.OrderByDescending(x => x.Value))
            {
                sb.Append($"<tr><td>{item.Key}</td><td>{item.Value}</td></tr>");
            }

            sb.Append("</table></div>");

            sb.Append("</body></html>");

            return sb.ToString();
        }
    }
}