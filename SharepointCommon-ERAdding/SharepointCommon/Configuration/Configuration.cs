using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.SharePoint;

namespace SharepointCommon.Configuration
{
    internal class Configuration : IDisposable
    {
        private readonly IQueryWeb _queryWeb;

        internal Configuration(Guid site, Guid web)
        {
            _queryWeb = WebFactory.Elevated(site, web);
        }

        public void Dispose()
        {
            _queryWeb.Dispose();
        }

        internal void EnsureEventReceiver(SPList list, Type receiver)
        {
            EnsureConfigStore();
            // todo: write er to config
        }

        private static void EnsureConfigStore()
        {
            //todo: create file in root of content database for store settings
        }

        private static string ReadFile(string fullUrl)
        {
            using (var wf = WebFactory.Elevated(fullUrl))
            {
                SPFile file = wf.Web.GetFile(fullUrl);

                if (file.Exists == false)
                {
                    byte[] buffer = Encoding.UTF8.GetBytes(string.Empty);
                    wf.Web.AllowUnsafeUpdates = true;
                    file = wf.Web.Files.Add(fullUrl, buffer);
                }
                using (var stream = file.OpenBinaryStream())
                {
                    using (var sr = new StreamReader(stream))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
        }

        private static void SaveFile(string fullUrl, string newContent)
        {
            using (var wf = WebFactory.Elevated(fullUrl))
            {
                SPFile file = wf.Web.GetFile(fullUrl);

                byte[] buffer;
                if (file.Exists == false)
                {
                    buffer = Encoding.UTF8.GetBytes(newContent);
                    file = wf.Web.Files.Add(fullUrl, buffer);
                }

                buffer = Encoding.UTF8.GetBytes(newContent);
                using (var ms = new MemoryStream(buffer, true))
                {
                    file.SaveBinary(ms);
                }
            }
        }

        internal class ConfigurationModel
        {
            public IEnumerable<EventReceiver> EventReceivers { get; set; }

            internal class EventReceiver
            {
                public string List { get; set; }
                public string Receicer { get; set; }
            }
        }
    }
}
