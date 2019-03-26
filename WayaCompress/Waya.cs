using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;
using System.IO;

namespace WayaCompress
{
    public class Waya
    {
        private const int WayaSignature = 0x41594157;

        // https://stackoverflow.com/questions/466204/rounding-up-to-next-power-of-2
        private int UpperPowerOfTwo(int v)
        {
            v--;
            v |= v >> 1;
            v |= v >> 2;
            v |= v >> 4;
            v |= v >> 8;
            v |= v >> 16;
            v++;
            return v;
        }

        private void LerpUV (int width, int height, YuvValue[] yuvSurface)
        {
            int lerpHeight = height - 1;
            int lerpWidth = width - 1;

            for (int y = 0; y < lerpHeight; y += 2)
            {
                for (int x = 0; x < lerpWidth; x += 2)
                {
                    YuvValue value00 = yuvSurface[y * lerpWidth + x];
                    YuvValue value01 = yuvSurface[y * lerpWidth + x + 1];
                    YuvValue value10 = yuvSurface[(y + 1) * lerpWidth + x];
                    YuvValue value11 = yuvSurface[(y + 1) * lerpWidth + x + 1];

                    int avgU = (value00.u + value01.u + value10.u + value11.u) / 4;
                    int avgV = (value00.v + value01.v + value10.v + value11.v) / 4;

                    yuvSurface[y * lerpWidth + x].u = avgU;
                    yuvSurface[y * lerpWidth + x].v = avgV;
                    yuvSurface[y * lerpWidth + x + 1].u = avgU;
                    yuvSurface[y * lerpWidth + x + 1].v = avgV;
                    yuvSurface[(y + 1) * lerpWidth + x].u = avgU;
                    yuvSurface[(y + 1) * lerpWidth + x].v = avgV;
                    yuvSurface[(y + 1) * lerpWidth + x + 1].u = avgU;
                    yuvSurface[(y + 1) * lerpWidth + x + 1].v = avgV;
                }
            }
        }

        public byte [] Compress (Image image, int iterations=3)
        {
            int width = image.Width;
            int height = image.Height;

            YuvValue [] yuvSurface = new YuvValue[width * height];

            /// RGB -> Yuv

            Bitmap bitmap = new Bitmap(image);

            for (int y=0; y<height; y++)
            {
                for(int x=0; x<width; x++)
                {
                    Color color = bitmap.GetPixel(x, y);

                    RgbValue rgb = new RgbValue();
                    rgb.r = color.R;
                    rgb.g = color.G;
                    rgb.b = color.B;

                    yuvSurface[y * width +  x] = Yuv.RgbToYuv(rgb);
                }
            }

            /// Lerp U/V
            /// Seems doesnt't affect compression ratio much...

            //LerpUV(width, height, yuvSurface);

            /// Extract components and align on power of 2

            int alignedSize = UpperPowerOfTwo(Math.Max(width, height));

            Int32[] yvalues = new Int32[alignedSize * alignedSize];
            Int32[] uvalues = new Int32[alignedSize * alignedSize];
            Int32[] vvalues = new Int32[alignedSize * alignedSize];

            for (int y=0; y< alignedSize; y++)
            {
                for(int x=0; x< alignedSize; x++)
                {
                    if (y >= height || x >= width)
                    {
                        yvalues[y * alignedSize + x] = -1;
                        uvalues[y * alignedSize + x] = -1;
                        vvalues[y * alignedSize + x] = -1;
                    }
                    else
                    {
                        yvalues[y * alignedSize + x] = yuvSurface[y * width + x].y;
                        uvalues[y * alignedSize + x] = yuvSurface[y * width + x].u;
                        vvalues[y * alignedSize + x] = yuvSurface[y * width + x].v;
                    }
                }
            }

            /// Adjust border pixels by last color, to avoid edge glitching

            if ( (width & 1) != 0)
            {
                for (int y = 0; y < height; y++)
                {
                    yvalues[y * alignedSize + width] = yvalues[y * alignedSize + width - 1];
                    uvalues[y * alignedSize + width] = uvalues[y * alignedSize + width - 1];
                    vvalues[y * alignedSize + width] = vvalues[y * alignedSize + width - 1];
                }
            }

            if ( (height & 1) != 0)
            {
                for (int x = 0; x < width; x++)
                {
                    yvalues[height * alignedSize + x] = yvalues[(height - 1) * alignedSize + x];
                    uvalues[height * alignedSize + x] = uvalues[(height - 1) * alignedSize + x];
                    vvalues[height * alignedSize + x] = vvalues[(height - 1) * alignedSize + x];
                }
            }

            /// Wavelet

            Wavelet.FWT(yvalues, alignedSize, alignedSize, iterations);
            Wavelet.FWT(uvalues, alignedSize, alignedSize, iterations);
            Wavelet.FWT(vvalues, alignedSize, alignedSize, iterations);

            /// Gzip

            byte[] ycomp = Gzip.Compress(ToByteArray(yvalues));
            byte[] ucomp = Gzip.Compress(ToByteArray(uvalues));
            byte[] vcomp = Gzip.Compress(ToByteArray(vvalues));

            /// Header

            Int32[] header = new Int32[8];

            header[0] = alignedSize;
            header[1] = width;
            header[2] = height;
            header[3] = iterations;

            header[4] = ycomp.Length;
            header[5] = ucomp.Length;
            header[6] = vcomp.Length;
            header[7] = WayaSignature;

            /// Waya Image

            byte[] headerByte = ToByteArray(header);
            byte[] waveImage = new byte[headerByte.Length + ycomp.Length + ucomp.Length + vcomp.Length];

            headerByte.CopyTo(waveImage, 0);
            ycomp.CopyTo(waveImage, headerByte.Length);
            ucomp.CopyTo(waveImage, headerByte.Length + ycomp.Length);
            vcomp.CopyTo(waveImage, headerByte.Length + ycomp.Length + ucomp.Length);

            return waveImage;
        }

