// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Parse "zoneinfo" time zone file.
// This is a fairly standard file format used on OS X, Linux, BSD, Sun, and others.
// See tzfile(5), http://en.wikipedia.org/wiki/Zoneinfo,
// and ftp://munnari.oz.au/pub/oldtz/

// package time -- go2cs converted at 2020 August 29 08:42:34 UTC
// import "time" ==> using time = go.time_package
// Original source: C:\Go\src\time\zoneinfo_read.go
using errors = go.errors_package;
using syscall = go.syscall_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class time_package
    {
        // maxFileSize is the max permitted size of files read by readFile.
        // As reference, the zoneinfo.zip distributed by Go is ~350 KB,
        // so 10MB is overkill.
        private static readonly long maxFileSize = 10L << (int)(20L);



        private partial struct fileSizeError // : @string
        {
        }

        private static @string Error(this fileSizeError f)
        {
            return "time: file " + string(f) + " is too large";
        }

        // Copies of io.Seek* constants to avoid importing "io":
        private static readonly long seekStart = 0L;
        private static readonly long seekCurrent = 1L;
        private static readonly long seekEnd = 2L;

        // Simple I/O interface to binary blob of data.
        private partial struct dataIO
        {
            public slice<byte> p;
            public bool error;
        }

        private static slice<byte> read(this ref dataIO d, long n)
        {
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

        private static (uint, bool) big4(this ref dataIO d)
        {
            var p = d.read(4L);
            if (len(p) < 4L)
            {
                d.error = true;
                return (0L, false);
            }
            return (uint32(p[0L]) << (int)(24L) | uint32(p[1L]) << (int)(16L) | uint32(p[2L]) << (int)(8L) | uint32(p[3L]), true);
        }

        private static (byte, bool) @byte(this ref dataIO d)
        {
            var p = d.read(1L);
            if (len(p) < 1L)
            {
                d.error = true;
                return (0L, false);
            }
            return (p[0L], true);
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
        public static (ref Location, error) LoadLocationFromTZData(@string name, slice<byte> data)
        {
            dataIO d = new dataIO(data,false); 

            // 4-byte magic "TZif"
            {
                var magic = d.read(4L);

                if (string(magic) != "TZif")
                {
                    return (null, badData);
                } 

                // 1-byte version, then 15 bytes of padding

            } 

            // 1-byte version, then 15 bytes of padding
            slice<byte> p = default;
            p = d.read(16L);

            if (len(p) != 16L || p[0L] != 0L && p[0L] != '2' && p[0L] != '3')
            {
                return (null, badData);
            } 

            // six big-endian 32-bit integers:
            //    number of UTC/local indicators
            //    number of standard/wall indicators
            //    number of leap seconds
            //    number of transition times
            //    number of local time zones
            //    number of characters of time zone abbrev strings
            const var NUTCLocal = iota;
            const var NStdWall = 0;
            const var NLeap = 1;
            const var NTime = 2;
            const var NZone = 3;
            const var NChar = 4;
            array<long> n = new array<long>(6L);
            {
                long i__prev1 = i;

                for (long i = 0L; i < 6L; i++)
                {
                    var (nn, ok) = d.big4();
                    if (!ok)
                    {
                        return (null, badData);
                    }
                    n[i] = int(nn);
                } 

                // Transition times.


                i = i__prev1;
            } 

            // Transition times.
            dataIO txtimes = new dataIO(d.read(n[NTime]*4),false); 

            // Time zone indices for transition times.
            var txzones = d.read(n[NTime]); 

            // Zone info structures
            dataIO zonedata = new dataIO(d.read(n[NZone]*6),false); 

            // Time zone abbreviations.
            var abbrev = d.read(n[NChar]); 

            // Leap-second time pairs
            d.read(n[NLeap] * 8L); 

            // Whether tx times associated with local time types
            // are specified as standard time or wall time.
            var isstd = d.read(n[NStdWall]); 

            // Whether tx times associated with local time types
            // are specified as UTC or local time.
            var isutc = d.read(n[NUTCLocal]);

            if (d.error)
            { // ran out of data
                return (null, badData);
            } 

            // If version == 2 or 3, the entire file repeats, this time using
            // 8-byte ints for txtimes and leap seconds.
            // We won't need those until 2106.

            // Now we can build up a useful data structure.
            // First the zone information.
            //    utcoff[4] isdst[1] nameindex[1]
            var zone = make_slice<zone>(n[NZone]);
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
                        return (null, badData);
                    }
                    zone[i].offset = int(int32(n));
                    byte b = default;
                    b, ok = zonedata.@byte();

                    if (!ok)
                    {
                        return (null, badData);
                    }
                    zone[i].isDST = b != 0L;
                    b, ok = zonedata.@byte();

                    if (!ok || int(b) >= len(abbrev))
                    {
                        return (null, badData);
                    }
                    zone[i].name = byteString(abbrev[b..]);
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
                    ok = default;
                    n = default;
                    n, ok = txtimes.big4();

                    if (!ok)
                    {
                        return (null, badData);
                    }
                    tx[i].when = int64(int32(n));
                    if (int(txzones[i]) >= len(zone))
                    {
                        return (null, badData);
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
            Location l = ref new Location(zone:zone,tx:tx,name:name); 

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
                        l.cacheZone = ref l.zone[tx[i].index];
                    }
                }

                i = i__prev1;
            }

            return (l, null);
        }

        // loadTzinfoFromDirOrZip returns the contents of the file with the given name
        // in dir. dir can either be an uncompressed zip file, or a directory.
        private static (slice<byte>, error) loadTzinfoFromDirOrZip(@string dir, @string name)
        {
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
            var (fd, err) = open(zipfile);
            if (err != null)
            {
                return (null, errors.New("open " + zipfile + ": " + err.Error()));
            }
            defer(closefd(fd));

            const ulong zecheader = 0x06054b50UL;
            const ulong zcheader = 0x02014b50UL;
            const long ztailsize = 22L;

            const long zheadersize = 30L;
            const ulong zheader = 0x04034b50UL;

            var buf = make_slice<byte>(ztailsize);
            {
                var err__prev1 = err;

                var err = preadn(fd, buf, -ztailsize);

                if (err != null || get4(buf) != zecheader)
                {
                    return (null, errors.New("corrupt zip file " + zipfile));
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
                    return (null, errors.New("corrupt zip file " + zipfile));
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
                    return (null, errors.New("unsupported compression for " + name + " in " + zipfile));
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
                        return (null, errors.New("corrupt zip file " + zipfile));
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
                        return (null, errors.New("corrupt zip file " + zipfile));
                    }

                    err = err__prev1;

                }

                return (buf, null);
            }


            return (null, errors.New("cannot find " + name + " in zip file " + zipfile));
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
        private static (ref Location, error) loadLocation(@string name, slice<@string> sources)
        {
            foreach (var (_, source) in sources)
            {

                if (err == null)
                {
                    z, err = LoadLocationFromTZData(name, zoneData);

                    if (err == null)
                    {
                        return (z, null);
                    }
                }
                if (firstErr == null && err != syscall.ENOENT)
                {
                    firstErr = err;
                }
            }
            if (firstErr != null)
            {
                return (null, firstErr);
            }
            return (null, errors.New("unknown time zone " + name));
        }

        // readFile reads and returns the content of the named file.
        // It is a trivial implementation of ioutil.ReadFile, reimplemented
        // here to avoid depending on io/ioutil or os.
        // It returns an error if name exceeds maxFileSize bytes.
        private static (slice<byte>, error) readFile(@string name) => func((defer, _, __) =>
        {
            var (f, err) = open(name);
            if (err != null)
            {
                return (null, err);
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
                    return (null, fileSizeError(name));
                }
            }

            return (ret, err);
        });
    }
}
