// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gcimporter -- go2cs converted at 2020 August 29 10:09:09 UTC
// import "go/internal/gcimporter" ==> using gcimporter = go.go.@internal.gcimporter_package
// Original source: C:\Go\src\go\internal\gcimporter\bimport.go
using binary = go.encoding.binary_package;
using fmt = go.fmt_package;
using constant = go.go.constant_package;
using token = go.go.token_package;
using types = go.go.types_package;
using sort = go.sort_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using sync = go.sync_package;
using unicode = go.unicode_package;
using utf8 = go.unicode.utf8_package;
using static go.builtin;
using System;

namespace go {
namespace go {
namespace @internal
{
    public static partial class gcimporter_package
    {
        private partial struct importer
        {
            public map<@string, ref types.Package> imports;
            public slice<byte> data;
            public @string importpath;
            public slice<byte> buf; // for reading strings
            public long version; // export format version

// object lists
            public slice<@string> strList; // in order of appearance
            public slice<@string> pathList; // in order of appearance
            public slice<ref types.Package> pkgList; // in order of appearance
            public slice<types.Type> typList; // in order of appearance
            public slice<ref types.Interface> interfaceList; // for delayed completion only
            public bool trackAllTypes; // position encoding
            public bool posInfoFormat;
            public @string prevFile;
            public long prevLine;
            public ptr<token.FileSet> fset;
            public map<@string, ref token.File> files; // debugging support
            public bool debugFormat;
            public long read; // bytes read
        }

        // BImportData imports a package from the serialized package data
        // and returns the number of bytes consumed and a reference to the package.
        // If the export data version is not recognized or the format is otherwise
        // compromised, an error is returned.
        public static (long, ref types.Package, error) BImportData(ref token.FileSet _fset, map<@string, ref types.Package> imports, slice<byte> data, @string path) => func(_fset, (ref token.FileSet fset, Defer defer, Panic _, Recover __) =>
        { 
            // catch panics and return them as errors
            defer(() =>
            {
                {
                    var e = recover();

                    if (e != null)
                    { 
                        // The package (filename) causing the problem is added to this
                        // error by a wrapper in the caller (Import in gcimporter.go).
                        // Return a (possibly nil or incomplete) package unchanged (see #16088).
                        err = fmt.Errorf("cannot import, possibly version skew (%v) - reinstall package", e);
                    }

                }
            }());

            importer p = new importer(imports:imports,data:data,importpath:path,version:-1,strList:[]string{""},pathList:[]string{""},fset:fset,files:make(map[string]*token.File),); 

            // read version info
            @string versionstr = default;
            {
                var b = p.rawByte();

                if (b == 'c' || b == 'd')
                { 
                    // Go1.7 encoding; first byte encodes low-level
                    // encoding format (compact vs debug).
                    // For backward-compatibility only (avoid problems with
                    // old installed packages). Newly compiled packages use
                    // the extensible format string.
                    // TODO(gri) Remove this support eventually; after Go1.8.
                    if (b == 'd')
                    {
                        p.debugFormat = true;
                    }
                    p.trackAllTypes = p.rawByte() == 'a';
                    p.posInfoFormat = p.@int() != 0L;
                    versionstr = p.@string();
                    if (versionstr == "v1")
                    {
                        p.version = 0L;
                    }
                }
                else
                { 
                    // Go1.8 extensible encoding
                    // read version string and extract version number (ignore anything after the version number)
                    versionstr = p.rawStringln(b);
                    {
                        var s = strings.SplitN(versionstr, " ", 3L);

                        if (len(s) >= 2L && s[0L] == "version")
                        {
                            {
                                var (v, err) = strconv.Atoi(s[1L]);

                                if (err == null && v > 0L)
                                {
                                    p.version = v;
                                }

                            }
                        }

                    }
                } 

                // read version specific flags - extend as necessary

            } 

            // read version specific flags - extend as necessary
            switch (p.version)
            { 
            // case 6:
            //     ...
            //    fallthrough
                case 5L: 

                case 4L: 

                case 3L: 

                case 2L: 

                case 1L: 
                    p.debugFormat = p.rawStringln(p.rawByte()) == "debug";
                    p.trackAllTypes = p.@int() != 0L;
                    p.posInfoFormat = p.@int() != 0L;
                    break;
                case 0L: 
                    break;
                default: 
                    errorf("unknown export format version %d (%q)", p.version, versionstr);
                    break;
            } 

            // --- generic export data ---

            // populate typList with predeclared "known" types
            p.typList = append(p.typList, predeclared); 

            // read package data
            pkg = p.pkg(); 

            // read objects of phase 1 only (see cmd/compile/internal/gc/bexport.go)
            long objcount = 0L;
            while (true)
            {
                var tag = p.tagOrIndex();
                if (tag == endTag)
                {
                    break;
                }
                p.obj(tag);
                objcount++;
            } 

            // self-verification
 

            // self-verification
            {
                var count = p.@int();

                if (count != objcount)
                {
                    errorf("got %d objects; want %d", objcount, count);
                } 

                // ignore compiler-specific import data

                // complete interfaces
                // TODO(gri) re-investigate if we still need to do this in a delayed fashion

            } 

            // ignore compiler-specific import data

            // complete interfaces
            // TODO(gri) re-investigate if we still need to do this in a delayed fashion
            foreach (var (_, typ) in p.interfaceList)
            {
                typ.Complete();
            } 

            // record all referenced packages as imports
            var list = append((slice<ref types.Package>)null, p.pkgList[1L..]);
            sort.Sort(byPath(list));
            pkg.SetImports(list); 

            // package was imported completely and without errors
            pkg.MarkComplete();

            return (p.read, pkg, null);
        });

