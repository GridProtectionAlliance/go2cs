// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Represents JSON data structure using native Go types: booleans, floats,
// strings, arrays, and maps.

// package json -- go2cs converted at 2020 August 29 08:35:12 UTC
// import "encoding/json" ==> using json = go.encoding.json_package
// Original source: C:\Go\src\encoding\json\decode.go
using bytes = go.bytes_package;
using encoding = go.encoding_package;
using base64 = go.encoding.base64_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using reflect = go.reflect_package;
using runtime = go.runtime_package;
using strconv = go.strconv_package;
using unicode = go.unicode_package;
using utf16 = go.unicode.utf16_package;
using utf8 = go.unicode.utf8_package;
using static go.builtin;
using System;

namespace go {
namespace encoding
{
    public static partial class json_package
    {
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
        // either be a string, an integer, or implement encoding.TextUnmarshaler.
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
        public static error Unmarshal(slice<byte> data, object v)
        { 
            // Check for well-formedness.
            // Avoids filling out half a data structure
            // before discovering a JSON syntax error.
            decodeState d = default;
            var err = checkValid(data, ref d.scan);
            if (err != null)
            {
                return error.As(err);
            }
            d.init(data);
            return error.As(d.unmarshal(v));
        }

        // Unmarshaler is the interface implemented by types
        // that can unmarshal a JSON description of themselves.
        // The input can be assumed to be a valid encoding of
        // a JSON value. UnmarshalJSON must copy the JSON data
        // if it wishes to retain the data after returning.
        //
        // By convention, to approximate the behavior of Unmarshal itself,
        // Unmarshalers implement UnmarshalJSON([]byte("null")) as a no-op.
        public partial interface Unmarshaler
        {
            error UnmarshalJSON(slice<byte> _p0);
        }

        // An UnmarshalTypeError describes a JSON value that was
        // not appropriate for a value of a specific Go type.
        public partial struct UnmarshalTypeError
        {
            public @string Value; // description of JSON value - "bool", "array", "number -5"
            public reflect.Type Type; // type of Go value it could not be assigned to
            public long Offset; // error occurred after reading Offset bytes
            public @string Struct; // name of the struct type containing the field
            public @string Field; // name of the field holding the Go value
        }

        private static @string Error(this ref UnmarshalTypeError e)
        {
            if (e.Struct != "" || e.Field != "")
            {
                return "json: cannot unmarshal " + e.Value + " into Go struct field " + e.Struct + "." + e.Field + " of type " + e.Type.String();
            }
            return "json: cannot unmarshal " + e.Value + " into Go value of type " + e.Type.String();
        }

        // An UnmarshalFieldError describes a JSON object key that
        // led to an unexported (and therefore unwritable) struct field.
        //
        // Deprecated: No longer used; kept for compatibility.
        public partial struct UnmarshalFieldError
        {
            public @string Key;
            public reflect.Type Type;
            public reflect.StructField Field;
        }

        private static @string Error(this ref UnmarshalFieldError e)
        {
            return "json: cannot unmarshal object key " + strconv.Quote(e.Key) + " into unexported field " + e.Field.Name + " of type " + e.Type.String();
        }

        // An InvalidUnmarshalError describes an invalid argument passed to Unmarshal.
        // (The argument to Unmarshal must be a non-nil pointer.)
        public partial struct InvalidUnmarshalError
        {
            public reflect.Type Type;
        }

        private static @string Error(this ref InvalidUnmarshalError e)
        {
            if (e.Type == null)
            {
                return "json: Unmarshal(nil)";
            }
            if (e.Type.Kind() != reflect.Ptr)
            {
                return "json: Unmarshal(non-pointer " + e.Type.String() + ")";
            }
            return "json: Unmarshal(nil " + e.Type.String() + ")";
        }

        private static error unmarshal(this ref decodeState _d, object v) => func(_d, (ref decodeState d, Defer defer, Panic panic, Recover _) =>
        {
            defer(() =>
            {
                {
                    var r = recover();

                    if (r != null)
                    {
                        {
                            runtime.Error (_, ok) = r._<runtime.Error>();

                            if (ok)
                            {
                                panic(r);
                            }

                        }
                        err = r._<error>();
                    }

                }
            }());

            var rv = reflect.ValueOf(v);
            if (rv.Kind() != reflect.Ptr || rv.IsNil())
            {
                return error.As(ref new InvalidUnmarshalError(reflect.TypeOf(v)));
            }
            d.scan.reset(); 
            // We decode rv not rv.Elem because the Unmarshaler interface
            // test must be applied at the top level of the value.
            d.value(rv);
            return error.As(d.savedError);
        });

        // A Number represents a JSON number literal.
        public partial struct Number // : @string
        {
        }

        // String returns the literal text of the number.
        public static @string String(this Number n)
        {
            return string(n);
        }

        // Float64 returns the number as a float64.
        public static (double, error) Float64(this Number n)
        {
            return strconv.ParseFloat(string(n), 64L);
        }

        // Int64 returns the number as an int64.
        public static (long, error) Int64(this Number n)
        {
            return strconv.ParseInt(string(n), 10L, 64L);
        }

