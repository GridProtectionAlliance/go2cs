// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package analysisflags defines helpers for processing flags of
// analysis driver tools.
// package analysisflags -- go2cs converted at 2020 October 09 06:01:17 UTC
// import "cmd/vendor/golang.org/x/tools/go/analysis/internal/analysisflags" ==> using analysisflags = go.cmd.vendor.golang.org.x.tools.go.analysis.@internal.analysisflags_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\tools\go\analysis\internal\analysisflags\flags.go
using sha256 = go.crypto.sha256_package;
using gob = go.encoding.gob_package;
using json = go.encoding.json_package;
using flag = go.flag_package;
using fmt = go.fmt_package;
using token = go.go.token_package;
using io = go.io_package;
using ioutil = go.io.ioutil_package;
using log = go.log_package;
using os = go.os_package;
using strconv = go.strconv_package;
using strings = go.strings_package;

using analysis = go.golang.org.x.tools.go.analysis_package;
using static go.builtin;
using System;
using System.ComponentModel;

namespace go {
namespace cmd {
namespace vendor {
namespace golang.org {
namespace x {
namespace tools {
namespace go {
namespace analysis {
namespace @internal
{
    public static partial class analysisflags_package
    {
        // flags common to all {single,multi,unit}checkers.
        public static var JSON = false;        public static long Context = -1L;

        // Parse creates a flag for each of the analyzer's flags,
        // including (in multi mode) a flag named after the analyzer,
        // parses the flags, then filters and returns the list of
        // analyzers enabled by flags.
        //
        // The result is intended to be passed to unitchecker.Run or checker.Run.
        // Use in unitchecker.Run will gob.Register all fact types for the returned
        // graph of analyzers but of course not the ones only reachable from
        // dropped analyzers. To avoid inconsistency about which gob types are
        // registered from run to run, Parse itself gob.Registers all the facts
        // only reachable from dropped analyzers.
        // This is not a particularly elegant API, but this is an internal package.
        public static slice<ptr<analysis.Analyzer>> Parse(slice<ptr<analysis.Analyzer>> analyzers, bool multi)
        { 
            // Connect each analysis flag to the command line as -analysis.flag.
            var enabled = make_map<ptr<analysis.Analyzer>, ptr<triState>>();
            {
                var a__prev1 = a;

                foreach (var (_, __a) in analyzers)
                {
                    a = __a;
                    @string prefix = default; 

                    // Add -NAME flag to enable it.
                    if (multi)
                    {
                        prefix = a.Name + ".";

                        ptr<triState> enable = @new<triState>();
                        @string enableUsage = "enable " + a.Name + " analysis";
                        flag.Var(enable, a.Name, enableUsage);
                        enabled[a] = enable;
                    }

                    a.Flags.VisitAll(f =>
                    {
                        if (!multi && flag.Lookup(f.Name) != null)
                        {
                            log.Printf("%s flag -%s would conflict with driver; skipping", a.Name, f.Name);
                            return ;
                        }

                        var name = prefix + f.Name;
                        flag.Var(f.Value, name, f.Usage);

                    });

                } 

                // standard flags: -flags, -V.

                a = a__prev1;
            }

            var printflags = flag.Bool("flags", false, "print analyzer flags in JSON");
            addVersionFlag(); 

            // flags common to all checkers
            flag.BoolVar(_addr_JSON, "json", JSON, "emit JSON output");
            flag.IntVar(_addr_Context, "c", Context, "display offending line with this many lines of context"); 

            // Add shims for legacy vet flags to enable existing
            // scripts that run vet to continue to work.
            _ = flag.Bool("source", false, "no effect (deprecated)");
            _ = flag.Bool("v", false, "no effect (deprecated)");
            _ = flag.Bool("all", false, "no effect (deprecated)");
            _ = flag.String("tags", "", "no effect (deprecated)");
            foreach (var (old, new) in vetLegacyFlags)
            {
                var newFlag = flag.Lookup(new);
                if (newFlag != null && flag.Lookup(old) == null)
                {
                    flag.Var(newFlag.Value, old, "deprecated alias for -" + new);
                }

            }
            flag.Parse(); // (ExitOnError)

            // -flags: print flags so that go vet knows which ones are legitimate.
            if (printflags.val)
            {
                printFlags();
                os.Exit(0L);
            }

            var everything = expand(analyzers); 

            // If any -NAME flag is true,  run only those analyzers. Otherwise,
            // if any -NAME flag is false, run all but those analyzers.
            if (multi)
            {
                bool hasTrue = default;                bool hasFalse = default;

                foreach (var (_, ts) in enabled)
                {

                    if (ts.val == setTrue) 
                        hasTrue = true;
                    else if (ts.val == setFalse) 
                        hasFalse = true;
                    
                }
                slice<ptr<analysis.Analyzer>> keep = default;
                if (hasTrue)
                {
                    {
                        var a__prev1 = a;

                        foreach (var (_, __a) in analyzers)
                        {
                            a = __a;
                            if (enabled[a] == setTrue.val)
                            {
                                keep = append(keep, a);
                            }

                        }

                        a = a__prev1;
                    }

                    analyzers = keep;

                }
                else if (hasFalse)
                {
                    {
                        var a__prev1 = a;

                        foreach (var (_, __a) in analyzers)
                        {
                            a = __a;
                            if (enabled[a] != setFalse.val)
                            {
                                keep = append(keep, a);
                            }

                        }

                        a = a__prev1;
                    }

                    analyzers = keep;

                }

            } 

            // Register fact types of skipped analyzers
            // in case we encounter them in imported files.
            var kept = expand(analyzers);
            {
                var a__prev1 = a;

                foreach (var (__a) in everything)
                {
                    a = __a;
                    if (!kept[a])
                    {
                        foreach (var (_, f) in a.FactTypes)
                        {
                            gob.Register(f);
                        }

                    }

                }

                a = a__prev1;
            }

            return analyzers;

        }

