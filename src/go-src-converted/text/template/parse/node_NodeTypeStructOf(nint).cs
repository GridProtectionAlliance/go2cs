//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 05:38:58 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Runtime.CompilerServices;
using go;

#nullable enable

namespace go {
namespace text {
namespace template
{
    public static partial class parse_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        public partial struct NodeType
        {
            // Value of the NodeType struct
            private readonly nint m_value;
            
            public NodeType(nint value) => m_value = value;

            // Enable implicit conversions between nint and NodeType struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator NodeType(nint value) => new NodeType(value);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator nint(NodeType value) => value.m_value;
            
            // Enable comparisons between nil and NodeType struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NodeType value, NilType nil) => value.Equals(default(NodeType));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NodeType value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, NodeType value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, NodeType value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator NodeType(NilType nil) => default(NodeType);
        }
    }
}}}