        // isValidNumber reports whether s is a valid JSON number literal.
        private static bool isValidNumber(@string s)
        { 
            // This function implements the JSON numbers grammar.
            // See https://tools.ietf.org/html/rfc7159#section-6
            // and http://json.org/number.gif

            if (s == "")
            {
                return false;
            } 

            // Optional -
            if (s[0L] == '-')
            {
                s = s[1L..];
                if (s == "")
                {
                    return false;
                }
            } 

            // Digits

            if (s[0L] == '0') 
                s = s[1L..];
            else if ('1' <= s[0L] && s[0L] <= '9') 
                s = s[1L..];
                while (len(s) > 0L && '0' <= s[0L] && s[0L] <= '9')
                {
                    s = s[1L..];
                }
            else 
                return false;
            // . followed by 1 or more digits.
            if (len(s) >= 2L && s[0L] == '.' && '0' <= s[1L] && s[1L] <= '9')
            {
                s = s[2L..];
                while (len(s) > 0L && '0' <= s[0L] && s[0L] <= '9')
                {
                    s = s[1L..];
                }

            } 

            // e or E followed by an optional - or + and
            // 1 or more digits.
            if (len(s) >= 2L && (s[0L] == 'e' || s[0L] == 'E'))
            {
                s = s[1L..];
                if (s[0L] == '+' || s[0L] == '-')
                {
                    s = s[1L..];
                    if (s == "")
                    {
                        return false;
                    }
                }
                while (len(s) > 0L && '0' <= s[0L] && s[0L] <= '9')
                {
                    s = s[1L..];
                }

            } 

            // Make sure we are at the end.
            return s == "";
        }

        // decodeState represents the state while decoding a JSON value.
        private partial struct decodeState
        {
            public slice<byte> data;
            public long off; // read offset in data
            public scanner scan;
            public scanner nextscan; // for calls to nextValue
            public error savedError;
            public bool useNumber;
            public bool disallowUnknownFields;
        }

        // errPhase is used for errors that should not happen unless
        // there is a bug in the JSON decoder or something is editing
        // the data slice while the decoder executes.
        private static var errPhase = errors.New("JSON decoder out of sync - data changing underfoot?");

        private static ref decodeState init(this ref decodeState d, slice<byte> data)
        {
            d.data = data;
            d.off = 0L;
            d.savedError = null;
            d.errorContext.Struct = "";
            d.errorContext.Field = "";
            return d;
        }

        // error aborts the decoding by panicking with err.
        private static void error(this ref decodeState _d, error err) => func(_d, (ref decodeState d, Defer _, Panic panic, Recover __) =>
        {
            panic(d.addErrorContext(err));
        });

        // saveError saves the first err it is called with,
        // for reporting at the end of the unmarshal.
        private static void saveError(this ref decodeState d, error err)
        {
            if (d.savedError == null)
            {
                d.savedError = d.addErrorContext(err);
            }
        }

        // addErrorContext returns a new error enhanced with information from d.errorContext
        private static error addErrorContext(this ref decodeState d, error err)
        {
            if (d.errorContext.Struct != "" || d.errorContext.Field != "")
            {
                switch (err.type())
                {
                    case ref UnmarshalTypeError err:
                        err.Struct = d.errorContext.Struct;
                        err.Field = d.errorContext.Field;
                        return error.As(err);
                        break;
                }
            }
            return error.As(err);
        }

        // next cuts off and returns the next full JSON value in d.data[d.off:].
        // The next value is known to be an object or array, not a literal.
        private static slice<byte> next(this ref decodeState d)
        {
            var c = d.data[d.off];
            var (item, rest, err) = nextValue(d.data[d.off..], ref d.nextscan);
            if (err != null)
            {
                d.error(err);
            }
            d.off = len(d.data) - len(rest); 

            // Our scanner has seen the opening brace/bracket
            // and thinks we're still in the middle of the object.
            // invent a closing brace/bracket to get it out.
            if (c == '{')
            {
                d.scan.step(ref d.scan, '}');
            }
            else
            {
                d.scan.step(ref d.scan, ']');
            }
            return item;
        }

        // scanWhile processes bytes in d.data[d.off:] until it
        // receives a scan code not equal to op.
        // It updates d.off and returns the new scan code.
        private static long scanWhile(this ref decodeState d, long op)
        {
            long newOp = default;
            while (true)
            {
                if (d.off >= len(d.data))
                {
                    newOp = d.scan.eof();
                    d.off = len(d.data) + 1L; // mark processed EOF with len+1
                }
                else
                {
                    var c = d.data[d.off];
                    d.off++;
                    newOp = d.scan.step(ref d.scan, c);
                }
                if (newOp != op)
                {
                    break;
                }
            }

            return newOp;
        }

