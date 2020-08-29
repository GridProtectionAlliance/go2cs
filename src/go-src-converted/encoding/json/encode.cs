// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package json implements encoding and decoding of JSON as defined in
// RFC 7159. The mapping between JSON and Go values is described
// in the documentation for the Marshal and Unmarshal functions.
//
// See "JSON and Go" for an introduction to this package:
// https://golang.org/doc/articles/json_and_go.html
// package json -- go2cs converted at 2020 August 29 08:35:48 UTC
// import "encoding/json" ==> using json = go.encoding.json_package
// Original source: C:\Go\src\encoding\json\encode.go
using bytes = go.bytes_package;
using encoding = go.encoding_package;
using base64 = go.encoding.base64_package;
using fmt = go.fmt_package;
using math = go.math_package;
using reflect = go.reflect_package;
using runtime = go.runtime_package;
using sort = go.sort_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using sync = go.sync_package;
using atomic = go.sync.atomic_package;
using unicode = go.unicode_package;
using utf8 = go.unicode.utf8_package;
using static go.builtin;
using System;

namespace go {
namespace encoding
{
    public static partial class json_package
    {
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
        // The angle brackets "<" and ">" are escaped to "\u003c" and "\u003e"
        // to keep some browsers from misinterpreting JSON output as HTML.
        // Ampersand "&" is also escaped to "\u0026" for the same reason.
        // This escaping can be disabled using an Encoder that had SetEscapeHTML(false)
        // called on it.
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
        //   - string keys are used directly
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
        // an infinite recursion.
        //
        public static (slice<byte>, error) Marshal(object v)
        {
            encodeState e = ref new encodeState();
            var err = e.marshal(v, new encOpts(escapeHTML:true));
            if (err != null)
            {
                return (null, err);
            }
            return (e.Bytes(), null);
        }

        // MarshalIndent is like Marshal but applies Indent to format the output.
        // Each JSON element in the output will begin on a new line beginning with prefix
        // followed by one or more copies of indent according to the indentation nesting.
        public static (slice<byte>, error) MarshalIndent(object v, @string prefix, @string indent)
        {
            var (b, err) = Marshal(v);
            if (err != null)
            {
                return (null, err);
            }
            bytes.Buffer buf = default;
            err = Indent(ref buf, b, prefix, indent);
            if (err != null)
            {
                return (null, err);
            }
            return (buf.Bytes(), null);
        }

        // HTMLEscape appends to dst the JSON-encoded src with <, >, &, U+2028 and U+2029
        // characters inside string literals changed to \u003c, \u003e, \u0026, \u2028, \u2029
        // so that the JSON will be safe to embed inside HTML <script> tags.
        // For historical reasons, web browsers don't honor standard HTML
        // escaping within <script> tags, so an alternative JSON encoding must
        // be used.
        public static void HTMLEscape(ref bytes.Buffer dst, slice<byte> src)
        { 
            // The characters can only appear in string literals,
            // so just scan the string one byte at a time.
            long start = 0L;
            foreach (var (i, c) in src)
            {
                if (c == '<' || c == '>' || c == '&')
                {
                    if (start < i)
                    {
                        dst.Write(src[start..i]);
                    }
                    dst.WriteString("\\u00");
                    dst.WriteByte(hex[c >> (int)(4L)]);
                    dst.WriteByte(hex[c & 0xFUL]);
                    start = i + 1L;
                } 
                // Convert U+2028 and U+2029 (E2 80 A8 and E2 80 A9).
                if (c == 0xE2UL && i + 2L < len(src) && src[i + 1L] == 0x80UL && src[i + 2L] & ~1L == 0xA8UL)
                {
                    if (start < i)
                    {
                        dst.Write(src[start..i]);
                    }
                    dst.WriteString("\\u202");
                    dst.WriteByte(hex[src[i + 2L] & 0xFUL]);
                    start = i + 3L;
                }
            }
            if (start < len(src))
            {
                dst.Write(src[start..]);
            }
        }

        // Marshaler is the interface implemented by types that
        // can marshal themselves into valid JSON.
        public partial interface Marshaler
        {
            (slice<byte>, error) MarshalJSON();
        }

        // An UnsupportedTypeError is returned by Marshal when attempting
        // to encode an unsupported value type.
        public partial struct UnsupportedTypeError
        {
            public reflect.Type Type;
        }

        private static @string Error(this ref UnsupportedTypeError e)
        {
            return "json: unsupported type: " + e.Type.String();
        }

        public partial struct UnsupportedValueError
        {
            public reflect.Value Value;
            public @string Str;
        }

        private static @string Error(this ref UnsupportedValueError e)
        {
            return "json: unsupported value: " + e.Str;
        }

        // Before Go 1.2, an InvalidUTF8Error was returned by Marshal when
        // attempting to encode a string value with invalid UTF-8 sequences.
        // As of Go 1.2, Marshal instead coerces the string to valid UTF-8 by
        // replacing invalid bytes with the Unicode replacement rune U+FFFD.
        //
        // Deprecated: No longer used; kept for compatibility.
        public partial struct InvalidUTF8Error
        {
            public @string S; // the whole string value that caused the error
        }

