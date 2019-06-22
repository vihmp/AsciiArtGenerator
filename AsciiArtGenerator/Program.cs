using System;
using System.Drawing;
using System.IO;
using System.Globalization;

namespace AsciiArtGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            string inputFile = args[0];
            int beta;
            double threshold;
            ushort iterationsCount;

            if (!int.TryParse(args[1], out beta))
            {
                Console.WriteLine("Error: 'beta' has invalid value, possible values are 0, 1 or 2");
                return;
            }

            if((beta != 0) && (beta != 1) && (beta != 2))
            {
                Console.WriteLine("Error: 'beta' has invalid value, possible values are 0, 1 or 2");
                return;
            }

            if(!double.TryParse(args[2], NumberStyles.Float, CultureInfo.InvariantCulture, out threshold))
            {
                Console.WriteLine(
                    "Error: 'threshold' has invalid value, possible values are from 0.0 to 1.0");
                return;
            }

            if((threshold < 0.0) || (threshold > 1.0))
            {
                Console.WriteLine(
                    "Error: 'threshold' has invalid value, possible values are from 0.0 to 1.0");
                return;
            }

            if(!ushort.TryParse(args[3], out iterationsCount))
            {
                Console.WriteLine(string.Format(
                    "Error: 'iterations_count' has invalid value, possible values are from 1 to {0}",
                    ushort.MaxValue));
                return;
            }

            if(iterationsCount < 1)
            {
                Console.WriteLine(string.Format(
                    "Error: 'iterations_count' has invalid value, possible values are from 1 to {0}",
                    ushort.MaxValue));
                return;
            }

            Bitmap image;
            char[,] asciiRepresentation;

            try
            {
                image = new Bitmap(inputFile);
            }
            catch
            {
                Console.WriteLine("Error: cannot open specified image");
                return;
            }

            Console.WriteLine("Converting image...");

            try
            {
                asciiRepresentation = ImageConverter.ConvertImage(
                    image, 
                    beta, 
                    threshold, 
                    iterationsCount,
                    progress => Console.WriteLine(string.Format("{0}%", progress)));
            }
            catch
            {
                Console.WriteLine("Error: cannot convert specified image");
                return;
            }

            Console.WriteLine("Saving result to output.html...");

            using (FileStream output = File.Open("output.html", FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter writer = new StreamWriter(output))
                {
                    writer.WriteLine("<font face=\"courier\"><pre>");
                    for (int i = 0; i < asciiRepresentation.GetLength(0); i++)
                    {
                        for (int j = 0; j < asciiRepresentation.GetLength(1); j++)
                        {
                            writer.Write(asciiRepresentation[i, j]);
                        }
                        writer.WriteLine();
                    }
                    writer.Write("</pre></font>");
                }
            }

            Console.WriteLine("Done!");
        }
    }
}
