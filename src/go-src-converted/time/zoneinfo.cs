// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package time -- go2cs converted at 2020 August 29 08:42:26 UTC
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
        // in use in a geographical area, such as CEST and CET for central Europe.
        public partial struct Location
        {
            public @string name;
            public slice<zone> zone;
            public slice<zoneTrans> tx; // Most lookups will be for the current time.
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

        // A zone represents a single time zone such as CEST or CET.
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
        private static readonly long alpha = -1L << (int)(63L); // math.MinInt64
        private static readonly long omega = 1L << (int)(63L) - 1L; // math.MaxInt64

        // UTC represents Universal Coordinated Time (UTC).
        public static ref Location UTC = ref utcLoc;

        // utcLoc is separate so that get can refer to &utcLoc
        // and ensure that it never returns a nil *Location,
        // even if a badly behaved client has changed UTC.
        private static Location utcLoc = new Location(name:"UTC");

        // Local represents the system's local time zone.
        public static ref Location Local = ref localLoc;

        // localLoc is separate so that initLocal can initialize
        // it even if a client has changed Local.
        private static Location localLoc = default;
        private static sync.Once localOnce = default;

        private static ref Location get(this ref Location l)
        {
            if (l == null)
            {
                return ref utcLoc;
            }
            if (l == ref localLoc)
            {
                localOnce.Do(initLocal);
            }
            return l;
        }

        // String returns a descriptive name for the time zone information,
        // corresponding to the name argument to LoadLocation or FixedZone.
        private static @string String(this ref Location l)
        {
            return l.get().name;
        }

        // FixedZone returns a Location that always uses
        // the given zone name and offset (seconds east of UTC).
        public static ref Location FixedZone(@string name, long offset)
        {
            Location l = ref new Location(name:name,zone:[]zone{{name,offset,false}},tx:[]zoneTrans{{alpha,0,false,false}},cacheStart:alpha,cacheEnd:omega,);
            l.cacheZone = ref l.zone[0L];
            return l;
        }

        // lookup returns information about the time zone in use at an
        // instant in time expressed as seconds since January 1, 1970 00:00:00 UTC.
        //
        // The returned information gives the name of the zone (such as "CET"),
        // the start and end times bracketing sec when that zone is in effect,
        // the offset in seconds east of UTC (such as -5*60*60), and whether
        // the daylight savings is being observed at that time.
        private static (@string, long, bool, long, long) lookup(this ref Location l, long sec)
        {
            l = l.get();

            if (len(l.zone) == 0L)
            {
                name = "UTC";
                offset = 0L;
                isDST = false;
                start = alpha;
                end = omega;
                return;
            }
            {
                var zone__prev1 = zone;

                var zone = l.cacheZone;

                if (zone != null && l.cacheStart <= sec && sec < l.cacheEnd)
                {
                    name = zone.name;
                    offset = zone.offset;
                    isDST = zone.isDST;
                    start = l.cacheStart;
                    end = l.cacheEnd;
                    return;
                }

                zone = zone__prev1;

            }

            if (len(l.tx) == 0L || sec < l.tx[0L].when)
            {
                zone = ref l.zone[l.lookupFirstZone()];
                name = zone.name;
                offset = zone.offset;
                isDST = zone.isDST;
                start = alpha;
                if (len(l.tx) > 0L)
                {
                    end = l.tx[0L].when;
                }
                else
                {
                    end = omega;
                }
                return;
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

            zone = ref l.zone[tx[lo].index];
            name = zone.name;
            offset = zone.offset;
            isDST = zone.isDST;
            start = tx[lo].when; 
            // end = maintained during the search
            return;
        }

        // lookupFirstZone returns the index of the time zone to use for times
        // before the first transition time, or when there are no transition
        // times.
        //
        // The reference implementation in localtime.c from
        // http://www.iana.org/time-zones/repository/releases/tzcode2013g.tar.gz
        // implements the following algorithm for these cases:
        // 1) If the first zone is unused by the transitions, use it.
        // 2) Otherwise, if there are transition times, and the first
        //    transition is to a zone in daylight time, find the first
        //    non-daylight-time zone before and closest to the first transition
        //    zone.
        // 3) Otherwise, use the first zone that is not daylight time, if
        //    there is one.
        // 4) Otherwise, use the first zone.
        private static long lookupFirstZone(this ref Location l)
        { 
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

        // firstZoneUsed returns whether the first zone is used by some
        // transition.
        private static bool firstZoneUsed(this ref Location l)
        {
            foreach (var (_, tx) in l.tx)
            {
                if (tx.index == 0L)
                {
                    return true;
                }
            }
            return false;
        }

        // lookupName returns information about the time zone with
        // the given name (such as "EST") at the given pseudo-Unix time
        // (what the given time of day would be in UTC).
        private static (long, bool) lookupName(this ref Location l, @string name, long unix)
        {
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
                    var zone = ref l.zone[i];
                    if (zone.name == name)
                    {
                        var (nam, offset, _, _, _) = l.lookup(unix - int64(zone.offset));
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
                    zone = ref l.zone[i];
                    if (zone.name == name)
                    {
                        return (zone.offset, true);
                    }
                } 

                // Otherwise, give up.

                i = i__prev1;
            }

            return;
        }

        // NOTE(rsc): Eventually we will need to accept the POSIX TZ environment
        // syntax too, but I don't feel like implementing it today.

        private static var errLocation = errors.New("time: invalid location name");

        private static ref @string zoneinfo = default;
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
        public static (ref Location, error) LoadLocation(@string name)
        {
            if (name == "" || name == "UTC")
            {
                return (UTC, null);
            }
            if (name == "Local")
            {
                return (Local, null);
            }
            if (containsDotDot(name) || name[0L] == '/' || name[0L] == '\\')
            { 
                // No valid IANA Time Zone name contains a single dot,
                // much less dot dot. Likewise, none begin with a slash.
                return (null, errLocation);
            }
            zoneinfoOnce.Do(() =>
            {
                var (env, _) = syscall.Getenv("ZONEINFO");
                zoneinfo = ref env;
            });
            if (zoneinfo != "".Value)
            {
                {
                    var (zoneData, err) = loadTzinfoFromDirOrZip(zoneinfo.Value, name);

                    if (err == null)
                    {
                        {
                            var (z, err) = LoadLocationFromTZData(name, zoneData);

                            if (err == null)
                            {
                                return (z, null);
                            }

                        }
                    }

                }
            }
            return loadLocation(name, zoneSources);
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