        // value decodes a JSON value from d.data[d.off:] into the value.
        // it updates d.off to point past the decoded value.
        private static void value(this ref decodeState d, reflect.Value v)
        {
            if (!v.IsValid())
            {
                var (_, rest, err) = nextValue(d.data[d.off..], ref d.nextscan);
                if (err != null)
                {
                    d.error(err);
                }
                d.off = len(d.data) - len(rest); 

                // d.scan thinks we're still at the beginning of the item.
                // Feed in an empty string - the shortest, simplest value -
                // so that it knows we got to the end of the value.
                if (d.scan.redo)
                { 
                    // rewind.
                    d.scan.redo = false;
                    d.scan.step = stateBeginValue;
                }
                d.scan.step(ref d.scan, '"');
                d.scan.step(ref d.scan, '"');

                var n = len(d.scan.parseState);
                if (n > 0L && d.scan.parseState[n - 1L] == parseObjectKey)
                { 
                    // d.scan thinks we just read an object key; finish the object
                    d.scan.step(ref d.scan, ':');
                    d.scan.step(ref d.scan, '"');
                    d.scan.step(ref d.scan, '"');
                    d.scan.step(ref d.scan, '}');
                }
                return;
            }
            {
                var op = d.scanWhile(scanSkipSpace);


                if (op == scanBeginArray) 
                    d.array(v);
                else if (op == scanBeginObject) 
                    d.@object(v);
                else if (op == scanBeginLiteral) 
                    d.literal(v);
                else 
                    d.error(errPhase);

            }
        }

        private partial struct unquotedValue
        {
        }

        // valueQuoted is like value but decodes a
        // quoted string literal or literal null into an interface value.
        // If it finds anything other than a quoted string literal or null,
        // valueQuoted returns unquotedValue{}.
        private static void valueQuoted(this ref decodeState d)
        {
            {
                var op = d.scanWhile(scanSkipSpace);


                if (op == scanBeginArray) 
                    d.array(new reflect.Value());
                else if (op == scanBeginObject) 
                    d.@object(new reflect.Value());
                else if (op == scanBeginLiteral) 
                    switch (d.literalInterface().type())
                    {
                        case @string v:
                            return v;
                            break;
                    }
                else 
                    d.error(errPhase);

            }
            return new unquotedValue();
        }

