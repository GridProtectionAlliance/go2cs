//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 05:25:13 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Runtime.CompilerServices;

#nullable enable

namespace go
{
    public static partial class runtime_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        private partial struct statDep
        {
            // Value of the statDep struct
            private readonly nuint m_value;
            
            public statDep(nuint value) => m_value = value;

            // Enable implicit conversions between nuint and statDep struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator statDep(nuint value) => new statDep(value);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator nuint(statDep value) => value.m_value;
            
            // Enable comparisons between nil and statDep struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(statDep value, NilType nil) => value.Equals(default(statDep));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(statDep value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, statDep value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, statDep value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator statDep(NilType nil) => default(statDep);
        }
    }
}
