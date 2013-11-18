using System;

namespace SharepointCommon
{
    public class Settings
    {
        public static string GetTestSiteCollectionUrl()
        {
            return string.Format("http://{0}/", Environment.MachineName);
        }
    }
}
