//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 05:41:08 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using errors = go.errors_package;
using io = go.io_package;
using utf8 = go.unicode.utf8_package;

#nullable enable

namespace go
{
    public static partial class strings_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        public partial struct Reader
        {
            // Constructors
            public Reader(NilType _)
            {
                this.s = default;
                this.i = default;
                this.prevRune = default;
            }

            public Reader(@string s = default, long i = default, nint prevRune = default)
            {
                this.s = s;
                this.i = i;
                this.prevRune = prevRune;
            }

            // Enable comparisons between nil and Reader struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(Reader value, NilType nil) => value.Equals(default(Reader));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(Reader value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, Reader value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, Reader value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Reader(NilType nil) => default(Reader);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        public static Reader Reader_cast(dynamic value)
        {
            return new Reader(value.s, value.i, value.prevRune);
        }
    }
}