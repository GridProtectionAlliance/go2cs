//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2020 October 09 06:01:50 UTC
// </auto-generated>
//---------------------------------------------------------
using System.CodeDom.Compiler;
using System.Runtime.CompilerServices;
using go;

#nullable enable

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace @internal {
namespace @event {
namespace export {
namespace ocagent
{
    public static partial class wire_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        public partial struct Timestamp
        {
            // Value of the Timestamp struct
            private readonly @string m_value;

            public Timestamp(@string value) => m_value = value;

            // Enable implicit conversions between @string and Timestamp struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Timestamp(@string value) => new Timestamp(value);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator @string(Timestamp value) => value.m_value;
            
            // Enable comparisons between nil and Timestamp struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(Timestamp value, NilType nil) => value.Equals(default(Timestamp));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(Timestamp value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, Timestamp value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, Timestamp value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Timestamp(NilType nil) => default(Timestamp);
        }
    }
}}}}}}}}
