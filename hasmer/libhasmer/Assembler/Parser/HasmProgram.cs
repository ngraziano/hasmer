using System.Collections.Generic;

namespace Hasmer.Assembler.Parser {
    public class HasmProgram {
        public required HasmHeader Header { get; set; }
        public required List<HasmDataDeclaration> Data { get; set; }
    }
}
