// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package time -- go2cs converted at 2020 August 29 08:16:09 UTC
// import "time" ==> using time = go.time_package
// Original source: C:\Go\src\time\format.go
using errors = go.errors_package;
using static go.builtin;

namespace go
{
    public static partial class time_package
    {
        // These are predefined layouts for use in Time.Format and time.Parse.
        // The reference time used in the layouts is the specific time:
        //    Mon Jan 2 15:04:05 MST 2006
        // which is Unix time 1136239445. Since MST is GMT-0700,
        // the reference time can be thought of as
        //    01/02 03:04:05PM '06 -0700
        // To define your own format, write down what the reference time would look
        // like formatted your way; see the values of constants like ANSIC,
        // StampMicro or Kitchen for examples. The model is to demonstrate what the
        // reference time looks like so that the Format and Parse methods can apply
        // the same transformation to a general time value.
        //
        // Some valid layouts are invalid time values for time.Parse, due to formats
        // such as _ for space padding and Z for zone information.
        //
        // Within the format string, an underscore _ represents a space that may be
        // replaced by a digit if the following number (a day) has two digits; for
        // compatibility with fixed-width Unix time formats.
        //
        // A decimal point followed by one or more zeros represents a fractional
        // second, printed to the given number of decimal places. A decimal point
        // followed by one or more nines represents a fractional second, printed to
        // the given number of decimal places, with trailing zeros removed.
        // When parsing (only), the input may contain a fractional second
        // field immediately after the seconds field, even if the layout does not
        // signify its presence. In that case a decimal point followed by a maximal
        // series of digits is parsed as a fractional second.
        //
        // Numeric time zone offsets format as follows:
        //    -0700  ±hhmm
        //    -07:00 ±hh:mm
        //    -07    ±hh
        // Replacing the sign in the format with a Z triggers
        // the ISO 8601 behavior of printing Z instead of an
        // offset for the UTC zone. Thus:
        //    Z0700  Z or ±hhmm
        //    Z07:00 Z or ±hh:mm
        //    Z07    Z or ±hh
        //
        // The recognized day of week formats are "Mon" and "Monday".
        // The recognized month formats are "Jan" and "January".
        //
        // Text in the format string that is not recognized as part of the reference
        // time is echoed verbatim during Format and expected to appear verbatim
        // in the input to Parse.
        //
        // The executable example for Time.Format demonstrates the working
        // of the layout string in detail and is a good reference.
        //
        // Note that the RFC822, RFC850, and RFC1123 formats should be applied
        // only to local times. Applying them to UTC times will use "UTC" as the
        // time zone abbreviation, while strictly speaking those RFCs require the
        // use of "GMT" in that case.
        // In general RFC1123Z should be used instead of RFC1123 for servers
        // that insist on that format, and RFC3339 should be preferred for new protocols.
        // RFC3339, RFC822, RFC822Z, RFC1123, and RFC1123Z are useful for formatting;
        // when used with time.Parse they do not accept all the time formats
        // permitted by the RFCs.
        // The RFC3339Nano format removes trailing zeros from the seconds field
        // and thus may not sort correctly once formatted.
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
        private static readonly stdHour stdZeroDay = iota + stdNeedClock; // "15"
        private static readonly var stdHour12 = 7; // "3"
        private static readonly var stdZeroHour12 = 8; // "03"
        private static readonly var stdMinute = 9; // "4"
        private static readonly var stdZeroMinute = 10; // "04"
        private static readonly var stdSecond = 11; // "5"
        private static readonly stdLongYear stdZeroSecond = iota + stdNeedDate; // "2006"
        private static readonly stdPM stdYear = iota + stdNeedClock; // "PM"
        private static readonly stdTZ stdpm = iota; // "MST"
        private static readonly var stdISO8601TZ = 12; // "Z0700"  // prints Z for UTC
        private static readonly var stdISO8601SecondsTZ = 13; // "Z070000"
        private static readonly var stdISO8601ShortTZ = 14; // "Z07"
        private static readonly var stdISO8601ColonTZ = 15; // "Z07:00" // prints Z for UTC
        private static readonly var stdISO8601ColonSecondsTZ = 16; // "Z07:00:00"
        private static readonly var stdNumTZ = 17; // "-0700"  // always numeric
        private static readonly var stdNumSecondsTz = 18; // "-070000"
        private static readonly var stdNumShortTZ = 19; // "-07"    // always numeric
        private static readonly var stdNumColonTZ = 20; // "-07:00" // always numeric
        private static readonly var stdNumColonSecondsTZ = 21; // "-07:00:00"
        private static readonly var stdFracSecond0 = 22; // ".0", ".00", ... , trailing zeros included
        private static readonly stdNeedDate stdFracSecond9 = 1L << (int)(8L); // need month, day, year
        private static readonly long stdNeedClock = 2L << (int)(8L); // need hour, minute, second
        private static readonly long stdArgShift = 16L; // extra argument in high bits, above low stdArgShift
        private static readonly long stdMask = 1L << (int)(stdArgShift) - 1L; // mask out argument

        // std0x records the std values for "01", "02", ..., "06".
        private static array<long> std0x = new array<long>(new long[] { stdZeroMonth, stdZeroDay, stdZeroHour12, stdZeroMinute, stdZeroSecond, stdYear });

        // startsWithLowerCase reports whether the string has a lower-case letter at the beginning.
        // Its purpose is to prevent matching strings like "Month" when looking for "Mon".
        private static bool startsWithLowerCase(@string str)
        {
            if (len(str) == 0L)
            {
                return false;
            }
            var c = str[0L];
            return 'a' <= c && c <= 'z';
        }

