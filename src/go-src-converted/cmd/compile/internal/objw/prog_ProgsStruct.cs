//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 05:59:02 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using @base = go.cmd.compile.@internal.@base_package;
using ir = go.cmd.compile.@internal.ir_package;
using obj = go.cmd.@internal.obj_package;
using objabi = go.cmd.@internal.objabi_package;
using src = go.cmd.@internal.src_package;
using go;

#nullable enable

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class objw_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        public partial struct Progs
        {
            // Constructors
            public Progs(NilType _)
            {
                this.Text = default;
                this.Next = default;
                this.PC = default;
                this.Pos = default;
                this.CurFunc = default;
                this.Cache = default;
                this.CacheIndex = default;
                this.NextLive = default;
                this.PrevLive = default;
            }

            public Progs(ref ptr<obj.Prog> Text = default, ref ptr<obj.Prog> Next = default, long PC = default, src.XPos Pos = default, ref ptr<ir.Func> CurFunc = default, slice<obj.Prog> Cache = default, nint CacheIndex = default, LivenessIndex NextLive = default, LivenessIndex PrevLive = default)
            {
                this.Text = Text;
                this.Next = Next;
                this.PC = PC;
                this.Pos = Pos;
                this.CurFunc = CurFunc;
                this.Cache = Cache;
                this.CacheIndex = CacheIndex;
                this.NextLive = NextLive;
                this.PrevLive = PrevLive;
            }

            // Enable comparisons between nil and Progs struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(Progs value, NilType nil) => value.Equals(default(Progs));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(Progs value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, Progs value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, Progs value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Progs(NilType nil) => default(Progs);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        public static Progs Progs_cast(dynamic value)
        {
            return new Progs(ref value.Text, ref value.Next, value.PC, value.Pos, ref value.CurFunc, value.Cache, value.CacheIndex, value.NextLive, value.PrevLive);
        }
    }
}}}}