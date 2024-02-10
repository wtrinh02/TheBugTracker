namespace TheBugTracker.Models.ViewModels
{
    public class InboxViewModel
    {
        public List<Notification> ReceivedNotifications { get; set; }

        public List<Notification> SentNotifications { get; set; }
    }
}
