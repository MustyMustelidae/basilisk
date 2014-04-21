﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
