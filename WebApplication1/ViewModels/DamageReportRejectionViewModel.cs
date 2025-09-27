using System;

namespace WebApplication1.ViewModels
{
    public class DamageReportRejectionViewModel
    {
        public int ReportId { get; set; }
        public string MachineName { get; set; }
        public string ProductionLineName { get; set; }
        public string Description { get; set; }
        public string ReporterName { get; set; }
        public DateTime ReportDate { get; set; }
    }
}