        public Image Decompress (byte [] data)
        {
            byte[] headerByte = new byte[8*4];

            Array.Copy(data, 0, headerByte, 0, headerByte.Length);
            Int32[] header = ToIntArray(headerByte);

            int alignedSize = header[0];
            int width = header[1];
            int height = header[2];
            int iterations = header[3];

            int ycompSize = header[4];
            int ucompSize = header[5];
            int vcompSize = header[6];
            int signature = header[7];

            if (signature != WayaSignature)
                return null;

            Bitmap bitmap = new Bitmap(width, height);

            YuvValue[] yuvSurface = new YuvValue[width * height];

            byte[] ycomp = new byte[ycompSize];
            byte[] ucomp = new byte[ucompSize];
            byte[] vcomp = new byte[vcompSize];

            Array.Copy(data, headerByte.Length, ycomp, 0, ycompSize);
            Array.Copy(data, headerByte.Length + ycompSize, ucomp, 0, ucompSize);
            Array.Copy(data, headerByte.Length + ycompSize + ucompSize, vcomp, 0, vcompSize);

            /// Gzip

            Int32[] yvalues = ToIntArray(Gzip.Decompress(ycomp));
            Int32[] uvalues = ToIntArray(Gzip.Decompress(ucomp));
            Int32[] vvalues = ToIntArray(Gzip.Decompress(vcomp));

            /// Wavelet

            Wavelet.IWT(yvalues, alignedSize, alignedSize, iterations);
            Wavelet.IWT(uvalues, alignedSize, alignedSize, iterations);
            Wavelet.IWT(vvalues, alignedSize, alignedSize, iterations);

            /// Extract components

            for (int y=0; y<height; y++)
            {
                for(int x=0; x<width; x++)
                {
                    yuvSurface[y * width + x] = new YuvValue();

                    yuvSurface[y * width + x].y = yvalues[y * alignedSize + x];
                    yuvSurface[y * width + x].u = uvalues[y * alignedSize + x];
                    yuvSurface[y * width + x].v = vvalues[y * alignedSize + x];
                }
            }

            /// Yuv -> RGB

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    RgbValue rgb = Yuv.YuvToRgb(yuvSurface[y * width + x]);
                    Color color = Color.FromArgb(rgb.r, rgb.g, rgb.b);
                    bitmap.SetPixel(x, y, color);
                }
            }

            return bitmap;
        }

        private byte[] ToByteArray(Int32[] array)
        {
            byte[] raw = new byte[array.Length * 4];

            for (int i = 0; i < array.Length; i++)
            {
                Int32 value = array[i];

                raw[4 * i + 0] = (byte)(value & 0xff);
                raw[4 * i + 1] = (byte)((value >> 8) & 0xff);
                raw[4 * i + 2] = (byte)((value >> 16) & 0xff);
                raw[4 * i + 3] = (byte)((value >> 24) & 0xff);
            }

            return raw;
        }

        private Int32[] ToIntArray(byte [] array)
        {
            Int32[] raw = new Int32[array.Length / 4];

            for (int i = 0; i < array.Length / 4; i++)
            {
                Int32 value = 0;

                value |= array[4 * i + 0];
                value |= array[4 * i + 1] << 8;
                value |= array[4 * i + 2] << 16;
                value |= array[4 * i + 3] << 24;

                raw[i] = value;
            }

            return raw;
        }

    }

}
