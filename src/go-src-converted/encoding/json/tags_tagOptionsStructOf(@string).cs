//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 05:39:56 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Runtime.CompilerServices;
using go;

#nullable enable

namespace go {
namespace encoding
{
    public static partial class json_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        private partial struct tagOptions
        {
            // Value of the tagOptions struct
            private readonly @string m_value;
            
            public tagOptions(@string value) => m_value = value;

            // Enable implicit conversions between @string and tagOptions struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator tagOptions(@string value) => new tagOptions(value);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator @string(tagOptions value) => value.m_value;
            
            // Enable comparisons between nil and tagOptions struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(tagOptions value, NilType nil) => value.Equals(default(tagOptions));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(tagOptions value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, tagOptions value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, tagOptions value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator tagOptions(NilType nil) => default(tagOptions);
        }
    }
}}
