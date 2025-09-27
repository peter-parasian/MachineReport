using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    [Table("repair_schedules")]
    public class RepairSchedule
    {
        [Key]
        [Column("schedule_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ScheduleId { get; set; }

        [Required]
        [Column("report_id")]
        public int ReportId { get; set; }

        [Required]
        [Column("created_by")]
        public int CreatedById { get; set; }

        [Column("schedule_date")]
        public DateTime? ScheduleDate { get; set; }

        [Column("approval_status")]
        public bool ApprovalStatus { get; set; } = false;
 
        [Required]
        [Column("description")]
        [MaxLength(1000)]
        public string Description { get; set; }

        public virtual DamageReport DamageReport { get; set; }
        public virtual User Creator { get; set; }
        public virtual ICollection<KYTReport>? KYTReports { get; set; }
    }
}