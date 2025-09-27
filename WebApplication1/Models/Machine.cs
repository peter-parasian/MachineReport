using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    [Table("machines")]
    public class Machine
    {
        [Key]
        [Column("machine_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MachineId { get; set; }

        [Required]
        [Column("production_line_id")]
        public int ProductionLineId { get; set; }

        [Required(ErrorMessage = "Nama mesin wajib diisi.")]
        [Column("name")]
        [MaxLength(100)]
        public string Name { get; set; }

        [Column("status")]
        [MaxLength(50)]
        public string? Status { get; set; }

        public virtual ProductionLine ProductionLine { get; set; }
        public virtual ICollection<DamageReport>? DamageReports { get; set; }
    }
}