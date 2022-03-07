// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package json implements encoding and decoding of JSON as defined in
// RFC 7159. The mapping between JSON and Go values is described
// in the documentation for the Marshal and Unmarshal functions.
//
// See "JSON and Go" for an introduction to this package:
// https://golang.org/doc/articles/json_and_go.html
// package json -- go2cs converted at 2022 March 06 22:25:19 UTC
// import "encoding/json" ==> using json = go.encoding.json_package
// Original source: C:\Program Files\Go\src\encoding\json\encode.go
using bytes = go.bytes_package;
using encoding = go.encoding_package;
using base64 = go.encoding.base64_package;
using fmt = go.fmt_package;
using math = go.math_package;
using reflect = go.reflect_package;
using sort = go.sort_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using sync = go.sync_package;
using unicode = go.unicode_package;
using utf8 = go.unicode.utf8_package;
using System;


namespace go.encoding;

public static partial class json_package {

    // Marshal returns the JSON encoding of v.
    //
    // Marshal traverses the value v recursively.
    // If an encountered value implements the Marshaler interface
    // and is not a nil pointer, Marshal calls its MarshalJSON method
    // to produce JSON. If no MarshalJSON method is present but the
    // value implements encoding.TextMarshaler instead, Marshal calls
    // its MarshalText method and encodes the result as a JSON string.
    // The nil pointer exception is not strictly necessary
    // but mimics a similar, necessary exception in the behavior of
    // UnmarshalJSON.
    //
    // Otherwise, Marshal uses the following type-dependent default encodings:
    //
    // Boolean values encode as JSON booleans.
    //
    // Floating point, integer, and Number values encode as JSON numbers.
    //
    // String values encode as JSON strings coerced to valid UTF-8,
    // replacing invalid bytes with the Unicode replacement rune.
    // So that the JSON will be safe to embed inside HTML <script> tags,
    // the string is encoded using HTMLEscape,
    // which replaces "<", ">", "&", U+2028, and U+2029 are escaped
    // to "\u003c","\u003e", "\u0026", "\u2028", and "\u2029".
    // This replacement can be disabled when using an Encoder,
    // by calling SetEscapeHTML(false).
    //
    // Array and slice values encode as JSON arrays, except that
    // []byte encodes as a base64-encoded string, and a nil slice
    // encodes as the null JSON value.
    //
    // Struct values encode as JSON objects.
    // Each exported struct field becomes a member of the object, using the
    // field name as the object key, unless the field is omitted for one of the
    // reasons given below.
    //
    // The encoding of each struct field can be customized by the format string
    // stored under the "json" key in the struct field's tag.
    // The format string gives the name of the field, possibly followed by a
    // comma-separated list of options. The name may be empty in order to
    // specify options without overriding the default field name.
    //
    // The "omitempty" option specifies that the field should be omitted
    // from the encoding if the field has an empty value, defined as
    // false, 0, a nil pointer, a nil interface value, and any empty array,
    // slice, map, or string.
    //
    // As a special case, if the field tag is "-", the field is always omitted.
    // Note that a field with name "-" can still be generated using the tag "-,".
    //
    // Examples of struct field tags and their meanings:
    //
    //   // Field appears in JSON as key "myName".
    //   Field int `json:"myName"`
    //
    //   // Field appears in JSON as key "myName" and
    //   // the field is omitted from the object if its value is empty,
    //   // as defined above.
    //   Field int `json:"myName,omitempty"`
    //
    //   // Field appears in JSON as key "Field" (the default), but
    //   // the field is skipped if empty.
    //   // Note the leading comma.
    //   Field int `json:",omitempty"`
    //
    //   // Field is ignored by this package.
    //   Field int `json:"-"`
    //
    //   // Field appears in JSON as key "-".
    //   Field int `json:"-,"`
    //
    // The "string" option signals that a field is stored as JSON inside a
    // JSON-encoded string. It applies only to fields of string, floating point,
    // integer, or boolean types. This extra level of encoding is sometimes used
    // when communicating with JavaScript programs:
    //
    //    Int64String int64 `json:",string"`
    //
    // The key name will be used if it's a non-empty string consisting of
    // only Unicode letters, digits, and ASCII punctuation except quotation
    // marks, backslash, and comma.
    //
    // Anonymous struct fields are usually marshaled as if their inner exported fields
    // were fields in the outer struct, subject to the usual Go visibility rules amended
    // as described in the next paragraph.
    // An anonymous struct field with a name given in its JSON tag is treated as
    // having that name, rather than being anonymous.
    // An anonymous struct field of interface type is treated the same as having
    // that type as its name, rather than being anonymous.
    //
    // The Go visibility rules for struct fields are amended for JSON when
    // deciding which field to marshal or unmarshal. If there are
    // multiple fields at the same level, and that level is the least
    // nested (and would therefore be the nesting level selected by the
    // usual Go rules), the following extra rules apply:
    //
    // 1) Of those fields, if any are JSON-tagged, only tagged fields are considered,
    // even if there are multiple untagged fields that would otherwise conflict.
    //
    // 2) If there is exactly one field (tagged or not according to the first rule), that is selected.
    //
    // 3) Otherwise there are multiple fields, and all are ignored; no error occurs.
    //
    // Handling of anonymous struct fields is new in Go 1.1.
    // Prior to Go 1.1, anonymous struct fields were ignored. To force ignoring of
    // an anonymous struct field in both current and earlier versions, give the field
    // a JSON tag of "-".
    //
    // Map values encode as JSON objects. The map's key type must either be a
    // string, an integer type, or implement encoding.TextMarshaler. The map keys
    // are sorted and used as JSON object keys by applying the following rules,
    // subject to the UTF-8 coercion described for string values above:
    //   - keys of any string type are used directly
    //   - encoding.TextMarshalers are marshaled
    //   - integer keys are converted to strings
    //
    // Pointer values encode as the value pointed to.
    // A nil pointer encodes as the null JSON value.
    //
    // Interface values encode as the value contained in the interface.
    // A nil interface value encodes as the null JSON value.
    //
    // Channel, complex, and function values cannot be encoded in JSON.
    // Attempting to encode such a value causes Marshal to return
    // an UnsupportedTypeError.
    //
    // JSON cannot represent cyclic data structures and Marshal does not
    // handle them. Passing cyclic structures to Marshal will result in
    // an error.
    //
public static (slice<byte>, error) Marshal(object v) {
    slice<byte> _p0 = default;
    error _p0 = default!;

    var e = newEncodeState();

    var err = e.marshal(v, new encOpts(escapeHTML:true));
    if (err != null) {
        return (null, error.As(err)!);
    }
    var buf = append((slice<byte>)null, e.Bytes());

    encodeStatePool.Put(e);

    return (buf, error.As(null!)!);

}

// MarshalIndent is like Marshal but applies Indent to format the output.
// Each JSON element in the output will begin on a new line beginning with prefix
// followed by one or more copies of indent according to the indentation nesting.
public static (slice<byte>, error) MarshalIndent(object v, @string prefix, @string indent) {
    slice<byte> _p0 = default;
    error _p0 = default!;

    var (b, err) = Marshal(v);
    if (err != null) {
        return (null, error.As(err)!);
    }
    ref bytes.Buffer buf = ref heap(out ptr<bytes.Buffer> _addr_buf);
    err = Indent(_addr_buf, b, prefix, indent);
    if (err != null) {
        return (null, error.As(err)!);
    }
    return (buf.Bytes(), error.As(null!)!);

}

// HTMLEscape appends to dst the JSON-encoded src with <, >, &, U+2028 and U+2029
// characters inside string literals changed to \u003c, \u003e, \u0026, \u2028, \u2029
// so that the JSON will be safe to embed inside HTML <script> tags.
// For historical reasons, web browsers don't honor standard HTML
// escaping within <script> tags, so an alternative JSON encoding must
// be used.
public static void HTMLEscape(ptr<bytes.Buffer> _addr_dst, slice<byte> src) {
    ref bytes.Buffer dst = ref _addr_dst.val;
 
    // The characters can only appear in string literals,
    // so just scan the string one byte at a time.
    nint start = 0;
    foreach (var (i, c) in src) {
        if (c == '<' || c == '>' || c == '&') {
            if (start < i) {
                dst.Write(src[(int)start..(int)i]);
            }
            dst.WriteString("\\u00");
            dst.WriteByte(hex[c >> 4]);
            dst.WriteByte(hex[c & 0xF]);
            start = i + 1;
        }
        if (c == 0xE2 && i + 2 < len(src) && src[i + 1] == 0x80 && src[i + 2] & ~1 == 0xA8) {
            if (start < i) {
                dst.Write(src[(int)start..(int)i]);
            }
            dst.WriteString("\\u202");
            dst.WriteByte(hex[src[i + 2] & 0xF]);
            start = i + 3;
        }
    }    if (start < len(src)) {
        dst.Write(src[(int)start..]);
    }
}

// Marshaler is the interface implemented by types that
// can marshal themselves into valid JSON.
public partial interface Marshaler {
    (slice<byte>, error) MarshalJSON();
}

// An UnsupportedTypeError is returned by Marshal when attempting
// to encode an unsupported value type.
public partial struct UnsupportedTypeError {
    public reflect.Type Type;
}

private static @string Error(this ptr<UnsupportedTypeError> _addr_e) {
    ref UnsupportedTypeError e = ref _addr_e.val;

    return "json: unsupported type: " + e.Type.String();
}

// An UnsupportedValueError is returned by Marshal when attempting
// to encode an unsupported value.
public partial struct UnsupportedValueError {
    public reflect.Value Value;
    public @string Str;
}

private static @string Error(this ptr<UnsupportedValueError> _addr_e) {
    ref UnsupportedValueError e = ref _addr_e.val;

    return "json: unsupported value: " + e.Str;
}

// Before Go 1.2, an InvalidUTF8Error was returned by Marshal when
// attempting to encode a string value with invalid UTF-8 sequences.
// As of Go 1.2, Marshal instead coerces the string to valid UTF-8 by
// replacing invalid bytes with the Unicode replacement rune U+FFFD.
//
// Deprecated: No longer used; kept for compatibility.
public partial struct InvalidUTF8Error {
    public @string S; // the whole string value that caused the error
}

private static @string Error(this ptr<InvalidUTF8Error> _addr_e) {
    ref InvalidUTF8Error e = ref _addr_e.val;

    return "json: invalid UTF-8 in string: " + strconv.Quote(e.S);
}

// A MarshalerError represents an error from calling a MarshalJSON or MarshalText method.
public partial struct MarshalerError {
    public reflect.Type Type;
    public error Err;
    public @string sourceFunc;
}

private static @string Error(this ptr<MarshalerError> _addr_e) {
    ref MarshalerError e = ref _addr_e.val;

    var srcFunc = e.sourceFunc;
    if (srcFunc == "") {
        srcFunc = "MarshalJSON";
    }
    return "json: error calling " + srcFunc + " for type " + e.Type.String() + ": " + e.Err.Error();

}

// Unwrap returns the underlying error.
private static error Unwrap(this ptr<MarshalerError> _addr_e) {
    ref MarshalerError e = ref _addr_e.val;

    return error.As(e.Err)!;
}

private static @string hex = "0123456789abcdef";

// An encodeState encodes JSON into a bytes.Buffer.
private partial struct encodeState {
    public ref bytes.Buffer Buffer => ref Buffer_val; // accumulated output
    public array<byte> scratch; // Keep track of what pointers we've seen in the current recursive call
// path, to avoid cycles that could lead to a stack overflow. Only do
// the relatively expensive map operations if ptrLevel is larger than
// startDetectingCyclesAfter, so that we skip the work if we're within a
// reasonable amount of nested pointers deep.
    public nuint ptrLevel;
}

private static readonly nint startDetectingCyclesAfter = 1000;



private static sync.Pool encodeStatePool = default;

private static ptr<encodeState> newEncodeState() => func((_, panic, _) => {
    {
        var v = encodeStatePool.Get();

        if (v != null) {
            ptr<encodeState> e = v._<ptr<encodeState>>();
            e.Reset();
            if (len(e.ptrSeen) > 0) {
                panic("ptrEncoder.encode should have emptied ptrSeen via defers");
            }
            e.ptrLevel = 0;
            return _addr_e!;
        }
    }

    return addr(new encodeState(ptrSeen:make(map[interface{}]struct{})));

});

// jsonError is an error wrapper type for internal use only.
// Panics with errors are wrapped in jsonError so that the top-level recover
// can distinguish intentional panics from this package.
private partial struct jsonError : error {
    public error error;
}

private static error marshal(this ptr<encodeState> _addr_e, object v, encOpts opts) => func((defer, panic, _) => {
    error err = default!;
    ref encodeState e = ref _addr_e.val;

    defer(() => {
        {
            var r = recover();

            if (r != null) {
                {
                    jsonError (je, ok) = r._<jsonError>();

                    if (ok) {
                        err = je.error;
                    }
                    else
 {
                        panic(r);
                    }

                }

            }

        }

    }());
    e.reflectValue(reflect.ValueOf(v), opts);
    return error.As(null!)!;

});

// error aborts the encoding by panicking with err wrapped in jsonError.
private static void error(this ptr<encodeState> _addr_e, error err) => func((_, panic, _) => {
    ref encodeState e = ref _addr_e.val;

    panic(new jsonError(err));
});

private static bool isEmptyValue(reflect.Value v) {

    if (v.Kind() == reflect.Array || v.Kind() == reflect.Map || v.Kind() == reflect.Slice || v.Kind() == reflect.String) 
        return v.Len() == 0;
    else if (v.Kind() == reflect.Bool) 
        return !v.Bool();
    else if (v.Kind() == reflect.Int || v.Kind() == reflect.Int8 || v.Kind() == reflect.Int16 || v.Kind() == reflect.Int32 || v.Kind() == reflect.Int64) 
        return v.Int() == 0;
    else if (v.Kind() == reflect.Uint || v.Kind() == reflect.Uint8 || v.Kind() == reflect.Uint16 || v.Kind() == reflect.Uint32 || v.Kind() == reflect.Uint64 || v.Kind() == reflect.Uintptr) 
        return v.Uint() == 0;
    else if (v.Kind() == reflect.Float32 || v.Kind() == reflect.Float64) 
        return v.Float() == 0;
    else if (v.Kind() == reflect.Interface || v.Kind() == reflect.Ptr) 
        return v.IsNil();
        return false;

}

private static void reflectValue(this ptr<encodeState> _addr_e, reflect.Value v, encOpts opts) {
    ref encodeState e = ref _addr_e.val;

    valueEncoder(v)(e, v, opts);
}

private partial struct encOpts {
    public bool quoted; // escapeHTML causes '<', '>', and '&' to be escaped in JSON strings.
    public bool escapeHTML;
}

public delegate void encoderFunc(ptr<encodeState>, reflect.Value, encOpts);

private static sync.Map encoderCache = default; // map[reflect.Type]encoderFunc

private static encoderFunc valueEncoder(reflect.Value v) {
    if (!v.IsValid()) {
        return invalidValueEncoder;
    }
    return typeEncoder(v.Type());

}

private static encoderFunc typeEncoder(reflect.Type t) {
    {
        var fi__prev1 = fi;

        var (fi, ok) = encoderCache.Load(t);

        if (ok) {
            return fi._<encoderFunc>();
        }
        fi = fi__prev1;

    } 

    // To deal with recursive types, populate the map with an
    // indirect func before we build it. This type waits on the
    // real func (f) to be ready and then calls it. This indirect
    // func is only used for recursive types.
    sync.WaitGroup wg = default;    encoderFunc f = default;
    wg.Add(1);
    var (fi, loaded) = encoderCache.LoadOrStore(t, encoderFunc((e, v, opts) => {
        wg.Wait();
        f(e, v, opts);
    }));
    if (loaded) {
        return fi._<encoderFunc>();
    }
    f = newTypeEncoder(t, true);
    wg.Done();
    encoderCache.Store(t, f);
    return f;

}

private static var marshalerType = reflect.TypeOf((Marshaler.val)(null)).Elem();private static var textMarshalerType = reflect.TypeOf((encoding.TextMarshaler.val)(null)).Elem();

// newTypeEncoder constructs an encoderFunc for a type.
// The returned encoder only checks CanAddr when allowAddr is true.
private static encoderFunc newTypeEncoder(reflect.Type t, bool allowAddr) { 
    // If we have a non-pointer value whose type implements
    // Marshaler with a value receiver, then we're better off taking
    // the address of the value - otherwise we end up with an
    // allocation as we cast the value to an interface.
    if (t.Kind() != reflect.Ptr && allowAddr && reflect.PtrTo(t).Implements(marshalerType)) {
        return newCondAddrEncoder(addrMarshalerEncoder, newTypeEncoder(t, false));
    }
    if (t.Implements(marshalerType)) {
        return marshalerEncoder;
    }
    if (t.Kind() != reflect.Ptr && allowAddr && reflect.PtrTo(t).Implements(textMarshalerType)) {
        return newCondAddrEncoder(addrTextMarshalerEncoder, newTypeEncoder(t, false));
    }
    if (t.Implements(textMarshalerType)) {
        return textMarshalerEncoder;
    }

    if (t.Kind() == reflect.Bool) 
        return boolEncoder;
    else if (t.Kind() == reflect.Int || t.Kind() == reflect.Int8 || t.Kind() == reflect.Int16 || t.Kind() == reflect.Int32 || t.Kind() == reflect.Int64) 
        return intEncoder;
    else if (t.Kind() == reflect.Uint || t.Kind() == reflect.Uint8 || t.Kind() == reflect.Uint16 || t.Kind() == reflect.Uint32 || t.Kind() == reflect.Uint64 || t.Kind() == reflect.Uintptr) 
        return uintEncoder;
    else if (t.Kind() == reflect.Float32) 
        return float32Encoder;
    else if (t.Kind() == reflect.Float64) 
        return float64Encoder;
    else if (t.Kind() == reflect.String) 
        return stringEncoder;
    else if (t.Kind() == reflect.Interface) 
        return interfaceEncoder;
    else if (t.Kind() == reflect.Struct) 
        return newStructEncoder(t);
    else if (t.Kind() == reflect.Map) 
        return newMapEncoder(t);
    else if (t.Kind() == reflect.Slice) 
        return newSliceEncoder(t);
    else if (t.Kind() == reflect.Array) 
        return newArrayEncoder(t);
    else if (t.Kind() == reflect.Ptr) 
        return newPtrEncoder(t);
    else 
        return unsupportedTypeEncoder;
    
}

private static void invalidValueEncoder(ptr<encodeState> _addr_e, reflect.Value v, encOpts _) {
    ref encodeState e = ref _addr_e.val;

    e.WriteString("null");
}

private static void marshalerEncoder(ptr<encodeState> _addr_e, reflect.Value v, encOpts opts) {
    ref encodeState e = ref _addr_e.val;

    if (v.Kind() == reflect.Ptr && v.IsNil()) {
        e.WriteString("null");
        return ;
    }
    Marshaler (m, ok) = Marshaler.As(v.Interface()._<Marshaler>())!;
    if (!ok) {
        e.WriteString("null");
        return ;
    }
    var (b, err) = m.MarshalJSON();
    if (err == null) { 
        // copy JSON into buffer, checking validity.
        err = compact(_addr_e.Buffer, b, opts.escapeHTML);

    }
    if (err != null) {
        e.error(addr(new MarshalerError(v.Type(),err,"MarshalJSON")));
    }
}

private static void addrMarshalerEncoder(ptr<encodeState> _addr_e, reflect.Value v, encOpts opts) {
    ref encodeState e = ref _addr_e.val;

    var va = v.Addr();
    if (va.IsNil()) {
        e.WriteString("null");
        return ;
    }
    Marshaler m = Marshaler.As(va.Interface()._<Marshaler>())!;
    var (b, err) = m.MarshalJSON();
    if (err == null) { 
        // copy JSON into buffer, checking validity.
        err = compact(_addr_e.Buffer, b, opts.escapeHTML);

    }
    if (err != null) {
        e.error(addr(new MarshalerError(v.Type(),err,"MarshalJSON")));
    }
}

private static void textMarshalerEncoder(ptr<encodeState> _addr_e, reflect.Value v, encOpts opts) {
    ref encodeState e = ref _addr_e.val;

    if (v.Kind() == reflect.Ptr && v.IsNil()) {
        e.WriteString("null");
        return ;
    }
    encoding.TextMarshaler (m, ok) = v.Interface()._<encoding.TextMarshaler>();
    if (!ok) {
        e.WriteString("null");
        return ;
    }
    var (b, err) = m.MarshalText();
    if (err != null) {
        e.error(addr(new MarshalerError(v.Type(),err,"MarshalText")));
    }
    e.stringBytes(b, opts.escapeHTML);

}

private static void addrTextMarshalerEncoder(ptr<encodeState> _addr_e, reflect.Value v, encOpts opts) {
    ref encodeState e = ref _addr_e.val;

    var va = v.Addr();
    if (va.IsNil()) {
        e.WriteString("null");
        return ;
    }
    encoding.TextMarshaler m = va.Interface()._<encoding.TextMarshaler>();
    var (b, err) = m.MarshalText();
    if (err != null) {
        e.error(addr(new MarshalerError(v.Type(),err,"MarshalText")));
    }
    e.stringBytes(b, opts.escapeHTML);

}

private static void boolEncoder(ptr<encodeState> _addr_e, reflect.Value v, encOpts opts) {
    ref encodeState e = ref _addr_e.val;

    if (opts.quoted) {
        e.WriteByte('"');
    }
    if (v.Bool()) {
        e.WriteString("true");
    }
    else
 {
        e.WriteString("false");
    }
    if (opts.quoted) {
        e.WriteByte('"');
    }
}

private static void intEncoder(ptr<encodeState> _addr_e, reflect.Value v, encOpts opts) {
    ref encodeState e = ref _addr_e.val;

    var b = strconv.AppendInt(e.scratch[..(int)0], v.Int(), 10);
    if (opts.quoted) {
        e.WriteByte('"');
    }
    e.Write(b);
    if (opts.quoted) {
        e.WriteByte('"');
    }
}

private static void uintEncoder(ptr<encodeState> _addr_e, reflect.Value v, encOpts opts) {
    ref encodeState e = ref _addr_e.val;

    var b = strconv.AppendUint(e.scratch[..(int)0], v.Uint(), 10);
    if (opts.quoted) {
        e.WriteByte('"');
    }
    e.Write(b);
    if (opts.quoted) {
        e.WriteByte('"');
    }
}

private partial struct floatEncoder { // : nint
} // number of bits

private static void encode(this floatEncoder bits, ptr<encodeState> _addr_e, reflect.Value v, encOpts opts) {
    ref encodeState e = ref _addr_e.val;

    var f = v.Float();
    if (math.IsInf(f, 0) || math.IsNaN(f)) {
        e.error(addr(new UnsupportedValueError(v,strconv.FormatFloat(f,'g',-1,int(bits)))));
    }
    var b = e.scratch[..(int)0];
    var abs = math.Abs(f);
    var fmt = byte('f'); 
    // Note: Must use float32 comparisons for underlying float32 value to get precise cutoffs right.
    if (abs != 0) {
        if (bits == 64 && (abs < 1e-6F || abs >= 1e21F) || bits == 32 && (float32(abs) < 1e-6F || float32(abs) >= 1e21F)) {
            fmt = 'e';
        }
    }
    b = strconv.AppendFloat(b, f, fmt, -1, int(bits));
    if (fmt == 'e') { 
        // clean up e-09 to e-9
        var n = len(b);
        if (n >= 4 && b[n - 4] == 'e' && b[n - 3] == '-' && b[n - 2] == '0') {
            b[n - 2] = b[n - 1];
            b = b[..(int)n - 1];
        }
    }
    if (opts.quoted) {
        e.WriteByte('"');
    }
    e.Write(b);
    if (opts.quoted) {
        e.WriteByte('"');
    }
}

private static var float32Encoder = (floatEncoder(32)).encode;private static var float64Encoder = (floatEncoder(64)).encode;

private static void stringEncoder(ptr<encodeState> _addr_e, reflect.Value v, encOpts opts) {
    ref encodeState e = ref _addr_e.val;

    if (v.Type() == numberType) {
        var numStr = v.String(); 
        // In Go1.5 the empty string encodes to "0", while this is not a valid number literal
        // we keep compatibility so check validity after this.
        if (numStr == "") {
            numStr = "0"; // Number's zero-val
        }
        if (!isValidNumber(numStr)) {
            e.error(fmt.Errorf("json: invalid number literal %q", numStr));
        }
        if (opts.quoted) {
            e.WriteByte('"');
        }
        e.WriteString(numStr);
        if (opts.quoted) {
            e.WriteByte('"');
        }
        return ;

    }
    if (opts.quoted) {
        var e2 = newEncodeState(); 
        // Since we encode the string twice, we only need to escape HTML
        // the first time.
        e2.@string(v.String(), opts.escapeHTML);
        e.stringBytes(e2.Bytes(), false);
        encodeStatePool.Put(e2);

    }
    else
 {
        e.@string(v.String(), opts.escapeHTML);
    }
}

// isValidNumber reports whether s is a valid JSON number literal.
private static bool isValidNumber(@string s) { 
    // This function implements the JSON numbers grammar.
    // See https://tools.ietf.org/html/rfc7159#section-6
    // and https://www.json.org/img/number.png

    if (s == "") {
        return false;
    }
    if (s[0] == '-') {
        s = s[(int)1..];
        if (s == "") {
            return false;
        }
    }

    if (s[0] == '0') 
        s = s[(int)1..];
    else if ('1' <= s[0] && s[0] <= '9') 
        s = s[(int)1..];
        while (len(s) > 0 && '0' <= s[0] && s[0] <= '9') {
            s = s[(int)1..];
        }
    else 
        return false;
    // . followed by 1 or more digits.
    if (len(s) >= 2 && s[0] == '.' && '0' <= s[1] && s[1] <= '9') {
        s = s[(int)2..];
        while (len(s) > 0 && '0' <= s[0] && s[0] <= '9') {
            s = s[(int)1..];
        }
    }
    if (len(s) >= 2 && (s[0] == 'e' || s[0] == 'E')) {
        s = s[(int)1..];
        if (s[0] == '+' || s[0] == '-') {
            s = s[(int)1..];
            if (s == "") {
                return false;
            }
        }
        while (len(s) > 0 && '0' <= s[0] && s[0] <= '9') {
            s = s[(int)1..];
        }

    }
    return s == "";

}

private static void interfaceEncoder(ptr<encodeState> _addr_e, reflect.Value v, encOpts opts) {
    ref encodeState e = ref _addr_e.val;

    if (v.IsNil()) {
        e.WriteString("null");
        return ;
    }
    e.reflectValue(v.Elem(), opts);

}

private static void unsupportedTypeEncoder(ptr<encodeState> _addr_e, reflect.Value v, encOpts _) {
    ref encodeState e = ref _addr_e.val;

    e.error(addr(new UnsupportedTypeError(v.Type())));
}

private partial struct structEncoder {
    public structFields fields;
}

private partial struct structFields {
    public slice<field> list;
    public map<@string, nint> nameIndex;
}

private static void encode(this structEncoder se, ptr<encodeState> _addr_e, reflect.Value v, encOpts opts) {
    ref encodeState e = ref _addr_e.val;

    var next = byte('{');
FieldLoop:
    {
        var i__prev1 = i;

        foreach (var (__i) in se.fields.list) {
            i = __i;
            var f = _addr_se.fields.list[i]; 

            // Find the nested struct field by following f.index.
            var fv = v;
            {
                var i__prev2 = i;

                foreach (var (_, __i) in f.index) {
                    i = __i;
                    if (fv.Kind() == reflect.Ptr) {
                        if (fv.IsNil()) {
                            _continueFieldLoop = true;
                            break;
                        }

                        fv = fv.Elem();

                    }

                    fv = fv.Field(i);

                }

                i = i__prev2;
            }

            if (f.omitEmpty && isEmptyValue(fv)) {
                continue;
            }

            e.WriteByte(next);
            next = ',';
            if (opts.escapeHTML) {
                e.WriteString(f.nameEscHTML);
            }
            else
 {
                e.WriteString(f.nameNonEsc);
            }

            opts.quoted = f.quoted;
            f.encoder(e, fv, opts);

        }
        i = i__prev1;
    }
    if (next == '{') {
        e.WriteString("{}");
    }
    else
 {
        e.WriteByte('}');
    }
}

private static encoderFunc newStructEncoder(reflect.Type t) {
    structEncoder se = new structEncoder(fields:cachedTypeFields(t));
    return se.encode;
}

private partial struct mapEncoder {
    public encoderFunc elemEnc;
}

private static void encode(this mapEncoder me, ptr<encodeState> _addr_e, reflect.Value v, encOpts opts) => func((defer, _, _) => {
    ref encodeState e = ref _addr_e.val;

    if (v.IsNil()) {
        e.WriteString("null");
        return ;
    }
    e.ptrLevel++;

    if (e.ptrLevel > startDetectingCyclesAfter) { 
        // We're a large number of nested ptrEncoder.encode calls deep;
        // start checking if we've run into a pointer cycle.
        var ptr = v.Pointer();
        {
            var (_, ok) = e.ptrSeen[ptr];

            if (ok) {
                e.error(addr(new UnsupportedValueError(v,fmt.Sprintf("encountered a cycle via %s",v.Type()))));
            }

        }

        e.ptrSeen[ptr] = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{};
        defer(delete(e.ptrSeen, ptr));

    }
    e.WriteByte('{'); 

    // Extract and sort the keys.
    var sv = make_slice<reflectWithString>(v.Len());
    var mi = v.MapRange();
    {
        nint i__prev1 = i;

        for (nint i = 0; mi.Next(); i++) {
            sv[i].k = mi.Key();
            sv[i].v = mi.Value();
            {
                var err = sv[i].resolve();

                if (err != null) {
                    e.error(fmt.Errorf("json: encoding error for type %q: %q", v.Type().String(), err.Error()));
                }

            }

        }

        i = i__prev1;
    }
    sort.Slice(sv, (i, j) => sv[i].ks < sv[j].ks);

    {
        nint i__prev1 = i;

        foreach (var (__i, __kv) in sv) {
            i = __i;
            kv = __kv;
            if (i > 0) {
                e.WriteByte(',');
            }
            e.@string(kv.ks, opts.escapeHTML);
            e.WriteByte(':');
            me.elemEnc(e, kv.v, opts);
        }
        i = i__prev1;
    }

    e.WriteByte('}');
    e.ptrLevel--;

});

private static encoderFunc newMapEncoder(reflect.Type t) {

    if (t.Key().Kind() == reflect.String || t.Key().Kind() == reflect.Int || t.Key().Kind() == reflect.Int8 || t.Key().Kind() == reflect.Int16 || t.Key().Kind() == reflect.Int32 || t.Key().Kind() == reflect.Int64 || t.Key().Kind() == reflect.Uint || t.Key().Kind() == reflect.Uint8 || t.Key().Kind() == reflect.Uint16 || t.Key().Kind() == reflect.Uint32 || t.Key().Kind() == reflect.Uint64 || t.Key().Kind() == reflect.Uintptr)     else 
        if (!t.Key().Implements(textMarshalerType)) {
            return unsupportedTypeEncoder;
        }
        mapEncoder me = new mapEncoder(typeEncoder(t.Elem()));
    return me.encode;

}

private static void encodeByteSlice(ptr<encodeState> _addr_e, reflect.Value v, encOpts _) {
    ref encodeState e = ref _addr_e.val;

    if (v.IsNil()) {
        e.WriteString("null");
        return ;
    }
    var s = v.Bytes();
    e.WriteByte('"');
    var encodedLen = base64.StdEncoding.EncodedLen(len(s));
    if (encodedLen <= len(e.scratch)) { 
        // If the encoded bytes fit in e.scratch, avoid an extra
        // allocation and use the cheaper Encoding.Encode.
        var dst = e.scratch[..(int)encodedLen];
        base64.StdEncoding.Encode(dst, s);
        e.Write(dst);

    }
    else if (encodedLen <= 1024) { 
        // The encoded bytes are short enough to allocate for, and
        // Encoding.Encode is still cheaper.
        dst = make_slice<byte>(encodedLen);
        base64.StdEncoding.Encode(dst, s);
        e.Write(dst);

    }
    else
 { 
        // The encoded bytes are too long to cheaply allocate, and
        // Encoding.Encode is no longer noticeably cheaper.
        var enc = base64.NewEncoder(base64.StdEncoding, e);
        enc.Write(s);
        enc.Close();

    }
    e.WriteByte('"');

}

// sliceEncoder just wraps an arrayEncoder, checking to make sure the value isn't nil.
private partial struct sliceEncoder {
    public encoderFunc arrayEnc;
}

private static void encode(this sliceEncoder se, ptr<encodeState> _addr_e, reflect.Value v, encOpts opts) => func((defer, _, _) => {
    ref encodeState e = ref _addr_e.val;

    if (v.IsNil()) {
        e.WriteString("null");
        return ;
    }
    e.ptrLevel++;

    if (e.ptrLevel > startDetectingCyclesAfter) { 
        // We're a large number of nested ptrEncoder.encode calls deep;
        // start checking if we've run into a pointer cycle.
        // Here we use a struct to memorize the pointer to the first element of the slice
        // and its length.
        struct{ptruintptrlenint} ptr = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{ptruintptrlenint}{v.Pointer(),v.Len()};
        {
            var (_, ok) = e.ptrSeen[ptr];

            if (ok) {
                e.error(addr(new UnsupportedValueError(v,fmt.Sprintf("encountered a cycle via %s",v.Type()))));
            }

        }

        e.ptrSeen[ptr] = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{};
        defer(delete(e.ptrSeen, ptr));

    }
    se.arrayEnc(e, v, opts);
    e.ptrLevel--;

});

private static encoderFunc newSliceEncoder(reflect.Type t) { 
    // Byte slices get special treatment; arrays don't.
    if (t.Elem().Kind() == reflect.Uint8) {
        var p = reflect.PtrTo(t.Elem());
        if (!p.Implements(marshalerType) && !p.Implements(textMarshalerType)) {
            return encodeByteSlice;
        }
    }
    sliceEncoder enc = new sliceEncoder(newArrayEncoder(t));
    return enc.encode;

}

private partial struct arrayEncoder {
    public encoderFunc elemEnc;
}

private static void encode(this arrayEncoder ae, ptr<encodeState> _addr_e, reflect.Value v, encOpts opts) {
    ref encodeState e = ref _addr_e.val;

    e.WriteByte('[');
    var n = v.Len();
    for (nint i = 0; i < n; i++) {
        if (i > 0) {
            e.WriteByte(',');
        }
        ae.elemEnc(e, v.Index(i), opts);

    }
    e.WriteByte(']');

}

private static encoderFunc newArrayEncoder(reflect.Type t) {
    arrayEncoder enc = new arrayEncoder(typeEncoder(t.Elem()));
    return enc.encode;
}

private partial struct ptrEncoder {
    public encoderFunc elemEnc;
}

private static void encode(this ptrEncoder pe, ptr<encodeState> _addr_e, reflect.Value v, encOpts opts) => func((defer, _, _) => {
    ref encodeState e = ref _addr_e.val;

    if (v.IsNil()) {
        e.WriteString("null");
        return ;
    }
    e.ptrLevel++;

    if (e.ptrLevel > startDetectingCyclesAfter) { 
        // We're a large number of nested ptrEncoder.encode calls deep;
        // start checking if we've run into a pointer cycle.
        var ptr = v.Interface();
        {
            var (_, ok) = e.ptrSeen[ptr];

            if (ok) {
                e.error(addr(new UnsupportedValueError(v,fmt.Sprintf("encountered a cycle via %s",v.Type()))));
            }

        }

        e.ptrSeen[ptr] = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{};
        defer(delete(e.ptrSeen, ptr));

    }
    pe.elemEnc(e, v.Elem(), opts);
    e.ptrLevel--;

});

private static encoderFunc newPtrEncoder(reflect.Type t) {
    ptrEncoder enc = new ptrEncoder(typeEncoder(t.Elem()));
    return enc.encode;
}

private partial struct condAddrEncoder {
    public encoderFunc canAddrEnc;
    public encoderFunc elseEnc;
}

private static void encode(this condAddrEncoder ce, ptr<encodeState> _addr_e, reflect.Value v, encOpts opts) {
    ref encodeState e = ref _addr_e.val;

    if (v.CanAddr()) {
        ce.canAddrEnc(e, v, opts);
    }
    else
 {
        ce.elseEnc(e, v, opts);
    }
}

// newCondAddrEncoder returns an encoder that checks whether its value
// CanAddr and delegates to canAddrEnc if so, else to elseEnc.
private static encoderFunc newCondAddrEncoder(encoderFunc canAddrEnc, encoderFunc elseEnc) {
    condAddrEncoder enc = new condAddrEncoder(canAddrEnc:canAddrEnc,elseEnc:elseEnc);
    return enc.encode;
}

private static bool isValidTag(@string s) {
    if (s == "") {
        return false;
    }
    foreach (var (_, c) in s) {

        if (strings.ContainsRune("!#$%&()*+-./:;<=>?@[]^_{|}~ ", c))         else if (!unicode.IsLetter(c) && !unicode.IsDigit(c)) 
            return false;
        
    }    return true;

}

private static reflect.Type typeByIndex(reflect.Type t, slice<nint> index) {
    foreach (var (_, i) in index) {
        if (t.Kind() == reflect.Ptr) {
            t = t.Elem();
        }
        t = t.Field(i).Type;

    }    return t;

}

private partial struct reflectWithString {
    public reflect.Value k;
    public reflect.Value v;
    public @string ks;
}

private static error resolve(this ptr<reflectWithString> _addr_w) => func((_, panic, _) => {
    ref reflectWithString w = ref _addr_w.val;

    if (w.k.Kind() == reflect.String) {
        w.ks = w.k.String();
        return error.As(null!)!;
    }
    {
        encoding.TextMarshaler (tm, ok) = w.k.Interface()._<encoding.TextMarshaler>();

        if (ok) {
            if (w.k.Kind() == reflect.Ptr && w.k.IsNil()) {
                return error.As(null!)!;
            }
            var (buf, err) = tm.MarshalText();
            w.ks = string(buf);
            return error.As(err)!;
        }
    }


    if (w.k.Kind() == reflect.Int || w.k.Kind() == reflect.Int8 || w.k.Kind() == reflect.Int16 || w.k.Kind() == reflect.Int32 || w.k.Kind() == reflect.Int64) 
        w.ks = strconv.FormatInt(w.k.Int(), 10);
        return error.As(null!)!;
    else if (w.k.Kind() == reflect.Uint || w.k.Kind() == reflect.Uint8 || w.k.Kind() == reflect.Uint16 || w.k.Kind() == reflect.Uint32 || w.k.Kind() == reflect.Uint64 || w.k.Kind() == reflect.Uintptr) 
        w.ks = strconv.FormatUint(w.k.Uint(), 10);
        return error.As(null!)!;
        panic("unexpected map key type");

});

// NOTE: keep in sync with stringBytes below.
private static void @string(this ptr<encodeState> _addr_e, @string s, bool escapeHTML) {
    ref encodeState e = ref _addr_e.val;

    e.WriteByte('"');
    nint start = 0;
    {
        nint i = 0;

        while (i < len(s)) {
            {
                var b = s[i];

                if (b < utf8.RuneSelf) {
                    if (htmlSafeSet[b] || (!escapeHTML && safeSet[b])) {
                        i++;
                        continue;
                    }
                    if (start < i) {
                        e.WriteString(s[(int)start..(int)i]);
                    }
                    e.WriteByte('\\');
                    switch (b) {
                        case '\\': 

                        case '"': 
                            e.WriteByte(b);
                            break;
                        case '\n': 
                            e.WriteByte('n');
                            break;
                        case '\r': 
                            e.WriteByte('r');
                            break;
                        case '\t': 
                            e.WriteByte('t');
                            break;
                        default: 
                            // This encodes bytes < 0x20 except for \t, \n and \r.
                            // If escapeHTML is set, it also escapes <, >, and &
                            // because they can lead to security holes when
                            // user-controlled strings are rendered into JSON
                            // and served to some browsers.
                            e.WriteString("u00");
                            e.WriteByte(hex[b >> 4]);
                            e.WriteByte(hex[b & 0xF]);
                            break;
                    }
                    i++;
                    start = i;
                    continue;

                }

            }

            var (c, size) = utf8.DecodeRuneInString(s[(int)i..]);
            if (c == utf8.RuneError && size == 1) {
                if (start < i) {
                    e.WriteString(s[(int)start..(int)i]);
                }
                e.WriteString("\\ufffd");
                i += size;
                start = i;
                continue;
            } 
            // U+2028 is LINE SEPARATOR.
            // U+2029 is PARAGRAPH SEPARATOR.
            // They are both technically valid characters in JSON strings,
            // but don't work in JSONP, which has to be evaluated as JavaScript,
            // and can lead to security holes there. It is valid JSON to
            // escape them, so we do so unconditionally.
            // See http://timelessrepo.com/json-isnt-a-javascript-subset for discussion.
            if (c == '\u2028' || c == '\u2029') {
                if (start < i) {
                    e.WriteString(s[(int)start..(int)i]);
                }
                e.WriteString("\\u202");
                e.WriteByte(hex[c & 0xF]);
                i += size;
                start = i;
                continue;
            }

            i += size;

        }
    }
    if (start < len(s)) {
        e.WriteString(s[(int)start..]);
    }
    e.WriteByte('"');

}

// NOTE: keep in sync with string above.
private static void stringBytes(this ptr<encodeState> _addr_e, slice<byte> s, bool escapeHTML) {
    ref encodeState e = ref _addr_e.val;

    e.WriteByte('"');
    nint start = 0;
    {
        nint i = 0;

        while (i < len(s)) {
            {
                var b = s[i];

                if (b < utf8.RuneSelf) {
                    if (htmlSafeSet[b] || (!escapeHTML && safeSet[b])) {
                        i++;
                        continue;
                    }
                    if (start < i) {
                        e.Write(s[(int)start..(int)i]);
                    }
                    e.WriteByte('\\');
                    switch (b) {
                        case '\\': 

                        case '"': 
                            e.WriteByte(b);
                            break;
                        case '\n': 
                            e.WriteByte('n');
                            break;
                        case '\r': 
                            e.WriteByte('r');
                            break;
                        case '\t': 
                            e.WriteByte('t');
                            break;
                        default: 
                            // This encodes bytes < 0x20 except for \t, \n and \r.
                            // If escapeHTML is set, it also escapes <, >, and &
                            // because they can lead to security holes when
                            // user-controlled strings are rendered into JSON
                            // and served to some browsers.
                            e.WriteString("u00");
                            e.WriteByte(hex[b >> 4]);
                            e.WriteByte(hex[b & 0xF]);
                            break;
                    }
                    i++;
                    start = i;
                    continue;

                }

            }

            var (c, size) = utf8.DecodeRune(s[(int)i..]);
            if (c == utf8.RuneError && size == 1) {
                if (start < i) {
                    e.Write(s[(int)start..(int)i]);
                }
                e.WriteString("\\ufffd");
                i += size;
                start = i;
                continue;
            } 
            // U+2028 is LINE SEPARATOR.
            // U+2029 is PARAGRAPH SEPARATOR.
            // They are both technically valid characters in JSON strings,
            // but don't work in JSONP, which has to be evaluated as JavaScript,
            // and can lead to security holes there. It is valid JSON to
            // escape them, so we do so unconditionally.
            // See http://timelessrepo.com/json-isnt-a-javascript-subset for discussion.
            if (c == '\u2028' || c == '\u2029') {
                if (start < i) {
                    e.Write(s[(int)start..(int)i]);
                }
                e.WriteString("\\u202");
                e.WriteByte(hex[c & 0xF]);
                i += size;
                start = i;
                continue;
            }

            i += size;

        }
    }
    if (start < len(s)) {
        e.Write(s[(int)start..]);
    }
    e.WriteByte('"');

}

// A field represents a single field found in a struct.
private partial struct field {
    public @string name;
    public slice<byte> nameBytes; // []byte(name)
    public Func<slice<byte>, slice<byte>, bool> equalFold; // bytes.EqualFold or equivalent

