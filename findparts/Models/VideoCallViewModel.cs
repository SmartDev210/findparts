using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Findparts.Models
{
    public class VideoCallViewModel
    {
        public VideoCallViewModel()
        {
            Participants = new List<string>();
        }
        public int VendorId { get; set; }
        public string Token { get; set; }
        public string RoomName { get; set; }
        public string UserEmail { get; set; }
        public List<string> Participants { get; set; }
    }
}