using Microsoft.SharePoint.Client;
using System;
using System.Net;
using System.Security;

namespace tpi.SharePointConnector
{
    class Utils
    {
        public static CookieContainer GetO365CookieContainer(SharePointOnlineCredentials credentials, string targetSiteUrl)
        {
            Uri targetSite = new Uri(targetSiteUrl);
            string cookieString = credentials.GetAuthenticationCookie(targetSite);
            CookieContainer container = new CookieContainer();
            string trimmedCookie = cookieString.TrimStart("SPOIDCRL=".ToCharArray());
            container.Add(new Cookie("FedAuth", trimmedCookie, string.Empty, targetSite.Authority));
            return container;
        }
        public static SharePointOnlineCredentials GetO365Credentials(string userName, string passWord)
        {
            SecureString securePassWord = new SecureString();
            foreach (char c in passWord.ToCharArray())
                securePassWord.AppendChar(c);
            SharePointOnlineCredentials credentials = new SharePointOnlineCredentials(userName, securePassWord);
            return credentials;
        }

    }
}
