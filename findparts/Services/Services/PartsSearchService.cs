using DAL;
using Findparts.Core;
using Findparts.Models;
using Findparts.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Findparts.Services.Services
{
    public class PartsSearchService : IPartsSearchService
    {
        private readonly FindPartsEntities _context;

        private static string[][] MERITS = new[] {
            new[] {"DER","DER","Repair Station has developed a DER 'Designated Engineering Approval'"},
            new[] {"CAAC","CAAC","CAAC ARC 'Aircraft Release Certificate' is available"},
            new[] {"Modified","MODIFIED","Modified ARC 'Aircraft Release Certificate' is available"},
            new[] {"Repairs Frequently", "REPAIRS_FREQUENTLY", "Greater than 20 units serviced."},
            new[] {"PMA","PMA","PMA replacement parts available to lower the repair price"},
            new[] {"Extended Warranty","EX-WAR","Warranty is longer than industry standard"},
            new[] {"Free Eval","FREE_EVAL","No Evaluation Fee"},
            new[] {"Flat Rate","FLAT_RATE","Flat Rate Repair or Overhaul"},
            new[] {"Range","RANGE","Low & High 'Price Range' is available"},
            new[] {"NTE","NTE","NTE 'Not To Exceed' price is available"},
            new[] {"MRO FINDER Quotable","QUOTED","Repair Station has previously quoted using MRO FINDER"},
            new[] {"Function Test","TEST","Function Test Only - No Repair or Overhaul workscope"},
            new[] {"No Overhaul Workscope","NO_OH","Unit will be released Serviceable, Repaired, or Tested"},
            new[] {"OEM","OEM","Repair Station is a OEM (original equipment manufacturer)"},
            new[] {"OEM Authorized","OEM_EX","Repair Station has OEM Approval(s)"},
            new[] {"OEM RMA","OEM_RMA","Repair Station is a OEM & requires a RMA authorization"}
        };

        public PartsSearchService(FindPartsEntities context)
        {
            _context = context;
        }

        public void PopulateHomePageViewModel(HomePageViewModel viewModel)
        {
            if (viewModel == null) return;

            viewModel.Merits = MERITS.Select(merit => new Merit
            {
                Name = merit[0],
                Icon = merit[1],
                Description = merit[2]
            }).ToList();
            
            var stats = AppCache.VendorListItemGetStats();
            viewModel.SpanVendorCount = stats.VendorCount;
            viewModel.SpanPartCount = stats.PartCount;

            viewModel.RecentSearches = _context.UserSearchGetRecent3().ToList();
        }
    }
}