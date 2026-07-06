using CoctailServer.Models;

namespace CoctailServer
{
    public class Cache
    {
        private readonly Dictionary<string, CacheEntry> _entries = new();
        private readonly int _durationMinutes;

        public Cache(int durationMinutes)
        {
            _durationMinutes = durationMinutes;
        }

        public CacheEntry ReplaceCocktails(
            string letter,
            List<CocktailInfo> cocktails
        )
        {
            letter = Helper.NormalizeLetter(letter);

            var entry = new CacheEntry(letter, _durationMinutes);

            entry.Cocktails = cocktails;

            foreach (var cocktail in cocktails)
            {
                foreach (var ingredient in cocktail.Ingredients)
                {
                    if (string.IsNullOrWhiteSpace(ingredient))
                        continue;

                    var key = Helper.NormalizeIngredient(ingredient);

                    if (!entry.Frequencies.ContainsKey(key))
                        entry.Frequencies[key] = 0;

                    entry.Frequencies[key]++;
                }
            }

            entry.RefreshExpiration(_durationMinutes);

            _entries[letter] = entry;

            return entry;
        }

        public CacheEntry? Get(string letter)
        {
            letter = Helper.NormalizeLetter(letter);

            if (!_entries.ContainsKey(letter))
                return null;

            return _entries[letter];
        }

        public bool IsExpired(string letter)
        {
            var entry = Get(letter);

            if (entry == null)
                return true;

            return entry.IsExpired();
        }
    }
}