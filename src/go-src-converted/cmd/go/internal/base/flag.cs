// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package @base -- go2cs converted at 2020 August 29 10:01:59 UTC
// import "cmd/go/internal/base" ==> using @base = go.cmd.go.@internal.@base_package
// Original source: C:\Go\src\cmd\go\internal\base\flag.go
using flag = go.flag_package;

using cfg = go.cmd.go.@internal.cfg_package;
using str = go.cmd.go.@internal.str_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class @base_package
    {
        // A StringsFlag is a command-line flag that interprets its argument
        // as a space-separated list of possibly-quoted strings.
        public partial struct StringsFlag // : slice<@string>
        {
        }

        private static error Set(this ref StringsFlag v, @string s)
        {
            error err = default;
            v.Value, err = str.SplitQuotedFields(s);
            if (v == null.Value)
            {
                v.Value = new slice<@string>(new @string[] {  });
            }
            return error.As(err);
        }

        private static @string String(this ref StringsFlag v)
        {
            return "<StringsFlag>";
        }

        // AddBuildFlagsNX adds the -n and -x build flags to the flag set.
        public static void AddBuildFlagsNX(ref flag.FlagSet flags)
        {
            flags.BoolVar(ref cfg.BuildN, "n", false, "");
            flags.BoolVar(ref cfg.BuildX, "x", false, "");
        }
    }
}}}}
