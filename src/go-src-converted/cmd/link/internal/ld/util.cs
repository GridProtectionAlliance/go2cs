// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ld -- go2cs converted at 2020 August 29 10:04:41 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Go\src\cmd\link\internal\ld\util.go
using sym = go.cmd.link.@internal.sym_package;
using binary = go.encoding.binary_package;
using fmt = go.fmt_package;
using os = go.os_package;
using strings = go.strings_package;
using time = go.time_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace link {
namespace @internal
{
    public static partial class ld_package
    {
        private static time.Time startTime = default;

        // TODO(josharian): delete. See issue 19865.
        public static double Cputime()
        {
            if (startTime.IsZero())
            {
                startTime = time.Now();
            }
            return time.Since(startTime).Seconds();
        }

        private static slice<@string> tokenize(@string s)
        {
            slice<@string> f = default;
            while (true)
            {
                s = strings.TrimLeft(s, " \t\r\n");
                if (s == "")
                {
                    break;
                }
                var quote = false;
                long i = 0L;
                while (i < len(s))
                {
                    if (s[i] == '\'')
                    {
                        if (quote && i + 1L < len(s) && s[i + 1L] == '\'')
                        {
                            i++;
                            continue;
                    i++;
                        }
                        quote = !quote;
                    }
                    if (!quote && (s[i] == ' ' || s[i] == '\t' || s[i] == '\r' || s[i] == '\n'))
                    {
                        break;
                    }
                }

                var next = s[..i];
                s = s[i..];
                if (strings.Contains(next, "'"))
                {
                    slice<byte> buf = default;
                    quote = false;
                    {
                        long i__prev2 = i;

                        for (i = 0L; i < len(next); i++)
                        {
                            if (next[i] == '\'')
                            {
                                if (quote && i + 1L < len(next) && next[i + 1L] == '\'')
                                {
                                    i++;
                                    buf = append(buf, '\'');
                                }
                                quote = !quote;
                                continue;
                            }
                            buf = append(buf, next[i]);
                        }


                        i = i__prev2;
                    }
                    next = string(buf);
                }
                f = append(f, next);
            }

            return f;
        }

        private static slice<Action> atExitFuncs = default;

        public static void AtExit(Action f)
        {
            atExitFuncs = append(atExitFuncs, f);
        }

        // Exit exits with code after executing all atExitFuncs.
        public static void Exit(long code)
        {
            for (var i = len(atExitFuncs) - 1L; i >= 0L; i--)
            {
                atExitFuncs[i]();
            }

            os.Exit(code);
        }

        // Exitf logs an error message then calls Exit(2).
        public static void Exitf(@string format, params object[] a)
        {
            a = a.Clone();

            fmt.Fprintf(os.Stderr, os.Args[0L] + ": " + format + "\n", a);
            nerrors++;
            Exit(2L);
        }

        // Errorf logs an error message.
        //
        // If more than 20 errors have been printed, exit with an error.
        //
        // Logging an error means that on exit cmd/link will delete any
        // output file and return a non-zero error code.
        public static void Errorf(ref sym.Symbol _s, @string format, params object[] args) => func(_s, (ref sym.Symbol s, Defer _, Panic panic, Recover __) =>
        {
            args = args.Clone();

            if (s != null)
            {
                format = s.Name + ": " + format;
            }
            format += "\n";
            fmt.Fprintf(os.Stderr, format, args);
            nerrors++;
            if (flagH.Value)
            {
                panic("error");
            }
            if (nerrors > 20L)
            {
                Exitf("too many errors");
            }
        });

        private static @string artrim(slice<byte> x)
        {
            long i = 0L;
            var j = len(x);
            while (i < len(x) && x[i] == ' ')
            {
                i++;
            }

            while (j > i && x[j - 1L] == ' ')
            {
                j--;
            }

            return string(x[i..j]);
        }

        private static void stringtouint32(slice<uint> x, @string s)
        {
            for (long i = 0L; len(s) > 0L; i++)
            {
                array<byte> buf = new array<byte>(4L);
                s = s[copy(buf[..], s)..];
                x[i] = binary.LittleEndian.Uint32(buf[..]);
            }

        }

        private static var start = time.Now();

        private static double elapsed()
        {
            return time.Since(start).Seconds();
        }
    }
}}}}
