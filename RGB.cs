using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TensorFlowSharpSSD
{
    internal struct RGB
    {
        public byte B;
        public byte G;
        public byte R;

        public unsafe void ReverseTo(RGB* value)
        {
            value->B = this.R;
            value->G = this.G;
            value->R = this.B;
        }
    }

    internal struct ARGB
    {
        public byte A;
        public byte B;
        public byte G;
        public byte R;

        public unsafe void ReverseTo(RGB* value)
        {
            value->R = this.B;
            value->G = this.G;
            value->B = this.R;
        }
    }
}
