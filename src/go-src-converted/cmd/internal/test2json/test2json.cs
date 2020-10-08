// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package test2json implements conversion of test binary output to JSON.
// It is used by cmd/test2json and cmd/go.
//
// See the cmd/test2json documentation for details of the JSON encoding.
// package test2json -- go2cs converted at 2020 October 08 04:35:14 UTC
// import "cmd/internal/test2json" ==> using test2json = go.cmd.@internal.test2json_package
// Original source: C:\Go\src\cmd\internal\test2json\test2json.go
using bytes = go.bytes_package;
using json = go.encoding.json_package;
using fmt = go.fmt_package;
using io = go.io_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using time = go.time_package;
using unicode = go.unicode_package;
using utf8 = go.unicode.utf8_package;
using static go.builtin;
using System.ComponentModel;
using System;

namespace go {
namespace cmd {
namespace @internal
{
    public static partial class test2json_package
    {
        // Mode controls details of the conversion.
        public partial struct Mode // : long
        {
        }

        public static readonly Mode Timestamp = (Mode)1L << (int)(iota); // include Time in events

        // event is the JSON struct we emit.
        private partial struct @event
        {
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
        private partial struct textBytes // : slice<byte>
        {
        }

        private static (slice<byte>, error) MarshalText(this textBytes b)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;

            return (b, error.As(null!)!);
        }

        // A Converter holds the state of a test-to-JSON conversion.
        // It implements io.WriteCloser; the caller writes test output in,
        // and the converter writes JSON output to w.
        public partial struct Converter
        {
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
        private static long inBuffer = 4096L;        private static long outBuffer = 1024L;

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
        public static ptr<Converter> NewConverter(io.Writer w, @string pkg, Mode mode)
        {
            ptr<Converter> c = @new<Converter>();
            c.val = new Converter(w:w,pkg:pkg,mode:mode,start:time.Now(),input:lineBuffer{b:make([]byte,0,inBuffer),line:c.handleInputLine,part:c.output.write,},output:lineBuffer{b:make([]byte,0,outBuffer),line:c.writeOutputEvent,part:c.writeOutputEvent,},);
            return _addr_c!;
        }

        // Write writes the test input to the converter.
        private static (long, error) Write(this ptr<Converter> _addr_c, slice<byte> b)
        {
            long _p0 = default;
            error _p0 = default!;
            ref Converter c = ref _addr_c.val;

            c.input.write(b);
            return (len(b), error.As(null!)!);
        }

        // Exited marks the test process as having exited with the given error.
        private static void Exited(this ptr<Converter> _addr_c, error err)
        {
            ref Converter c = ref _addr_c.val;

            if (err == null)
            {
                c.result = "pass";
            }
            else
            {
                c.result = "fail";
            }

        }

 
        // printed by test on successful run.
        private static slice<byte> bigPass = (slice<byte>)"PASS\n";        private static slice<byte> bigFail = (slice<byte>)"FAIL\n";        private static slice<byte> bigFailErrorPrefix = (slice<byte>)"FAIL\t";        private static slice<byte> updates = new slice<slice<byte>>(new slice<byte>[] { []byte("=== RUN   "), []byte("=== PAUSE "), []byte("=== CONT  ") });        private static slice<byte> reports = new slice<slice<byte>>(new slice<byte>[] { []byte("--- PASS: "), []byte("--- FAIL: "), []byte("--- SKIP: "), []byte("--- BENCH: ") });        private static slice<byte> fourSpace = (slice<byte>)"    ";        private static slice<byte> skipLinePrefix = (slice<byte>)"?   \t";        private static slice<byte> skipLineSuffix = (slice<byte>)"\t[no test files]\n";

