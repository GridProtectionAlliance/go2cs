// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build gen
// +build gen

// This program generates Go code that applies rewrite rules to a Value.
// The generated code implements a function of type func (v *Value) bool
// which reports whether if did something.
// Ideas stolen from Swift: http://www.hpl.hp.com/techreports/Compaq-DEC/WRL-2000-2.html

// package main -- go2cs converted at 2022 March 06 23:09:30 UTC
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ssa\gen\rulegen.go
using bufio = go.bufio_package;
using bytes = go.bytes_package;
using flag = go.flag_package;
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using format = go.go.format_package;
using parser = go.go.parser_package;
using printer = go.go.printer_package;
using token = go.go.token_package;
using io = go.io_package;
using ioutil = go.io.ioutil_package;
using log = go.log_package;
using os = go.os_package;
using path = go.path_package;
using regexp = go.regexp_package;
using sort = go.sort_package;
using strconv = go.strconv_package;
using strings = go.strings_package;

using astutil = go.golang.org.x.tools.go.ast.astutil_package;
using System;


namespace go;

public static partial class main_package {

    // rule syntax:
    //  sexpr [&& extra conditions] => [@block] sexpr
    //
    // sexpr are s-expressions (lisp-like parenthesized groupings)
    // sexpr ::= [variable:](opcode sexpr*)
    //         | variable
    //         | <type>
    //         | [auxint]
    //         | {aux}
    //
    // aux      ::= variable | {code}
    // type     ::= variable | {code}
    // variable ::= some token
    // opcode   ::= one of the opcodes from the *Ops.go files

    // special rules: trailing ellipsis "..." (in the outermost sexpr?) must match on both sides of a rule.
    //                trailing three underscore "___" in the outermost match sexpr indicate the presence of
    //                   extra ignored args that need not appear in the replacement

    // extra conditions is just a chunk of Go that evaluates to a boolean. It may use
    // variables declared in the matching tsexpr. The variable "v" is predefined to be
    // the value matched by the entire rule.

