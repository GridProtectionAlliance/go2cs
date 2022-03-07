// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Plan 9 directory marshaling. See intro(5).

// package syscall -- go2cs converted at 2022 March 06 22:08:11 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Program Files\Go\src\syscall\dir_plan9.go
using errors = go.errors_package;

namespace go;

public static partial class syscall_package {

public static var ErrShortStat = errors.New("stat buffer too short");public static var ErrBadStat = errors.New("malformed stat buffer");public static var ErrBadName = errors.New("bad character in file name");

// A Qid represents a 9P server's unique identification for a file.
public partial struct Qid {
    public ulong Path; // the file server's unique identification for the file
    public uint Vers; // version number for given Path
    public byte Type; // the type of the file (syscall.QTDIR for example)
}

// A Dir contains the metadata for a file.
public partial struct Dir {
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
private static void Null(this ptr<Dir> _addr_d) {
    ref Dir d = ref _addr_d.val;

    d.val = nullDir;
}

// Marshal encodes a 9P stat message corresponding to d into b
//
// If there isn't enough space in b for a stat message, ErrShortStat is returned.
private static (nint, error) Marshal(this ptr<Dir> _addr_d, slice<byte> b) {
    nint n = default;
    error err = default!;
    ref Dir d = ref _addr_d.val;

    n = STATFIXLEN + len(d.Name) + len(d.Uid) + len(d.Gid) + len(d.Muid);
    if (n > len(b)) {
        return (n, error.As(ErrShortStat)!);
    }
    foreach (var (_, c) in d.Name) {
        if (c == '/') {
            return (n, error.As(ErrBadName)!);
        }
    }    b = pbit16(b, uint16(n) - 2);
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

    return (n, error.As(null!)!);

}

// UnmarshalDir decodes a single 9P stat message from b and returns the resulting Dir.
//
// If b is too small to hold a valid stat message, ErrShortStat is returned.
//
// If the stat message itself is invalid, ErrBadStat is returned.
public static (ptr<Dir>, error) UnmarshalDir(slice<byte> b) {
    ptr<Dir> _p0 = default!;
    error _p0 = default!;

    if (len(b) < STATFIXLEN) {
        return (_addr_null!, error.As(ErrShortStat)!);
    }
    var (size, buf) = gbit16(b);
    if (len(b) != int(size) + 2) {
        return (_addr_null!, error.As(ErrBadStat)!);
    }
    b = buf;

    ref Dir d = ref heap(out ptr<Dir> _addr_d);
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

    if (!ok) {
        return (_addr_null!, error.As(ErrBadStat)!);
    }
    d.Uid, b, ok = gstring(b);

    if (!ok) {
        return (_addr_null!, error.As(ErrBadStat)!);
    }
    d.Gid, b, ok = gstring(b);

    if (!ok) {
        return (_addr_null!, error.As(ErrBadStat)!);
    }
    d.Muid, b, ok = gstring(b);

    if (!ok) {
        return (_addr_null!, error.As(ErrBadStat)!);
    }
    return (_addr__addr_d!, error.As(null!)!);

}

// pbit8 copies the 8-bit number v to b and returns the remaining slice of b.
private static slice<byte> pbit8(slice<byte> b, byte v) {
    b[0] = byte(v);
    return b[(int)1..];
}

// pbit16 copies the 16-bit number v to b in little-endian order and returns the remaining slice of b.
private static slice<byte> pbit16(slice<byte> b, ushort v) {
    b[0] = byte(v);
    b[1] = byte(v >> 8);
    return b[(int)2..];
}

// pbit32 copies the 32-bit number v to b in little-endian order and returns the remaining slice of b.
private static slice<byte> pbit32(slice<byte> b, uint v) {
    b[0] = byte(v);
    b[1] = byte(v >> 8);
    b[2] = byte(v >> 16);
    b[3] = byte(v >> 24);
    return b[(int)4..];
}

// pbit64 copies the 64-bit number v to b in little-endian order and returns the remaining slice of b.
private static slice<byte> pbit64(slice<byte> b, ulong v) {
    b[0] = byte(v);
    b[1] = byte(v >> 8);
    b[2] = byte(v >> 16);
    b[3] = byte(v >> 24);
    b[4] = byte(v >> 32);
    b[5] = byte(v >> 40);
    b[6] = byte(v >> 48);
    b[7] = byte(v >> 56);
    return b[(int)8..];
}

// pstring copies the string s to b, prepending it with a 16-bit length in little-endian order, and
// returning the remaining slice of b..
private static slice<byte> pstring(slice<byte> b, @string s) {
    b = pbit16(b, uint16(len(s)));
    var n = copy(b, s);
    return b[(int)n..];
}

// gbit8 reads an 8-bit number from b and returns it with the remaining slice of b.
private static (byte, slice<byte>) gbit8(slice<byte> b) {
    byte _p0 = default;
    slice<byte> _p0 = default;

    return (uint8(b[0]), b[(int)1..]);
}

// gbit16 reads a 16-bit number in little-endian order from b and returns it with the remaining slice of b.
//go:nosplit
private static (ushort, slice<byte>) gbit16(slice<byte> b) {
    ushort _p0 = default;
    slice<byte> _p0 = default;

    return (uint16(b[0]) | uint16(b[1]) << 8, b[(int)2..]);
}

// gbit32 reads a 32-bit number in little-endian order from b and returns it with the remaining slice of b.
private static (uint, slice<byte>) gbit32(slice<byte> b) {
    uint _p0 = default;
    slice<byte> _p0 = default;

    return (uint32(b[0]) | uint32(b[1]) << 8 | uint32(b[2]) << 16 | uint32(b[3]) << 24, b[(int)4..]);
}

// gbit64 reads a 64-bit number in little-endian order from b and returns it with the remaining slice of b.
private static (ulong, slice<byte>) gbit64(slice<byte> b) {
    ulong _p0 = default;
    slice<byte> _p0 = default;

    var lo = uint32(b[0]) | uint32(b[1]) << 8 | uint32(b[2]) << 16 | uint32(b[3]) << 24;
    var hi = uint32(b[4]) | uint32(b[5]) << 8 | uint32(b[6]) << 16 | uint32(b[7]) << 24;
    return (uint64(lo) | uint64(hi) << 32, b[(int)8..]);
}

// gstring reads a string from b, prefixed with a 16-bit length in little-endian order.
// It returns the string with the remaining slice of b and a boolean. If the length is
// greater than the number of bytes in b, the boolean will be false.
private static (@string, slice<byte>, bool) gstring(slice<byte> b) {
    @string _p0 = default;
    slice<byte> _p0 = default;
    bool _p0 = default;

    var (n, b) = gbit16(b);
    if (int(n) > len(b)) {
        return ("", b, false);
    }
    return (string(b[..(int)n]), b[(int)n..], true);

}

} // end syscall_package
