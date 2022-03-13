// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package cmdflag handles flag processing common to several go tools.

// package cmdflag -- go2cs converted at 2022 March 13 06:31:07 UTC
// import "cmd/go/internal/cmdflag" ==> using cmdflag = go.cmd.go.@internal.cmdflag_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\cmdflag\flag.go
namespace go.cmd.go.@internal;

using errors = errors_package;
using flag = flag_package;
using fmt = fmt_package;
using strings = strings_package;


// The flag handling part of go commands such as test is large and distracting.
// We can't use the standard flag package because some of the flags from
// our command line are for us, and some are for the binary we're running,
// and some are for both.

// ErrFlagTerminator indicates the distinguished token "--", which causes the
// flag package to treat all subsequent arguments as non-flags.

using System;
public static partial class cmdflag_package {

public static var ErrFlagTerminator = errors.New("flag terminator");

// A FlagNotDefinedError indicates a flag-like argument that does not correspond
// to any registered flag in a FlagSet.
public partial struct FlagNotDefinedError {
    public @string RawArg; // the original argument, like --foo or -foo=value
    public @string Name;
    public bool HasValue; // is this the -foo=value or --foo=value form?
    public @string Value; // only provided if HasValue is true
}

public static @string Error(this FlagNotDefinedError e) {
    return fmt.Sprintf("flag provided but not defined: -%s", e.Name);
}

// A NonFlagError indicates an argument that is not a syntactically-valid flag.
public partial struct NonFlagError {
    public @string RawArg;
}

public static @string Error(this NonFlagError e) {
    return fmt.Sprintf("not a flag: %q", e.RawArg);
}

// ParseOne sees if args[0] is present in the given flag set and if so,
// sets its value and returns the flag along with the remaining (unused) arguments.
//
// ParseOne always returns either a non-nil Flag or a non-nil error,
// and always consumes at least one argument (even on error).
//
// Unlike (*flag.FlagSet).Parse, ParseOne does not log its own errors.
public static (ptr<flag.Flag>, slice<@string>, error) ParseOne(ptr<flag.FlagSet> _addr_fs, slice<@string> args) {
    ptr<flag.Flag> f = default!;
    slice<@string> remainingArgs = default;
    error err = default!;
    ref flag.FlagSet fs = ref _addr_fs.val;
 
    // This function is loosely derived from (*flag.FlagSet).parseOne.

    var raw = args[0];
    var args = args[(int)1..];
    var arg = raw;
    if (strings.HasPrefix(arg, "--")) {
        if (arg == "--") {
            return (_addr_null!, args, error.As(ErrFlagTerminator)!);
        }
        arg = arg[(int)1..]; // reduce two minuses to one
    }
    switch (arg) {
        case "-?": 

        case "-h": 

        case "-help": 
            return (_addr_null!, args, error.As(flag.ErrHelp)!);
            break;
    }
    if (len(arg) < 2 || arg[0] != '-' || arg[1] == '-' || arg[1] == '=') {
        return (_addr_null!, args, error.As(new NonFlagError(RawArg:raw))!);
    }
    var name = arg[(int)1..];
    var hasValue = false;
    @string value = "";
    {
        var i = strings.Index(name, "=");

        if (i >= 0) {
            value = name[(int)i + 1..];
            hasValue = true;
            name = name[(int)0..(int)i];
        }
    }

    f = fs.Lookup(name);
    if (f == null) {
        return (_addr_null!, args, error.As(new FlagNotDefinedError(RawArg:raw,Name:name,HasValue:hasValue,Value:value,))!);
    }
    Func<@string, object[], (ptr<flag.Flag>, slice<@string>, error)> failf = (format, a) => (_addr_f!, args, error.As(fmt.Errorf(format, a))!);

    {
        boolFlag (fv, ok) = boolFlag.As(f.Value._<boolFlag>())!;

        if (ok && fv.IsBoolFlag()) { // special case: doesn't need an arg
            if (hasValue) {
                {
                    var err__prev3 = err;

                    var err = fs.Set(name, value);

                    if (err != null) {
                        return _addr_failf("invalid boolean value %q for -%s: %v", value, name, err)!;
                    }

                    err = err__prev3;

                }
            }
            else
 {
                {
                    var err__prev3 = err;

                    err = fs.Set(name, "true");

                    if (err != null) {
                        return _addr_failf("invalid boolean flag %s: %v", name, err)!;
                    }

                    err = err__prev3;

                }
            }
        }
        else
 { 
            // It must have a value, which might be the next argument.
            if (!hasValue && len(args) > 0) { 
                // value is the next arg
                hasValue = true;
                (value, args) = (args[0], args[(int)1..]);
            }
            if (!hasValue) {
                return _addr_failf("flag needs an argument: -%s", name)!;
            }
            {
                var err__prev2 = err;

                err = fs.Set(name, value);

                if (err != null) {
                    return _addr_failf("invalid value %q for flag -%s: %v", value, name, err)!;
                }

                err = err__prev2;

            }
        }
    }

    return (_addr_f!, args, error.As(null!)!);
}

private partial interface boolFlag {
    bool IsBoolFlag();
}

} // end cmdflag_package
