// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build aix darwin dragonfly freebsd js,wasm linux netbsd openbsd solaris

// package mime -- go2cs converted at 2020 October 09 04:56:15 UTC
// import "mime" ==> using mime = go.mime_package
// Original source: C:\Go\src\mime\type_unix.go
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
            osInitMime = initMimeUnix;
        }

        private static @string typeFiles = new slice<@string>(new @string[] { "/etc/mime.types", "/etc/apache2/mime.types", "/etc/apache/mime.types" });

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
                if (len(fields) <= 1L || fields[0L][0L] == '#')
                {
                    continue;
                }

                var mimeType = fields[0L];
                foreach (var (_, ext) in fields[1L..])
                {
                    if (ext[0L] == '#')
                    {
                        break;
                    }

                    setExtensionType("." + ext, mimeType);

                }

            }

            {
                var err = scanner.Err();

                if (err != null)
                {
                    panic(err);
                }

            }

        });

        private static void initMimeUnix()
        {
            foreach (var (_, filename) in typeFiles)
            {
                loadMimeFile(filename);
            }

        }

        private static map<@string, @string> initMimeForTests()
        {
            typeFiles = new slice<@string>(new @string[] { "testdata/test.types" });
            return /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, @string>{".T1":"application/test",".t2":"text/test; charset=utf-8",".png":"image/png",};
        }
    }
}
