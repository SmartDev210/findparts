using DAL;
using Findparts.Core;
using Findparts.Extensions;
using Findparts.Models;
using Findparts.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Findparts.Services.Services
{
    public class PartsSearchService : IPartsSearchService
    {
        private readonly FindPartsEntities _context;
        

        public PartsSearchService(FindPartsEntities context)
        {
            _context = context;
        }

        public void PopulateHomePageViewModel(HomePageViewModel viewModel)
        {
            if (viewModel == null) return;

            viewModel.Merits = Constants.MERITS.Select(merit => new Merit
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

        public List<VendorListItemSearchDetail9_Result> GetDetails(PartsSearchQueryParams queryParams, bool isAdmin)
        {
            string viewAsVendorID;
            if (!string.IsNullOrEmpty(queryParams.VendorID) && isAdmin)
            {
                viewAsVendorID = queryParams.VendorID;
            }
            else
            {
                viewAsVendorID = SessionVariables.VendorID;
            }

            var list = _context.VendorListItemSearchDetail9(queryParams.PartNumberDetail, SessionVariables.SubscriberID.ToNullableInt()).ToList();

            if ((SessionVariables.SubscriberID == "" || !SessionVariables.CanSearch) && !isAdmin)
            {
                list.ForEach(x =>
                {
                    if (x.VendorID != viewAsVendorID.ToNullableInt())
                    {
                        x.VendorID = 0;
                        x.VendorName = "***";
                        x.Phone = "***";
                        x.Email = "***";
                        x.Fax = "***";
                        x.WebsiteURL = "************";
                        x.Address1 = "************";
                        x.Address2 = "************";
                        x.Address3 = "************";
                        x.City = "";
                        x.State = "************";
                        x.Zipcode = "************";
                        x.Country = "************";
                        x.Currency = "USD";
                    }
                    x.VendorListItemID = 0;
                    x.ATAChapter = null;
                    x.Aircraft = null;
                    x.Engine = null;
                    x.Cage = null;
                });
            } else
            {
                list.ForEach(x =>
                {
                    if (x.VendorID == queryParams.VendorCerts.ToNullableInt())
                    {
                        var certs = _context.VendorCertGetByVendorID(queryParams.VendorCerts.ToNullableInt()).ToList();
                        StringBuilder concat = new StringBuilder();
                        foreach (var c in certs)
                        {
                            string cert = c.Cert;
                            string number = c.Number;
                            concat.Append(cert + ": " + number + "<br/>");
                        }
                        x.CertifyingAgenciesCerts = concat.ToString();
                    }
                });
            }
            return list;
        }

        public void PopulatePartsPageViewModel(PartsPageViewModel viewModel, string text, bool partPage)
        {
            var result = _context.VendorListItemSearch7(text, SessionVariables.SubscriberID.ToNullableInt(), partPage).Take(Constants.MAX_RECORDS_RETURNED).ToList();
            viewModel.RealCount = result.Count();
            if (viewModel.RealCount >= Constants.MAX_RECORDS_RETURNED)
            {
                viewModel.HasMoreRow = true;
            }
            
            viewModel.SearchResults = result;

            viewModel.SimiliarSearches = _context.VendorListItemSearchSimilar(text, SessionVariables.SubscriberID.ToNullableInt()).ToList();
            viewModel.Merit = Constants.MERITS;

            bool exactMatchFound = false;

            if (viewModel.RealCount > 0)
            {
                if (!partPage)
                {
                    exactMatchFound = viewModel.SearchResults.First().ExactMatchFirst == 0;
                    if (exactMatchFound)
                    {
                        text = viewModel.SearchResults.First().PartNumber;
                    }
                   
                }
                _context.UserSearchInsert3(SessionVariables.UserID.ToNullableInt() ?? 0, text, partPage, viewModel.RealCount, exactMatchFound);
            }
        }

        public List<PartAutoComplete> GetPartAutoCompletes(string text)
        {
            string cleanText = new Regex("[^a-zA-Z0-9]").Replace(text, "");
            if (cleanText.Length < Constants.MIN_SEARCH_LENGTH)
                return new List<PartAutoComplete>();

            return _context.VendorListItemSearch7(text, SessionVariables.SubscriberID.ToNullableInt(), false)                
                .Select(x => new PartAutoComplete {
                    value = x.PartNumber,
                    label = string.IsNullOrEmpty(x.Match) ? x.PartNumber : $"{x.PartNumber} ({x.Match})"
                })
                .ToList();
        }
    }
}