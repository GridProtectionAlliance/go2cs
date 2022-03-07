// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Parse Plan 9 timezone(2) files.

// package time -- go2cs converted at 2022 March 06 22:30:17 UTC
// import "time" ==> using time = go.time_package
// Original source: C:\Program Files\Go\src\time\zoneinfo_plan9.go
using runtime = go.runtime_package;
using syscall = go.syscall_package;

namespace go;

public static partial class time_package {

private static @string zoneSources = new slice<@string>(new @string[] { runtime.GOROOT()+"/lib/time/zoneinfo.zip" });

private static bool isSpace(int r) {
    return r == ' ' || r == '\t' || r == '\n';
}

// Copied from strings to avoid a dependency.
private static slice<@string> fields(@string s) { 
    // First count the fields.
    nint n = 0;
    var inField = false;
    {
        var rune__prev1 = rune;

        foreach (var (_, __rune) in s) {
            rune = __rune;
            var wasInField = inField;
            inField = !isSpace(rune);
            if (inField && !wasInField) {
                n++;
            }
        }
        rune = rune__prev1;
    }

    var a = make_slice<@string>(n);
    nint na = 0;
    nint fieldStart = -1; // Set to -1 when looking for start of field.
    {
        var rune__prev1 = rune;

        foreach (var (__i, __rune) in s) {
            i = __i;
            rune = __rune;
            if (isSpace(rune)) {
                if (fieldStart >= 0) {
                    a[na] = s[(int)fieldStart..(int)i];
                    na++;
                    fieldStart = -1;
                }
            }
            else if (fieldStart == -1) {
                fieldStart = i;
            }

        }
        rune = rune__prev1;
    }

    if (fieldStart >= 0) { // Last field might end at EOF.
        a[na] = s[(int)fieldStart..];

    }
    return a;

}

private static (ptr<Location>, error) loadZoneDataPlan9(@string s) {
    ptr<Location> l = default!;
    error err = default!;

    var f = fields(s);
    if (len(f) < 4) {
        if (len(f) == 2 && f[0] == "GMT") {
            return (_addr_UTC!, error.As(null!)!);
        }
        return (_addr_null!, error.As(badData)!);

    }
    array<zone> zones = new array<zone>(2); 

    // standard timezone offset
    var (o, err) = atoi(f[1]);
    if (err != null) {
        return (_addr_null!, error.As(badData)!);
    }
    zones[0] = new zone(name:f[0],offset:o,isDST:false); 

    // alternate timezone offset
    o, err = atoi(f[3]);
    if (err != null) {
        return (_addr_null!, error.As(badData)!);
    }
    zones[1] = new zone(name:f[2],offset:o,isDST:true); 

    // transition time pairs
    slice<zoneTrans> tx = default;
    f = f[(int)4..];
    {
        nint i__prev1 = i;

        for (nint i = 0; i < len(f); i++) {
            nint zi = 0;
            if (i % 2 == 0) {
                zi = 1;
            }
            var (t, err) = atoi(f[i]);
            if (err != null) {
                return (_addr_null!, error.As(badData)!);
            }
            t -= zones[0].offset;
            tx = append(tx, new zoneTrans(when:int64(t),index:uint8(zi)));
        }

        i = i__prev1;
    } 

    // Committed to succeed.
    l = addr(new Location(zone:zones[:],tx:tx)); 

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
                if (i + 1 < len(tx)) {
                    l.cacheEnd = tx[i + 1].when;
                }
                l.cacheZone = _addr_l.zone[tx[i].index];
            }
        }
        i = i__prev1;
    }

    return (_addr_l!, error.As(null!)!);

}

private static (ptr<Location>, error) loadZoneFilePlan9(@string name) {
    ptr<Location> _p0 = default!;
    error _p0 = default!;

    var (b, err) = readFile(name);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    return _addr_loadZoneDataPlan9(string(b))!;

}

private static void initLocal() {
    var (t, ok) = syscall.Getenv("timezone");
    if (ok) {
        {
            var z__prev2 = z;

            var (z, err) = loadZoneDataPlan9(t);

            if (err == null) {
                localLoc = z.val;
                return ;
            }

            z = z__prev2;

        }

    }
    else
 {
        {
            var z__prev2 = z;

            (z, err) = loadZoneFilePlan9("/adm/timezone/local");

            if (err == null) {
                localLoc = z.val;
                localLoc.name = "Local";
                return ;
            }

            z = z__prev2;

        }

    }
    localLoc.name = "UTC";

}

} // end time_package
