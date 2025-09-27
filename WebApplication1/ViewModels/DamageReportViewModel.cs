using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WebApplication1.Models;

namespace WebApplication1.ViewModels
{
    public class DamageReportViewModel
    {
        [Required(ErrorMessage = "Mesin wajib dipilih.")]
        public int MachineId { get; set; }

        [Required(ErrorMessage = "Deskripsi kerusakan wajib diisi.")]
        [StringLength(1000)]
        public string Description { get; set; }

        public List<Machine> AvailableMachines { get; set; } = new();
    }
}