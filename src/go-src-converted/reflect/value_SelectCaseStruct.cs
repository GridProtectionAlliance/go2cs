//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2020 October 09 05:06:56 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using static go.builtin;
using unsafeheader = go.@internal.unsafeheader_package;
using math = go.math_package;
using runtime = go.runtime_package;
using @unsafe = go.@unsafe_package;

#nullable enable

namespace go
{
    public static partial class reflect_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        public partial struct SelectCase
        {
            // Constructors
            public SelectCase(NilType _)
            {
                this.Dir = default;
                this.Chan = default;
                this.Send = default;
            }

            public SelectCase(SelectDir Dir = default, Value Chan = default, Value Send = default)
            {
                this.Dir = Dir;
                this.Chan = Chan;
                this.Send = Send;
            }

            // Enable comparisons between nil and SelectCase struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(SelectCase value, NilType nil) => value.Equals(default(SelectCase));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(SelectCase value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, SelectCase value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, SelectCase value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator SelectCase(NilType nil) => default(SelectCase);
        }

        [GeneratedCode("go2cs", "0.1.0.0")]
        public static SelectCase SelectCase_cast(dynamic value)
        {
            return new SelectCase(value.Dir, value.Chan, value.Send);
        }
    }
}