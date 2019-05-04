namespace DependencyInjectionWorkshop.Models
{
    public interface ILogAdapter
    {
        void LogMessage(string message);
    }

    public class NLogAdapter : ILogAdapter
    {
        public void LogMessage(string message)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info(message);
        }
    }
}