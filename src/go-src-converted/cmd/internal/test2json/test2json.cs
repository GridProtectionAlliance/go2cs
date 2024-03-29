// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package test2json implements conversion of test binary output to JSON.
// It is used by cmd/test2json and cmd/go.
//
// See the cmd/test2json documentation for details of the JSON encoding.

// package test2json -- go2cs converted at 2022 March 13 06:31:16 UTC
// import "cmd/internal/test2json" ==> using test2json = go.cmd.@internal.test2json_package
// Original source: C:\Program Files\Go\src\cmd\internal\test2json\test2json.go
namespace go.cmd.@internal;

using bytes = bytes_package;
using json = encoding.json_package;
using fmt = fmt_package;
using io = io_package;
using strconv = strconv_package;
using strings = strings_package;
using time = time_package;
using unicode = unicode_package;
using utf8 = unicode.utf8_package;


// Mode controls details of the conversion.

using System.ComponentModel;
using System;
public static partial class test2json_package {

public partial struct Mode { // : nint
}

public static readonly Mode Timestamp = 1 << (int)(iota); // include Time in events

// event is the JSON struct we emit.
private partial struct @event {
    [Description("json:\",omitempty\"")]
    public ptr<time.Time> Time;
    public @string Action;
    [Description("json:\",omitempty\"")]
    public @string Package;
    [Description("json:\",omitempty\"")]
    public @string Test;
    [Description("json:\",omitempty\"")]
    public ptr<double> Elapsed;
    [Description("json:\",omitempty\"")]
    public ptr<textBytes> Output;
}

// textBytes is a hack to get JSON to emit a []byte as a string
// without actually copying it to a string.
// It implements encoding.TextMarshaler, which returns its text form as a []byte,
// and then json encodes that text form as a string (which was our goal).
private partial struct textBytes { // : slice<byte>
}

private static (slice<byte>, error) MarshalText(this textBytes b) {
    slice<byte> _p0 = default;
    error _p0 = default!;

    return (b, error.As(null!)!);
}

// A Converter holds the state of a test-to-JSON conversion.
// It implements io.WriteCloser; the caller writes test output in,
// and the converter writes JSON output to w.
public partial struct Converter {
    public io.Writer w; // JSON output stream
    public @string pkg; // package to name in events
    public Mode mode; // mode bits
    public time.Time start; // time converter started
    public @string testName; // name of current test, for output attribution
    public slice<ptr<event>> report; // pending test result reports (nested for subtests)
    public @string result; // overall test result if seen
    public lineBuffer input; // input buffer
    public lineBuffer output; // output buffer
}

// inBuffer and outBuffer are the input and output buffer sizes.
// They're variables so that they can be reduced during testing.
//
// The input buffer needs to be able to hold any single test
// directive line we want to recognize, like:
//
//     <many spaces> --- PASS: very/nested/s/u/b/t/e/s/t
//
// If anyone reports a test directive line > 4k not working, it will
// be defensible to suggest they restructure their test or test names.
//
// The output buffer must be >= utf8.UTFMax, so that it can
// accumulate any single UTF8 sequence. Lines that fit entirely
// within the output buffer are emitted in single output events.
// Otherwise they are split into multiple events.
// The output buffer size therefore limits the size of the encoding
// of a single JSON output event. 1k seems like a reasonable balance
// between wanting to avoid splitting an output line and not wanting to
// generate enormous output events.
private static nint inBuffer = 4096;private static nint outBuffer = 1024;

// NewConverter returns a "test to json" converter.
// Writes on the returned writer are written as JSON to w,
// with minimal delay.
//
// The writes to w are whole JSON events ending in \n,
// so that it is safe to run multiple tests writing to multiple converters
// writing to a single underlying output stream w.
// As long as the underlying output w can handle concurrent writes
// from multiple goroutines, the result will be a JSON stream
// describing the relative ordering of execution in all the concurrent tests.
//
// The mode flag adjusts the behavior of the converter.
// Passing ModeTime includes event timestamps and elapsed times.
//
// The pkg string, if present, specifies the import path to
// report in the JSON stream.
public static ptr<Converter> NewConverter(io.Writer w, @string pkg, Mode mode) {
    ptr<Converter> c = @new<Converter>();
    c.val = new Converter(w:w,pkg:pkg,mode:mode,start:time.Now(),input:lineBuffer{b:make([]byte,0,inBuffer),line:c.handleInputLine,part:c.output.write,},output:lineBuffer{b:make([]byte,0,outBuffer),line:c.writeOutputEvent,part:c.writeOutputEvent,},);
    return _addr_c!;
}

// Write writes the test input to the converter.
private static (nint, error) Write(this ptr<Converter> _addr_c, slice<byte> b) {
    nint _p0 = default;
    error _p0 = default!;
    ref Converter c = ref _addr_c.val;

    c.input.write(b);
    return (len(b), error.As(null!)!);
}

// Exited marks the test process as having exited with the given error.
private static void Exited(this ptr<Converter> _addr_c, error err) {
    ref Converter c = ref _addr_c.val;

    if (err == null) {
        c.result = "pass";
    }
    else
 {
        c.result = "fail";
    }
}

 
// printed by test on successful run.
private static slice<byte> bigPass = (slice<byte>)"PASS\n";private static slice<byte> bigFail = (slice<byte>)"FAIL\n";private static slice<byte> bigFailErrorPrefix = (slice<byte>)"FAIL\t";private static slice<byte> updates = new slice<slice<byte>>(new slice<byte>[] { []byte("=== RUN   "), []byte("=== PAUSE "), []byte("=== CONT  ") });private static slice<byte> reports = new slice<slice<byte>>(new slice<byte>[] { []byte("--- PASS: "), []byte("--- FAIL: "), []byte("--- SKIP: "), []byte("--- BENCH: ") });private static slice<byte> fourSpace = (slice<byte>)"    ";private static slice<byte> skipLinePrefix = (slice<byte>)"?   \t";private static slice<byte> skipLineSuffix = (slice<byte>)"\t[no test files]\n";

// handleInputLine handles a single whole test output line.
// It must write the line to c.output but may choose to do so
// before or after emitting other events.
private static void handleInputLine(this ptr<Converter> _addr_c, slice<byte> line) {
    ref Converter c = ref _addr_c.val;
 
    // Final PASS or FAIL.
    if (bytes.Equal(line, bigPass) || bytes.Equal(line, bigFail) || bytes.HasPrefix(line, bigFailErrorPrefix)) {
        c.flushReport(0);
        c.output.write(line);
        if (bytes.Equal(line, bigPass)) {
            c.result = "pass";
        }
        else
 {
            c.result = "fail";
        }
        return ;
    }
    if (bytes.HasPrefix(line, skipLinePrefix) && bytes.HasSuffix(line, skipLineSuffix) && len(c.report) == 0) {
        c.result = "skip";
    }
    var actionColon = false;
    var origLine = line;
    var ok = false;
    nint indent = 0;
    {
        var magic__prev1 = magic;

        foreach (var (_, __magic) in updates) {
            magic = __magic;
            if (bytes.HasPrefix(line, magic)) {
                ok = true;
                break;
            }
        }
        magic = magic__prev1;
    }

    if (!ok) { 
        // "--- PASS: "
        // "--- FAIL: "
        // "--- SKIP: "
        // "--- BENCH: "
        // but possibly indented.
        while (bytes.HasPrefix(line, fourSpace)) {
            line = line[(int)4..];
            indent++;
        }
        {
            var magic__prev1 = magic;

            foreach (var (_, __magic) in reports) {
                magic = __magic;
                if (bytes.HasPrefix(line, magic)) {
                    actionColon = true;
                    ok = true;
                    break;
                }
            }

            magic = magic__prev1;
        }
    }
    if (!ok) { 
        // Lookup the name of the test which produced the output using the
        // indentation of the output as an index into the stack of the current
        // subtests.
        // If the indentation is greater than the number of current subtests
        // then the output must have included extra indentation. We can't
        // determine which subtest produced this output, so we default to the
        // old behaviour of assuming the most recently run subtest produced it.
        if (indent > 0 && indent <= len(c.report)) {
            c.testName = c.report[indent - 1].Test;
        }
        c.output.write(origLine);
        return ;
    }
    nint i = 0;
    if (actionColon) {
        i = bytes.IndexByte(line, ':') + 1;
    }
    if (i == 0) {
        i = len(updates[0]);
    }
    var action = strings.ToLower(strings.TrimSuffix(strings.TrimSpace(string(line[(int)4..(int)i])), ":"));
    var name = strings.TrimSpace(string(line[(int)i..]));

    ptr<event> e = addr(new event(Action:action));
    if (line[0] == '-') { // PASS or FAIL report
        // Parse out elapsed time.
        {
            nint i__prev2 = i;

            i = strings.Index(name, " (");

            if (i >= 0) {
                if (strings.HasSuffix(name, "s)")) {
                    var (t, err) = strconv.ParseFloat(name[(int)i + 2..(int)len(name) - 2], 64);
                    if (err == null) {
                        if (c.mode & Timestamp != 0) {
                            e.Elapsed = _addr_t;
                        }
                    }
                }
                name = name[..(int)i];
            }

            i = i__prev2;

        }
        if (len(c.report) < indent) { 
            // Nested deeper than expected.
            // Treat this line as plain output.
            c.output.write(origLine);
            return ;
        }
        c.flushReport(indent);
        e.Test = name;
        c.testName = name;
        c.report = append(c.report, e);
        c.output.write(origLine);
        return ;
    }
    c.flushReport(0);
    c.testName = name;

    if (action == "pause") { 
        // For a pause, we want to write the pause notification before
        // delivering the pause event, just so it doesn't look like the test
        // is generating output immediately after being paused.
        c.output.write(origLine);
    }
    c.writeEvent(e);
    if (action != "pause") {
        c.output.write(origLine);
    }
    return ;
}

// flushReport flushes all pending PASS/FAIL reports at levels >= depth.
private static void flushReport(this ptr<Converter> _addr_c, nint depth) {
    ref Converter c = ref _addr_c.val;

    c.testName = "";
    while (len(c.report) > depth) {
        var e = c.report[len(c.report) - 1];
        c.report = c.report[..(int)len(c.report) - 1];
        c.writeEvent(e);
    }
}

// Close marks the end of the go test output.
// It flushes any pending input and then output (only partial lines at this point)
// and then emits the final overall package-level pass/fail event.
private static error Close(this ptr<Converter> _addr_c) {
    ref Converter c = ref _addr_c.val;

    c.input.flush();
    c.output.flush();
    if (c.result != "") {
        ptr<event> e = addr(new event(Action:c.result));
        if (c.mode & Timestamp != 0) {
            ref var dt = ref heap(time.Since(c.start).Round(1 * time.Millisecond).Seconds(), out ptr<var> _addr_dt);
            _addr_e.Elapsed = _addr_dt;
            e.Elapsed = ref _addr_e.Elapsed.val;
        }
        c.writeEvent(e);
    }
    return error.As(null!)!;
}

// writeOutputEvent writes a single output event with the given bytes.
private static void writeOutputEvent(this ptr<Converter> _addr_c, slice<byte> @out) {
    ref Converter c = ref _addr_c.val;

    c.writeEvent(addr(new event(Action:"output",Output:(*textBytes)(&out),)));
}

// writeEvent writes a single event.
// It adds the package, time (if requested), and test name (if needed).
private static void writeEvent(this ptr<Converter> _addr_c, ptr<event> _addr_e) {
    ref Converter c = ref _addr_c.val;
    ref event e = ref _addr_e.val;

    e.Package = c.pkg;
    if (c.mode & Timestamp != 0) {
        ref var t = ref heap(time.Now(), out ptr<var> _addr_t);
        _addr_e.Time = _addr_t;
        e.Time = ref _addr_e.Time.val;
    }
    if (e.Test == "") {
        e.Test = c.testName;
    }
    var (js, err) = json.Marshal(e);
    if (err != null) { 
        // Should not happen - event is valid for json.Marshal.
        c.w.Write((slice<byte>)fmt.Sprintf("testjson internal error: %v\n", err));
        return ;
    }
    js = append(js, '\n');
    c.w.Write(js);
}

// A lineBuffer is an I/O buffer that reacts to writes by invoking
// input-processing callbacks on whole lines or (for long lines that
// have been split) line fragments.
//
// It should be initialized with b set to a buffer of length 0 but non-zero capacity,
// and line and part set to the desired input processors.
// The lineBuffer will call line(x) for any whole line x (including the final newline)
// that fits entirely in cap(b). It will handle input lines longer than cap(b) by
// calling part(x) for sections of the line. The line will be split at UTF8 boundaries,
// and the final call to part for a long line includes the final newline.
private partial struct lineBuffer {
    public slice<byte> b; // buffer
    public bool mid; // whether we're in the middle of a long line
    public Action<slice<byte>> line; // line callback
    public Action<slice<byte>> part; // partial line callback
}

// write writes b to the buffer.
private static void write(this ptr<lineBuffer> _addr_l, slice<byte> b) {
    ref lineBuffer l = ref _addr_l.val;

    while (len(b) > 0) { 
        // Copy what we can into b.
        var m = copy(l.b[(int)len(l.b)..(int)cap(l.b)], b);
        l.b = l.b[..(int)len(l.b) + m];
        b = b[(int)m..]; 

        // Process lines in b.
        nint i = 0;
        while (i < len(l.b)) {
            var j = bytes.IndexByte(l.b[(int)i..], '\n');
            if (j < 0) {
                if (!l.mid) {
                    {
                        var j__prev3 = j;

                        j = bytes.IndexByte(l.b[(int)i..], '\t');

                        if (j >= 0) {
                            if (isBenchmarkName(bytes.TrimRight(l.b[(int)i..(int)i + j], " "))) {
                                l.part(l.b[(int)i..(int)i + j + 1]);
                                l.mid = true;
                                i += j + 1;
                            }
                        }

                        j = j__prev3;

                    }
                }
                break;
            }
            var e = i + j + 1;
            if (l.mid) { 
                // Found the end of a partial line.
                l.part(l.b[(int)i..(int)e]);
                l.mid = false;
            }
            else
 { 
                // Found a whole line.
                l.line(l.b[(int)i..(int)e]);
            }
            i = e;
        } 

        // Whatever's left in l.b is a line fragment.
        if (i == 0 && len(l.b) == cap(l.b)) { 
            // The whole buffer is a fragment.
            // Emit it as the beginning (or continuation) of a partial line.
            var t = trimUTF8(l.b);
            l.part(l.b[..(int)t]);
            l.b = l.b[..(int)copy(l.b, l.b[(int)t..])];
            l.mid = true;
        }
        if (i > 0) {
            l.b = l.b[..(int)copy(l.b, l.b[(int)i..])];
        }
    }
}

// flush flushes the line buffer.
private static void flush(this ptr<lineBuffer> _addr_l) {
    ref lineBuffer l = ref _addr_l.val;

    if (len(l.b) > 0) { 
        // Must be a line without a \n, so a partial line.
        l.part(l.b);
        l.b = l.b[..(int)0];
    }
}

private static slice<byte> benchmark = (slice<byte>)"Benchmark";

// isBenchmarkName reports whether b is a valid benchmark name
// that might appear as the first field in a benchmark result line.
private static bool isBenchmarkName(slice<byte> b) {
    if (!bytes.HasPrefix(b, benchmark)) {
        return false;
    }
    if (len(b) == len(benchmark)) { // just "Benchmark"
        return true;
    }
    var (r, _) = utf8.DecodeRune(b[(int)len(benchmark)..]);
    return !unicode.IsLower(r);
}

// trimUTF8 returns a length t as close to len(b) as possible such that b[:t]
// does not end in the middle of a possibly-valid UTF-8 sequence.
//
// If a large text buffer must be split before position i at the latest,
// splitting at position trimUTF(b[:i]) avoids splitting a UTF-8 sequence.
private static nint trimUTF8(slice<byte> b) { 
    // Scan backward to find non-continuation byte.
    for (nint i = 1; i < utf8.UTFMax && i <= len(b); i++) {
        {
            var c = b[len(b) - i];

            if (c & 0xc0 != 0x80) {

                if (c & 0xe0 == 0xc0) 
                    if (i < 2) {
                        return len(b) - i;
                    }
                else if (c & 0xf0 == 0xe0) 
                    if (i < 3) {
                        return len(b) - i;
                    }
                else if (c & 0xf8 == 0xf0) 
                    if (i < 4) {
                        return len(b) - i;
                    }
                                break;
            }

        }
    }
    return len(b);
}

} // end test2json_package
