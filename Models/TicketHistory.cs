using System.ComponentModel.DataAnnotations;

namespace BugTracker.Models
{
    public class TicketHistory
    {
        public int Id { get; set; }

        public string? PropertyName { get; set; }

        public string? Description { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? Created { get; set; }

        public string? OldValue { get; set; }

        public string? NewValue { get; set; }

        public int TicketId { get; set; }

        [Required]
        public string? UserId { get; set; }

        public virtual Ticket? Ticket { get; set; }

        public virtual User? User { get; set; }
    }
}