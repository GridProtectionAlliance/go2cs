// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Parse "zoneinfo" time zone file.
// This is a fairly standard file format used on OS X, Linux, BSD, Sun, and others.
// See tzfile(5), https://en.wikipedia.org/wiki/Zoneinfo,
// and ftp://munnari.oz.au/pub/oldtz/

// package time -- go2cs converted at 2022 March 13 05:41:07 UTC
// import "time" ==> using time = go.time_package
// Original source: C:\Program Files\Go\src\time\zoneinfo_read.go
namespace go;

using errors = errors_package;
using runtime = runtime_package;
using syscall = syscall_package;


// registerLoadFromEmbeddedTZData is called by the time/tzdata package,
// if it is imported.

using System;
public static partial class time_package {

private static (@string, error) registerLoadFromEmbeddedTZData(Func<@string, (@string, error)> f) {
    @string _p0 = default;
    error _p0 = default!;

    loadFromEmbeddedTZData = f;
}

// loadFromEmbeddedTZData is used to load a specific tzdata file
// from tzdata information embedded in the binary itself.
// This is set when the time/tzdata package is imported,
// via registerLoadFromEmbeddedTzdata.
private static Func<@string, (@string, error)> loadFromEmbeddedTZData = default;

// maxFileSize is the max permitted size of files read by readFile.
// As reference, the zoneinfo.zip distributed by Go is ~350 KB,
// so 10MB is overkill.
private static readonly nint maxFileSize = 10 << 20;



private partial struct fileSizeError { // : @string
}

private static @string Error(this fileSizeError f) {
    return "time: file " + string(f) + " is too large";
}

// Copies of io.Seek* constants to avoid importing "io":
private static readonly nint seekStart = 0;
private static readonly nint seekCurrent = 1;
private static readonly nint seekEnd = 2;

// Simple I/O interface to binary blob of data.
private partial struct dataIO {
    public slice<byte> p;
    public bool error;
}

private static slice<byte> read(this ptr<dataIO> _addr_d, nint n) {
    ref dataIO d = ref _addr_d.val;

    if (len(d.p) < n) {
        d.p = null;
        d.error = true;
        return null;
    }
    var p = d.p[(int)0..(int)n];
    d.p = d.p[(int)n..];
    return p;
}

private static (uint, bool) big4(this ptr<dataIO> _addr_d) {
    uint n = default;
    bool ok = default;
    ref dataIO d = ref _addr_d.val;

    var p = d.read(4);
    if (len(p) < 4) {
        d.error = true;
        return (0, false);
    }
    return (uint32(p[3]) | uint32(p[2]) << 8 | uint32(p[1]) << 16 | uint32(p[0]) << 24, true);
}

private static (ulong, bool) big8(this ptr<dataIO> _addr_d) {
    ulong n = default;
    bool ok = default;
    ref dataIO d = ref _addr_d.val;

    var (n1, ok1) = d.big4();
    var (n2, ok2) = d.big4();
    if (!ok1 || !ok2) {
        d.error = true;
        return (0, false);
    }
    return ((uint64(n1) << 32) | uint64(n2), true);
}

private static (byte, bool) @byte(this ptr<dataIO> _addr_d) {
    byte n = default;
    bool ok = default;
    ref dataIO d = ref _addr_d.val;

    var p = d.read(1);
    if (len(p) < 1) {
        d.error = true;
        return (0, false);
    }
    return (p[0], true);
}

// read returns the read of the data in the buffer.
private static slice<byte> rest(this ptr<dataIO> _addr_d) {
    ref dataIO d = ref _addr_d.val;

    var r = d.p;
    d.p = null;
    return r;
}

// Make a string by stopping at the first NUL
private static @string byteString(slice<byte> p) {
    for (nint i = 0; i < len(p); i++) {
        if (p[i] == 0) {
            return string(p[(int)0..(int)i]);
        }
    }
    return string(p);
}

private static var badData = errors.New("malformed time zone information");

// LoadLocationFromTZData returns a Location with the given name
// initialized from the IANA Time Zone database-formatted data.
// The data should be in the format of a standard IANA time zone file
// (for example, the content of /etc/localtime on Unix systems).
public static (ptr<Location>, error) LoadLocationFromTZData(@string name, slice<byte> data) {
    ptr<Location> _p0 = default!;
    error _p0 = default!;

    dataIO d = new dataIO(data,false); 

    // 4-byte magic "TZif"
    {
        var magic = d.read(4);

        if (string(magic) != "TZif") {
            return (_addr_null!, error.As(badData)!);
        }
    } 

    // 1-byte version, then 15 bytes of padding
    nint version = default;
    slice<byte> p = default;
    p = d.read(16);

    if (len(p) != 16) {
        return (_addr_null!, error.As(badData)!);
    }
    else
 {
        switch (p[0]) {
            case 0: 
                version = 1;
                break;
            case '2': 
                version = 2;
                break;
            case '3': 
                version = 3;
                break;
            default: 
                return (_addr_null!, error.As(badData)!);
                break;
        }
    }
    const var NUTCLocal = iota;
    const var NStdWall = 0;
    const var NLeap = 1;
    const var NTime = 2;
    const var NZone = 3;
    const var NChar = 4;
    array<nint> n = new array<nint>(6);
    {
        nint i__prev1 = i;

        for (nint i = 0; i < 6; i++) {
            var (nn, ok) = d.big4();
            if (!ok) {
                return (_addr_null!, error.As(badData)!);
            }
            if (uint32(int(nn)) != nn) {
                return (_addr_null!, error.As(badData)!);
            }
            n[i] = int(nn);
        }

        i = i__prev1;
    } 

    // If we have version 2 or 3, then the data is first written out
    // in a 32-bit format, then written out again in a 64-bit format.
    // Skip the 32-bit format and read the 64-bit one, as it can
    // describe a broader range of dates.

    var is64 = false;
    if (version > 1) { 
        // Skip the 32-bit data.
        var skip = n[NTime] * 4 + n[NTime] + n[NZone] * 6 + n[NChar] + n[NLeap] * 8 + n[NStdWall] + n[NUTCLocal]; 
        // Skip the version 2 header that we just read.
        skip += 4 + 16;
        d.read(skip);

        is64 = true; 

        // Read the counts again, they can differ.
        {
            nint i__prev1 = i;

            for (i = 0; i < 6; i++) {
                (nn, ok) = d.big4();
                if (!ok) {
                    return (_addr_null!, error.As(badData)!);
                }
                if (uint32(int(nn)) != nn) {
                    return (_addr_null!, error.As(badData)!);
                }
                n[i] = int(nn);
            }


            i = i__prev1;
        }
    }
    nint size = 4;
    if (is64) {
        size = 8;
    }
    dataIO txtimes = new dataIO(d.read(n[NTime]*size),false); 

    // Time zone indices for transition times.
    var txzones = d.read(n[NTime]); 

    // Zone info structures
    dataIO zonedata = new dataIO(d.read(n[NZone]*6),false); 

    // Time zone abbreviations.
    var abbrev = d.read(n[NChar]); 

    // Leap-second time pairs
    d.read(n[NLeap] * (size + 4)); 

    // Whether tx times associated with local time types
    // are specified as standard time or wall time.
    var isstd = d.read(n[NStdWall]); 

    // Whether tx times associated with local time types
    // are specified as UTC or local time.
    var isutc = d.read(n[NUTCLocal]);

    if (d.error) { // ran out of data
        return (_addr_null!, error.As(badData)!);
    }
    @string extend = default;
    var rest = d.rest();
    if (len(rest) > 2 && rest[0] == '\n' && rest[len(rest) - 1] == '\n') {
        extend = string(rest[(int)1..(int)len(rest) - 1]);
    }
    var nzone = n[NZone];
    if (nzone == 0) { 
        // Reject tzdata files with no zones. There's nothing useful in them.
        // This also avoids a panic later when we add and then use a fake transition (golang.org/issue/29437).
        return (_addr_null!, error.As(badData)!);
    }
    var zones = make_slice<zone>(nzone);
    {
        nint i__prev1 = i;

        foreach (var (__i) in zones) {
            i = __i;
            bool ok = default;
            n = default;
            n, ok = zonedata.big4();

            if (!ok) {
                return (_addr_null!, error.As(badData)!);
            }
            if (uint32(int(n)) != n) {
                return (_addr_null!, error.As(badData)!);
            }
            zones[i].offset = int(int32(n));
            byte b = default;
            b, ok = zonedata.@byte();

            if (!ok) {
                return (_addr_null!, error.As(badData)!);
            }
            zones[i].isDST = b != 0;
            b, ok = zonedata.@byte();

            if (!ok || int(b) >= len(abbrev)) {
                return (_addr_null!, error.As(badData)!);
            }
            zones[i].name = byteString(abbrev[(int)b..]);
            if (runtime.GOOS == "aix" && len(name) > 8 && (name[..(int)8] == "Etc/GMT+" || name[..(int)8] == "Etc/GMT-")) { 
                // There is a bug with AIX 7.2 TL 0 with files in Etc,
                // GMT+1 will return GMT-1 instead of GMT+1 or -01.
                if (name != "Etc/GMT+0") { 
                    // GMT+0 is OK
                    zones[i].name = name[(int)4..];
                }
            }
        }
        i = i__prev1;
    }

    var tx = make_slice<zoneTrans>(n[NTime]);
    {
        nint i__prev1 = i;

        foreach (var (__i) in tx) {
            i = __i;
            n = default;
            if (!is64) {
                {
                    var (n4, ok) = txtimes.big4();

                    if (!ok) {
                        return (_addr_null!, error.As(badData)!);
                    }
                    else
 {
                        n = int64(int32(n4));
                    }

                }
            }
            else
 {
                {
                    var (n8, ok) = txtimes.big8();

                    if (!ok) {
                        return (_addr_null!, error.As(badData)!);
                    }
                    else
 {
                        n = int64(n8);
                    }

                }
            }
            tx[i].when = n;
            if (int(txzones[i]) >= len(zones)) {
                return (_addr_null!, error.As(badData)!);
            }
            tx[i].index = txzones[i];
            if (i < len(isstd)) {
                tx[i].isstd = isstd[i] != 0;
            }
            if (i < len(isutc)) {
                tx[i].isutc = isutc[i] != 0;
            }
        }
        i = i__prev1;
    }

    if (len(tx) == 0) { 
        // Build fake transition to cover all time.
        // This happens in fixed locations like "Etc/GMT0".
        tx = append(tx, new zoneTrans(when:alpha,index:0));
    }
    ptr<Location> l = addr(new Location(zone:zones,tx:tx,name:name,extend:extend)); 

    // Fill in the cache with information about right now,
    // since that will be the most common lookup.
    var (sec, _, _) = now();
    {
        nint i__prev1 = i;

        foreach (var (__i) in tx) {
            i = __i;
            if (tx[i].when <= sec && (i + 1 == len(tx) || sec < tx[i + 1].when)) {
                l.cacheStart = tx[i].when;
                l.cacheEnd = omega;
                l.cacheZone = _addr_l.zone[tx[i].index];
                if (i + 1 < len(tx)) {
                    l.cacheEnd = tx[i + 1].when;
                }
                else if (l.extend != "") { 
                    // If we're at the end of the known zone transitions,
                    // try the extend string.
                    {
                        var (name, offset, estart, eend, isDST, ok) = tzset(l.extend, l.cacheEnd, sec);

                        if (ok) {
                            l.cacheStart = estart;
                            l.cacheEnd = eend; 
                            // Find the zone that is returned by tzset to avoid allocation if possible.
                            {
                                var zoneIdx = findZone(l.zone, name, offset, isDST);

                                if (zoneIdx != -1) {
                                    l.cacheZone = _addr_l.zone[zoneIdx];
                                }
                                else
 {
                                    l.cacheZone = addr(new zone(name:name,offset:offset,isDST:isDST,));
                                }

                            }
                        }

                    }
                }
                break;
            }
        }
        i = i__prev1;
    }

    return (_addr_l!, error.As(null!)!);
}

private static nint findZone(slice<zone> zones, @string name, nint offset, bool isDST) {
    foreach (var (i, z) in zones) {
        if (z.name == name && z.offset == offset && z.isDST == isDST) {
            return i;
        }
    }    return -1;
}

// loadTzinfoFromDirOrZip returns the contents of the file with the given name
// in dir. dir can either be an uncompressed zip file, or a directory.
private static (slice<byte>, error) loadTzinfoFromDirOrZip(@string dir, @string name) {
    slice<byte> _p0 = default;
    error _p0 = default!;

    if (len(dir) > 4 && dir[(int)len(dir) - 4..] == ".zip") {
        return loadTzinfoFromZip(dir, name);
    }
    if (dir != "") {
        name = dir + "/" + name;
    }
    return readFile(name);
}

// There are 500+ zoneinfo files. Rather than distribute them all
// individually, we ship them in an uncompressed zip file.
// Used this way, the zip file format serves as a commonly readable
// container for the individual small files. We choose zip over tar
// because zip files have a contiguous table of contents, making
// individual file lookups faster, and because the per-file overhead
// in a zip file is considerably less than tar's 512 bytes.

// get4 returns the little-endian 32-bit value in b.
private static nint get4(slice<byte> b) {
    if (len(b) < 4) {
        return 0;
    }
    return int(b[0]) | int(b[1]) << 8 | int(b[2]) << 16 | int(b[3]) << 24;
}

// get2 returns the little-endian 16-bit value in b.
private static nint get2(slice<byte> b) {
    if (len(b) < 2) {
        return 0;
    }
    return int(b[0]) | int(b[1]) << 8;
}

// loadTzinfoFromZip returns the contents of the file with the given name
// in the given uncompressed zip file.
private static (slice<byte>, error) loadTzinfoFromZip(@string zipfile, @string name) => func((defer, _, _) => {
    slice<byte> _p0 = default;
    error _p0 = default!;

    var (fd, err) = open(zipfile);
    if (err != null) {
        return (null, error.As(err)!);
    }
    defer(closefd(fd));

    const nuint zecheader = 0x06054b50;
    const nuint zcheader = 0x02014b50;
    const nint ztailsize = 22;

    const nint zheadersize = 30;
    const nuint zheader = 0x04034b50;

    var buf = make_slice<byte>(ztailsize);
    {
        var err__prev1 = err;

        var err = preadn(fd, buf, -ztailsize);

        if (err != null || get4(buf) != zecheader) {
            return (null, error.As(errors.New("corrupt zip file " + zipfile))!);
        }
        err = err__prev1;

    }
    var n = get2(buf[(int)10..]);
    var size = get4(buf[(int)12..]);
    var off = get4(buf[(int)16..]);

    buf = make_slice<byte>(size);
    {
        var err__prev1 = err;

        err = preadn(fd, buf, off);

        if (err != null) {
            return (null, error.As(errors.New("corrupt zip file " + zipfile))!);
        }
        err = err__prev1;

    }

    for (nint i = 0; i < n; i++) { 
        // zip entry layout:
        //    0    magic[4]
        //    4    madevers[1]
        //    5    madeos[1]
        //    6    extvers[1]
        //    7    extos[1]
        //    8    flags[2]
        //    10    meth[2]
        //    12    modtime[2]
        //    14    moddate[2]
        //    16    crc[4]
        //    20    csize[4]
        //    24    uncsize[4]
        //    28    namelen[2]
        //    30    xlen[2]
        //    32    fclen[2]
        //    34    disknum[2]
        //    36    iattr[2]
        //    38    eattr[4]
        //    42    off[4]
        //    46    name[namelen]
        //    46+namelen+xlen+fclen - next header
        //
        if (get4(buf) != zcheader) {
            break;
        }
        var meth = get2(buf[(int)10..]);
        size = get4(buf[(int)24..]);
        var namelen = get2(buf[(int)28..]);
        var xlen = get2(buf[(int)30..]);
        var fclen = get2(buf[(int)32..]);
        off = get4(buf[(int)42..]);
        var zname = buf[(int)46..(int)46 + namelen];
        buf = buf[(int)46 + namelen + xlen + fclen..];
        if (string(zname) != name) {
            continue;
        }
        if (meth != 0) {
            return (null, error.As(errors.New("unsupported compression for " + name + " in " + zipfile))!);
        }
        buf = make_slice<byte>(zheadersize + namelen);
        {
            var err__prev1 = err;

            err = preadn(fd, buf, off);

            if (err != null || get4(buf) != zheader || get2(buf[(int)8..]) != meth || get2(buf[(int)26..]) != namelen || string(buf[(int)30..(int)30 + namelen]) != name) {
                return (null, error.As(errors.New("corrupt zip file " + zipfile))!);
            }

            err = err__prev1;

        }
        xlen = get2(buf[(int)28..]);

        buf = make_slice<byte>(size);
        {
            var err__prev1 = err;

            err = preadn(fd, buf, off + 30 + namelen + xlen);

            if (err != null) {
                return (null, error.As(errors.New("corrupt zip file " + zipfile))!);
            }

            err = err__prev1;

        }

        return (buf, error.As(null!)!);
    }

    return (null, error.As(syscall.ENOENT)!);
});

// loadTzinfoFromTzdata returns the time zone information of the time zone
// with the given name, from a tzdata database file as they are typically
// found on android.
private static Func<@string, @string, (slice<byte>, error)> loadTzinfoFromTzdata = default;

// loadTzinfo returns the time zone information of the time zone
// with the given name, from a given source. A source may be a
// timezone database directory, tzdata database file or an uncompressed
// zip file, containing the contents of such a directory.
private static (slice<byte>, error) loadTzinfo(@string name, @string source) {
    slice<byte> _p0 = default;
    error _p0 = default!;

    if (len(source) >= 6 && source[(int)len(source) - 6..] == "tzdata") {
        return loadTzinfoFromTzdata(source, name);
    }
    return loadTzinfoFromDirOrZip(source, name);
}

// loadLocation returns the Location with the given name from one of
// the specified sources. See loadTzinfo for a list of supported sources.
// The first timezone data matching the given name that is successfully loaded
// and parsed is returned as a Location.
private static (ptr<Location>, error) loadLocation(@string name, slice<@string> sources) {
    ptr<Location> z = default!;
    error firstErr = default!;

    foreach (var (_, source) in sources) {

        if (err == null) {
            z, err = LoadLocationFromTZData(name, zoneData);

            if (err == null) {
                return (_addr_z!, error.As(null!)!);
            }
        }
        if (firstErr == null && err != syscall.ENOENT) {
            firstErr = err;
        }
    }    if (loadFromEmbeddedTZData != null) {
        var (zonedata, err) = loadFromEmbeddedTZData(name);
        if (err == null) {
            z, err = LoadLocationFromTZData(name, (slice<byte>)zonedata);

            if (err == null) {
                return (_addr_z!, error.As(null!)!);
            }
        }
        if (firstErr == null && err != syscall.ENOENT) {
            firstErr = err;
        }
    }
    if (firstErr != null) {
        return (_addr_null!, error.As(firstErr)!);
    }
    return (_addr_null!, error.As(errors.New("unknown time zone " + name))!);
}

// readFile reads and returns the content of the named file.
// It is a trivial implementation of os.ReadFile, reimplemented
// here to avoid depending on io/ioutil or os.
// It returns an error if name exceeds maxFileSize bytes.
private static (slice<byte>, error) readFile(@string name) => func((defer, _, _) => {
    slice<byte> _p0 = default;
    error _p0 = default!;

    var (f, err) = open(name);
    if (err != null) {
        return (null, error.As(err)!);
    }
    defer(closefd(f));
    array<byte> buf = new array<byte>(4096);    slice<byte> ret = default;    nint n = default;
    while (true) {
        n, err = read(f, buf[..]);
        if (n > 0) {
            ret = append(ret, buf[..(int)n]);
        }
        if (n == 0 || err != null) {
            break;
        }
        if (len(ret) > maxFileSize) {
            return (null, error.As(fileSizeError(name))!);
        }
    }
    return (ret, error.As(err)!);
});

} // end time_package
