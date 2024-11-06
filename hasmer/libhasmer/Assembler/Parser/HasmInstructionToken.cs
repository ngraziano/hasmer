using System.Collections.Generic;

namespace Hasmer.Assembler.Parser {
    /// <summary>
    /// Represents a Hasm instruction and its operands.
    /// </summary>
    public class HasmInstructionToken : HasmToken {
        /// <summary>
        /// The name of the instruction.
        /// </summary>
        public required string Instruction { get; set; }

        /// <summary>
        /// The operands passed to the instruction.
        /// </summary>
        public required List<HasmOperandToken> Operands { get; set; }

        public HasmInstructionToken(HasmStringStreamState state) : base(state) { }
    }
}
