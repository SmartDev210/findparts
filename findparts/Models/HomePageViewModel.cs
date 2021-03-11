using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Findparts.Models
{  
    public class HomePageViewModel
    {
		public HomePageViewModel()
        {
			Merits = new List<Merit>();
			RecentSearches = new LinkedList<string>();

		}
        public bool ShowMetaDescription { get; set; }
        public int SpanVendorCount { get; set; }
        public int SpanPartCount { get; set; }
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