// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build gen

// This program generates Go code that applies rewrite rules to a Value.
// The generated code implements a function of type func (v *Value) bool
// which returns true iff if did something.
// Ideas stolen from Swift: http://www.hpl.hp.com/techreports/Compaq-DEC/WRL-2000-2.html

// package main -- go2cs converted at 2020 August 29 09:24:37 UTC
// Original source: C:\Go\src\cmd\compile\internal\ssa\gen\rulegen.go
using bufio = go.bufio_package;
using bytes = go.bytes_package;
using flag = go.flag_package;
using fmt = go.fmt_package;
using format = go.go.format_package;
using io = go.io_package;
using ioutil = go.io.ioutil_package;
using log = go.log_package;
using os = go.os_package;
using regexp = go.regexp_package;
using sort = go.sort_package;
using strings = go.strings_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class main_package
    {
        // rule syntax:
        //  sexpr [&& extra conditions] -> [@block] sexpr
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

        // extra conditions is just a chunk of Go that evaluates to a boolean. It may use
        // variables declared in the matching sexpr. The variable "v" is predefined to be
        // the value matched by the entire rule.

        // If multiple rules match, the first one in file order is selected.
        private static var genLog = flag.Bool("log", false, "generate code that logs; for debugging only");

        public partial struct Rule
        {
            public @string rule;
            public @string loc; // file name & line number
        }

        public static @string String(this Rule r)
        {
            return fmt.Sprintf("rule %q at %s", r.rule, r.loc);
        }

        // parse returns the matching part of the rule, additional conditions, and the result.
        public static (@string, @string, @string) parse(this Rule r)
        {
            var s = strings.Split(r.rule, "->");
            if (len(s) != 2L)
            {
                log.Fatalf("no arrow in %s", r);
            }
            match = strings.TrimSpace(s[0L]);
            result = strings.TrimSpace(s[1L]);
            cond = "";
            {
                var i = strings.Index(match, "&&");

                if (i >= 0L)
                {
                    cond = strings.TrimSpace(match[i + 2L..]);
                    match = strings.TrimSpace(match[..i]);
                }

            }
            return (match, cond, result);
        }

        private static void genRules(arch arch) => func((_, panic, __) =>
        { 
            // Open input file.
            var (text, err) = os.Open(arch.name + ".rules");
            if (err != null)
            {
                log.Fatalf("can't read rule file: %v", err);
            } 

            // oprules contains a list of rules for each block and opcode
            map blockrules = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, slice<Rule>>{};
            map oprules = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, slice<Rule>>{}; 

            // read rule file
            var scanner = bufio.NewScanner(text);
            @string rule = "";
            long lineno = default;
            long ruleLineno = default; // line number of "->"
            while (scanner.Scan())
            {
                lineno++;
                var line = scanner.Text();
                {
                    var i__prev1 = i;

                    var i = strings.Index(line, "//");

                    if (i >= 0L)
                    { 
                        // Remove comments. Note that this isn't string safe, so
                        // it will truncate lines with // inside strings. Oh well.
                        line = line[..i];
                    }

                    i = i__prev1;

                }
                rule += " " + line;
                rule = strings.TrimSpace(rule);
                if (rule == "")
                {
                    continue;
                }
                if (!strings.Contains(rule, "->"))
                {
                    continue;
                }
                if (ruleLineno == 0L)
                {
                    ruleLineno = lineno;
                }
                if (strings.HasSuffix(rule, "->"))
                {
                    continue;
                }
                if (unbalanced(rule))
                {
                    continue;
                }
                var loc = fmt.Sprintf("%s.rules:%d", arch.name, ruleLineno);
                foreach (var (_, crule) in commute(rule, arch))
                {
                    Rule r = new Rule(rule:crule,loc:loc);
                    {
                        var rawop = strings.Split(crule, " ")[0L][1L..];

                        if (isBlock(rawop, arch))
                        {
                            blockrules[rawop] = append(blockrules[rawop], r);
                        }
                        else
                        { 
                            // Do fancier value op matching.
                            var (match, _, _) = r.parse();
                            var (op, oparch, _, _, _, _) = parseValue(match, arch, loc);
                            var opname = fmt.Sprintf("Op%s%s", oparch, op.name);
                            oprules[opname] = append(oprules[opname], r);
                        }

                    }
                }
                rule = "";
                ruleLineno = 0L;
            }

            {
                var err = scanner.Err();

                if (err != null)
                {
                    log.Fatalf("scanner failed: %v\n", err);
                }

            }
            if (unbalanced(rule))
            {
                log.Fatalf("%s.rules:%d: unbalanced rule: %v\n", arch.name, lineno, rule);
            } 

            // Order all the ops.
            slice<@string> ops = default;
            {
                var op__prev1 = op;

                foreach (var (__op) in oprules)
                {
                    op = __op;
                    ops = append(ops, op);
                }

                op = op__prev1;
            }

            sort.Strings(ops); 

            // Start output buffer, write header.
            ptr<object> w = @new<bytes.Buffer>();
            fmt.Fprintf(w, "// Code generated from gen/%s.rules; DO NOT EDIT.\n", arch.name);
            fmt.Fprintln(w, "// generated with: cd gen; go run *.go");
            fmt.Fprintln(w);
            fmt.Fprintln(w, "package ssa");
            fmt.Fprintln(w, "import \"math\"");
            fmt.Fprintln(w, "import \"cmd/internal/obj\"");
            fmt.Fprintln(w, "import \"cmd/internal/objabi\"");
            fmt.Fprintln(w, "import \"cmd/compile/internal/types\"");
            fmt.Fprintln(w, "var _ = math.MinInt8  // in case not otherwise used");
            fmt.Fprintln(w, "var _ = obj.ANOP      // in case not otherwise used");
            fmt.Fprintln(w, "var _ = objabi.GOROOT // in case not otherwise used");
            fmt.Fprintln(w, "var _ = types.TypeMem // in case not otherwise used");
            fmt.Fprintln(w);

            const long chunkSize = 10L; 
            // Main rewrite routine is a switch on v.Op.
 
            // Main rewrite routine is a switch on v.Op.
            fmt.Fprintf(w, "func rewriteValue%s(v *Value) bool {\n", arch.name);
            fmt.Fprintf(w, "switch v.Op {\n");
            {
                var op__prev1 = op;

                foreach (var (_, __op) in ops)
                {
                    op = __op;
                    fmt.Fprintf(w, "case %s:\n", op);
                    fmt.Fprint(w, "return ");
                    {
                        long chunk__prev2 = chunk;

                        long chunk = 0L;

                        while (chunk < len(oprules[op]))
                        {
                            if (chunk > 0L)
                            {
                                fmt.Fprint(w, " || ");
                            chunk += chunkSize;
                            }
                            fmt.Fprintf(w, "rewriteValue%s_%s_%d(v)", arch.name, op, chunk);
                        }


                        chunk = chunk__prev2;
                    }
                    fmt.Fprintln(w);
                }

                op = op__prev1;
            }

            fmt.Fprintf(w, "}\n");
            fmt.Fprintf(w, "return false\n");
            fmt.Fprintf(w, "}\n"); 

            // Generate a routine per op. Note that we don't make one giant routine
            // because it is too big for some compilers.
            {
                var op__prev1 = op;

                foreach (var (_, __op) in ops)
                {
                    op = __op;
                    {
                        long chunk__prev2 = chunk;

                        chunk = 0L;

                        while (chunk < len(oprules[op]))
                        {
                            ptr<object> buf = @new<bytes.Buffer>();
                            bool canFail = default;
                            var endchunk = chunk + chunkSize;
                            if (endchunk > len(oprules[op]))
                            {
                                endchunk = len(oprules[op]);
                            chunk += chunkSize;
                            }
                            {
                                var i__prev3 = i;
                                @string rule__prev3 = rule;

                                foreach (var (__i, __rule) in oprules[op][chunk..endchunk])
                                {
                                    i = __i;
                                    rule = __rule;
                                    var (match, cond, result) = rule.parse();
                                    fmt.Fprintf(buf, "// match: %s\n", match);
                                    fmt.Fprintf(buf, "// cond: %s\n", cond);
                                    fmt.Fprintf(buf, "// result: %s\n", result);

                                    canFail = false;
                                    fmt.Fprintf(buf, "for {\n");
                                    if (genMatch(buf, arch, match, rule.loc))
                                    {
                                        canFail = true;
                                    }
                                    if (cond != "")
                                    {
                                        fmt.Fprintf(buf, "if !(%s) {\nbreak\n}\n", cond);
                                        canFail = true;
                                    }
                                    if (!canFail && i + chunk != len(oprules[op]) - 1L)
                                    {
                                        log.Fatalf("unconditional rule %s is followed by other rules", match);
                                    }
                                    genResult(buf, arch, result, rule.loc);
                                    if (genLog.Value)
                                    {
                                        fmt.Fprintf(buf, "logRule(\"%s\")\n", rule.loc);
                                    }
                                    fmt.Fprintf(buf, "return true\n");

                                    fmt.Fprintf(buf, "}\n");
                                }

                                i = i__prev3;
                                rule = rule__prev3;
                            }

                            if (canFail)
                            {
                                fmt.Fprintf(buf, "return false\n");
                            }
                            var body = buf.String(); 
                            // Do a rough match to predict whether we need b, config, fe, and/or types.
                            // It's not precise--thus the blank assignments--but it's good enough
                            // to avoid generating needless code and doing pointless nil checks.
                            var hasb = strings.Contains(body, "b.");
                            var hasconfig = strings.Contains(body, "config.") || strings.Contains(body, "config)");
                            var hasfe = strings.Contains(body, "fe.");
                            var hastyps = strings.Contains(body, "typ.");
                            fmt.Fprintf(w, "func rewriteValue%s_%s_%d(v *Value) bool {\n", arch.name, op, chunk);
                            if (hasb || hasconfig || hasfe || hastyps)
                            {
                                fmt.Fprintln(w, "b := v.Block");
                                fmt.Fprintln(w, "_ = b");
                            }
                            if (hasconfig)
                            {
                                fmt.Fprintln(w, "config := b.Func.Config");
                                fmt.Fprintln(w, "_ = config");
                            }
                            if (hasfe)
                            {
                                fmt.Fprintln(w, "fe := b.Func.fe");
                                fmt.Fprintln(w, "_ = fe");
                            }
                            if (hastyps)
                            {
                                fmt.Fprintln(w, "typ := &b.Func.Config.Types");
                                fmt.Fprintln(w, "_ = typ");
                            }
                            fmt.Fprint(w, body);
                            fmt.Fprintf(w, "}\n");
                        }


                        chunk = chunk__prev2;
                    }
                } 

                // Generate block rewrite function. There are only a few block types
                // so we can make this one function with a switch.

                op = op__prev1;
            }

            fmt.Fprintf(w, "func rewriteBlock%s(b *Block) bool {\n", arch.name);
            fmt.Fprintln(w, "config := b.Func.Config");
            fmt.Fprintln(w, "_ = config");
            fmt.Fprintln(w, "fe := b.Func.fe");
            fmt.Fprintln(w, "_ = fe");
            fmt.Fprintln(w, "typ := &config.Types");
            fmt.Fprintln(w, "_ = typ");
            fmt.Fprintf(w, "switch b.Kind {\n");
            ops = null;
            {
                var op__prev1 = op;

                foreach (var (__op) in blockrules)
                {
                    op = __op;
                    ops = append(ops, op);
                }

                op = op__prev1;
            }

            sort.Strings(ops);
            {
                var op__prev1 = op;

                foreach (var (_, __op) in ops)
                {
                    op = __op;
                    fmt.Fprintf(w, "case %s:\n", blockName(op, arch));
                    {
                        @string rule__prev2 = rule;

                        foreach (var (_, __rule) in blockrules[op])
                        {
                            rule = __rule;
                            (match, cond, result) = rule.parse();
                            fmt.Fprintf(w, "// match: %s\n", match);
                            fmt.Fprintf(w, "// cond: %s\n", cond);
                            fmt.Fprintf(w, "// result: %s\n", result);

                            fmt.Fprintf(w, "for {\n");

                            var (_, _, _, aux, s) = extract(match); // remove parens, then split

                            // check match of control value
                            if (s[0L] != "nil")
                            {
                                fmt.Fprintf(w, "v := b.Control\n");
                                if (strings.Contains(s[0L], "("))
                                {
                                    genMatch0(w, arch, s[0L], "v", false, rule.loc);
                                }
                                else
                                {
                                    fmt.Fprintf(w, "_ = v\n"); // in case we don't use v
                                    fmt.Fprintf(w, "%s := b.Control\n", s[0L]);
                                }
                            }
                            if (aux != "")
                            {
                                fmt.Fprintf(w, "%s := b.Aux\n", aux);
                            }
                            if (cond != "")
                            {
                                fmt.Fprintf(w, "if !(%s) {\nbreak\n}\n", cond);
                            } 

                            // Rule matches. Generate result.
                            var (outop, _, _, aux, t) = extract(result); // remove parens, then split
                            var newsuccs = t[1L..]; 

                            // Check if newsuccs is the same set as succs.
                            var succs = s[1L..];
                            map m = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{};
                            {
                                var succ__prev3 = succ;

                                foreach (var (_, __succ) in succs)
                                {
                                    succ = __succ;
                                    if (m[succ])
                                    {
                                        log.Fatalf("can't have a repeat successor name %s in %s", succ, rule);
                                    }
                                    m[succ] = true;
                                }

                                succ = succ__prev3;
                            }

                            {
                                var succ__prev3 = succ;

                                foreach (var (_, __succ) in newsuccs)
                                {
                                    succ = __succ;
                                    if (!m[succ])
                                    {
                                        log.Fatalf("unknown successor %s in %s", succ, rule);
                                    }
                                    delete(m, succ);
                                }

                                succ = succ__prev3;
                            }

                            if (len(m) != 0L)
                            {
                                log.Fatalf("unmatched successors %v in %s", m, rule);
                            }
                            fmt.Fprintf(w, "b.Kind = %s\n", blockName(outop, arch));
                            if (t[0L] == "nil")
                            {
                                fmt.Fprintf(w, "b.SetControl(nil)\n");
                            }
                            else
                            {
                                fmt.Fprintf(w, "b.SetControl(%s)\n", genResult0(w, arch, t[0L], @new<int>(), false, false, rule.loc));
                            }
                            if (aux != "")
                            {
                                fmt.Fprintf(w, "b.Aux = %s\n", aux);
                            }
                            else
                            {
                                fmt.Fprintln(w, "b.Aux = nil");
                            }
                            var succChanged = false;
                            {
                                var i__prev3 = i;

                                for (i = 0L; i < len(succs); i++)
                                {
                                    if (succs[i] != newsuccs[i])
                                    {
                                        succChanged = true;
                                    }
                                }


                                i = i__prev3;
                            }
                            if (succChanged)
                            {
                                if (len(succs) != 2L)
                                {
                                    log.Fatalf("changed successors, len!=2 in %s", rule);
                                }
                                if (succs[0L] != newsuccs[1L] || succs[1L] != newsuccs[0L])
                                {
                                    log.Fatalf("can only handle swapped successors in %s", rule);
                                }
                                fmt.Fprintln(w, "b.swapSuccessors()");
                            }
                            if (genLog.Value)
                            {
                                fmt.Fprintf(w, "logRule(\"%s\")\n", rule.loc);
                            }
                            fmt.Fprintf(w, "return true\n");

                            fmt.Fprintf(w, "}\n");
                        }

                        rule = rule__prev2;
                    }

                }

                op = op__prev1;
            }

            fmt.Fprintf(w, "}\n");
            fmt.Fprintf(w, "return false\n");
            fmt.Fprintf(w, "}\n"); 

            // gofmt result
            var b = w.Bytes();
            var (src, err) = format.Source(b);
            if (err != null)
            {
                fmt.Printf("%s\n", b);
                panic(err);
            } 

            // Write to file
            err = ioutil.WriteFile("../rewrite" + arch.name + ".go", src, 0666L);
            if (err != null)
            {
                log.Fatalf("can't write output: %v\n", err);
            }
        });

        // genMatch returns true if the match can fail.
        private static bool genMatch(io.Writer w, arch arch, @string match, @string loc)
        {
            return genMatch0(w, arch, match, "v", true, loc);
        }

        private static bool genMatch0(io.Writer w, arch arch, @string match, @string v, object m, bool top, @string loc) => func((_, panic, __) =>
        {
            if (match[0L] != '(' || match[len(match) - 1L] != ')')
            {
                panic("non-compound expr in genMatch0: " + match);
            }
            var canFail = false;

            var (op, oparch, typ, auxint, aux, args) = parseValue(match, arch, loc); 

            // check op
            if (!top)
            {
                fmt.Fprintf(w, "if %s.Op != Op%s%s {\nbreak\n}\n", v, oparch, op.name);
                canFail = true;
            }
            if (typ != "")
            {
                if (!isVariable(typ))
                { 
                    // code. We must match the results of this code.
                    fmt.Fprintf(w, "if %s.Type != %s {\nbreak\n}\n", v, typ);
                    canFail = true;
                }
                else
                { 
                    // variable
                    {
                        var (_, ok) = m[typ];

                        if (ok)
                        { 
                            // must match previous variable
                            fmt.Fprintf(w, "if %s.Type != %s {\nbreak\n}\n", v, typ);
                            canFail = true;
                        }
                        else
                        {
                            m[typ] = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{};
                            fmt.Fprintf(w, "%s := %s.Type\n", typ, v);
                        }

                    }
                }
            }
            if (auxint != "")
            {
                if (!isVariable(auxint))
                { 
                    // code
                    fmt.Fprintf(w, "if %s.AuxInt != %s {\nbreak\n}\n", v, auxint);
                    canFail = true;
                }
                else
                { 
                    // variable
                    {
                        (_, ok) = m[auxint];

                        if (ok)
                        {
                            fmt.Fprintf(w, "if %s.AuxInt != %s {\nbreak\n}\n", v, auxint);
                            canFail = true;
                        }
                        else
                        {
                            m[auxint] = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{};
                            fmt.Fprintf(w, "%s := %s.AuxInt\n", auxint, v);
                        }

                    }
                }
            }
            if (aux != "")
            {
                if (!isVariable(aux))
                { 
                    // code
                    fmt.Fprintf(w, "if %s.Aux != %s {\nbreak\n}\n", v, aux);
                    canFail = true;
                }
                else
                { 
                    // variable
                    {
                        (_, ok) = m[aux];

                        if (ok)
                        {
                            fmt.Fprintf(w, "if %s.Aux != %s {\nbreak\n}\n", v, aux);
                            canFail = true;
                        }
                        else
                        {
                            m[aux] = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{};
                            fmt.Fprintf(w, "%s := %s.Aux\n", aux, v);
                        }

                    }
                }
            }
            {
                var n = len(args);

                if (n > 1L)
                {
                    fmt.Fprintf(w, "_ = %s.Args[%d]\n", v, n - 1L); // combine some bounds checks
                }

            }
            foreach (var (i, arg) in args)
            {
                if (arg == "_")
                {
                    continue;
                }
                if (!strings.Contains(arg, "("))
                { 
                    // leaf variable
                    {
                        (_, ok) = m[arg];

                        if (ok)
                        { 
                            // variable already has a definition. Check whether
                            // the old definition and the new definition match.
                            // For example, (add x x).  Equality is just pointer equality
                            // on Values (so cse is important to do before lowering).
                            fmt.Fprintf(w, "if %s != %s.Args[%d] {\nbreak\n}\n", arg, v, i);
                            canFail = true;
                        }
                        else
                        { 
                            // remember that this variable references the given value
                            m[arg] = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{};
                            fmt.Fprintf(w, "%s := %s.Args[%d]\n", arg, v, i);
                        }

                    }
                    continue;
                } 
                // compound sexpr
                @string argname = default;
                var colon = strings.Index(arg, ":");
                var openparen = strings.Index(arg, "(");
                if (colon >= 0L && openparen >= 0L && colon < openparen)
                { 
                    // rule-specified name
                    argname = arg[..colon];
                    arg = arg[colon + 1L..];
                }
                else
                { 
                    // autogenerated name
                    argname = fmt.Sprintf("%s_%d", v, i);
                }
                fmt.Fprintf(w, "%s := %s.Args[%d]\n", argname, v, i);
                if (genMatch0(w, arch, arg, argname, m, false, loc))
                {
                    canFail = true;
                }
            }
            if (op.argLength == -1L)
            {
                fmt.Fprintf(w, "if len(%s.Args) != %d {\nbreak\n}\n", v, len(args));
                canFail = true;
            }
            return canFail;
        });

        private static void genResult(io.Writer w, arch arch, @string result, @string loc)
        {
            var move = false;
            if (result[0L] == '@')
            { 
                // parse @block directive
                var s = strings.SplitN(result[1L..], " ", 2L);
                fmt.Fprintf(w, "b = %s\n", s[0L]);
                result = s[1L];
                move = true;
            }
            genResult0(w, arch, result, @new<int>(), true, move, loc);
        }
        private static @string genResult0(io.Writer w, arch arch, @string result, ref long alloc, bool top, bool move, @string loc)
        { 
            // TODO: when generating a constant result, use f.constVal to avoid
            // introducing copies just to clean them up again.
            if (result[0L] != '(')
            { 
                // variable
                if (top)
                { 
                    // It in not safe in general to move a variable between blocks
                    // (and particularly not a phi node).
                    // Introduce a copy.
                    fmt.Fprintf(w, "v.reset(OpCopy)\n");
                    fmt.Fprintf(w, "v.Type = %s.Type\n", result);
                    fmt.Fprintf(w, "v.AddArg(%s)\n", result);
                }
                return result;
            }
            var (op, oparch, typ, auxint, aux, args) = parseValue(result, arch, loc); 

            // Find the type of the variable.
            var typeOverride = typ != "";
            if (typ == "" && op.typ != "")
            {
                typ = typeName(op.typ);
            }
            @string v = default;
            if (top && !move)
            {
                v = "v";
                fmt.Fprintf(w, "v.reset(Op%s%s)\n", oparch, op.name);
                if (typeOverride)
                {
                    fmt.Fprintf(w, "v.Type = %s\n", typ);
                }
            }
            else
            {
                if (typ == "")
                {
                    log.Fatalf("sub-expression %s (op=Op%s%s) must have a type", result, oparch, op.name);
                }
                v = fmt.Sprintf("v%d", alloc.Value);
                alloc.Value++;
                fmt.Fprintf(w, "%s := b.NewValue0(v.Pos, Op%s%s, %s)\n", v, oparch, op.name, typ);
                if (move && top)
                { 
                    // Rewrite original into a copy
                    fmt.Fprintf(w, "v.reset(OpCopy)\n");
                    fmt.Fprintf(w, "v.AddArg(%s)\n", v);
                }
            }
            if (auxint != "")
            {
                fmt.Fprintf(w, "%s.AuxInt = %s\n", v, auxint);
            }
            if (aux != "")
            {
                fmt.Fprintf(w, "%s.Aux = %s\n", v, aux);
            }
            foreach (var (_, arg) in args)
            {
                var x = genResult0(w, arch, arg, alloc, false, move, loc);
                fmt.Fprintf(w, "%s.AddArg(%s)\n", v, x);
            }
            return v;
        }

        private static slice<@string> split(@string s) => func((_, panic, __) =>
        {
            slice<@string> r = default;

outer:
            while (s != "")
            {
                long d = 0L; // depth of ({[<
                byte open = default;                byte close = default; // opening and closing markers ({[< or )}]>
 // opening and closing markers ({[< or )}]>
                var nonsp = false; // found a non-space char so far
                for (long i = 0L; i < len(s); i++)
                {

                    if (d == 0L && s[i] == '(') 
                        open = '(';
                        close = ')';
                        d++;
                    else if (d == 0L && s[i] == '<') 
                        open = '<';
                        close = '>';
                        d++;
                    else if (d == 0L && s[i] == '[') 
                        open = '[';
                        close = ']';
                        d++;
                    else if (d == 0L && s[i] == '{') 
                        open = '{';
                        close = '}';
                        d++;
                    else if (d == 0L && (s[i] == ' ' || s[i] == '\t')) 
                        if (nonsp)
                        {
                            r = append(r, strings.TrimSpace(s[..i]));
                            s = s[i..];
                            _continueouter = true;
                            break;
                        }
                    else if (d > 0L && s[i] == open) 
                        d++;
                    else if (d > 0L && s[i] == close) 
                        d--;
                    else 
                        nonsp = true;
                                    }

                if (d != 0L)
                {
                    panic("imbalanced expression: " + s);
                }
                if (nonsp)
                {
                    r = append(r, strings.TrimSpace(s));
                }
                break;
            }
            return r;
        });

        // isBlock returns true if this op is a block opcode.
        private static bool isBlock(@string name, arch arch)
        {
            {
                var b__prev1 = b;

                foreach (var (_, __b) in genericBlocks)
                {
                    b = __b;
                    if (b.name == name)
                    {
                        return true;
                    }
                }

                b = b__prev1;
            }

            {
                var b__prev1 = b;

                foreach (var (_, __b) in arch.blocks)
                {
                    b = __b;
                    if (b.name == name)
                    {
                        return true;
                    }
                }

                b = b__prev1;
            }

            return false;
        }

        private static (@string, @string, @string, @string, slice<@string>) extract(@string val)
        {
            val = val[1L..len(val) - 1L]; // remove ()

            // Split val up into regions.
            // Split by spaces/tabs, except those contained in (), {}, [], or <>.
            var s = split(val); 

            // Extract restrictions and args.
            op = s[0L];
            foreach (var (_, a) in s[1L..])
            {
                switch (a[0L])
                {
                    case '<': 
                        typ = a[1L..len(a) - 1L]; // remove <>
                        break;
                    case '[': 
                        auxint = a[1L..len(a) - 1L]; // remove []
                        break;
                    case '{': 
                        aux = a[1L..len(a) - 1L]; // remove {}
                        break;
                    default: 
                        args = append(args, a);
                        break;
                }
            }
            return;
        }

        // parseValue parses a parenthesized value from a rule.
        // The value can be from the match or the result side.
        // It returns the op and unparsed strings for typ, auxint, and aux restrictions and for all args.
        // oparch is the architecture that op is located in, or "" for generic.
        private static (opData, @string, @string, @string, @string, slice<@string>) parseValue(@string val, arch arch, @string loc)
        {
            // Resolve the op.
            @string s = default;
            s, typ, auxint, aux, args = extract(val); 

            // match reports whether x is a good op to select.
            // If strict is true, rule generation might succeed.
            // If strict is false, rule generation has failed,
            // but we're trying to generate a useful error.
            // Doing strict=true then strict=false allows
            // precise op matching while retaining good error messages.
            Func<opData, bool, @string, bool> match = (x, strict, archname) =>
            {
                if (x.name != s)
                {
                    return false;
                }
                if (x.argLength != -1L && int(x.argLength) != len(args))
                {
                    if (strict)
                    {
                        return false;
                    }
                    else
                    {
                        log.Printf("%s: op %s (%s) should have %d args, has %d", loc, s, archname, x.argLength, len(args));
                    }
                }
                return true;
            }
;

            {
                var x__prev1 = x;

                foreach (var (_, __x) in genericOps)
                {
                    x = __x;
                    if (match(x, true, "generic"))
                    {
                        op = x;
                        break;
                    }
                }

                x = x__prev1;
            }

            if (arch.name != "generic")
            {
                {
                    var x__prev1 = x;

                    foreach (var (_, __x) in arch.ops)
                    {
                        x = __x;
                        if (match(x, true, arch.name))
                        {
                            if (op.name != "")
                            {
                                log.Fatalf("%s: matches for op %s found in both generic and %s", loc, op.name, arch.name);
                            }
                            op = x;
                            oparch = arch.name;
                            break;
                        }
                    }

                    x = x__prev1;
                }

            }
            if (op.name == "")
            { 
                // Failed to find the op.
                // Run through everything again with strict=false
                // to generate useful diagnosic messages before failing.
                {
                    var x__prev1 = x;

                    foreach (var (_, __x) in genericOps)
                    {
                        x = __x;
                        match(x, false, "generic");
                    }

                    x = x__prev1;
                }

                {
                    var x__prev1 = x;

                    foreach (var (_, __x) in arch.ops)
                    {
                        x = __x;
                        match(x, false, arch.name);
                    }

                    x = x__prev1;
                }

                log.Fatalf("%s: unknown op %s", loc, s);
            } 

            // Sanity check aux, auxint.
            if (auxint != "")
            {
                switch (op.aux)
                {
                    case "Bool": 

                    case "Int8": 

                    case "Int16": 

                    case "Int32": 

                    case "Int64": 

                    case "Int128": 

                    case "Float32": 

                    case "Float64": 

                    case "SymOff": 

                    case "SymValAndOff": 

                    case "SymInt32": 

                    case "TypSize": 
                        break;
                    default: 
                        log.Fatalf("%s: op %s %s can't have auxint", loc, op.name, op.aux);
                        break;
                }
            }
            if (aux != "")
            {
                switch (op.aux)
                {
                    case "String": 

                    case "Sym": 

                    case "SymOff": 

                    case "SymValAndOff": 

                    case "SymInt32": 

                    case "Typ": 

                    case "TypSize": 
                        break;
                    default: 
                        log.Fatalf("%s: op %s %s can't have aux", loc, op.name, op.aux);
                        break;
                }
            }
            return;
        }

        private static @string blockName(@string name, arch arch)
        {
            foreach (var (_, b) in genericBlocks)
            {
                if (b.name == name)
                {
                    return "Block" + name;
                }
            }
            return "Block" + arch.name + name;
        }

        // typeName returns the string to use to generate a type.
        private static @string typeName(@string typ) => func((_, panic, __) =>
        {
            if (typ[0L] == '(')
            {
                var ts = strings.Split(typ[1L..len(typ) - 1L], ",");
                if (len(ts) != 2L)
                {
                    panic("Tuple expect 2 arguments");
                }
                return "types.NewTuple(" + typeName(ts[0L]) + ", " + typeName(ts[1L]) + ")";
            }
            switch (typ)
            {
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
        });

        // unbalanced returns true if there aren't the same number of ( and ) in the string.
        private static bool unbalanced(@string s)
        {
            long left = default;            long right = default;

            foreach (var (_, c) in s)
            {
                if (c == '(')
                {
                    left++;
                }
                if (c == ')')
                {
                    right++;
                }
            }
            return left != right;
        }

        // isVariable reports whether s is a single Go alphanumeric identifier.
        private static bool isVariable(@string s) => func((_, panic, __) =>
        {
            var (b, err) = regexp.MatchString("^[A-Za-z_][A-Za-z_0-9]*$", s);
            if (err != null)
            {
                panic("bad variable regexp");
            }
            return b;
        });

        // commute returns all equivalent rules to r after applying all possible
        // argument swaps to the commutable ops in r.
        // Potentially exponential, be careful.
        private static slice<@string> commute(@string r, arch arch) => func((_, panic, __) =>
        {
            Rule (match, cond, result) = new Rule(rule:r).parse();
            var a = commute1(match, varCount(match), arch);
            foreach (var (i, m) in a)
            {
                if (cond != "")
                {
                    m += " && " + cond;
                }
                m += " -> " + result;
                a[i] = m;
            }
            if (len(a) == 1L && normalizeWhitespace(r) != normalizeWhitespace(a[0L]))
            {
                fmt.Println(normalizeWhitespace(r));
                fmt.Println(normalizeWhitespace(a[0L]));
                panic("commute() is not the identity for noncommuting rule");
            }
            if (false && len(a) > 1L)
            {
                fmt.Println(r);
                foreach (var (_, x) in a)
                {
                    fmt.Println("  " + x);
                }
            }
            return a;
        });

        private static slice<@string> commute1(@string m, map<@string, long> cnt, arch arch) => func((_, panic, __) =>
        {
            if (m[0L] == '<' || m[0L] == '[' || m[0L] == '{' || isVariable(m))
            {
                return new slice<@string>(new @string[] { m });
            } 
            // Split up input.
            @string prefix = default;
            var colon = strings.Index(m, ":");
            if (colon >= 0L && isVariable(m[..colon]))
            {
                prefix = m[..colon + 1L];
                m = m[colon + 1L..];
            }
            if (m[0L] != '(' || m[len(m) - 1L] != ')')
            {
                panic("non-compound expr in commute1: " + m);
            }
            var s = split(m[1L..len(m) - 1L]);
            var op = s[0L]; 

            // Figure out if the op is commutative or not.
            var commutative = false;
            {
                var x__prev1 = x;

                foreach (var (_, __x) in genericOps)
                {
                    x = __x;
                    if (op == x.name)
                    {
                        if (x.commutative)
                        {
                            commutative = true;
                        }
                        break;
                    }
                }

                x = x__prev1;
            }

            if (arch.name != "generic")
            {
                {
                    var x__prev1 = x;

                    foreach (var (_, __x) in arch.ops)
                    {
                        x = __x;
                        if (op == x.name)
                        {
                            if (x.commutative)
                            {
                                commutative = true;
                            }
                            break;
                        }
                    }

                    x = x__prev1;
                }

            }
            long idx0 = default;            long idx1 = default;

            if (commutative)
            { 
                // Find indexes of two args we can swap.
                {
                    var i__prev1 = i;
                    var arg__prev1 = arg;

                    foreach (var (__i, __arg) in s)
                    {
                        i = __i;
                        arg = __arg;
                        if (i == 0L || arg[0L] == '<' || arg[0L] == '[' || arg[0L] == '{')
                        {
                            continue;
                        }
                        if (idx0 == 0L)
                        {
                            idx0 = i;
                            continue;
                        }
                        if (idx1 == 0L)
                        {
                            idx1 = i;
                            break;
                        }
                    }

                    i = i__prev1;
                    arg = arg__prev1;
                }

                if (idx1 == 0L)
                {
                    panic("couldn't find first two args of commutative op " + s[0L]);
                }
                if (cnt[s[idx0]] == 1L && cnt[s[idx1]] == 1L || s[idx0] == s[idx1] && cnt[s[idx0]] == 2L)
                { 
                    // When we have (Add x y) with no ther uses of x and y in the matching rule,
                    // then we can skip the commutative match (Add y x).
                    commutative = false;
                }
            } 

            // Recursively commute arguments.
            var a = make_slice<slice<@string>>(len(s));
            {
                var i__prev1 = i;
                var arg__prev1 = arg;

                foreach (var (__i, __arg) in s)
                {
                    i = __i;
                    arg = __arg;
                    a[i] = commute1(arg, cnt, arch);
                } 

                // Choose all possibilities from all args.

                i = i__prev1;
                arg = arg__prev1;
            }

            var r = crossProduct(a); 

            // If commutative, do that again with its two args reversed.
            if (commutative)
            {
                a[idx0] = a[idx1];
                a[idx1] = a[idx0];
                r = append(r, crossProduct(a));
            } 

            // Construct result.
            {
                var i__prev1 = i;
                var x__prev1 = x;

                foreach (var (__i, __x) in r)
                {
                    i = __i;
                    x = __x;
                    r[i] = prefix + "(" + x + ")";
                }

                i = i__prev1;
                x = x__prev1;
            }

            return r;
        });

        // varCount returns a map which counts the number of occurrences of
        // Value variables in m.
        private static map<@string, long> varCount(@string m)
        {
            map cnt = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, long>{};
            varCount1(m, cnt);
            return cnt;
        }
        private static void varCount1(@string m, map<@string, long> cnt) => func((_, panic, __) =>
        {
            if (m[0L] == '<' || m[0L] == '[' || m[0L] == '{')
            {
                return;
            }
            if (isVariable(m))
            {
                cnt[m]++;
                return;
            } 
            // Split up input.
            var colon = strings.Index(m, ":");
            if (colon >= 0L && isVariable(m[..colon]))
            {
                cnt[m[..colon]]++;
                m = m[colon + 1L..];
            }
            if (m[0L] != '(' || m[len(m) - 1L] != ')')
            {
                panic("non-compound expr in commute1: " + m);
            }
            var s = split(m[1L..len(m) - 1L]);
            foreach (var (_, arg) in s[1L..])
            {
                varCount1(arg, cnt);
            }
        });

        // crossProduct returns all possible values
        // x[0][i] + " " + x[1][j] + " " + ... + " " + x[len(x)-1][k]
        // for all valid values of i, j, ..., k.
        private static slice<@string> crossProduct(slice<slice<@string>> x)
        {
            if (len(x) == 1L)
            {
                return x[0L];
            }
            slice<@string> r = default;
            foreach (var (_, tail) in crossProduct(x[1L..]))
            {
                foreach (var (_, first) in x[0L])
                {
                    r = append(r, first + " " + tail);
                }
            }
            return r;
        }

        // normalizeWhitespace replaces 2+ whitespace sequences with a single space.
        private static @string normalizeWhitespace(@string x)
        {
            x = strings.Join(strings.Fields(x), " ");
            x = strings.Replace(x, "( ", "(", -1L);
            x = strings.Replace(x, " )", ")", -1L);
            return x;
        }
    }
}
