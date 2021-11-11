using DAL;
using Findparts.Models.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Findparts.Services.Interfaces
{
    public interface IAdminService
    {
        VendorPagedListViewModel GetVendors(int start, int length, int draw, string sortParam, string direction, string filter);
        VendorDetailViewModel GetVendorDetailViewModel(int vendorId);
        void SaveVendorStatusAndNotes(VendorDetailViewModel viewModel);
        VendorList GetVendorList(int vendorListId);
        List<T> LoadDataFromExcelFile<T>(string filePath) where T : new();
        bool ImportVendorList(VendorList vendorList, out string message);
        VendorAchievementList GetVendorAchievementList(int vendorAchievementId);
        void ImportVendorAchievementList(VendorAchievementList vendorAchievementList, out string message);
        void DeleteVendorList(int vendorListId);
        void DeleteAchievementList(int vendorAchievementListId);
        SubscriberPagedListViewModel GetSubscribers(int start, int length, int draw, string sortParam, string direction, string filter);
        SubscriberDetailViewModel GetSubscriberDetailViewModel(int subscriberId);
        bool UpdateSubscriberDetail(SubscriberDetailViewModel viewModel);
        void GenerateSitemaps(int portalCode);
    }
}
