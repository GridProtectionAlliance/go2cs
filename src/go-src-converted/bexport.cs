// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Binary package export.
// This file was derived from $GOROOT/src/cmd/compile/internal/gc/bexport.go;
// see that file for specification of the format.

// package gcimporter -- go2cs converted at 2022 March 06 23:31:56 UTC
// import "golang.org/x/tools/go/internal/gcimporter" ==> using gcimporter = go.golang.org.x.tools.go.@internal.gcimporter_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\internal\gcimporter\bexport.go
using bytes = go.bytes_package;
using binary = go.encoding.binary_package;
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using constant = go.go.constant_package;
using token = go.go.token_package;
using types = go.go.types_package;
using math = go.math_package;
using big = go.math.big_package;
using sort = go.sort_package;
using strings = go.strings_package;
using System;


namespace go.golang.org.x.tools.go.@internal;

public static partial class gcimporter_package {

    // If debugFormat is set, each integer and string value is preceded by a marker
    // and position information in the encoding. This mechanism permits an importer
    // to recognize immediately when it is out of sync. The importer recognizes this
    // mode automatically (i.e., it can import export data produced with debugging
    // support even if debugFormat is not set at the time of import). This mode will
    // lead to massively larger export data (by a factor of 2 to 3) and should only
    // be enabled during development and debugging.
    //
    // NOTE: This flag is the first flag to enable if importing dies because of
    // (suspected) format errors, and whenever a change is made to the format.
private static readonly var debugFormat = false; // default: false

// If trace is set, debugging output is printed to std out.
 // default: false

// If trace is set, debugging output is printed to std out.
private static readonly var trace = false; // default: false

// Current export format version. Increase with each format change.
// Note: The latest binary (non-indexed) export format is at version 6.
//       This exporter is still at level 4, but it doesn't matter since
//       the binary importer can handle older versions just fine.
// 6: package height (CL 105038) -- NOT IMPLEMENTED HERE
// 5: improved position encoding efficiency (issue 20080, CL 41619) -- NOT IMPLEMEMTED HERE
// 4: type name objects support type aliases, uses aliasTag
// 3: Go1.8 encoding (same as version 2, aliasTag defined but never used)
// 2: removed unused bool in ODCL export (compiler only)
// 1: header format change (more regular), export package for _ struct fields
// 0: Go1.7 encoding
 // default: false

// Current export format version. Increase with each format change.
// Note: The latest binary (non-indexed) export format is at version 6.
//       This exporter is still at level 4, but it doesn't matter since
//       the binary importer can handle older versions just fine.
// 6: package height (CL 105038) -- NOT IMPLEMENTED HERE
// 5: improved position encoding efficiency (issue 20080, CL 41619) -- NOT IMPLEMEMTED HERE
// 4: type name objects support type aliases, uses aliasTag
// 3: Go1.8 encoding (same as version 2, aliasTag defined but never used)
// 2: removed unused bool in ODCL export (compiler only)
// 1: header format change (more regular), export package for _ struct fields
// 0: Go1.7 encoding
private static readonly nint exportVersion = 4;

// trackAllTypes enables cycle tracking for all types, not just named
// types. The existing compiler invariants assume that unnamed types
// that are not completely set up are not used, or else there are spurious
// errors.
// If disabled, only named types are tracked, possibly leading to slightly
// less efficient encoding in rare cases. It also prevents the export of
// some corner-case type declarations (but those are not handled correctly
// with with the textual export format either).
// TODO(gri) enable and remove once issues caused by it are fixed


// trackAllTypes enables cycle tracking for all types, not just named
// types. The existing compiler invariants assume that unnamed types
// that are not completely set up are not used, or else there are spurious
// errors.
// If disabled, only named types are tracked, possibly leading to slightly
// less efficient encoding in rare cases. It also prevents the export of
// some corner-case type declarations (but those are not handled correctly
// with with the textual export format either).
// TODO(gri) enable and remove once issues caused by it are fixed
private static readonly var trackAllTypes = false;



private partial struct exporter {
    public ptr<token.FileSet> fset;
    public bytes.Buffer @out; // object -> index maps, indexed in order of serialization
    public map<@string, nint> strIndex;
    public map<ptr<types.Package>, nint> pkgIndex;
    public map<types.Type, nint> typIndex; // position encoding
    public bool posInfoFormat;
    public @string prevFile;
    public nint prevLine; // debugging support
    public nint written; // bytes written
    public nint indent; // for trace
}

// internalError represents an error generated inside this package.
private partial struct internalError { // : @string
}

private static @string Error(this internalError e) {
    return "gcimporter: " + string(e);
}

private static error internalErrorf(@string format, params object[] args) {
    args = args.Clone();

    return error.As(internalError(fmt.Sprintf(format, args)))!;
}

// BExportData returns binary export data for pkg.
// If no file set is provided, position info will be missing.
public static (slice<byte>, error) BExportData(ptr<token.FileSet> _addr_fset, ptr<types.Package> _addr_pkg) => func((defer, panic, _) => {
    slice<byte> b = default;
    error err = default!;
    ref token.FileSet fset = ref _addr_fset.val;
    ref types.Package pkg = ref _addr_pkg.val;

    defer(() => {
        {
            var e = recover();

            if (e != null) {
                {
                    internalError (ierr, ok) = e._<internalError>();

                    if (ok) {
                        err = ierr;
                        return ;
                    } 
                    // Not an internal error; panic again.

                } 
                // Not an internal error; panic again.
                panic(e);

            }

        }

    }());

    exporter p = new exporter(fset:fset,strIndex:map[string]int{"":0},pkgIndex:make(map[*types.Package]int),typIndex:make(map[types.Type]int),posInfoFormat:true,); 

    // write version info
    // The version string must start with "version %d" where %d is the version
    // number. Additional debugging information may follow after a blank; that
    // text is ignored by the importer.
    p.rawStringln(fmt.Sprintf("version %d", exportVersion));
    @string debug = default;
    if (debugFormat) {
        debug = "debug";
    }
    p.rawStringln(debug); // cannot use p.bool since it's affected by debugFormat; also want to see this clearly
    p.@bool(trackAllTypes);
    p.@bool(p.posInfoFormat); 

    // --- generic export data ---

    // populate type map with predeclared "known" types
    foreach (var (index, typ) in predeclared()) {
        p.typIndex[typ] = index;
    }    if (len(p.typIndex) != len(predeclared())) {
        return (null, error.As(internalError("duplicate entries in type map?"))!);
    }
    p.pkg(pkg, true);
    if (trace) {
        p.tracef("\n");
    }
    nint objcount = 0;
    var scope = pkg.Scope();
    foreach (var (_, name) in scope.Names()) {
        if (!ast.IsExported(name)) {
            continue;
        }
        if (trace) {
            p.tracef("\n");
        }
        p.obj(scope.Lookup(name));
        objcount++;

    }    if (trace) {
        p.tracef("\n");
    }
    p.tag(endTag); 

    // for self-verification only (redundant)
    p.@int(objcount);

    if (trace) {
        p.tracef("\n");
    }
    return (p.@out.Bytes(), error.As(null!)!);

});

private static void pkg(this ptr<exporter> _addr_p, ptr<types.Package> _addr_pkg, bool emptypath) => func((defer, panic, _) => {
    ref exporter p = ref _addr_p.val;
    ref types.Package pkg = ref _addr_pkg.val;

    if (pkg == null) {
        panic(internalError("unexpected nil pkg"));
    }
    {
        var (i, ok) = p.pkgIndex[pkg];

        if (ok) {
            p.index('P', i);
            return ;
        }
    } 

    // otherwise, remember the package, write the package tag (< 0) and package data
    if (trace) {
        p.tracef("P%d = { ", len(p.pkgIndex));
        defer(p.tracef("} "));
    }
    p.pkgIndex[pkg] = len(p.pkgIndex);

    p.tag(packageTag);
    p.@string(pkg.Name());
    if (emptypath) {
        p.@string("");
    }
    else
 {
        p.@string(pkg.Path());
    }
});

private static void obj(this ptr<exporter> _addr_p, types.Object obj) => func((_, panic, _) => {
    ref exporter p = ref _addr_p.val;

    switch (obj.type()) {
        case ptr<types.Const> obj:
            p.tag(constTag);
            p.pos(obj);
            p.qualifiedName(obj);
            p.typ(obj.Type());
            p.value(obj.Val());
            break;
        case ptr<types.TypeName> obj:
            if (obj.IsAlias()) {
                p.tag(aliasTag);
                p.pos(obj);
                p.qualifiedName(obj);
            }
            else
 {
                p.tag(typeTag);
            }

            p.typ(obj.Type());
            break;
        case ptr<types.Var> obj:
            p.tag(varTag);
            p.pos(obj);
            p.qualifiedName(obj);
            p.typ(obj.Type());
            break;
        case ptr<types.Func> obj:
            p.tag(funcTag);
            p.pos(obj);
            p.qualifiedName(obj);
            ptr<types.Signature> sig = obj.Type()._<ptr<types.Signature>>();
            p.paramList(sig.Params(), sig.Variadic());
            p.paramList(sig.Results(), false);
            break;
        default:
        {
            var obj = obj.type();
            panic(internalErrorf("unexpected object %v (%T)", obj, obj));
            break;
        }
    }

});

private static void pos(this ptr<exporter> _addr_p, types.Object obj) {
    ref exporter p = ref _addr_p.val;

    if (!p.posInfoFormat) {
        return ;
    }
    var (file, line) = p.fileLine(obj);
    if (file == p.prevFile) { 
        // common case: write line delta
        // delta == 0 means different file or no line change
        var delta = line - p.prevLine;
        p.@int(delta);
        if (delta == 0) {
            p.@int(-1); // -1 means no file change
        }
    }
    else
 { 
        // different file
        p.@int(0); 
        // Encode filename as length of common prefix with previous
        // filename, followed by (possibly empty) suffix. Filenames
        // frequently share path prefixes, so this can save a lot
        // of space and make export data size less dependent on file
        // path length. The suffix is unlikely to be empty because
        // file names tend to end in ".go".
        var n = commonPrefixLen(p.prevFile, file);
        p.@int(n); // n >= 0
        p.@string(file[(int)n..]); // write suffix only
        p.prevFile = file;
        p.@int(line);

    }
    p.prevLine = line;

}

private static (@string, nint) fileLine(this ptr<exporter> _addr_p, types.Object obj) {
    @string file = default;
    nint line = default;
    ref exporter p = ref _addr_p.val;

    if (p.fset != null) {
        var pos = p.fset.Position(obj.Pos());
        file = pos.Filename;
        line = pos.Line;
    }
    return ;

}

private static nint commonPrefixLen(@string a, @string b) {
    if (len(a) > len(b)) {
        (a, b) = (b, a);
    }
    nint i = 0;
    while (i < len(a) && a[i] == b[i]) {
        i++;
    }
    return i;

}

private static void qualifiedName(this ptr<exporter> _addr_p, types.Object obj) {
    ref exporter p = ref _addr_p.val;

    p.@string(obj.Name());
    p.pkg(obj.Pkg(), false);
}

private static void typ(this ptr<exporter> _addr_p, types.Type t) => func((defer, panic, _) => {
    ref exporter p = ref _addr_p.val;

    if (t == null) {
        panic(internalError("nil type"));
    }
    {
        var (i, ok) = p.typIndex[t];

        if (ok) {
            p.index('T', i);
            return ;
        }
    } 

    // otherwise, remember the type, write the type tag (< 0) and type data
    if (trackAllTypes) {
        if (trace) {
            p.tracef("T%d = {>\n", len(p.typIndex));
            defer(p.tracef("<\n} "));
        }
        p.typIndex[t] = len(p.typIndex);

    }
    switch (t.type()) {
        case ptr<types.Named> t:
            if (!trackAllTypes) { 
                // if we don't track all types, track named types now
                p.typIndex[t] = len(p.typIndex);

            }

            p.tag(namedTag);
            p.pos(t.Obj());
            p.qualifiedName(t.Obj());
            p.typ(t.Underlying());
            if (!types.IsInterface(t)) {
                p.assocMethods(t);
            }

            break;
        case ptr<types.Array> t:
            p.tag(arrayTag);
            p.int64(t.Len());
            p.typ(t.Elem());
            break;
        case ptr<types.Slice> t:
            p.tag(sliceTag);
            p.typ(t.Elem());
            break;
        case ptr<dddSlice> t:
            p.tag(dddTag);
            p.typ(t.elem);
            break;
        case ptr<types.Struct> t:
            p.tag(structTag);
            p.fieldList(t);
            break;
        case ptr<types.Pointer> t:
            p.tag(pointerTag);
            p.typ(t.Elem());
            break;
        case ptr<types.Signature> t:
            p.tag(signatureTag);
            p.paramList(t.Params(), t.Variadic());
            p.paramList(t.Results(), false);
            break;
        case ptr<types.Interface> t:
            p.tag(interfaceTag);
            p.iface(t);
            break;
        case ptr<types.Map> t:
            p.tag(mapTag);
            p.typ(t.Key());
            p.typ(t.Elem());
            break;
        case ptr<types.Chan> t:
            p.tag(chanTag);
            p.@int(int(3 - t.Dir())); // hack
            p.typ(t.Elem());
            break;
        default:
        {
            var t = t.type();
            panic(internalErrorf("unexpected type %T: %s", t, t));
            break;
        }
    }

});

private static void assocMethods(this ptr<exporter> _addr_p, ptr<types.Named> _addr_named) {
    ref exporter p = ref _addr_p.val;
    ref types.Named named = ref _addr_named.val;
 
    // Sort methods (for determinism).
    slice<ptr<types.Func>> methods = default;
    {
        nint i__prev1 = i;

        for (nint i = 0; i < named.NumMethods(); i++) {
            methods = append(methods, named.Method(i));
        }

        i = i__prev1;
    }
    sort.Sort(methodsByName(methods));

    p.@int(len(methods));

    if (trace && methods != null) {
        p.tracef("associated methods {>\n");
    }
    {
        nint i__prev1 = i;

        foreach (var (__i, __m) in methods) {
            i = __i;
            m = __m;
            if (trace && i > 0) {
                p.tracef("\n");
            }
            p.pos(m);
            var name = m.Name();
            p.@string(name);
            if (!exported(name)) {
                p.pkg(m.Pkg(), false);
            }
            ptr<types.Signature> sig = m.Type()._<ptr<types.Signature>>();
            p.paramList(types.NewTuple(sig.Recv()), false);
            p.paramList(sig.Params(), sig.Variadic());
            p.paramList(sig.Results(), false);
            p.@int(0); // dummy value for go:nointerface pragma - ignored by importer
        }
        i = i__prev1;
    }

    if (trace && methods != null) {
        p.tracef("<\n} ");
    }
}

private partial struct methodsByName { // : slice<ptr<types.Func>>
}

private static nint Len(this methodsByName x) {
    return len(x);
}
private static void Swap(this methodsByName x, nint i, nint j) {
    (x[i], x[j]) = (x[j], x[i]);
}
private static bool Less(this methodsByName x, nint i, nint j) {
    return x[i].Name() < x[j].Name();
}

private static void fieldList(this ptr<exporter> _addr_p, ptr<types.Struct> _addr_t) => func((defer, _, _) => {
    ref exporter p = ref _addr_p.val;
    ref types.Struct t = ref _addr_t.val;

    if (trace && t.NumFields() > 0) {
        p.tracef("fields {>\n");
        defer(p.tracef("<\n} "));
    }
    p.@int(t.NumFields());
    for (nint i = 0; i < t.NumFields(); i++) {
        if (trace && i > 0) {
            p.tracef("\n");
        }
        p.field(t.Field(i));
        p.@string(t.Tag(i));

    }

});

private static void field(this ptr<exporter> _addr_p, ptr<types.Var> _addr_f) => func((_, panic, _) => {
    ref exporter p = ref _addr_p.val;
    ref types.Var f = ref _addr_f.val;

    if (!f.IsField()) {
        panic(internalError("field expected"));
    }
    p.pos(f);
    p.fieldName(f);
    p.typ(f.Type());

});

private static void iface(this ptr<exporter> _addr_p, ptr<types.Interface> _addr_t) => func((defer, _, _) => {
    ref exporter p = ref _addr_p.val;
    ref types.Interface t = ref _addr_t.val;
 
    // TODO(gri): enable importer to load embedded interfaces,
    // then emit Embeddeds and ExplicitMethods separately here.
    p.@int(0);

    var n = t.NumMethods();
    if (trace && n > 0) {
        p.tracef("methods {>\n");
        defer(p.tracef("<\n} "));
    }
    p.@int(n);
    for (nint i = 0; i < n; i++) {
        if (trace && i > 0) {
            p.tracef("\n");
        }
        p.method(t.Method(i));

    }

});

private static void method(this ptr<exporter> _addr_p, ptr<types.Func> _addr_m) => func((_, panic, _) => {
    ref exporter p = ref _addr_p.val;
    ref types.Func m = ref _addr_m.val;

    ptr<types.Signature> sig = m.Type()._<ptr<types.Signature>>();
    if (sig.Recv() == null) {
        panic(internalError("method expected"));
    }
    p.pos(m);
    p.@string(m.Name());
    if (m.Name() != "_" && !ast.IsExported(m.Name())) {
        p.pkg(m.Pkg(), false);
    }
    p.paramList(sig.Params(), sig.Variadic());
    p.paramList(sig.Results(), false);

});

private static void fieldName(this ptr<exporter> _addr_p, ptr<types.Var> _addr_f) {
    ref exporter p = ref _addr_p.val;
    ref types.Var f = ref _addr_f.val;

    var name = f.Name();

    if (f.Anonymous()) { 
        // anonymous field - we distinguish between 3 cases:
        // 1) field name matches base type name and is exported
        // 2) field name matches base type name and is not exported
        // 3) field name doesn't match base type name (alias name)
        var bname = basetypeName(f.Type());
        if (name == bname) {
            if (ast.IsExported(name)) {
                name = ""; // 1) we don't need to know the field name or package
            }
            else
 {
                name = "?"; // 2) use unexported name "?" to force package export
            }

        }
        else
 { 
            // 3) indicate alias and export name as is
            // (this requires an extra "@" but this is a rare case)
            p.@string("@");

        }
    }
    p.@string(name);
    if (name != "" && !ast.IsExported(name)) {
        p.pkg(f.Pkg(), false);
    }
}

private static @string basetypeName(types.Type typ) {
    switch (deref(typ).type()) {
        case ptr<types.Basic> typ:
            return typ.Name();
            break;
        case ptr<types.Named> typ:
            return typ.Obj().Name();
            break;
        default:
        {
            var typ = deref(typ).type();
            return ""; // unnamed type
            break;
        }
    }

}

private static void paramList(this ptr<exporter> _addr_p, ptr<types.Tuple> _addr_@params, bool variadic) {
    ref exporter p = ref _addr_p.val;
    ref types.Tuple @params = ref _addr_@params.val;
 
    // use negative length to indicate unnamed parameters
    // (look at the first parameter only since either all
    // names are present or all are absent)
    var n = @params.Len();
    if (n > 0 && @params.At(0).Name() == "") {
        n = -n;
    }
    p.@int(n);
    for (nint i = 0; i < @params.Len(); i++) {
        var q = @params.At(i);
        var t = q.Type();
        if (variadic && i == @params.Len() - 1) {
            t = addr(new dddSlice(t.(*types.Slice).Elem()));
        }
        p.typ(t);
        if (n > 0) {
            var name = q.Name();
            p.@string(name);
            if (name != "_") {
                p.pkg(q.Pkg(), false);
            }
        }
        p.@string(""); // no compiler-specific info
    }

}

private static void value(this ptr<exporter> _addr_p, constant.Value x) => func((_, panic, _) => {
    ref exporter p = ref _addr_p.val;

    if (trace) {
        p.tracef("= ");
    }

    if (x.Kind() == constant.Bool) 
        var tag = falseTag;
        if (constant.BoolVal(x)) {
            tag = trueTag;
        }
        p.tag(tag);
    else if (x.Kind() == constant.Int) 
        {
            var (v, exact) = constant.Int64Val(x);

            if (exact) { 
                // common case: x fits into an int64 - use compact encoding
                p.tag(int64Tag);
                p.int64(v);
                return ;

            } 
            // uncommon case: large x - use float encoding
            // (powers of 2 will be encoded efficiently with exponent)

        } 
        // uncommon case: large x - use float encoding
        // (powers of 2 will be encoded efficiently with exponent)
        p.tag(floatTag);
        p.@float(constant.ToFloat(x));
    else if (x.Kind() == constant.Float) 
        p.tag(floatTag);
        p.@float(x);
    else if (x.Kind() == constant.Complex) 
        p.tag(complexTag);
        p.@float(constant.Real(x));
        p.@float(constant.Imag(x));
    else if (x.Kind() == constant.String) 
        p.tag(stringTag);
        p.@string(constant.StringVal(x));
    else if (x.Kind() == constant.Unknown) 
        // package contains type errors
        p.tag(unknownTag);
    else 
        panic(internalErrorf("unexpected value %v (%T)", x, x));
    
});

private static void @float(this ptr<exporter> _addr_p, constant.Value x) => func((_, panic, _) => {
    ref exporter p = ref _addr_p.val;

    if (x.Kind() != constant.Float) {
        panic(internalErrorf("unexpected constant %v, want float", x));
    }
    var sign = constant.Sign(x);
    if (sign == 0) { 
        // x == 0
        p.@int(0);
        return ;

    }
    big.Float f = default;
    {
        var (v, exact) = constant.Float64Val(x);

        if (exact) { 
            // float64
            f.SetFloat64(v);

        }        {
            var num = constant.Num(x);
            var denom = constant.Denom(x);


            else if (num.Kind() == constant.Int) { 
                // TODO(gri): add big.Rat accessor to constant.Value.
                var r = valueToRat(num);
                f.SetRat(r.Quo(r, valueToRat(denom)));

            }
            else
 { 
                // Value too large to represent as a fraction => inaccessible.
                // TODO(gri): add big.Float accessor to constant.Value.
                f.SetFloat64(math.MaxFloat64); // FIXME
            } 

            // extract exponent such that 0.5 <= m < 1.0

        } 

        // extract exponent such that 0.5 <= m < 1.0

    } 

    // extract exponent such that 0.5 <= m < 1.0
    ref big.Float m = ref heap(out ptr<big.Float> _addr_m);
    var exp = f.MantExp(_addr_m); 

    // extract mantissa as *big.Int
    // - set exponent large enough so mant satisfies mant.IsInt()
    // - get *big.Int from mant
    m.SetMantExp(_addr_m, int(m.MinPrec()));
    var (mant, acc) = m.Int(null);
    if (acc != big.Exact) {
        panic(internalError("internal error"));
    }
    p.@int(sign);
    p.@int(exp);
    p.@string(string(mant.Bytes()));

});

private static ptr<big.Rat> valueToRat(constant.Value x) { 
    // Convert little-endian to big-endian.
    // I can't believe this is necessary.
    var bytes = constant.Bytes(x);
    for (nint i = 0; i < len(bytes) / 2; i++) {
        (bytes[i], bytes[len(bytes) - 1 - i]) = (bytes[len(bytes) - 1 - i], bytes[i]);
    }
    return @new<big.Rat>().SetInt(@new<big.Int>().SetBytes(bytes));

}

private static bool @bool(this ptr<exporter> _addr_p, bool b) => func((defer, _, _) => {
    ref exporter p = ref _addr_p.val;

    if (trace) {
        p.tracef("[");
        defer(p.tracef("= %v] ", b));
    }
    nint x = 0;
    if (b) {
        x = 1;
    }
    p.@int(x);
    return b;

});

// ----------------------------------------------------------------------------
// Low-level encoders

private static void index(this ptr<exporter> _addr_p, byte marker, nint index) => func((_, panic, _) => {
    ref exporter p = ref _addr_p.val;

    if (index < 0) {
        panic(internalError("invalid index < 0"));
    }
    if (debugFormat) {
        p.marker('t');
    }
    if (trace) {
        p.tracef("%c%d ", marker, index);
    }
    p.rawInt64(int64(index));

});

private static void tag(this ptr<exporter> _addr_p, nint tag) => func((_, panic, _) => {
    ref exporter p = ref _addr_p.val;

    if (tag >= 0) {
        panic(internalError("invalid tag >= 0"));
    }
    if (debugFormat) {
        p.marker('t');
    }
    if (trace) {
        p.tracef("%s ", tagString[-tag]);
    }
    p.rawInt64(int64(tag));

});

private static void @int(this ptr<exporter> _addr_p, nint x) {
    ref exporter p = ref _addr_p.val;

    p.int64(int64(x));
}

private static void int64(this ptr<exporter> _addr_p, long x) {
    ref exporter p = ref _addr_p.val;

    if (debugFormat) {
        p.marker('i');
    }
    if (trace) {
        p.tracef("%d ", x);
    }
    p.rawInt64(x);

}

private static void @string(this ptr<exporter> _addr_p, @string s) {
    ref exporter p = ref _addr_p.val;

    if (debugFormat) {
        p.marker('s');
    }
    if (trace) {
        p.tracef("%q ", s);
    }
    {
        var i__prev1 = i;

        var (i, ok) = p.strIndex[s];

        if (ok) {
            p.rawInt64(int64(i));
            return ;
        }
        i = i__prev1;

    } 
    // otherwise, remember string and write its negative length and bytes
    p.strIndex[s] = len(p.strIndex);
    p.rawInt64(-int64(len(s)));
    {
        var i__prev1 = i;

        for (nint i = 0; i < len(s); i++) {
            p.rawByte(s[i]);
        }

        i = i__prev1;
    }

}

// marker emits a marker byte and position information which makes
// it easy for a reader to detect if it is "out of sync". Used for
// debugFormat format only.
private static void marker(this ptr<exporter> _addr_p, byte m) {
    ref exporter p = ref _addr_p.val;

    p.rawByte(m); 
    // Enable this for help tracking down the location
    // of an incorrect marker when running in debugFormat.
    if (false && trace) {
        p.tracef("#%d ", p.written);
    }
    p.rawInt64(int64(p.written));

}

// rawInt64 should only be used by low-level encoders.
private static void rawInt64(this ptr<exporter> _addr_p, long x) {
    ref exporter p = ref _addr_p.val;

    array<byte> tmp = new array<byte>(binary.MaxVarintLen64);
    var n = binary.PutVarint(tmp[..], x);
    for (nint i = 0; i < n; i++) {
        p.rawByte(tmp[i]);
    }
}

// rawStringln should only be used to emit the initial version string.
private static void rawStringln(this ptr<exporter> _addr_p, @string s) {
    ref exporter p = ref _addr_p.val;

    for (nint i = 0; i < len(s); i++) {
        p.rawByte(s[i]);
    }
    p.rawByte('\n');
}

// rawByte is the bottleneck interface to write to p.out.
// rawByte escapes b as follows (any encoding does that
// hides '$'):
//
//    '$'  => '|' 'S'
//    '|'  => '|' '|'
//
// Necessary so other tools can find the end of the
// export data by searching for "$$".
// rawByte should only be used by low-level encoders.
private static void rawByte(this ptr<exporter> _addr_p, byte b) {
    ref exporter p = ref _addr_p.val;


    if (b == '$') 
    {
        // write '$' as '|' 'S'
        b = 'S';
        fallthrough = true;
    }
    if (fallthrough || b == '|') 
    {
        // write '|' as '|' '|'
        p.@out.WriteByte('|');
        p.written++;
        goto __switch_break0;
    }

    __switch_break0:;
    p.@out.WriteByte(b);
    p.written++;

}

// tracef is like fmt.Printf but it rewrites the format string
// to take care of indentation.
private static void tracef(this ptr<exporter> _addr_p, @string format, params object[] args) {
    args = args.Clone();
    ref exporter p = ref _addr_p.val;

    if (strings.ContainsAny(format, "<>\n")) {
        bytes.Buffer buf = default;
        for (nint i = 0; i < len(format); i++) { 
            // no need to deal with runes
            var ch = format[i];
            switch (ch) {
                case '>': 
                    p.indent++;
                    continue;
                    break;
                case '<': 
                    p.indent--;
                    continue;
                    break;
            }
            buf.WriteByte(ch);
            if (ch == '\n') {
                for (var j = p.indent; j > 0; j--) {
                    buf.WriteString(".  ");
                }
            }

        }
        format = buf.String();

    }
    fmt.Printf(format, args);

}

// Debugging support.
// (tagString is only used when tracing is enabled)
private static array<@string> tagString = new array<@string>(InitKeyedValues<@string>((-packageTag, "package"), (-namedTag, "named type"), (-arrayTag, "array"), (-sliceTag, "slice"), (-dddTag, "ddd"), (-structTag, "struct"), (-pointerTag, "pointer"), (-signatureTag, "signature"), (-interfaceTag, "interface"), (-mapTag, "map"), (-chanTag, "chan"), (-falseTag, "false"), (-trueTag, "true"), (-int64Tag, "int64"), (-floatTag, "float"), (-fractionTag, "fraction"), (-complexTag, "complex"), (-stringTag, "string"), (-unknownTag, "unknown"), (-aliasTag, "alias")));

} // end gcimporter_package
