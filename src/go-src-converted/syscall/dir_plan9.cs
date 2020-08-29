// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Plan 9 directory marshaling. See intro(5).

// package syscall -- go2cs converted at 2020 August 29 08:16:19 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\dir_plan9.go
using errors = go.errors_package;
using static go.builtin;

namespace go
{
    public static partial class syscall_package
    {
        public static var ErrShortStat = errors.New("stat buffer too short");        public static var ErrBadStat = errors.New("malformed stat buffer");        public static var ErrBadName = errors.New("bad character in file name");

        // A Qid represents a 9P server's unique identification for a file.
        public partial struct Qid
        {
            public ulong Path; // the file server's unique identification for the file
            public uint Vers; // version number for given Path
            public byte Type; // the type of the file (syscall.QTDIR for example)
        }

        // A Dir contains the metadata for a file.
        public partial struct Dir
        {
            public ushort Type; // server type
            public uint Dev; // server subtype

// file data
            public Qid Qid; // unique id from server
            public uint Mode; // permissions
            public uint Atime; // last read time
            public uint Mtime; // last write time
            public long Length; // file length
            public @string Name; // last element of path
            public @string Uid; // owner name
            public @string Gid; // group name
            public @string Muid; // last modifier name
        }

        private static Dir nullDir = new Dir(Type:^uint16(0),Dev:^uint32(0),Qid:Qid{Path:^uint64(0),Vers:^uint32(0),Type:^uint8(0),},Mode:^uint32(0),Atime:^uint32(0),Mtime:^uint32(0),Length:^int64(0),);

        // Null assigns special "don't touch" values to members of d to
        // avoid modifying them during syscall.Wstat.
        private static void Null(this ref Dir d)
        {
            d.Value = nullDir;

        }

        // Marshal encodes a 9P stat message corresponding to d into b
        //
        // If there isn't enough space in b for a stat message, ErrShortStat is returned.
        private static (long, error) Marshal(this ref Dir d, slice<byte> b)
        {
            n = STATFIXLEN + len(d.Name) + len(d.Uid) + len(d.Gid) + len(d.Muid);
            if (n > len(b))
            {
                return (n, ErrShortStat);
            }
            foreach (var (_, c) in d.Name)
            {
                if (c == '/')
                {
                    return (n, ErrBadName);
                }
            }
            b = pbit16(b, uint16(n) - 2L);
            b = pbit16(b, d.Type);
            b = pbit32(b, d.Dev);
            b = pbit8(b, d.Qid.Type);
            b = pbit32(b, d.Qid.Vers);
            b = pbit64(b, d.Qid.Path);
            b = pbit32(b, d.Mode);
            b = pbit32(b, d.Atime);
            b = pbit32(b, d.Mtime);
            b = pbit64(b, uint64(d.Length));
            b = pstring(b, d.Name);
            b = pstring(b, d.Uid);
            b = pstring(b, d.Gid);
            b = pstring(b, d.Muid);

            return (n, null);
        }

        // UnmarshalDir decodes a single 9P stat message from b and returns the resulting Dir.
        //
        // If b is too small to hold a valid stat message, ErrShortStat is returned.
        //
        // If the stat message itself is invalid, ErrBadStat is returned.
        public static (ref Dir, error) UnmarshalDir(slice<byte> b)
        {
            if (len(b) < STATFIXLEN)
            {
                return (null, ErrShortStat);
            }
            var (size, buf) = gbit16(b);
            if (len(b) != int(size) + 2L)
            {
                return (null, ErrBadStat);
            }
            b = buf;

            Dir d = default;
            d.Type, b = gbit16(b);
            d.Dev, b = gbit32(b);
            d.Qid.Type, b = gbit8(b);
            d.Qid.Vers, b = gbit32(b);
            d.Qid.Path, b = gbit64(b);
            d.Mode, b = gbit32(b);
            d.Atime, b = gbit32(b);
            d.Mtime, b = gbit32(b);

            var (n, b) = gbit64(b);
            d.Length = int64(n);

            bool ok = default;
            d.Name, b, ok = gstring(b);

            if (!ok)
            {
                return (null, ErrBadStat);
            }
            d.Uid, b, ok = gstring(b);

            if (!ok)
            {
                return (null, ErrBadStat);
            }
            d.Gid, b, ok = gstring(b);

            if (!ok)
            {
                return (null, ErrBadStat);
            }
            d.Muid, b, ok = gstring(b);

            if (!ok)
            {
                return (null, ErrBadStat);
            }
            return (ref d, null);
        }

