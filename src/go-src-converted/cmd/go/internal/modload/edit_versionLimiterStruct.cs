//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 06:31:26 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using mvs = go.cmd.go.@internal.mvs_package;
using context = go.context_package;
using reflect = go.reflect_package;
using sort = go.sort_package;
using module = go.golang.org.x.mod.module_package;
using semver = go.golang.org.x.mod.semver_package;
using go;

#nullable enable

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class modload_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        private partial struct versionLimiter
        {
            // Constructors
            public versionLimiter(NilType _)
            {
                this.depth = default;
                this.max = default;
                this.selected = default;
                this.dqReason = default;
                this.requiring = default;
            }

            public versionLimiter(modDepth depth = default, map<@string, @string> max = default, map<@string, @string> selected = default, map<module.Version, dqState> dqReason = default, map<module.Version, slice<module.Version>> requiring = default)
            {
                this.depth = depth;
                this.max = max;
                this.selected = selected;
                this.dqReason = dqReason;
                this.requiring = requiring;
            }

            // Enable comparisons between nil and versionLimiter struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(versionLimiter value, NilType nil) => value.Equals(default(versionLimiter));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(versionLimiter value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, versionLimiter value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, versionLimiter value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator versionLimiter(NilType nil) => default(versionLimiter);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        private static versionLimiter versionLimiter_cast(dynamic value)
        {
            return new versionLimiter(value.depth, value.max, value.selected, value.dqReason, value.requiring);
        }
    }
}}}}