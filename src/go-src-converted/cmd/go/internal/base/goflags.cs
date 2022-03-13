// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package @base -- go2cs converted at 2022 March 13 06:32:31 UTC
// import "cmd/go/internal/base" ==> using @base = go.cmd.go.@internal.@base_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\base\goflags.go
namespace go.cmd.go.@internal;

using flag = flag_package;
using fmt = fmt_package;
using runtime = runtime_package;
using strings = strings_package;

using cfg = cmd.go.@internal.cfg_package;

public static partial class @base_package {

private static slice<@string> goflags = default; // cached $GOFLAGS list; can be -x or --x form

// GOFLAGS returns the flags from $GOFLAGS.
// The list can be assumed to contain one string per flag,
// with each string either beginning with -name or --name.
public static slice<@string> GOFLAGS() {
    InitGOFLAGS();
    return goflags;
}

// InitGOFLAGS initializes the goflags list from $GOFLAGS.
// If goflags is already initialized, it does nothing.
public static void InitGOFLAGS() {
    if (goflags != null) { // already initialized
        return ;
    }
    goflags = strings.Fields(cfg.Getenv("GOFLAGS"));
    if (len(goflags) == 0) { 
        // nothing to do; avoid work on later InitGOFLAGS call
        goflags = new slice<@string>(new @string[] {  });
        return ;
    }
    var hideErrors = cfg.CmdName == "env" || cfg.CmdName == "bug"; 

    // Each of the words returned by strings.Fields must be its own flag.
    // To set flag arguments use -x=value instead of -x value.
    // For boolean flags, -x is fine instead of -x=true.
    foreach (var (_, f) in goflags) { 
        // Check that every flag looks like -x --x -x=value or --x=value.
        if (!strings.HasPrefix(f, "-") || f == "-" || f == "--" || strings.HasPrefix(f, "---") || strings.HasPrefix(f, "-=") || strings.HasPrefix(f, "--=")) {
            if (hideErrors) {
                continue;
            }
            Fatalf("go: parsing $GOFLAGS: non-flag %q", f);
        }
        var name = f[(int)1..];
        if (name[0] == '-') {
            name = name[(int)1..];
        }
        {
            var i = strings.Index(name, "=");

            if (i >= 0) {
                name = name[..(int)i];
            }

        }
        if (!hasFlag(Go, name)) {
            if (hideErrors) {
                continue;
            }
            Fatalf("go: parsing $GOFLAGS: unknown flag -%s", name);
        }
    }
}

// boolFlag is the optional interface for flag.Value known to the flag package.
// (It is not clear why package flag does not export this interface.)
private partial interface boolFlag {
    bool IsBoolFlag();
}

// SetFromGOFLAGS sets the flags in the given flag set using settings in $GOFLAGS.
public static void SetFromGOFLAGS(ptr<flag.FlagSet> _addr_flags) {
    ref flag.FlagSet flags = ref _addr_flags.val;

    InitGOFLAGS(); 

    // This loop is similar to flag.Parse except that it ignores
    // unknown flags found in goflags, so that setting, say, GOFLAGS=-ldflags=-w
    // does not break commands that don't have a -ldflags.
    // It also adjusts the output to be clear that the reported problem is from $GOFLAGS.
    @string where = "$GOFLAGS";
    if (runtime.GOOS == "windows") {
        where = "%GOFLAGS%";
    }
    foreach (var (_, goflag) in goflags) {
        var name = goflag;
        @string value = "";
        var hasValue = false; 
        // Ignore invalid flags like '=' or '=value'.
        // If it is not reported in InitGOFlags it means we don't want to report it.
        {
            var i = strings.Index(goflag, "=");

            if (i == 0) {
                continue;
            }
            else if (i > 0) {
                (name, value, hasValue) = (goflag[..(int)i], goflag[(int)i + 1..], true);
            }

        }
        if (strings.HasPrefix(name, "--")) {
            name = name[(int)1..];
        }
        var f = flags.Lookup(name[(int)1..]);
        if (f == null) {
            continue;
        }
        {
            boolFlag (fb, ok) = boolFlag.As(f.Value._<boolFlag>())!;

            if (ok && fb.IsBoolFlag()) {
                if (hasValue) {
                    {
                        var err__prev3 = err;

                        var err = flags.Set(f.Name, value);

                        if (err != null) {
                            fmt.Fprintf(flags.Output(), "go: invalid boolean value %q for flag %s (from %s): %v\n", value, name, where, err);
                            flags.Usage();
                        }

                        err = err__prev3;

                    }
                }
                else
 {
                    {
                        var err__prev3 = err;

                        err = flags.Set(f.Name, "true");

                        if (err != null) {
                            fmt.Fprintf(flags.Output(), "go: invalid boolean flag %s (from %s): %v\n", name, where, err);
                            flags.Usage();
                        }

                        err = err__prev3;

                    }
                }
            }
            else
 {
                if (!hasValue) {
                    fmt.Fprintf(flags.Output(), "go: flag needs an argument: %s (from %s)\n", name, where);
                    flags.Usage();
                }
                {
                    var err__prev2 = err;

                    err = flags.Set(f.Name, value);

                    if (err != null) {
                        fmt.Fprintf(flags.Output(), "go: invalid value %q for flag %s (from %s): %v\n", value, name, where, err);
                        flags.Usage();
                    }

                    err = err__prev2;

                }
            }

        }
    }
}

// InGOFLAGS returns whether GOFLAGS contains the given flag, such as "-mod".
public static bool InGOFLAGS(@string flag) {
    foreach (var (_, goflag) in GOFLAGS()) {
        var name = goflag;
        if (strings.HasPrefix(name, "--")) {
            name = name[(int)1..];
        }
        {
            var i = strings.Index(name, "=");

            if (i >= 0) {
                name = name[..(int)i];
            }

        }
        if (name == flag) {
            return true;
        }
    }    return false;
}

} // end @base_package
