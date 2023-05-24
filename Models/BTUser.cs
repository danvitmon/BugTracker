using System.ComponentModel.DataAnnotations;

namespace BugTracker.Models
{
    public class BTUser
    {
        [Required]
        public string? FirstName { get; set; }

        [Required]
        public string? LastName { get; set;}

        public string? FullName { get; set;}

        public IFormFile? ImageFormFile { get; set;}

        public byte[]? ImageFileData { get; set;}

        public string? ImageFileType { get; set;}

        public int CompanyId { get; set;}

        public virtual Company? Company { get; set;}

        public virtual ICollection<Project>? Projects { get; set;}
    }
}