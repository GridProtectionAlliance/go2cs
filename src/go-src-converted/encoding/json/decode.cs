// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Represents JSON data structure using native Go types: booleans, floats,
// strings, arrays, and maps.

// package json -- go2cs converted at 2022 March 13 05:39:22 UTC
// import "encoding/json" ==> using json = go.encoding.json_package
// Original source: C:\Program Files\Go\src\encoding\json\decode.go
namespace go.encoding;

using encoding = encoding_package;
using base64 = encoding.base64_package;
using fmt = fmt_package;
using reflect = reflect_package;
using strconv = strconv_package;
using strings = strings_package;
using unicode = unicode_package;
using utf16 = unicode.utf16_package;
using utf8 = unicode.utf8_package;


// Unmarshal parses the JSON-encoded data and stores the result
// in the value pointed to by v. If v is nil or not a pointer,
// Unmarshal returns an InvalidUnmarshalError.
//
// Unmarshal uses the inverse of the encodings that
// Marshal uses, allocating maps, slices, and pointers as necessary,
// with the following additional rules:
//
// To unmarshal JSON into a pointer, Unmarshal first handles the case of
// the JSON being the JSON literal null. In that case, Unmarshal sets
// the pointer to nil. Otherwise, Unmarshal unmarshals the JSON into
// the value pointed at by the pointer. If the pointer is nil, Unmarshal
// allocates a new value for it to point to.
//
// To unmarshal JSON into a value implementing the Unmarshaler interface,
// Unmarshal calls that value's UnmarshalJSON method, including
// when the input is a JSON null.
// Otherwise, if the value implements encoding.TextUnmarshaler
// and the input is a JSON quoted string, Unmarshal calls that value's
// UnmarshalText method with the unquoted form of the string.
//
// To unmarshal JSON into a struct, Unmarshal matches incoming object
// keys to the keys used by Marshal (either the struct field name or its tag),
// preferring an exact match but also accepting a case-insensitive match. By
// default, object keys which don't have a corresponding struct field are
// ignored (see Decoder.DisallowUnknownFields for an alternative).
//
// To unmarshal JSON into an interface value,
// Unmarshal stores one of these in the interface value:
//
//    bool, for JSON booleans
//    float64, for JSON numbers
//    string, for JSON strings
//    []interface{}, for JSON arrays
//    map[string]interface{}, for JSON objects
//    nil for JSON null
//
// To unmarshal a JSON array into a slice, Unmarshal resets the slice length
// to zero and then appends each element to the slice.
// As a special case, to unmarshal an empty JSON array into a slice,
// Unmarshal replaces the slice with a new empty slice.
//
// To unmarshal a JSON array into a Go array, Unmarshal decodes
// JSON array elements into corresponding Go array elements.
// If the Go array is smaller than the JSON array,
// the additional JSON array elements are discarded.
// If the JSON array is smaller than the Go array,
// the additional Go array elements are set to zero values.
//
// To unmarshal a JSON object into a map, Unmarshal first establishes a map to
// use. If the map is nil, Unmarshal allocates a new map. Otherwise Unmarshal
// reuses the existing map, keeping existing entries. Unmarshal then stores
// key-value pairs from the JSON object into the map. The map's key type must
// either be any string type, an integer, implement json.Unmarshaler, or
// implement encoding.TextUnmarshaler.
//
// If a JSON value is not appropriate for a given target type,
// or if a JSON number overflows the target type, Unmarshal
// skips that field and completes the unmarshaling as best it can.
// If no more serious errors are encountered, Unmarshal returns
// an UnmarshalTypeError describing the earliest such error. In any
// case, it's not guaranteed that all the remaining fields following
// the problematic one will be unmarshaled into the target object.
//
// The JSON null value unmarshals into an interface, map, pointer, or slice
// by setting that Go value to nil. Because null is often used in JSON to mean
// ``not present,'' unmarshaling a JSON null into any other Go type has no effect
// on the value and produces no error.
//
// When unmarshaling quoted strings, invalid UTF-8 or
// invalid UTF-16 surrogate pairs are not treated as an error.
// Instead, they are replaced by the Unicode replacement
// character U+FFFD.
//

