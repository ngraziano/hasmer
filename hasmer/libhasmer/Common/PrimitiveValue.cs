using System;
using System.Globalization;

namespace Hasmer {
    /// <summary>
    /// A wrapper type for any other given type, which keeps track of the original type that was passed.
    /// This is used to ensure that primitive types can maintain their original type when cast from object to their type.
    /// By just using object, all primitive values are coerced into a double, which is undesirable.
    /// </summary>
    public abstract class PrimitiveValue {
        /// <summary>
        /// The raw value, stored as an object. You probably don't want to use this, use GetValue() instead.
        /// </summary>
        public abstract object RawValue { get;}

        /// <summary>
        /// Returns the raw value coerced to a ulong.
        /// The the raw value is not an integer type, an exception is thrown.
        /// </summary>
        public abstract ulong GetIntegerValue();

        /// <summary>
        /// Returns the raw value coerced to type T.
        /// It is the duty of the caller to ensure that the type actually is of type T before calling.
        /// </summary>
        public abstract T GetValue<T>();

        public abstract string ToAsmString();
    }

    public class PrimitiveIdxStringValue : PrimitiveValue {
        private int idx;
        private readonly HbcFile source;

        public PrimitiveIdxStringValue(int idx, HbcFile source) {
            this.idx = idx;
            this.source = source;
        }

        public override object RawValue => source.GetStringTableEntry(idx).Value;

        public override ulong GetIntegerValue() {
            throw new Exception("cannot get integer value of non-integer PrimitiveValue");
        }

        // a revoir
        public override T GetValue<T>() => (T)(object)source.GetStringTableEntry(idx).Value;
        public override int GetHashCode() {
            return idx.GetHashCode();
        }

        public override bool Equals(object obj) {
            if (obj is PrimitiveIdxStringValue primitive) {
                return idx.Equals(primitive.idx);
            }
            return source.GetStringTableEntry(idx).Value.Equals(obj);
        }

        public override string ToString() => (string)RawValue;

        public override string ToAsmString() => StringEscape.Escape(source.GetStringTableEntry(idx).Value);
    }

    public class PrimitiveNumberValue : PrimitiveValue {
        private double typedValue;


        public PrimitiveNumberValue(double value) {
            typedValue = value;
        }

        public override object RawValue => typedValue;

        public override ulong GetIntegerValue() {
            throw new Exception("cannot get integer value of non-integer PrimitiveValue");
        }

        public override T GetValue<T>() => (T)Convert.ChangeType(typedValue, typeof(T));

        public override string ToString() => StringEscape.DoubleToString(typedValue);

        public override int GetHashCode() {
            return typedValue.GetHashCode();
        }

        public override bool Equals(object obj) {
            if (obj is PrimitiveNumberValue primitive) {
                return typedValue.Equals(primitive.typedValue);
            }
            return typedValue.Equals(obj);
        }

        public override string ToAsmString() => ToString();
    }

    public class PrimitiveIntegerValue : PrimitiveValue {
        private long typedValue;
        public PrimitiveIntegerValue(long value) {
            typedValue = value;
        }

        public override object RawValue => typedValue;

        public override ulong GetIntegerValue() => (ulong)typedValue;
        public override T GetValue<T>() => (T)Convert.ChangeType(typedValue, typeof(T));

        public override string ToString() => typedValue.ToString(CultureInfo.InvariantCulture);

        public override int GetHashCode() {
            return typedValue.GetHashCode();
        }

        public override bool Equals(object obj) {
            if (obj is PrimitiveIntegerValue primitive) {
                return typedValue.Equals(primitive.typedValue);
            }
            return typedValue.Equals(obj);
        }

        public override string ToAsmString() => ToString();
    }

    public class PrimitiveNullValue : PrimitiveValue {

        public override object RawValue => null;

        public override ulong GetIntegerValue() {
            throw new Exception("cannot get integer value of non-integer PrimitiveValue");
        }

        public override T GetValue<T>() => default;

        public override string ToString() => "null";

        public override int GetHashCode() {
            return "null".GetHashCode();
        }

        public override bool Equals(object obj) {
            if (obj is PrimitiveNullValue primitive) {
                return true;
            }
            return obj is null;
        }

        public override string ToAsmString() => "null";
    }

    public class PrimitiveBoolValue : PrimitiveValue {
        private bool typedValue;

        public PrimitiveBoolValue(bool value) { typedValue = value; }

        public override object RawValue => typedValue;

        public override ulong GetIntegerValue() {
            throw new Exception("cannot get integer value of non-integer PrimitiveValue");
        }

        public override T GetValue<T>() => (T)Convert.ChangeType(typedValue, typeof(T));

        public override string ToString() => typedValue ? "true" : "false";

        public override int GetHashCode() {
            return typedValue.GetHashCode();
        }

        public override bool Equals(object obj) {
            if (obj is PrimitiveBoolValue primitive) {
                return typedValue.Equals(primitive.typedValue);
            }
            return typedValue.Equals(obj);
        }
        
        public override string ToAsmString() => ToString();
    }
}
