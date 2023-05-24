using System.ComponentModel.DataAnnotations;

namespace BugTracker.Models
{
    public class TicketAttachment
    {
        public int Id { get; set; }

        public string? Description { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? Created { get; set; }

        public int TicketId { get; set; }

        [Required]
        public string? BTUserId { get; set; }

        public IFormFile? FormFile { get; set; }

        public byte[]? FileData { get; set; }

        public string? FileType { get; set; }

        public virtual Ticket? Ticket { get; set; }

        public virtual BTUser? BTUser { get; set; }
    }
}