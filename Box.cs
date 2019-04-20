using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TensorFlowSharpSSD
{
    internal class Box
    {
        public int ClassId { get; }
        public float Score { get; }
        public Rectangle Rectangle { get; }

        public Box(int classId, float score, int x, int y, int width, int height)
        {
            this.ClassId = classId;
            this.Score = score;
            this.Rectangle = new Rectangle(x, y, width, height);
        }
    }
}
