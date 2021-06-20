using System.IO;
using System.Reflection;

namespace Extension.Helper
{
    public static class ReflectionHelper
    {
        public static void ExtractEmbeddedResource(
            string fullPath,
            string resourceName
        )
        {
            if (!File.Exists(fullPath))
            {
                using (var target = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None))
                using (var source = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                {
                    source.CopyTo(target);

                    target.Flush();
                }
            }
        }

    }
}
