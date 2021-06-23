using DAL;
using Findparts.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Findparts.Services.Services
{
    public class JitsiService : IJitsiService
    {
        private readonly FindPartsEntities _context;
        private readonly IMailService _emailService;

        public JitsiService(FindPartsEntities context, IMailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }
        
        public List<string> GetVendorUserList(int vendorId)
        {
            var users = _context.UserGetByVendorID(vendorId).ToList();
            return users.Select(x => x.Email).ToList();
        }

        public void SendInvitiationEmails(int vendorId, string userEmail, string meetUrl)
        {
            var users = _context.UserGetByVendorID(vendorId).ToList();
            
            foreach (var user in users)
            {
                _emailService.SendJitsiMeetingInvitationEmail(userEmail, user.Email, meetUrl);
            }
        }
    }
}