        private static void errorf(@string format, params object[] args) => func((_, panic, __) =>
        {
            args = args.Clone();

            panic(fmt.Sprintf(format, args));
        });

        private static ref types.Package pkg(this ref importer p)
        { 
            // if the package was seen before, i is its index (>= 0)
            var i = p.tagOrIndex();
            if (i >= 0L)
            {
                return p.pkgList[i];
            } 

            // otherwise, i is the package tag (< 0)
            if (i != packageTag)
            {
                errorf("unexpected package tag %d version %d", i, p.version);
            } 

            // read package data
            var name = p.@string();
            @string path = default;
            if (p.version >= 5L)
            {
                path = p.path();
            }
            else
            {
                path = p.@string();
            } 

            // we should never see an empty package name
            if (name == "")
            {
                errorf("empty package name in import");
            } 

            // an empty path denotes the package we are currently importing;
            // it must be the first package we see
            if ((path == "") != (len(p.pkgList) == 0L))
            {
                errorf("package path %q for pkg index %d", path, len(p.pkgList));
            } 

            // if the package was imported before, use that one; otherwise create a new one
            if (path == "")
            {
                path = p.importpath;
            }
            var pkg = p.imports[path];
            if (pkg == null)
            {
                pkg = types.NewPackage(path, name);
                p.imports[path] = pkg;
            }
            else if (pkg.Name() != name)
            {
                errorf("conflicting names %s and %s for package %q", pkg.Name(), name, path);
            }
            p.pkgList = append(p.pkgList, pkg);

            return pkg;
        }

        // objTag returns the tag value for each object kind.
        private static long objTag(types.Object obj) => func((_, panic, __) =>
        {
            switch (obj.type())
            {
                case ref types.Const _:
                    return constTag;
                    break;
                case ref types.TypeName _:
                    return typeTag;
                    break;
                case ref types.Var _:
                    return varTag;
                    break;
                case ref types.Func _:
                    return funcTag;
                    break;
                default:
                {
                    errorf("unexpected object: %v (%T)", obj, obj); // panics
                    panic("unreachable");
                    break;
                }
            }
        });

        private static bool sameObj(types.Object a, types.Object b)
        { 
            // Because unnamed types are not canonicalized, we cannot simply compare types for
            // (pointer) identity.
            // Ideally we'd check equality of constant values as well, but this is good enough.
            return objTag(a) == objTag(b) && types.Identical(a.Type(), b.Type());
        }

