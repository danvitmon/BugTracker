﻿using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BugTracker.Models
{
    public class BTUser : IdentityUser
    {
        [Required]
        [Display(Name = "First Name")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)]
        public string? FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)]
        public string? LastName { get; set;}

        [NotMapped]
        public string? FullName { get { return $"{FirstName} {LastName}"; } }

        [NotMapped]
        public IFormFile? ImageFormFile { get; set;}

        public byte[]? ImageFileData { get; set;}

        public string? ImageFileType { get; set;}

        public int CompanyId { get; set;}

        public virtual Company? Company { get; set;}

        public virtual ICollection<Project> Projects { get; set;} = new HashSet<Project>();
    }
}