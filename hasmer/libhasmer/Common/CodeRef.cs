using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hasmer.Common {
    public readonly record struct CodeRef(uint FunctionId, uint Offset) {

        public override string ToString() => $"FCT{FunctionId}:{Offset}";
    }
}
