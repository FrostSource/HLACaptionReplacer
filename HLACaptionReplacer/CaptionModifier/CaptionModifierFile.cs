using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ValveResourceFormat.ClosedCaptions;

#nullable enable

//TODO: Proper exceptions.
//TODO: Method summaries/documentation.

namespace HLACaptionReplacer
{
    class CaptionModifierFile
    {
        public List<CaptionModifierRule> Rules { get; private set; } = null!;
        public int DeletionCount { get; private set; }
        public int ReplacementCount { get; private set; }

        public void Read(string filename)
        {
            var sr = new StreamReader(filename);

            Rules = new List<CaptionModifierRule>();
            DeletionCount = 0;
            ReplacementCount = 0;

            int lineCount = 0;
            string? line;
            (string? sndevt, string? text) parsed;
            while ((line = sr.ReadLine()) != null)
            {
                lineCount++;

                // Ignore comment and empty lines
                if (line == string.Empty || line.TrimStart().StartsWith("//")) continue;

                // Parse the current line with whitespace trimmed
                if ((parsed = ParseString(line.Trim())).sndevt == null)
                {
                    Console.WriteLine($"Modification line {lineCount} is invalid!");
                    continue;
                }

                ModificationType type;
                if (parsed.text == null)
                {
                    type = ModificationType.Delete;
                    DeletionCount++;
                }
                else
                {
                    type = ModificationType.Replace;
                    ReplacementCount++;
                }

                // Add new modifier
                Rules.Add(new CaptionModifierRule()
                {
                    Hash = ClosedCaptions.ComputeHash(parsed.sndevt),
                    Text = parsed.text,
                    ModificationType = type
                });
            }
        }

        public (string? sndevt, string? text) ParseString(string input)
        {
            //var match = Regex.Match(input, @"^(\d+)(?:[ \t]*\""(.*)\"")?$", RegexOptions.Compiled);
            //return match.Groups
            //    .Cast<Group>()
            //    .Skip(1)
            //    .Select(m => m.Value)
            //    .ToList();
            var match = Regex.Match(input, @"^(\S+)(?:[ \t]+(.+))?$", RegexOptions.Compiled);
            var groups = match.Groups;
            var sndevt = groups[1].Success ? groups[1].Value : null;
            var text = groups[2].Success ? groups[2].Value : null;

            return (sndevt, text);
        }

        public (int replaceCount, int deleteCount) ModifyCaptions(ClosedCaptions captions)
        {
            int replaceCount = 0;
            int deleteCount = 0;
            foreach (var caption in captions.Captions)
            {
                foreach (var rule in Rules)
                {
                    if (caption.SoundEventHash == rule.Hash)
                    {
                        // Deleting actually means nullifying the string until we figure out
                        // why changing blocks messes up captions
                        if (rule.ModificationType == ModificationType.Delete)
                        {
                            if (!caption.IsBlank)
                            {
                                caption.Definition = new string('b', (caption.Length - 2) / 2);
                                //Console.WriteLine(Encoding.Unicode.GetBytes(new string('\0', (caption.Length - 2))).Length);
                                //caption.Definition = "\0";
                                deleteCount++;
                            }
                        }
                        else
                        {
                            caption.Definition = rule.Text;
                            replaceCount++;
                        }
                    }
                }
            }

            return (replaceCount, deleteCount);
        }
    }
}
