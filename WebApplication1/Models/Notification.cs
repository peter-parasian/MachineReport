using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    [Table("notifications")]
    public class Notification
    {
        [Key]
        [Column("notification_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int NotificationId { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [Column("title")]
        [MaxLength(255)]
        public string Title { get; set; }

        [Required]
        [Column("message")]
        [MaxLength(1000)]
        public string Message { get; set; }

        [Required]
        [Column("type")]
        [MaxLength(50)]
        public string Type { get; set; }  

        [Column("action_url")]
        [MaxLength(500)]
        public string? ActionUrl { get; set; }

        [Column("is_read")]
        public bool IsRead { get; set; } = false;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        public virtual User User { get; set; }
    }
}