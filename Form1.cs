using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TensorFlowSharpSSD
{
    public partial class Form1 : Form
    {
        public const float LabelFontSize = 14f;
        public const string LabelFontName = "TakaoPGothic";

        public const float ScoreThreshold = 0.5f;
        public const string GraphFilePath = "./frozen_inference_graph.pb";
        public const string LabelFilePath = "./labels.txt";

        public Form1()
        {
            this.InitializeComponent();
        }

        protected override async void OnShown(EventArgs e)
        {
            base.OnShown(e);

            var ofd = new OpenFileDialog
            {
                Filter = "画像ファイル|*.png;*.jpg;*.jpeg;*.tiff;*.tif;*.bmp",
            };

            this.UseWaitCursor = true;

            this.SetStatusText("Loading labels...");

            var labels = LabelUtility.GetLabelsFromFile(LabelFilePath);

            this.SetStatusText("Loading graph data...");

            var detector = new ObjectDetector(GraphFilePath);

            if (ofd.ShowDialog(this) == DialogResult.OK)
            {
                this.Text = $"{Path.GetFileName(ofd.FileName)} / 閾値: {ScoreThreshold:0.00}";

                using (var image = new Bitmap(ofd.FileName))
                {
                    this.SetStatusText("Loading...");

                    var results = await Task.Run(() => detector.Predict(image));
                    var output = await Task.Run(() => DrawLabels(image, results, labels, threshold: ScoreThreshold));

                    this.HideStatusText();

                    this.SetPreviewImage(output);
                }

            }

            this.UseWaitCursor = false;

            detector.Dispose();

            ofd.Dispose();
            ofd = null;
        }

        private void SetPreviewImage(Bitmap image)
        {
            this.pictureBox1.Image = image;
        }

        private void ShowStatusText()
        {
            this.label1.Visible = true;
        }

        private void HideStatusText()
        {
            this.label1.Visible = false;
        }

        private void SetStatusText(string text)
        {
            this.label1.Text = text;
        }

        private Bitmap DrawLabels(Bitmap image, Box[] boxes, LabelInfo[] labels, float threshold = 0f)
        {
            var destImage = new Bitmap(image);

            var g = Graphics.FromImage(destImage);
            var textDrawBrush = Brushes.Black;

            var font = new Font(LabelFontName, LabelFontSize);

            foreach (var box in boxes.Where(b => b.Score >= threshold))
            {
                var label = labels[box.ClassId];

                var labelText = $"{labels[box.ClassId].Text} ({box.Score:0.00})";
                var rect = box.Rectangle;

                g.DrawRectangle(label.DrawPen, box.Rectangle);

                var textSize = g.MeasureString(labelText, font);
                float textYPos = Math.Max(0, rect.Y - (int)textSize.Height);

                g.FillRectangle(label.DrawBrush, rect.X, textYPos, textSize.Width, textSize.Height);
                g.DrawString(labelText, font, textDrawBrush, rect.X, textYPos);
            }

            g.Dispose();

            return destImage;
        }
    }
}
