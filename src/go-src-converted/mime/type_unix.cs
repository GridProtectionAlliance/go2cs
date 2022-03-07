// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || darwin || dragonfly || freebsd || (js && wasm) || linux || netbsd || openbsd || solaris
// +build aix darwin dragonfly freebsd js,wasm linux netbsd openbsd solaris

// package mime -- go2cs converted at 2022 March 06 22:21:18 UTC
// import "mime" ==> using mime = go.mime_package
// Original source: C:\Program Files\Go\src\mime\type_unix.go
using bufio = go.bufio_package;
using os = go.os_package;
using strings = go.strings_package;

namespace go;

public static partial class mime_package {

private static void init() {
    osInitMime = initMimeUnix;
}

// See https://specifications.freedesktop.org/shared-mime-info-spec/shared-mime-info-spec-0.21.html
// for the FreeDesktop Shared MIME-info Database specification.
private static @string mimeGlobs = new slice<@string>(new @string[] { "/usr/local/share/mime/globs2", "/usr/share/mime/globs2" });

// Common locations for mime.types files on unix.
private static @string typeFiles = new slice<@string>(new @string[] { "/etc/mime.types", "/etc/apache2/mime.types", "/etc/apache/mime.types", "/etc/httpd/conf/mime.types" });

private static error loadMimeGlobsFile(@string filename) => func((defer, panic, _) => {
    var (f, err) = os.Open(filename);
    if (err != null) {
        return error.As(err)!;
    }
    defer(f.Close());

    var scanner = bufio.NewScanner(f);
    while (scanner.Scan()) { 
        // Each line should be of format: weight:mimetype:*.ext
        var fields = strings.Split(scanner.Text(), ":");
        if (len(fields) < 3 || len(fields[0]) < 1 || len(fields[2]) < 2) {
            continue;
        }
        else if (fields[0][0] == '#' || fields[2][0] != '*') {
            continue;
        }
        var extension = fields[2][(int)1..];
        {
            var (_, ok) = mimeTypes.Load(extension);

            if (ok) { 
                // We've already seen this extension.
                // The file is in weight order, so we keep
                // the first entry that we see.
                continue;

            }

        }


        setExtensionType(extension, fields[1]);

    }
    {
        var err = scanner.Err();

        if (err != null) {
            panic(err);
        }
    }

    return error.As(null!)!;

});

private static void loadMimeFile(@string filename) => func((defer, panic, _) => {
    var (f, err) = os.Open(filename);
    if (err != null) {
        return ;
    }
    defer(f.Close());

    var scanner = bufio.NewScanner(f);
    while (scanner.Scan()) {
        var fields = strings.Fields(scanner.Text());
        if (len(fields) <= 1 || fields[0][0] == '#') {
            continue;
        }
        var mimeType = fields[0];
        foreach (var (_, ext) in fields[(int)1..]) {
            if (ext[0] == '#') {
                break;
            }
            setExtensionType("." + ext, mimeType);
        }
    }
    {
        var err = scanner.Err();

        if (err != null) {
            panic(err);
        }
    }

});

private static void initMimeUnix() {
    {
        var filename__prev1 = filename;

        foreach (var (_, __filename) in mimeGlobs) {
            filename = __filename;
            {
                var err = loadMimeGlobsFile(filename);

                if (err == null) {
                    return ; // Stop checking more files if mimetype database is found.
                }

            }

        }
        filename = filename__prev1;
    }

    {
        var filename__prev1 = filename;

        foreach (var (_, __filename) in typeFiles) {
            filename = __filename;
            loadMimeFile(filename);
        }
        filename = filename__prev1;
    }
}

private static map<@string, @string> initMimeForTests() {
    mimeGlobs = new slice<@string>(new @string[] { "" });
    typeFiles = new slice<@string>(new @string[] { "testdata/test.types" });
    return /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, @string>{".T1":"application/test",".t2":"text/test; charset=utf-8",".png":"image/png",};
}

} // end mime_package
