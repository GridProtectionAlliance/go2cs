// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2020 October 08 04:39:55 UTC
// Original source: C:\Go\src\cmd\nm\nm.go
using bufio = go.bufio_package;
using flag = go.flag_package;
using fmt = go.fmt_package;
using log = go.log_package;
using os = go.os_package;
using sort = go.sort_package;

using objfile = go.cmd.@internal.objfile_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class main_package
    {
        private static readonly @string helpText = (@string)@"usage: go tool nm [options] file...
  -n
      an alias for -sort address (numeric),
      for compatibility with other nm commands
  -size
      print symbol size in decimal between address and type
  -sort {address,name,none,size}
      sort output in the given order (default name)
      size orders from largest to smallest
  -type
      print symbol type after name
";



        private static void usage()
        {
            fmt.Fprintf(os.Stderr, helpText);
            os.Exit(2L);
        }

        private static var sortOrder = flag.String("sort", "name", "");        private static var printSize = flag.Bool("size", false, "");        private static var printType = flag.Bool("type", false, "");        private static var filePrefix = false;

        private static void init()
        {
            flag.Var(nflag(0L), "n", ""); // alias for -sort address
        }

        private partial struct nflag // : long
        {
        }

        private static bool IsBoolFlag(this nflag _p0)
        {
            return true;
        }

        private static error Set(this nflag _p0, @string value)
        {
            if (value == "true")
            {
                sortOrder.val = "address";
            }

            return error.As(null!)!;

        }

        private static @string String(this nflag _p0)
        {
            if (sortOrder == "address".val)
            {
                return "true";
            }

            return "false";

        }

        private static void Main()
        {
            log.SetFlags(0L);
            flag.Usage = usage;
            flag.Parse();

            switch (sortOrder.val)
            {
                case "address": 

                case "name": 

                case "none": 

                case "size": 
                    break;
                default: 
                    fmt.Fprintf(os.Stderr, "nm: unknown sort order %q\n", sortOrder.val);
                    os.Exit(2L);
                    break;
            }

            var args = flag.Args();
            filePrefix = len(args) > 1L;
            if (len(args) == 0L)
            {
                flag.Usage();
            }

            foreach (var (_, file) in args)
            {
                nm(file);
            }
            os.Exit(exitCode);

        }

        private static long exitCode = 0L;

        private static void errorf(@string format, params object[] args)
        {
            args = args.Clone();

            log.Printf(format, args);
            exitCode = 1L;
        }

        private static void nm(@string file) => func((defer, _, __) =>
        {
            var (f, err) = objfile.Open(file);
            if (err != null)
            {
                errorf("%v", err);
                return ;
            }

            defer(f.Close());

            var w = bufio.NewWriter(os.Stdout);

            var entries = f.Entries();

            bool found = default;

            foreach (var (_, e) in entries)
            {
                var (syms, err) = e.Symbols();
                if (err != null)
                {
                    errorf("reading %s: %v", file, err);
                }

                if (len(syms) == 0L)
                {
                    continue;
                }

                found = true;

                switch (sortOrder.val)
                {
                    case "address": 
                        sort.Slice(syms, (i, j) => syms[i].Addr < syms[j].Addr);
                        break;
                    case "name": 
                        sort.Slice(syms, (i, j) => syms[i].Name < syms[j].Name);
                        break;
                    case "size": 
                        sort.Slice(syms, (i, j) => syms[i].Size > syms[j].Size);
                        break;
                }

                foreach (var (_, sym) in syms)
                {
                    if (len(entries) > 1L)
                    {
                        var name = e.Name();
                        if (name == "")
                        {
                            fmt.Fprintf(w, "%s(%s):\t", file, "_go_.o");
                        }
                        else
                        {
                            fmt.Fprintf(w, "%s(%s):\t", file, name);
                        }

                    }
                    else if (filePrefix)
                    {
                        fmt.Fprintf(w, "%s:\t", file);
                    }

                    if (sym.Code == 'U')
                    {
                        fmt.Fprintf(w, "%8s", "");
                    }
                    else
                    {
                        fmt.Fprintf(w, "%8x", sym.Addr);
                    }

                    if (printSize.val)
                    {
                        fmt.Fprintf(w, " %10d", sym.Size);
                    }

                    fmt.Fprintf(w, " %c %s", sym.Code, sym.Name);
                    if (printType && sym.Type != "".val)
                    {
                        fmt.Fprintf(w, " %s", sym.Type);
                    }

                    fmt.Fprintf(w, "\n");

                }

            }
            if (!found)
            {
                errorf("reading %s: no symbols", file);
            }

            w.Flush();

        });
    }
}
