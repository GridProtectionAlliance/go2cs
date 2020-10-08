// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package mime -- go2cs converted at 2020 October 08 03:38:35 UTC
// import "mime" ==> using mime = go.mime_package
// Original source: C:\Go\src\mime\type_plan9.go
using bufio = go.bufio_package;
using os = go.os_package;
using strings = go.strings_package;
using static go.builtin;

namespace go
{
    public static partial class mime_package
    {
        private static void init()
        {
            osInitMime = initMimePlan9;
        }

        private static void initMimePlan9()
        {
            foreach (var (_, filename) in typeFiles)
            {
                loadMimeFile(filename);
            }

        }

        private static @string typeFiles = new slice<@string>(new @string[] { "/sys/lib/mimetype" });

        private static map<@string, @string> initMimeForTests()
        {
            typeFiles = new slice<@string>(new @string[] { "testdata/test.types.plan9" });
            return /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, @string>{".t1":"application/test",".t2":"text/test; charset=utf-8",".pNg":"image/png",};
        }

        private static void loadMimeFile(@string filename) => func((defer, panic, _) =>
        {
            var (f, err) = os.Open(filename);
            if (err != null)
            {
                return ;
            }

            defer(f.Close());

            var scanner = bufio.NewScanner(f);
            while (scanner.Scan())
            {
                var fields = strings.Fields(scanner.Text());
                if (len(fields) <= 2L || fields[0L][0L] != '.')
                {
                    continue;
                }

                if (fields[1L] == "-" || fields[2L] == "-")
                {
                    continue;
                }

                setExtensionType(fields[0L], fields[1L] + "/" + fields[2L]);

            }

            {
                var err = scanner.Err();

                if (err != null)
                {
                    panic(err);
                }

            }

        });
    }
}
