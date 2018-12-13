using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Main.Other
{
    public static class ExtensionPath
    {
        public static string GetFullPathToFile(
            this string fileName
            )
        {
            if (fileName == null)
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            if(Path.IsPathRooted(fileName))
            {
                return
                    fileName;
            }

            var fi = new FileInfo(Assembly.GetExecutingAssembly().Location);
            var di = fi.Directory.FullName;

            var result = Path.Combine(
                di,
                fileName
                );

            return result;
        }
    }
}
