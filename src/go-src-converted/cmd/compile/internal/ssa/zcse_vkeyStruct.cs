//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 06:22:42 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using types = go.cmd.compile.@internal.types_package;
using go;

#nullable enable

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class ssa_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        private partial struct vkey
        {
            // Constructors
            public vkey(NilType _)
            {
                this.op = default;
                this.ai = default;
                this.ax = default;
                this.t = default;
            }

            public vkey(Op op = default, long ai = default, Aux ax = default, ref ptr<types.Type> t = default)
            {
                this.op = op;
                this.ai = ai;
                this.ax = ax;
                this.t = t;
            }

            // Enable comparisons between nil and vkey struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(vkey value, NilType nil) => value.Equals(default(vkey));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(vkey value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, vkey value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, vkey value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator vkey(NilType nil) => default(vkey);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        private static vkey vkey_cast(dynamic value)
        {
            return new vkey(value.op, value.ai, value.ax, ref value.t);
        }
    }
}}}}