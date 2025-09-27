using System;

namespace WebApplication1.ViewModels
{
    public class PendingLeaderCompletionViewModel
    {
        public int LogId { get; set; }
        public string MachineName { get; set; }
        public string ProductionLineName { get; set; }
        public string ReporterName { get; set; }
        public DateTime? KytApprovalDate { get; set; }
    }
}
