// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Represents JSON data structure using native Go types: booleans, floats,
// strings, arrays, and maps.
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
using _ = unsafe_package; // for linkname
using unicode;

partial class json_package {

// Unmarshal parses the JSON-encoded data and stores the result
// in the value pointed to by v. If v is nil or not a pointer,
// Unmarshal returns an [InvalidUnmarshalError].
//
// Unmarshal uses the inverse of the encodings that
// [Marshal] uses, allocating maps, slices, and pointers as necessary,
// with the following additional rules:
//
// To unmarshal JSON into a pointer, Unmarshal first handles the case of
// the JSON being the JSON literal null. In that case, Unmarshal sets
// the pointer to nil. Otherwise, Unmarshal unmarshals the JSON into
// the value pointed at by the pointer. If the pointer is nil, Unmarshal
// allocates a new value for it to point to.
//
// To unmarshal JSON into a value implementing [Unmarshaler],
// Unmarshal calls that value's [Unmarshaler.UnmarshalJSON] method, including
// when the input is a JSON null.
// Otherwise, if the value implements [encoding.TextUnmarshaler]
// and the input is a JSON quoted string, Unmarshal calls
// [encoding.TextUnmarshaler.UnmarshalText] with the unquoted form of the string.
//
// To unmarshal JSON into a struct, Unmarshal matches incoming object
// keys to the keys used by [Marshal] (either the struct field name or its tag),
// preferring an exact match but also accepting a case-insensitive match. By
// default, object keys which don't have a corresponding struct field are
// ignored (see [Decoder.DisallowUnknownFields] for an alternative).
//
// To unmarshal JSON into an interface value,
// Unmarshal stores one of these in the interface value:
//
//   - bool, for JSON booleans
//   - float64, for JSON numbers
//   - string, for JSON strings
//   - []interface{}, for JSON arrays
//   - map[string]interface{}, for JSON objects
//   - nil for JSON null
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
// either be any string type, an integer, or implement [encoding.TextUnmarshaler].
//
// If the JSON-encoded data contain a syntax error, Unmarshal returns a [SyntaxError].
//
// If a JSON value is not appropriate for a given target type,
// or if a JSON number overflows the target type, Unmarshal
// skips that field and completes the unmarshaling as best it can.
// If no more serious errors are encountered, Unmarshal returns
// an [UnmarshalTypeError] describing the earliest such error. In any
// case, it's not guaranteed that all the remaining fields following
// the problematic one will be unmarshaled into the target object.
//
// The JSON null value unmarshals into an interface, map, pointer, or slice
// by setting that Go value to nil. Because null is often used in JSON to mean
// “not present,” unmarshaling a JSON null into any other Go type has no effect
// on the value and produces no error.
//
// When unmarshaling quoted strings, invalid UTF-8 or
// invalid UTF-16 surrogate pairs are not treated as an error.
// Instead, they are replaced by the Unicode replacement
// character U+FFFD.
public static error Unmarshal(slice<byte> data, any v) {
    // Check for well-formedness.
    // Avoids filling out half a data structure
    // before discovering a JSON syntax error.
    ref var d = ref heap(new decodeState(), out var Ꮡd);
    var err = checkValid(data, Ꮡd.of(decodeState.Ꮡscan));
    if (err != default!) {
        return err;
    }
    d.init(data);
    return d.unmarshal(v);
}

// Unmarshaler is the interface implemented by types
// that can unmarshal a JSON description of themselves.
// The input can be assumed to be a valid encoding of
// a JSON value. UnmarshalJSON must copy the JSON data
// if it wishes to retain the data after returning.
//
// By convention, to approximate the behavior of [Unmarshal] itself,
// Unmarshalers implement UnmarshalJSON([]byte("null")) as a no-op.
[GoType] partial interface Unmarshaler {
    error UnmarshalJSON(slice<byte> _);
}

// An UnmarshalTypeError describes a JSON value that was
// not appropriate for a value of a specific Go type.
[GoType] partial struct UnmarshalTypeError {
    public @string Value;      // description of JSON value - "bool", "array", "number -5"
    public reflect_package.ΔType Type; // type of Go value it could not be assigned to
    public int64 Offset;        // error occurred after reading Offset bytes
    public @string Struct;      // name of the struct type containing the field
    public @string Field;      // the full path from root node to the field
}

[GoRecv] public static @string Error(this ref UnmarshalTypeError e) {
    if (e.Struct != ""u8 || e.Field != ""u8) {
        return "json: cannot unmarshal "u8 + e.Value + " into Go struct field "u8 + e.Struct + "."u8 + e.Field + " of type "u8 + e.Type.String();
    }
    return "json: cannot unmarshal "u8 + e.Value + " into Go value of type "u8 + e.Type.String();
}

// An UnmarshalFieldError describes a JSON object key that
// led to an unexported (and therefore unwritable) struct field.
//
// Deprecated: No longer used; kept for compatibility.
[GoType] partial struct UnmarshalFieldError {
    public @string Key;
    public reflect_package.ΔType Type;
    public reflect_package.StructField Field;
}

[GoRecv] public static @string Error(this ref UnmarshalFieldError e) {
    return "json: cannot unmarshal object key "u8 + strconv.Quote(e.Key) + " into unexported field "u8 + e.Field.Name + " of type "u8 + e.Type.String();
}

// An InvalidUnmarshalError describes an invalid argument passed to [Unmarshal].
// (The argument to [Unmarshal] must be a non-nil pointer.)
[GoType] partial struct InvalidUnmarshalError {
    public reflect_package.ΔType Type;
}

[GoRecv] public static @string Error(this ref InvalidUnmarshalError e) {
    if (e.Type == default!) {
        return "json: Unmarshal(nil)"u8;
    }
    if (e.Type.Kind() != reflect.ΔPointer) {
        return "json: Unmarshal(non-pointer "u8 + e.Type.String() + ")"u8;
    }
    return "json: Unmarshal(nil "u8 + e.Type.String() + ")"u8;
}

[GoRecv] internal static error unmarshal(this ref decodeState d, any v) {
    var rv = reflect.ValueOf(v);
    if (rv.Kind() != reflect.ΔPointer || rv.IsNil()) {
        return new InvalidUnmarshalError(reflect.TypeOf(v));
    }
    d.scan.reset();
    d.scanWhile(scanSkipSpace);
    // We decode rv not rv.Elem because the Unmarshaler interface
    // test must be applied at the top level of the value.
    var err = d.value(rv);
    if (err != default!) {
        return d.addErrorContext(err);
    }
    return d.savedError;
}

[GoType("@string")] partial struct Number;

// String returns the literal text of the number.
public static @string String(this Number n) {
    return ((@string)n);
}

// Float64 returns the number as a float64.
public static (float64, error) Float64(this Number n) {
    return strconv.ParseFloat(((@string)n), 64);
}

// Int64 returns the number as an int64.
public static (int64, error) Int64(this Number n) {
    return strconv.ParseInt(((@string)n), 10, 64);
}

// An errorContext provides context for type errors during decoding.
[GoType] partial struct errorContext {
    public reflect_package.ΔType Struct;
    public slice<@string> FieldStack;
}

// decodeState represents the state while decoding a JSON value.
[GoType] partial struct decodeState {
    internal slice<byte> data;
    internal nint off; // next read offset in data
    internal nint opcode; // last read result
    internal scanner scan;
    internal ж<errorContext> errorContext;
    internal error savedError;
    internal bool useNumber;
    internal bool disallowUnknownFields;
}

// readIndex returns the position of the last byte read.
[GoRecv] internal static nint readIndex(this ref decodeState d) {
    return d.off - 1;
}

// phasePanicMsg is used as a panic message when we end up with something that
// shouldn't happen. It can indicate a bug in the JSON decoder, or that
// something is editing the data slice while the decoder executes.
internal static readonly @string phasePanicMsg = "JSON decoder out of sync - data changing underfoot?"u8;

[GoRecv("capture")] internal static ж<decodeState> init(this ref decodeState d, slice<byte> data) {
    d.data = data;
    d.off = 0;
    d.savedError = default!;
    if (d.errorContext != nil) {
        d.errorContext.Struct = default!;
        // Reuse the allocated space for the FieldStack slice.
        d.errorContext.FieldStack = d.errorContext.FieldStack[..0];
    }
    return initꓸᏑd;
}

// saveError saves the first err it is called with,
// for reporting at the end of the unmarshal.
[GoRecv] internal static void saveError(this ref decodeState d, error err) {
    if (d.savedError == default!) {
        d.savedError = d.addErrorContext(err);
    }
}

// addErrorContext returns a new error enhanced with information from d.errorContext
[GoRecv] internal static error addErrorContext(this ref decodeState d, error err) {
    if (d.errorContext != nil && (d.errorContext.Struct != default! || len(d.errorContext.FieldStack) > 0)) {
        switch (err.type()) {
        case UnmarshalTypeError.val err: {
            err.val.Struct = d.errorContext.Struct.Name();
            err.val.Field = strings.Join(d.errorContext.FieldStack, "."u8);
            break;
        }}
    }
    return err;
}

// skip scans to the end of what was started.
[GoRecv] internal static void skip(this ref decodeState d) {
    var s = Ꮡ(d.scan);
    var data = d.data;
    nint i = d.off;
    nint depth = len((~s).parseState);
    while (ᐧ) {
        nint op = (~s).step(s, data[i]);
        i++;
        if (len((~s).parseState) < depth) {
            d.off = i;
            d.opcode = op;
            return;
        }
    }
}

// scanNext processes the byte at d.data[d.off].
[GoRecv] internal static void scanNext(this ref decodeState d) {
    if (d.off < len(d.data)){
        d.opcode = d.scan.step(Ꮡ(d.scan), d.data[d.off]);
        d.off++;
    } else {
        d.opcode = d.scan.eof();
        d.off = len(d.data) + 1;
    }
}

// mark processed EOF with len+1

// scanWhile processes bytes in d.data[d.off:] until it
// receives a scan code not equal to op.
[GoRecv] internal static void scanWhile(this ref decodeState d, nint op) {
    var s = Ꮡ(d.scan);
    var data = d.data;
    nint i = d.off;
    while (i < len(data)) {
        nint newOp = (~s).step(s, data[i]);
        i++;
        if (newOp != op) {
            d.opcode = newOp;
            d.off = i;
            return;
        }
    }
    d.off = len(data) + 1;
    // mark processed EOF with len+1
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
[GoRecv] internal static void rescanLiteral(this ref decodeState d) {
    var data = d.data;
    nint i = d.off;
Switch:
    switch (data[i - 1]) {
    case (rune)'"': {
        for (; i < len(data); i++) {
            // string
            switch (data[i]) {
            case (rune)'\\': {
                i++;
                break;
            }
            case (rune)'"': {
                i++;
                goto break_Switch;
                break;
            }}

        }
        break;
    }
    case (rune)'0' or (rune)'1' or (rune)'2' or (rune)'3' or (rune)'4' or (rune)'5' or (rune)'6' or (rune)'7' or (rune)'8' or (rune)'9' or (rune)'-': {
        for (; i < len(data); i++) {
            // escaped char
            // tokenize the closing quote too
            // number
            switch (data[i]) {
            case (rune)'0' or (rune)'1' or (rune)'2' or (rune)'3' or (rune)'4' or (rune)'5' or (rune)'6' or (rune)'7' or (rune)'8' or (rune)'9' or (rune)'.' or (rune)'e' or (rune)'E' or (rune)'+' or (rune)'-': {
                break;
            }
            default: {
                goto break_Switch;
                break;
            }}

        }
        break;
    }
    case (rune)'t': {
        i += len("rue");
        break;
    }
    case (rune)'f': {
        i += len("alse");
        break;
    }
    case (rune)'n': {
        i += len("ull");
        break;
    }}

    // true
    // false
    // null
    if (i < len(data)){
        d.opcode = stateEndValue(Ꮡ(d.scan), data[i]);
    } else {
        d.opcode = scanEnd;
    }
    d.off = i + 1;
}

// value consumes a JSON value from d.data[d.off-1:], decoding into v, and
// reads the following byte ahead. If v is invalid, the value is discarded.
// The first byte of the value has been read already.
[GoRecv] internal static error value(this ref decodeState d, reflectꓸValue v) {
    var exprᴛ1 = d.opcode;
    { /* default: */
        throw panic(phasePanicMsg);
    }
    else if (exprᴛ1 == scanBeginArray) {
        if (v.IsValid()){
            {
                var err = d.Δarray(v); if (err != default!) {
                    return err;
                }
            }
        } else {
            d.skip();
        }
        d.scanNext();
    }
    else if (exprᴛ1 == scanBeginObject) {
        if (v.IsValid()){
            {
                var err = d.@object(v); if (err != default!) {
                    return err;
                }
            }
        } else {
            d.skip();
        }
        d.scanNext();
    }
    else if (exprᴛ1 == scanBeginLiteral) {
        nint start = d.readIndex();
        d.rescanLiteral();
        if (v.IsValid()) {
            // All bytes inside literal return scanContinue op code.
            {
                var err = d.literalStore(d.data[(int)(start)..(int)(d.readIndex())], v, false); if (err != default!) {
                    return err;
                }
            }
        }
    }

    return default!;
}

[GoType] partial struct unquotedValue {
}

// valueQuoted is like value but decodes a
// quoted string literal or literal null into an interface value.
// If it finds anything other than a quoted string literal or null,
// valueQuoted returns unquotedValue{}.
[GoRecv] internal static any valueQuoted(this ref decodeState d) {
    var exprᴛ1 = d.opcode;
    { /* default: */
        throw panic(phasePanicMsg);
    }
    else if (exprᴛ1 == scanBeginArray || exprᴛ1 == scanBeginObject) {
        d.skip();
        d.scanNext();
    }
    else if (exprᴛ1 == scanBeginLiteral) {
        var v = d.literalInterface();
        switch (v.type()) {
        case default! : {
            return v;
        }
        case @string : {
            return v;
        }}

    }

    return new unquotedValue(nil);
}

// indirect walks down v allocating pointers as needed,
// until it gets to a non-pointer.
// If it encounters an Unmarshaler, indirect stops and returns that.
// If decodingNull is true, indirect stops at the first settable pointer so it
// can be set to nil.
internal static (Unmarshaler, encoding.TextUnmarshaler, reflectꓸValue) indirect(reflectꓸValue v, bool decodingNull) {
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
    if (v.Kind() != reflect.ΔPointer && v.Type().Name() != ""u8 && v.CanAddr()) {
        haveAddr = true;
        v = v.Addr();
    }
    while (ᐧ) {
        // Load value from interface, but only if the result will be
        // usefully addressable.
        if (v.Kind() == reflect.ΔInterface && !v.IsNil()) {
            var e = v.Elem();
            if (e.Kind() == reflect.ΔPointer && !e.IsNil() && (!decodingNull || e.Elem().Kind() == reflect.ΔPointer)) {
                haveAddr = false;
                v = e;
                continue;
            }
        }
        if (v.Kind() != reflect.ΔPointer) {
            break;
        }
        if (decodingNull && v.CanSet()) {
            break;
        }
        // Prevent infinite loop if v is an interface pointing to its own address:
        //     var v interface{}
        //     v = &v
        if (v.Elem().Kind() == reflect.ΔInterface && v.Elem().Elem() == v) {
            v = v.Elem();
            break;
        }
        if (v.IsNil()) {
            v.Set(reflect.New(v.Type().Elem()));
        }
        if (v.Type().NumMethod() > 0 && v.CanInterface()) {
            {
                var (u, ok) = v.Interface()._<Unmarshaler>(ᐧ); if (ok) {
                    return (u, default!, new reflectꓸValue(nil));
                }
            }
            if (!decodingNull) {
                {
                    var (u, ok) = v.Interface()._<encoding.TextUnmarshaler>(ᐧ); if (ok) {
                        return (default!, u, new reflectꓸValue(nil));
                    }
                }
            }
        }
        if (haveAddr){
            v = v0;
            // restore original value after round-trip Value.Addr().Elem()
            haveAddr = false;
        } else {
            v = v.Elem();
        }
    }
    return (default!, default!, v);
}

// array consumes an array from d.data[d.off-1:], decoding into v.
// The first byte of the array ('[') has been read already.
[GoRecv] internal static error Δarray(this ref decodeState d, reflectꓸValue v) {
    // Check for unmarshaler.
    var (u, ut, pv) = indirect(v, false);
    if (u != default!) {
        nint start = d.readIndex();
        d.skip();
        return u.UnmarshalJSON(d.data[(int)(start)..(int)(d.off)]);
    }
    if (ut != default!) {
        d.saveError(new UnmarshalTypeError(Value: "array"u8, Type: v.Type(), Offset: ((int64)d.off)));
        d.skip();
        return default!;
    }
    v = pv;
    // Check type of target.
    var exprᴛ1 = v.Kind();
    var matchᴛ1 = false;
    if (exprᴛ1 == reflect.ΔInterface) { matchᴛ1 = true;
        if (v.NumMethod() == 0) {
            // Decoding into nil interface? Switch to non-reflect code.
            var ai = d.arrayInterface();
            v.Set(reflect.ValueOf(ai));
            return default!;
        }
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1) { /* default: */
        d.saveError(new UnmarshalTypeError(Value: "array"u8, Type: v.Type(), Offset: ((int64)d.off)));
        d.skip();
        return default!;
    }
    if (exprᴛ1 == reflect.Array || exprᴛ1 == reflect.ΔSlice) { matchᴛ1 = true;
        break;
    }

    nint i = 0;
    while (ᐧ) {
        // Look ahead for ] - can only happen on first iteration.
        d.scanWhile(scanSkipSpace);
        if (d.opcode == scanEndArray) {
            break;
        }
        // Expand slice length, growing the slice if necessary.
        if (v.Kind() == reflect.ΔSlice) {
            if (i >= v.Cap()) {
                v.Grow(1);
            }
            if (i >= v.Len()) {
                v.SetLen(i + 1);
            }
        }
        if (i < v.Len()){
            // Decode into element.
            {
                var err = d.value(v.Index(i)); if (err != default!) {
                    return err;
                }
            }
        } else {
            // Ran out of fixed array: skip.
            {
                var err = d.value(new reflectꓸValue(nil)); if (err != default!) {
                    return err;
                }
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
            throw panic(phasePanicMsg);
        }
    }
    if (i < v.Len()) {
        if (v.Kind() == reflect.Array){
            for (; i < v.Len(); i++) {
                v.Index(i).SetZero();
            }
        } else {
            // zero remainder of array
            v.SetLen(i);
        }
    }
    // truncate the slice
    if (i == 0 && v.Kind() == reflect.ΔSlice) {
        v.Set(reflect.MakeSlice(v.Type(), 0, 0));
    }
    return default!;
}

internal static slice<byte> nullLiteral = slice<byte>("null");

internal static reflectꓸType textUnmarshalerType = reflect.TypeFor[encoding.TextUnmarshaler]();

// object consumes an object from d.data[d.off-1:], decoding into v.
// The first byte ('{') of the object has been read already.
[GoRecv] internal static error @object(this ref decodeState d, reflectꓸValue v) {
    // Check for unmarshaler.
    var (u, ut, pv) = indirect(v, false);
    if (u != default!) {
        nint start = d.readIndex();
        d.skip();
        return u.UnmarshalJSON(d.data[(int)(start)..(int)(d.off)]);
    }
    if (ut != default!) {
        d.saveError(new UnmarshalTypeError(Value: "object"u8, Type: v.Type(), Offset: ((int64)d.off)));
        d.skip();
        return default!;
    }
    v = pv;
    var t = v.Type();
    // Decoding into nil interface? Switch to non-reflect code.
    if (v.Kind() == reflect.ΔInterface && v.NumMethod() == 0) {
        var oi = d.objectInterface();
        v.Set(reflect.ValueOf(oi));
        return default!;
    }
    structFields fields = default!;
    // Check type of target:
    //   struct or
    //   map[T1]T2 where T1 is string, an integer type,
    //             or an encoding.TextUnmarshaler
    var exprᴛ1 = v.Kind();
    if (exprᴛ1 == reflect.Map) {
        var exprᴛ2 = t.Key().Kind();
        if (exprᴛ2 == reflect.ΔString || exprᴛ2 == reflect.ΔInt || exprᴛ2 == reflect.Int8 || exprᴛ2 == reflect.Int16 || exprᴛ2 == reflect.Int32 || exprᴛ2 == reflect.Int64 || exprᴛ2 == reflect.ΔUint || exprᴛ2 == reflect.Uint8 || exprᴛ2 == reflect.Uint16 || exprᴛ2 == reflect.Uint32 || exprᴛ2 == reflect.Uint64 || exprᴛ2 == reflect.Uintptr) {
        }
        else { /* default: */
            if (!reflect.PointerTo(t.Key()).Implements(textUnmarshalerType)) {
                // Map key must either have string kind, have an integer kind,
                // or be an encoding.TextUnmarshaler.
                d.saveError(new UnmarshalTypeError(Value: "object"u8, Type: t, Offset: ((int64)d.off)));
                d.skip();
                return default!;
            }
        }

        if (v.IsNil()) {
            v.Set(reflect.MakeMap(t));
        }
    }
    else if (exprᴛ1 == reflect.Struct) {
        fields = cachedTypeFields(t);
    }
    else { /* default: */
        d.saveError(new UnmarshalTypeError(Value: "object"u8, Type: t, Offset: ((int64)d.off)));
        d.skip();
        return default!;
    }

    reflectꓸValue mapElem = default!;
    errorContext origErrorContext = default!;
    if (d.errorContext != nil) {
        origErrorContext = d.errorContext.val;
    }
    while (ᐧ) {
        // Read opening " of string key or closing }.
        d.scanWhile(scanSkipSpace);
        if (d.opcode == scanEndObject) {
            // closing } - can only happen on first iteration.
            break;
        }
        if (d.opcode != scanBeginLiteral) {
            throw panic(phasePanicMsg);
        }
        // Read key.
        nint start = d.readIndex();
        d.rescanLiteral();
        var item = d.data[(int)(start)..(int)(d.readIndex())];
        var (key, ok) = unquoteBytes(item);
        if (!ok) {
            throw panic(phasePanicMsg);
        }
        // Figure out field corresponding to key.
        reflectꓸValue subv = default!;
        var destring = false;
        // whether the value is wrapped in a string to be decoded first
        if (v.Kind() == reflect.Map){
            var elemType = t.Elem();
            if (!mapElem.IsValid()){
                mapElem = reflect.New(elemType).Elem();
            } else {
                mapElem.SetZero();
            }
            subv = mapElem;
        } else {
            var f = fields.byExactName[((@string)key)];
            if (f == nil) {
                f = fields.byFoldedName[((@string)foldName(key))];
            }
            if (f != nil){
                subv = v;
                destring = f.val.quoted;
                foreach (var (_, i) in (~f).index) {
                    if (subv.Kind() == reflect.ΔPointer) {
                        if (subv.IsNil()) {
                            // If a struct embeds a pointer to an unexported type,
                            // it is not possible to set a newly allocated value
                            // since the field is unexported.
                            //
                            // See https://golang.org/issue/21357
                            if (!subv.CanSet()) {
                                d.saveError(fmt.Errorf("json: cannot set embedded pointer to unexported struct: %v"u8, subv.Type().Elem()));
                                // Invalidate subv to ensure d.value(subv) skips over
                                // the JSON value without assigning it to subv.
                                subv = new reflectꓸValue(nil);
                                destring = false;
                                break;
                            }
                            subv.Set(reflect.New(subv.Type().Elem()));
                        }
                        subv = subv.Elem();
                    }
                    subv = subv.Field(i);
                }
                if (d.errorContext == nil) {
                    d.errorContext = @new<errorContext>();
                }
                d.errorContext.FieldStack = append(d.errorContext.FieldStack, (~f).name);
                d.errorContext.Struct = t;
            } else 
            if (d.disallowUnknownFields) {
                d.saveError(fmt.Errorf("json: unknown field %q"u8, key));
            }
        }
        // Read : before value.
        if (d.opcode == scanSkipSpace) {
            d.scanWhile(scanSkipSpace);
        }
        if (d.opcode != scanObjectKey) {
            throw panic(phasePanicMsg);
        }
        d.scanWhile(scanSkipSpace);
        if (destring){
            switch (d.valueQuoted().type()) {
            case default! qv: {
                {
                    var err = d.literalStore(nullLiteral, subv, false); if (err != default!) {
                        return err;
                    }
                }
                break;
            }
            case @string qv: {
                {
                    var err = d.literalStore(slice<byte>(qv), subv, true); if (err != default!) {
                        return err;
                    }
                }
                break;
            }
            default: {
                var qv = d.valueQuoted().type();
                d.saveError(fmt.Errorf("json: invalid use of ,string struct tag, trying to unmarshal unquoted value into %v"u8, subv.Type()));
                break;
            }}
        } else {
            {
                var err = d.value(subv); if (err != default!) {
                    return err;
                }
            }
        }
        // Write value back to map;
        // if using struct, subv points into struct already.
        if (v.Kind() == reflect.Map) {
            var kt = t.Key();
            reflectꓸValue kv = default!;
            if (reflect.PointerTo(kt).Implements(textUnmarshalerType)){
                kv = reflect.New(kt);
                {
                    var err = d.literalStore(item, kv, true); if (err != default!) {
                        return err;
                    }
                }
                kv = kv.Elem();
            } else {
                var exprᴛ3 = kt.Kind();
                if (exprᴛ3 == reflect.ΔString) {
                    kv = reflect.New(kt).Elem();
                    kv.SetString(((@string)key));
                }
                else if (exprᴛ3 == reflect.ΔInt || exprᴛ3 == reflect.Int8 || exprᴛ3 == reflect.Int16 || exprᴛ3 == reflect.Int32 || exprᴛ3 == reflect.Int64) {
                    @string s = ((@string)key);
                    var (n, err) = strconv.ParseInt(s, 10, 64);
                    if (err != default! || kt.OverflowInt(n)) {
                        d.saveError(new UnmarshalTypeError(Value: "number "u8 + s, Type: kt, Offset: ((int64)(start + 1))));
                        break;
                    }
                    kv = reflect.New(kt).Elem();
                    kv.SetInt(n);
                }
                else if (exprᴛ3 == reflect.ΔUint || exprᴛ3 == reflect.Uint8 || exprᴛ3 == reflect.Uint16 || exprᴛ3 == reflect.Uint32 || exprᴛ3 == reflect.Uint64 || exprᴛ3 == reflect.Uintptr) {
                    @string s = ((@string)key);
                    var (n, err) = strconv.ParseUint(s, 10, 64);
                    if (err != default! || kt.OverflowUint(n)) {
                        d.saveError(new UnmarshalTypeError(Value: "number "u8 + s, Type: kt, Offset: ((int64)(start + 1))));
                        break;
                    }
                    kv = reflect.New(kt).Elem();
                    kv.SetUint(n);
                }
                else { /* default: */
                    throw panic("json: Unexpected key type");
                }

            }
            // should never occur
            if (kv.IsValid()) {
                v.SetMapIndex(kv, subv);
            }
        }
        // Next token must be , or }.
        if (d.opcode == scanSkipSpace) {
            d.scanWhile(scanSkipSpace);
        }
        if (d.errorContext != nil) {
            // Reset errorContext to its original state.
            // Keep the same underlying array for FieldStack, to reuse the
            // space and avoid unnecessary allocs.
            d.errorContext.FieldStack = d.errorContext.FieldStack[..(int)(len(origErrorContext.FieldStack))];
            d.errorContext.Struct = origErrorContext.Struct;
        }
        if (d.opcode == scanEndObject) {
            break;
        }
        if (d.opcode != scanObjectValue) {
            throw panic(phasePanicMsg);
        }
    }
    return default!;
}

// convertNumber converts the number literal s to a float64 or a Number
// depending on the setting of d.useNumber.
[GoRecv] internal static (any, error) convertNumber(this ref decodeState d, @string s) {
    if (d.useNumber) {
        return (((Number)s), default!);
    }
    var (f, err) = strconv.ParseFloat(s, 64);
    if (err != default!) {
        return (default!, new UnmarshalTypeError(Value: "number "u8 + s, Type: reflect.TypeFor<float64>(), Offset: ((int64)d.off)));
    }
    return (f, default!);
}

internal static reflectꓸType numberType = reflect.TypeFor<Number>();

// literalStore decodes a literal stored in item into v.
//
// fromQuoted indicates whether this literal came from unwrapping a
// string from the ",string" struct tag option. this is used only to
// produce more helpful error messages.
[GoRecv] internal static error literalStore(this ref decodeState d, slice<byte> item, reflectꓸValue v, bool fromQuoted) {
    // Check for unmarshaler.
    if (len(item) == 0) {
        // Empty string given.
        d.saveError(fmt.Errorf("json: invalid use of ,string struct tag, trying to unmarshal %q into %v"u8, item, v.Type()));
        return default!;
    }
    var isNull = item[0] == (rune)'n';
    // null
    var (u, ut, pv) = indirect(v, isNull);
    if (u != default!) {
        return u.UnmarshalJSON(item);
    }
    if (ut != default!) {
        if (item[0] != (rune)'"') {
            if (fromQuoted) {
                d.saveError(fmt.Errorf("json: invalid use of ,string struct tag, trying to unmarshal %q into %v"u8, item, v.Type()));
                return default!;
            }
            @string val = "number"u8;
            switch (item[0]) {
            case (rune)'n': {
                val = "null"u8;
                break;
            }
            case (rune)'t' or (rune)'f': {
                val = "bool"u8;
                break;
            }}

            d.saveError(new UnmarshalTypeError(Value: val, Type: v.Type(), Offset: ((int64)d.readIndex())));
            return default!;
        }
        var (s, ok) = unquoteBytes(item);
        if (!ok) {
            if (fromQuoted) {
                return fmt.Errorf("json: invalid use of ,string struct tag, trying to unmarshal %q into %v"u8, item, v.Type());
            }
            throw panic(phasePanicMsg);
        }
        return ut.UnmarshalText(s);
    }
    v = pv;
    {
        var c = item[0];
        switch (c) {
        case (rune)'n': {
            if (fromQuoted && ((@string)item) != "null"u8) {
                // null
                // The main parser checks that only true and false can reach here,
                // but if this was a quoted string input, it could be anything.
                d.saveError(fmt.Errorf("json: invalid use of ,string struct tag, trying to unmarshal %q into %v"u8, item, v.Type()));
                break;
            }
            var exprᴛ1 = v.Kind();
            if (exprᴛ1 == reflect.ΔInterface || exprᴛ1 == reflect.ΔPointer || exprᴛ1 == reflect.Map || exprᴛ1 == reflect.ΔSlice) {
                v.SetZero();
            }

            break;
        }
        case (rune)'t' or (rune)'f': {
            var value = item[0] == (rune)'t';
            if (fromQuoted && ((@string)item) != "true"u8 && ((@string)item) != "false"u8) {
                // otherwise, ignore null for primitives/string
                // true, false
                // The main parser checks that only true and false can reach here,
                // but if this was a quoted string input, it could be anything.
                d.saveError(fmt.Errorf("json: invalid use of ,string struct tag, trying to unmarshal %q into %v"u8, item, v.Type()));
                break;
            }
            var exprᴛ2 = v.Kind();
            { /* default: */
                if (fromQuoted){
                    d.saveError(fmt.Errorf("json: invalid use of ,string struct tag, trying to unmarshal %q into %v"u8, item, v.Type()));
                } else {
                    d.saveError(new UnmarshalTypeError(Value: "bool"u8, Type: v.Type(), Offset: ((int64)d.readIndex())));
                }
            }
            else if (exprᴛ2 == reflect.ΔBool) {
                v.SetBool(value);
            }
            else if (exprᴛ2 == reflect.ΔInterface) {
                if (v.NumMethod() == 0){
                    v.Set(reflect.ValueOf(value));
                } else {
                    d.saveError(new UnmarshalTypeError(Value: "bool"u8, Type: v.Type(), Offset: ((int64)d.readIndex())));
                }
            }

            break;
        }
        case (rune)'"': {
            var (s, ok) = unquoteBytes(item);
            if (!ok) {
                // string
                if (fromQuoted) {
                    return fmt.Errorf("json: invalid use of ,string struct tag, trying to unmarshal %q into %v"u8, item, v.Type());
                }
                throw panic(phasePanicMsg);
            }
            var exprᴛ3 = v.Kind();
            { /* default: */
                d.saveError(new UnmarshalTypeError(Value: "string"u8, Type: v.Type(), Offset: ((int64)d.readIndex())));
            }
            else if (exprᴛ3 == reflect.ΔSlice) {
                if (v.Type().Elem().Kind() != reflect.Uint8) {
                    d.saveError(new UnmarshalTypeError(Value: "string"u8, Type: v.Type(), Offset: ((int64)d.readIndex())));
                    break;
                }
                var b = new slice<byte>(base64.StdEncoding.DecodedLen(len(s)));
                var (n, err) = base64.StdEncoding.Decode(b, s);
                if (err != default!) {
                    d.saveError(err);
                    break;
                }
                v.SetBytes(b[..(int)(n)]);
            }
            else if (exprᴛ3 == reflect.ΔString) {
                @string t = ((@string)s);
                if (AreEqual(v.Type(), numberType) && !isValidNumber(t)) {
                    return fmt.Errorf("json: invalid number literal, trying to unmarshal %q into Number"u8, item);
                }
                v.SetString(t);
            }
            else if (exprᴛ3 == reflect.ΔInterface) {
                if (v.NumMethod() == 0){
                    v.Set(reflect.ValueOf(((@string)s)));
                } else {
                    d.saveError(new UnmarshalTypeError(Value: "string"u8, Type: v.Type(), Offset: ((int64)d.readIndex())));
                }
            }

            break;
        }
        default: {
            if (c != (rune)'-' && (c < (rune)'0' || c > (rune)'9')) {
                // number
                if (fromQuoted) {
                    return fmt.Errorf("json: invalid use of ,string struct tag, trying to unmarshal %q into %v"u8, item, v.Type());
                }
                throw panic(phasePanicMsg);
            }
            var exprᴛ4 = v.Kind();
            { /* default: */
                if (v.Kind() == reflect.ΔString && AreEqual(v.Type(), numberType)) {
                    // s must be a valid number, because it's
                    // already been tokenized.
                    v.SetString(((@string)item));
                    break;
                }
                if (fromQuoted) {
                    return fmt.Errorf("json: invalid use of ,string struct tag, trying to unmarshal %q into %v"u8, item, v.Type());
                }
                d.saveError(new UnmarshalTypeError(Value: "number"u8, Type: v.Type(), Offset: ((int64)d.readIndex())));
            }
            else if (exprᴛ4 == reflect.ΔInterface) {
                (n, err) = d.convertNumber(((@string)item));
                if (err != default!) {
                    d.saveError(err);
                    break;
                }
                if (v.NumMethod() != 0) {
                    d.saveError(new UnmarshalTypeError(Value: "number"u8, Type: v.Type(), Offset: ((int64)d.readIndex())));
                    break;
                }
                v.Set(reflect.ValueOf(n));
            }
            else if (exprᴛ4 == reflect.ΔInt || exprᴛ4 == reflect.Int8 || exprᴛ4 == reflect.Int16 || exprᴛ4 == reflect.Int32 || exprᴛ4 == reflect.Int64) {
                var (n, err) = strconv.ParseInt(((@string)item), 10, 64);
                if (err != default! || v.OverflowInt(n)) {
                    d.saveError(new UnmarshalTypeError(Value: "number "u8 + ((@string)item), Type: v.Type(), Offset: ((int64)d.readIndex())));
                    break;
                }
                v.SetInt(n);
            }
            else if (exprᴛ4 == reflect.ΔUint || exprᴛ4 == reflect.Uint8 || exprᴛ4 == reflect.Uint16 || exprᴛ4 == reflect.Uint32 || exprᴛ4 == reflect.Uint64 || exprᴛ4 == reflect.Uintptr) {
                var (n, err) = strconv.ParseUint(((@string)item), 10, 64);
                if (err != default! || v.OverflowUint(n)) {
                    d.saveError(new UnmarshalTypeError(Value: "number "u8 + ((@string)item), Type: v.Type(), Offset: ((int64)d.readIndex())));
                    break;
                }
                v.SetUint(n);
            }
            else if (exprᴛ4 == reflect.Float32 || exprᴛ4 == reflect.Float64) {
                var (n, err) = strconv.ParseFloat(((@string)item), v.Type().Bits());
                if (err != default! || v.OverflowFloat(n)) {
                    d.saveError(new UnmarshalTypeError(Value: "number "u8 + ((@string)item), Type: v.Type(), Offset: ((int64)d.readIndex())));
                    break;
                }
                v.SetFloat(n);
            }

            break;
        }}
    }

    return default!;
}

// The xxxInterface routines build up a value to be stored
// in an empty interface. They are not strictly necessary,
// but they avoid the weight of reflection in this common case.

// valueInterface is like value but returns interface{}
[GoRecv] internal static any /*val*/ valueInterface(this ref decodeState d) {
    any val = default!;

    var exprᴛ1 = d.opcode;
    { /* default: */
        throw panic(phasePanicMsg);
    }
    else if (exprᴛ1 == scanBeginArray) {
        val = d.arrayInterface();
        d.scanNext();
    }
    else if (exprᴛ1 == scanBeginObject) {
        val = d.objectInterface();
        d.scanNext();
    }
    else if (exprᴛ1 == scanBeginLiteral) {
        val = d.literalInterface();
    }

    return val;
}

// arrayInterface is like array but returns []interface{}.
[GoRecv] internal static slice<any> arrayInterface(this ref decodeState d) {
    slice<any> v = new slice<any>(0);
    while (ᐧ) {
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
            throw panic(phasePanicMsg);
        }
    }
    return v;
}

// objectInterface is like object but returns map[string]interface{}.
[GoRecv] internal static map<@string, any> objectInterface(this ref decodeState d) {
    var m = new map<@string, any>();
    while (ᐧ) {
        // Read opening " of string key or closing }.
        d.scanWhile(scanSkipSpace);
        if (d.opcode == scanEndObject) {
            // closing } - can only happen on first iteration.
            break;
        }
        if (d.opcode != scanBeginLiteral) {
            throw panic(phasePanicMsg);
        }
        // Read string key.
        nint start = d.readIndex();
        d.rescanLiteral();
        var item = d.data[(int)(start)..(int)(d.readIndex())];
        var (key, ok) = unquote(item);
        if (!ok) {
            throw panic(phasePanicMsg);
        }
        // Read : before value.
        if (d.opcode == scanSkipSpace) {
            d.scanWhile(scanSkipSpace);
        }
        if (d.opcode != scanObjectKey) {
            throw panic(phasePanicMsg);
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
            throw panic(phasePanicMsg);
        }
    }
    return m;
}

// literalInterface consumes and returns a literal from d.data[d.off-1:] and
// it reads the following byte ahead. The first byte of the literal has been
// read already (that's how the caller knows it's a literal).
[GoRecv] internal static any literalInterface(this ref decodeState d) {
    // All bytes inside literal return scanContinue op code.
    nint start = d.readIndex();
    d.rescanLiteral();
    var item = d.data[(int)(start)..(int)(d.readIndex())];
    {
        var c = item[0];
        switch (c) {
        case (rune)'n': {
            return default!;
        }
        case (rune)'t' or (rune)'f': {
            return c == (rune)'t';
        }
        case (rune)'"': {
            var (s, ok) = unquote(item);
            if (!ok) {
                // null
                // true, false
                // string
                throw panic(phasePanicMsg);
            }
            return s;
        }
        default: {
            if (c != (rune)'-' && (c < (rune)'0' || c > (rune)'9')) {
                // number
                throw panic(phasePanicMsg);
            }
            (n, err) = d.convertNumber(((@string)item));
            if (err != default!) {
                d.saveError(err);
            }
            return n;
        }}
    }

}

// getu4 decodes \uXXXX from the beginning of s, returning the hex value,
// or it returns -1.
internal static rune getu4(slice<byte> s) {
    if (len(s) < 6 || s[0] != (rune)'\\' || s[1] != (rune)'u') {
        return -1;
    }
    rune r = default!;
    foreach (var (_, c) in s[2..6]) {
        switch (ᐧ) {
        case {} when (rune)'0' <= c && c <= (rune)'9': {
            c = c - (rune)'0';
            break;
        }
        case {} when (rune)'a' <= c && c <= (rune)'f': {
            c = c - (rune)'a' + 10;
            break;
        }
        case {} when (rune)'A' <= c && c <= (rune)'F': {
            c = c - (rune)'A' + 10;
            break;
        }
        default: {
            return -1;
        }}

        r = r * 16 + ((rune)c);
    }
    return r;
}

// unquote converts a quoted JSON string literal s into an actual string t.
// The rules are different than for Go, so cannot use strconv.Unquote.
internal static (@string t, bool ok) unquote(slice<byte> s) {
    @string t = default!;
    bool ok = default!;

    (s, ok) = unquoteBytes(s);
    t = ((@string)s);
    return (t, ok);
}

// unquoteBytes should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/bytedance/sonic
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname unquoteBytes
internal static (slice<byte> t, bool ok) unquoteBytes(slice<byte> s) {
    slice<byte> t = default!;
    bool ok = default!;

    if (len(s) < 2 || s[0] != (rune)'"' || s[len(s) - 1] != (rune)'"') {
        return (t, ok);
    }
    s = s[1..(int)(len(s) - 1)];
    // Check for unusual characters. If there are none,
    // then no unquoting is needed, so return a slice of the
    // original bytes.
    nint r = 0;
    while (r < len(s)) {
        var c = s[r];
        if (c == (rune)'\\' || c == (rune)'"' || c < (rune)' ') {
            break;
        }
        if (c < utf8.RuneSelf) {
            r++;
            continue;
        }
        var (rr, size) = utf8.DecodeRune(s[(int)(r)..]);
        if (rr == utf8.RuneError && size == 1) {
            break;
        }
        r += size;
    }
    if (r == len(s)) {
        return (s, true);
    }
    var b = new slice<byte>(len(s) + 2 * utf8.UTFMax);
    nint w = copy(b, s[0..(int)(r)]);
    while (r < len(s)) {
        // Out of room? Can only happen if s is full of
        // malformed UTF-8 and we're replacing each
        // byte with RuneError.
        if (w >= len(b) - 2 * utf8.UTFMax) {
            var nb = new slice<byte>((len(b) + utf8.UTFMax) * 2);
            copy(nb, b[0..(int)(w)]);
            b = nb;
        }
        {
            var c = s[r];
            switch (ᐧ) {
            case {} when c is (rune)'\\': {
                r++;
                if (r >= len(s)) {
                    return (t, ok);
                }
                switch (s[r]) {
                default: {
                    return (t, ok);
                }
                case (rune)'"' or (rune)'\\' or (rune)'/' or (rune)'\'': {
                    b[w] = s[r];
                    r++;
                    w++;
                    break;
                }
                case (rune)'b': {
                    b[w] = (rune)'\b';
                    r++;
                    w++;
                    break;
                }
                case (rune)'f': {
                    b[w] = (rune)'\f';
                    r++;
                    w++;
                    break;
                }
                case (rune)'n': {
                    b[w] = (rune)'\n';
                    r++;
                    w++;
                    break;
                }
                case (rune)'r': {
                    b[w] = (rune)'\r';
                    r++;
                    w++;
                    break;
                }
                case (rune)'t': {
                    b[w] = (rune)'\t';
                    r++;
                    w++;
                    break;
                }
                case (rune)'u': {
                    r--;
                    var rr = getu4(s[(int)(r)..]);
                    if (rr < 0) {
                        return (t, ok);
                    }
                    r += 6;
                    if (utf16.IsSurrogate(rr)) {
                        var rr1 = getu4(s[(int)(r)..]);
                        {
                            var dec = utf16.DecodeRune(rr, rr1); if (dec != unicode.ReplacementChar) {
                                // A valid pair; consume.
                                r += 6;
                                w += utf8.EncodeRune(b[(int)(w)..], dec);
                                break;
                            }
                        }
                        // Invalid surrogate; fall back to replacement rune.
                        rr = unicode.ReplacementChar;
                    }
                    w += utf8.EncodeRune(b[(int)(w)..], rr);
                    break;
                }}

                break;
            }
            case {} when c is (rune)'"' or < (rune)' ': {
                return (t, ok);
            }
            case {} when c is < utf8.RuneSelf: {
                b[w] = c;
                r++;
                w++;
                break;
            }
            default: {
                var (rr, size) = utf8.DecodeRune(s[(int)(r)..]);
                r += size;
                w += utf8.EncodeRune(b[(int)(w)..], // Quote, control characters are invalid.
 // ASCII
 // Coerce to well-formed UTF-8.
 rr);
                break;
            }}
        }

    }
    return (b[0..(int)(w)], true);
}

} // end json_package
