using System.ComponentModel.DataAnnotations;

namespace WebApplication1.ViewModels
{
    public class RejectKYTInputModel
    {
        [Required]
        public int KytId { get; set; }

        [Required(ErrorMessage = "Alasan penolakan wajib diisi.")]
        [StringLength(500, ErrorMessage = "Alasan penolakan maksimal 500 karakter.")]
        public string RejectionReason { get; set; }
    }
}