//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2020 October 09 04:45:31 UTC
// </auto-generated>
//---------------------------------------------------------
using System.CodeDom.Compiler;
using System.Runtime.CompilerServices;
using go;

#nullable enable

namespace go {
namespace @internal
{
    public static partial class cpu_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        private partial struct function
        {
            // Value of the function struct
            private readonly byte m_value;

            public function(byte value) => m_value = value;

            // Enable implicit conversions between byte and function struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator function(byte value) => new function(value);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator byte(function value) => value.m_value;
            
            // Enable comparisons between nil and function struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(function value, NilType nil) => value.Equals(default(function));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(function value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, function value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, function value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator function(NilType nil) => default(function);
        }
    }
}}
