using System;
using System.IO;
using System.Text;

#nullable enable

namespace HLACaptionCompiler
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
            //ParseArgs(args);

            var str = "basic.tag \"enclosed string with, whspace.  \" 1005 5.6";
            var parser = new Parser.GenericParser(str);
            parser.BoundaryChars = "{}";

            Console.WriteLine("Word: " + parser.NextWord());
            Console.WriteLine($"Enclosed: \"{parser.NextEnclosed()}\"");
            Console.WriteLine("Integer: "+ parser.NextInteger());
            Console.WriteLine("Decimal: "+ parser.NextDecimal());


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
            
            string captionPath = "";
            string modifyPath = "";

            foreach (var arg in args)
            {
                if (arg == "-p")
                {
                    PauseOnCompletion = true;
                    Console.Write("[Pause on completion]");
                }
                else if (Path.GetExtension(arg) == ".dat")
                {
                    captionPath = arg;
                }
                else if (Path.GetExtension(arg) == ".txt")
                {
                    modifyPath = arg;
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

            var modifyFile = new FileInfo(modifyPath);
            var captionFile = new FileInfo(captionPath);

            if (modifyFile == null && captionFile == null)
            {
                PrintHelp();
                PauseOnCompletion = true;
                return false;
            }

            // Loading the modifier file
            if (modifyFile == null)
            {
                Console.WriteLine("\n!! Modifier file provided does not exist.");
                Console.WriteLine(modifyPath);
                return false;
            }
            var modifier = new CaptionModifierFile();
            int invalids = modifier.Read(modifyPath);
            if (invalids > 0)
                Console.WriteLine($"Modifier file has {modifier.Rules.Count} rule(s) and {invalids} invalid lines.");
            else
                Console.WriteLine($"Modifier file has {modifier.Rules.Count} rule(s).");

            if (modifier.Rules.Count == 0) return false;

            Console.WriteLine("\n");

            // New custom caption file mode
            if (captionPath == "")
            {
                Console.WriteLine("[Custom Caption Mode]");
                CustomCaptionFile(modifier);
                return true;
            }

            // Replacing and adding captions to a compiled file
            if (captionFile == null)
            {
                Console.WriteLine("!! Caption file provided does not exist.");
                Console.WriteLine(captionPath);
                return false;
            }

            Console.WriteLine("[Caption Replacement Mode]");
            ReplaceCaptions(modifier, captionFile);

            return true;
        }

        public static void ReplaceCaptions(CaptionModifierFile modifier, FileInfo captionFile)
        {
            var captions = new ClosedCaptions();
            using (var stream = captionFile.OpenRead())
            {
                captions.Read(stream);
            }
            Console.WriteLine($"Successfully read {captions.Count} compiled captions.\n");
            
            var result = modifier.ModifyCaptions(captions);
            Console.WriteLine($"Made the following modifications:\n" +
                $"{result.deleteCount} deletions.\n" +
                $"{result.replaceCount} replacements.\n" +
                $"{result.additionCount} additions.");

#pragma warning disable CS8604 // Possible null reference argument.
            //var outputPath = Path.Combine(Path.GetDirectoryName(captionFile), $"{Path.GetFileNameWithoutExtension(captionFile)}_new{Path.GetExtension(captionFile)}");
            var outputPath = Path.Combine(Directory.GetCurrentDirectory(), $"closecaption_{modifier.FileName}.dat");
#pragma warning restore CS8604 // Possible null reference argument.

            WriteCaptionFile(captions, outputPath);
        }

        public static void CustomCaptionFile(CaptionModifierFile modifier)
        {
            var customCaptions = new ClosedCaptions();
            modifier.AddAllToClosedCaptions(customCaptions);

            Console.WriteLine($"Added {modifier.Rules.Count} captions.\n");

            var outputPath = Path.Combine(Directory.GetCurrentDirectory(), $"closecaption_{modifier.FileName}.dat");
            
            WriteCaptionFile(customCaptions, outputPath);
        }

        public static void CloneCaptionFile(FileInfo captionFile)
        {
            var captions = new ClosedCaptions();
            using (var stream = captionFile.OpenRead())
            {
                captions.Read(stream);
            }

#pragma warning disable CS8604 // Possible null reference argument.
            var outputPath = Path.Combine(captionFile.DirectoryName, $"{captionFile.Name}_new{captionFile.Extension}");
#pragma warning restore CS8604 // Possible null reference argument.

            WriteCaptionFile(captions, outputPath);
        }

        public static void WriteCaptionFile(ClosedCaptions captions, string filename)
        {
            // Need a better way to clear the file
            if (File.Exists(filename)) File.Delete(filename);

            captions.Write(filename);
            Console.WriteLine("Wrote new caption file:");
            Console.WriteLine(Path.GetFullPath(filename));
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
