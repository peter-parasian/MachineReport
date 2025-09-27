using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    [Table("users")]
    public class User
    {
        [Key]
        [Column("user_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Nama wajib diisi.")]
        [Column("name")]
        [MaxLength(50)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        [Column("email")]
        [MaxLength(100)]
        public string Email { get; set; }

        [Required]
        [Column("password_hash")]
        [MaxLength(255)]
        public string PasswordHash { get; set; }

        [Column("avatar")]
        [MaxLength(255)]
        public string? Avatar { get; set; }

        [Required]
        [Column("phone_number")]
        [MaxLength(255)]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Role wajib diisi.")]
        [Column("role")]
        [MaxLength(50)]
        public string Role { get; set; }

        [Column("is_verified")]
        public bool IsVerified { get; set; } = false;

        [Column("business_unit_id")]
        public int? BusinessUnitId { get; set; }

        [Column("production_line_id")]
        public int? ProductionLineId { get; set; }

        public virtual BusinessUnit? BusinessUnit { get; set; }
        public virtual ProductionLine? ProductionLine { get; set; }
        public virtual ICollection<Notification>? Notifications { get; set; }
        public virtual ICollection<DamageReport>? ReportedDamageReports { get; set; }
        public virtual ICollection<RepairSchedule>? CreatedRepairSchedules { get; set; }
        public virtual ICollection<KYTReport>? CreatedKYTReports { get; set; }
        public virtual ICollection<KYTReport>? ReviewedKYTReports { get; set; }


        public virtual ICollection<KYTReport> AssignedKYTReports { get; set; } = new List<KYTReport>();
    }
}