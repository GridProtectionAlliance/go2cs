//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2020 October 09 05:53:25 UTC
// </auto-generated>
//---------------------------------------------------------
using System.CodeDom.Compiler;
using System.Runtime.CompilerServices;
using go;

#nullable enable

namespace go {
namespace cmd {
namespace vendor {
namespace github.com {
namespace google {
namespace pprof {
namespace @internal
{
    public static partial class driver_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        private partial struct variables
        {
            // Value of the variables struct
            private readonly map<@string, ptr<variable>> m_value;

            public variables(map<@string, ptr<variable>> value) => m_value = value;

            // Enable implicit conversions between map<@string, ptr<variable>> and variables struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator variables(map<@string, ptr<variable>> value) => new variables(value);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator map<@string, ptr<variable>>(variables value) => value.m_value;
            
            // Enable comparisons between nil and variables struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(variables value, NilType nil) => value.Equals(default(variables));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(variables value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, variables value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, variables value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator variables(NilType nil) => default(variables);
        }
    }
}}}}}}}
