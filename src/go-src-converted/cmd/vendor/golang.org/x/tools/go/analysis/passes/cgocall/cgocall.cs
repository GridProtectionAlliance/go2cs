// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package cgocall defines an Analyzer that detects some violations of
// the cgo pointer passing rules.
// package cgocall -- go2cs converted at 2020 October 08 04:57:52 UTC
// import "cmd/vendor/golang.org/x/tools/go/analysis/passes/cgocall" ==> using cgocall = go.cmd.vendor.golang.org.x.tools.go.analysis.passes.cgocall_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\tools\go\analysis\passes\cgocall\cgocall.go
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using format = go.go.format_package;
using parser = go.go.parser_package;
using token = go.go.token_package;
using types = go.go.types_package;
using log = go.log_package;
using os = go.os_package;
using strconv = go.strconv_package;

using analysis = go.golang.org.x.tools.go.analysis_package;
using analysisutil = go.golang.org.x.tools.go.analysis.passes.@internal.analysisutil_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace vendor {
namespace golang.org {
namespace x {
namespace tools {
namespace go {
namespace analysis {
namespace passes
{
    public static partial class cgocall_package
    {
        private static readonly var debug = (var)false;



        public static readonly @string Doc = (@string)@"detect some violations of the cgo pointer passing rules

Check for invalid cgo pointer passing.
This looks for code that uses cgo to call C code passing values
whose types are almost always invalid according to the cgo pointer
sharing rules.
Specifically, it warns about attempts to pass a Go chan, map, func,
or slice to C, either directly, or via a pointer, array, or struct.";



        public static ptr<analysis.Analyzer> Analyzer = addr(new analysis.Analyzer(Name:"cgocall",Doc:Doc,RunDespiteErrors:true,Run:run,));

        private static (object, error) run(ptr<analysis.Pass> _addr_pass)
        {
            object _p0 = default;
            error _p0 = default!;
            ref analysis.Pass pass = ref _addr_pass.val;

            if (!analysisutil.Imports(pass.Pkg, "runtime/cgo"))
            {
                return (null, error.As(null!)!); // doesn't use cgo
            }

            var (cgofiles, info, err) = typeCheckCgoSourceFiles(_addr_pass.Fset, _addr_pass.Pkg, pass.Files, _addr_pass.TypesInfo, pass.TypesSizes);
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            foreach (var (_, f) in cgofiles)
            {
                checkCgo(_addr_pass.Fset, _addr_f, _addr_info, pass.Reportf);
            }
            return (null, error.As(null!)!);

        }

        private static void checkCgo(ptr<token.FileSet> _addr_fset, ptr<ast.File> _addr_f, ptr<types.Info> _addr_info, params Action<token.Pos, @string, object>[] reportf)
        {
            reportf = reportf.Clone();
            ref token.FileSet fset = ref _addr_fset.val;
            ref ast.File f = ref _addr_f.val;
            ref types.Info info = ref _addr_info.val;

            ast.Inspect(f, n =>
            {
                ptr<ast.CallExpr> (call, ok) = n._<ptr<ast.CallExpr>>();
                if (!ok)
                {
                    return true;
                } 

                // Is this a C.f() call?
                @string name = default;
                {
                    ptr<ast.SelectorExpr> (sel, ok) = analysisutil.Unparen(call.Fun)._<ptr<ast.SelectorExpr>>();

                    if (ok)
                    {
                        {
                            ptr<ast.Ident> (id, ok) = sel.X._<ptr<ast.Ident>>();

                            if (ok && id.Name == "C")
                            {
                                name = sel.Sel.Name;
                            }

                        }

                    }

                }

                if (name == "")
                {
                    return true; // not a call we need to check
                } 

                // A call to C.CBytes passes a pointer but is always safe.
                if (name == "CBytes")
                {
                    return true;
                }

                if (debug)
                {
                    log.Printf("%s: call to C.%s", fset.Position(call.Lparen), name);
                }

                foreach (var (_, arg) in call.Args)
                {
                    if (!typeOKForCgoCall(cgoBaseType(_addr_info, arg), make_map<types.Type, bool>()))
                    {
                        reportf(arg.Pos(), "possibly passing Go type with embedded pointer to C");
                        break;
                    } 

                    // Check for passing the address of a bad type.
                    {
                        ptr<ast.CallExpr> (conv, ok) = arg._<ptr<ast.CallExpr>>();

                        if (ok && len(conv.Args) == 1L && isUnsafePointer(_addr_info, conv.Fun))
                        {
                            arg = conv.Args[0L];
                        }

                    }

                    {
                        ptr<ast.UnaryExpr> (u, ok) = arg._<ptr<ast.UnaryExpr>>();

                        if (ok && u.Op == token.AND)
                        {
                            if (!typeOKForCgoCall(cgoBaseType(_addr_info, u.X), make_map<types.Type, bool>()))
                            {
                                reportf(arg.Pos(), "possibly passing Go type with embedded pointer to C");
                                break;
                            }

                        }

                    }

                }
                return true;

            });

        }

        // typeCheckCgoSourceFiles returns type-checked syntax trees for the raw
        // cgo files of a package (those that import "C"). Such files are not
        // Go, so there may be gaps in type information around C.f references.
        //
        // This checker was initially written in vet to inspect raw cgo source
        // files using partial type information. However, Analyzers in the new
        // analysis API are presented with the type-checked, "cooked" Go ASTs
        // resulting from cgo-processing files, so we must choose between
        // working with the cooked file generated by cgo (which was tried but
        // proved fragile) or locating the raw cgo file (e.g. from //line
        // directives) and working with that, as we now do.
        //
        // Specifically, we must type-check the raw cgo source files (or at
        // least the subtrees needed for this analyzer) in an environment that
        // simulates the rest of the already type-checked package.
        //
        // For example, for each raw cgo source file in the original package,
        // such as this one:
        //
        //     package p
        //     import "C"
        //    import "fmt"
        //    type T int
        //    const k = 3
        //    var x, y = fmt.Println()
        //    func f() { ... }
        //    func g() { ... C.malloc(k) ... }
        //    func (T) f(int) string { ... }
        //
        // we synthesize a new ast.File, shown below, that dot-imports the
        // original "cooked" package using a special name ("·this·"), so that all
        // references to package members resolve correctly. (References to
        // unexported names cause an "unexported" error, which we ignore.)
        //
        // To avoid shadowing names imported from the cooked package,
        // package-level declarations in the new source file are modified so
        // that they do not declare any names.
        // (The cgocall analysis is concerned with uses, not declarations.)
        // Specifically, type declarations are discarded;
        // all names in each var and const declaration are blanked out;
        // each method is turned into a regular function by turning
        // the receiver into the first parameter;
        // and all functions are renamed to "_".
        //
        //     package p
        //     import . "·this·" // declares T, k, x, y, f, g, T.f
        //     import "C"
        //    import "fmt"
        //    const _ = 3
        //    var _, _ = fmt.Println()
        //    func _() { ... }
        //    func _() { ... C.malloc(k) ... }
        //    func _(T, int) string { ... }
        //
        // In this way, the raw function bodies and const/var initializer
        // expressions are preserved but refer to the "cooked" objects imported
        // from "·this·", and none of the transformed package-level declarations
        // actually declares anything. In the example above, the reference to k
        // in the argument of the call to C.malloc resolves to "·this·".k, which
        // has an accurate type.
        //
        // This approach could in principle be generalized to more complex
        // analyses on raw cgo files. One could synthesize a "C" package so that
        // C.f would resolve to "·this·"._C_func_f, for example. But we have
        // limited ourselves here to preserving function bodies and initializer
        // expressions since that is all that the cgocall analyzer needs.
        //
        private static (slice<ptr<ast.File>>, ptr<types.Info>, error) typeCheckCgoSourceFiles(ptr<token.FileSet> _addr_fset, ptr<types.Package> _addr_pkg, slice<ptr<ast.File>> files, ptr<types.Info> _addr_info, types.Sizes sizes)
        {
            slice<ptr<ast.File>> _p0 = default;
            ptr<types.Info> _p0 = default!;
            error _p0 = default!;
            ref token.FileSet fset = ref _addr_fset.val;
            ref types.Package pkg = ref _addr_pkg.val;
            ref types.Info info = ref _addr_info.val;

            const @string thispkg = (@string)"·this·"; 

            // Which files are cgo files?
 

            // Which files are cgo files?
            slice<ptr<ast.File>> cgoFiles = default;
            map importMap = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, ptr<types.Package>>{thispkg:pkg};
            foreach (var (_, raw) in files)
            { 
                // If f is a cgo-generated file, Position reports
                // the original file, honoring //line directives.
                var filename = fset.Position(raw.Pos()).Filename;
                var (f, err) = parser.ParseFile(fset, filename, null, parser.Mode(0L));
                if (err != null)
                {
                    return (null, _addr_null!, error.As(fmt.Errorf("can't parse raw cgo file: %v", err))!);
                }

                var found = false;
                {
                    var spec__prev2 = spec;

                    foreach (var (_, __spec) in f.Imports)
                    {
                        spec = __spec;
                        if (spec.Path.Value == "\"C\"")
                        {
                            found = true;
                            break;
                        }

                    }

                    spec = spec__prev2;
                }

                if (!found)
                {
                    continue; // not a cgo file
                } 

                // Record the original import map.
                {
                    var spec__prev2 = spec;

                    foreach (var (_, __spec) in raw.Imports)
                    {
                        spec = __spec;
                        var (path, _) = strconv.Unquote(spec.Path.Value);
                        importMap[path] = imported(_addr_info, _addr_spec);
                    } 

                    // Add special dot-import declaration:
                    //    import . "·this·"

                    spec = spec__prev2;
                }

                slice<ast.Decl> decls = default;
                decls = append(decls, addr(new ast.GenDecl(Tok:token.IMPORT,Specs:[]ast.Spec{&ast.ImportSpec{Name:&ast.Ident{Name:"."},Path:&ast.BasicLit{Kind:token.STRING,Value:strconv.Quote(thispkg),},},},))); 

                // Transform declarations from the raw cgo file.
                {
                    var decl__prev2 = decl;

                    foreach (var (_, __decl) in f.Decls)
                    {
                        decl = __decl;
                        switch (decl.type())
                        {
                            case ptr<ast.GenDecl> decl:

                                if (decl.Tok == token.TYPE) 
                                    // Discard type declarations.
                                    continue;
                                else if (decl.Tok == token.IMPORT)                                 else if (decl.Tok == token.VAR || decl.Tok == token.CONST) 
                                    // Blank the declared var/const names.
                                    {
                                        var spec__prev3 = spec;

                                        foreach (var (_, __spec) in decl.Specs)
                                        {
                                            spec = __spec;
                                            ptr<ast.ValueSpec> spec = spec._<ptr<ast.ValueSpec>>();
                                            foreach (var (i) in spec.Names)
                                            {
                                                spec.Names[i].Name = "_";
                                            }

                                        }

                                        spec = spec__prev3;
                                    }
                                                                break;
                            case ptr<ast.FuncDecl> decl:
                                decl.Name.Name = "_"; 

                                // Turn a method receiver:  func (T) f(P) R {...}
                                // into regular parameter:  func _(T, P) R {...}
                                if (decl.Recv != null)
                                {
                                    slice<ptr<ast.Field>> @params = default;
                                    params = append(params, decl.Recv.List);
                                    params = append(params, decl.Type.Params.List);
                                    decl.Type.Params.List = params;
                                    decl.Recv = null;
                                }

                                break;
                        }
                        decls = append(decls, decl);

                    }

                    decl = decl__prev2;
                }

                f.Decls = decls;
                if (debug)
                {
                    format.Node(os.Stderr, fset, f); // debugging
                }

                cgoFiles = append(cgoFiles, f);

            }
            if (cgoFiles == null)
            {
                return (null, _addr_null!, error.As(null!)!); // nothing to do (can't happen?)
            } 

            // Type-check the synthetic files.
            ptr<types.Config> tc = addr(new types.Config(FakeImportC:true,Importer:importerFunc(func(pathstring)(*types.Package,error){returnimportMap[path],nil}),Sizes:sizes,Error:func(error){},)); 

            // It's tempting to record the new types in the
            // existing pass.TypesInfo, but we don't own it.
            ptr<types.Info> altInfo = addr(new types.Info(Types:make(map[ast.Expr]types.TypeAndValue),));
            tc.Check(pkg.Path(), fset, cgoFiles, altInfo);

            return (cgoFiles, _addr_altInfo!, error.As(null!)!);

        }

        // cgoBaseType tries to look through type conversions involving
        // unsafe.Pointer to find the real type. It converts:
        //   unsafe.Pointer(x) => x
        //   *(*unsafe.Pointer)(unsafe.Pointer(&x)) => x
        private static types.Type cgoBaseType(ptr<types.Info> _addr_info, ast.Expr arg)
        {
            ref types.Info info = ref _addr_info.val;

            switch (arg.type())
            {
                case ptr<ast.CallExpr> arg:
                    if (len(arg.Args) == 1L && isUnsafePointer(_addr_info, arg.Fun))
                    {
                        return cgoBaseType(_addr_info, arg.Args[0L]);
                    }

                    break;
                case ptr<ast.StarExpr> arg:
                    ptr<ast.CallExpr> (call, ok) = arg.X._<ptr<ast.CallExpr>>();
                    if (!ok || len(call.Args) != 1L)
                    {
                        break;
                    } 
                    // Here arg is *f(v).
                    var t = info.Types[call.Fun].Type;
                    if (t == null)
                    {
                        break;
                    }

                    ptr<types.Pointer> (ptr, ok) = t.Underlying()._<ptr<types.Pointer>>();
                    if (!ok)
                    {
                        break;
                    } 
                    // Here arg is *(*p)(v)
                    ptr<types.Basic> (elem, ok) = ptr.Elem().Underlying()._<ptr<types.Basic>>();
                    if (!ok || elem.Kind() != types.UnsafePointer)
                    {
                        break;
                    } 
                    // Here arg is *(*unsafe.Pointer)(v)
                    call, ok = call.Args[0L]._<ptr<ast.CallExpr>>();
                    if (!ok || len(call.Args) != 1L)
                    {
                        break;
                    } 
                    // Here arg is *(*unsafe.Pointer)(f(v))
                    if (!isUnsafePointer(_addr_info, call.Fun))
                    {
                        break;
                    } 
                    // Here arg is *(*unsafe.Pointer)(unsafe.Pointer(v))
                    ptr<ast.UnaryExpr> (u, ok) = call.Args[0L]._<ptr<ast.UnaryExpr>>();
                    if (!ok || u.Op != token.AND)
                    {
                        break;
                    } 
                    // Here arg is *(*unsafe.Pointer)(unsafe.Pointer(&v))
                    return cgoBaseType(_addr_info, u.X);
                    break;

            }

            return info.Types[arg].Type;

        }

        // typeOKForCgoCall reports whether the type of arg is OK to pass to a
        // C function using cgo. This is not true for Go types with embedded
        // pointers. m is used to avoid infinite recursion on recursive types.
        private static bool typeOKForCgoCall(types.Type t, map<types.Type, bool> m)
        {
            if (t == null || m[t])
            {
                return true;
            }

            m[t] = true;
            switch (t.Underlying().type())
            {
                case ptr<types.Chan> t:
                    return false;
                    break;
                case ptr<types.Map> t:
                    return false;
                    break;
                case ptr<types.Signature> t:
                    return false;
                    break;
                case ptr<types.Slice> t:
                    return false;
                    break;
                case ptr<types.Pointer> t:
                    return typeOKForCgoCall(t.Elem(), m);
                    break;
                case ptr<types.Array> t:
                    return typeOKForCgoCall(t.Elem(), m);
                    break;
                case ptr<types.Struct> t:
                    for (long i = 0L; i < t.NumFields(); i++)
                    {
                        if (!typeOKForCgoCall(t.Field(i).Type(), m))
                        {
                            return false;
                        }

                    }
                    break;
            }
            return true;

        }

        private static bool isUnsafePointer(ptr<types.Info> _addr_info, ast.Expr e)
        {
            ref types.Info info = ref _addr_info.val;

            var t = info.Types[e].Type;
            return t != null && t.Underlying() == types.Typ[types.UnsafePointer];
        }

        public delegate  error) importerFunc(@string,  (ptr<types.Package>);

        private static (ptr<types.Package>, error) Import(this importerFunc f, @string path)
        {
            ptr<types.Package> _p0 = default!;
            error _p0 = default!;

            return _addr_f(path)!;
        }

        // TODO(adonovan): make this a library function or method of Info.
        private static ptr<types.Package> imported(ptr<types.Info> _addr_info, ptr<ast.ImportSpec> _addr_spec)
        {
            ref types.Info info = ref _addr_info.val;
            ref ast.ImportSpec spec = ref _addr_spec.val;

            var (obj, ok) = info.Implicits[spec];
            if (!ok)
            {
                obj = info.Defs[spec.Name]; // renaming import
            }

            return obj._<ptr<types.PkgName>>().Imported();

        }
    }
}}}}}}}}}
