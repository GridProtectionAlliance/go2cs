// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.coverage;

// This package provides apis for producing human-readable summaries
// of coverage data (e.g. a coverage percentage for a given package or
// set of packages) and for writing data in the legacy test format
// emitted by "go test -coverprofile=<outfile>".
//
// The model for using these apis is to create a Formatter object,
// then make a series of calls to SetPackage and AddUnit passing in
// data read from coverage meta-data and counter-data files. E.g.
//
//		myformatter := cformat.NewFormatter()
//		...
//		for each package P in meta-data file: {
//			myformatter.SetPackage(P)
//			for each function F in P: {
//				for each coverable unit U in F: {
//					myformatter.AddUnit(U)
//				}
//			}
//		}
//		myformatter.EmitPercent(os.Stdout, nil, "", true, true)
//		myformatter.EmitTextual(somefile)
//
// These apis are linked into tests that are built with "-cover", and
// called at the end of test execution to produce text output or
// emit coverage percentages.
using cmp = cmp_package;
using fmt = fmt_package;
using coverage = @internal.coverage_package;
using cmerge = @internal.coverage.cmerge_package;
using io = io_package;
using slices = slices_package;
using strings = strings_package;
using tabwriter = text.tabwriter_package;
using @internal;
using text;

partial class cformat_package {

[GoType] partial struct Formatter {
    // Maps import path to package state.
    internal map<@string, ж<pstate>> pm;
    // Records current package being visited.
    internal @string pkg;
    // Pointer to current package state.
    internal ж<pstate> p;
    // Counter mode.
    internal @internal.coverage_package.CounterMode cm;
}

// pstate records package-level coverage data state:
// - a table of functions (file/fname/literal)
// - a map recording the index/ID of each func encountered so far
// - a table storing execution count for the coverable units in each func
[GoType] partial struct pstate {
    // slice of unique functions
    internal slice<fnfile> funcs;
    // maps function to index in slice above (index acts as function ID)
    internal map<fnfile, uint32> funcTable;
    // A table storing coverage counts for each coverable unit.
    internal map<extcu, uint32> unitTable;
}

// extcu encapsulates a coverable unit within some function.
[GoType] partial struct extcu {
    internal uint32 fnfid; // index into p.funcs slice
    public partial ref @internal.coverage_package.CoverableUnit CoverableUnit { get; }
}

// fnfile is a function-name/file-name tuple.
[GoType] partial struct fnfile {
    internal @string file;
    internal @string fname;
    internal bool lit;
}

public static ж<Formatter> NewFormatter(coverage.CounterMode cm) {
    return Ꮡ(new Formatter(
        pm: new map<@string, ж<pstate>>(),
        cm: cm
    ));
}

// SetPackage tells the formatter that we're about to visit the
// coverage data for the package with the specified import path.
// Note that it's OK to call SetPackage more than once with the
// same import path; counter data values will be accumulated.
[GoRecv] public static void SetPackage(this ref Formatter fm, @string importpath) {
    if (importpath == fm.pkg) {
        return;
    }
    fm.pkg = importpath;
    var ps = fm.pm[importpath];
    var ok = fm.pm[importpath];
    if (!ok) {
        ps = @new<pstate>();
        fm.pm[importpath] = ps;
        ps.val.unitTable = new map<extcu, uint32>();
        ps.val.funcTable = new map<fnfile, uint32>();
    }
    fm.p = ps;
}

// AddUnit passes info on a single coverable unit (file, funcname,
// literal flag, range of lines, and counter value) to the formatter.
// Counter values will be accumulated where appropriate.
[GoRecv] public static void AddUnit(this ref Formatter fm, @string file, @string fname, bool isfnlit, coverage.CoverableUnit unit, uint32 count) {
    if (fm.p == nil) {
        throw panic("AddUnit invoked before SetPackage");
    }
    var fkey = new fnfile(file: file, fname: fname, lit: isfnlit);
    var (idx, ok) = fm.p.funcTable[fkey];
    if (!ok) {
        idx = ((uint32)len(fm.p.funcs));
        fm.p.funcs = append(fm.p.funcs, fkey);
        fm.p.funcTable[fkey] = idx;
    }
    var ukey = new extcu(fnfid: idx, CoverableUnit: unit);
    var pcount = fm.p.unitTable[ukey];
    uint32 result = default!;
    if (fm.cm == coverage.CtrModeSet){
        if (count != 0 || pcount != 0) {
            result = 1;
        }
    } else {
        // Use saturating arithmetic.
        (result, _) = cmerge.SaturatingAdd(pcount, count);
    }
    fm.p.unitTable[ukey] = result;
}

// sortUnits sorts a slice of extcu objects in a package according to
// source position information (e.g. file and line). Note that we don't
// include function name as part of the sorting criteria, the thinking
// being that is better to provide things in the original source order.
[GoRecv] internal static void sortUnits(this ref pstate p, slice<extcu> units) {
    slices.SortFunc(units, (extcu ui, extcu uj) => {
        @string ifile = p.funcs[ui.fnfid].file;
        @string jfile = p.funcs[uj.fnfid].file;
        {
            nint r = strings.Compare(ifile, jfile); if (r != 0) {
                return r;
            }
        }
        // NB: not taking function literal flag into account here (no
        // need, since other fields are guaranteed to be distinct).
        {
            nint r = cmp.Compare(ui.StLine, uj.StLine); if (r != 0) {
                return r;
            }
        }
        {
            nint r = cmp.Compare(ui.EnLine, uj.EnLine); if (r != 0) {
                return r;
            }
        }
        {
            nint r = cmp.Compare(ui.StCol, uj.StCol); if (r != 0) {
                return r;
            }
        }
        {
            nint r = cmp.Compare(ui.EnCol, uj.EnCol); if (r != 0) {
                return r;
            }
        }
        return cmp.Compare(ui.NxStmts, uj.NxStmts);
    });
}

// EmitTextual writes the accumulated coverage data in the legacy
// cmd/cover text format to the writer 'w'. We sort the data items by
// importpath, source file, and line number before emitting (this sorting
// is not explicitly mandated by the format, but seems like a good idea
// for repeatable/deterministic dumps).
[GoRecv] public static error EmitTextual(this ref Formatter fm, io.Writer w) {
    if (fm.cm == coverage.CtrModeInvalid) {
        throw panic("internal error, counter mode unset");
    }
    {
        var (_, err) = fmt.Fprintf(w, "mode: %s\n"u8, fm.cm.String()); if (err != default!) {
            return err;
        }
    }
    var pkgs = new slice<@string>(0, len(fm.pm));
    foreach (var (importpath, _) in fm.pm) {
        pkgs = append(pkgs, importpath);
    }
    slices.Sort(pkgs);
    foreach (var (_, importpath) in pkgs) {
        var p = fm.pm[importpath];
        var units = new slice<extcu>(0, len((~p).unitTable));
        foreach (var (u, _) in (~p).unitTable) {
            units = append(units, u);
        }
        p.sortUnits(units);
        foreach (var (_, u) in units) {
            var count = (~p).unitTable[u];
            @string file = (~p).funcs[u.fnfid].file;
            {
                var (_, err) = fmt.Fprintf(w, "%s:%d.%d,%d.%d %d %d\n"u8,
                    file, u.StLine, u.StCol,
                    u.EnLine, u.EnCol, u.NxStmts, count); if (err != default!) {
                    return err;
                }
            }
        }
    }
    return default!;
}

// EmitPercent writes out a "percentage covered" string to the writer
// 'w', selecting the set of packages in 'pkgs' and suffixing the
// printed string with 'inpkgs'.
[GoRecv] public static error EmitPercent(this ref Formatter fm, io.Writer w, slice<@string> pkgs, @string inpkgs, bool noteEmpty, bool aggregate) {
    if (len(pkgs) == 0) {
        pkgs = new slice<@string>(0, len(fm.pm));
        foreach (var (importpath, _) in fm.pm) {
            pkgs = append(pkgs, importpath);
        }
    }
    var rep = (uint64 cov, uint64 tot) => {
        if (tot != 0){
            {
                var (_, err) = fmt.Fprintf(w, "coverage: %.1f%% of statements%s\n"u8,
                    100.0F * ((float64)cov) / ((float64)tot), inpkgs); if (err != default!) {
                    return err;
                }
            }
        } else 
        if (noteEmpty) {
            {
                var (_, err) = fmt.Fprintf(w, "coverage: [no statements]\n"u8); if (err != default!) {
                    return err;
                }
            }
        }
        return default!;
    };
    slices.Sort(pkgs);
    uint64 totalStmts = default!;
    uint64 coveredStmts = default!;
    foreach (var (_, importpath) in pkgs) {
        var p = fm.pm[importpath];
        if (p == nil) {
            continue;
        }
        if (!aggregate) {
            (totalStmts, coveredStmts) = (0, 0);
        }
        foreach (var (unit, count) in (~p).unitTable) {
            var nx = ((uint64)unit.NxStmts);
            totalStmts += nx;
            if (count != 0) {
                coveredStmts += nx;
            }
        }
        if (!aggregate) {
            {
                var (_, err) = fmt.Fprintf(w, "\t%s\t\t"u8, importpath); if (err != default!) {
                    return err;
                }
            }
            {
                var err = rep(coveredStmts, totalStmts); if (err != default!) {
                    return err;
                }
            }
        }
    }
    if (aggregate) {
        {
            var err = rep(coveredStmts, totalStmts); if (err != default!) {
                return err;
            }
        }
    }
    return default!;
}

// EmitFuncs writes out a function-level summary to the writer 'w'. A
// note on handling function literals: although we collect coverage
// data for unnamed literals, it probably does not make sense to
// include them in the function summary since there isn't any good way
// to name them (this is also consistent with the legacy cmd/cover
// implementation). We do want to include their counts in the overall
// summary however.
[GoRecv] public static error EmitFuncs(this ref Formatter fm, io.Writer w) => func((defer, _) => {
    if (fm.cm == coverage.CtrModeInvalid) {
        throw panic("internal error, counter mode unset");
    }
    var perc = (uint64 covered, uint64 total) => {
        if (total == 0) {
            total = 1;
        }
        return 100.0F * ((float64)covered) / ((float64)total);
    };
    var tabber = tabwriter.NewWriter(w, 1, 8, 1, (rune)'\t', 0);
    var tabberʗ1 = tabber;
    defer(tabberʗ1.Flush);
    var allStmts = ((uint64)0);
    var covStmts = ((uint64)0);
    var pkgs = new slice<@string>(0, len(fm.pm));
    foreach (var (importpath, _) in fm.pm) {
        pkgs = append(pkgs, importpath);
    }
    slices.Sort(pkgs);
    // Emit functions for each package, sorted by import path.
    foreach (var (_, importpath) in pkgs) {
        var p = fm.pm[importpath];
        if (len((~p).unitTable) == 0) {
            continue;
        }
        var units = new slice<extcu>(0, len((~p).unitTable));
        foreach (var (u, _) in (~p).unitTable) {
            units = append(units, u);
        }
        // Within a package, sort the units, then walk through the
        // sorted array. Each time we hit a new function, emit the
        // summary entry for the previous function, then make one last
        // emit call at the end of the loop.
        p.sortUnits(units);
        @string fname = ""u8;
        @string ffile = ""u8;
        var flit = false;
        uint32 fline = default!;
        uint64 cstmts = default!;
        uint64 tstmts = default!;
        var captureFuncStart = 
        var pʗ1 = p;
        (extcu u) => {
            fname = (~pʗ1).funcs[u.fnfid].fname;
            ffile = (~pʗ1).funcs[u.fnfid].file;
            flit = (~pʗ1).funcs[u.fnfid].lit;
            fline = u.StLine;
        };
        var emitFunc = 
        var captureFuncStartʗ1 = captureFuncStart;
        var percʗ1 = perc;
        var tabberʗ2 = tabber;
        (extcu u) => {
            // Don't emit entries for function literals (see discussion
            // in function header comment above).
            if (!flit) {
                {
                    var (_, err) = fmt.Fprintf(~tabberʗ2, "%s:%d:\t%s\t%.1f%%\n"u8,
                        ffile, fline, fname, percʗ1(cstmts, tstmts)); if (err != default!) {
                        return err;
                    }
                }
            }
            captureFuncStartʗ1(u);
            allStmts += tstmts;
            covStmts += cstmts;
            tstmts = 0;
            cstmts = 0;
            return default!;
        };
        foreach (var (k, u) in units) {
            if (k == 0){
                captureFuncStart(u);
            } else {
                if (fname != (~p).funcs[u.fnfid].fname) {
                    // New function; emit entry for previous one.
                    {
                        var err = emitFunc(u); if (err != default!) {
                            return err;
                        }
                    }
                }
            }
            tstmts += ((uint64)u.NxStmts);
            var count = (~p).unitTable[u];
            if (count != 0) {
                cstmts += ((uint64)u.NxStmts);
            }
        }
        {
            var err = emitFunc(new extcu(nil)); if (err != default!) {
                return err;
            }
        }
    }
    {
        var (_, err) = fmt.Fprintf(~tabber, "%s\t%s\t%.1f%%\n"u8,
            "total", "(statements)", perc(covStmts, allStmts)); if (err != default!) {
            return err;
        }
    }
    return default!;
});

} // end cformat_package
