// https://www.codeproject.com/Articles/683663/Discrete-Haar-Wavelet-Transformation

// With Int32 and flatten arrays modifications

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WayaCompress
{
    class Wavelet
    {
        /// <summary>
        ///   Discrete Haar Wavelet Transform
        /// </summary>
        /// 
        public static void FWT(Int32[] data)
        {
            Int32[] temp = new Int32[data.Length];

            int h = data.Length >> 1;
            for (int i = 0; i < h; i++)
            {
                int k = (i << 1);
                temp[i] = data[k] / 2 + data[k + 1] / 2;
                temp[i + h] = data[k] / 2 - data[k + 1] / 2;
            }

            for (int i = 0; i < data.Length; i++)
                data[i] = temp[i];
        }

        /// <summary>
        ///   Discrete Haar Wavelet 2D Transform
        /// </summary>
        /// 
        public static void FWT(Int32[] data, int rows, int cols, int iterations)
        {
            Int32[] row = new Int32[cols];
            Int32[] col = new Int32[rows];

            for (int k = 0; k < iterations; k++)
            {
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < row.Length; j++)
                        row[j] = data[i * rows + j];

                    FWT(row);

                    for (int j = 0; j < row.Length; j++)
                        data[i * rows + j] = row[j];
                }

                for (int j = 0; j < cols; j++)
                {
                    for (int i = 0; i < col.Length; i++)
                        col[i] = data[i * rows + j];

                    FWT(col);

                    for (int i = 0; i < col.Length; i++)
                        data[i * rows + j] = col[i];
                }
            }
        }

        /// <summary>
        ///   Inverse Haar Wavelet Transform
        /// </summary>
        /// 
        public static void IWT(Int32[] data)
        {
            Int32[] temp = new Int32[data.Length];

            int h = data.Length >> 1;
            for (int i = 0; i < h; i++)
            {
                int k = (i << 1);
                temp[k] = (data[i] / 2 + data[i + h] / 2) * 2;
                temp[k + 1] = (data[i] / 2 - data[i + h] / 2) * 2;
            }

            for (int i = 0; i < data.Length; i++)
                data[i] = temp[i];
        }

        /// <summary>
        ///   Inverse Haar Wavelet 2D Transform
        /// </summary>
        /// 
        public static void IWT(Int32[] data, int rows, int cols, int iterations)
        {
            Int32[] col = new Int32[rows];
            Int32[] row = new Int32[cols];

            for (int l = 0; l < iterations; l++)
            {
                for (int j = 0; j < cols; j++)
                {
                    for (int i = 0; i < row.Length; i++)
                        col[i] = data[i * rows + j];

                    IWT(col);

                    for (int i = 0; i < col.Length; i++)
                        data[i * rows + j] = col[i];
                }

                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < row.Length; j++)
                        row[j] = data[i * rows + j];

                    IWT(row);

                    for (int j = 0; j < row.Length; j++)
                        data[i * rows + j] = row[j];
                }
            }
        }

    }
}