        // nextStdChunk finds the first occurrence of a std string in
        // layout and returns the text before, the std string, and the text after.
        private static (@string, long, @string) nextStdChunk(@string layout)
        {
            for (long i = 0L; i < len(layout); i++)
            {
                {
                    var c = int(layout[i]);

                    switch (c)
                    {
                        case 'J': // January, Jan
                            if (len(layout) >= i + 3L && layout[i..i + 3L] == "Jan")
                            {
                                if (len(layout) >= i + 7L && layout[i..i + 7L] == "January")
                                {
                                    return (layout[0L..i], stdLongMonth, layout[i + 7L..]);
                                }
                                if (!startsWithLowerCase(layout[i + 3L..]))
                                {
                                    return (layout[0L..i], stdMonth, layout[i + 3L..]);
                                }
                            }
                            break;
                        case 'M': // Monday, Mon, MST
                            if (len(layout) >= i + 3L)
                            {
                                if (layout[i..i + 3L] == "Mon")
                                {
                                    if (len(layout) >= i + 6L && layout[i..i + 6L] == "Monday")
                                    {
                                        return (layout[0L..i], stdLongWeekDay, layout[i + 6L..]);
                                    }
                                    if (!startsWithLowerCase(layout[i + 3L..]))
                                    {
                                        return (layout[0L..i], stdWeekDay, layout[i + 3L..]);
                                    }
                                }
                                if (layout[i..i + 3L] == "MST")
                                {
                                    return (layout[0L..i], stdTZ, layout[i + 3L..]);
                                }
                            }
                            break;
                        case '0': // 01, 02, 03, 04, 05, 06
                            if (len(layout) >= i + 2L && '1' <= layout[i + 1L] && layout[i + 1L] <= '6')
                            {
                                return (layout[0L..i], std0x[layout[i + 1L] - '1'], layout[i + 2L..]);
                            }
                            break;
                        case '1': // 15, 1
                            if (len(layout) >= i + 2L && layout[i + 1L] == '5')
                            {
                                return (layout[0L..i], stdHour, layout[i + 2L..]);
                            }
                            return (layout[0L..i], stdNumMonth, layout[i + 1L..]);
                            break;
                        case '2': // 2006, 2
                            if (len(layout) >= i + 4L && layout[i..i + 4L] == "2006")
                            {
                                return (layout[0L..i], stdLongYear, layout[i + 4L..]);
                            }
                            return (layout[0L..i], stdDay, layout[i + 1L..]);
                            break;
                        case '_': // _2, _2006
                            if (len(layout) >= i + 2L && layout[i + 1L] == '2')
                            { 
                                //_2006 is really a literal _, followed by stdLongYear
                                if (len(layout) >= i + 5L && layout[i + 1L..i + 5L] == "2006")
                                {
                                    return (layout[0L..i + 1L], stdLongYear, layout[i + 5L..]);
                                }
                                return (layout[0L..i], stdUnderDay, layout[i + 2L..]);
                            }
                            break;
                        case '3': 
                            return (layout[0L..i], stdHour12, layout[i + 1L..]);
                            break;
                        case '4': 
                            return (layout[0L..i], stdMinute, layout[i + 1L..]);
                            break;
                        case '5': 
                            return (layout[0L..i], stdSecond, layout[i + 1L..]);
                            break;
                        case 'P': // PM
                            if (len(layout) >= i + 2L && layout[i + 1L] == 'M')
                            {
                                return (layout[0L..i], stdPM, layout[i + 2L..]);
                            }
                            break;
                        case 'p': // pm
                            if (len(layout) >= i + 2L && layout[i + 1L] == 'm')
                            {
                                return (layout[0L..i], stdpm, layout[i + 2L..]);
                            }
                            break;
                        case '-': // -070000, -07:00:00, -0700, -07:00, -07
                            if (len(layout) >= i + 7L && layout[i..i + 7L] == "-070000")
                            {
                                return (layout[0L..i], stdNumSecondsTz, layout[i + 7L..]);
                            }
                            if (len(layout) >= i + 9L && layout[i..i + 9L] == "-07:00:00")
                            {
                                return (layout[0L..i], stdNumColonSecondsTZ, layout[i + 9L..]);
                            }
                            if (len(layout) >= i + 5L && layout[i..i + 5L] == "-0700")
                            {
                                return (layout[0L..i], stdNumTZ, layout[i + 5L..]);
                            }
                            if (len(layout) >= i + 6L && layout[i..i + 6L] == "-07:00")
                            {
                                return (layout[0L..i], stdNumColonTZ, layout[i + 6L..]);
                            }
                            if (len(layout) >= i + 3L && layout[i..i + 3L] == "-07")
                            {
                                return (layout[0L..i], stdNumShortTZ, layout[i + 3L..]);
                            }
                            break;
                        case 'Z': // Z070000, Z07:00:00, Z0700, Z07:00,
                            if (len(layout) >= i + 7L && layout[i..i + 7L] == "Z070000")
                            {
                                return (layout[0L..i], stdISO8601SecondsTZ, layout[i + 7L..]);
                            }
                            if (len(layout) >= i + 9L && layout[i..i + 9L] == "Z07:00:00")
                            {
                                return (layout[0L..i], stdISO8601ColonSecondsTZ, layout[i + 9L..]);
                            }
                            if (len(layout) >= i + 5L && layout[i..i + 5L] == "Z0700")
                            {
                                return (layout[0L..i], stdISO8601TZ, layout[i + 5L..]);
                            }
                            if (len(layout) >= i + 6L && layout[i..i + 6L] == "Z07:00")
                            {
                                return (layout[0L..i], stdISO8601ColonTZ, layout[i + 6L..]);
                            }
                            if (len(layout) >= i + 3L && layout[i..i + 3L] == "Z07")
                            {
                                return (layout[0L..i], stdISO8601ShortTZ, layout[i + 3L..]);
                            }
                            break;
                        case '.': // .000 or .999 - repeated digits for fractional seconds.
                            if (i + 1L < len(layout) && (layout[i + 1L] == '0' || layout[i + 1L] == '9'))
                            {
                                var ch = layout[i + 1L];
                                var j = i + 1L;
                                while (j < len(layout) && layout[j] == ch)
                                {
                                    j++;
                                } 
                                // String of digits must end here - only fractional second is all digits.

                                // String of digits must end here - only fractional second is all digits.
                                if (!isDigit(layout, j))
                                {
                                    var std = stdFracSecond0;
                                    if (layout[i + 1L] == '9')
                                    {
                                        std = stdFracSecond9;
                                    }
                                    std |= (j - (i + 1L)) << (int)(stdArgShift);
                                    return (layout[0L..i], std, layout[j..]);
                                }
                            }
                            break;
                    }
                }
            }

            return (layout, 0L, "");
        }

