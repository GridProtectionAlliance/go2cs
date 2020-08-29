// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package time -- go2cs converted at 2020 August 29 08:42:37 UTC
// import "time" ==> using time = go.time_package
// Original source: C:\Go\src\time\zoneinfo_windows.go
using errors = go.errors_package;
using registry = go.@internal.syscall.windows.registry_package;
using runtime = go.runtime_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go
{
    public static partial class time_package
    {
        private static @string zoneSources = new slice<@string>(new @string[] { runtime.GOROOT()+"/lib/time/zoneinfo.zip" });

        // TODO(rsc): Fall back to copy of zoneinfo files.

        // BUG(brainman,rsc): On Windows, the operating system does not provide complete
        // time zone information.
        // The implementation assumes that this year's rules for daylight savings
        // time apply to all previous and future years as well.

        // matchZoneKey checks if stdname and dstname match the corresponding key
        // values "MUI_Std" and MUI_Dlt" or "Std" and "Dlt" (the latter down-level
        // from Vista) in the kname key stored under the open registry key zones.
        private static (bool, error) matchZoneKey(registry.Key zones, @string kname, @string stdname, @string dstname) => func((defer, _, __) =>
        {
            var (k, err) = registry.OpenKey(zones, kname, registry.READ);
            if (err != null)
            {
                return (false, err);
            }
            defer(k.Close());

            @string std = default;            @string dlt = default;

            err = registry.LoadRegLoadMUIString();

            if (err == null)
            { 
                // Try MUI_Std and MUI_Dlt first, fallback to Std and Dlt if *any* error occurs
                std, err = k.GetMUIStringValue("MUI_Std");
                if (err == null)
                {
                    dlt, err = k.GetMUIStringValue("MUI_Dlt");
                }
            }
            if (err != null)
            { // Fallback to Std and Dlt
                std, _, err = k.GetStringValue("Std");

                if (err != null)
                {
                    return (false, err);
                }
                dlt, _, err = k.GetStringValue("Dlt");

                if (err != null)
                {
                    return (false, err);
                }
            }
            if (std != stdname)
            {
                return (false, null);
            }
            if (dlt != dstname && dstname != stdname)
            {
                return (false, null);
            }
            return (true, null);
        });

        // toEnglishName searches the registry for an English name of a time zone
        // whose zone names are stdname and dstname and returns the English name.
        private static (@string, error) toEnglishName(@string stdname, @string dstname) => func((defer, _, __) =>
        {
            var (k, err) = registry.OpenKey(registry.LOCAL_MACHINE, "SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Time Zones", registry.ENUMERATE_SUB_KEYS | registry.QUERY_VALUE);
            if (err != null)
            {
                return ("", err);
            }
            defer(k.Close());

            var (names, err) = k.ReadSubKeyNames(-1L);
            if (err != null)
            {
                return ("", err);
            }
            foreach (var (_, name) in names)
            {
                var (matched, err) = matchZoneKey(k, name, stdname, dstname);
                if (err == null && matched)
                {
                    return (name, null);
                }
            }
            return ("", errors.New("English name for time zone \"" + stdname + "\" not found in registry"));
        });

        // extractCAPS extracts capital letters from description desc.
        private static @string extractCAPS(@string desc)
        {
            slice<int> @short = default;
            foreach (var (_, c) in desc)
            {
                if ('A' <= c && c <= 'Z')
                {
                    short = append(short, c);
                }
            }
            return string(short);
        }

        // abbrev returns the abbreviations to use for the given zone z.
        private static (@string, @string) abbrev(ref syscall.Timezoneinformation z)
        {
            var stdName = syscall.UTF16ToString(z.StandardName[..]);
            var (a, ok) = abbrs[stdName];
            if (!ok)
            {
                var dstName = syscall.UTF16ToString(z.DaylightName[..]); 
                // Perhaps stdName is not English. Try to convert it.
                var (englishName, err) = toEnglishName(stdName, dstName);
                if (err == null)
                {
                    a, ok = abbrs[englishName];
                    if (ok)
                    {
                        return (a.std, a.dst);
                    }
                } 
                // fallback to using capital letters
                return (extractCAPS(stdName), extractCAPS(dstName));
            }
            return (a.std, a.dst);
        }

        // pseudoUnix returns the pseudo-Unix time (seconds since Jan 1 1970 *LOCAL TIME*)
        // denoted by the system date+time d in the given year.
        // It is up to the caller to convert this local time into a UTC-based time.
        private static long pseudoUnix(long year, ref syscall.Systemtime d)
        { 
            // Windows specifies daylight savings information in "day in month" format:
            // d.Month is month number (1-12)
            // d.DayOfWeek is appropriate weekday (Sunday=0 to Saturday=6)
            // d.Day is week within the month (1 to 5, where 5 is last week of the month)
            // d.Hour, d.Minute and d.Second are absolute time
            long day = 1L;
            var t = Date(year, Month(d.Month), day, int(d.Hour), int(d.Minute), int(d.Second), 0L, UTC);
            var i = int(d.DayOfWeek) - int(t.Weekday());
            if (i < 0L)
            {
                i += 7L;
            }
            day += i;
            {
                var week = int(d.Day) - 1L;

                if (week < 4L)
                {
                    day += week * 7L;
                }
                else
                { 
                    // "Last" instance of the day.
                    day += 4L * 7L;
                    if (day > daysIn(Month(d.Month), year))
                    {
                        day -= 7L;
                    }
                }

            }
            return t.sec() + int64(day - 1L) * secondsPerDay + internalToUnix;
        }

        private static void initLocalFromTZI(ref syscall.Timezoneinformation i)
        {
            var l = ref localLoc;

            l.name = "Local";

            long nzone = 1L;
            if (i.StandardDate.Month > 0L)
            {
                nzone++;
            }
            l.zone = make_slice<zone>(nzone);

            var (stdname, dstname) = abbrev(i);

            var std = ref l.zone[0L];
            std.name = stdname;
            if (nzone == 1L)
            { 
                // No daylight savings.
                std.offset = -int(i.Bias) * 60L;
                l.cacheStart = alpha;
                l.cacheEnd = omega;
                l.cacheZone = std;
                l.tx = make_slice<zoneTrans>(1L);
                l.tx[0L].when = l.cacheStart;
                l.tx[0L].index = 0L;
                return;
            } 

            // StandardBias must be ignored if StandardDate is not set,
            // so this computation is delayed until after the nzone==1
            // return above.
            std.offset = -int(i.Bias + i.StandardBias) * 60L;

            var dst = ref l.zone[1L];
            dst.name = dstname;
            dst.offset = -int(i.Bias + i.DaylightBias) * 60L;
            dst.isDST = true; 

            // Arrange so that d0 is first transition date, d1 second,
            // i0 is index of zone after first transition, i1 second.
            var d0 = ref i.StandardDate;
            var d1 = ref i.DaylightDate;
            long i0 = 0L;
            long i1 = 1L;
            if (d0.Month > d1.Month)
            {
                d0 = d1;
                d1 = d0;
                i0 = i1;
                i1 = i0;
            } 

            // 2 tx per year, 100 years on each side of this year
            l.tx = make_slice<zoneTrans>(400L);

            var t = Now().UTC();
            var year = t.Year();
            long txi = 0L;
            for (var y = year - 100L; y < year + 100L; y++)
            {
                var tx = ref l.tx[txi];
                tx.when = pseudoUnix(y, d0) - int64(l.zone[i1].offset);
                tx.index = uint8(i0);
                txi++;

                tx = ref l.tx[txi];
                tx.when = pseudoUnix(y, d1) - int64(l.zone[i0].offset);
                tx.index = uint8(i1);
                txi++;
            }

        }

        private static syscall.Timezoneinformation usPacific = new syscall.Timezoneinformation(Bias:8*60,StandardName:[32]uint16{'P','a','c','i','f','i','c',' ','S','t','a','n','d','a','r','d',' ','T','i','m','e',},StandardDate:syscall.Systemtime{Month:11,Day:1,Hour:2},DaylightName:[32]uint16{'P','a','c','i','f','i','c',' ','D','a','y','l','i','g','h','t',' ','T','i','m','e',},DaylightDate:syscall.Systemtime{Month:3,Day:2,Hour:2},DaylightBias:-60,);

        private static syscall.Timezoneinformation aus = new syscall.Timezoneinformation(Bias:-10*60,StandardName:[32]uint16{'A','U','S',' ','E','a','s','t','e','r','n',' ','S','t','a','n','d','a','r','d',' ','T','i','m','e',},StandardDate:syscall.Systemtime{Month:4,Day:1,Hour:3},DaylightName:[32]uint16{'A','U','S',' ','E','a','s','t','e','r','n',' ','D','a','y','l','i','g','h','t',' ','T','i','m','e',},DaylightDate:syscall.Systemtime{Month:10,Day:1,Hour:2},DaylightBias:-60,);

        private static void initLocal()
        {
            syscall.Timezoneinformation i = default;
            {
                var (_, err) = syscall.GetTimeZoneInformation(ref i);

                if (err != null)
                {
                    localLoc.name = "UTC";
                    return;
                }

            }
            initLocalFromTZI(ref i);
        }
    }
}
