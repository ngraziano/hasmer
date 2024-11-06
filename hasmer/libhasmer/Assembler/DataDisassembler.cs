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
            var idx = buffer.FindIndex(item => item.Offset == offset);
            if (idx < 0) {
                Console.WriteLine($"WARN :Offset {offset} not found in buffer.");
                return [];
                //throw new IndexOutOfRangeException("Offset invalid");
            }

            return buffer.Skip(idx).Take(length).Select(i => i.Item).ToList();
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
                string tagType = items.TagType switch {
                    HbcDataBufferTagType.ByteString or HbcDataBufferTagType.ShortString or HbcDataBufferTagType.LongString => "String",
                    _ => items.TagType.ToString()
                };
                builder.AppendLine($".data {prefix}{i} Off:{items.Offset} {tagType}[] {items.Item.ToAsmString()}");
            }
            if (buffer.Count > 0) {
                builder.AppendLine();
            }
        }



        /// <summary>
        /// Disassembles the Hermes bytecode data buffers and returns a string representing the disassembly.
        /// </summary>
        public string Disassemble() {
            StringBuilder builder = new StringBuilder();

         
            //AppendDisassembly(builder, ArrayBuffer, 'A');
            //AppendDisassembly(builder, KeyBuffer, 'K');
            //AppendDisassembly(builder, ValueBuffer, 'V');

            return builder.ToString();
        }
    }
}
