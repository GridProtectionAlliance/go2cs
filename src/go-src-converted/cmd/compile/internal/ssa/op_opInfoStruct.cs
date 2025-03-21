//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 06:01:42 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using abi = go.cmd.compile.@internal.abi_package;
using ir = go.cmd.compile.@internal.ir_package;
using types = go.cmd.compile.@internal.types_package;
using obj = go.cmd.@internal.obj_package;
using fmt = go.fmt_package;
using strings = go.strings_package;
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
        private partial struct opInfo
        {
            // Constructors
            public opInfo(NilType _)
            {
                this.name = default;
                this.reg = default;
                this.auxType = default;
                this.argLen = default;
                this.asm = default;
                this.generic = default;
                this.rematerializeable = default;
                this.commutative = default;
                this.resultInArg0 = default;
                this.resultNotInArgs = default;
                this.clobberFlags = default;
                this.call = default;
                this.nilCheck = default;
                this.faultOnNilArg0 = default;
                this.faultOnNilArg1 = default;
                this.usesScratch = default;
                this.hasSideEffects = default;
                this.zeroWidth = default;
                this.unsafePoint = default;
                this.symEffect = default;
                this.scale = default;
            }

            public opInfo(@string name = default, regInfo reg = default, auxType auxType = default, int argLen = default, obj.As asm = default, bool generic = default, bool rematerializeable = default, bool commutative = default, bool resultInArg0 = default, bool resultNotInArgs = default, bool clobberFlags = default, bool call = default, bool nilCheck = default, bool faultOnNilArg0 = default, bool faultOnNilArg1 = default, bool usesScratch = default, bool hasSideEffects = default, bool zeroWidth = default, bool unsafePoint = default, SymEffect symEffect = default, byte scale = default)
            {
                this.name = name;
                this.reg = reg;
                this.auxType = auxType;
                this.argLen = argLen;
                this.asm = asm;
                this.generic = generic;
                this.rematerializeable = rematerializeable;
                this.commutative = commutative;
                this.resultInArg0 = resultInArg0;
                this.resultNotInArgs = resultNotInArgs;
                this.clobberFlags = clobberFlags;
                this.call = call;
                this.nilCheck = nilCheck;
                this.faultOnNilArg0 = faultOnNilArg0;
                this.faultOnNilArg1 = faultOnNilArg1;
                this.usesScratch = usesScratch;
                this.hasSideEffects = hasSideEffects;
                this.zeroWidth = zeroWidth;
                this.unsafePoint = unsafePoint;
                this.symEffect = symEffect;
                this.scale = scale;
            }

            // Enable comparisons between nil and opInfo struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(opInfo value, NilType nil) => value.Equals(default(opInfo));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(opInfo value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, opInfo value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, opInfo value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator opInfo(NilType nil) => default(opInfo);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        private static opInfo opInfo_cast(dynamic value)
        {
            return new opInfo(value.name, value.reg, value.auxType, value.argLen, value.asm, value.generic, value.rematerializeable, value.commutative, value.resultInArg0, value.resultNotInArgs, value.clobberFlags, value.call, value.nilCheck, value.faultOnNilArg0, value.faultOnNilArg1, value.usesScratch, value.hasSideEffects, value.zeroWidth, value.unsafePoint, value.symEffect, value.scale);
        }
    }
}}}}