        // indirect walks down v allocating pointers as needed,
        // until it gets to a non-pointer.
        // if it encounters an Unmarshaler, indirect stops and returns that.
        // if decodingNull is true, indirect stops at the last pointer so it can be set to nil.
        private static (Unmarshaler, encoding.TextUnmarshaler, reflect.Value) indirect(this ref decodeState d, reflect.Value v, bool decodingNull)
        { 
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
            if (v.Kind() != reflect.Ptr && v.Type().Name() != "" && v.CanAddr())
            {
                haveAddr = true;
                v = v.Addr();
            }
            while (true)
            { 
                // Load value from interface, but only if the result will be
                // usefully addressable.
                if (v.Kind() == reflect.Interface && !v.IsNil())
                {
                    var e = v.Elem();
                    if (e.Kind() == reflect.Ptr && !e.IsNil() && (!decodingNull || e.Elem().Kind() == reflect.Ptr))
                    {
                        haveAddr = false;
                        v = e;
                        continue;
                    }
                }
                if (v.Kind() != reflect.Ptr)
                {
                    break;
                }
                if (v.Elem().Kind() != reflect.Ptr && decodingNull && v.CanSet())
                {
                    break;
                }
                if (v.IsNil())
                {
                    v.Set(reflect.New(v.Type().Elem()));
                }
                if (v.Type().NumMethod() > 0L)
                {
                    {
                        Unmarshaler u__prev2 = u;

                        Unmarshaler (u, ok) = v.Interface()._<Unmarshaler>();

                        if (ok)
                        {
                            return (u, null, new reflect.Value());
                        }

                        u = u__prev2;

                    }
                    if (!decodingNull)
                    {
                        {
                            Unmarshaler u__prev3 = u;

                            (u, ok) = v.Interface()._<encoding.TextUnmarshaler>();

                            if (ok)
                            {
                                return (null, u, new reflect.Value());
                            }

                            u = u__prev3;

                        }
                    }
                }
                if (haveAddr)
                {
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

        // array consumes an array from d.data[d.off-1:], decoding into the value v.
        // the first byte of the array ('[') has been read already.
        private static void array(this ref decodeState d, reflect.Value v)
        { 
            // Check for unmarshaler.
            var (u, ut, pv) = d.indirect(v, false);
            if (u != null)
            {
                d.off--;
                var err = u.UnmarshalJSON(d.next());
                if (err != null)
                {
                    d.error(err);
                }
                return;
            }
            if (ut != null)
            {
                d.saveError(ref new UnmarshalTypeError(Value:"array",Type:v.Type(),Offset:int64(d.off)));
                d.off--;
                d.next();
                return;
            }
            v = pv; 

            // Check type of target.

            if (v.Kind() == reflect.Interface)
            {
                if (v.NumMethod() == 0L)
                { 
                    // Decoding into nil interface? Switch to non-reflect code.
                    v.Set(reflect.ValueOf(d.arrayInterface()));
                    return;
                } 
                // Otherwise it's invalid.
                fallthrough = true;
            }
            if (fallthrough || v.Kind() == reflect.Array)
            {
                goto __switch_break0;
            }
            if (v.Kind() == reflect.Slice)
            {
                break;
                goto __switch_break0;
            }
            // default: 
                d.saveError(ref new UnmarshalTypeError(Value:"array",Type:v.Type(),Offset:int64(d.off)));
                d.off--;
                d.next();
                return;

            __switch_break0:;

            long i = 0L;
            while (true)
            { 
                // Look ahead for ] - can only happen on first iteration.
                var op = d.scanWhile(scanSkipSpace);
                if (op == scanEndArray)
                {
                    break;
                } 

                // Back up so d.value can have the byte we just read.
                d.off--;
                d.scan.undo(op); 

                // Get element of array, growing if necessary.
                if (v.Kind() == reflect.Slice)
                { 
                    // Grow slice if necessary
                    if (i >= v.Cap())
                    {
                        var newcap = v.Cap() + v.Cap() / 2L;
                        if (newcap < 4L)
                        {
                            newcap = 4L;
                        }
                        var newv = reflect.MakeSlice(v.Type(), v.Len(), newcap);
                        reflect.Copy(newv, v);
                        v.Set(newv);
                    }
                    if (i >= v.Len())
                    {
                        v.SetLen(i + 1L);
                    }
                }
                if (i < v.Len())
                { 
                    // Decode into element.
                    d.value(v.Index(i));
                }
                else
                { 
                    // Ran out of fixed array: skip.
                    d.value(new reflect.Value());
                }
                i++; 

                // Next token must be , or ].
                op = d.scanWhile(scanSkipSpace);
                if (op == scanEndArray)
                {
                    break;
                }
                if (op != scanArrayValue)
                {
                    d.error(errPhase);
                }
            }


            if (i < v.Len())
            {
                if (v.Kind() == reflect.Array)
                { 
                    // Array. Zero the rest.
                    var z = reflect.Zero(v.Type().Elem());
                    while (i < v.Len())
                    {
                        v.Index(i).Set(z);
                        i++;
                    }
                else

                }                {
                    v.SetLen(i);
                }
            }
            if (i == 0L && v.Kind() == reflect.Slice)
            {
                v.Set(reflect.MakeSlice(v.Type(), 0L, 0L));
            }
        }

        private static slice<byte> nullLiteral = (slice<byte>)"null";
        private static var textUnmarshalerType = reflect.TypeOf(@new<encoding.TextUnmarshaler>()).Elem();

        // object consumes an object from d.data[d.off-1:], decoding into the value v.
        // the first byte ('{') of the object has been read already.
        private static void @object(this ref decodeState _d, reflect.Value v) => func(_d, (ref decodeState d, Defer _, Panic panic, Recover __) =>
        { 
            // Check for unmarshaler.
            var (u, ut, pv) = d.indirect(v, false);
            if (u != null)
            {
                d.off--;
                var err = u.UnmarshalJSON(d.next());
                if (err != null)
                {
                    d.error(err);
                }
                return;
            }
            if (ut != null)
            {
                d.saveError(ref new UnmarshalTypeError(Value:"object",Type:v.Type(),Offset:int64(d.off)));
                d.off--;
                d.next(); // skip over { } in input
                return;
            }
            v = pv; 

            // Decoding into nil interface? Switch to non-reflect code.
            if (v.Kind() == reflect.Interface && v.NumMethod() == 0L)
            {
                v.Set(reflect.ValueOf(d.objectInterface()));
                return;
            } 

            // Check type of target:
            //   struct or
            //   map[T1]T2 where T1 is string, an integer type,
            //             or an encoding.TextUnmarshaler

            if (v.Kind() == reflect.Map) 
                // Map key must either have string kind, have an integer kind,
                // or be an encoding.TextUnmarshaler.
                var t = v.Type();

                if (t.Key().Kind() == reflect.String || t.Key().Kind() == reflect.Int || t.Key().Kind() == reflect.Int8 || t.Key().Kind() == reflect.Int16 || t.Key().Kind() == reflect.Int32 || t.Key().Kind() == reflect.Int64 || t.Key().Kind() == reflect.Uint || t.Key().Kind() == reflect.Uint8 || t.Key().Kind() == reflect.Uint16 || t.Key().Kind() == reflect.Uint32 || t.Key().Kind() == reflect.Uint64 || t.Key().Kind() == reflect.Uintptr)                 else 
                    if (!reflect.PtrTo(t.Key()).Implements(textUnmarshalerType))
                    {
                        d.saveError(ref new UnmarshalTypeError(Value:"object",Type:v.Type(),Offset:int64(d.off)));
                        d.off--;
                        d.next(); // skip over { } in input
                        return;
                    }
                                if (v.IsNil())
                {
                    v.Set(reflect.MakeMap(t));
                }
            else if (v.Kind() == reflect.Struct)             else 
                d.saveError(ref new UnmarshalTypeError(Value:"object",Type:v.Type(),Offset:int64(d.off)));
                d.off--;
                d.next(); // skip over { } in input
                return;
                        reflect.Value mapElem = default;

            while (true)
            { 
                // Read opening " of string key or closing }.
                var op = d.scanWhile(scanSkipSpace);
                if (op == scanEndObject)
                { 
                    // closing } - can only happen on first iteration.
                    break;
                }
                if (op != scanBeginLiteral)
                {
                    d.error(errPhase);
                } 

                // Read key.
                var start = d.off - 1L;
                op = d.scanWhile(scanContinue);
                var item = d.data[start..d.off - 1L];
                var (key, ok) = unquoteBytes(item);
                if (!ok)
                {
                    d.error(errPhase);
                } 

                // Figure out field corresponding to key.
                reflect.Value subv = default;
                var destring = false; // whether the value is wrapped in a string to be decoded first

                if (v.Kind() == reflect.Map)
                {
                    var elemType = v.Type().Elem();
                    if (!mapElem.IsValid())
                    {
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
                    ref field f = default;
                    var fields = cachedTypeFields(v.Type());
                    {
                        var i__prev2 = i;

                        foreach (var (__i) in fields)
                        {
                            i = __i;
                            var ff = ref fields[i];
                            if (bytes.Equal(ff.nameBytes, key))
                            {
                                f = ff;
                                break;
                            }
                            if (f == null && ff.equalFold(ff.nameBytes, key))
                            {
                                f = ff;
                            }
                        }

                        i = i__prev2;
                    }

                    if (f != null)
                    {
                        subv = v;
                        destring = f.quoted;
                        {
                            var i__prev2 = i;

                            foreach (var (_, __i) in f.index)
                            {
                                i = __i;
                                if (subv.Kind() == reflect.Ptr)
                                {
                                    if (subv.IsNil())
                                    { 
                                        // If a struct embeds a pointer to an unexported type,
                                        // it is not possible to set a newly allocated value
                                        // since the field is unexported.
                                        //
                                        // See https://golang.org/issue/21357
                                        if (!subv.CanSet())
                                        {
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

                        d.errorContext.Field = f.name;
                        d.errorContext.Struct = v.Type().Name();
                    }
                    else if (d.disallowUnknownFields)
                    {
                        d.saveError(fmt.Errorf("json: unknown field %q", key));
                    }
                } 

                // Read : before value.
                if (op == scanSkipSpace)
                {
                    op = d.scanWhile(scanSkipSpace);
                }
                if (op != scanObjectKey)
                {
                    d.error(errPhase);
                }
                if (destring)
                {
                    switch (d.valueQuoted().type())
                    {
                        case 
                            d.literalStore(nullLiteral, subv, false);
                            break;
                        case @string qv:
                            d.literalStore((slice<byte>)qv, subv, true);
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
                    d.value(subv);
                } 

                // Write value back to map;
                // if using struct, subv points into struct already.
                if (v.Kind() == reflect.Map)
                {
                    var kt = v.Type().Key();
                    reflect.Value kv = default;

                    if (kt.Kind() == reflect.String) 
                        kv = reflect.ValueOf(key).Convert(kt);
                    else if (reflect.PtrTo(kt).Implements(textUnmarshalerType)) 
                        kv = reflect.New(v.Type().Key());
                        d.literalStore(item, kv, true);
                        kv = kv.Elem();
                    else 

                        if (kt.Kind() == reflect.Int || kt.Kind() == reflect.Int8 || kt.Kind() == reflect.Int16 || kt.Kind() == reflect.Int32 || kt.Kind() == reflect.Int64) 
                            var s = string(key);
                            var (n, err) = strconv.ParseInt(s, 10L, 64L);
                            if (err != null || reflect.Zero(kt).OverflowInt(n))
                            {
                                d.saveError(ref new UnmarshalTypeError(Value:"number "+s,Type:kt,Offset:int64(start+1)));
                                return;
                            }
                            kv = reflect.ValueOf(n).Convert(kt);
                        else if (kt.Kind() == reflect.Uint || kt.Kind() == reflect.Uint8 || kt.Kind() == reflect.Uint16 || kt.Kind() == reflect.Uint32 || kt.Kind() == reflect.Uint64 || kt.Kind() == reflect.Uintptr) 
                            s = string(key);
                            (n, err) = strconv.ParseUint(s, 10L, 64L);
                            if (err != null || reflect.Zero(kt).OverflowUint(n))
                            {
                                d.saveError(ref new UnmarshalTypeError(Value:"number "+s,Type:kt,Offset:int64(start+1)));
                                return;
                            }
                            kv = reflect.ValueOf(n).Convert(kt);
                        else 
                            panic("json: Unexpected key type"); // should never occur
                                                                v.SetMapIndex(kv, subv);
                } 

                // Next token must be , or }.
                op = d.scanWhile(scanSkipSpace);
                if (op == scanEndObject)
                {
                    break;
                }
                if (op != scanObjectValue)
                {
                    d.error(errPhase);
                }
                d.errorContext.Struct = "";
                d.errorContext.Field = "";
            }

        });

        // literal consumes a literal from d.data[d.off-1:], decoding into the value v.
        // The first byte of the literal has been read already
        // (that's how the caller knows it's a literal).
        private static void literal(this ref decodeState d, reflect.Value v)
        { 
            // All bytes inside literal return scanContinue op code.
            var start = d.off - 1L;
            var op = d.scanWhile(scanContinue); 

            // Scan read one byte too far; back up.
            d.off--;
            d.scan.undo(op);

            d.literalStore(d.data[start..d.off], v, false);
        }

        // convertNumber converts the number literal s to a float64 or a Number
        // depending on the setting of d.useNumber.
        private static (object, error) convertNumber(this ref decodeState d, @string s)
        {
            if (d.useNumber)
            {
                return (Number(s), null);
            }
            var (f, err) = strconv.ParseFloat(s, 64L);
            if (err != null)
            {
                return (null, ref new UnmarshalTypeError(Value:"number "+s,Type:reflect.TypeOf(0.0),Offset:int64(d.off)));
            }
            return (f, null);
        }

        private static var numberType = reflect.TypeOf(Number(""));

        // literalStore decodes a literal stored in item into v.
        //
        // fromQuoted indicates whether this literal came from unwrapping a
        // string from the ",string" struct tag option. this is used only to
        // produce more helpful error messages.
        private static void literalStore(this ref decodeState d, slice<byte> item, reflect.Value v, bool fromQuoted)
        { 
            // Check for unmarshaler.
            if (len(item) == 0L)
            { 
                //Empty string given
                d.saveError(fmt.Errorf("json: invalid use of ,string struct tag, trying to unmarshal %q into %v", item, v.Type()));
                return;
            }
            var isNull = item[0L] == 'n'; // null
            var (u, ut, pv) = d.indirect(v, isNull);
            if (u != null)
            {
                var err = u.UnmarshalJSON(item);
                if (err != null)
                {
                    d.error(err);
                }
                return;
            }
            if (ut != null)
            {
                if (item[0L] != '"')
                {
                    if (fromQuoted)
                    {
                        d.saveError(fmt.Errorf("json: invalid use of ,string struct tag, trying to unmarshal %q into %v", item, v.Type()));
                    }
                    else
                    {
                        @string val = default;
                        switch (item[0L])
                        {
                            case 'n': 
                                val = "null";
                                break;
                            case 't': 

                            case 'f': 
                                val = "bool";
                                break;
                            default: 
                                val = "number";
                                break;
                        }
                        d.saveError(ref new UnmarshalTypeError(Value:val,Type:v.Type(),Offset:int64(d.off)));
                    }
                    return;
                }
                var (s, ok) = unquoteBytes(item);
                if (!ok)
                {
                    if (fromQuoted)
                    {
                        d.error(fmt.Errorf("json: invalid use of ,string struct tag, trying to unmarshal %q into %v", item, v.Type()));
                    }
                    else
                    {
                        d.error(errPhase);
                    }
                }
                err = ut.UnmarshalText(s);
                if (err != null)
                {
                    d.error(err);
                }
                return;
            }
            v = pv;

            {
                var c = item[0L];

                switch (c)
                {
                    case 'n': // null
                        // The main parser checks that only true and false can reach here,
                        // but if this was a quoted string input, it could be anything.
                        if (fromQuoted && string(item) != "null")
                        {
                            d.saveError(fmt.Errorf("json: invalid use of ,string struct tag, trying to unmarshal %q into %v", item, v.Type()));
                            break;
                        }

                        if (v.Kind() == reflect.Interface || v.Kind() == reflect.Ptr || v.Kind() == reflect.Map || v.Kind() == reflect.Slice) 
                            v.Set(reflect.Zero(v.Type())); 
                            // otherwise, ignore null for primitives/string
                        break;
                    case 't': // true, false

                    case 'f': // true, false
                        var value = item[0L] == 't'; 
                        // The main parser checks that only true and false can reach here,
                        // but if this was a quoted string input, it could be anything.
                        if (fromQuoted && string(item) != "true" && string(item) != "false")
                        {
                            d.saveError(fmt.Errorf("json: invalid use of ,string struct tag, trying to unmarshal %q into %v", item, v.Type()));
                            break;
                        }

                        if (v.Kind() == reflect.Bool) 
                            v.SetBool(value);
                        else if (v.Kind() == reflect.Interface) 
                            if (v.NumMethod() == 0L)
                            {
                                v.Set(reflect.ValueOf(value));
                            }
                            else
                            {
                                d.saveError(ref new UnmarshalTypeError(Value:"bool",Type:v.Type(),Offset:int64(d.off)));
                            }
                        else 
                            if (fromQuoted)
                            {
                                d.saveError(fmt.Errorf("json: invalid use of ,string struct tag, trying to unmarshal %q into %v", item, v.Type()));
                            }
                            else
                            {
                                d.saveError(ref new UnmarshalTypeError(Value:"bool",Type:v.Type(),Offset:int64(d.off)));
                            }
                        break;
                    case '"': // string
                        (s, ok) = unquoteBytes(item);
                        if (!ok)
                        {
                            if (fromQuoted)
                            {
                                d.error(fmt.Errorf("json: invalid use of ,string struct tag, trying to unmarshal %q into %v", item, v.Type()));
                            }
                            else
                            {
                                d.error(errPhase);
                            }
                        }

                        if (v.Kind() == reflect.Slice) 
                            if (v.Type().Elem().Kind() != reflect.Uint8)
                            {
                                d.saveError(ref new UnmarshalTypeError(Value:"string",Type:v.Type(),Offset:int64(d.off)));
                                break;
                            }
                            var b = make_slice<byte>(base64.StdEncoding.DecodedLen(len(s)));
                            var (n, err) = base64.StdEncoding.Decode(b, s);
                            if (err != null)
                            {
                                d.saveError(err);
                                break;
                            }
                            v.SetBytes(b[..n]);
                        else if (v.Kind() == reflect.String) 
                            v.SetString(string(s));
                        else if (v.Kind() == reflect.Interface) 
                            if (v.NumMethod() == 0L)
                            {
                                v.Set(reflect.ValueOf(string(s)));
                            }
                            else
                            {
                                d.saveError(ref new UnmarshalTypeError(Value:"string",Type:v.Type(),Offset:int64(d.off)));
                            }
                        else 
                            d.saveError(ref new UnmarshalTypeError(Value:"string",Type:v.Type(),Offset:int64(d.off)));
                        break;
                    default: // number
                        if (c != '-' && (c < '0' || c > '9'))
                        {
                            if (fromQuoted)
                            {
                                d.error(fmt.Errorf("json: invalid use of ,string struct tag, trying to unmarshal %q into %v", item, v.Type()));
                            }
                            else
                            {
                                d.error(errPhase);
                            }
                        }
                        var s = string(item);

                        if (v.Kind() == reflect.Interface) 
                            (n, err) = d.convertNumber(s);
                            if (err != null)
                            {
                                d.saveError(err);
                                break;
                            }
                            if (v.NumMethod() != 0L)
                            {
                                d.saveError(ref new UnmarshalTypeError(Value:"number",Type:v.Type(),Offset:int64(d.off)));
                                break;
                            }
                            v.Set(reflect.ValueOf(n));
                        else if (v.Kind() == reflect.Int || v.Kind() == reflect.Int8 || v.Kind() == reflect.Int16 || v.Kind() == reflect.Int32 || v.Kind() == reflect.Int64) 
                            (n, err) = strconv.ParseInt(s, 10L, 64L);
                            if (err != null || v.OverflowInt(n))
                            {
                                d.saveError(ref new UnmarshalTypeError(Value:"number "+s,Type:v.Type(),Offset:int64(d.off)));
                                break;
                            }
                            v.SetInt(n);
                        else if (v.Kind() == reflect.Uint || v.Kind() == reflect.Uint8 || v.Kind() == reflect.Uint16 || v.Kind() == reflect.Uint32 || v.Kind() == reflect.Uint64 || v.Kind() == reflect.Uintptr) 
                            (n, err) = strconv.ParseUint(s, 10L, 64L);
                            if (err != null || v.OverflowUint(n))
                            {
                                d.saveError(ref new UnmarshalTypeError(Value:"number "+s,Type:v.Type(),Offset:int64(d.off)));
                                break;
                            }
                            v.SetUint(n);
                        else if (v.Kind() == reflect.Float32 || v.Kind() == reflect.Float64) 
                            (n, err) = strconv.ParseFloat(s, v.Type().Bits());
                            if (err != null || v.OverflowFloat(n))
                            {
                                d.saveError(ref new UnmarshalTypeError(Value:"number "+s,Type:v.Type(),Offset:int64(d.off)));
                                break;
                            }
                            v.SetFloat(n);
                        else 
                            if (v.Kind() == reflect.String && v.Type() == numberType)
                            {
                                v.SetString(s);
                                if (!isValidNumber(s))
                                {
                                    d.error(fmt.Errorf("json: invalid number literal, trying to unmarshal %q into Number", item));
                                }
                                break;
                            }
                            if (fromQuoted)
                            {
                                d.error(fmt.Errorf("json: invalid use of ,string struct tag, trying to unmarshal %q into %v", item, v.Type()));
                            }
                            else
                            {
                                d.error(ref new UnmarshalTypeError(Value:"number",Type:v.Type(),Offset:int64(d.off)));
                            }
                        break;
                }
            }
        }

        // The xxxInterface routines build up a value to be stored
        // in an empty interface. They are not strictly necessary,
        // but they avoid the weight of reflection in this common case.

        // valueInterface is like value but returns interface{}
        private static void valueInterface(this ref decodeState _d) => func(_d, (ref decodeState d, Defer _, Panic panic, Recover __) =>
        {

            if (d.scanWhile(scanSkipSpace) == scanBeginArray) 
                return d.arrayInterface();
            else if (d.scanWhile(scanSkipSpace) == scanBeginObject) 
                return d.objectInterface();
            else if (d.scanWhile(scanSkipSpace) == scanBeginLiteral) 
                return d.literalInterface();
            else 
                d.error(errPhase);
                panic("unreachable");
                    });

        // arrayInterface is like array but returns []interface{}.
        private static slice<object> arrayInterface(this ref decodeState d)
        {
            var v = make_slice<object>(0L);
            while (true)
            { 
                // Look ahead for ] - can only happen on first iteration.
                var op = d.scanWhile(scanSkipSpace);
                if (op == scanEndArray)
                {
                    break;
                } 

                // Back up so d.value can have the byte we just read.
                d.off--;
                d.scan.undo(op);

                v = append(v, d.valueInterface()); 

                // Next token must be , or ].
                op = d.scanWhile(scanSkipSpace);
                if (op == scanEndArray)
                {
                    break;
                }
                if (op != scanArrayValue)
                {
                    d.error(errPhase);
                }
            }

            return v;
        }

        // objectInterface is like object but returns map[string]interface{}.
        private static void objectInterface(this ref decodeState d)
        {
            var m = make();
            while (true)
            { 
                // Read opening " of string key or closing }.
                var op = d.scanWhile(scanSkipSpace);
                if (op == scanEndObject)
                { 
                    // closing } - can only happen on first iteration.
                    break;
                }
                if (op != scanBeginLiteral)
                {
                    d.error(errPhase);
                } 

                // Read string key.
                var start = d.off - 1L;
                op = d.scanWhile(scanContinue);
                var item = d.data[start..d.off - 1L];
                var (key, ok) = unquote(item);
                if (!ok)
                {
                    d.error(errPhase);
                } 

                // Read : before value.
                if (op == scanSkipSpace)
                {
                    op = d.scanWhile(scanSkipSpace);
                }
                if (op != scanObjectKey)
                {
                    d.error(errPhase);
                } 

                // Read value.
                m[key] = d.valueInterface(); 

                // Next token must be , or }.
                op = d.scanWhile(scanSkipSpace);
                if (op == scanEndObject)
                {
                    break;
                }
                if (op != scanObjectValue)
                {
                    d.error(errPhase);
                }
            }

            return m;
        }

        // literalInterface is like literal but returns an interface value.
        private static void literalInterface(this ref decodeState d)
        { 
            // All bytes inside literal return scanContinue op code.
            var start = d.off - 1L;
            var op = d.scanWhile(scanContinue); 

            // Scan read one byte too far; back up.
            d.off--;
            d.scan.undo(op);
            var item = d.data[start..d.off];

            {
                var c = item[0L];

                switch (c)
                {
                    case 'n': // null
                        return null;
                        break;
                    case 't': // true, false

                    case 'f': // true, false
                        return c == 't';
                        break;
                    case '"': // string
                        var (s, ok) = unquote(item);
                        if (!ok)
                        {
                            d.error(errPhase);
                        }
                        return s;
                        break;
                    default: // number
                        if (c != '-' && (c < '0' || c > '9'))
                        {
                            d.error(errPhase);
                        }
                        var (n, err) = d.convertNumber(string(item));
                        if (err != null)
                        {
                            d.saveError(err);
                        }
                        return n;
                        break;
                }
            }
        }

        // getu4 decodes \uXXXX from the beginning of s, returning the hex value,
        // or it returns -1.
        private static int getu4(slice<byte> s)
        {
            if (len(s) < 6L || s[0L] != '\\' || s[1L] != 'u')
            {
                return -1L;
            }
            int r = default;
            foreach (var (_, c) in s[2L..6L])
            {

                if ('0' <= c && c <= '9') 
                    c = c - '0';
                else if ('a' <= c && c <= 'f') 
                    c = c - 'a' + 10L;
                else if ('A' <= c && c <= 'F') 
                    c = c - 'A' + 10L;
                else 
                    return -1L;
                                r = r * 16L + rune(c);
            }
            return r;
        }

        // unquote converts a quoted JSON string literal s into an actual string t.
        // The rules are different than for Go, so cannot use strconv.Unquote.
        private static (@string, bool) unquote(slice<byte> s)
        {
            s, ok = unquoteBytes(s);
            t = string(s);
            return;
        }

        private static (slice<byte>, bool) unquoteBytes(slice<byte> s)
        {
            if (len(s) < 2L || s[0L] != '"' || s[len(s) - 1L] != '"')
            {
                return;
            }
            s = s[1L..len(s) - 1L]; 

            // Check for unusual characters. If there are none,
            // then no unquoting is needed, so return a slice of the
            // original bytes.
            long r = 0L;
            while (r < len(s))
            {
                var c = s[r];
                if (c == '\\' || c == '"' || c < ' ')
                {
                    break;
                }
                if (c < utf8.RuneSelf)
                {
                    r++;
                    continue;
                }
                var (rr, size) = utf8.DecodeRune(s[r..]);
                if (rr == utf8.RuneError && size == 1L)
                {
                    break;
                }
                r += size;
            }

            if (r == len(s))
            {
                return (s, true);
            }
            var b = make_slice<byte>(len(s) + 2L * utf8.UTFMax);
            var w = copy(b, s[0L..r]);
            while (r < len(s))
            { 
                // Out of room? Can only happen if s is full of
                // malformed UTF-8 and we're replacing each
                // byte with RuneError.
                if (w >= len(b) - 2L * utf8.UTFMax)
                {
                    var nb = make_slice<byte>((len(b) + utf8.UTFMax) * 2L);
                    copy(nb, b[0L..w]);
                    b = nb;
                }
                {
                    var c__prev1 = c;

                    c = s[r];


                    if (c == '\\') 
                        r++;
                        if (r >= len(s))
                        {
                            return;
                        }
                        switch (s[r])
                        {
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
                                var rr = getu4(s[r..]);
                                if (rr < 0L)
                                {
                                    return;
                                }
                                r += 6L;
                                if (utf16.IsSurrogate(rr))
                                {
                                    var rr1 = getu4(s[r..]);
                                    {
                                        var dec = utf16.DecodeRune(rr, rr1);

                                        if (dec != unicode.ReplacementChar)
                                        { 
                                            // A valid pair; consume.
                                            r += 6L;
                                            w += utf8.EncodeRune(b[w..], dec);
                                            break;
                                        } 
                                        // Invalid surrogate; fall back to replacement rune.

                                    } 
                                    // Invalid surrogate; fall back to replacement rune.
                                    rr = unicode.ReplacementChar;
                                }
                                w += utf8.EncodeRune(b[w..], rr);
                                break;
                            default: 
                                return;
                                break;
                        } 

                        // Quote, control characters are invalid.
                    else if (c == '"' || c < ' ') 
                        return; 

                        // ASCII
                    else if (c < utf8.RuneSelf) 
                        b[w] = c;
                        r++;
                        w++; 

                        // Coerce to well-formed UTF-8.
                    else 
                        (rr, size) = utf8.DecodeRune(s[r..]);
                        r += size;
                        w += utf8.EncodeRune(b[w..], rr);


                    c = c__prev1;
                }
            }

            return (b[0L..w], true);
        }
    }
}}
