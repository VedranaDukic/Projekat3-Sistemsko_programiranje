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

        public CacheEntry AddOrUpdate(string letter, string ingredient)
        {
            letter = Helper.NormalizeLetter(letter);
            ingredient = Helper.NormalizeIngredient(ingredient);

            if (!_entries.ContainsKey(letter) || _entries[letter].IsExpired())
            {
                _entries[letter] = new CacheEntry(letter, _durationMinutes);
            }

            var entry = _entries[letter];

            if (!entry.Frequencies.ContainsKey(ingredient))
            {
                entry.Frequencies[ingredient] = 0;
            }

            entry.Frequencies[ingredient]++;
            entry.RefreshExpiration(_durationMinutes);

            return entry;
        }

        public CacheEntry? Get(string letter)
        {
            letter = Helper.NormalizeLetter(letter);

            if (!_entries.ContainsKey(letter))
            {
                return null;
            }

            return _entries[letter];
        }
        public void AddCocktail(string letter, string cocktailName, List<string> ingredients)
        {
            letter = Helper.NormalizeLetter(letter);

            if (!_entries.ContainsKey(letter) || _entries[letter].IsExpired())
            {
                _entries[letter] = new CacheEntry(letter, _durationMinutes);
            }

            var entry = _entries[letter];

            if (!entry.Cocktails.Any(c => c.Name == cocktailName))
            {
                entry.Cocktails.Add(new CocktailInfo(cocktailName, ingredients));
            }

            entry.RefreshExpiration(_durationMinutes);
        }
    }
}