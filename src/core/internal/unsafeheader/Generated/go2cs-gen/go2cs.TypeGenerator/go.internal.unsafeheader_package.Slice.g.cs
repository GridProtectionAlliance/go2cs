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
using go;

#nullable enable

namespace go.@internal;

public static partial class unsafeheader_package
{
    [GeneratedCode("go2cs-gen", "0.1.4")]
    public partial struct Slice
    {
        // Promoted Struct References
        // -- Slice has no promoted structs

        // Field References
        public static ref global::go.unsafe_package.Pointer ᏑData(ref Slice instance) => ref instance.Data;
        internal static ref nint ᏑLen(ref Slice instance) => ref instance.Len;
        internal static ref nint ᏑCap(ref Slice instance) => ref instance.Cap;
        
        // Constructors
        public Slice(NilType _)
        {
            this.Data = default!;
            this.Len = default!;
            this.Cap = default!;
        }

        public Slice(global::go.unsafe_package.Pointer Data = default!, nint Len = default!, nint Cap = default!)
        {
            this.Data = Data;
            this.Len = Len;
            this.Cap = Cap;
        }
        
        // Handle comparisons between struct 'Slice' instances
        public bool Equals(Slice other) =>
            Data == other.Data &&
            Len == other.Len &&
            Cap == other.Cap;
        
        public override bool Equals(object? obj) => obj is Slice other && Equals(other);
        
        public override int GetHashCode() => runtime.HashCode.Combine(
            Data,
            Len,
            Cap);
        
        public static bool operator ==(Slice left, Slice right) => left.Equals(right);
        
        public static bool operator !=(Slice left, Slice right) => !(left == right);

        // Handle comparisons between 'nil' and struct 'Slice'
        public static bool operator ==(Slice value, NilType nil) => value.Equals(default(Slice));

        public static bool operator !=(Slice value, NilType nil) => !(value == nil);

        public static bool operator ==(NilType nil, Slice value) => value == nil;

        public static bool operator !=(NilType nil, Slice value) => value != nil;

        public static implicit operator Slice(NilType nil) => default(Slice);

        public override string ToString() => string.Concat("{", string.Join(" ",
        [
            Data?.ToString() ?? "<nil>",
            Len.ToString(),
            Cap.ToString()
        ]), "}");
    }
}
