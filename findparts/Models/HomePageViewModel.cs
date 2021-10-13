using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static Findparts.Core.AppCache;

namespace Findparts.Models
{  
    public class HomePageViewModel
    {
		public HomePageViewModel()
        {
			Merits = new List<Merit>();
			RecentSearches = new LinkedList<string>();

		}
		public VendorListItemStats Portal0Stat { get; set; }
		public VendorListItemStats Portal1Stat { get; set; }
		public bool ShowMetaDescription { get; set; }
		public ICollection<Merit> Merits { get; set; }
		public ICollection<string> RecentSearches { get; set; }


	}

	public class Merit
    {
		public string Name { get; set; }
		public string Icon { get; set; }
		public string Description { get; set; }
    }
}