//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 06:01:33 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using fmt = go.fmt_package;
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
        private partial struct loop
        {
            // Constructors
            public loop(NilType _)
            {
                this.header = default;
                this.outer = default;
                this.children = default;
                this.exits = default;
                this.nBlocks = default;
                this.depth = default;
                this.isInner = default;
                this.containsUnavoidableCall = default;
            }

            public loop(ref ptr<Block> header = default, ref ptr<loop> outer = default, slice<ptr<loop>> children = default, slice<ptr<Block>> exits = default, int nBlocks = default, short depth = default, bool isInner = default, bool containsUnavoidableCall = default)
            {
                this.header = header;
                this.outer = outer;
                this.children = children;
                this.exits = exits;
                this.nBlocks = nBlocks;
                this.depth = depth;
                this.isInner = isInner;
                this.containsUnavoidableCall = containsUnavoidableCall;
            }

            // Enable comparisons between nil and loop struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(loop value, NilType nil) => value.Equals(default(loop));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(loop value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, loop value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, loop value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator loop(NilType nil) => default(loop);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        private static loop loop_cast(dynamic value)
        {
            return new loop(ref value.header, ref value.outer, value.children, value.exits, value.nBlocks, value.depth, value.isInner, value.containsUnavoidableCall);
        }
    }
}}}}