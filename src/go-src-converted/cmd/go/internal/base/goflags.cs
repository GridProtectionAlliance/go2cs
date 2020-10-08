// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package @base -- go2cs converted at 2020 October 08 04:36:56 UTC
// import "cmd/go/internal/base" ==> using @base = go.cmd.go.@internal.@base_package
// Original source: C:\Go\src\cmd\go\internal\base\goflags.go
using flag = go.flag_package;
using fmt = go.fmt_package;
using runtime = go.runtime_package;
using strings = go.strings_package;

using cfg = go.cmd.go.@internal.cfg_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class @base_package
    {
        private static slice<@string> goflags = default;        private static var knownFlag = make_map<@string, bool>();

        // AddKnownFlag adds name to the list of known flags for use in $GOFLAGS.
        public static void AddKnownFlag(@string name)
        {
            knownFlag[name] = true;
        }

        // GOFLAGS returns the flags from $GOFLAGS.
        // The list can be assumed to contain one string per flag,
        // with each string either beginning with -name or --name.
        public static slice<@string> GOFLAGS()
        {
            InitGOFLAGS();
            return goflags;
        }

        // InitGOFLAGS initializes the goflags list from $GOFLAGS.
        // If goflags is already initialized, it does nothing.
        public static void InitGOFLAGS()
        {
            if (goflags != null)
            { // already initialized
                return ;

            } 

            // Build list of all flags for all commands.
            // If no command has that flag, then we report the problem.
            // This catches typos while still letting users record flags in GOFLAGS
            // that only apply to a subset of go commands.
            // Commands using CustomFlags can report their flag names
            // by calling AddKnownFlag instead.
            Action<ptr<Command>> walkFlags = default;
            walkFlags = cmd =>
            {
                foreach (var (_, sub) in cmd.Commands)
                {
                    walkFlags(sub);
                }
                cmd.Flag.VisitAll(f =>
                {
                    knownFlag[f.Name] = true;
                });

            }
;
            walkFlags(Go); 

            // Ignore bad flag in go env and go bug, because
            // they are what people reach for when debugging
            // a problem, and maybe they're debugging GOFLAGS.
            // (Both will show the GOFLAGS setting if let succeed.)
            var hideErrors = cfg.CmdName == "env" || cfg.CmdName == "bug";

            goflags = strings.Fields(cfg.Getenv("GOFLAGS"));
            if (goflags == null)
            {
                goflags = new slice<@string>(new @string[] {  }); // avoid work on later InitGOFLAGS call
            } 

            // Each of the words returned by strings.Fields must be its own flag.
            // To set flag arguments use -x=value instead of -x value.
            // For boolean flags, -x is fine instead of -x=true.
            foreach (var (_, f) in goflags)
            { 
                // Check that every flag looks like -x --x -x=value or --x=value.
                if (!strings.HasPrefix(f, "-") || f == "-" || f == "--" || strings.HasPrefix(f, "---") || strings.HasPrefix(f, "-=") || strings.HasPrefix(f, "--="))
                {
                    if (hideErrors)
                    {
                        continue;
                    }

                    Fatalf("go: parsing $GOFLAGS: non-flag %q", f);

                }

                var name = f[1L..];
                if (name[0L] == '-')
                {
                    name = name[1L..];
                }

                {
                    var i = strings.Index(name, "=");

                    if (i >= 0L)
                    {
                        name = name[..i];
                    }

                }

                if (!knownFlag[name])
                {
                    if (hideErrors)
                    {
                        continue;
                    }

                    Fatalf("go: parsing $GOFLAGS: unknown flag -%s", name);

                }

            }

        }

        // boolFlag is the optional interface for flag.Value known to the flag package.
        // (It is not clear why package flag does not export this interface.)
        private partial interface boolFlag : flag.Value
        {
            bool IsBoolFlag();
        }

        // SetFromGOFLAGS sets the flags in the given flag set using settings in $GOFLAGS.
        public static void SetFromGOFLAGS(ptr<flag.FlagSet> _addr_flags)
        {
            ref flag.FlagSet flags = ref _addr_flags.val;

            InitGOFLAGS(); 

            // This loop is similar to flag.Parse except that it ignores
            // unknown flags found in goflags, so that setting, say, GOFLAGS=-ldflags=-w
            // does not break commands that don't have a -ldflags.
            // It also adjusts the output to be clear that the reported problem is from $GOFLAGS.
            @string where = "$GOFLAGS";
            if (runtime.GOOS == "windows")
            {
                where = "%GOFLAGS%";
            }

            foreach (var (_, goflag) in goflags)
            {
                var name = goflag;
                @string value = "";
                var hasValue = false;
                {
                    var i = strings.Index(goflag, "=");

                    if (i >= 0L)
                    {
                        name = goflag[..i];
                        value = goflag[i + 1L..];
                        hasValue = true;

                    }

                }

                if (strings.HasPrefix(name, "--"))
                {
                    name = name[1L..];
                }

                var f = flags.Lookup(name[1L..]);
                if (f == null)
                {
                    continue;
                } 

                // Use flags.Set consistently (instead of f.Value.Set) so that a subsequent
                // call to flags.Visit will correctly visit the flags that have been set.
                {
                    boolFlag (fb, ok) = boolFlag.As(f.Value._<boolFlag>())!;

                    if (ok && fb.IsBoolFlag())
                    {
                        if (hasValue)
                        {
                            {
                                var err__prev3 = err;

                                var err = flags.Set(f.Name, value);

                                if (err != null)
                                {
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

                                if (err != null)
                                {
                                    fmt.Fprintf(flags.Output(), "go: invalid boolean flag %s (from %s): %v\n", name, where, err);
                                    flags.Usage();
                                }

                                err = err__prev3;

                            }

                        }

                    }
                    else
                    {
                        if (!hasValue)
                        {
                            fmt.Fprintf(flags.Output(), "go: flag needs an argument: %s (from %s)\n", name, where);
                            flags.Usage();
                        }

                        {
                            var err__prev2 = err;

                            err = flags.Set(f.Name, value);

                            if (err != null)
                            {
                                fmt.Fprintf(flags.Output(), "go: invalid value %q for flag %s (from %s): %v\n", value, name, where, err);
                                flags.Usage();
                            }

                            err = err__prev2;

                        }

                    }

                }

            }

        }
    }
}}}}
