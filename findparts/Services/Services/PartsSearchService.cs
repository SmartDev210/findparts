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
        private readonly IMailService _mailService;
        

        public PartsSearchService(FindPartsEntities context, IMailService mailService)
        {
            _context = context;
            _mailService = mailService;
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
            
            var stats = AppCache.VendorListItemGetStats(Config.PortalCode);
            viewModel.Portal0Stat = AppCache.VendorListItemGetStats(0);
            viewModel.Portal1Stat = AppCache.VendorListItemGetStats(1);
            
            viewModel.RecentSearches = _context.UserSearchGetRecent3().ToList();
        }

        public List<VendorListItemSearchDetail9_Result> GetDetails(PartsSearchQueryParams queryParams, bool isAdmin, bool showResult)
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

            var list = _context.VendorListItemSearchDetail9(queryParams.PartNumberDetail, SessionVariables.SubscriberID.ToNullableInt(), Config.PortalCode).ToList();

            if (!showResult)
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

        public void PopulatePartsPageViewModel(PartsPageViewModel viewModel, string text, bool exactMatch)
        {
            var result = _context.VendorListItemSearch7(text, SessionVariables.SubscriberID.ToNullableInt(), exactMatch, Config.PortalCode).Take(Constants.MAX_RECORDS_RETURNED).ToList();
            viewModel.RealCount = result.Count();
            if (viewModel.RealCount >= Constants.MAX_RECORDS_RETURNED)
            {
                viewModel.HasMoreRow = true;
            }
            
            viewModel.SearchResults = result;

            //viewModel.SimiliarSearches = _context.VendorListItemSearchSimilar(text, SessionVariables.SubscriberID.ToNullableInt(), Config.PortalCode).ToList();
            viewModel.Merit = Constants.MERITS;

            bool exactMatchFound = false;

            if (viewModel.RealCount > 0)
            {
                if (!exactMatch)
                {
                    exactMatchFound = viewModel.SearchResults.First().ExactMatchFirst == 0;
                    if (exactMatchFound)
                    {
                        text = viewModel.SearchResults.First().PartNumber;
                    }
                   
                }
                _context.UserSearchInsert3(SessionVariables.UserID.ToNullableInt() ?? 0, text, exactMatch, viewModel.RealCount, exactMatchFound);
            }
        }

        public List<PartAutoComplete> GetPartAutoCompletes(string text)
        {
            string cleanText = new Regex("[^a-zA-Z0-9]").Replace(text, "");
            if (cleanText.Length < Constants.MIN_SEARCH_LENGTH)
                return new List<PartAutoComplete>();

            return _context.VendorListItemSearch7(text, SessionVariables.SubscriberID.ToNullableInt(), false, Config.PortalCode)                
                .Select(x => new PartAutoComplete {
                    value = x.PartNumber,
                    label = string.IsNullOrEmpty(x.Match) ? x.PartNumber : $"{x.PartNumber} ({x.Match})"
                })
                .ToList();
        }

        public void PreferBlockVendor(string vendorId, bool prefer, string state)
        {
            if (prefer)
            {
                if (state == "1")
                {
                    _context.VendorSubscriberPreferredInsert(vendorId.ToNullableInt(), SessionVariables.SubscriberID.ToNullableInt());
                } else
                {
                    _context.VendorSubscriberPreferredDelete(vendorId.ToNullableInt(), SessionVariables.SubscriberID.ToNullableInt());
                }
            } else
            {
                _context.VendorSubscriberBlockedInsert(vendorId.ToNullableInt(), SessionVariables.SubscriberID.ToNullableInt());
            }
        }

        public void SendRFQ(string vendorID, string vendorListItemID, string comments, string rFQID)
        {
            var userId = SessionVariables.UserID;
            var vendorQuoteId = _context.VendorQuoteInsert3(vendorID.ToNullableInt(), vendorListItemID.ToNullableInt(), userId.ToNullableInt(), comments, rFQID, Config.PortalCode).FirstOrDefault();

            _mailService.SendVendorRFQEmail(vendorQuoteId.ToString(), vendorID);
        }

        public void SendDisabledFeatureEmail(string disabledFeatureEmail)
        {
            _mailService.SendDisabledFeatureEmail(disabledFeatureEmail);
        }
    }
}