﻿//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
// </auto-generated>
//---------------------------------------------------------

using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;

#nullable enable

namespace go;

public static partial class main_package
{
    [GeneratedCode("go2cs-gen", "0.1.4")]
    public partial struct Uncommon_uᴛ3
    {
        // Promoted Struct References
        private readonly ж<global::go.main_package.ΔArrayType> ᏑʗArrayType;

        // Promoted Struct Accessors
        public partial ref global::go.main_package.ΔArrayType ArrayType => ref ᏑʗArrayType.val;

        // Promoted Struct Field Accessors
        public ref global::go.main_package.Type Type => ref ArrayType.Type;
        public ref global::go.ж<global::go.main_package.Type> Elem => ref ArrayType.Elem;
        public ref global::go.ж<global::go.main_package.Type> Slice => ref ArrayType.Slice;
        internal ref nuint Len => ref ArrayType.Len;

        // Promoted Struct Field Accessor References
        public static ref global::go.main_package.Type ᏑType(ref Uncommon_uᴛ3 instance) => ref instance.ArrayType.Type;
        public static ref global::go.ж<global::go.main_package.Type> ᏑElem(ref Uncommon_uᴛ3 instance) => ref instance.ArrayType.Elem;
        public static ref global::go.ж<global::go.main_package.Type> ᏑSlice(ref Uncommon_uᴛ3 instance) => ref instance.ArrayType.Slice;
        internal static ref nuint ᏑLen(ref Uncommon_uᴛ3 instance) => ref instance.ArrayType.Len;

        // Field References
        public static ref global::go.main_package.ΔArrayType ᏑArrayType(ref Uncommon_uᴛ3 instance) => ref instance.ArrayType;
        public static ref global::go.main_package.UncommonType Ꮡu(ref Uncommon_uᴛ3 instance) => ref instance.u;
        
        // Constructors
        public Uncommon_uᴛ3(NilType _)
        {
            ᏑʗArrayType = new ж<global::go.main_package.ΔArrayType>(new global::go.main_package.ΔArrayType(nil));
            this.u = default!;
        }

        public Uncommon_uᴛ3(global::go.main_package.ΔArrayType ArrayType = default!)
        {
            ᏑʗArrayType = new ж<global::go.main_package.ΔArrayType>(ArrayType);
        }

        internal Uncommon_uᴛ3(global::go.main_package.ΔArrayType ArrayType = default!, global::go.main_package.UncommonType u = default!)
        {
            ᏑʗArrayType = new ж<global::go.main_package.ΔArrayType>(ArrayType);
            this.u = u;
        }
        
        // Handle comparisons between struct 'Uncommon_uᴛ3' instances
        public bool Equals(Uncommon_uᴛ3 other) =>
            ArrayType == other.ArrayType &&
            u == other.u;
        
        public override bool Equals(object? obj) => obj is Uncommon_uᴛ3 other && Equals(other);
        
        public override int GetHashCode() => runtime.HashCode.Combine(
            ArrayType,
            u);
        
        public static bool operator ==(Uncommon_uᴛ3 left, Uncommon_uᴛ3 right) => left.Equals(right);
        
        public static bool operator !=(Uncommon_uᴛ3 left, Uncommon_uᴛ3 right) => !(left == right);

        // Handle comparisons between 'nil' and struct 'Uncommon_uᴛ3'
        public static bool operator ==(Uncommon_uᴛ3 value, NilType nil) => value.Equals(default(Uncommon_uᴛ3));

        public static bool operator !=(Uncommon_uᴛ3 value, NilType nil) => !(value == nil);

        public static bool operator ==(NilType nil, Uncommon_uᴛ3 value) => value == nil;

        public static bool operator !=(NilType nil, Uncommon_uᴛ3 value) => value != nil;

        public static implicit operator Uncommon_uᴛ3(NilType nil) => default(Uncommon_uᴛ3);

        public override string ToString() => string.Concat("{", string.Join(" ",
        [
            ArrayType.ToString(),
            u.ToString()
        ]), "}");
    }

    // Promoted Struct Receivers
}
