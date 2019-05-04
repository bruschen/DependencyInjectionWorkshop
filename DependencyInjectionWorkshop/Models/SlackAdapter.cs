using SlackAPI;

namespace DependencyInjectionWorkshop.Models
{
    public interface INotifyAdapter
    {
        void Notify();
    }

    public class SlackAdapter : INotifyAdapter
    {
        public void Notify()
        {
            var slackClient = new SlackClient("my api token");
            slackClient.PostMessage(response1 => { }, "my channel", "my message", "my bot name");
        }
    }
}