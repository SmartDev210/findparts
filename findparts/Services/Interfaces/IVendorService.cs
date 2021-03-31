using DAL;
using Findparts.Models;
using Findparts.Models.Vendor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Findparts.Services.Interfaces
{
    public interface IVendorService
    {
        VendorIndexPageViewModel GetVendorIndexPageViewModel(string vendorID);
        void UpdateVendorGeneral(VendorGeneralTabViewModel input);
        void UpdateVendorRFQPrefs(RFQPreferencesViewModel input);
        void AddVendorCert(CertsViewModel input);
        void UpdateVendorAddress(AddressViewModel input);
        void UpdateVendorOEM(OEMsViewModel input);
        void DeleteVendorCert(int certId);
        VendorUploadListViewModel GetVendorUploadListViewModel(string vendorID);
        UploadVendorFileViewModel GetVendorUploadCapabilityViewModel(string vendorID, int vendorListId = 0);
        bool UploadVendorList(UploadVendorFileViewModel viewModel, string fileType);
        string GetVendorListFileName(int vendorListId);
        UploadVendorFileViewModel GetVendorUploadAchievementViewModel(string vendorID, int vendorAchievementId = 0);
        bool UploadVendorAchievement(UploadVendorFileViewModel viewModel, string fileType);
        string GetVendorAchievementFileName(int vendorAchievementId);
        List<VendorListItemGetByVendor4_Result> GetMasterVendorList(string vendorID);
        void DeleteVendorList(int vendorListId);
        void DeleteVendorAchievement(int vendorAchievementId);
        void VendorQuoteUpdateNoQuote(string vendorQuoteId, string vendorId);
        object GetVendorQuotesPageViewModel(string vendorId);
        void VendorQuoteUpdateIgnore(string vendorQuoteID, string vendorId);
        void VendorQuoteUpdate(string vendorQuoteIDInput, string vendorId, string currency, string testPrice, string testTAT, string repairPrice, string repairPriceRangeLow, string repairPriceRangeHigh, string repairTAT, string overhaulPrice, string overhaulPriceRangeLow, string overhaulPriceRangeHigh, string overhaulTAT, string notToExceed, bool repairsFrequently, bool pma, bool der, bool freeEval, bool modified, bool functionTestOnly, bool noOverhaulWorkscope, bool caac, bool extendedWarranty, bool flatRate, bool range, bool nte, string quoteComments);
    }
}
