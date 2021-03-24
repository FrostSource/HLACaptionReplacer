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

        public string FileName { get; private set; } = "";
        public string FilePath { get; private set; } = "";

        public int Read(string filename)
        {
            FileName = Path.GetFileNameWithoutExtension(filename);
            FilePath = Path.GetDirectoryName(filename);
            var sr = new StreamReader(filename);

            Rules = new List<CaptionModifierRule>();

            int invalids = 0;

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
                    invalids++;
                    continue;
                }

                var rule = new CaptionModifierRule();

                // If no text is provided then this is a deletion rule
                if (parsed.text == null)
                {
                    rule.ModificationType = ModificationType.Delete;
                }
                else
                {
                    rule.ModificationType = ModificationType.Replace;
                    rule.Caption.Definition = parsed.text;
                }

                // Assume a valid unsigned integer is a sound event hash
                if (uint.TryParse(parsed.sndevt, out uint hash))
                {
                    rule.Caption.SoundEventHash = hash;
                }
                else
                {
                    rule.Caption.SoundEvent = parsed.sndevt;
                }

                // Add new modifier
                Rules.Add(rule);

            }

            return invalids;
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

        public (int replaceCount, int deleteCount, int additionCount) ModifyCaptions(ClosedCaptions captions)
        {
            var additions = new List<CaptionModifierRule>();

            int replaceCount = 0;
            int deleteCount = 0;
            int additionCount = 0;
            foreach (var rule in Rules)
            {
                bool isAddition = true;
                foreach (var caption in captions.Captions)
                {
                    if (caption.SoundEventHash == rule.Caption.SoundEventHash)
                    {
                        // Deleting actually means nullifying the string until we figure out
                        // why changing blocks messes up captions
                        if (rule.ModificationType == ModificationType.Delete)
                        {
                            if (!caption.IsBlank)
                            {
                                // What's the best way to erase a caption file but keep the hash there?
                                // can we just remove the caption entirely?
                                caption.Definition = "";
                                //caption.Definition = new string('b', (caption.Length - 2) / 2);
                                //Console.WriteLine(Encoding.Unicode.GetBytes(new string('\0', (caption.Length - 2))).Length);
                                //caption.Definition = "\0";
                                deleteCount++;
                            }
                        }
                        else
                        {
                            caption.Definition = rule.Caption.Definition;
                            replaceCount++;
                        }

                        isAddition = false;
                    }
                }

                if (isAddition)
                {
                    additions.Add(rule);
                    additionCount++;
                }
            }

            // Add the additions after the loops have ended
            foreach (var addition in additions)
            {
                captions.Insert(0, addition.Caption);
            }

            return (replaceCount, deleteCount, additionCount);
        }

        public void AddAllToClosedCaptions(ClosedCaptions captions)
        {
            foreach (var rule in Rules)
            {
                captions.Add(rule.Caption);
            }
        }
    }
}
