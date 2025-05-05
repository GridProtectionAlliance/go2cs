// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.encoding;

using fmt = fmt_package;
using reflect = reflect_package;
using strings = strings_package;
using sync = sync_package;

partial class xml_package {

// typeInfo holds details for the xml representation of a type.
[GoType] partial struct typeInfo {
    internal ж<fieldInfo> xmlname;
    internal slice<fieldInfo> fields;
}

// fieldInfo holds details for the xml representation of a single field.
[GoType] partial struct fieldInfo {
    internal slice<nint> idx;
    internal @string name;
    internal @string xmlns;
    internal fieldFlags flags;
    internal slice<@string> parents;
}

[GoType("num:nint")] partial struct fieldFlags;

internal static readonly fieldFlags fElement = /* 1 << iota */ 1;
internal static readonly fieldFlags fAttr = 2;
internal static readonly fieldFlags fCDATA = 4;
internal static readonly fieldFlags fCharData = 8;
internal static readonly fieldFlags fInnerXML = 16;
internal static readonly fieldFlags fComment = 32;
internal static readonly fieldFlags fAny = 64;
internal static readonly fieldFlags fOmitEmpty = 128;
internal static readonly fieldFlags fMode = /* fElement | fAttr | fCDATA | fCharData | fInnerXML | fComment | fAny */ 127;
internal static readonly @string xmlName = "XMLName"u8;

internal static sync.Map tinfoMap; // map[reflect.Type]*typeInfo

internal static reflectꓸType nameType = reflect.TypeFor<Name>();

// getTypeInfo returns the typeInfo structure with details necessary
// for marshaling and unmarshaling typ.
internal static (ж<typeInfo>, error) getTypeInfo(reflectꓸType typ) {
    {
        var (tiΔ1, ok) = tinfoMap.Load(typ); if (ok) {
            return (tiΔ1._<typeInfo.val>(), default!);
        }
    }
    var tinfo = Ꮡ(new typeInfo(nil));
    if (typ.Kind() == reflect.Struct && !AreEqual(typ, nameType)) {
        nint n = typ.NumField();
        for (nint i = 0; i < n; i++) {
            ref var f = ref heap<reflect_package.StructField>(out var Ꮡf);
            f = typ.Field(i);
            if ((!f.IsExported() && !f.Anonymous) || f.Tag.Get("xml"u8) == "-"u8) {
                continue;
            }
            // Private field
            // For embedded structs, embed its fields.
            if (f.Anonymous) {
                var t = f.Type;
                if (t.Kind() == reflect.ΔPointer) {
                    t = t.Elem();
                }
                if (t.Kind() == reflect.Struct) {
                    (inner, err) = getTypeInfo(t);
                    if (err != default!) {
                        return (default!, err);
                    }
                    if ((~tinfo).xmlname == nil) {
                        tinfo.val.xmlname = inner.val.xmlname;
                    }
                    ref var finfo = ref heap(new fieldInfo(), out var Ꮡfinfo);

                    foreach (var (_, finfo) in (~inner).fields) {
                        finfo.idx = append(new nint[]{i}.slice(), finfo.idx.ꓸꓸꓸ);
                        {
                            var errΔ1 = addFieldInfo(typ, tinfo, Ꮡfinfo); if (errΔ1 != default!) {
                                return (default!, errΔ1);
                            }
                        }
                    }
                    continue;
                }
            }
            (finfo, err) = structFieldInfo(typ, Ꮡf);
            if (err != default!) {
                return (default!, err);
            }
            if (f.Name == xmlName) {
                tinfo.val.xmlname = finfo;
                continue;
            }
            // Add the field if it doesn't conflict with other fields.
            {
                var errΔ1 = addFieldInfo(typ, tinfo, finfo); if (errΔ1 != default!) {
                    return (default!, errΔ1);
                }
            }
        }
    }
    var (ti, _) = tinfoMap.LoadOrStore(typ, tinfo);
    return (ti._<typeInfo.val>(), default!);
}

// structFieldInfo builds and returns a fieldInfo for f.
internal static (ж<fieldInfo>, error) structFieldInfo(reflectꓸType typ, ж<reflect.StructField> Ꮡf) {
    ref var f = ref Ꮡf.val;

    var finfo = Ꮡ(new fieldInfo(idx: f.Index));
    // Split the tag from the xml namespace if necessary.
    @string tag = f.Tag.Get("xml"u8);
    {
        var (ns, t, ok) = strings.Cut(tag, " "u8); if (ok) {
            (finfo.val.xmlns, tag) = (ns, t);
        }
    }
    // Parse flags.
    var tokens = strings.Split(tag, ","u8);
    if (len(tokens) == 1){
        finfo.val.flags = fElement;
    } else {
        tag = tokens[0];
        foreach (var (_, flag) in tokens[1..]) {
            var exprᴛ1 = flag;
            if (exprᴛ1 == "attr"u8) {
                finfo.val.flags |= (fieldFlags)(fAttr);
            }
            else if (exprᴛ1 == "cdata"u8) {
                finfo.val.flags |= (fieldFlags)(fCDATA);
            }
            else if (exprᴛ1 == "chardata"u8) {
                finfo.val.flags |= (fieldFlags)(fCharData);
            }
            else if (exprᴛ1 == "innerxml"u8) {
                finfo.val.flags |= (fieldFlags)(fInnerXML);
            }
            else if (exprᴛ1 == "comment"u8) {
                finfo.val.flags |= (fieldFlags)(fComment);
            }
            else if (exprᴛ1 == "any"u8) {
                finfo.val.flags |= (fieldFlags)(fAny);
            }
            else if (exprᴛ1 == "omitempty"u8) {
                finfo.val.flags |= (fieldFlags)(fOmitEmpty);
            }

        }
        // Validate the flags used.
        var valid = true;
        {
            fieldFlags mode = (fieldFlags)((~finfo).flags & fMode);
            var exprᴛ2 = mode;
            if (exprᴛ2 == 0) {
                finfo.val.flags |= (fieldFlags)(fElement);
            }
            else if (exprᴛ2 == fAttr || exprᴛ2 == fCDATA || exprᴛ2 == fCharData || exprᴛ2 == fInnerXML || exprᴛ2 == fComment || exprᴛ2 == fAny || exprᴛ2 == (fieldFlags)(fAny | fAttr)) {
                if (f.Name == xmlName || tag != ""u8 && mode != fAttr) {
                    valid = false;
                }
            }
            else { /* default: */
                valid = false;
            }
        }

        // This will also catch multiple modes in a single field.
        if ((fieldFlags)((~finfo).flags & fMode) == fAny) {
            finfo.val.flags |= (fieldFlags)(fElement);
        }
        if ((fieldFlags)((~finfo).flags & fOmitEmpty) != 0 && (fieldFlags)((~finfo).flags & ((fieldFlags)(fElement | fAttr))) == 0) {
            valid = false;
        }
        if (!valid) {
            return (default!, fmt.Errorf("xml: invalid tag in field %s of type %s: %q"u8,
                f.Name, typ, f.Tag.Get("xml"u8)));
        }
    }
    // Use of xmlns without a name is not allowed.
    if ((~finfo).xmlns != ""u8 && tag == ""u8) {
        return (default!, fmt.Errorf("xml: namespace without name in field %s of type %s: %q"u8,
            f.Name, typ, f.Tag.Get("xml"u8)));
    }
    if (f.Name == xmlName) {
        // The XMLName field records the XML element name. Don't
        // process it as usual because its name should default to
        // empty rather than to the field name.
        finfo.val.name = tag;
        return (finfo, default!);
    }
    if (tag == ""u8) {
        // If the name part of the tag is completely empty, get
        // default from XMLName of underlying struct if feasible,
        // or field name otherwise.
        {
            var xmlname = lookupXMLName(f.Type); if (xmlname != nil){
                (finfo.val.xmlns, finfo.val.name) = (xmlname.val.xmlns, xmlname.val.name);
            } else {
                finfo.val.name = f.Name;
            }
        }
        return (finfo, default!);
    }
    // Prepare field name and parents.
    var parents = strings.Split(tag, ">"u8);
    if (parents[0] == "") {
        parents[0] = f.Name;
    }
    if (parents[len(parents) - 1] == "") {
        return (default!, fmt.Errorf("xml: trailing '>' in field %s of type %s"u8, f.Name, typ));
    }
    finfo.val.name = parents[len(parents) - 1];
    if (len(parents) > 1) {
        if (((fieldFlags)((~finfo).flags & fElement)) == 0) {
            return (default!, fmt.Errorf("xml: %s chain not valid with %s flag"u8, tag, strings.Join(tokens[1..], ","u8)));
        }
        finfo.val.parents = parents[..(int)(len(parents) - 1)];
    }
    // If the field type has an XMLName field, the names must match
    // so that the behavior of both marshaling and unmarshaling
    // is straightforward and unambiguous.
    if ((fieldFlags)((~finfo).flags & fElement) != 0) {
        var ftyp = f.Type;
        var xmlname = lookupXMLName(ftyp);
        if (xmlname != nil && (~xmlname).name != (~finfo).name) {
            return (default!, fmt.Errorf("xml: name %q in tag of %s.%s conflicts with name %q in %s.XMLName"u8,
                (~finfo).name, typ, f.Name, (~xmlname).name, ftyp));
        }
    }
    return (finfo, default!);
}

// lookupXMLName returns the fieldInfo for typ's XMLName field
// in case it exists and has a valid xml field tag, otherwise
// it returns nil.
internal static ж<fieldInfo> /*xmlname*/ lookupXMLName(reflectꓸType typ) {
    ж<fieldInfo> xmlname = default!;

    while (typ.Kind() == reflect.ΔPointer) {
        typ = typ.Elem();
    }
    if (typ.Kind() != reflect.Struct) {
        return default!;
    }
    for (nint i = 0;nint n = typ.NumField(); i < n; i++) {
        ref var f = ref heap<reflect_package.StructField>(out var Ꮡf);
        f = typ.Field(i);
        if (f.Name != xmlName) {
            continue;
        }
        (finfo, err) = structFieldInfo(typ, Ꮡf);
        if (err == default! && (~finfo).name != ""u8) {
            return finfo;
        }
        // Also consider errors as a non-existent field tag
        // and let getTypeInfo itself report the error.
        break;
    }
    return default!;
}

// addFieldInfo adds finfo to tinfo.fields if there are no
// conflicts, or if conflicts arise from previous fields that were
// obtained from deeper embedded structures than finfo. In the latter
// case, the conflicting entries are dropped.
// A conflict occurs when the path (parent + name) to a field is
// itself a prefix of another path, or when two paths match exactly.
// It is okay for field paths to share a common, shorter prefix.
internal static error addFieldInfo(reflectꓸType typ, ж<typeInfo> Ꮡtinfo, ж<fieldInfo> Ꮡnewf) {
    ref var tinfo = ref Ꮡtinfo.val;
    ref var newf = ref Ꮡnewf.val;

    slice<nint> conflicts = default!;
Loop:
    foreach (var (i, _) in tinfo.fields) {
        // First, figure all conflicts. Most working code will have none.
        var oldf = Ꮡ(tinfo.fields, i);
        if ((fieldFlags)((~oldf).flags & fMode) != (fieldFlags)(newf.flags & fMode)) {
            continue;
        }
        if ((~oldf).xmlns != ""u8 && newf.xmlns != ""u8 && (~oldf).xmlns != newf.xmlns) {
            continue;
        }
        nint minl = min(len(newf.parents), len((~oldf).parents));
        for (nint p = 0; p < minl; p++) {
            if ((~oldf).parents[p] != newf.parents[p]) {
                goto continue_Loop;
            }
        }
        if (len((~oldf).parents) > len(newf.parents)){
            if ((~oldf).parents[len(newf.parents)] == newf.name) {
                conflicts = append(conflicts, i);
            }
        } else 
        if (len((~oldf).parents) < len(newf.parents)){
            if (newf.parents[len((~oldf).parents)] == (~oldf).name) {
                conflicts = append(conflicts, i);
            }
        } else {
            if (newf.name == (~oldf).name && newf.xmlns == (~oldf).xmlns) {
                conflicts = append(conflicts, i);
            }
        }
    }
    // Without conflicts, add the new field and return.
    if (conflicts == default!) {
        tinfo.fields = append(tinfo.fields, newf);
        return default!;
    }
    // If any conflict is shallower, ignore the new field.
    // This matches the Go field resolution on embedding.
    foreach (var (_, i) in conflicts) {
        if (len(tinfo.fields[i].idx) < len(newf.idx)) {
            return default!;
        }
    }
    // Otherwise, if any of them is at the same depth level, it's an error.
    foreach (var (_, i) in conflicts) {
        var oldf = Ꮡ(tinfo.fields, i);
        if (len((~oldf).idx) == len(newf.idx)) {
            var f1 = typ.FieldByIndex((~oldf).idx);
            var f2 = typ.FieldByIndex(newf.idx);
            return new TagPathError(typ, f1.Name, f1.Tag.Get("xml"u8), f2.Name, f2.Tag.Get("xml"u8));
        }
    }
    // Otherwise, the new field is shallower, and thus takes precedence,
    // so drop the conflicting fields from tinfo and append the new one.
    for (nint c = len(conflicts) - 1; c >= 0; c--) {
        nint i = conflicts[c];
        copy(tinfo.fields[(int)(i)..], tinfo.fields[(int)(i + 1)..]);
        tinfo.fields = tinfo.fields[..(int)(len(tinfo.fields) - 1)];
    }
    tinfo.fields = append(tinfo.fields, newf);
    return default!;
}

// A TagPathError represents an error in the unmarshaling process
// caused by the use of field tags with conflicting paths.
[GoType] partial struct TagPathError {
    public reflect_package.ΔType Struct;
    public @string Field1;
    public @string Tag1;
    public @string Field2;
    public @string Tag2;
}

[GoRecv] public static @string Error(this ref TagPathError e) {
    return fmt.Sprintf("%s field %q with tag %q conflicts with field %q with tag %q"u8, e.Struct, e.Field1, e.Tag1, e.Field2, e.Tag2);
}

internal const bool initNilPointers = true;
internal const bool dontInitNilPointers = false;

// value returns v's field value corresponding to finfo.
// It's equivalent to v.FieldByIndex(finfo.idx), but when passed
// initNilPointers, it initializes and dereferences pointers as necessary.
// When passed dontInitNilPointers and a nil pointer is reached, the function
// returns a zero reflect.Value.
[GoRecv] internal static reflectꓸValue value(this ref fieldInfo finfo, reflectꓸValue v, bool shouldInitNilPointers) {
    foreach (var (i, x) in finfo.idx) {
        if (i > 0) {
            var t = v.Type();
            if (t.Kind() == reflect.ΔPointer && t.Elem().Kind() == reflect.Struct) {
                if (v.IsNil()) {
                    if (!shouldInitNilPointers) {
                        return new reflectꓸValue(nil);
                    }
                    v.Set(reflect.New(v.Type().Elem()));
                }
                v = v.Elem();
            }
        }
        v = v.Field(x);
    }
    return v;
}

} // end xml_package
