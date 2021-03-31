using Imprint.Imaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;

namespace UnitTest.Imaging
{
    [TestClass]
    public class EffImageTest
    {
        Graphics g;
        Bitmap bmp;
        EffImage image;

        [TestInitialize]
        public void Setup()
        {
            bmp = new Bitmap(32, 32);
            g = Graphics.FromImage(bmp);
            g.Clear(Color.Black);

            image = new EffImage(bmp);
        }

        [TestMethod]
        public void TestConnectedArea()
        {
            bmp = new Bitmap(32, 32);
            g = Graphics.FromImage(bmp);
            g.Clear(Color.White);

            // 三条线
            g.DrawLine(Pens.Black, new Point(0, 4), new Point(32, 4));
            g.DrawLine(Pens.Black, new Point(1, 8), new Point(20, 8));
            g.DrawLine(Pens.Black, new Point(1, 12), new Point(20, 12));

            // 框框 1-20, 22-27
            g.FillRectangle(Brushes.Black, new Rectangle(1, 22, 20, 5));
            g.FillRectangle(Brushes.Black, new Rectangle(1, 30, 20,32));

            g.Save();
            image = new EffImage(bmp);
            var area = image.ConnectedAreas.ToArray();
            Assert.AreEqual(5, area.Length);
            var max = area.Max(i => i.Size.Height * i.Size.Width);
            var maxCount = area.Max(i => i.Points.Count);
            // 最大面积
            Assert.AreEqual(100, max);
            Assert.AreEqual(100, maxCount);
        }

        // 1000w像素图片 i3测试 1s， 并行850ms
        [TestMethod]
        public void TestBinaryWithNoParallel()
        {
            var img = new EffImage((Bitmap)Bitmap.FromFile("z:/test.jpg"));
            var th = 50;
            
            for (int i = 0; i < img.Width; i++)
            {
                for (int j = 0; j < img.Height; j++)
                {
                    // 直接操作pixel数据， 快一丢丢
                    var c = img.Pixels[i, j];
                    if (c.R < th)
                    {
                        img.Pixels[i, j] = Color.Black;
                    }
                    else
                    {
                        img.Pixels[i, j] = Color.White;
                    }
                }
            }
        }

        [TestMethod]
        public void TestBinaryWithParallel()
        {
            var img = new EffImage((Bitmap)Bitmap.FromFile("z:/test.jpg"));
            // 多核并行
            img.ProcessEach((EffImage x, int i, int j) =>
            {
                var c = x.At(i, j);
                if (c.R < 50)
                    x.Set(i, j, Color.Black);
                else
                    x.Set(i, j, Color.White);

            }, parallel: true);
        }

        [TestMethod]
        public void TestOCR()
        {
            var img = new EffImage((Bitmap)Bitmap.FromFile("z:/test.jpg"));
            img.GrayScale();
            img.Binarization(100);
            img.Origin.Save("z:/test_bin.bmp");

            img = EffImage.Resize(img, 1024 , 768);


            
            var vhist = img.ProjectionHistV();

            // 用垂直投影找底部
            var bottom = img.Bottom;
            int upperBound = 0;
            for (upperBound = bottom; upperBound > 0; upperBound--)
            {
                if (vhist[upperBound] <= 5) break;
            }
            
            var idImg = EffImage.CutV(img, upperBound, bottom);
            idImg.ClearNoise(3);

            idImg.Binarization(50);
            var area = idImg.ConnectedAreas;
            int i = 0;
            
            foreach (var item in area)
            {
                var rect = item.ValidArea;
                var seg = EffImage.CutH(EffImage.CutV(idImg, rect.Top, rect.Bottom), rect.Left, rect.Right);
                var resz = EffImage.Resize(seg, 28, 28);
                resz.Origin.Save($"z:/seg/{i++}.bmp");
            }
        }

        [TestMethod]
        public void TestReverse()
        {
            var eff = new EffImage(bmp);
            eff.Reverse();

            var color = eff.At(11, 12);
            Assert.AreEqual(color.ToArgb(), Color.White.ToArgb());
        }

        [TestMethod]
        public void TestForeground()
        {
            var fore = Color.FromArgb(11, 22, 33);
            g.Clear(Color.White);
            image = new EffImage(bmp, ImageColorType.Colored);

            var rnd = new Random();
            // 随机几个前景点
            Dictionary<int, int> marks = new Dictionary<int, int>();
            for (int i = 0; i < 100; i++)
            {
                var x = rnd.Next(0, 32);
                if (marks.ContainsKey(x))
                {
                    ++marks[x];
                }
                else
                {
                    marks[x] = 1;
                }

                image.Set(x, marks[x], fore);
            }

            var hist = image.ProjectionHistH();

            for (int i = 0; i < image.Width; i++)
            {
                if (marks.ContainsKey(i))
                {
                    Assert.AreEqual(marks[i], hist[i]);
                }
            }
        }
    }
}
