// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package analysisinternal exposes internal-only fields from go/analysis.
// package analysisinternal -- go2cs converted at 2020 October 09 06:01:15 UTC
// import "golang.org/x/tools/internal/analysisinternal" ==> using analysisinternal = go.golang.org.x.tools.@internal.analysisinternal_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\internal\analysisinternal\analysis.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using token = go.go.token_package;
using types = go.go.types_package;
using strings = go.strings_package;

using astutil = go.golang.org.x.tools.go.ast.astutil_package;
using static go.builtin;
using System;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace @internal
{
    public static partial class analysisinternal_package
    {
        public static token.Pos TypeErrorEndPos(ptr<token.FileSet> _addr_fset, slice<byte> src, token.Pos start)
        {
            ref token.FileSet fset = ref _addr_fset.val;
 
            // Get the end position for the type error.
            var offset = fset.PositionFor(start, false).Offset;
            var end = start;
            if (offset >= len(src))
            {
                return end;
            }
            {
                var width = bytes.IndexAny(src[offset..], " \n,():;[]+-*");

                if (width > 0L)
                {
                    end = start + token.Pos(width);
                }
            }

            return end;

        }

        public static ast.Expr ZeroValue(ptr<token.FileSet> _addr_fset, ptr<ast.File> _addr_f, ptr<types.Package> _addr_pkg, types.Type typ) => func((_, panic, __) =>
        {
            ref token.FileSet fset = ref _addr_fset.val;
            ref ast.File f = ref _addr_f.val;
            ref types.Package pkg = ref _addr_pkg.val;

            var under = typ;
            {
                ptr<types.Named> (n, ok) = typ._<ptr<types.Named>>();

                if (ok)
                {
                    under = n.Underlying();
                }

            }

            switch (under.type())
            {
                case ptr<types.Basic> u:

                    if (u.Info() & types.IsNumeric != 0L) 
                        return addr(new ast.BasicLit(Kind:token.INT,Value:"0"));
                    else if (u.Info() & types.IsBoolean != 0L) 
                        return addr(new ast.Ident(Name:"false"));
                    else if (u.Info() & types.IsString != 0L) 
                        return addr(new ast.BasicLit(Kind:token.STRING,Value:`""`));
                    else 
                        panic("unknown basic type");
                                        break;
                case ptr<types.Chan> u:
                    return ast.NewIdent("nil");
                    break;
                case ptr<types.Interface> u:
                    return ast.NewIdent("nil");
                    break;
                case ptr<types.Map> u:
                    return ast.NewIdent("nil");
                    break;
                case ptr<types.Pointer> u:
                    return ast.NewIdent("nil");
                    break;
                case ptr<types.Signature> u:
                    return ast.NewIdent("nil");
                    break;
                case ptr<types.Slice> u:
                    return ast.NewIdent("nil");
                    break;
                case ptr<types.Struct> u:
                    var texpr = typeExpr(_addr_fset, _addr_f, _addr_pkg, typ); // typ because we want the name here.
                    if (texpr == null)
                    {
                        return null;
                    }

                    return addr(new ast.CompositeLit(Type:texpr,));
                    break;
                case ptr<types.Array> u:
                    texpr = typeExpr(_addr_fset, _addr_f, _addr_pkg, u.Elem());
                    if (texpr == null)
                    {
                        return null;
                    }

                    return addr(new ast.CompositeLit(Type:&ast.ArrayType{Elt:texpr,Len:&ast.BasicLit{Kind:token.INT,Value:fmt.Sprintf("%v",u.Len())},},));
                    break;
            }
            return null;

        });

        private static ast.Expr typeExpr(ptr<token.FileSet> _addr_fset, ptr<ast.File> _addr_f, ptr<types.Package> _addr_pkg, types.Type typ)
        {
            ref token.FileSet fset = ref _addr_fset.val;
            ref ast.File f = ref _addr_f.val;
            ref types.Package pkg = ref _addr_pkg.val;

            switch (typ.type())
            {
                case ptr<types.Basic> t:

                    if (t.Kind() == types.UnsafePointer) 
                        return addr(new ast.SelectorExpr(X:ast.NewIdent("unsafe"),Sel:ast.NewIdent("Pointer")));
                    else 
                        return ast.NewIdent(t.Name());
                                        break;
                case ptr<types.Named> t:
                    if (t.Obj().Pkg() == pkg)
                    {
                        return ast.NewIdent(t.Obj().Name());
                    }

                    var pkgName = t.Obj().Pkg().Name(); 
                    // If the file already imports the package under another name, use that.
                    foreach (var (_, group) in astutil.Imports(fset, f))
                    {
                        foreach (var (_, cand) in group)
                        {
                            if (strings.Trim(cand.Path.Value, "\"") == t.Obj().Pkg().Path())
                            {
                                if (cand.Name != null && cand.Name.Name != "")
                                {
                                    pkgName = cand.Name.Name;
                                }

                            }

                        }

                    }
                    if (pkgName == ".")
                    {
                        return ast.NewIdent(t.Obj().Name());
                    }

                    return addr(new ast.SelectorExpr(X:ast.NewIdent(pkgName),Sel:ast.NewIdent(t.Obj().Name()),));
                    break;
                default:
                {
                    var t = typ.type();
                    return null; // TODO: anonymous structs, but who does that
                    break;
                }
            }

        }

        public static Func<object, slice<types.Error>> GetTypeErrors = p => null;
        public static Action<object, slice<types.Error>> SetTypeErrors = (p, errors) =>
        {
        };

        public partial struct TypeErrorPass // : @string
        {
        }

        public static readonly TypeErrorPass NoNewVars = (TypeErrorPass)"nonewvars";
        public static readonly TypeErrorPass NoResultValues = (TypeErrorPass)"noresultvalues";
        public static readonly TypeErrorPass UndeclaredName = (TypeErrorPass)"undeclaredname";

    }
}}}}}
