using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LifeSimulator
{
    public class Resource
    {
        public Point Location { get; set; }
        public int Size { get; set; }

        public void Draw(Graphics g)
        {
            g.FillEllipse(new SolidBrush(Color.Green), new Rectangle(Location.X - Size/2, Location.Y - Size / 2, Size, Size));
        }
    }
}
