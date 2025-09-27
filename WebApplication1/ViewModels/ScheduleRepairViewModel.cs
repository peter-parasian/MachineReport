using System;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.ViewModels
{
    public class ScheduleRepairViewModel
    {
        public int ReportId { get; set; }

        public string? MachineName { get; set; }

        [Required(ErrorMessage = "Deskripsi kerusakan wajib diisi.")]
        [StringLength(1000)]
        public string Description { get; set; }

        [Required(ErrorMessage = "Tanggal perbaikan wajib diisi.")]
        public DateTime? ScheduleDate { get; set; }
    }
}