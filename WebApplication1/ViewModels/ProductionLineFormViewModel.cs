using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace WebApplication1.ViewModels
{
    public class ProductionLineFormViewModel
    {
        [Required(ErrorMessage = "Nama lini produksi wajib diisi.")]
        [Display(Name = "Nama Lini Produksi")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Unit bisnis wajib dipilih.")]
        [Display(Name = "Unit Bisnis")]
        public int BusinessUnitId { get; set; }

        public List<SelectListItem>? BusinessUnits { get; set; }
    }
}