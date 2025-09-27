using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace WebApplication1.ViewModels
{
    public class MachineFormViewModel
    {
        [Required(ErrorMessage = "Nama mesin wajib diisi.")]
        [Display(Name = "Nama Mesin")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Lini produksi wajib dipilih.")]
        [Display(Name = "Lini Produksi")]
        public int ProductionLineId { get; set; }
        public List<SelectListItem>? ProductionLines { get; set; }
    }
}