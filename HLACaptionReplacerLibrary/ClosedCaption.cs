using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HLACaptionReplacer
{
    public class ClosedCaption
    {
        private string definition;
        private string soundEvent;

        /// <summary>
        /// Gets the length of the text definition.
        /// </summary>
        public ushort Length { get; private set; }
        /// <summary>
        /// Gets or sets the hash value of the sound event name.
        /// </summary>
        /// <remarks>
        /// Set this property ONLY if you do not know the sound event name.
        /// </remarks>
        public uint SoundEventHash { get; set; }
        /// <summary>
        /// Gets the hash value of the text definition.
        /// </summary>
        public uint DefinitionHash { get; private set; } = 0;
        /// <summary>
        /// Gets or sets the sound event name that this caption relates to.
        /// </summary>
        public string SoundEvent
        {
            get => soundEvent;
            set
            {
                soundEvent = value;
                // Why is this needed? Should throw exception if string is empty instead of allowing^?
                if (!string.IsNullOrEmpty(soundEvent))
                {
                    SoundEventHash = ValveResourceFormat.Crc32.Compute(Encoding.UTF8.GetBytes(soundEvent));
                }
            }
        }
        /// <summary>
        /// Gets or sets the text definition of the caption.
        /// </summary>
        public string Definition
        {
            get => definition;
            set
            {
                //TODO: Check if given string has a null terminator before adding one?
                definition = value + '\0';
                Length = (ushort)Encoding.Unicode.GetByteCount(definition);
                DefinitionHash = ValveResourceFormat.Crc32.Compute(Encoding.Unicode.GetBytes(value));
                RawDefinition = value;
            }
        }
        /// <summary>
        /// Gets the text definition of the caption without the null terminator.
        /// </summary>
        public string RawDefinition { get; private set; }

        public ClosedCaption():this("", "")
        {
        }

        // How do you solve this warning? The backing field is definitely being set.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public ClosedCaption(string sndevt, string definition)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            SoundEvent = sndevt;
            Definition = definition;
        }

        /// <summary>
        /// Gets a value indicating whether the text definition is empty.
        /// </summary>
        public bool IsBlank
        {
            get
            {
                return RawDefinition == "";
            }
        }
    }

    
}
