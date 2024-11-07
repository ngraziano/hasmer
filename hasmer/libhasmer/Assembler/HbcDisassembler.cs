using Hasmer.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hasmer.Assembler {
    /// <summary>
    /// Represents a Hermes bytecode disassembler.
    /// </summary>
    public class HbcDisassembler {
        /// <summary>
        /// The source bytecode file for disassembly.
        /// </summary>
        public HbcFile Source { get; }


        /// <summary>
        /// The specified options to be used for the processing of Hermes bytecode and output of Hasm assembly.
        /// </summary>
        public DisassemblerOptions Options { get; }

        /// <summary>
        /// Created a new disassembler given a bytecode file.
        /// </summary>
        /// <param name="source">The bytecode file to disassemble.</param>
        public HbcDisassembler(HbcFile source, DisassemblerOptions options) {
            Source = source;
            Options = options;
        }

        /// <summary>
        /// Disassembles the bytecode file to Hasm disassembly.
        /// </summary>
        public string Disassemble() {
            StringBuilder builder = new StringBuilder();
            builder.Append(".hasm ");
            builder.Append(Source.Header.Version.ToString());
            if (Options.IsExact) {
                builder.AppendLine(" exact");
            } else {
                builder.AppendLine(" auto");
            }

            Console.WriteLine("Check references... ");
            foreach (var funcHeader in Source.SmallFuncHeaders) {
                var header = funcHeader.GetAssemblerHeader();
                foreach (var instr in header.Disassemble()) {
                    // TODO see if we can make better
                    switch (Source.BytecodeFormat.Definitions[instr.Opcode].Name) {
                        case "NewArrayWithBuffer":
                        case "NewArrayWithBufferLong":
                            if (instr.Operands[3].Value is PrimitiveIntegerValue offset 
                                && instr.Operands[2].Value is PrimitiveIntegerValue size) {
                                Source.ArrayBuffer.AddRef(offset.GetIntegerValue(), size.GetIntegerValue(),new CodeRef(funcHeader.FunctionId,instr.Offset));
                            }
                            break;
                    }
                }
            }

            Console.WriteLine("Disassembling data... ");
            

            builder.AppendLine();
            var dataDisassembler = new DataDisassembler(Source, Source.ArrayBuffer, 'A');
            builder.AppendLine(dataDisassembler.Disassemble());
            builder.AppendLine();

            Console.WriteLine("Disassembling functions... ");
            using (ConsoleProgressBar progress = new ConsoleProgressBar()) {
                for (int i = 0; i < Source.SmallFuncHeaders.Length; i++) {
                    progress.Report(i / (double)Source.SmallFuncHeaders.Length);

                    HbcSmallFuncHeader func = Source.SmallFuncHeaders[i];
                    FunctionDisassembler decompiler = new FunctionDisassembler(this, func.GetAssemblerHeader());
                    builder.AppendLine(decompiler.Disassemble());
                    builder.AppendLine();
                }
            }

            Console.WriteLine("done!");

            return builder.ToString();
        }
    }
}
