// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package @base -- go2cs converted at 2022 March 06 23:19:42 UTC
// import "cmd/go/internal/base" ==> using @base = go.cmd.go.@internal.@base_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\base\flag.go
using flag = go.flag_package;

using cfg = go.cmd.go.@internal.cfg_package;
using fsys = go.cmd.go.@internal.fsys_package;
using str = go.cmd.go.@internal.str_package;

namespace go.cmd.go.@internal;

public static partial class @base_package {

    // A StringsFlag is a command-line flag that interprets its argument
    // as a space-separated list of possibly-quoted strings.
public partial struct StringsFlag { // : slice<@string>
}

private static error Set(this ptr<StringsFlag> _addr_v, @string s) {
    ref StringsFlag v = ref _addr_v.val;

    error err = default!;
    v.val, err = str.SplitQuotedFields(s);
    if (v == null.val) {
        v.val = new slice<@string>(new @string[] {  });
    }
    return error.As(err)!;

}

private static @string String(this ptr<StringsFlag> _addr_v) {
    ref StringsFlag v = ref _addr_v.val;

    return "<StringsFlag>";
}

// explicitStringFlag is like a regular string flag, but it also tracks whether
// the string was set explicitly to a non-empty value.
private partial struct explicitStringFlag {
    public ptr<@string> value;
    public ptr<bool> @explicit;
}

private static @string String(this explicitStringFlag f) {
    if (f.value == null) {
        return "";
    }
    return f.value.val;

}

private static error Set(this explicitStringFlag f, @string v) {
    f.value.val = v;
    if (v != "") {
        f.@explicit.val = true;
    }
    return error.As(null!)!;

}

// AddBuildFlagsNX adds the -n and -x build flags to the flag set.
public static void AddBuildFlagsNX(ptr<flag.FlagSet> _addr_flags) {
    ref flag.FlagSet flags = ref _addr_flags.val;

    flags.BoolVar(_addr_cfg.BuildN, "n", false, "");
    flags.BoolVar(_addr_cfg.BuildX, "x", false, "");
}

// AddModFlag adds the -mod build flag to the flag set.
public static void AddModFlag(ptr<flag.FlagSet> _addr_flags) {
    ref flag.FlagSet flags = ref _addr_flags.val;

    flags.Var(new explicitStringFlag(value:&cfg.BuildMod,explicit:&cfg.BuildModExplicit), "mod", "");
}

// AddModCommonFlags adds the module-related flags common to build commands
// and 'go mod' subcommands.
public static void AddModCommonFlags(ptr<flag.FlagSet> _addr_flags) {
    ref flag.FlagSet flags = ref _addr_flags.val;

    flags.BoolVar(_addr_cfg.ModCacheRW, "modcacherw", false, "");
    flags.StringVar(_addr_cfg.ModFile, "modfile", "", "");
    flags.StringVar(_addr_fsys.OverlayFile, "overlay", "", "");
}

} // end @base_package
