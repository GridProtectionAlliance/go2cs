//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2020 August 29 10:07:41 UTC
// </auto-generated>
//---------------------------------------------------------
using System.CodeDom.Compiler;
using System.Runtime.CompilerServices;
using go;

namespace go {
namespace cmd {
namespace vendor {
namespace golang.org {
namespace x {
namespace arch {
namespace arm64
{
    public static partial class arm64asm_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        public partial struct Imm_option
        {
            // Value of the Imm_option struct
            private readonly byte m_value;

            public Imm_option(byte value) => m_value = value;

            // Enable implicit conversions between byte and Imm_option struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Imm_option(byte value) => new Imm_option(value);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator byte(Imm_option value) => value.m_value;
            
            // Enable comparisons between nil and Imm_option struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(Imm_option value, NilType nil) => value.Equals(default(Imm_option));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(Imm_option value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, Imm_option value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, Imm_option value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Imm_option(NilType nil) => default(Imm_option);
        }
    }
}}}}}}}