        private static @string Error(this ref InvalidUTF8Error e)
        {
            return "json: invalid UTF-8 in string: " + strconv.Quote(e.S);
        }

        public partial struct MarshalerError
        {
            public reflect.Type Type;
            public error Err;
        }

        private static @string Error(this ref MarshalerError e)
        {
            return "json: error calling MarshalJSON for type " + e.Type.String() + ": " + e.Err.Error();
        }

        private static @string hex = "0123456789abcdef";

        // An encodeState encodes JSON into a bytes.Buffer.
        private partial struct encodeState
        {
            public ref bytes.Buffer Buffer => ref Buffer_val; // accumulated output
            public array<byte> scratch;
        }

        private static sync.Pool encodeStatePool = default;

        private static ref encodeState newEncodeState()
        {
            {
                var v = encodeStatePool.Get();

                if (v != null)
                {
                    ref encodeState e = v._<ref encodeState>();
                    e.Reset();
                    return e;
                }

            }
            return @new<encodeState>();
        }

        private static error marshal(this ref encodeState _e, object v, encOpts opts) => func(_e, (ref encodeState e, Defer defer, Panic panic, Recover _) =>
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
                        {
                            @string (s, ok) = r._<@string>();

                            if (ok)
                            {
                                panic(s);
                            }

                        }
                        err = r._<error>();
                    }

                }
            }());
            e.reflectValue(reflect.ValueOf(v), opts);
            return error.As(null);
        });

        private static void error(this ref encodeState _e, error err) => func(_e, (ref encodeState e, Defer _, Panic panic, Recover __) =>
        {
            panic(err);
        });

        private static bool isEmptyValue(reflect.Value v)
        {

            if (v.Kind() == reflect.Array || v.Kind() == reflect.Map || v.Kind() == reflect.Slice || v.Kind() == reflect.String) 
                return v.Len() == 0L;
            else if (v.Kind() == reflect.Bool) 
                return !v.Bool();
            else if (v.Kind() == reflect.Int || v.Kind() == reflect.Int8 || v.Kind() == reflect.Int16 || v.Kind() == reflect.Int32 || v.Kind() == reflect.Int64) 
                return v.Int() == 0L;
            else if (v.Kind() == reflect.Uint || v.Kind() == reflect.Uint8 || v.Kind() == reflect.Uint16 || v.Kind() == reflect.Uint32 || v.Kind() == reflect.Uint64 || v.Kind() == reflect.Uintptr) 
                return v.Uint() == 0L;
            else if (v.Kind() == reflect.Float32 || v.Kind() == reflect.Float64) 
                return v.Float() == 0L;
            else if (v.Kind() == reflect.Interface || v.Kind() == reflect.Ptr) 
                return v.IsNil();
                        return false;
        }

        private static void reflectValue(this ref encodeState e, reflect.Value v, encOpts opts)
        {
            valueEncoder(v)(e, v, opts);
        }

        private partial struct encOpts
        {
            public bool quoted; // escapeHTML causes '<', '>', and '&' to be escaped in JSON strings.
            public bool escapeHTML;
        }

        public delegate void encoderFunc(ref encodeState, reflect.Value, encOpts);

        private static sync.Map encoderCache = default; // map[reflect.Type]encoderFunc

        private static encoderFunc valueEncoder(reflect.Value v)
        {
            if (!v.IsValid())
            {
                return invalidValueEncoder;
            }
            return typeEncoder(v.Type());
        }

        private static encoderFunc typeEncoder(reflect.Type t)
        {
            {
                var fi__prev1 = fi;

                var (fi, ok) = encoderCache.Load(t);

                if (ok)
                {
                    return fi._<encoderFunc>();
                } 

                // To deal with recursive types, populate the map with an
                // indirect func before we build it. This type waits on the
                // real func (f) to be ready and then calls it. This indirect
                // func is only used for recursive types.

                fi = fi__prev1;

            } 

            // To deal with recursive types, populate the map with an
            // indirect func before we build it. This type waits on the
            // real func (f) to be ready and then calls it. This indirect
            // func is only used for recursive types.
            sync.WaitGroup wg = default;            encoderFunc f = default;
            wg.Add(1L);
            var (fi, loaded) = encoderCache.LoadOrStore(t, encoderFunc((e, v, opts) =>
            {
                wg.Wait();
                f(e, v, opts);
            }));
            if (loaded)
            {
                return fi._<encoderFunc>();
            } 

            // Compute the real encoder and replace the indirect func with it.
            f = newTypeEncoder(t, true);
            wg.Done();
            encoderCache.Store(t, f);
            return f;
        }

        private static var marshalerType = reflect.TypeOf(@new<Marshaler>()).Elem();        private static var textMarshalerType = reflect.TypeOf(@new<encoding.TextMarshaler>()).Elem();

        // newTypeEncoder constructs an encoderFunc for a type.
        // The returned encoder only checks CanAddr when allowAddr is true.
        private static encoderFunc newTypeEncoder(reflect.Type t, bool allowAddr)
        {
            if (t.Implements(marshalerType))
            {
                return marshalerEncoder;
            }
            if (t.Kind() != reflect.Ptr && allowAddr)
            {
                if (reflect.PtrTo(t).Implements(marshalerType))
                {
                    return newCondAddrEncoder(addrMarshalerEncoder, newTypeEncoder(t, false));
                }
            }
            if (t.Implements(textMarshalerType))
            {
                return textMarshalerEncoder;
            }
            if (t.Kind() != reflect.Ptr && allowAddr)
            {
                if (reflect.PtrTo(t).Implements(textMarshalerType))
                {
                    return newCondAddrEncoder(addrTextMarshalerEncoder, newTypeEncoder(t, false));
                }
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

        private static void invalidValueEncoder(ref encodeState e, reflect.Value v, encOpts _)
        {
            e.WriteString("null");
        }

        private static void marshalerEncoder(ref encodeState e, reflect.Value v, encOpts opts)
        {
            if (v.Kind() == reflect.Ptr && v.IsNil())
            {
                e.WriteString("null");
                return;
            }
            Marshaler (m, ok) = v.Interface()._<Marshaler>();
            if (!ok)
            {
                e.WriteString("null");
                return;
            }
            var (b, err) = m.MarshalJSON();
            if (err == null)
            { 
                // copy JSON into buffer, checking validity.
                err = compact(ref e.Buffer, b, opts.escapeHTML);
            }
            if (err != null)
            {
                e.error(ref new MarshalerError(v.Type(),err));
            }
        }

        private static void addrMarshalerEncoder(ref encodeState e, reflect.Value v, encOpts _)
        {
            var va = v.Addr();
            if (va.IsNil())
            {
                e.WriteString("null");
                return;
            }
            Marshaler m = va.Interface()._<Marshaler>();
            var (b, err) = m.MarshalJSON();
            if (err == null)
            { 
                // copy JSON into buffer, checking validity.
                err = compact(ref e.Buffer, b, true);
            }
            if (err != null)
            {
                e.error(ref new MarshalerError(v.Type(),err));
            }
        }

        private static void textMarshalerEncoder(ref encodeState e, reflect.Value v, encOpts opts)
        {
            if (v.Kind() == reflect.Ptr && v.IsNil())
            {
                e.WriteString("null");
                return;
            }
            encoding.TextMarshaler m = v.Interface()._<encoding.TextMarshaler>();
            var (b, err) = m.MarshalText();
            if (err != null)
            {
                e.error(ref new MarshalerError(v.Type(),err));
            }
            e.stringBytes(b, opts.escapeHTML);
        }

        private static void addrTextMarshalerEncoder(ref encodeState e, reflect.Value v, encOpts opts)
        {
            var va = v.Addr();
            if (va.IsNil())
            {
                e.WriteString("null");
                return;
            }
            encoding.TextMarshaler m = va.Interface()._<encoding.TextMarshaler>();
            var (b, err) = m.MarshalText();
            if (err != null)
            {
                e.error(ref new MarshalerError(v.Type(),err));
            }
            e.stringBytes(b, opts.escapeHTML);
        }

        private static void boolEncoder(ref encodeState e, reflect.Value v, encOpts opts)
        {
            if (opts.quoted)
            {
                e.WriteByte('"');
            }
            if (v.Bool())
            {
                e.WriteString("true");
            }
            else
            {
                e.WriteString("false");
            }
            if (opts.quoted)
            {
                e.WriteByte('"');
            }
        }

        private static void intEncoder(ref encodeState e, reflect.Value v, encOpts opts)
        {
            var b = strconv.AppendInt(e.scratch[..0L], v.Int(), 10L);
            if (opts.quoted)
            {
                e.WriteByte('"');
            }
            e.Write(b);
            if (opts.quoted)
            {
                e.WriteByte('"');
            }
        }

        private static void uintEncoder(ref encodeState e, reflect.Value v, encOpts opts)
        {
            var b = strconv.AppendUint(e.scratch[..0L], v.Uint(), 10L);
            if (opts.quoted)
            {
                e.WriteByte('"');
            }
            e.Write(b);
            if (opts.quoted)
            {
                e.WriteByte('"');
            }
        }

        private partial struct floatEncoder // : long
        {
        } // number of bits

        private static void encode(this floatEncoder bits, ref encodeState e, reflect.Value v, encOpts opts)
        {
            var f = v.Float();
            if (math.IsInf(f, 0L) || math.IsNaN(f))
            {
                e.error(ref new UnsupportedValueError(v,strconv.FormatFloat(f,'g',-1,int(bits))));
            } 

            // Convert as if by ES6 number to string conversion.
            // This matches most other JSON generators.
            // See golang.org/issue/6384 and golang.org/issue/14135.
            // Like fmt %g, but the exponent cutoffs are different
            // and exponents themselves are not padded to two digits.
            var b = e.scratch[..0L];
            var abs = math.Abs(f);
            var fmt = byte('f'); 
            // Note: Must use float32 comparisons for underlying float32 value to get precise cutoffs right.
            if (abs != 0L)
            {
                if (bits == 64L && (abs < 1e-6F || abs >= 1e21F) || bits == 32L && (float32(abs) < 1e-6F || float32(abs) >= 1e21F))
                {
                    fmt = 'e';
                }
            }
            b = strconv.AppendFloat(b, f, fmt, -1L, int(bits));
            if (fmt == 'e')
            { 
                // clean up e-09 to e-9
                var n = len(b);
                if (n >= 4L && b[n - 4L] == 'e' && b[n - 3L] == '-' && b[n - 2L] == '0')
                {
                    b[n - 2L] = b[n - 1L];
                    b = b[..n - 1L];
                }
            }
            if (opts.quoted)
            {
                e.WriteByte('"');
            }
            e.Write(b);
            if (opts.quoted)
            {
                e.WriteByte('"');
            }
        }

        private static var float32Encoder = (floatEncoder(32L)).encode;        private static var float64Encoder = (floatEncoder(64L)).encode;

        private static void stringEncoder(ref encodeState e, reflect.Value v, encOpts opts)
        {
            if (v.Type() == numberType)
            {
                var numStr = v.String(); 
                // In Go1.5 the empty string encodes to "0", while this is not a valid number literal
                // we keep compatibility so check validity after this.
                if (numStr == "")
                {
                    numStr = "0"; // Number's zero-val
                }
                if (!isValidNumber(numStr))
                {
                    e.error(fmt.Errorf("json: invalid number literal %q", numStr));
                }
                e.WriteString(numStr);
                return;
            }
            if (opts.quoted)
            {
                var (sb, err) = Marshal(v.String());
                if (err != null)
                {
                    e.error(err);
                }
                e.@string(string(sb), opts.escapeHTML);
            }
            else
            {
                e.@string(v.String(), opts.escapeHTML);
            }
        }

        private static void interfaceEncoder(ref encodeState e, reflect.Value v, encOpts opts)
        {
            if (v.IsNil())
            {
                e.WriteString("null");
                return;
            }
            e.reflectValue(v.Elem(), opts);
        }

        private static void unsupportedTypeEncoder(ref encodeState e, reflect.Value v, encOpts _)
        {
            e.error(ref new UnsupportedTypeError(v.Type()));
        }

        private partial struct structEncoder
        {
            public slice<field> fields;
            public slice<encoderFunc> fieldEncs;
        }

        private static void encode(this ref structEncoder se, ref encodeState e, reflect.Value v, encOpts opts)
        {
            e.WriteByte('{');
            var first = true;
            foreach (var (i, f) in se.fields)
            {
                var fv = fieldByIndex(v, f.index);
                if (!fv.IsValid() || f.omitEmpty && isEmptyValue(fv))
                {
                    continue;
                }
                if (first)
                {
                    first = false;
                }
                else
                {
                    e.WriteByte(',');
                }
                e.@string(f.name, opts.escapeHTML);
                e.WriteByte(':');
                opts.quoted = f.quoted;
                se.fieldEncs[i](e, fv, opts);
            }
            e.WriteByte('}');
        }

        private static encoderFunc newStructEncoder(reflect.Type t)
        {
            var fields = cachedTypeFields(t);
            structEncoder se = ref new structEncoder(fields:fields,fieldEncs:make([]encoderFunc,len(fields)),);
            foreach (var (i, f) in fields)
            {
                se.fieldEncs[i] = typeEncoder(typeByIndex(t, f.index));
            }
            return se.encode;
        }

        private partial struct mapEncoder
        {
            public encoderFunc elemEnc;
        }

        private static void encode(this ref mapEncoder me, ref encodeState e, reflect.Value v, encOpts opts)
        {
            if (v.IsNil())
            {
                e.WriteString("null");
                return;
            }
            e.WriteByte('{'); 

            // Extract and sort the keys.
            var keys = v.MapKeys();
            var sv = make_slice<reflectWithString>(len(keys));
            {
                var i__prev1 = i;

                foreach (var (__i, __v) in keys)
                {
                    i = __i;
                    v = __v;
                    sv[i].v = v;
                    {
                        var err = sv[i].resolve();

                        if (err != null)
                        {
                            e.error(ref new MarshalerError(v.Type(),err));
                        }

                    }
                }

                i = i__prev1;
            }

            sort.Slice(sv, (i, j) => sv[i].s < sv[j].s);

            {
                var i__prev1 = i;

                foreach (var (__i, __kv) in sv)
                {
                    i = __i;
                    kv = __kv;
                    if (i > 0L)
                    {
                        e.WriteByte(',');
                    }
                    e.@string(kv.s, opts.escapeHTML);
                    e.WriteByte(':');
                    me.elemEnc(e, v.MapIndex(kv.v), opts);
                }

                i = i__prev1;
            }

            e.WriteByte('}');
        }

        private static encoderFunc newMapEncoder(reflect.Type t)
        {

            if (t.Key().Kind() == reflect.String || t.Key().Kind() == reflect.Int || t.Key().Kind() == reflect.Int8 || t.Key().Kind() == reflect.Int16 || t.Key().Kind() == reflect.Int32 || t.Key().Kind() == reflect.Int64 || t.Key().Kind() == reflect.Uint || t.Key().Kind() == reflect.Uint8 || t.Key().Kind() == reflect.Uint16 || t.Key().Kind() == reflect.Uint32 || t.Key().Kind() == reflect.Uint64 || t.Key().Kind() == reflect.Uintptr)             else 
                if (!t.Key().Implements(textMarshalerType))
                {
                    return unsupportedTypeEncoder;
                }
                        mapEncoder me = ref new mapEncoder(typeEncoder(t.Elem()));
            return me.encode;
        }

        private static void encodeByteSlice(ref encodeState e, reflect.Value v, encOpts _)
        {
            if (v.IsNil())
            {
                e.WriteString("null");
                return;
            }
            var s = v.Bytes();
            e.WriteByte('"');
            if (len(s) < 1024L)
            { 
                // for small buffers, using Encode directly is much faster.
                var dst = make_slice<byte>(base64.StdEncoding.EncodedLen(len(s)));
                base64.StdEncoding.Encode(dst, s);
                e.Write(dst);
            }
            else
            { 
                // for large buffers, avoid unnecessary extra temporary
                // buffer space.
                var enc = base64.NewEncoder(base64.StdEncoding, e);
                enc.Write(s);
                enc.Close();
            }
            e.WriteByte('"');
        }

        // sliceEncoder just wraps an arrayEncoder, checking to make sure the value isn't nil.
        private partial struct sliceEncoder
        {
            public encoderFunc arrayEnc;
        }

        private static void encode(this ref sliceEncoder se, ref encodeState e, reflect.Value v, encOpts opts)
        {
            if (v.IsNil())
            {
                e.WriteString("null");
                return;
            }
            se.arrayEnc(e, v, opts);
        }

        private static encoderFunc newSliceEncoder(reflect.Type t)
        { 
            // Byte slices get special treatment; arrays don't.
            if (t.Elem().Kind() == reflect.Uint8)
            {
                var p = reflect.PtrTo(t.Elem());
                if (!p.Implements(marshalerType) && !p.Implements(textMarshalerType))
                {
                    return encodeByteSlice;
                }
            }
            sliceEncoder enc = ref new sliceEncoder(newArrayEncoder(t));
            return enc.encode;
        }

        private partial struct arrayEncoder
        {
            public encoderFunc elemEnc;
        }

        private static void encode(this ref arrayEncoder ae, ref encodeState e, reflect.Value v, encOpts opts)
        {
            e.WriteByte('[');
            var n = v.Len();
            for (long i = 0L; i < n; i++)
            {
                if (i > 0L)
                {
                    e.WriteByte(',');
                }
                ae.elemEnc(e, v.Index(i), opts);
            }

            e.WriteByte(']');
        }

        private static encoderFunc newArrayEncoder(reflect.Type t)
        {
            arrayEncoder enc = ref new arrayEncoder(typeEncoder(t.Elem()));
            return enc.encode;
        }

        private partial struct ptrEncoder
        {
            public encoderFunc elemEnc;
        }

        private static void encode(this ref ptrEncoder pe, ref encodeState e, reflect.Value v, encOpts opts)
        {
            if (v.IsNil())
            {
                e.WriteString("null");
                return;
            }
            pe.elemEnc(e, v.Elem(), opts);
        }

        private static encoderFunc newPtrEncoder(reflect.Type t)
        {
            ptrEncoder enc = ref new ptrEncoder(typeEncoder(t.Elem()));
            return enc.encode;
        }

        private partial struct condAddrEncoder
        {
            public encoderFunc canAddrEnc;
            public encoderFunc elseEnc;
        }

        private static void encode(this ref condAddrEncoder ce, ref encodeState e, reflect.Value v, encOpts opts)
        {
            if (v.CanAddr())
            {
                ce.canAddrEnc(e, v, opts);
            }
            else
            {
                ce.elseEnc(e, v, opts);
            }
        }

        // newCondAddrEncoder returns an encoder that checks whether its value
        // CanAddr and delegates to canAddrEnc if so, else to elseEnc.
        private static encoderFunc newCondAddrEncoder(encoderFunc canAddrEnc, encoderFunc elseEnc)
        {
            condAddrEncoder enc = ref new condAddrEncoder(canAddrEnc:canAddrEnc,elseEnc:elseEnc);
            return enc.encode;
        }

        private static bool isValidTag(@string s)
        {
            if (s == "")
            {
                return false;
            }
            foreach (var (_, c) in s)
            {

                if (strings.ContainsRune("!#$%&()*+-./:<=>?@[]^_{|}~ ", c))                 else 
                    if (!unicode.IsLetter(c) && !unicode.IsDigit(c))
                    {
                        return false;
                    }
                            }
            return true;
        }

        private static reflect.Value fieldByIndex(reflect.Value v, slice<long> index)
        {
            foreach (var (_, i) in index)
            {
                if (v.Kind() == reflect.Ptr)
                {
                    if (v.IsNil())
                    {
                        return new reflect.Value();
                    }
                    v = v.Elem();
                }
                v = v.Field(i);
            }
            return v;
        }

        private static reflect.Type typeByIndex(reflect.Type t, slice<long> index)
        {
            foreach (var (_, i) in index)
            {
                if (t.Kind() == reflect.Ptr)
                {
                    t = t.Elem();
                }
                t = t.Field(i).Type;
            }
            return t;
        }

        private partial struct reflectWithString
        {
            public reflect.Value v;
            public @string s;
        }

        private static error resolve(this ref reflectWithString _w) => func(_w, (ref reflectWithString w, Defer _, Panic panic, Recover __) =>
        {
            if (w.v.Kind() == reflect.String)
            {
                w.s = w.v.String();
                return error.As(null);
            }
            {
                encoding.TextMarshaler (tm, ok) = w.v.Interface()._<encoding.TextMarshaler>();

                if (ok)
                {
                    var (buf, err) = tm.MarshalText();
                    w.s = string(buf);
                    return error.As(err);
                }

            }

            if (w.v.Kind() == reflect.Int || w.v.Kind() == reflect.Int8 || w.v.Kind() == reflect.Int16 || w.v.Kind() == reflect.Int32 || w.v.Kind() == reflect.Int64) 
                w.s = strconv.FormatInt(w.v.Int(), 10L);
                return error.As(null);
            else if (w.v.Kind() == reflect.Uint || w.v.Kind() == reflect.Uint8 || w.v.Kind() == reflect.Uint16 || w.v.Kind() == reflect.Uint32 || w.v.Kind() == reflect.Uint64 || w.v.Kind() == reflect.Uintptr) 
                w.s = strconv.FormatUint(w.v.Uint(), 10L);
                return error.As(null);
                        panic("unexpected map key type");
        });

        // NOTE: keep in sync with stringBytes below.
        private static void @string(this ref encodeState e, @string s, bool escapeHTML)
        {
            e.WriteByte('"');
            long start = 0L;
            {
                long i = 0L;

                while (i < len(s))
                {
                    {
                        var b = s[i];

                        if (b < utf8.RuneSelf)
                        {
                            if (htmlSafeSet[b] || (!escapeHTML && safeSet[b]))
                            {
                                i++;
                                continue;
                            }
                            if (start < i)
                            {
                                e.WriteString(s[start..i]);
                            }
                            switch (b)
                            {
                                case '\\': 

                                case '"': 
                                    e.WriteByte('\\');
                                    e.WriteByte(b);
                                    break;
                                case '\n': 
                                    e.WriteByte('\\');
                                    e.WriteByte('n');
                                    break;
                                case '\r': 
                                    e.WriteByte('\\');
                                    e.WriteByte('r');
                                    break;
                                case '\t': 
                                    e.WriteByte('\\');
                                    e.WriteByte('t');
                                    break;
                                default: 
                                    // This encodes bytes < 0x20 except for \t, \n and \r.
                                    // If escapeHTML is set, it also escapes <, >, and &
                                    // because they can lead to security holes when
                                    // user-controlled strings are rendered into JSON
                                    // and served to some browsers.
                                    e.WriteString("\\u00");
                                    e.WriteByte(hex[b >> (int)(4L)]);
                                    e.WriteByte(hex[b & 0xFUL]);
                                    break;
                            }
                            i++;
                            start = i;
                            continue;
                        }

                    }
                    var (c, size) = utf8.DecodeRuneInString(s[i..]);
                    if (c == utf8.RuneError && size == 1L)
                    {
                        if (start < i)
                        {
                            e.WriteString(s[start..i]);
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
                    if (c == '\u2028' || c == '\u2029')
                    {
                        if (start < i)
                        {
                            e.WriteString(s[start..i]);
                        }
                        e.WriteString("\\u202");
                        e.WriteByte(hex[c & 0xFUL]);
                        i += size;
                        start = i;
                        continue;
                    }
                    i += size;
                }

            }
            if (start < len(s))
            {
                e.WriteString(s[start..]);
            }
            e.WriteByte('"');
        }

        // NOTE: keep in sync with string above.
        private static void stringBytes(this ref encodeState e, slice<byte> s, bool escapeHTML)
        {
            e.WriteByte('"');
            long start = 0L;
            {
                long i = 0L;

                while (i < len(s))
                {
                    {
                        var b = s[i];

                        if (b < utf8.RuneSelf)
                        {
                            if (htmlSafeSet[b] || (!escapeHTML && safeSet[b]))
                            {
                                i++;
                                continue;
                            }
                            if (start < i)
                            {
                                e.Write(s[start..i]);
                            }
                            switch (b)
                            {
                                case '\\': 

                                case '"': 
                                    e.WriteByte('\\');
                                    e.WriteByte(b);
                                    break;
                                case '\n': 
                                    e.WriteByte('\\');
                                    e.WriteByte('n');
                                    break;
                                case '\r': 
                                    e.WriteByte('\\');
                                    e.WriteByte('r');
                                    break;
                                case '\t': 
                                    e.WriteByte('\\');
                                    e.WriteByte('t');
                                    break;
                                default: 
                                    // This encodes bytes < 0x20 except for \t, \n and \r.
                                    // If escapeHTML is set, it also escapes <, >, and &
                                    // because they can lead to security holes when
                                    // user-controlled strings are rendered into JSON
                                    // and served to some browsers.
                                    e.WriteString("\\u00");
                                    e.WriteByte(hex[b >> (int)(4L)]);
                                    e.WriteByte(hex[b & 0xFUL]);
                                    break;
                            }
                            i++;
                            start = i;
                            continue;
                        }

                    }
                    var (c, size) = utf8.DecodeRune(s[i..]);
                    if (c == utf8.RuneError && size == 1L)
                    {
                        if (start < i)
                        {
                            e.Write(s[start..i]);
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
                    if (c == '\u2028' || c == '\u2029')
                    {
                        if (start < i)
                        {
                            e.Write(s[start..i]);
                        }
                        e.WriteString("\\u202");
                        e.WriteByte(hex[c & 0xFUL]);
                        i += size;
                        start = i;
                        continue;
                    }
                    i += size;
                }

            }
            if (start < len(s))
            {
                e.Write(s[start..]);
            }
            e.WriteByte('"');
        }

        // A field represents a single field found in a struct.
        private partial struct field
        {
            public @string name;
            public slice<byte> nameBytes; // []byte(name)
            public Func<slice<byte>, slice<byte>, bool> equalFold; // bytes.EqualFold or equivalent

            public bool tag;
            public slice<long> index;
            public reflect.Type typ;
            public bool omitEmpty;
            public bool quoted;
        }

        private static field fillField(field f)
        {
            f.nameBytes = (slice<byte>)f.name;
            f.equalFold = foldFunc(f.nameBytes);
            return f;
        }

        // byIndex sorts field by index sequence.
        private partial struct byIndex // : slice<field>
        {
        }

        private static long Len(this byIndex x)
        {
            return len(x);
        }

        private static void Swap(this byIndex x, long i, long j)
        {
            x[i] = x[j];
            x[j] = x[i];

        }

        private static bool Less(this byIndex x, long i, long j)
        {
            foreach (var (k, xik) in x[i].index)
            {
                if (k >= len(x[j].index))
                {
                    return false;
                }
                if (xik != x[j].index[k])
                {
                    return xik < x[j].index[k];
                }
            }
            return len(x[i].index) < len(x[j].index);
        }

        // typeFields returns a list of fields that JSON should recognize for the given type.
        // The algorithm is breadth-first search over the set of structs to include - the top struct
        // and then any reachable anonymous structs.
        private static slice<field> typeFields(reflect.Type t)
        { 
            // Anonymous fields to explore at the current level and the next.
            field current = new slice<field>(new field[] {  });
            field next = new slice<field>(new field[] { {typ:t} }); 

            // Count of queued names for current level and the next.
            map count = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<reflect.Type, long>{};
            map nextCount = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<reflect.Type, long>{}; 

            // Types already visited at an earlier level.
            map visited = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<reflect.Type, bool>{}; 

            // Fields found.
            slice<field> fields = default;

            while (len(next) > 0L)
            {
                current = next;
                next = current[..0L];
                count = nextCount;
                nextCount = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<reflect.Type, long>{};

                foreach (var (_, f) in current)
                {
                    if (visited[f.typ])
                    {
                        continue;
                    }
                    visited[f.typ] = true; 

                    // Scan f.typ for fields to include.
                    {
                        long i__prev3 = i;

                        for (long i = 0L; i < f.typ.NumField(); i++)
                        {
                            var sf = f.typ.Field(i);
                            var isUnexported = sf.PkgPath != "";
                            if (sf.Anonymous)
                            {
                                var t = sf.Type;
                                if (t.Kind() == reflect.Ptr)
                                {
                                    t = t.Elem();
                                }
                                if (isUnexported && t.Kind() != reflect.Struct)
                                { 
                                    // Ignore embedded fields of unexported non-struct types.
                                    continue;
                                } 
                                // Do not ignore embedded fields of unexported struct types
                                // since they may have exported fields.
                            }
                            else if (isUnexported)
                            { 
                                // Ignore unexported non-embedded fields.
                                continue;
                            }
                            var tag = sf.Tag.Get("json");
                            if (tag == "-")
                            {
                                continue;
                            }
                            var (name, opts) = parseTag(tag);
                            if (!isValidTag(name))
                            {
                                name = "";
                            }
                            var index = make_slice<long>(len(f.index) + 1L);
                            copy(index, f.index);
                            index[len(f.index)] = i;

                            var ft = sf.Type;
                            if (ft.Name() == "" && ft.Kind() == reflect.Ptr)
                            { 
                                // Follow pointer.
                                ft = ft.Elem();
                            } 

                            // Only strings, floats, integers, and booleans can be quoted.
                            var quoted = false;
                            if (opts.Contains("string"))
                            {

                                if (ft.Kind() == reflect.Bool || ft.Kind() == reflect.Int || ft.Kind() == reflect.Int8 || ft.Kind() == reflect.Int16 || ft.Kind() == reflect.Int32 || ft.Kind() == reflect.Int64 || ft.Kind() == reflect.Uint || ft.Kind() == reflect.Uint8 || ft.Kind() == reflect.Uint16 || ft.Kind() == reflect.Uint32 || ft.Kind() == reflect.Uint64 || ft.Kind() == reflect.Uintptr || ft.Kind() == reflect.Float32 || ft.Kind() == reflect.Float64 || ft.Kind() == reflect.String) 
                                    quoted = true;
                                                            } 

                            // Record found field and index sequence.
                            if (name != "" || !sf.Anonymous || ft.Kind() != reflect.Struct)
                            {
                                var tagged = name != "";
                                if (name == "")
                                {
                                    name = sf.Name;
                                }
                                fields = append(fields, fillField(new field(name:name,tag:tagged,index:index,typ:ft,omitEmpty:opts.Contains("omitempty"),quoted:quoted,)));
                                if (count[f.typ] > 1L)
                                { 
                                    // If there were multiple instances, add a second,
                                    // so that the annihilation code will see a duplicate.
                                    // It only cares about the distinction between 1 or 2,
                                    // so don't bother generating any more copies.
                                    fields = append(fields, fields[len(fields) - 1L]);
                                }
                                continue;
                            } 

                            // Record new anonymous struct to explore in next round.
                            nextCount[ft]++;
                            if (nextCount[ft] == 1L)
                            {
                                next = append(next, fillField(new field(name:ft.Name(),index:index,typ:ft)));
                            }
                        }


                        i = i__prev3;
                    }
                }
            }


            sort.Slice(fields, (i, j) =>
            {
                var x = fields; 
                // sort field by name, breaking ties with depth, then
                // breaking ties with "name came from json tag", then
                // breaking ties with index sequence.
                if (x[i].name != x[j].name)
                {
                    return x[i].name < x[j].name;
                }
                if (len(x[i].index) != len(x[j].index))
                {
                    return len(x[i].index) < len(x[j].index);
                }
                if (x[i].tag != x[j].tag)
                {
                    return x[i].tag;
                }
                return byIndex(x).Less(i, j);
            }); 

            // Delete all fields that are hidden by the Go rules for embedded fields,
            // except that fields with JSON tags are promoted.

            // The fields are sorted in primary order of name, secondary order
            // of field index length. Loop over names; for each name, delete
            // hidden fields by choosing the one dominant field that survives.
            var @out = fields[..0L];
            {
                long i__prev1 = i;

                long advance = 0L;
                i = 0L;

                while (i < len(fields))
                { 
                    // One iteration per name.
                    // Find the sequence of fields with the name of this first field.
                    var fi = fields[i];
                    var name = fi.name;
                    for (advance = 1L; i + advance < len(fields); advance++)
                    {
                        var fj = fields[i + advance];
                        if (fj.name != name)
                        {
                            break;
                        }
                    i += advance;
                    }

                    if (advance == 1L)
                    { // Only one field with this name
                        out = append(out, fi);
                        continue;
                    }
                    var (dominant, ok) = dominantField(fields[i..i + advance]);
                    if (ok)
                    {
                        out = append(out, dominant);
                    }
                }


                i = i__prev1;
            }

            fields = out;
            sort.Sort(byIndex(fields));

            return fields;
        }

        // dominantField looks through the fields, all of which are known to
        // have the same name, to find the single field that dominates the
        // others using Go's embedding rules, modified by the presence of
        // JSON tags. If there are multiple top-level fields, the boolean
        // will be false: This condition is an error in Go and we skip all
        // the fields.
        private static (field, bool) dominantField(slice<field> fields)
        { 
            // The fields are sorted in increasing index-length order. The winner
            // must therefore be one with the shortest index length. Drop all
            // longer entries, which is easy: just truncate the slice.
            var length = len(fields[0L].index);
            long tagged = -1L; // Index of first tagged field.
            foreach (var (i, f) in fields)
            {
                if (len(f.index) > length)
                {
                    fields = fields[..i];
                    break;
                }
                if (f.tag)
                {
                    if (tagged >= 0L)
                    { 
                        // Multiple tagged fields at the same level: conflict.
                        // Return no field.
                        return (new field(), false);
                    }
                    tagged = i;
                }
            }
            if (tagged >= 0L)
            {
                return (fields[tagged], true);
            } 
            // All remaining fields have the same length. If there's more than one,
            // we have a conflict (two fields named "X" at the same level) and we
            // return no field.
            if (len(fields) > 1L)
            {
                return (new field(), false);
            }
            return (fields[0L], true);
        }

        private static var fieldCache = default;

        // cachedTypeFields is like typeFields but uses a cache to avoid repeated work.
        private static slice<field> cachedTypeFields(reflect.Type t)
        {
            map<reflect.Type, slice<field>> (m, _) = fieldCache.value.Load()._<map<reflect.Type, slice<field>>>();
            var f = m[t];
            if (f != null)
            {
                return f;
            } 

            // Compute fields without lock.
            // Might duplicate effort but won't hold other computations back.
            f = typeFields(t);
            if (f == null)
            {
                f = new slice<field>(new field[] {  });
            }
            fieldCache.mu.Lock();
            m, _ = fieldCache.value.Load()._<map<reflect.Type, slice<field>>>();
            var newM = make_map<reflect.Type, slice<field>>(len(m) + 1L);
            foreach (var (k, v) in m)
            {
                newM[k] = v;
            }
            newM[t] = f;
            fieldCache.value.Store(newM);
            fieldCache.mu.Unlock();
            return f;
        }
    }
}}
