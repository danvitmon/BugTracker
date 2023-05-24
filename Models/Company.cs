using System.ComponentModel.DataAnnotations;

namespace BugTracker.Models
{
    public class Company
    {
        // Primary Key
        public int Id { get; set; }

        [Required]
        public string? Name { get; set; }

        public string? Description { get; set; }

        public IFormFile? ImageFormFile { get; set; }

        public byte[]? ImageFileData { get; set; }

        public string? ImageFileType { get; set; }

        // Navigation Properties
        public virtual ICollection<Project> Projects { get; set; } = new HashSet<Project>();

        public virtual ICollection<Member> Members { get; set; } = new HashSet<Member>();

        public virtual ICollection<Invite> Invites { get; set; } = new HashSet<Invite>();


    }
}
