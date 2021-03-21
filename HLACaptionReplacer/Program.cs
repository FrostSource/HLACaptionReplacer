using System;
using System.IO;
using System.Text;
using ValveResourceFormat.ClosedCaptions;

#nullable enable

namespace HLACaptionReplacer
{
    public enum InputMode
    {
        Custom,
        Replace
    }

    class Program
    {
        // Currently defaults to true while testing
        public static bool PauseOnCompletion { get; set; } = true;
        public static InputMode InputMode { get; set; } = InputMode.Replace;

        static void Main(string[] args)
        {
            //Console.WriteLine("Hello Caption Replacers!!");


            //Console.WriteLine($"Current dir: {Directory.GetCurrentDirectory()}");
            //Console.WriteLine($"Num args: {args.Length}");

            // Needs a proper parsing library if released
            ParseArgs(args);


            if (PauseOnCompletion)
            {
                Console.WriteLine("\nPress any key to exit the process...");
                Console.ReadKey();
            }
        }

        static bool ParseArgs(string[] args)
        {
            /*
                -c  Custom file input
                -r  Replacement/removal file input

                -p  Wait for input after finishing
            */

            if (args.Length == 0)
            {
                PrintHelp();
                PauseOnCompletion = true;
                return false;
            }

            string captionFile = "";
            string modifyFile = "";

            foreach (var arg in args)
            {
                if (arg == "-c")
                {
                    InputMode = InputMode.Custom;
                }
                else if (arg == "-r")
                {
                    InputMode = InputMode.Replace;
                }
                else if (arg == "-p")
                {
                    PauseOnCompletion = true;
                    Console.Write("[Pause on completion]");
                }
                else if (Path.GetExtension(arg) == ".dat")
                {
                    captionFile = arg;
                }
                else if (Path.GetExtension(arg) == ".txt")
                {
                    modifyFile = arg;
                }

                else
                {
                    Console.Write($"\n!! Unknown argument {arg}");
                    return false;
                }
            }

            if (captionFile == "" || modifyFile == "")
            {
                Console.WriteLine("\n!! Must provide both a caption file (.dat) and modifier file (.txt)");
                return false;
            }

            if (!File.Exists(captionFile))
            {
                Console.WriteLine("\n!! Caption file provided does not exist.");
                Console.WriteLine(captionFile);
                return false;
            }
            if (!File.Exists(modifyFile))
            {
                Console.WriteLine("\n!! Modifier file provided does not exist.");
                Console.WriteLine(modifyFile);
                return false;
            }

            Console.WriteLine();

            switch (InputMode)
            {
                case InputMode.Custom:
                    Console.WriteLine("Custom caption compiler not currently working, sorry!");
                    break;

                case InputMode.Replace:
                    Console.WriteLine("[Caption Replacement Mode]");
                    ReplaceCaptions(captionFile, modifyFile);
                    break;

                default:
                    Console.WriteLine($"Somehow you've entered an unknown input mode! ({InputMode})");
                    return false;
            }

            return true;
        }

        public static void ReplaceCaptions(string captionFile, string modifyFile)
        {
            try
            {
                var compiledCaptions = new ValveResourceFormat.ClosedCaptions.ClosedCaptions();
                compiledCaptions.Read(captionFile);

                var captionCompiler = new ClosedCaptions();
                foreach (var caption in compiledCaptions)
                {
                    captionCompiler.Add(caption.Hash, caption.Text);
                }
                Console.WriteLine($"Successfully read {captionCompiler.Captions.Count} compiled captions.\n");

                var modifier = new CaptionModifierFile();
                int invalids = modifier.Read(modifyFile);

                Console.WriteLine($"Modifier file has {modifier.Rules.Count} rule(s).");
                if (modifier.Rules.Count == 0) return;
                var result = modifier.ModifyCaptions(captionCompiler);
                Console.WriteLine($"Made the following modifications:\n" +
                    $"{result.deleteCount} deletions.\n" +
                    $"{result.replaceCount} replacements.\n" +
                    $"{result.additionCount} additions.");

#pragma warning disable CS8604 // Possible null reference argument.
                var outputPath = Path.Combine(Path.GetDirectoryName(captionFile), $"{Path.GetFileNameWithoutExtension(captionFile)}_new{Path.GetExtension(captionFile)}");
#pragma warning restore CS8604 // Possible null reference argument.
                if (File.Exists(outputPath)) File.Delete(outputPath);
                captionCompiler.Write(outputPath);
                Console.WriteLine("Wrote new caption file:");
                Console.WriteLine(outputPath);
            }
            catch (IOException e)
            {
                Console.WriteLine(e);
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine(e);
            }


        }

        static public void PrintClosedCaptionData(ValveResourceFormat.ClosedCaptions.ClosedCaptions captions)
        {
            Console.WriteLine("Closed Caption Data:");
            Console.WriteLine($"Version: {captions.Version}");
            Console.WriteLine($"NumBlocks: {captions.NumBlocks}");
            Console.WriteLine($"BlockSize: {captions.BlockSize}");
            Console.WriteLine($"DirectorySize: {captions.DirectorySize}");
            Console.WriteLine($"DataOffset: {captions.DataOffset}");
            Console.WriteLine($"Total num captions: {captions.Captions.Count}");
        }

        public static void PrintHelp()
        {
            var help = "Command line arguments must contain an input file and a caption file.\n" +
                ".txt is recognized as an input file (either a custom language file or modifier file).\n" +
                ".dat is recognized as a compiled caption file (e.g. closecaption_english.dat).\n" +
                "\n" +
                "Flags:\n" +
                "-c     Custom file input mode\n" +
                "-r     Replacement / removal file input mode [default mode when unspecified]\n" +
                "-p     Wait for input after finishing\n";
            Console.WriteLine(help);
        }


    }

    
}
