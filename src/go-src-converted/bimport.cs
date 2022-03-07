// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file is a copy of $GOROOT/src/go/internal/gcimporter/bimport.go.

// package gcimporter -- go2cs converted at 2022 March 06 23:32:01 UTC
// import "golang.org/x/tools/go/internal/gcimporter" ==> using gcimporter = go.golang.org.x.tools.go.@internal.gcimporter_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\internal\gcimporter\bimport.go
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
using System;


namespace go.golang.org.x.tools.go.@internal;

public static partial class gcimporter_package {

private partial struct importer {
    public map<@string, ptr<types.Package>> imports;
    public slice<byte> data;
    public @string importpath;
    public slice<byte> buf; // for reading strings
    public nint version; // export format version

// object lists
    public slice<@string> strList; // in order of appearance
    public slice<@string> pathList; // in order of appearance
    public slice<ptr<types.Package>> pkgList; // in order of appearance
    public slice<types.Type> typList; // in order of appearance
    public slice<ptr<types.Interface>> interfaceList; // for delayed completion only
    public bool trackAllTypes; // position encoding
    public bool posInfoFormat;
    public @string prevFile;
    public nint prevLine;
    public fakeFileSet fake; // debugging support
    public bool debugFormat;
    public nint read; // bytes read
}

// BImportData imports a package from the serialized package data
// and returns the number of bytes consumed and a reference to the package.
// If the export data version is not recognized or the format is otherwise
// compromised, an error is returned.
public static (nint, ptr<types.Package>, error) BImportData(ptr<token.FileSet> _addr_fset, map<@string, ptr<types.Package>> imports, slice<byte> data, @string path) => func((defer, _, _) => {
    nint _ = default;
    ptr<types.Package> pkg = default!;
    error err = default!;
    ref token.FileSet fset = ref _addr_fset.val;
 
    // catch panics and return them as errors
    const nint currentVersion = 6;

    nint version = -1; // unknown version
    defer(() => {
        {
            var e = recover();

            if (e != null) { 
                // Return a (possibly nil or incomplete) package unchanged (see #16088).
                if (version > currentVersion) {
                    err = fmt.Errorf("cannot import %q (%v), export data is newer version - update tool", path, e);
                }
                else
 {
                    err = fmt.Errorf("cannot import %q (%v), possibly version skew - reinstall package", path, e);
                }

            }

        }

    }());

    importer p = new importer(imports:imports,data:data,importpath:path,version:version,strList:[]string{""},pathList:[]string{""},fake:fakeFileSet{fset:fset,files:make(map[string]*token.File),},); 

    // read version info
    @string versionstr = default;
    {
        var b = p.rawByte();

        if (b == 'c' || b == 'd') { 
            // Go1.7 encoding; first byte encodes low-level
            // encoding format (compact vs debug).
            // For backward-compatibility only (avoid problems with
            // old installed packages). Newly compiled packages use
            // the extensible format string.
            // TODO(gri) Remove this support eventually; after Go1.8.
            if (b == 'd') {
                p.debugFormat = true;
            }

            p.trackAllTypes = p.rawByte() == 'a';
            p.posInfoFormat = p.@int() != 0;
            versionstr = p.@string();
            if (versionstr == "v1") {
                version = 0;
            }

        }
        else
 { 
            // Go1.8 extensible encoding
            // read version string and extract version number (ignore anything after the version number)
            versionstr = p.rawStringln(b);
            {
                var s = strings.SplitN(versionstr, " ", 3);

                if (len(s) >= 2 && s[0] == "version") {
                    {
                        var (v, err) = strconv.Atoi(s[1]);

                        if (err == null && v > 0) {
                            version = v;
                        }

                    }

                }

            }

        }
    }

    p.version = version; 

    // read version specific flags - extend as necessary

    // case currentVersion:
    //     ...
    //    fallthrough
    if (p.version == currentVersion || p.version == 5 || p.version == 4 || p.version == 3 || p.version == 2 || p.version == 1) 
        p.debugFormat = p.rawStringln(p.rawByte()) == "debug";
        p.trackAllTypes = p.@int() != 0;
        p.posInfoFormat = p.@int() != 0;
    else if (p.version == 0)     else 
        errorf("unknown bexport format version %d (%q)", p.version, versionstr);
    // --- generic export data ---

    // populate typList with predeclared "known" types
    p.typList = append(p.typList, predeclared()); 

    // read package data
    pkg = p.pkg(); 

    // read objects of phase 1 only (see cmd/compile/internal/gc/bexport.go)
    nint objcount = 0;
    while (true) {
        var tag = p.tagOrIndex();
        if (tag == endTag) {
            break;
        }
        p.obj(tag);
        objcount++;

    } 

    // self-verification
    {
        var count = p.@int();

        if (count != objcount) {
            errorf("got %d objects; want %d", objcount, count);
        }
    } 

    // ignore compiler-specific import data

    // complete interfaces
    // TODO(gri) re-investigate if we still need to do this in a delayed fashion
    foreach (var (_, typ) in p.interfaceList) {
        typ.Complete();
    }    var list = append((slice<ptr<types.Package>>)null, p.pkgList[(int)1..]);
    sort.Sort(byPath(list));
    pkg.SetImports(list); 

    // package was imported completely and without errors
    pkg.MarkComplete();

    return (p.read, _addr_pkg!, error.As(null!)!);

});

private static void errorf(@string format, params object[] args) => func((_, panic, _) => {
    args = args.Clone();

    panic(fmt.Sprintf(format, args));
});

private static ptr<types.Package> pkg(this ptr<importer> _addr_p) {
    ref importer p = ref _addr_p.val;
 
    // if the package was seen before, i is its index (>= 0)
    var i = p.tagOrIndex();
    if (i >= 0) {
        return _addr_p.pkgList[i]!;
    }
    if (i != packageTag) {
        errorf("unexpected package tag %d version %d", i, p.version);
    }
    var name = p.@string();
    @string path = default;
    if (p.version >= 5) {
        path = p.path();
    }
    else
 {
        path = p.@string();
    }
    if (p.version >= 6) {
        p.@int(); // package height; unused by go/types
    }
    if (name == "") {
        errorf("empty package name in import");
    }
    if ((path == "") != (len(p.pkgList) == 0)) {
        errorf("package path %q for pkg index %d", path, len(p.pkgList));
    }
    if (path == "") {
        path = p.importpath;
    }
    var pkg = p.imports[path];
    if (pkg == null) {
        pkg = types.NewPackage(path, name);
        p.imports[path] = pkg;
    }
    else if (pkg.Name() != name) {
        errorf("conflicting names %s and %s for package %q", pkg.Name(), name, path);
    }
    p.pkgList = append(p.pkgList, pkg);

    return _addr_pkg!;

}

// objTag returns the tag value for each object kind.
private static nint objTag(types.Object obj) => func((_, panic, _) => {
    switch (obj.type()) {
        case ptr<types.Const> _:
            return constTag;
            break;
        case ptr<types.TypeName> _:
            return typeTag;
            break;
        case ptr<types.Var> _:
            return varTag;
            break;
        case ptr<types.Func> _:
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

private static bool sameObj(types.Object a, types.Object b) { 
    // Because unnamed types are not canonicalized, we cannot simply compare types for
    // (pointer) identity.
    // Ideally we'd check equality of constant values as well, but this is good enough.
    return objTag(a) == objTag(b) && types.Identical(a.Type(), b.Type());

}

private static void declare(this ptr<importer> _addr_p, types.Object obj) {
    ref importer p = ref _addr_p.val;

    var pkg = obj.Pkg();
    {
        var alt = pkg.Scope().Insert(obj);

        if (alt != null) { 
            // This can only trigger if we import a (non-type) object a second time.
            // Excluding type aliases, this cannot happen because 1) we only import a package
            // once; and b) we ignore compiler-specific export data which may contain
            // functions whose inlined function bodies refer to other functions that
            // were already imported.
            // However, type aliases require reexporting the original type, so we need
            // to allow it (see also the comment in cmd/compile/internal/gc/bimport.go,
            // method importer.obj, switch case importing functions).
            // TODO(gri) review/update this comment once the gc compiler handles type aliases.
            if (!sameObj(obj, alt)) {
                errorf("inconsistent import:\n\t%v\npreviously imported as:\n\t%v\n", obj, alt);
            }

        }
    }

}

private static void obj(this ptr<importer> _addr_p, nint tag) {
    ref importer p = ref _addr_p.val;


    if (tag == constTag) 
        var pos = p.pos();
        var (pkg, name) = p.qualifiedName();
        var typ = p.typ(null, null);
        var val = p.value();
        p.declare(types.NewConst(pos, pkg, name, typ, val));
    else if (tag == aliasTag) 
        // TODO(gri) verify type alias hookup is correct
        pos = p.pos();
        (pkg, name) = p.qualifiedName();
        typ = p.typ(null, null);
        p.declare(types.NewTypeName(pos, pkg, name, typ));
    else if (tag == typeTag) 
        p.typ(null, null);
    else if (tag == varTag) 
        pos = p.pos();
        (pkg, name) = p.qualifiedName();
        typ = p.typ(null, null);
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

private static readonly nint deltaNewFile = -64; // see cmd/compile/internal/gc/bexport.go

 // see cmd/compile/internal/gc/bexport.go

private static token.Pos pos(this ptr<importer> _addr_p) {
    ref importer p = ref _addr_p.val;

    if (!p.posInfoFormat) {
        return token.NoPos;
    }
    var file = p.prevFile;
    var line = p.prevLine;
    var delta = p.@int();
    line += delta;
    if (p.version >= 5) {
        if (delta == deltaNewFile) {
            {
                var n__prev3 = n;

                var n = p.@int();

                if (n >= 0) { 
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
        if (delta == 0) {
            {
                var n__prev3 = n;

                n = p.@int();

                if (n >= 0) { 
                    // file changed
                    file = p.prevFile[..(int)n] + p.@string();
                    line = p.@int();

                }

                n = n__prev3;

            }

        }
    }
    p.prevFile = file;
    p.prevLine = line;

    return p.fake.pos(file, line, 0);

}

// Synthesize a token.Pos
private partial struct fakeFileSet {
    public ptr<token.FileSet> fset;
    public map<@string, ptr<token.File>> files;
}

private static token.Pos pos(this ptr<fakeFileSet> _addr_s, @string file, nint line, nint column) {
    ref fakeFileSet s = ref _addr_s.val;
 
    // TODO(mdempsky): Make use of column.

    // Since we don't know the set of needed file positions, we
    // reserve maxlines positions per file.
    const nint maxlines = 64 * 1024;

    var f = s.files[file];
    if (f == null) {
        f = s.fset.AddFile(file, -1, maxlines);
        s.files[file] = f; 
        // Allocate the fake linebreak indices on first use.
        // TODO(adonovan): opt: save ~512KB using a more complex scheme?
        fakeLinesOnce.Do(() => {
            fakeLines = make_slice<nint>(maxlines);
            foreach (var (i) in fakeLines) {
                fakeLines[i] = i;
            }
        });
        f.SetLines(fakeLines);

    }
    if (line > maxlines) {
        line = 1;
    }
    return f.Pos(line - 1);

}

private static slice<nint> fakeLines = default;private static sync.Once fakeLinesOnce = default;

private static (ptr<types.Package>, @string) qualifiedName(this ptr<importer> _addr_p) {
    ptr<types.Package> pkg = default!;
    @string name = default;
    ref importer p = ref _addr_p.val;

    name = p.@string();
    pkg = p.pkg();
    return ;
}

private static void record(this ptr<importer> _addr_p, types.Type t) {
    ref importer p = ref _addr_p.val;

    p.typList = append(p.typList, t);
}

// A dddSlice is a types.Type representing ...T parameters.
// It only appears for parameter types and does not escape
// the importer.
private partial struct dddSlice {
    public types.Type elem;
}

private static types.Type Underlying(this ptr<dddSlice> _addr_t) {
    ref dddSlice t = ref _addr_t.val;

    return t;
}
private static @string String(this ptr<dddSlice> _addr_t) {
    ref dddSlice t = ref _addr_t.val;

    return "..." + t.elem.String();
}

// parent is the package which declared the type; parent == nil means
// the package currently imported. The parent package is needed for
// exported struct fields and interface methods which don't contain
// explicit package information in the export data.
//
// A non-nil tname is used as the "owner" of the result type; i.e.,
// the result type is the underlying type of tname. tname is used
// to give interface methods a named receiver type where possible.
private static types.Type typ(this ptr<importer> _addr_p, ptr<types.Package> _addr_parent, ptr<types.Named> _addr_tname) => func((_, panic, _) => {
    ref importer p = ref _addr_p.val;
    ref types.Package parent = ref _addr_parent.val;
    ref types.Named tname = ref _addr_tname.val;
 
    // if the type was seen before, i is its index (>= 0)
    var i = p.tagOrIndex();
    if (i >= 0) {
        return p.typList[i];
    }

    if (i == namedTag) 
        // read type object
        var pos = p.pos();
        var (parent, name) = p.qualifiedName();
        var scope = parent.Scope();
        var obj = scope.Lookup(name); 

        // if the object doesn't exist yet, create and insert it
        if (obj == null) {
            obj = types.NewTypeName(pos, parent, name, null);
            scope.Insert(obj);
        }
        {
            ptr<types.TypeName> (_, ok) = obj._<ptr<types.TypeName>>();

            if (!ok) {
                errorf("pkg = %s, name = %s => %s", parent, name, obj);
            } 

            // associate new named type with obj if it doesn't exist yet

        } 

        // associate new named type with obj if it doesn't exist yet
        var t0 = types.NewNamed(obj._<ptr<types.TypeName>>(), null, null); 

        // but record the existing type, if any
        ptr<types.Named> tname = obj.Type()._<ptr<types.Named>>(); // tname is either t0 or the existing type
        p.record(tname); 

        // read underlying type
        t0.SetUnderlying(p.typ(parent, t0)); 

        // interfaces don't have associated methods
        if (types.IsInterface(t0)) {
            return tname;
        }
        {
            var i__prev1 = i;

            for (i = p.@int(); i > 0; i--) { 
                // TODO(gri) replace this with something closer to fieldName
                pos = p.pos();
                var name = p.@string();
                if (!exported(name)) {
                    p.pkg();
                }

                var (recv, _) = p.paramList(); // TODO(gri) do we need a full param list for the receiver?
                var (params, isddd) = p.paramList();
                var (result, _) = p.paramList();
                p.@int(); // go:nointerface pragma - discarded

                var sig = types.NewSignature(recv.At(0), params, result, isddd);
                t0.AddMethod(types.NewFunc(pos, parent, name, sig));

            }


            i = i__prev1;
        }

        return tname;
    else if (i == arrayTag) 
        ptr<object> t = @new<types.Array>();
        if (p.trackAllTypes) {
            p.record(t);
        }
        var n = p.int64();
        t.val = types.NewArray(p.typ(parent, null), n).val;
        return t;
    else if (i == sliceTag) 
        t = @new<types.Slice>();
        if (p.trackAllTypes) {
            p.record(t);
        }
        t.val = new ptr<ptr<types.NewSlice>>(p.typ(parent, null));
        return t;
    else if (i == dddTag) 
        t = @new<dddSlice>();
        if (p.trackAllTypes) {
            p.record(t);
        }
        t.elem = p.typ(parent, null);
        return t;
    else if (i == structTag) 
        t = @new<types.Struct>();
        if (p.trackAllTypes) {
            p.record(t);
        }
        t.val = new ptr<ptr<types.NewStruct>>(p.fieldList(parent));
        return t;
    else if (i == pointerTag) 
        t = @new<types.Pointer>();
        if (p.trackAllTypes) {
            p.record(t);
        }
        t.val = new ptr<ptr<types.NewPointer>>(p.typ(parent, null));
        return t;
    else if (i == signatureTag) 
        t = @new<types.Signature>();
        if (p.trackAllTypes) {
            p.record(t);
        }
        (params, isddd) = p.paramList();
        (result, _) = p.paramList();
        t.val = types.NewSignature(null, params, result, isddd).val;
        return t;
    else if (i == interfaceTag) 
        // Create a dummy entry in the type list. This is safe because we
        // cannot expect the interface type to appear in a cycle, as any
        // such cycle must contain a named type which would have been
        // first defined earlier.
        // TODO(gri) Is this still true now that we have type aliases?
        // See issue #23225.
        n = len(p.typList);
        if (p.trackAllTypes) {
            p.record(null);
        }
        slice<types.Type> embeddeds = default;
        {
            var n__prev1 = n;

            for (n = p.@int(); n > 0; n--) {
                p.pos();
                embeddeds = append(embeddeds, p.typ(parent, null));
            }


            n = n__prev1;
        }

        t = newInterface(p.methodList(parent, tname), embeddeds);
        p.interfaceList = append(p.interfaceList, t);
        if (p.trackAllTypes) {
            p.typList[n] = t;
        }
        return t;
    else if (i == mapTag) 
        t = @new<types.Map>();
        if (p.trackAllTypes) {
            p.record(t);
        }
        var key = p.typ(parent, null);
        var val = p.typ(parent, null);
        t.val = types.NewMap(key, val).val;
        return t;
    else if (i == chanTag) 
        t = @new<types.Chan>();
        if (p.trackAllTypes) {
            p.record(t);
        }
        var dir = chanDir(p.@int());
        val = p.typ(parent, null);
        t.val = types.NewChan(dir, val).val;
        return t;
    else 
        errorf("unexpected type tag %d", i); // panics
        panic("unreachable");
    
});

private static types.ChanDir chanDir(nint d) { 
    // tag values must match the constants in cmd/compile/internal/gc/go.go
    switch (d) {
        case 1: 
            return types.RecvOnly;
            break;
        case 2: 
            return types.SendOnly;
            break;
        case 3: 
            return types.SendRecv;
            break;
        default: 
            errorf("unexpected channel dir %d", d);
            return 0;
            break;
    }

}

private static (slice<ptr<types.Var>>, slice<@string>) fieldList(this ptr<importer> _addr_p, ptr<types.Package> _addr_parent) {
    slice<ptr<types.Var>> fields = default;
    slice<@string> tags = default;
    ref importer p = ref _addr_p.val;
    ref types.Package parent = ref _addr_parent.val;

    {
        var n = p.@int();

        if (n > 0) {
            fields = make_slice<ptr<types.Var>>(n);
            tags = make_slice<@string>(n);
            foreach (var (i) in fields) {
                fields[i], tags[i] = p.field(parent);
            }
        }
    }

    return ;

}

private static (ptr<types.Var>, @string) field(this ptr<importer> _addr_p, ptr<types.Package> _addr_parent) {
    ptr<types.Var> _p0 = default!;
    @string _p0 = default;
    ref importer p = ref _addr_p.val;
    ref types.Package parent = ref _addr_parent.val;

    var pos = p.pos();
    var (pkg, name, alias) = p.fieldName(parent);
    var typ = p.typ(parent, null);
    var tag = p.@string();

    var anonymous = false;
    if (name == "") { 
        // anonymous field - typ must be T or *T and T must be a type name
        switch (deref(typ).type()) {
            case ptr<types.Basic> typ:
                pkg = null; // // objects defined in Universe scope have no package
                name = typ.Name();
                break;
            case ptr<types.Named> typ:
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
    else if (alias) { 
        // anonymous field: we have an explicit name because it's an alias
        anonymous = true;

    }
    return (_addr_types.NewField(pos, pkg, name, typ, anonymous)!, tag);

}

private static slice<ptr<types.Func>> methodList(this ptr<importer> _addr_p, ptr<types.Package> _addr_parent, ptr<types.Named> _addr_baseType) {
    slice<ptr<types.Func>> methods = default;
    ref importer p = ref _addr_p.val;
    ref types.Package parent = ref _addr_parent.val;
    ref types.Named baseType = ref _addr_baseType.val;

    {
        var n = p.@int();

        if (n > 0) {
            methods = make_slice<ptr<types.Func>>(n);
            foreach (var (i) in methods) {
                methods[i] = p.method(parent, baseType);
            }
        }
    }

    return ;

}

private static ptr<types.Func> method(this ptr<importer> _addr_p, ptr<types.Package> _addr_parent, ptr<types.Named> _addr_baseType) {
    ref importer p = ref _addr_p.val;
    ref types.Package parent = ref _addr_parent.val;
    ref types.Named baseType = ref _addr_baseType.val;

    var pos = p.pos();
    var (pkg, name, _) = p.fieldName(parent); 
    // If we don't have a baseType, use a nil receiver.
    // A receiver using the actual interface type (which
    // we don't know yet) will be filled in when we call
    // types.Interface.Complete.
    ptr<types.Var> recv;
    if (baseType != null) {
        recv = types.NewVar(token.NoPos, parent, "", baseType);
    }
    var (params, isddd) = p.paramList();
    var (result, _) = p.paramList();
    var sig = types.NewSignature(recv, params, result, isddd);
    return _addr_types.NewFunc(pos, pkg, name, sig)!;

}

private static (ptr<types.Package>, @string, bool) fieldName(this ptr<importer> _addr_p, ptr<types.Package> _addr_parent) {
    ptr<types.Package> pkg = default!;
    @string name = default;
    bool alias = default;
    ref importer p = ref _addr_p.val;
    ref types.Package parent = ref _addr_parent.val;

    name = p.@string();
    pkg = parent;
    if (pkg == null) { 
        // use the imported package instead
        pkg = p.pkgList[0];

    }
    if (p.version == 0 && name == "_") { 
        // version 0 didn't export a package for _ fields
        return ;

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
        if (!exported(name)) {
            pkg = p.pkg();
        }

    __switch_break0:;
    return ;

}

private static (ptr<types.Tuple>, bool) paramList(this ptr<importer> _addr_p) {
    ptr<types.Tuple> _p0 = default!;
    bool _p0 = default;
    ref importer p = ref _addr_p.val;

    var n = p.@int();
    if (n == 0) {
        return (_addr_null!, false);
    }
    var named = true;
    if (n < 0) {
        n = -n;
        named = false;
    }
    var @params = make_slice<ptr<types.Var>>(n);
    var isddd = false;
    foreach (var (i) in params) {
        params[i], isddd = p.param(named);
    }    return (_addr_types.NewTuple(params)!, isddd);

}

private static (ptr<types.Var>, bool) param(this ptr<importer> _addr_p, bool named) {
    ptr<types.Var> _p0 = default!;
    bool _p0 = default;
    ref importer p = ref _addr_p.val;

    var t = p.typ(null, null);
    ptr<dddSlice> (td, isddd) = t._<ptr<dddSlice>>();
    if (isddd) {
        t = types.NewSlice(td.elem);
    }
    ptr<types.Package> pkg;
    @string name = default;
    if (named) {
        name = p.@string();
        if (name == "") {
            errorf("expected named parameter");
        }
        if (name != "_") {
            pkg = p.pkg();
        }
        {
            var i = strings.Index(name, "·");

            if (i > 0) {
                name = name[..(int)i]; // cut off gc-specific parameter numbering
            }

        }

    }
    p.@string();

    return (_addr_types.NewVar(token.NoPos, pkg, name, t)!, isddd);

}

private static bool exported(@string name) {
    var (ch, _) = utf8.DecodeRuneInString(name);
    return unicode.IsUpper(ch);
}

private static constant.Value value(this ptr<importer> _addr_p) => func((_, panic, _) => {
    ref importer p = ref _addr_p.val;

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

private static constant.Value @float(this ptr<importer> _addr_p) {
    ref importer p = ref _addr_p.val;

    var sign = p.@int();
    if (sign == 0) {
        return constant.MakeInt64(0);
    }
    var exp = p.@int();
    slice<byte> mant = (slice<byte>)p.@string(); // big endian

    // remove leading 0's if any
    while (len(mant) > 0 && mant[0] == 0) {
        mant = mant[(int)1..];
    } 

    // convert to little endian
    // TODO(gri) go/constant should have a more direct conversion function
    //           (e.g., once it supports a big.Float based implementation)
    {
        nint i = 0;
        var j = len(mant) - 1;

        while (i < j) {
            (mant[i], mant[j]) = (mant[j], mant[i]);            (i, j) = (i + 1, j - 1);
        }
    } 

    // adjust exponent (constant.MakeFromBytes creates an integer value,
    // but mant represents the mantissa bits such that 0.5 <= mant < 1.0)
    exp -= len(mant) << 3;
    if (len(mant) > 0) {
        {
            var msd = mant[len(mant) - 1];

            while (msd & 0x80 == 0) {
                exp++;
                msd<<=1;
            }

        }

    }
    var x = constant.MakeFromBytes(mant);

    if (exp < 0) 
        var d = constant.Shift(constant.MakeInt64(1), token.SHL, uint(-exp));
        x = constant.BinaryOp(x, token.QUO, d);
    else if (exp > 0) 
        x = constant.Shift(x, token.SHL, uint(exp));
        if (sign < 0) {
        x = constant.UnaryOp(token.SUB, x, 0);
    }
    return x;

}

// ----------------------------------------------------------------------------
// Low-level decoders

private static nint tagOrIndex(this ptr<importer> _addr_p) {
    ref importer p = ref _addr_p.val;

    if (p.debugFormat) {
        p.marker('t');
    }
    return int(p.rawInt64());

}

private static nint @int(this ptr<importer> _addr_p) {
    ref importer p = ref _addr_p.val;

    var x = p.int64();
    if (int64(int(x)) != x) {
        errorf("exported integer too large");
    }
    return int(x);

}

private static long int64(this ptr<importer> _addr_p) {
    ref importer p = ref _addr_p.val;

    if (p.debugFormat) {
        p.marker('i');
    }
    return p.rawInt64();

}

private static @string path(this ptr<importer> _addr_p) {
    ref importer p = ref _addr_p.val;

    if (p.debugFormat) {
        p.marker('p');
    }
    var i = p.rawInt64();
    if (i >= 0) {
        return p.pathList[i];
    }
    var a = make_slice<@string>(-i);
    foreach (var (n) in a) {
        a[n] = p.@string();
    }    var s = strings.Join(a, "/");
    p.pathList = append(p.pathList, s);
    return s;

}

private static @string @string(this ptr<importer> _addr_p) {
    ref importer p = ref _addr_p.val;

    if (p.debugFormat) {
        p.marker('s');
    }
    var i = p.rawInt64();
    if (i >= 0) {
        return p.strList[i];
    }
    {
        var n = int(-i);

        if (n <= cap(p.buf)) {
            p.buf = p.buf[..(int)n];
        }
        else
 {
            p.buf = make_slice<byte>(n);
        }
    }

    {
        var i__prev1 = i;

        foreach (var (__i) in p.buf) {
            i = __i;
            p.buf[i] = p.rawByte();
        }
        i = i__prev1;
    }

    var s = string(p.buf);
    p.strList = append(p.strList, s);
    return s;

}

private static void marker(this ptr<importer> _addr_p, byte want) {
    ref importer p = ref _addr_p.val;

    {
        var got = p.rawByte();

        if (got != want) {
            errorf("incorrect marker: got %c; want %c (pos = %d)", got, want, p.read);
        }
    }


    var pos = p.read;
    {
        var n = int(p.rawInt64());

        if (n != pos) {
            errorf("incorrect position: got %d; want %d", n, pos);
        }
    }

}

// rawInt64 should only be used by low-level decoders.
private static long rawInt64(this ptr<importer> _addr_p) {
    ref importer p = ref _addr_p.val;

    var (i, err) = binary.ReadVarint(p);
    if (err != null) {
        errorf("read error: %v", err);
    }
    return i;

}

// rawStringln should only be used to read the initial version string.
private static @string rawStringln(this ptr<importer> _addr_p, byte b) {
    ref importer p = ref _addr_p.val;

    p.buf = p.buf[..(int)0];
    while (b != '\n') {
        p.buf = append(p.buf, b);
        b = p.rawByte();
    }
    return string(p.buf);
}

// needed for binary.ReadVarint in rawInt64
private static (byte, error) ReadByte(this ptr<importer> _addr_p) {
    byte _p0 = default;
    error _p0 = default!;
    ref importer p = ref _addr_p.val;

    return (p.rawByte(), error.As(null!)!);
}

// byte is the bottleneck interface for reading p.data.
// It unescapes '|' 'S' to '$' and '|' '|' to '|'.
// rawByte should only be used by low-level decoders.
private static byte rawByte(this ptr<importer> _addr_p) {
    ref importer p = ref _addr_p.val;

    var b = p.data[0];
    nint r = 1;
    if (b == '|') {
        b = p.data[1];
        r = 2;
        switch (b) {
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
    p.data = p.data[(int)r..];
    p.read += r;
    return b;


}

// ----------------------------------------------------------------------------
// Export format

// Tags. Must be < 0.
 
// Objects
private static readonly var packageTag = -(iota + 1);
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


private static sync.Once predeclOnce = default;
private static slice<types.Type> predecl = default; // initialized lazily

private static slice<types.Type> predeclared() {
    predeclOnce.Do(() => { 
        // initialize lazily to be sure that all
        // elements have been initialized before
        predecl = new slice<types.Type>(new types.Type[] { types.Typ[types.Bool], types.Typ[types.Int], types.Typ[types.Int8], types.Typ[types.Int16], types.Typ[types.Int32], types.Typ[types.Int64], types.Typ[types.Uint], types.Typ[types.Uint8], types.Typ[types.Uint16], types.Typ[types.Uint32], types.Typ[types.Uint64], types.Typ[types.Uintptr], types.Typ[types.Float32], types.Typ[types.Float64], types.Typ[types.Complex64], types.Typ[types.Complex128], types.Typ[types.String], types.Universe.Lookup("byte").Type(), types.Universe.Lookup("rune").Type(), types.Universe.Lookup("error").Type(), types.Typ[types.UntypedBool], types.Typ[types.UntypedInt], types.Typ[types.UntypedRune], types.Typ[types.UntypedFloat], types.Typ[types.UntypedComplex], types.Typ[types.UntypedString], types.Typ[types.UntypedNil], types.Typ[types.UnsafePointer], types.Typ[types.Invalid], anyType{} });

    });
    return predecl;

}

private partial struct anyType {
}

private static types.Type Underlying(this anyType t) {
    return t;
}
private static @string String(this anyType t) {
    return "any";
}

} // end gcimporter_package
