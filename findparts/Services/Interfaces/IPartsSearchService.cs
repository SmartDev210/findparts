using DAL;
using Findparts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Findparts.Services.Interfaces
{
    public interface IPartsSearchService
    {
        void PopulateHomePageViewModel(HomePageViewModel viewModel);
        List<VendorListItemSearchDetail9_Result> GetDetails(PartsSearchQueryParams queryParams, bool isAdmin, bool showResult);
        void PopulatePartsPageViewModel(PartsPageViewModel viewModel, string text, bool partPage);
        List<PartAutoComplete> GetPartAutoCompletes(string text);
        void PreferBlockVendor(string vendorId, bool prefer, string state);
        void SendRFQ(string vendorID, string vendorListItemID, string comments, string rFQID);
        void SendDisabledFeatureEmail(string disabledFeatureEmail);
    }
}
