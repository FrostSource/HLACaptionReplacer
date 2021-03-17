using System;
using System.IO;
using System.Text;
using ValveResourceFormat.ClosedCaptions;

namespace HLACaptionReplacer
{
    public enum InputMode
    {
        Custom,
        Replace
    }
    class Program
    {
        public static bool PauseOnCompletion { get; set; } = false;
        public static InputMode InputMode { get; set; } = InputMode.Replace;

        static void Main(string[] args)
        {
            //Console.WriteLine("Hello Caption Replacers!!");


            //Console.WriteLine($"Current dir: {Directory.GetCurrentDirectory()}");
            //Console.WriteLine($"Num args: {args.Length}");

            // Needs a proper parsing library if released
            ParseArgs(args);

            /*var vCaptions = new ValveResourceFormat.ClosedCaptions.ClosedCaptions();
            vCaptions.Read(args[0]);

            var nCaptions = new ClosedCaptions();
            foreach(var caption in vCaptions.Captions)
            {
                //nCaptions.Add(caption.Hash, caption.Text.Replace("clr:255,190,255", "clr:255,100,100"));
                //nCaptions.Add(caption.Hash, caption.Text.Replace("clr:255,190,255", "clr:255,0,0"));
                //if (caption.Text == "Map") Console.WriteLine(nCaptions.GetLastCaption().Length);
                nCaptions.Add(caption.Hash, caption.Text);
            }
            var rep = nCaptions.FindByHash(nCaptions.ComputeHash("vo.01_13035"));
            rep.Definition = "<clr:255,100,160>Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam";
            rep.SoundEvent = "TestCC.Alyx";

            if (File.Exists(args[1])) File.Delete(args[1]);
            nCaptions.Write(args[1]);*/


            if (PauseOnCompletion)
            {
                Console.WriteLine("\nPress any key to exit the process...");
                Console.ReadKey();
            }

            /*var captions = new ClosedCaptions();
            captions.Read(file);
            Console.WriteLine($"Loaded file {Path.GetFileName(file)} ...");
            PrintClosedCaptionData(captions);
            //captions.Captions.RemoveRange(0,6851);
            captions.Captions.ForEach(x => { x.Text += " TEST."; });
            captions.Write(fileNew);

            Console.WriteLine("\nWritten...\n");

            var captionsNew = new ClosedCaptions();
            captionsNew.Read(fileNew);
            Console.WriteLine($"Loaded file {Path.GetFileName(fileNew)} ...");
            PrintClosedCaptionData(captionsNew);*/
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
                    Console.Write("[Custom input file]");
                }
                else if (arg == "-r")
                {
                    InputMode = InputMode.Replace;
                    Console.Write("[Replacement input file]");
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
            var modifier = new CaptionModifierFile();

            modifier.Read(modifyFile);
            Console.WriteLine($"Modifier file has {modifier.DeletionCount} deletions and {modifier.ReplacementCount} replacements.\n");
            var compiledCaptions = new ValveResourceFormat.ClosedCaptions.ClosedCaptions();
            compiledCaptions.Read(captionFile);

            var captionCompiler = new ClosedCaptions();
            foreach (var caption in compiledCaptions)
            {
                captionCompiler.Add(caption.Hash, caption.Text);
            }

            var count = modifier.ModifyCaptions(captionCompiler);
            Console.WriteLine($"\nMade {count.deleteCount}/{modifier.DeletionCount} deletions and {count.replaceCount}/{modifier.ReplacementCount} replacements changes.");

            var outputPath = Path.Combine(Path.GetDirectoryName(captionFile), $"{Path.GetFileNameWithoutExtension(captionFile)}_new{Path.GetExtension(captionFile)}");
            compiledCaptions.Write(outputPath);
            Console.WriteLine("\nWrote new caption file:");
            Console.WriteLine(outputPath);
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
                "-c     Custom file input\n" +
                "-r     Replacement / removal file input\n" +
                "-p     Wait for input after finishing\n";
            Console.WriteLine(help);
        }


    }

    
}