    // If multiple rules match, the first one in file order is selected.
private static var genLog = flag.Bool("log", false, "generate code that logs; for debugging only");private static var addLine = flag.Bool("line", false, "add line number comment to generated rules; for debugging only");

public partial struct Rule {
    public @string Rule;
    public @string Loc; // file name & line number
}

public static @string String(this Rule r) {
    return fmt.Sprintf("rule %q at %s", r.Rule, r.Loc);
}

private static @string normalizeSpaces(@string s) {
    return strings.Join(strings.Fields(strings.TrimSpace(s)), " ");
}

// parse returns the matching part of the rule, additional conditions, and the result.
public static (@string, @string, @string) parse(this Rule r) {
    @string match = default;
    @string cond = default;
    @string result = default;

    var s = strings.Split(r.Rule, "=>");
    match = normalizeSpaces(s[0]);
    result = normalizeSpaces(s[1]);
    cond = "";
    {
        var i = strings.Index(match, "&&");

        if (i >= 0) {
            cond = normalizeSpaces(match[(int)i + 2..]);
            match = normalizeSpaces(match[..(int)i]);
        }
    }

    return (match, cond, result);

}

private static void genRules(arch arch) {
    genRulesSuffix(arch, "");
}
private static void genSplitLoadRules(arch arch) {
    genRulesSuffix(arch, "splitload");
}

private static void genRulesSuffix(arch arch, @string suff) => func((defer, panic, _) => { 
    // Open input file.
    var (text, err) = os.Open(arch.name + suff + ".rules");
    if (err != null) {
        if (suff == "") { 
            // All architectures must have a plain rules file.
            log.Fatalf("can't read rule file: %v", err);

        }
        return ;

    }
    map blockrules = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, slice<Rule>>{};
    map oprules = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, slice<Rule>>{}; 

    // read rule file
    var scanner = bufio.NewScanner(text);
    @string rule = "";
    nint lineno = default;
    nint ruleLineno = default; // line number of "=>"
    while (scanner.Scan()) {
        lineno++;
        var line = scanner.Text();
        {
            var i = strings.Index(line, "//");

            if (i >= 0) { 
                // Remove comments. Note that this isn't string safe, so
                // it will truncate lines with // inside strings. Oh well.
                line = line[..(int)i];

            }

        }

        rule += " " + line;
        rule = strings.TrimSpace(rule);
        if (rule == "") {
            continue;
        }
        if (!strings.Contains(rule, "=>")) {
            continue;
        }
        if (ruleLineno == 0) {
            ruleLineno = lineno;
        }
        if (strings.HasSuffix(rule, "=>")) {
            continue; // continue on the next line
        }
        {
            var n = balance(rule);

            if (n > 0) {
                continue; // open parentheses remain, continue on the next line
            }
            else if (n < 0) {
                break; // continuing the line can't help, and it will only make errors worse
            }


        }


        var loc = fmt.Sprintf("%s%s.rules:%d", arch.name, suff, ruleLineno);
        foreach (var (_, rule2) in expandOr(rule)) {
            Rule r = new Rule(Rule:rule2,Loc:loc);
            {
                var rawop = strings.Split(rule2, " ")[0][(int)1..];

                if (isBlock(rawop, arch)) {
                    blockrules[rawop] = append(blockrules[rawop], r);
                    continue;
                } 
                // Do fancier value op matching.

            } 
            // Do fancier value op matching.
            var (match, _, _) = r.parse();
            var (op, oparch, _, _, _, _) = parseValue(match, arch, loc);
            var opname = fmt.Sprintf("Op%s%s", oparch, op.name);
            oprules[opname] = append(oprules[opname], r);

        }        rule = "";
        ruleLineno = 0;

    }
    {
        var err__prev1 = err;

        var err = scanner.Err();

        if (err != null) {
            log.Fatalf("scanner failed: %v\n", err);
        }
        err = err__prev1;

    }

    if (balance(rule) != 0) {
        log.Fatalf("%s.rules:%d: unbalanced rule: %v\n", arch.name, lineno, rule);
    }
    slice<@string> ops = default;
    {
        var op__prev1 = op;

        foreach (var (__op) in oprules) {
            op = __op;
            ops = append(ops, op);
        }
        op = op__prev1;
    }

    sort.Strings(ops);

    ptr<File> genFile = addr(new File(Arch:arch,Suffix:suff)); 
    // Main rewrite routine is a switch on v.Op.
    ptr<Func> fn = addr(new Func(Kind:"Value",ArgLen:-1));

    ptr<Switch> sw = addr(new Switch(Expr:exprf("v.Op")));
    {
        var op__prev1 = op;

        foreach (var (_, __op) in ops) {
            op = __op;
            var (eop, ok) = parseEllipsisRules(oprules[op], arch);
            if (ok) {
                if (strings.Contains(oprules[op][0].Rule, "=>") && opByName(arch, op).aux != opByName(arch, eop).aux) {
                    panic(fmt.Sprintf("can't use ... for ops that have different aux types: %s and %s", op, eop));
                }
                ptr<Case> swc = addr(new Case(Expr:exprf("%s",op)));
                swc.add(stmtf("v.Op = %s", eop));
                swc.add(stmtf("return true"));
                sw.add(swc);
                continue;
            }
            swc = addr(new Case(Expr:exprf("%s",op)));
            swc.add(stmtf("return rewriteValue%s%s_%s(v)", arch.name, suff, op));
            sw.add(swc);
        }
        op = op__prev1;
    }

    if (len(sw.List) > 0) { // skip if empty
        fn.add(sw);

    }
    fn.add(stmtf("return false"));
    genFile.add(fn); 

    // Generate a routine per op. Note that we don't make one giant routine
    // because it is too big for some compilers.
    {
        var op__prev1 = op;

        foreach (var (_, __op) in ops) {
            op = __op;
            var rules = oprules[op];
            var (_, ok) = parseEllipsisRules(oprules[op], arch);
            if (ok) {
                continue;
            } 

            // rr is kept between iterations, so that each rule can check
            // that the previous rule wasn't unconditional.
            ptr<RuleRewrite> rr;
            fn = addr(new Func(Kind:"Value",Suffix:fmt.Sprintf("_%s",op),ArgLen:opByName(arch,op).argLength,));
            fn.add(declReserved("b", "v.Block"));
            fn.add(declReserved("config", "b.Func.Config"));
            fn.add(declReserved("fe", "b.Func.fe"));
            fn.add(declReserved("typ", "&b.Func.Config.Types"));
            {
                @string rule__prev2 = rule;

                foreach (var (_, __rule) in rules) {
                    rule = __rule;
                    if (rr != null && !rr.CanFail) {
                        log.Fatalf("unconditional rule %s is followed by other rules", rr.Match);
                    }
                    rr = addr(new RuleRewrite(Loc:rule.Loc));
                    rr.Match, rr.Cond, rr.Result = rule.parse();
                    var (pos, _) = genMatch(rr, arch, rr.Match, fn.ArgLen >= 0);
                    if (pos == "") {
                        pos = "v.Pos";
                    }
                    if (rr.Cond != "") {
                        rr.add(breakf("!(%s)", rr.Cond));
                    }
                    genResult(rr, arch, rr.Result, pos);
                    if (genLog.val) {
                        rr.add(stmtf("logRule(%q)", rule.Loc));
                    }
                    fn.add(rr);
                }

                rule = rule__prev2;
            }

            if (rr.CanFail) {
                fn.add(stmtf("return false"));
            }

            genFile.add(fn);

        }
        op = op__prev1;
    }

    fn = addr(new Func(Kind:"Block"));
    fn.add(declReserved("config", "b.Func.Config"));
    fn.add(declReserved("typ", "&b.Func.Config.Types"));

    sw = addr(new Switch(Expr:exprf("b.Kind")));
    ops = ops[..(int)0];
    {
        var op__prev1 = op;

        foreach (var (__op) in blockrules) {
            op = __op;
            ops = append(ops, op);
        }
        op = op__prev1;
    }

    sort.Strings(ops);
    {
        var op__prev1 = op;

        foreach (var (_, __op) in ops) {
            op = __op;
            var (name, data) = getBlockInfo(op, arch);
            swc = addr(new Case(Expr:exprf("%s",name)));
            {
                @string rule__prev2 = rule;

                foreach (var (_, __rule) in blockrules[op]) {
                    rule = __rule;
                    swc.add(genBlockRewrite(rule, arch, data));
                }

                rule = rule__prev2;
            }

            sw.add(swc);

        }
        op = op__prev1;
    }

    if (len(sw.List) > 0) { // skip if empty
        fn.add(sw);

    }
    fn.add(stmtf("return false"));
    genFile.add(fn); 

    // Remove unused imports and variables.
    ptr<object> buf = @new<bytes.Buffer>();
    fprint(buf, genFile);
    var fset = token.NewFileSet();
    var (file, err) = parser.ParseFile(fset, "", buf, parser.ParseComments);
    if (err != null) {
        var filename = fmt.Sprintf("%s_broken.go", arch.name);
        {
            var err__prev2 = err;

            err = ioutil.WriteFile(filename, buf.Bytes(), 0644);

            if (err != null) {
                log.Printf("failed to dump broken code to %s: %v", filename, err);
            }
            else
 {
                log.Printf("dumped broken code to %s", filename);
            }

            err = err__prev2;

        }

        log.Fatalf("failed to parse generated code for arch %s: %v", arch.name, err);

    }
    var tfile = fset.File(file.Pos()); 

    // First, use unusedInspector to find the unused declarations by their
    // start position.
    unusedInspector u = new unusedInspector(unused:make(map[token.Pos]bool));
    u.node(file); 

    // Then, delete said nodes via astutil.Apply.
    Func<ptr<astutil.Cursor>, bool> pre = c => {
        var node = c.Node();
        if (node == null) {
            return true;
        }
        if (u.unused[node.Pos()]) {
            c.Delete(); 
            // Unused imports and declarations use exactly
            // one line. Prevent leaving an empty line.
            tfile.MergeLine(tfile.Position(node.Pos()).Line);
            return false;

        }
        return true;

    };
    Func<ptr<astutil.Cursor>, bool> post = c => {
        switch (c.Node().type()) {
            case ptr<ast.GenDecl> node:
                if (len(node.Specs) == 0) { 
                    // Don't leave a broken or empty GenDecl behind,
                    // such as "import ()".
                    c.Delete();

                }

                break;
        }
        return true;

    };
    file = astutil.Apply(file, pre, post)._<ptr<ast.File>>(); 

    // Write the well-formatted source to file
    var (f, err) = os.Create("../rewrite" + arch.name + suff + ".go");
    if (err != null) {
        log.Fatalf("can't write output: %v", err);
    }
    defer(f.Close()); 
    // gofmt result; use a buffered writer, as otherwise go/format spends
    // far too much time in syscalls.
    var bw = bufio.NewWriter(f);
    {
        var err__prev1 = err;

        err = format.Node(bw, fset, file);

        if (err != null) {
            log.Fatalf("can't format output: %v", err);
        }
        err = err__prev1;

    }

    {
        var err__prev1 = err;

        err = bw.Flush();

        if (err != null) {
            log.Fatalf("can't write output: %v", err);
        }
        err = err__prev1;

    }

    {
        var err__prev1 = err;

        err = f.Close();

        if (err != null) {
            log.Fatalf("can't write output: %v", err);
        }
        err = err__prev1;

    }

});

// unusedInspector can be used to detect unused variables and imports in an
// ast.Node via its node method. The result is available in the "unused" map.
//
// note that unusedInspector is lazy and best-effort; it only supports the node
// types and patterns used by the rulegen program.
private partial struct unusedInspector {
    public ptr<scope> scope; // unused is the resulting set of unused declared names, indexed by the
// starting position of the node that declared the name.
    public map<token.Pos, bool> unused; // defining is the object currently being defined; this is useful so
// that if "foo := bar" is unused and removed, we can then detect if
// "bar" becomes unused as well.
    public ptr<object> defining;
}

// scoped opens a new scope when called, and returns a function which closes
// that same scope. When a scope is closed, unused variables are recorded.
private static Action scoped(this ptr<unusedInspector> _addr_u) {
    ref unusedInspector u = ref _addr_u.val;

    var outer = u.scope;
    u.scope = addr(new scope(outer:outer,objects:map[string]*object{}));
    return () => {
        {
            var anyUnused = true;

            while (anyUnused) {
                anyUnused = false;
                foreach (var (_, obj) in u.scope.objects) {
                    if (obj.numUses > 0) {
                        continue;
                    }
                    u.unused[obj.pos] = true;
                    foreach (var (_, used) in obj.used) {
                        used.numUses--;

                        if (used.numUses == 0) {
                            anyUnused = true;
                        }

                    } 
                    // We've decremented numUses for each of the
                    // objects in used. Zero this slice too, to keep
                    // everything consistent.
                    obj.used = null;

                }

            }

        }
        u.scope = outer;

    };

}

private static void exprs(this ptr<unusedInspector> _addr_u, slice<ast.Expr> list) {
    ref unusedInspector u = ref _addr_u.val;

    foreach (var (_, x) in list) {
        u.node(x);
    }
}

private static void node(this ptr<unusedInspector> _addr_u, ast.Node node) => func((defer, panic, _) => {
    ref unusedInspector u = ref _addr_u.val;

    switch (node.type()) {
        case ptr<ast.File> node:
            defer(u.scoped()());
            foreach (var (_, decl) in node.Decls) {
                u.node(decl);
            }
            break;
        case ptr<ast.GenDecl> node:
            foreach (var (_, spec) in node.Specs) {
                u.node(spec);
            }
            break;
        case ptr<ast.ImportSpec> node:
            var (impPath, _) = strconv.Unquote(node.Path.Value);
            var name = path.Base(impPath);
            u.scope.objects[name] = addr(new object(name:name,pos:node.Pos(),));
            break;
        case ptr<ast.FuncDecl> node:
            u.node(node.Type);
            if (node.Body != null) {
                u.node(node.Body);
            }
            break;
        case ptr<ast.FuncType> node:
            if (node.Params != null) {
                u.node(node.Params);
            }
            if (node.Results != null) {
                u.node(node.Results);
            }
            break;
        case ptr<ast.FieldList> node:
            foreach (var (_, field) in node.List) {
                u.node(field);
            }
            break;
        case ptr<ast.Field> node:
            u.node(node.Type); 

            // statements
            break;
        case ptr<ast.BlockStmt> node:
            defer(u.scoped()());
            {
                var stmt__prev1 = stmt;

                foreach (var (_, __stmt) in node.List) {
                    stmt = __stmt;
                    u.node(stmt);
                }

                stmt = stmt__prev1;
            }
            break;
        case ptr<ast.DeclStmt> node:
            u.node(node.Decl);
            break;
        case ptr<ast.IfStmt> node:
            if (node.Init != null) {
                u.node(node.Init);
            }
            u.node(node.Cond);
            u.node(node.Body);
            if (node.Else != null) {
                u.node(node.Else);
            }
            break;
        case ptr<ast.ForStmt> node:
            if (node.Init != null) {
                u.node(node.Init);
            }
            if (node.Cond != null) {
                u.node(node.Cond);
            }
            if (node.Post != null) {
                u.node(node.Post);
            }
            u.node(node.Body);
            break;
        case ptr<ast.SwitchStmt> node:
            if (node.Init != null) {
                u.node(node.Init);
            }
            if (node.Tag != null) {
                u.node(node.Tag);
            }
            u.node(node.Body);
            break;
        case ptr<ast.CaseClause> node:
            u.exprs(node.List);
            defer(u.scoped()());
            {
                var stmt__prev1 = stmt;

                foreach (var (_, __stmt) in node.Body) {
                    stmt = __stmt;
                    u.node(stmt);
                }

                stmt = stmt__prev1;
            }
            break;
        case ptr<ast.BranchStmt> node:
            break;
        case ptr<ast.ExprStmt> node:
            u.node(node.X);
            break;
        case ptr<ast.AssignStmt> node:
            if (node.Tok != token.DEFINE) {
                u.exprs(node.Rhs);
                u.exprs(node.Lhs);
                break;
            }
            var lhs = node.Lhs;
            if (len(lhs) == 2 && lhs[1]._<ptr<ast.Ident>>().Name == "_") {
                lhs = lhs[..(int)1];
            }
            if (len(lhs) != 1) {
                panic("no support for := with multiple names");
            }
            name = lhs[0]._<ptr<ast.Ident>>();
            ptr<object> obj = addr(new object(name:name.Name,pos:name.NamePos,));

            var old = u.defining;
            u.defining = obj;
            u.exprs(node.Rhs);
            u.defining = old;

            u.scope.objects[name.Name] = obj;
            break;
        case ptr<ast.ReturnStmt> node:
            u.exprs(node.Results);
            break;
        case ptr<ast.IncDecStmt> node:
            u.node(node.X); 

            // expressions
            break;
        case ptr<ast.CallExpr> node:
            u.node(node.Fun);
            u.exprs(node.Args);
            break;
        case ptr<ast.SelectorExpr> node:
            u.node(node.X);
            break;
        case ptr<ast.UnaryExpr> node:
            u.node(node.X);
            break;
        case ptr<ast.BinaryExpr> node:
            u.node(node.X);
            u.node(node.Y);
            break;
        case ptr<ast.StarExpr> node:
            u.node(node.X);
            break;
        case ptr<ast.ParenExpr> node:
            u.node(node.X);
            break;
        case ptr<ast.IndexExpr> node:
            u.node(node.X);
            u.node(node.Index);
            break;
        case ptr<ast.TypeAssertExpr> node:
            u.node(node.X);
            u.node(node.Type);
            break;
        case ptr<ast.Ident> node:
            {
                ptr<object> obj__prev1 = obj;

                obj = u.scope.Lookup(node.Name);

                if (obj != null) {
                    obj.numUses++;
                    if (u.defining != null) {
                        u.defining.used = append(u.defining.used, obj);
                    }
                }

                obj = obj__prev1;

            }

            break;
        case ptr<ast.BasicLit> node:
            break;
        case ptr<ast.ValueSpec> node:
            u.exprs(node.Values);
            break;
        default:
        {
            var node = node.type();
            panic(fmt.Sprintf("unhandled node: %T", node));
            break;
        }
    }

});

// scope keeps track of a certain scope and its declared names, as well as the
// outer (parent) scope.
private partial struct scope {
    public ptr<scope> outer; // can be nil, if this is the top-level scope
    public map<@string, ptr<object>> objects; // indexed by each declared name
}

private static ptr<object> Lookup(this ptr<scope> _addr_s, @string name) {
    ref scope s = ref _addr_s.val;

    {
        var obj = s.objects[name];

        if (obj != null) {
            return _addr_obj!;
        }
    }

    if (s.outer == null) {
        return _addr_null!;
    }
    return _addr_s.outer.Lookup(name)!;

}

// object keeps track of a declared name, such as a variable or import.
private partial struct @object {
    public @string name;
    public token.Pos pos; // start position of the node declaring the object

