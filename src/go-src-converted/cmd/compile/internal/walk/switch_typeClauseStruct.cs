//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 06 23:12:07 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using constant = go.go.constant_package;
using token = go.go.token_package;
using sort = go.sort_package;
using @base = go.cmd.compile.@internal.@base_package;
using ir = go.cmd.compile.@internal.ir_package;
using typecheck = go.cmd.compile.@internal.typecheck_package;
using types = go.cmd.compile.@internal.types_package;
using src = go.cmd.@internal.src_package;
using go;

#nullable enable

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class walk_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        private partial struct typeClause
        {
            // Constructors
            public typeClause(NilType _)
            {
                this.hash = default;
                this.body = default;
            }

            public typeClause(uint hash = default, ir.Nodes body = default)
            {
                this.hash = hash;
                this.body = body;
            }

            // Enable comparisons between nil and typeClause struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(typeClause value, NilType nil) => value.Equals(default(typeClause));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(typeClause value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, typeClause value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, typeClause value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator typeClause(NilType nil) => default(typeClause);
        }

        [GeneratedCode("go2cs", "0.1.0.0")]
        private static typeClause typeClause_cast(dynamic value)
        {
            return new typeClause(value.hash, value.body);
        }
    }
}}}}