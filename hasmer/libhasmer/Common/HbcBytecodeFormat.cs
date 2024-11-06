﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;

namespace Hasmer {
    /// <summary>
    /// Represents a definition of the bytecode operations for a given Hermes version.
    /// <br />
    /// This is generally used by deserializing a JSON object,
    /// specifically the "Bytecode*.json" file in the Resources directory that corresponds to the desired Hermes version.
    /// <br />
    /// The "Bytecode*.json" themselves are autogenerated using the "bytecode-format-generator" tool,
    /// which is located in a directory at the root of this git repository.
    /// </summary>
    public class HbcBytecodeFormat {
        /// <summary>
        /// The Hermes bytecode version this format is relevant to.
        /// </summary>
        [JsonProperty]
        public uint Version { get; set; }

        /// <summary>
        /// The definitions of all opcodes available for the bytecode version.
        /// This list can be indexed by the encoded value of the opcode.
        /// </summary>
        [JsonProperty]
        public required List<HbcInstructionDefinition> Definitions { get; set; }

        /// <summary>
        /// The definitions of all abstractions for instructions that can take different length operands.
        /// See <see cref="HbcAbstractInstructionDefinition"/> for more information.
        /// </summary>
        public required List<HbcAbstractInstructionDefinition> AbstractDefinitions { get; set; }
    }
}
