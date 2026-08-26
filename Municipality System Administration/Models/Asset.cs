using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Municipality_System_Administration.Models
{
    public class Asset
    {
        [Key]
        public int AssetId { get; set; }

        [Required]
        [Display(Name = "Asset Number")]
        public string AssetNumber { get; set; }

        [Required]
        [Display(Name = "Asset Name")]
        public string AssetName { get; set; }

        [Required]
        [Display(Name = "Category")]
        public string Category { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Brand")]
        public string Brand { get; set; }

        [Display(Name = "Model")]
        public string Model { get; set; }

        [Display(Name = "Serial Number")]
        public string SerialNumber { get; set; }

        [Display(Name = "Purchase Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? PurchaseDate { get; set; }

        [Display(Name = "Purchase Price")]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = true)]
        public decimal? PurchasePrice { get; set; }

        [Display(Name = "Supplier")]
        public string Supplier { get; set; }

        [Display(Name = "Condition")]
        public string Condition { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; }

        [Display(Name = "Location")]
        public string Location { get; set; }

        [Display(Name = "Location Address")]
        public string LocationAddress { get; set; }

        [Display(Name = "Latitude")]
        public decimal? Latitude { get; set; }

        [Display(Name = "Longitude")]
        public decimal? Longitude { get; set; }

        [Display(Name = "Google Place ID")]
        public string GooglePlaceId { get; set; }

        [Display(Name = "Assigned To")]
        public string AssignedToUserId { get; set; }

        [Display(Name = "Date Assigned")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? DateAssigned { get; set; }

        [Display(Name = "Date Returned")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? DateReturned { get; set; }

        [Display(Name = "Warranty Expiry")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? WarrantyExpiry { get; set; }

        [Display(Name = "Last Maintenance")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? LastMaintenanceDate { get; set; }

        [Display(Name = "Next Maintenance")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? NextMaintenanceDate { get; set; }

        [Display(Name = "Maintenance Frequency (Months)")]
        public int? MaintenanceFrequency { get; set; } // e.g., 6 months

        [Display(Name = "Maintenance Notes")]
        public string MaintenanceNotes { get; set; }

        [Display(Name = "Is Assigned")]
        public bool IsAssigned { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; }

        [Display(Name = "Date Created")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
        public DateTime DateCreated { get; set; }

        [Display(Name = "Last Updated")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
        public DateTime? LastUpdated { get; set; }

        [Display(Name = "Notes")]
        public string Notes { get; set; }

        public bool RepairRequested { get; set; }

        public DateTime? RepairRequestDate { get; set; }

        public string RepairStatus { get; set; }

        public string RepairAssignedToUserId { get; set; }

        public string RepairReport { get; set; }

        public decimal? RepairCost { get; set; }

        public string RepairRequestedBy { get; set; }

        public DateTime? RepairCompletedDate { get; set; }

        [ForeignKey("AssignedToUserId")]
        public virtual ApplicationUser AssignedToUser { get; set; }

        public virtual Staff AssignedStaff { get; set; }

        [ForeignKey("RepairAssignedToUserId")]
        public virtual ApplicationUser RepairAssignedToUser { get; set; }

        public virtual Staff RepairAssignedStaff { get; set; }
        public string AssignedToId { get; internal set; }

        // Add these properties to your Asset model
        [Display(Name = "Depreciation Method")]
        public string DepreciationMethod { get; set; } // StraightLine, DecliningBalance, DoubleDeclining

        [Display(Name = "Depreciation Rate (%)")]
        [DisplayFormat(DataFormatString = "{0:P2}", ApplyFormatInEditMode = true)]
        public decimal? DepreciationRate { get; set; }

        [Display(Name = "Salvage Value")]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = true)]
        public decimal? SalvageValue { get; set; }

        [Display(Name = "Useful Life (Years)")]
        public int? UsefulLife { get; set; }

        [Display(Name = "Current Book Value")]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = true)]
        public decimal? CurrentBookValue { get; set; }

        [Display(Name = "Accumulated Depreciation")]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = true)]
        public decimal? AccumulatedDepreciation { get; set; }

        [Display(Name = "Last Depreciation Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? LastDepreciationDate { get; set; }

        [Display(Name = "Next Depreciation Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? NextDepreciationDate { get; set; }

        [Display(Name = "Depreciation Status")]
        public string DepreciationStatus { get; set; } // Active, FullyDepreciated, Disposed
        public int? AccumumulatedDepreciation { get; set; }
    }
}