using System;
using System.Drawing;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Distributions;

namespace AsciiArtGenerator
{
    public partial class ImageConverter
    {
        public static char[,] ConvertImage(Bitmap image, double threshold)
        {
            int charNumHor = (int)Math.Round((double)image.Width / glyphWidth);
            int charNumVert = (int)Math.Round((double)image.Height / glyphHeight);

            Matrix<double> v = SplitImage(image, charNumVert, charNumHor);
            Matrix<double> h = wNorm.PseudoInverse().Multiply(v);

            var result = GetAsciiRepresentation(h, charNumVert, charNumHor, threshold);

            return result;
        }

        public static char[,] ConvertImage(
            Bitmap image, 
            double beta,
            double threshold,
            ushort iterationsCount,
            ushort threadsNumber,
            Action<int> ProgressUpdated)
        {
            int charNumHor = (int)Math.Round((double)image.Width / glyphWidth);
            int charNumVert = (int)Math.Round((double)image.Height / glyphHeight);
            int totalCharactersNumber = charNumVert * charNumHor;
            int glyphSetSize = wNorm.ColumnCount;

            Matrix<double> v = SplitImage(image, charNumVert, charNumHor);

            Matrix<double> h = Matrix<double>.Build.Random(
                glyphSetSize, 
                totalCharactersNumber, 
                new ContinuousUniform());

            int progress = 0;
            ushort step = (ushort)(iterationsCount / 10);

            for (ushort i = 0; i < iterationsCount; i++)
            {
                UpdateH(v, wNorm, h, beta, threadsNumber);

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

        private static Matrix<double> SplitImage(Bitmap image, int charNumVert, int charNumHor)
        {
            Matrix<double> result = 
                Matrix<double>.Build.Dense(glyphHeight * glyphWidth, charNumHor * charNumVert);

            for (int y = 0; y < charNumVert; y++)
            {
                for (int x = 0; x < charNumHor; x++)
                {
                    for (int j = 0; j < glyphHeight; j++)
                    {
                        for (int i = 0; i < glyphWidth; i++)
                        {
                            byte color = 0;

                            if ((x * glyphWidth + i < image.Width) &&
                                (y * glyphHeight + j < image.Height))
                            {
                                color = (byte)(255 - image.GetPixel(
                                    x * glyphWidth + i,
                                    y * glyphHeight + j).R);
                            }

                            result[glyphWidth * j + i, charNumHor * y + x] = color;
                        }
                    }
                }
            }

            result = result.NormalizeColumns(2.0);

            return result;
        }

        private static void UpdateH(
            Matrix<double> v, 
            Matrix<double> w, 
            Matrix<double> h, 
            double beta,
            ushort threadsNumber)
        {
            const double epsilon = 1e-6;
            Matrix<double> vApprox = w.Multiply(h);

            Parallel.For(0, h.RowCount, new ParallelOptions() { MaxDegreeOfParallelism = threadsNumber }, j =>
            {
                for (int k = 0; k < h.ColumnCount; k++)
                {
                    double numerator = 0.0;
                    double denominator = 0.0;

                    for (int i = 0; i < w.RowCount; i++)
                    {
                        if (Math.Abs(vApprox[i, k]) > epsilon)
                        {
                            numerator += w[i, j] * v[i, k] / Math.Pow(vApprox[i, k], 2.0 - beta);
                            denominator += w[i, j] * Math.Pow(vApprox[i, k], beta - 1.0);
                        }
                        else
                        {
                            numerator += w[i, j] * v[i, k];

                            if (beta - 1.0 > 0.0)
                            {
                                denominator += w[i, j] * Math.Pow(vApprox[i, k], beta - 1.0);
                            }
                            else
                            {
                                denominator += w[i, j];
                            }
                        }
                    }

                    if (Math.Abs(denominator) > epsilon)
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

        private static char[,] GetAsciiRepresentation(
            Matrix<double> h, 
            int charNumVert, 
            int charNumHor, 
            double threshold)
        {
            char[,] result = new char[charNumVert, charNumHor];

            for (int j = 0; j < h.ColumnCount; j++)
            {
                double max = 0.0;
                int maxIndex = 0;

                for (int i = 0; i < h.RowCount; i++)
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