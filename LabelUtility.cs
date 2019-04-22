using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TensorFlowSharpSSD
{
    internal static class LabelUtility
    {
        public static LabelInfo[] GetLabelsFromFile(string textFilePath)
        {
            return GetLabels(GetLabelTextList(textFilePath));
        }

        public static LabelInfo[] GetLabels(string[] labelTexts)
        {
            var labels = new LabelInfo[labelTexts.Length];
            float length = labelTexts.Length;

            for (int idx = 0; idx < labels.Length; ++idx)
            {
                var color = ColorUtility.ColorFromHue(idx / length);

                var label = new LabelInfo
                {
                    ClassId = idx,
                    Text = labelTexts[idx],
                    DrawBrush = new SolidBrush(color),
                };
                label.DrawPen = new Pen(label.DrawBrush, 4.0f);

                labels[idx] = label;
            }

            return labels;
        }

        public static string[] GetLabelTextList(string textFilePath)
        {
            using (var file = File.OpenRead(textFilePath))
            using (var reader = new StreamReader(file))
            {
                var list = new List<string>();
                list.Add("Unknown");

                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    list.Add(line);
                }

                return list.ToArray();
            }
        }
    }
}
