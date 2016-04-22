using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectServer
{
    public static class Extensions
    {
        public static bool Between(this float v, float v1, float v2)
        {
            return v >= v1 && v <= v2;
        }
    }
}