        private static void declare(this ref importer p, types.Object obj)
        {
            var pkg = obj.Pkg();
            {
                var alt = pkg.Scope().Insert(obj);

                if (alt != null)
                { 
                    // This can only trigger if we import a (non-type) object a second time.
                    // Excluding type aliases, this cannot happen because 1) we only import a package
                    // once; and b) we ignore compiler-specific export data which may contain
                    // functions whose inlined function bodies refer to other functions that
                    // were already imported.
                    // However, type aliases require reexporting the original type, so we need
                    // to allow it (see also the comment in cmd/compile/internal/gc/bimport.go,
                    // method importer.obj, switch case importing functions).
                    // TODO(gri) review/update this comment once the gc compiler handles type aliases.
                    if (!sameObj(obj, alt))
                    {
                        errorf("inconsistent import:\n\t%v\npreviously imported as:\n\t%v\n", obj, alt);
                    }
                }

            }
        }

        private static void obj(this ref importer p, long tag)
        {

            if (tag == constTag) 
                var pos = p.pos();
                var (pkg, name) = p.qualifiedName();
                var typ = p.typ(null);
                var val = p.value();
                p.declare(types.NewConst(pos, pkg, name, typ, val));
            else if (tag == aliasTag) 
                // TODO(gri) verify type alias hookup is correct
                pos = p.pos();
                (pkg, name) = p.qualifiedName();
                typ = p.typ(null);
                p.declare(types.NewTypeName(pos, pkg, name, typ));
            else if (tag == typeTag) 
                p.typ(null);
            else if (tag == varTag) 
                pos = p.pos();
                (pkg, name) = p.qualifiedName();
                typ = p.typ(null);
                p.declare(types.NewVar(pos, pkg, name, typ));
            else if (tag == funcTag) 
                pos = p.pos();
                (pkg, name) = p.qualifiedName();
                var (params, isddd) = p.paramList();
                var (result, _) = p.paramList();
                var sig = types.NewSignature(null, params, result, isddd);
                p.declare(types.NewFunc(pos, pkg, name, sig));
            else 
                errorf("unexpected object tag %d", tag);
                    }

        private static readonly long deltaNewFile = -64L; // see cmd/compile/internal/gc/bexport.go

 // see cmd/compile/internal/gc/bexport.go

        private static token.Pos pos(this ref importer p)
        {
            if (!p.posInfoFormat)
            {
                return token.NoPos;
            }
            var file = p.prevFile;
            var line = p.prevLine;
            var delta = p.@int();
            line += delta;
            if (p.version >= 5L)
            {
                if (delta == deltaNewFile)
                {
                    {
                        var n__prev3 = n;

                        var n = p.@int();

                        if (n >= 0L)
                        { 
                            // file changed
                            file = p.path();
                            line = n;
                        }

                        n = n__prev3;

                    }
                }
            }
            else
            {
                if (delta == 0L)
                {
                    {
                        var n__prev3 = n;

                        n = p.@int();

                        if (n >= 0L)
                        { 
                            // file changed
                            file = p.prevFile[..n] + p.@string();
                            line = p.@int();
                        }

                        n = n__prev3;

                    }
                }
            }
            p.prevFile = file;
            p.prevLine = line; 

            // Synthesize a token.Pos

            // Since we don't know the set of needed file positions, we
            // reserve maxlines positions per file.
            const long maxlines = 64L * 1024L;

            var f = p.files[file];
            if (f == null)
            {
                f = p.fset.AddFile(file, -1L, maxlines);
                p.files[file] = f; 
                // Allocate the fake linebreak indices on first use.
                // TODO(adonovan): opt: save ~512KB using a more complex scheme?
                fakeLinesOnce.Do(() =>
                {
                    fakeLines = make_slice<long>(maxlines);
                    foreach (var (i) in fakeLines)
                    {
                        fakeLines[i] = i;
                    }
                });
                f.SetLines(fakeLines);
            }
            if (line > maxlines)
            {
                line = 1L;
            } 

            // Treat the file as if it contained only newlines
            // and column=1: use the line number as the offset.
            return f.Pos(line - 1L);
        }

