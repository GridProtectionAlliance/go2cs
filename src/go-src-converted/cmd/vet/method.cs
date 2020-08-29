// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file contains the code to check canonical methods.

// package main -- go2cs converted at 2020 August 29 10:09:24 UTC
// Original source: C:\Go\src\cmd\vet\method.go
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using printer = go.go.printer_package;
using strings = go.strings_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static void init()
        {
            register("methods", "check that canonically named methods are canonically defined", checkCanonicalMethod, funcDecl, interfaceType);
        }

        public partial struct MethodSig
        {
            public slice<@string> args;
            public slice<@string> results;
        }

        // canonicalMethods lists the input and output types for Go methods
        // that are checked using dynamic interface checks. Because the
        // checks are dynamic, such methods would not cause a compile error
        // if they have the wrong signature: instead the dynamic check would
        // fail, sometimes mysteriously. If a method is found with a name listed
        // here but not the input/output types listed here, vet complains.
        //
        // A few of the canonical methods have very common names.
        // For example, a type might implement a Scan method that
        // has nothing to do with fmt.Scanner, but we still want to check
        // the methods that are intended to implement fmt.Scanner.
        // To do that, the arguments that have a = prefix are treated as
        // signals that the canonical meaning is intended: if a Scan
        // method doesn't have a fmt.ScanState as its first argument,
        // we let it go. But if it does have a fmt.ScanState, then the
        // rest has to match.
        private static map canonicalMethods = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, MethodSig>{"Format":{[]string{"=fmt.State","rune"},[]string{}},"GobDecode":{[]string{"[]byte"},[]string{"error"}},"GobEncode":{[]string{},[]string{"[]byte","error"}},"MarshalJSON":{[]string{},[]string{"[]byte","error"}},"MarshalXML":{[]string{"*xml.Encoder","xml.StartElement"},[]string{"error"}},"ReadByte":{[]string{},[]string{"byte","error"}},"ReadFrom":{[]string{"=io.Reader"},[]string{"int64","error"}},"ReadRune":{[]string{},[]string{"rune","int","error"}},"Scan":{[]string{"=fmt.ScanState","rune"},[]string{"error"}},"Seek":{[]string{"=int64","int"},[]string{"int64","error"}},"UnmarshalJSON":{[]string{"[]byte"},[]string{"error"}},"UnmarshalXML":{[]string{"*xml.Decoder","xml.StartElement"},[]string{"error"}},"UnreadByte":{[]string{},[]string{"error"}},"UnreadRune":{[]string{},[]string{"error"}},"WriteByte":{[]string{"byte"},[]string{"error"}},"WriteTo":{[]string{"=io.Writer"},[]string{"int64","error"}},};

        private static void checkCanonicalMethod(ref File f, ast.Node node)
        {
            switch (node.type())
            {
                case ref ast.FuncDecl n:
                    if (n.Recv != null)
                    {
                        canonicalMethod(f, n.Name, n.Type);
                    }
                    break;
                case ref ast.InterfaceType n:
                    foreach (var (_, field) in n.Methods.List)
                    {
                        foreach (var (_, id) in field.Names)
                        {
                            canonicalMethod(f, id, field.Type._<ref ast.FuncType>());
                        }
                    }
                    break;
            }
        }

        private static void canonicalMethod(ref File f, ref ast.Ident id, ref ast.FuncType t)
        { 
            // Expected input/output.
            var (expect, ok) = canonicalMethods[id.Name];
            if (!ok)
            {
                return;
            } 

            // Actual input/output
            var args = typeFlatten(t.Params.List);
            slice<ast.Expr> results = default;
            if (t.Results != null)
            {
                results = typeFlatten(t.Results.List);
            } 

            // Do the =s (if any) all match?
            if (!f.matchParams(expect.args, args, "=") || !f.matchParams(expect.results, results, "="))
            {
                return;
            } 

            // Everything must match.
            if (!f.matchParams(expect.args, args, "") || !f.matchParams(expect.results, results, ""))
            {
                var expectFmt = id.Name + "(" + argjoin(expect.args) + ")";
                if (len(expect.results) == 1L)
                {
                    expectFmt += " " + argjoin(expect.results);
                }
                else if (len(expect.results) > 1L)
                {
                    expectFmt += " (" + argjoin(expect.results) + ")";
                }
                f.b.Reset();
                {
                    var err = printer.Fprint(ref f.b, f.fset, t);

                    if (err != null)
                    {
                        fmt.Fprintf(ref f.b, "<%s>", err);
                    }

                }
                var actual = f.b.String();
                actual = strings.TrimPrefix(actual, "func");
                actual = id.Name + actual;

                f.Badf(id.Pos(), "method %s should have signature %s", actual, expectFmt);
            }
        }

        private static @string argjoin(slice<@string> x)
        {
            var y = make_slice<@string>(len(x));
            foreach (var (i, s) in x)
            {
                if (s[0L] == '=')
                {
                    s = s[1L..];
                }
                y[i] = s;
            }
            return strings.Join(y, ", ");
        }

        // Turn parameter list into slice of types
        // (in the ast, types are Exprs).
        // Have to handle f(int, bool) and f(x, y, z int)
        // so not a simple 1-to-1 conversion.
        private static slice<ast.Expr> typeFlatten(slice<ref ast.Field> l)
        {
            slice<ast.Expr> t = default;
            foreach (var (_, f) in l)
            {
                if (len(f.Names) == 0L)
                {
                    t = append(t, f.Type);
                    continue;
                }
                foreach (>>MARKER:FORRANGEEXPRESSIONS_LEVEL_2<< in f.Names)
                {>>MARKER:FORRANGEMUTABLEEXPRESSIONS_LEVEL_2<<
                    t = append(t, f.Type);
                }
            }
            return t;
        }

        // Does each type in expect with the given prefix match the corresponding type in actual?
        private static bool matchParams(this ref File f, slice<@string> expect, slice<ast.Expr> actual, @string prefix)
        {
            foreach (var (i, x) in expect)
            {
                if (!strings.HasPrefix(x, prefix))
                {
                    continue;
                }
                if (i >= len(actual))
                {
                    return false;
                }
                if (!f.matchParamType(x, actual[i]))
                {
                    return false;
                }
            }
            if (prefix == "" && len(actual) > len(expect))
            {
                return false;
            }
            return true;
        }

        // Does this one type match?
        private static bool matchParamType(this ref File f, @string expect, ast.Expr actual)
        {
            if (strings.HasPrefix(expect, "="))
            {
                expect = expect[1L..];
            } 
            // Strip package name if we're in that package.
            {
                var n = len(f.file.Name.Name);

                if (len(expect) > n && expect[..n] == f.file.Name.Name && expect[n] == '.')
                {
                    expect = expect[n + 1L..];
                } 

                // Overkill but easy.

            } 

            // Overkill but easy.
            f.b.Reset();
            printer.Fprint(ref f.b, f.fset, actual);
            return f.b.String() == expect;
        }
    }
}
