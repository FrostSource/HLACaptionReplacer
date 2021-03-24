using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ValveResourceFormat;

#nullable enable

namespace HLACaptionReplacer
{
    public class ClosedCaptions : IEnumerable<ClosedCaption>
    {
        private const uint MAGIC = 0x44434356; // "VCCD"
        private const uint Version = 2;

        public uint BlockSize { get; set; } = 8192;  //TODO: BlockSize currently temporarily settable to test different values.  Will be made const once confident of standard size of 8192.

        //TODO: Should this property be private and force to use as enumerator?
        public List<ClosedCaption> Captions { get; private set; } = new List<ClosedCaption>();
        /// <summary>
        /// Gets the number of captions in this object.
        /// </summary>
        public int Count { get => Captions.Count; }

        public IEnumerator<ClosedCaption> GetEnumerator()
        {
            return ((IEnumerable<ClosedCaption>)Captions).GetEnumerator();
        }
        public ClosedCaption this[string key]
        {
            get
            {
                var hash = Crc32.Compute(Encoding.UTF8.GetBytes(key));
                return Captions.Find(caption => caption.SoundEventHash == hash);
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

        /// <summary>
        /// Adds an existing <see cref="ClosedCaption"/> to the object.
        /// </summary>
        /// <param name="caption">the <see cref="ClosedCaption"/> to add.</param>
        public void Add(ClosedCaption caption)
        {
            Add(caption.SoundEvent, caption.RawDefinition);
        }

        /// <summary>
        /// Adds a new caption for a sound event with a text definition.
        /// </summary>
        /// <param name="sndevt"></param>
        /// <param name="definition"></param>
        public void Add(string sndevt, string definition)
        {
            Captions.Add(new ClosedCaption()
            {
                SoundEvent = sndevt,
                Definition = definition
            });
        }

        /// <summary>
        /// Adds a new caption for a hashed sound event with a text definition.
        /// Useful for overriding a known caption without knowing the sound event name.
        /// </summary>
        /// <param name="hash"></param>
        /// <param name="definition"></param>
        public void Add(uint hash, string definition)
        {
            Captions.Add(new ClosedCaption()
            {
                SoundEventHash = hash,
                Definition = definition
            });
        }

        /// <summary>
        /// Reads a compiled caption file's data into this object.
        /// </summary>
        /// <param name="bytes"></param>
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

                // Check that magic number is here.
                if (MAGIC != binReader.ReadUInt32())  //4
                {
                    throw new InvalidFileFormatException("Invalid caption file (not VCCD).");
                }
                // Make sure the version is correct for Source 2.
                if (Version != binReader.ReadUInt32())  //8 
                {
                    throw new InvalidFileFormatException("Only VCCD version 2 is supported.");
                }

                // Block count isn't needed to read the file and is calculated on write.
                binReader.ReadUInt32();  //12
                // We don't need to use the block size found in the file but we can match if we want to.
                BlockSize = binReader.ReadUInt32(); //16
                uint numberOfCaptions = binReader.ReadUInt32(); //20  (match to DirectorySize in VRF)

                uint dataOffset = binReader.ReadUInt32();
                // This is not needed - data offset is different for every file.
                //if (dataOffset != (baseHeader + paddingBytes + (numberOfCaptions * headerSize)))
                //{
                //throw new InvalidFileFormatException("Data offset is invalid.");  //Apparently this calculation is wrong.  So, for now we will ignore it.
                //}

                long prevPos = binReader.BaseStream.Position;
                for (uint i = 0; i< numberOfCaptions; i++)
                {
                    // Move the head back to the caption header
                    binReader.BaseStream.Position = prevPos;

                    var caption = new ClosedCaption();
                    caption.SoundEventHash = binReader.ReadUInt32();
                    // These values don't need to be saved outside this scope unless we want to display the values,
                    // they are calculated on write.
                    binReader.ReadUInt32(); // definition hash
                    uint blockNum = binReader.ReadUInt32(); // block num
                    ushort offset = binReader.ReadUInt16(); // offset
                    binReader.ReadUInt16(); // length

                    // Save the position in the caption data so we can go back later.
                    prevPos = binReader.BaseStream.Position;

                    binReader.BaseStream.Position = dataOffset + (blockNum * BlockSize) + offset;
                    var captionText = binReader.ReadNullTermString(Encoding.Unicode);

                    //possibility of validating definitionHash against hash of captiontext here.

                    caption.Definition = captionText;
                    Captions.Add(caption);
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
                // numblocks placeholder, come back to write this later.
                writer.Write((uint)0);
                writer.Write(BlockSize);
                writer.Write((uint)Captions.Count);
                uint headerBytes = 24;
                uint bytesPerCaption = 16;
                uint dataOffset = (uint)Captions.Count * bytesPerCaption + headerBytes;
                writer.Write(dataOffset);

                // Writing information for each caption.
                // Caption definitions are written after.
                uint blockNum = 0;
                ushort runningOffset = 0;
                long prevPos = writer.BaseStream.Position;
                foreach (var caption in Captions)
                {
                    // Move the writer head back to the caption data.
                    writer.BaseStream.Position = prevPos;

                    writer.Write(caption.SoundEventHash);

                    //TODO: Do we need to check for version if we're only writing alyx captions?
                    if (Version >= 2)
                    {
                        writer.Write(caption.DefinitionHash);
                    }

                    // If out of space for this block, move to new block.
                    if (runningOffset + caption.Length >= BlockSize)
                    {
                        blockNum++;
                        runningOffset = 0;
                    }

                    writer.Write(blockNum);
                    writer.Write(runningOffset);
                    writer.Write(caption.Length);

                    // Save the position in the caption data so we can go back later.
                    prevPos = writer.BaseStream.Position;

                    //TODO: Does position scrubbing give terrible performance?
                    writer.BaseStream.Position = dataOffset + (blockNum * BlockSize) + runningOffset;
                    writer.Write(Encoding.Unicode.GetBytes(caption.Definition));

                    //NOTE: Padding wasn't needed because it's actually handled above by just skipping over the bytes!

                    runningOffset += caption.Length;
                }

                // Add left over bytes in the last block.
                //TODO: Is this strictly needed for valve to process the file?
                var leftOver = BlockSize - runningOffset;
                if (leftOver > 0)
                {
                    writer.Write(new byte[leftOver]);
                }

                // Write number of blocks to header now that we know how many.
                writer.BaseStream.Position = 8;
                uint numBlocks = blockNum + 1;
                writer.Write(numBlocks);
            }
        }
        public void Write(string filename)
        {
            using (var fs = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite))
            {
                Write(fs);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<ClosedCaption>)Captions).GetEnumerator();
        }

    }
}
