using DAL;
using Findparts.Models.Admin;
using Findparts.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Findparts.Services.Services
{
    public class AdminService : IAdminService
    {
        private readonly FindPartsEntities _context;
        public AdminService(FindPartsEntities context)
        {
            _context = context;
        }

        public VendorDetailViewModel GetVendorDetailViewModel(int vendorId)
        {
            VendorDetailViewModel viewModel = new VendorDetailViewModel();
            
            var vendor = _context.VendorGetByID(vendorId).FirstOrDefault();
            viewModel.VendorId = vendor.VendorID;
            viewModel.VendorName = vendor.VendorName;
            viewModel.Status = vendor.StatusID;
            viewModel.MuteWorkscopeIcons = vendor.Muted;
            viewModel.DateCreated = vendor.DateCreated;
            viewModel.Notes = vendor.Notes;

            viewModel.VendorList = _context.VendorListGetByVendorID(vendorId).ToList();
            viewModel.VendorAchievementList = _context.VendorAchievementListGetByVendorID2(vendorId).ToList();

            var user = _context.UserGetByVendorID(vendorId).FirstOrDefault();
            if (user != null)
                viewModel.SubscriberId = user.SubscriberID;

            viewModel.StatusSelectList = _context.StatusGetAll().ToList().Select(x => new SelectListItem
            {
                Value = x.StatusID.ToString(),
                Text = x.Status
            }).ToList();

            return viewModel;
        }

        public VendorPagedListViewModel GetVendors(int start, int length, int draw, string sortParam, string direction, string filter)
        {
            var query = _context.VendorGetAll().ToList();

            if (direction == "asc")
            {
                query = query.OrderBy(x =>
                    {
                        var property = x.GetType().GetProperty(sortParam);
                        if (property != null)
                        {
                            return property.GetValue(x);
                        }
                        else return x.VendorName;
                    })
                    .ToList();
            } else
            {
                query = query.OrderByDescending(x =>
                    {
                        var property = x.GetType().GetProperty(sortParam);
                        if (property != null)
                        {
                            return property.GetValue(x);
                        }
                        else return x.VendorName;
                    })
                    .ToList();
            }

            IEnumerable<VendorGetAll_Result> filteredResult = query;
            if (!string.IsNullOrEmpty(filter))
            {
                filteredResult = query.Where(x => x.VendorName.Contains(filter) || x.Email.Contains(filter) || x.Status.Contains(filter));
            }
            
            return new VendorPagedListViewModel
            {
                Draw = draw,
                TotalRecords = query.Count(),
                FilteredRecords = filteredResult.Count(),
                Length = length,
                Start = start,
                Vendors = filteredResult.Skip(start).Take(length).Select(x => new VendorViewModel
                {
                    VendorID = x.VendorID,
                    VendorName = x.VendorName,
                    Email = x.Email,
                    Active = x.Active,
                    Status = x.Status,
                    DateCreated = x.DateCreated.ToString("MMM d, yyyy"),
                    DateActivated = x.DateActivated.HasValue ? x.DateActivated.Value.ToString("MMM d, yyyy") : "",
                    UserID = x.UserID,
                    ProfileFieldsCompleted = x.ProfileFieldsCompleted,
                    MuteStatus = x.MuteStatus,
                    OEM = x.OEM,
                    PreferredCount = x.PreferredCount,
                    BlockedCount = x.BlockedCount,
                    RecentListApprovalDate = x.RecentListApprovalDate.HasValue ? x.RecentListApprovalDate.Value.ToString("MMM d, yyyy") : "",
                    RFQReceivedCount = x.RFQReceivedCount,
                    QuotesSentCount = x.QuotesSentCount,
                    PartsCount = x.PartsCount,
                    AchievementsCount = x.AchievementsCount,
                    ListsApprovalNeeded = x.ListsApprovalNeeded,
                    ListsApproved = x.ListsApproved,
                    AchievementsApprovalNeeded = x.AchievementsApprovalNeeded,
                    AchievementsApproved = x.AchievementsApproved
                }).ToList()
            };
        }

        public void SaveVendorStatusAndNotes(VendorDetailViewModel viewModel)
        {
            _context.VendorUpdateStatusAndNotes2(viewModel.VendorId, viewModel.VendorName, viewModel.Status, viewModel.MuteWorkscopeIcons, viewModel.Notes);
            var subscriber = _context.UserGetByVendorID(viewModel.VendorId).FirstOrDefault();
            if (subscriber != null)
            {
                var status = _context.StatusGetByID(viewModel.Status).FirstOrDefault();
                if (status != null)
                {
                    bool canSearch = status.CanSearch;

                    _context.SubscriberUpdateStatus3(subscriber.SubscriberID, viewModel.Status, canSearch);
                }
            }

        }
    }
}