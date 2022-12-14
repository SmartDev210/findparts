using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Findparts.Core
{
    public static class Config
    {
        public static string Environment => ConfigurationManager.AppSettings["Environment"];
        public static string InvoicePath => ConfigurationManager.AppSettings["InvoicePath"];
        public static string UploadPath => ConfigurationManager.AppSettings["UploadPath"];
        public static string LogoPath => ConfigurationManager.AppSettings["LogoPath"];
        public static string SMTPHost => ConfigurationManager.AppSettings["SMTPHost"];
        public static string SMTPPort => ConfigurationManager.AppSettings["SMTPPort"];
        public static string SMTPUsername => ConfigurationManager.AppSettings["SMTPUsername"];
        public static string SMTPPassword => ConfigurationManager.AppSettings["SMTPPassword"];
        public static string AdminEmail => ConfigurationManager.AppSettings["AdminEmail"];
        public static string StripeApiKey => ConfigurationManager.AppSettings["StripeApiKey"];
        public static string StripePublishableApiKey => ConfigurationManager.AppSettings["StripePublishableApiKey"];
        public static string BccEmail => ConfigurationManager.AppSettings["BccEmail"];
        public static string FromEmail => ConfigurationManager.AppSettings["FromEmail"];
        public static string DevEmail => ConfigurationManager.AppSettings["DevEmail"];
        public static string StripeSecret => ConfigurationManager.AppSettings["StripeSecret"];

        public static string StreamChatKey => ConfigurationManager.AppSettings["StreamChatKey"];
        public static string StreamSecretKey => ConfigurationManager.AppSettings["StreamSecretKey"];

        public static string WeavyUrl => ConfigurationManager.AppSettings["WeavyUrl"];
        public static string WeavyClientId => ConfigurationManager.AppSettings["WeavyClientId"];
        public static string WeavyClientSecret => ConfigurationManager.AppSettings["ClientSecret"];

        public static string AppleAppId => ConfigurationManager.AppSettings["AppleAppId"];
        public static string AppleAuthRedirectUri => ConfigurationManager.AppSettings["AppleAuthRedirectUri"];

        public static int PortalCode => Int32.Parse(ConfigurationManager.AppSettings["PortalCode"]);    // 0: MROFinder, 1: FindParts
        public static string PortalName => ConfigurationManager.AppSettings["PortalName"];

        public static string JitsiPrivateKeyPath => ConfigurationManager.AppSettings["JitsiPrivateKeyPath"];
        public static string JitsiAppId => ConfigurationManager.AppSettings["JitsiAppId"];
        public static string JitsiApiKey => ConfigurationManager.AppSettings["JitsiApiKey"];
        public static string SharedJsonPath => ConfigurationManager.AppSettings["SharedJsonPath"];
        public static string FeedbackSpaceId => ConfigurationManager.AppSettings["FeedbackSpaceId"];
        public static string GooglePlayStoreLink => ConfigurationManager.AppSettings["GooglePlayStoreLink"];
        public static string AppStoreLink => ConfigurationManager.AppSettings["AppStoreLink"];
        public static string FindPartsLink => ConfigurationManager.AppSettings["FindPartsLink"];
        public static string MROFinderLink => ConfigurationManager.AppSettings["MROFinderLink"];
        public static string SitemapPath => ConfigurationManager.AppSettings["SitemapPath"];
        public static string AdminBccEmail => ConfigurationManager.AppSettings["AdminBccEmail"];
    }
}