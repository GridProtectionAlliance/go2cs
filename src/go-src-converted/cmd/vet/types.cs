// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file contains the pieces of the tool that use typechecking from the go/types package.

// package main -- go2cs converted at 2020 August 29 10:09:32 UTC
// Original source: C:\Go\src\cmd\vet\types.go
using ast = go.go.ast_package;
using build = go.go.build_package;
using importer = go.go.importer_package;
using token = go.go.token_package;
using types = go.go.types_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class main_package
    {
        // stdImporter is the importer we use to import packages.
        // It is shared so that all packages are imported by the same importer.
        private static types.Importer stdImporter = default;

        private static ref types.Interface errorType = default;        private static ref types.Interface stringerType = default;        private static ref types.Interface formatterType = default;

        private static void inittypes()
        {
            errorType = types.Universe.Lookup("error").Type().Underlying()._<ref types.Interface>();

            {
                var typ__prev1 = typ;

                var typ = importType("fmt", "Stringer");

                if (typ != null)
                {
                    stringerType = typ.Underlying()._<ref types.Interface>();
                }

                typ = typ__prev1;

            }
            {
                var typ__prev1 = typ;

                typ = importType("fmt", "Formatter");

                if (typ != null)
                {
                    formatterType = typ.Underlying()._<ref types.Interface>();
                }

                typ = typ__prev1;

            }
        }

        // isNamedType reports whether t is the named type path.name.
        private static bool isNamedType(types.Type t, @string path, @string name)
        {
            ref types.Named (n, ok) = t._<ref types.Named>();
            if (!ok)
            {
                return false;
            }
            var obj = n.Obj();
            return obj.Name() == name && obj.Pkg() != null && obj.Pkg().Path() == path;
        }

        // importType returns the type denoted by the qualified identifier
        // path.name, and adds the respective package to the imports map
        // as a side effect. In case of an error, importType returns nil.
        private static types.Type importType(@string path, @string name)
        {
            var (pkg, err) = stdImporter.Import(path);
            if (err != null)
            { 
                // This can happen if the package at path hasn't been compiled yet.
                warnf("import failed: %v", err);
                return null;
            }
            {
                ref types.TypeName (obj, ok) = pkg.Scope().Lookup(name)._<ref types.TypeName>();

                if (ok)
                {
                    return obj.Type();
                }

            }
            warnf("invalid type name %q", name);
            return null;
        }

        private static slice<error> check(this ref Package pkg, ref token.FileSet fs, slice<ref ast.File> astFiles)
        {
            if (stdImporter == null)
            {
                if (source.Value)
                {
                    stdImporter = importer.For("source", null);
                }
                else
                {
                    stdImporter = importer.Default();
                }
                inittypes();
            }
            pkg.defs = make_map<ref ast.Ident, types.Object>();
            pkg.uses = make_map<ref ast.Ident, types.Object>();
            pkg.selectors = make_map<ref ast.SelectorExpr, ref types.Selection>();
            pkg.spans = make_map<types.Object, Span>();
            pkg.types = make_map<ast.Expr, types.TypeAndValue>();

            slice<error> allErrors = default;
            types.Config config = new types.Config(Importer:stdImporter,Error:func(eerror){allErrors=append(allErrors,e)},Sizes:archSizes,);
            types.Info info = ref new types.Info(Selections:pkg.selectors,Types:pkg.types,Defs:pkg.defs,Uses:pkg.uses,);
            var (typesPkg, err) = config.Check(pkg.path, fs, astFiles, info);
            if (len(allErrors) == 0L && err != null)
            {
                allErrors = append(allErrors, err);
            }
            pkg.typesPkg = typesPkg; 
            // update spans
            {
                var id__prev1 = id;
                var obj__prev1 = obj;

                foreach (var (__id, __obj) in pkg.defs)
                {
                    id = __id;
                    obj = __obj;
                    pkg.growSpan(id, obj);
                }

                id = id__prev1;
                obj = obj__prev1;
            }

            {
                var id__prev1 = id;
                var obj__prev1 = obj;

                foreach (var (__id, __obj) in pkg.uses)
                {
                    id = __id;
                    obj = __obj;
                    pkg.growSpan(id, obj);
                }

                id = id__prev1;
                obj = obj__prev1;
            }

            return allErrors;
        }

        // matchArgType reports an error if printf verb t is not appropriate
        // for operand arg.
        //
        // typ is used only for recursive calls; external callers must supply nil.
        //
        // (Recursion arises from the compound types {map,chan,slice} which
        // may be printed with %d etc. if that is appropriate for their element
        // types.)
        private static bool matchArgType(this ref File f, printfArgType t, types.Type typ, ast.Expr arg)
        {
            return f.matchArgTypeInternal(t, typ, arg, make_map<types.Type, bool>());
        }

        // matchArgTypeInternal is the internal version of matchArgType. It carries a map
        // remembering what types are in progress so we don't recur when faced with recursive
        // types or mutually recursive types.
        private static bool matchArgTypeInternal(this ref File _f, printfArgType t, types.Type typ, ast.Expr arg, map<types.Type, bool> inProgress) => func(_f, (ref File f, Defer _, Panic panic, Recover __) =>
        { 
            // %v, %T accept any argument type.
            if (t == anyType)
            {
                return true;
            }
            if (typ == null)
            { 
                // external call
                typ = f.pkg.types[arg].Type;
                if (typ == null)
                {
                    return true; // probably a type check problem
                }
            } 
            // If the type implements fmt.Formatter, we have nothing to check.
            if (f.isFormatter(typ))
            {
                return true;
            } 
            // If we can use a string, might arg (dynamically) implement the Stringer or Error interface?
            if (t & argString != 0L && isConvertibleToString(typ))
            {
                return true;
            }
            typ = typ.Underlying();
            if (inProgress[typ])
            { 
                // We're already looking at this type. The call that started it will take care of it.
                return true;
            }
            inProgress[typ] = true;

            switch (typ.type())
            {
                case ref types.Signature typ:
                    return t & argPointer != 0L;
                    break;
                case ref types.Map typ:
                    return t & argPointer != 0L || (f.matchArgTypeInternal(t, typ.Key(), arg, inProgress) && f.matchArgTypeInternal(t, typ.Elem(), arg, inProgress));
                    break;
                case ref types.Chan typ:
                    return t & argPointer != 0L;
                    break;
                case ref types.Array typ:
                    if (types.Identical(typ.Elem().Underlying(), types.Typ[types.Byte]) && t & argString != 0L)
                    {
                        return true; // %s matches []byte
                    } 
                    // Recur: []int matches %d.
                    return t & argPointer != 0L || f.matchArgTypeInternal(t, typ.Elem(), arg, inProgress);
                    break;
                case ref types.Slice typ:
                    if (types.Identical(typ.Elem().Underlying(), types.Typ[types.Byte]) && t & argString != 0L)
                    {
                        return true; // %s matches []byte
                    } 
                    // Recur: []int matches %d. But watch out for
                    //    type T []T
                    // If the element is a pointer type (type T[]*T), it's handled fine by the Pointer case below.
                    return t & argPointer != 0L || f.matchArgTypeInternal(t, typ.Elem(), arg, inProgress);
                    break;
                case ref types.Pointer typ:
                    if (typ.Elem().String() == "invalid type")
                    {
                        if (verbose.Value)
                        {
                            f.Warnf(arg.Pos(), "printf argument %v is pointer to invalid or unknown type", f.gofmt(arg));
                        }
                        return true; // special case
                    } 
                    // If it's actually a pointer with %p, it prints as one.
                    if (t == argPointer)
                    {
                        return true;
                    } 
                    // If it's pointer to struct, that's equivalent in our analysis to whether we can print the struct.
                    {
                        ref types.Struct (str, ok) = typ.Elem().Underlying()._<ref types.Struct>();

                        if (ok)
                        {
                            return f.matchStructArgType(t, str, arg, inProgress);
                        } 
                        // The rest can print with %p as pointers, or as integers with %x etc.

                    } 
                    // The rest can print with %p as pointers, or as integers with %x etc.
                    return t & (argInt | argPointer) != 0L;
                    break;
                case ref types.Struct typ:
                    return f.matchStructArgType(t, typ, arg, inProgress);
                    break;
                case ref types.Interface typ:
                    return true;
                    break;
                case ref types.Basic typ:

                    if (typ.Kind() == types.UntypedBool || typ.Kind() == types.Bool) 
                        return t & argBool != 0L;
                    else if (typ.Kind() == types.UntypedInt || typ.Kind() == types.Int || typ.Kind() == types.Int8 || typ.Kind() == types.Int16 || typ.Kind() == types.Int32 || typ.Kind() == types.Int64 || typ.Kind() == types.Uint || typ.Kind() == types.Uint8 || typ.Kind() == types.Uint16 || typ.Kind() == types.Uint32 || typ.Kind() == types.Uint64 || typ.Kind() == types.Uintptr) 
                        return t & argInt != 0L;
                    else if (typ.Kind() == types.UntypedFloat || typ.Kind() == types.Float32 || typ.Kind() == types.Float64) 
                        return t & argFloat != 0L;
                    else if (typ.Kind() == types.UntypedComplex || typ.Kind() == types.Complex64 || typ.Kind() == types.Complex128) 
                        return t & argComplex != 0L;
                    else if (typ.Kind() == types.UntypedString || typ.Kind() == types.String) 
                        return t & argString != 0L;
                    else if (typ.Kind() == types.UnsafePointer) 
                        return t & (argPointer | argInt) != 0L;
                    else if (typ.Kind() == types.UntypedRune) 
                        return t & (argInt | argRune) != 0L;
                    else if (typ.Kind() == types.UntypedNil) 
                        return t & argPointer != 0L; // TODO?
                    else if (typ.Kind() == types.Invalid) 
                        if (verbose.Value)
                        {
                            f.Warnf(arg.Pos(), "printf argument %v has invalid or unknown type", f.gofmt(arg));
                        }
                        return true; // Probably a type check problem.
                                        panic("unreachable");
                    break;

            }

            return false;
        });

        private static bool isConvertibleToString(types.Type typ)
        {
            {
                ref types.Basic (bt, ok) = typ._<ref types.Basic>();

                if (ok && bt.Kind() == types.UntypedNil)
                { 
                    // We explicitly don't want untyped nil, which is
                    // convertible to both of the interfaces below, as it
                    // would just panic anyway.
                    return false;
                }

            }
            if (types.ConvertibleTo(typ, errorType))
            {
                return true; // via .Error()
            }
            if (stringerType != null && types.ConvertibleTo(typ, stringerType))
            {
                return true; // via .String()
            }
            return false;
        }

        // hasBasicType reports whether x's type is a types.Basic with the given kind.
        private static bool hasBasicType(this ref File f, ast.Expr x, types.BasicKind kind)
        {
            var t = f.pkg.types[x].Type;
            if (t != null)
            {
                t = t.Underlying();
            }
            ref types.Basic (b, ok) = t._<ref types.Basic>();
            return ok && b.Kind() == kind;
        }

        // matchStructArgType reports whether all the elements of the struct match the expected
        // type. For instance, with "%d" all the elements must be printable with the "%d" format.
        private static bool matchStructArgType(this ref File f, printfArgType t, ref types.Struct typ, ast.Expr arg, map<types.Type, bool> inProgress)
        {
            for (long i = 0L; i < typ.NumFields(); i++)
            {
                var typf = typ.Field(i);
                if (!f.matchArgTypeInternal(t, typf.Type(), arg, inProgress))
                {
                    return false;
                }
                if (t & argString != 0L && !typf.Exported() && isConvertibleToString(typf.Type()))
                { 
                    // Issue #17798: unexported Stringer or error cannot be properly fomatted.
                    return false;
                }
            }

            return true;
        }

        // hasMethod reports whether the type contains a method with the given name.
        // It is part of the workaround for Formatters and should be deleted when
        // that workaround is no longer necessary.
        // TODO: This could be better once issue 6259 is fixed.
        private static bool hasMethod(this ref File f, types.Type typ, @string name)
        { 
            // assume we have an addressable variable of type typ
            var (obj, _, _) = types.LookupFieldOrMethod(typ, true, f.pkg.typesPkg, name);
            ref types.Func (_, ok) = obj._<ref types.Func>();
            return ok;
        }

        private static var archSizes = types.SizesFor("gc", build.Default.GOARCH);
    }
}
