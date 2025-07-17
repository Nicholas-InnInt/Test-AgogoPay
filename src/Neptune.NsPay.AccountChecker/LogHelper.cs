namespace Neptune.NsPay.AccountChecker
{
    public class LogHelper
    {
        private readonly ILogger<LogHelper> _logger;

        // Constructor that accepts an ILogger instance
        public LogHelper(ILogger<LogHelper> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Logs an informational message
        public void LogInformation(string message)
        {
            _logger.LogInformation(message);
        }

        // Logs a warning message
        public void LogWarning(string message)
        {
            _logger.LogWarning(message);
        }

        // Logs an error message with exception details
        public void LogError(string message, Exception ex)
        {
            _logger.LogError(ex, message);
        }

        // Logs a critical error message
        public void LogCritical(string message)
        {
            _logger.LogCritical(message);
        }

        // Logs a debug message (useful for troubleshooting)
        public void LogDebug(string message)
        {
            _logger.LogDebug(message);
        }

        // Logs a trace message (the most verbose level of logging)
        public void LogTrace(string message)
        {
            _logger.LogTrace(message);
        }
    }
}