using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Tests
{
    class Tests
    {
        static void Main(string[] args)
        {

            // Why did I do it like this?
            var exePath = Path.GetFullPath(@"../../../../HLACaptionReplacer/bin/Debug/net5.0/HLACaptionReplacer.exe");
            var captionPath = Path.GetFullPath(@"../../../Files/closecaption_english.dat");
            var captionPathReplica = Path.GetFullPath(@"../../../Files/closecaption_replica.dat");
            var captionPathNew = Path.Combine(Path.GetDirectoryName(captionPath), $"{Path.GetFileNameWithoutExtension(captionPath)}_new{Path.GetExtension(captionPath)}");
            var customCaptionPath = Path.GetFullPath(@"../../../Files/closecaption_custom.dat");
            //var captionPathNew = string.Format("{0}{1}_new{2}", Path.GetDirectoryName(captionPath),
            //                                                    Path.GetFileNameWithoutExtension(captionPath),
            //                                                    Path.GetExtension(captionPath));
            var modifierPath = Path.GetFullPath(@"../../../Files/closecaption_own_modifier.txt");

            // should be 45 deletions
            //var proc = System.Diagnostics.Process.Start(exePath, $"-e {captionPath} {modifierPath}");
            //var proc = System.Diagnostics.Process.Start(exePath, $"{captionPath}");

            TestClosedCaptionFileParser();


        }

        public static void TestClosedCaptionFileParser()
        {
            var str1 = "lang\n{\n                Language \"English\" //(or \"French\", etc)\n\n    Tokens\n\n    {\n                    // Captions defined here.\n                    nameofsound \"This is the caption.\"\n\n\n        barn.chatter    \"We're picking up radio chatter. They're looking for your car.\"\n        // etc...\n                }\n            }";
            var str2 = "\"lang\"\n" +
                "{ \n" +
                "\"Language\" \"English\" \n" +
                "\"Tokens\" \n" +
                "{\n" +
                "	\"levi.health01\"		\"Take this medkit.\"\n" +
                "	\"levi.health02\"		\"Here, have a medkit.\"\n" +
                "	\"levi.health03\"     \"Here, path yourself up.\"\n" +
                "    \"levi.health04\"     \"Heal up.\"\n" +
                "    \"levi.health05\"     \"Have a medkit.\"\n" +
                "    \"caption1\"      \"caption 1\"\n" +
                "    \"caption2\"      \"caption 2\"\n" +
                "    \"nothingtodo\"		\"Just never be seen.\"\n" +
                "    \"captain.captain_01\"    \"<clr:222,67,0>Hello! My name is Captain Dickhead and I want to tell you about custom captions.\"\n" +
                "    \"captain.captain_02\"    \"<clr:222,67,0>You can have cool colors and other markup I guess.\"\n" +
                "    \"padding1\"      \"padding 1\"\n" +
                "    \"padding2\"      \"padding 2\"\n" +
                "\n" +
                "}\n" +
                "}\n";
            var strUsed = str1;
            Console.WriteLine(strUsed);
            var parser = new HLACaptionCompiler.Parser.ClosedCaptionFileParser(strUsed);

            var result = parser.Parse();

            Console.WriteLine(result["Language"]);
            foreach (KeyValuePair<string, string> token in result["Tokens"])
            {
                Console.WriteLine($"{token.Key}\t{token.Value}");
            }
        }
    }
}
