// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package xml -- go2cs converted at 2022 March 13 05:40:03 UTC
// import "encoding/xml" ==> using xml = go.encoding.xml_package
// Original source: C:\Program Files\Go\src\encoding\xml\typeinfo.go
namespace go.encoding;

using fmt = fmt_package;
using reflect = reflect_package;
using strings = strings_package;
using sync = sync_package;


// typeInfo holds details for the xml representation of a type.

public static partial class xml_package {

private partial struct typeInfo {
    public ptr<fieldInfo> xmlname;
    public slice<fieldInfo> fields;
}

// fieldInfo holds details for the xml representation of a single field.
private partial struct fieldInfo {
    public slice<nint> idx;
    public @string name;
    public @string xmlns;
    public fieldFlags flags;
    public slice<@string> parents;
}

private partial struct fieldFlags { // : nint
}

private static readonly fieldFlags fElement = 1 << (int)(iota);
private static readonly var fAttr = 0;
private static readonly var fCDATA = 1;
private static readonly var fCharData = 2;
private static readonly var fInnerXML = 3;
private static readonly var fComment = 4;
private static readonly var fAny = 5;

private static readonly fMode fOmitEmpty = fElement | fAttr | fCDATA | fCharData | fInnerXML | fComment | fAny;

private static readonly @string xmlName = "XMLName";

private static sync.Map tinfoMap = default; // map[reflect.Type]*typeInfo

private static var nameType = reflect.TypeOf(new Name());

// getTypeInfo returns the typeInfo structure with details necessary
// for marshaling and unmarshaling typ.
private static (ptr<typeInfo>, error) getTypeInfo(reflect.Type typ) {
    ptr<typeInfo> _p0 = default!;
    error _p0 = default!;

    {
        var ti__prev1 = ti;

        var (ti, ok) = tinfoMap.Load(typ);

        if (ok) {
            return (ti._<ptr<typeInfo>>(), error.As(null!)!);
        }
        ti = ti__prev1;

    }

    ptr<typeInfo> tinfo = addr(new typeInfo());
    if (typ.Kind() == reflect.Struct && typ != nameType) {
        var n = typ.NumField();
        for (nint i = 0; i < n; i++) {
            ref var f = ref heap(typ.Field(i), out ptr<var> _addr_f);
            if ((!f.IsExported() && !f.Anonymous) || f.Tag.Get("xml") == "-") {
                continue; // Private field
            } 

            // For embedded structs, embed its fields.
            if (f.Anonymous) {
                var t = f.Type;
                if (t.Kind() == reflect.Ptr) {
                    t = t.Elem();
                }
                if (t.Kind() == reflect.Struct) {
                    var (inner, err) = getTypeInfo(t);
                    if (err != null) {
                        return (_addr_null!, error.As(err)!);
                    }
                    if (tinfo.xmlname == null) {
                        tinfo.xmlname = inner.xmlname;
                    }
                    {
                        var finfo__prev2 = finfo;

                        foreach (var (_, __finfo) in inner.fields) {
                            finfo = __finfo;
                            finfo.idx = append(new slice<nint>(new nint[] { i }), finfo.idx);
                            {
                                var err__prev4 = err;

                                var err = addFieldInfo(typ, tinfo, _addr_finfo);

                                if (err != null) {
                                    return (_addr_null!, error.As(err)!);
                                }

                                err = err__prev4;

                            }
                        }

                        finfo = finfo__prev2;
                    }

                    continue;
                }
            }
            var (finfo, err) = structFieldInfo(typ, _addr_f);
            if (err != null) {
                return (_addr_null!, error.As(err)!);
            }
            if (f.Name == xmlName) {
                tinfo.xmlname = finfo;
                continue;
            } 

            // Add the field if it doesn't conflict with other fields.
            {
                var err__prev2 = err;

                err = addFieldInfo(typ, tinfo, _addr_finfo);

                if (err != null) {
                    return (_addr_null!, error.As(err)!);
                }

                err = err__prev2;

            }
        }
    }
    var (ti, _) = tinfoMap.LoadOrStore(typ, tinfo);
    return (ti._<ptr<typeInfo>>(), error.As(null!)!);
}

// structFieldInfo builds and returns a fieldInfo for f.
private static (ptr<fieldInfo>, error) structFieldInfo(reflect.Type typ, ptr<reflect.StructField> _addr_f) {
    ptr<fieldInfo> _p0 = default!;
    error _p0 = default!;
    ref reflect.StructField f = ref _addr_f.val;

    ptr<fieldInfo> finfo = addr(new fieldInfo(idx:f.Index)); 

    // Split the tag from the xml namespace if necessary.
    var tag = f.Tag.Get("xml");
    {
        var i = strings.Index(tag, " ");

        if (i >= 0) {
            (finfo.xmlns, tag) = (tag[..(int)i], tag[(int)i + 1..]);
        }
    } 

    // Parse flags.
    var tokens = strings.Split(tag, ",");
    if (len(tokens) == 1) {
        finfo.flags = fElement;
    }
    else
 {
        tag = tokens[0];
        foreach (var (_, flag) in tokens[(int)1..]) {
            switch (flag) {
                case "attr": 
                    finfo.flags |= fAttr;
                    break;
                case "cdata": 
                    finfo.flags |= fCDATA;
                    break;
                case "chardata": 
                    finfo.flags |= fCharData;
                    break;
                case "innerxml": 
                    finfo.flags |= fInnerXML;
                    break;
                case "comment": 
                    finfo.flags |= fComment;
                    break;
                case "any": 
                    finfo.flags |= fAny;
                    break;
                case "omitempty": 
                    finfo.flags |= fOmitEmpty;
                    break;
            }
        }        var valid = true;
        {
            var mode = finfo.flags & fMode;


            if (mode == 0) 
                finfo.flags |= fElement;
            else if (mode == fAttr || mode == fCDATA || mode == fCharData || mode == fInnerXML || mode == fComment || mode == fAny || mode == fAny | fAttr) 
                if (f.Name == xmlName || tag != "" && mode != fAttr) {
                    valid = false;
                }
            else 
                // This will also catch multiple modes in a single field.
                valid = false;

        }
        if (finfo.flags & fMode == fAny) {
            finfo.flags |= fElement;
        }
        if (finfo.flags & fOmitEmpty != 0 && finfo.flags & (fElement | fAttr) == 0) {
            valid = false;
        }
        if (!valid) {
            return (_addr_null!, error.As(fmt.Errorf("xml: invalid tag in field %s of type %s: %q", f.Name, typ, f.Tag.Get("xml")))!);
        }
    }
    if (finfo.xmlns != "" && tag == "") {
        return (_addr_null!, error.As(fmt.Errorf("xml: namespace without name in field %s of type %s: %q", f.Name, typ, f.Tag.Get("xml")))!);
    }
    if (f.Name == xmlName) { 
        // The XMLName field records the XML element name. Don't
        // process it as usual because its name should default to
        // empty rather than to the field name.
        finfo.name = tag;
        return (_addr_finfo!, error.As(null!)!);
    }
    if (tag == "") { 
        // If the name part of the tag is completely empty, get
        // default from XMLName of underlying struct if feasible,
        // or field name otherwise.
        {
            var xmlname__prev2 = xmlname;

            var xmlname = lookupXMLName(f.Type);

            if (xmlname != null) {
                (finfo.xmlns, finfo.name) = (xmlname.xmlns, xmlname.name);
            }
            else
 {
                finfo.name = f.Name;
            }

            xmlname = xmlname__prev2;

        }
        return (_addr_finfo!, error.As(null!)!);
    }
    var parents = strings.Split(tag, ">");
    if (parents[0] == "") {
        parents[0] = f.Name;
    }
    if (parents[len(parents) - 1] == "") {
        return (_addr_null!, error.As(fmt.Errorf("xml: trailing '>' in field %s of type %s", f.Name, typ))!);
    }
    finfo.name = parents[len(parents) - 1];
    if (len(parents) > 1) {
        if ((finfo.flags & fElement) == 0) {
            return (_addr_null!, error.As(fmt.Errorf("xml: %s chain not valid with %s flag", tag, strings.Join(tokens[(int)1..], ",")))!);
        }
        finfo.parents = parents[..(int)len(parents) - 1];
    }
    if (finfo.flags & fElement != 0) {
        var ftyp = f.Type;
        xmlname = lookupXMLName(ftyp);
        if (xmlname != null && xmlname.name != finfo.name) {
            return (_addr_null!, error.As(fmt.Errorf("xml: name %q in tag of %s.%s conflicts with name %q in %s.XMLName", finfo.name, typ, f.Name, xmlname.name, ftyp))!);
        }
    }
    return (_addr_finfo!, error.As(null!)!);
}

// lookupXMLName returns the fieldInfo for typ's XMLName field
// in case it exists and has a valid xml field tag, otherwise
// it returns nil.
private static ptr<fieldInfo> lookupXMLName(reflect.Type typ) {
    ptr<fieldInfo> xmlname = default!;

    while (typ.Kind() == reflect.Ptr) {
        typ = typ.Elem();
    }
    if (typ.Kind() != reflect.Struct) {
        return _addr_null!;
    }
    for (nint i = 0;
    var n = typ.NumField(); i < n; i++) {
        ref var f = ref heap(typ.Field(i), out ptr<var> _addr_f);
        if (f.Name != xmlName) {
            continue;
        }
        var (finfo, err) = structFieldInfo(typ, _addr_f);
        if (err == null && finfo.name != "") {
            return _addr_finfo!;
        }
        break;
    }
    return _addr_null!;
}

private static nint min(nint a, nint b) {
    if (a <= b) {
        return a;
    }
    return b;
}

// addFieldInfo adds finfo to tinfo.fields if there are no
// conflicts, or if conflicts arise from previous fields that were
// obtained from deeper embedded structures than finfo. In the latter
// case, the conflicting entries are dropped.
// A conflict occurs when the path (parent + name) to a field is
// itself a prefix of another path, or when two paths match exactly.
// It is okay for field paths to share a common, shorter prefix.
private static error addFieldInfo(reflect.Type typ, ptr<typeInfo> _addr_tinfo, ptr<fieldInfo> _addr_newf) {
    ref typeInfo tinfo = ref _addr_tinfo.val;
    ref fieldInfo newf = ref _addr_newf.val;

    slice<nint> conflicts = default;
Loop: 
    // Without conflicts, add the new field and return.
    {
        var i__prev1 = i;

        foreach (var (__i) in tinfo.fields) {
            i = __i;
            var oldf = _addr_tinfo.fields[i];
            if (oldf.flags & fMode != newf.flags & fMode) {
                continue;
            }
            if (oldf.xmlns != "" && newf.xmlns != "" && oldf.xmlns != newf.xmlns) {
                continue;
            }
            var minl = min(len(newf.parents), len(oldf.parents));
            for (nint p = 0; p < minl; p++) {
                if (oldf.parents[p] != newf.parents[p]) {
                    _continueLoop = true;
                    break;
                }
            }

            if (len(oldf.parents) > len(newf.parents)) {
                if (oldf.parents[len(newf.parents)] == newf.name) {
                    conflicts = append(conflicts, i);
                }
            }
            else if (len(oldf.parents) < len(newf.parents)) {
                if (newf.parents[len(oldf.parents)] == oldf.name) {
                    conflicts = append(conflicts, i);
                }
            }
            else
 {
                if (newf.name == oldf.name) {
                    conflicts = append(conflicts, i);
                }
            }
        }
        i = i__prev1;
    }
    if (conflicts == null) {
        tinfo.fields = append(tinfo.fields, newf);
        return error.As(null!)!;
    }
    {
        var i__prev1 = i;

        foreach (var (_, __i) in conflicts) {
            i = __i;
            if (len(tinfo.fields[i].idx) < len(newf.idx)) {
                return error.As(null!)!;
            }
        }
        i = i__prev1;
    }

    {
        var i__prev1 = i;

        foreach (var (_, __i) in conflicts) {
            i = __i;
            oldf = _addr_tinfo.fields[i];
            if (len(oldf.idx) == len(newf.idx)) {
                var f1 = typ.FieldByIndex(oldf.idx);
                var f2 = typ.FieldByIndex(newf.idx);
                return error.As(addr(new TagPathError(typ,f1.Name,f1.Tag.Get("xml"),f2.Name,f2.Tag.Get("xml")))!)!;
            }
        }
        i = i__prev1;
    }

    for (var c = len(conflicts) - 1; c >= 0; c--) {
        var i = conflicts[c];
        copy(tinfo.fields[(int)i..], tinfo.fields[(int)i + 1..]);
        tinfo.fields = tinfo.fields[..(int)len(tinfo.fields) - 1];
    }
    tinfo.fields = append(tinfo.fields, newf);
    return error.As(null!)!;
}

// A TagPathError represents an error in the unmarshaling process
// caused by the use of field tags with conflicting paths.
public partial struct TagPathError {
    public reflect.Type Struct;
    public @string Field1;
    public @string Tag1;
    public @string Field2;
    public @string Tag2;
}

private static @string Error(this ptr<TagPathError> _addr_e) {
    ref TagPathError e = ref _addr_e.val;

    return fmt.Sprintf("%s field %q with tag %q conflicts with field %q with tag %q", e.Struct, e.Field1, e.Tag1, e.Field2, e.Tag2);
}

private static readonly var initNilPointers = true;
private static readonly var dontInitNilPointers = false;

// value returns v's field value corresponding to finfo.
// It's equivalent to v.FieldByIndex(finfo.idx), but when passed
// initNilPointers, it initializes and dereferences pointers as necessary.
// When passed dontInitNilPointers and a nil pointer is reached, the function
// returns a zero reflect.Value.
private static reflect.Value value(this ptr<fieldInfo> _addr_finfo, reflect.Value v, bool shouldInitNilPointers) {
    ref fieldInfo finfo = ref _addr_finfo.val;

    foreach (var (i, x) in finfo.idx) {
        if (i > 0) {
            var t = v.Type();
            if (t.Kind() == reflect.Ptr && t.Elem().Kind() == reflect.Struct) {
                if (v.IsNil()) {
                    if (!shouldInitNilPointers) {
                        return new reflect.Value();
                    }
                    v.Set(reflect.New(v.Type().Elem()));
                }
                v = v.Elem();
            }
        }
        v = v.Field(x);
    }    return v;
}

} // end xml_package
