using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.SharePoint;

namespace SharepointCommon.Common
{
    internal class InvariantCultureScope : IDisposable
    {
        private readonly CultureInfo _originalUICulture;

        public InvariantCultureScope(SPWeb web)
        {
            _originalUICulture = Thread.CurrentThread.CurrentUICulture;
            Thread.CurrentThread.CurrentUICulture = new CultureInfo((int)web.Language);
        }

        public void Dispose()
        {
            Thread.CurrentThread.CurrentUICulture = _originalUICulture;
        }
    }
}
