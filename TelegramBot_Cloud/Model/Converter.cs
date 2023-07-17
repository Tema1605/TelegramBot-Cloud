using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot_Cloud.Model
{
    internal class Converter
    {
        public static double BytesToMegabytes(long bytes)
        {
            double mb = Math.Round(bytes / Math.Pow(1024, 2), 2);
            return mb;
        }        
    }
}