        private static @string longDayNames = new slice<@string>(new @string[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" });

        private static @string shortDayNames = new slice<@string>(new @string[] { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" });

        private static @string shortMonthNames = new slice<@string>(new @string[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" });

        private static @string longMonthNames = new slice<@string>(new @string[] { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" });

        // match reports whether s1 and s2 match ignoring case.
        // It is assumed s1 and s2 are the same length.
        private static bool match(@string s1, @string s2)
        {
            for (long i = 0L; i < len(s1); i++)
            {
                var c1 = s1[i];
                var c2 = s2[i];
                if (c1 != c2)
                { 
                    // Switch to lower-case; 'a'-'A' is known to be a single bit.
                    c1 |= 'a' - 'A';
                    c2 |= 'a' - 'A';
                    if (c1 != c2 || c1 < 'a' || c1 > 'z')
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static (long, @string, error) lookup(slice<@string> tab, @string val)
        {
            foreach (var (i, v) in tab)
            {
                if (len(val) >= len(v) && match(val[0L..len(v)], v))
                {
                    return (i, val[len(v)..], null);
                }
            }
            return (-1L, val, errBad);
        }

        // appendInt appends the decimal form of x to b and returns the result.
        // If the decimal form (excluding sign) is shorter than width, the result is padded with leading 0's.
        // Duplicates functionality in strconv, but avoids dependency.
        private static slice<byte> appendInt(slice<byte> b, long x, long width)
        {
            var u = uint(x);
            if (x < 0L)
            {
                b = append(b, '-');
                u = uint(-x);
            } 

            // Assemble decimal in reverse order.
            array<byte> buf = new array<byte>(20L);
            var i = len(buf);
            while (u >= 10L)
            {
                i--;
                var q = u / 10L;
                buf[i] = byte('0' + u - q * 10L);
                u = q;
            }

            i--;
            buf[i] = byte('0' + u); 

            // Add 0-padding.
            for (var w = len(buf) - i; w < width; w++)
            {
                b = append(b, '0');
            }


            return append(b, buf[i..]);
        }

        // Never printed, just needs to be non-nil for return by atoi.
        private static var atoiError = errors.New("time: invalid number");

        // Duplicates functionality in strconv, but avoids dependency.
        private static (long, error) atoi(@string s)
        {
            var neg = false;
            if (s != "" && (s[0L] == '-' || s[0L] == '+'))
            {
                neg = s[0L] == '-';
                s = s[1L..];
            }
            var (q, rem, err) = leadingInt(s);
            x = int(q);
            if (err != null || rem != "")
            {
                return (0L, atoiError);
            }
            if (neg)
            {
                x = -x;
            }
            return (x, null);
        }

        // formatNano appends a fractional second, as nanoseconds, to b
        // and returns the result.
        private static slice<byte> formatNano(slice<byte> b, ulong nanosec, long n, bool trim)
        {
            var u = nanosec;
            array<byte> buf = new array<byte>(9L);
            {
                var start = len(buf);

                while (start > 0L)
                {
                    start--;
                    buf[start] = byte(u % 10L + '0');
                    u /= 10L;
                }

            }

            if (n > 9L)
            {
                n = 9L;
            }
            if (trim)
            {
                while (n > 0L && buf[n - 1L] == '0')
                {
                    n--;
                }

                if (n == 0L)
                {
                    return b;
                }
            }
            b = append(b, '.');
            return append(b, buf[..n]);
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
        public static @string String(this Time t)
        {
            var s = t.Format("2006-01-02 15:04:05.999999999 -0700 MST"); 

            // Format monotonic clock reading as m=±ddd.nnnnnnnnn.
            if (t.wall & hasMonotonic != 0L)
            {
                var m2 = uint64(t.ext);
                var sign = byte('+');
                if (t.ext < 0L)
                {
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
                long wid = 0L;
                if (m0 != 0L)
                {
                    buf = appendInt(buf, int(m0), 0L);
                    wid = 9L;
                }
                buf = appendInt(buf, int(m1), wid);
                buf = append(buf, '.');
                buf = appendInt(buf, int(m2), 9L);
                s += string(buf);
            }
            return s;
        }

        // Format returns a textual representation of the time value formatted
        // according to layout, which defines the format by showing how the reference
        // time, defined to be
        //    Mon Jan 2 15:04:05 -0700 MST 2006
        // would be displayed if it were the value; it serves as an example of the
        // desired output. The same display rules will then be applied to the time
        // value.
        //
        // A fractional second is represented by adding a period and zeros
        // to the end of the seconds section of layout string, as in "15:04:05.000"
        // to format a time stamp with millisecond precision.
        //
        // Predefined layouts ANSIC, UnixDate, RFC3339 and others describe standard
        // and convenient representations of the reference time. For more information
        // about the formats and the definition of the reference time, see the
        // documentation for ANSIC and the other constants defined by this package.
        public static @string Format(this Time t, @string layout)
        {
            const long bufSize = 64L;

            slice<byte> b = default;
            var max = len(layout) + 10L;
            if (max < bufSize)
            {
                array<byte> buf = new array<byte>(bufSize);
                b = buf[..0L];
            }
            else
            {
                b = make_slice<byte>(0L, max);
            }
            b = t.AppendFormat(b, layout);
            return string(b);
        }

        // AppendFormat is like Format but appends the textual
        // representation to b and returns the extended buffer.
        public static slice<byte> AppendFormat(this Time t, slice<byte> b, @string layout)
        {

            long year = -1L;            Month month = default;            long day = default;            long hour = -1L;            long min = default;            long sec = default; 
            // Each iteration generates one std value.
            while (layout != "")
            {
                var (prefix, std, suffix) = nextStdChunk(layout);
                if (prefix != "")
                {
                    b = append(b, prefix);
                }
                if (std == 0L)
                {
                    break;
                }
                layout = suffix; 

                // Compute year, month, day if needed.
                if (year < 0L && std & stdNeedDate != 0L)
                {
                    year, month, day, _ = absDate(abs, true);
                } 

                // Compute hour, minute, second if needed.
                if (hour < 0L && std & stdNeedClock != 0L)
                {
                    hour, min, sec = absClock(abs);
                }

                if (std & stdMask == stdYear) 
                    var y = year;
                    if (y < 0L)
                    {
                        y = -y;
                    }
                    b = appendInt(b, y % 100L, 2L);
                else if (std & stdMask == stdLongYear) 
                    b = appendInt(b, year, 4L);
                else if (std & stdMask == stdMonth) 
                    b = append(b, month.String()[..3L]);
                else if (std & stdMask == stdLongMonth) 
                    var m = month.String();
                    b = append(b, m);
                else if (std & stdMask == stdNumMonth) 
                    b = appendInt(b, int(month), 0L);
                else if (std & stdMask == stdZeroMonth) 
                    b = appendInt(b, int(month), 2L);
                else if (std & stdMask == stdWeekDay) 
                    b = append(b, absWeekday(abs).String()[..3L]);
                else if (std & stdMask == stdLongWeekDay) 
                    var s = absWeekday(abs).String();
                    b = append(b, s);
                else if (std & stdMask == stdDay) 
                    b = appendInt(b, day, 0L);
                else if (std & stdMask == stdUnderDay) 
                    if (day < 10L)
                    {
                        b = append(b, ' ');
                    }
                    b = appendInt(b, day, 0L);
                else if (std & stdMask == stdZeroDay) 
                    b = appendInt(b, day, 2L);
                else if (std & stdMask == stdHour) 
                    b = appendInt(b, hour, 2L);
                else if (std & stdMask == stdHour12) 
                    // Noon is 12PM, midnight is 12AM.
                    var hr = hour % 12L;
                    if (hr == 0L)
                    {
                        hr = 12L;
                    }
                    b = appendInt(b, hr, 0L);
                else if (std & stdMask == stdZeroHour12) 
                    // Noon is 12PM, midnight is 12AM.
                    hr = hour % 12L;
                    if (hr == 0L)
                    {
                        hr = 12L;
                    }
                    b = appendInt(b, hr, 2L);
                else if (std & stdMask == stdMinute) 
                    b = appendInt(b, min, 0L);
                else if (std & stdMask == stdZeroMinute) 
                    b = appendInt(b, min, 2L);
                else if (std & stdMask == stdSecond) 
                    b = appendInt(b, sec, 0L);
                else if (std & stdMask == stdZeroSecond) 
                    b = appendInt(b, sec, 2L);
                else if (std & stdMask == stdPM) 
                    if (hour >= 12L)
                    {
                        b = append(b, "PM");
                    }
                    else
                    {
                        b = append(b, "AM");
                    }
                else if (std & stdMask == stdpm) 
                    if (hour >= 12L)
                    {
                        b = append(b, "pm");
                    }
                    else
                    {
                        b = append(b, "am");
                    }
                else if (std & stdMask == stdISO8601TZ || std & stdMask == stdISO8601ColonTZ || std & stdMask == stdISO8601SecondsTZ || std & stdMask == stdISO8601ShortTZ || std & stdMask == stdISO8601ColonSecondsTZ || std & stdMask == stdNumTZ || std & stdMask == stdNumColonTZ || std & stdMask == stdNumSecondsTz || std & stdMask == stdNumShortTZ || std & stdMask == stdNumColonSecondsTZ) 
                    // Ugly special case. We cheat and take the "Z" variants
                    // to mean "the time zone as formatted for ISO 8601".
                    if (offset == 0L && (std == stdISO8601TZ || std == stdISO8601ColonTZ || std == stdISO8601SecondsTZ || std == stdISO8601ShortTZ || std == stdISO8601ColonSecondsTZ))
                    {
                        b = append(b, 'Z');
                        break;
                    }
                    var zone = offset / 60L; // convert to minutes
                    var absoffset = offset;
                    if (zone < 0L)
                    {
                        b = append(b, '-');
                        zone = -zone;
                        absoffset = -absoffset;
                    }
                    else
                    {
                        b = append(b, '+');
                    }
                    b = appendInt(b, zone / 60L, 2L);
                    if (std == stdISO8601ColonTZ || std == stdNumColonTZ || std == stdISO8601ColonSecondsTZ || std == stdNumColonSecondsTZ)
                    {
                        b = append(b, ':');
                    }
                    if (std != stdNumShortTZ && std != stdISO8601ShortTZ)
                    {
                        b = appendInt(b, zone % 60L, 2L);
                    } 

                    // append seconds if appropriate
                    if (std == stdISO8601SecondsTZ || std == stdNumSecondsTz || std == stdNumColonSecondsTZ || std == stdISO8601ColonSecondsTZ)
                    {
                        if (std == stdNumColonSecondsTZ || std == stdISO8601ColonSecondsTZ)
                        {
                            b = append(b, ':');
                        }
                        b = appendInt(b, absoffset % 60L, 2L);
                    }
                else if (std & stdMask == stdTZ) 
                    if (name != "")
                    {
                        b = append(b, name);
                        break;
                    } 
                    // No time zone known for this time, but we must print one.
                    // Use the -0700 format.
                    zone = offset / 60L; // convert to minutes
                    if (zone < 0L)
                    {
                        b = append(b, '-');
                        zone = -zone;
                    }
                    else
                    {
                        b = append(b, '+');
                    }
                    b = appendInt(b, zone / 60L, 2L);
                    b = appendInt(b, zone % 60L, 2L);
                else if (std & stdMask == stdFracSecond0 || std & stdMask == stdFracSecond9) 
                    b = formatNano(b, uint(t.Nanosecond()), std >> (int)(stdArgShift), std & stdMask == stdFracSecond9);
                            }

            return b;
        }

        private static var errBad = errors.New("bad value for field"); // placeholder not passed to user

        // ParseError describes a problem parsing a time string.
        public partial struct ParseError
        {
            public @string Layout;
            public @string Value;
            public @string LayoutElem;
            public @string ValueElem;
            public @string Message;
        }

        private static @string quote(@string s)
        {
            return "\"" + s + "\"";
        }

        // Error returns the string representation of a ParseError.
        private static @string Error(this ref ParseError e)
        {
            if (e.Message == "")
            {
                return "parsing time " + quote(e.Value) + " as " + quote(e.Layout) + ": cannot parse " + quote(e.ValueElem) + " as " + quote(e.LayoutElem);
            }
            return "parsing time " + quote(e.Value) + e.Message;
        }

        // isDigit reports whether s[i] is in range and is a decimal digit.
        private static bool isDigit(@string s, long i)
        {
            if (len(s) <= i)
            {
                return false;
            }
            var c = s[i];
            return '0' <= c && c <= '9';
        }

        // getnum parses s[0:1] or s[0:2] (fixed forces the latter)
        // as a decimal integer and returns the integer and the
        // remainder of the string.
        private static (long, @string, error) getnum(@string s, bool @fixed)
        {
            if (!isDigit(s, 0L))
            {
                return (0L, s, errBad);
            }
            if (!isDigit(s, 1L))
            {
                if (fixed)
                {
                    return (0L, s, errBad);
                }
                return (int(s[0L] - '0'), s[1L..], null);
            }
            return (int(s[0L] - '0') * 10L + int(s[1L] - '0'), s[2L..], null);
        }

        private static @string cutspace(@string s)
        {
            while (len(s) > 0L && s[0L] == ' ')
            {
                s = s[1L..];
            }

            return s;
        }

        // skip removes the given prefix from value,
        // treating runs of space characters as equivalent.
        private static (@string, error) skip(@string value, @string prefix)
        {
            while (len(prefix) > 0L)
            {
                if (prefix[0L] == ' ')
                {
                    if (len(value) > 0L && value[0L] != ' ')
                    {
                        return (value, errBad);
                    }
                    prefix = cutspace(prefix);
                    value = cutspace(value);
                    continue;
                }
                if (len(value) == 0L || value[0L] != prefix[0L])
                {
                    return (value, errBad);
                }
                prefix = prefix[1L..];
                value = value[1L..];
            }

            return (value, null);
        }

        // Parse parses a formatted string and returns the time value it represents.
        // The layout defines the format by showing how the reference time,
        // defined to be
        //    Mon Jan 2 15:04:05 -0700 MST 2006
        // would be interpreted if it were the value; it serves as an example of
        // the input format. The same interpretation will then be made to the
        // input string.
        //
        // Predefined layouts ANSIC, UnixDate, RFC3339 and others describe standard
        // and convenient representations of the reference time. For more information
        // about the formats and the definition of the reference time, see the
        // documentation for ANSIC and the other constants defined by this package.
        // Also, the executable example for Time.Format demonstrates the working
        // of the layout string in detail and is a good reference.
        //
        // Elements omitted from the value are assumed to be zero or, when
        // zero is impossible, one, so parsing "3:04pm" returns the time
        // corresponding to Jan 1, year 0, 15:04:00 UTC (note that because the year is
        // 0, this time is before the zero Time).
        // Years must be in the range 0000..9999. The day of the week is checked
        // for syntax but it is otherwise ignored.
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
        public static (Time, error) Parse(@string layout, @string value)
        {
            return parse(layout, value, UTC, Local);
        }

        // ParseInLocation is like Parse but differs in two important ways.
        // First, in the absence of time zone information, Parse interprets a time as UTC;
        // ParseInLocation interprets the time as in the given location.
        // Second, when given a zone offset or abbreviation, Parse tries to match it
        // against the Local location; ParseInLocation uses the given location.
        public static (Time, error) ParseInLocation(@string layout, @string value, ref Location loc)
        {
            return parse(layout, value, loc, loc);
        }

        private static (Time, error) parse(@string layout, @string value, ref Location defaultLocation, ref Location local)
        {
            var alayout = layout;
            var avalue = value;
            @string rangeErrString = ""; // set if a value is out of range
            var amSet = false; // do we need to subtract 12 from the hour for midnight?
            var pmSet = false; // do we need to add 12 to the hour?

            // Time being constructed.
            long year = default;            long month = 1L;            long day = 1L;            long hour = default;            long min = default;            long sec = default;            long nsec = default;            ref Location z = default;            long zoneOffset = -1L;            @string zoneName = default; 

            // Each iteration processes one std value.
            while (true)
            {
                error err = default;
                var (prefix, std, suffix) = nextStdChunk(layout);
                var stdstr = layout[len(prefix)..len(layout) - len(suffix)];
                value, err = skip(value, prefix);
                if (err != null)
                {
                    return (new Time(), ref new ParseError(alayout,avalue,prefix,value,""));
                }
                if (std == 0L)
                {
                    if (len(value) != 0L)
                    {
                        return (new Time(), ref new ParseError(alayout,avalue,"",value,": extra text: "+value));
                    }
                    break;
                }
                layout = suffix;
                @string p = default;

                if (std & stdMask == stdYear) 
                    if (len(value) < 2L)
                    {
                        err = error.As(errBad);
                        break;
                    }
                    p = value[0L..2L];
                    value = value[2L..];
                    year, err = atoi(p);
                    if (year >= 69L)
                    { // Unix time starts Dec 31 1969 in some time zones
                        year += 1900L;
                    }
                    else
                    {
                        year += 2000L;
                    }
                else if (std & stdMask == stdLongYear) 
                    if (len(value) < 4L || !isDigit(value, 0L))
                    {
                        err = error.As(errBad);
                        break;
                    }
                    p = value[0L..4L];
                    value = value[4L..];
                    year, err = atoi(p);
                else if (std & stdMask == stdMonth) 
                    month, value, err = lookup(shortMonthNames, value);
                    month++;
                else if (std & stdMask == stdLongMonth) 
                    month, value, err = lookup(longMonthNames, value);
                    month++;
                else if (std & stdMask == stdNumMonth || std & stdMask == stdZeroMonth) 
                    month, value, err = getnum(value, std == stdZeroMonth);
                    if (month <= 0L || 12L < month)
                    {
                        rangeErrString = "month";
                    }
                else if (std & stdMask == stdWeekDay) 
                    // Ignore weekday except for error checking.
                    _, value, err = lookup(shortDayNames, value);
                else if (std & stdMask == stdLongWeekDay) 
                    _, value, err = lookup(longDayNames, value);
                else if (std & stdMask == stdDay || std & stdMask == stdUnderDay || std & stdMask == stdZeroDay) 
                    if (std == stdUnderDay && len(value) > 0L && value[0L] == ' ')
                    {
                        value = value[1L..];
                    }
                    day, value, err = getnum(value, std == stdZeroDay);
                    if (day < 0L)
                    { 
                        // Note that we allow any one- or two-digit day here.
                        rangeErrString = "day";
                    }
                else if (std & stdMask == stdHour) 
                    hour, value, err = getnum(value, false);
                    if (hour < 0L || 24L <= hour)
                    {
                        rangeErrString = "hour";
                    }
                else if (std & stdMask == stdHour12 || std & stdMask == stdZeroHour12) 
                    hour, value, err = getnum(value, std == stdZeroHour12);
                    if (hour < 0L || 12L < hour)
                    {
                        rangeErrString = "hour";
                    }
                else if (std & stdMask == stdMinute || std & stdMask == stdZeroMinute) 
                    min, value, err = getnum(value, std == stdZeroMinute);
                    if (min < 0L || 60L <= min)
                    {
                        rangeErrString = "minute";
                    }
                else if (std & stdMask == stdSecond || std & stdMask == stdZeroSecond) 
                    sec, value, err = getnum(value, std == stdZeroSecond);
                    if (sec < 0L || 60L <= sec)
                    {
                        rangeErrString = "second";
                        break;
                    } 
                    // Special case: do we have a fractional second but no
                    // fractional second in the format?
                    if (len(value) >= 2L && value[0L] == '.' && isDigit(value, 1L))
                    {
                        _, std, _ = nextStdChunk(layout);
                        std &= stdMask;
                        if (std == stdFracSecond0 || std == stdFracSecond9)
                        { 
                            // Fractional second in the layout; proceed normally
                            break;
                        } 
                        // No fractional second in the layout but we have one in the input.
                        long n = 2L;
                        while (n < len(value) && isDigit(value, n))
                        {
                            n++;
                        }

                        nsec, rangeErrString, err = parseNanoseconds(value, n);
                        value = value[n..];
                    }
                else if (std & stdMask == stdPM) 
                    if (len(value) < 2L)
                    {
                        err = error.As(errBad);
                        break;
                    }
                    p = value[0L..2L];
                    value = value[2L..];
                    switch (p)
                    {
                        case "PM": 
                            pmSet = true;
                            break;
                        case "AM": 
                            amSet = true;
                            break;
                        default: 
                            err = error.As(errBad);
                            break;
                    }
                else if (std & stdMask == stdpm) 
                    if (len(value) < 2L)
                    {
                        err = error.As(errBad);
                        break;
                    }
                    p = value[0L..2L];
                    value = value[2L..];
                    switch (p)
                    {
                        case "pm": 
                            pmSet = true;
                            break;
                        case "am": 
                            amSet = true;
                            break;
                        default: 
                            err = error.As(errBad);
                            break;
                    }
                else if (std & stdMask == stdISO8601TZ || std & stdMask == stdISO8601ColonTZ || std & stdMask == stdISO8601SecondsTZ || std & stdMask == stdISO8601ShortTZ || std & stdMask == stdISO8601ColonSecondsTZ || std & stdMask == stdNumTZ || std & stdMask == stdNumShortTZ || std & stdMask == stdNumColonTZ || std & stdMask == stdNumSecondsTz || std & stdMask == stdNumColonSecondsTZ) 
                    if ((std == stdISO8601TZ || std == stdISO8601ShortTZ || std == stdISO8601ColonTZ) && len(value) >= 1L && value[0L] == 'Z')
                    {
                        value = value[1L..];
                        z = UTC;
                        break;
                    }
                    @string sign = default;                    hour = default;                    min = default;                    @string seconds = default;

                    if (std == stdISO8601ColonTZ || std == stdNumColonTZ)
                    {
                        if (len(value) < 6L)
                        {
                            err = error.As(errBad);
                            break;
                        }
                        if (value[3L] != ':')
                        {
                            err = error.As(errBad);
                            break;
                        }
                        sign = value[0L..1L];
                        hour = value[1L..3L];
                        min = value[4L..6L];
                        seconds = "00";
                        value = value[6L..];
                    }
                    else if (std == stdNumShortTZ || std == stdISO8601ShortTZ)
                    {
                        if (len(value) < 3L)
                        {
                            err = error.As(errBad);
                            break;
                        }
                        sign = value[0L..1L];
                        hour = value[1L..3L];
                        min = "00";
                        seconds = "00";
                        value = value[3L..];
                    }
                    else if (std == stdISO8601ColonSecondsTZ || std == stdNumColonSecondsTZ)
                    {
                        if (len(value) < 9L)
                        {
                            err = error.As(errBad);
                            break;
                        }
                        if (value[3L] != ':' || value[6L] != ':')
                        {
                            err = error.As(errBad);
                            break;
                        }
                        sign = value[0L..1L];
                        hour = value[1L..3L];
                        min = value[4L..6L];
                        seconds = value[7L..9L];
                        value = value[9L..];
                    }
                    else if (std == stdISO8601SecondsTZ || std == stdNumSecondsTz)
                    {
                        if (len(value) < 7L)
                        {
                            err = error.As(errBad);
                            break;
                        }
                        sign = value[0L..1L];
                        hour = value[1L..3L];
                        min = value[3L..5L];
                        seconds = value[5L..7L];
                        value = value[7L..];
                    }
                    else
                    {
                        if (len(value) < 5L)
                        {
                            err = error.As(errBad);
                            break;
                        }
                        sign = value[0L..1L];
                        hour = value[1L..3L];
                        min = value[3L..5L];
                        seconds = "00";
                        value = value[5L..];
                    }
                    long hr = default;                    long mm = default;                    long ss = default;

                    hr, err = atoi(hour);
                    if (err == null)
                    {
                        mm, err = atoi(min);
                    }
                    if (err == null)
                    {
                        ss, err = atoi(seconds);
                    }
                    zoneOffset = (hr * 60L + mm) * 60L + ss; // offset is in seconds
                    switch (sign[0L])
                    {
                        case '+': 
                            break;
                        case '-': 
                            zoneOffset = -zoneOffset;
                            break;
                        default: 
                            err = error.As(errBad);
                            break;
                    }
                else if (std & stdMask == stdTZ) 
                    // Does it look like a time zone?
                    if (len(value) >= 3L && value[0L..3L] == "UTC")
                    {
                        z = UTC;
                        value = value[3L..];
                        break;
                    }
                    var (n, ok) = parseTimeZone(value);
                    if (!ok)
                    {
                        err = error.As(errBad);
                        break;
                    }
                    zoneName = value[..n];
                    value = value[n..];
                else if (std & stdMask == stdFracSecond0) 
                    // stdFracSecond0 requires the exact number of digits as specified in
                    // the layout.
                    long ndigit = 1L + (std >> (int)(stdArgShift));
                    if (len(value) < ndigit)
                    {
                        err = error.As(errBad);
                        break;
                    }
                    nsec, rangeErrString, err = parseNanoseconds(value, ndigit);
                    value = value[ndigit..];
                else if (std & stdMask == stdFracSecond9) 
                    if (len(value) < 2L || value[0L] != '.' || value[1L] < '0' || '9' < value[1L])
                    { 
                        // Fractional second omitted.
                        break;
                    } 
                    // Take any number of digits, even more than asked for,
                    // because it is what the stdSecond case would do.
                    long i = 0L;
                    while (i < 9L && i + 1L < len(value) && '0' <= value[i + 1L] && value[i + 1L] <= '9')
                    {
                        i++;
                    }

                    nsec, rangeErrString, err = parseNanoseconds(value, 1L + i);
                    value = value[1L + i..];
                                if (rangeErrString != "")
                {
                    return (new Time(), ref new ParseError(alayout,avalue,stdstr,value,": "+rangeErrString+" out of range"));
                }
                if (err != null)
                {
                    return (new Time(), ref new ParseError(alayout,avalue,stdstr,value,""));
                }
            }

            if (pmSet && hour < 12L)
            {
                hour += 12L;
            }
            else if (amSet && hour == 12L)
            {
                hour = 0L;
            } 

            // Validate the day of the month.
            if (day < 1L || day > daysIn(Month(month), year))
            {
                return (new Time(), ref new ParseError(alayout,avalue,"",value,": day out of range"));
            }
            if (z != null)
            {
                return (Date(year, Month(month), day, hour, min, sec, nsec, z), null);
            }
            if (zoneOffset != -1L)
            {
                var t = Date(year, Month(month), day, hour, min, sec, nsec, UTC);
                t.addSec(-int64(zoneOffset)); 

                // Look for local zone with the given offset.
                // If that zone was in effect at the given time, use it.
                var (name, offset, _, _, _) = local.lookup(t.unixSec());
                if (offset == zoneOffset && (zoneName == "" || name == zoneName))
                {
                    t.setLoc(local);
                    return (t, null);
                } 

                // Otherwise create fake zone to record offset.
                t.setLoc(FixedZone(zoneName, zoneOffset));
                return (t, null);
            }
            if (zoneName != "")
            {
                t = Date(year, Month(month), day, hour, min, sec, nsec, UTC); 
                // Look for local zone with the given offset.
                // If that zone was in effect at the given time, use it.
                var (offset, ok) = local.lookupName(zoneName, t.unixSec());
                if (ok)
                {
                    t.addSec(-int64(offset));
                    t.setLoc(local);
                    return (t, null);
                } 

                // Otherwise, create fake zone with unknown offset.
                if (len(zoneName) > 3L && zoneName[..3L] == "GMT")
                {
                    offset, _ = atoi(zoneName[3L..]); // Guaranteed OK by parseGMT.
                    offset *= 3600L;
                }
                t.setLoc(FixedZone(zoneName, offset));
                return (t, null);
            } 

            // Otherwise, fall back to default.
            return (Date(year, Month(month), day, hour, min, sec, nsec, defaultLocation), null);
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
        private static (long, bool) parseTimeZone(@string value)
        {
            if (len(value) < 3L)
            {
                return (0L, false);
            } 
            // Special case 1: ChST and MeST are the only zones with a lower-case letter.
            if (len(value) >= 4L && (value[..4L] == "ChST" || value[..4L] == "MeST"))
            {
                return (4L, true);
            } 
            // Special case 2: GMT may have an hour offset; treat it specially.
            if (value[..3L] == "GMT")
            {
                length = parseGMT(value);
                return (length, true);
            } 
            // How many upper-case letters are there? Need at least three, at most five.
            long nUpper = default;
            for (nUpper = 0L; nUpper < 6L; nUpper++)
            {
                if (nUpper >= len(value))
                {
                    break;
                }
                {
                    var c = value[nUpper];

                    if (c < 'A' || 'Z' < c)
                    {
                        break;
                    }

                }
            }

            switch (nUpper)
            {
                case 0L: 

                case 1L: 

                case 2L: 

                case 6L: 
                    return (0L, false);
                    break;
                case 5L: // Must end in T to match.
                    if (value[4L] == 'T')
                    {
                        return (5L, true);
                    }
                    break;
                case 4L: 
                    // Must end in T, except one special case.
                    if (value[3L] == 'T' || value[..4L] == "WITA")
                    {
                        return (4L, true);
                    }
                    break;
                case 3L: 
                    return (3L, true);
                    break;
            }
            return (0L, false);
        }

        // parseGMT parses a GMT time zone. The input string is known to start "GMT".
        // The function checks whether that is followed by a sign and a number in the
        // range -14 through 12 excluding zero.
        private static long parseGMT(@string value)
        {
            value = value[3L..];
            if (len(value) == 0L)
            {
                return 3L;
            }
            var sign = value[0L];
            if (sign != '-' && sign != '+')
            {
                return 3L;
            }
            var (x, rem, err) = leadingInt(value[1L..]);
            if (err != null)
            {
                return 3L;
            }
            if (sign == '-')
            {
                x = -x;
            }
            if (x == 0L || x < -14L || 12L < x)
            {
                return 3L;
            }
            return 3L + len(value) - len(rem);
        }

        private static (long, @string, error) parseNanoseconds(@string value, long nbytes)
        {
            if (value[0L] != '.')
            {
                err = errBad;
                return;
            }
            ns, err = atoi(value[1L..nbytes]);

            if (err != null)
            {
                return;
            }
            if (ns < 0L || 1e9F <= ns)
            {
                rangeErrString = "fractional second";
                return;
            } 
            // We need nanoseconds, which means scaling by the number
            // of missing digits in the format, maximum length 10. If it's
            // longer than 10, we won't scale.
            long scaleDigits = 10L - nbytes;
            for (long i = 0L; i < scaleDigits; i++)
            {
                ns *= 10L;
            }

            return;
        }

        private static var errLeadingInt = errors.New("time: bad [0-9]*"); // never printed

        // leadingInt consumes the leading [0-9]* from s.
        private static (long, @string, error) leadingInt(@string s)
        {
            long i = 0L;
            while (i < len(s))
            {
                var c = s[i];
                if (c < '0' || c > '9')
                {
                    break;
                i++;
                }
                if (x > (1L << (int)(63L) - 1L) / 10L)
                { 
                    // overflow
                    return (0L, "", errLeadingInt);
                }
                x = x * 10L + int64(c) - '0';
                if (x < 0L)
                { 
                    // overflow
                    return (0L, "", errLeadingInt);
                }
            }

            return (x, s[i..], null);
        }

        // leadingFraction consumes the leading [0-9]* from s.
        // It is used only for fractions, so does not return an error on overflow,
        // it just stops accumulating precision.
        private static (long, double, @string) leadingFraction(@string s)
        {
            long i = 0L;
            scale = 1L;
            var overflow = false;
            while (i < len(s))
            {
                var c = s[i];
                if (c < '0' || c > '9')
                {
                    break;
                i++;
                }
                if (overflow)
                {
                    continue;
                }
                if (x > (1L << (int)(63L) - 1L) / 10L)
                { 
                    // It's possible for overflow to give a positive number, so take care.
                    overflow = true;
                    continue;
                }
                var y = x * 10L + int64(c) - '0';
                if (y < 0L)
                {
                    overflow = true;
                    continue;
                }
                x = y;
                scale *= 10L;
            }

            return (x, scale, s[i..]);
        }

        private static map unitMap = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, long>{"ns":int64(Nanosecond),"us":int64(Microsecond),"µs":int64(Microsecond),"μs":int64(Microsecond),"ms":int64(Millisecond),"s":int64(Second),"m":int64(Minute),"h":int64(Hour),};

        // ParseDuration parses a duration string.
        // A duration string is a possibly signed sequence of
        // decimal numbers, each with optional fraction and a unit suffix,
        // such as "300ms", "-1.5h" or "2h45m".
        // Valid time units are "ns", "us" (or "µs"), "ms", "s", "m", "h".
        public static (Duration, error) ParseDuration(@string s)
        { 
            // [-+]?([0-9]*(\.[0-9]*)?[a-z]+)+
            var orig = s;
            long d = default;
            var neg = false; 

            // Consume [-+]?
            if (s != "")
            {
                var c = s[0L];
                if (c == '-' || c == '+')
                {
                    neg = c == '-';
                    s = s[1L..];
                }
            } 
            // Special case: if all that is left is "0", this is zero.
            if (s == "0")
            {
                return (0L, null);
            }
            if (s == "")
            {
                return (0L, errors.New("time: invalid duration " + orig));
            }
            while (s != "")
            {
                long v = default;                long f = default; // integers before, after decimal point
                double scale = 1L;

                error err = default; 

                // The next character must be [0-9.]
                if (!(s[0L] == '.' || '0' <= s[0L] && s[0L] <= '9'))
                {
                    return (0L, errors.New("time: invalid duration " + orig));
                } 
                // Consume [0-9]*
                var pl = len(s);
                v, s, err = leadingInt(s);
                if (err != null)
                {
                    return (0L, errors.New("time: invalid duration " + orig));
                }
                var pre = pl != len(s); // whether we consumed anything before a period

                // Consume (\.[0-9]*)?
                var post = false;
                if (s != "" && s[0L] == '.')
                {
                    s = s[1L..];
                    pl = len(s);
                    f, scale, s = leadingFraction(s);
                    post = pl != len(s);
                }
                if (!pre && !post)
                { 
                    // no digits (e.g. ".s" or "-.s")
                    return (0L, errors.New("time: invalid duration " + orig));
                } 

                // Consume unit.
                long i = 0L;
                while (i < len(s))
                {
                    c = s[i];
                    if (c == '.' || '0' <= c && c <= '9')
                    {
                        break;
                    i++;
                    }
                }

                if (i == 0L)
                {
                    return (0L, errors.New("time: missing unit in duration " + orig));
                }
                var u = s[..i];
                s = s[i..];
                var (unit, ok) = unitMap[u];
                if (!ok)
                {
                    return (0L, errors.New("time: unknown unit " + u + " in duration " + orig));
                }
                if (v > (1L << (int)(63L) - 1L) / unit)
                { 
                    // overflow
                    return (0L, errors.New("time: invalid duration " + orig));
                }
                v *= unit;
                if (f > 0L)
                { 
                    // float64 is needed to be nanosecond accurate for fractions of hours.
                    // v >= 0 && (f*unit/scale) <= 3.6e+12 (ns/h, h is the largest unit)
                    v += int64(float64(f) * (float64(unit) / scale));
                    if (v < 0L)
                    { 
                        // overflow
                        return (0L, errors.New("time: invalid duration " + orig));
                    }
                }
                d += v;
                if (d < 0L)
                { 
                    // overflow
                    return (0L, errors.New("time: invalid duration " + orig));
                }
            }


            if (neg)
            {
                d = -d;
            }
            return (Duration(d), null);
        }
    }
}
