//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2020 August 29 08:54:11 UTC
// </auto-generated>
//---------------------------------------------------------
using System.CodeDom.Compiler;
using System.Runtime.CompilerServices;
using go;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class ssa_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        public partial struct SymEffect
        {
            // Value of the SymEffect struct
            private readonly sbyte m_value;

            public SymEffect(sbyte value) => m_value = value;

            // Enable implicit conversions between sbyte and SymEffect struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator SymEffect(sbyte value) => new SymEffect(value);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator sbyte(SymEffect value) => value.m_value;
            
            // Enable comparisons between nil and SymEffect struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(SymEffect value, NilType nil) => value.Equals(default(SymEffect));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(SymEffect value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, SymEffect value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, SymEffect value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator SymEffect(NilType nil) => default(SymEffect);
        }
    }
}}}}