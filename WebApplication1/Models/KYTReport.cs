using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    [Table("kyt_reports")]
    public class KYTReport
    {
        [Key]
        [Column("kyt_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int KytId { get; set; }

        [Required]
        [Column("schedule_id")]
        public int ScheduleId { get; set; }

        [Required]
        [Column("created_by")]
        public int CreatedById { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("approval_status")]
        public bool? ApprovalStatus { get; set; }
  
        [Required]
        [Column("dangerous_mode")]
        public int DangerousMode { get; set; }

        [Required]
        [Column("prepare_process")]
        [MaxLength(1000)]
        public string PrepareProcess { get; set; }

        [Required]
        [Column("prepare_prediction")]
        [MaxLength(1000)]
        public string PreparePrediction { get; set; }

        [Required]
        [Column("prepare_control")]
        [MaxLength(1000)]
        public string PrepareControl { get; set; }

        [Required]
        [Column("main_process")]
        [MaxLength(1000)]
        public string MainProcess { get; set; }

        [Required]
        [Column("main_prediction")]
        [MaxLength(1000)]
        public string MainPrediction { get; set; }

        [Required]
        [Column("main_control")]
        [MaxLength(1000)]
        public string MainControl { get; set; }

        [Required]
        [Column("confirm_process")]
        [MaxLength(1000)]
        public string ConfirmProcess { get; set; }

        [Required]
        [Column("confirm_prediction")]
        [MaxLength(1000)]
        public string ConfirmPrediction { get; set; }

        [Required]
        [Column("confirm_control")]
        [MaxLength(1000)]
        public string ConfirmControl { get; set; }

        [Required]
        [Column("analysis")]
        [MaxLength(1000)]
        public string Analysis { get; set; }

        [Required]
        [Column("description")]
        [MaxLength(1000)]
        public string Description { get; set; }

        [Required]
        [Column("action")]
        [MaxLength(1000)]
        public string Action { get; set; }

        [Column("reviewed_by")]
        public int? ReviewedById { get; set; }

        public virtual RepairSchedule RepairSchedule { get; set; }
        public virtual User Creator { get; set; }
        public virtual User? Reviewer { get; set; }

        public virtual ICollection<User> Technicians { get; set; } = new List<User>();

        [NotMapped]
        public List<User> AvailableTechnicians { get; set; } = new List<User>();
    }
}