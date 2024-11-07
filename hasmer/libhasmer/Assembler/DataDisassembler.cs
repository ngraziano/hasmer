using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hasmer.Assembler {
    /// <summary>
    /// Used for diassembling the data section of a Hermes bytecode file.
    /// </summary>
    public  static class DataDisassembler {
 
        /// <summary>
        /// Writes an entire buffer (i.e. key/array/value/etc.) as disassembly.
        /// </summary>
        public static string Disassemble(HbcDataBuffer buffer, bool isVerbose) {
            var builder = new StringBuilder();
            foreach (var (offset, references) in buffer.References) {

                var (listPrefix, listelement) = buffer.GetOneSerie(offset);


                switch (listPrefix.TagType) {
                    case HbcDataBufferTagType.Null:
                    case HbcDataBufferTagType.True:
                    case HbcDataBufferTagType.False:
                        builder.Append($".data {references.Name} {listPrefix.TagType}[{listelement.Count}]");
                        break;
                    default:
                        string tagType = listPrefix.TagType switch {
                            HbcDataBufferTagType.ByteString or HbcDataBufferTagType.ShortString or HbcDataBufferTagType.LongString => "String",
                            _ => listPrefix.TagType.ToString()
                        };
                        builder.Append($".data {references.Name} {tagType}[{listelement.Count}] {{ {string.Join(", ", listelement.Select(l => l.ToAsmString()))} }}");
                        break;
                }
                if(isVerbose) {
                    builder.AppendLine($" // offset {offset} ");
                } else {
                    builder.AppendLine();
                }

                if(isVerbose) {
                    foreach (var reference in references.Refs) {
                        builder.Append(' ', 50);
                        builder.AppendLine($"// Ref: {reference}");
                    }
                }
            }

            return builder.ToString();
        }




    }
}
