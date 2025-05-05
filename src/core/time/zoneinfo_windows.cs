// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using errors = errors_package;
using registry = @internal.syscall.windows.registry_package;
using syscall = syscall_package;
using @internal.syscall.windows;

partial class time_package {

internal static slice<@string> platformZoneSources; // none: Windows uses system calls instead

// TODO(rsc): Fall back to copy of zoneinfo files.
// BUG(brainman,rsc): On Windows, the operating system does not provide complete
// time zone information.
// The implementation assumes that this year's rules for daylight savings
// time apply to all previous and future years as well.

// matchZoneKey checks if stdname and dstname match the corresponding key
// values "MUI_Std" and MUI_Dlt" or "Std" and "Dlt" in the kname key stored
// under the open registry key zones.
internal static (bool matched, error err2) matchZoneKey(registry.Key zones, @string kname, @string stdname, @string dstname) => func((defer, _) => {
    bool matched = default!;
    error err2 = default!;

    var (k, err) = registry.OpenKey(zones, kname, registry.READ);
    if (err != default!) {
        return (false, err);
    }
    defer(k.Close);
    @string std = default!;
    @string dlt = default!;
    // Try MUI_Std and MUI_Dlt first, fallback to Std and Dlt if *any* error occurs
    (std, err) = k.GetMUIStringValue("MUI_Std"u8);
    if (err == default!) {
        (dlt, err) = k.GetMUIStringValue("MUI_Dlt"u8);
    }
    if (err != default!) {
        // Fallback to Std and Dlt
        {
            (std, _, err) = k.GetStringValue("Std"u8); if (err != default!) {
                return (false, err);
            }
        }
        {
            (dlt, _, err) = k.GetStringValue("Dlt"u8); if (err != default!) {
                return (false, err);
            }
        }
    }
    if (std != stdname) {
        return (false, default!);
    }
    if (dlt != dstname && dstname != stdname) {
        return (false, default!);
    }
    return (true, default!);
});

// toEnglishName searches the registry for an English name of a time zone
// whose zone names are stdname and dstname and returns the English name.
internal static (@string, error) toEnglishName(@string stdname, @string dstname) => func((defer, _) => {
    var (k, err) = registry.OpenKey(registry.LOCAL_MACHINE, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Time Zones"u8, (uint32)(registry.ENUMERATE_SUB_KEYS | registry.QUERY_VALUE));
    if (err != default!) {
        return ("", err);
    }
    defer(k.Close);
    (names, err) = k.ReadSubKeyNames();
    if (err != default!) {
        return ("", err);
    }
    foreach (var (_, name) in names) {
        var (matched, errΔ1) = matchZoneKey(k, name, stdname, dstname);
        if (errΔ1 == default! && matched) {
            return (name, default!);
        }
    }
    return ("", errors.New(@"English name for time zone """u8 + stdname + @""" not found in registry"u8));
});

// extractCAPS extracts capital letters from description desc.
internal static @string extractCAPS(@string desc) {
    slice<rune> @short = default!;
    foreach (var (_, c) in desc) {
        if ((rune)'A' <= c && c <= (rune)'Z') {
            @short = append(@short, c);
        }
    }
    return ((@string)@short);
}

// abbrev returns the abbreviations to use for the given zone z.
internal static (@string std, @string dst) abbrev(ж<syscall.Timezoneinformation> Ꮡz) {
    @string std = default!;
    @string dst = default!;

    ref var z = ref Ꮡz.val;
    @string stdName = syscall.UTF16ToString(z.StandardName[..]);
    var (a, ok) = abbrs[stdName];
    if (!ok) {
        @string dstName = syscall.UTF16ToString(z.DaylightName[..]);
        // Perhaps stdName is not English. Try to convert it.
        var (englishName, err) = toEnglishName(stdName, dstName);
        if (err == default!) {
            (a, ok) = abbrs[englishName];
            if (ok) {
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
internal static int64 pseudoUnix(nint year, ж<syscall.Systemtime> Ꮡd) {
    ref var d = ref Ꮡd.val;

    // Windows specifies daylight savings information in "day in month" format:
    // d.Month is month number (1-12)
    // d.DayOfWeek is appropriate weekday (Sunday=0 to Saturday=6)
    // d.Day is week within the month (1 to 5, where 5 is last week of the month)
    // d.Hour, d.Minute and d.Second are absolute time
    nint day = 1;
    var t = Date(year, ((ΔMonth)d.Month), day, ((nint)d.Hour), ((nint)d.Minute), ((nint)d.Second), 0, ΔUTC);
    nint i = ((nint)d.DayOfWeek) - ((nint)t.Weekday());
    if (i < 0) {
        i += 7;
    }
    day += i;
    {
        nint week = ((nint)d.Day) - 1; if (week < 4){
            day += week * 7;
        } else {
            // "Last" instance of the day.
            day += 4 * 7;
            if (day > daysIn(((ΔMonth)d.Month), year)) {
                day -= 7;
            }
        }
    }
    return t.sec() + ((int64)(day - 1)) * secondsPerDay + internalToUnix;
}

internal static void initLocalFromTZI(ж<syscall.Timezoneinformation> Ꮡi) {
    ref var i = ref Ꮡi.val;

    var l = Ꮡ(localLoc);
    l.val.name = "Local"u8;
    nint nzone = 1;
    if (i.StandardDate.Month > 0) {
        nzone++;
    }
    l.val.zone = new slice<zone>(nzone);
    var (stdname, dstname) = abbrev(Ꮡi);
    var std = Ꮡ((~l).zone, 0);
    std.val.name = stdname;
    if (nzone == 1) {
        // No daylight savings.
        std.val.offset = -((nint)i.Bias) * 60;
        l.val.cacheStart = alpha;
        l.val.cacheEnd = omega;
        l.val.cacheZone = std;
        l.val.tx = new slice<zoneTrans>(1);
        (~l).tx[0].when = l.val.cacheStart;
        (~l).tx[0].index = 0;
        return;
    }
    // StandardBias must be ignored if StandardDate is not set,
    // so this computation is delayed until after the nzone==1
    // return above.
    std.val.offset = -((nint)(i.Bias + i.StandardBias)) * 60;
    var dst = Ꮡ((~l).zone, 1);
    dst.val.name = dstname;
    dst.val.offset = -((nint)(i.Bias + i.DaylightBias)) * 60;
    dst.val.isDST = true;
    // Arrange so that d0 is first transition date, d1 second,
    // i0 is index of zone after first transition, i1 second.
    var d0 = Ꮡ(i.StandardDate);
    var d1 = Ꮡ(i.DaylightDate);
    nint i0 = 0;
    nint i1 = 1;
    if ((~d0).Month > (~d1).Month) {
        (d0, d1) = (d1, d0);
        (i0, i1) = (i1, i0);
    }
    // 2 tx per year, 100 years on each side of this year
    l.val.tx = new slice<zoneTrans>(400);
    var t = Now().UTC();
    nint year = t.Year();
    nint txi = 0;
    for (nint y = year - 100; y < year + 100; y++) {
        var tx = Ꮡ((~l).tx, txi);
        tx.val.when = pseudoUnix(y, d0) - ((int64)(~l).zone[i1].offset);
        tx.val.index = ((uint8)i0);
        txi++;
        tx = Ꮡ((~l).tx, txi);
        tx.val.when = pseudoUnix(y, d1) - ((int64)(~l).zone[i0].offset);
        tx.val.index = ((uint8)i1);
        txi++;
    }
}

internal static syscall.Timezoneinformation usPacific = new syscall.Timezoneinformation(
    Bias: 8 * 60,
    StandardName: new uint16[]{
        (rune)'P', (rune)'a', (rune)'c', (rune)'i', (rune)'f', (rune)'i', (rune)'c', (rune)' ', (rune)'S', (rune)'t', (rune)'a', (rune)'n', (rune)'d', (rune)'a', (rune)'r', (rune)'d', (rune)' ', (rune)'T', (rune)'i', (rune)'m', (rune)'e'
    }.array(),
    StandardDate: new syscall.Systemtime(ΔMonth: 11, Day: 1, ΔHour: 2),
    DaylightName: new uint16[]{
        (rune)'P', (rune)'a', (rune)'c', (rune)'i', (rune)'f', (rune)'i', (rune)'c', (rune)' ', (rune)'D', (rune)'a', (rune)'y', (rune)'l', (rune)'i', (rune)'g', (rune)'h', (rune)'t', (rune)' ', (rune)'T', (rune)'i', (rune)'m', (rune)'e'
    }.array(),
    DaylightDate: new syscall.Systemtime(ΔMonth: 3, Day: 2, ΔHour: 2),
    DaylightBias: -60
);

internal static syscall.Timezoneinformation aus = new syscall.Timezoneinformation(
    Bias: -10 * 60,
    StandardName: new uint16[]{
        (rune)'A', (rune)'U', (rune)'S', (rune)' ', (rune)'E', (rune)'a', (rune)'s', (rune)'t', (rune)'e', (rune)'r', (rune)'n', (rune)' ', (rune)'S', (rune)'t', (rune)'a', (rune)'n', (rune)'d', (rune)'a', (rune)'r', (rune)'d', (rune)' ', (rune)'T', (rune)'i', (rune)'m', (rune)'e'
    }.array(),
    StandardDate: new syscall.Systemtime(ΔMonth: 4, Day: 1, ΔHour: 3),
    DaylightName: new uint16[]{
        (rune)'A', (rune)'U', (rune)'S', (rune)' ', (rune)'E', (rune)'a', (rune)'s', (rune)'t', (rune)'e', (rune)'r', (rune)'n', (rune)' ', (rune)'D', (rune)'a', (rune)'y', (rune)'l', (rune)'i', (rune)'g', (rune)'h', (rune)'t', (rune)' ', (rune)'T', (rune)'i', (rune)'m', (rune)'e'
    }.array(),
    DaylightDate: new syscall.Systemtime(ΔMonth: 10, Day: 1, ΔHour: 2),
    DaylightBias: -60
);

internal static void initLocal() {
    ref var i = ref heap(new syscall_package.Timezoneinformation(), out var Ꮡi);
    {
        var (_, err) = syscall.GetTimeZoneInformation(Ꮡi); if (err != default!) {
            localLoc.name = "UTC"u8;
            return;
        }
    }
    initLocalFromTZI(Ꮡi);
}

} // end time_package
