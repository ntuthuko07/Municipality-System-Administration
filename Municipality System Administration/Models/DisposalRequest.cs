using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Municipality_System_Administration.Models
{
    public class DisposalRequest
    {
        [Key]
        public int DisposalRequestId { get; set; }

        [Required]
        public int AssetId { get; set; }

        [Required]
        public string RequestedByUserId { get; set; }

        [Required]
        public DateTime RequestDate { get; set; }

        [Required]
        [Display(Name = "Reason for Disposal")]
        public string Reason { get; set; }

        [Display(Name = "Disposal Method")]
        public string DisposalMethod { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; } // Pending, Approved, Rejected

        // Finance Officer Review
        [Display(Name = "Finance Review Date")]
        public DateTime? FinanceReviewDate { get; set; }

        [Display(Name = "Finance Reviewed By")]
        public string FinanceReviewedByUserId { get; set; }

        [Display(Name = "Finance Review Notes")]
        public string FinanceReviewNotes { get; set; }

        [Display(Name = "Disposal Value")]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = true)]
        public decimal? DisposalValue { get; set; }

        // Navigation properties
        [ForeignKey("AssetId")]
        public virtual Asset Asset { get; set; }

        [ForeignKey("RequestedByUserId")]
        public virtual ApplicationUser RequestedBy { get; set; }

        [ForeignKey("FinanceReviewedByUserId")]
        public virtual ApplicationUser FinanceReviewedBy { get; set; }
    }
}