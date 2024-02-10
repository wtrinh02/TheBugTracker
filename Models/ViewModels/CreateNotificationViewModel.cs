using Microsoft.AspNetCore.Mvc.Rendering;

namespace TheBugTracker.Models.ViewModels
{
    public class CreateNotificationViewModel
    {
        public Notification Notification { get; set; }
        public SelectList RecipientList { get; set; }
    }
}
