namespace CoctailServer.Models
{
    public class CacheEntry
    {
        public string Letter { get; set; }
        public Dictionary<string, int> Frequencies { get; set; }
        public DateTime LastUpdated { get; set; }
        public DateTime ExpiresAt { get; set; }
        public List<CocktailInfo> Cocktails { get; set; }

        public CacheEntry(string letter, int durationMinutes)
        {
            Letter = letter;
            Frequencies = new Dictionary<string, int>();
            LastUpdated = DateTime.Now;
            ExpiresAt = DateTime.Now.AddMinutes(durationMinutes);
            Cocktails = new List<CocktailInfo>();
        }

        public bool IsExpired()
        {
            return DateTime.Now > ExpiresAt;
        }

        public void RefreshExpiration(int durationMinutes)
        {
            LastUpdated = DateTime.Now;
            ExpiresAt = DateTime.Now.AddMinutes(durationMinutes);
        }
    }
}