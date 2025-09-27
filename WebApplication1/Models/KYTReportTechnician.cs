using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    [Table("kyt_report_technicians")]
    public class KYTReportTechnician
    {
        [Column("kyt_id")]
        public int KytId { get; set; }

        [Column("technician_id")]
        public int TechnicianId { get; set; }

        [ForeignKey("KytId")]
        public virtual KYTReport KYTReport { get; set; }

        [ForeignKey("TechnicianId")]
        public virtual User Technician { get; set; }
    }
}