// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package cmdflag handles flag processing common to several go tools.
// package cmdflag -- go2cs converted at 2020 August 29 10:01:39 UTC
// import "cmd/go/internal/cmdflag" ==> using cmdflag = go.cmd.go.@internal.cmdflag_package
// Original source: C:\Go\src\cmd\go\internal\cmdflag\flag.go
using flag = go.flag_package;
using fmt = go.fmt_package;
using os = go.os_package;
using strconv = go.strconv_package;
using strings = go.strings_package;

using @base = go.cmd.go.@internal.@base_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class cmdflag_package
    {
        // The flag handling part of go commands such as test is large and distracting.
        // We can't use the standard flag package because some of the flags from
        // our command line are for us, and some are for the binary we're running,
        // and some are for both.

        // Defn defines a flag we know about.
        public partial struct Defn
        {
            public @string Name; // Name on command line.
            public ptr<bool> BoolVar; // If it's a boolean flag, this points to it.
            public flag.Value Value; // The flag.Value represented.
            public bool PassToTest; // Pass to the test binary? Used only by go test.
            public bool Present; // Flag has been seen.
        }

        // IsBool reports whether v is a bool flag.
        public static bool IsBool(flag.Value v)
        {
            if (ok)
            {
                return vv.IsBoolFlag();
            }
            return false;
        }

        // SetBool sets the addressed boolean to the value.
        public static void SetBool(@string cmd, ref bool flag, @string value)
        {
            var (x, err) = strconv.ParseBool(value);
            if (err != null)
            {
                SyntaxError(cmd, "illegal bool flag value " + value);
            }
            flag.Value = x;
        }

        // SetInt sets the addressed integer to the value.
        public static void SetInt(@string cmd, ref long flag, @string value)
        {
            var (x, err) = strconv.Atoi(value);
            if (err != null)
            {
                SyntaxError(cmd, "illegal int flag value " + value);
            }
            flag.Value = x;
        }

        // SyntaxError reports an argument syntax error and exits the program.
        public static void SyntaxError(@string cmd, @string msg)
        {
            fmt.Fprintf(os.Stderr, "go %s: %s\n", cmd, msg);
            if (cmd == "test")
            {
                fmt.Fprintf(os.Stderr, "run \"go help %s\" or \"go help testflag\" for more information" + "\n", cmd);
            }
            else
            {
                fmt.Fprintf(os.Stderr, "run \"go help %s\" for more information" + "\n", cmd);
            }
            os.Exit(2L);
        }

        // Parse sees if argument i is present in the definitions and if so,
        // returns its definition, value, and whether it consumed an extra word.
        // If the flag begins (cmd+".") it is ignored for the purpose of this function.
        public static (ref Defn, @string, bool) Parse(@string cmd, slice<ref Defn> defns, slice<@string> args, long i)
        {
            var arg = args[i];
            if (strings.HasPrefix(arg, "--"))
            { // reduce two minuses to one
                arg = arg[1L..];
            }
            switch (arg)
            {
                case "-?": 

                case "-h": 

                case "-help": 
                    @base.Usage();
                    break;
            }
            if (arg == "" || arg[0L] != '-')
            {
                return;
            }
            var name = arg[1L..]; 
            // If there's already a prefix such as "test.", drop it for now.
            name = strings.TrimPrefix(name, cmd + ".");
            var equals = strings.Index(name, "=");
            if (equals >= 0L)
            {
                value = name[equals + 1L..];
                name = name[..equals];
            }
            foreach (var (_, __f) in defns)
            {
                f = __f;
                if (name == f.Name)
                { 
                    // Booleans are special because they have modes -x, -x=true, -x=false.
                    if (f.BoolVar != null || IsBool(f.Value))
                    {
                        if (equals < 0L)
                        { // Otherwise, it's been set and will be verified in SetBool.
                            value = "true";
                        }
                        else
                        { 
                            // verify it parses
                            SetBool(cmd, @new<bool>(), value);
                        }
                    }
                    else
                    { // Non-booleans must have a value.
                        extra = equals < 0L;
                        if (extra)
                        {
                            if (i + 1L >= len(args))
                            {
                                SyntaxError(cmd, "missing argument for flag " + f.Name);
                            }
                            value = args[i + 1L];
                        }
                    }
                    if (f.Present)
                    {
                        SyntaxError(cmd, f.Name + " flag may be set only once");
                    }
                    f.Present = true;
                    return;
                }
            }

            f = null;
            return;
        }
    }
}}}}
