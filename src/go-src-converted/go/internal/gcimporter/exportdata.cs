// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements FindExportData.

// package gcimporter -- go2cs converted at 2020 October 08 04:56:07 UTC
// import "go/internal/gcimporter" ==> using gcimporter = go.go.@internal.gcimporter_package
// Original source: C:\Go\src\go\internal\gcimporter\exportdata.go
using bufio = go.bufio_package;
using fmt = go.fmt_package;
using io = go.io_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using static go.builtin;

namespace go {
namespace go {
namespace @internal
{
    public static partial class gcimporter_package
    {
        private static (@string, long, error) readGopackHeader(ptr<bufio.Reader> _addr_r)
        {
            @string name = default;
            long size = default;
            error err = default!;
            ref bufio.Reader r = ref _addr_r.val;
 
            // See $GOROOT/include/ar.h.
            var hdr = make_slice<byte>(16L + 12L + 6L + 6L + 8L + 10L + 2L);
            _, err = io.ReadFull(r, hdr);
            if (err != null)
            {
                return ;
            }
            if (false)
            {
                fmt.Printf("header: %s", hdr);
            }
            var s = strings.TrimSpace(string(hdr[16L + 12L + 6L + 6L + 8L..][..10L]));
            size, err = strconv.Atoi(s);
            if (err != null || hdr[len(hdr) - 2L] != '`' || hdr[len(hdr) - 1L] != '\n')
            {
                err = fmt.Errorf("invalid archive header");
                return ;
            }
            name = strings.TrimSpace(string(hdr[..16L]));
            return ;

        }

        // FindExportData positions the reader r at the beginning of the
        // export data section of an underlying GC-created object/archive
        // file by reading from it. The reader must be positioned at the
        // start of the file before calling this function. The hdr result
        // is the string before the export data, either "$$" or "$$B".
        //
        public static (@string, error) FindExportData(ptr<bufio.Reader> _addr_r)
        {
            @string hdr = default;
            error err = default!;
            ref bufio.Reader r = ref _addr_r.val;
 
            // Read first line to make sure this is an object file.
            var (line, err) = r.ReadSlice('\n');
            if (err != null)
            {
                err = fmt.Errorf("can't find export data (%v)", err);
                return ;
            }

            if (string(line) == "!<arch>\n")
            { 
                // Archive file. Scan to __.PKGDEF.
                @string name = default;
                name, _, err = readGopackHeader(_addr_r);

                if (err != null)
                {
                    return ;
                } 

                // First entry should be __.PKGDEF.
                if (name != "__.PKGDEF")
                {
                    err = fmt.Errorf("go archive is missing __.PKGDEF");
                    return ;
                } 

                // Read first line of __.PKGDEF data, so that line
                // is once again the first line of the input.
                line, err = r.ReadSlice('\n');

                if (err != null)
                {
                    err = fmt.Errorf("can't find export data (%v)", err);
                    return ;
                }

            } 

            // Now at __.PKGDEF in archive or still at beginning of file.
            // Either way, line should begin with "go object ".
            if (!strings.HasPrefix(string(line), "go object "))
            {
                err = fmt.Errorf("not a Go object file");
                return ;
            } 

            // Skip over object header to export data.
            // Begins after first line starting with $$.
            while (line[0L] != '$')
            {
                line, err = r.ReadSlice('\n');

                if (err != null)
                {
                    err = fmt.Errorf("can't find export data (%v)", err);
                    return ;
                }

            }

            hdr = string(line);

            return ;

        }
    }
}}}