        private static map<ptr<analysis.Analyzer>, bool> expand(slice<ptr<analysis.Analyzer>> analyzers)
        {
            var seen = make_map<ptr<analysis.Analyzer>, bool>();
            Action<slice<ptr<analysis.Analyzer>>> visitAll = default;
            visitAll = analyzers =>
            {
                foreach (var (_, a) in analyzers)
                {
                    if (!seen[a])
                    {
                        seen[a] = true;
                        visitAll(a.Requires);
                    }

                }

            }
;
            visitAll(analyzers);
            return seen;

        }

        private static void printFlags()
        {
            private partial struct jsonFlag
            {
                public @string Name;
                public bool Bool;
                public @string Usage;
            }
            slice<jsonFlag> flags = null;
            flag.VisitAll(f =>
            { 
                // Don't report {single,multi}checker debugging
                // flags or fix as these have no effect on unitchecker
                // (as invoked by 'go vet').
                switch (f.Name)
                {
                    case "debug": 

                    case "cpuprofile": 

                    case "memprofile": 

                    case "trace": 

                    case "fix": 
                        return ;
                        break;
                }

                var isBool = ok && b.IsBoolFlag();
                flags = append(flags, new jsonFlag(f.Name,isBool,f.Usage));

            });
            var (data, err) = json.MarshalIndent(flags, "", "\t");
            if (err != null)
            {
                log.Fatal(err);
            }

            os.Stdout.Write(data);

        }

        // addVersionFlag registers a -V flag that, if set,
        // prints the executable version and exits 0.
        //
        // If the -V flag already exists — for example, because it was already
        // registered by a call to cmd/internal/objabi.AddVersionFlag — then
        // addVersionFlag does nothing.
        private static void addVersionFlag()
        {
            if (flag.Lookup("V") == null)
            {
                flag.Var(new versionFlag(), "V", "print version and exit");
            }

        }

        // versionFlag minimally complies with the -V protocol required by "go vet".
        private partial struct versionFlag
        {
        }

        private static bool IsBoolFlag(this versionFlag _p0)
        {
            return true;
        }
        private static void Get(this versionFlag _p0)
        {
            return null;
        }
        private static @string String(this versionFlag _p0)
        {
            return "";
        }
        private static error Set(this versionFlag _p0, @string s)
        {
            if (s != "full")
            {
                log.Fatalf("unsupported flag value: -V=%s", s);
            } 

            // This replicates the minimal subset of
            // cmd/internal/objabi.AddVersionFlag, which is private to the
            // go tool yet forms part of our command-line interface.
            // TODO(adonovan): clarify the contract.

            // Print the tool version so the build system can track changes.
            // Formats:
            //   $progname version devel ... buildID=...
            //   $progname version go1.9.1
            var progname = os.Args[0L];
            var (f, err) = os.Open(progname);
            if (err != null)
            {
                log.Fatal(err);
            }

            var h = sha256.New();
            {
                var (_, err) = io.Copy(h, f);

                if (err != null)
                {
                    log.Fatal(err);
                }

            }

            f.Close();
            fmt.Printf("%s version devel comments-go-here buildID=%02x\n", progname, string(h.Sum(null)));
            os.Exit(0L);
            return error.As(null!)!;

        }

        // A triState is a boolean that knows whether
        // it has been set to either true or false.
        // It is used to identify whether a flag appears;
        // the standard boolean flag cannot
        // distinguish missing from unset.
        // It also satisfies flag.Value.
        private partial struct triState // : long
        {
        }

        private static readonly triState unset = (triState)iota;
        private static readonly var setTrue = 0;
        private static readonly var setFalse = 1;


