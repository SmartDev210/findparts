using DAL;
using Findparts.Core;
using Findparts.Models.Admin;
using Findparts.Services.Interfaces;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Xml;

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

            viewModel.VendorList = _context.VendorListGetByVendorID(vendorId, Config.PortalCode).ToList();
            //viewModel.VendorAchievementList = _context.VendorAchievementListGetByVendorID2(vendorId).ToList();

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
                    DateCreated = x.DateCreated.ToString("MMM d, yyyy HH:mm:dd"),
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
                    AchievementsApproved = x.AchievementsApproved,
                    WeavyCompanyId = x.WeavyCompanyId
                }).ToList()
            };
        }

        public SubscriberPagedListViewModel GetSubscribers(int start, int length, int draw, string sortParam, string direction, string filter)
        {
            var query = _context.SubscriberGetAll2().ToList();

            if (direction == "asc")
            {
                query = query.OrderBy(x =>
                {
                    var property = x.GetType().GetProperty(sortParam);
                    if (property != null)
                    {
                        return property.GetValue(x);
                    }
                    else return x.SubscriberName;
                })
                    .ToList();
            }
            else
            {
                query = query.OrderByDescending(x =>
                {
                    var property = x.GetType().GetProperty(sortParam);
                    if (property != null)
                    {
                        return property.GetValue(x);
                    }
                    else return x.SubscriberName;
                })
                    .ToList();
            }

            IEnumerable<SubscriberGetAll2_Result> filteredResult = query;
            if (!string.IsNullOrEmpty(filter))
            {
                filteredResult = query.Where(x => x.SubscriberName.Contains(filter) || x.Email.Contains(filter) || x.Status.Contains(filter));
            }

            return new SubscriberPagedListViewModel
            {
                Draw = draw,
                TotalRecords = query.Count(),
                FilteredRecords = filteredResult.Count(),
                Length = length,
                Start = start,
                Subscribers = filteredResult.Skip(start).Take(length).Select(x => new SubscriberViewModel
                {
                    SubscriberID = x.SubscriberID,
                    SubscriberName = x.SubscriberName,
                    Email = x.Email,                    
                    Status = x.Status,
                    DateCreated = x.DateCreated.ToString("MMM d, yyyy HH:mm:dd"),
                    DateActivated = x.DateActivated.HasValue ? x.DateActivated.Value.ToString("MMM d, yyyy") : "",
                    UserID = x.UserID,
                    ProfileFieldsCompleted = x.ProfileFieldsCompleted,
                    MembershipLevel = x.MembershipLevel,
                    SearchCount = x.SearchCount ?? 0,
                    InvoiceCount = x.InvoiceCount ?? 0,
                    UserCount = x.UserCount ?? 0,
                    RecentInvoiceDate = x.RecentInvoiceDate.HasValue ? x.RecentInvoiceDate.Value.ToString("MMM d, yyyy") : "",
                    RFQSentCount = x.RFQSentCount ?? 0,
                    QuotesReceivedCount = x.QuotesReceivedCount ?? 0,
                    PreferredCount = x.PreferredCount ?? 0,
                    BlockedCount = x.BlockedCount ?? 0,
                    BlockedByVendorCount = x.BlockedByVendorCount,
                    MRO = x.MRO ?? false,
                    EmailDomains = x.EmailDomains ?? 0
                }).ToList()
            };
        }

        public VendorList GetVendorList(int vendorListId)
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
                    var noOfColumn = worksheet.Dimension.End.Column;

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

                        for (var col = 0; col < noOfColumn; col ++)
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

        public bool ImportVendorList(VendorList vendorList, out string message)
        {
            message = "";
            var filePath = Path.Combine(Config.UploadPath, $"{vendorList.VendorListID}{vendorList.Filetype}");

            var items = LoadDataFromExcelFile<VendorListItem>(filePath);
            if (items == null) throw new Exception("Invalid file");

            foreach (var item in items)
            {
                item.VendorID = vendorList.VendorID;
                item.VendorListID = vendorList.VendorListID;
                item.PortalCode = vendorList.PortalCode;
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
                var existingItems = _context.VendorListItems.Where(x => x.VendorID == vendorList.VendorID && x.VendorListID != vendorList.VendorListID && x.PortalCode == Config.PortalCode).ToList();

                var archiveItems = existingItems.Select(x => new VendorListItemArchive
                    {
                        PartNumber = x.PartNumber,
                        PMA = x.PMA,
                        DER = x.DER,
                        Description = x.Description,
                        Aircraft = x.Aircraft,
                        AlternatePartNumber = x.AlternatePartNumber,
                        AlternatePartNumber2 = x.AlternatePartNumber2,
                        ATAChapter = x.ATAChapter,
                        CAAC = x.CAAC,
                        Cage = x.Cage,
                        NSN = x.NSN,
                        WorkShopSite = x.WorkShopSite,
                        Engine = x.Engine,
                        ExtendedWarranty = x.ExtendedWarranty,
                        FlatRate = x.FlatRate,
                        FreeEval = x.FreeEval,
                        FunctionTestOnly = x.FunctionTestOnly,
                        RepairsFrequently = x.RepairsFrequently,
                        Manufacturer = x.Manufacturer,
                        ModelNumber = x.ModelNumber,
                        Modified = x.Modified,
                        NoOverhaulWorkscope = x.NoOverhaulWorkscope,
                        NotesRemarks = x.NotesRemarks,
                        NTE = x.NTE,
                        Range = x.Range,
                        VendorID = x.VendorID,
                        VendorListID = x.VendorListID,
                        VendorListItemID = x.VendorListItemID,
                        Workscope = x.Workscope,
                    });
                _context.VendorListItemArchives.AddRange(archiveItems);
                _context.VendorListItems.RemoveRange(existingItems);
                _context.SaveChanges();
                /*
                _context.VendorListItemArchiveByVendorIDAndNotEqualVendorListID2(vendorList.VendorID, vendorList.VendorListID);
                */
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
                    _mailService.SendVendorListApprovedEmail(vendorEmail, vendor.VendorName, true, partsCount.ToString(), achievementsCount.ToString(), vendorList.PortalCode);
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
                    _mailService.SendVendorListApprovedEmail(user.Email, vendor.VendorName, false, partsCount.ToString(), achievementsCount.ToString(), Config.PortalCode);
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

        public SubscriberDetailViewModel GetSubscriberDetailViewModel(int subscriberId)
        {
            SubscriberDetailViewModel viewModel = new SubscriberDetailViewModel();

            var subscriber = _context.SubscriberGetByID(subscriberId).FirstOrDefault();
            if (subscriber == null) return null;

            viewModel.SubscriberId = subscriber.SubscriberID;
            viewModel.SubscriberName = subscriber.SubscriberName;
            viewModel.StatusId = subscriber.StatusID;
            viewModel.Notes = subscriber.Notes;

            viewModel.SignupSubscriberTypeId = subscriber.SignupSubscriberTypeID;

            if (viewModel.SignupSubscriberTypeId.HasValue)
            {
                if (viewModel.SignupSubscriberTypeId.Value == (int)SubscriberTypeID.NoCreditCard)
                {
                    viewModel.SignupSubscriberTypeText = "Pay by Check or Wire";
                } else
                {
                    viewModel.SignupSubscriberTypeText = ((SubscriberTypeID)viewModel.SignupSubscriberTypeId).ToString();
                }
            }

            var user = _context.UserGetFirstBySubscriberID(subscriberId).FirstOrDefault();
            if (user != null)
            {
                viewModel.VendorId = user.VendorID;
            }

            viewModel.StatusSelectList = _context.StatusGetAll().ToList().Select(x => new SelectListItem
            {
                Value = x.StatusID.ToString(),
                Text = x.Status
            }).ToList();

            return viewModel;
        }

        public bool UpdateSubscriberDetail(SubscriberDetailViewModel viewModel)
        {
            var status = _context.StatusGetByID(viewModel.StatusId).FirstOrDefault();

            if (status != null)
            {
                bool canSearch = status.CanSearch;

                _context.SubscriberUpdateStatusAndNotes3(viewModel.SubscriberId, viewModel.SubscriberName, viewModel.StatusId, canSearch, viewModel.Notes);

                // update vendor status if joint account
                var user = _context.UserGetFirstBySubscriberID(viewModel.SubscriberId).FirstOrDefault();
                if (user != null)
                {
                    if (user.VendorID != null)
                    {
                        _context.VendorUpdateStatus3(user.VendorID, viewModel.StatusId);
                    }
                }

                return true;
            }
            return false;
        }
        private string CleanInvalidXmlChars(string text)
        {
            // From xml spec valid chars: 
            // #x9 | #xA | #xD | [#x20-#xD7FF] | [#xE000-#xFFFD] | [#x10000-#x10FFFF]     
            // any Unicode character, excluding the surrogate blocks, FFFE, and FFFF. 
            string re = @"[^\x09\x0A\x0D\x20-\xD7FF\xE000-\xFFFD\x10000-x10FFFF]";
            return Regex.Replace(text, re, "");
        }
        private void WriteOneUrl(XmlWriter writer, string domain, string urlPath)
        {
            urlPath = CleanInvalidXmlChars(urlPath);

            if (!urlPath.StartsWith("/"))
                urlPath = "/" + urlPath;

            writer.WriteStartElement("url");
            writer.WriteElementString("loc", $"{domain}{urlPath}");
            writer.WriteRaw("\n  ");

            writer.WriteEndElement();
            writer.Flush();
        }
        private void WriteStartElement(XmlWriter writer)
        {
            writer.WriteStartElement("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");
            writer.WriteAttributeString("xmlns", "xhtml", null, "http://www.w3.org/1999/xhtml");
            writer.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
            writer.WriteAttributeString("xsi", "schemaLocation", null, "http://www.sitemaps.org/schemas/sitemap/0.9 http://www.sitemaps.org/schemas/sitemap/0.9/sitemap.xsd");
        }
        private XmlWriter CreateSitemapFile(string portalCode, string filename)
        {
            try
            {
                if (!Directory.Exists(Path.Combine(Config.SitemapPath, portalCode)))
                    Directory.CreateDirectory(Path.Combine(Config.SitemapPath, portalCode));

                var path = Path.Combine(Config.SitemapPath, portalCode, filename);

                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.IndentChars = ("  ");

                return XmlWriter.Create(path, settings);
            }
            catch (Exception)
            {
                return null;
            }

        }
        public void GenerateSitemaps(int portalCode)
        {
            string domain = Config.PortalCode == 0 ? Config.MROFinderLink : Config.FindPartsLink;
            if (domain.EndsWith("/"))
                domain = domain.Substring(0, domain.Length - 1);

            int limit = 50000, last_id = 0;
            int count = 0;
            while (true)
            {
                using (XmlWriter writer = CreateSitemapFile(portalCode.ToString(), $"sitemap-{count}.xml"))
                {
                    if (writer == null)
                        continue;
                    try
                    {
                        WriteStartElement(writer);

                        var vendorListItems = _context.VendorListItems
                            .AsNoTracking()
                            .Where(x => x.PortalCode == portalCode)
                            .Where(x => x.VendorListItemID > last_id)
                            .OrderBy(x => x.VendorListItemID)
                            .Take(limit).ToList();
                        
                        foreach (var item in vendorListItems)
                        {   
                            WriteOneUrl(writer, domain, $"parts?PartNumber={item.PartNumber}");
                            if (last_id < item.VendorListItemID) last_id = item.VendorListItemID;
                        }
                        
                        writer.WriteEndElement();
                        writer.Flush();

                        if (vendorListItems.Count == 0 || vendorListItems.Count < limit) break;
                        
                        vendorListItems.Clear();
                        
                    }
                    finally
                    {
                        writer.Close();
                    }
                }
                count++;
            }

            using (XmlWriter writer = CreateSitemapFile(portalCode.ToString(), $"sitemap-main.xml"))
            {
                writer.WriteStartElement("sitemapindex", "http://www.sitemaps.org/schemas/sitemap/0.9");
                for (var i = 0; i <= count; i++)
                {
                    writer.WriteStartElement("sitemap");
                    writer.WriteElementString("loc", $"{domain}/sitemaps/sitemap-{i}.xml");
                    writer.WriteRaw("\n  ");

                    writer.WriteEndElement();

                }
                writer.WriteEndElement();
                writer.Flush();
                writer.Close();
            }
        }

        public void SendAllTestEmails()
        {
            var vendorQuote = _context.VendorQuotes.FirstOrDefault();
            _mailService.SendVendorRFQEmail(vendorQuote.VendorQuoteID.ToString(), vendorQuote.VendorID.ToString());
            _mailService.SendVendorUploadEmail(Config.AdminEmail, "Test Vendor", true);
            _mailService.SendVendorListApprovedEmail(Config.AdminEmail, "Test Vendor", true, "100", "100", Config.PortalCode);
            _mailService.SendJitsiMeetingInvitationEmail("test user", Config.AdminEmail, "https://meeting-url");
            _mailService.SendStripeChargeSucceededEmail(Config.AdminEmail, "test vendor", 10000);
            _mailService.SendStripeChargeFailedEmail(Config.AdminEmail, "test vendor");
        }
    }
    
    class VendorListComparer: IEqualityComparer<VendorListItem>
    {
        
        public bool Equals(VendorListItem x, VendorListItem y)
        {
            return x.PartNumber == y.PartNumber && x.PortalCode == y.PortalCode;
        }

        public int GetHashCode(VendorListItem obj)
        {
            return obj.PartNumber.GetHashCode();
        }
    }

    
}