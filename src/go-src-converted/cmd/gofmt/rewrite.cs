// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2022 March 13 06:32:36 UTC
// Original source: C:\Program Files\Go\src\cmd\gofmt\rewrite.go
namespace go;

using fmt = fmt_package;
using ast = go.ast_package;
using parser = go.parser_package;
using token = go.token_package;
using os = os_package;
using reflect = reflect_package;
using strings = strings_package;
using unicode = unicode_package;
using utf8 = unicode.utf8_package;
using System;

public static partial class main_package {

private static void initRewrite() {
    if (rewriteRule == "".val) {
        rewrite = null; // disable any previous rewrite
        return ;
    }
    var f = strings.Split(rewriteRule.val, "->");
    if (len(f) != 2) {
        fmt.Fprintf(os.Stderr, "rewrite rule must be of the form 'pattern -> replacement'\n");
        os.Exit(2);
    }
    var pattern = parseExpr(f[0], "pattern");
    var replace = parseExpr(f[1], "replacement");
    rewrite = p => rewriteFile(pattern, replace, _addr_p);
}

// parseExpr parses s as an expression.
// It might make sense to expand this to allow statement patterns,
// but there are problems with preserving formatting and also
// with what a wildcard for a statement looks like.
private static ast.Expr parseExpr(@string s, @string what) {
    var (x, err) = parser.ParseExpr(s);
    if (err != null) {
        fmt.Fprintf(os.Stderr, "parsing %s %s at %s\n", what, s, err);
        os.Exit(2);
    }
    return x;
}

// Keep this function for debugging.
/*
func dump(msg string, val reflect.Value) {
    fmt.Printf("%s:\n", msg)
    ast.Print(fileSet, val.Interface())
    fmt.Println()
}
*/

// rewriteFile applies the rewrite rule 'pattern -> replace' to an entire file.
private static ptr<ast.File> rewriteFile(ast.Expr pattern, ast.Expr replace, ptr<ast.File> _addr_p) {
    ref ast.File p = ref _addr_p.val;

    var cmap = ast.NewCommentMap(fileSet, p, p.Comments);
    var m = make_map<@string, reflect.Value>();
    var pat = reflect.ValueOf(pattern);
    var repl = reflect.ValueOf(replace);

    Func<reflect.Value, reflect.Value> rewriteVal = default;
    rewriteVal = val => { 
        // don't bother if val is invalid to start with
        if (!val.IsValid()) {
            return _addr_new reflect.Value()!;
        }
        val = apply(rewriteVal, val);
        foreach (var (k) in m) {
            delete(m, k);
        }        if (match(m, pat, val)) {
            val = subst(m, repl, reflect.ValueOf(val.Interface()._<ast.Node>().Pos()));
        }
        return _addr_val!;
    };

    ptr<ast.File> r = apply(rewriteVal, reflect.ValueOf(p)).Interface()._<ptr<ast.File>>();
    r.Comments = cmap.Filter(r).Comments(); // recreate comments list
    return _addr_r!;
}

// set is a wrapper for x.Set(y); it protects the caller from panics if x cannot be changed to y.
private static void set(reflect.Value x, reflect.Value y) => func((defer, panic, _) => { 
    // don't bother if x cannot be set or y is invalid
    if (!x.CanSet() || !y.IsValid()) {
        return ;
    }
    defer(() => {
        {
            var x = recover();

            if (x != null) {
                {
                    @string (s, ok) = x._<@string>();

                    if (ok && (strings.Contains(s, "type mismatch") || strings.Contains(s, "not assignable"))) { 
                        // x cannot be set to y - ignore this rewrite
                        return ;
                    }

                }
                panic(x);
            }

        }
    }());
    x.Set(y);
});

// Values/types for special cases.
private static var objectPtrNil = reflect.ValueOf((ast.Object.val)(null));private static var scopePtrNil = reflect.ValueOf((ast.Scope.val)(null));private static var identType = reflect.TypeOf((ast.Ident.val)(null));private static var objectPtrType = reflect.TypeOf((ast.Object.val)(null));private static var positionType = reflect.TypeOf(token.NoPos);private static var callExprType = reflect.TypeOf((ast.CallExpr.val)(null));private static var scopePtrType = reflect.TypeOf((ast.Scope.val)(null));

// apply replaces each AST field x in val with f(x), returning val.
// To avoid extra conversions, f operates on the reflect.Value form.
private static reflect.Value apply(Func<reflect.Value, reflect.Value> f, reflect.Value val) {
    if (!val.IsValid()) {
        return new reflect.Value();
    }
    if (val.Type() == objectPtrType) {
        return objectPtrNil;
    }
    if (val.Type() == scopePtrType) {
        return scopePtrNil;
    }
    {
        var v = reflect.Indirect(val);


        if (v.Kind() == reflect.Slice) 
            {
                nint i__prev1 = i;

                for (nint i = 0; i < v.Len(); i++) {
                    var e = v.Index(i);
                    set(e, f(e));
                }


                i = i__prev1;
            }
        else if (v.Kind() == reflect.Struct) 
            {
                nint i__prev1 = i;

                for (i = 0; i < v.NumField(); i++) {
                    e = v.Field(i);
                    set(e, f(e));
                }


                i = i__prev1;
            }
        else if (v.Kind() == reflect.Interface) 
            e = v.Elem();
            set(v, f(e));

    }
    return val;
}

private static bool isWildcard(@string s) {
    var (rune, size) = utf8.DecodeRuneInString(s);
    return size == len(s) && unicode.IsLower(rune);
}

// match reports whether pattern matches val,
// recording wildcard submatches in m.
// If m == nil, match checks whether pattern == val.
private static bool match(map<@string, reflect.Value> m, reflect.Value pattern, reflect.Value val) { 
    // Wildcard matches any expression. If it appears multiple
    // times in the pattern, it must match the same expression
    // each time.
    if (m != null && pattern.IsValid() && pattern.Type() == identType) {
        ptr<ast.Ident> name = pattern.Interface()._<ptr<ast.Ident>>().Name;
        if (isWildcard(name) && val.IsValid()) { 
            // wildcards only match valid (non-nil) expressions.
            {
                ast.Expr (_, ok) = val.Interface()._<ast.Expr>();

                if (ok && !val.IsNil()) {
                    {
                        var (old, ok) = m[name];

                        if (ok) {
                            return match(null, old, val);
                        }

                    }
                    m[name] = val;
                    return true;
                }

            }
        }
    }
    if (!pattern.IsValid() || !val.IsValid()) {
        return !pattern.IsValid() && !val.IsValid();
    }
    if (pattern.Type() != val.Type()) {
        return false;
    }

    if (pattern.Type() == identType) 
        // For identifiers, only the names need to match
        // (and none of the other *ast.Object information).
        // This is a common case, handle it all here instead
        // of recursing down any further via reflection.
        ptr<ast.Ident> p = pattern.Interface()._<ptr<ast.Ident>>();
        ptr<ast.Ident> v = val.Interface()._<ptr<ast.Ident>>();
        return p == null && v == null || p != null && v != null && p.Name == v.Name;
    else if (pattern.Type() == objectPtrType || pattern.Type() == positionType) 
        // object pointers and token positions always match
        return true;
    else if (pattern.Type() == callExprType) 
        // For calls, the Ellipsis fields (token.Position) must
        // match since that is how f(x) and f(x...) are different.
        // Check them here but fall through for the remaining fields.
        p = pattern.Interface()._<ptr<ast.CallExpr>>();
        v = val.Interface()._<ptr<ast.CallExpr>>();
        if (p.Ellipsis.IsValid() != v.Ellipsis.IsValid()) {
            return false;
        }
        p = reflect.Indirect(pattern);
    v = reflect.Indirect(val);
    if (!p.IsValid() || !v.IsValid()) {
        return !p.IsValid() && !v.IsValid();
    }

    if (p.Kind() == reflect.Slice) 
        if (p.Len() != v.Len()) {
            return false;
        }
        {
            nint i__prev1 = i;

            for (nint i = 0; i < p.Len(); i++) {
                if (!match(m, p.Index(i), v.Index(i))) {
                    return false;
                }
            }


            i = i__prev1;
        }
        return true;
    else if (p.Kind() == reflect.Struct) 
        {
            nint i__prev1 = i;

            for (i = 0; i < p.NumField(); i++) {
                if (!match(m, p.Field(i), v.Field(i))) {
                    return false;
                }
            }


            i = i__prev1;
        }
        return true;
    else if (p.Kind() == reflect.Interface) 
        return match(m, p.Elem(), v.Elem());
    // Handle token integers, etc.
    return p.Interface() == v.Interface();
}

// subst returns a copy of pattern with values from m substituted in place
// of wildcards and pos used as the position of tokens from the pattern.
// if m == nil, subst returns a copy of pattern and doesn't change the line
// number information.
private static reflect.Value subst(map<@string, reflect.Value> m, reflect.Value pattern, reflect.Value pos) {
    if (!pattern.IsValid()) {
        return new reflect.Value();
    }
    if (m != null && pattern.Type() == identType) {
        ptr<ast.Ident> name = pattern.Interface()._<ptr<ast.Ident>>().Name;
        if (isWildcard(name)) {
            {
                var old__prev3 = old;

                var (old, ok) = m[name];

                if (ok) {
                    return subst(null, old, new reflect.Value());
                }

                old = old__prev3;

            }
        }
    }
    if (pos.IsValid() && pattern.Type() == positionType) { 
        // use new position only if old position was valid in the first place
        {
            var old__prev2 = old;

            token.Pos old = pattern.Interface()._<token.Pos>();

            if (!old.IsValid()) {
                return pattern;
            }

            old = old__prev2;

        }
        return pos;
    }
    {
        var p = pattern;


        if (p.Kind() == reflect.Slice) 
            if (p.IsNil()) { 
                // Do not turn nil slices into empty slices. go/ast
                // guarantees that certain lists will be nil if not
                // populated.
                return reflect.Zero(p.Type());
            }
            var v = reflect.MakeSlice(p.Type(), p.Len(), p.Len());
            {
                nint i__prev1 = i;

                for (nint i = 0; i < p.Len(); i++) {
                    v.Index(i).Set(subst(m, p.Index(i), pos));
                }


                i = i__prev1;
            }
            return v;
        else if (p.Kind() == reflect.Struct) 
            v = reflect.New(p.Type()).Elem();
            {
                nint i__prev1 = i;

                for (i = 0; i < p.NumField(); i++) {
                    v.Field(i).Set(subst(m, p.Field(i), pos));
                }


                i = i__prev1;
            }
            return v;
        else if (p.Kind() == reflect.Ptr) 
            v = reflect.New(p.Type()).Elem();
            {
                var elem__prev1 = elem;

                var elem = p.Elem();

                if (elem.IsValid()) {
                    v.Set(subst(m, elem, pos).Addr());
                }

                elem = elem__prev1;

            }
            return v;
        else if (p.Kind() == reflect.Interface) 
            v = reflect.New(p.Type()).Elem();
            {
                var elem__prev1 = elem;

                elem = p.Elem();

                if (elem.IsValid()) {
                    v.Set(subst(m, elem, pos));
                }

                elem = elem__prev1;

            }
            return v;

    }

    return pattern;
}

} // end main_package
