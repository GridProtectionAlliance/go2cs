//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 06:33:14 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using bytes = go.bytes_package;
using objabi = go.cmd.@internal.objabi_package;
using ld = go.cmd.link.@internal.ld_package;
using loader = go.cmd.link.@internal.loader_package;
using sym = go.cmd.link.@internal.sym_package;
using buildcfg = go.@internal.buildcfg_package;
using io = go.io_package;
using regexp = go.regexp_package;
using go;

#nullable enable

namespace go {
namespace cmd {
namespace link {
namespace @internal
{
    public static partial class wasm_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        private partial struct wasmFuncType
        {
            // Constructors
            public wasmFuncType(NilType _)
            {
                this.Params = default;
                this.Results = default;
            }

            public wasmFuncType(slice<byte> Params = default, slice<byte> Results = default)
            {
                this.Params = Params;
                this.Results = Results;
            }

            // Enable comparisons between nil and wasmFuncType struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(wasmFuncType value, NilType nil) => value.Equals(default(wasmFuncType));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(wasmFuncType value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, wasmFuncType value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, wasmFuncType value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator wasmFuncType(NilType nil) => default(wasmFuncType);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        private static wasmFuncType wasmFuncType_cast(dynamic value)
        {
            return new wasmFuncType(value.Params, value.Results);
        }
    }
}}}}