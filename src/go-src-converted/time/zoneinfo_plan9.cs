// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Parse Plan 9 timezone(2) files.

// package time -- go2cs converted at 2020 August 29 08:42:30 UTC
// import "time" ==> using time = go.time_package
// Original source: C:\Go\src\time\zoneinfo_plan9.go
using runtime = go.runtime_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go
{
    public static partial class time_package
    {
        private static @string zoneSources = new slice<@string>(new @string[] { runtime.GOROOT()+"/lib/time/zoneinfo.zip" });

        private static bool isSpace(int r)
        {
            return r == ' ' || r == '\t' || r == '\n';
        }

        // Copied from strings to avoid a dependency.
        private static slice<@string> fields(@string s)
        { 
            // First count the fields.
            long n = 0L;
            var inField = false;
            {
                var rune__prev1 = rune;

                foreach (var (_, __rune) in s)
                {
                    rune = __rune;
                    var wasInField = inField;
                    inField = !isSpace(rune);
                    if (inField && !wasInField)
                    {
                        n++;
                    }
                } 

                // Now create them.

                rune = rune__prev1;
            }

            var a = make_slice<@string>(n);
            long na = 0L;
            long fieldStart = -1L; // Set to -1 when looking for start of field.
            {
                var rune__prev1 = rune;

                foreach (var (__i, __rune) in s)
                {
                    i = __i;
                    rune = __rune;
                    if (isSpace(rune))
                    {
                        if (fieldStart >= 0L)
                        {
                            a[na] = s[fieldStart..i];
                            na++;
                            fieldStart = -1L;
                        }
                    }
                    else if (fieldStart == -1L)
                    {
                        fieldStart = i;
                    }
                }

                rune = rune__prev1;
            }

            if (fieldStart >= 0L)
            { // Last field might end at EOF.
                a[na] = s[fieldStart..];
            }
            return a;
        }

        private static (ref Location, error) loadZoneDataPlan9(@string s)
        {
            var f = fields(s);
            if (len(f) < 4L)
            {
                if (len(f) == 2L && f[0L] == "GMT")
                {
                    return (UTC, null);
                }
                return (null, badData);
            }
            array<zone> zones = new array<zone>(2L); 

            // standard timezone offset
            var (o, err) = atoi(f[1L]);
            if (err != null)
            {
                return (null, badData);
            }
            zones[0L] = new zone(name:f[0],offset:o,isDST:false); 

            // alternate timezone offset
            o, err = atoi(f[3L]);
            if (err != null)
            {
                return (null, badData);
            }
            zones[1L] = new zone(name:f[2],offset:o,isDST:true); 

            // transition time pairs
            slice<zoneTrans> tx = default;
            f = f[4L..];
            {
                long i__prev1 = i;

                for (long i = 0L; i < len(f); i++)
                {
                    long zi = 0L;
                    if (i % 2L == 0L)
                    {
                        zi = 1L;
                    }
                    var (t, err) = atoi(f[i]);
                    if (err != null)
                    {
                        return (null, badData);
                    }
                    t -= zones[0L].offset;
                    tx = append(tx, new zoneTrans(when:int64(t),index:uint8(zi)));
                } 

                // Committed to succeed.


                i = i__prev1;
            } 

            // Committed to succeed.
            l = ref new Location(zone:zones[:],tx:tx); 

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

        private static (ref Location, error) loadZoneFilePlan9(@string name)
        {
            var (b, err) = readFile(name);
            if (err != null)
            {
                return (null, err);
            }
            return loadZoneDataPlan9(string(b));
        }

        private static void initLocal()
        {
            var (t, ok) = syscall.Getenv("timezone");
            if (ok)
            {
                {
                    var z__prev2 = z;

                    var (z, err) = loadZoneDataPlan9(t);

                    if (err == null)
                    {
                        localLoc = z.Value;
                        return;
                    }

                    z = z__prev2;

                }
            }
            else
            {
                {
                    var z__prev2 = z;

                    (z, err) = loadZoneFilePlan9("/adm/timezone/local");

                    if (err == null)
                    {
                        localLoc = z.Value;
                        localLoc.name = "Local";
                        return;
                    }

                    z = z__prev2;

                }
            } 

            // Fall back to UTC.
            localLoc.name = "UTC";
        }
    }
}