    public nint numUses; // number of times this object is used
    public slice<ptr<object>> used; // objects that its declaration makes use of
}

private static void fprint(io.Writer w, Node n) => func((_, panic, _) => {
    switch (n.type()) {
        case ptr<File> n:
            var file = n;
            var seenRewrite = make_map<array<@string>, @string>();
            fmt.Fprintf(w, "// Code generated from gen/%s%s.rules; DO NOT EDIT.\n", n.Arch.name, n.Suffix);
            fmt.Fprintf(w, "// generated with: cd gen; go run *.go\n");
            fmt.Fprintf(w, "\npackage ssa\n");
            foreach (var (_, path) in append(new slice<@string>(new @string[] { "fmt", "internal/buildcfg", "math", "cmd/internal/obj", "cmd/compile/internal/base", "cmd/compile/internal/types" }), n.Arch.imports)) {
                fmt.Fprintf(w, "import %q\n", path);
            }
            {
                var f__prev1 = f;

                foreach (var (_, __f) in n.List) {
                    f = __f;
                    ptr<Func> f = f._<ptr<Func>>();
                    fmt.Fprintf(w, "func rewrite%s%s%s%s(", f.Kind, n.Arch.name, n.Suffix, f.Suffix);
                    fmt.Fprintf(w, "%c *%s) bool {\n", strings.ToLower(f.Kind)[0], f.Kind);
                    if (f.Kind == "Value" && f.ArgLen > 0) {
                        {
                            var i__prev2 = i;

                            for (var i = f.ArgLen - 1; i >= 0; i--) {
                                fmt.Fprintf(w, "v_%d := v.Args[%d]\n", i, i);
                            }


                            i = i__prev2;
                        }

                    }

                    {
                        var n__prev2 = n;

                        foreach (var (_, __n) in f.List) {
                            n = __n;
                            fprint(w, n);

                            {
                                ptr<RuleRewrite> (rr, ok) = n._<ptr<RuleRewrite>>();

                                if (ok) {
                                    array<@string> k = new array<@string>(new @string[] { normalizeMatch(rr.Match,file.Arch), normalizeWhitespace(rr.Cond), normalizeWhitespace(rr.Result) });
                                    {
                                        var (prev, ok) = seenRewrite[k];

                                        if (ok) {
                                            log.Fatalf("duplicate rule %s, previously seen at %s\n", rr.Loc, prev);
                                        }

                                    }

                                    seenRewrite[k] = rr.Loc;

                                }

                            }

                        }

                        n = n__prev2;
                    }

                    fmt.Fprintf(w, "}\n");

                }

                f = f__prev1;
            }
            break;
        case ptr<Switch> n:
            fmt.Fprintf(w, "switch ");
            fprint(w, n.Expr);
            fmt.Fprintf(w, " {\n");
            {
                var n__prev1 = n;

                foreach (var (_, __n) in n.List) {
                    n = __n;
                    fprint(w, n);
                }

                n = n__prev1;
            }

            fmt.Fprintf(w, "}\n");
            break;
        case ptr<Case> n:
            fmt.Fprintf(w, "case ");
            fprint(w, n.Expr);
            fmt.Fprintf(w, ":\n");
            {
                var n__prev1 = n;

                foreach (var (_, __n) in n.List) {
                    n = __n;
                    fprint(w, n);
                }

                n = n__prev1;
            }
            break;
        case ptr<RuleRewrite> n:
            if (addLine.val) {
                fmt.Fprintf(w, "// %s\n", n.Loc);
            }
            fmt.Fprintf(w, "// match: %s\n", n.Match);
            if (n.Cond != "") {
                fmt.Fprintf(w, "// cond: %s\n", n.Cond);
            }
            fmt.Fprintf(w, "// result: %s\n", n.Result);
            fmt.Fprintf(w, "for %s {\n", n.Check);
            nint nCommutative = 0;
            {
                var n__prev1 = n;

                foreach (var (_, __n) in n.List) {
                    n = __n;
                    {
                        ptr<CondBreak> (b, ok) = n._<ptr<CondBreak>>();

                        if (ok) {
                            b.InsideCommuteLoop = nCommutative > 0;
                        }

                    }

                    fprint(w, n);
                    {
                        StartCommuteLoop (loop, ok) = n._<StartCommuteLoop>();

                        if (ok) {
                            if (nCommutative != loop.Depth) {
                                panic("mismatch commute loop depth");
                            }
                            nCommutative++;
                        }

                    }

                }

                n = n__prev1;
            }

            fmt.Fprintf(w, "return true\n");
            {
                var i__prev1 = i;

                for (i = 0; i < nCommutative; i++) {
                    fmt.Fprintln(w, "}");
                }


                i = i__prev1;
            }
            if (n.CommuteDepth > 0 && n.CanFail) {
                fmt.Fprint(w, "break\n");
            }

            fmt.Fprintf(w, "}\n");
            break;
        case ptr<Declare> n:
            fmt.Fprintf(w, "%s := ", n.Name);
            fprint(w, n.Value);
            fmt.Fprintln(w);
            break;
        case ptr<CondBreak> n:
            fmt.Fprintf(w, "if ");
            fprint(w, n.Cond);
            fmt.Fprintf(w, " {\n");
            if (n.InsideCommuteLoop) {
                fmt.Fprintf(w, "continue");
            }
            else
 {
                fmt.Fprintf(w, "break");
            }

            fmt.Fprintf(w, "\n}\n");
            break;
        case ast.Node n:
            printConfig.Fprint(w, emptyFset, n);
            {
                ast.Stmt (_, ok) = n._<ast.Stmt>();

                if (ok) {
                    fmt.Fprintln(w);
                }

            }

            break;
        case StartCommuteLoop n:
            fmt.Fprintf(w, "for _i%[1]d := 0; _i%[1]d <= 1; _i%[1]d, %[2]s_0, %[2]s_1 = _i%[1]d + 1, %[2]s_1, %[2]s_0 {\n", n.Depth, n.V);
            break;
        default:
        {
            var n = n.type();
            log.Fatalf("cannot print %T", n);
            break;
        }
    }

});

private static printer.Config printConfig = new printer.Config(Mode:printer.RawFormat,);

private static var emptyFset = token.NewFileSet();

// Node can be a Statement or an ast.Expr.
public partial interface Node {
}

// Statement can be one of our high-level statement struct types, or an
// ast.Stmt under some limited circumstances.
public partial interface Statement {
}

// BodyBase is shared by all of our statement pseudo-node types which can
// contain other statements.
public partial struct BodyBase {
    public slice<Statement> List;
    public bool CanFail;
}

private static void add(this ptr<BodyBase> _addr_w, Statement node) {
    ref BodyBase w = ref _addr_w.val;

    Statement last = default!;
    if (len(w.List) > 0) {
        last = Statement.As(w.List[len(w.List) - 1])!;
    }
    {
        ptr<CondBreak> (node, ok) = node._<ptr<CondBreak>>();

        if (ok) {
            w.CanFail = true;
            {
                Statement last__prev2 = last;

                ptr<CondBreak> (last, ok) = last._<ptr<CondBreak>>();

                if (ok) { 
                    // Add to the previous "if <cond> { break }" via a
                    // logical OR, which will save verbosity.
                    last.Cond = addr(new ast.BinaryExpr(Op:token.LOR,X:last.Cond,Y:node.Cond,));
                    return ;

                }

                last = last__prev2;

            }

        }
    }


    w.List = append(w.List, node);

}

// predeclared contains globally known tokens that should not be redefined.
private static map predeclared = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{"nil":true,"false":true,"true":true,};

// declared reports if the body contains a Declare with the given name.
private static bool declared(this ptr<BodyBase> _addr_w, @string name) {
    ref BodyBase w = ref _addr_w.val;

    if (predeclared[name]) { 
        // Treat predeclared names as having already been declared.
        // This lets us use nil to match an aux field or
        // true and false to match an auxint field.
        return true;

    }
    foreach (var (_, s) in w.List) {
        {
            ptr<Declare> (decl, ok) = s._<ptr<Declare>>();

            if (ok && decl.Name == name) {
                return true;
            }

        }

    }    return false;

}

// These types define some high-level statement struct types, which can be used
// as a Statement. This allows us to keep some node structs simpler, and have
// higher-level nodes such as an entire rule rewrite.
//
// Note that ast.Expr is always used as-is; we don't declare our own expression
// nodes.
public partial struct File {
    public ref BodyBase BodyBase => ref BodyBase_val; // []*Func
    public arch Arch;
    public @string Suffix;
}
public partial struct Func {
    public ref BodyBase BodyBase => ref BodyBase_val;
    public @string Kind; // "Value" or "Block"
    public @string Suffix;
    public int ArgLen; // if kind == "Value", number of args for this op
}
public partial struct Switch {
    public ref BodyBase BodyBase => ref BodyBase_val; // []*Case
    public ast.Expr Expr;
}
public partial struct Case {
    public ref BodyBase BodyBase => ref BodyBase_val;
    public ast.Expr Expr;
}
public partial struct RuleRewrite {
    public ref BodyBase BodyBase => ref BodyBase_val;
    public @string Match; // top comments
    public @string Cond; // top comments
    public @string Result; // top comments
    public @string Check; // top-level boolean expression

