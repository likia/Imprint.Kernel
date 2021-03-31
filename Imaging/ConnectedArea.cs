using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imprint.Imaging
{
    /// <summary>
    /// 图像中的连接区域
    /// </summary>
    public class ConnectedArea
    {
        public List<Point> Points
        {
            get;
            set;
        } = new List<Point>();

        /// <summary>
        /// 实际区域
        /// </summary>
        public Rectangle ValidArea
        {
            get
            {
                int minX = int.MaxValue, minY = int.MaxValue, maxX = 0, maxY = 0;

                foreach (var p in Points)
                {
                    if (p.X < minX) minX = p.X;

                    if (p.Y < minY) minY = p.Y;

                    if (p.X > maxX) maxX = p.X;

                    if (p.Y > maxY) maxY = p.Y;
                }
                return new Rectangle(minX, minY, maxX - minX + 1, maxY - minY + 1);
            }
        }

        /// <summary>
        /// 区域大小
        /// </summary>
        public Size Size
        {
            get
            {
                return ValidArea.Size;
            }
        }
    }
}
