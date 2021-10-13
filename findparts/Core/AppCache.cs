using DAL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Caching;

namespace Findparts.Core
{
    public class AppCache
    {
		private const string _vendorListItemStatsCacheKey = "VendorListItemStats";
		private static object _vendorListItemStatsLock = new object();

		public AppCache()
		{
		}

		public class VendorListItemStats
		{
			public int VendorCount = 0;
			public int PartCount = 0;
			public int AchievementCount = 0;
		}
		public static VendorListItemStats VendorListItemGetStats(int portalCode)
		{
			string cacheKey = _vendorListItemStatsCacheKey + portalCode.ToString();
			int[] counts = (int[])HttpRuntime.Cache[cacheKey];
			if (counts == null)
			{
				lock (_vendorListItemStatsLock)
				{
					counts = (int[])HttpRuntime.Cache[cacheKey];
					if (counts == null)
					{
						var context = new FindPartsEntities();
						var result = context.VendorListItemGetStats(portalCode).ToList();
						if (result.Count > 0)
                        {
							if (portalCode == 1)
								result[0].VendorCount = 5127;

							counts = new int[]
							{
								result[0].VendorCount ?? 0,
								result[0].PartCount ?? 0,
								result[0].AchievementCount ?? 0
							};
							HttpRuntime.Cache.Add(
								cacheKey,
								counts,
								null,
								Cache.NoAbsoluteExpiration,
								TimeSpan.FromMinutes(300),
								CacheItemPriority.NotRemovable,
								null
							);

							if (!Directory.Exists(Config.SharedJsonPath))
                            {
								Directory.CreateDirectory(Config.SharedJsonPath);
                            }
							var content = JsonConvert.SerializeObject(result[0]);
							File.WriteAllText($"{Config.SharedJsonPath}stats-{portalCode}.json", content);
						}
					}
				}
			}
			var stats = new VendorListItemStats();
			if (counts != null && counts.Length == 3)
			{
				stats.VendorCount = counts[0];
				stats.PartCount = counts[1];
				stats.AchievementCount = counts[2];
			}
			return stats;
		}
	}
}