//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2020 October 09 05:08:17 UTC
// </auto-generated>
//---------------------------------------------------------
using System.CodeDom.Compiler;
using System.Runtime.CompilerServices;
using go;

#nullable enable

namespace go {
namespace cmd {
namespace @internal
{
    public static partial class src_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        public partial struct PosXlogue
        {
            // Value of the PosXlogue struct
            private readonly ulong m_value;

            public PosXlogue(ulong value) => m_value = value;

            // Enable implicit conversions between ulong and PosXlogue struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator PosXlogue(ulong value) => new PosXlogue(value);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator ulong(PosXlogue value) => value.m_value;
            
            // Enable comparisons between nil and PosXlogue struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(PosXlogue value, NilType nil) => value.Equals(default(PosXlogue));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(PosXlogue value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, PosXlogue value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, PosXlogue value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator PosXlogue(NilType nil) => default(PosXlogue);
        }
    }
}}}
