using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;

namespace Imprint.Imaging
{
    public delegate bool CustomCaseProc(EffImage sender, int srcx, int srcy);
    public delegate void CustomProc(EffImage sender, int srcx, int srcy);

    public enum ImageColorType
    {
        // 二值图
        Binary,
        // 灰度图
        GrayScale,
        // 彩图 (三通道
        Colored,
    }


    /// <summary>
    /// 高性能验证码图像处理类
    /// </summary>
    public partial class EffImage : IDisposable , ICloneable
    {
        /// <summary>
        /// 宽高
        /// </summary>
        private int width, height;

        /// <summary>
        /// 像素数据
        /// </summary>
        private Color[,] pixels;

        /// <summary>
        /// 颜色类型
        /// </summary>
        private ImageColorType imageColorType;

        /// <summary>
        /// 前景门限颜色值
        /// </summary>
        private Color foregroundThreshold;

        /// <summary>
        /// 是否开启并行计算
        /// </summary>
        private bool useParalle => width * height > 500000;

        /// <summary>
        /// 判断是否是前景值
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        private bool isForeground(Color color)
        {
            switch(imageColorType)
            {
                case ImageColorType.Binary:
                case ImageColorType.GrayScale:
                    return color.R <= foregroundThreshold.R;
                case ImageColorType.Colored:
                    return color.R <= foregroundThreshold.R &&
                           color.G <= foregroundThreshold.G &&
                           color.B <= foregroundThreshold.B;
            }
            return false;
        }

        public Color[,] Pixels
        {
            get
            {
                return pixels;
            }
        }

        /// <summary>
        /// BFS搜索所有连接区域
        /// </summary>
        public List<ConnectedArea> ConnectedAreas
        {
            get
            {
                var areaList = new List<ConnectedArea>();

                using (var img = (EffImage)Clone())
                {

                    while (img.Left != -1)
                    {
                        var pointList = new List<Point>();
                        var queue = new Queue<Point>();
                        var map = new bool[img.Width, img.Height];

                        // 找左边第一个前景点
                        int j;
                        var i = img.Left;
                        for (j = 0; j < img.Height; j++)
                        {
                            if (img.isForeground(img.At(i, j)))
                            {
                                break;
                            }
                        }
                        // 起始点出发
                        var initPoint = new Point(i, j);

                        queue.Enqueue(initPoint);

                        while (queue.Count > 0)
                        {
                            var p = queue.Dequeue();
                            // 去重
                            if (map[p.X, p.Y]) continue;

                            map[initPoint.X, initPoint.Y] = true;

                            img.Set(p.X, p.Y, Color.White);

                            if (!pointList.Contains(p))
                                pointList.Add(p);

                            var offsetX = new int[] { -1, 0, 1 };
                            var offsetY = new int[] { -1, 0, 1 };

                            foreach (var x in offsetX)
                            {
                                foreach (var y in offsetY)
                                {
                                    if (x == 0 && y == 0) continue;

                                    var newPonint = new Point(p.X + x, p.Y + y);

                                    if (img.isForeground(img.At(newPonint.X, newPonint.Y)))
                                    {
                                        if (!map[newPonint.X, newPonint.Y] && !queue.Contains(newPonint))
                                        {
                                            queue.Enqueue(newPonint);
                                        }
                                    }
                                }
                            }
                        }
                        if (pointList.Count > 2)
                        {
                            // 队列空
                            areaList.Add(new ConnectedArea()
                            {
                                Points = pointList
                            });
                        }
                    }
                    // 图片空
                    return areaList;
                }
            }
        }

        /// <summary>
        /// 色域直方图
        /// </summary>
        /// <returns></returns>
        public int[] ColorHistogram
        {
            get
            {
                if (imageColorType != ImageColorType.GrayScale) throw new NotSupportedException("只支持灰度图");

                var hist = new int[256];
                hist.Initialize();

                ProcessEach((img, i, j) =>
                {
                    var color = img.At(i, j);

                    hist[color.R]++;
                });
                return hist;
            }
        }


