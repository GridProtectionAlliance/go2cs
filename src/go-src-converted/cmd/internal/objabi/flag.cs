// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package objabi -- go2cs converted at 2020 August 29 08:46:19 UTC
// import "cmd/internal/objabi" ==> using objabi = go.cmd.@internal.objabi_package
// Original source: C:\Go\src\cmd\internal\objabi\flag.go
using flag = go.flag_package;
using fmt = go.fmt_package;
using os = go.os_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace @internal
{
    public static partial class objabi_package
    {
        public static void Flagcount(@string name, @string usage, ref long val)
        {
            flag.Var((count.Value)(val), name, usage);
        }

        public static void Flagfn1(@string name, @string usage, Action<@string> f)
        {
            flag.Var(fn1(f), name, usage);
        }

        public static void Flagprint(long fd)
        {
            if (fd == 1L)
            {
                flag.CommandLine.SetOutput(os.Stdout);
            }
            flag.PrintDefaults();
        }

        public static void Flagparse(Action usage)
        {
            flag.Usage = usage;
            flag.Parse();
        }

        public static void AddVersionFlag()
        {
            flag.Var(new versionFlag(), "V", "print version and exit");
        }

        private static @string buildID = default; // filled in by linker

        private partial struct versionFlag
        {
        }

        private static bool IsBoolFlag(this versionFlag _p0)
        {
            return true;
        }
        private static void Get(this versionFlag _p0)
        {
            return null;
        }
        private static @string String(this versionFlag _p0)
        {
            return "";
        }
        private static error Set(this versionFlag _p0, @string s)
        {
            var name = os.Args[0L];
            name = name[strings.LastIndex(name, "/") + 1L..];
            name = name[strings.LastIndex(name, "\\") + 1L..];
            name = strings.TrimSuffix(name, ".exe");
            var p = Expstring();
            if (p == DefaultExpstring())
            {
                p = "";
            }
            @string sep = "";
            if (p != "")
            {
                sep = " ";
            } 

            // The go command invokes -V=full to get a unique identifier
            // for this tool. It is assumed that the release version is sufficient
            // for releases, but during development we include the full
            // build ID of the binary, so that if the compiler is changed and
            // rebuilt, we notice and rebuild all packages.
            if (s == "full" && strings.HasPrefix(Version, "devel"))
            {
                p += " buildID=" + buildID;
            }
            fmt.Printf("%s version %s%s%s\n", name, Version, sep, p);
            os.Exit(0L);
            return error.As(null);
        }

        // count is a flag.Value that is like a flag.Bool and a flag.Int.
        // If used as -name, it increments the count, but -name=x sets the count.
        // Used for verbose flag -v.
        private partial struct count // : long
        {
        }

        private static @string String(this ref count c)
        {
            return fmt.Sprint(int(c.Value));
        }

        private static error Set(this ref count c, @string s)
        {
            switch (s)
            {
                case "true": 
                    c.Value++;
                    break;
                case "false": 
                    c.Value = 0L;
                    break;
                default: 
                    var (n, err) = strconv.Atoi(s);
                    if (err != null)
                    {
                        return error.As(fmt.Errorf("invalid count %q", s));
                    }
                    c.Value = count(n);
                    break;
            }
            return error.As(null);
        }

        private static void Get(this ref count c)
        {
            return int(c.Value);
        }

        private static bool IsBoolFlag(this ref count c)
        {
            return true;
        }

        private static bool IsCountFlag(this ref count c)
        {
            return true;
        }

        public delegate void fn0();

        private static error Set(this fn0 f, @string s)
        {
            f();
            return error.As(null);
        }

        private static void Get(this fn0 f)
        {
            return null;
        }

        private static @string String(this fn0 f)
        {
            return "";
        }

        private static bool IsBoolFlag(this fn0 f)
        {
            return true;
        }

        public delegate void fn1(@string);

        private static error Set(this fn1 f, @string s)
        {
            f(s);
            return error.As(null);
        }

        private static @string String(this fn1 f)
        {
            return "";
        }
    }
}}}
