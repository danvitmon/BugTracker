using System.ComponentModel.DataAnnotations;

namespace BugTracker.Models
{
    public class Notification
    {
        // Primary Key
        public int Id { get; set; }

        // Foreign Keys
        public int ProjectId { get; set; }

        public int TicketId { get; set; }

        [Required]
        public string? SenderId { get; set; }

        [Required]
        public string? RecipientId { get; set; }

        public int NotificationTypeId { get; set; }

        [Required]
        public string? Title { get; set; }

        [Required]
        public string? Message { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? Created { get; set; }

        public bool HasBeenViewed { get; set; }

        // Navigation Properties

        public virtual NotificationType? NotificationType { get; set; }

        public virtual Ticket? Ticket { get; set; }

        public virtual Project? Project { get; set; }

        public virtual BTUser? Sender { get; set; }

        public virtual BTUser? Recipient { get; set; }

    }
}
