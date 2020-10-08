// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package @base -- go2cs converted at 2020 October 08 04:36:56 UTC
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

        private static error Set(this ptr<StringsFlag> _addr_v, @string s)
        {
            ref StringsFlag v = ref _addr_v.val;

            error err = default!;
            v.val, err = str.SplitQuotedFields(s);
            if (v == null.val)
            {
                v.val = new slice<@string>(new @string[] {  });
            }

            return error.As(err)!;

        }

        private static @string String(this ptr<StringsFlag> _addr_v)
        {
            ref StringsFlag v = ref _addr_v.val;

            return "<StringsFlag>";
        }

        // AddBuildFlagsNX adds the -n and -x build flags to the flag set.
        public static void AddBuildFlagsNX(ptr<flag.FlagSet> _addr_flags)
        {
            ref flag.FlagSet flags = ref _addr_flags.val;

            flags.BoolVar(_addr_cfg.BuildN, "n", false, "");
            flags.BoolVar(_addr_cfg.BuildX, "x", false, "");
        }

        // AddLoadFlags adds the -mod build flag to the flag set.
        public static void AddLoadFlags(ptr<flag.FlagSet> _addr_flags)
        {
            ref flag.FlagSet flags = ref _addr_flags.val;

            flags.StringVar(_addr_cfg.BuildMod, "mod", "", "");
        }
    }
}}}}