        private static slice<long> fakeLines = default;        private static sync.Once fakeLinesOnce = default;

        private static (ref types.Package, @string) qualifiedName(this ref importer p)
        {
            name = p.@string();
            pkg = p.pkg();
            return;
        }

        private static void record(this ref importer p, types.Type t)
        {
            p.typList = append(p.typList, t);
        }

        // A dddSlice is a types.Type representing ...T parameters.
        // It only appears for parameter types and does not escape
        // the importer.
        private partial struct dddSlice
        {
            public types.Type elem;
        }

        private static types.Type Underlying(this ref dddSlice t)
        {
            return t;
        }
        private static @string String(this ref dddSlice t)
        {
            return "..." + t.elem.String();
        }

        // parent is the package which declared the type; parent == nil means
        // the package currently imported. The parent package is needed for
        // exported struct fields and interface methods which don't contain
        // explicit package information in the export data.
        private static types.Type typ(this ref importer _p, ref types.Package _parent) => func(_p, _parent, (ref importer p, ref types.Package parent, Defer _, Panic panic, Recover __) =>
        { 
            // if the type was seen before, i is its index (>= 0)
            var i = p.tagOrIndex();
            if (i >= 0L)
            {
                return p.typList[i];
            } 

            // otherwise, i is the type tag (< 0)

            if (i == namedTag) 
                // read type object
                var pos = p.pos();
                var (parent, name) = p.qualifiedName();
                var scope = parent.Scope();
                var obj = scope.Lookup(name); 

                // if the object doesn't exist yet, create and insert it
                if (obj == null)
                {
                    obj = types.NewTypeName(pos, parent, name, null);
                    scope.Insert(obj);
                }
                {
                    ref types.TypeName (_, ok) = obj._<ref types.TypeName>();

                    if (!ok)
                    {
                        errorf("pkg = %s, name = %s => %s", parent, name, obj);
                    } 

                    // associate new named type with obj if it doesn't exist yet

                } 

                // associate new named type with obj if it doesn't exist yet
                var t0 = types.NewNamed(obj._<ref types.TypeName>(), null, null); 

                // but record the existing type, if any
                ref types.Named t = obj.Type()._<ref types.Named>();
                p.record(t); 

                // read underlying type
                t0.SetUnderlying(p.typ(parent)); 

                // interfaces don't have associated methods
                if (types.IsInterface(t0))
                {
                    return t;
                } 

                // read associated methods
                {
                    var i__prev1 = i;

                    for (i = p.@int(); i > 0L; i--)
                    { 
                        // TODO(gri) replace this with something closer to fieldName
                        pos = p.pos();
                        var name = p.@string();
                        if (!exported(name))
                        {
                            p.pkg();
                        }
                        var (recv, _) = p.paramList(); // TODO(gri) do we need a full param list for the receiver?
                        var (params, isddd) = p.paramList();
                        var (result, _) = p.paramList();
                        p.@int(); // go:nointerface pragma - discarded

                        var sig = types.NewSignature(recv.At(0L), params, result, isddd);
                        t0.AddMethod(types.NewFunc(pos, parent, name, sig));
                    }


                    i = i__prev1;
                }

                return t;
            else if (i == arrayTag) 
                t = @new<types.Array>();
                if (p.trackAllTypes)
                {
                    p.record(t);
                }
                var n = p.int64();
                t.Value = types.NewArray(p.typ(parent), n).Value;
                return t;
            else if (i == sliceTag) 
                t = @new<types.Slice>();
                if (p.trackAllTypes)
                {
                    p.record(t);
                }
                t.Value = new ptr<ref types.NewSlice>(p.typ(parent));
                return t;
            else if (i == dddTag) 
                t = @new<dddSlice>();
                if (p.trackAllTypes)
                {
                    p.record(t);
                }
                t.elem = p.typ(parent);
                return t;
            else if (i == structTag) 
                t = @new<types.Struct>();
                if (p.trackAllTypes)
                {
                    p.record(t);
                }
                t.Value = new ptr<ref types.NewStruct>(p.fieldList(parent));
                return t;
            else if (i == pointerTag) 
                t = @new<types.Pointer>();
                if (p.trackAllTypes)
                {
                    p.record(t);
                }
                t.Value = new ptr<ref types.NewPointer>(p.typ(parent));
                return t;
            else if (i == signatureTag) 
                t = @new<types.Signature>();
                if (p.trackAllTypes)
                {
                    p.record(t);
                }
                (params, isddd) = p.paramList();
                (result, _) = p.paramList();
                t.Value = types.NewSignature(null, params, result, isddd).Value;
                return t;
            else if (i == interfaceTag) 
                // Create a dummy entry in the type list. This is safe because we
                // cannot expect the interface type to appear in a cycle, as any
                // such cycle must contain a named type which would have been
                // first defined earlier.
                n = len(p.typList);
                if (p.trackAllTypes)
                {
                    p.record(null);
                }
                slice<ref types.Named> embeddeds = default;
                {
                    var n__prev1 = n;

                    for (n = p.@int(); n > 0L; n--)
                    {
                        p.pos();
                        embeddeds = append(embeddeds, p.typ(parent)._<ref types.Named>());
                    }


                    n = n__prev1;
                }

                t = types.NewInterface(p.methodList(parent), embeddeds);
                p.interfaceList = append(p.interfaceList, t);
                if (p.trackAllTypes)
                {
                    p.typList[n] = t;
                }
                return t;
            else if (i == mapTag) 
                t = @new<types.Map>();
                if (p.trackAllTypes)
                {
                    p.record(t);
                }
                var key = p.typ(parent);
                var val = p.typ(parent);
                t.Value = types.NewMap(key, val).Value;
                return t;
            else if (i == chanTag) 
                t = @new<types.Chan>();
                if (p.trackAllTypes)
                {
                    p.record(t);
                }
                types.ChanDir dir = default; 
                // tag values must match the constants in cmd/compile/internal/gc/go.go
                {
                    var d = p.@int();

                    switch (d)
                    {
                        case 1L: 
                            dir = types.RecvOnly;
                            break;
                        case 2L: 
                            dir = types.SendOnly;
                            break;
                        case 3L: 
                            dir = types.SendRecv;
                            break;
                        default: 
                            errorf("unexpected channel dir %d", d);
                            break;
                    }
                }
                val = p.typ(parent);
                t.Value = types.NewChan(dir, val).Value;
                return t;
            else 
                errorf("unexpected type tag %d", i); // panics
                panic("unreachable");
                    });

