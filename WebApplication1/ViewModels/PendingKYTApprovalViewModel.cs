using System;

namespace WebApplication1.ViewModels
{
    public class PendingKYTApprovalViewModel
    {
        public int KytId { get; set; }
        public string CreatorName { get; set; }
        public string MachineName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ReporterName { get; set; }
    }
}
