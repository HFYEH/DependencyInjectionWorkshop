namespace DependencyInjectionWorkshop.Models
{
    public interface ILogger
    {
        void Info(string message);
    }

    public class Logger : ILogger
    {
        public void Info(string message)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info(message);
        }
    }
}