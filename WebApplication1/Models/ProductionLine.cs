using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    [Table("production_lines")]
    public class ProductionLine
    {
        [Key]
        [Column("production_line_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProductionLineId { get; set; }

        [Required]
        [Column("business_unit_id")]
        public int BusinessUnitId { get; set; }

        [Required(ErrorMessage = "Nama lini produksi wajib diisi.")]
        [Column("name")]
        [MaxLength(100)]
        public string Name { get; set; }

        public virtual BusinessUnit BusinessUnit { get; set; }
        public virtual ICollection<Machine>? Machines { get; set; } 
    }
}