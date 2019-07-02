using System;
using System.Drawing;
using System.IO;
using System.Globalization;

namespace AsciiArtGenerator
{
    enum Modes
    {
        Pseudoinverse,
        BetaDivergence
    }

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
                Console.WriteLine(string.Format("   {0,-25}{1}", "/B <beta>", "Sets 'beta' parameter, that enables the selection"));
                Console.WriteLine(string.Format("   {0,-25}{1}", string.Empty, "of many cost functions (ignored if /P is set)."));
                Console.WriteLine(string.Format("   {0,-25}{1}", string.Empty, "Possible values:"));
                Console.WriteLine(string.Format("   {0,-25}{1}", string.Empty, "0 - Itakura-Saito Divergence"));
                Console.WriteLine(string.Format("   {0,-25}{1}", string.Empty, "1 - Kullback-Leibler Divergence"));
                Console.WriteLine(string.Format("   {0,-25}{1}", string.Empty, "2 - Squared Euclidean Distance"));
                Console.WriteLine(string.Format("   {0,-25}{1}", "/T <threshold>", "Sets threshold for maximum activation values."));
                Console.WriteLine(string.Format("   {0,-25}{1}", string.Empty, "Possible values are from 0.0 to 1.0."));
                Console.WriteLine(string.Format("   {0,-25}{1}", "/I <iterations_count>", "Sets number of iterations of an algorithm."));
                Console.WriteLine(string.Format("   {0,-25}{1} {2}", string.Empty, "Possible values are from 1 to", ushort.MaxValue));
                Console.WriteLine(string.Format("   {0,-25}{1}", string.Empty, "(ignored if /P is set)."));
                Console.WriteLine(string.Format("   {0,-25}{1}", "/O <output_file>", "Sets name of output HTML file."));
                return;
            }

            string inputFile = args[0];
            Modes mode = Modes.BetaDivergence;
            int beta = 1;
            double threshold = 0.0;
            ushort iterationsCount = 100;
            string outputFile = "output.html";

            for(int i = 1; i < args.Length; i++)
            {
                switch(args[i])
                {
                    case "/P":
                    case "/p":
                        mode = Modes.Pseudoinverse;
                        break;
                    case "/B":
                    case "/b":
                        if ((i + 2) > args.Length)
                        {
                            Console.WriteLine("Parameter /B value is undefined");
                            return;
                        }

                        if (!int.TryParse(args[i + 1], out beta))
                        {
                            Console.WriteLine(
                                "Parameter /B has invalid value, possible values are 0, 1 or 2");
                            return;
                        }

                        if ((beta != 0) && (beta != 1) && (beta != 2))
                        {
                            Console.WriteLine(
                                "Parameter /B has invalid value, possible values are 0, 1 or 2");
                            return;
                        }
                        i++;
                        break;
                    case "/T":
                    case "/t":
                        if ((i + 2) > args.Length)
                        {
                            Console.WriteLine("Parameter /T value is undefined");
                            return;
                        }

                        if (!double.TryParse(
                            args[i + 1], 
                            NumberStyles.Float, 
                            CultureInfo.InvariantCulture, 
                            out threshold))
                        {
                            Console.WriteLine(
                                "Parameter /T has invalid value, possible values are from 0.0 to 1.0");
                            return;
                        }

                        if ((threshold < 0.0) || (threshold > 1.0))
                        {
                            Console.WriteLine(
                                "Parameter /T has invalid value, possible values are from 0.0 to 1.0");
                            return;
                        }
                        i++;
                        break;
                    case "/I":
                    case "/i":
                        if ((i + 2) > args.Length)
                        {
                            Console.WriteLine("Parameter /I value is undefined");
                            return;
                        }

                        if (!ushort.TryParse(args[i + 1], out iterationsCount))
                        {
                            Console.WriteLine(string.Format(
                                "Parameter /I has invalid value, possible values are from 1 to {0}",
                                ushort.MaxValue));
                            return;
                        }

                        if (iterationsCount < 1)
                        {
                            Console.WriteLine(string.Format(
                                "Parameter /I has invalid value, possible values are from 1 to {0}",
                                ushort.MaxValue));
                            return;
                        }
                        i++;
                        break;
                    case "/O":
                    case "/o":
                        if ((i + 2) > args.Length)
                        {
                            Console.WriteLine("Parameter /O value is undefined");
                            return;
                        }

                        outputFile = string.Format("{0}.html", args[i + 1]);
                        i++;
                        break;
                    default:
                        Console.WriteLine(string.Format("Invalid parameter {0}", args[i]));
                        return;
                }
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
                if (mode == Modes.BetaDivergence)
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
    }
}