        private static (slice<ref types.Var>, slice<@string>) fieldList(this ref importer p, ref types.Package parent)
        {
            {
                var n = p.@int();

                if (n > 0L)
                {
                    fields = make_slice<ref types.Var>(n);
                    tags = make_slice<@string>(n);
                    foreach (var (i) in fields)
                    {
                        fields[i], tags[i] = p.field(parent);
                    }
                }

            }
            return;
        }

        private static (ref types.Var, @string) field(this ref importer p, ref types.Package parent)
        {
            var pos = p.pos();
            var (pkg, name, alias) = p.fieldName(parent);
            var typ = p.typ(parent);
            var tag = p.@string();

            var anonymous = false;
            if (name == "")
            { 
                // anonymous field - typ must be T or *T and T must be a type name
                switch (deref(typ).type())
                {
                    case ref types.Basic typ:
                        pkg = null; // // objects defined in Universe scope have no package
                        name = typ.Name();
                        break;
                    case ref types.Named typ:
                        name = typ.Obj().Name();
                        break;
                    default:
                    {
                        var typ = deref(typ).type();
                        errorf("named base type expected");
                        break;
                    }
                }
                anonymous = true;
            }
            else if (alias)
            { 
                // anonymous field: we have an explicit name because it's an alias
                anonymous = true;
            }
            return (types.NewField(pos, pkg, name, typ, anonymous), tag);
        }

