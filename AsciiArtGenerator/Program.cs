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
            if(args.Length == 0)
            {
                Console.WriteLine("Input file is undefined");
                return;
            }

            if(args[0] == "/?")
            {
                Console.WriteLine();
                Console.WriteLine("Usage: AsciiArtGenerator <input_image> [/P] [/B <beta>] [/T <threshold>] ");
                Console.WriteLine("       [/I <iterations_count>] [/O <output_file>]");
                Console.WriteLine();
                Console.WriteLine("Options:");
                Console.WriteLine(string.Format("   {0,-25}{1}", "/P", "Use this option to convert the image using the"));
                Console.WriteLine(string.Format("   {0,-25}{1}", string.Empty, "pseudoinverse."));
                Console.WriteLine(string.Format("   {0,-25}{1}", "/B <beta>", "Sets 'beta' parameter, that affects cost function"));
                Console.WriteLine(string.Format("   {0,-25}{1}", string.Empty, "(ignored if /P is set). Default value is 2.0."));
                Console.WriteLine(string.Format("   {0,-25}{1}", "/T <threshold>", "Sets threshold for maximum activation values."));
                Console.WriteLine(string.Format("   {0,-25}{1}", string.Empty, "Possible values are from 0.0 to 1.0."));
                Console.WriteLine(string.Format("   {0,-25}{1}", string.Empty, "Default value is 0.0."));
                Console.WriteLine(string.Format("   {0,-25}{1}", "/I <iterations_count>", "Sets number of iterations of an algorithm."));
                Console.WriteLine(string.Format("   {0,-25}{1} {2}", string.Empty, "Possible values are from 1 to", ushort.MaxValue));
                Console.WriteLine(string.Format("   {0,-25}{1}", string.Empty, "(ignored if /P is set). Default value is 100."));
                Console.WriteLine(string.Format("   {0,-25}{1}", "/O <output_file>", "Sets name of output HTML file."));
                return;
            }

            string inputFile = args[0];
            bool pseudoInverseMode = false;
            double beta = 2.0;
            double threshold = 0.0;
            ushort iterationsCount = 100;
            string outputFile = "output.html";

            string errorMsg = ParseArgs(
                args, 
                ref pseudoInverseMode, 
                ref beta, 
                ref threshold, 
                ref iterationsCount, 
                ref outputFile);

            if(!string.IsNullOrEmpty(errorMsg))
            {
                Console.WriteLine(errorMsg);
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
                Console.WriteLine(string.Format("Cannot open file {0}", inputFile));
                return;
            }

            Console.WriteLine("Converting image...");

            try
            {
                if (!pseudoInverseMode)
                {
                    asciiRepresentation = ImageConverter.ConvertImage(
                        image,
                        beta,
                        threshold,
                        iterationsCount,
                        progress => Console.WriteLine(string.Format("{0}%", progress)));
                }
                else
                {
                    asciiRepresentation = ImageConverter.ConvertImage(image, threshold);
                }
            }
            catch
            {
                Console.WriteLine("Cannot convert specified image");
                return;
            }

            Console.WriteLine(string.Format("Saving result to {0}...", outputFile));

            using (FileStream output = File.Open(outputFile, FileMode.Create, FileAccess.Write))
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

        static string ParseArgs(
            string[] args, 
            ref bool pseudoInverseMode,
            ref double beta,
            ref double threshold,
            ref ushort iterationsCount,
            ref string outputFile)
        {
            for (int i = 1; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "/P":
                    case "/p":
                        pseudoInverseMode = true;
                        break;
                    case "/B":
                    case "/b":
                        if ((i + 2) > args.Length)
                        {
                            return "Parameter /B value is undefined";
                        }

                        if (!double.TryParse(args[i + 1], out beta))
                        {
                            return "Parameter /B has invalid value";
                        }

                        i++;
                        break;
                    case "/T":
                    case "/t":
                        if ((i + 2) > args.Length)
                        {
                            return "Parameter /T value is undefined";
                        }

                        if (!double.TryParse(
                            args[i + 1],
                            NumberStyles.Float,
                            CultureInfo.InvariantCulture,
                            out threshold))
                        {
                            return "Parameter /T has invalid value, possible values are from 0.0 to 1.0";
                        }

                        if ((threshold < 0.0) || (threshold > 1.0))
                        {
                            return "Parameter /T has invalid value, possible values are from 0.0 to 1.0";
                        }
                        i++;
                        break;
                    case "/I":
                    case "/i":
                        if ((i + 2) > args.Length)
                        {
                            return "Parameter /I value is undefined";
                        }

                        if (!ushort.TryParse(args[i + 1], out iterationsCount))
                        {
                            return string.Format(
                                "Parameter /I has invalid value, possible values are from 1 to {0}",
                                ushort.MaxValue);
                        }

                        if (iterationsCount < 1)
                        {
                            return string.Format(
                                "Parameter /I has invalid value, possible values are from 1 to {0}",
                                ushort.MaxValue);
        }
                        i++;
                        break;
                    case "/O":
                    case "/o":
                        if ((i + 2) > args.Length)
                        {
                            return "Parameter /O value is undefined";
                        }

                        outputFile = string.Format("{0}.html", args[i + 1]);
                        i++;
                        break;
                    default:
                        return string.Format("Invalid parameter {0}", args[i]);
                }
            }

            return string.Empty;
        }
    }
}
