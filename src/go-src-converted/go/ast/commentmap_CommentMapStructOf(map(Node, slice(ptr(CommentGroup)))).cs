//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 05:54:05 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Runtime.CompilerServices;
using go;

#nullable enable

namespace go {
namespace go
{
    public static partial class ast_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        public partial struct CommentMap : IMap
        {
            // Value of the CommentMap struct
            private readonly map<Node, slice<ptr<CommentGroup>>> m_value;
            
            public nint Length => ((IMap)m_value).Length;

            object? IMap.this[object key]
            {
                get => ((IMap)m_value)[key];
                set => ((IMap)m_value)[key] = value;
            }

            public slice<ptr<CommentGroup>> this[Node key]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => m_value[key];
            
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set => m_value[key] = value;
            }

            public (slice<ptr<CommentGroup>>, bool) this[Node key, bool _]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => m_value.TryGetValue(key, out slice<ptr<CommentGroup>> value) ? (value!, true) : (default!, false);
            }

            public CommentMap(map<Node, slice<ptr<CommentGroup>>> value) => m_value = value;

            // Enable implicit conversions between map<Node, slice<ptr<CommentGroup>>> and CommentMap struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator CommentMap(map<Node, slice<ptr<CommentGroup>>> value) => new CommentMap(value);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator map<Node, slice<ptr<CommentGroup>>>(CommentMap value) => value.m_value;
            
            // Enable comparisons between nil and CommentMap struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(CommentMap value, NilType nil) => value.Equals(default(CommentMap));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(CommentMap value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, CommentMap value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, CommentMap value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator CommentMap(NilType nil) => default(CommentMap);
        }
    }
}}
