//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 06 22:49:01 UTC
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
    public static partial class ir_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        public partial struct CallUse
        {
            // Value of the CallUse struct
            private readonly byte m_value;
            
            public CallUse(byte value) => m_value = value;

            // Enable implicit conversions between byte and CallUse struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator CallUse(byte value) => new CallUse(value);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator byte(CallUse value) => value.m_value;
            
            // Enable comparisons between nil and CallUse struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(CallUse value, NilType nil) => value.Equals(default(CallUse));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(CallUse value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, CallUse value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, CallUse value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator CallUse(NilType nil) => default(CallUse);
        }
    }
}}}}