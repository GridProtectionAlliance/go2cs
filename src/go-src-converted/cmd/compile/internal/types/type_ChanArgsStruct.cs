//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 05:59:16 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using @base = go.cmd.compile.@internal.@base_package;
using src = go.cmd.@internal.src_package;
using fmt = go.fmt_package;
using sync = go.sync_package;
using go;

#nullable enable

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class types_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        public partial struct ChanArgs
        {
            // Constructors
            public ChanArgs(NilType _)
            {
                this.T = default;
            }

            public ChanArgs(ref ptr<Type> T = default)
            {
                this.T = T;
            }

            // Enable comparisons between nil and ChanArgs struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(ChanArgs value, NilType nil) => value.Equals(default(ChanArgs));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(ChanArgs value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, ChanArgs value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, ChanArgs value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator ChanArgs(NilType nil) => default(ChanArgs);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        public static ChanArgs ChanArgs_cast(dynamic value)
        {
            return new ChanArgs(ref value.T);
        }
    }
}}}}