//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2020 August 29 09:26:56 UTC
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
    public static partial class gc_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        private partial struct methodbyname
        {
            // Value of the methodbyname struct
            private readonly slice<ref types.Field> m_value;

            public methodbyname(slice<ref types.Field> value) => m_value = value;

            // Enable implicit conversions between slice<ref types.Field> and methodbyname struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator methodbyname(slice<ref types.Field> value) => new methodbyname(value);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator slice<ref types.Field>(methodbyname value) => value.m_value;
            
            // Enable comparisons between nil and methodbyname struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(methodbyname value, NilType nil) => value.Equals(default(methodbyname));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(methodbyname value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, methodbyname value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, methodbyname value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator methodbyname(NilType nil) => default(methodbyname);
        }
    }
}}}}