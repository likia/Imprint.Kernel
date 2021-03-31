using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imprint.Imaging
{
    public static class EffImageExtensions
    {
        /// <summary>
        /// 细化
        /// </summary>
        public static void Thining(this EffImage image)
        {
            thiniter(0, image);
            thiniter(1, image);
        }

        /// <summary>
        /// 某论文上的细化算法OpenCV改EffImage版
        /// </summary>
        /// <param name="iter"></param>
        private static void thiniter(byte iter, EffImage img)
        {
            int convertColor(Color c)
            {
                return c.R < 100 ? 1 : 0;
            }


            for (int i = 1; i < img.Width - 1; i++)
                for (int j = 1; j < img.Height - 1; j++)
                {
                    int p2 = convertColor(img.At(i - 1, j));
                    int p3 = convertColor(img.At(i - 1, j + 1));
                    int p4 = convertColor(img.At(i, j + 1));
                    int p5 = convertColor(img.At(i + 1, j + 1));
                    int p6 = convertColor(img.At(i + 1, j));
                    int p7 = convertColor(img.At(i + 1, j - 1));
                    int p8 = convertColor(img.At(i, j - 1));
                    int p9 = convertColor(img.At(i - 1, j - 1));

                    int A = Convert.ToInt32((p2 == 0 && p3 == 1)) + Convert.ToInt32((p3 == 0 && p4 == 1)) +
                              Convert.ToInt32((p4 == 0 && p5 == 1)) + Convert.ToInt32((p5 == 0 && p6 == 1)) +
                              Convert.ToInt32((p6 == 0 && p7 == 1)) + Convert.ToInt32((p7 == 0 && p8 == 1)) +
                              Convert.ToInt32((p8 == 0 && p9 == 1)) + Convert.ToInt32((p9 == 0 && p2 == 1));
                    int B = p2 + p3 + p4 + p5 + p6 + p7 + p8 + p9;
                    int m1 = iter == 0 ?
                        (p2 * p4 * p6) :
                        (p2 * p4 * p8);
                    int m2 = iter == 0 ?
                        (p4 * p6 * p8) :
                        (p2 * p6 * p8);

                    if (A == 1 && (B >= 2 && B <= 6) && m1 == 0 && m2 == 0)
                        img.Set(i, j, Color.White);
                }
        }

        /// <summary>
        /// Laplacian边缘提取
        /// </summary>
        public static double[,] Laplacian3x3 = new double[,]
        {         { -1, -1, -1,  },
                  { -1,  8, -1,  },
                  { -1, -1, -1,  }, };

        /// <summary>
        /// 边缘提取
        /// </summary>
        public static void Contouring(this EffImage image)
        {
            image.ConvolutionFilter(Laplacian3x3, 1, 0);
        }


        /// <summary>
        ///  去掉杂点（适合杂点/杂线粗为1）
        ///  NOTE: 没什么用
        /// </summary>
        public static void ClearNoise(this EffImage img, int MaxNearPoints)
        {
            Color piexl;
            int nearDots = 0;
            int dgGrayValue = 50;
            //     int XSpan, YSpan, tmpX, tmpY;
            //逐点判断
            for (int i = 0; i < img.Width; i++)
                for (int j = 0; j < img.Height; j++)
                {
                    piexl = img.At(i, j);
                    if (piexl.R < dgGrayValue)
                    {
                        nearDots = 0;
                        //判断周围8个点是否全为空
                        if (i == 0 || i == img.Width - 1 || j == 0 || j == img.Height - 1)  //边框全去掉
                        {
                            img.Set(i, j, Color.FromArgb(255, 255, 255));
                        }
                        else
                        {
                            if (img.At(i - 1, j - 1).R < dgGrayValue) nearDots++;
                            if (img.At(i, j - 1).R < dgGrayValue) nearDots += 2;

                            if (img.At(i + 1, j - 1).R < dgGrayValue) nearDots += 2;

                            if (img.At(i - 1, j).R < dgGrayValue) nearDots++;

                            if (img.At(i + 1, j).R < dgGrayValue) nearDots++;

                            if (img.At(i - 1, j + 1).R < dgGrayValue) nearDots += 2;

                            if (img.At(i, j + 1).R < dgGrayValue) nearDots += 2;

                            if (img.At(i + 1, j + 1).R < dgGrayValue) nearDots += 2;
                        }

                        if (nearDots < MaxNearPoints)
                            img.Set(i, j, Color.FromArgb(255, 255, 255));   //去掉单点 && 粗细小3邻边点
                    }
                    else  //背景
                        img.Set(i, j, Color.FromArgb(255, 255, 255));
                }
        }
    }
}