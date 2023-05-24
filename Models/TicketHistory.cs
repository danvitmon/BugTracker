using System.ComponentModel.DataAnnotations;

namespace BugTracker.Models
{
    public class TicketHistory
    {
        public int Id { get; set; }

        [Display(Name = "Property Name")]
        [StringLength(300, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)]
        public string? PropertyName { get; set; }

        [Display(Name = "Description")]
        [StringLength(800, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)]
        public string? Description { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime Created { get; set; }

        public string? OldValue { get; set; }

        public string? NewValue { get; set; }

        public int TicketId { get; set; }

        [Required]
        public string? UserId { get; set; }

        public virtual Ticket? Ticket { get; set; }

        public virtual BTUser? User { get; set; }
    }
}