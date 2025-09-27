using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.ViewModels
{
    public class KYTFormViewModel
    {
        [Required]
        public int ScheduleId { get; set; }

        public int KytId { get; set; }
        public string? MachineName { get; set; }
        public string? ProductionLineName { get; set; }
        public string? DamageDescription { get; set; }
        public string? ReporterName { get; set; }
        public string? FilledByName { get; set; }

        [Required(ErrorMessage = "Setidaknya satu teknisi wajib dipilih.")]
        public List<int> TechnicianIds { get; set; } = new List<int>();

        [Required(ErrorMessage = "Analisis Kerusakan (Crash Analysis) wajib diisi.")]
        [StringLength(1000)]
        public string? Analysis { get; set; }

        [Required(ErrorMessage = "Tindakan yang Diperlukan (Action Needed) wajib diisi.")]
        [StringLength(1000)]
        public string? Action { get; set; }

        [Required(ErrorMessage = "Mode Berbahaya wajib dipilih.")]
        [Range(1, int.MaxValue, ErrorMessage = "Pilih setidaknya satu mode berbahaya.")]
        public int DangerousMode { get; set; }

        [Required(ErrorMessage = "Proses (Prepare) wajib diisi.")]
        [StringLength(1000)]
        public string? PrepareProcess { get; set; }

        [Required(ErrorMessage = "Prediksi Bahaya (Prepare) wajib diisi.")]
        [StringLength(1000)]
        public string? PreparePrediction { get; set; }

        [Required(ErrorMessage = "Tindakan Pengendalian (Prepare) wajib diisi.")]
        [StringLength(1000)]
        public string? PrepareControl { get; set; }

        [Required(ErrorMessage = "Proses (Main) wajib diisi.")]
        [StringLength(1000)]
        public string? MainProcess { get; set; }

        [Required(ErrorMessage = "Prediksi Bahaya (Main) wajib diisi.")]
        [StringLength(1000)]
        public string? MainPrediction { get; set; }

        [Required(ErrorMessage = "Tindakan Pengendalian (Main) wajib diisi.")]
        [StringLength(1000)]
        public string? MainControl { get; set; }

        [Required(ErrorMessage = "Proses (Confirm) wajib diisi.")]
        [StringLength(1000)]
        public string? ConfirmProcess { get; set; }

        [Required(ErrorMessage = "Prediksi Bahaya (Confirm) wajib diisi.")]
        [StringLength(1000)]
        public string? ConfirmPrediction { get; set; }

        [Required(ErrorMessage = "Tindakan Pengendalian (Confirm) wajib diisi.")]
        [StringLength(1000)]
        public string? ConfirmControl { get; set; }

        public List<SelectListItem> AvailableTechnicians { get; set; } = new List<SelectListItem>();
        public List<string> TechnicianNames { get; set; } = new List<string>();
    }
}