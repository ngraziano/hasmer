using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hasmer.Decompiler.AST {
    public class CallExpression : SyntaxNode {
        public required SyntaxNode Callee { get; set; }
        public List<SyntaxNode> Arguments { get; set; } = [];
        public bool IsCalleeConstructor { get; set; }

        public override void WriteDirect(SourceCodeBuilder builder) {
            if (IsCalleeConstructor) {
                builder.Write("new ");
            }
            Callee.Write(builder);
            builder.Write("(");
            for (int i = 0; i < Arguments.Count; i++) {
                Arguments[i].Write(builder);
                if (i < Arguments.Count - 1) {
                    builder.Write(", ");
                }
            }
            builder.Write(")");
        }

        public override string ToString() {
            return $"{Callee}()";
        }
    }
}
