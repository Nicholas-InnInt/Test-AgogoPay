namespace Neptune.NsPay.Web.RabbitMQApi
{
    public static class ExtensionClass
    {
        public static void FireAndForgetSafeAsync(this Task task, Action<Exception>? errorHandler = null)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await task;
                }
                catch (Exception ex)
                {
                    errorHandler?.Invoke(ex);
                    // Optional: log the exception
                }
            });
        }
    }
}