// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Parse the "tzdata" packed timezone file used on Android.
// The format is lifted from ZoneInfoDB.java and ZoneInfo.java in
// java/libcore/util in the AOSP.

// package time -- go2cs converted at 2020 October 08 03:45:50 UTC
// import "time" ==> using time = go.time_package
// Original source: C:\Go\src\time\zoneinfo_android.go
using errors = go.errors_package;
using runtime = go.runtime_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go
{
    public static partial class time_package
    {
        private static @string zoneSources = new slice<@string>(new @string[] { "/system/usr/share/zoneinfo/tzdata", "/data/misc/zoneinfo/current/tzdata", runtime.GOROOT()+"/lib/time/zoneinfo.zip" });

        private static void initLocal()
        { 
            // TODO(elias.naur): getprop persist.sys.timezone
            localLoc = UTC.val;

        }

        private static void init()
        {
            loadTzinfoFromTzdata = androidLoadTzinfoFromTzdata;
        }

        private static (slice<byte>, error) androidLoadTzinfoFromTzdata(@string file, @string name) => func((defer, _, __) =>
        {
            slice<byte> _p0 = default;
            error _p0 = default!;

            const long headersize = (long)12L + 3L * 4L;
            const long namesize = (long)40L;
            const var entrysize = (var)namesize + 3L * 4L;
            if (len(name) > namesize)
            {
                return (null, error.As(errors.New(name + " is longer than the maximum zone name length (40 bytes)"))!);
            }

            var (fd, err) = open(file);
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            defer(closefd(fd));

            var buf = make_slice<byte>(headersize);
            {
                var err__prev1 = err;

                var err = preadn(fd, buf, 0L);

                if (err != null)
                {
                    return (null, error.As(errors.New("corrupt tzdata file " + file))!);
                }

                err = err__prev1;

            }

            dataIO d = new dataIO(buf,false);
            {
                var magic = d.read(6L);

                if (string(magic) != "tzdata")
                {
                    return (null, error.As(errors.New("corrupt tzdata file " + file))!);
                }

            }

            d = new dataIO(buf[12:],false);
            var (indexOff, _) = d.big4();
            var (dataOff, _) = d.big4();
            var indexSize = dataOff - indexOff;
            var entrycount = indexSize / entrysize;
            buf = make_slice<byte>(indexSize);
            {
                var err__prev1 = err;

                err = preadn(fd, buf, int(indexOff));

                if (err != null)
                {
                    return (null, error.As(errors.New("corrupt tzdata file " + file))!);
                }

                err = err__prev1;

            }

            for (long i = 0L; i < int(entrycount); i++)
            {
                var entry = buf[i * entrysize..(i + 1L) * entrysize]; 
                // len(name) <= namesize is checked at function entry
                if (string(entry[..len(name)]) != name)
                {
                    continue;
                }

                d = new dataIO(entry[namesize:],false);
                var (off, _) = d.big4();
                var (size, _) = d.big4();
                buf = make_slice<byte>(size);
                {
                    var err__prev1 = err;

                    err = preadn(fd, buf, int(off + dataOff));

                    if (err != null)
                    {
                        return (null, error.As(errors.New("corrupt tzdata file " + file))!);
                    }

                    err = err__prev1;

                }

                return (buf, error.As(null!)!);

            }

            return (null, error.As(syscall.ENOENT)!);

        });
    }
}
