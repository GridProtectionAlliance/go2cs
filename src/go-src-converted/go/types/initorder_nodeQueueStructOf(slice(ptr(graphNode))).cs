//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2020 October 09 05:19:29 UTC
// </auto-generated>
//---------------------------------------------------------
using System.CodeDom.Compiler;
using System.Runtime.CompilerServices;
using go;

#nullable enable

namespace go {
namespace go
{
    public static partial class types_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        private partial struct nodeQueue
        {
            // Value of the nodeQueue struct
            private readonly slice<ptr<graphNode>> m_value;

            public nodeQueue(slice<ptr<graphNode>> value) => m_value = value;

            // Enable implicit conversions between slice<ptr<graphNode>> and nodeQueue struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator nodeQueue(slice<ptr<graphNode>> value) => new nodeQueue(value);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator slice<ptr<graphNode>>(nodeQueue value) => value.m_value;
            
            // Enable comparisons between nil and nodeQueue struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(nodeQueue value, NilType nil) => value.Equals(default(nodeQueue));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(nodeQueue value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, nodeQueue value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, nodeQueue value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator nodeQueue(NilType nil) => default(nodeQueue);
        }
    }
}}
