using System;
using System.IO;
using ValveResourceFormat.ClosedCaptions;

namespace HLACaptionReplacer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello Caption Replacers!!");

            var file = "C:\\Users\\jjhfr\\OneDrive\\HLACaptionReplacer\\closecaption_english.dat";
            var fileNew = "C:\\Users\\jjhfr\\OneDrive\\HLACaptionReplacer\\closecaption_english_new.dat";

            var captions = new ClosedCaptions();
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
            PrintClosedCaptionData(captionsNew);
        }

        static void PrintClosedCaptionData(ClosedCaptions captions)
        {
            Console.WriteLine($"Version: {captions.Version}");
            Console.WriteLine($"NumBlocks: {captions.NumBlocks}");
            Console.WriteLine($"BlockSize: {captions.BlockSize}");
            Console.WriteLine($"DirectorySize: {captions.DirectorySize}");
            Console.WriteLine($"DataOffset: {captions.DataOffset}");
            Console.WriteLine($"Total num captions: {captions.Captions.Count}");
        }
    }
}
