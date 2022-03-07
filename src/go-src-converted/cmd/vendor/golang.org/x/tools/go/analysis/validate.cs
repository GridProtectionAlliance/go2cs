// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package analysis -- go2cs converted at 2022 March 06 23:31:07 UTC
// import "cmd/vendor/golang.org/x/tools/go/analysis" ==> using analysis = go.cmd.vendor.golang.org.x.tools.go.analysis_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\tools\go\analysis\validate.go
using fmt = go.fmt_package;
using reflect = go.reflect_package;
using strings = go.strings_package;
using unicode = go.unicode_package;
using System;


namespace go.cmd.vendor.golang.org.x.tools.go;

public static partial class analysis_package {

    // Validate reports an error if any of the analyzers are misconfigured.
    // Checks include:
    // that the name is a valid identifier;
    // that the Requires graph is acyclic;
    // that analyzer fact types are unique;
    // that each fact type is a pointer.
public static error Validate(slice<ptr<Analyzer>> analyzers) { 
    // Map each fact type to its sole generating analyzer.
    var factTypes = make_map<reflect.Type, ptr<Analyzer>>(); 

    // Traverse the Requires graph, depth first.
    const var white = iota;
    const var grey = 0;
    const var black = 1;
    const var finished = 2;

    var color = make_map<ptr<Analyzer>, byte>();
    Func<ptr<Analyzer>, error> visit = default;
    visit = a => {
        if (a == null) {
            return error.As(fmt.Errorf("nil *Analyzer"))!;
        }
        if (color[a] == white) {
            color[a] = grey; 

            // names
            if (!validIdent(a.Name)) {
                return error.As(fmt.Errorf("invalid analyzer name %q", a))!;
            }
            if (a.Doc == "") {
                return error.As(fmt.Errorf("analyzer %q is undocumented", a))!;
            }
            foreach (var (_, f) in a.FactTypes) {
                if (f == null) {
                    return error.As(fmt.Errorf("analyzer %s has nil FactType", a))!;
                }
                var t = reflect.TypeOf(f);
                {
                    var prev = factTypes[t];

                    if (prev != null) {
                        return error.As(fmt.Errorf("fact type %s registered by two analyzers: %v, %v", t, a, prev))!;
                    }
                }

                if (t.Kind() != reflect.Ptr) {
                    return error.As(fmt.Errorf("%s: fact type %s is not a pointer", a, t))!;
                }
                factTypes[t] = a;

            }            foreach (var (_, req) in a.Requires) {
                {
                    var err__prev2 = err;

                    var err = visit(req);

                    if (err != null) {
                        return error.As(err)!;
                    }
                    err = err__prev2;

                }

            }            color[a] = black;

        }
        if (color[a] == grey) {
            ptr<Analyzer> stack = new slice<ptr<Analyzer>>(new ptr<Analyzer>[] { a });
            map inCycle = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{};
            while (len(stack) > 0) {
                var current = stack[len(stack) - 1];
                stack = stack[..(int)len(stack) - 1];
                if (color[current] == grey && !inCycle[current.Name]) {
                    inCycle[current.Name] = true;
                    stack = append(stack, current.Requires);
                }
            }
            return error.As(addr(new CycleInRequiresGraphError(AnalyzerNames:inCycle))!)!;

        }
        return error.As(null!)!;

    };
    {
        var a__prev1 = a;

        foreach (var (_, __a) in analyzers) {
            a = __a;
            {
                var err__prev1 = err;

                err = visit(a);

                if (err != null) {
                    return error.As(err)!;
                }
                err = err__prev1;

            }

        }
        a = a__prev1;
    }

    {
        var a__prev1 = a;

        foreach (var (_, __a) in analyzers) {
            a = __a;
            if (color[a] == finished) {
                return error.As(fmt.Errorf("duplicate analyzer: %s", a.Name))!;
            }
            color[a] = finished;

        }
        a = a__prev1;
    }

    return error.As(null!)!;

}

private static bool validIdent(@string name) {
    foreach (var (i, r) in name) {
        if (!(r == '_' || unicode.IsLetter(r) || i > 0 && unicode.IsDigit(r))) {
            return false;
        }
    }    return name != "";

}

public partial struct CycleInRequiresGraphError {
    public map<@string, bool> AnalyzerNames;
}

private static @string Error(this ptr<CycleInRequiresGraphError> _addr_e) {
    ref CycleInRequiresGraphError e = ref _addr_e.val;

    strings.Builder b = default;
    b.WriteString("cycle detected involving the following analyzers:");
    foreach (var (n) in e.AnalyzerNames) {
        b.WriteByte(' ');
        b.WriteString(n);
    }    return b.String();
}

} // end analysis_package
