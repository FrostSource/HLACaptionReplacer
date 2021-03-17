﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

#nullable enable

namespace HLACaptionReplacer
{
    class ClosedCaptions
    {
        public const uint MAGIC = 0x44434356; // "VCCD"
        public const uint Version = 2;
        // How is block size determined?
        public const uint BlockSize = 8192;

        public List<ClosedCaption> Captions { get; private set; } = new List<ClosedCaption>();

        public void Add(string sndevt, string definition)
        {
            Captions.Add(new ClosedCaption()
            {
                SoundEvent = sndevt,
                Definition = definition
            });
        }
        public void Add(uint hash, string definition)
        {
            Captions.Add(new ClosedCaption()
            {
                SoundEventHash = hash,
                Definition = definition
            });
        }

        public void Write(string filename)
        {
            var fs = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
            var writer = new BinaryWriter(fs);

            writer.Write(MAGIC);
            writer.Write(Version);
            // numblocks placeholder, come back to write this later
            writer.Write(new byte[4]);
            writer.Write(BlockSize);
            writer.Write((uint)Captions.Count);
            // 16 is the number of bytes each caption header uses
            // 24 is bytes written above plus one below
            //472=valve extra padding bytes?
            uint dataOffset = (uint)(24 + 472 + (Captions.Count * 16));
            writer.Write(dataOffset);

            // Header information for each caption
            // Caption definitions are written after
            int blockNum = 0;
            ushort runningOffset = 0;
            int padAmount = 0;
            var prevPos = writer.BaseStream.Position;
            foreach (var caption in Captions)
            {
                // Move the head back to the caption header
                writer.BaseStream.Position = prevPos;

                writer.Write(caption.SoundEventHash);
                
                if (Version >= 2)
                {
                    // what does this integer mean?
                    // is it needed for captions to work?

                    // This turned out to be crc32 of the definition in unicode
                    writer.Write(caption.DefinitionHash);
                }

                // If out of space for this block, move to new block
                if (runningOffset + caption.Length >= BlockSize)
                {
                    // Prepare the amount to be padded for the end of the block
                    if (runningOffset < BlockSize)
                        padAmount = (int)(BlockSize - runningOffset);
    
                    blockNum++;
                    runningOffset = 0;
                }

                writer.Write(blockNum);
                writer.Write(runningOffset);
                writer.Write(caption.Length);

                // Save the position in the caption header so we can go back later
                prevPos = writer.BaseStream.Position;
                // does position scrubbing give terrible performance?
                writer.BaseStream.Position = dataOffset + (blockNum * BlockSize) + runningOffset;
                writer.Write(Encoding.Unicode.GetBytes(caption.Definition));
                if (padAmount > 0)
                {
                    writer.Write(new byte[padAmount]);
                    padAmount = 0;
                }

                runningOffset += caption.Length;
            }

            // for some reason caption files have 2180 bytes at the end
            writer.Write(new byte[2180]);

            // write number of blocks to header now that we know how many
            writer.BaseStream.Position = 8;
            writer.Write(blockNum + 1);

            writer.Close();
            fs.Close();
        }

        public ClosedCaption? LastCaption
        {
            get
            {
                if (Captions.Count == 0) return null;

                return Captions[^1];
            }
        }

        public ClosedCaption? FindByHash(uint hash)
        {
            foreach (var caption in Captions)
            {
                if (caption.SoundEventHash == hash)
                    return caption;
            }

            return null;
        }

        public static uint ComputeHash(string input)
        {
            return ValveResourceFormat.Crc32.Compute(Encoding.UTF8.GetBytes(input));
        }
    }
}
