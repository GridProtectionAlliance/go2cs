//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2020 October 09 05:54:25 UTC
// </auto-generated>
//---------------------------------------------------------
using System.CodeDom.Compiler;
using System.Runtime.CompilerServices;
using go;

#nullable enable

namespace go {
namespace cmd {
namespace vendor {
namespace golang.org {
namespace x {
namespace arch {
namespace arm
{
    public static partial class armasm_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        public partial struct Float32Imm
        {
            // Value of the Float32Imm struct
            private readonly float m_value;

            public Float32Imm(float value) => m_value = value;

            // Enable implicit conversions between float and Float32Imm struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Float32Imm(float value) => new Float32Imm(value);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator float(Float32Imm value) => value.m_value;
            
            // Enable comparisons between nil and Float32Imm struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(Float32Imm value, NilType nil) => value.Equals(default(Float32Imm));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(Float32Imm value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, Float32Imm value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, Float32Imm value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Float32Imm(NilType nil) => default(Float32Imm);
        }
    }
}}}}}}}
