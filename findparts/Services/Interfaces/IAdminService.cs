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
        VendorListGetByID_Result GetVendorList(int vendorListId);
        List<T> LoadDataFromExcelFile<T>(string filePath) where T : new();
        bool ImportVendorList(VendorListGetByID_Result vendorList, out string message);
        VendorAchievementList GetVendorAchievementList(int vendorAchievementId);
        void ImportVendorAchievementList(VendorAchievementList vendorAchievementList, out string message);
        void DeleteVendorList(int vendorListId);
        void DeleteAchievementList(int vendorAchievementListId);
    }
}
