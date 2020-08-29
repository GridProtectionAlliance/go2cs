// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2020 August 29 08:24:29 UTC
// Original source: C:\Go\src\runtime\testdata\testprog\memprof.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using ioutil = go.io.ioutil_package;
using os = go.os_package;
using runtime = go.runtime_package;
using pprof = go.runtime.pprof_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static void init()
        {
            register("MemProf", MemProf);
        }

        private static bytes.Buffer memProfBuf = default;
        private static @string memProfStr = default;

        public static void MemProf()
        {
            for (long i = 0L; i < 1000L; i++)
            {
                fmt.Fprintf(ref memProfBuf, "%*d\n", i, i);
            }

            memProfStr = memProfBuf.String();

            runtime.GC();

            var (f, err) = ioutil.TempFile("", "memprof");
            if (err != null)
            {
                fmt.Fprintln(os.Stderr, err);
                os.Exit(2L);
            }
            {
                var err__prev1 = err;

                var err = pprof.WriteHeapProfile(f);

                if (err != null)
                {
                    fmt.Fprintln(os.Stderr, err);
                    os.Exit(2L);
                }

                err = err__prev1;

            }

            var name = f.Name();
            {
                var err__prev1 = err;

                err = f.Close();

                if (err != null)
                {
                    fmt.Fprintln(os.Stderr, err);
                    os.Exit(2L);
                }

                err = err__prev1;

            }

            fmt.Println(name);
        }
    }
}
