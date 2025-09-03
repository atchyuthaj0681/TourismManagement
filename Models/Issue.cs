using System;
using System.ComponentModel.DataAnnotations;

namespace TourismManagement.Models
{
    public class Issue
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public ApplicationUser User { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public DateTime ReportedAt { get; set; }

        public string Status { get; set; } // Open, Resolved, In Progress
    }
}
