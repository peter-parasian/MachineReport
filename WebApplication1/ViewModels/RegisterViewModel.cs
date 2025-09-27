using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WebApplication1.Models;

namespace WebApplication1.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Nama wajib diisi.")]
        [MaxLength(50)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email wajib diisi.")]
        [EmailAddress(ErrorMessage = "Format email tidak valid.")]
        [MaxLength(100)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password wajib diisi.")]
        [MinLength(6, ErrorMessage = "Password minimal 6 karakter.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Nomor telepon wajib diisi.")]
        [Phone(ErrorMessage = "Format nomor telepon tidak valid.")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Bisnis unit wajib dipilih.")]
        [Display(Name = "Bisnis Unit")]
        public int BusinessUnitId { get; set; }

        [Required(ErrorMessage = "Peran wajib dipilih.")]
        [Display(Name = "Peran")]
        public string Role { get; set; }

        [Display(Name = "Lini Produksi")]
        public int? ProductionLineId { get; set; }

        public List<BusinessUnit>? BusinessUnits { get; set; }
        public List<string>? Roles { get; set; }
        public List<ProductionLine>? ProductionLines { get; set; }
    }
}