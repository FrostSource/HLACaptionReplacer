using System;
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
            var proc = System.Diagnostics.Process.Start(exePath, $"{captionPath}");


        }
    }
}
