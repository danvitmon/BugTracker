using System.ComponentModel.DataAnnotations;
using System.Net.Mail;

namespace BugTracker.Models
{
    public class Ticket
    {
        // Primary Key
        public int Id { get; set; }

        [Required]
        public string? Title { get; set; }

        [Required]
        public string? Description { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? Created { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? Updated { get; set; }

        public bool Archived { get; set; }

        public bool ArchivedByProject { get; set; }

        // Foreign Keys
        public int ProjectId { get; set; }

        public int TicketTypeId { get; set; }

        public int TicketStatusId { get; set; }

        public int TicketPriorityId { get; set; }

        public string? DeveloperUserId { get; set; }

        [Required]
        public string? SubmitterUserId { get; set; }

        // Navigation Properties
        public virtual Project? Project { get; set; }

        public virtual TicketType? TicketType { get; set; }

        public virtual TicketStatus? TicketStatus { get; set; }

        public virtual DeveloperUser? DeveloperUser { get; set; }

        public virtual SubmitterUser? SubmitterUser { get; set; }

        public virtual TicketHistory? History { get; set; }

        public virtual ICollection<TicketComment>? Comments { get; set; }

        public virtual ICollection<TicketAttachment>? Attachments { get; set; }

    }
}