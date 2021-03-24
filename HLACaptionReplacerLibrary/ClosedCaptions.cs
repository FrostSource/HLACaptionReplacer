using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ValveResourceFormat;

#nullable enable

namespace HLACaptionReplacer
{
    public class ClosedCaptions
    {
        private const uint MAGIC = 0x44434356; // "VCCD"
        private const uint Version = 2;
        // How is block size determined?
        //public const uint BlockSize = 8192;
        public uint BlockSize { get; set; } = 8192;  //TODO: BlockSize currently temporarily settable to test different values.  Will be made const once confident of standard size of 8192.

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
        const uint paddingBytes = 472;
        const int endPaddingBytes = 2180;
        const uint headerSize = 16;
        const uint baseHeader = 24;
        public void Read(byte[] bytes)
        {
            
            using (var ms = new MemoryStream(bytes))
            {
                Read(ms);
               
            }
        }
        public void Read(Stream stream)
        {
            using (var binReader = new BinaryReader(stream))
            {
                //Check that magic number is here.
                if (MAGIC != binReader.ReadUInt32())  //4
                {
                    throw new InvalidFileFormatException("Invalid caption file (not VCCD).");
                }
                if (Version != binReader.ReadUInt32())  //8 
                {
                    throw new InvalidFileFormatException("Only VCCD version 2 is supported.");
                }
                uint blockCount = binReader.ReadUInt32();  //12
                BlockSize = binReader.ReadUInt32(); //16
                uint numberOfCaptions = binReader.ReadUInt32(); //20  (match to DirectorySize in VRF)

                uint dataOffset = binReader.ReadUInt32();
                if (dataOffset != (baseHeader + paddingBytes + (numberOfCaptions * headerSize)))
                {
                    //throw new InvalidFileFormatException("Data offset is invalid.");  //Apparently this calculation is wrong.  So, for now we will ignore it.
                }
                for (uint i = 0; i< numberOfCaptions; i++)
                {
                    var caption = new ClosedCaption();
                    caption.SoundEventHash = binReader.ReadUInt32();
                    caption.DefinitionHash = binReader.ReadUInt32();
                    caption.Blocknum = binReader.ReadUInt32();
                    caption.Offset = binReader.ReadUInt16();
                    caption.ReadLength = binReader.ReadUInt16();
                    Captions.Add(caption);
                }
                foreach (var caption in Captions)
                {
                    binReader.BaseStream.Position = dataOffset + (caption.Blocknum * BlockSize) + caption.Offset;
                    var captionText = binReader.ReadNullTermString(Encoding.Unicode);

                    //possibility of validating definitionHash against hash of captiontext here.

                    caption.Definition = captionText;
                }

            }
        }
        public byte[] GetBytes()
        {
            byte[] retVal;
            using (MemoryStream ms = new MemoryStream())
            {
                Write(ms);
                retVal = ms.ToArray();
            }
            return retVal;
        }
        //Naming convention: closecaption_*.dat
        // store location: /game/hlvr_addons/<addon>/resource/subtitles
        //Original Write left for now.

        public void Write(Stream stream)
        {
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(MAGIC);
                writer.Write(Version);
                // numblocks placeholder, come back to write this later
                writer.Write((uint)0);
                writer.Write(BlockSize);
                writer.Write((uint)Captions.Count);
                // 16 is the number of bytes each caption header uses
                // 24 is bytes written above plus one below
                //472=valve extra padding bytes?
                //uint dataOffset = (uint)(baseHeader + paddingBytes + (Captions.Count * headerSize));
                uint dataOffset = (uint)(baseHeader + (Captions.Count * headerSize));
                writer.Write(dataOffset);

                // Header information for each caption
                // Caption definitions are written after
                uint blockNum = 0;
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
                    caption.Blocknum = blockNum;
                    caption.Offset = runningOffset;

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


            }
        }
        public void Write(string filename)
        {
            using (var fs = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite))
            {
                Write(fs);
            }
        }

      
        public ClosedCaption this[string key]
        {
            get
            {
                var hash = Crc32.Compute(Encoding.UTF8.GetBytes(key));
#pragma warning disable CS8603 // Possible null reference return.
                return Captions.Find(caption => caption.SoundEventHash == hash);
#pragma warning restore CS8603 // Possible null reference return.
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

    }
}
