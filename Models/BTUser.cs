using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BugTracker.Models
{
    public class BTUser
    {
        [Required]
        public string? FirstName { get; set; }

        [Required]
        public string? LastName { get; set;}

        [NotMapped]
        public string? FullName { get { return $"{FirstName} {LastName}"; } }

        [NotMapped]
        public IFormFile? ImageFormFile { get; set;}

        public byte[]? ImageFileData { get; set;}

        public string? ImageFileType { get; set;}

        public int CompanyId { get; set;}

        public virtual Company? Company { get; set;}

        public virtual ICollection<Project>? Projects { get; set;} = new HashSet<Project>();
    }
}