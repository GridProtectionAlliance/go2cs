// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package buildid -- go2cs converted at 2020 August 29 08:51:59 UTC
// import "cmd/internal/buildid" ==> using buildid = go.cmd.@internal.buildid_package
// Original source: C:\Go\src\cmd\internal\buildid\rewrite.go
using bytes = go.bytes_package;
using sha256 = go.crypto.sha256_package;
using fmt = go.fmt_package;
using io = go.io_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace @internal
{
    public static partial class buildid_package
    {
        // FindAndHash reads all of r and returns the offsets of occurrences of id.
        // While reading, findAndHash also computes and returns
        // a hash of the content of r, but with occurrences of id replaced by zeros.
        // FindAndHash reads bufSize bytes from r at a time.
        // If bufSize == 0, FindAndHash uses a reasonable default.
        public static (slice<long>, array<byte>, error) FindAndHash(io.Reader r, @string id, long bufSize)
        {
            if (bufSize == 0L)
            {
                bufSize = 31L * 1024L; // bufSize+little will likely fit in 32 kB
            }
            if (len(id) > bufSize)
            {
                return (null, new array<byte>(new byte[] {  }), fmt.Errorf("buildid.FindAndHash: buffer too small"));
            }
            var zeros = make_slice<byte>(len(id));
            slice<byte> idBytes = (slice<byte>)id; 

            // The strategy is to read the file through buf, looking for id,
            // but we need to worry about what happens if id is broken up
            // and returned in parts by two different reads.
            // We allocate a tiny buffer (at least len(id)) and a big buffer (bufSize bytes)
            // next to each other in memory and then copy the tail of
            // one read into the tiny buffer before reading new data into the big buffer.
            // The search for id is over the entire tiny+big buffer.
            var tiny = (len(id) + 127L) & ~127L; // round up to 128-aligned
            var buf = make_slice<byte>(tiny + bufSize);
            var h = sha256.New();
            var start = tiny;
            {
                var offset = int64(0L);

                while (>>MARKER:FOREXPRESSION_LEVEL_1<<)
                { 
                    // The file offset maintained by the loop corresponds to &buf[tiny].
                    // buf[start:tiny] is left over from previous iteration.
                    // After reading n bytes into buf[tiny:], we process buf[start:tiny+n].
                    var (n, err) = io.ReadFull(r, buf[tiny..]);
                    if (err != io.ErrUnexpectedEOF && err != io.EOF && err != null)
                    {
                        return (null, new array<byte>(new byte[] {  }), err);
                    }
                    while (true)
                    {
                        var i = bytes.Index(buf[start..tiny + n], idBytes);
                        if (i < 0L)
                        {
                            break;
                        }
                        matches = append(matches, offset + int64(start + i - tiny));
                        h.Write(buf[start..start + i]);
                        h.Write(zeros);
                        start += i + len(id);
                    }
                    if (n < bufSize)
                    { 
                        // Did not fill buffer, must be at end of file.
                        h.Write(buf[start..tiny + n]);
                        break;
                    }
                    if (start < len(buf) - tiny)
                    {
                        h.Write(buf[start..len(buf) - tiny]);
                        start = len(buf) - tiny;
                    }
                    copy(buf[0L..], buf[bufSize..]);
                    start -= bufSize;
                    offset += int64(bufSize);
                }
            }
            h.Sum(hash[..0L]);
            return (matches, hash, null);
        }

        public static error Rewrite(io.WriterAt w, slice<long> pos, @string id)
        {
            slice<byte> b = (slice<byte>)id;
            foreach (var (_, p) in pos)
            {
                {
                    var (_, err) = w.WriteAt(b, p);

                    if (err != null)
                    {
                        return error.As(err);
                    }

                }
            }
            return error.As(null);
        }
    }
}}}
