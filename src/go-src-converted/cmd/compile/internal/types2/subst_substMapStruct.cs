//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 06:26:22 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using bytes = go.bytes_package;
using syntax = go.cmd.compile.@internal.syntax_package;
using fmt = go.fmt_package;
using go;

#nullable enable

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class types2_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        private partial struct substMap
        {
            // Constructors
            public substMap(NilType _)
            {
                this.targs = default;
                this.proj = default;
            }

            public substMap(slice<Type> targs = default, map<ptr<TypeParam>, Type> proj = default)
            {
                this.targs = targs;
                this.proj = proj;
            }

            // Enable comparisons between nil and substMap struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(substMap value, NilType nil) => value.Equals(default(substMap));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(substMap value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, substMap value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, substMap value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator substMap(NilType nil) => default(substMap);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        private static substMap substMap_cast(dynamic value)
        {
            return new substMap(value.targs, value.proj);
        }
    }
}}}}