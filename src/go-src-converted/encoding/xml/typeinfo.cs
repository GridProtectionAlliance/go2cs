// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package xml -- go2cs converted at 2020 October 08 03:43:04 UTC
// import "encoding/xml" ==> using xml = go.encoding.xml_package
// Original source: C:\Go\src\encoding\xml\typeinfo.go
using fmt = go.fmt_package;
using reflect = go.reflect_package;
using strings = go.strings_package;
using sync = go.sync_package;
using static go.builtin;

namespace go {
namespace encoding
{
    public static partial class xml_package
    {
        // typeInfo holds details for the xml representation of a type.
        private partial struct typeInfo
        {
            public ptr<fieldInfo> xmlname;
            public slice<fieldInfo> fields;
        }

        // fieldInfo holds details for the xml representation of a single field.
        private partial struct fieldInfo
        {
            public slice<long> idx;
            public @string name;
            public @string xmlns;
            public fieldFlags flags;
            public slice<@string> parents;
        }

        private partial struct fieldFlags // : long
        {
        }

        private static readonly fieldFlags fElement = (fieldFlags)1L << (int)(iota);
        private static readonly var fAttr = (var)0;
        private static readonly var fCDATA = (var)1;
        private static readonly var fCharData = (var)2;
        private static readonly var fInnerXML = (var)3;
        private static readonly var fComment = (var)4;
        private static readonly var fAny = (var)5;

        private static readonly fMode fOmitEmpty = (fMode)fElement | fAttr | fCDATA | fCharData | fInnerXML | fComment | fAny;

        private static readonly @string xmlName = (@string)"XMLName";


        private static sync.Map tinfoMap = default; // map[reflect.Type]*typeInfo

        private static var nameType = reflect.TypeOf(new Name());

