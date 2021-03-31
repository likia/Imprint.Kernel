using Imprint.Imaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
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
