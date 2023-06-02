using Microsoft.AspNetCore.Mvc.Rendering;

namespace BugTracker.Models.ViewModels
{
    public class AssignMemberViewModel
    {
        public Project? Project { get; set; }
        public SelectList? UnassignedList { get; set; }
        public SelectList? CurrentList { get; set; }

        public string? MemberId { get; set; }
    }
}
