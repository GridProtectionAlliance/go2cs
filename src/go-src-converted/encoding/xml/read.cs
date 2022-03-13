// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package xml -- go2cs converted at 2022 March 13 05:40:02 UTC
// import "encoding/xml" ==> using xml = go.encoding.xml_package
// Original source: C:\Program Files\Go\src\encoding\xml\read.go
namespace go.encoding;

using bytes = bytes_package;
using encoding = encoding_package;
using errors = errors_package;
using fmt = fmt_package;
using reflect = reflect_package;
using strconv = strconv_package;
using strings = strings_package;


// BUG(rsc): Mapping between XML elements and data structures is inherently flawed:
// an XML element is an order-dependent collection of anonymous
// values, while a data structure is an order-independent collection
// of named values.
// See package json for a textual representation more suitable
// to data structures.

// Unmarshal parses the XML-encoded data and stores the result in
// the value pointed to by v, which must be an arbitrary struct,
// slice, or string. Well-formed data that does not fit into v is
// discarded.
//
// Because Unmarshal uses the reflect package, it can only assign
// to exported (upper case) fields. Unmarshal uses a case-sensitive
// comparison to match XML element names to tag values and struct
// field names.
//
// Unmarshal maps an XML element to a struct using the following rules.
// In the rules, the tag of a field refers to the value associated with the
// key 'xml' in the struct field's tag (see the example above).
//
//   * If the struct has a field of type []byte or string with tag
//      ",innerxml", Unmarshal accumulates the raw XML nested inside the
//      element in that field. The rest of the rules still apply.
//
//   * If the struct has a field named XMLName of type Name,
//      Unmarshal records the element name in that field.
//
//   * If the XMLName field has an associated tag of the form
//      "name" or "namespace-URL name", the XML element must have
//      the given name (and, optionally, name space) or else Unmarshal
//      returns an error.
//
//   * If the XML element has an attribute whose name matches a
//      struct field name with an associated tag containing ",attr" or
//      the explicit name in a struct field tag of the form "name,attr",
//      Unmarshal records the attribute value in that field.
//
//   * If the XML element has an attribute not handled by the previous
//      rule and the struct has a field with an associated tag containing
//      ",any,attr", Unmarshal records the attribute value in the first
//      such field.
//
//   * If the XML element contains character data, that data is
//      accumulated in the first struct field that has tag ",chardata".
//      The struct field may have type []byte or string.
//      If there is no such field, the character data is discarded.
//
//   * If the XML element contains comments, they are accumulated in
//      the first struct field that has tag ",comment".  The struct
//      field may have type []byte or string. If there is no such
//      field, the comments are discarded.
//
//   * If the XML element contains a sub-element whose name matches
//      the prefix of a tag formatted as "a" or "a>b>c", unmarshal
//      will descend into the XML structure looking for elements with the
//      given names, and will map the innermost elements to that struct
//      field. A tag starting with ">" is equivalent to one starting
//      with the field name followed by ">".
//
//   * If the XML element contains a sub-element whose name matches
//      a struct field's XMLName tag and the struct field has no
//      explicit name tag as per the previous rule, unmarshal maps
//      the sub-element to that struct field.
//
//   * If the XML element contains a sub-element whose name matches a
//      field without any mode flags (",attr", ",chardata", etc), Unmarshal
//      maps the sub-element to that struct field.
//
//   * If the XML element contains a sub-element that hasn't matched any
//      of the above rules and the struct has a field with tag ",any",
//      unmarshal maps the sub-element to that struct field.
//
//   * An anonymous struct field is handled as if the fields of its
//      value were part of the outer struct.
//
//   * A struct field with tag "-" is never unmarshaled into.
//
// If Unmarshal encounters a field type that implements the Unmarshaler
// interface, Unmarshal calls its UnmarshalXML method to produce the value from
// the XML element.  Otherwise, if the value implements
// encoding.TextUnmarshaler, Unmarshal calls that value's UnmarshalText method.
//
// Unmarshal maps an XML element to a string or []byte by saving the
// concatenation of that element's character data in the string or
// []byte. The saved []byte is never nil.
//
// Unmarshal maps an attribute value to a string or []byte by saving
// the value in the string or slice.
//
// Unmarshal maps an attribute value to an Attr by saving the attribute,
// including its name, in the Attr.
//
// Unmarshal maps an XML element or attribute value to a slice by
// extending the length of the slice and mapping the element or attribute
// to the newly created value.
//
// Unmarshal maps an XML element or attribute value to a bool by
// setting it to the boolean value represented by the string. Whitespace
// is trimmed and ignored.
//
// Unmarshal maps an XML element or attribute value to an integer or
// floating-point field by setting the field to the result of
// interpreting the string value in decimal. There is no check for
// overflow. Whitespace is trimmed and ignored.
//
// Unmarshal maps an XML element to a Name by recording the element
// name.
//
// Unmarshal maps an XML element to a pointer by setting the pointer
// to a freshly allocated value and then mapping the element to that value.
//
// A missing element or empty attribute value will be unmarshaled as a zero value.
// If the field is a slice, a zero value will be appended to the field. Otherwise, the
// field will be set to its zero value.

