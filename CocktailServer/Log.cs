namespace CoctailServer
{
    public static class Log
    {
        private static readonly object LockObject = new object();

        public static void Info(string message)
        {
            Write("INFO", message);
        }

        public static void Success(string message)
        {
            Write("SUCCESS", message);
        }

        public static void Error(string message)
        {
            Write("ERROR", message);
        }

        private static void Write(string type, string message)
        {
            lock (LockObject)
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [{type}] {message}");
            }
        }
    }
}