        private static slice<ref types.Func> methodList(this ref importer p, ref types.Package parent)
        {
            {
                var n = p.@int();

                if (n > 0L)
                {
                    methods = make_slice<ref types.Func>(n);
                    foreach (var (i) in methods)
                    {
                        methods[i] = p.method(parent);
                    }
                }

            }
            return;
        }

        private static ref types.Func method(this ref importer p, ref types.Package parent)
        {
            var pos = p.pos();
            var (pkg, name, _) = p.fieldName(parent);
            var (params, isddd) = p.paramList();
            var (result, _) = p.paramList();
            var sig = types.NewSignature(null, params, result, isddd);
            return types.NewFunc(pos, pkg, name, sig);
        }

        private static (ref types.Package, @string, bool) fieldName(this ref importer p, ref types.Package parent)
        {
            name = p.@string();
            pkg = parent;
            if (pkg == null)
            { 
                // use the imported package instead
                pkg = p.pkgList[0L];
            }
            if (p.version == 0L && name == "_")
            { 
                // version 0 didn't export a package for _ fields
                return;
            }

            if (name == "")
            {
                goto __switch_break0;
            }
            if (name == "?") 
            {
                // 2) field name matches base type name and is not exported: need package
                name = "";
                pkg = p.pkg();
                goto __switch_break0;
            }
            if (name == "@") 
            {
                // 3) field name doesn't match type name (alias)
                name = p.@string();
                alias = true;
            }
            // default: 
                if (!exported(name))
                {
                    pkg = p.pkg();
                }

            __switch_break0:;
            return;
        }

        private static (ref types.Tuple, bool) paramList(this ref importer p)
        {
            var n = p.@int();
            if (n == 0L)
            {
                return (null, false);
            } 
            // negative length indicates unnamed parameters
            var named = true;
            if (n < 0L)
            {
                n = -n;
                named = false;
            } 
            // n > 0
            var @params = make_slice<ref types.Var>(n);
            var isddd = false;
            foreach (var (i) in params)
            {
                params[i], isddd = p.param(named);
            }
            return (types.NewTuple(params), isddd);
        }

        private static (ref types.Var, bool) param(this ref importer p, bool named)
        {
            var t = p.typ(null);
            ref dddSlice (td, isddd) = t._<ref dddSlice>();
            if (isddd)
            {
                t = types.NewSlice(td.elem);
            }
            ref types.Package pkg = default;
            @string name = default;
            if (named)
            {
                name = p.@string();
                if (name == "")
                {
                    errorf("expected named parameter");
                }
                if (name != "_")
                {
                    pkg = p.pkg();
                }
                {
                    var i = strings.Index(name, "·");

                    if (i > 0L)
                    {
                        name = name[..i]; // cut off gc-specific parameter numbering
                    }

                }
            } 

            // read and discard compiler-specific info
            p.@string();

            return (types.NewVar(token.NoPos, pkg, name, t), isddd);
        }

        private static bool exported(@string name)
        {
            var (ch, _) = utf8.DecodeRuneInString(name);
            return unicode.IsUpper(ch);
        }

        private static constant.Value value(this ref importer _p) => func(_p, (ref importer p, Defer _, Panic panic, Recover __) =>
        {
            {
                var tag = p.tagOrIndex();


                if (tag == falseTag) 
                    return constant.MakeBool(false);
                else if (tag == trueTag) 
                    return constant.MakeBool(true);
                else if (tag == int64Tag) 
                    return constant.MakeInt64(p.int64());
                else if (tag == floatTag) 
                    return p.@float();
                else if (tag == complexTag) 
                    var re = p.@float();
                    var im = p.@float();
                    return constant.BinaryOp(re, token.ADD, constant.MakeImag(im));
                else if (tag == stringTag) 
                    return constant.MakeString(p.@string());
                else if (tag == unknownTag) 
                    return constant.MakeUnknown();
                else 
                    errorf("unexpected value tag %d", tag); // panics
                    panic("unreachable");

            }
        });

