using System;

namespace WebApplication1.ViewModels
{
    public class PendingApprovalViewModel
    {
        public int ScheduleId { get; set; }
        public DateTime? ScheduleDate { get; set; }
        public string MachineName { get; set; } = "";
        public string ProductionLineName { get; set; } = "";
        public string ReporterName { get; set; } = "";
        public string DamageDescription { get; set; } = "";
        public string ScheduleDescription { get; set; } = "";
        public DateTime ReportDate { get; set; }
    }
}