    public nint Alloc; // for unique var names
    public @string Loc; // file name & line number of the original rule
    public nint CommuteDepth; // used to track depth of commute loops
}
public partial struct Declare {
    public @string Name;
    public ast.Expr Value;
}
public partial struct CondBreak {
    public ast.Expr Cond;
    public bool InsideCommuteLoop;
}
public partial struct StartCommuteLoop {
    public nint Depth;
    public @string V;
}
private static ast.Expr exprf(@string format, params object[] a) {
    a = a.Clone();

    var src = fmt.Sprintf(format, a);
    var (expr, err) = parser.ParseExpr(src);
    if (err != null) {
        log.Fatalf("expr parse error on %q: %v", src, err);
    }
    return expr;

}

// stmtf parses a Go statement generated from fmt.Sprintf. This function is only
// meant for simple statements that don't have a custom Statement node declared
// in this package, such as ast.ReturnStmt or ast.ExprStmt.
private static Statement stmtf(@string format, params object[] a) {
    a = a.Clone();

    var src = fmt.Sprintf(format, a);
    @string fsrc = "package p\nfunc _() {\n" + src + "\n}\n";
    var (file, err) = parser.ParseFile(token.NewFileSet(), "", fsrc, 0);
    if (err != null) {
        log.Fatalf("stmt parse error on %q: %v", src, err);
    }
    return file.Decls[0]._<ptr<ast.FuncDecl>>().Body.List[0];

}

private static map reservedNames = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{"v":true,"b":true,"config":true,"fe":true,"typ":true,};

// declf constructs a simple "name := value" declaration,
// using exprf for its value.
//
// name must not be one of reservedNames.
// This helps prevent unintended shadowing and name clashes.
// To declare a reserved name, use declReserved.
private static ptr<Declare> declf(@string loc, @string name, @string format, params object[] a) {
    a = a.Clone();

    if (reservedNames[name]) {
        log.Fatalf("rule %s uses the reserved name %s", loc, name);
    }
    return addr(new Declare(name,exprf(format,a...)));

}

// declReserved is like declf, but the name must be one of reservedNames.
// Calls to declReserved should generally be static and top-level.
private static ptr<Declare> declReserved(@string name, @string value) => func((_, panic, _) => {
    if (!reservedNames[name]) {
        panic(fmt.Sprintf("declReserved call does not use a reserved name: %q", name));
    }
    return addr(new Declare(name,exprf(value)));

});

// breakf constructs a simple "if cond { break }" statement, using exprf for its
// condition.
private static ptr<CondBreak> breakf(@string format, params object[] a) {
    a = a.Clone();

    return addr(new CondBreak(Cond:exprf(format,a...)));
}

private static ptr<RuleRewrite> genBlockRewrite(Rule rule, arch arch, blockData data) {
    ptr<RuleRewrite> rr = addr(new RuleRewrite(Loc:rule.Loc));
    rr.Match, rr.Cond, rr.Result = rule.parse();
    var (_, _, auxint, aux, s) = extract(rr.Match); // remove parens, then split

    // check match of control values
    if (len(s) < data.controls) {
        log.Fatalf("incorrect number of arguments in %s, got %v wanted at least %v", rule, len(s), data.controls);
    }
    var controls = s[..(int)data.controls];
    var pos = make_slice<@string>(data.controls);
    {
        var i__prev1 = i;

        foreach (var (__i, __arg) in controls) {
            i = __i;
            arg = __arg;
            var cname = fmt.Sprintf("b.Controls[%v]", i);
            if (strings.Contains(arg, "(")) {
                var (vname, expr) = splitNameExpr(arg);
                if (vname == "") {
                    vname = fmt.Sprintf("v_%v", i);
                }
                rr.add(declf(rr.Loc, vname, cname));
                var (p, op) = genMatch0(rr, arch, expr, vname, null, false); // TODO: pass non-nil cnt?
                if (op != "") {
                    var check = fmt.Sprintf("%s.Op == %s", cname, op);
                    if (rr.Check == "") {
                        rr.Check = check;
                    }
                    else
 {
                        rr.Check += " && " + check;
                    }

                }

                if (p == "") {
                    p = vname + ".Pos";
                }

                pos[i] = p;

            }
            else
 {
                rr.add(declf(rr.Loc, arg, cname));
                pos[i] = arg + ".Pos";
            }

        }
        i = i__prev1;
    }

    foreach (var (_, e) in true) {
        if (e.name == "") {
            continue;
        }
        if (e.dclType == "") {
            log.Fatalf("op %s has no declared type for %s", data.name, e.field);
        }
        if (!token.IsIdentifier(e.name) || rr.declared(e.name)) {
            rr.add(breakf("%sTo%s(b.%s) != %s", unTitle(e.field), title(e.dclType), e.field, e.name));
        }
        else
 {
            rr.add(declf(rr.Loc, e.name, "%sTo%s(b.%s)", unTitle(e.field), title(e.dclType), e.field));
        }
    }    if (rr.Cond != "") {
        rr.add(breakf("!(%s)", rr.Cond));
    }
    var (outop, _, auxint, aux, t) = extract(rr.Result); // remove parens, then split
    var (blockName, outdata) = getBlockInfo(outop, arch);
    if (len(t) < outdata.controls) {
        log.Fatalf("incorrect number of output arguments in %s, got %v wanted at least %v", rule, len(s), outdata.controls);
    }
    var succs = s[(int)data.controls..];
    var newsuccs = t[(int)outdata.controls..];
    map m = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{};
    {
        var succ__prev1 = succ;

        foreach (var (_, __succ) in succs) {
            succ = __succ;
            if (m[succ]) {
                log.Fatalf("can't have a repeat successor name %s in %s", succ, rule);
            }
            m[succ] = true;
        }
        succ = succ__prev1;
    }

    {
        var succ__prev1 = succ;

        foreach (var (_, __succ) in newsuccs) {
            succ = __succ;
            if (!m[succ]) {
                log.Fatalf("unknown successor %s in %s", succ, rule);
            }
            delete(m, succ);
        }
        succ = succ__prev1;
    }

    if (len(m) != 0) {
        log.Fatalf("unmatched successors %v in %s", m, rule);
    }
    array<@string> genControls = new array<@string>(2);
    {
        var i__prev1 = i;

        foreach (var (__i, __control) in t[..(int)outdata.controls]) {
            i = __i;
            control = __control; 
            // Select a source position for any new control values.
            // TODO: does it always make sense to use the source position
            // of the original control values or should we be using the
            // block's source position in some cases?
            @string newpos = "b.Pos"; // default to block's source position
            if (i < len(pos) && pos[i] != "") { 
                // Use the previous control value's source position.
                newpos = pos[i];

            } 

            // Generate a new control value (or copy an existing value).
            genControls[i] = genResult0(rr, arch, control, false, false, newpos, null);

        }
        i = i__prev1;
    }

    switch (outdata.controls) {
        case 0: 
            rr.add(stmtf("b.Reset(%s)", blockName));
            break;
        case 1: 
            rr.add(stmtf("b.resetWithControl(%s, %s)", blockName, genControls[0]));
            break;
        case 2: 
            rr.add(stmtf("b.resetWithControl2(%s, %s, %s)", blockName, genControls[0], genControls[1]));
            break;
        default: 
            log.Fatalf("too many controls: %d", outdata.controls);
            break;
    }

    if (auxint != "") { 
        // Make sure auxint value has the right type.
        rr.add(stmtf("b.AuxInt = %sToAuxInt(%s)", unTitle(outdata.auxIntType()), auxint));

    }
    if (aux != "") { 
        // Make sure aux value has the right type.
        rr.add(stmtf("b.Aux = %sToAux(%s)", unTitle(outdata.auxType()), aux));

    }
    var succChanged = false;
    {
        var i__prev1 = i;

        for (nint i = 0; i < len(succs); i++) {
            if (succs[i] != newsuccs[i]) {
                succChanged = true;
            }
        }

        i = i__prev1;
    }
    if (succChanged) {
        if (len(succs) != 2) {
            log.Fatalf("changed successors, len!=2 in %s", rule);
        }
        if (succs[0] != newsuccs[1] || succs[1] != newsuccs[0]) {
            log.Fatalf("can only handle swapped successors in %s", rule);
        }
        rr.add(stmtf("b.swapSuccessors()"));

    }
    if (genLog.val) {
        rr.add(stmtf("logRule(%q)", rule.Loc));
    }
    return _addr_rr!;

}

// genMatch returns the variable whose source position should be used for the
// result (or "" if no opinion), and a boolean that reports whether the match can fail.
private static (@string, @string) genMatch(ptr<RuleRewrite> _addr_rr, arch arch, @string match, bool pregenTop) {
    @string pos = default;
    @string checkOp = default;
    ref RuleRewrite rr = ref _addr_rr.val;

    var cnt = varCount(_addr_rr);
    return genMatch0(_addr_rr, arch, match, "v", cnt, pregenTop);
}

private static (@string, @string) genMatch0(ptr<RuleRewrite> _addr_rr, arch arch, @string match, @string v, map<@string, nint> cnt, bool pregenTop) {
    @string pos = default;
    @string checkOp = default;
    ref RuleRewrite rr = ref _addr_rr.val;

    if (match[0] != '(' || match[len(match) - 1] != ')') {
        log.Fatalf("%s: non-compound expr in genMatch0: %q", rr.Loc, match);
    }
    var (op, oparch, typ, auxint, aux, args) = parseValue(match, arch, rr.Loc);

    checkOp = fmt.Sprintf("Op%s%s", oparch, op.name);

    if (op.faultOnNilArg0 || op.faultOnNilArg1) { 
        // Prefer the position of an instruction which could fault.
        pos = v + ".Pos";

    }
    if (op.argLength == -1) {
        var l = len(args);
        if (l == 0 || args[l - 1] != "___") {
            rr.add(breakf("len(%s.Args) != %d", v, l));
        }
        else if (l > 1 && args[l - 1] == "___") {
            rr.add(breakf("len(%s.Args) < %d", v, l - 1));
        }
    }
    foreach (var (_, e) in true) {
        if (e.name == "") {
            continue;
        }
        if (e.dclType == "") {
            log.Fatalf("op %s has no declared type for %s", op.name, e.field);
        }
        if (!token.IsIdentifier(e.name) || rr.declared(e.name)) {
            switch (e.field) {
                case "Aux": 
                    rr.add(breakf("auxTo%s(%s.%s) != %s", title(e.dclType), v, e.field, e.name));
                    break;
                case "AuxInt": 
                    rr.add(breakf("auxIntTo%s(%s.%s) != %s", title(e.dclType), v, e.field, e.name));
                    break;
                case "Type": 
                    rr.add(breakf("%s.%s != %s", v, e.field, e.name));
                    break;
            }

        }
        else
 {
            switch (e.field) {
                case "Aux": 
                    rr.add(declf(rr.Loc, e.name, "auxTo%s(%s.%s)", title(e.dclType), v, e.field));
                    break;
                case "AuxInt": 
                    rr.add(declf(rr.Loc, e.name, "auxIntTo%s(%s.%s)", title(e.dclType), v, e.field));
                    break;
                case "Type": 
                    rr.add(declf(rr.Loc, e.name, "%s.%s", v, e.field));
                    break;
            }

        }
    }    var commutative = op.commutative;
    if (commutative) {
        if (args[0] == args[1]) { 
            // When we have (Add x x), for any x,
            // even if there are other uses of x besides these two,
            // and even if x is not a variable,
            // we can skip the commutative match.
            commutative = false;

        }
        if (cnt[args[0]] == 1 && cnt[args[1]] == 1) { 
            // When we have (Add x y) with no other uses
            // of x and y in the matching rule and condition,
            // then we can skip the commutative match (Add y x).
            commutative = false;

        }
    }
    if (!pregenTop) { 
        // Access last argument first to minimize bounds checks.
        for (var n = len(args) - 1; n > 0; n--) {
            var a = args[n];
            if (a == "_") {
                continue;
            }
            if (!rr.declared(a) && token.IsIdentifier(a) && !(commutative && len(args) == 2)) {
                rr.add(declf(rr.Loc, a, "%s.Args[%d]", v, n)); 
                // delete the last argument so it is not reprocessed
                args = args[..(int)n];

            }
            else
 {
                rr.add(stmtf("_ = %s.Args[%d]", v, n));
            }

            break;

        }

    }
    if (commutative && !pregenTop) {
        {
            nint i__prev1 = i;

            for (nint i = 0; i <= 1; i++) {
                var vname = fmt.Sprintf("%s_%d", v, i);
                rr.add(declf(rr.Loc, vname, "%s.Args[%d]", v, i));
            }


            i = i__prev1;
        }

    }
    if (commutative) {
        rr.add(new StartCommuteLoop(rr.CommuteDepth,v));
        rr.CommuteDepth++;
    }
    {
        nint i__prev1 = i;

        foreach (var (__i, __arg) in args) {
            i = __i;
            arg = __arg;
            if (arg == "_") {
                continue;
            }
            @string rhs = default;
            if ((commutative && i < 2) || pregenTop) {
                rhs = fmt.Sprintf("%s_%d", v, i);
            }
            else
 {
                rhs = fmt.Sprintf("%s.Args[%d]", v, i);
            }

            if (!strings.Contains(arg, "(")) { 
                // leaf variable
                if (rr.declared(arg)) { 
                    // variable already has a definition. Check whether
                    // the old definition and the new definition match.
                    // For example, (add x x).  Equality is just pointer equality
                    // on Values (so cse is important to do before lowering).
                    rr.add(breakf("%s != %s", arg, rhs));

                }
                else
 {
                    if (arg != rhs) {
                        rr.add(declf(rr.Loc, arg, "%s", rhs));
                    }
                }

                continue;

            } 
            // compound sexpr
            var (argname, expr) = splitNameExpr(arg);
            if (argname == "") {
                argname = fmt.Sprintf("%s_%d", v, i);
            }

            if (argname == "b") {
                log.Fatalf("don't name args 'b', it is ambiguous with blocks");
            }

            if (argname != rhs) {
                rr.add(declf(rr.Loc, argname, "%s", rhs));
            }

            var bexpr = exprf("%s.Op != addLater", argname);
            rr.add(addr(new CondBreak(Cond:bexpr)));
            var (argPos, argCheckOp) = genMatch0(_addr_rr, arch, expr, argname, cnt, false);
            bexpr._<ptr<ast.BinaryExpr>>().Y._<ptr<ast.Ident>>().Name = argCheckOp;

            if (argPos != "") { 
                // Keep the argument in preference to the parent, as the
                // argument is normally earlier in program flow.
                // Keep the argument in preference to an earlier argument,
                // as that prefers the memory argument which is also earlier
                // in the program flow.
                pos = argPos;

            }

        }
        i = i__prev1;
    }

    return (pos, checkOp);

}

private static void genResult(ptr<RuleRewrite> _addr_rr, arch arch, @string result, @string pos) {
    ref RuleRewrite rr = ref _addr_rr.val;

    var move = result[0] == '@';
    if (move) { 
        // parse @block directive
        var s = strings.SplitN(result[(int)1..], " ", 2);
        rr.add(stmtf("b = %s", s[0]));
        result = s[1];

    }
    var cse = make_map<@string, @string>();
    genResult0(_addr_rr, arch, result, true, move, pos, cse);

}

private static @string genResult0(ptr<RuleRewrite> _addr_rr, arch arch, @string result, bool top, bool move, @string pos, map<@string, @string> cse) {
    ref RuleRewrite rr = ref _addr_rr.val;

    var (resname, expr) = splitNameExpr(result);
    result = expr; 
    // TODO: when generating a constant result, use f.constVal to avoid
    // introducing copies just to clean them up again.
    if (result[0] != '(') { 
        // variable
        if (top) { 
            // It in not safe in general to move a variable between blocks
            // (and particularly not a phi node).
            // Introduce a copy.
            rr.add(stmtf("v.copyOf(%s)", result));

        }
        return result;

    }
    var w = normalizeWhitespace(result);
    {
        var prev = cse[w];

        if (prev != "") {
            return prev;
        }
    }


    var (op, oparch, typ, auxint, aux, args) = parseValue(result, arch, rr.Loc); 

    // Find the type of the variable.
    var typeOverride = typ != "";
    if (typ == "" && op.typ != "") {
        typ = typeName(op.typ);
    }
    @string v = "v";
    if (top && !move) {
        rr.add(stmtf("v.reset(Op%s%s)", oparch, op.name));
        if (typeOverride) {
            rr.add(stmtf("v.Type = %s", typ));
        }
    }
    else
 {
        if (typ == "") {
            log.Fatalf("sub-expression %s (op=Op%s%s) at %s must have a type", result, oparch, op.name, rr.Loc);
        }
        if (resname == "") {
            v = fmt.Sprintf("v%d", rr.Alloc);
        }
        else
 {
            v = resname;
        }
        rr.Alloc++;
        rr.add(declf(rr.Loc, v, "b.NewValue0(%s, Op%s%s, %s)", pos, oparch, op.name, typ));
        if (move && top) { 
            // Rewrite original into a copy
            rr.add(stmtf("v.copyOf(%s)", v));

        }
    }
    if (auxint != "") { 
        // Make sure auxint value has the right type.
        rr.add(stmtf("%s.AuxInt = %sToAuxInt(%s)", v, unTitle(op.auxIntType()), auxint));

    }
    if (aux != "") { 
        // Make sure aux value has the right type.
        rr.add(stmtf("%s.Aux = %sToAux(%s)", v, unTitle(op.auxType()), aux));

    }
    ptr<object> all = @new<strings.Builder>();
    foreach (var (i, arg) in args) {
        var x = genResult0(_addr_rr, arch, arg, false, move, pos, cse);
        if (i > 0) {
            all.WriteString(", ");
        }
        all.WriteString(x);

    }    switch (len(args)) {
        case 0: 

            break;
        case 1: 
            rr.add(stmtf("%s.AddArg(%s)", v, all.String()));
            break;
        default: 
            rr.add(stmtf("%s.AddArg%d(%s)", v, len(args), all.String()));
            break;
    }

    if (cse != null) {
        cse[w] = v;
    }
    return v;

}

private static slice<@string> split(@string s) {
    slice<@string> r = default;

outer:
    while (s != "") {
        nint d = 0; // depth of ({[<
        byte open = default;        byte close = default; // opening and closing markers ({[< or )}]>
 // opening and closing markers ({[< or )}]>
        var nonsp = false; // found a non-space char so far
        for (nint i = 0; i < len(s); i++) {

            if (d == 0 && s[i] == '(') 
                (open, close) = ('(', ')');                d++;
            else if (d == 0 && s[i] == '<') 
                (open, close) = ('<', '>');                d++;
            else if (d == 0 && s[i] == '[') 
                (open, close) = ('[', ']');                d++;
            else if (d == 0 && s[i] == '{') 
                (open, close) = ('{', '}');                d++;
            else if (d == 0 && (s[i] == ' ' || s[i] == '\t')) 
                if (nonsp) {
                    r = append(r, strings.TrimSpace(s[..(int)i]));
                    s = s[(int)i..];
                    _continueouter = true;
                    break;
                }

            else if (d > 0 && s[i] == open) 
                d++;
            else if (d > 0 && s[i] == close) 
                d--;
            else 
                nonsp = true;
            
        }
        if (d != 0) {
            log.Fatalf("imbalanced expression: %q", s);
        }
        if (nonsp) {
            r = append(r, strings.TrimSpace(s));
        }
        break;

    }
    return r;

}

// isBlock reports whether this op is a block opcode.
private static bool isBlock(@string name, arch arch) {
    {
        var b__prev1 = b;

        foreach (var (_, __b) in genericBlocks) {
            b = __b;
            if (b.name == name) {
                return true;
            }
        }
        b = b__prev1;
    }

    {
        var b__prev1 = b;

        foreach (var (_, __b) in arch.blocks) {
            b = __b;
            if (b.name == name) {
                return true;
            }
        }
        b = b__prev1;
    }

    return false;

}

private static (@string, @string, @string, @string, slice<@string>) extract(@string val) {
    @string op = default;
    @string typ = default;
    @string auxint = default;
    @string aux = default;
    slice<@string> args = default;

    val = val[(int)1..(int)len(val) - 1]; // remove ()

    // Split val up into regions.
    // Split by spaces/tabs, except those contained in (), {}, [], or <>.
    var s = split(val); 

    // Extract restrictions and args.
    op = s[0];
    foreach (var (_, a) in s[(int)1..]) {
        switch (a[0]) {
            case '<': 
                typ = a[(int)1..(int)len(a) - 1]; // remove <>
                break;
            case '[': 
                auxint = a[(int)1..(int)len(a) - 1]; // remove []
                break;
            case '{': 
                aux = a[(int)1..(int)len(a) - 1]; // remove {}
                break;
            default: 
                args = append(args, a);
                break;
        }

    }    return ;

}

// parseValue parses a parenthesized value from a rule.
// The value can be from the match or the result side.
// It returns the op and unparsed strings for typ, auxint, and aux restrictions and for all args.
// oparch is the architecture that op is located in, or "" for generic.
private static (opData, @string, @string, @string, @string, slice<@string>) parseValue(@string val, arch arch, @string loc) {
    opData op = default;
    @string oparch = default;
    @string typ = default;
    @string auxint = default;
    @string aux = default;
    slice<@string> args = default;
 
    // Resolve the op.
    @string s = default;
    s, typ, auxint, aux, args = extract(val); 

    // match reports whether x is a good op to select.
    // If strict is true, rule generation might succeed.
    // If strict is false, rule generation has failed,
    // but we're trying to generate a useful error.
    // Doing strict=true then strict=false allows
    // precise op matching while retaining good error messages.
    Func<opData, bool, @string, bool> match = (x, strict, archname) => {
        if (x.name != s) {
            return false;
        }
        if (x.argLength != -1 && int(x.argLength) != len(args) && (len(args) != 1 || args[0] != "...")) {
            if (strict) {
                return false;
            }
            log.Printf("%s: op %s (%s) should have %d args, has %d", loc, s, archname, x.argLength, len(args));
        }
        return true;

    };

    {
        var x__prev1 = x;

        foreach (var (_, __x) in genericOps) {
            x = __x;
            if (match(x, true, "generic")) {
                op = x;
                break;
            }
        }
        x = x__prev1;
    }

    {
        var x__prev1 = x;

        foreach (var (_, __x) in arch.ops) {
            x = __x;
            if (arch.name != "generic" && match(x, true, arch.name)) {
                if (op.name != "") {
                    log.Fatalf("%s: matches for op %s found in both generic and %s", loc, op.name, arch.name);
                }
                op = x;
                oparch = arch.name;
                break;
            }
        }
        x = x__prev1;
    }

    if (op.name == "") { 
        // Failed to find the op.
        // Run through everything again with strict=false
        // to generate useful diagnosic messages before failing.
        {
            var x__prev1 = x;

            foreach (var (_, __x) in genericOps) {
                x = __x;
                match(x, false, "generic");
            }

            x = x__prev1;
        }

        {
            var x__prev1 = x;

            foreach (var (_, __x) in arch.ops) {
                x = __x;
                match(x, false, arch.name);
            }

            x = x__prev1;
        }

        log.Fatalf("%s: unknown op %s", loc, s);

    }
    if (auxint != "" && !opHasAuxInt(op)) {
        log.Fatalf("%s: op %s %s can't have auxint", loc, op.name, op.aux);
    }
    if (aux != "" && !opHasAux(op)) {
        log.Fatalf("%s: op %s %s can't have aux", loc, op.name, op.aux);
    }
    return ;

}

private static bool opHasAuxInt(opData op) {
    switch (op.aux) {
        case "Bool": 

        case "Int8": 

        case "Int16": 

        case "Int32": 

        case "Int64": 

        case "Int128": 

        case "UInt8": 

        case "Float32": 

        case "Float64": 

        case "SymOff": 

        case "CallOff": 

        case "SymValAndOff": 

        case "TypSize": 

        case "ARM64BitField": 

        case "FlagConstant": 

        case "CCop": 
            return true;
            break;
    }
    return false;

}

private static bool opHasAux(opData op) {
    switch (op.aux) {
        case "String": 

        case "Sym": 

        case "SymOff": 

        case "Call": 

        case "CallOff": 

        case "SymValAndOff": 

        case "Typ": 

        case "TypSize": 

        case "S390XCCMask": 

        case "S390XRotateParams": 
            return true;
            break;
    }
    return false;

}

// splitNameExpr splits s-expr arg, possibly prefixed by "name:",
// into name and the unprefixed expression.
// For example, "x:(Foo)" yields "x", "(Foo)",
// and "(Foo)" yields "", "(Foo)".
private static (@string, @string) splitNameExpr(@string arg) {
    @string name = default;
    @string expr = default;

    var colon = strings.Index(arg, ":");
    if (colon < 0) {
        return ("", arg);
    }
    var openparen = strings.Index(arg, "(");
    if (openparen < 0) {
        log.Fatalf("splitNameExpr(%q): colon but no open parens", arg);
    }
    if (colon > openparen) { 
        // colon is inside the parens, such as in "(Foo x:(Bar))".
        return ("", arg);

    }
    return (arg[..(int)colon], arg[(int)colon + 1..]);

}

private static (@string, blockData) getBlockInfo(@string op, arch arch) => func((_, panic, _) => {
    @string name = default;
    blockData data = default;

    {
        var b__prev1 = b;

        foreach (var (_, __b) in genericBlocks) {
            b = __b;
            if (b.name == op) {
                return ("Block" + op, b);
            }
        }
        b = b__prev1;
    }

    {
        var b__prev1 = b;

        foreach (var (_, __b) in arch.blocks) {
            b = __b;
            if (b.name == op) {
                return ("Block" + arch.name + op, b);
            }
        }
        b = b__prev1;
    }

    log.Fatalf("could not find block data for %s", op);
    panic("unreachable");

});

// typeName returns the string to use to generate a type.
private static @string typeName(@string typ) {
    if (typ[0] == '(') {
        var ts = strings.Split(typ[(int)1..(int)len(typ) - 1], ",");
        if (len(ts) != 2) {
            log.Fatalf("Tuple expect 2 arguments");
        }
        return "types.NewTuple(" + typeName(ts[0]) + ", " + typeName(ts[1]) + ")";

    }
    switch (typ) {
        case "Flags": 

        case "Mem": 

        case "Void": 

        case "Int128": 
            return "types.Type" + typ;
            break;
        default: 
            return "typ." + typ;
            break;
    }

}

// balance returns the number of unclosed '(' characters in s.
// If a ')' appears without a corresponding '(', balance returns -1.
private static nint balance(@string s) {
    nint balance = 0;
    foreach (var (_, c) in s) {
        switch (c) {
            case '(': 
                balance++;
                break;
            case ')': 
                balance--;
                if (balance < 0) { 
                    // don't allow ")(" to return 0
                    return -1;

                }

                break;
        }

    }    return balance;

}

// findAllOpcode is a function to find the opcode portion of s-expressions.
private static var findAllOpcode = regexp.MustCompile("[(](\\w+[|])+\\w+[)]").FindAllStringIndex;

// excludeFromExpansion reports whether the substring s[idx[0]:idx[1]] in a rule
// should be disregarded as a candidate for | expansion.
// It uses simple syntactic checks to see whether the substring
// is inside an AuxInt expression or inside the && conditions.
private static bool excludeFromExpansion(@string s, slice<nint> idx) {
    var left = s[..(int)idx[0]];
    if (strings.LastIndexByte(left, '[') > strings.LastIndexByte(left, ']')) { 
        // Inside an AuxInt expression.
        return true;

    }
    var right = s[(int)idx[1]..];
    if (strings.Contains(left, "&&") && strings.Contains(right, "=>")) { 
        // Inside && conditions.
        return true;

    }
    return false;

}

// expandOr converts a rule into multiple rules by expanding | ops.
private static slice<@string> expandOr(@string r) { 
    // Find every occurrence of |-separated things.
    // They look like MOV(B|W|L|Q|SS|SD)load or MOV(Q|L)loadidx(1|8).
    // Generate rules selecting one case from each |-form.

    // Count width of |-forms.  They must match.
    nint n = 1;
    {
        var idx__prev1 = idx;

        foreach (var (_, __idx) in findAllOpcode(r, -1)) {
            idx = __idx;
            if (excludeFromExpansion(r, idx)) {
                continue;
            }
            var s = r[(int)idx[0]..(int)idx[1]];
            var c = strings.Count(s, "|") + 1;
            if (c == 1) {
                continue;
            }
            if (n > 1 && n != c) {
                log.Fatalf("'|' count doesn't match in %s: both %d and %d\n", r, n, c);
            }
            n = c;
        }
        idx = idx__prev1;
    }

    if (n == 1) { 
        // No |-form in this rule.
        return new slice<@string>(new @string[] { r });

    }
    var res = make_slice<@string>(n);
    for (nint i = 0; i < n; i++) {
        ptr<object> buf = @new<strings.Builder>();
        nint x = 0;
        {
            var idx__prev2 = idx;

            foreach (var (_, __idx) in findAllOpcode(r, -1)) {
                idx = __idx;
                if (excludeFromExpansion(r, idx)) {
                    continue;
                }
                buf.WriteString(r[(int)x..(int)idx[0]]); // write bytes we've skipped over so far
                s = r[(int)idx[0] + 1..(int)idx[1] - 1]; // remove leading "(" and trailing ")"
                buf.WriteString(strings.Split(s, "|")[i]); // write the op component for this rule
                x = idx[1]; // note that we've written more bytes
            }

            idx = idx__prev2;
        }

        buf.WriteString(r[(int)x..]);
        res[i] = buf.String();

    }
    return res;

}

// varCount returns a map which counts the number of occurrences of
// Value variables in the s-expression rr.Match and the Go expression rr.Cond.
private static map<@string, nint> varCount(ptr<RuleRewrite> _addr_rr) {
    ref RuleRewrite rr = ref _addr_rr.val;

    map cnt = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, nint>{};
    varCount1(rr.Loc, rr.Match, cnt);
    if (rr.Cond != "") {
        var (expr, err) = parser.ParseExpr(rr.Cond);
        if (err != null) {
            log.Fatalf("%s: failed to parse cond %q: %v", rr.Loc, rr.Cond, err);
        }
        ast.Inspect(expr, n => {
            {
                ptr<ast.Ident> (id, ok) = n._<ptr<ast.Ident>>();

                if (ok) {
                    cnt[id.Name]++;
                }

            }

            return true;

        });

    }
    return cnt;

}

private static void varCount1(@string loc, @string m, map<@string, nint> cnt) {
    if (m[0] == '<' || m[0] == '[' || m[0] == '{') {
        return ;
    }
    if (token.IsIdentifier(m)) {
        cnt[m]++;
        return ;
    }
    var (name, expr) = splitNameExpr(m);
    if (name != "") {
        cnt[name]++;
    }
    if (expr[0] != '(' || expr[len(expr) - 1] != ')') {
        log.Fatalf("%s: non-compound expr in varCount1: %q", loc, expr);
    }
    var s = split(expr[(int)1..(int)len(expr) - 1]);
    foreach (var (_, arg) in s[(int)1..]) {
        varCount1(loc, arg, cnt);
    }
}

// normalizeWhitespace replaces 2+ whitespace sequences with a single space.
private static @string normalizeWhitespace(@string x) {
    x = strings.Join(strings.Fields(x), " ");
    x = strings.Replace(x, "( ", "(", -1);
    x = strings.Replace(x, " )", ")", -1);
    x = strings.Replace(x, "[ ", "[", -1);
    x = strings.Replace(x, " ]", "]", -1);
    x = strings.Replace(x, ")=>", ") =>", -1);
    return x;
}

// opIsCommutative reports whether op s is commutative.
private static bool opIsCommutative(@string op, arch arch) {
    {
        var x__prev1 = x;

        foreach (var (_, __x) in genericOps) {
            x = __x;
            if (op == x.name) {
                if (x.commutative) {
                    return true;
                }
                break;
            }
        }
        x = x__prev1;
    }

    if (arch.name != "generic") {
        {
            var x__prev1 = x;

            foreach (var (_, __x) in arch.ops) {
                x = __x;
                if (op == x.name) {
                    if (x.commutative) {
                        return true;
                    }
                    break;
                }
            }

            x = x__prev1;
        }
    }
    return false;

}

private static @string normalizeMatch(@string m, arch arch) {
    if (token.IsIdentifier(m)) {
        return m;
    }
    var (op, typ, auxint, aux, args) = extract(m);
    if (opIsCommutative(op, arch)) {
        if (args[1] < args[0]) {
            (args[0], args[1]) = (args[1], args[0]);
        }
    }
    ptr<object> s = @new<strings.Builder>();
    fmt.Fprintf(s, "%s <%s> [%s] {%s}", op, typ, auxint, aux);
    foreach (var (_, arg) in args) {
        var (prefix, expr) = splitNameExpr(arg);
        fmt.Fprint(s, " ", prefix, normalizeMatch(expr, arch));
    }    return s.String();

}

private static (@string, bool) parseEllipsisRules(slice<Rule> rules, arch arch) {
    @string newop = default;
    bool ok = default;

    if (len(rules) != 1) {
        foreach (var (_, r) in rules) {
            if (strings.Contains(r.Rule, "...")) {
                log.Fatalf("%s: found ellipsis in rule, but there are other rules with the same op", r.Loc);
            }
        }        return ("", false);
    }
    var rule = rules[0];
    var (match, cond, result) = rule.parse();
    if (cond != "" || !isEllipsisValue(match) || !isEllipsisValue(result)) {
        if (strings.Contains(rule.Rule, "...")) {
            log.Fatalf("%s: found ellipsis in non-ellipsis rule", rule.Loc);
        }
        checkEllipsisRuleCandidate(rule, arch);
        return ("", false);

    }
    var (op, oparch, _, _, _, _) = parseValue(result, arch, rule.Loc);
    return (fmt.Sprintf("Op%s%s", oparch, op.name), true);

}

// isEllipsisValue reports whether s is of the form (OpX ...).
private static bool isEllipsisValue(@string s) {
    if (len(s) < 2 || s[0] != '(' || s[len(s) - 1] != ')') {
        return false;
    }
    var c = split(s[(int)1..(int)len(s) - 1]);
    if (len(c) != 2 || c[1] != "...") {
        return false;
    }
    return true;

}

private static void checkEllipsisRuleCandidate(Rule rule, arch arch) {
    var (match, cond, result) = rule.parse();
    if (cond != "") {
        return ;
    }
    var (op, _, _, auxint, aux, args) = parseValue(match, arch, rule.Loc);
    @string auxint2 = default;    @string aux2 = default;

    slice<@string> args2 = default;
    @string usingCopy = default;
    opData eop = default;
    if (result[0] != '(') { 
        // Check for (Foo x) => x, which can be converted to (Foo ...) => (Copy ...).
        args2 = new slice<@string>(new @string[] { result });
        usingCopy = " using Copy";

    }
    else
 {
        eop, _, _, auxint2, aux2, args2 = parseValue(result, arch, rule.Loc);
    }
    if (aux != aux2 || auxint != auxint2 || len(args) != len(args2)) {
        return ;
    }
    if (strings.Contains(rule.Rule, "=>") && op.aux != eop.aux) {
        return ;
    }
    foreach (var (i) in args) {
        if (args[i] != args2[i]) {
            return ;
        }
    }
    if (opHasAux(op) && aux == "" && aux2 == "") 
        fmt.Printf("%s: rule silently zeros aux, either copy aux or explicitly zero\n", rule.Loc);
    else if (opHasAuxInt(op) && auxint == "" && auxint2 == "") 
        fmt.Printf("%s: rule silently zeros auxint, either copy auxint or explicitly zero\n", rule.Loc);
    else 
        fmt.Printf("%s: possible ellipsis rule candidate%s: %q\n", rule.Loc, usingCopy, rule.Rule);
    
}

private static opData opByName(arch arch, @string name) => func((_, panic, _) => {
    name = name[(int)2..];
    {
        var x__prev1 = x;

        foreach (var (_, __x) in genericOps) {
            x = __x;
            if (name == x.name) {
                return x;
            }
        }
        x = x__prev1;
    }

    if (arch.name != "generic") {
        name = name[(int)len(arch.name)..];
        {
            var x__prev1 = x;

            foreach (var (_, __x) in arch.ops) {
                x = __x;
                if (name == x.name) {
                    return x;
                }
            }

            x = x__prev1;
        }
    }
    log.Fatalf("failed to find op named %s in arch %s", name, arch.name);
    panic("unreachable");

});

// auxType returns the Go type that this operation should store in its aux field.
private static @string auxType(this opData op) {
    switch (op.aux) {
        case "String": 
            return "string";
            break;
        case "Sym": 
            // Note: a Sym can be an *obj.LSym, a *gc.Node, or nil.
            return "Sym";
            break;
        case "SymOff": 
            return "Sym";
            break;
        case "Call": 
            return "Call";
            break;
        case "CallOff": 
            return "Call";
            break;
        case "SymValAndOff": 
            return "Sym";
            break;
        case "Typ": 
            return "*types.Type";
            break;
        case "TypSize": 
            return "*types.Type";
            break;
        case "S390XCCMask": 
            return "s390x.CCMask";
            break;
        case "S390XRotateParams": 
            return "s390x.RotateParams";
            break;
        default: 
            return "invalid";
            break;
    }

}

// auxIntType returns the Go type that this operation should store in its auxInt field.
private static @string auxIntType(this opData op) {
    switch (op.aux) {
        case "Bool": 
            return "bool";
            break;
        case "Int8": 
            return "int8";
            break;
        case "Int16": 
            return "int16";
            break;
        case "Int32": 
            return "int32";
            break;
        case "Int64": 
            return "int64";
            break;
        case "Int128": 
            return "int128";
            break;
        case "UInt8": 
            return "uint8";
            break;
        case "Float32": 
            return "float32";
            break;
        case "Float64": 
            return "float64";
            break;
        case "CallOff": 
            return "int32";
            break;
        case "SymOff": 
            return "int32";
            break;
        case "SymValAndOff": 
            return "ValAndOff";
            break;
        case "TypSize": 
            return "int64";
            break;
        case "CCop": 
            return "Op";
            break;
        case "FlagConstant": 
            return "flagConstant";
            break;
        case "ARM64BitField": 
            return "arm64BitField";
            break;
        default: 
            return "invalid";
            break;
    }

}

// auxType returns the Go type that this block should store in its aux field.
private static @string auxType(this blockData b) {
    switch (b.aux) {
        case "S390XCCMask": 

        case "S390XCCMaskInt8": 

        case "S390XCCMaskUint8": 
            return "s390x.CCMask";
            break;
        case "S390XRotateParams": 
            return "s390x.RotateParams";
            break;
        default: 
            return "invalid";
            break;
    }

}

// auxIntType returns the Go type that this block should store in its auxInt field.
private static @string auxIntType(this blockData b) {
    switch (b.aux) {
        case "S390XCCMaskInt8": 
            return "int8";
            break;
        case "S390XCCMaskUint8": 
            return "uint8";
            break;
        case "Int64": 
            return "int64";
            break;
        default: 
            return "invalid";
            break;
    }

}

private static @string title(@string s) {
    {
        var i = strings.Index(s, ".");

        if (i >= 0) {
            switch (strings.ToLower(s[..(int)i])) {
                case "s390x": // keep arch prefix for clarity
                    s = s[..(int)i] + s[(int)i + 1..];
                    break;
                default: 
                    s = s[(int)i + 1..];
                    break;
            }

        }
    }

    return strings.Title(s);

}

private static @string unTitle(@string s) {
    {
        var i = strings.Index(s, ".");

        if (i >= 0) {
            switch (strings.ToLower(s[..(int)i])) {
                case "s390x": // keep arch prefix for clarity
                    s = s[..(int)i] + s[(int)i + 1..];
                    break;
                default: 
                    s = s[(int)i + 1..];
                    break;
            }

        }
    }

    return strings.ToLower(s[..(int)1]) + s[(int)1..];

}

} // end main_package