        // getTypeInfo returns the typeInfo structure with details necessary
        // for marshaling and unmarshaling typ.
        private static (ptr<typeInfo>, error) getTypeInfo(reflect.Type typ)
        {
            ptr<typeInfo> _p0 = default!;
            error _p0 = default!;

            {
                var ti__prev1 = ti;

                var (ti, ok) = tinfoMap.Load(typ);

                if (ok)
                {
                    return (ti._<ptr<typeInfo>>(), error.As(null!)!);
                }

                ti = ti__prev1;

            }


            ptr<typeInfo> tinfo = addr(new typeInfo());
            if (typ.Kind() == reflect.Struct && typ != nameType)
            {
                var n = typ.NumField();
                for (long i = 0L; i < n; i++)
                {
                    ref var f = ref heap(typ.Field(i), out ptr<var> _addr_f);
                    if ((f.PkgPath != "" && !f.Anonymous) || f.Tag.Get("xml") == "-")
                    {
                        continue; // Private field
                    } 

                    // For embedded structs, embed its fields.
                    if (f.Anonymous)
                    {
                        var t = f.Type;
                        if (t.Kind() == reflect.Ptr)
                        {
                            t = t.Elem();
                        }

                        if (t.Kind() == reflect.Struct)
                        {
                            var (inner, err) = getTypeInfo(t);
                            if (err != null)
                            {
                                return (_addr_null!, error.As(err)!);
                            }

                            if (tinfo.xmlname == null)
                            {
                                tinfo.xmlname = inner.xmlname;
                            }

                            {
                                var finfo__prev2 = finfo;

                                foreach (var (_, __finfo) in inner.fields)
                                {
                                    finfo = __finfo;
                                    finfo.idx = append(new slice<long>(new long[] { i }), finfo.idx);
                                    {
                                        var err__prev4 = err;

                                        var err = addFieldInfo(typ, _addr_tinfo, _addr_finfo);

                                        if (err != null)
                                        {
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
                    if (err != null)
                    {
                        return (_addr_null!, error.As(err)!);
                    }

                    if (f.Name == xmlName)
                    {
                        tinfo.xmlname = finfo;
                        continue;
                    } 

                    // Add the field if it doesn't conflict with other fields.
                    {
                        var err__prev2 = err;

                        err = addFieldInfo(typ, _addr_tinfo, _addr_finfo);

                        if (err != null)
                        {
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
        private static (ptr<fieldInfo>, error) structFieldInfo(reflect.Type typ, ptr<reflect.StructField> _addr_f)
        {
            ptr<fieldInfo> _p0 = default!;
            error _p0 = default!;
            ref reflect.StructField f = ref _addr_f.val;

            ptr<fieldInfo> finfo = addr(new fieldInfo(idx:f.Index)); 

            // Split the tag from the xml namespace if necessary.
            var tag = f.Tag.Get("xml");
            {
                var i = strings.Index(tag, " ");

                if (i >= 0L)
                {
                    finfo.xmlns = tag[..i];
                    tag = tag[i + 1L..];

                } 

                // Parse flags.

            } 

            // Parse flags.
            var tokens = strings.Split(tag, ",");
            if (len(tokens) == 1L)
            {
                finfo.flags = fElement;
            }
            else
            {
                tag = tokens[0L];
                foreach (var (_, flag) in tokens[1L..])
                {
                    switch (flag)
                    {
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

                } 

                // Validate the flags used.
                var valid = true;
                {
                    var mode = finfo.flags & fMode;


                    if (mode == 0L) 
                        finfo.flags |= fElement;
                    else if (mode == fAttr || mode == fCDATA || mode == fCharData || mode == fInnerXML || mode == fComment || mode == fAny || mode == fAny | fAttr) 
                        if (f.Name == xmlName || tag != "" && mode != fAttr)
                        {
                            valid = false;
                        }

                    else 
                        // This will also catch multiple modes in a single field.
                        valid = false;

                }
                if (finfo.flags & fMode == fAny)
                {
                    finfo.flags |= fElement;
                }

                if (finfo.flags & fOmitEmpty != 0L && finfo.flags & (fElement | fAttr) == 0L)
                {
                    valid = false;
                }

                if (!valid)
                {
                    return (_addr_null!, error.As(fmt.Errorf("xml: invalid tag in field %s of type %s: %q", f.Name, typ, f.Tag.Get("xml")))!);
                }

            } 

            // Use of xmlns without a name is not allowed.
            if (finfo.xmlns != "" && tag == "")
            {
                return (_addr_null!, error.As(fmt.Errorf("xml: namespace without name in field %s of type %s: %q", f.Name, typ, f.Tag.Get("xml")))!);
            }

            if (f.Name == xmlName)
            { 
                // The XMLName field records the XML element name. Don't
                // process it as usual because its name should default to
                // empty rather than to the field name.
                finfo.name = tag;
                return (_addr_finfo!, error.As(null!)!);

            }

            if (tag == "")
            { 
                // If the name part of the tag is completely empty, get
                // default from XMLName of underlying struct if feasible,
                // or field name otherwise.
                {
                    var xmlname__prev2 = xmlname;

                    var xmlname = lookupXMLName(f.Type);

                    if (xmlname != null)
                    {
                        finfo.xmlns = xmlname.xmlns;
                        finfo.name = xmlname.name;

                    }
                    else
                    {
                        finfo.name = f.Name;
                    }

                    xmlname = xmlname__prev2;

                }

                return (_addr_finfo!, error.As(null!)!);

            } 

            // Prepare field name and parents.
            var parents = strings.Split(tag, ">");
            if (parents[0L] == "")
            {
                parents[0L] = f.Name;
            }

            if (parents[len(parents) - 1L] == "")
            {
                return (_addr_null!, error.As(fmt.Errorf("xml: trailing '>' in field %s of type %s", f.Name, typ))!);
            }

            finfo.name = parents[len(parents) - 1L];
            if (len(parents) > 1L)
            {
                if ((finfo.flags & fElement) == 0L)
                {
                    return (_addr_null!, error.As(fmt.Errorf("xml: %s chain not valid with %s flag", tag, strings.Join(tokens[1L..], ",")))!);
                }

                finfo.parents = parents[..len(parents) - 1L];

            } 

            // If the field type has an XMLName field, the names must match
            // so that the behavior of both marshaling and unmarshaling
            // is straightforward and unambiguous.
            if (finfo.flags & fElement != 0L)
            {
                var ftyp = f.Type;
                xmlname = lookupXMLName(ftyp);
                if (xmlname != null && xmlname.name != finfo.name)
                {
                    return (_addr_null!, error.As(fmt.Errorf("xml: name %q in tag of %s.%s conflicts with name %q in %s.XMLName", finfo.name, typ, f.Name, xmlname.name, ftyp))!);
                }

            }

            return (_addr_finfo!, error.As(null!)!);

        }

        // lookupXMLName returns the fieldInfo for typ's XMLName field
        // in case it exists and has a valid xml field tag, otherwise
        // it returns nil.
        private static ptr<fieldInfo> lookupXMLName(reflect.Type typ)
        {
            ptr<fieldInfo> xmlname = default!;

            while (typ.Kind() == reflect.Ptr)
            {
                typ = typ.Elem();
            }

            if (typ.Kind() != reflect.Struct)
            {
                return _addr_null!;
            }

            for (long i = 0L;
            var n = typ.NumField(); i < n; i++)
            {
                ref var f = ref heap(typ.Field(i), out ptr<var> _addr_f);
                if (f.Name != xmlName)
                {
                    continue;
                }

                var (finfo, err) = structFieldInfo(typ, _addr_f);
                if (err == null && finfo.name != "")
                {
                    return _addr_finfo!;
                } 
                // Also consider errors as a non-existent field tag
                // and let getTypeInfo itself report the error.
                break;

            }

            return _addr_null!;

        }

        private static long min(long a, long b)
        {
            if (a <= b)
            {
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
        private static error addFieldInfo(reflect.Type typ, ptr<typeInfo> _addr_tinfo, ptr<fieldInfo> _addr_newf)
        {
            ref typeInfo tinfo = ref _addr_tinfo.val;
            ref fieldInfo newf = ref _addr_newf.val;

            slice<long> conflicts = default;
Loop: 
            // Without conflicts, add the new field and return.
            {
                var i__prev1 = i;

                foreach (var (__i) in tinfo.fields)
                {
                    i = __i;
                    var oldf = _addr_tinfo.fields[i];
                    if (oldf.flags & fMode != newf.flags & fMode)
                    {
                        continue;
                    }

                    if (oldf.xmlns != "" && newf.xmlns != "" && oldf.xmlns != newf.xmlns)
                    {
                        continue;
                    }

                    var minl = min(len(newf.parents), len(oldf.parents));
                    for (long p = 0L; p < minl; p++)
                    {
                        if (oldf.parents[p] != newf.parents[p])
                        {
                            _continueLoop = true;
                            break;
                        }

                    }

                    if (len(oldf.parents) > len(newf.parents))
                    {
                        if (oldf.parents[len(newf.parents)] == newf.name)
                        {
                            conflicts = append(conflicts, i);
                        }

                    }
                    else if (len(oldf.parents) < len(newf.parents))
                    {
                        if (newf.parents[len(oldf.parents)] == oldf.name)
                        {
                            conflicts = append(conflicts, i);
                        }

                    }
                    else
                    {
                        if (newf.name == oldf.name)
                        {
                            conflicts = append(conflicts, i);
                        }

                    }

                } 
                // Without conflicts, add the new field and return.

                i = i__prev1;
            }
            if (conflicts == null)
            {
                tinfo.fields = append(tinfo.fields, newf);
                return error.As(null!)!;
            } 

            // If any conflict is shallower, ignore the new field.
            // This matches the Go field resolution on embedding.
            {
                var i__prev1 = i;

                foreach (var (_, __i) in conflicts)
                {
                    i = __i;
                    if (len(tinfo.fields[i].idx) < len(newf.idx))
                    {
                        return error.As(null!)!;
                    }

                } 

                // Otherwise, if any of them is at the same depth level, it's an error.

                i = i__prev1;
            }

            {
                var i__prev1 = i;

                foreach (var (_, __i) in conflicts)
                {
                    i = __i;
                    oldf = _addr_tinfo.fields[i];
                    if (len(oldf.idx) == len(newf.idx))
                    {
                        var f1 = typ.FieldByIndex(oldf.idx);
                        var f2 = typ.FieldByIndex(newf.idx);
                        return error.As(addr(new TagPathError(typ,f1.Name,f1.Tag.Get("xml"),f2.Name,f2.Tag.Get("xml")))!)!;
                    }

                } 

                // Otherwise, the new field is shallower, and thus takes precedence,
                // so drop the conflicting fields from tinfo and append the new one.

                i = i__prev1;
            }

            for (var c = len(conflicts) - 1L; c >= 0L; c--)
            {
                var i = conflicts[c];
                copy(tinfo.fields[i..], tinfo.fields[i + 1L..]);
                tinfo.fields = tinfo.fields[..len(tinfo.fields) - 1L];
            }

            tinfo.fields = append(tinfo.fields, newf);
            return error.As(null!)!;

        }

        // A TagPathError represents an error in the unmarshaling process
        // caused by the use of field tags with conflicting paths.
        public partial struct TagPathError
        {
            public reflect.Type Struct;
            public @string Field1;
            public @string Tag1;
            public @string Field2;
            public @string Tag2;
        }

        private static @string Error(this ptr<TagPathError> _addr_e)
        {
            ref TagPathError e = ref _addr_e.val;

            return fmt.Sprintf("%s field %q with tag %q conflicts with field %q with tag %q", e.Struct, e.Field1, e.Tag1, e.Field2, e.Tag2);
        }

        private static readonly var initNilPointers = (var)true;
        private static readonly var dontInitNilPointers = (var)false;


        // value returns v's field value corresponding to finfo.
        // It's equivalent to v.FieldByIndex(finfo.idx), but when passed
        // initNilPointers, it initializes and dereferences pointers as necessary.
        // When passed dontInitNilPointers and a nil pointer is reached, the function
        // returns a zero reflect.Value.
        private static reflect.Value value(this ptr<fieldInfo> _addr_finfo, reflect.Value v, bool shouldInitNilPointers)
        {
            ref fieldInfo finfo = ref _addr_finfo.val;

            foreach (var (i, x) in finfo.idx)
            {
                if (i > 0L)
                {
                    var t = v.Type();
                    if (t.Kind() == reflect.Ptr && t.Elem().Kind() == reflect.Struct)
                    {
                        if (v.IsNil())
                        {
                            if (!shouldInitNilPointers)
                            {
                                return new reflect.Value();
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
    }
}}
