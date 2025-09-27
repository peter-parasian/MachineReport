using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    [Table("repair_logs")]
    public class RepairLog
    {
        [Key]
        [Column("log_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LogId { get; set; }

        [Required]
        [Column("report_id")]
        public int ReportId { get; set; }

        [Column("kyt_approval_time")]
        public DateTime? KytApprovalTime { get; set; }

        [Column("repair_completion_time")]
        public DateTime? RepairCompletionTime { get; set; }

        [Column("approval_status")]
        public bool? ApprovalStatus { get; set; }

        public virtual DamageReport DamageReport { get; set; }
    }
}