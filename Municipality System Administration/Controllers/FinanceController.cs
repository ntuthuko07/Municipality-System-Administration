using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Municipality_System_Administration.Models;
using Municipality_System_Administration.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Municipality_System_Administration.Controllers
{
    [Authorize(Roles = "FinanceOfficer, Admin")]
    public class FinanceController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private DepreciationService depreciationService;

        public FinanceController()
        {
            depreciationService = new DepreciationService(db);
        }

        //==============================
        // FINANCE DASHBOARD
        //==============================
        [HttpGet]
        public ActionResult Index()
        {
            // Get disposal statistics
            var pendingDisposals = db.DisposalRequests.Count(r => r.Status == "Pending");
            var approvedDisposals = db.DisposalRequests.Count(r => r.Status == "Approved");
            var rejectedDisposals = db.DisposalRequests.Count(r => r.Status == "Rejected");
            var totalValue = db.DisposalRequests
                .Where(r => r.Status == "Approved" && r.DisposalValue.HasValue)
                .Sum(r => r.DisposalValue ?? 0);

            // Get depreciation statistics
            var depreciationSummary = depreciationService.GetDepreciationSummary();

            // Get assets needing depreciation
            var assetsNeedingDepreciation = depreciationService.GetAssetsNeedingDepreciation();

            ViewBag.PendingDisposals = pendingDisposals;
            ViewBag.ApprovedDisposals = approvedDisposals;
            ViewBag.RejectedDisposals = rejectedDisposals;
            ViewBag.TotalDisposalValue = totalValue;
            ViewBag.DepreciationSummary = depreciationSummary;
            ViewBag.AssetsNeedingDepreciation = assetsNeedingDepreciation;
            ViewBag.AssetsNeedingDepreciationCount = assetsNeedingDepreciation.Length;

            // Get recent requests
            var recentRequests = db.DisposalRequests
                .OrderByDescending(r => r.RequestDate)
                .Take(10)
                .ToList();

            var assetIds = recentRequests.Select(r => r.AssetId).Distinct().ToList();
            var assets = db.Assets.Where(a => assetIds.Contains(a.AssetId)).ToDictionary(a => a.AssetId);

            ViewBag.RecentRequests = recentRequests;
            ViewBag.Assets = assets;

            return View();
        }

        //==============================
        // DEPRECIATION REPORT
        //==============================
        [HttpGet]
        public ActionResult DepreciationReport()
        {
            // Get all active assets (not disposed)
            var assets = db.Assets
                .Where(a => a.IsActive && a.Status != "Disposed")
                .OrderBy(a => a.AssetName)
                .ToList();

            // Get depreciation summary
            var summary = depreciationService.GetDepreciationSummary();

            ViewBag.Summary = summary;

            // Return the list of assets as the model
            return View(assets);
        }

        //==============================
        // PROCESS DEPRECIATION
        //==============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProcessDepreciation(DateTime? asOfDate)
        {
            var processedCount = depreciationService.ProcessAllDepreciation(asOfDate);

            TempData["Success"] = $"Depreciation processed successfully for {processedCount} assets.";
            return RedirectToAction("DepreciationReport");
        }

        //==============================
        // ASSET DEPRECIATION DETAILS
        //==============================
        [HttpGet]
        public ActionResult AssetDepreciation(int id)
        {
            var asset = db.Assets.Find(id);
            if (asset == null)
            {
                TempData["Error"] = "Asset not found.";
                return RedirectToAction("DepreciationReport");
            }

            return View(asset);
        }

        //==============================
        // UPDATE ASSET DEPRECIATION
        //==============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateAssetDepreciation(Asset asset)
        {
            var existingAsset = db.Assets.Find(asset.AssetId);
            if (existingAsset == null)
            {
                TempData["Error"] = "Asset not found.";
                return RedirectToAction("DepreciationReport");
            }

            // Update depreciation settings
            existingAsset.DepreciationMethod = asset.DepreciationMethod;
            existingAsset.DepreciationRate = asset.DepreciationRate;
            existingAsset.SalvageValue = asset.SalvageValue;
            existingAsset.UsefulLife = asset.UsefulLife;
            existingAsset.LastUpdated = DateTime.Now;

            // Calculate initial book value if not set
            if (existingAsset.CurrentBookValue == null)
            {
                existingAsset.CurrentBookValue = existingAsset.PurchasePrice;
                existingAsset.AccumulatedDepreciation = 0;
                existingAsset.DepreciationStatus = "Active";
                existingAsset.NextDepreciationDate = DateTime.Now.AddMonths(1);
            }

            db.SaveChanges();

            TempData["Success"] = $"Depreciation settings updated for '{existingAsset.AssetName}'.";
            return RedirectToAction("DepreciationReport");
        }

        //==============================
        // REPORTS - Financial Reports
        //==============================
        [HttpGet]
        public ActionResult Reports()
        {
            // Get all approved disposals
            var approvedDisposals = db.DisposalRequests
                .Where(r => r.Status == "Approved")
                .OrderByDescending(r => r.FinanceReviewDate)
                .ToList();

            // Get all rejected disposals
            var rejectedDisposals = db.DisposalRequests
                .Where(r => r.Status == "Rejected")
                .OrderByDescending(r => r.FinanceReviewDate)
                .ToList();

            // Get pending disposals
            var pendingDisposals = db.DisposalRequests
                .Where(r => r.Status == "Pending")
                .OrderByDescending(r => r.RequestDate)
                .ToList();

            // Get depreciation summary
            var depreciationSummary = depreciationService.GetDepreciationSummary();

            // Statistics
            var totalApproved = approvedDisposals.Count();
            var totalRejected = rejectedDisposals.Count();
            var totalPending = pendingDisposals.Count();
            var totalValue = approvedDisposals.Sum(r => r.DisposalValue ?? 0);
            var averageValue = totalApproved > 0 ? totalValue / totalApproved : 0;

            // Get all asset IDs to fetch names
            var allAssetIds = approvedDisposals.Select(r => r.AssetId)
                .Union(rejectedDisposals.Select(r => r.AssetId))
                .Union(pendingDisposals.Select(r => r.AssetId))
                .Distinct()
                .ToList();

            var assets = db.Assets.Where(a => allAssetIds.Contains(a.AssetId)).ToDictionary(a => a.AssetId);

            // Get finance officer names
            var userIds = approvedDisposals.Select(r => r.FinanceReviewedByUserId)
                .Union(rejectedDisposals.Select(r => r.FinanceReviewedByUserId))
                .Where(id => id != null)
                .Distinct()
                .ToList();

            var financeOfficers = db.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionary(u => u.Id, u => u.UserName);

            ViewBag.ApprovedDisposals = approvedDisposals;
            ViewBag.RejectedDisposals = rejectedDisposals;
            ViewBag.PendingDisposals = pendingDisposals;
            ViewBag.TotalApproved = totalApproved;
            ViewBag.TotalRejected = totalRejected;
            ViewBag.TotalPending = totalPending;
            ViewBag.TotalValue = totalValue;
            ViewBag.AverageValue = averageValue;
            ViewBag.Assets = assets;
            ViewBag.FinanceOfficers = financeOfficers;
            ViewBag.DepreciationSummary = depreciationSummary;

            // Group by disposal method
            var methodStats = approvedDisposals
                .GroupBy(r => r.DisposalMethod)
                .Select(g => new MethodStat
                {
                    Method = g.Key ?? "Not Specified",
                    Count = g.Count(),
                    Total = g.Sum(r => r.DisposalValue ?? 0)
                })
                .OrderByDescending(g => g.Count)
                .ToList();

            ViewBag.MethodStats = methodStats;

            return View();
        }

        //==============================
        // DETAILS - View disposal request details
        //==============================
        [HttpGet]
        public ActionResult Details(int id)
        {
            var request = db.DisposalRequests.Find(id);
            if (request == null)
            {
                TempData["Error"] = "Request not found.";
                return RedirectToAction("Index");
            }

            // Get related data
            var asset = db.Assets.Find(request.AssetId);
            var requestedBy = db.Users.Find(request.RequestedByUserId);
            var financeReviewedBy = db.Users.Find(request.FinanceReviewedByUserId);

            ViewBag.Asset = asset;
            ViewBag.RequestedBy = requestedBy;
            ViewBag.FinanceReviewedBy = financeReviewedBy;

            return View(request);
        }

        //==============================
        // GET PENDING COUNT (AJAX)
        //==============================
        [HttpGet]
        public JsonResult GetPendingCount()
        {
            var count = db.DisposalRequests.Count(r => r.Status == "Pending");
            return Json(new { count = count }, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    public class MethodStat
    {
        public string Method { get; set; }
        public int Count { get; set; }
        public decimal Total { get; set; }
    }
}