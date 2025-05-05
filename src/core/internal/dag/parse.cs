// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package dag implements a language for expressing directed acyclic
// graphs.
//
// The general syntax of a rule is:
//
//	a, b < c, d;
//
// which means c and d come after a and b in the partial order
// (that is, there are edges from c and d to a and b),
// but doesn't provide a relative order between a vs b or c vs d.
//
// The rules can chain together, as in:
//
//	e < f, g < h;
//
// which is equivalent to
//
//	e < f, g;
//	f, g < h;
//
// Except for the special bottom element "NONE", each name
// must appear exactly once on the right-hand side of any rule.
// That rule serves as the definition of the allowed successor
// for that name. The definition must appear before any uses
// of the name on the left-hand side of a rule. (That is, the
// rules themselves must be ordered according to the partial
// order, for easier reading by people.)
//
// Negative assertions double-check the partial order:
//
//	i !< j
//
// means that it must NOT be the case that i < j.
// Negative assertions may appear anywhere in the rules,
// even before i and j have been defined.
//
// Comments begin with #.
namespace go.@internal;

using cmp = cmp_package;
using fmt = fmt_package;
using slices = slices_package;
using strings = strings_package;
using ꓸꓸꓸany = Span<any>;

partial class dag_package {

[GoType] partial struct Graph {
    public slice<@string> Nodes;
    internal map<@string, nint> byLabel;
    internal map<@string, map<@string, bool>> edges;
}

internal static ж<Graph> newGraph() {
    return Ꮡ(new Graph(byLabel: new map<@string, nint>{}, edges: new map<@string, map<@string, bool>>{}));
}

[GoRecv] internal static bool addNode(this ref Graph g, @string label) {
    {
        nint _ = g.byLabel[label];
        var ok = g.byLabel[label]; if (ok) {
            return false;
        }
    }
    g.byLabel[label] = len(g.Nodes);
    g.Nodes = append(g.Nodes, label);
    g.edges[label] = new map<@string, bool>{};
    return true;
}

[GoRecv] public static void AddEdge(this ref Graph g, @string from, @string to) {
    g.edges[from][to] = true;
}

[GoRecv] public static void DelEdge(this ref Graph g, @string from, @string to) {
    delete(g.edges[from], to);
}

[GoRecv] public static bool HasEdge(this ref Graph g, @string from, @string to) {
    return g.edges[from] != default! && g.edges[from][to];
}

[GoRecv] public static slice<@string> Edges(this ref Graph g, @string from) {
    var edges = new slice<@string>(0, 16);
    foreach (var (k, _) in g.edges[from]) {
        edges = append(edges, k);
    }
    slices.SortFunc(edges, (@string a, @string b) => cmp.Compare(g.byLabel[a], g.byLabel[b]));
    return edges;
}

// Parse parses the DAG language and returns the transitive closure of
// the described graph. In the returned graph, there is an edge from "b"
// to "a" if b < a (or a > b) in the partial order.
public static (ж<Graph>, error) Parse(@string dag) {
    var g = newGraph();
    var disallowed = new rule[]{}.slice();
    (rules, err) = parseRules(dag);
    if (err != default!) {
        return (default!, err);
    }
    // TODO: Add line numbers to errors.
    slice<@string> errors = default!;
    var errorf = 
    var errorsʗ1 = errors;
    (@string format, params ꓸꓸꓸany aʗp) => {
        errorsʗ1 = append(errorsʗ1, fmt.Sprintf(format, a.ꓸꓸꓸ));
    };
    foreach (var (_, r) in rules) {
        if (r.op == "!<"u8) {
            disallowed = append(disallowed, r);
            continue;
        }
        foreach (var (_, def) in r.def) {
            if (def == "NONE"u8) {
                errorf("NONE cannot be a predecessor"u8);
                continue;
            }
            if (!g.addNode(def)) {
                errorf("multiple definitions for %s"u8, def);
            }
            foreach (var (_, less) in r.less) {
                if (less == "NONE"u8) {
                    continue;
                }
                {
                    nint _ = (~g).byLabel[less];
                    var ok = (~g).byLabel[less]; if (!ok){
                        errorf("use of %s before its definition"u8, less);
                    } else {
                        g.AddEdge(def, less);
                    }
                }
            }
        }
    }
    // Check for missing definition.
    foreach (var (_, tos) in (~g).edges) {
        foreach (var (to, _) in tos) {
            if ((~g).edges[to] == default!) {
                errorf("missing definition for %s"u8, to);
            }
        }
    }
    // Complete transitive closure.
    foreach (var (_, k) in (~g).Nodes) {
        foreach (var (_, i) in (~g).Nodes) {
            foreach (var (_, j) in (~g).Nodes) {
                if (i != k && k != j && g.HasEdge(i, k) && g.HasEdge(k, j)) {
                    if (i == j) {
                        // Can only happen along with a "use of X before deps" error above,
                        // but this error is more specific - it makes clear that reordering the
                        // rules will not be enough to fix the problem.
                        errorf("graph cycle: %s < %s < %s"u8, j, k, i);
                    }
                    g.AddEdge(i, j);
                }
            }
        }
    }
    // Check negative assertions against completed allowed graph.
    foreach (var (_, bad) in disallowed) {
        foreach (var (_, less) in bad.less) {
            foreach (var (_, def) in bad.def) {
                if (g.HasEdge(def, less)) {
                    errorf("graph edge assertion failed: %s !< %s"u8, less, def);
                }
            }
        }
    }
    if (len(errors) > 0) {
        return (default!, fmt.Errorf("%s"u8, strings.Join(errors, "\n"u8)));
    }
    return (g, default!);
}

// A rule is a line in the DAG language where "less < def" or "less !< def".
[GoType] partial struct rule {
    internal slice<@string> less;
    internal @string op; // Either "<" or "!<"
    internal slice<@string> def;
}

[GoType("@string")] partial struct ΔsyntaxError;

public static @string Error(this ΔsyntaxError e) {
    return ((@string)e);
}

// parseRules parses the rules of a DAG.
internal static (slice<rule> @out, error err) parseRules(@string rules) => func((defer, recover) => {
    slice<rule> @out = default!;
    error err = default!;

    defer(() => {
        var e = recover();
        switch (e.type()) {
        case default! e: {
            return (@out, err);
        }
        case ΔsyntaxError e: {
            err = e;
            break;
        }
        default: {
            var e = e.type();
            throw panic(e);
            break;
        }}
    });
    var p = Ꮡ(new rulesParser(lineno: 1, text: rules));
    slice<@string> prev = default!;
    @string op = default!;
    while (ᐧ) {
        var (list, tok) = p.nextList();
        if (tok == ""u8) {
            if (prev == default!) {
                break;
            }
            p.syntaxError("unexpected EOF"u8);
        }
        if (prev != default!) {
            @out = append(@out, new rule(prev, op, list));
        }
        prev = list;
        if (tok == ";"u8) {
            prev = default!;
            op = ""u8;
            continue;
        }
        if (tok != "<"u8 && tok != "!<"u8) {
            p.syntaxError("missing <"u8);
        }
        op = tok;
    }
    return (@out, err);
});

// A rulesParser parses the depsRules syntax described above.
[GoType] partial struct rulesParser {
    internal nint lineno;
    internal @string lastWord;
    internal @string text;
}

// syntaxError reports a parsing error.
[GoRecv] internal static void syntaxError(this ref rulesParser p, @string msg) {
    throw panic(((ΔsyntaxError)fmt.Sprintf("parsing graph: line %d: syntax error: %s near %s"u8, p.lineno, msg, p.lastWord)));
}

// nextList parses and returns a comma-separated list of names.
[GoRecv] internal static (slice<@string> list, @string token) nextList(this ref rulesParser p) {
    slice<@string> list = default!;
    @string token = default!;

    while (ᐧ) {
        @string tok = p.nextToken();
        var exprᴛ1 = tok;
        var matchᴛ1 = false;
        if (exprᴛ1 == ""u8) {
            if (len(list) == 0) {
                return (default!, "");
            }
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && (exprᴛ1 == ","u8 || exprᴛ1 == "<"u8 || exprᴛ1 == "!<"u8 || exprᴛ1 == ";"u8)) { matchᴛ1 = true;
            p.syntaxError("bad list syntax"u8);
        }

        list = append(list, tok);
        tok = p.nextToken();
        if (tok != ","u8) {
            return (list, tok);
        }
    }
}

// nextToken returns the next token in the deps rules,
// one of ";" "," "<" "!<" or a name.
[GoRecv] internal static @string nextToken(this ref rulesParser p) {
    while (ᐧ) {
        if (p.text == ""u8) {
            return ""u8;
        }
        var exprᴛ1 = p.text[0];
        var matchᴛ1 = false;
        if (exprᴛ1 is (rune)';' or (rune)',' or (rune)'<') { matchᴛ1 = true;
            @string t = p.text[..1];
            p.text = p.text[1..];
            return t;
        }
        if (exprᴛ1 is (rune)'!') { matchᴛ1 = true;
            if (len(p.text) < 2 || p.text[1] != (rune)'<') {
                p.syntaxError("unexpected token !"u8);
            }
            p.text = p.text[2..];
            return "!<"u8;
        }
        if (exprᴛ1 is (rune)'#') { matchᴛ1 = true;
            nint i = strings.Index(p.text, "\n"u8);
            if (i < 0) {
                i = len(p.text);
            }
            p.text = p.text[(int)(i)..];
            continue;
        }
        else if (exprᴛ1 is (rune)'\n') { matchᴛ1 = true;
            p.lineno++;
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && (exprᴛ1 is (rune)' ' or (rune)'\t')) {
            p.text = p.text[1..];
            continue;
        }
        else { /* default: */
            nint i = strings.IndexAny(p.text, "!;,<#\n \t"u8);
            if (i < 0) {
                i = len(p.text);
            }
            @string t = p.text[..(int)(i)];
            p.text = p.text[(int)(i)..];
            p.lastWord = t;
            return t;
        }

    }
}

} // end dag_package
