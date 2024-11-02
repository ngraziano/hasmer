using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hasmer.Decompiler.AST;

namespace Hasmer.Decompiler.Visitor {
    /// <summary>
    /// Visitors for instructions that load constant values.
    /// </summary>
    [VisitorCollection]
    public class LoadConstantOperations {
        /// <summary>
        /// Clears the specified register.
        /// </summary>
        [Visitor]
        public static void LoadConstEmpty(DecompilerContext context) {
            byte register = context.Instruction.Operands[0].GetValue<byte>();
            context.Block.WriteResult(register, new Identifier("undefined"));
        }

        /// <summary>
        /// Loads the JavaScript identifier "undefined" into the specified register.
        /// </summary>
        [Visitor]
        public static void LoadConstUndefined(DecompilerContext context) {
            byte register = context.Instruction.Operands[0].GetValue<byte>();
            context.Block.WriteResult(register, new Identifier("undefined"));
        }

        /// <summary>
        /// Loads the JavaScript literal null into the specified register.
        /// </summary>
        [Visitor]
        public static void LoadConstNull(DecompilerContext context) {
            byte register = context.Instruction.Operands[0].GetValue<byte>();
            context.Block.WriteResult(register, new Literal(new PrimitiveNullValue()));
        }

        /// <summary>
        /// Loads the JavaScript literal boolean `true` into the specified register.
        /// </summary>
        [Visitor]
        public static void LoadConstTrue(DecompilerContext context) {
            byte register = context.Instruction.Operands[0].GetValue<byte>();
            context.Block.WriteResult(register, new Literal(new PrimitiveBoolValue(true)));
        }

        /// <summary>
        /// Loads the JavaScript literal boolean `false` into the specified register.
        /// </summary>
        [Visitor]
        public static void LoadConstFalse(DecompilerContext context) {
            byte register = context.Instruction.Operands[0].GetValue<byte>();
            context.Block.WriteResult(register, new Literal(new PrimitiveBoolValue(false)));
        }

        /// <summary>
        /// Loads the JavaScript literal number 0 into the specified register.
        /// </summary>
        [Visitor]
        public static void LoadConstZero(DecompilerContext context) {
            byte register = context.Instruction.Operands[0].GetValue<byte>();
            context.Block.WriteResult(register, new Literal(new PrimitiveNumberValue(0)));
        }

        /// <summary>
        /// Loads a given constant string value into the specified register.
        /// </summary>
        [Visitor]
        public static void LoadConstString(DecompilerContext context) {
            byte register = context.Instruction.Operands[0].GetValue<byte>();
            context.Block.WriteResult(register, new Literal(new PrimitiveIdxStringValue(context.Instruction.Operands[1].GetValue<int>(), context.Source)));
        }

   
        /// <summary>
        /// Loads a constant unsigned byte into the specified register.
        /// </summary>
        [Visitor]
        public static void LoadConstUInt8(DecompilerContext context) {
            byte register = context.Instruction.Operands[0].GetValue<byte>();
            byte value = context.Instruction.Operands[1].GetValue<byte>();
            context.Block.WriteResult(register, new Literal(new PrimitiveIntegerValue(value)));
        }

        /// <summary>
        /// Loads a constant unsigned unsigned 4-byte integer into the specified register.
        /// </summary>
        [Visitor]
        public static void LoadConstInt(DecompilerContext context) {
            byte register = context.Instruction.Operands[0].GetValue<byte>();
            uint value = context.Instruction.Operands[1].GetValue<byte>();
            context.Block.WriteResult(register, new Literal(new PrimitiveIntegerValue(value)));
        }

        /// <summary>
        /// Loads a constant 8-byte IEE754 floating point number into the specified register.
        /// </summary>
        [Visitor]
        public static void LoadConstDouble(DecompilerContext context) {
            byte register = context.Instruction.Operands[0].GetValue<byte>();
            double value = context.Instruction.Operands[1].GetValue<byte>();
            context.Block.WriteResult(register, new Literal(new PrimitiveNumberValue(value)));
        }
    }
}
