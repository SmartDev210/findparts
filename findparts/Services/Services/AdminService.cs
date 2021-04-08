﻿using DAL;
using Findparts.Core;
using Findparts.Models.Admin;
using Findparts.Services.Interfaces;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Findparts.Services.Services
{
    public class AdminService : IAdminService
    {
        private readonly FindPartsEntities _context;
        private readonly IMailService _mailService;
        public AdminService(FindPartsEntities context, IMailService mailService)
        {
            _context = context;
            _mailService = mailService;
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

        public VendorListGetByID_Result GetVendorList(int vendorListId)
        {
            return _context.VendorListGetByID(vendorListId).FirstOrDefault();
        }

        public List<T> LoadDataFromExcelFile<T>(string filePath) where T: new()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
         
            if (File.Exists(filePath))
            {
                var fi = new FileInfo(filePath);
                using (var pck = new ExcelPackage(fi))
                {
                    var workbook = pck.Workbook;
                    var worksheet = workbook.Worksheets.First();

                    var noOfRow = worksheet.Dimension.End.Row;

                    var obj = (object[,])worksheet.Cells.Value;

                    bool searchingForData = true;
                    string compareKey = "";
                    int count = 0;

                    int startRow = 0;
                    if ((obj[0, 0]?.ToString() ?? "").Replace(" ", "") == "PartNumber")
                        startRow = 0;
                    else if ((obj[1, 0]?.ToString()??"").Replace(" ", "") == "PartNumber")
                        startRow = 1;
                    else return null;

                    List<T> items = new List<T>();

                    for (var i = startRow + 1; i < noOfRow; i ++)
                    {
                        var item = new T();

                        if (string.IsNullOrEmpty(obj[i, 0]?.ToString()))
                            break;

                        for (var col = 0; col < 23; col ++)
                        {   
                            var propName = obj[startRow, col]?.ToString();
                            if (propName != null)
                                propName = propName.Replace(" ", "");
                            
                            if (string.IsNullOrEmpty(obj[i, col]?.ToString()))
                                continue;

                            var strVal = (obj[i, col]?.ToString() ?? "").Trim();

                            var property = item.GetType().GetProperty(propName);
                            if (property != null)
                            {
                                if (property.PropertyType.Name == "String")
                                {
                                    property.SetValue(item, strVal);                                    
                                } else if (property.PropertyType.Name == "Int32")
                                {
                                    property.SetValue(item, Int32.Parse(strVal));
                                } else if (property.PropertyType.Name.Contains("Nullable"))
                                {
                                    strVal = strVal.ToLower();
                                    if (strVal == "1" || strVal == "y" || strVal == "true" || strVal == "yes" || strVal == "t" || strVal == "x")
                                        property.SetValue(item, true);
                                }
                            }
                        }

                        items.Add(item);
                    }

                    return items;
                }
            }
            return null;
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

        public bool ImportVendorList(VendorListGetByID_Result vendorList, out string message)
        {
            message = "";
            var filePath = Path.Combine(Config.UploadPath, $"{vendorList.VendorListID}{vendorList.Filetype}");

            var items = LoadDataFromExcelFile<VendorListItem>(filePath);
            if (items == null) throw new Exception("Invalid file");

            foreach (var item in items)
            {
                item.VendorID = vendorList.VendorID;
                item.VendorListID = vendorList.VendorListID;
            }
            var distinctItems = items.Distinct(new VendorListComparer()).ToList();

            if (items.Count > distinctItems.Count)
                message += "PartNumber field contained duplicates. " + (items.Count - distinctItems.Count);

            _context.VendorListItems.AddRange(distinctItems);
            _context.SaveChanges();

            _context.VendorListItemUpdateClean();

            if (vendorList.ReplaceList)
            {
                // import to Live (or error)
                // copy old Live to Archive, delete old from Live
                _context.VendorListItemArchiveByVendorIDAndNotEqualVendorListID2(vendorList.VendorID, vendorList.VendorListID);
                /*
                 INSERT INTO VendorListItemArchive(*)
                 SELECT *
                 FROM VendorListItem
                 WHERE VendorID = @VendorID
                    AND VendorListID != @VendorListID

                 DELETE 
                 FROM VendorListItem
                 WHERE VendorID = @VendorID
                    AND VendorListID != @VendorListID
                 */
                // TODO: update FKEYS?
            } else
            {
                // import to Live (or error)
                // if (overlap with existing lists)
                //      copy old overlap Live to Archive
                //      delete old overlap from Live
                int duplicateCount =  _context.VendorListItemArchiveDuplicatesByVendorIDAndNotEqualVendorListID(vendorList.VendorID, vendorList.VendorListID);
                /*
                 INSERT INTO VendorListItemArchive(*)
                 SELECT v1.*
                 FROM VendorListItem v1
                    INNER JOIN VendorListItem v2 ON v1.PartNumber = v2.PartNumber
                 WHERE v1.VendorID = @VendorID
                    AND v1.VendorListID != @VendorListID 
                    AND v2.VendorID = @VendorID
                    AND v2.VendorListID = @VendorListID 

                 DELETE 
                 FROM VendorListItem v1
                    INNER JOIN VendorListItem v2 ON v1.PartNumber = v2.PartNumber
                 WHERE v1.VendorID = @VendorID
                    AND v1.VendorListID != @VendorListID 
                    AND v2.VendorID = @VendorID
                    AND v2.VendorListID = @VendorListID 
                 */
                //      TODO: update FKEYS?
                //      report back to user how many overlap?
            }

            // update from acheivement lists
            _context.VendorListItemUpdateAchievements4(vendorList.VendorID);
            // update from quotes
            _context.VendorListItemUpdateQuoteAchievements2(vendorList.VendorID);
            _context.VendorListUpdateDateApproved(vendorList.VendorListID);
            _context.VendorUpdateStats(vendorList.VendorID);

            var vendor = _context.VendorGetByID(vendorList.VendorID).FirstOrDefault();
            
            if (vendor != null)
            {
                int partsCount = vendor.PartsCount ?? 0;
                int achievementsCount = vendor.AchievementsCount ?? 0;

                // "email all users when Admin Approves:ANY list. Thats Fine"
                var users = _context.UserGetByVendorID(vendorList.VendorID).ToList();
                foreach (var user in users)
                {
                    string vendorEmail = user.Email;
                    _mailService.SendVendorListApprovedEmail(vendorEmail, vendor.VendorName, true, partsCount.ToString(), achievementsCount.ToString());
                }
            }

            return true;
        }

        public VendorAchievementList GetVendorAchievementList(int vendorAchievementId)
        {
            return _context.VendorAchievementListGetByID(vendorAchievementId).FirstOrDefault();
        }

        public void ImportVendorAchievementList(VendorAchievementList vendorAchievementList, out string message)
        {
            message = "";
            var filePath = Path.Combine(Config.UploadPath, $"Achievement_{vendorAchievementList.VendorAchievementListID}{vendorAchievementList.Filetype}");

            var items = LoadDataFromExcelFile<VendorAchievementListItem>(filePath);
            if (items == null) throw new Exception("Invalid file");

            foreach (var item in items)
            {
                item.Id = 0;
                item.VendorAchievementListID = vendorAchievementList.VendorAchievementListID;
            }
            if (items.Count > 0)
            {   
                _context.VendorAchievementListItems.AddRange(items);
                _context.SaveChanges();
            }
            if (vendorAchievementList.ReplaceList)
            {

            } else
            {

            }

            _context.VendorListItemUpdateAchievements4(vendorAchievementList.VendorID);
            _context.VendorAchievementListUpdateDateApproved(vendorAchievementList.VendorAchievementListID);
            _context.VendorUpdateStats(vendorAchievementList.VendorID);

            var vendor = _context.VendorGetByID(vendorAchievementList.VendorID).FirstOrDefault();
            if (vendor != null)
            {
                int partsCount = vendor.PartsCount ?? 0;
                int achievementsCount = vendor.AchievementsCount ?? 0;

                var users = _context.UserGetByVendorID(vendor.VendorID);
                foreach (var user in users)
                {   
                    _mailService.SendVendorListApprovedEmail(user.Email, vendor.VendorName, false, partsCount.ToString(), achievementsCount.ToString());
                }
            }
        }

        public void DeleteVendorList(int vendorListId)
        {
            _context.VendorListDeleteByID(vendorListId);
        }

        public void DeleteAchievementList(int vendorAchievementListId)
        {
            _context.VendorAchievementListDeleteByID(vendorAchievementListId);
        }
    }
    
    class VendorListComparer: IEqualityComparer<VendorListItem>
    {
        
        public bool Equals(VendorListItem x, VendorListItem y)
        {
            return x.PartNumber == y.PartNumber;
        }

        public int GetHashCode(VendorListItem obj)
        {
            return obj.PartNumber.GetHashCode();
        }
    }

    
}