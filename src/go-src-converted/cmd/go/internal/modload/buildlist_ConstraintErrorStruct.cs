//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 06:31:22 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using @base = go.cmd.go.@internal.@base_package;
using cfg = go.cmd.go.@internal.cfg_package;
using mvs = go.cmd.go.@internal.mvs_package;
using par = go.cmd.go.@internal.par_package;
using context = go.context_package;
using fmt = go.fmt_package;
using os = go.os_package;
using reflect = go.reflect_package;
using runtime = go.runtime_package;
using debug = go.runtime.debug_package;
using strings = go.strings_package;
using sync = go.sync_package;
using atomic = go.sync.atomic_package;
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
        public partial struct ConstraintError
        {
            // Constructors
            public ConstraintError(NilType _)
            {
                this.Conflicts = default;
            }

            public ConstraintError(slice<Conflict> Conflicts = default)
            {
                this.Conflicts = Conflicts;
            }

            // Enable comparisons between nil and ConstraintError struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(ConstraintError value, NilType nil) => value.Equals(default(ConstraintError));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(ConstraintError value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, ConstraintError value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, ConstraintError value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator ConstraintError(NilType nil) => default(ConstraintError);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        public static ConstraintError ConstraintError_cast(dynamic value)
        {
            return new ConstraintError(value.Conflicts);
        }
    }
}}}}