//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2020 October 09 06:02:45 UTC
// </auto-generated>
//---------------------------------------------------------
using System.CodeDom.Compiler;
using System.Runtime.CompilerServices;
using go;

#nullable enable

namespace go {
namespace go {
namespace @internal
{
    public static partial class exports_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        public partial struct T3
        {
            // Value of the T3 struct
            private readonly slice<long> m_value;

            public T3(slice<long> value) => m_value = value;

            // Enable implicit conversions between slice<long> and T3 struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator T3(slice<long> value) => new T3(value);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator slice<long>(T3 value) => value.m_value;
            
            // Enable comparisons between nil and T3 struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(T3 value, NilType nil) => value.Equals(default(T3));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(T3 value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, T3 value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, T3 value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator T3(NilType nil) => default(T3);
        }
    }
}}}
