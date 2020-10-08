// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package objectpath defines a naming scheme for types.Objects
// (that is, named entities in Go programs) relative to their enclosing
// package.
//
// Type-checker objects are canonical, so they are usually identified by
// their address in memory (a pointer), but a pointer has meaning only
// within one address space. By contrast, objectpath names allow the
// identity of an object to be sent from one program to another,
// establishing a correspondence between types.Object variables that are
// distinct but logically equivalent.
//
// A single object may have multiple paths. In this example,
//     type A struct{ X int }
//     type B A
// the field X has two paths due to its membership of both A and B.
// The For(obj) function always returns one of these paths, arbitrarily
// but consistently.
// package objectpath -- go2cs converted at 2020 October 08 04:55:59 UTC
// import "golang.org/x/tools/go/types/objectpath" ==> using objectpath = go.golang.org.x.tools.go.types.objectpath_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\types\objectpath\objectpath.go
using fmt = go.fmt_package;
using strconv = go.strconv_package;
using strings = go.strings_package;

using types = go.go.types_package;
using static go.builtin;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace go {
namespace types
{
    public static partial class objectpath_package
    {
        // A Path is an opaque name that identifies a types.Object
        // relative to its package. Conceptually, the name consists of a
        // sequence of destructuring operations applied to the package scope
        // to obtain the original object.
        // The name does not include the package itself.
        public partial struct Path // : @string
        {
        }

        // Encoding
        //
        // An object path is a textual and (with training) human-readable encoding
        // of a sequence of destructuring operators, starting from a types.Package.
        // The sequences represent a path through the package/object/type graph.
        // We classify these operators by their type:
        //
        //   PO package->object    Package.Scope.Lookup
        //   OT  object->type     Object.Type
        //   TT    type->type     Type.{Elem,Key,Params,Results,Underlying} [EKPRU]
        //   TO   type->object    Type.{At,Field,Method,Obj} [AFMO]
        //
        // All valid paths start with a package and end at an object
        // and thus may be defined by the regular language:
        //
        //   objectpath = PO (OT TT* TO)*
        //
        // The concrete encoding follows directly:
        // - The only PO operator is Package.Scope.Lookup, which requires an identifier.
        // - The only OT operator is Object.Type,
        //   which we encode as '.' because dot cannot appear in an identifier.
        // - The TT operators are encoded as [EKPRU].
        // - The OT operators are encoded as [AFMO];
        //   three of these (At,Field,Method) require an integer operand,
        //   which is encoded as a string of decimal digits.
        //   These indices are stable across different representations
        //   of the same package, even source and export data.
        //
        // In the example below,
        //
        //    package p
        //
        //    type T interface {
        //        f() (a string, b struct{ X int })
        //    }
        //
        // field X has the path "T.UM0.RA1.F0",
        // representing the following sequence of operations:
        //
        //    p.Lookup("T")                    T
        //    .Type().Underlying().Method(0).            f
        //    .Type().Results().At(1)                b
        //    .Type().Field(0)                    X
        //
        // The encoding is not maximally compact---every R or P is
        // followed by an A, for example---but this simplifies the
        // encoder and decoder.
        //
 
        // object->type operators
        private static readonly char opType = (char)'.'; // .Type()          (Object)

        // type->type operators
        private static readonly char opElem = (char)'E'; // .Elem()        (Pointer, Slice, Array, Chan, Map)
        private static readonly char opKey = (char)'K'; // .Key()        (Map)
        private static readonly char opParams = (char)'P'; // .Params()        (Signature)
        private static readonly char opResults = (char)'R'; // .Results()    (Signature)
        private static readonly char opUnderlying = (char)'U'; // .Underlying()    (Named)

        // type->object operators
        private static readonly char opAt = (char)'A'; // .At(i)        (Tuple)
        private static readonly char opField = (char)'F'; // .Field(i)        (Struct)
        private static readonly char opMethod = (char)'M'; // .Method(i)        (Named or Interface; not Struct: "promoted" names are ignored)
        private static readonly char opObj = (char)'O'; // .Obj()        (Named)

        // The For function returns the path to an object relative to its package,
        // or an error if the object is not accessible from the package's Scope.
        //
        // The For function guarantees to return a path only for the following objects:
        // - package-level types
        // - exported package-level non-types
        // - methods
        // - parameter and result variables
        // - struct fields
        // These objects are sufficient to define the API of their package.
        // The objects described by a package's export data are drawn from this set.
        //
        // For does not return a path for predeclared names, imported package
        // names, local names, and unexported package-level names (except
        // types).
        //
        // Example: given this definition,
        //
        //    package p
        //
        //    type T interface {
        //        f() (a string, b struct{ X int })
        //    }
        //
        // For(X) would return a path that denotes the following sequence of operations:
        //
        //    p.Scope().Lookup("T")                (TypeName T)
        //    .Type().Underlying().Method(0).            (method Func f)
        //    .Type().Results().At(1)                (field Var b)
        //    .Type().Field(0)                    (field Var X)
        //
        // where p is the package (*types.Package) to which X belongs.
        public static (Path, error) For(types.Object obj) => func((_, panic, __) =>
        {
            Path _p0 = default;
            error _p0 = default!;

            var pkg = obj.Pkg(); 

            // This table lists the cases of interest.
            //
            // Object                Action
            // ------                               ------
            // nil                    reject
            // builtin                reject
            // pkgname                reject
            // label                reject
            // var
            //    package-level            accept
            //    func param/result            accept
            //    local                reject
            //    struct field            accept
            // const
            //    package-level            accept
            //    local                reject
            // func
            //    package-level            accept
            //    init functions            reject
            //    concrete method            accept
            //    interface method            accept
            // type
            //    package-level            accept
            //    local                reject
            //
            // The only accessible package-level objects are members of pkg itself.
            //
            // The cases are handled in four steps:
            //
            // 1. reject nil and builtin
            // 2. accept package-level objects
            // 3. reject obviously invalid objects
            // 4. search the API for the path to the param/result/field/method.

            // 1. reference to nil or builtin?
            if (pkg == null)
            {
                return ("", error.As(fmt.Errorf("predeclared %s has no path", obj))!);
            }

            var scope = pkg.Scope(); 

            // 2. package-level object?
            if (scope.Lookup(obj.Name()) == obj)
            { 
                // Only exported objects (and non-exported types) have a path.
                // Non-exported types may be referenced by other objects.
                {
                    ptr<types.TypeName> (_, ok) = obj._<ptr<types.TypeName>>();

                    if (!ok && !obj.Exported())
                    {
                        return ("", error.As(fmt.Errorf("no path for non-exported %v", obj))!);
                    }

                }

                return (Path(obj.Name()), error.As(null!)!);

            } 

            // 3. Not a package-level object.
            //    Reject obviously non-viable cases.
            switch (obj.type())
            {
                case ptr<types.Const> obj:
                    return ("", error.As(fmt.Errorf("no path for %v", obj))!);
                    break;
                case ptr<types.TypeName> obj:
                    return ("", error.As(fmt.Errorf("no path for %v", obj))!);
                    break;
                case ptr<types.Label> obj:
                    return ("", error.As(fmt.Errorf("no path for %v", obj))!);
                    break;
                case ptr<types.PkgName> obj:
                    return ("", error.As(fmt.Errorf("no path for %v", obj))!);
                    break;
                case ptr<types.Var> obj:
                    break;
                case ptr<types.Func> obj:
                    {
                        ptr<types.Signature> recv = obj.Type()._<ptr<types.Signature>>().Recv();

                        if (recv == null)
                        {
                            return ("", error.As(fmt.Errorf("func is not a method: %v", obj))!);
                        } 
                        // TODO(adonovan): opt: if the method is concrete,
                        // do a specialized version of the rest of this function so
                        // that it's O(1) not O(|scope|).  Basically 'find' is needed
                        // only for struct fields and interface methods.

                    } 
                    // TODO(adonovan): opt: if the method is concrete,
                    // do a specialized version of the rest of this function so
                    // that it's O(1) not O(|scope|).  Basically 'find' is needed
                    // only for struct fields and interface methods.
                    break;
                default:
                {
                    var obj = obj.type();
                    panic(obj);
                    break;
                } 

                // 4. Search the API for the path to the var (field/param/result) or method.

                // First inspect package-level named types.
                // In the presence of path aliases, these give
                // the best paths because non-types may
                // refer to types, but not the reverse.
            } 

            // 4. Search the API for the path to the var (field/param/result) or method.

            // First inspect package-level named types.
            // In the presence of path aliases, these give
            // the best paths because non-types may
            // refer to types, but not the reverse.
            var empty = make_slice<byte>(0L, 48L); // initial space
            var names = scope.Names();
            {
                var name__prev1 = name;

                foreach (var (_, __name) in names)
                {
                    name = __name;
                    var o = scope.Lookup(name);
                    ptr<types.TypeName> (tname, ok) = o._<ptr<types.TypeName>>();
                    if (!ok)
                    {
                        continue; // handle non-types in second pass
                    }

                    var path = append(empty, name);
                    path = append(path, opType);

                    var T = o.Type();

                    if (tname.IsAlias())
                    { 
                        // type alias
                        {
                            var r__prev2 = r;

                            var r = find(obj, T, path);

                            if (r != null)
                            {
                                return (Path(r), error.As(null!)!);
                            }

                            r = r__prev2;

                        }

                    }
                    else
                    { 
                        // defined (named) type
                        {
                            var r__prev2 = r;

                            r = find(obj, T.Underlying(), append(path, opUnderlying));

                            if (r != null)
                            {
                                return (Path(r), error.As(null!)!);
                            }

                            r = r__prev2;

                        }

                    }

                } 

                // Then inspect everything else:
                // non-types, and declared methods of defined types.

                name = name__prev1;
            }

            {
                var name__prev1 = name;

                foreach (var (_, __name) in names)
                {
                    name = __name;
                    o = scope.Lookup(name);
                    path = append(empty, name);
                    {
                        (_, ok) = o._<ptr<types.TypeName>>();

                        if (!ok)
                        {
                            if (o.Exported())
                            { 
                                // exported non-type (const, var, func)
                                {
                                    var r__prev3 = r;

                                    r = find(obj, o.Type(), append(path, opType));

                                    if (r != null)
                                    {
                                        return (Path(r), error.As(null!)!);
                                    }

                                    r = r__prev3;

                                }

                            }

                            continue;

                        } 

                        // Inspect declared methods of defined types.

                    } 

                    // Inspect declared methods of defined types.
                    {
                        var T__prev1 = T;

                        ptr<types.Named> (T, ok) = o.Type()._<ptr<types.Named>>();

                        if (ok)
                        {
                            path = append(path, opType);
                            for (long i = 0L; i < T.NumMethods(); i++)
                            {
                                var m = T.Method(i);
                                var path2 = appendOpArg(path, opMethod, i);
                                if (m == obj)
                                {
                                    return (Path(path2), error.As(null!)!); // found declared method
                                }

                                {
                                    var r__prev2 = r;

                                    r = find(obj, m.Type(), append(path2, opType));

                                    if (r != null)
                                    {
                                        return (Path(r), error.As(null!)!);
                                    }

                                    r = r__prev2;

                                }

                            }


                        }

                        T = T__prev1;

                    }

                }

                name = name__prev1;
            }

            return ("", error.As(fmt.Errorf("can't find path for %v in %s", obj, pkg.Path()))!);

        });

        private static slice<byte> appendOpArg(slice<byte> path, byte op, long arg)
        {
            path = append(path, op);
            path = strconv.AppendInt(path, int64(arg), 10L);
            return path;
        }

        // find finds obj within type T, returning the path to it, or nil if not found.
        private static slice<byte> find(types.Object obj, types.Type T, slice<byte> path) => func((_, panic, __) =>
        {
            switch (T.type())
            {
                case ptr<types.Basic> T:
                    return null;
                    break;
                case ptr<types.Named> T:
                    return null;
                    break;
                case ptr<types.Pointer> T:
                    return find(obj, T.Elem(), append(path, opElem));
                    break;
                case ptr<types.Slice> T:
                    return find(obj, T.Elem(), append(path, opElem));
                    break;
                case ptr<types.Array> T:
                    return find(obj, T.Elem(), append(path, opElem));
                    break;
                case ptr<types.Chan> T:
                    return find(obj, T.Elem(), append(path, opElem));
                    break;
                case ptr<types.Map> T:
                    {
                        var r__prev1 = r;

                        var r = find(obj, T.Key(), append(path, opKey));

                        if (r != null)
                        {
                            return r;
                        }

                        r = r__prev1;

                    }

                    return find(obj, T.Elem(), append(path, opElem));
                    break;
                case ptr<types.Signature> T:
                    {
                        var r__prev1 = r;

                        r = find(obj, T.Params(), append(path, opParams));

                        if (r != null)
                        {
                            return r;
                        }

                        r = r__prev1;

                    }

                    return find(obj, T.Results(), append(path, opResults));
                    break;
                case ptr<types.Struct> T:
                    {
                        long i__prev1 = i;

                        for (long i = 0L; i < T.NumFields(); i++)
                        {
                            var f = T.Field(i);
                            var path2 = appendOpArg(path, opField, i);
                            if (f == obj)
                            {
                                return path2; // found field var
                            }

                            {
                                var r__prev1 = r;

                                r = find(obj, f.Type(), append(path2, opType));

                                if (r != null)
                                {
                                    return r;
                                }

                                r = r__prev1;

                            }

                        }


                        i = i__prev1;
                    }
                    return null;
                    break;
                case ptr<types.Tuple> T:
                    {
                        long i__prev1 = i;

                        for (i = 0L; i < T.Len(); i++)
                        {
                            var v = T.At(i);
                            path2 = appendOpArg(path, opAt, i);
                            if (v == obj)
                            {
                                return path2; // found param/result var
                            }

                            {
                                var r__prev1 = r;

                                r = find(obj, v.Type(), append(path2, opType));

                                if (r != null)
                                {
                                    return r;
                                }

                                r = r__prev1;

                            }

                        }


                        i = i__prev1;
                    }
                    return null;
                    break;
                case ptr<types.Interface> T:
                    {
                        long i__prev1 = i;

                        for (i = 0L; i < T.NumMethods(); i++)
                        {
                            var m = T.Method(i);
                            path2 = appendOpArg(path, opMethod, i);
                            if (m == obj)
                            {
                                return path2; // found interface method
                            }

                            {
                                var r__prev1 = r;

                                r = find(obj, m.Type(), append(path2, opType));

                                if (r != null)
                                {
                                    return r;
                                }

                                r = r__prev1;

                            }

                        }


                        i = i__prev1;
                    }
                    return null;
                    break;
            }
            panic(T);

        });

        // Object returns the object denoted by path p within the package pkg.
        public static (types.Object, error) Object(ptr<types.Package> _addr_pkg, Path p)
        {
            types.Object _p0 = default;
            error _p0 = default!;
            ref types.Package pkg = ref _addr_pkg.val;

            if (p == "")
            {
                return (null, error.As(fmt.Errorf("empty path"))!);
            }

            var pathstr = string(p);
            @string pkgobj = default;            @string suffix = default;

            {
                var dot = strings.IndexByte(pathstr, opType);

                if (dot < 0L)
                {
                    pkgobj = pathstr;
                }
                else
                {
                    pkgobj = pathstr[..dot];
                    suffix = pathstr[dot..]; // suffix starts with "."
                }

            }


            var obj = pkg.Scope().Lookup(pkgobj);
            if (obj == null)
            {
                return (null, error.As(fmt.Errorf("package %s does not contain %q", pkg.Path(), pkgobj))!);
            } 

            // abstraction of *types.{Pointer,Slice,Array,Chan,Map}
            private partial interface hasElem
            {
                types.Type Elem();
            } 
            // abstraction of *types.{Interface,Named}
            private partial interface hasMethods
            {
                long Method(long _p0);
                long NumMethods();
            } 

            // The loop state is the pair (t, obj),
            // exactly one of which is non-nil, initially obj.
            // All suffixes start with '.' (the only object->type operation),
            // followed by optional type->type operations,
            // then a type->object operation.
            // The cycle then repeats.
            types.Type t = default;
            while (suffix != "")
            {
                var code = suffix[0L];
                suffix = suffix[1L..]; 

                // Codes [AFM] have an integer operand.
                long index = default;

                if (code == opAt || code == opField || code == opMethod) 
                    var rest = strings.TrimLeft(suffix, "0123456789");
                    var numerals = suffix[..len(suffix) - len(rest)];
                    suffix = rest;
                    var (i, err) = strconv.Atoi(numerals);
                    if (err != null)
                    {
                        return (null, error.As(fmt.Errorf("invalid path: bad numeric operand %q for code %q", numerals, code))!);
                    }

                    index = int(i);
                else if (code == opObj)                 else 
                    // The suffix must end with a type->object operation.
                    if (suffix == "")
                    {
                        return (null, error.As(fmt.Errorf("invalid path: ends with %q, want [AFMO]", code))!);
                    }

                                if (code == opType)
                {
                    if (t != null)
                    {
                        return (null, error.As(fmt.Errorf("invalid path: unexpected %q in type context", opType))!);
                    }

                    t = obj.Type();
                    obj = null;
                    continue;

                }

                if (t == null)
                {
                    return (null, error.As(fmt.Errorf("invalid path: code %q in object context", code))!);
                } 

                // Inv: t != nil, obj == nil

                if (code == opElem) 
                    hasElem (hasElem, ok) = hasElem.As(t._<hasElem>())!; // Pointer, Slice, Array, Chan, Map
                    if (!ok)
                    {
                        return (null, error.As(fmt.Errorf("cannot apply %q to %s (got %T, want pointer, slice, array, chan or map)", code, t, t))!);
                    }

                    t = hasElem.Elem();
                else if (code == opKey) 
                    ptr<types.Map> (mapType, ok) = t._<ptr<types.Map>>();
                    if (!ok)
                    {
                        return (null, error.As(fmt.Errorf("cannot apply %q to %s (got %T, want map)", code, t, t))!);
                    }

                    t = mapType.Key();
                else if (code == opParams) 
                    ptr<types.Signature> (sig, ok) = t._<ptr<types.Signature>>();
                    if (!ok)
                    {
                        return (null, error.As(fmt.Errorf("cannot apply %q to %s (got %T, want signature)", code, t, t))!);
                    }

                    t = sig.Params();
                else if (code == opResults) 
                    (sig, ok) = t._<ptr<types.Signature>>();
                    if (!ok)
                    {
                        return (null, error.As(fmt.Errorf("cannot apply %q to %s (got %T, want signature)", code, t, t))!);
                    }

                    t = sig.Results();
                else if (code == opUnderlying) 
                    ptr<types.Named> (named, ok) = t._<ptr<types.Named>>();
                    if (!ok)
                    {
                        return (null, error.As(fmt.Errorf("cannot apply %q to %s (got %s, want named)", code, t, t))!);
                    }

                    t = named.Underlying();
                else if (code == opAt) 
                    ptr<types.Tuple> (tuple, ok) = t._<ptr<types.Tuple>>();
                    if (!ok)
                    {
                        return (null, error.As(fmt.Errorf("cannot apply %q to %s (got %s, want tuple)", code, t, t))!);
                    }

                    {
                        var n__prev1 = n;

                        var n = tuple.Len();

                        if (index >= n)
                        {
                            return (null, error.As(fmt.Errorf("tuple index %d out of range [0-%d)", index, n))!);
                        }

                        n = n__prev1;

                    }

                    obj = tuple.At(index);
                    t = null;
                else if (code == opField) 
                    ptr<types.Struct> (structType, ok) = t._<ptr<types.Struct>>();
                    if (!ok)
                    {
                        return (null, error.As(fmt.Errorf("cannot apply %q to %s (got %T, want struct)", code, t, t))!);
                    }

                    {
                        var n__prev1 = n;

                        n = structType.NumFields();

                        if (index >= n)
                        {
                            return (null, error.As(fmt.Errorf("field index %d out of range [0-%d)", index, n))!);
                        }

                        n = n__prev1;

                    }

                    obj = structType.Field(index);
                    t = null;
                else if (code == opMethod) 
                    hasMethods (hasMethods, ok) = hasMethods.As(t._<hasMethods>())!; // Interface or Named
                    if (!ok)
                    {
                        return (null, error.As(fmt.Errorf("cannot apply %q to %s (got %s, want interface or named)", code, t, t))!);
                    }

                    {
                        var n__prev1 = n;

                        n = hasMethods.NumMethods();

                        if (index >= n)
                        {
                            return (null, error.As(fmt.Errorf("method index %d out of range [0-%d)", index, n))!);
                        }

                        n = n__prev1;

                    }

                    obj = hasMethods.Method(index);
                    t = null;
                else if (code == opObj) 
                    (named, ok) = t._<ptr<types.Named>>();
                    if (!ok)
                    {
                        return (null, error.As(fmt.Errorf("cannot apply %q to %s (got %s, want named)", code, t, t))!);
                    }

                    obj = named.Obj();
                    t = null;
                else 
                    return (null, error.As(fmt.Errorf("invalid path: unknown code %q", code))!);
                
            }


            if (obj.Pkg() != pkg)
            {
                return (null, error.As(fmt.Errorf("path denotes %s, which belongs to a different package", obj))!);
            }

            return (obj, error.As(null!)!); // success
        }
    }
}}}}}}