        private static constant.Value @float(this ref importer p)
        {
            var sign = p.@int();
            if (sign == 0L)
            {
                return constant.MakeInt64(0L);
            }
            var exp = p.@int();
            slice<byte> mant = (slice<byte>)p.@string(); // big endian

            // remove leading 0's if any
            while (len(mant) > 0L && mant[0L] == 0L)
            {
                mant = mant[1L..];
            } 

            // convert to little endian
            // TODO(gri) go/constant should have a more direct conversion function
            //           (e.g., once it supports a big.Float based implementation)
 

            // convert to little endian
            // TODO(gri) go/constant should have a more direct conversion function
            //           (e.g., once it supports a big.Float based implementation)
            {
                long i = 0L;
                var j = len(mant) - 1L;

                while (i < j)
                {
                    mant[i] = mant[j];
                    mant[j] = mant[i];
                    i = i + 1L;
                j = j - 1L;
                } 

                // adjust exponent (constant.MakeFromBytes creates an integer value,
                // but mant represents the mantissa bits such that 0.5 <= mant < 1.0)

            } 

            // adjust exponent (constant.MakeFromBytes creates an integer value,
            // but mant represents the mantissa bits such that 0.5 <= mant < 1.0)
            exp -= len(mant) << (int)(3L);
            if (len(mant) > 0L)
            {
                {
                    var msd = mant[len(mant) - 1L];

                    while (msd & 0x80UL == 0L)
                    {
                        exp++;
                        msd <<= 1L;
                    }

                }
            }
            var x = constant.MakeFromBytes(mant);

            if (exp < 0L) 
                var d = constant.Shift(constant.MakeInt64(1L), token.SHL, uint(-exp));
                x = constant.BinaryOp(x, token.QUO, d);
            else if (exp > 0L) 
                x = constant.Shift(x, token.SHL, uint(exp));
                        if (sign < 0L)
            {
                x = constant.UnaryOp(token.SUB, x, 0L);
            }
            return x;
        }

        // ----------------------------------------------------------------------------
        // Low-level decoders

        private static long tagOrIndex(this ref importer p)
        {
            if (p.debugFormat)
            {
                p.marker('t');
            }
            return int(p.rawInt64());
        }

        private static long @int(this ref importer p)
        {
            var x = p.int64();
            if (int64(int(x)) != x)
            {
                errorf("exported integer too large");
            }
            return int(x);
        }

        private static long int64(this ref importer p)
        {
            if (p.debugFormat)
            {
                p.marker('i');
            }
            return p.rawInt64();
        }

        private static @string path(this ref importer p)
        {
            if (p.debugFormat)
            {
                p.marker('p');
            } 
            // if the path was seen before, i is its index (>= 0)
            // (the empty string is at index 0)
            var i = p.rawInt64();
            if (i >= 0L)
            {
                return p.pathList[i];
            } 
            // otherwise, i is the negative path length (< 0)
            var a = make_slice<@string>(-i);
            foreach (var (n) in a)
            {
                a[n] = p.@string();
            }
            var s = strings.Join(a, "/");
            p.pathList = append(p.pathList, s);
            return s;
        }

        private static @string @string(this ref importer p)
        {
            if (p.debugFormat)
            {
                p.marker('s');
            } 
            // if the string was seen before, i is its index (>= 0)
            // (the empty string is at index 0)
            var i = p.rawInt64();
            if (i >= 0L)
            {
                return p.strList[i];
            } 
            // otherwise, i is the negative string length (< 0)
            {
                var n = int(-i);

                if (n <= cap(p.buf))
                {
                    p.buf = p.buf[..n];
                }
                else
                {
                    p.buf = make_slice<byte>(n);
                }

            }
            {
                var i__prev1 = i;

                foreach (var (__i) in p.buf)
                {
                    i = __i;
                    p.buf[i] = p.rawByte();
                }

                i = i__prev1;
            }

            var s = string(p.buf);
            p.strList = append(p.strList, s);
            return s;
        }

        private static void marker(this ref importer p, byte want)
        {
            {
                var got = p.rawByte();

                if (got != want)
                {
                    errorf("incorrect marker: got %c; want %c (pos = %d)", got, want, p.read);
                }

            }

            var pos = p.read;
            {
                var n = int(p.rawInt64());

                if (n != pos)
                {
                    errorf("incorrect position: got %d; want %d", n, pos);
                }

            }
        }

