using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace Imprint.Imaging
{
    public delegate bool CustomCaseProc(EffImage sender,int srcx,int srcy);
    public delegate void CustomProc(EffImage sender,int srcx,int srcy);


    /// <summary>
    /// 高性能验证码图像处理类
    /// </summary>
    public class EffImage
    {
        /// <summary>
        /// Laplacian边缘提取
        /// </summary>
        public static double[,] Laplacian3x3 = new double[,]
        {         { -1, -1, -1,  },
                  { -1,  8, -1,  },
                  { -1, -1, -1,  }, };
        /// <summary>
        /// 创建全白图片
        /// </summary>
        /// <param name="w">宽</param>
        /// <param name="h">高</param>
        /// <returns></returns>
        public static EffImage White(int w,int h)
        {
            Bitmap rt = new Bitmap(w,h);
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
        public static Bitmap Padding(Bitmap src,int w,int h)
        {
            var eimg = new EffImage(src);
            Bitmap rt = new Bitmap(w, h);
            Graphics g = Graphics.FromImage(rt);
            g.Clear(Color.White);
            int left = eimg.Left;
            int right = eimg.Right;
            int top = eimg.Top;
            int bot = eimg.Bottom;
            int pwid =right - left ;
            int phei = bot - top ;
            int padleft = (w - pwid) / 2 -1;
            int padright = (h - phei) / 2 -1;
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
        /// 非背景左X坐标
        /// TODO: 前景阀值
        /// </summary>
        public int Left
        {
            get
            {
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        if (At(i, j).R < 50)
                        {
                            return i;
                        }
                    }
                }
                return -1;
            }
        }
        private bool checkBoundary(int x,int y)
        {
            if (x < 0 || x >= width || y < 0 || y >= height) return false;
            return true;
        }
        
        /// <summary>
        /// 前景区域
        /// TODO: 前景阀值
        /// </summary>
        public EffImage ValidArea
        {
            get
            {
                var re = checkBoundary(Left -1, Top - 1);
                var re2 = checkBoundary(Right + 1, Bottom + 1);
               
                if (re && re2)
                {
                    // not out of range
                    return CutH(CutV(this, Top - 1, Bottom + 1), Left - 1, Right + 1);
                }
                else
                {
                    return CutH(CutV(this, Top, Bottom), Left, Right);    
                }
            }
        }
        /// <summary>
        /// 前景右X坐标
        /// </summary>
        public int Right
        {
            get
            {
                for (int i = width - 1; i >= 0; i--)
                {
                    for (int j = height - 1; j >= 0; j--)
                    {
                        if (At(i, j).R < 50)
                        {
                            return i;
                        }
                    }
                }
                return -1;
                
            }
        }
        /// <summary>
        /// 前景上Y坐标
        /// </summary>
        public int Top
        {
            get
            {
                for (int j = 0; j < height; j++)
                {
                    for (int i = 0; i < width; i++)
                    {
                        if (At(i, j).R < 50)
                        {
                            return j;
                        }
                    }
                }
                return -1;
            }
        }

        /// <summary>
        /// 每个像素进行处理
        /// </summary>
        /// <param name="proc">回调</param>
        /// <param name="linefirst">是否一行一行遍历</param>
        public void ProcessEach(CustomProc proc, bool linefirst = true)
        {
            int fstLim = 0, lstLim = 0;

            if (linefirst)
            {
                fstLim = width;
                lstLim = height;
            }
            else
            {
                fstLim = height;
                lstLim = width;
            }

            for (int i = 0; i < fstLim; i++)
                for (int j = 0; j < lstLim; j++)
                {
                    proc(this, i, j);
                }
        }

        byte[] getBuf(string str)
        {
            return Encoding.Default.GetBytes(str);
        }
        /// <summary>
        /// 前景下Y坐标
        /// </summary>
        public int Bottom
        {
            get
            {
                for (int j = height - 1; j >= 0; j--)
                {
                    for (int i = width - 1; i >= 0; i--)
                    {
                        if (At(i, j).R < 50)
                        {
                            return j;
                        }
                    }
                }
                return -1;
            }
        }

        /// <summary>
        /// 宽高
        /// </summary>
        private int width, height;

        /// <summary>
        /// 像素数据
        /// </summary>
        private Color[,] pixels;


        /// <summary>
        /// 高
        /// </summary>
        public int Height
        {
            get { return height; }
            
        }
        /// <summary>
        /// 宽
        /// </summary>
        public int Width
        {
            get { return width; }
        }
        /// <summary>
        /// 前景大小
        /// </summary>
        public Size ActualSize
        {
            get
            {
                return new Size(Right - Left, Bottom - Top);
            }
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
            if (e < x.Width-1) e += 1;

            EffImage rt = White(e - s, x.Height);

            for (int i = s; i < e; i++)
            {
                for (int j = 0; j < x.Height; j++)
                {
                    rt.Set
                     (i - s, j, x.At(i, j));
                }
            }
            return rt;
        }
        /// <summary>
        /// 旋转图片
        /// </summary>
        /// <param name="degree">度数</param>
        /// <returns></returns>
        public EffImage Rotate(int degree)
        {
            EffImage rtImg = White(Width, Height);
            var org = EffImage.CutV(EffImage.CutH(this, Left, Right), Top, Bottom);
            var bmp = org.Origin;
            var rwid = (org.width + org.height) * 2;
            Bitmap r = new Bitmap(rwid, rwid);
            Graphics g = Graphics.FromImage(r);
            g.Clear(Color.White);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            g.RotateTransform(degree);
            g.DrawImage(bmp, new Point((rwid / 2) - (org.width / 2), (rwid / 2) - (org.height / 2)));
            return new EffImage(r).ValidArea;
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
            return rt ;
        }
        /// <summary>
        /// 相当于GetPixel
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public Color At(int x, int y)
        {
            if (x >= width || x < 0 || y >= height || y < 0) return Color.White;

            return pixels[x, y];
        }

        /// <summary>
        /// 相当于SetPixel
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="c"></param>
        public void Set(int x, int y,Color c)
        {
            pixels[x, y] = c;
        }

        /// <summary>
        /// 转换成灰度图片
        /// 灰度策略并不是平均
        /// </summary>
        public void GrayScale()
        {
            ProcessEach((CustomProc)delegate(EffImage x, int i, int j){
                var color = x.At(i, j);
                var grayscale = (color.R * 30 + color.G * 59 + color.B * 11) / 100;
                x.Set(i, j, Color.FromArgb(grayscale, grayscale, grayscale));
            });
        }

        /// <summary>
        /// 二值化
        /// </summary>
        /// <param name="thresold">前景门限</param>
        public void Binarization(int thresold)
        {
            ProcessEach((CustomProc)delegate(EffImage x, int i, int j)
                {
                    var c = x.At(i, j);
                    if (c.R < thresold)
                        x.Set(i, j, Color.Black);
                    else
                        x.Set(i, j, Color.White);
                });
        }
        /// <summary>
        /// 像素数据转位图对象
        /// </summary>
        public Bitmap Origin
        {
            get
            {
                Bitmap bm = new Bitmap(width, height, PixelFormat.Format24bppRgb);
                BitmapData srcBmData = bm.LockBits(new Rectangle(0,0,width,height),
                    ImageLockMode.ReadWrite, bm.PixelFormat);

                System.IntPtr srcScan = srcBmData.Scan0;
                unsafe
                {
                    byte* srcP = (byte*)(void*)srcScan;
                    int srcOffset = srcBmData.Stride - width * 3; //该行填充字节大小 
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++, srcP += 3)
                        {
                            Color c = At(x, y);
                            srcP[0] = c.B;
                            srcP[1] = c.G;
                            srcP[2] = c.R;             
                        }
                        srcP += srcOffset;//跳过填充字节 
                    }
                }
                bm.UnlockBits(srcBmData);
                return bm;
            }
        }

        /// <summary>
        /// X轴投影直方图
        /// TODO: 前景门限
        /// <returns></returns>
        public int[] ProjectionHistH()
        {
            int[] hist = new int[width];
            ProcessEach((im, i, j) =>
            {
                var colr = im.At(i, j);
                if (colr.R < 50) //foreground
                    hist[i]++;
            });
            return hist;
        }

        /// <summary>
        /// Y轴投影直方图
        /// TODO: 前景门限
        /// </summary>
        /// <returns></returns>
        public int[] ProjectionHistV()
        {
            int[] hist = new int[height];
            ProcessEach((im, i, j) =>
            {
                var colr = im.At(i, j);
                if (colr.R < 50) //foreground
                    hist[j]++;
            },false);
            return hist;
        }
        /// <summary>
        ///  去掉杂点（适合杂点/杂线粗为1）
        ///  NOTE: 没什么用
        /// </summary>
        public void ClearNoise(int MaxNearPoints)
        {
            Color piexl;
            int nearDots = 0;
            int dgGrayValue = 50;
            //     int XSpan, YSpan, tmpX, tmpY;
            //逐点判断
            for (int i = 0; i < Width; i++)
                for (int j = 0; j < Height; j++)
                {
                    piexl = At(i, j);
                    if (piexl.R < dgGrayValue)
                    {
                        nearDots = 0;
                        //判断周围8个点是否全为空
                        if (i == 0 || i == Width - 1 || j == 0 || j == Height - 1)  //边框全去掉
                        {
                            Set(i, j, Color.FromArgb(255, 255, 255));
                        }
                        else
                        {
                            if (At(i - 1, j - 1).R < dgGrayValue) nearDots++;
                            if (At(i, j - 1).R < dgGrayValue) nearDots += 2;

                            if (At(i + 1, j - 1).R < dgGrayValue) nearDots += 2;

                            if (At(i - 1, j).R < dgGrayValue) nearDots++;

                            if (At(i + 1, j).R < dgGrayValue) nearDots++;

                            if (At(i - 1, j + 1).R < dgGrayValue) nearDots += 2;

                            if (At(i, j + 1).R < dgGrayValue) nearDots += 2;

                            if (At(i + 1, j + 1).R < dgGrayValue) nearDots += 2;
                        }

                        if (nearDots < MaxNearPoints)
                            Set(i, j, Color.FromArgb(255, 255, 255));   //去掉单点 && 粗细小3邻边点
                    }
                    else  //背景
                        Set(i, j, Color.FromArgb(255, 255, 255));
                }
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
        public EffImage(Bitmap src)
        {
            width = src.Width;
            height = src.Height;
            pixels = new Color[width, height];
            Rectangle rect = new Rectangle(0, 0, width, height);
            int offset=3;
            if (src.PixelFormat == PixelFormat.Format24bppRgb)
                offset = 3;
            else if (src.PixelFormat == PixelFormat.Format32bppArgb)
                offset = 4;
            else throw new Exception("pixelformat not supported");

            BitmapData srcBmData = src.LockBits(rect,
            ImageLockMode.ReadWrite, src.PixelFormat);
            //BITMAPDATA 结构 
            /* 
            *
            * Scan0 //指向颜色存储矩阵的首地址</pre> 
            * Stride //行实际字节大小 
            * Width // 宽度 
            * BGR BGR BGR BGR XXX(xxx为填充字节) 
            * BGR BGR BGR BGR XXX 
            * (XXX大小为 Stride-(Width*3) ) 
            * 
            */
            System.IntPtr srcScan = srcBmData.Scan0;
            unsafe
            {
                byte* srcP = (byte*)(void*)srcScan;
                int srcOffset = srcBmData.Stride - width * offset; //该行填充字节大小 
                byte red, green, blue;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++, srcP += offset)
                    {
                        blue = srcP[0];
                        green = srcP[1];
                        red = srcP[2];
                        //a = [3]
                        Color c = Color.FromArgb(red, green, blue);
                        pixels[x, y] = c;
                    }
                    srcP += srcOffset;//跳过填充字节 
                }
            }
            src.UnlockBits(srcBmData);
        }

        /// <summary>
        /// 卷积运算
        /// </summary>
        /// <param name="filterMatrix">矩阵</param>
        /// <param name="factor">因子</param>
        /// <param name="bias">偏移</param>
        public void ConvolutionFilter(double[,] filterMatrix,
                                              double factor = 1,
                                                   int bias = 0)
        {
            EffImage rt = EffImage.White(width, height);
            double blue = 0.0;
            double green = 0.0;
            double red = 0.0;

            int filterWidth = filterMatrix.GetLength(1);
            int filterHeight = filterMatrix.GetLength(0);

            int filterOffset = (filterWidth - 1) / 2;
            int calcOffset = 0;

            int byteOffset = 0;



            for (int x = filterOffset; x <
                Width - filterOffset; x++)
            {
                for (int y = filterOffset; y <
           Height - filterOffset; y++)
                {

                    blue = green = red = 0d;

                    for (int filterX = -filterOffset;
                        filterX <= filterOffset; filterX++)
                    {
                        for (int filterY = -filterOffset;
                            filterY <= filterOffset; filterY++)
                        {
                            var color = At(x + filterX, y + filterY);
                            red += color.R * filterMatrix[filterY + filterOffset, filterX + filterOffset];
                            green += color.G * filterMatrix[filterY + filterOffset, filterX + filterOffset];
                            blue += color.B * filterMatrix[filterY + filterOffset, filterX + filterOffset];
                        }
                    }

                    red = red * factor + bias;
                    green = green * factor + bias;
                    blue = blue * factor + bias;

                    if (red > 255)
                    { red = 255; }
                    else if (red < 0)
                    { red = 0; }

                    if (green > 255)
                    { green = 255; }
                    else if (green < 0)
                    { green = 0; }

                    if (blue > 255)
                    { blue = 255; }
                    else if (blue < 0)
                    { blue = 0; }

                    rt.Set(x, y, Color.FromArgb((int)red, (int)green, (int)blue));
                }
            }
            this.pixels = rt.pixels;
        }
        /// <summary>
        /// 质心
        /// </summary>
        public Point CenterofMass
        {
            get
            {
                int meanX = 0, meanY = 0, count = 0;
                ProcessEach((m, i, j) =>
                {
                    var c = m.At(i, j);
                    if (c.R < 50)
                    {
                        meanX += i;
                        meanY += j;
                        ++count;
                    }

                });
                meanX /= count;
                meanY /= count;
                return new Point(meanX, meanY);
            }
        }
        /// <summary>
        /// 边缘提取
        /// </summary>
        public void Contouring()
        {
            ConvolutionFilter(Laplacian3x3, 1, 0);
        }
        /// <summary>
        /// 细化
        /// </summary>
        public void Thining()
        {
            thiniter(0);
            thiniter(1);
        }

        int Convert(Color c)
        {
            return c.R < 100 ? 1 : 0;
        }
        /// <summary>
        /// 某论文上的细化算法OpenCV改EffImage版
        /// </summary>
        /// <param name="iter"></param>
        private void thiniter(byte iter)
        {
            for (int i = 1; i < Width - 1; i++)
                for (int j = 1; j < Height - 1; j++)
                {
                    int p2 = Convert(At(i - 1, j)); //im.at<uchar>(i - 1, j);
                    int p3 = Convert(At(i - 1, j + 1)); //im.at<uchar>(i - 1, j + 1);
                    int p4 = Convert(At(i, j + 1)); //im.at<uchar>(i, j + 1);
                    int p5 = Convert(At(i + 1, j + 1)); //im.at<uchar>(i + 1, j + 1);
                    int p6 = Convert(At(i + 1, j)); //im.at<uchar>(i + 1, j);
                    int p7 = Convert(At(i + 1, j - 1)); //im.at<uchar>(i + 1, j - 1);
                    int p8 = Convert(At(i, j - 1)); //im.at<uchar>(i, j - 1);
                    int p9 = Convert(At(i - 1, j - 1));//im.at<uchar>(i - 1, j - 1);

                    int A = System.Convert.ToInt32((p2 == 0 && p3 == 1)) + System.Convert.ToInt32((p3 == 0 && p4 == 1)) +
                              System.Convert.ToInt32((p4 == 0 && p5 == 1)) + System.Convert.ToInt32((p5 == 0 && p6 == 1)) +
                              System.Convert.ToInt32((p6 == 0 && p7 == 1)) + System.Convert.ToInt32((p7 == 0 && p8 == 1)) +
                              System.Convert.ToInt32((p8 == 0 && p9 == 1)) + System.Convert.ToInt32((p9 == 0 && p2 == 1));
                    int B = p2 + p3 + p4 + p5 + p6 + p7 + p8 + p9;
                    int m1 = iter == 0 ?
                        (p2 * p4 * p6) :
                        (p2 * p4 * p8);
                    int m2 = iter == 0 ?
                        (p4 * p6 * p8) :
                        (p2 * p6 * p8);

                    if (A == 1 && (B >= 2 && B <= 6) && m1 == 0 && m2 == 0)
                        Set(i, j, Color.White);
                }
        }
    }
}
