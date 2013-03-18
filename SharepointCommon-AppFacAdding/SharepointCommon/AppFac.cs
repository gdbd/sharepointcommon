using System;

namespace SharepointCommon
{
    public class AppFac<T> where T : AppBase
    {
        public static T GetCurrent()
        {
            return (T)Activator.CreateInstance(typeof(T), WebFactory.CurrentContext());
        }

        public static T Open(Guid siteId, Guid webId)
        {
            return (T)Activator.CreateInstance(typeof(T), WebFactory.Open(siteId, webId));
        }

        public static T Open(string webUrl)
        {
            return (T)Activator.CreateInstance(typeof(T), WebFactory.Open(webUrl));
        }
    }

    public class AppBase
    {
        public AppBase(IQueryWeb queryWeb)
        {
            QueryWeb = queryWeb;
        }

        protected IQueryWeb QueryWeb { get; set; }
    }
}