        // rawInt64 should only be used by low-level decoders.
        private static long rawInt64(this ref importer p)
        {
            var (i, err) = binary.ReadVarint(p);
            if (err != null)
            {
                errorf("read error: %v", err);
            }
            return i;
        }

        // rawStringln should only be used to read the initial version string.
        private static @string rawStringln(this ref importer p, byte b)
        {
            p.buf = p.buf[..0L];
            while (b != '\n')
            {
                p.buf = append(p.buf, b);
                b = p.rawByte();
            }

            return string(p.buf);
        }

        // needed for binary.ReadVarint in rawInt64
        private static (byte, error) ReadByte(this ref importer p)
        {
            return (p.rawByte(), null);
        }

        // byte is the bottleneck interface for reading p.data.
        // It unescapes '|' 'S' to '$' and '|' '|' to '|'.
        // rawByte should only be used by low-level decoders.
        private static byte rawByte(this ref importer p)
        {
            var b = p.data[0L];
            long r = 1L;
            if (b == '|')
            {
                b = p.data[1L];
                r = 2L;
                switch (b)
                {
                    case 'S': 
                        b = '$';
                        break;
                    case '|': 
                        break;
                    default: 
                        errorf("unexpected escape sequence in export data");
                        break;
                }
            }
            p.data = p.data[r..];
            p.read += r;
            return b;

        }

        // ----------------------------------------------------------------------------
        // Export format

        // Tags. Must be < 0.
 
        // Objects
        private static readonly var packageTag = -(iota + 1L);
        private static readonly var constTag = 0;
        private static readonly var typeTag = 1;
        private static readonly var varTag = 2;
        private static readonly var funcTag = 3;
        private static readonly var endTag = 4; 

        // Types
        private static readonly var namedTag = 5;
        private static readonly var arrayTag = 6;
        private static readonly var sliceTag = 7;
        private static readonly var dddTag = 8;
        private static readonly var structTag = 9;
        private static readonly var pointerTag = 10;
        private static readonly var signatureTag = 11;
        private static readonly var interfaceTag = 12;
        private static readonly var mapTag = 13;
        private static readonly var chanTag = 14; 

        // Values
        private static readonly var falseTag = 15;
        private static readonly var trueTag = 16;
        private static readonly var int64Tag = 17;
        private static readonly var floatTag = 18;
        private static readonly var fractionTag = 19; // not used by gc
        private static readonly var complexTag = 20;
        private static readonly var stringTag = 21;
        private static readonly var nilTag = 22; // only used by gc (appears in exported inlined function bodies)
        private static readonly var unknownTag = 23; // not used by gc (only appears in packages with errors)

        // Type aliases
        private static readonly var aliasTag = 24;

        private static types.Type predeclared = new slice<types.Type>(new types.Type[] { types.Typ[types.Bool], types.Typ[types.Int], types.Typ[types.Int8], types.Typ[types.Int16], types.Typ[types.Int32], types.Typ[types.Int64], types.Typ[types.Uint], types.Typ[types.Uint8], types.Typ[types.Uint16], types.Typ[types.Uint32], types.Typ[types.Uint64], types.Typ[types.Uintptr], types.Typ[types.Float32], types.Typ[types.Float64], types.Typ[types.Complex64], types.Typ[types.Complex128], types.Typ[types.String], types.Universe.Lookup("byte").Type(), types.Universe.Lookup("rune").Type(), types.Universe.Lookup("error").Type(), types.Typ[types.UntypedBool], types.Typ[types.UntypedInt], types.Typ[types.UntypedRune], types.Typ[types.UntypedFloat], types.Typ[types.UntypedComplex], types.Typ[types.UntypedString], types.Typ[types.UntypedNil], types.Typ[types.UnsafePointer], types.Typ[types.Invalid], anyType{} });

        private partial struct anyType
        {
        }

        private static types.Type Underlying(this anyType t)
        {
            return t;
        }
        private static @string String(this anyType t)
        {
            return "any";
        }
    }
}}}
