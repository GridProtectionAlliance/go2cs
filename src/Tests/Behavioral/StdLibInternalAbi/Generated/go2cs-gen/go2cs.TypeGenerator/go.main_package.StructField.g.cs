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
    public partial struct StructField
    {
        // Promoted Struct References
        // -- StructField has no promoted structs

        // Field References
        public static ref global::go.main_package.ΔName ᏑName(ref StructField instance) => ref instance.Name;
        public static ref global::go.ж<global::go.main_package.Type> ᏑTyp(ref StructField instance) => ref instance.Typ;
        internal static ref nuint ᏑOffset(ref StructField instance) => ref instance.Offset;
        
        // Constructors
        public StructField(NilType _)
        {
            this.Name = default!;
            this.Typ = default!;
            this.Offset = default!;
        }

        public StructField(global::go.main_package.ΔName Name = default!, global::go.ж<global::go.main_package.Type> Typ = default!, nuint Offset = default!)
        {
            this.Name = Name;
            this.Typ = Typ;
            this.Offset = Offset;
        }
        
        // Handle comparisons between struct 'StructField' instances
        public bool Equals(StructField other) =>
            Name == other.Name &&
            Typ == other.Typ &&
            Offset == other.Offset;
        
        public override bool Equals(object? obj) => obj is StructField other && Equals(other);
        
        public override int GetHashCode() => runtime.HashCode.Combine(
            Name,
            Typ,
            Offset);
        
        public static bool operator ==(StructField left, StructField right) => left.Equals(right);
        
        public static bool operator !=(StructField left, StructField right) => !(left == right);

        // Handle comparisons between 'nil' and struct 'StructField'
        public static bool operator ==(StructField value, NilType nil) => value.Equals(default(StructField));

        public static bool operator !=(StructField value, NilType nil) => !(value == nil);

        public static bool operator ==(NilType nil, StructField value) => value == nil;

        public static bool operator !=(NilType nil, StructField value) => value != nil;

        public static implicit operator StructField(NilType nil) => default(StructField);

        public override string ToString() => string.Concat("{", string.Join(" ",
        [
            Name.ToString(),
            Typ?.ToString() ?? "<nil>",
            Offset.ToString()
        ]), "}");
    }
}
