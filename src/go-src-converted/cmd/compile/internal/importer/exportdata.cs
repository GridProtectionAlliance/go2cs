// UNREVIEWED
// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements FindExportData.

// package importer -- go2cs converted at 2022 March 13 06:27:18 UTC
// import "cmd/compile/internal/importer" ==> using importer = go.cmd.compile.@internal.importer_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\importer\exportdata.go
namespace go.cmd.compile.@internal;

using bufio = bufio_package;
using fmt = fmt_package;
using io = io_package;
using strconv = strconv_package;
using strings = strings_package;

public static partial class importer_package {

private static (@string, nint, error) readGopackHeader(ptr<bufio.Reader> _addr_r) {
    @string name = default;
    nint size = default;
    error err = default!;
    ref bufio.Reader r = ref _addr_r.val;
 
    // See $GOROOT/include/ar.h.
    var hdr = make_slice<byte>(16 + 12 + 6 + 6 + 8 + 10 + 2);
    _, err = io.ReadFull(r, hdr);
    if (err != null) {
        return ;
    }
    if (false) {
        fmt.Printf("header: %s", hdr);
    }
    var s = strings.TrimSpace(string(hdr[(int)16 + 12 + 6 + 6 + 8..][..(int)10]));
    size, err = strconv.Atoi(s);
    if (err != null || hdr[len(hdr) - 2] != '`' || hdr[len(hdr) - 1] != '\n') {
        err = fmt.Errorf("invalid archive header");
        return ;
    }
    name = strings.TrimSpace(string(hdr[..(int)16]));
    return ;
}

// FindExportData positions the reader r at the beginning of the
// export data section of an underlying GC-created object/archive
// file by reading from it. The reader must be positioned at the
// start of the file before calling this function. The hdr result
// is the string before the export data, either "$$" or "$$B".
//
public static (@string, error) FindExportData(ptr<bufio.Reader> _addr_r) {
    @string hdr = default;
    error err = default!;
    ref bufio.Reader r = ref _addr_r.val;
 
    // Read first line to make sure this is an object file.
    var (line, err) = r.ReadSlice('\n');
    if (err != null) {
        err = fmt.Errorf("can't find export data (%v)", err);
        return ;
    }
    if (string(line) == "!<arch>\n") { 
        // Archive file. Scan to __.PKGDEF.
        @string name = default;
        name, _, err = readGopackHeader(_addr_r);

        if (err != null) {
            return ;
        }
        if (name != "__.PKGDEF") {
            err = fmt.Errorf("go archive is missing __.PKGDEF");
            return ;
        }
        line, err = r.ReadSlice('\n');

        if (err != null) {
            err = fmt.Errorf("can't find export data (%v)", err);
            return ;
        }
    }
    if (!strings.HasPrefix(string(line), "go object ")) {
        err = fmt.Errorf("not a Go object file");
        return ;
    }
    while (line[0] != '$') {
        line, err = r.ReadSlice('\n');

        if (err != null) {
            err = fmt.Errorf("can't find export data (%v)", err);
            return ;
        }
    }
    hdr = string(line);

    return ;
}

} // end importer_package
