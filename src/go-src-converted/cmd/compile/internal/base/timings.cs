// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package @base -- go2cs converted at 2022 March 06 23:14:31 UTC
// import "cmd/compile/internal/base" ==> using @base = go.cmd.compile.@internal.@base_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\base\timings.go
using fmt = go.fmt_package;
using io = go.io_package;
using strings = go.strings_package;
using time = go.time_package;
using System;


namespace go.cmd.compile.@internal;

public static partial class @base_package {

public static Timings Timer = default;

// Timings collects the execution times of labeled phases
// which are added trough a sequence of Start/Stop calls.
// Events may be associated with each phase via AddEvent.
public partial struct Timings {
    public slice<timestamp> list;
    public map<nint, slice<ptr<event>>> events; // lazily allocated
}

private partial struct timestamp {
    public time.Time time;
    public @string label;
    public bool start;
}

private partial struct @event {
    public long size; // count or amount of data processed (allocations, data size, lines, funcs, ...)
    public @string unit; // unit of size measure (count, MB, lines, funcs, ...)
}

private static void append(this ptr<Timings> _addr_t, slice<@string> labels, bool start) {
    ref Timings t = ref _addr_t.val;

    t.list = append(t.list, new timestamp(time.Now(),strings.Join(labels,":"),start));
}

// Start marks the beginning of a new phase and implicitly stops the previous phase.
// The phase name is the colon-separated concatenation of the labels.
private static void Start(this ptr<Timings> _addr_t, params @string[] labels) {
    labels = labels.Clone();
    ref Timings t = ref _addr_t.val;

    t.append(labels, true);
}

// Stop marks the end of a phase and implicitly starts a new phase.
// The labels are added to the labels of the ended phase.
private static void Stop(this ptr<Timings> _addr_t, params @string[] labels) {
    labels = labels.Clone();
    ref Timings t = ref _addr_t.val;

    t.append(labels, false);
}

// AddEvent associates an event, i.e., a count, or an amount of data,
// with the most recently started or stopped phase; or the very first
// phase if Start or Stop hasn't been called yet. The unit specifies
// the unit of measurement (e.g., MB, lines, no. of funcs, etc.).
private static void AddEvent(this ptr<Timings> _addr_t, long size, @string unit) {
    ref Timings t = ref _addr_t.val;

    var m = t.events;
    if (m == null) {
        m = make_map<nint, slice<ptr<event>>>();
        t.events = m;
    }
    var i = len(t.list);
    if (i > 0) {
        i--;
    }
    m[i] = append(m[i], addr(new event(size,unit)));

}

// Write prints the phase times to w.
// The prefix is printed at the start of each line.
private static void Write(this ptr<Timings> _addr_t, io.Writer w, @string prefix) {
    ref Timings t = ref _addr_t.val;

    if (len(t.list) > 0) {
        lines lines = default; 

        // group of phases with shared non-empty label prefix
        var group = default; 

        // accumulated time between Stop/Start timestamps
        time.Duration unaccounted = default; 

        // process Start/Stop timestamps
        var pt = _addr_t.list[0]; // previous timestamp
        var tot = t.list[len(t.list) - 1].time.Sub(pt.time);
        for (nint i = 1; i < len(t.list); i++) {
            var qt = _addr_t.list[i]; // current timestamp
            var dt = qt.time.Sub(pt.time);

            @string label = default;
            slice<ptr<event>> events = default;
            if (pt.start) { 
                // previous phase started
                label = pt.label;
                events = t.events[i - 1];
                if (qt.start) { 
                    // start implicitly ended previous phase; nothing to do
                }
                else
 { 
                    // stop ended previous phase; append stop labels, if any
                    if (qt.label != "") {
                        label += ":" + qt.label;
                    } 
                    // events associated with stop replace prior events
                    {
                        var e = t.events[i];

                        if (e != null) {
                            events = e;
                        }

                    }

                }

            }
            else
 { 
                // previous phase stopped
                if (qt.start) { 
                    // between a stopped and started phase; unaccounted time
                    unaccounted += dt;

                }
                else
 { 
                    // previous stop implicitly started current phase
                    label = qt.label;
                    events = t.events[i];

                }

            }

            if (label != "") { 
                // add phase to existing group, or start a new group
                var l = commonPrefix(group.label, label);
                if (group.size == 1 && l != "" || group.size > 1 && l == group.label) { 
                    // add to existing group
                    group.label = l;
                    group.tot += dt;
                    group.size++;

                }
                else
 { 
                    // start a new group
                    if (group.size > 1) {
                        lines.add(prefix + group.label + "subtotal", 1, group.tot, tot, null);
                    }

                    group.label = label;
                    group.tot = dt;
                    group.size = 1;

                } 

                // write phase
                lines.add(prefix + label, 1, dt, tot, events);

            }

            pt = qt;

        }

        if (group.size > 1) {
            lines.add(prefix + group.label + "subtotal", 1, group.tot, tot, null);
        }
        if (unaccounted != 0) {
            lines.add(prefix + "unaccounted", 1, unaccounted, tot, null);
        }
        lines.add(prefix + "total", 1, tot, tot, null);

        lines.write(w);

    }
}

private static @string commonPrefix(@string a, @string b) {
    nint i = 0;
    while (i < len(a) && i < len(b) && a[i] == b[i]) {
        i++;
    }
    return a[..(int)i];
}

private partial struct lines { // : slice<slice<@string>>
}

private static void add(this ptr<lines> _addr_lines, @string label, nint n, time.Duration dt, time.Duration tot, slice<ptr<event>> events) {
    ref lines lines = ref _addr_lines.val;

    slice<@string> line = default;
    Action<@string, object[]> add = (format, args) => {
        line = append(line, fmt.Sprintf(format, args));
    };

    add("%s", label);
    add("    %d", n);
    add("    %d ns/op", dt);
    add("    %.2f %%", float64(dt) / float64(tot) * 100);

    foreach (var (_, e) in events) {
        add("    %d", e.size);
        add(" %s", e.unit);
        add("    %d", int64(float64(e.size) / dt.Seconds() + 0.5F));
        add(" %s/s", e.unit);
    }    lines.val = append(lines.val, line);
}

private static void write(this lines lines, io.Writer w) { 
    // determine column widths and contents
    slice<nint> widths = default;
    slice<bool> number = default;
    {
        var line__prev1 = line;

        foreach (var (_, __line) in lines) {
            line = __line;
            {
                var i__prev2 = i;
                var col__prev2 = col;

                foreach (var (__i, __col) in line) {
                    i = __i;
                    col = __col;
                    if (i < len(widths)) {
                        if (len(col) > widths[i]) {
                            widths[i] = len(col);
                        }
                    }
                    else
 {
                        widths = append(widths, len(col));
                        number = append(number, isnumber(col)); // first line determines column contents
                    }

                }

                i = i__prev2;
                col = col__prev2;
            }
        }
        line = line__prev1;
    }

    const nint align = 1; // set to a value > 1 to enable
 // set to a value > 1 to enable
    if (align > 1) {
        {
            var i__prev1 = i;

            foreach (var (__i, __w) in widths) {
                i = __i;
                w = __w;
                w += align - 1;
                widths[i] = w - w % align;
            }

            i = i__prev1;
        }
    }
    {
        var line__prev1 = line;

        foreach (var (_, __line) in lines) {
            line = __line;
            {
                var i__prev2 = i;
                var col__prev2 = col;

                foreach (var (__i, __col) in line) {
                    i = __i;
                    col = __col;
                    @string format = "%-*s";
                    if (number[i]) {
                        format = "%*s"; // numbers are right-aligned
                    }

                    fmt.Fprintf(w, format, widths[i], col);

                }

                i = i__prev2;
                col = col__prev2;
            }

            fmt.Fprintln(w);

        }
        line = line__prev1;
    }
}

private static bool isnumber(@string s) {
    foreach (var (_, ch) in s) {
        if (ch <= ' ') {
            continue; // ignore leading whitespace
        }
        return '0' <= ch && ch <= '9' || ch == '.' || ch == '-' || ch == '+';

    }    return false;

}

} // end @base_package
