using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hasmer.Assembler {
    /// <summary>
    /// Used for diassembling the data section of a Hermes bytecode file.
    /// </summary>
    public class DataDisassembler {
        /// <summary>
        /// The Hermes bytecode file being disassembled.
        /// </summary>
        public HbcFile Source { get; set; }

        /// <summary>
        /// The decoded Array Buffer.
        /// </summary>
        public List<HbcDataBufferItems> ArrayBuffer { get; set; }

        /// <summary>
        /// The decoded Object Key Buffer.
        /// </summary>
        public List<HbcDataBufferItems> KeyBuffer { get; set; }

        /// <summary>
        /// The decoded Object Value Buffer.
        /// </summary>
        public List<HbcDataBufferItems> ValueBuffer { get; set; }

        /// <summary>
        /// Creates a new DataDisassembler for a given Hermes bytecode file.
        /// </summary>
        public DataDisassembler(HbcFile source) {
            Source = source;
        }

        /// <summary>
        /// Returns an array containing *length* items starting at buffer offset *offset* in the given buffer.
        /// If *length* extends over multiple entries in the array buffer (i.e. multiple data declarations),
        /// the elements from all entries are returned in order.
        /// This enables reading over multiple entries at once.
        /// </summary>
        public static List<PrimitiveValue> GetElementSeries(List<HbcDataBufferItems> buffer, uint offset, int length) {
            var series = new List<PrimitiveValue>(length);
            var idx = buffer.FindIndex(item => item.Offset == offset);
            if (idx < 0) throw new IndexOutOfRangeException("Offset invalid");

            for (int i = 0; i < length; i++) {
                series.Add(buffer[idx + i].Items);
            }
            return series;
        }

        /// <summary>
        /// Writes an entire buffer (i.e. key/array/value/etc.) as disassembly.
        /// </summary>
        private void AppendDisassembly(StringBuilder builder, List<HbcDataBufferItems> buffer, char prefix) {
            for (int i = 0; i < buffer.Count; i++) {
                HbcDataBufferItems items = buffer[i];
                switch (items.TagType) {
                    case HbcDataBufferTagType.Null:
                    case HbcDataBufferTagType.True:
                    case HbcDataBufferTagType.False:
                        builder.AppendLine($".data {prefix}{i} Off:{items.Offset} {items.TagType}[]");
                        continue;
                    default:
                        break;
                }
                string mapped;
                if (items.Items is PrimitiveIdxStringValue) {
                    mapped = StringEscape.Escape(items.Items.ToString());
                } else {
                    mapped = items.Items.ToString();
                }
                string tagType = items.TagType switch {
                    HbcDataBufferTagType.ByteString or HbcDataBufferTagType.ShortString or HbcDataBufferTagType.LongString => "String",
                    _ => items.TagType.ToString()
                };
                builder.AppendLine($".data {prefix}{i} Off:{items.Offset} {tagType}[] {mapped}");
            }
            if (buffer.Count > 0) {
                builder.AppendLine();
            }
        }

        public void DisassembleData() {
            Console.WriteLine("Parsing array buffer...");
            ArrayBuffer = Source.ArrayBuffer.ReadAll(Source);
            Console.WriteLine("Parsing object key buffer...");
            KeyBuffer = Source.ObjectKeyBuffer.ReadAll(Source);
            Console.WriteLine("Parsing object value buffer...");
            ValueBuffer = Source.ObjectValueBuffer.ReadAll(Source);
        }

        /// <summary>
        /// Disassembles the Hermes bytecode data buffers and returns a string representing the disassembly.
        /// </summary>
        public string Disassemble() {
            StringBuilder builder = new StringBuilder();

            DisassembleData();
            AppendDisassembly(builder, ArrayBuffer, 'A');
            AppendDisassembly(builder, KeyBuffer, 'K');
            AppendDisassembly(builder, ValueBuffer, 'V');

            return builder.ToString();
        }
    }
}
