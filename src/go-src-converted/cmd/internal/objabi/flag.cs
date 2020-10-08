// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package objabi -- go2cs converted at 2020 October 08 03:50:14 UTC
// import "cmd/internal/objabi" ==> using objabi = go.cmd.@internal.objabi_package
// Original source: C:\Go\src\cmd\internal\objabi\flag.go
using flag = go.flag_package;
using fmt = go.fmt_package;
using io = go.io_package;
using ioutil = go.io.ioutil_package;
using log = go.log_package;
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
        public static void Flagcount(@string name, @string usage, ptr<long> _addr_val)
        {
            ref long val = ref _addr_val.val;

            flag.Var((count.val)(val), name, usage);
        }

        public static void Flagfn1(@string name, @string usage, Action<@string> f)
        {
            flag.Var(fn1(f), name, usage);
        }

        public static void Flagprint(io.Writer w)
        {
            flag.CommandLine.SetOutput(w);
            flag.PrintDefaults();
        }

        public static void Flagparse(Action usage)
        {
            flag.Usage = usage;
            os.Args = expandArgs(os.Args);
            flag.Parse();
        }

        // expandArgs expands "response files" arguments in the provided slice.
        //
        // A "response file" argument starts with '@' and the rest of that
        // argument is a filename with CR-or-CRLF-separated arguments. Each
        // argument in the named files can also contain response file
        // arguments. See Issue 18468.
        //
        // The returned slice 'out' aliases 'in' iff the input did not contain
        // any response file arguments.
        //
        // TODO: handle relative paths of recursive expansions in different directories?
        // Is there a spec for this? Are relative paths allowed?
        private static slice<@string> expandArgs(slice<@string> @in)
        {
            slice<@string> @out = default;
 
            // out is nil until we see a "@" argument.
            foreach (var (i, s) in in)
            {
                if (strings.HasPrefix(s, "@"))
                {
                    if (out == null)
                    {
                        out = make_slice<@string>(0L, len(in) * 2L);
                        out = append(out, in[..i]);
                    }

                    var (slurp, err) = ioutil.ReadFile(s[1L..]);
                    if (err != null)
                    {
                        log.Fatal(err);
                    }

                    var args = strings.Split(strings.TrimSpace(strings.Replace(string(slurp), "\r", "", -1L)), "\n");
                    out = append(out, expandArgs(args));

                }
                else if (out != null)
                {
                    out = append(out, s);
                }

            }
            if (out == null)
            {
                return in;
            }

            return ;

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

            // If there's an active experiment, include that,
            // to distinguish go1.10.2 with an experiment
            // from go1.10.2 without an experiment.
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
            if (s == "full")
            {
                if (strings.HasPrefix(Version, "devel"))
                {
                    p += " buildID=" + buildID;
                }

            }

            fmt.Printf("%s version %s%s%s\n", name, Version, sep, p);
            os.Exit(0L);
            return error.As(null!)!;

        }

        // count is a flag.Value that is like a flag.Bool and a flag.Int.
        // If used as -name, it increments the count, but -name=x sets the count.
        // Used for verbose flag -v.
        private partial struct count // : long
        {
        }

        private static @string String(this ptr<count> _addr_c)
        {
            ref count c = ref _addr_c.val;

            return fmt.Sprint(int(c.val));
        }

        private static error Set(this ptr<count> _addr_c, @string s)
        {
            ref count c = ref _addr_c.val;

            switch (s)
            {
                case "true": 
                    c.val++;
                    break;
                case "false": 
                    c.val = 0L;
                    break;
                default: 
                    var (n, err) = strconv.Atoi(s);
                    if (err != null)
                    {
                        return error.As(fmt.Errorf("invalid count %q", s))!;
                    }

                    c.val = count(n);
                    break;
            }
            return error.As(null!)!;

        }

        private static void Get(this ptr<count> _addr_c)
        {
            ref count c = ref _addr_c.val;

            return int(c.val);
        }

        private static bool IsBoolFlag(this ptr<count> _addr_c)
        {
            ref count c = ref _addr_c.val;

            return true;
        }

        private static bool IsCountFlag(this ptr<count> _addr_c)
        {
            ref count c = ref _addr_c.val;

            return true;
        }

        public delegate void fn1(@string);

        private static error Set(this fn1 f, @string s)
        {
            f(s);
            return error.As(null!)!;
        }

        private static @string String(this fn1 f)
        {
            return "";
        }
    }
}}}
