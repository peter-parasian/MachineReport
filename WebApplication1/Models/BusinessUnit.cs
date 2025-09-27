using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    [Table("business_units")]
    public class BusinessUnit
    {
        [Key]
        [Column("business_unit_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BusinessUnitId { get; set; }

        [Required(ErrorMessage = "Nama unit bisnis wajib diisi.")]
        [Column("name")]
        [MaxLength(100)]
        public string Name { get; set; }

        public virtual ICollection<User>? Users { get; set; }
        public virtual ICollection<ProductionLine>? ProductionLines { get; set; }
    }
}