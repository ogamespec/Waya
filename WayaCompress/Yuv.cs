using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace WayaCompress
{
    public class YuvValue
    {
        public Int32 y = 0;
        public Int32 u = 0;
        public Int32 v = 0;
    }

    public class RgbValue
    {
        public byte r = 0;
        public byte g = 0;
        public byte b = 0;
    }

    class Yuv
    {
        private static int Clip(int x)
        {
            return Math.Max(0, Math.Min(255, x));
        }

        /// Borrowed from Microprose: Magic The Gathering (1997)
        /// (can't identify if its borrowed from somewhere else :-))

        /// Buggy.

        public static YuvValue RgbToYuv2(RgbValue rgb)
        {
            YuvValue yuv = new YuvValue();

            yuv.y = (64 * rgb.g + 8 * rgb.r + 5 * rgb.b) / 28;
            yuv.u = (4 * rgb.b - yuv.y + 0x400) / 2;
            yuv.v = (32 * rgb.r - 8 * yuv.y + 0x1998) / 13;

            return yuv;
        }

        public static RgbValue YuvToRgb2(YuvValue yuv)
        {
            RgbValue rgb = new RgbValue();

            int r = yuv.y + yuv.v + yuv.v / 2 + yuv.v / 8 - 0x333;
            int b = yuv.y + yuv.u * 2 - 0x400;
            int g = yuv.y * 2 - yuv.y / 4 - r / 2 - b / 4 - b / 16;
            rgb.r = (byte)Clip(r / 4);
            rgb.g = (byte)Clip(g / 4);
            rgb.b = (byte)Clip(b / 4);

            return rgb;
        }

        // https://stackoverflow.com/questions/1737726/how-to-perform-rgb-yuv-conversion-in-c-c

        public static YuvValue RgbToYuv (RgbValue rgb)
        {
            YuvValue yuv = new YuvValue();

            yuv.y = Clip((19595 * rgb.r + 38470 * rgb.g + 7471 * rgb.b) >> 16);
            yuv.u = Clip((36962 * (rgb.b - Clip((19595 * rgb.r + 38470 * rgb.g + 7471 * rgb.b) >> 16)) >> 16) + 128);
            yuv.v = Clip((46727 * (rgb.r - Clip((19595 * rgb.r + 38470 * rgb.g + 7471 * rgb.b) >> 16)) >> 16) + 128);

            return yuv;
        }

        public static RgbValue YuvToRgb (YuvValue yuv)
        {
            RgbValue rgb = new RgbValue();

            rgb.r = (byte)Clip(yuv.y + (91881 * yuv.v >> 16) - 179);
            rgb.g = (byte)Clip(yuv.y - ((22544 * yuv.u + 46793 * yuv.v) >> 16) + 135);
            rgb.b = (byte)Clip(yuv.y + (116129 * yuv.u >> 16) - 226);

            return rgb;
        }

    }
}
