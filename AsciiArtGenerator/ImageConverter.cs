using System;
using System.Linq;
using System.Drawing;
using System.Threading.Tasks;

namespace AsciiArtGenerator
{
    public partial class ImageConverter
    {
        public static char[,] ConvertImage(
            Bitmap image, 
            int beta,
            double threshold,
            ushort iterationsCount,
            Action<int> ProgressUpdated)
        {
            int charNumHor = (int)Math.Round((double)image.Width / glyphWidth);
            int charNumVert = (int)Math.Round((double)image.Height / glyphHeight);
            int totalCharactersNumber = charNumVert * charNumHor;
            int glyphSetSize = wNorm.GetLength(1);

            double[,] v = SplitImage(image, charNumVert, charNumHor);

            double[,] h = new double[glyphSetSize, totalCharactersNumber];
            Random rand = new Random();

            for (int i = 0; i < glyphSetSize; i++)
            {
                for (int j = 0; j < totalCharactersNumber; j++)
                {
                    h[i, j] = rand.NextDouble();
                }
            }

            int progress = 0;
            ushort step = (ushort)(iterationsCount / 10);

            for (ushort i = 0; i < iterationsCount; i++)
            {
                UpdateH(v, wNorm, h, beta);

                if((i + 1) % step == 0)
                {
                    progress += 10;

                    if(progress < 100)
                    {
                        ProgressUpdated(progress);
                    }
                }
            }

            var result = GetAsciiRepresentation(h, charNumVert, charNumHor, threshold);
            ProgressUpdated(100);

            return result;
        }

        private static double[,] SplitImage(Bitmap image, int charNumVert, int charNumHor)
        {
            double[,] result =
                new double[glyphHeight * glyphWidth, charNumHor * charNumVert];

            for (int y = 0; y < charNumVert; y++)
            {
                for (int x = 0; x < charNumHor; x++)
                {
                    double[] bitmapVector = new double[glyphHeight * glyphWidth];

                    for (int j = 0; j < glyphHeight; j++)
                    {
                        for (int i = 0; i < glyphWidth; i++)
                        {
                            byte color = (x * glyphWidth + i < image.Width) && (y * glyphHeight + j < image.Height) ?
                                (byte)(255 - image.GetPixel(x * glyphWidth + i, y * glyphHeight + j).R) : (byte)0;
                            bitmapVector[glyphWidth * j + i] = color;
                        }
                    }

                    double l2norm = Math.Sqrt(bitmapVector.Select(value => value * value).Sum());

                    for (int k = 0; k < bitmapVector.Length; k++)
                    {
                        if (l2norm != 0.0)
                        {
                            result[k, charNumHor * y + x] = bitmapVector[k] / l2norm;
                        }
                        else
                        {
                            result[k, charNumHor * y + x] = bitmapVector[k];
                        }
                    }
                }
            }

            return result;
        }

        private static void UpdateH(double[,] v, double[,] w, double[,] h, int beta)
        {
            double[,] vApprox = MultiplyMatrix(w, h);

            Parallel.For(0, h.GetLength(0), j =>
            {
                for (int k = 0; k < h.GetLength(1); k++)
                {
                    double numerator = 0.0;
                    double denominator = 0.0;

                    for (int i = 0; i < w.GetLength(0); i++)
                    {
                        if (vApprox[i, k] != 0.0)
                        {
                            numerator += w[i, j] * v[i, k] / Math.Pow(vApprox[i, k], 2 - beta);
                        }
                        else
                        {
                            numerator += w[i, j] * v[i, k];
                        }

                        denominator += w[i, j] * Math.Pow(vApprox[i, k], beta - 1);
                    }

                    if (denominator != 0.0)
                    {
                        h[j, k] = h[j, k] * numerator / denominator;
                    }
                    else
                    {
                        h[j, k] = h[j, k] * numerator;
                    }
                }
            });
        }

        private static double[,] MultiplyMatrix(double[,] a, double[,] b)
        {
            double[,] c = new double[a.GetLength(0), b.GetLength(1)];

            for (int i = 0; i < c.GetLength(0); i++)
            {
                for (int j = 0; j < c.GetLength(1); j++)
                {
                    for (int k = 0; k < a.GetLength(1); k++)
                    {
                        c[i, j] += a[i, k] * b[k, j];
                    }
                }
            }

            return c;
        }

        private static char[,] GetAsciiRepresentation(
            double[,] h, 
            int charNumVert, 
            int charNumHor, 
            double threshold)
        {
            char[,] result = new char[charNumVert, charNumHor];

            for (int j = 0; j < h.GetLength(1); j++)
            {
                double max = 0.0;
                int maxIndex = 0;

                for (int i = 0; i < h.GetLength(0); i++)
                {
                    if (max < h[i, j])
                    {
                        max = h[i, j];
                        maxIndex = i;
                    }
                }

                result[j / charNumHor, j % charNumHor] =
                   (max >= threshold) ? (char)(firstGlyphCode + maxIndex) : ' ';
            }

            return result;
        }
    }
}