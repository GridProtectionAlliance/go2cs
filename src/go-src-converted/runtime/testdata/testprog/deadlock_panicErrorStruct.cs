//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 05:29:21 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using fmt = go.fmt_package;
using runtime = go.runtime_package;
using debug = go.runtime.debug_package;
using time = go.time_package;

#nullable enable

namespace go
{
    public static partial class main_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        private partial struct panicError
        {
            // Constructors
            public panicError(NilType _)
            {
            }
            // Enable comparisons between nil and panicError struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(panicError value, NilType nil) => value.Equals(default(panicError));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(panicError value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, panicError value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, panicError value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator panicError(NilType nil) => default(panicError);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        private static panicError panicError_cast(dynamic value)
        {
            return new panicError();
        }
    }
}