    public @string nameNonEsc; // `"` + name + `":`
    public @string nameEscHTML; // `"` + HTMLEscape(name) + `":`

    public bool tag;
    public slice<nint> index;
    public reflect.Type typ;
    public bool omitEmpty;
    public bool quoted;
    public encoderFunc encoder;
}

// byIndex sorts field by index sequence.
private partial struct byIndex { // : slice<field>
}

private static nint Len(this byIndex x) {
    return len(x);
}

private static void Swap(this byIndex x, nint i, nint j) {
    (x[i], x[j]) = (x[j], x[i]);
}

private static bool Less(this byIndex x, nint i, nint j) {
    foreach (var (k, xik) in x[i].index) {
        if (k >= len(x[j].index)) {
            return false;
        }
        if (xik != x[j].index[k]) {
            return xik < x[j].index[k];
        }
    }    return len(x[i].index) < len(x[j].index);

}

// typeFields returns a list of fields that JSON should recognize for the given type.
// The algorithm is breadth-first search over the set of structs to include - the top struct
// and then any reachable anonymous structs.
private static structFields typeFields(reflect.Type t) { 
    // Anonymous fields to explore at the current level and the next.
    field current = new slice<field>(new field[] {  });
    field next = new slice<field>(new field[] { {typ:t} }); 

    // Count of queued names for current level and the next.
    map<reflect.Type, nint> count = default;    map<reflect.Type, nint> nextCount = default; 

    // Types already visited at an earlier level.
 

    // Types already visited at an earlier level.
    map visited = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<reflect.Type, bool>{}; 

    // Fields found.
    slice<field> fields = default; 

    // Buffer to run HTMLEscape on field names.
    ref bytes.Buffer nameEscBuf = ref heap(out ptr<bytes.Buffer> _addr_nameEscBuf);

    while (len(next) > 0) {
        (current, next) = (next, current[..(int)0]);        (count, nextCount) = (nextCount, /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<reflect.Type, nint>{});        {
            var f__prev2 = f;

            foreach (var (_, __f) in current) {
                f = __f;
                if (visited[f.typ]) {
                    continue;
                }
                visited[f.typ] = true; 

                // Scan f.typ for fields to include.
                {
                    nint i__prev3 = i;

                    for (nint i = 0; i < f.typ.NumField(); i++) {
                        var sf = f.typ.Field(i);
                        if (sf.Anonymous) {
                            var t = sf.Type;
                            if (t.Kind() == reflect.Ptr) {
                                t = t.Elem();
                            }
                            if (!sf.IsExported() && t.Kind() != reflect.Struct) { 
                                // Ignore embedded fields of unexported non-struct types.
                                continue;

                            } 
                            // Do not ignore embedded fields of unexported struct types
                            // since they may have exported fields.
                        }
                        else if (!sf.IsExported()) { 
                            // Ignore unexported non-embedded fields.
                            continue;

                        }

                        var tag = sf.Tag.Get("json");
                        if (tag == "-") {
                            continue;
                        }

                        var (name, opts) = parseTag(tag);
                        if (!isValidTag(name)) {
                            name = "";
                        }

                        var index = make_slice<nint>(len(f.index) + 1);
                        copy(index, f.index);
                        index[len(f.index)] = i;

                        var ft = sf.Type;
                        if (ft.Name() == "" && ft.Kind() == reflect.Ptr) { 
                            // Follow pointer.
                            ft = ft.Elem();

                        } 

                        // Only strings, floats, integers, and booleans can be quoted.
                        var quoted = false;
                        if (opts.Contains("string")) {

                            if (ft.Kind() == reflect.Bool || ft.Kind() == reflect.Int || ft.Kind() == reflect.Int8 || ft.Kind() == reflect.Int16 || ft.Kind() == reflect.Int32 || ft.Kind() == reflect.Int64 || ft.Kind() == reflect.Uint || ft.Kind() == reflect.Uint8 || ft.Kind() == reflect.Uint16 || ft.Kind() == reflect.Uint32 || ft.Kind() == reflect.Uint64 || ft.Kind() == reflect.Uintptr || ft.Kind() == reflect.Float32 || ft.Kind() == reflect.Float64 || ft.Kind() == reflect.String) 
                                quoted = true;
                            
                        } 

                        // Record found field and index sequence.
                        if (name != "" || !sf.Anonymous || ft.Kind() != reflect.Struct) {
                            var tagged = name != "";
                            if (name == "") {
                                name = sf.Name;
                            }
                            field field = new field(name:name,tag:tagged,index:index,typ:ft,omitEmpty:opts.Contains("omitempty"),quoted:quoted,);
                            field.nameBytes = (slice<byte>)field.name;
                            field.equalFold = foldFunc(field.nameBytes); 

                            // Build nameEscHTML and nameNonEsc ahead of time.
                            nameEscBuf.Reset();
                            nameEscBuf.WriteString("\"");
                            HTMLEscape(_addr_nameEscBuf, field.nameBytes);
                            nameEscBuf.WriteString("\":");
                            field.nameEscHTML = nameEscBuf.String();
                            field.nameNonEsc = "\"" + field.name + "\":";

                            fields = append(fields, field);
                            if (count[f.typ] > 1) { 
                                // If there were multiple instances, add a second,
                                // so that the annihilation code will see a duplicate.
                                // It only cares about the distinction between 1 or 2,
                                // so don't bother generating any more copies.
                                fields = append(fields, fields[len(fields) - 1]);

                            }

                            continue;

                        } 

                        // Record new anonymous struct to explore in next round.
                        nextCount[ft]++;
                        if (nextCount[ft] == 1) {
                            next = append(next, new field(name:ft.Name(),index:index,typ:ft));
                        }

                    }


                    i = i__prev3;
                }

            }

            f = f__prev2;
        }
    }

    sort.Slice(fields, (i, j) => {
        var x = fields; 
        // sort field by name, breaking ties with depth, then
        // breaking ties with "name came from json tag", then
        // breaking ties with index sequence.
        if (x[i].name != x[j].name) {
            return x[i].name < x[j].name;
        }
        if (len(x[i].index) != len(x[j].index)) {
            return len(x[i].index) < len(x[j].index);
        }
        if (x[i].tag != x[j].tag) {
            return x[i].tag;
        }
        return byIndex(x).Less(i, j);

    }); 

    // Delete all fields that are hidden by the Go rules for embedded fields,
    // except that fields with JSON tags are promoted.

    // The fields are sorted in primary order of name, secondary order
    // of field index length. Loop over names; for each name, delete
    // hidden fields by choosing the one dominant field that survives.
    var @out = fields[..(int)0];
    {
        nint i__prev1 = i;

        nint advance = 0;
        i = 0;

        while (i < len(fields)) { 
            // One iteration per name.
            // Find the sequence of fields with the name of this first field.
            var fi = fields[i];
            var name = fi.name;
            for (advance = 1; i + advance < len(fields); advance++) {
                var fj = fields[i + advance];
                if (fj.name != name) {
                    break;
                }
            i += advance;
            }

            if (advance == 1) { // Only one field with this name
                out = append(out, fi);
                continue;

            }

            var (dominant, ok) = dominantField(fields[(int)i..(int)i + advance]);
            if (ok) {
                out = append(out, dominant);
            }

        }

        i = i__prev1;
    }

    fields = out;
    sort.Sort(byIndex(fields));

    {
        nint i__prev1 = i;

        foreach (var (__i) in fields) {
            i = __i;
            var f = _addr_fields[i];
            f.encoder = typeEncoder(typeByIndex(t, f.index));
        }
        i = i__prev1;
    }

    var nameIndex = make_map<@string, nint>(len(fields));
    {
        nint i__prev1 = i;
        field field__prev1 = field;

        foreach (var (__i, __field) in fields) {
            i = __i;
            field = __field;
            nameIndex[field.name] = i;
        }
        i = i__prev1;
        field = field__prev1;
    }

    return new structFields(fields,nameIndex);

}

// dominantField looks through the fields, all of which are known to
// have the same name, to find the single field that dominates the
// others using Go's embedding rules, modified by the presence of
// JSON tags. If there are multiple top-level fields, the boolean
// will be false: This condition is an error in Go and we skip all
// the fields.
private static (field, bool) dominantField(slice<field> fields) {
    field _p0 = default;
    bool _p0 = default;
 
    // The fields are sorted in increasing index-length order, then by presence of tag.
    // That means that the first field is the dominant one. We need only check
    // for error cases: two fields at top level, either both tagged or neither tagged.
    if (len(fields) > 1 && len(fields[0].index) == len(fields[1].index) && fields[0].tag == fields[1].tag) {
        return (new field(), false);
    }
    return (fields[0], true);

}

private static sync.Map fieldCache = default; // map[reflect.Type]structFields

// cachedTypeFields is like typeFields but uses a cache to avoid repeated work.
private static structFields cachedTypeFields(reflect.Type t) {
    {
        var f__prev1 = f;

        var (f, ok) = fieldCache.Load(t);

        if (ok) {
            return f._<structFields>();
        }
        f = f__prev1;

    }

    var (f, _) = fieldCache.LoadOrStore(t, typeFields(t));
    return f._<structFields>();

}

} // end json_package