public static partial class json_package {

public static error Unmarshal(slice<byte> data, object v) { 
    // Check for well-formedness.
    // Avoids filling out half a data structure
    // before discovering a JSON syntax error.
    decodeState d = default;
    var err = checkValid(data, _addr_d.scan);
    if (err != null) {
        return error.As(err)!;
    }
    d.init(data);
    return error.As(d.unmarshal(v))!;
}

// Unmarshaler is the interface implemented by types
// that can unmarshal a JSON description of themselves.
// The input can be assumed to be a valid encoding of
// a JSON value. UnmarshalJSON must copy the JSON data
// if it wishes to retain the data after returning.
//
// By convention, to approximate the behavior of Unmarshal itself,
// Unmarshalers implement UnmarshalJSON([]byte("null")) as a no-op.
public partial interface Unmarshaler {
    error UnmarshalJSON(slice<byte> _p0);
}

// An UnmarshalTypeError describes a JSON value that was
// not appropriate for a value of a specific Go type.
public partial struct UnmarshalTypeError {
    public @string Value; // description of JSON value - "bool", "array", "number -5"
    public reflect.Type Type; // type of Go value it could not be assigned to
    public long Offset; // error occurred after reading Offset bytes
    public @string Struct; // name of the struct type containing the field
    public @string Field; // the full path from root node to the field
}

private static @string Error(this ptr<UnmarshalTypeError> _addr_e) {
    ref UnmarshalTypeError e = ref _addr_e.val;

    if (e.Struct != "" || e.Field != "") {
        return "json: cannot unmarshal " + e.Value + " into Go struct field " + e.Struct + "." + e.Field + " of type " + e.Type.String();
    }
    return "json: cannot unmarshal " + e.Value + " into Go value of type " + e.Type.String();
}

// An UnmarshalFieldError describes a JSON object key that
// led to an unexported (and therefore unwritable) struct field.
//
// Deprecated: No longer used; kept for compatibility.
public partial struct UnmarshalFieldError {
    public @string Key;
    public reflect.Type Type;
    public reflect.StructField Field;
}

private static @string Error(this ptr<UnmarshalFieldError> _addr_e) {
    ref UnmarshalFieldError e = ref _addr_e.val;

    return "json: cannot unmarshal object key " + strconv.Quote(e.Key) + " into unexported field " + e.Field.Name + " of type " + e.Type.String();
}

// An InvalidUnmarshalError describes an invalid argument passed to Unmarshal.
// (The argument to Unmarshal must be a non-nil pointer.)
public partial struct InvalidUnmarshalError {
    public reflect.Type Type;
}

private static @string Error(this ptr<InvalidUnmarshalError> _addr_e) {
    ref InvalidUnmarshalError e = ref _addr_e.val;

    if (e.Type == null) {
        return "json: Unmarshal(nil)";
    }
    if (e.Type.Kind() != reflect.Ptr) {
        return "json: Unmarshal(non-pointer " + e.Type.String() + ")";
    }
    return "json: Unmarshal(nil " + e.Type.String() + ")";
}

private static error unmarshal(this ptr<decodeState> _addr_d, object v) {
    ref decodeState d = ref _addr_d.val;

    var rv = reflect.ValueOf(v);
    if (rv.Kind() != reflect.Ptr || rv.IsNil()) {
        return error.As(addr(new InvalidUnmarshalError(reflect.TypeOf(v)))!)!;
    }
    d.scan.reset();
    d.scanWhile(scanSkipSpace); 
    // We decode rv not rv.Elem because the Unmarshaler interface
    // test must be applied at the top level of the value.
    var err = d.value(rv);
    if (err != null) {
        return error.As(d.addErrorContext(err))!;
    }
    return error.As(d.savedError)!;
}

// A Number represents a JSON number literal.
public partial struct Number { // : @string
}

// String returns the literal text of the number.
public static @string String(this Number n) {
    return string(n);
}

// Float64 returns the number as a float64.
public static (double, error) Float64(this Number n) {
    double _p0 = default;
    error _p0 = default!;

    return strconv.ParseFloat(string(n), 64);
}

// Int64 returns the number as an int64.
public static (long, error) Int64(this Number n) {
    long _p0 = default;
    error _p0 = default!;

    return strconv.ParseInt(string(n), 10, 64);
}

// An errorContext provides context for type errors during decoding.
private partial struct errorContext {
    public reflect.Type Struct;
    public slice<@string> FieldStack;
}

// decodeState represents the state while decoding a JSON value.
private partial struct decodeState {
    public slice<byte> data;
    public nint off; // next read offset in data
    public nint opcode; // last read result
    public scanner scan;
    public ptr<errorContext> errorContext;
    public error savedError;
    public bool useNumber;
    public bool disallowUnknownFields;
}

// readIndex returns the position of the last byte read.
private static nint readIndex(this ptr<decodeState> _addr_d) {
    ref decodeState d = ref _addr_d.val;

    return d.off - 1;
}

// phasePanicMsg is used as a panic message when we end up with something that
// shouldn't happen. It can indicate a bug in the JSON decoder, or that
// something is editing the data slice while the decoder executes.
private static readonly @string phasePanicMsg = "JSON decoder out of sync - data changing underfoot?";



private static ptr<decodeState> init(this ptr<decodeState> _addr_d, slice<byte> data) {
    ref decodeState d = ref _addr_d.val;

    d.data = data;
    d.off = 0;
    d.savedError = null;
    if (d.errorContext != null) {
        d.errorContext.Struct = null; 
        // Reuse the allocated space for the FieldStack slice.
        d.errorContext.FieldStack = d.errorContext.FieldStack[..(int)0];
    }
    return _addr_d!;
}

// saveError saves the first err it is called with,
// for reporting at the end of the unmarshal.
private static void saveError(this ptr<decodeState> _addr_d, error err) {
    ref decodeState d = ref _addr_d.val;

    if (d.savedError == null) {
        d.savedError = d.addErrorContext(err);
    }
}

// addErrorContext returns a new error enhanced with information from d.errorContext
private static error addErrorContext(this ptr<decodeState> _addr_d, error err) {
    ref decodeState d = ref _addr_d.val;

    if (d.errorContext != null && (d.errorContext.Struct != null || len(d.errorContext.FieldStack) > 0)) {
        switch (err.type()) {
            case ptr<UnmarshalTypeError> err:
                err.Struct = d.errorContext.Struct.Name();
                err.Field = strings.Join(d.errorContext.FieldStack, ".");
                break;
        }
    }
    return error.As(err)!;
}

// skip scans to the end of what was started.
private static void skip(this ptr<decodeState> _addr_d) {
    ref decodeState d = ref _addr_d.val;

    var s = _addr_d.scan;
    var data = d.data;
    var i = d.off;
    var depth = len(s.parseState);
    while (true) {
        var op = s.step(s, data[i]);
        i++;
        if (len(s.parseState) < depth) {
            d.off = i;
            d.opcode = op;
            return ;
        }
    }
}

// scanNext processes the byte at d.data[d.off].
private static void scanNext(this ptr<decodeState> _addr_d) {
    ref decodeState d = ref _addr_d.val;

    if (d.off < len(d.data)) {
        d.opcode = d.scan.step(_addr_d.scan, d.data[d.off]);
        d.off++;
    }
    else
 {
        d.opcode = d.scan.eof();
        d.off = len(d.data) + 1; // mark processed EOF with len+1
    }
}

// scanWhile processes bytes in d.data[d.off:] until it
// receives a scan code not equal to op.
private static void scanWhile(this ptr<decodeState> _addr_d, nint op) {
    ref decodeState d = ref _addr_d.val;

    var s = _addr_d.scan;
    var data = d.data;
    var i = d.off;
    while (i < len(data)) {
        var newOp = s.step(s, data[i]);
        i++;
        if (newOp != op) {
            d.opcode = newOp;
            d.off = i;
            return ;
        }
    }

    d.off = len(data) + 1; // mark processed EOF with len+1
    d.opcode = d.scan.eof();
}

// rescanLiteral is similar to scanWhile(scanContinue), but it specialises the
// common case where we're decoding a literal. The decoder scans the input
// twice, once for syntax errors and to check the length of the value, and the
// second to perform the decoding.
//
// Only in the second step do we use decodeState to tokenize literals, so we
// know there aren't any syntax errors. We can take advantage of that knowledge,
// and scan a literal's bytes much more quickly.
private static void rescanLiteral(this ptr<decodeState> _addr_d) {
    ref decodeState d = ref _addr_d.val;

    var data = d.data;
    var i = d.off;
Switch:
    switch (data[i - 1]) {
        case '"': // string
            while (i < len(data)) {
                switch (data[i]) {
                    case '\\': 
                        i++; // escaped char
                        break;
                    case '"': 
                        i++; // tokenize the closing quote too
                        _breakSwitch = true;
                        break;
                        break;
                }
                i++;
            }
            break;
        case '0': // number

        case '1': // number

        case '2': // number

        case '3': // number

        case '4': // number

        case '5': // number

        case '6': // number

        case '7': // number

        case '8': // number

        case '9': // number

        case '-': // number
            while (i < len(data)) {
                switch (data[i]) {
                    case '0': 

                    case '1': 

                    case '2': 

                    case '3': 

                    case '4': 

                    case '5': 

                    case '6': 

                    case '7': 

                    case '8': 

                    case '9': 

                    case '.': 

                    case 'e': 

                    case 'E': 

                    case '+': 

                    case '-': 

                        break;
                    default: 
                        _breakSwitch = true;
                        break;
                        break;
                }
                i++;
            }
            break;
        case 't': // true
            i += len("rue");
            break;
        case 'f': // false
            i += len("alse");
            break;
        case 'n': // null
            i += len("ull");
            break;
    }
    if (i < len(data)) {
        d.opcode = stateEndValue(_addr_d.scan, data[i]);
    }
    else
 {
        d.opcode = scanEnd;
    }
    d.off = i + 1;
}

// value consumes a JSON value from d.data[d.off-1:], decoding into v, and
// reads the following byte ahead. If v is invalid, the value is discarded.
// The first byte of the value has been read already.
private static error value(this ptr<decodeState> _addr_d, reflect.Value v) => func((_, panic, _) => {
    ref decodeState d = ref _addr_d.val;


    if (d.opcode == scanBeginArray) 
        if (v.IsValid()) {
            {
                var err__prev2 = err;

                var err = d.array(v);

                if (err != null) {
                    return error.As(err)!;
                }

                err = err__prev2;

            }
        }
        else
 {
            d.skip();
        }
        d.scanNext();
    else if (d.opcode == scanBeginObject) 
        if (v.IsValid()) {
            {
                var err__prev2 = err;

                err = d.@object(v);

                if (err != null) {
                    return error.As(err)!;
                }

                err = err__prev2;

            }
        }
        else
 {
            d.skip();
        }
        d.scanNext();
    else if (d.opcode == scanBeginLiteral) 
        // All bytes inside literal return scanContinue op code.
        var start = d.readIndex();
        d.rescanLiteral();

        if (v.IsValid()) {
            {
                var err__prev2 = err;

                err = d.literalStore(d.data[(int)start..(int)d.readIndex()], v, false);

                if (err != null) {
                    return error.As(err)!;
                }

                err = err__prev2;

            }
        }
    else 
        panic(phasePanicMsg);
        return error.As(null!)!;
});

private partial struct unquotedValue {
}

// valueQuoted is like value but decodes a
// quoted string literal or literal null into an interface value.
// If it finds anything other than a quoted string literal or null,
// valueQuoted returns unquotedValue{}.
private static void valueQuoted(this ptr<decodeState> _addr_d) => func((_, panic, _) => {
    ref decodeState d = ref _addr_d.val;


    if (d.opcode == scanBeginArray || d.opcode == scanBeginObject) 
        d.skip();
        d.scanNext();
    else if (d.opcode == scanBeginLiteral) 
        var v = d.literalInterface();
        switch (v.type()) {
            case @string _:
                return v;
                break;
        }
    else 
        panic(phasePanicMsg);
        return new unquotedValue();
});

// indirect walks down v allocating pointers as needed,
// until it gets to a non-pointer.
// If it encounters an Unmarshaler, indirect stops and returns that.
// If decodingNull is true, indirect stops at the first settable pointer so it
// can be set to nil.
private static (Unmarshaler, encoding.TextUnmarshaler, reflect.Value) indirect(reflect.Value v, bool decodingNull) {
    Unmarshaler _p0 = default;
    encoding.TextUnmarshaler _p0 = default;
    reflect.Value _p0 = default;
 
    // Issue #24153 indicates that it is generally not a guaranteed property
    // that you may round-trip a reflect.Value by calling Value.Addr().Elem()
    // and expect the value to still be settable for values derived from
    // unexported embedded struct fields.
    //
    // The logic below effectively does this when it first addresses the value
    // (to satisfy possible pointer methods) and continues to dereference
    // subsequent pointers as necessary.
    //
    // After the first round-trip, we set v back to the original value to
    // preserve the original RW flags contained in reflect.Value.
    var v0 = v;
    var haveAddr = false; 

    // If v is a named type and is addressable,
    // start with its address, so that if the type has pointer methods,
    // we find them.
    if (v.Kind() != reflect.Ptr && v.Type().Name() != "" && v.CanAddr()) {
        haveAddr = true;
        v = v.Addr();
    }
    while (true) { 
        // Load value from interface, but only if the result will be
        // usefully addressable.
        if (v.Kind() == reflect.Interface && !v.IsNil()) {
            var e = v.Elem();
            if (e.Kind() == reflect.Ptr && !e.IsNil() && (!decodingNull || e.Elem().Kind() == reflect.Ptr)) {
                haveAddr = false;
                v = e;
                continue;
            }
        }
        if (v.Kind() != reflect.Ptr) {
            break;
        }
        if (decodingNull && v.CanSet()) {
            break;
        }
        if (v.Elem().Kind() == reflect.Interface && v.Elem().Elem() == v) {
            v = v.Elem();
            break;
        }
        if (v.IsNil()) {
            v.Set(reflect.New(v.Type().Elem()));
        }
        if (v.Type().NumMethod() > 0 && v.CanInterface()) {
            {
                Unmarshaler u__prev2 = u;

                Unmarshaler (u, ok) = Unmarshaler.As(v.Interface()._<Unmarshaler>())!;

                if (ok) {
                    return (u, null, new reflect.Value());
                }

                u = u__prev2;

            }
            if (!decodingNull) {
                {
                    Unmarshaler u__prev3 = u;

                    (u, ok) = v.Interface()._<encoding.TextUnmarshaler>();

                    if (ok) {
                        return (null, u, new reflect.Value());
                    }

                    u = u__prev3;

                }
            }
        }
        if (haveAddr) {
            v = v0; // restore original value after round-trip Value.Addr().Elem()
            haveAddr = false;
        }
        else
 {
            v = v.Elem();
        }
    }
    return (null, null, v);
}

// array consumes an array from d.data[d.off-1:], decoding into v.
// The first byte of the array ('[') has been read already.
private static error array(this ptr<decodeState> _addr_d, reflect.Value v) => func((_, panic, _) => {
    ref decodeState d = ref _addr_d.val;
 
    // Check for unmarshaler.
    var (u, ut, pv) = indirect(v, false);
    if (u != null) {
        var start = d.readIndex();
        d.skip();
        return error.As(u.UnmarshalJSON(d.data[(int)start..(int)d.off]))!;
    }
    if (ut != null) {
        d.saveError(addr(new UnmarshalTypeError(Value:"array",Type:v.Type(),Offset:int64(d.off))));
        d.skip();
        return error.As(null!)!;
    }
    v = pv; 

    // Check type of target.

    if (v.Kind() == reflect.Interface)
    {
        if (v.NumMethod() == 0) { 
            // Decoding into nil interface? Switch to non-reflect code.
            var ai = d.arrayInterface();
            v.Set(reflect.ValueOf(ai));
            return error.As(null!)!;
        }
        fallthrough = true;
    }
    if (fallthrough || v.Kind() == reflect.Array || v.Kind() == reflect.Slice)
    {
        break;
        goto __switch_break0;
    }
    // default: 
        d.saveError(addr(new UnmarshalTypeError(Value:"array",Type:v.Type(),Offset:int64(d.off))));
        d.skip();
        return error.As(null!)!;

    __switch_break0:;

    nint i = 0;
    while (true) { 
        // Look ahead for ] - can only happen on first iteration.
        d.scanWhile(scanSkipSpace);
        if (d.opcode == scanEndArray) {
            break;
        }
        if (v.Kind() == reflect.Slice) { 
            // Grow slice if necessary
            if (i >= v.Cap()) {
                var newcap = v.Cap() + v.Cap() / 2;
                if (newcap < 4) {
                    newcap = 4;
                }
                var newv = reflect.MakeSlice(v.Type(), v.Len(), newcap);
                reflect.Copy(newv, v);
                v.Set(newv);
            }
            if (i >= v.Len()) {
                v.SetLen(i + 1);
            }
        }
        if (i < v.Len()) { 
            // Decode into element.
            {
                var err__prev2 = err;

                var err = d.value(v.Index(i));

                if (err != null) {
                    return error.As(err)!;
                }

                err = err__prev2;

            }
        }
        else
 { 
            // Ran out of fixed array: skip.
            {
                var err__prev2 = err;

                err = d.value(new reflect.Value());

                if (err != null) {
                    return error.As(err)!;
                }

                err = err__prev2;

            }
        }
        i++; 

        // Next token must be , or ].
        if (d.opcode == scanSkipSpace) {
            d.scanWhile(scanSkipSpace);
        }
        if (d.opcode == scanEndArray) {
            break;
        }
        if (d.opcode != scanArrayValue) {
            panic(phasePanicMsg);
        }
    }

    if (i < v.Len()) {
        if (v.Kind() == reflect.Array) { 
            // Array. Zero the rest.
            var z = reflect.Zero(v.Type().Elem());
            while (i < v.Len()) {
                v.Index(i).Set(z);
                i++;
            }
        else
        } {
            v.SetLen(i);
        }
    }
    if (i == 0 && v.Kind() == reflect.Slice) {
        v.Set(reflect.MakeSlice(v.Type(), 0, 0));
    }
    return error.As(null!)!;
});

private static slice<byte> nullLiteral = (slice<byte>)"null";
private static var textUnmarshalerType = reflect.TypeOf((encoding.TextUnmarshaler.val)(null)).Elem();

// object consumes an object from d.data[d.off-1:], decoding into v.
// The first byte ('{') of the object has been read already.
private static error @object(this ptr<decodeState> _addr_d, reflect.Value v) => func((_, panic, _) => {
    ref decodeState d = ref _addr_d.val;
 
    // Check for unmarshaler.
    var (u, ut, pv) = indirect(v, false);
    if (u != null) {
        var start = d.readIndex();
        d.skip();
        return error.As(u.UnmarshalJSON(d.data[(int)start..(int)d.off]))!;
    }
    if (ut != null) {
        d.saveError(addr(new UnmarshalTypeError(Value:"object",Type:v.Type(),Offset:int64(d.off))));
        d.skip();
        return error.As(null!)!;
    }
    v = pv;
    var t = v.Type(); 

    // Decoding into nil interface? Switch to non-reflect code.
    if (v.Kind() == reflect.Interface && v.NumMethod() == 0) {
        var oi = d.objectInterface();
        v.Set(reflect.ValueOf(oi));
        return error.As(null!)!;
    }
    structFields fields = default; 

    // Check type of target:
    //   struct or
    //   map[T1]T2 where T1 is string, an integer type,
    //             or an encoding.TextUnmarshaler

    if (v.Kind() == reflect.Map) 
        // Map key must either have string kind, have an integer kind,
        // or be an encoding.TextUnmarshaler.

        if (t.Key().Kind() == reflect.String || t.Key().Kind() == reflect.Int || t.Key().Kind() == reflect.Int8 || t.Key().Kind() == reflect.Int16 || t.Key().Kind() == reflect.Int32 || t.Key().Kind() == reflect.Int64 || t.Key().Kind() == reflect.Uint || t.Key().Kind() == reflect.Uint8 || t.Key().Kind() == reflect.Uint16 || t.Key().Kind() == reflect.Uint32 || t.Key().Kind() == reflect.Uint64 || t.Key().Kind() == reflect.Uintptr)         else 
            if (!reflect.PtrTo(t.Key()).Implements(textUnmarshalerType)) {
                d.saveError(addr(new UnmarshalTypeError(Value:"object",Type:t,Offset:int64(d.off))));
                d.skip();
                return error.As(null!)!;
            }
                if (v.IsNil()) {
            v.Set(reflect.MakeMap(t));
        }
    else if (v.Kind() == reflect.Struct) 
        fields = cachedTypeFields(t); 
        // ok
    else 
        d.saveError(addr(new UnmarshalTypeError(Value:"object",Type:t,Offset:int64(d.off))));
        d.skip();
        return error.As(null!)!;
        reflect.Value mapElem = default;
    errorContext origErrorContext = default;
    if (d.errorContext != null) {
        origErrorContext = d.errorContext.val;
    }
    while (true) { 
        // Read opening " of string key or closing }.
        d.scanWhile(scanSkipSpace);
        if (d.opcode == scanEndObject) { 
            // closing } - can only happen on first iteration.
            break;
        }
        if (d.opcode != scanBeginLiteral) {
            panic(phasePanicMsg);
        }
        start = d.readIndex();
        d.rescanLiteral();
        var item = d.data[(int)start..(int)d.readIndex()];
        var (key, ok) = unquoteBytes(item);
        if (!ok) {
            panic(phasePanicMsg);
        }
        reflect.Value subv = default;
        var destring = false; // whether the value is wrapped in a string to be decoded first

        if (v.Kind() == reflect.Map) {
            var elemType = t.Elem();
            if (!mapElem.IsValid()) {
                mapElem = reflect.New(elemType).Elem();
            }
            else
 {
                mapElem.Set(reflect.Zero(elemType));
            }
            subv = mapElem;
        }
        else
 {
            ptr<field> f;
            {
                var i__prev2 = i;

                var (i, ok) = fields.nameIndex[string(key)];

                if (ok) { 
                    // Found an exact name match.
                    f = _addr_fields.list[i];
                }
                else
 { 
                    // Fall back to the expensive case-insensitive
                    // linear search.
                    {
                        var i__prev2 = i;

                        foreach (var (__i) in fields.list) {
                            i = __i;
                            var ff = _addr_fields.list[i];
                            if (ff.equalFold(ff.nameBytes, key)) {
                                f = ff;
                                break;
                            }
                        }

                        i = i__prev2;
                    }
                }

                i = i__prev2;

            }
            if (f != null) {
                subv = v;
                destring = f.quoted;
                {
                    var i__prev2 = i;

                    foreach (var (_, __i) in f.index) {
                        i = __i;
                        if (subv.Kind() == reflect.Ptr) {
                            if (subv.IsNil()) { 
                                // If a struct embeds a pointer to an unexported type,
                                // it is not possible to set a newly allocated value
                                // since the field is unexported.
                                //
                                // See https://golang.org/issue/21357
                                if (!subv.CanSet()) {
                                    d.saveError(fmt.Errorf("json: cannot set embedded pointer to unexported struct: %v", subv.Type().Elem())); 
                                    // Invalidate subv to ensure d.value(subv) skips over
                                    // the JSON value without assigning it to subv.
                                    subv = new reflect.Value();
                                    destring = false;
                                    break;
                                }
                                subv.Set(reflect.New(subv.Type().Elem()));
                            }
                            subv = subv.Elem();
                        }
                        subv = subv.Field(i);
                    }

                    i = i__prev2;
                }

                if (d.errorContext == null) {
                    d.errorContext = @new<errorContext>();
                }
                d.errorContext.FieldStack = append(d.errorContext.FieldStack, f.name);
                d.errorContext.Struct = t;
            }
            else if (d.disallowUnknownFields) {
                d.saveError(fmt.Errorf("json: unknown field %q", key));
            }
        }
        if (d.opcode == scanSkipSpace) {
            d.scanWhile(scanSkipSpace);
        }
        if (d.opcode != scanObjectKey) {
            panic(phasePanicMsg);
        }
        d.scanWhile(scanSkipSpace);

        if (destring) {
            switch (d.valueQuoted().type()) {
                case 
                    {
                        var err__prev2 = err;

                        var err = d.literalStore(nullLiteral, subv, false);

                        if (err != null) {
                            return error.As(err)!;
                        }

                        err = err__prev2;

                    }
                    break;
                case @string qv:
                    {
                        var err__prev2 = err;

                        err = d.literalStore((slice<byte>)qv, subv, true);

                        if (err != null) {
                            return error.As(err)!;
                        }

                        err = err__prev2;

                    }
                    break;
                default:
                {
                    var qv = d.valueQuoted().type();
                    d.saveError(fmt.Errorf("json: invalid use of ,string struct tag, trying to unmarshal unquoted value into %v", subv.Type()));
                    break;
                }
            }
        }
        else
 {
            {
                var err__prev2 = err;

                err = d.value(subv);

                if (err != null) {
                    return error.As(err)!;
                }

                err = err__prev2;

            }
        }
        if (v.Kind() == reflect.Map) {
            var kt = t.Key();
            reflect.Value kv = default;

            if (reflect.PtrTo(kt).Implements(textUnmarshalerType)) 
                kv = reflect.New(kt);
                {
                    var err__prev2 = err;

                    err = d.literalStore(item, kv, true);

                    if (err != null) {
                        return error.As(err)!;
                    }

                    err = err__prev2;

                }
                kv = kv.Elem();
            else if (kt.Kind() == reflect.String) 
                kv = reflect.ValueOf(key).Convert(kt);
            else 

                if (kt.Kind() == reflect.Int || kt.Kind() == reflect.Int8 || kt.Kind() == reflect.Int16 || kt.Kind() == reflect.Int32 || kt.Kind() == reflect.Int64) 
                    var s = string(key);
                    var (n, err) = strconv.ParseInt(s, 10, 64);
                    if (err != null || reflect.Zero(kt).OverflowInt(n)) {
                        d.saveError(addr(new UnmarshalTypeError(Value:"number "+s,Type:kt,Offset:int64(start+1))));
                        break;
                    }
                    kv = reflect.ValueOf(n).Convert(kt);
                else if (kt.Kind() == reflect.Uint || kt.Kind() == reflect.Uint8 || kt.Kind() == reflect.Uint16 || kt.Kind() == reflect.Uint32 || kt.Kind() == reflect.Uint64 || kt.Kind() == reflect.Uintptr) 
                    s = string(key);
                    (n, err) = strconv.ParseUint(s, 10, 64);
                    if (err != null || reflect.Zero(kt).OverflowUint(n)) {
                        d.saveError(addr(new UnmarshalTypeError(Value:"number "+s,Type:kt,Offset:int64(start+1))));
                        break;
                    }
                    kv = reflect.ValueOf(n).Convert(kt);
                else 
                    panic("json: Unexpected key type"); // should never occur
                                        if (kv.IsValid()) {
                v.SetMapIndex(kv, subv);
            }
        }
        if (d.opcode == scanSkipSpace) {
            d.scanWhile(scanSkipSpace);
        }
        if (d.errorContext != null) { 
            // Reset errorContext to its original state.
            // Keep the same underlying array for FieldStack, to reuse the
            // space and avoid unnecessary allocs.
            d.errorContext.FieldStack = d.errorContext.FieldStack[..(int)len(origErrorContext.FieldStack)];
            d.errorContext.Struct = origErrorContext.Struct;
        }
        if (d.opcode == scanEndObject) {
            break;
        }
        if (d.opcode != scanObjectValue) {
            panic(phasePanicMsg);
        }
    }
    return error.As(null!)!;
});

// convertNumber converts the number literal s to a float64 or a Number
// depending on the setting of d.useNumber.
private static (object, error) convertNumber(this ptr<decodeState> _addr_d, @string s) {
    object _p0 = default;
    error _p0 = default!;
    ref decodeState d = ref _addr_d.val;

    if (d.useNumber) {
        return (Number(s), error.As(null!)!);
    }
    var (f, err) = strconv.ParseFloat(s, 64);
    if (err != null) {
        return (null, error.As(addr(new UnmarshalTypeError(Value:"number "+s,Type:reflect.TypeOf(0.0),Offset:int64(d.off)))!)!);
    }
    return (f, error.As(null!)!);
}

private static var numberType = reflect.TypeOf(Number(""));

// literalStore decodes a literal stored in item into v.
//
// fromQuoted indicates whether this literal came from unwrapping a
// string from the ",string" struct tag option. this is used only to
// produce more helpful error messages.
private static error literalStore(this ptr<decodeState> _addr_d, slice<byte> item, reflect.Value v, bool fromQuoted) => func((_, panic, _) => {
    ref decodeState d = ref _addr_d.val;
 
    // Check for unmarshaler.
    if (len(item) == 0) { 
        //Empty string given
        d.saveError(fmt.Errorf("json: invalid use of ,string struct tag, trying to unmarshal %q into %v", item, v.Type()));
        return error.As(null!)!;
    }
    var isNull = item[0] == 'n'; // null
    var (u, ut, pv) = indirect(v, isNull);
    if (u != null) {
        return error.As(u.UnmarshalJSON(item))!;
    }
    if (ut != null) {
        if (item[0] != '"') {
            if (fromQuoted) {
                d.saveError(fmt.Errorf("json: invalid use of ,string struct tag, trying to unmarshal %q into %v", item, v.Type()));
                return error.As(null!)!;
            }
            @string val = "number";
            switch (item[0]) {
                case 'n': 
                    val = "null";
                    break;
                case 't': 

                case 'f': 
                    val = "bool";
                    break;
            }
            d.saveError(addr(new UnmarshalTypeError(Value:val,Type:v.Type(),Offset:int64(d.readIndex()))));
            return error.As(null!)!;
        }
        var (s, ok) = unquoteBytes(item);
        if (!ok) {
            if (fromQuoted) {
                return error.As(fmt.Errorf("json: invalid use of ,string struct tag, trying to unmarshal %q into %v", item, v.Type()))!;
            }
            panic(phasePanicMsg);
        }
        return error.As(ut.UnmarshalText(s))!;
    }
    v = pv;

    {
        var c = item[0];

        switch (c) {
            case 'n': // null
                // The main parser checks that only true and false can reach here,
                // but if this was a quoted string input, it could be anything.
                if (fromQuoted && string(item) != "null") {
                    d.saveError(fmt.Errorf("json: invalid use of ,string struct tag, trying to unmarshal %q into %v", item, v.Type()));
                    break;
                }

                if (v.Kind() == reflect.Interface || v.Kind() == reflect.Ptr || v.Kind() == reflect.Map || v.Kind() == reflect.Slice) 
                    v.Set(reflect.Zero(v.Type())); 
                    // otherwise, ignore null for primitives/string
                break;
            case 't': // true, false

            case 'f': // true, false
                           var value = item[0] == 't'; 
                           // The main parser checks that only true and false can reach here,
                           // but if this was a quoted string input, it could be anything.
                           if (fromQuoted && string(item) != "true" && string(item) != "false") {
                               d.saveError(fmt.Errorf("json: invalid use of ,string struct tag, trying to unmarshal %q into %v", item, v.Type()));
                               break;
                           }

                           if (v.Kind() == reflect.Bool) 
                               v.SetBool(value);
                           else if (v.Kind() == reflect.Interface) 
                               if (v.NumMethod() == 0) {
                                   v.Set(reflect.ValueOf(value));
                               }
                               else
                {
                                   d.saveError(addr(new UnmarshalTypeError(Value:"bool",Type:v.Type(),Offset:int64(d.readIndex()))));
                               }
                           else 
                               if (fromQuoted) {
                                   d.saveError(fmt.Errorf("json: invalid use of ,string struct tag, trying to unmarshal %q into %v", item, v.Type()));
                               }
                               else
                {
                                   d.saveError(addr(new UnmarshalTypeError(Value:"bool",Type:v.Type(),Offset:int64(d.readIndex()))));
                               }
                break;
            case '"': // string
                           (s, ok) = unquoteBytes(item);
                           if (!ok) {
                               if (fromQuoted) {
                                   return error.As(fmt.Errorf("json: invalid use of ,string struct tag, trying to unmarshal %q into %v", item, v.Type()))!;
                               }
                               panic(phasePanicMsg);
                           }

                           if (v.Kind() == reflect.Slice) 
                               if (v.Type().Elem().Kind() != reflect.Uint8) {
                                   d.saveError(addr(new UnmarshalTypeError(Value:"string",Type:v.Type(),Offset:int64(d.readIndex()))));
                                   break;
                               }
                               var b = make_slice<byte>(base64.StdEncoding.DecodedLen(len(s)));
                               var (n, err) = base64.StdEncoding.Decode(b, s);
                               if (err != null) {
                                   d.saveError(err);
                                   break;
                               }
                               v.SetBytes(b[..(int)n]);
                           else if (v.Kind() == reflect.String) 
                               if (v.Type() == numberType && !isValidNumber(string(s))) {
                                   return error.As(fmt.Errorf("json: invalid number literal, trying to unmarshal %q into Number", item))!;
                               }
                               v.SetString(string(s));
                           else if (v.Kind() == reflect.Interface) 
                               if (v.NumMethod() == 0) {
                                   v.Set(reflect.ValueOf(string(s)));
                               }
                               else
                {
                                   d.saveError(addr(new UnmarshalTypeError(Value:"string",Type:v.Type(),Offset:int64(d.readIndex()))));
                               }
                           else 
                               d.saveError(addr(new UnmarshalTypeError(Value:"string",Type:v.Type(),Offset:int64(d.readIndex()))));
                break;
            default: // number
                if (c != '-' && (c < '0' || c > '9')) {
                    if (fromQuoted) {
                        return error.As(fmt.Errorf("json: invalid use of ,string struct tag, trying to unmarshal %q into %v", item, v.Type()))!;
                    }
                    panic(phasePanicMsg);
                }
                var s = string(item);

                if (v.Kind() == reflect.Interface) 
                    (n, err) = d.convertNumber(s);
                    if (err != null) {
                        d.saveError(err);
                        break;
                    }
                    if (v.NumMethod() != 0) {
                        d.saveError(addr(new UnmarshalTypeError(Value:"number",Type:v.Type(),Offset:int64(d.readIndex()))));
                        break;
                    }
                    v.Set(reflect.ValueOf(n));
                else if (v.Kind() == reflect.Int || v.Kind() == reflect.Int8 || v.Kind() == reflect.Int16 || v.Kind() == reflect.Int32 || v.Kind() == reflect.Int64) 
                    (n, err) = strconv.ParseInt(s, 10, 64);
                    if (err != null || v.OverflowInt(n)) {
                        d.saveError(addr(new UnmarshalTypeError(Value:"number "+s,Type:v.Type(),Offset:int64(d.readIndex()))));
                        break;
                    }
                    v.SetInt(n);
                else if (v.Kind() == reflect.Uint || v.Kind() == reflect.Uint8 || v.Kind() == reflect.Uint16 || v.Kind() == reflect.Uint32 || v.Kind() == reflect.Uint64 || v.Kind() == reflect.Uintptr) 
                    (n, err) = strconv.ParseUint(s, 10, 64);
                    if (err != null || v.OverflowUint(n)) {
                        d.saveError(addr(new UnmarshalTypeError(Value:"number "+s,Type:v.Type(),Offset:int64(d.readIndex()))));
                        break;
                    }
                    v.SetUint(n);
                else if (v.Kind() == reflect.Float32 || v.Kind() == reflect.Float64) 
                    (n, err) = strconv.ParseFloat(s, v.Type().Bits());
                    if (err != null || v.OverflowFloat(n)) {
                        d.saveError(addr(new UnmarshalTypeError(Value:"number "+s,Type:v.Type(),Offset:int64(d.readIndex()))));
                        break;
                    }
                    v.SetFloat(n);
                else 
                    if (v.Kind() == reflect.String && v.Type() == numberType) { 
                        // s must be a valid number, because it's
                        // already been tokenized.
                        v.SetString(s);
                        break;
                    }
                    if (fromQuoted) {
                        return error.As(fmt.Errorf("json: invalid use of ,string struct tag, trying to unmarshal %q into %v", item, v.Type()))!;
                    }
                    d.saveError(addr(new UnmarshalTypeError(Value:"number",Type:v.Type(),Offset:int64(d.readIndex()))));
                break;
        }
    }
    return error.As(null!)!;
});

// The xxxInterface routines build up a value to be stored
// in an empty interface. They are not strictly necessary,
// but they avoid the weight of reflection in this common case.

// valueInterface is like value but returns interface{}
private static object valueInterface(this ptr<decodeState> _addr_d) => func((_, panic, _) => {
    object val = default;
    ref decodeState d = ref _addr_d.val;


    if (d.opcode == scanBeginArray) 
        val = d.arrayInterface();
        d.scanNext();
    else if (d.opcode == scanBeginObject) 
        val = d.objectInterface();
        d.scanNext();
    else if (d.opcode == scanBeginLiteral) 
        val = d.literalInterface();
    else 
        panic(phasePanicMsg);
        return ;
});

// arrayInterface is like array but returns []interface{}.
private static slice<object> arrayInterface(this ptr<decodeState> _addr_d) => func((_, panic, _) => {
    ref decodeState d = ref _addr_d.val;

    var v = make_slice<object>(0);
    while (true) { 
        // Look ahead for ] - can only happen on first iteration.
        d.scanWhile(scanSkipSpace);
        if (d.opcode == scanEndArray) {
            break;
        }
        v = append(v, d.valueInterface()); 

        // Next token must be , or ].
        if (d.opcode == scanSkipSpace) {
            d.scanWhile(scanSkipSpace);
        }
        if (d.opcode == scanEndArray) {
            break;
        }
        if (d.opcode != scanArrayValue) {
            panic(phasePanicMsg);
        }
    }
    return v;
});

// objectInterface is like object but returns map[string]interface{}.
private static void objectInterface(this ptr<decodeState> _addr_d) => func((_, panic, _) => {
    ref decodeState d = ref _addr_d.val;

    var m = make();
    while (true) { 
        // Read opening " of string key or closing }.
        d.scanWhile(scanSkipSpace);
        if (d.opcode == scanEndObject) { 
            // closing } - can only happen on first iteration.
            break;
        }
        if (d.opcode != scanBeginLiteral) {
            panic(phasePanicMsg);
        }
        var start = d.readIndex();
        d.rescanLiteral();
        var item = d.data[(int)start..(int)d.readIndex()];
        var (key, ok) = unquote(item);
        if (!ok) {
            panic(phasePanicMsg);
        }
        if (d.opcode == scanSkipSpace) {
            d.scanWhile(scanSkipSpace);
        }
        if (d.opcode != scanObjectKey) {
            panic(phasePanicMsg);
        }
        d.scanWhile(scanSkipSpace); 

        // Read value.
        m[key] = d.valueInterface(); 

        // Next token must be , or }.
        if (d.opcode == scanSkipSpace) {
            d.scanWhile(scanSkipSpace);
        }
        if (d.opcode == scanEndObject) {
            break;
        }
        if (d.opcode != scanObjectValue) {
            panic(phasePanicMsg);
        }
    }
    return m;
});

// literalInterface consumes and returns a literal from d.data[d.off-1:] and
// it reads the following byte ahead. The first byte of the literal has been
// read already (that's how the caller knows it's a literal).
private static void literalInterface(this ptr<decodeState> _addr_d) => func((_, panic, _) => {
    ref decodeState d = ref _addr_d.val;
 
    // All bytes inside literal return scanContinue op code.
    var start = d.readIndex();
    d.rescanLiteral();

    var item = d.data[(int)start..(int)d.readIndex()];

    {
        var c = item[0];

        switch (c) {
            case 'n': // null
                return null;
                break;
            case 't': // true, false

            case 'f': // true, false
                return c == 't';
                break;
            case '"': // string
                var (s, ok) = unquote(item);
                if (!ok) {
                    panic(phasePanicMsg);
                }
                return s;
                break;
            default: // number
                if (c != '-' && (c < '0' || c > '9')) {
                    panic(phasePanicMsg);
                }
                var (n, err) = d.convertNumber(string(item));
                if (err != null) {
                    d.saveError(err);
                }
                return n;
                break;
        }
    }
});

// getu4 decodes \uXXXX from the beginning of s, returning the hex value,
// or it returns -1.
private static int getu4(slice<byte> s) {
    if (len(s) < 6 || s[0] != '\\' || s[1] != 'u') {
        return -1;
    }
    int r = default;
    foreach (var (_, c) in s[(int)2..(int)6]) {

        if ('0' <= c && c <= '9') 
            c = c - '0';
        else if ('a' <= c && c <= 'f') 
            c = c - 'a' + 10;
        else if ('A' <= c && c <= 'F') 
            c = c - 'A' + 10;
        else 
            return -1;
                r = r * 16 + rune(c);
    }    return r;
}

// unquote converts a quoted JSON string literal s into an actual string t.
// The rules are different than for Go, so cannot use strconv.Unquote.
private static (@string, bool) unquote(slice<byte> s) {
    @string t = default;
    bool ok = default;

    s, ok = unquoteBytes(s);
    t = string(s);
    return ;
}

private static (slice<byte>, bool) unquoteBytes(slice<byte> s) {
    slice<byte> t = default;
    bool ok = default;

    if (len(s) < 2 || s[0] != '"' || s[len(s) - 1] != '"') {
        return ;
    }
    s = s[(int)1..(int)len(s) - 1]; 

    // Check for unusual characters. If there are none,
    // then no unquoting is needed, so return a slice of the
    // original bytes.
    nint r = 0;
    while (r < len(s)) {
        var c = s[r];
        if (c == '\\' || c == '"' || c < ' ') {
            break;
        }
        if (c < utf8.RuneSelf) {
            r++;
            continue;
        }
        var (rr, size) = utf8.DecodeRune(s[(int)r..]);
        if (rr == utf8.RuneError && size == 1) {
            break;
        }
        r += size;
    }
    if (r == len(s)) {
        return (s, true);
    }
    var b = make_slice<byte>(len(s) + 2 * utf8.UTFMax);
    var w = copy(b, s[(int)0..(int)r]);
    while (r < len(s)) { 
        // Out of room? Can only happen if s is full of
        // malformed UTF-8 and we're replacing each
        // byte with RuneError.
        if (w >= len(b) - 2 * utf8.UTFMax) {
            var nb = make_slice<byte>((len(b) + utf8.UTFMax) * 2);
            copy(nb, b[(int)0..(int)w]);
            b = nb;
        }
        {
            var c__prev1 = c;

            c = s[r];


            if (c == '\\') 
                r++;
                if (r >= len(s)) {
                    return ;
                }
                switch (s[r]) {
                    case '"': 

                    case '\\': 

                    case '/': 

                    case '\'': 
                        b[w] = s[r];
                        r++;
                        w++;
                        break;
                    case 'b': 
                        b[w] = '\b';
                        r++;
                        w++;
                        break;
                    case 'f': 
                        b[w] = '\f';
                        r++;
                        w++;
                        break;
                    case 'n': 
                        b[w] = '\n';
                        r++;
                        w++;
                        break;
                    case 'r': 
                        b[w] = '\r';
                        r++;
                        w++;
                        break;
                    case 't': 
                        b[w] = '\t';
                        r++;
                        w++;
                        break;
                    case 'u': 
                        r--;
                        var rr = getu4(s[(int)r..]);
                        if (rr < 0) {
                            return ;
                        }
                        r += 6;
                        if (utf16.IsSurrogate(rr)) {
                            var rr1 = getu4(s[(int)r..]);
                            {
                                var dec = utf16.DecodeRune(rr, rr1);

                                if (dec != unicode.ReplacementChar) { 
                                    // A valid pair; consume.
                                    r += 6;
                                    w += utf8.EncodeRune(b[(int)w..], dec);
                                    break;
                                } 
                                // Invalid surrogate; fall back to replacement rune.

                            } 
                            // Invalid surrogate; fall back to replacement rune.
                            rr = unicode.ReplacementChar;
                        }
                        w += utf8.EncodeRune(b[(int)w..], rr);
                        break;
                    default: 
                        return ;
                        break;
                } 

                // Quote, control characters are invalid.
            else if (c == '"' || c < ' ') 
                return ; 

                // ASCII
            else if (c < utf8.RuneSelf) 
                b[w] = c;
                r++;
                w++; 

                // Coerce to well-formed UTF-8.
            else 
                (rr, size) = utf8.DecodeRune(s[(int)r..]);
                r += size;
                w += utf8.EncodeRune(b[(int)w..], rr);


            c = c__prev1;
        }
    }
    return (b[(int)0..(int)w], true);
}

} // end json_package
