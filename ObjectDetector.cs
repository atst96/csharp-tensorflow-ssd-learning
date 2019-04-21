using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TensorFlow;

namespace TensorFlowSharpSSD
{
    internal class ObjectDetector : IDisposable
    {
        private readonly TFSession _session;
        private readonly TFOutput _detectionInputTensor;
        private readonly TFOutput[] _detectionOutputTensors;

        public ObjectDetector(string graphPath)
        {
            using (var graph = new TFGraph())
            {
                var model = File.ReadAllBytes(graphPath);
                graph.Import(new TFBuffer(model));

                this._session = new TFSession(graph);
                this._detectionInputTensor = graph["image_tensor"][0];
                this._detectionOutputTensors = new[]
                {
                    graph["detection_boxes"][0],
                    graph["detection_scores"][0],
                    graph["detection_classes"][0],
                    // graph["num_detections"][0],
                };
            }
        }

        private static (TFSession session, TFOutput input, TFOutput output) CreateRawImageGraph(int imageWidth, int imageHeight)
        {
            var graph = new TFGraph();
            var input = graph.Placeholder(TFDataType.String);
            var rawImage = graph.DecodeRaw(input, TFDataType.UInt8);

            var shape = graph.Stack(new[]
            {
                graph.Const(-1, TFDataType.Int32),
                graph.Const(imageHeight, TFDataType.Int32),
                graph.Const(imageWidth, TFDataType.Int32),
                graph.Const(3, TFDataType.Int32),
            });

            var output = graph.Reshape(rawImage, shape);

            return (new TFSession(graph), input, output);
        }

        private TFTensor CreateTensor(byte[] imageData, int width, int height)
        {
            var (session, input, output) = CreateRawImageGraph(width, height);

            using (session)
            {
                var value = TFTensor.CreateString(imageData);

                return session.Run(
                    inputValues: new[] { value  },
                    inputs     : new[] { input  },
                    outputs    : new[] { output }
                )[0];
            }
        }

        public Box[][] Predict(byte[] data, int width, int height)
        {
            var tensor = this.CreateTensor(data, width, height);

            var runner = this._session.GetRunner();

            var output = runner
                .AddInput(this._detectionInputTensor, tensor)
                .Fetch(this._detectionOutputTensors)
                .Run();

            var boxes = output[0].GetValue<float[][][]>();
            var scores = output[1].GetValue<float[][]>();
            var classes = output[2].GetValue<float[][]>();

            var predicts = new Box[boxes.Length][];

            for (int i = 0; i < predicts.Length; ++i)
            {
                predicts[i] = GetBoxes(width, height, boxes[i], scores[i], classes[i]);
            }

            return predicts;
        }

        public Box[] Predict(Bitmap image)
        {
            var data = ToBytes(image);

            return Predict(data, image.Width, image.Height)[0];
        }

        private static Box[] GetBoxes(int width, int height, float[][] boxes, float[] scores, float[] classes)
        {
            var results = new Box[boxes.Length];

            for (int i = 0; i < results.Length; ++i)
            {
                var box = boxes[i];
                (float ymin, float xmin) = (box[0], box[1]);
                (float ymax, float xmax) = (box[2], box[3]);

                int classId = (int)classes[i];
                float score = scores[i];

                int x = (int)(xmin * width - 1f);
                int y = (int)(ymin * height - 1f);
                int w = (int)((xmax - xmin) * width);
                int h = (int)((ymax - ymin) * height);

                results[i] = new Box(classId, score, x, y, w, h);
            }

            return results;
        }

        private static byte[] ToBytes(Bitmap image)
        {
            var rect = new Rectangle(0, 0, image.Width, image.Height);
            var imageData = image.LockBits(rect, ImageLockMode.ReadOnly, image.PixelFormat);

            try
            {
                switch (image.PixelFormat)
                {
                    case PixelFormat.Format24bppRgb:
                        return ToRGBFrom24Bpp(imageData);

                    case PixelFormat.Format32bppArgb:
                        return ToRGBFrom32Bpp(imageData);

                    default:
                        throw new NotImplementedException();
                }
            }
            finally
            {
                image.UnlockBits(imageData);
            }
        }

        private static unsafe byte[] ToRGBFrom24Bpp(BitmapData imageData)
        {
            int width = imageData.Width;
            int height = imageData.Height;
            int destStride = width * sizeof(RGB);

            var dist = new byte[destStride * imageData.Height];

            RGB* srcImgPtr = (RGB*)imageData.Scan0;
            fixed (byte* destImgDataPinnedPtr = dist)
            {
                byte* destImageDataPtr = destImgDataPinnedPtr;

                Parallel.For(0, imageData.Height, y =>
                {
                    RGB* yDestRgbPtr = (RGB*)(destImageDataPtr + (destStride * y));
                    RGB* ySrcRgbPtr = srcImgPtr + (width * y);

                    for (int x = 0; x < width; ++x)
                    {
                        ySrcRgbPtr++->ReverseTo(yDestRgbPtr++);
                    }
                });
            }

            return dist;
        }

        private static unsafe byte[] ToRGBFrom32Bpp(BitmapData imageData)
        {
            int width = imageData.Width;
            int height = imageData.Height;
            int destStride = width * sizeof(RGB);

            var dist = new byte[imageData.Width * imageData.Height * 3];

            ARGB* srcImgPtr = (ARGB*)imageData.Scan0;
            fixed (byte* destImgDataPinnedPtr = dist)
            {
                byte* destImageDataPtr = destImgDataPinnedPtr;

                Parallel.For(0, imageData.Height, y =>
                {
                    RGB* yDestRgbPtr = (RGB*)(destImageDataPtr + (destStride * y));
                    ARGB* ySrcRgbPtr = srcImgPtr + (width * y);

                    for (int x = 0; x < width; ++x)
                    {
                        ySrcRgbPtr++->ReverseTo(yDestRgbPtr++);
                    }
                });
            }

            return dist;
        }

        public void Dispose()
        {
            this._session.Dispose();
        }
    }
}
