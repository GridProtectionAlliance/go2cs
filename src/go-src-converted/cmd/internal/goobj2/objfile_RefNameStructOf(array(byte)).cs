//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2020 October 09 05:08:50 UTC
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
    public static partial class goobj2_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        public partial struct RefName
        {
            // Value of the RefName struct
            private readonly array<byte> m_value;

            public RefName(array<byte> value) => m_value = value;

            // Enable implicit conversions between array<byte> and RefName struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator RefName(array<byte> value) => new RefName(value);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator array<byte>(RefName value) => value.m_value;
            
            // Enable comparisons between nil and RefName struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(RefName value, NilType nil) => value.Equals(default(RefName));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(RefName value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, RefName value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, RefName value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator RefName(NilType nil) => default(RefName);
        }
    }
}}}