        // handleInputLine handles a single whole test output line.
        // It must write the line to c.output but may choose to do so
        // before or after emitting other events.
        private static void handleInputLine(this ptr<Converter> _addr_c, slice<byte> line)
        {
            ref Converter c = ref _addr_c.val;
 
            // Final PASS or FAIL.
            if (bytes.Equal(line, bigPass) || bytes.Equal(line, bigFail) || bytes.HasPrefix(line, bigFailErrorPrefix))
            {
                c.flushReport(0L);
                c.output.write(line);
                if (bytes.Equal(line, bigPass))
                {
                    c.result = "pass";
                }
                else
                {
                    c.result = "fail";
                }

                return ;

            } 

            // Special case for entirely skipped test binary: "?   \tpkgname\t[no test files]\n" is only line.
            // Report it as plain output but remember to say skip in the final summary.
            if (bytes.HasPrefix(line, skipLinePrefix) && bytes.HasSuffix(line, skipLineSuffix) && len(c.report) == 0L)
            {
                c.result = "skip";
            } 

            // "=== RUN   "
            // "=== PAUSE "
            // "=== CONT  "
            var actionColon = false;
            var origLine = line;
            var ok = false;
            long indent = 0L;
            {
                var magic__prev1 = magic;

                foreach (var (_, __magic) in updates)
                {
                    magic = __magic;
                    if (bytes.HasPrefix(line, magic))
                    {
                        ok = true;
                        break;
                    }

                }

                magic = magic__prev1;
            }

            if (!ok)
            { 
                // "--- PASS: "
                // "--- FAIL: "
                // "--- SKIP: "
                // "--- BENCH: "
                // but possibly indented.
                while (bytes.HasPrefix(line, fourSpace))
                {
                    line = line[4L..];
                    indent++;
                }

                {
                    var magic__prev1 = magic;

                    foreach (var (_, __magic) in reports)
                    {
                        magic = __magic;
                        if (bytes.HasPrefix(line, magic))
                        {
                            actionColon = true;
                            ok = true;
                            break;
                        }

                    }

                    magic = magic__prev1;
                }
            } 

            // Not a special test output line.
            if (!ok)
            { 
                // Lookup the name of the test which produced the output using the
                // indentation of the output as an index into the stack of the current
                // subtests.
                // If the indentation is greater than the number of current subtests
                // then the output must have included extra indentation. We can't
                // determine which subtest produced this output, so we default to the
                // old behaviour of assuming the most recently run subtest produced it.
                if (indent > 0L && indent <= len(c.report))
                {
                    c.testName = c.report[indent - 1L].Test;
                }

                c.output.write(origLine);
                return ;

            } 

            // Parse out action and test name.
            long i = 0L;
            if (actionColon)
            {
                i = bytes.IndexByte(line, ':') + 1L;
            }

            if (i == 0L)
            {
                i = len(updates[0L]);
            }

            var action = strings.ToLower(strings.TrimSuffix(strings.TrimSpace(string(line[4L..i])), ":"));
            var name = strings.TrimSpace(string(line[i..]));

            ptr<event> e = addr(new event(Action:action));
            if (line[0L] == '-')
            { // PASS or FAIL report
                // Parse out elapsed time.
                {
                    long i__prev2 = i;

                    i = strings.Index(name, " (");

                    if (i >= 0L)
                    {
                        if (strings.HasSuffix(name, "s)"))
                        {
                            var (t, err) = strconv.ParseFloat(name[i + 2L..len(name) - 2L], 64L);
                            if (err == null)
                            {
                                if (c.mode & Timestamp != 0L)
                                {
                                    e.Elapsed = _addr_t;
                                }

                            }

                        }

                        name = name[..i];

                    }

                    i = i__prev2;

                }

                if (len(c.report) < indent)
                { 
                    // Nested deeper than expected.
                    // Treat this line as plain output.
                    c.output.write(origLine);
                    return ;

                } 
                // Flush reports at this indentation level or deeper.
                c.flushReport(indent);
                e.Test = name;
                c.testName = name;
                c.report = append(c.report, e);
                c.output.write(origLine);
                return ;

            } 
            // === update.
            // Finish any pending PASS/FAIL reports.
            c.flushReport(0L);
            c.testName = name;

            if (action == "pause")
            { 
                // For a pause, we want to write the pause notification before
                // delivering the pause event, just so it doesn't look like the test
                // is generating output immediately after being paused.
                c.output.write(origLine);

            }

            c.writeEvent(e);
            if (action != "pause")
            {
                c.output.write(origLine);
            }

            return ;

        }

        // flushReport flushes all pending PASS/FAIL reports at levels >= depth.
        private static void flushReport(this ptr<Converter> _addr_c, long depth)
        {
            ref Converter c = ref _addr_c.val;

            c.testName = "";
            while (len(c.report) > depth)
            {
                var e = c.report[len(c.report) - 1L];
                c.report = c.report[..len(c.report) - 1L];
                c.writeEvent(e);
            }


        }

        // Close marks the end of the go test output.
        // It flushes any pending input and then output (only partial lines at this point)
        // and then emits the final overall package-level pass/fail event.
        private static error Close(this ptr<Converter> _addr_c)
        {
            ref Converter c = ref _addr_c.val;

            c.input.flush();
            c.output.flush();
            if (c.result != "")
            {
                ptr<event> e = addr(new event(Action:c.result));
                if (c.mode & Timestamp != 0L)
                {
                    ref var dt = ref heap(time.Since(c.start).Round(1L * time.Millisecond).Seconds(), out ptr<var> _addr_dt);
                    _addr_e.Elapsed = _addr_dt;
                    e.Elapsed = ref _addr_e.Elapsed.val;

                }

                c.writeEvent(e);

            }

            return error.As(null!)!;

        }

        // writeOutputEvent writes a single output event with the given bytes.
        private static void writeOutputEvent(this ptr<Converter> _addr_c, slice<byte> @out)
        {
            ref Converter c = ref _addr_c.val;

            c.writeEvent(addr(new event(Action:"output",Output:(*textBytes)(&out),)));
        }

