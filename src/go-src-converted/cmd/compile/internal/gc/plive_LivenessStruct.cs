//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2020 October 09 05:42:21 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using static go.builtin;
using ssa = go.cmd.compile.@internal.ssa_package;
using types = go.cmd.compile.@internal.types_package;
using obj = go.cmd.@internal.obj_package;
using objabi = go.cmd.@internal.objabi_package;
using md5 = go.crypto.md5_package;
using fmt = go.fmt_package;
using strings = go.strings_package;
using go;

#nullable enable

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class gc_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        public partial struct Liveness
        {
            // Constructors
            public Liveness(NilType _)
            {
                this.fn = default;
                this.f = default;
                this.vars = default;
                this.idx = default;
                this.stkptrsize = default;
                this.be = default;
                this.allUnsafe = default;
                this.unsafePoints = default;
                this.livevars = default;
                this.livenessMap = default;
                this.stackMapSet = default;
                this.stackMaps = default;
                this.regMapSet = default;
                this.regMaps = default;
                this.cache = default;
                this.openDeferVars = default;
                this.openDeferVardefToBlockMap = default;
                this.nonReturnBlocks = default;
            }

            public Liveness(ref ptr<Node> fn = default, ref ptr<ssa.Func> f = default, slice<ptr<Node>> vars = default, map<ptr<Node>, int> idx = default, long stkptrsize = default, slice<BlockEffects> be = default, bool allUnsafe = default, bvec unsafePoints = default, slice<varRegVec> livevars = default, LivenessMap livenessMap = default, bvecSet stackMapSet = default, slice<bvec> stackMaps = default, map<liveRegMask, long> regMapSet = default, slice<liveRegMask> regMaps = default, progeffectscache cache = default, slice<openDeferVarInfo> openDeferVars = default, map<ptr<Node>, ptr<ssa.Block>> openDeferVardefToBlockMap = default, map<ptr<ssa.Block>, bool> nonReturnBlocks = default)
            {
                this.fn = fn;
                this.f = f;
                this.vars = vars;
                this.idx = idx;
                this.stkptrsize = stkptrsize;
                this.be = be;
                this.allUnsafe = allUnsafe;
                this.unsafePoints = unsafePoints;
                this.livevars = livevars;
                this.livenessMap = livenessMap;
                this.stackMapSet = stackMapSet;
                this.stackMaps = stackMaps;
                this.regMapSet = regMapSet;
                this.regMaps = regMaps;
                this.cache = cache;
                this.openDeferVars = openDeferVars;
                this.openDeferVardefToBlockMap = openDeferVardefToBlockMap;
                this.nonReturnBlocks = nonReturnBlocks;
            }

            // Enable comparisons between nil and Liveness struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(Liveness value, NilType nil) => value.Equals(default(Liveness));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(Liveness value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, Liveness value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, Liveness value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Liveness(NilType nil) => default(Liveness);
        }

        [GeneratedCode("go2cs", "0.1.0.0")]
        public static Liveness Liveness_cast(dynamic value)
        {
            return new Liveness(ref value.fn, ref value.f, value.vars, value.idx, value.stkptrsize, value.be, value.allUnsafe, value.unsafePoints, value.livevars, value.livenessMap, value.stackMapSet, value.stackMaps, value.regMapSet, value.regMaps, value.cache, value.openDeferVars, value.openDeferVardefToBlockMap, value.nonReturnBlocks);
        }
    }
}}}}