        // pbit8 copies the 8-bit number v to b and returns the remaining slice of b.
        private static slice<byte> pbit8(slice<byte> b, byte v)
        {
            b[0L] = byte(v);
            return b[1L..];
        }

        // pbit16 copies the 16-bit number v to b in little-endian order and returns the remaining slice of b.
        private static slice<byte> pbit16(slice<byte> b, ushort v)
        {
            b[0L] = byte(v);
            b[1L] = byte(v >> (int)(8L));
            return b[2L..];
        }

        // pbit32 copies the 32-bit number v to b in little-endian order and returns the remaining slice of b.
        private static slice<byte> pbit32(slice<byte> b, uint v)
        {
            b[0L] = byte(v);
            b[1L] = byte(v >> (int)(8L));
            b[2L] = byte(v >> (int)(16L));
            b[3L] = byte(v >> (int)(24L));
            return b[4L..];
        }

        // pbit64 copies the 64-bit number v to b in little-endian order and returns the remaining slice of b.
        private static slice<byte> pbit64(slice<byte> b, ulong v)
        {
            b[0L] = byte(v);
            b[1L] = byte(v >> (int)(8L));
            b[2L] = byte(v >> (int)(16L));
            b[3L] = byte(v >> (int)(24L));
            b[4L] = byte(v >> (int)(32L));
            b[5L] = byte(v >> (int)(40L));
            b[6L] = byte(v >> (int)(48L));
            b[7L] = byte(v >> (int)(56L));
            return b[8L..];
        }

        // pstring copies the string s to b, prepending it with a 16-bit length in little-endian order, and
        // returning the remaining slice of b..
        private static slice<byte> pstring(slice<byte> b, @string s)
        {
            b = pbit16(b, uint16(len(s)));
            var n = copy(b, s);
            return b[n..];
        }

        // gbit8 reads an 8-bit number from b and returns it with the remaining slice of b.
        private static (byte, slice<byte>) gbit8(slice<byte> b)
        {
            return (uint8(b[0L]), b[1L..]);
        }

        // gbit16 reads a 16-bit number in little-endian order from b and returns it with the remaining slice of b.
        //go:nosplit
        private static (ushort, slice<byte>) gbit16(slice<byte> b)
        {
            return (uint16(b[0L]) | uint16(b[1L]) << (int)(8L), b[2L..]);
        }

        // gbit32 reads a 32-bit number in little-endian order from b and returns it with the remaining slice of b.
        private static (uint, slice<byte>) gbit32(slice<byte> b)
        {
            return (uint32(b[0L]) | uint32(b[1L]) << (int)(8L) | uint32(b[2L]) << (int)(16L) | uint32(b[3L]) << (int)(24L), b[4L..]);
        }

        // gbit64 reads a 64-bit number in little-endian order from b and returns it with the remaining slice of b.
        private static (ulong, slice<byte>) gbit64(slice<byte> b)
        {
            var lo = uint32(b[0L]) | uint32(b[1L]) << (int)(8L) | uint32(b[2L]) << (int)(16L) | uint32(b[3L]) << (int)(24L);
            var hi = uint32(b[4L]) | uint32(b[5L]) << (int)(8L) | uint32(b[6L]) << (int)(16L) | uint32(b[7L]) << (int)(24L);
            return (uint64(lo) | uint64(hi) << (int)(32L), b[8L..]);
        }

        // gstring reads a string from b, prefixed with a 16-bit length in little-endian order.
        // It returns the string with the remaining slice of b and a boolean. If the length is
        // greater than the number of bytes in b, the boolean will be false.
        private static (@string, slice<byte>, bool) gstring(slice<byte> b)
        {
            var (n, b) = gbit16(b);
            if (int(n) > len(b))
            {
                return ("", b, false);
            }
            return (string(b[..n]), b[n..], true);
        }
    }
}