        /// <summary>
        /// 非背景左X坐标
        /// </summary>
        public int Left
        {
            get
            {
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        if (isForeground(At(i, j)))
                        {
                            return i;
                        }
                    }
                }
                return -1;
            }
        }


        /// <summary>
        /// 前景区域
        /// </summary>
        public EffImage ValidArea
        {
            get
            {
                bool checkBoundary(int x, int y)
                {
                    if (x < 0 || x >= width || y < 0 || y >= height) return false;
                    return true;
                }


                var re = checkBoundary(Left - 1, Top - 1);
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
                        if (isForeground(At(i, j)))
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
                        if (isForeground(At(i, j)))
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
        /// 
        /// </summary>
        /// <param name="proc">回调</param>
        /// <param name="linefirst">是否一行一行遍历</param>
        /// <param name="parallel">是否并行， 发挥多核cpu性能， 适合大图片， 小图片反而增加开销</param>
        public void ProcessEach(CustomProc proc, bool linefirst = true, bool? parallel = null)
        {
            int fstLim = 0, lstLim = 0;

            if (parallel == null)
            {
                parallel = useParalle;
            }

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
            if (parallel == true)
            {
                Parallel.For(0, fstLim,(i) =>
                {
                    Parallel.For(0, lstLim, (j) =>
                    {
                        proc(this, i, j);
                    });
                });
            }
            else
            {
                for (int i = 0; i < fstLim; i++)
                    for (int j = 0; j < lstLim; j++)
                    {
                        proc(this, i, j);
                    }
            }
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
                        if (isForeground(At(i, j)))
                        {
                            return j;
                        }
                    }
                }
                return -1;
            }
        }

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
        /// 旋转图片
        /// </summary>
        /// <param name="degree">度数</param>
        /// <returns></returns>
        public EffImage Rotate(int degree)
        {
            EffImage rtImg = White(Width, Height);
            var org = CutV(CutH(this, Left, Right), Top, Bottom);
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
        /// 相当于GetPixel
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public Color At(int x, int y)
        {
            if (x >= width || x < 0 || y >= height || y < 0) 
                return Color.White;

            return pixels[x, y];
        }

        /// <summary>
        /// 相当于SetPixel
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="c"></param>
        public void Set(int x, int y, Color c)
        {
            pixels[x, y] = c;
        }

        /// <summary>
        /// 转换成灰度图片
        /// 灰度策略并不是平均
        /// </summary>
        public void GrayScale()
        {
            ProcessEach((CustomProc)delegate (EffImage x, int i, int j)
            {
                var color = x.At(i, j);
                var grayscale = (color.R * 30 + color.G * 59 + color.B * 11) / 100;
                x.Set(i, j, Color.FromArgb(grayscale, grayscale, grayscale));
            });
            imageColorType = ImageColorType.GrayScale;
        }

        /// <summary>
        /// 二值化
        /// </summary>
        /// <param name="thresold">前景门限</param>
        public void Binarization(int thresold)
        {
            ProcessEach((CustomProc)delegate (EffImage x, int i, int j)
            {
                var c = x.At(i, j);
                if (c.R < thresold)
                    x.Set(i, j, Color.Black);
                else
                    x.Set(i, j, Color.White);
            });
            imageColorType = ImageColorType.Binary;
        }

        /// <summary>
        /// 反色
        /// </summary>
        public void Reverse()
        {
            ProcessEach((img, x, y) =>
            {
                var color = img.At(x, y);
                var revColor = Color.FromArgb(255 - color.R, 255 - color.G, 255 - color.B);

                img.Set(x, y, revColor);
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
                BitmapData srcBmData = bm.LockBits(new Rectangle(0, 0, width, height),
                    ImageLockMode.ReadWrite, bm.PixelFormat);

                IntPtr srcScan = srcBmData.Scan0;
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
        /// <returns></returns>
        public int[] ProjectionHistH()
        {
            int[] hist = new int[width];
            ProcessEach((im, i, j) =>
            {
                var colr = im.At(i, j);
                if (isForeground(colr)) //foreground
                    hist[i]++;
            });
            return hist;
        }

        /// <summary>
        /// Y轴投影直方图
        /// </summary>
        /// <returns></returns>
        public int[] ProjectionHistV()
        {
            int[] hist = new int[height];
            ProcessEach((im, i, j) =>
            {
                var colr = im.At(i, j);
                if (isForeground(colr)) //foreground
                    hist[j]++;
            }, false);
            return hist;
        }


        /// <summary>
        /// NOTE: 只支持24bit RGB/32bit ARGB, 其他类型需要先使用ToPixelImg进行转换
        /// </summary>
        /// <param name="src"></param>
        public EffImage(Bitmap src, ImageColorType colorType = ImageColorType.Binary, byte rThreshold = 50, byte gThreshold = 50, byte bThreshold = 50)
        {
            width = src.Width;
            height = src.Height;
            pixels = new Color[width, height];

            // 前景门限
            imageColorType = colorType;
            foregroundThreshold = Color.FromArgb(rThreshold, gThreshold, bThreshold);

            Rectangle rect = new Rectangle(0, 0, width, height);
            int offset = 3;
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
            IntPtr srcScan = srcBmData.Scan0;
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
                    if (isForeground(c))
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
        /// 自适应二值化
        /// 
        /// OTSU
        /// </summary>
        public void AdativeBinarization()
        {
            var hist = ColorHistogram;

            var vet = new float[256];
            vet.Initialize();


            float p1, p2, p12;

            // function is used to compute the q values in the equation
            float Px(int init, int end)
            {
                int sum = 0;
                int i;
                for (i = init; i <= end; i++)
                    sum += hist[i];

                return (float)sum;
            }

            // function is used to compute the mean values in the equation (mu)
            float Mx(int init, int end)
            {
                int sum = 0;
                int i;
                for (i = init; i <= end; i++)
                    sum += i * hist[i];

                return (float)sum;
            }

            // loop through all possible t values and maximize between class variance
            for (int k = 1; k < 255; k++)
            {
                p1 = Px(0, k);
                p2 = Px(k + 1, 255);
                p12 = p1 * p2;
                if (p12 == 0)
                    p12 = 1;
                float diff = (Mx(0, k) * p2) - (Mx(k + 1, 255) * p1);
                vet[k] = (float)diff * diff / p12;
                //vet[k] = (float)Math.Pow((Mx(0, k, hist) * p2) - (Mx(k + 1, 255, hist) * p1), 2) / p12;
            }

            var max = 0f;
            var maxIndex = -1;
            for (int i = 0; i < vet.Length; i++)
            {
                if (vet[i] > max)
                {
                    max = vet[i];
                    maxIndex = i;
                }
            }

            Binarization(maxIndex);
        }

   
        //public void ElasticDistortion (bool bNorm = false,
        //                 double sigma = 4,
        //                 double alpha = 34)
        //{
        //    double low = -1.0;
        //    double high = 1.0;

        //    //The image deformations were created by first generating
        //    //random displacement fields, that's dx(x,y) = rand(-1, +1) and dy(x,y) = rand(-1, +1)
        //    cv::randu(dx, cv::Scalar(low), cv::Scalar(high));
        //    cv::randu(dy, cv::Scalar(low), cv::Scalar(high));

        //    //The fields dx and dy are then convolved with a Gaussian of standard deviation sigma(in pixels)
        //    cv::Size kernel_size(sigma*6 + 1, sigma * 6 + 1);
        //    cv::GaussianBlur(dx, dx, kernel_size, sigma);
        //    cv::GaussianBlur(dy, dy, kernel_size, sigma);

        //    //If we normalize the displacement field (to a norm of 1,
        //    //the field is then close to constant, with a random direction
        //    if (bNorm)
        //    {
        //        dx /= cv::norm(dx, cv::NORM_L1);
        //        dy /= cv::norm(dy, cv::NORM_L1);
        //    }

        //    //The displacement fields are then multiplied by a scaling factor alpha
        //    //that controls the intensity of the deformation.
        //    dx *= alpha;
        //    dy *= alpha;

        //    //Inverse(or Backward) Mapping to avoid gaps and overlaps.
        //    cv::Rect checkError(0, 0, src.cols, src.rows);
        //    int nCh = src.channels();

        //    for (int displaced_y = 0; displaced_y < src.rows; displaced_y++)
        //        for (int displaced_x = 0; displaced_x < src.cols; displaced_x++)
        //        {
        //            int org_x = displaced_x - dx.at<double>(displaced_y, displaced_x);
        //            int org_y = displaced_y - dy.at<double>(displaced_y, displaced_x);

        //            if (checkError.contains(cv::Point(org_x, org_y)))
        //            {
        //                for (int ch = 0; ch < nCh; ch++)
        //                {
        //                    dst.data[(displaced_y * src.cols + displaced_x) * nCh + ch] = src.data[(org_y * src.cols + org_x) * nCh + ch];
        //                }
        //            }
        //        }
        //}        



        public void Dispose()
        {
            pixels = null;
        }

        public object Clone()
        {
            var img = EffImage.White(Width, Height);
            img.pixels = this.pixels.Clone() as Color[,];
            return img;
        }
    }
}