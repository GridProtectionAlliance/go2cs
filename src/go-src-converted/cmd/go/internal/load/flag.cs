// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package load -- go2cs converted at 2020 August 29 10:00:52 UTC
// import "cmd/go/internal/load" ==> using load = go.cmd.go.@internal.load_package
// Original source: C:\Go\src\cmd\go\internal\load\flag.go
using @base = go.cmd.go.@internal.@base_package;
using str = go.cmd.go.@internal.str_package;
using fmt = go.fmt_package;
using strings = go.strings_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class load_package
    {
        public static PerPackageFlag BuildAsmflags = default;        public static PerPackageFlag BuildGcflags = default;        public static PerPackageFlag BuildLdflags = default;        public static PerPackageFlag BuildGccgoflags = default;

        // A PerPackageFlag is a command-line flag implementation (a flag.Value)
        // that allows specifying different effective flags for different packages.
        // See 'go help build' for more details about per-package flags.
        public partial struct PerPackageFlag
        {
            public bool present;
            public slice<ppfValue> values;
        }

        // A ppfValue is a single <pattern>=<flags> per-package flag value.
        private partial struct ppfValue
        {
            public Func<ref Package, bool> match; // compiled pattern
            public slice<@string> flags;
        }

        // Set is called each time the flag is encountered on the command line.
        private static error Set(this ref PerPackageFlag f, @string v)
        {
            return error.As(f.set(v, @base.Cwd));
        }

        // set is the implementation of Set, taking a cwd (current working directory) for easier testing.
        private static error set(this ref PerPackageFlag f, @string v, @string cwd)
        {
            f.present = true;
            Func<ref Package, bool> match = p => error.As(p.Internal.CmdlinePkg || p.Internal.CmdlineFiles);
            } // default predicate with no pattern
            // For backwards compatibility with earlier flag splitting, ignore spaces around flags.; // default predicate with no pattern
            // For backwards compatibility with earlier flag splitting, ignore spaces around flags.
            v = strings.TrimSpace(v);
            if (v == "")
            { 
                // Special case: -gcflags="" means no flags for command-line arguments
                // (overrides previous -gcflags="-whatever").
                f.values = append(f.values, new ppfValue(match,[]string{}));
                return error.As(null);
            }
            if (!strings.HasPrefix(v, "-"))
            {
                var i = strings.Index(v, "=");
                if (i < 0L)
                {
                    return error.As(fmt.Errorf("missing =<value> in <pattern>=<value>"));
                }
                if (i == 0L)
                {
                    return error.As(fmt.Errorf("missing <pattern> in <pattern>=<value>"));
                }
                var pattern = strings.TrimSpace(v[..i]);
                match = MatchPackage(pattern, cwd);
                v = v[i + 1L..];
            }
            var (flags, err) = str.SplitQuotedFields(v);
            if (err != null)
            {
                return error.As(err);
            }
            if (flags == null)
            {
                flags = new slice<@string>(new @string[] {  });
            }
            f.values = append(f.values, new ppfValue(match,flags));
            return error.As(null);
        }

        // String is required to implement flag.Value.
        // It is not used, because cmd/go never calls flag.PrintDefaults.
        private static @string String(this ref PerPackageFlag f)
        {
            return "<PerPackageFlag>";
        }

        // Present reports whether the flag appeared on the command line.
        private static bool Present(this ref PerPackageFlag f)
        {
            return f.present;
        }

        // For returns the flags to use for the given package.
        private static slice<@string> For(this ref PerPackageFlag f, ref Package p)
        {
            @string flags = new slice<@string>(new @string[] {  });
            foreach (var (_, v) in f.values)
            {
                if (v.match(p))
                {
                    flags = v.flags;
                }
            }
            return flags;
        }

        private static slice<Func<ref Package, bool>> cmdlineMatchers = default;

        // SetCmdlinePatterns records the set of patterns given on the command line,
        // for use by the PerPackageFlags.
        public static void SetCmdlinePatterns(slice<@string> args)
        {
            setCmdlinePatterns(args, @base.Cwd);
        }

        private static void setCmdlinePatterns(slice<@string> args, @string cwd)
        {
            if (len(args) == 0L)
            {
                args = new slice<@string>(new @string[] { "." });
            }
            cmdlineMatchers = null; // allow reset for testing
            foreach (var (_, arg) in args)
            {
                cmdlineMatchers = append(cmdlineMatchers, MatchPackage(arg, cwd));
            }
        }

        // isCmdlinePkg reports whether p is a package listed on the command line.
        private static bool isCmdlinePkg(ref Package p)
        {
            foreach (var (_, m) in cmdlineMatchers)
            {
                if (m(p))
                {
                    return true;
                }
            }
            return false;
        }
    }
}}}}