        // writeEvent writes a single event.
        // It adds the package, time (if requested), and test name (if needed).
        private static void writeEvent(this ptr<Converter> _addr_c, ptr<event> _addr_e)
        {
            ref Converter c = ref _addr_c.val;
            ref event e = ref _addr_e.val;

            e.Package = c.pkg;
            if (c.mode & Timestamp != 0L)
            {
                ref var t = ref heap(time.Now(), out ptr<var> _addr_t);
                _addr_e.Time = _addr_t;
                e.Time = ref _addr_e.Time.val;

            }

            if (e.Test == "")
            {
                e.Test = c.testName;
            }

            var (js, err) = json.Marshal(e);
            if (err != null)
            { 
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
        private partial struct lineBuffer
        {
            public slice<byte> b; // buffer
            public bool mid; // whether we're in the middle of a long line
            public Action<slice<byte>> line; // line callback
            public Action<slice<byte>> part; // partial line callback
        }

        // write writes b to the buffer.
        private static void write(this ptr<lineBuffer> _addr_l, slice<byte> b)
        {
            ref lineBuffer l = ref _addr_l.val;

            while (len(b) > 0L)
            { 
                // Copy what we can into b.
                var m = copy(l.b[len(l.b)..cap(l.b)], b);
                l.b = l.b[..len(l.b) + m];
                b = b[m..]; 

                // Process lines in b.
                long i = 0L;
                while (i < len(l.b))
                {
                    var j = bytes.IndexByte(l.b[i..], '\n');
                    if (j < 0L)
                    {
                        if (!l.mid)
                        {
                            {
                                var j__prev3 = j;

                                j = bytes.IndexByte(l.b[i..], '\t');

                                if (j >= 0L)
                                {
                                    if (isBenchmarkName(bytes.TrimRight(l.b[i..i + j], " ")))
                                    {
                                        l.part(l.b[i..i + j + 1L]);
                                        l.mid = true;
                                        i += j + 1L;
                                    }

                                }

                                j = j__prev3;

                            }

                        }

                        break;

                    }

                    var e = i + j + 1L;
                    if (l.mid)
                    { 
                        // Found the end of a partial line.
                        l.part(l.b[i..e]);
                        l.mid = false;

                    }
                    else
                    { 
                        // Found a whole line.
                        l.line(l.b[i..e]);

                    }

                    i = e;

                } 

                // Whatever's left in l.b is a line fragment.
 

                // Whatever's left in l.b is a line fragment.
                if (i == 0L && len(l.b) == cap(l.b))
                { 
                    // The whole buffer is a fragment.
                    // Emit it as the beginning (or continuation) of a partial line.
                    var t = trimUTF8(l.b);
                    l.part(l.b[..t]);
                    l.b = l.b[..copy(l.b, l.b[t..])];
                    l.mid = true;

                } 

                // There's room for more input.
                // Slide it down in hope of completing the line.
                if (i > 0L)
                {
                    l.b = l.b[..copy(l.b, l.b[i..])];
                }

            }


        }

        // flush flushes the line buffer.
        private static void flush(this ptr<lineBuffer> _addr_l)
        {
            ref lineBuffer l = ref _addr_l.val;

            if (len(l.b) > 0L)
            { 
                // Must be a line without a \n, so a partial line.
                l.part(l.b);
                l.b = l.b[..0L];

            }

        }

        private static slice<byte> benchmark = (slice<byte>)"Benchmark";

        // isBenchmarkName reports whether b is a valid benchmark name
        // that might appear as the first field in a benchmark result line.
        private static bool isBenchmarkName(slice<byte> b)
        {
            if (!bytes.HasPrefix(b, benchmark))
            {
                return false;
            }

            if (len(b) == len(benchmark))
            { // just "Benchmark"
                return true;

            }

            var (r, _) = utf8.DecodeRune(b[len(benchmark)..]);
            return !unicode.IsLower(r);

        }

        // trimUTF8 returns a length t as close to len(b) as possible such that b[:t]
        // does not end in the middle of a possibly-valid UTF-8 sequence.
        //
        // If a large text buffer must be split before position i at the latest,
        // splitting at position trimUTF(b[:i]) avoids splitting a UTF-8 sequence.
        private static long trimUTF8(slice<byte> b)
        { 
            // Scan backward to find non-continuation byte.
            for (long i = 1L; i < utf8.UTFMax && i <= len(b); i++)
            {
                {
                    var c = b[len(b) - i];

                    if (c & 0xc0UL != 0x80UL)
                    {

                        if (c & 0xe0UL == 0xc0UL) 
                            if (i < 2L)
                            {
                                return len(b) - i;
                            }

                        else if (c & 0xf0UL == 0xe0UL) 
                            if (i < 3L)
                            {
                                return len(b) - i;
                            }

                        else if (c & 0xf8UL == 0xf0UL) 
                            if (i < 4L)
                            {
                                return len(b) - i;
                            }

                                                break;

                    }

                }

            }

            return len(b);

        }
    }
}}}
