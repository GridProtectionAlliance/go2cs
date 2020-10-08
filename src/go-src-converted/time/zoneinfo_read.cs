// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Parse "zoneinfo" time zone file.
// This is a fairly standard file format used on OS X, Linux, BSD, Sun, and others.
// See tzfile(5), https://en.wikipedia.org/wiki/Zoneinfo,
// and ftp://munnari.oz.au/pub/oldtz/

// package time -- go2cs converted at 2020 October 08 03:45:56 UTC
// import "time" ==> using time = go.time_package
// Original source: C:\Go\src\time\zoneinfo_read.go
using errors = go.errors_package;
using runtime = go.runtime_package;
using syscall = go.syscall_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class time_package
    {
        // registerLoadFromEmbeddedTZData is called by the time/tzdata package,
        // if it is imported.
        private static (@string, error) registerLoadFromEmbeddedTZData(Func<@string, (@string, error)> f)
        {
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
        private static readonly long maxFileSize = (long)10L << (int)(20L);



        private partial struct fileSizeError // : @string
        {
        }

        private static @string Error(this fileSizeError f)
        {
            return "time: file " + string(f) + " is too large";
        }

        // Copies of io.Seek* constants to avoid importing "io":
        private static readonly long seekStart = (long)0L;
        private static readonly long seekCurrent = (long)1L;
        private static readonly long seekEnd = (long)2L;


        // Simple I/O interface to binary blob of data.
        private partial struct dataIO
        {
            public slice<byte> p;
            public bool error;
        }

        private static slice<byte> read(this ptr<dataIO> _addr_d, long n)
        {
            ref dataIO d = ref _addr_d.val;

            if (len(d.p) < n)
            {
                d.p = null;
                d.error = true;
                return null;
            }

            var p = d.p[0L..n];
            d.p = d.p[n..];
            return p;

        }

        private static (uint, bool) big4(this ptr<dataIO> _addr_d)
        {
            uint n = default;
            bool ok = default;
            ref dataIO d = ref _addr_d.val;

            var p = d.read(4L);
            if (len(p) < 4L)
            {
                d.error = true;
                return (0L, false);
            }

            return (uint32(p[3L]) | uint32(p[2L]) << (int)(8L) | uint32(p[1L]) << (int)(16L) | uint32(p[0L]) << (int)(24L), true);

        }

        private static (ulong, bool) big8(this ptr<dataIO> _addr_d)
        {
            ulong n = default;
            bool ok = default;
            ref dataIO d = ref _addr_d.val;

            var (n1, ok1) = d.big4();
            var (n2, ok2) = d.big4();
            if (!ok1 || !ok2)
            {
                d.error = true;
                return (0L, false);
            }

            return ((uint64(n1) << (int)(32L)) | uint64(n2), true);

        }

        private static (byte, bool) @byte(this ptr<dataIO> _addr_d)
        {
            byte n = default;
            bool ok = default;
            ref dataIO d = ref _addr_d.val;

            var p = d.read(1L);
            if (len(p) < 1L)
            {
                d.error = true;
                return (0L, false);
            }

            return (p[0L], true);

        }

        // read returns the read of the data in the buffer.
        private static slice<byte> rest(this ptr<dataIO> _addr_d)
        {
            ref dataIO d = ref _addr_d.val;

            var r = d.p;
            d.p = null;
            return r;
        }

        // Make a string by stopping at the first NUL
        private static @string byteString(slice<byte> p)
        {
            for (long i = 0L; i < len(p); i++)
            {
                if (p[i] == 0L)
                {
                    return string(p[0L..i]);
                }

            }

            return string(p);

        }

        private static var badData = errors.New("malformed time zone information");

        // LoadLocationFromTZData returns a Location with the given name
        // initialized from the IANA Time Zone database-formatted data.
        // The data should be in the format of a standard IANA time zone file
        // (for example, the content of /etc/localtime on Unix systems).
        public static (ptr<Location>, error) LoadLocationFromTZData(@string name, slice<byte> data)
        {
            ptr<Location> _p0 = default!;
            error _p0 = default!;

            dataIO d = new dataIO(data,false); 

            // 4-byte magic "TZif"
            {
                var magic = d.read(4L);

                if (string(magic) != "TZif")
                {
                    return (_addr_null!, error.As(badData)!);
                } 

                // 1-byte version, then 15 bytes of padding

            } 

            // 1-byte version, then 15 bytes of padding
            long version = default;
            slice<byte> p = default;
            p = d.read(16L);

            if (len(p) != 16L)
            {
                return (_addr_null!, error.As(badData)!);
            }
            else
            {
                switch (p[0L])
                {
                    case 0L: 
                        version = 1L;
                        break;
                    case '2': 
                        version = 2L;
                        break;
                    case '3': 
                        version = 3L;
                        break;
                    default: 
                        return (_addr_null!, error.As(badData)!);
                        break;
                }

            } 

            // six big-endian 32-bit integers:
            //    number of UTC/local indicators
            //    number of standard/wall indicators
            //    number of leap seconds
            //    number of transition times
            //    number of local time zones
            //    number of characters of time zone abbrev strings
            const var NUTCLocal = (var)iota;
            const var NStdWall = (var)0;
            const var NLeap = (var)1;
            const var NTime = (var)2;
            const var NZone = (var)3;
            const var NChar = (var)4;

            array<long> n = new array<long>(6L);
            {
                long i__prev1 = i;

                for (long i = 0L; i < 6L; i++)
                {
                    var (nn, ok) = d.big4();
                    if (!ok)
                    {
                        return (_addr_null!, error.As(badData)!);
                    }

                    if (uint32(int(nn)) != nn)
                    {
                        return (_addr_null!, error.As(badData)!);
                    }

                    n[i] = int(nn);

                } 

                // If we have version 2 or 3, then the data is first written out
                // in a 32-bit format, then written out again in a 64-bit format.
                // Skip the 32-bit format and read the 64-bit one, as it can
                // describe a broader range of dates.


                i = i__prev1;
            } 

            // If we have version 2 or 3, then the data is first written out
            // in a 32-bit format, then written out again in a 64-bit format.
            // Skip the 32-bit format and read the 64-bit one, as it can
            // describe a broader range of dates.

            var is64 = false;
            if (version > 1L)
            { 
                // Skip the 32-bit data.
                var skip = n[NTime] * 4L + n[NTime] + n[NZone] * 6L + n[NChar] + n[NLeap] * 8L + n[NStdWall] + n[NUTCLocal]; 
                // Skip the version 2 header that we just read.
                skip += 4L + 16L;
                d.read(skip);

                is64 = true; 

                // Read the counts again, they can differ.
                {
                    long i__prev1 = i;

                    for (i = 0L; i < 6L; i++)
                    {
                        (nn, ok) = d.big4();
                        if (!ok)
                        {
                            return (_addr_null!, error.As(badData)!);
                        }

                        if (uint32(int(nn)) != nn)
                        {
                            return (_addr_null!, error.As(badData)!);
                        }

                        n[i] = int(nn);

                    }


                    i = i__prev1;
                }

            }

            long size = 4L;
            if (is64)
            {
                size = 8L;
            } 

            // Transition times.
            dataIO txtimes = new dataIO(d.read(n[NTime]*size),false); 

            // Time zone indices for transition times.
            var txzones = d.read(n[NTime]); 

            // Zone info structures
            dataIO zonedata = new dataIO(d.read(n[NZone]*6),false); 

            // Time zone abbreviations.
            var abbrev = d.read(n[NChar]); 

            // Leap-second time pairs
            d.read(n[NLeap] * (size + 4L)); 

            // Whether tx times associated with local time types
            // are specified as standard time or wall time.
            var isstd = d.read(n[NStdWall]); 

            // Whether tx times associated with local time types
            // are specified as UTC or local time.
            var isutc = d.read(n[NUTCLocal]);

            if (d.error)
            { // ran out of data
                return (_addr_null!, error.As(badData)!);

            }

            @string extend = default;
            var rest = d.rest();
            if (len(rest) > 2L && rest[0L] == '\n' && rest[len(rest) - 1L] == '\n')
            {
                extend = string(rest[1L..len(rest) - 1L]);
            } 

            // Now we can build up a useful data structure.
            // First the zone information.
            //    utcoff[4] isdst[1] nameindex[1]
            var nzone = n[NZone];
            if (nzone == 0L)
            { 
                // Reject tzdata files with no zones. There's nothing useful in them.
                // This also avoids a panic later when we add and then use a fake transition (golang.org/issue/29437).
                return (_addr_null!, error.As(badData)!);

            }

            var zone = make_slice<zone>(nzone);
            {
                long i__prev1 = i;

                foreach (var (__i) in zone)
                {
                    i = __i;
                    bool ok = default;
                    n = default;
                    n, ok = zonedata.big4();

                    if (!ok)
                    {
                        return (_addr_null!, error.As(badData)!);
                    }

                    if (uint32(int(n)) != n)
                    {
                        return (_addr_null!, error.As(badData)!);
                    }

                    zone[i].offset = int(int32(n));
                    byte b = default;
                    b, ok = zonedata.@byte();

                    if (!ok)
                    {
                        return (_addr_null!, error.As(badData)!);
                    }

                    zone[i].isDST = b != 0L;
                    b, ok = zonedata.@byte();

                    if (!ok || int(b) >= len(abbrev))
                    {
                        return (_addr_null!, error.As(badData)!);
                    }

                    zone[i].name = byteString(abbrev[b..]);
                    if (runtime.GOOS == "aix" && len(name) > 8L && (name[..8L] == "Etc/GMT+" || name[..8L] == "Etc/GMT-"))
                    { 
                        // There is a bug with AIX 7.2 TL 0 with files in Etc,
                        // GMT+1 will return GMT-1 instead of GMT+1 or -01.
                        if (name != "Etc/GMT+0")
                        { 
                            // GMT+0 is OK
                            zone[i].name = name[4L..];

                        }

                    }

                } 

                // Now the transition time info.

                i = i__prev1;
            }

            var tx = make_slice<zoneTrans>(n[NTime]);
            {
                long i__prev1 = i;

                foreach (var (__i) in tx)
                {
                    i = __i;
                    n = default;
                    if (!is64)
                    {
                        {
                            var (n4, ok) = txtimes.big4();

                            if (!ok)
                            {
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

                            if (!ok)
                            {
                                return (_addr_null!, error.As(badData)!);
                            }
                            else
                            {
                                n = int64(n8);
                            }

                        }

                    }

                    tx[i].when = n;
                    if (int(txzones[i]) >= len(zone))
                    {
                        return (_addr_null!, error.As(badData)!);
                    }

                    tx[i].index = txzones[i];
                    if (i < len(isstd))
                    {
                        tx[i].isstd = isstd[i] != 0L;
                    }

                    if (i < len(isutc))
                    {
                        tx[i].isutc = isutc[i] != 0L;
                    }

                }

                i = i__prev1;
            }

            if (len(tx) == 0L)
            { 
                // Build fake transition to cover all time.
                // This happens in fixed locations like "Etc/GMT0".
                tx = append(tx, new zoneTrans(when:alpha,index:0));

            } 

            // Committed to succeed.
            ptr<Location> l = addr(new Location(zone:zone,tx:tx,name:name,extend:extend)); 

            // Fill in the cache with information about right now,
            // since that will be the most common lookup.
            var (sec, _, _) = now();
            {
                long i__prev1 = i;

                foreach (var (__i) in tx)
                {
                    i = __i;
                    if (tx[i].when <= sec && (i + 1L == len(tx) || sec < tx[i + 1L].when))
                    {
                        l.cacheStart = tx[i].when;
                        l.cacheEnd = omega;
                        if (i + 1L < len(tx))
                        {
                            l.cacheEnd = tx[i + 1L].when;
                        }

                        l.cacheZone = _addr_l.zone[tx[i].index];

                    }

                }

                i = i__prev1;
            }

            return (_addr_l!, error.As(null!)!);

        }

        // loadTzinfoFromDirOrZip returns the contents of the file with the given name
        // in dir. dir can either be an uncompressed zip file, or a directory.
        private static (slice<byte>, error) loadTzinfoFromDirOrZip(@string dir, @string name)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;

            if (len(dir) > 4L && dir[len(dir) - 4L..] == ".zip")
            {
                return loadTzinfoFromZip(dir, name);
            }

            if (dir != "")
            {
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
        private static long get4(slice<byte> b)
        {
            if (len(b) < 4L)
            {
                return 0L;
            }

            return int(b[0L]) | int(b[1L]) << (int)(8L) | int(b[2L]) << (int)(16L) | int(b[3L]) << (int)(24L);

        }

        // get2 returns the little-endian 16-bit value in b.
        private static long get2(slice<byte> b)
        {
            if (len(b) < 2L)
            {
                return 0L;
            }

            return int(b[0L]) | int(b[1L]) << (int)(8L);

        }

        // loadTzinfoFromZip returns the contents of the file with the given name
        // in the given uncompressed zip file.
        private static (slice<byte>, error) loadTzinfoFromZip(@string zipfile, @string name) => func((defer, _, __) =>
        {
            slice<byte> _p0 = default;
            error _p0 = default!;

            var (fd, err) = open(zipfile);
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            defer(closefd(fd));

            const ulong zecheader = (ulong)0x06054b50UL;
            const ulong zcheader = (ulong)0x02014b50UL;
            const long ztailsize = (long)22L;

            const long zheadersize = (long)30L;
            const ulong zheader = (ulong)0x04034b50UL;


            var buf = make_slice<byte>(ztailsize);
            {
                var err__prev1 = err;

                var err = preadn(fd, buf, -ztailsize);

                if (err != null || get4(buf) != zecheader)
                {
                    return (null, error.As(errors.New("corrupt zip file " + zipfile))!);
                }

                err = err__prev1;

            }

            var n = get2(buf[10L..]);
            var size = get4(buf[12L..]);
            var off = get4(buf[16L..]);

            buf = make_slice<byte>(size);
            {
                var err__prev1 = err;

                err = preadn(fd, buf, off);

                if (err != null)
                {
                    return (null, error.As(errors.New("corrupt zip file " + zipfile))!);
                }

                err = err__prev1;

            }


            for (long i = 0L; i < n; i++)
            { 
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
                if (get4(buf) != zcheader)
                {
                    break;
                }

                var meth = get2(buf[10L..]);
                size = get4(buf[24L..]);
                var namelen = get2(buf[28L..]);
                var xlen = get2(buf[30L..]);
                var fclen = get2(buf[32L..]);
                off = get4(buf[42L..]);
                var zname = buf[46L..46L + namelen];
                buf = buf[46L + namelen + xlen + fclen..];
                if (string(zname) != name)
                {
                    continue;
                }

                if (meth != 0L)
                {
                    return (null, error.As(errors.New("unsupported compression for " + name + " in " + zipfile))!);
                } 

                // zip per-file header layout:
                //    0    magic[4]
                //    4    extvers[1]
                //    5    extos[1]
                //    6    flags[2]
                //    8    meth[2]
                //    10    modtime[2]
                //    12    moddate[2]
                //    14    crc[4]
                //    18    csize[4]
                //    22    uncsize[4]
                //    26    namelen[2]
                //    28    xlen[2]
                //    30    name[namelen]
                //    30+namelen+xlen - file data
                //
                buf = make_slice<byte>(zheadersize + namelen);
                {
                    var err__prev1 = err;

                    err = preadn(fd, buf, off);

                    if (err != null || get4(buf) != zheader || get2(buf[8L..]) != meth || get2(buf[26L..]) != namelen || string(buf[30L..30L + namelen]) != name)
                    {
                        return (null, error.As(errors.New("corrupt zip file " + zipfile))!);
                    }

                    err = err__prev1;

                }

                xlen = get2(buf[28L..]);

                buf = make_slice<byte>(size);
                {
                    var err__prev1 = err;

                    err = preadn(fd, buf, off + 30L + namelen + xlen);

                    if (err != null)
                    {
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
        private static (slice<byte>, error) loadTzinfo(@string name, @string source)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;

            if (len(source) >= 6L && source[len(source) - 6L..] == "tzdata")
            {
                return loadTzinfoFromTzdata(source, name);
            }

            return loadTzinfoFromDirOrZip(source, name);

        }

        // loadLocation returns the Location with the given name from one of
        // the specified sources. See loadTzinfo for a list of supported sources.
        // The first timezone data matching the given name that is successfully loaded
        // and parsed is returned as a Location.
        private static (ptr<Location>, error) loadLocation(@string name, slice<@string> sources)
        {
            ptr<Location> z = default!;
            error firstErr = default!;

            foreach (var (_, source) in sources)
            {

                if (err == null)
                {
                    z, err = LoadLocationFromTZData(name, zoneData);

                    if (err == null)
                    {
                        return (_addr_z!, error.As(null!)!);
                    }

                }

                if (firstErr == null && err != syscall.ENOENT)
                {
                    firstErr = err;
                }

            }
            if (loadFromEmbeddedTZData != null)
            {
                var (zonedata, err) = loadFromEmbeddedTZData(name);
                if (err == null)
                {
                    z, err = LoadLocationFromTZData(name, (slice<byte>)zonedata);

                    if (err == null)
                    {
                        return (_addr_z!, error.As(null!)!);
                    }

                }

                if (firstErr == null && err != syscall.ENOENT)
                {
                    firstErr = err;
                }

            }

            if (firstErr != null)
            {
                return (_addr_null!, error.As(firstErr)!);
            }

            return (_addr_null!, error.As(errors.New("unknown time zone " + name))!);

        }

        // readFile reads and returns the content of the named file.
        // It is a trivial implementation of ioutil.ReadFile, reimplemented
        // here to avoid depending on io/ioutil or os.
        // It returns an error if name exceeds maxFileSize bytes.
        private static (slice<byte>, error) readFile(@string name) => func((defer, _, __) =>
        {
            slice<byte> _p0 = default;
            error _p0 = default!;

            var (f, err) = open(name);
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            defer(closefd(f));
            array<byte> buf = new array<byte>(4096L);            slice<byte> ret = default;            long n = default;
            while (true)
            {
                n, err = read(f, buf[..]);
                if (n > 0L)
                {
                    ret = append(ret, buf[..n]);
                }

                if (n == 0L || err != null)
                {
                    break;
                }

                if (len(ret) > maxFileSize)
                {
                    return (null, error.As(fileSizeError(name))!);
                }

            }

            return (ret, error.As(err)!);

        });
    }
}
