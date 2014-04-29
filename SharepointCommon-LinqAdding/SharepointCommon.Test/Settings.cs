using System.Linq;
using Microsoft.SharePoint.Administration;

namespace SharepointCommon.Test
{
    public class Settings
    {
        private static string _url;

        public static string TestSiteUrl
        {
            get
            {
                if (_url != null) return _url;

                var unitTestedSpWeb = SPFarm.Local.Services.OfType<SPWebService>()
                    .SelectMany(ws => ws.WebApplications)
                    .Where(apps => !apps.IsAdministrationWebApplication)
                    .SelectMany(apps => apps.Sites)
                    .SelectMany(sites => sites.AllWebs)
                    .FirstOrDefault();

                if (unitTestedSpWeb == null)
                {
                    throw new SharepointCommonException("cannot find any sp web to run unit tests!");
                }

                _url = unitTestedSpWeb.Url;

                return _url;
            }
        }
    }
}
