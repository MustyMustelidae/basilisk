using JetBrains.Annotations;

namespace Shared
{
    public static class SFormatExt
    {
         [StringFormatMethod("self")]
        public static string SFormat(this string self,params object[] args)
        {
            return string.Format(self, args);
        }
    }
}
