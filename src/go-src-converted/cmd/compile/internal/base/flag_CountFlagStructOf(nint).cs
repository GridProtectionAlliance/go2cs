//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 06 23:14:30 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Runtime.CompilerServices;
using go;

#nullable enable

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class @base_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        public partial struct CountFlag
        {
            // Value of the CountFlag struct
            private readonly nint m_value;
            
            public CountFlag(nint value) => m_value = value;

            // Enable implicit conversions between nint and CountFlag struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator CountFlag(nint value) => new CountFlag(value);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator nint(CountFlag value) => value.m_value;
            
            // Enable comparisons between nil and CountFlag struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(CountFlag value, NilType nil) => value.Equals(default(CountFlag));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(CountFlag value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, CountFlag value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, CountFlag value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator CountFlag(NilType nil) => default(CountFlag);
        }
    }
}}}}