// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package time -- go2cs converted at 2020 October 09 05:06:11 UTC
// import "time" ==> using time = go.time_package
// Original source: C:\Go\src\time\zoneinfo.go
using errors = go.errors_package;
using sync = go.sync_package;
using syscall = go.syscall_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class time_package
    {
        //go:generate env ZONEINFO=$GOROOT/lib/time/zoneinfo.zip go run genzabbrs.go -output zoneinfo_abbrs_windows.go

        // A Location maps time instants to the zone in use at that time.
        // Typically, the Location represents the collection of time offsets
        // in use in a geographical area. For many Locations the time offset varies
        // depending on whether daylight savings time is in use at the time instant.
        public partial struct Location
        {
            public @string name;
            public slice<zone> zone;
            public slice<zoneTrans> tx; // The tzdata information can be followed by a string that describes
// how to handle DST transitions not recorded in zoneTrans.
// The format is the TZ environment variable without a colon; see
// https://pubs.opengroup.org/onlinepubs/9699919799/basedefs/V1_chap08.html.
// Example string, for America/Los_Angeles: PST8PDT,M3.2.0,M11.1.0
            public @string extend; // Most lookups will be for the current time.
// To avoid the binary search through tx, keep a
// static one-element cache that gives the correct
// zone for the time when the Location was created.
// if cacheStart <= t < cacheEnd,
// lookup can return cacheZone.
// The units for cacheStart and cacheEnd are seconds
// since January 1, 1970 UTC, to match the argument
// to lookup.
            public long cacheStart;
            public long cacheEnd;
            public ptr<zone> cacheZone;
        }

        // A zone represents a single time zone such as CET.
        private partial struct zone
        {
            public @string name; // abbreviated name, "CET"
            public long offset; // seconds east of UTC
            public bool isDST; // is this zone Daylight Savings Time?
        }

        // A zoneTrans represents a single time zone transition.
        private partial struct zoneTrans
        {
            public long when; // transition time, in seconds since 1970 GMT
            public byte index; // the index of the zone that goes into effect at that time
            public bool isstd; // ignored - no idea what these mean
            public bool isutc; // ignored - no idea what these mean
        }

        // alpha and omega are the beginning and end of time for zone
        // transitions.
        private static readonly long alpha = (long)-1L << (int)(63L); // math.MinInt64
        private static readonly long omega = (long)1L << (int)(63L) - 1L; // math.MaxInt64

        // UTC represents Universal Coordinated Time (UTC).
        public static ptr<Location> UTC_addr_utcLoc;

        // utcLoc is separate so that get can refer to &utcLoc
        // and ensure that it never returns a nil *Location,
        // even if a badly behaved client has changed UTC.
        private static Location utcLoc = new Location(name:"UTC");

        // Local represents the system's local time zone.
        // On Unix systems, Local consults the TZ environment
        // variable to find the time zone to use. No TZ means
        // use the system default /etc/localtime.
        // TZ="" means use UTC.
        // TZ="foo" means use file foo in the system timezone directory.
        public static ptr<Location> Local_addr_localLoc;

        // localLoc is separate so that initLocal can initialize
        // it even if a client has changed Local.
        private static Location localLoc = default;
        private static sync.Once localOnce = default;

        private static ptr<Location> get(this ptr<Location> _addr_l)
        {
            ref Location l = ref _addr_l.val;

            if (l == null)
            {
                return _addr__addr_utcLoc!;
            }

            if (l == _addr_localLoc)
            {
                localOnce.Do(initLocal);
            }

            return _addr_l!;

        }

        // String returns a descriptive name for the time zone information,
        // corresponding to the name argument to LoadLocation or FixedZone.
        private static @string String(this ptr<Location> _addr_l)
        {
            ref Location l = ref _addr_l.val;

            return l.get().name;
        }

        // FixedZone returns a Location that always uses
        // the given zone name and offset (seconds east of UTC).
        public static ptr<Location> FixedZone(@string name, long offset)
        {
            ptr<Location> l = addr(new Location(name:name,zone:[]zone{{name,offset,false}},tx:[]zoneTrans{{alpha,0,false,false}},cacheStart:alpha,cacheEnd:omega,));
            l.cacheZone = _addr_l.zone[0L];
            return _addr_l!;
        }

        // lookup returns information about the time zone in use at an
        // instant in time expressed as seconds since January 1, 1970 00:00:00 UTC.
        //
        // The returned information gives the name of the zone (such as "CET"),
        // the start and end times bracketing sec when that zone is in effect,
        // the offset in seconds east of UTC (such as -5*60*60), and whether
        // the daylight savings is being observed at that time.
        private static (@string, long, long, long) lookup(this ptr<Location> _addr_l, long sec)
        {
            @string name = default;
            long offset = default;
            long start = default;
            long end = default;
            ref Location l = ref _addr_l.val;

            l = l.get();

            if (len(l.zone) == 0L)
            {
                name = "UTC";
                offset = 0L;
                start = alpha;
                end = omega;
                return ;
            }

            {
                var zone__prev1 = zone;

                var zone = l.cacheZone;

                if (zone != null && l.cacheStart <= sec && sec < l.cacheEnd)
                {
                    name = zone.name;
                    offset = zone.offset;
                    start = l.cacheStart;
                    end = l.cacheEnd;
                    return ;
                }

                zone = zone__prev1;

            }


            if (len(l.tx) == 0L || sec < l.tx[0L].when)
            {
                zone = _addr_l.zone[l.lookupFirstZone()];
                name = zone.name;
                offset = zone.offset;
                start = alpha;
                if (len(l.tx) > 0L)
                {
                    end = l.tx[0L].when;
                }
                else
                {
                    end = omega;
                }

                return ;

            } 

            // Binary search for entry with largest time <= sec.
            // Not using sort.Search to avoid dependencies.
            var tx = l.tx;
            end = omega;
            long lo = 0L;
            var hi = len(tx);
            while (hi - lo > 1L)
            {
                var m = lo + (hi - lo) / 2L;
                var lim = tx[m].when;
                if (sec < lim)
                {
                    end = lim;
                    hi = m;
                }
                else
                {
                    lo = m;
                }

            }

            zone = _addr_l.zone[tx[lo].index];
            name = zone.name;
            offset = zone.offset;
            start = tx[lo].when; 
            // end = maintained during the search

            // If we're at the end of the known zone transitions,
            // try the extend string.
            if (lo == len(tx) - 1L && l.extend != "")
            {
                {
                    var (ename, eoffset, estart, eend, ok) = tzset(l.extend, end, sec);

                    if (ok)
                    {
                        return (ename, eoffset, estart, eend);
                    }

                }

            }

            return ;

        }

        // lookupFirstZone returns the index of the time zone to use for times
        // before the first transition time, or when there are no transition
        // times.
        //
        // The reference implementation in localtime.c from
        // https://www.iana.org/time-zones/repository/releases/tzcode2013g.tar.gz
        // implements the following algorithm for these cases:
        // 1) If the first zone is unused by the transitions, use it.
        // 2) Otherwise, if there are transition times, and the first
        //    transition is to a zone in daylight time, find the first
        //    non-daylight-time zone before and closest to the first transition
        //    zone.
        // 3) Otherwise, use the first zone that is not daylight time, if
        //    there is one.
        // 4) Otherwise, use the first zone.
        private static long lookupFirstZone(this ptr<Location> _addr_l)
        {
            ref Location l = ref _addr_l.val;
 
            // Case 1.
            if (!l.firstZoneUsed())
            {
                return 0L;
            } 

            // Case 2.
            if (len(l.tx) > 0L && l.zone[l.tx[0L].index].isDST)
            {
                {
                    var zi__prev1 = zi;

                    for (var zi = int(l.tx[0L].index) - 1L; zi >= 0L; zi--)
                    {
                        if (!l.zone[zi].isDST)
                        {
                            return zi;
                        }

                    }


                    zi = zi__prev1;
                }

            } 

            // Case 3.
            {
                var zi__prev1 = zi;

                foreach (var (__zi) in l.zone)
                {
                    zi = __zi;
                    if (!l.zone[zi].isDST)
                    {
                        return zi;
                    }

                } 

                // Case 4.

                zi = zi__prev1;
            }

            return 0L;

        }

        // firstZoneUsed reports whether the first zone is used by some
        // transition.
        private static bool firstZoneUsed(this ptr<Location> _addr_l)
        {
            ref Location l = ref _addr_l.val;

            foreach (var (_, tx) in l.tx)
            {
                if (tx.index == 0L)
                {
                    return true;
                }

            }
            return false;

        }

        // tzset takes a timezone string like the one found in the TZ environment
        // variable, the end of the last time zone transition expressed as seconds
        // since January 1, 1970 00:00:00 UTC, and a time expressed the same way.
        // We call this a tzset string since in C the function tzset reads TZ.
        // The return values are as for lookup, plus ok which reports whether the
        // parse succeeded.
        private static (@string, long, long, long, bool) tzset(@string s, long initEnd, long sec)
        {
            @string name = default;
            long offset = default;
            long start = default;
            long end = default;
            bool ok = default;

            @string stdName = default;            @string dstName = default;
            long stdOffset = default;            long dstOffset = default;

            stdName, s, ok = tzsetName(s);
            if (ok)
            {
                stdOffset, s, ok = tzsetOffset(s);
            }

            if (!ok)
            {
                return ("", 0L, 0L, 0L, false);
            } 

            // The numbers in the tzset string are added to local time to get UTC,
            // but our offsets are added to UTC to get local time,
            // so we negate the number we see here.
            stdOffset = -stdOffset;

            if (len(s) == 0L || s[0L] == ',')
            { 
                // No daylight savings time.
                return (stdName, stdOffset, initEnd, omega, true);

            }

            dstName, s, ok = tzsetName(s);
            if (ok)
            {
                if (len(s) == 0L || s[0L] == ',')
                {
                    dstOffset = stdOffset + secondsPerHour;
                }
                else
                {
                    dstOffset, s, ok = tzsetOffset(s);
                    dstOffset = -dstOffset; // as with stdOffset, above
                }

            }

            if (!ok)
            {
                return ("", 0L, 0L, 0L, false);
            }

            if (len(s) == 0L)
            { 
                // Default DST rules per tzcode.
                s = ",M3.2.0,M11.1.0";

            } 
            // The TZ definition does not mention ';' here but tzcode accepts it.
            if (s[0L] != ',' && s[0L] != ';')
            {
                return ("", 0L, 0L, 0L, false);
            }

            s = s[1L..];

            rule startRule = default;            rule endRule = default;

            startRule, s, ok = tzsetRule(s);
            if (!ok || len(s) == 0L || s[0L] != ',')
            {
                return ("", 0L, 0L, 0L, false);
            }

            s = s[1L..];
            endRule, s, ok = tzsetRule(s);
            if (!ok || len(s) > 0L)
            {
                return ("", 0L, 0L, 0L, false);
            }

            var (year, _, _, yday) = absDate(uint64(sec + unixToInternal + internalToAbsolute), false);

            var ysec = int64(yday * secondsPerDay) + sec % secondsPerDay; 

            // Compute start of year in seconds since Unix epoch.
            var d = daysSinceEpoch(year);
            var abs = int64(d * secondsPerDay);
            abs += absoluteToInternal + internalToUnix;

            var startSec = int64(tzruleTime(year, startRule, stdOffset));
            var endSec = int64(tzruleTime(year, endRule, dstOffset));
            if (endSec < startSec)
            {
                startSec = endSec;
                endSec = startSec;
                stdName = dstName;
                dstName = stdName;
                stdOffset = dstOffset;
                dstOffset = stdOffset;

            } 

            // The start and end values that we return are accurate
            // close to a daylight savings transition, but are otherwise
            // just the start and end of the year. That suffices for
            // the only caller that cares, which is Date.
            if (ysec < startSec)
            {
                return (stdName, stdOffset, abs, startSec + abs, true);
            }
            else if (ysec >= endSec)
            {
                return (stdName, stdOffset, endSec + abs, abs + 365L * secondsPerDay, true);
            }
            else
            {
                return (dstName, dstOffset, startSec + abs, endSec + abs, true);
            }

        }

        // tzsetName returns the timezone name at the start of the tzset string s,
        // and the remainder of s, and reports whether the parsing is OK.
        private static (@string, @string, bool) tzsetName(@string s)
        {
            @string _p0 = default;
            @string _p0 = default;
            bool _p0 = default;

            if (len(s) == 0L)
            {
                return ("", "", false);
            }

            if (s[0L] != '<')
            {
                {
                    var i__prev1 = i;
                    var r__prev1 = r;

                    foreach (var (__i, __r) in s)
                    {
                        i = __i;
                        r = __r;
                        switch (r)
                        {
                            case '0': 

                            case '1': 

                            case '2': 

                            case '3': 

                            case '4': 

                            case '5': 

                            case '6': 

                            case '7': 

                            case '8': 

                            case '9': 

                            case ',': 

                            case '-': 

                            case '+': 
                                if (i < 3L)
                                {
                                    return ("", "", false);
                                }

                                return (s[..i], s[i..], true);
                                break;
                        }

                    }
            else

                    i = i__prev1;
                    r = r__prev1;
                }

                if (len(s) < 3L)
                {
                    return ("", "", false);
                }

                return (s, "", true);

            }            {
                {
                    var i__prev1 = i;
                    var r__prev1 = r;

                    foreach (var (__i, __r) in s)
                    {
                        i = __i;
                        r = __r;
                        if (r == '>')
                        {
                            return (s[1L..i], s[i + 1L..], true);
                        }

                    }

                    i = i__prev1;
                    r = r__prev1;
                }

                return ("", "", false);

            }

        }

        // tzsetOffset returns the timezone offset at the start of the tzset string s,
        // and the remainder of s, and reports whether the parsing is OK.
        // The timezone offset is returned as a number of seconds.
        private static (long, @string, bool) tzsetOffset(@string s)
        {
            long offset = default;
            @string rest = default;
            bool ok = default;

            if (len(s) == 0L)
            {
                return (0L, "", false);
            }

            var neg = false;
            if (s[0L] == '+')
            {
                s = s[1L..];
            }
            else if (s[0L] == '-')
            {
                s = s[1L..];
                neg = true;
            }

            long hours = default;
            hours, s, ok = tzsetNum(s, 0L, 24L);
            if (!ok)
            {
                return (0L, "", false);
            }

            var off = hours * secondsPerHour;
            if (len(s) == 0L || s[0L] != ':')
            {
                if (neg)
                {
                    off = -off;
                }

                return (off, s, true);

            }

            long mins = default;
            mins, s, ok = tzsetNum(s[1L..], 0L, 59L);
            if (!ok)
            {
                return (0L, "", false);
            }

            off += mins * secondsPerMinute;
            if (len(s) == 0L || s[0L] != ':')
            {
                if (neg)
                {
                    off = -off;
                }

                return (off, s, true);

            }

            long secs = default;
            secs, s, ok = tzsetNum(s[1L..], 0L, 59L);
            if (!ok)
            {
                return (0L, "", false);
            }

            off += secs;

            if (neg)
            {
                off = -off;
            }

            return (off, s, true);

        }

        // ruleKind is the kinds of rules that can be seen in a tzset string.
        private partial struct ruleKind // : long
        {
        }

        private static readonly ruleKind ruleJulian = (ruleKind)iota;
        private static readonly var ruleDOY = 0;
        private static readonly var ruleMonthWeekDay = 1;


        // rule is a rule read from a tzset string.
        private partial struct rule
        {
            public ruleKind kind;
            public long day;
            public long week;
            public long mon;
            public long time; // transition time
        }

        // tzsetRule parses a rule from a tzset string.
        // It returns the rule, and the remainder of the string, and reports success.
        private static (rule, @string, bool) tzsetRule(@string s)
        {
            rule _p0 = default;
            @string _p0 = default;
            bool _p0 = default;

            rule r = default;
            if (len(s) == 0L)
            {
                return (new rule(), "", false);
            }

            var ok = false;
            if (s[0L] == 'J')
            {
                long jday = default;
                jday, s, ok = tzsetNum(s[1L..], 1L, 365L);
                if (!ok)
                {
                    return (new rule(), "", false);
                }

                r.kind = ruleJulian;
                r.day = jday;

            }
            else if (s[0L] == 'M')
            {
                long mon = default;
                mon, s, ok = tzsetNum(s[1L..], 1L, 12L);
                if (!ok || len(s) == 0L || s[0L] != '.')
                {
                    return (new rule(), "", false);
                }

                long week = default;
                week, s, ok = tzsetNum(s[1L..], 1L, 5L);
                if (!ok || len(s) == 0L || s[0L] != '.')
                {
                    return (new rule(), "", false);
                }

                long day = default;
                day, s, ok = tzsetNum(s[1L..], 0L, 6L);
                if (!ok)
                {
                    return (new rule(), "", false);
                }

                r.kind = ruleMonthWeekDay;
                r.day = day;
                r.week = week;
                r.mon = mon;

            }
            else
            {
                day = default;
                day, s, ok = tzsetNum(s, 0L, 365L);
                if (!ok)
                {
                    return (new rule(), "", false);
                }

                r.kind = ruleDOY;
                r.day = day;

            }

            if (len(s) == 0L || s[0L] != '/')
            {
                r.time = 2L * secondsPerHour; // 2am is the default
                return (r, s, true);

            }

            var (offset, s, ok) = tzsetOffset(s[1L..]);
            if (!ok || offset < 0L)
            {
                return (new rule(), "", false);
            }

            r.time = offset;

            return (r, s, true);

        }

        // tzsetNum parses a number from a tzset string.
        // It returns the number, and the remainder of the string, and reports success.
        // The number must be between min and max.
        private static (long, @string, bool) tzsetNum(@string s, long min, long max)
        {
            long num = default;
            @string rest = default;
            bool ok = default;

            if (len(s) == 0L)
            {
                return (0L, "", false);
            }

            num = 0L;
            foreach (var (i, r) in s)
            {
                if (r < '0' || r > '9')
                {
                    if (i == 0L || num < min)
                    {
                        return (0L, "", false);
                    }

                    return (num, s[i..], true);

                }

                num *= 10L;
                num += int(r) - '0';
                if (num > max)
                {
                    return (0L, "", false);
                }

            }
            if (num < min)
            {
                return (0L, "", false);
            }

            return (num, "", true);

        }

        // tzruleTime takes a year, a rule, and a timezone offset,
        // and returns the number of seconds since the start of the year
        // that the rule takes effect.
        private static long tzruleTime(long year, rule r, long off)
        {
            long s = default;

            if (r.kind == ruleJulian) 
                s = (r.day - 1L) * secondsPerDay;
                if (isLeap(year) && r.day >= 60L)
                {
                    s += secondsPerDay;
                }

            else if (r.kind == ruleDOY) 
                s = r.day * secondsPerDay;
            else if (r.kind == ruleMonthWeekDay) 
                // Zeller's Congruence.
                var m1 = (r.mon + 9L) % 12L + 1L;
                var yy0 = year;
                if (r.mon <= 2L)
                {
                    yy0--;
                }

                var yy1 = yy0 / 100L;
                var yy2 = yy0 % 100L;
                long dow = ((26L * m1 - 2L) / 10L + 1L + yy2 + yy2 / 4L + yy1 / 4L - 2L * yy1) % 7L;
                if (dow < 0L)
                {
                    dow += 7L;
                } 
                // Now dow is the day-of-week of the first day of r.mon.
                // Get the day-of-month of the first "dow" day.
                var d = r.day - dow;
                if (d < 0L)
                {
                    d += 7L;
                }

                for (long i = 1L; i < r.week; i++)
                {
                    if (d + 7L >= daysIn(Month(r.mon), year))
                    {
                        break;
                    }

                    d += 7L;

                }

                d += int(daysBefore[r.mon - 1L]);
                if (isLeap(year) && r.mon > 2L)
                {
                    d++;
                }

                s = d * secondsPerDay;
                        return s + r.time - off;

        }

        // lookupName returns information about the time zone with
        // the given name (such as "EST") at the given pseudo-Unix time
        // (what the given time of day would be in UTC).
        private static (long, bool) lookupName(this ptr<Location> _addr_l, @string name, long unix)
        {
            long offset = default;
            bool ok = default;
            ref Location l = ref _addr_l.val;

            l = l.get(); 

            // First try for a zone with the right name that was actually
            // in effect at the given time. (In Sydney, Australia, both standard
            // and daylight-savings time are abbreviated "EST". Using the
            // offset helps us pick the right one for the given time.
            // It's not perfect: during the backward transition we might pick
            // either one.)
            {
                var i__prev1 = i;

                foreach (var (__i) in l.zone)
                {
                    i = __i;
                    var zone = _addr_l.zone[i];
                    if (zone.name == name)
                    {
                        var (nam, offset, _, _) = l.lookup(unix - int64(zone.offset));
                        if (nam == zone.name)
                        {
                            return (offset, true);
                        }

                    }

                } 

                // Otherwise fall back to an ordinary name match.

                i = i__prev1;
            }

            {
                var i__prev1 = i;

                foreach (var (__i) in l.zone)
                {
                    i = __i;
                    zone = _addr_l.zone[i];
                    if (zone.name == name)
                    {
                        return (zone.offset, true);
                    }

                } 

                // Otherwise, give up.

                i = i__prev1;
            }

            return ;

        }

        // NOTE(rsc): Eventually we will need to accept the POSIX TZ environment
        // syntax too, but I don't feel like implementing it today.

        private static var errLocation = errors.New("time: invalid location name");

        private static ptr<@string> zoneinfo;
        private static sync.Once zoneinfoOnce = default;

        // LoadLocation returns the Location with the given name.
        //
        // If the name is "" or "UTC", LoadLocation returns UTC.
        // If the name is "Local", LoadLocation returns Local.
        //
        // Otherwise, the name is taken to be a location name corresponding to a file
        // in the IANA Time Zone database, such as "America/New_York".
        //
        // The time zone database needed by LoadLocation may not be
        // present on all systems, especially non-Unix systems.
        // LoadLocation looks in the directory or uncompressed zip file
        // named by the ZONEINFO environment variable, if any, then looks in
        // known installation locations on Unix systems,
        // and finally looks in $GOROOT/lib/time/zoneinfo.zip.
        public static (ptr<Location>, error) LoadLocation(@string name)
        {
            ptr<Location> _p0 = default!;
            error _p0 = default!;

            if (name == "" || name == "UTC")
            {
                return (_addr_UTC!, error.As(null!)!);
            }

            if (name == "Local")
            {
                return (_addr_Local!, error.As(null!)!);
            }

            if (containsDotDot(name) || name[0L] == '/' || name[0L] == '\\')
            { 
                // No valid IANA Time Zone name contains a single dot,
                // much less dot dot. Likewise, none begin with a slash.
                return (_addr_null!, error.As(errLocation)!);

            }

            zoneinfoOnce.Do(() =>
            {
                var (env, _) = syscall.Getenv("ZONEINFO");
                zoneinfo = _addr_env;
            });
            error firstErr = default!;
            if (zoneinfo != "".val)
            {
                {
                    var (zoneData, err) = loadTzinfoFromDirOrZip(zoneinfo.val, name);

                    if (err == null)
                    {
                        {
                            var z__prev3 = z;

                            var (z, err) = LoadLocationFromTZData(name, zoneData);

                            if (err == null)
                            {
                                return (_addr_z!, error.As(null!)!);
                            }

                            z = z__prev3;

                        }

                        firstErr = error.As(err)!;

                    }
                    else if (err != syscall.ENOENT)
                    {
                        firstErr = error.As(err)!;
                    }


                }

            }

            {
                var z__prev1 = z;

                (z, err) = loadLocation(name, zoneSources);

                if (err == null)
                {
                    return (_addr_z!, error.As(null!)!);
                }
                else if (firstErr == null)
                {
                    firstErr = error.As(err)!;
                }


                z = z__prev1;

            }

            return (_addr_null!, error.As(firstErr)!);

        }

        // containsDotDot reports whether s contains "..".
        private static bool containsDotDot(@string s)
        {
            if (len(s) < 2L)
            {
                return false;
            }

            for (long i = 0L; i < len(s) - 1L; i++)
            {
                if (s[i] == '.' && s[i + 1L] == '.')
                {
                    return true;
                }

            }

            return false;

        }
    }
}
