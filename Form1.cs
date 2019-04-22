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

        private LabelInfo[] _labels;
        private ObjectDetector _objectDetector;

        public Form1()
        {
            this.InitializeComponent();
            this.ContextMenu = this.contextMenu1;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            this.SetStatusText("Loading labels...");

            this._labels = LabelUtility.GetLabelsFromFile(LabelFilePath);

            this.SetStatusText("Loading graph data...");

            this._objectDetector = new ObjectDetector(GraphFilePath);

            this.HideStatusText();

            this.SelectImage();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            this._objectDetector?.Dispose();

            base.OnClosing(e);
        }

        private Image GetPreviewImage()
        {
            return this.pictureBox1.Image;
        }

        private void SetPreviewImage(Bitmap image)
        {
            this.pictureBox1.Image = image;
        }

        private void ShowStatusText()
        {
            this.label1.Visible = true;
            this.menuItem1.Enabled = false;
            this.menuItem3.Enabled = false;
        }

        private void HideStatusText()
        {
            this.label1.Visible = false;
            this.menuItem1.Enabled = true;
            this.menuItem3.Enabled = true;
        }

        private void SetStatusText(string text)
        {
            this.label1.Text = text;
        }

        private async void SelectImage()
        {
            var ofd = new OpenFileDialog
            {
                Filter = "画像ファイル|*.png;*.jpg;*.jpeg;*.tiff;*.tif;*.bmp",
            };

            this.UseWaitCursor = true;

            this.SetStatusText("Loading...");
            this.ShowStatusText();

            if (ofd.ShowDialog(this) == DialogResult.OK)
            {
                this.Text = $"{Path.GetFileName(ofd.FileName)} / 閾値: {ScoreThreshold:0.00}";

                using (var image = new Bitmap(ofd.FileName))
                {
                    var results = await Task.Run(() => this._objectDetector.Predict(image));

                    var validBoxes = results
                        .Where(b => b.Score > ScoreThreshold)
                        .OrderBy(b => b.Score);

                    var output = await Task.Run(() => DrawLabels(image, validBoxes, this._labels));

                    this.HideStatusText();

                    this.SetPreviewImage(output);
                }

            }

            this.UseWaitCursor = false;

            ofd.Dispose();
            ofd = null;

            this.HideStatusText();
        }

        private void SaveImage()
        {
            var dialog = new SaveFileDialog()
            {
                Filter = "BMP|*.bmp|PNG|*.png|JPEG|*.jpg|TIFF|*.tif;*.tiff|すべて|*.*"
            };

            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                var image = this.GetPreviewImage();
                image.Save(dialog.FileName);
            }

            dialog.Dispose();
            dialog = null;
        }

        private Bitmap DrawLabels(Bitmap image, IEnumerable<Box> boxes, LabelInfo[] labels, bool drawLegend = true)
        {
            var destImage = new Bitmap(image);

            var g = Graphics.FromImage(destImage);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            float textPadding = 2f;

            var textDrawBrush = (SolidBrush)Brushes.Black;

            var font = new Font(LabelFontName, LabelFontSize);

            foreach (var box in boxes)
            {
                var label = labels[box.ClassId];

                var labelText = $"{labels[box.ClassId].Text} ({box.Score:0.00})";
                var rect = box.Rectangle;

                g.DrawRectangle(label.DrawPen, box.Rectangle);

                var textSize = g.MeasureString(labelText, font);
                float textYPos = Math.Max(0, rect.Y - textSize.Height);

                g.FillRectangle(label.DrawBrush, rect.X - textPadding, textYPos - textPadding, textSize.Width + textPadding, textSize.Height + textPadding);
                g.DrawString(labelText, font, textDrawBrush, rect.X, textYPos);
            }

            if (drawLegend)
            {
                var pos = new PointF();

                foreach (var label in labels.Skip(1))
                {
                    int classCount = boxes.Count(box => box.ClassId == label.ClassId);

                    var text = $"#{label.ClassId} {label.Text} ({classCount})";
                    var rect = DrawStringWithBackground(g, text, font, textDrawBrush, label.DrawBrush, pos, 4);

                    pos.Y += (int)rect.Height;
                }
            }

            g.Dispose();

            return destImage;
        }

        private static RectangleF DrawStringWithBackground(Graphics g, string text, Font font, Brush textBrush, Brush backgroundBrush, PointF pos, float padding = 0f)
        {
            var textSize = g.MeasureString(text, font);
            var rect = new RectangleF(pos.X, pos.Y, textSize.Width + padding * 2, textSize.Height + padding * 2);

            g.FillRectangle(backgroundBrush, rect);
            g.DrawString(text, font, textBrush, pos.X + padding, pos.Y + padding);

            return rect;
        }

        private void MenuItem1_Click(object sender, EventArgs e)
        {
            this.SelectImage();
        }

        private void MenuItem3_Click(object sender, EventArgs e)
        {
            this.SaveImage();
        }
    }
}
