// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package time -- go2cs converted at 2022 March 13 05:40:57 UTC
// import "time" ==> using time = go.time_package
// Original source: C:\Program Files\Go\src\time\format.go
namespace go;

using errors = errors_package;

public static partial class time_package {

// These are predefined layouts for use in Time.Format and time.Parse.
// The reference time used in these layouts is the specific time stamp:
//    01/02 03:04:05PM '06 -0700
// (January 2, 15:04:05, 2006, in time zone seven hours west of GMT).
// That value is recorded as the constant named Layout, listed below. As a Unix
// time, this is 1136239445. Since MST is GMT-0700, the reference would be
// printed by the Unix date command as:
//    Mon Jan 2 15:04:05 MST 2006
// It is a regrettable historic error that the date uses the American convention
// of putting the numerical month before the day.
//
// The example for Time.Format demonstrates the working of the layout string
// in detail and is a good reference.
//
// Note that the RFC822, RFC850, and RFC1123 formats should be applied
// only to local times. Applying them to UTC times will use "UTC" as the
// time zone abbreviation, while strictly speaking those RFCs require the
// use of "GMT" in that case.
// In general RFC1123Z should be used instead of RFC1123 for servers
// that insist on that format, and RFC3339 should be preferred for new protocols.
// RFC3339, RFC822, RFC822Z, RFC1123, and RFC1123Z are useful for formatting;
// when used with time.Parse they do not accept all the time formats
// permitted by the RFCs and they do accept time formats not formally defined.
// The RFC3339Nano format removes trailing zeros from the seconds field
// and thus may not sort correctly once formatted.
//
// Most programs can use one of the defined constants as the layout passed to
// Format or Parse. The rest of this comment can be ignored unless you are
// creating a custom layout string.
//
// To define your own format, write down what the reference time would look like
// formatted your way; see the values of constants like ANSIC, StampMicro or
// Kitchen for examples. The model is to demonstrate what the reference time
// looks like so that the Format and Parse methods can apply the same
// transformation to a general time value.
//
// Here is a summary of the components of a layout string. Each element shows by
// example the formatting of an element of the reference time. Only these values
// are recognized. Text in the layout string that is not recognized as part of
// the reference time is echoed verbatim during Format and expected to appear
// verbatim in the input to Parse.
//
//    Year: "2006" "06"
//    Month: "Jan" "January"
//    Textual day of the week: "Mon" "Monday"
//    Numeric day of the month: "2" "_2" "02"
//    Numeric day of the year: "__2" "002"
//    Hour: "15" "3" "03" (PM or AM)
//    Minute: "4" "04"
//    Second: "5" "05"
//    AM/PM mark: "PM"
//
// Numeric time zone offsets format as follows:
//    "-0700"  ±hhmm
//    "-07:00" ±hh:mm
//    "-07"    ±hh
// Replacing the sign in the format with a Z triggers
// the ISO 8601 behavior of printing Z instead of an
// offset for the UTC zone. Thus:
//    "Z0700"  Z or ±hhmm
//    "Z07:00" Z or ±hh:mm
//    "Z07"    Z or ±hh
//
// Within the format string, the underscores in "_2" and "__2" represent spaces
// that may be replaced by digits if the following number has multiple digits,
// for compatibility with fixed-width Unix time formats. A leading zero represents
// a zero-padded value.
//
// The formats  and 002 are space-padded and zero-padded
// three-character day of year; there is no unpadded day of year format.
//
// A comma or decimal point followed by one or more zeros represents
// a fractional second, printed to the given number of decimal places.
// A comma or decimal point followed by one or more nines represents
// a fractional second, printed to the given number of decimal places, with
// trailing zeros removed.
// For example "15:04:05,000" or "15:04:05.000" formats or parses with
// millisecond precision.
//
// Some valid layouts are invalid time values for time.Parse, due to formats
// such as _ for space padding and Z for zone information.
//
public static readonly @string Layout = "01/02 03:04:05PM '06 -0700"; // The reference time, in numerical order.
public static readonly @string ANSIC = "Mon Jan _2 15:04:05 2006";
public static readonly @string UnixDate = "Mon Jan _2 15:04:05 MST 2006";
public static readonly @string RubyDate = "Mon Jan 02 15:04:05 -0700 2006";
public static readonly @string RFC822 = "02 Jan 06 15:04 MST";
public static readonly @string RFC822Z = "02 Jan 06 15:04 -0700"; // RFC822 with numeric zone
public static readonly @string RFC850 = "Monday, 02-Jan-06 15:04:05 MST";
public static readonly @string RFC1123 = "Mon, 02 Jan 2006 15:04:05 MST";
public static readonly @string RFC1123Z = "Mon, 02 Jan 2006 15:04:05 -0700"; // RFC1123 with numeric zone
public static readonly @string RFC3339 = "2006-01-02T15:04:05Z07:00";
public static readonly @string RFC3339Nano = "2006-01-02T15:04:05.999999999Z07:00";
public static readonly @string Kitchen = "3:04PM"; 
// Handy time stamps.
public static readonly @string Stamp = "Jan _2 15:04:05";
public static readonly @string StampMilli = "Jan _2 15:04:05.000";
public static readonly @string StampMicro = "Jan _2 15:04:05.000000";
public static readonly @string StampNano = "Jan _2 15:04:05.000000000";

private static readonly var _ = iota;
private static readonly var stdLongMonth = iota + stdNeedDate; // "January"
private static readonly var stdMonth = 0; // "Jan"
private static readonly var stdNumMonth = 1; // "1"
private static readonly var stdZeroMonth = 2; // "01"
private static readonly var stdLongWeekDay = 3; // "Monday"
private static readonly var stdWeekDay = 4; // "Mon"
private static readonly var stdDay = 5; // "2"
private static readonly var stdUnderDay = 6; // "_2"
private static readonly var stdZeroDay = 7; // "02"
private static readonly var stdUnderYearDay = 8; // "__2"
private static readonly stdHour stdZeroYearDay = iota + stdNeedClock; // "15"
private static readonly var stdHour12 = 9; // "3"
private static readonly var stdZeroHour12 = 10; // "03"
private static readonly var stdMinute = 11; // "4"
private static readonly var stdZeroMinute = 12; // "04"
private static readonly var stdSecond = 13; // "5"
private static readonly stdLongYear stdZeroSecond = iota + stdNeedDate; // "2006"
private static readonly stdPM stdYear = iota + stdNeedClock; // "PM"
private static readonly stdTZ stdpm = iota; // "MST"
private static readonly var stdISO8601TZ = 14; // "Z0700"  // prints Z for UTC
private static readonly var stdISO8601SecondsTZ = 15; // "Z070000"
private static readonly var stdISO8601ShortTZ = 16; // "Z07"
private static readonly var stdISO8601ColonTZ = 17; // "Z07:00" // prints Z for UTC
private static readonly var stdISO8601ColonSecondsTZ = 18; // "Z07:00:00"
private static readonly var stdNumTZ = 19; // "-0700"  // always numeric
private static readonly var stdNumSecondsTz = 20; // "-070000"
private static readonly var stdNumShortTZ = 21; // "-07"    // always numeric
private static readonly var stdNumColonTZ = 22; // "-07:00" // always numeric
private static readonly var stdNumColonSecondsTZ = 23; // "-07:00:00"
private static readonly var stdFracSecond0 = 24; // ".0", ".00", ... , trailing zeros included
private static readonly stdNeedDate stdFracSecond9 = 1 << 8; // need month, day, year
private static readonly nint stdNeedClock = 2 << 8; // need hour, minute, second
private static readonly nint stdArgShift = 16; // extra argument in high bits, above low stdArgShift
private static readonly nint stdSeparatorShift = 28; // extra argument in high 4 bits for fractional second separators
private static readonly nint stdMask = 1 << (int)(stdArgShift) - 1; // mask out argument

// std0x records the std values for "01", "02", ..., "06".
private static array<nint> std0x = new array<nint>(new nint[] { stdZeroMonth, stdZeroDay, stdZeroHour12, stdZeroMinute, stdZeroSecond, stdYear });

// startsWithLowerCase reports whether the string has a lower-case letter at the beginning.
// Its purpose is to prevent matching strings like "Month" when looking for "Mon".
private static bool startsWithLowerCase(@string str) {
    if (len(str) == 0) {
        return false;
    }
    var c = str[0];
    return 'a' <= c && c <= 'z';
}

// nextStdChunk finds the first occurrence of a std string in
// layout and returns the text before, the std string, and the text after.
private static (@string, nint, @string) nextStdChunk(@string layout) {
    @string prefix = default;
    nint std = default;
    @string suffix = default;

    for (nint i = 0; i < len(layout); i++) {
        {
            var c = int(layout[i]);

            switch (c) {
                case 'J': // January, Jan
                    if (len(layout) >= i + 3 && layout[(int)i..(int)i + 3] == "Jan") {
                        if (len(layout) >= i + 7 && layout[(int)i..(int)i + 7] == "January") {
                            return (layout[(int)0..(int)i], stdLongMonth, layout[(int)i + 7..]);
                        }
                        if (!startsWithLowerCase(layout[(int)i + 3..])) {
                            return (layout[(int)0..(int)i], stdMonth, layout[(int)i + 3..]);
                        }
                    }
                    break;
                case 'M': // Monday, Mon, MST
                    if (len(layout) >= i + 3) {
                        if (layout[(int)i..(int)i + 3] == "Mon") {
                            if (len(layout) >= i + 6 && layout[(int)i..(int)i + 6] == "Monday") {
                                return (layout[(int)0..(int)i], stdLongWeekDay, layout[(int)i + 6..]);
                            }
                            if (!startsWithLowerCase(layout[(int)i + 3..])) {
                                return (layout[(int)0..(int)i], stdWeekDay, layout[(int)i + 3..]);
                            }
                        }
                        if (layout[(int)i..(int)i + 3] == "MST") {
                            return (layout[(int)0..(int)i], stdTZ, layout[(int)i + 3..]);
                        }
                    }
                    break;
                case '0': // 01, 02, 03, 04, 05, 06, 002
                    if (len(layout) >= i + 2 && '1' <= layout[i + 1] && layout[i + 1] <= '6') {
                        return (layout[(int)0..(int)i], std0x[layout[i + 1] - '1'], layout[(int)i + 2..]);
                    }
                    if (len(layout) >= i + 3 && layout[i + 1] == '0' && layout[i + 2] == '2') {
                        return (layout[(int)0..(int)i], stdZeroYearDay, layout[(int)i + 3..]);
                    }
                    break;
                case '1': // 15, 1
                    if (len(layout) >= i + 2 && layout[i + 1] == '5') {
                        return (layout[(int)0..(int)i], stdHour, layout[(int)i + 2..]);
                    }
                    return (layout[(int)0..(int)i], stdNumMonth, layout[(int)i + 1..]);
                    break;
                case '2': // 2006, 2
                    if (len(layout) >= i + 4 && layout[(int)i..(int)i + 4] == "2006") {
                        return (layout[(int)0..(int)i], stdLongYear, layout[(int)i + 4..]);
                    }
                    return (layout[(int)0..(int)i], stdDay, layout[(int)i + 1..]);
                    break;
                case '_': // _2, _2006, __2
                    if (len(layout) >= i + 2 && layout[i + 1] == '2') { 
                        //_2006 is really a literal _, followed by stdLongYear
                        if (len(layout) >= i + 5 && layout[(int)i + 1..(int)i + 5] == "2006") {
                            return (layout[(int)0..(int)i + 1], stdLongYear, layout[(int)i + 5..]);
                        }
                        return (layout[(int)0..(int)i], stdUnderDay, layout[(int)i + 2..]);
                    }
                    if (len(layout) >= i + 3 && layout[i + 1] == '_' && layout[i + 2] == '2') {
                        return (layout[(int)0..(int)i], stdUnderYearDay, layout[(int)i + 3..]);
                    }
                    break;
                case '3': 
                    return (layout[(int)0..(int)i], stdHour12, layout[(int)i + 1..]);
                    break;
                case '4': 
                    return (layout[(int)0..(int)i], stdMinute, layout[(int)i + 1..]);
                    break;
                case '5': 
                    return (layout[(int)0..(int)i], stdSecond, layout[(int)i + 1..]);
                    break;
                case 'P': // PM
                    if (len(layout) >= i + 2 && layout[i + 1] == 'M') {
                        return (layout[(int)0..(int)i], stdPM, layout[(int)i + 2..]);
                    }
                    break;
                case 'p': // pm
                    if (len(layout) >= i + 2 && layout[i + 1] == 'm') {
                        return (layout[(int)0..(int)i], stdpm, layout[(int)i + 2..]);
                    }
                    break;
                case '-': // -070000, -07:00:00, -0700, -07:00, -07
                    if (len(layout) >= i + 7 && layout[(int)i..(int)i + 7] == "-070000") {
                        return (layout[(int)0..(int)i], stdNumSecondsTz, layout[(int)i + 7..]);
                    }
                    if (len(layout) >= i + 9 && layout[(int)i..(int)i + 9] == "-07:00:00") {
                        return (layout[(int)0..(int)i], stdNumColonSecondsTZ, layout[(int)i + 9..]);
                    }
                    if (len(layout) >= i + 5 && layout[(int)i..(int)i + 5] == "-0700") {
                        return (layout[(int)0..(int)i], stdNumTZ, layout[(int)i + 5..]);
                    }
                    if (len(layout) >= i + 6 && layout[(int)i..(int)i + 6] == "-07:00") {
                        return (layout[(int)0..(int)i], stdNumColonTZ, layout[(int)i + 6..]);
                    }
                    if (len(layout) >= i + 3 && layout[(int)i..(int)i + 3] == "-07") {
                        return (layout[(int)0..(int)i], stdNumShortTZ, layout[(int)i + 3..]);
                    }
                    break;
                case 'Z': // Z070000, Z07:00:00, Z0700, Z07:00,
                    if (len(layout) >= i + 7 && layout[(int)i..(int)i + 7] == "Z070000") {
                        return (layout[(int)0..(int)i], stdISO8601SecondsTZ, layout[(int)i + 7..]);
                    }
                    if (len(layout) >= i + 9 && layout[(int)i..(int)i + 9] == "Z07:00:00") {
                        return (layout[(int)0..(int)i], stdISO8601ColonSecondsTZ, layout[(int)i + 9..]);
                    }
                    if (len(layout) >= i + 5 && layout[(int)i..(int)i + 5] == "Z0700") {
                        return (layout[(int)0..(int)i], stdISO8601TZ, layout[(int)i + 5..]);
                    }
                    if (len(layout) >= i + 6 && layout[(int)i..(int)i + 6] == "Z07:00") {
                        return (layout[(int)0..(int)i], stdISO8601ColonTZ, layout[(int)i + 6..]);
                    }
                    if (len(layout) >= i + 3 && layout[(int)i..(int)i + 3] == "Z07") {
                        return (layout[(int)0..(int)i], stdISO8601ShortTZ, layout[(int)i + 3..]);
                    }
                    break;
                case '.': // ,000, or .000, or ,999, or .999 - repeated digits for fractional seconds.

                case ',': // ,000, or .000, or ,999, or .999 - repeated digits for fractional seconds.
                    if (i + 1 < len(layout) && (layout[i + 1] == '0' || layout[i + 1] == '9')) {
                        var ch = layout[i + 1];
                        var j = i + 1;
                        while (j < len(layout) && layout[j] == ch) {
                            j++;
                        } 
                        // String of digits must end here - only fractional second is all digits.

                        // String of digits must end here - only fractional second is all digits.
                        if (!isDigit(layout, j)) {
                            var code = stdFracSecond0;
                            if (layout[i + 1] == '9') {
                                code = stdFracSecond9;
                            }
                            var std = stdFracSecond(code, j - (i + 1), c);
                            return (layout[(int)0..(int)i], std, layout[(int)j..]);
                        }
                    }
                    break;
            }
        }
    }
    return (layout, 0, "");
}

private static @string longDayNames = new slice<@string>(new @string[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" });

private static @string shortDayNames = new slice<@string>(new @string[] { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" });

private static @string shortMonthNames = new slice<@string>(new @string[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" });

private static @string longMonthNames = new slice<@string>(new @string[] { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" });

// match reports whether s1 and s2 match ignoring case.
// It is assumed s1 and s2 are the same length.
private static bool match(@string s1, @string s2) {
    for (nint i = 0; i < len(s1); i++) {
        var c1 = s1[i];
        var c2 = s2[i];
        if (c1 != c2) { 
            // Switch to lower-case; 'a'-'A' is known to be a single bit.
            c1 |= 'a' - 'A';
            c2 |= 'a' - 'A';
            if (c1 != c2 || c1 < 'a' || c1 > 'z') {
                return false;
            }
        }
    }
    return true;
}

private static (nint, @string, error) lookup(slice<@string> tab, @string val) {
    nint _p0 = default;
    @string _p0 = default;
    error _p0 = default!;

    foreach (var (i, v) in tab) {
        if (len(val) >= len(v) && match(val[(int)0..(int)len(v)], v)) {
            return (i, val[(int)len(v)..], error.As(null!)!);
        }
    }    return (-1, val, error.As(errBad)!);
}

// appendInt appends the decimal form of x to b and returns the result.
// If the decimal form (excluding sign) is shorter than width, the result is padded with leading 0's.
// Duplicates functionality in strconv, but avoids dependency.
private static slice<byte> appendInt(slice<byte> b, nint x, nint width) {
    var u = uint(x);
    if (x < 0) {
        b = append(b, '-');
        u = uint(-x);
    }
    array<byte> buf = new array<byte>(20);
    var i = len(buf);
    while (u >= 10) {
        i--;
        var q = u / 10;
        buf[i] = byte('0' + u - q * 10);
        u = q;
    }
    i--;
    buf[i] = byte('0' + u); 

    // Add 0-padding.
    for (var w = len(buf) - i; w < width; w++) {
        b = append(b, '0');
    }

    return append(b, buf[(int)i..]);
}

// Never printed, just needs to be non-nil for return by atoi.
private static var atoiError = errors.New("time: invalid number");

// Duplicates functionality in strconv, but avoids dependency.
private static (nint, error) atoi(@string s) {
    nint x = default;
    error err = default!;

    var neg = false;
    if (s != "" && (s[0] == '-' || s[0] == '+')) {
        neg = s[0] == '-';
        s = s[(int)1..];
    }
    var (q, rem, err) = leadingInt(s);
    x = int(q);
    if (err != null || rem != "") {
        return (0, error.As(atoiError)!);
    }
    if (neg) {
        x = -x;
    }
    return (x, error.As(null!)!);
}

// The "std" value passed to formatNano contains two packed fields: the number of
// digits after the decimal and the separator character (period or comma).
// These functions pack and unpack that variable.
private static nint stdFracSecond(nint code, nint n, nint c) { 
    // Use 0xfff to make the failure case even more absurd.
    if (c == '.') {
        return code | ((n & 0xfff) << (int)(stdArgShift));
    }
    return code | ((n & 0xfff) << (int)(stdArgShift)) | 1 << (int)(stdSeparatorShift);
}

private static nint digitsLen(nint std) {
    return (std >> (int)(stdArgShift)) & 0xfff;
}

private static byte separator(nint std) {
    if ((std >> (int)(stdSeparatorShift)) == 0) {
        return '.';
    }
    return ',';
}

// formatNano appends a fractional second, as nanoseconds, to b
// and returns the result.
private static slice<byte> formatNano(slice<byte> b, nuint nanosec, nint std) {
    var n = digitsLen(std);    var separator = separator(std);    var trim = std & stdMask == stdFracSecond9;
    var u = nanosec;
    array<byte> buf = new array<byte>(9);
    {
        var start = len(buf);

        while (start > 0) {
            start--;
            buf[start] = byte(u % 10 + '0');
            u /= 10;
        }
    }

    if (n > 9) {
        n = 9;
    }
    if (trim) {
        while (n > 0 && buf[n - 1] == '0') {
            n--;
        }
        if (n == 0) {
            return b;
        }
    }
    b = append(b, separator);
    return append(b, buf[..(int)n]);
}

// String returns the time formatted using the format string
//    "2006-01-02 15:04:05.999999999 -0700 MST"
//
// If the time has a monotonic clock reading, the returned string
// includes a final field "m=±<value>", where value is the monotonic
// clock reading formatted as a decimal number of seconds.
//
// The returned string is meant for debugging; for a stable serialized
// representation, use t.MarshalText, t.MarshalBinary, or t.Format
// with an explicit format string.
public static @string String(this Time t) {
    var s = t.Format("2006-01-02 15:04:05.999999999 -0700 MST"); 

    // Format monotonic clock reading as m=±ddd.nnnnnnnnn.
    if (t.wall & hasMonotonic != 0) {
        var m2 = uint64(t.ext);
        var sign = byte('+');
        if (t.ext < 0) {
            sign = '-';
            m2 = -m2;
        }
        var m1 = m2 / 1e9F;
        m2 = m2 % 1e9F;
        var m0 = m1 / 1e9F;
        m1 = m1 % 1e9F;
        slice<byte> buf = default;
        buf = append(buf, " m=");
        buf = append(buf, sign);
        nint wid = 0;
        if (m0 != 0) {
            buf = appendInt(buf, int(m0), 0);
            wid = 9;
        }
        buf = appendInt(buf, int(m1), wid);
        buf = append(buf, '.');
        buf = appendInt(buf, int(m2), 9);
        s += string(buf);
    }
    return s;
}

// GoString implements fmt.GoStringer and formats t to be printed in Go source
// code.
public static @string GoString(this Time t) {
    slice<byte> buf = (slice<byte>)"time.Date(";
    buf = appendInt(buf, t.Year(), 0);
    var month = t.Month();
    if (January <= month && month <= December) {
        buf = append(buf, ", time.");
        buf = append(buf, t.Month().String());
    }
    else
 { 
        // It's difficult to construct a time.Time with a date outside the
        // standard range but we might as well try to handle the case.
        buf = appendInt(buf, int(month), 0);
    }
    buf = append(buf, ", ");
    buf = appendInt(buf, t.Day(), 0);
    buf = append(buf, ", ");
    buf = appendInt(buf, t.Hour(), 0);
    buf = append(buf, ", ");
    buf = appendInt(buf, t.Minute(), 0);
    buf = append(buf, ", ");
    buf = appendInt(buf, t.Second(), 0);
    buf = append(buf, ", ");
    buf = appendInt(buf, t.Nanosecond(), 0);
    buf = append(buf, ", ");
    {
        var loc = t.Location();


        if (loc == UTC || loc == null) 
            buf = append(buf, "time.UTC");
        else if (loc == Local) 
            buf = append(buf, "time.Local");
        else 
            // there are several options for how we could display this, none of
            // which are great:
            //
            // - use Location(loc.name), which is not technically valid syntax
            // - use LoadLocation(loc.name), which will cause a syntax error when
            // embedded and also would require us to escape the string without
            // importing fmt or strconv
            // - try to use FixedZone, which would also require escaping the name
            // and would represent e.g. "America/Los_Angeles" daylight saving time
            // shifts inaccurately
            // - use the pointer format, which is no worse than you'd get with the
            // old fmt.Sprintf("%#v", t) format.
            //
            // Of these, Location(loc.name) is the least disruptive. This is an edge
            // case we hope not to hit too often.
            buf = append(buf, "time.Location(");
            buf = append(buf, (slice<byte>)quote(loc.name));
            buf = append(buf, ")");

    }
    buf = append(buf, ')');
    return string(buf);
}

// Format returns a textual representation of the time value formatted according
// to the layout defined by the argument. See the documentation for the
// constant called Layout to see how to represent the layout format.
//
// The executable example for Time.Format demonstrates the working
// of the layout string in detail and is a good reference.
public static @string Format(this Time t, @string layout) {
    const nint bufSize = 64;

    slice<byte> b = default;
    var max = len(layout) + 10;
    if (max < bufSize) {
        array<byte> buf = new array<byte>(bufSize);
        b = buf[..(int)0];
    }
    else
 {
        b = make_slice<byte>(0, max);
    }
    b = t.AppendFormat(b, layout);
    return string(b);
}

// AppendFormat is like Format but appends the textual
// representation to b and returns the extended buffer.
public static slice<byte> AppendFormat(this Time t, slice<byte> b, @string layout) {

    nint year = -1;    Month month = default;    nint day = default;    nint yday = default;    nint hour = -1;    nint min = default;    nint sec = default; 
    // Each iteration generates one std value.
    while (layout != "") {
        var (prefix, std, suffix) = nextStdChunk(layout);
        if (prefix != "") {
            b = append(b, prefix);
        }
        if (std == 0) {
            break;
        }
        layout = suffix; 

        // Compute year, month, day if needed.
        if (year < 0 && std & stdNeedDate != 0) {
            year, month, day, yday = absDate(abs, true);
            yday++;
        }
        if (hour < 0 && std & stdNeedClock != 0) {
            hour, min, sec = absClock(abs);
        }

        if (std & stdMask == stdYear) 
            var y = year;
            if (y < 0) {
                y = -y;
            }
            b = appendInt(b, y % 100, 2);
        else if (std & stdMask == stdLongYear) 
            b = appendInt(b, year, 4);
        else if (std & stdMask == stdMonth) 
            b = append(b, month.String()[..(int)3]);
        else if (std & stdMask == stdLongMonth) 
            var m = month.String();
            b = append(b, m);
        else if (std & stdMask == stdNumMonth) 
            b = appendInt(b, int(month), 0);
        else if (std & stdMask == stdZeroMonth) 
            b = appendInt(b, int(month), 2);
        else if (std & stdMask == stdWeekDay) 
            b = append(b, absWeekday(abs).String()[..(int)3]);
        else if (std & stdMask == stdLongWeekDay) 
            var s = absWeekday(abs).String();
            b = append(b, s);
        else if (std & stdMask == stdDay) 
            b = appendInt(b, day, 0);
        else if (std & stdMask == stdUnderDay) 
            if (day < 10) {
                b = append(b, ' ');
            }
            b = appendInt(b, day, 0);
        else if (std & stdMask == stdZeroDay) 
            b = appendInt(b, day, 2);
        else if (std & stdMask == stdUnderYearDay) 
            if (yday < 100) {
                b = append(b, ' ');
                if (yday < 10) {
                    b = append(b, ' ');
                }
            }
            b = appendInt(b, yday, 0);
        else if (std & stdMask == stdZeroYearDay) 
            b = appendInt(b, yday, 3);
        else if (std & stdMask == stdHour) 
            b = appendInt(b, hour, 2);
        else if (std & stdMask == stdHour12) 
            // Noon is 12PM, midnight is 12AM.
            var hr = hour % 12;
            if (hr == 0) {
                hr = 12;
            }
            b = appendInt(b, hr, 0);
        else if (std & stdMask == stdZeroHour12) 
            // Noon is 12PM, midnight is 12AM.
            hr = hour % 12;
            if (hr == 0) {
                hr = 12;
            }
            b = appendInt(b, hr, 2);
        else if (std & stdMask == stdMinute) 
            b = appendInt(b, min, 0);
        else if (std & stdMask == stdZeroMinute) 
            b = appendInt(b, min, 2);
        else if (std & stdMask == stdSecond) 
            b = appendInt(b, sec, 0);
        else if (std & stdMask == stdZeroSecond) 
            b = appendInt(b, sec, 2);
        else if (std & stdMask == stdPM) 
            if (hour >= 12) {
                b = append(b, "PM");
            }
            else
 {
                b = append(b, "AM");
            }
        else if (std & stdMask == stdpm) 
            if (hour >= 12) {
                b = append(b, "pm");
            }
            else
 {
                b = append(b, "am");
            }
        else if (std & stdMask == stdISO8601TZ || std & stdMask == stdISO8601ColonTZ || std & stdMask == stdISO8601SecondsTZ || std & stdMask == stdISO8601ShortTZ || std & stdMask == stdISO8601ColonSecondsTZ || std & stdMask == stdNumTZ || std & stdMask == stdNumColonTZ || std & stdMask == stdNumSecondsTz || std & stdMask == stdNumShortTZ || std & stdMask == stdNumColonSecondsTZ) 
            // Ugly special case. We cheat and take the "Z" variants
            // to mean "the time zone as formatted for ISO 8601".
            if (offset == 0 && (std == stdISO8601TZ || std == stdISO8601ColonTZ || std == stdISO8601SecondsTZ || std == stdISO8601ShortTZ || std == stdISO8601ColonSecondsTZ)) {
                b = append(b, 'Z');
                break;
            }
            var zone = offset / 60; // convert to minutes
            var absoffset = offset;
            if (zone < 0) {
                b = append(b, '-');
                zone = -zone;
                absoffset = -absoffset;
            }
            else
 {
                b = append(b, '+');
            }
            b = appendInt(b, zone / 60, 2);
            if (std == stdISO8601ColonTZ || std == stdNumColonTZ || std == stdISO8601ColonSecondsTZ || std == stdNumColonSecondsTZ) {
                b = append(b, ':');
            }
            if (std != stdNumShortTZ && std != stdISO8601ShortTZ) {
                b = appendInt(b, zone % 60, 2);
            } 

            // append seconds if appropriate
            if (std == stdISO8601SecondsTZ || std == stdNumSecondsTz || std == stdNumColonSecondsTZ || std == stdISO8601ColonSecondsTZ) {
                if (std == stdNumColonSecondsTZ || std == stdISO8601ColonSecondsTZ) {
                    b = append(b, ':');
                }
                b = appendInt(b, absoffset % 60, 2);
            }
        else if (std & stdMask == stdTZ) 
            if (name != "") {
                b = append(b, name);
                break;
            } 
            // No time zone known for this time, but we must print one.
            // Use the -0700 format.
            zone = offset / 60; // convert to minutes
            if (zone < 0) {
                b = append(b, '-');
                zone = -zone;
            }
            else
 {
                b = append(b, '+');
            }
            b = appendInt(b, zone / 60, 2);
            b = appendInt(b, zone % 60, 2);
        else if (std & stdMask == stdFracSecond0 || std & stdMask == stdFracSecond9) 
            b = formatNano(b, uint(t.Nanosecond()), std);
            }
    return b;
}

private static var errBad = errors.New("bad value for field"); // placeholder not passed to user

// ParseError describes a problem parsing a time string.
public partial struct ParseError {
    public @string Layout;
    public @string Value;
    public @string LayoutElem;
    public @string ValueElem;
    public @string Message;
}

// These are borrowed from unicode/utf8 and strconv and replicate behavior in
// that package, since we can't take a dependency on either.
private static readonly @string lowerhex = "0123456789abcdef";
private static readonly nuint runeSelf = 0x80;
private static readonly char runeError = '\uFFFD';

private static @string quote(@string s) {
    var buf = make_slice<byte>(1, len(s) + 2); // slice will be at least len(s) + quotes
    buf[0] = '"';
    foreach (var (i, c) in s) {
        if (c >= runeSelf || c < ' ') { 
            // This means you are asking us to parse a time.Duration or
            // time.Location with unprintable or non-ASCII characters in it.
            // We don't expect to hit this case very often. We could try to
            // reproduce strconv.Quote's behavior with full fidelity but
            // given how rarely we expect to hit these edge cases, speed and
            // conciseness are better.
            nint width = default;
            if (c == runeError) {
                width = 1;
                if (i + 2 < len(s) && s[(int)i..(int)i + 3] == string(runeError)) {
                    width = 3;
                }
            }
            else
 {
                width = len(string(c));
            }
            for (nint j = 0; j < width; j++) {
                buf = append(buf, "\\x");
                buf = append(buf, lowerhex[s[i + j] >> 4]);
                buf = append(buf, lowerhex[s[i + j] & 0xF]);
            }
        else
        } {
            if (c == '"' || c == '\\') {
                buf = append(buf, '\\');
            }
            buf = append(buf, string(c));
        }
    }    buf = append(buf, '"');
    return string(buf);
}

// Error returns the string representation of a ParseError.
private static @string Error(this ptr<ParseError> _addr_e) {
    ref ParseError e = ref _addr_e.val;

    if (e.Message == "") {
        return "parsing time " + quote(e.Value) + " as " + quote(e.Layout) + ": cannot parse " + quote(e.ValueElem) + " as " + quote(e.LayoutElem);
    }
    return "parsing time " + quote(e.Value) + e.Message;
}

// isDigit reports whether s[i] is in range and is a decimal digit.
private static bool isDigit(@string s, nint i) {
    if (len(s) <= i) {
        return false;
    }
    var c = s[i];
    return '0' <= c && c <= '9';
}

// getnum parses s[0:1] or s[0:2] (fixed forces s[0:2])
// as a decimal integer and returns the integer and the
// remainder of the string.
private static (nint, @string, error) getnum(@string s, bool @fixed) {
    nint _p0 = default;
    @string _p0 = default;
    error _p0 = default!;

    if (!isDigit(s, 0)) {
        return (0, s, error.As(errBad)!);
    }
    if (!isDigit(s, 1)) {
        if (fixed) {
            return (0, s, error.As(errBad)!);
        }
        return (int(s[0] - '0'), s[(int)1..], error.As(null!)!);
    }
    return (int(s[0] - '0') * 10 + int(s[1] - '0'), s[(int)2..], error.As(null!)!);
}

// getnum3 parses s[0:1], s[0:2], or s[0:3] (fixed forces s[0:3])
// as a decimal integer and returns the integer and the remainder
// of the string.
private static (nint, @string, error) getnum3(@string s, bool @fixed) {
    nint _p0 = default;
    @string _p0 = default;
    error _p0 = default!;

    nint n = default;    nint i = default;

    for (i = 0; i < 3 && isDigit(s, i); i++) {
        n = n * 10 + int(s[i] - '0');
    }
    if (i == 0 || fixed && i != 3) {
        return (0, s, error.As(errBad)!);
    }
    return (n, s[(int)i..], error.As(null!)!);
}

private static @string cutspace(@string s) {
    while (len(s) > 0 && s[0] == ' ') {
        s = s[(int)1..];
    }
    return s;
}

// skip removes the given prefix from value,
// treating runs of space characters as equivalent.
private static (@string, error) skip(@string value, @string prefix) {
    @string _p0 = default;
    error _p0 = default!;

    while (len(prefix) > 0) {
        if (prefix[0] == ' ') {
            if (len(value) > 0 && value[0] != ' ') {
                return (value, error.As(errBad)!);
            }
            prefix = cutspace(prefix);
            value = cutspace(value);
            continue;
        }
        if (len(value) == 0 || value[0] != prefix[0]) {
            return (value, error.As(errBad)!);
        }
        prefix = prefix[(int)1..];
        value = value[(int)1..];
    }
    return (value, error.As(null!)!);
}

// Parse parses a formatted string and returns the time value it represents.
// See the documentation for the constant called Layout to see how to
// represent the format. The second argument must be parseable using
// the format string (layout) provided as the first argument.
//
// The example for Time.Format demonstrates the working of the layout string
// in detail and is a good reference.
//
// When parsing (only), the input may contain a fractional second
// field immediately after the seconds field, even if the layout does not
// signify its presence. In that case either a comma or a decimal point
// followed by a maximal series of digits is parsed as a fractional second.
//
// Elements omitted from the layout are assumed to be zero or, when
// zero is impossible, one, so parsing "3:04pm" returns the time
// corresponding to Jan 1, year 0, 15:04:00 UTC (note that because the year is
// 0, this time is before the zero Time).
// Years must be in the range 0000..9999. The day of the week is checked
// for syntax but it is otherwise ignored.
//
// For layouts specifying the two-digit year 06, a value NN >= 69 will be treated
// as 19NN and a value NN < 69 will be treated as 20NN.
//
// The remainder of this comment describes the handling of time zones.
//
// In the absence of a time zone indicator, Parse returns a time in UTC.
//
// When parsing a time with a zone offset like -0700, if the offset corresponds
// to a time zone used by the current location (Local), then Parse uses that
// location and zone in the returned time. Otherwise it records the time as
// being in a fabricated location with time fixed at the given zone offset.
//
// When parsing a time with a zone abbreviation like MST, if the zone abbreviation
// has a defined offset in the current location, then that offset is used.
// The zone abbreviation "UTC" is recognized as UTC regardless of location.
// If the zone abbreviation is unknown, Parse records the time as being
// in a fabricated location with the given zone abbreviation and a zero offset.
// This choice means that such a time can be parsed and reformatted with the
// same layout losslessly, but the exact instant used in the representation will
// differ by the actual zone offset. To avoid such problems, prefer time layouts
// that use a numeric zone offset, or use ParseInLocation.
public static (Time, error) Parse(@string layout, @string value) {
    Time _p0 = default;
    error _p0 = default!;

    return parse(layout, value, _addr_UTC, _addr_Local);
}

// ParseInLocation is like Parse but differs in two important ways.
// First, in the absence of time zone information, Parse interprets a time as UTC;
// ParseInLocation interprets the time as in the given location.
// Second, when given a zone offset or abbreviation, Parse tries to match it
// against the Local location; ParseInLocation uses the given location.
public static (Time, error) ParseInLocation(@string layout, @string value, ptr<Location> _addr_loc) {
    Time _p0 = default;
    error _p0 = default!;
    ref Location loc = ref _addr_loc.val;

    return parse(layout, value, _addr_loc, _addr_loc);
}

private static (Time, error) parse(@string layout, @string value, ptr<Location> _addr_defaultLocation, ptr<Location> _addr_local) {
    Time _p0 = default;
    error _p0 = default!;
    ref Location defaultLocation = ref _addr_defaultLocation.val;
    ref Location local = ref _addr_local.val;

    var alayout = layout;
    var avalue = value;
    @string rangeErrString = ""; // set if a value is out of range
    var amSet = false; // do we need to subtract 12 from the hour for midnight?
    var pmSet = false; // do we need to add 12 to the hour?

    // Time being constructed.
    nint year = default;    nint month = -1;    nint day = -1;    nint yday = -1;    nint hour = default;    nint min = default;    nint sec = default;    nint nsec = default;    ptr<Location> z;    nint zoneOffset = -1;    @string zoneName = default; 

    // Each iteration processes one std value.
    while (true) {
        error err = default!;
        var (prefix, std, suffix) = nextStdChunk(layout);
        var stdstr = layout[(int)len(prefix)..(int)len(layout) - len(suffix)];
        value, err = skip(value, prefix);
        if (err != null) {
            return (new Time(), error.As(addr(new ParseError(alayout,avalue,prefix,value,""))!)!);
        }
        if (std == 0) {
            if (len(value) != 0) {
                return (new Time(), error.As(addr(new ParseError(alayout,avalue,"",value,": extra text: "+quote(value)))!)!);
            }
            break;
        }
        layout = suffix;
        @string p = default;

        if (std & stdMask == stdYear) 
            if (len(value) < 2) {
                err = error.As(errBad)!;
                break;
            }
            var hold = value;
            (p, value) = (value[(int)0..(int)2], value[(int)2..]);            year, err = atoi(p);
            if (err != null) {
                value = hold;
            }
            else if (year >= 69) { // Unix time starts Dec 31 1969 in some time zones
                year += 1900;
            }
            else
 {
                year += 2000;
            }
        else if (std & stdMask == stdLongYear) 
            if (len(value) < 4 || !isDigit(value, 0)) {
                err = error.As(errBad)!;
                break;
            }
            (p, value) = (value[(int)0..(int)4], value[(int)4..]);            year, err = atoi(p);
        else if (std & stdMask == stdMonth) 
            month, value, err = lookup(shortMonthNames, value);
            month++;
        else if (std & stdMask == stdLongMonth) 
            month, value, err = lookup(longMonthNames, value);
            month++;
        else if (std & stdMask == stdNumMonth || std & stdMask == stdZeroMonth) 
            month, value, err = getnum(value, std == stdZeroMonth);
            if (err == null && (month <= 0 || 12 < month)) {
                rangeErrString = "month";
            }
        else if (std & stdMask == stdWeekDay) 
            // Ignore weekday except for error checking.
            _, value, err = lookup(shortDayNames, value);
        else if (std & stdMask == stdLongWeekDay) 
            _, value, err = lookup(longDayNames, value);
        else if (std & stdMask == stdDay || std & stdMask == stdUnderDay || std & stdMask == stdZeroDay) 
            if (std == stdUnderDay && len(value) > 0 && value[0] == ' ') {
                value = value[(int)1..];
            }
            day, value, err = getnum(value, std == stdZeroDay); 
            // Note that we allow any one- or two-digit day here.
            // The month, day, year combination is validated after we've completed parsing.
        else if (std & stdMask == stdUnderYearDay || std & stdMask == stdZeroYearDay) 
            {
                nint i__prev2 = i;

                for (nint i = 0; i < 2; i++) {
                    if (std == stdUnderYearDay && len(value) > 0 && value[0] == ' ') {
                        value = value[(int)1..];
                    }
                }


                i = i__prev2;
            }
            yday, value, err = getnum3(value, std == stdZeroYearDay); 
            // Note that we allow any one-, two-, or three-digit year-day here.
            // The year-day, year combination is validated after we've completed parsing.
        else if (std & stdMask == stdHour) 
            hour, value, err = getnum(value, false);
            if (hour < 0 || 24 <= hour) {
                rangeErrString = "hour";
            }
        else if (std & stdMask == stdHour12 || std & stdMask == stdZeroHour12) 
            hour, value, err = getnum(value, std == stdZeroHour12);
            if (hour < 0 || 12 < hour) {
                rangeErrString = "hour";
            }
        else if (std & stdMask == stdMinute || std & stdMask == stdZeroMinute) 
            min, value, err = getnum(value, std == stdZeroMinute);
            if (min < 0 || 60 <= min) {
                rangeErrString = "minute";
            }
        else if (std & stdMask == stdSecond || std & stdMask == stdZeroSecond) 
            sec, value, err = getnum(value, std == stdZeroSecond);
            if (sec < 0 || 60 <= sec) {
                rangeErrString = "second";
                break;
            } 
            // Special case: do we have a fractional second but no
            // fractional second in the format?
            if (len(value) >= 2 && commaOrPeriod(value[0]) && isDigit(value, 1)) {
                _, std, _ = nextStdChunk(layout);
                std &= stdMask;
                if (std == stdFracSecond0 || std == stdFracSecond9) { 
                    // Fractional second in the layout; proceed normally
                    break;
                } 
                // No fractional second in the layout but we have one in the input.
                nint n = 2;
                while (n < len(value) && isDigit(value, n)) {
                    n++;
                }

                nsec, rangeErrString, err = parseNanoseconds(value, n);
                value = value[(int)n..];
            }
        else if (std & stdMask == stdPM) 
            if (len(value) < 2) {
                err = error.As(errBad)!;
                break;
            }
            (p, value) = (value[(int)0..(int)2], value[(int)2..]);            switch (p) {
                case "PM": 
                    pmSet = true;
                    break;
                case "AM": 
                    amSet = true;
                    break;
                default: 
                    err = error.As(errBad)!;
                    break;
            }
        else if (std & stdMask == stdpm) 
            if (len(value) < 2) {
                err = error.As(errBad)!;
                break;
            }
            (p, value) = (value[(int)0..(int)2], value[(int)2..]);            switch (p) {
                case "pm": 
                    pmSet = true;
                    break;
                case "am": 
                    amSet = true;
                    break;
                default: 
                    err = error.As(errBad)!;
                    break;
            }
        else if (std & stdMask == stdISO8601TZ || std & stdMask == stdISO8601ColonTZ || std & stdMask == stdISO8601SecondsTZ || std & stdMask == stdISO8601ShortTZ || std & stdMask == stdISO8601ColonSecondsTZ || std & stdMask == stdNumTZ || std & stdMask == stdNumShortTZ || std & stdMask == stdNumColonTZ || std & stdMask == stdNumSecondsTz || std & stdMask == stdNumColonSecondsTZ) 
            if ((std == stdISO8601TZ || std == stdISO8601ShortTZ || std == stdISO8601ColonTZ) && len(value) >= 1 && value[0] == 'Z') {
                value = value[(int)1..];
                z = UTC;
                break;
            }
            @string sign = default;            hour = default;            min = default;            @string seconds = default;

            if (std == stdISO8601ColonTZ || std == stdNumColonTZ) {
                if (len(value) < 6) {
                    err = error.As(errBad)!;
                    break;
                }
                if (value[3] != ':') {
                    err = error.As(errBad)!;
                    break;
                }
                (sign, hour, min, seconds, value) = (value[(int)0..(int)1], value[(int)1..(int)3], value[(int)4..(int)6], "00", value[(int)6..]);
            }
            else if (std == stdNumShortTZ || std == stdISO8601ShortTZ) {
                if (len(value) < 3) {
                    err = error.As(errBad)!;
                    break;
                }
                (sign, hour, min, seconds, value) = (value[(int)0..(int)1], value[(int)1..(int)3], "00", "00", value[(int)3..]);
            }
            else if (std == stdISO8601ColonSecondsTZ || std == stdNumColonSecondsTZ) {
                if (len(value) < 9) {
                    err = error.As(errBad)!;
                    break;
                }
                if (value[3] != ':' || value[6] != ':') {
                    err = error.As(errBad)!;
                    break;
                }
                (sign, hour, min, seconds, value) = (value[(int)0..(int)1], value[(int)1..(int)3], value[(int)4..(int)6], value[(int)7..(int)9], value[(int)9..]);
            }
            else if (std == stdISO8601SecondsTZ || std == stdNumSecondsTz) {
                if (len(value) < 7) {
                    err = error.As(errBad)!;
                    break;
                }
                (sign, hour, min, seconds, value) = (value[(int)0..(int)1], value[(int)1..(int)3], value[(int)3..(int)5], value[(int)5..(int)7], value[(int)7..]);
            }
            else
 {
                if (len(value) < 5) {
                    err = error.As(errBad)!;
                    break;
                }
                (sign, hour, min, seconds, value) = (value[(int)0..(int)1], value[(int)1..(int)3], value[(int)3..(int)5], "00", value[(int)5..]);
            }
            nint hr = default;            nint mm = default;            nint ss = default;

            hr, err = atoi(hour);
            if (err == null) {
                mm, err = atoi(min);
            }
            if (err == null) {
                ss, err = atoi(seconds);
            }
            zoneOffset = (hr * 60 + mm) * 60 + ss; // offset is in seconds
            switch (sign[0]) {
                case '+': 

                    break;
                case '-': 
                    zoneOffset = -zoneOffset;
                    break;
                default: 
                    err = error.As(errBad)!;
                    break;
            }
        else if (std & stdMask == stdTZ) 
            // Does it look like a time zone?
            if (len(value) >= 3 && value[(int)0..(int)3] == "UTC") {
                z = UTC;
                value = value[(int)3..];
                break;
            }
            var (n, ok) = parseTimeZone(value);
            if (!ok) {
                err = error.As(errBad)!;
                break;
            }
            (zoneName, value) = (value[..(int)n], value[(int)n..]);        else if (std & stdMask == stdFracSecond0) 
            // stdFracSecond0 requires the exact number of digits as specified in
            // the layout.
            nint ndigit = 1 + digitsLen(std);
            if (len(value) < ndigit) {
                err = error.As(errBad)!;
                break;
            }
            nsec, rangeErrString, err = parseNanoseconds(value, ndigit);
            value = value[(int)ndigit..];
        else if (std & stdMask == stdFracSecond9) 
            if (len(value) < 2 || !commaOrPeriod(value[0]) || value[1] < '0' || '9' < value[1]) { 
                // Fractional second omitted.
                break;
            } 
            // Take any number of digits, even more than asked for,
            // because it is what the stdSecond case would do.
            i = 0;
            while (i < 9 && i + 1 < len(value) && '0' <= value[i + 1] && value[i + 1] <= '9') {
                i++;
            }

            nsec, rangeErrString, err = parseNanoseconds(value, 1 + i);
            value = value[(int)1 + i..];
                if (rangeErrString != "") {
            return (new Time(), error.As(addr(new ParseError(alayout,avalue,stdstr,value,": "+rangeErrString+" out of range"))!)!);
        }
        if (err != null) {
            return (new Time(), error.As(addr(new ParseError(alayout,avalue,stdstr,value,""))!)!);
        }
    }
    if (pmSet && hour < 12) {
        hour += 12;
    }
    else if (amSet && hour == 12) {
        hour = 0;
    }
    if (yday >= 0) {
        nint d = default;
        nint m = default;
        if (isLeap(year)) {
            if (yday == 31 + 29) {
                m = int(February);
                d = 29;
            }
            else if (yday > 31 + 29) {
                yday--;
            }
        }
        if (yday < 1 || yday > 365) {
            return (new Time(), error.As(addr(new ParseError(alayout,avalue,"",value,": day-of-year out of range"))!)!);
        }
        if (m == 0) {
            m = (yday - 1) / 31 + 1;
            if (int(daysBefore[m]) < yday) {
                m++;
            }
            d = yday - int(daysBefore[m - 1]);
        }
        if (month >= 0 && month != m) {
            return (new Time(), error.As(addr(new ParseError(alayout,avalue,"",value,": day-of-year does not match month"))!)!);
        }
        month = m;
        if (day >= 0 && day != d) {
            return (new Time(), error.As(addr(new ParseError(alayout,avalue,"",value,": day-of-year does not match day"))!)!);
        }
        day = d;
    }
    else
 {
        if (month < 0) {
            month = int(January);
        }
        if (day < 0) {
            day = 1;
        }
    }
    if (day < 1 || day > daysIn(Month(month), year)) {
        return (new Time(), error.As(addr(new ParseError(alayout,avalue,"",value,": day out of range"))!)!);
    }
    if (z != null) {
        return (Date(year, Month(month), day, hour, min, sec, nsec, z), error.As(null!)!);
    }
    if (zoneOffset != -1) {
        var t = Date(year, Month(month), day, hour, min, sec, nsec, UTC);
        t.addSec(-int64(zoneOffset)); 

        // Look for local zone with the given offset.
        // If that zone was in effect at the given time, use it.
        var (name, offset, _, _, _) = local.lookup(t.unixSec());
        if (offset == zoneOffset && (zoneName == "" || name == zoneName)) {
            t.setLoc(local);
            return (t, error.As(null!)!);
        }
        t.setLoc(FixedZone(zoneName, zoneOffset));
        return (t, error.As(null!)!);
    }
    if (zoneName != "") {
        t = Date(year, Month(month), day, hour, min, sec, nsec, UTC); 
        // Look for local zone with the given offset.
        // If that zone was in effect at the given time, use it.
        var (offset, ok) = local.lookupName(zoneName, t.unixSec());
        if (ok) {
            t.addSec(-int64(offset));
            t.setLoc(local);
            return (t, error.As(null!)!);
        }
        if (len(zoneName) > 3 && zoneName[..(int)3] == "GMT") {
            offset, _ = atoi(zoneName[(int)3..]); // Guaranteed OK by parseGMT.
            offset *= 3600;
        }
        t.setLoc(FixedZone(zoneName, offset));
        return (t, error.As(null!)!);
    }
    return (Date(year, Month(month), day, hour, min, sec, nsec, defaultLocation), error.As(null!)!);
}

// parseTimeZone parses a time zone string and returns its length. Time zones
// are human-generated and unpredictable. We can't do precise error checking.
// On the other hand, for a correct parse there must be a time zone at the
// beginning of the string, so it's almost always true that there's one
// there. We look at the beginning of the string for a run of upper-case letters.
// If there are more than 5, it's an error.
// If there are 4 or 5 and the last is a T, it's a time zone.
// If there are 3, it's a time zone.
// Otherwise, other than special cases, it's not a time zone.
// GMT is special because it can have an hour offset.
private static (nint, bool) parseTimeZone(@string value) {
    nint length = default;
    bool ok = default;

    if (len(value) < 3) {
        return (0, false);
    }
    if (len(value) >= 4 && (value[..(int)4] == "ChST" || value[..(int)4] == "MeST")) {
        return (4, true);
    }
    if (value[..(int)3] == "GMT") {
        length = parseGMT(value);
        return (length, true);
    }
    if (value[0] == '+' || value[0] == '-') {
        length = parseSignedOffset(value);
        var ok = length > 0; // parseSignedOffset returns 0 in case of bad input
        return (length, ok);
    }
    nint nUpper = default;
    for (nUpper = 0; nUpper < 6; nUpper++) {
        if (nUpper >= len(value)) {
            break;
        }
        {
            var c = value[nUpper];

            if (c < 'A' || 'Z' < c) {
                break;
            }

        }
    }
    switch (nUpper) {
        case 0: 

        case 1: 

        case 2: 

        case 6: 
            return (0, false);
            break;
        case 5: // Must end in T to match.
            if (value[4] == 'T') {
                return (5, true);
            }
            break;
        case 4: 
            // Must end in T, except one special case.
            if (value[3] == 'T' || value[..(int)4] == "WITA") {
                return (4, true);
            }
            break;
        case 3: 
            return (3, true);
            break;
    }
    return (0, false);
}

// parseGMT parses a GMT time zone. The input string is known to start "GMT".
// The function checks whether that is followed by a sign and a number in the
// range -23 through +23 excluding zero.
private static nint parseGMT(@string value) {
    value = value[(int)3..];
    if (len(value) == 0) {
        return 3;
    }
    return 3 + parseSignedOffset(value);
}

// parseSignedOffset parses a signed timezone offset (e.g. "+03" or "-04").
// The function checks for a signed number in the range -23 through +23 excluding zero.
// Returns length of the found offset string or 0 otherwise
private static nint parseSignedOffset(@string value) {
    var sign = value[0];
    if (sign != '-' && sign != '+') {
        return 0;
    }
    var (x, rem, err) = leadingInt(value[(int)1..]); 

    // fail if nothing consumed by leadingInt
    if (err != null || value[(int)1..] == rem) {
        return 0;
    }
    if (sign == '-') {
        x = -x;
    }
    if (x < -23 || 23 < x) {
        return 0;
    }
    return len(value) - len(rem);
}

private static bool commaOrPeriod(byte b) {
    return b == '.' || b == ',';
}

private static (nint, @string, error) parseNanoseconds(@string value, nint nbytes) {
    nint ns = default;
    @string rangeErrString = default;
    error err = default!;

    if (!commaOrPeriod(value[0])) {
        err = errBad;
        return ;
    }
    ns, err = atoi(value[(int)1..(int)nbytes]);

    if (err != null) {
        return ;
    }
    if (ns < 0 || 1e9F <= ns) {
        rangeErrString = "fractional second";
        return ;
    }
    nint scaleDigits = 10 - nbytes;
    for (nint i = 0; i < scaleDigits; i++) {
        ns *= 10;
    }
    return ;
}

private static var errLeadingInt = errors.New("time: bad [0-9]*"); // never printed

// leadingInt consumes the leading [0-9]* from s.
private static (long, @string, error) leadingInt(@string s) {
    long x = default;
    @string rem = default;
    error err = default!;

    nint i = 0;
    while (i < len(s)) {
        var c = s[i];
        if (c < '0' || c > '9') {
            break;
        i++;
        }
        if (x > (1 << 63 - 1) / 10) { 
            // overflow
            return (0, "", error.As(errLeadingInt)!);
        }
        x = x * 10 + int64(c) - '0';
        if (x < 0) { 
            // overflow
            return (0, "", error.As(errLeadingInt)!);
        }
    }
    return (x, s[(int)i..], error.As(null!)!);
}

// leadingFraction consumes the leading [0-9]* from s.
// It is used only for fractions, so does not return an error on overflow,
// it just stops accumulating precision.
private static (long, double, @string) leadingFraction(@string s) {
    long x = default;
    double scale = default;
    @string rem = default;

    nint i = 0;
    scale = 1;
    var overflow = false;
    while (i < len(s)) {
        var c = s[i];
        if (c < '0' || c > '9') {
            break;
        i++;
        }
        if (overflow) {
            continue;
        }
        if (x > (1 << 63 - 1) / 10) { 
            // It's possible for overflow to give a positive number, so take care.
            overflow = true;
            continue;
        }
        var y = x * 10 + int64(c) - '0';
        if (y < 0) {
            overflow = true;
            continue;
        }
        x = y;
        scale *= 10;
    }
    return (x, scale, s[(int)i..]);
}

private static map unitMap = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, long>{"ns":int64(Nanosecond),"us":int64(Microsecond),"µs":int64(Microsecond),"μs":int64(Microsecond),"ms":int64(Millisecond),"s":int64(Second),"m":int64(Minute),"h":int64(Hour),};

// ParseDuration parses a duration string.
// A duration string is a possibly signed sequence of
// decimal numbers, each with optional fraction and a unit suffix,
// such as "300ms", "-1.5h" or "2h45m".
// Valid time units are "ns", "us" (or "µs"), "ms", "s", "m", "h".
public static (Duration, error) ParseDuration(@string s) {
    Duration _p0 = default;
    error _p0 = default!;
 
    // [-+]?([0-9]*(\.[0-9]*)?[a-z]+)+
    var orig = s;
    long d = default;
    var neg = false; 

    // Consume [-+]?
    if (s != "") {
        var c = s[0];
        if (c == '-' || c == '+') {
            neg = c == '-';
            s = s[(int)1..];
        }
    }
    if (s == "0") {
        return (0, error.As(null!)!);
    }
    if (s == "") {
        return (0, error.As(errors.New("time: invalid duration " + quote(orig)))!);
    }
    while (s != "") {
        long v = default;        long f = default; // integers before, after decimal point
        double scale = 1;

        error err = default!; 

        // The next character must be [0-9.]
        if (!(s[0] == '.' || '0' <= s[0] && s[0] <= '9')) {
            return (0, error.As(errors.New("time: invalid duration " + quote(orig)))!);
        }
        var pl = len(s);
        v, s, err = leadingInt(s);
        if (err != null) {
            return (0, error.As(errors.New("time: invalid duration " + quote(orig)))!);
        }
        var pre = pl != len(s); // whether we consumed anything before a period

        // Consume (\.[0-9]*)?
        var post = false;
        if (s != "" && s[0] == '.') {
            s = s[(int)1..];
            pl = len(s);
            f, scale, s = leadingFraction(s);
            post = pl != len(s);
        }
        if (!pre && !post) { 
            // no digits (e.g. ".s" or "-.s")
            return (0, error.As(errors.New("time: invalid duration " + quote(orig)))!);
        }
        nint i = 0;
        while (i < len(s)) {
            c = s[i];
            if (c == '.' || '0' <= c && c <= '9') {
                break;
            i++;
            }
        }
        if (i == 0) {
            return (0, error.As(errors.New("time: missing unit in duration " + quote(orig)))!);
        }
        var u = s[..(int)i];
        s = s[(int)i..];
        var (unit, ok) = unitMap[u];
        if (!ok) {
            return (0, error.As(errors.New("time: unknown unit " + quote(u) + " in duration " + quote(orig)))!);
        }
        if (v > (1 << 63 - 1) / unit) { 
            // overflow
            return (0, error.As(errors.New("time: invalid duration " + quote(orig)))!);
        }
        v *= unit;
        if (f > 0) { 
            // float64 is needed to be nanosecond accurate for fractions of hours.
            // v >= 0 && (f*unit/scale) <= 3.6e+12 (ns/h, h is the largest unit)
            v += int64(float64(f) * (float64(unit) / scale));
            if (v < 0) { 
                // overflow
                return (0, error.As(errors.New("time: invalid duration " + quote(orig)))!);
            }
        }
        d += v;
        if (d < 0) { 
            // overflow
            return (0, error.As(errors.New("time: invalid duration " + quote(orig)))!);
        }
    }

    if (neg) {
        d = -d;
    }
    return (Duration(d), error.As(null!)!);
}

} // end time_package
