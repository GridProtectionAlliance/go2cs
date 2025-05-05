// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.coverage;

using json = encoding.json_package;
using fmt = fmt_package;
using coverage = @internal.coverage_package;
using calloc = @internal.coverage.calloc_package;
using cformat = @internal.coverage.cformat_package;
using cmerge = @internal.coverage.cmerge_package;
using decodecounter = @internal.coverage.decodecounter_package;
using decodemeta = @internal.coverage.decodemeta_package;
using pods = @internal.coverage.pods_package;
using rtcov = @internal.coverage.rtcov_package;
using atomic = @internal.runtime.atomic_package;
using io = io_package;
using os = os_package;
using filepath = path.filepath_package;
using strings = strings_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;
using encoding;
using path;

partial class cfile_package {

// ProcessCoverTestDir is called from
// testmain code when "go test -cover" is in effect. It is not
// intended to be used other than internally by the Go command's
// generated code.
public static error ProcessCoverTestDir(@string dir, @string cfile, @string cm, @string cpkg, io.Writer w, slice<@string> selpkgs) => func((defer, _) => {
    ref var cmode = ref heap<@internal.coverage_package.CounterMode>(out var Ꮡcmode);
    cmode = coverage.ParseCounterMode(cm);
    if (cmode == coverage.CtrModeInvalid) {
        return fmt.Errorf("invalid counter mode %q"u8, cm);
    }
    // Emit meta-data and counter data.
    var ml = rtcov.Meta.List;
    if (len(ml) == 0){
    } else {
        // This corresponds to the case where we have a package that
        // contains test code but no functions (which is fine). In this
        // case there is no need to emit anything.
        {
            var errΔ1 = emitMetaDataToDirectory(dir, ml); if (errΔ1 != default!) {
                return errΔ1;
            }
        }
        {
            var errΔ2 = emitCounterDataToDirectory(dir); if (errΔ2 != default!) {
                return errΔ2;
            }
        }
    }
    // Collect pods from test run. For the majority of cases we would
    // expect to see a single pod here, but allow for multiple pods in
    // case the test harness is doing extra work to collect data files
    // from builds that it kicks off as part of the testing.
    (podlist, errΔ3) = pods.CollectPods(new @string[]{dir}.slice(), false);
    if (errΔ3 != default!) {
        return fmt.Errorf("reading from %s: %v"u8, dir, errΔ3);
    }
    // Open text output file if appropriate.
    ж<os.File> tf = default!;
    bool tfClosed = default!;
    if (cfile != ""u8) {
        error errΔ4 = default!;
        (tf, errΔ4) = os.Create(cfile);
        if (errΔ4 != default!) {
            return fmt.Errorf("internal error: opening coverage data output file %q: %v"u8, cfile, errΔ4);
        }
        var tfʗ1 = tf;
        defer(() => {
            if (!tfClosed) {
                tfClosed = true;
                tfʗ1.Close();
            }
        });
    }
    // Read/process the pods.
    var ts = Ꮡ(new tstate(
        cm: Ꮡ(new cmerge.Merger(nil)),
        cf: cformat.NewFormatter(cmode),
        cmode: cmode
    ));
    // Generate the expected hash string based on the final meta-data
    // hash for this test, then look only for pods that refer to that
    // hash (just in case there are multiple instrumented executables
    // in play). See issue #57924 for more on this.
    @string hashstring = fmt.Sprintf("%x"u8, finalHash);
    var importpaths = new map<@string, struct{}>();
    foreach (var (_, p) in podlist) {
        if (!strings.Contains(p.MetaFile, hashstring)) {
            continue;
        }
        {
            var errΔ5 = ts.processPod(p, importpaths); if (errΔ5 != default!) {
                return errΔ5;
            }
        }
    }
    @string metafilespath = filepath.Join(dir, coverage.MetaFilesFileName);
    {
        (_, errΔ6) = os.Stat(metafilespath); if (errΔ6 == default!) {
            {
                var errΔ7 = ts.readAuxMetaFiles(metafilespath, importpaths); if (errΔ7 != default!) {
                    return errΔ7;
                }
            }
        }
    }
    // Emit percent.
    {
        var errΔ8 = (~ts).cf.EmitPercent(w, selpkgs, cpkg, true, true); if (errΔ8 != default!) {
            return errΔ8;
        }
    }
    // Emit text output.
    if (tf != nil) {
        {
            var errΔ9 = (~ts).cf.EmitTextual(~tf); if (errΔ9 != default!) {
                return errΔ9;
            }
        }
        tfClosed = true;
        {
            var errΔ10 = tf.Close(); if (errΔ10 != default!) {
                return fmt.Errorf("closing %s: %v"u8, cfile, errΔ10);
            }
        }
    }
    return default!;
});

[GoType] partial struct tstate {
    public partial ref @internal.coverage.calloc_package.BatchCounterAlloc BatchCounterAlloc { get; }
    internal ж<@internal.coverage.cmerge_package.Merger> cm;
    internal ж<@internal.coverage.cformat_package.Formatter> cf;
    internal @internal.coverage_package.CounterMode cmode;
}

[GoType("dyn")] partial struct processPod_importpaths {
}

// processPod reads coverage counter data for a specific pod.
[GoRecv] internal static error processPod(this ref tstate ts, pods.Pod p, map<@string, struct{}> importpaths) => func((defer, _) => {
    // Open meta-data file
    (f, err) = os.Open(p.MetaFile);
    if (err != default!) {
        return fmt.Errorf("unable to open meta-data file %s: %v"u8, p.MetaFile, err);
    }
    var fʗ1 = f;
    defer(() => {
        fʗ1.Close();
    });
    ж<decodemeta.CoverageMetaFileReader> mfr = default!;
    (mfr, err) = decodemeta.NewCoverageMetaFileReader(f, default!);
    if (err != default!) {
        return fmt.Errorf("error reading meta-data file %s: %v"u8, p.MetaFile, err);
    }
    var newmode = mfr.CounterMode();
    if (newmode != ts.cmode) {
        return fmt.Errorf("internal error: counter mode clash: %q from test harness, %q from data file %s"u8, ts.cmode.String(), newmode.String(), p.MetaFile);
    }
    var newgran = mfr.CounterGranularity();
    {
        var errΔ1 = ts.cm.SetModeAndGranularity(p.MetaFile, cmode, newgran); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    // A map to store counter data, indexed by pkgid/fnid tuple.
    var pmm = new map<pkfunc, slice<uint32>>();
    // Helper to read a single counter data file.
    var readcdf = 
    var pmmʗ1 = pmm;
    (@string cdf) => {
        (cf, errΔ2) = os.Open(cdf);
        if (errΔ2 != default!) {
            return fmt.Errorf("opening counter data file %s: %s"u8, cdf, errΔ2);
        }
        var cfʗ1 = cf;
        defer(cfʗ1.Close);
        ж<decodecounter.CounterDataReader> cdr = default!;
        (cdr, err) = decodecounter.NewCounterDataReader(cdf, ~cf);
        if (errΔ2 != default!) {
            return fmt.Errorf("reading counter data file %s: %s"u8, cdf, errΔ2);
        }
        ref var data = ref heap(new @internal.coverage.decodecounter_package.FuncPayload(), out var Ꮡdata);
        while (ᐧ) {
            var (ok, errΔ3) = cdr.NextFunc(Ꮡdata);
            if (errΔ3 != default!) {
                return fmt.Errorf("reading counter data file %s: %v"u8, cdf, errΔ3);
            }
            if (!ok) {
                break;
            }
            // NB: sanity check on pkg and func IDs?
            ref var key = ref heap<pkfunc>(out var Ꮡkey);
            key = new pkfunc(pk: data.PkgIdx, fcn: data.FuncIdx);
            {
                var prev = pmm[key];
                var found = pmm[key]; if (found) {
                    // Note: no overflow reporting here.
                    {
                        var (errΔ4, _) = ts.cm.MergeCounters(data.Counters, prev); if (errΔ4 != default!) {
                            return fmt.Errorf("processing counter data file %s: %v"u8, cdf, errΔ4);
                        }
                    }
                }
            }
            var c = ts.AllocateCounters(len(data.Counters));
            copy(c, data.Counters);
            pmm[key] = c;
        }
        return default!;
    };
    // Read counter data files.
    foreach (var (_, cdf) in p.CounterDataFiles) {
        {
            var errΔ5 = readcdf(cdf); if (errΔ5 != default!) {
                return errΔ5;
            }
        }
    }
    // Visit meta-data file.
    var np = ((uint32)mfr.NumPackages());
    var payload = new byte[]{}.slice();
    for (var pkIdx = ((uint32)0); pkIdx < np; pkIdx++) {
        ж<decodemeta.CoverageMetaDataDecoder> pd = default!;
        (pd, payload, err) = mfr.GetPackageDecoder(pkIdx, payload);
        if (err != default!) {
            return fmt.Errorf("reading pkg %d from meta-file %s: %s"u8, pkIdx, p.MetaFile, err);
        }
        ts.cf.SetPackage(pd.PackagePath());
        importpaths[pd.PackagePath()] = new processPod_importpaths();
        ref var fd = ref heap(new @internal.coverage_package.FuncDesc(), out var Ꮡfd);
        var nf = pd.NumFuncs();
        for (var fnIdx = ((uint32)0); fnIdx < nf; fnIdx++) {
            {
                var errΔ6 = pd.ReadFunc(fnIdx, Ꮡfd); if (errΔ6 != default!) {
                    return fmt.Errorf("reading meta-data file %s: %v"u8,
                        p.MetaFile, errΔ6);
                }
            }
            var key = new pkfunc(pk: pkIdx, fcn: fnIdx);
            var counters = pmm[key];
            var haveCounters = pmm[key];
            for (nint i = 0; i < len(fd.Units); i++) {
                var u = fd.Units[i];
                // Skip units with non-zero parent (no way to represent
                // these in the existing format).
                if (u.Parent != 0) {
                    continue;
                }
                var count = ((uint32)0);
                if (haveCounters) {
                    count = counters[i];
                }
                ts.cf.AddUnit(fd.Srcfile, fd.Funcname, fd.Lit, u, count);
            }
        }
    }
    return default!;
});

[GoType] partial struct pkfunc {
    internal uint32 pk;
    internal uint32 fcn;
}

[GoRecv] internal static error readAuxMetaFiles(this ref tstate ts, @string metafiles, map<@string, struct{}> importpaths) {
    // Unmarshal the information on available aux metafiles into
    // a MetaFileCollection struct.
    ref var mfc = ref heap(new @internal.coverage_package.MetaFileCollection(), out var Ꮡmfc);
    (data, err) = os.ReadFile(metafiles);
    if (err != default!) {
        return fmt.Errorf("error reading auxmetafiles file %q: %v"u8, metafiles, err);
    }
    {
        var errΔ1 = json.Unmarshal(data, Ꮡmfc); if (errΔ1 != default!) {
            return fmt.Errorf("error reading auxmetafiles file %q: %v"u8, metafiles, errΔ1);
        }
    }
    // Walk through each available aux meta-file. If we've already
    // seen the package path in question during the walk of the
    // "regular" meta-data file, then we can skip the package,
    // otherwise construct a dummy pod with the single meta-data file
    // (no counters) and invoke processPod on it.
    foreach (var (i, _) in mfc.ImportPaths) {
        @string p = mfc.ImportPaths[i];
        {
            var (_, ok) = importpaths[p]; if (ok) {
                continue;
            }
        }
        pods.Pod pod = default!;
        pod.MetaFile = mfc.MetaFileFragments[i];
        {
            var errΔ2 = ts.processPod(pod, importpaths); if (errΔ2 != default!) {
                return errΔ2;
            }
        }
    }
    return default!;
}

// Snapshot returns a snapshot of coverage percentage at a moment of
// time within a running test, so as to support the testing.Coverage()
// function. This version doesn't examine coverage meta-data, so the
// result it returns will be less accurate (more "slop") due to the
// fact that we don't look at the meta data to see how many statements
// are associated with each counter.
public static float64 Snapshot() {
    var cl = getCovCounterList();
    if (len(cl) == 0) {
        // no work to do here.
        return 0.0F;
    }
    var tot = ((uint64)0);
    var totExec = ((uint64)0);
    foreach (var (_, c) in cl) {
        var sd = @unsafe.Slice((ж<atomic.Uint32>)(uintptr)(new @unsafe.Pointer(c.Counters)), c.Len);
        tot += ((uint64)len(sd));
        for (nint i = 0; i < len(sd); i++) {
            // Skip ahead until the next non-zero value.
            if (sd[i].Load() == 0) {
                continue;
            }
            // We found a function that was executed.
            var nCtrs = sd[i + coverage.NumCtrsOffset].Load();
            nint cst = i + coverage.FirstCtrOffset;
            if (cst + ((nint)nCtrs) > len(sd)) {
                break;
            }
            var counters = sd[(int)(cst)..(int)(cst + ((nint)nCtrs))];
            foreach (var (iΔ1, _) in counters) {
                if (counters[iΔ1].Load() != 0) {
                    totExec++;
                }
            }
            i += coverage.FirstCtrOffset + ((nint)nCtrs) - 1;
        }
    }
    if (tot == 0) {
        return 0.0F;
    }
    return ((float64)totExec) / ((float64)tot);
}

} // end cfile_package
