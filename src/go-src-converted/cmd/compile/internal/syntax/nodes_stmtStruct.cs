//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 06:26:36 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

using go;

#nullable enable

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class syntax_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        [PromotedStruct(typeof(node))]
        private partial struct stmt
        {
            // node structure promotion - sourced from value copy
            private readonly ptr<node> m_nodeRef;

            private ref node node_val => ref m_nodeRef.Value;

            public ref Pos pos => ref m_nodeRef.Value.pos;

            // Constructors
            public stmt(NilType _)
            {
                this.m_nodeRef = new ptr<node>(new node(nil));
            }

            public stmt(node node = default)
            {
                this.m_nodeRef = new ptr<node>(node);
            }

            // Enable comparisons between nil and stmt struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(stmt value, NilType nil) => value.Equals(default(stmt));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(stmt value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, stmt value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, stmt value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator stmt(NilType nil) => default(stmt);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        private static stmt stmt_cast(dynamic value)
        {
            return new stmt(value.node);
        }
    }
}}}}