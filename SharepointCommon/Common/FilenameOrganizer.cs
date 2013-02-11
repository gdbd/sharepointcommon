using System;
using System.IO;

namespace SharepointCommon.Common
{
    internal class FilenameOrganizer
    {
        internal static string AppendSuffix(string sourceName, Func<string, bool> checkUnique, int retryLimit)
        {
            if (checkUnique(sourceName)) return sourceName;

            string filename = Path.GetFileNameWithoutExtension(sourceName);
            string extention = Path.GetExtension(sourceName);

            int index = 2;

            filename += "(1)";

            while (checkUnique(filename + extention) == false)
            {
                string suffix = filename.Substring(filename.IndexOf('('));

                filename = filename.Replace(suffix, "(" + index + ")");

                index++;

                if (index == retryLimit)
                    throw new SharepointCommonException(string.Format("FilenameOrganizer.AppendSuffix try {0} retries and canot find unique name.", retryLimit));
            }

            return filename + extention;
        }
    }
}
