using DAL;
using Findparts.Core;
using Findparts.Extensions;
using Findparts.Models;
using Findparts.Models.Vendor;
using Findparts.Services.Interfaces;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Findparts.Services.Services
{
    public class VendorService : IVendorService
    {
        private readonly FindPartsEntities _context;
        public VendorService(FindPartsEntities context)
        {
            _context = context;
        }

        public void AddVendorCert(CertsViewModel input)
        {
            _context.VendorCertInsert(input.VendorId.ToNullableInt(), input.Cert, input.Number);
        }

        public void DeleteVendorAchievement(int vendorAchievementId)
        {
            _context.VendorAchievementListDeleteByID(vendorAchievementId);
        }

        public void DeleteVendorCert(int certId)
        {
            _context.VendorCertDelete(certId);
        }

        public void DeleteVendorList(int vendorListId)
        {
            _context.VendorListDeleteByID(vendorListId);
        }

        public List<VendorListItemGetByVendor4_Result> GetMasterVendorList(string vendorID)
        {
            return _context.VendorListItemGetByVendor4(vendorID.ToNullableInt()).ToList();
            
        }

        public string GetVendorAchievementFileName(int vendorAchievementId)
        {
            var vendorAchievement = _context.VendorAchievementListGetByID(vendorAchievementId).FirstOrDefault();
            if (vendorAchievement != null)
            {
                return "Achievement_" + vendorAchievement.VendorAchievementListID.ToString() + vendorAchievement.Filetype;
            }
            return "";
        }

        public VendorGeneralTabViewModel GetVendorGeneralTabViewModel(string vendorID)
        {
            VendorGeneralTabViewModel generalTabModel = new VendorGeneralTabViewModel() { VendorId = vendorID };

            var vendor = _context.VendorGetByID(vendorID.ToNullableInt()).FirstOrDefault();

            if (vendor != null)
            {
                generalTabModel.VendorName = vendor.VendorName;
                generalTabModel.Country = vendor.Country;
                generalTabModel.WebsiteUrl = vendor.WebsiteURL;
                generalTabModel.DefaultCurrency = vendor.DefaultCurrency;
                if (string.IsNullOrEmpty(generalTabModel.DefaultCurrency))
                    generalTabModel.DefaultCurrency = "USD";
            }
            return generalTabModel;
        }
        public RFQPreferencesViewModel GetRFQPreferencesViewModel(string vendorID)
        {
            RFQPreferencesViewModel rfqViewModel = new RFQPreferencesViewModel() { VendorId = vendorID };

            var vendor = _context.VendorGetByID(vendorID.ToNullableInt()).FirstOrDefault();

            if (vendor != null)
            {
                // RFQ

                rfqViewModel.RFQPhone = vendor.RFQPhone;
                rfqViewModel.RFQEmail = vendor.RFQEmail;
                rfqViewModel.RFQWebEmails = vendor.RFQWebEmails;
                rfqViewModel.RFQFax = vendor.RFQFax;
            }
            return rfqViewModel;
        }
        public OEMsViewModel GetOEMsViewModel (string vendorID)
        {
            OEMsViewModel oemViewModel = new OEMsViewModel() { VendorId = vendorID };
            var vendor = _context.VendorGetByID(vendorID.ToNullableInt()).FirstOrDefault();
            if (vendor != null)
            {
                // OEM
                oemViewModel.IsOEM = vendor.OEM ?? false;
                oemViewModel.OEMExclusive = vendor.OEMExclusive ?? false;
                oemViewModel.OEMRequiresRMA = vendor.OEMRequiresRMA ?? false;
            }
            return oemViewModel;
        }
        public CertsViewModel GetCertsViewModel(string vendorID)
        {

            CertsViewModel certsViewModel = new CertsViewModel() { VendorId = vendorID };

            
            certsViewModel.CertList =  _context.VendorCertGetByVendorID(vendorID.ToNullableInt()).ToList();
            certsViewModel.CertSelectList = new List<SelectListItem>()
            {   
                new SelectListItem() { Value = "FAA", Text = "FAA" },
                new SelectListItem() { Value = "EASA", Text = "EASA" },
                new SelectListItem() { Value = "TCCA", Text = "TCCA" },
                new SelectListItem() { Value = "CAAC", Text = "CAAC" },
                new SelectListItem() { Value = "DGCA", Text = "DGCA" },
                new SelectListItem() { Value = "CAA", Text = "CAA" },
                new SelectListItem() { Value = "CAAV", Text = "CAAV" },
                new SelectListItem() { Value = "GACA", Text = "GACA" },
                new SelectListItem() { Value = "JCAB", Text = "JCAB" },
                new SelectListItem() { Value = "ANAB", Text = "ANAB" },
                new SelectListItem() { Value = "DCAT", Text = "DCAT" },
                new SelectListItem() { Value = "", Text = "Other" }
            };
            return certsViewModel;
        }

        public string GetVendorListFileName(int vendorListId)
        {
            var vendorList = _context.VendorListGetByID(vendorListId).FirstOrDefault();
            if (vendorList != null)
            {
                return vendorList.VendorListID.ToString() + vendorList.Filetype;
            }
            return "";
        }

        public object GetVendorQuotesPageViewModel(string vendorId)
        {
            QuotePageViewModel viewModel = new QuotePageViewModel();
            var defaultCurrency = "USD";
            var vendor = _context.VendorGetByID(vendorId.ToNullableInt()).FirstOrDefault();
            if (vendor != null)
                defaultCurrency = vendor.DefaultCurrency;

            viewModel.Quotes = _context.VendorQuoteGetByVendorID6(vendorId.ToNullableInt()).ToList();
            viewModel.DefaultCurrency = defaultCurrency;
            viewModel.AchievementsSelectList = _context.VendorAchievementTypeGetAllQuoteSelectable().ToList().Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.VendorAchievementTypeID.ToString()
            }).ToList();
            
            return viewModel;
        }

        public UploadVendorFileViewModel GetVendorUploadAchievementViewModel(string vendorID, int vendorAchievementId = 0)
        {
            var achievementTypes = _context.VendorAchievementTypeGetAll().ToList();

            UploadVendorFileViewModel viewModel = new UploadVendorFileViewModel()
            {
                Id = vendorAchievementId,
                VendorId = vendorID.ToNullableInt() ?? 0,                
                ReplaceList = true,
                VendorAchievementTypeSelectList = achievementTypes.Select(x => new SelectListItem
                {
                    Value = x.VendorAchievementTypeID.ToString(),
                    Text = x.Name
                }).ToList()
            };

            if (vendorAchievementId > 0)
            {
                var vendorAchievement = _context.VendorAchievementListGetByID(vendorAchievementId).FirstOrDefault();
                if (vendorAchievement != null)
                {
                    viewModel.ReplaceList = vendorAchievement.ReplaceList;
                    viewModel.Comment = vendorAchievement.Comments;
                    viewModel.Approved = vendorAchievement.DateApproved.HasValue;
                    viewModel.VendorAchievementTypeId = vendorAchievement.VendorAchievementTypeID;
                }
            }
            return viewModel;
        }

        public UploadVendorFileViewModel GetVendorUploadCapabilityViewModel(string vendorID, int vendorListId = 0)
        {

            UploadVendorFileViewModel viewModel = new UploadVendorFileViewModel()
            {
                Id = vendorListId,
                VendorId = vendorID.ToNullableInt() ?? 0,
                IsFirst = vendorListId ==0 && _context.VendorListGetByVendorID(vendorID.ToNullableInt(), Config.PortalCode).Count() == 0,
                ReplaceList = true
            };

            if (vendorListId > 0)
            {
                var vendorList = _context.VendorListGetByID(vendorListId).FirstOrDefault();
                if (vendorList != null)
                {
                    viewModel.ReplaceList = vendorList.ReplaceList;
                    viewModel.Comment = vendorList.Comments;
                    viewModel.Approved = vendorList.DateApproved.HasValue;
                }
            }

            return viewModel;
        }

        public VendorUploadListViewModel GetVendorUploadListViewModel(string vendorID)
        {
            var viewModel = new VendorUploadListViewModel();
            var vendorList = _context.VendorListGetByVendorID(vendorID.ToNullableInt(), Config.PortalCode).ToList();
            foreach (var item in vendorList)
            {
                viewModel.VendorCapabilityList.Add(new VendorFileViewModel
                {
                    Id = item.VendorListID,
                    DateApproved = item.DateApproved,
                    DateCreated = item.DateCreated,
                    Comments = item.Comments,
                    FileType = item.Filetype,
                    ReplaceList = item.ReplaceList,
                });
            }

            var vendorAchievementList = _context.VendorAchievementListGetByVendorID2(vendorID.ToNullableInt());
            foreach (var item in vendorAchievementList)
            {
                viewModel.VendorAchievementList.Add(new VendorFileViewModel
                {
                    Id = item.VendorAchievementListID,
                    DateApproved = item.DateApproved,
                    DateCreated = item.DateCreated,
                    Comments = item.Comments,
                    FileType = item.Filetype,
                    ReplaceList = item.ReplaceList,
                    AchievementType = item.Name
                });
            }
            return viewModel;
        }

        public void UpdateVendorAddress(AddressViewModel input)
        {
            _context.VendorUpdateAddress2(input.VendorId.ToNullableInt(), input.Address1, input.Address2, input.Address3, input.City, input.State, input.Zipcode, input.Country, input.Phone);
        }

        public void UpdateVendorGeneral(VendorGeneralTabViewModel input)
        {
            _context.VendorUpdateGeneral(input.VendorId.ToNullableInt(), input.WebsiteUrl, input.DefaultCurrency);
        }

        public void UpdateVendorOEM(OEMsViewModel input)
        {
            _context.VendorUpdateOEM(input.VendorId.ToNullableInt(), input.IsOEM, input.OEMExclusive, input.OEMRequiresRMA);
        }

        public void UpdateVendorRFQPrefs(RFQPreferencesViewModel input)
        {
            _context.VendorUpdateRFQPrefs(input.VendorId.ToNullableInt(), input.RFQPhone, input.RFQEmail, input.RFQWebEmails, input.RFQFax);
        }

        public bool UploadVendorAchievement(UploadVendorFileViewModel viewModel, string fileType)
        {
            

            if (viewModel.Upload != null && viewModel.Upload.ContentLength > 0)
            {
                var achievementId = _context.VendorAchievementListUpdate(viewModel.Id, viewModel.VendorId, viewModel.VendorAchievementTypeId, viewModel.Comment, fileType, viewModel.ReplaceList).FirstOrDefault();
                var fileName = $"Achievement_{achievementId}{fileType}";
                viewModel.Upload.SaveAs(Path.Combine(Config.UploadPath, fileName));
            } else if (viewModel.Id > 0)
            {
                _context.VendorAchievementListUpdate(viewModel.Id, viewModel.VendorId, viewModel.VendorAchievementTypeId, viewModel.Comment, fileType, viewModel.ReplaceList);
            }
            
            return true;
        }

        public bool UploadVendorList(UploadVendorFileViewModel viewModel, string filetype)
        {
            
            if (viewModel.Upload != null && viewModel.Upload.ContentLength > 0)
            {
                var vendorListId = _context.VendorListUpdate2(viewModel.Id, SessionVariables.UserID.ToNullableInt(), viewModel.VendorId, viewModel.Comment, filetype, viewModel.ReplaceList, Config.PortalCode).FirstOrDefault();
                string fileName = $"{vendorListId}{filetype}";

                var path = Config.UploadPath;
                if (!File.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                viewModel.Upload.SaveAs(Path.Combine(path, fileName));
            } else if (viewModel.Id > 0)
            {
                _context.VendorListUpdate2(viewModel.Id, SessionVariables.UserID.ToNullableInt(), viewModel.VendorId, viewModel.Comment, filetype, viewModel.ReplaceList, Config.PortalCode);
            }
            
            return true;
        }

        public void VendorQuoteUpdate(string vendorQuoteIDInput, string vendorId, string currency, string testPrice, string testTAT, string repairPrice, string repairPriceRangeLow, string repairPriceRangeHigh, string repairTAT, string overhaulPrice, string overhaulPriceRangeLow, string overhaulPriceRangeHigh, string overhaulTAT, string notToExceed, bool repairsFrequently, bool pma, bool der, bool freeEval, bool modified, bool functionTestOnly, bool noOverhaulWorkscope, bool caac, bool extendedWarranty, bool flatRate, bool range, bool nte, string quoteComments)
        {
            _context.VendorQuoteUpdate4(vendorQuoteIDInput.ToNullableInt(), vendorId.ToNullableInt(), currency, testPrice.ToNullableInt(), testTAT.ToNullableInt(), repairPrice.ToNullableInt(), repairPriceRangeLow.ToNullableInt(), repairPriceRangeHigh.ToNullableInt(), repairTAT.ToNullableInt(), overhaulPrice.ToNullableInt(), overhaulPriceRangeLow.ToNullableInt(), overhaulPriceRangeHigh.ToNullableInt(), overhaulTAT.ToNullableInt(), notToExceed.ToNullableInt(), repairsFrequently, pma, der, freeEval, modified, functionTestOnly, noOverhaulWorkscope, caac, extendedWarranty, flatRate, range, nte, quoteComments);

            _context.VendorListItemUpdateQuoteAchievements(vendorId.ToNullableInt());
        }

        public void VendorQuoteUpdateIgnore(string vendorQuoteID, string vendorId)
        {
            _context.VendorQuoteUpdateIgnore(vendorQuoteID.ToNullableInt(), vendorId.ToNullableInt());
        }

        public void VendorQuoteUpdateNoQuote(string vendorQuoteId, string vendorId)
        {
            _context.VendorQuoteUpdateNoQuote(vendorQuoteId.ToNullableInt(), vendorId.ToNullableInt());
        }

        public VendorAdvertiseViewModel GetAdvertiseViewModel(string vendorID)
        {
            int vendorId = vendorID.ToInt();
            var list = _context.VendorPurchases.Where(x => x.VendorId == vendorId).OrderBy(x => x.PurchasedAt).ToList();
            var viewModel = new VendorAdvertiseViewModel()
            {
                VendorPurchases = list
            };
            return viewModel;
        }
    }
}