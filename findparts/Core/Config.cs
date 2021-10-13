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

        public static int PortalCode => Int32.Parse(ConfigurationManager.AppSettings["PortalCode"]);
        public static string PortalName => ConfigurationManager.AppSettings["PortalName"];

        public static string JitsiPrivateKeyPath => ConfigurationManager.AppSettings["JitsiPrivateKeyPath"];
        public static string JitsiAppId => ConfigurationManager.AppSettings["JitsiAppId"];
        public static string JitsiApiKey => ConfigurationManager.AppSettings["JitsiApiKey"];
        public static string SharedJsonPath => ConfigurationManager.AppSettings["SharedJsonPath"];
    }
}