using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TensorFlowSharpSSD
{
    internal static class ColorUtility
    {
        public static Color ColorFromHue(float hue)
        {
            return Color.FromArgb(180, ColorFromHSV(hue, 0.5f, 1.0f));
        }

        public static Color ColorFromHSV(float h, float s, float v)
        {
            float r, g, b;

            if (s > 0.0f)
            {
                float _h = (h >= 1f ? (h - (h % 1f)) : h) * 6.0f;
                float c = s;
                float x = c * (1 - Math.Abs(_h % 2 - 1));

                float _col = v - c;
                (r, g, b) = (_col, _col, _col);

                switch (Math.Floor(_h))
                {
                    case 0.0f:
                        r += c;
                        g += x;
                        break;
                    case 1.0f:
                        r += x;
                        g += c;
                        break;
                    case 2.0f:
                        g += c;
                        b += x;
                        break;
                    case 3.0f:
                        g += x;
                        b += c;
                        break;
                    case 4.0f:
                        r += x;
                        b += c;
                        break;
                    case 5.0f:
                        r += c;
                        b += x;
                        break;
                    default:
                        (r, g, b) = (0.0f, 0.0f, 0.0f);
                        break;
                }
            }
            else
            {
                (r, g, b) = (v, v, v);
            }

            return Color.FromArgb((int)(r * 255), (int)(g * 255), (int)(b * 255));
        }
    }
}
