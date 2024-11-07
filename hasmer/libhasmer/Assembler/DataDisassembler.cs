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
        private readonly HbcDataBuffer buffer;
        private readonly char prefix;

        /// <summary>
        /// The Hermes bytecode file being disassembled.
        /// </summary>
        public HbcFile Source { get; set; }

        /// <summary>
        /// Creates a new DataDisassembler for a given Hermes bytecode file.
        /// </summary>
        public DataDisassembler(HbcFile source, HbcDataBuffer arrayBuffer, char prefix) {
            Source = source;

            this.buffer = arrayBuffer;
            this.prefix = prefix;
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
        public string Disassemble() {
            StringBuilder builder = new StringBuilder();
            int i = 0;
            foreach (var(offset,references) in buffer.References) {

                var (listPrefix, listelement) = buffer.GetOneSerie(offset);


                switch (listPrefix.TagType) {
                    case HbcDataBufferTagType.Null:
                    case HbcDataBufferTagType.True:
                    case HbcDataBufferTagType.False:
                        builder.AppendLine($".data {references.Name} Off:{offset} {listPrefix.TagType}[{listelement.Count}]");
                        continue;
                    default:
                        break;
                }
                string tagType = listPrefix.TagType switch {
                    HbcDataBufferTagType.ByteString or HbcDataBufferTagType.ShortString or HbcDataBufferTagType.LongString => "String",
                    _ => listPrefix.TagType.ToString()
                };
                builder.AppendLine($".data {references.Name} Off:{offset} {tagType}[{listelement.Count}] {{ {string.Join(", ", listelement.Select(l => l.ToAsmString()))} }}");
                i++;
            }

            return builder.ToString();
        }




    }
}
