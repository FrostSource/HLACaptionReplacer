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
        // Currently defaults to true while testing.
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
                if (arg == "-p")
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

            /*if (true)
            {
                AddTestToAll(captionFile);
                return false;
            }*/

            if (modifyFile == "" && captionFile == "")
            {
                PrintHelp();
                PauseOnCompletion = true;
                return false;
            }

            // Cloning compiled captions and displaying
            if (modifyFile == "")
            {
                if (!File.Exists(captionFile))
                {
                    Console.WriteLine("\n!! Caption file provided does not exist.");
                    Console.WriteLine(captionFile);
                    return false;
                }
                CloneCaptionFile(captionFile);
                PauseOnCompletion = true;
                return true;
            }

            // Loading the modifier file
            if (!File.Exists(modifyFile))
            {
                Console.WriteLine("\n!! Modifier file provided does not exist.");
                Console.WriteLine(modifyFile);
                return false;
            }
            var modifier = new CaptionModifierFile();
            int invalids = modifier.Read(modifyFile);
            if (invalids > 0)
                Console.WriteLine($"Modifier file has {modifier.Rules.Count} rule(s) and {invalids} invalid lines.");
            else
                Console.WriteLine($"Modifier file has {modifier.Rules.Count} rule(s).");

            if (modifier.Rules.Count == 0) return false;

            Console.WriteLine("\n");

            // New custom caption file mode
            if (captionFile == "")
            {
                Console.WriteLine("[Custom Caption Mode]");
                CustomCaptionFile(modifier);
                return true;
            }

            // Replacing and adding captions to a compiled file
            if (!File.Exists(captionFile))
            {
                Console.WriteLine("!! Caption file provided does not exist.");
                Console.WriteLine(captionFile);
                return false;
            }

            Console.WriteLine("[Caption Replacement Mode]");
            ReplaceCaptions(modifier, captionFile);

            return true;
        }

        public static void ReplaceCaptions(CaptionModifierFile modifier, string captionFile)
        {
            var compiledCaptions = new ValveResourceFormat.ClosedCaptions.ClosedCaptions();
            compiledCaptions.Read(captionFile);

            var captionCompiler = new ClosedCaptions();
            captionCompiler.Version = compiledCaptions.Version;
            foreach (var caption in compiledCaptions)
            {
                captionCompiler.Add(caption.Hash, caption.Text);
            }
            Console.WriteLine($"Successfully read {captionCompiler.Captions.Count} compiled captions.\n");
            
            var result = modifier.ModifyCaptions(captionCompiler);
            Console.WriteLine($"Made the following modifications:\n" +
                $"{result.deleteCount} deletions.\n" +
                $"{result.replaceCount} replacements.\n" +
                $"{result.additionCount} additions.");

#pragma warning disable CS8604 // Possible null reference argument.
            //var outputPath = Path.Combine(Path.GetDirectoryName(captionFile), $"{Path.GetFileNameWithoutExtension(captionFile)}_new{Path.GetExtension(captionFile)}");
            var outputPath = Path.Combine(Directory.GetCurrentDirectory(), $"closecaption_{modifier.FileName}.dat");
#pragma warning restore CS8604 // Possible null reference argument.

            WriteCaptionFile(captionCompiler, outputPath);
        }

        public static void CustomCaptionFile(CaptionModifierFile modifier)
        {
            var customCaptions = new ClosedCaptions();
            modifier.AddAllToClosedCaptions(customCaptions);

            Console.WriteLine($"Added {modifier.Rules.Count} captions.\n");

            var outputPath = Path.Combine(Directory.GetCurrentDirectory(), $"closecaption_{modifier.FileName}.dat");
            
            WriteCaptionFile(customCaptions, outputPath);
        }

        public static void WriteCaptionFile(ClosedCaptions captions, string filename)
        {
            // Need a better way to clear the file
            if (File.Exists(filename)) File.Delete(filename);

            captions.Write(filename);
            Console.WriteLine("Wrote new caption file:");
            Console.WriteLine(Path.GetFullPath(filename));
        }

        public static void CloneCaptionFile(string captionFile)
        {
            var compiledCaptions = new ValveResourceFormat.ClosedCaptions.ClosedCaptions();
            compiledCaptions.Read(captionFile);

            PrintClosedCaptionData(compiledCaptions);

            var captionCompiler = new ClosedCaptions();
            captionCompiler.Version = compiledCaptions.Version;
            foreach (var caption in compiledCaptions)
            {
                captionCompiler.Add(caption.Hash, caption.Text);
            }

#pragma warning disable CS8604 // Possible null reference argument.
            var outputPath = Path.Combine(Path.GetDirectoryName(captionFile), $"{Path.GetFileNameWithoutExtension(captionFile)}_new{Path.GetExtension(captionFile)}");
#pragma warning restore CS8604 // Possible null reference argument.

            WriteCaptionFile(captionCompiler, outputPath);
        }

        public static void AddTestToAll(string captionFile)
        {
            Console.WriteLine("Adding \" TEST\" to all");
            var compiledCaptions = new ValveResourceFormat.ClosedCaptions.ClosedCaptions();
            compiledCaptions.Read(captionFile);

            var captionCompiler = new ClosedCaptions();
            foreach (var caption in compiledCaptions)
            {
                captionCompiler.Add(caption.Hash, caption.Text + " TEST");
            }

#pragma warning disable CS8604 // Possible null reference argument.
            var outputPath = Path.Combine(Path.GetDirectoryName(captionFile), $"{Path.GetFileNameWithoutExtension(captionFile)}_new{Path.GetExtension(captionFile)}");
#pragma warning restore CS8604 // Possible null reference argument.

            WriteCaptionFile(captionCompiler, outputPath);
        }

        public static void PrintClosedCaptionData(ValveResourceFormat.ClosedCaptions.ClosedCaptions captions)
        {
            Console.WriteLine("Closed Caption Data:");
            Console.WriteLine($"\tVersion:\t{captions.Version}");
            Console.WriteLine($"\tNumBlocks:\t{captions.NumBlocks}");
            Console.WriteLine($"\tBlockSize:\t{captions.BlockSize}");
            Console.WriteLine($"\tDirectorySize:\t{captions.DirectorySize}");
            Console.WriteLine($"\tDataOffset:\t{captions.DataOffset}");
            Console.WriteLine($"\tTotal num captions:\t{captions.Captions.Count}");
        }

        public static void PrintHelp()
        {
            var help = "Command line arguments must contain an input file and a caption file.\n" +
                ".txt is recognized as an input file (either a custom language file or modifier file).\n" +
                ".dat is recognized as a compiled caption file (e.g. closecaption_english.dat).\n" +
                "\n" +
                "Flags:\n" +
                "-p     Wait for input after finishing\n";
            Console.WriteLine(help);
        }


    }

    
}
