// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ld -- go2cs converted at 2020 October 09 05:52:37 UTC
// import "cmd/oldlink/internal/ld" ==> using ld = go.cmd.oldlink.@internal.ld_package
// Original source: C:\Go\src\cmd\oldlink\internal\ld\util.go
using sym = go.cmd.oldlink.@internal.sym_package;
using binary = go.encoding.binary_package;
using fmt = go.fmt_package;
using os = go.os_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace oldlink {
namespace @internal
{
    public static partial class ld_package
    {
        private static slice<Action> atExitFuncs = default;

        public static void AtExit(Action f)
        {
            atExitFuncs = append(atExitFuncs, f);
        }

        // runAtExitFuncs runs the queued set of AtExit functions.
        private static void runAtExitFuncs()
        {
            for (var i = len(atExitFuncs) - 1L; i >= 0L; i--)
            {
                atExitFuncs[i]();
            }

            atExitFuncs = null;

        }

        // Exit exits with code after executing all atExitFuncs.
        public static void Exit(long code)
        {
            runAtExitFuncs();
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
        public static void Errorf(ptr<sym.Symbol> _addr_s, @string format, params object[] args) => func((_, panic, __) =>
        {
            args = args.Clone();
            ref sym.Symbol s = ref _addr_s.val;

            if (s != null)
            {
                format = s.Name + ": " + format;
            }

            format += "\n";
            fmt.Fprintf(os.Stderr, format, args);
            nerrors++;
            if (flagH.val)
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

        // contains reports whether v is in s.
        private static bool contains(slice<@string> s, @string v)
        {
            foreach (var (_, x) in s)
            {
                if (x == v)
                {
                    return true;
                }

            }
            return false;

        }

        // implements sort.Interface, for sorting symbols by name.
        private partial struct byName // : slice<ptr<sym.Symbol>>
        {
        }

        private static long Len(this byName s)
        {
            return len(s);
        }
        private static void Swap(this byName s, long i, long j)
        {
            s[i] = s[j];
            s[j] = s[i];
        }
        private static bool Less(this byName s, long i, long j)
        {
            return s[i].Name < s[j].Name;
        }
    }
}}}}
