using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imprint.Imaging
{
    public partial class EffImage
    {


        /// <summary>
        /// 检测是否支持pixelformat
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static bool CheckSupportPixel(Bitmap src)
        {
            if (src.PixelFormat == PixelFormat.Format24bppRgb)
                return true;
            else if (src.PixelFormat == PixelFormat.Format32bppArgb)
                return true;
            else
                return false;
        }

        /// <summary>
        /// 复制图片并转换像素信息编码成EffImage支持格式
        /// XXX: 低效 . 慎用
        /// </summary>
        /// <param name="Img"></param>
        /// <returns></returns>
        public static Bitmap ToPixelImg(Bitmap Img)
        {
            Bitmap Copy = new Bitmap(Img.Width, Img.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            for (int i = 0; i < Copy.Width; i++)
            {
                for (int j = 0; j < Copy.Height; j++)
                {
                    Copy.SetPixel(i, j, Img.GetPixel(i, j));
                }
            }
            return Copy;
        }

        /// <summary>
        /// 创建全白图片
        /// </summary>
        /// <param name="w">宽</param>
        /// <param name="h">高</param>
        /// <returns></returns>
        public static EffImage White(int w, int h)
        {
            Bitmap rt = new Bitmap(w, h);
            Graphics g = Graphics.FromImage(rt);
            g.Clear(Color.White);
            return new EffImage(rt);
        }

        /// <summary>
        /// EffImage填充边距
        /// </summary>
        /// <param name="src">源图</param>
        /// <param name="w">目标宽</param>
        /// <param name="h">目标高</param>
        /// <returns></returns>
        public static EffImage Padding(EffImage src, int w, int h)
        {
            return new EffImage(Padding(src.Origin, w, h));
        }


        /// <summary>
        /// 位图填充
        /// </summary>
        /// <param name="src">原图</param>
        /// <param name="w">目标宽</param>
        /// <param name="h">目标高</param>
        /// <returns></returns>
        public static Bitmap Padding(Bitmap src, int w, int h)
        {
            var eimg = new EffImage(src);
            Bitmap rt = new Bitmap(w, h);
            Graphics g = Graphics.FromImage(rt);
            g.Clear(Color.White);
            int left = eimg.Left;
            int right = eimg.Right;
            int top = eimg.Top;
            int bot = eimg.Bottom;
            int pwid = right - left;
            int phei = bot - top;
            int padleft = (w - pwid) / 2 - 1;
            int padright = (h - phei) / 2 - 1;
            var sr = CutV(CutH(eimg, left, right), top, bot);
            g.DrawImage(sr.Origin, new Point(padleft, padright));
            g.Save();
            g.Dispose();
            return rt;
        }


        /// <summary>
        /// 缩放
        /// </summary>
        /// <param name="img"></param>
        /// <param name="w">目标宽</param>
        /// <param name="h">目标高</param>
        /// <returns></returns>
        public static EffImage Resize(EffImage img, int w, int h)
        {
            Bitmap bmp = new Bitmap(w, h);
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.White);
            var src = img.Origin;

            g.DrawImage(src,
                new Rectangle(0, 0, w, h),
                new Rectangle(0, 0, img.width, img.height)
                , GraphicsUnit.Pixel);

            var rt = new EffImage(bmp);

            src.Dispose();
            bmp.Dispose();
            return rt;
        }


        /// <summary>
        /// Y方向切割图片
        /// </summary>
        /// <param name="x"></param>
        /// <param name="s"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public static EffImage CutV(EffImage x, int s, int e)
        {
            if (e - s <= 0) return null;
            if (s > 1) s -= 1;
            if (e < x.Height - 1) e += 1;

            EffImage rt = White(x.Width, e - s);
            for (int j = s; j < e; j++)
            {
                for (int i = 0; i < x.Width; i++)
                {
                    rt.Set(i, j - s, x.At(i, j));
                }
            }
            return rt;
        }

        /// <summary>
        /// 切割图片 X坐标
        /// </summary>
        /// <param name="x">图片</param>
        /// <param name="s">X开始坐标</param>
        /// <param name="e">X结束坐标</param>
        /// <returns></returns>
        public static EffImage CutH(EffImage x, int s, int e)
        {
            if (e - s <= 0) return null;
            if (s > 1) s -= 1;
            if (e < x.Width - 1) e += 1;

            EffImage rt = White(e - s, x.Height);

            for (int i = s; i < e; i++)
            {
                for (int j = 0; j < x.Height; j++)
                {
                    rt.Set(i - s, j, x.At(i, j));
                }
            }
            return rt;
        }

    }
}
