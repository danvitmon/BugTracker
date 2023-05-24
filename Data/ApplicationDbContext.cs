using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BugTracker.Models;

namespace BugTracker.Data
{
    public class ApplicationDbContext : IdentityDbContext<BTUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<BugTracker.Models.Company> Company { get; set; } = default!;
        public DbSet<BugTracker.Models.Invite> Invite { get; set; } = default!;
        public DbSet<BugTracker.Models.Notification> Notification { get; set; } = default!;
        public DbSet<BugTracker.Models.NotificationType> NotificationType { get; set; } = default!;
        public DbSet<BugTracker.Models.Project> Project { get; set; } = default!;
        public DbSet<BugTracker.Models.Ticket> Ticket { get; set; } = default!;
        public DbSet<BugTracker.Models.TicketAttachment> TicketAttachment { get; set; } = default!;
        public DbSet<BugTracker.Models.TicketComment> TicketComment { get; set; } = default!;
        public DbSet<BugTracker.Models.TicketHistory> TicketHistory { get; set; } = default!;
    }
}