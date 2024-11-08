﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Hasmer.Common;

namespace Hasmer {
    /// <summary>
    /// Represents the type of a series of data in the DataBuffer.
    /// </summary>
    public enum HbcDataBufferTagType {
        Null = 0,
        True = 1 << 4,
        False = 2 << 4,
        Number = 3 << 4,
        LongString = 4 << 4,
        ShortString = 5 << 4,
        ByteString = 6 << 4,
        Integer = 7 << 4
    }

    /// <summary>
    /// Represents the header of an array in the data buffer.
    /// </summary>
    public class HbcDataBufferPrefix {
        /// <summary>
        /// The amount of values represented by the array.
        /// </summary>
        public uint Length { get; set; }
        /// <summary>
        /// The type of the data in the data buffer.
        /// </summary>
        public HbcDataBufferTagType TagType { get; set; }

        public override string ToString() {
            return $"{TagType} x {Length}";
        }
    }

    /// <summary>
    /// Represents an entry in the data buffer (data type and subsequent items).
    /// </summary>
    public record HbcDataBufferItems {
        public required HbcDataBufferTagType TagType { get; set; }
        /// <summary>
        /// The items for the data buffer entry.
        /// </summary>
        public required PrimitiveValue Item { get; set; }
        /// <summary>
        /// The offset of the entry (from the start of the header) relative to the start of the entire buffer.
        /// </summary>
        public required uint Offset { get; set; }
    }

    public record NameAndRefs {
        public required string Name { get; init; }
        public List<CodeRef> Refs { get; } = [];
    }

    /// <summary>
    /// Represents a Hermes data buffer, such as the array buffer.
    /// </summary>
    public class HbcDataBuffer {
        private readonly HbcFile source;

        /// <summary>
        /// The raw binary of the buffer.
        /// </summary>
        private readonly byte[] buffer;

        private char namePrefix;

        /// <summary>
        /// Reference to array element in the buffer.
        /// </summary>
        public Dictionary<long, NameAndRefs> References { get; } = [];


        /// <summary>
        /// Creates a new HbcDataBuffer given the raw binary data in the buffer.
        /// </summary>
        public HbcDataBuffer(byte[] buffer, HbcFile source, char namePrefix) {
            this.buffer = buffer;
            this.source = source;
            this.namePrefix = namePrefix;
        }

        /// <summary>
        /// Writes the serialized buffer to a stream.
        /// </summary>
        public void WriteAll(BinaryWriter writer) {
            writer.Write(buffer);
        }

        /// <summary>
        /// Reads a single PrimitiveValue from a stream given the type of the value.
        /// </summary>
        private PrimitiveValue ReadValue(HbcFile source, HbcDataBufferTagType tagType, BinaryReader reader) {
            // new PrimitiveValue made for each switch to preserve the PrimitiveValue type tagging mechanism for numbers
            return tagType switch {
                HbcDataBufferTagType.ByteString => new PrimitiveIdxStringValue(reader.ReadByte(), source),
                HbcDataBufferTagType.ShortString => new PrimitiveIdxStringValue(reader.ReadUInt16(), source),
                HbcDataBufferTagType.LongString => new PrimitiveIdxStringValue((int)reader.ReadUInt32(), source),
                HbcDataBufferTagType.Number => new PrimitiveNumberValue(reader.ReadDouble()),
                HbcDataBufferTagType.Integer => new PrimitiveIntegerValue(reader.ReadInt32()),
                HbcDataBufferTagType.Null => new PrimitiveNullValue(),
                HbcDataBufferTagType.True => new PrimitiveBoolValue(true),
                HbcDataBufferTagType.False => new PrimitiveBoolValue(false),
                _ => throw new InvalidOperationException()
            };
        }

        /// <summary>
        /// Reads the tag type (and length) for an entry in the data buffer. All subsequent items will have that type.
        /// </summary>
        private static HbcDataBufferPrefix ReadTagType(BinaryReader reader) {
            const byte TAG_MASK = 0x70;

            // if the length of the data is longer than 0x0F, an additional length byte is written
            byte keyTag = reader.ReadByte();
            if ((keyTag & 0x80) == 0x80) {
                return new HbcDataBufferPrefix {
                    TagType = (HbcDataBufferTagType)(keyTag & TAG_MASK),
                    Length = (uint)(keyTag & 0x0F) << 8 | reader.ReadByte()
                };
            }
            return new HbcDataBufferPrefix {
                TagType = (HbcDataBufferTagType)(keyTag & TAG_MASK),
                Length = (uint)(keyTag & 0x0F)
            };
        }


        public List<PrimitiveValue> GetElementSerie(long arrayBufferOffset, long arrayBufferLengh) {
            using var ms = new MemoryStream(buffer);
            using var reader = new BinaryReader(ms);
            ms.Position = arrayBufferOffset;

            var result = new List<PrimitiveValue>();

            // List can be on multiple array entry
            while (result.Count < arrayBufferLengh) {
                var prefix = ReadTagType(reader);
                for (int i = 0; result.Count < arrayBufferLengh && i < prefix.Length && ms.Position < ms.Length; i++) {
                    var value = ReadValue(source, prefix.TagType, reader);
                    result.Add(value);

                }
            }

            return result;
        }

        public (HbcDataBufferPrefix, List<PrimitiveValue>) GetOneSerie(long arrayBufferOffset) {
            using var ms = new MemoryStream(buffer);
            using var reader = new BinaryReader(ms);
            ms.Position = arrayBufferOffset;

            var result = new List<PrimitiveValue>();
            var prefix = ReadTagType(reader);
            for (int i = 0; i < prefix.Length && ms.Position < ms.Length; i++) {
                var value = ReadValue(source, prefix.TagType, reader);
                result.Add(value);
            }

            return (prefix, result);
        }

        public void AddRef(long arrayBufferOffset, long arrayBufferLengh, CodeRef codeRef) {
            using var ms = new MemoryStream(buffer);
            using var reader = new BinaryReader(ms);
            ms.Position = arrayBufferOffset;

            long nbElem = 0;
            while (nbElem < arrayBufferLengh) {
                var offset = ms.Position;
                var prefix = ReadTagType(reader);
                var nbWantedElement = Math.Min(arrayBufferLengh - nbElem, prefix.Length);
                if (References.TryGetValue(offset, out var elem)) {
                    elem.Refs.Add(codeRef);
                } else {
                    References[offset] = new() {
                        Name = $"{namePrefix}{References.Count}",
                        Refs = { codeRef },
                    };
                }

                nbElem += prefix.Length;
                // skip data if need more element
                if (nbElem < arrayBufferLengh) {
                    for (int i = 0; i < prefix.Length; i++) {
                        ReadValue(source, prefix.TagType, reader);
                    }
                }
            }
        }
    }
}
