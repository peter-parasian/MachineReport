using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    [Table("damage_reports")]
    public class DamageReport
    {
        [Key]
        [Column("report_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ReportId { get; set; }

        [Required]
        [Column("machine_id")]
        public int MachineId { get; set; }

        [Required]
        [Column("reported_by")]
        public int ReportedById { get; set; }

        [Column("reported_at")]
        public DateTime ReportedAt { get; set; }

        [Column("status")]
        public bool? Status { get; set; }

        [Required]
        [Column("description")]
        [MaxLength(1000)]
        public string Description { get; set; }

        public virtual Machine Machine { get; set; }
        public virtual User Reporter { get; set; }
        public virtual ICollection<RepairSchedule>? RepairSchedules { get; set; }
        public virtual ICollection<RepairLog>? RepairLogs { get; set; }
    }
}
