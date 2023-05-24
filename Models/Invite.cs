using System.ComponentModel.DataAnnotations;

namespace BugTracker.Models
{
    public class Invite
    {
        // Primary Key
        public int Id { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime InviteDate { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime JoinDate { get; set; }

        public Guid CompanyToken { get; set; }

        // Foreign Key
        public int CompanyId { get; set; }

        public int ProjectId { get; set; }

        [Required]
        public string? InvitorId { get; set; }

        [Required]
        public string? InviteeId { get; set; }

        [Required]
        public string? InviteeEmail { get; set; }

        [Required]
        public string? InviteeFirstName { get; set; }

        [Required]
        public string? InviteeLastName { get; set; }

        public string? Message { get; set; }

        public bool IsValid { get; set; }

        // Navigation Properties
        public virtual Company? Company { get; set; }

        public virtual Project? Project { get; set; }

        public virtual Invitor? Invitor { get; set; }

        public virtual Invitee? Invitee { get; set; }

    }
}