        private static ptr<triState> triStateFlag(@string name, triState value, @string usage)
        {
            flag.Var(_addr_value, name, usage);
            return _addr__addr_value!;
        }

        // triState implements flag.Value, flag.Getter, and flag.boolFlag.
        // They work like boolean flags: we can say vet -printf as well as vet -printf=true
        private static void Get(this ptr<triState> _addr_ts)
        {
            ref triState ts = ref _addr_ts.val;

            return ts == setTrue.val;
        }

        private static bool isTrue(this triState ts)
        {
            return ts == setTrue;
        }

        private static error Set(this ptr<triState> _addr_ts, @string value)
        {
            ref triState ts = ref _addr_ts.val;

            var (b, err) = strconv.ParseBool(value);
            if (err != null)
            { 
                // This error message looks poor but package "flag" adds
                // "invalid boolean value %q for -NAME: %s"
                return error.As(fmt.Errorf("want true or false"))!;

            }

            if (b)
            {
                ts.val = setTrue;
            }
            else
            {
                ts.val = setFalse;
            }

            return error.As(null!)!;

        }

        private static @string String(this ptr<triState> _addr_ts) => func((_, panic, __) =>
        {
            ref triState ts = ref _addr_ts.val;


            if (ts.val == unset) 
                return "true";
            else if (ts.val == setTrue) 
                return "true";
            else if (ts.val == setFalse) 
                return "false";
                        panic("not reached");

        });

        private static bool IsBoolFlag(this triState ts)
        {
            return true;
        }

        // Legacy flag support

        // vetLegacyFlags maps flags used by legacy vet to their corresponding
        // new names. The old names will continue to work.
        private static map vetLegacyFlags = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, @string>{"bool":"bools","buildtags":"buildtag","methods":"stdmethods","rangeloops":"loopclosure","compositewhitelist":"composites.whitelist","printfuncs":"printf.funcs","shadowstrict":"shadow.strict","unusedfuncs":"unusedresult.funcs","unusedstringmethods":"unusedresult.stringmethods",};

        // ---- output helpers common to all drivers ----

        // PrintPlain prints a diagnostic in plain text form,
        // with context specified by the -c flag.
        public static void PrintPlain(ptr<token.FileSet> _addr_fset, analysis.Diagnostic diag)
        {
            ref token.FileSet fset = ref _addr_fset.val;

            var posn = fset.Position(diag.Pos);
            fmt.Fprintf(os.Stderr, "%s: %s\n", posn, diag.Message); 

            // -c=N: show offending line plus N lines of context.
            if (Context >= 0L)
            {
                posn = fset.Position(diag.Pos);
                var end = fset.Position(diag.End);
                if (!end.IsValid())
                {
                    end = posn;
                }

                var (data, _) = ioutil.ReadFile(posn.Filename);
                var lines = strings.Split(string(data), "\n");
                for (var i = posn.Line - Context; i <= end.Line + Context; i++)
                {
                    if (1L <= i && i <= len(lines))
                    {
                        fmt.Fprintf(os.Stderr, "%d\t%s\n", i, lines[i - 1L]);
                    }

                }


            }

        }

        // A JSONTree is a mapping from package ID to analysis name to result.
        // Each result is either a jsonError or a list of jsonDiagnostic.
        public static void Add(this JSONTree tree, ptr<token.FileSet> _addr_fset, @string id, @string name, slice<analysis.Diagnostic> diags, error err)
        {
            ref token.FileSet fset = ref _addr_fset.val;

            var v = default;
            if (err != null)
            {
                private partial struct jsonError
                {
                    [Description("json:\"error\"")]
                    public @string Err;
                }
                v = new jsonError(err.Error());

            }
            else if (len(diags) > 0L)
            {
                private partial struct jsonDiagnostic
                {
                    [Description("json:\"category,omitempty\"")]
                    public @string Category;
                    [Description("json:\"posn\"")]
                    public @string Posn;
                    [Description("json:\"message\"")]
                    public @string Message;
                }
                slice<jsonDiagnostic> diagnostics = default; 
                // TODO(matloob): Should the JSON diagnostics contain ranges?
                // If so, how should they be formatted?
                foreach (var (_, f) in diags)
                {
                    diagnostics = append(diagnostics, new jsonDiagnostic(Category:f.Category,Posn:fset.Position(f.Pos).String(),Message:f.Message,));
                }
                v = diagnostics;

            }

            if (v != null)
            {
                var (m, ok) = tree[id];
                if (!ok)
                {
                    m = make();
                    tree[id] = m;
                }

                m[name] = v;

            }

        }

        public static void Print(this JSONTree tree)
        {
            var (data, err) = json.MarshalIndent(tree, "", "\t");
            if (err != null)
            {
                log.Panicf("internal error: JSON marshaling failed: %v", err);
            }

            fmt.Printf("%s\n", data);

        }
    }
}}}}}}}}}
