using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ValveResourceFormat.ClosedCaptions
{
    public class ClosedCaptions : IEnumerable<ClosedCaption>
    {
        public const int MAGIC = 0x44434356; // "VCCD"

        public List<ClosedCaption> Captions { get; private set; }

        public UInt32 Version { get; private set; }
        public UInt32 NumBlocks { get; private set; }
        public UInt32 BlockSize { get; private set; }
        public UInt32 DirectorySize { get; private set; }
        public UInt32 DataOffset { get; private set; }

        public IEnumerator<ClosedCaption> GetEnumerator()
        {
            return ((IEnumerable<ClosedCaption>)Captions).GetEnumerator();
        }

        public ClosedCaption this[string key]
        {
            get
            {
                var hash = Crc32.Compute(Encoding.UTF8.GetBytes(key));
                return Captions.Find(caption => caption.Hash == hash);
            }
        }

        /// <summary>
        /// Opens and reads the given filename.
        /// The file is held open until the object is disposed.
        /// </summary>
        /// <param name="filename">The file to open and read.</param>
        public void Read(string filename)
        {
            var fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            Read(filename, fs);
        }

        /// <summary>
        /// Reads the given <see cref="Stream"/>.
        /// </summary>
        /// <param name="filename">The filename <see cref="string"/>.</param>
        /// <param name="input">The input <see cref="Stream"/> to read from.</param>
        public void Read(string filename, Stream input)
        {
            if (!filename.StartsWith("subtitles_"))
            {
                // TODO: Throw warning?
            }

            var reader = new BinaryReader(input);
            Captions = new List<ClosedCaption>();

            if (reader.ReadUInt32() != MAGIC)
            {
                throw new InvalidDataException("Given file is not a VCCD.");
            }

            Version = reader.ReadUInt32();

            if (Version != 1 && Version != 2)
            {
                throw new InvalidDataException("Unsupported VCCD version: " + Version);
            }

            // numblocks, not actually required for hash lookups or populating entire list
            NumBlocks = reader.ReadUInt32();
            BlockSize = reader.ReadUInt32();
            DirectorySize = reader.ReadUInt32();
            DataOffset = reader.ReadUInt32();

            for (uint i = 0; i < DirectorySize; i++)
            {
                var caption = new ClosedCaption();
                caption.Hash = reader.ReadUInt32();

                if (Version >= 2)
                {
                    caption.UnknownV2 = reader.ReadUInt32();
                }

                caption.Blocknum = reader.ReadInt32();
                caption.Offset = reader.ReadUInt16();
                caption.Length = reader.ReadUInt16();

                Captions.Add(caption);
            }

            // Probably could be inside the for loop above, but I'm unsure what the performance costs are of moving the position head manually a bunch compared to reading sequentually
            foreach (var caption in Captions)
            {
                reader.BaseStream.Position = DataOffset + (caption.Blocknum * BlockSize) + caption.Offset;
                caption.Text = reader.ReadNullTermString(Encoding.Unicode);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<ClosedCaption>)Captions).GetEnumerator();
        }
    }
}