public static partial class xml_package {

public static error Unmarshal(slice<byte> data, object v) {
    return error.As(NewDecoder(bytes.NewReader(data)).Decode(v))!;
}

// Decode works like Unmarshal, except it reads the decoder
// stream to find the start element.
private static error Decode(this ptr<Decoder> _addr_d, object v) {
    ref Decoder d = ref _addr_d.val;

    return error.As(d.DecodeElement(v, null))!;
}

// DecodeElement works like Unmarshal except that it takes
// a pointer to the start XML element to decode into v.
// It is useful when a client reads some raw XML tokens itself
// but also wants to defer to Unmarshal for some elements.
private static error DecodeElement(this ptr<Decoder> _addr_d, object v, ptr<StartElement> _addr_start) {
    ref Decoder d = ref _addr_d.val;
    ref StartElement start = ref _addr_start.val;

    var val = reflect.ValueOf(v);
    if (val.Kind() != reflect.Ptr) {
        return error.As(errors.New("non-pointer passed to Unmarshal"))!;
    }
    return error.As(d.unmarshal(val.Elem(), start))!;
}

// An UnmarshalError represents an error in the unmarshaling process.
public partial struct UnmarshalError { // : @string
}

public static @string Error(this UnmarshalError e) {
    return string(e);
}

// Unmarshaler is the interface implemented by objects that can unmarshal
// an XML element description of themselves.
//
// UnmarshalXML decodes a single XML element
// beginning with the given start element.
// If it returns an error, the outer call to Unmarshal stops and
// returns that error.
// UnmarshalXML must consume exactly one XML element.
// One common implementation strategy is to unmarshal into
// a separate value with a layout matching the expected XML
// using d.DecodeElement, and then to copy the data from
// that value into the receiver.
// Another common strategy is to use d.Token to process the
// XML object one token at a time.
// UnmarshalXML may not use d.RawToken.
public partial interface Unmarshaler {
    error UnmarshalXML(ptr<Decoder> d, StartElement start);
}

// UnmarshalerAttr is the interface implemented by objects that can unmarshal
// an XML attribute description of themselves.
//
// UnmarshalXMLAttr decodes a single XML attribute.
// If it returns an error, the outer call to Unmarshal stops and
// returns that error.
// UnmarshalXMLAttr is used only for struct fields with the
// "attr" option in the field tag.
public partial interface UnmarshalerAttr {
    error UnmarshalXMLAttr(Attr attr);
}

// receiverType returns the receiver type to use in an expression like "%s.MethodName".
private static @string receiverType(object val) {
    var t = reflect.TypeOf(val);
    if (t.Name() != "") {
        return t.String();
    }
    return "(" + t.String() + ")";
}

// unmarshalInterface unmarshals a single XML element into val.
// start is the opening tag of the element.
private static error unmarshalInterface(this ptr<Decoder> _addr_d, Unmarshaler val, ptr<StartElement> _addr_start) {
    ref Decoder d = ref _addr_d.val;
    ref StartElement start = ref _addr_start.val;
 
    // Record that decoder must stop at end tag corresponding to start.
    d.pushEOF();

    d.unmarshalDepth++;
    var err = val.UnmarshalXML(d, start);
    d.unmarshalDepth--;
    if (err != null) {
        d.popEOF();
        return error.As(err)!;
    }
    if (!d.popEOF()) {
        return error.As(fmt.Errorf("xml: %s.UnmarshalXML did not consume entire <%s> element", receiverType(val), start.Name.Local))!;
    }
    return error.As(null!)!;
}

// unmarshalTextInterface unmarshals a single XML element into val.
// The chardata contained in the element (but not its children)
// is passed to the text unmarshaler.
private static error unmarshalTextInterface(this ptr<Decoder> _addr_d, encoding.TextUnmarshaler val) {
    ref Decoder d = ref _addr_d.val;

    slice<byte> buf = default;
    nint depth = 1;
    while (depth > 0) {
        var (t, err) = d.Token();
        if (err != null) {
            return error.As(err)!;
        }
        switch (t.type()) {
            case CharData t:
                if (depth == 1) {
                    buf = append(buf, t);
                }
                break;
            case StartElement t:
                depth++;
                break;
            case EndElement t:
                depth--;
                break;
        }
    }
    return error.As(val.UnmarshalText(buf))!;
}

// unmarshalAttr unmarshals a single XML attribute into val.
private static error unmarshalAttr(this ptr<Decoder> _addr_d, reflect.Value val, Attr attr) {
    ref Decoder d = ref _addr_d.val;

    if (val.Kind() == reflect.Ptr) {
        if (val.IsNil()) {
            val.Set(reflect.New(val.Type().Elem()));
        }
        val = val.Elem();
    }
    if (val.CanInterface() && val.Type().Implements(unmarshalerAttrType)) { 
        // This is an unmarshaler with a non-pointer receiver,
        // so it's likely to be incorrect, but we do what we're told.
        return error.As(val.Interface()._<UnmarshalerAttr>().UnmarshalXMLAttr(attr)!)!;
    }
    if (val.CanAddr()) {
        var pv = val.Addr();
        if (pv.CanInterface() && pv.Type().Implements(unmarshalerAttrType)) {
            return error.As(pv.Interface()._<UnmarshalerAttr>().UnmarshalXMLAttr(attr)!)!;
        }
    }
    if (val.CanInterface() && val.Type().Implements(textUnmarshalerType)) { 
        // This is an unmarshaler with a non-pointer receiver,
        // so it's likely to be incorrect, but we do what we're told.
        return error.As(val.Interface()._<encoding.TextUnmarshaler>().UnmarshalText((slice<byte>)attr.Value))!;
    }
    if (val.CanAddr()) {
        pv = val.Addr();
        if (pv.CanInterface() && pv.Type().Implements(textUnmarshalerType)) {
            return error.As(pv.Interface()._<encoding.TextUnmarshaler>().UnmarshalText((slice<byte>)attr.Value))!;
        }
    }
    if (val.Type().Kind() == reflect.Slice && val.Type().Elem().Kind() != reflect.Uint8) { 
        // Slice of element values.
        // Grow slice.
        var n = val.Len();
        val.Set(reflect.Append(val, reflect.Zero(val.Type().Elem()))); 

        // Recur to read element into slice.
        {
            var err = d.unmarshalAttr(val.Index(n), attr);

            if (err != null) {
                val.SetLen(n);
                return error.As(err)!;
            }

        }
        return error.As(null!)!;
    }
    if (val.Type() == attrType) {
        val.Set(reflect.ValueOf(attr));
        return error.As(null!)!;
    }
    return error.As(copyValue(val, (slice<byte>)attr.Value))!;
}

private static var attrType = reflect.TypeOf(new Attr());private static var unmarshalerType = reflect.TypeOf((Unmarshaler.val)(null)).Elem();private static var unmarshalerAttrType = reflect.TypeOf((UnmarshalerAttr.val)(null)).Elem();private static var textUnmarshalerType = reflect.TypeOf((encoding.TextUnmarshaler.val)(null)).Elem();

// Unmarshal a single XML element into val.
private static error unmarshal(this ptr<Decoder> _addr_d, reflect.Value val, ptr<StartElement> _addr_start) {
    ref Decoder d = ref _addr_d.val;
    ref StartElement start = ref _addr_start.val;
 
    // Find start element if we need it.
    if (start == null) {
        while (true) {
            var (tok, err) = d.Token();
            if (err != null) {
                return error.As(err)!;
            }
            {
                StartElement t__prev2 = t;

                StartElement (t, ok) = tok._<StartElement>();

                if (ok) {
                    start = _addr_t;
                    break;
                }

                t = t__prev2;

            }
        }
    }
    if (val.Kind() == reflect.Interface && !val.IsNil()) {
        var e = val.Elem();
        if (e.Kind() == reflect.Ptr && !e.IsNil()) {
            val = e;
        }
    }
    if (val.Kind() == reflect.Ptr) {
        if (val.IsNil()) {
            val.Set(reflect.New(val.Type().Elem()));
        }
        val = val.Elem();
    }
    if (val.CanInterface() && val.Type().Implements(unmarshalerType)) { 
        // This is an unmarshaler with a non-pointer receiver,
        // so it's likely to be incorrect, but we do what we're told.
        return error.As(d.unmarshalInterface(val.Interface()._<Unmarshaler>(), start))!;
    }
    if (val.CanAddr()) {
        var pv = val.Addr();
        if (pv.CanInterface() && pv.Type().Implements(unmarshalerType)) {
            return error.As(d.unmarshalInterface(pv.Interface()._<Unmarshaler>(), start))!;
        }
    }
    if (val.CanInterface() && val.Type().Implements(textUnmarshalerType)) {
        return error.As(d.unmarshalTextInterface(val.Interface()._<encoding.TextUnmarshaler>()))!;
    }
    if (val.CanAddr()) {
        pv = val.Addr();
        if (pv.CanInterface() && pv.Type().Implements(textUnmarshalerType)) {
            return error.As(d.unmarshalTextInterface(pv.Interface()._<encoding.TextUnmarshaler>()))!;
        }
    }
    slice<byte> data = default;    reflect.Value saveData = default;    slice<byte> comment = default;    reflect.Value saveComment = default;    reflect.Value saveXML = default;    nint saveXMLIndex = default;    slice<byte> saveXMLData = default;    reflect.Value saveAny = default;    reflect.Value sv = default;    ptr<typeInfo> tinfo;    error err = default!;

    {
        var v = val;


        if (v.Kind() == reflect.Interface) 
            // TODO: For now, simply ignore the field. In the near
            //       future we may choose to unmarshal the start
            //       element on it, if not nil.
            return error.As(d.Skip())!;
        else if (v.Kind() == reflect.Slice) 
            var typ = v.Type();
            if (typ.Elem().Kind() == reflect.Uint8) { 
                // []byte
                saveData = v;
                break;
            } 

            // Slice of element values.
            // Grow slice.
            var n = v.Len();
            v.Set(reflect.Append(val, reflect.Zero(v.Type().Elem()))); 

            // Recur to read element into slice.
            {
                error err__prev1 = err;

                err = d.unmarshal(v.Index(n), start);

                if (err != null) {
                    v.SetLen(n);
                    return error.As(err)!;
                }

                err = err__prev1;

            }
            return error.As(null!)!;
        else if (v.Kind() == reflect.Bool || v.Kind() == reflect.Float32 || v.Kind() == reflect.Float64 || v.Kind() == reflect.Int || v.Kind() == reflect.Int8 || v.Kind() == reflect.Int16 || v.Kind() == reflect.Int32 || v.Kind() == reflect.Int64 || v.Kind() == reflect.Uint || v.Kind() == reflect.Uint8 || v.Kind() == reflect.Uint16 || v.Kind() == reflect.Uint32 || v.Kind() == reflect.Uint64 || v.Kind() == reflect.Uintptr || v.Kind() == reflect.String) 
            saveData = v;
        else if (v.Kind() == reflect.Struct) 
            typ = v.Type();
            if (typ == nameType) {
                v.Set(reflect.ValueOf(start.Name));
                break;
            }
            sv = v;
            tinfo, err = getTypeInfo(typ);
            if (err != null) {
                return error.As(err)!;
            } 

            // Validate and assign element name.
            if (tinfo.xmlname != null) {
                var finfo = tinfo.xmlname;
                if (finfo.name != "" && finfo.name != start.Name.Local) {
                    return error.As(UnmarshalError("expected element type <" + finfo.name + "> but have <" + start.Name.Local + ">"))!;
                }
                if (finfo.xmlns != "" && finfo.xmlns != start.Name.Space) {
                    e = "expected element <" + finfo.name + "> in name space " + finfo.xmlns + " but have ";
                    if (start.Name.Space == "") {
                        e += "no name space";
                    }
                    else
 {
                        e += start.Name.Space;
                    }
                    return error.As(UnmarshalError(e))!;
                }
                var fv = finfo.value(sv, initNilPointers);
                {
                    Name (_, ok) = fv.Interface()._<Name>();

                    if (ok) {
                        fv.Set(reflect.ValueOf(start.Name));
                    }

                }
            } 

            // Assign attributes.
            foreach (var (_, a) in start.Attr) {
                var handled = false;
                nint any = -1;
                {
                    var i__prev2 = i;

                    foreach (var (__i) in tinfo.fields) {
                        i = __i;
                        finfo = _addr_tinfo.fields[i];

                        if (finfo.flags & fMode == fAttr) 
                            var strv = finfo.value(sv, initNilPointers);
                            if (a.Name.Local == finfo.name && (finfo.xmlns == "" || finfo.xmlns == a.Name.Space)) {
                                {
                                    error err__prev2 = err;

                                    err = d.unmarshalAttr(strv, a);

                                    if (err != null) {
                                        return error.As(err)!;
                                    }

                                    err = err__prev2;

                                }
                                handled = true;
                            }
                        else if (finfo.flags & fMode == fAny | fAttr) 
                            if (any == -1) {
                                any = i;
                            }
                                            }

                    i = i__prev2;
                }

                if (!handled && any >= 0) {
                    finfo = _addr_tinfo.fields[any];
                    strv = finfo.value(sv, initNilPointers);
                    {
                        error err__prev2 = err;

                        err = d.unmarshalAttr(strv, a);

                        if (err != null) {
                            return error.As(err)!;
                        }

                        err = err__prev2;

                    }
                }
            } 

            // Determine whether we need to save character data or comments.
            {
                var i__prev1 = i;

                foreach (var (__i) in tinfo.fields) {
                    i = __i;
                    finfo = _addr_tinfo.fields[i];

                    if (finfo.flags & fMode == fCDATA || finfo.flags & fMode == fCharData) 
                        if (!saveData.IsValid()) {
                            saveData = finfo.value(sv, initNilPointers);
                        }
                    else if (finfo.flags & fMode == fComment) 
                        if (!saveComment.IsValid()) {
                            saveComment = finfo.value(sv, initNilPointers);
                        }
                    else if (finfo.flags & fMode == fAny || finfo.flags & fMode == fAny | fElement) 
                        if (!saveAny.IsValid()) {
                            saveAny = finfo.value(sv, initNilPointers);
                        }
                    else if (finfo.flags & fMode == fInnerXML) 
                        if (!saveXML.IsValid()) {
                            saveXML = finfo.value(sv, initNilPointers);
                            if (d.saved == null) {
                                saveXMLIndex = 0;
                                d.saved = @new<bytes.Buffer>();
                            }
                            else
 {
                                saveXMLIndex = d.savedOffset();
                            }
                        }
                                    }

                i = i__prev1;
            }
        else 
            return error.As(errors.New("unknown type " + v.Type().String()))!;

    } 

    // Find end element.
    // Process sub-elements along the way.
Loop:

    while (true) {
        nint savedOffset = default;
        if (saveXML.IsValid()) {
            savedOffset = d.savedOffset();
        }
        (tok, err) = d.Token();
        if (err != null) {
            return error.As(err)!;
        }
        switch (tok.type()) {
            case StartElement t:
                var consumed = false;
                if (sv.IsValid()) {
                    consumed, err = d.unmarshalPath(tinfo, sv, null, _addr_t);
                    if (err != null) {
                        return error.As(err)!;
                    }
                    if (!consumed && saveAny.IsValid()) {
                        consumed = true;
                        {
                            error err__prev3 = err;

                            err = d.unmarshal(saveAny, _addr_t);

                            if (err != null) {
                                return error.As(err)!;
                            }

                            err = err__prev3;

                        }
                    }
                }
                if (!consumed) {
                    {
                        error err__prev2 = err;

                        err = d.Skip();

                        if (err != null) {
                            return error.As(err)!;
                        }

                        err = err__prev2;

                    }
                }
                break;
            case EndElement t:
                if (saveXML.IsValid()) {
                    saveXMLData = d.saved.Bytes()[(int)saveXMLIndex..(int)savedOffset];
                    if (saveXMLIndex == 0) {
                        d.saved = null;
                    }
                }
                _breakLoop = true;

                break;
                break;
            case CharData t:
                if (saveData.IsValid()) {
                    data = append(data, t);
                }
                break;
            case Comment t:
                if (saveComment.IsValid()) {
                    comment = append(comment, t);
                }
                break;
        }
    }
    if (saveData.IsValid() && saveData.CanInterface() && saveData.Type().Implements(textUnmarshalerType)) {
        {
            error err__prev2 = err;

            err = saveData.Interface()._<encoding.TextUnmarshaler>().UnmarshalText(data);

            if (err != null) {
                return error.As(err)!;
            }

            err = err__prev2;

        }
        saveData = new reflect.Value();
    }
    if (saveData.IsValid() && saveData.CanAddr()) {
        pv = saveData.Addr();
        if (pv.CanInterface() && pv.Type().Implements(textUnmarshalerType)) {
            {
                error err__prev3 = err;

                err = pv.Interface()._<encoding.TextUnmarshaler>().UnmarshalText(data);

                if (err != null) {
                    return error.As(err)!;
                }

                err = err__prev3;

            }
            saveData = new reflect.Value();
        }
    }
    {
        error err__prev1 = err;

        err = copyValue(saveData, data);

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    }

    {
        StartElement t__prev1 = t;

        ref var t = ref heap(saveComment, out ptr<var> _addr_t);


        if (t.Kind() == reflect.String) 
            t.SetString(string(comment));
        else if (t.Kind() == reflect.Slice) 
            t.Set(reflect.ValueOf(comment));


        t = t__prev1;
    }

    {
        StartElement t__prev1 = t;

        t = saveXML;


        if (t.Kind() == reflect.String) 
            t.SetString(string(saveXMLData));
        else if (t.Kind() == reflect.Slice) 
            if (t.Type().Elem().Kind() == reflect.Uint8) {
                t.Set(reflect.ValueOf(saveXMLData));
            }


        t = t__prev1;
    }

    return error.As(null!)!;
}

private static error copyValue(reflect.Value dst, slice<byte> src) {
    error err = default!;

    var dst0 = dst;

    if (dst.Kind() == reflect.Ptr) {
        if (dst.IsNil()) {
            dst.Set(reflect.New(dst.Type().Elem()));
        }
        dst = dst.Elem();
    }

    if (dst.Kind() == reflect.Invalid)     else if (dst.Kind() == reflect.Int || dst.Kind() == reflect.Int8 || dst.Kind() == reflect.Int16 || dst.Kind() == reflect.Int32 || dst.Kind() == reflect.Int64) 
        if (len(src) == 0) {
            dst.SetInt(0);
            return error.As(null!)!;
        }
        var (itmp, err) = strconv.ParseInt(strings.TrimSpace(string(src)), 10, dst.Type().Bits());
        if (err != null) {
            return error.As(err)!;
        }
        dst.SetInt(itmp);
    else if (dst.Kind() == reflect.Uint || dst.Kind() == reflect.Uint8 || dst.Kind() == reflect.Uint16 || dst.Kind() == reflect.Uint32 || dst.Kind() == reflect.Uint64 || dst.Kind() == reflect.Uintptr) 
        if (len(src) == 0) {
            dst.SetUint(0);
            return error.As(null!)!;
        }
        var (utmp, err) = strconv.ParseUint(strings.TrimSpace(string(src)), 10, dst.Type().Bits());
        if (err != null) {
            return error.As(err)!;
        }
        dst.SetUint(utmp);
    else if (dst.Kind() == reflect.Float32 || dst.Kind() == reflect.Float64) 
        if (len(src) == 0) {
            dst.SetFloat(0);
            return error.As(null!)!;
        }
        var (ftmp, err) = strconv.ParseFloat(strings.TrimSpace(string(src)), dst.Type().Bits());
        if (err != null) {
            return error.As(err)!;
        }
        dst.SetFloat(ftmp);
    else if (dst.Kind() == reflect.Bool) 
        if (len(src) == 0) {
            dst.SetBool(false);
            return error.As(null!)!;
        }
        var (value, err) = strconv.ParseBool(strings.TrimSpace(string(src)));
        if (err != null) {
            return error.As(err)!;
        }
        dst.SetBool(value);
    else if (dst.Kind() == reflect.String) 
        dst.SetString(string(src));
    else if (dst.Kind() == reflect.Slice) 
        if (len(src) == 0) { 
            // non-nil to flag presence
            src = new slice<byte>(new byte[] {  });
        }
        dst.SetBytes(src);
    else 
        return error.As(errors.New("cannot unmarshal into " + dst0.Type().String()))!;
        return error.As(null!)!;
}

// unmarshalPath walks down an XML structure looking for wanted
// paths, and calls unmarshal on them.
// The consumed result tells whether XML elements have been consumed
// from the Decoder until start's matching end element, or if it's
// still untouched because start is uninteresting for sv's fields.
private static (bool, error) unmarshalPath(this ptr<Decoder> _addr_d, ptr<typeInfo> _addr_tinfo, reflect.Value sv, slice<@string> parents, ptr<StartElement> _addr_start) {
    bool consumed = default;
    error err = default!;
    ref Decoder d = ref _addr_d.val;
    ref typeInfo tinfo = ref _addr_tinfo.val;
    ref StartElement start = ref _addr_start.val;

    var recurse = false;
Loop:
    foreach (var (i) in tinfo.fields) {
        var finfo = _addr_tinfo.fields[i];
        if (finfo.flags & fElement == 0 || len(finfo.parents) < len(parents) || finfo.xmlns != "" && finfo.xmlns != start.Name.Space) {
            continue;
        }
        foreach (var (j) in parents) {
            if (parents[j] != finfo.parents[j]) {
                _continueLoop = true;
                break;
            }
        }        if (len(finfo.parents) == len(parents) && finfo.name == start.Name.Local) { 
            // It's a perfect match, unmarshal the field.
            return (true, error.As(d.unmarshal(finfo.value(sv, initNilPointers), start))!);
        }
        if (len(finfo.parents) > len(parents) && finfo.parents[len(parents)] == start.Name.Local) { 
            // It's a prefix for the field. Break and recurse
            // since it's not ok for one field path to be itself
            // the prefix for another field path.
            recurse = true; 

            // We can reuse the same slice as long as we
            // don't try to append to it.
            parents = finfo.parents[..(int)len(parents) + 1];
            break;
        }
    }    if (!recurse) { 
        // We have no business with this element.
        return (false, error.As(null!)!);
    }
    while (true) {
        Token tok = default;
        tok, err = d.Token();
        if (err != null) {
            return (true, error.As(err)!);
        }
        switch (tok.type()) {
            case StartElement t:
                var (consumed2, err) = d.unmarshalPath(tinfo, sv, parents, _addr_t);
                if (err != null) {
                    return (true, error.As(err)!);
                }
                if (!consumed2) {
                    {
                        var err = d.Skip();

                        if (err != null) {
                            return (true, error.As(err)!);
                        }

                    }
                }
                break;
            case EndElement t:
                return (true, error.As(null!)!);
                break;
        }
    }
}

// Skip reads tokens until it has consumed the end element
// matching the most recent start element already consumed.
// It recurs if it encounters a start element, so it can be used to
// skip nested structures.
// It returns nil if it finds an end element matching the start
// element; otherwise it returns an error describing the problem.
private static error Skip(this ptr<Decoder> _addr_d) {
    ref Decoder d = ref _addr_d.val;

    while (true) {
        var (tok, err) = d.Token();
        if (err != null) {
            return error.As(err)!;
        }
        switch (tok.type()) {
            case StartElement _:
                {
                    var err = d.Skip();

                    if (err != null) {
                        return error.As(err)!;
                    }

                }
                break;
            case EndElement _:
                return error.As(null!)!;
                break;
        }
    }
}

} // end xml_package
