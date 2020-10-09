// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2020 October 09 05:23:11 UTC
// Original source: C:\Go\src\cmd\buildid\buildid.go
using flag = go.flag_package;
using fmt = go.fmt_package;
using log = go.log_package;
using os = go.os_package;
using strings = go.strings_package;

using buildid = go.cmd.@internal.buildid_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static void usage()
        {
            fmt.Fprintf(os.Stderr, "usage: go tool buildid [-w] file\n");
            flag.PrintDefaults();
            os.Exit(2L);
        }

        private static var wflag = flag.Bool("w", false, "write build ID");

        // taken from cmd/go/internal/work/buildid.go
        private static @string hashToString(array<byte> h)
        {
            h = h.Clone();

            const @string b64 = (@string)"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_";

            const long chunks = (long)5L;

            array<byte> dst = new array<byte>(chunks * 4L);
            for (long i = 0L; i < chunks; i++)
            {
                var v = uint32(h[3L * i]) << (int)(16L) | uint32(h[3L * i + 1L]) << (int)(8L) | uint32(h[3L * i + 2L]);
                dst[4L * i + 0L] = b64[(v >> (int)(18L)) & 0x3FUL];
                dst[4L * i + 1L] = b64[(v >> (int)(12L)) & 0x3FUL];
                dst[4L * i + 2L] = b64[(v >> (int)(6L)) & 0x3FUL];
                dst[4L * i + 3L] = b64[v & 0x3FUL];
            }

            return string(dst[..]);

        }

        private static void Main()
        {
            log.SetPrefix("buildid: ");
            log.SetFlags(0L);
            flag.Usage = usage;
            flag.Parse();
            if (flag.NArg() != 1L)
            {
                usage();
            }

            var file = flag.Arg(0L);
            var (id, err) = buildid.ReadFile(file);
            if (err != null)
            {
                log.Fatal(err);
            }

            if (!wflag.val)
            {
                fmt.Printf("%s\n", id);
                return ;
            } 

            // Keep in sync with src/cmd/go/internal/work/buildid.go:updateBuildID
            var (f, err) = os.Open(file);
            if (err != null)
            {
                log.Fatal(err);
            }

            var (matches, hash, err) = buildid.FindAndHash(f, id, 0L);
            if (err != null)
            {
                log.Fatal(err);
            }

            f.Close();

            var newID = id[..strings.LastIndex(id, "/")] + "/" + hashToString(hash);
            if (len(newID) != len(id))
            {
                log.Fatalf("%s: build ID length mismatch %q vs %q", file, id, newID);
            }

            if (len(matches) == 0L)
            {
                return ;
            }

            f, err = os.OpenFile(file, os.O_WRONLY, 0L);
            if (err != null)
            {
                log.Fatal(err);
            }

            {
                var err__prev1 = err;

                var err = buildid.Rewrite(f, matches, newID);

                if (err != null)
                {
                    log.Fatal(err);
                }

                err = err__prev1;

            }

            {
                var err__prev1 = err;

                err = f.Close();

                if (err != null)
                {
                    log.Fatal(err);
                }

                err = err__prev1;

            }

        }
    }
}
