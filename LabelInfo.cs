using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TensorFlowSharpSSD
{
    internal class LabelInfo
    {
        public int ClassId { get; set; }
        public string Text { get; set; }
        public Pen DrawPen { get; set; }
        public SolidBrush DrawBrush { get; set; }
    }
}
