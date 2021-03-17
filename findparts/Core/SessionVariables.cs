using DAL;
using Findparts;
using Findparts.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Unity;

namespace Findparts.Core
{
    public class SessionVariables
    {   
        public SessionVariables()
        {
            
        }

        public static string UserID
        {
            get
            {
                if (HttpContext.Current.Session["_UserID"] == null)
                {
                    Populate("");
                }
                return HttpContext.Current.Session["_UserID"].ToString();
            }
            set { HttpContext.Current.Session["_UserID"] = value; }
        }

        public static string SubscriberID
        {
            get
            {
                if (HttpContext.Current.Session["_SubscriberID"] == null)
                {
                    Populate("");
                }
                return HttpContext.Current.Session["_SubscriberID"].ToString();
            }
            set { HttpContext.Current.Session["_SubscriberID"] = value; }
        }

        public static string VendorID
        {
            get
            {
                if (HttpContext.Current.Session["_VendorID"] == null)
                {
                    Populate("");
                }
                return HttpContext.Current.Session["_VendorID"].ToString();
            }
            set { HttpContext.Current.Session["_VendorID"] = value; }
        }

        public static string Email
        {
            get
            {
                if (HttpContext.Current.Session["_Email"] == null)
                {
                    Populate("");
                    //return "";
                }
                return HttpContext.Current.Session["_Email"].ToString();
            }
            set { HttpContext.Current.Session["_Email"] = value; }
        }
        public static string CompanyName
        {
            get
            {
                if (HttpContext.Current.Session["_CompanyName"] == null)
                {
                    Populate("");
                    //return "";
                }
                return HttpContext.Current.Session["_CompanyName"].ToString();
            }
            set { HttpContext.Current.Session["_CompanyName"] = value; }
        }
        public static bool CanSearch
        {
            get
            {
                if (HttpContext.Current.Session["_CanSearch"] == null)
                {
                    Populate("");
                    //return "";
                }
                return (bool)HttpContext.Current.Session["_CanSearch"];
            }
            set { HttpContext.Current.Session["_CanSearch"] = value; }
        }

        public static void Populate(string userName)
        {
            ApplicationUser user;
            var userManager = System.Web.HttpContext.Current.Request.GetOwinContext()
                                .GetUserManager<ApplicationUserManager>();
            if (userName == "")
            {
                user = userManager.FindById(HttpContext.Current.User.Identity.GetUserId());
            }
            else
            {
                user = userManager.FindByName(userName);
            }
            if (user != null)
            {
                SessionVariables.Email = user.Email;
                string providerUserKey = user.Id;

                using (var context = new FindPartsEntities())
                {
                    var result =  context.UserGetByProviderUserKey2(new Guid(providerUserKey)).ToList();
                    if (result.Count > 0)
                    {
                        SessionVariables.UserID = result[0].UserID.ToString();
                        SessionVariables.SubscriberID = result[0].SubscriberID.ToString();
                        SessionVariables.VendorID = result[0].VendorID.ToString();
                        SessionVariables.CompanyName = result[0].Name.ToString();
                        SessionVariables.CanSearch = Convert.ToBoolean(result[0].CanSearch);
                    }
                }
            }
            else
            {
                // populate session variables:
                SessionVariables.UserID = "";
                SessionVariables.SubscriberID = "";
                SessionVariables.VendorID = "";
                SessionVariables.CompanyName = "";
                SessionVariables.CanSearch = false;
            }
        }

        public static void Clear()
        {
            //SessionVariables.Email = null;
            SessionVariables.UserID = "";
            SessionVariables.SubscriberID = "";
            SessionVariables.VendorID = "";
            SessionVariables.CompanyName = "";
            SessionVariables.CanSearch = false;
        }
    }
}