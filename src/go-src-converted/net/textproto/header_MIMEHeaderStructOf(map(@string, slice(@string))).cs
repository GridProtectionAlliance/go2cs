//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2020 October 09 04:56:05 UTC
// </auto-generated>
//---------------------------------------------------------
using System.CodeDom.Compiler;
using System.Runtime.CompilerServices;
using go;

#nullable enable

namespace go {
namespace net
{
    public static partial class textproto_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        public partial struct MIMEHeader
        {
            // Value of the MIMEHeader struct
            private readonly map<@string, slice<@string>> m_value;

            public MIMEHeader(map<@string, slice<@string>> value) => m_value = value;

            // Enable implicit conversions between map<@string, slice<@string>> and MIMEHeader struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator MIMEHeader(map<@string, slice<@string>> value) => new MIMEHeader(value);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator map<@string, slice<@string>>(MIMEHeader value) => value.m_value;
            
            // Enable comparisons between nil and MIMEHeader struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(MIMEHeader value, NilType nil) => value.Equals(default(MIMEHeader));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(MIMEHeader value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, MIMEHeader value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, MIMEHeader value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator MIMEHeader(NilType nil) => default(MIMEHeader);
        }
    }
}}
