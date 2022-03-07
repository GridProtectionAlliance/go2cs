// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package benchmark provides a Metrics object that enables memory and CPU
// profiling for the linker. The Metrics objects can be used to mark stages
// of the code, and name the measurements during that stage. There is also
// optional GCs that can be performed at the end of each stage, so you
// can get an accurate measurement of how each stage changes live memory.
// package benchmark -- go2cs converted at 2022 March 06 23:22:02 UTC
// import "cmd/link/internal/benchmark" ==> using benchmark = go.cmd.link.@internal.benchmark_package
// Original source: C:\Program Files\Go\src\cmd\link\internal\benchmark\bench.go
using fmt = go.fmt_package;
using io = go.io_package;
using os = go.os_package;
using runtime = go.runtime_package;
using pprof = go.runtime.pprof_package;
using time = go.time_package;
using unicode = go.unicode_package;

namespace go.cmd.link.@internal;

public static partial class benchmark_package {

public partial struct Flags { // : nint
}

public static readonly nint GC = 1 << (int)(iota);
public static readonly Flags NoGC = 0;


public partial struct Metrics {
    public Flags gc;
    public slice<ptr<mark>> marks;
    public ptr<mark> curMark;
    public @string filebase;
    public ptr<os.File> pprofFile;
}

private partial struct mark {
    public @string name;
    public runtime.MemStats startM;
    public runtime.MemStats endM;
    public runtime.MemStats gcM;
    public time.Time startT;
    public time.Time endT;
}

// New creates a new Metrics object.
//
// Typical usage should look like:
//
// func main() {
//   filename := "" // Set to enable per-phase pprof file output.
//   bench := benchmark.New(benchmark.GC, filename)
//   defer bench.Report(os.Stdout)
//   // etc
//   bench.Start("foo")
//   foo()
//   bench.Start("bar")
//   bar()
// }
//
// Note that a nil Metrics object won't cause any errors, so one could write
// code like:
//
//  func main() {
//    enableBenchmarking := flag.Bool("enable", true, "enables benchmarking")
//    flag.Parse()
//    var bench *benchmark.Metrics
//    if *enableBenchmarking {
//      bench = benchmark.New(benchmark.GC)
//    }
//    bench.Start("foo")
//    // etc.
//  }
public static ptr<Metrics> New(Flags gc, @string filebase) {
    if (gc == GC) {
        runtime.GC();
    }
    return addr(new Metrics(gc:gc,filebase:filebase));

}

// Report reports the metrics.
// Closes the currently Start(ed) range, and writes the report to the given io.Writer.
private static void Report(this ptr<Metrics> _addr_m, io.Writer w) {
    ref Metrics m = ref _addr_m.val;

    if (m == null) {
        return ;
    }
    m.closeMark();

    @string gcString = "";
    if (m.gc == GC) {
        gcString = "_GC";
    }
    time.Duration totTime = default;
    foreach (var (_, curMark) in m.marks) {
        var dur = curMark.endT.Sub(curMark.startT);
        totTime += dur;
        fmt.Fprintf(w, "%s 1 %d ns/op", makeBenchString(curMark.name + gcString), dur.Nanoseconds());
        fmt.Fprintf(w, "\t%d B/op", curMark.endM.TotalAlloc - curMark.startM.TotalAlloc);
        fmt.Fprintf(w, "\t%d allocs/op", curMark.endM.Mallocs - curMark.startM.Mallocs);
        if (m.gc == GC) {
            fmt.Fprintf(w, "\t%d live-B", curMark.gcM.HeapAlloc);
        }
        else
 {
            fmt.Fprintf(w, "\t%d heap-B", curMark.endM.HeapAlloc);
        }
        fmt.Fprintf(w, "\n");

    }    fmt.Fprintf(w, "%s 1 %d ns/op\n", makeBenchString("total time" + gcString), totTime.Nanoseconds());

}

// Starts marks the beginning of a new measurement phase.
// Once a metric is started, it continues until either a Report is issued, or another Start is called.
private static void Start(this ptr<Metrics> _addr_m, @string name) => func((_, panic, _) => {
    ref Metrics m = ref _addr_m.val;

    if (m == null) {
        return ;
    }
    m.closeMark();
    m.curMark = addr(new mark(name:name)); 
    // Unlikely we need to a GC here, as one was likely just done in closeMark.
    if (m.shouldPProf()) {
        var (f, err) = os.Create(makePProfFilename(m.filebase, name, "cpuprof"));
        if (err != null) {
            panic(err);
        }
        m.pprofFile = f;
        err = pprof.StartCPUProfile(m.pprofFile);

        if (err != null) {
            panic(err);
        }
    }
    runtime.ReadMemStats(_addr_m.curMark.startM);
    m.curMark.startT = time.Now();

});

private static void closeMark(this ptr<Metrics> _addr_m) => func((_, panic, _) => {
    ref Metrics m = ref _addr_m.val;

    if (m == null || m.curMark == null) {
        return ;
    }
    m.curMark.endT = time.Now();
    if (m.shouldPProf()) {
        pprof.StopCPUProfile();
        m.pprofFile.Close();
        m.pprofFile = null;
    }
    runtime.ReadMemStats(_addr_m.curMark.endM);
    if (m.gc == GC) {
        runtime.GC();
        runtime.ReadMemStats(_addr_m.curMark.gcM);
        if (m.shouldPProf()) { 
            // Collect a profile of the live heap. Do a
            // second GC to force sweep completion so we
            // get a complete snapshot of the live heap at
            // the end of this phase.
            runtime.GC();
            var (f, err) = os.Create(makePProfFilename(m.filebase, m.curMark.name, "memprof"));
            if (err != null) {
                panic(err);
            }

            err = pprof.WriteHeapProfile(f);
            if (err != null) {
                panic(err);
            }

            err = f.Close();
            if (err != null) {
                panic(err);
            }

        }
    }
    m.marks = append(m.marks, m.curMark);
    m.curMark = null;

});

// shouldPProf returns true if we should be doing pprof runs.
private static bool shouldPProf(this ptr<Metrics> _addr_m) {
    ref Metrics m = ref _addr_m.val;

    return m != null && len(m.filebase) > 0;
}

// makeBenchString makes a benchmark string consumable by Go's benchmarking tools.
private static @string makeBenchString(@string name) {
    var needCap = true;
    slice<int> ret = (slice<int>)"Benchmark";
    foreach (var (_, r) in name) {
        if (unicode.IsSpace(r)) {
            needCap = true;
            continue;
        }
        if (needCap) {
            r = unicode.ToUpper(r);
            needCap = false;
        }
        ret = append(ret, r);

    }    return string(ret);

}

private static @string makePProfFilename(@string filebase, @string name, @string typ) {
    return fmt.Sprintf("%s_%s.%s", filebase, makeBenchString(name), typ);
}

} // end benchmark_package
