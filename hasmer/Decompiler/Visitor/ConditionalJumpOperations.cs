﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hasmer.Decompiler.AST;

namespace Hasmer.Decompiler.Visitor {
    /// <summary>
    /// Visits instructions that perform jump based on given citeria.
    /// </summary>
    [VisitorCollection]
    public class ConditionalJumpOperations {
        /// <summary>
        /// Decompiles a block contained within a conditional statement (e.g. JNotEqual, JLess, etc), given the boundary instruction offsets.
        /// The instruction offsets passed are the actual binary offsets, not the index in the instructions list.
        /// <br />
        /// The start parameter is exclusive, but the end parameter is inclusive.
        /// <br />
        /// Returns a new BlockStatement containing the decompiled instructions.
        /// </summary>
        private static BlockStatement DecompileConditionalBlock(DecompilerContext context, uint start, uint end) {
            BlockStatement block = new BlockStatement();

            context.Block = block;
            context.Instructions = context.Instructions.Where(x => x.Offset > start && x.Offset <= end).ToList();
            context.CurrentInstructionIndex = 0;

            while (context.CurrentInstructionIndex < context.Instructions.Count) {
                FunctionDecompiler.ObserveInstruction(context, context.CurrentInstructionIndex);
            }

            return block;
        }

        /// <summary>
        /// Decompiles a conditional jump (e.g. JNotEqual),
        /// given the JavaScript operator that will be used in the <see cref="IfStatement"/> that will be used in the decompilation.
        /// <br />
        /// For example, for a JNotEqual instruction, the operator is "==",
        /// since the if statement is executed if the expression is equal,
        /// and the jump is performed if the expression is not equal.
        /// </summary>
        private static void ConditionalJump(DecompilerContext context, ISyntax expr) {
            int jump = context.Instruction.Operands[0].GetValue<int>();

            IfStatement block = new IfStatement {
                Test = expr
            };

            if (jump < 0) {
                throw new NotImplementedException();
            } else if (jump > 0) {
                DecompilerContext consequentContext = context.DeepCopy();

                uint start = context.Instruction.Offset;
                uint to = (uint)((int)context.Instruction.Offset - (int)context.Instruction.Length + jump);

                int toIndex = context.Instructions.FindIndex(insn => insn.Offset == to);
                HbcInstruction endInstruction = context.Instructions[toIndex];
                string endInstructionName = context.Source.BytecodeFormat.Definitions[endInstruction.Opcode].Name;
                if (endInstructionName == "Jmp") { // unconditional jump means else statement
                    block.Consequent = DecompileConditionalBlock(consequentContext, start, to - endInstruction.Length); // remove the Jmp from the consequent block

                    DecompilerContext alternateContext = context.DeepCopy();
                    jump = endInstruction.Operands[0].GetValue<int>();
                    start = endInstruction.Offset;
                    to = (uint)((int)endInstruction.Offset - (int)endInstruction.Length + jump);
                    block.Alternate = DecompileConditionalBlock(alternateContext, start, to);
                    toIndex = context.Instructions.FindIndex(insn => insn.Offset == to);

                    FunctionDecompiler.WriteRemainingRegisters(alternateContext);
                } else {
                    block.Consequent = DecompileConditionalBlock(consequentContext, start, to);
                }

                FunctionDecompiler.WriteRemainingRegisters(consequentContext);
                context.CurrentInstructionIndex = toIndex + 1;
            }

            context.Block.Body.Add(block);
        }

        private static void JumpBinaryExpression(DecompilerContext context, string op) =>
            ConditionalJump(context, new BinaryExpression {
                Left = context.State.Registers[context.Instruction.Operands[1].GetValue<byte>()],
                Right = context.State.Registers[context.Instruction.Operands[2].GetValue<byte>()],
                Operator = op
            });

        [Visitor]
        public static void JEqual(DecompilerContext context) => JumpBinaryExpression(context, "==");

        [Visitor]
        public static void JNotEqual(DecompilerContext context) => JumpBinaryExpression(context, "!=");

        [Visitor]
        public static void JStrictEqual(DecompilerContext context) => JumpBinaryExpression(context, "===");

        [Visitor]
        public static void JStrictNotEqual(DecompilerContext context) => JumpBinaryExpression(context, "!==");

        [Visitor]
        public static void JmpTrue(DecompilerContext context) {
            ConditionalJump(context, new UnaryExpression {
                Operator = "!",
                Argument = context.State.Registers[context.Instruction.Operands[1].GetValue<byte>()]
            });
        }
    }
}