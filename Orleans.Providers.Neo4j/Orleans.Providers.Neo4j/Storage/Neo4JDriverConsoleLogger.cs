using Neo4j.Driver;

namespace Orleans.Providers.Neo4j.Storage
{
    internal class Neo4JDriverConsoleLogger : ILogger
    {
        public void Error(Exception cause, string message)
        {
            Console.WriteLine($"ERROR: {message}, Exception: {cause.Message}");
        }

        public void Info(string message, params object[] parameters)
        {
            Console.WriteLine($"INFO: {string.Format(message, parameters)}");
        }

        public void Warn(string message, params object[] parameters)
        {
            Console.WriteLine($"WARN: {string.Format(message, parameters)}");
        }

        public void Debug(string message, params object[] parameters)
        {
            Console.WriteLine($"DEBUG: {string.Format(message, parameters)}");
        }

        public bool IsDebugEnabled() => true;
        public bool IsTraceEnabled() => true;

        public void Error(Exception cause, string message, params object[] args)
        {
            Console.WriteLine($"ERROR: {message}, Exception: {cause.Message}");
        }

        public void Warn(Exception cause, string message, params object[] args)
        {
            Console.WriteLine($"WARN: {message}, Exception: {cause.Message}");
        }

        public void Trace(string message, params object[] args)
        {
            Console.WriteLine($"TRACE: {message}");
        }
    }
}