using Municipality_System_Administration.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Municipality_System_Administration.Services
{
    public class DepreciationService
    {
        private ApplicationDbContext db;

        public DepreciationService(ApplicationDbContext context)
        {
            db = context;
        }

        // Calculate straight-line depreciation
        public decimal CalculateStraightLineDepreciation(Asset asset)
        {
            if (!asset.PurchasePrice.HasValue || !asset.SalvageValue.HasValue || !asset.UsefulLife.HasValue)
                return 0;

            var depreciableAmount = asset.PurchasePrice.Value - asset.SalvageValue.Value;
            var annualDepreciation = depreciableAmount / asset.UsefulLife.Value;
            return annualDepreciation;
        }

        // Calculate declining balance depreciation
        public decimal CalculateDecliningBalanceDepreciation(Asset asset)
        {
            if (!asset.CurrentBookValue.HasValue || !asset.DepreciationRate.HasValue)
                return 0;

            return asset.CurrentBookValue.Value * (asset.DepreciationRate.Value / 100);
        }

        // Calculate double declining balance depreciation
        public decimal CalculateDoubleDecliningDepreciation(Asset asset)
        {
            if (!asset.CurrentBookValue.HasValue || !asset.UsefulLife.HasValue)
                return 0;

            var rate = 2.0m / asset.UsefulLife.Value;
            return asset.CurrentBookValue.Value * rate;
        }

        // Calculate depreciation based on method
        public decimal CalculateDepreciation(Asset asset)
        {
            if (asset.Status == "Disposed" || asset.DepreciationStatus == "FullyDepreciated")
                return 0;

            switch (asset.DepreciationMethod)
            {
                case "StraightLine":
                    return CalculateStraightLineDepreciation(asset);
                case "DecliningBalance":
                    return CalculateDecliningBalanceDepreciation(asset);
                case "DoubleDeclining":
                    return CalculateDoubleDecliningDepreciation(asset);
                default:
                    return 0;
            }
        }

        // Process depreciation for all active assets
        public int ProcessAllDepreciation(DateTime? asOfDate = null)
        {
            var processedCount = 0;
            var date = asOfDate ?? DateTime.Now;

            var assets = db.Assets.Where(a => a.IsActive && a.Status != "Disposed").ToList();

            foreach (var asset in assets)
            {
                // Skip if no purchase date
                if (!asset.PurchaseDate.HasValue)
                    continue;

                // Skip if asset is fully depreciated
                if (asset.DepreciationStatus == "FullyDepreciated")
                    continue;

                // Calculate current depreciation
                var depreciationAmount = CalculateDepreciation(asset);

                if (depreciationAmount > 0)
                {
                    // Update asset values
                    asset.AccumulatedDepreciation = (asset.AccumulatedDepreciation ?? 0) + depreciationAmount;
                    asset.CurrentBookValue = (asset.PurchasePrice ?? 0) - (asset.AccumumulatedDepreciation ?? 0);
                    asset.LastDepreciationDate = date;
                    asset.NextDepreciationDate = date.AddMonths(1); // Monthly depreciation
                    asset.LastUpdated = DateTime.Now;

                    // Check if fully depreciated
                    if (asset.CurrentBookValue <= (asset.SalvageValue ?? 0))
                    {
                        asset.CurrentBookValue = asset.SalvageValue;
                        asset.DepreciationStatus = "FullyDepreciated";
                    }

                    processedCount++;
                }
            }

            if (processedCount > 0)
            {
                db.SaveChanges();
            }

            return processedCount;
        }

        // Get asset depreciation summary
        public DepreciationSummary GetDepreciationSummary()
        {
            var assets = db.Assets.Where(a => a.IsActive && a.Status != "Disposed").ToList();

            var summary = new DepreciationSummary
            {
                TotalAssets = assets.Count,
                AssetsWithDepreciation = assets.Count(a => a.DepreciationMethod != null),
                TotalPurchaseValue = assets.Sum(a => a.PurchasePrice ?? 0),
                TotalCurrentBookValue = assets.Sum(a => a.CurrentBookValue ?? 0),
                TotalAccumulatedDepreciation = assets.Sum(a => a.AccumulatedDepreciation ?? 0),
                FullyDepreciatedAssets = assets.Count(a => a.DepreciationStatus == "FullyDepreciated"),
                AssetsByMethod = assets
                    .GroupBy(a => a.DepreciationMethod ?? "Not Set")
                    .Select(g => new MethodDepreciationSummary
                    {
                        Method = g.Key,
                        Count = g.Count(),
                        TotalBookValue = g.Sum(a => a.CurrentBookValue ?? 0),
                        TotalDepreciation = g.Sum(a => a.AccumulatedDepreciation ?? 0)
                    })
                    .ToList()
            };

            return summary;
        }

        // Get assets needing depreciation
        public Asset[] GetAssetsNeedingDepreciation()
        {
            var today = DateTime.Now.Date;
            return db.Assets
                .Where(a => a.IsActive &&
                           a.Status != "Disposed" &&
                           a.DepreciationStatus != "FullyDepreciated" &&
                           a.DepreciationMethod != null &&
                           a.NextDepreciationDate.HasValue &&
                           a.NextDepreciationDate <= today)
                .OrderBy(a => a.NextDepreciationDate)
                .ToArray();
        }
    }

    // Helper classes
    public class DepreciationSummary
    {
        public int TotalAssets { get; set; }
        public int AssetsWithDepreciation { get; set; }
        public decimal TotalPurchaseValue { get; set; }
        public decimal TotalCurrentBookValue { get; set; }
        public decimal TotalAccumulatedDepreciation { get; set; }
        public int FullyDepreciatedAssets { get; set; }
        public List<MethodDepreciationSummary> AssetsByMethod { get; set; }
    }

    public class MethodDepreciationSummary
    {
        public string Method { get; set; }
        public int Count { get; set; }
        public decimal TotalBookValue { get; set; }
        public decimal TotalDepreciation { get; set; }
    }
}