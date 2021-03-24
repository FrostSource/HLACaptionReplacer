using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

#nullable enable

namespace HLACaptionReplacer
{
    class ClosedCaptions
    {
        public const uint MAGIC = 0x44434356; // "VCCD"
        public uint Version { get; set; } = 2;
        // How is block size determined?
        public const uint BlockSize = 8192;

        public List<ClosedCaption> Captions { get; private set; } = new List<ClosedCaption>();

        public void Add(ClosedCaption caption)
        {
            Add(caption.SoundEvent, caption.RawDefinition);
        }
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

        public void Insert(int index, ClosedCaption caption)
        {
            Insert(index, caption.SoundEvent, caption.RawDefinition);
        }
        public void Insert(int index, string sndevt, string definition)
        {
            Captions.Insert(index, new ClosedCaption()
            {
                SoundEvent = sndevt,
                Definition = definition
            });
        }
        public void Insert(int index, uint hash, string definition)
        {
            Captions.Insert(index, new ClosedCaption()
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
            writer.Write((uint) 0); // numblocks placeholder, come back to write this later
            writer.Write(BlockSize);
            writer.Write((uint)Captions.Count);
            // 16 is the number of bytes each caption header uses
            // 24 is bytes written above plus one below
            //472=valve extra padding bytes?
            // -16 for adesi
            //uint dataOffset = (uint)(24 + (472 - 16) + (Captions.Count * 16));
            uint dataOffset = (uint)(24 + (Captions.Count * 16));
            // 110592 hard coded offset??
            //uint dataOffset = 110592;
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

            var leftOver = BlockSize - runningOffset;
            if (leftOver > 0)
            {
                writer.Write(new byte[leftOver]);
            }

            // write number of blocks to header now that we know how many
            writer.BaseStream.Position = 8;
            uint numBlocks = (uint)(blockNum + 1);
            writer.Write(numBlocks);

            // for some reason caption files have 2180 bytes at the end
            //writer.Write(new byte[2180]);
            // This is Adesi code
            //writer.BaseStream.Position = dataOffset + (numBlocks * BlockSize) - (BlockSize / 256) + 31;
            //writer.Write(new byte());

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
