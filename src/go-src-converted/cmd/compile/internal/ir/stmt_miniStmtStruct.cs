//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 06 22:49:16 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using @base = go.cmd.compile.@internal.@base_package;
using types = go.cmd.compile.@internal.types_package;
using src = go.cmd.@internal.src_package;
using go;

#nullable enable

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class ir_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        private partial struct miniStmt
        {
            // Constructors
            public miniStmt(NilType _)
            {
                this.miniNode = default;
                this.init = default;
            }

            public miniStmt(miniNode miniNode = default, Nodes init = default)
            {
                this.miniNode = miniNode;
                this.init = init;
            }

            // Enable comparisons between nil and miniStmt struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(miniStmt value, NilType nil) => value.Equals(default(miniStmt));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(miniStmt value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, miniStmt value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, miniStmt value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator miniStmt(NilType nil) => default(miniStmt);
        }

        [GeneratedCode("go2cs", "0.1.0.0")]
        private static miniStmt miniStmt_cast(dynamic value)
        {
            return new miniStmt(value.miniNode, value.init);
        }
    }
}}}}