// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using bytes = bytes_package;
using fmt = fmt_package;
using ast = go.ast_package;
using parser = go.parser_package;
using token = go.token_package;
using math = math_package;
using strconv = strconv_package;
using strings = strings_package;
using utf8 = unicode.utf8_package;
using go;
using unicode;
using ꓸꓸꓸany = Span<any>;

partial class fuzz_package {

// encVersion1 will be the first line of a file with version 1 encoding.
internal static @string encVersion1 = "go test fuzz v1"u8;

// marshalCorpusFile encodes an arbitrary number of arguments into the file format for the
// corpus.
internal static slice<byte> marshalCorpusFile(params ꓸꓸꓸany valsʗp) {
    var vals = valsʗp.slice();

    if (len(vals) == 0) {
        throw panic("must have at least one value to marshal");
    }
    var b = bytes.NewBuffer(slice<byte>(encVersion1 + "\n"u8));
    // TODO(katiehockman): keep uint8 and int32 encoding where applicable,
    // instead of changing to byte and rune respectively.
    foreach (var (_, val) in vals) {
        switch (val.type()) {
        case nint t: {
            fmt.Fprintf(~b, "%T(%v)\n"u8, t, t);
            break;
        }
        case int32 t: {
            fmt.Fprintf(~b, "%T(%v)\n"u8, t, t);
            break;
        }
        case int8 t: {
            fmt.Fprintf(~b, "%T(%v)\n"u8, t, t);
            break;
        }
        case int16 t: {
            fmt.Fprintf(~b, "%T(%v)\n"u8, t, t);
            break;
        }
        case int64 t: {
            fmt.Fprintf(~b, "%T(%v)\n"u8, t, t);
            break;
        }
        case nuint t: {
            fmt.Fprintf(~b, "%T(%v)\n"u8, t, t);
            break;
        }
        case uint32 t: {
            fmt.Fprintf(~b, "%T(%v)\n"u8, t, t);
            break;
        }
        case uint16 t: {
            fmt.Fprintf(~b, "%T(%v)\n"u8, t, t);
            break;
        }
        case uint32 t: {
            fmt.Fprintf(~b, "%T(%v)\n"u8, t, t);
            break;
        }
        case uint64 t: {
            fmt.Fprintf(~b, "%T(%v)\n"u8, t, t);
            break;
        }
        case bool t: {
            fmt.Fprintf(~b, "%T(%v)\n"u8, t, t);
            break;
        }
        case float32 t: {
            if (math.IsNaN(((float64)t)) && math.Float32bits(t) != math.Float32bits(((float32)math.NaN()))){
                // We encode unusual NaNs as hex values, because that is how users are
                // likely to encounter them in literature about floating-point encoding.
                // This allows us to reproduce fuzz failures that depend on the specific
                // NaN representation (for float32 there are about 2^24 possibilities!),
                // not just the fact that the value is *a* NaN.
                //
                // Note that the specific value of float32(math.NaN()) can vary based on
                // whether the architecture represents signaling NaNs using a low bit
                // (as is common) or a high bit (as commonly implemented on MIPS
                // hardware before around 2012). We believe that the increase in clarity
                // from identifying "NaN" with math.NaN() is worth the slight ambiguity
                // from a platform-dependent value.
                fmt.Fprintf(~b, "math.Float32frombits(0x%x)\n"u8, math.Float32bits(t));
            } else {
                // We encode all other values — including the NaN value that is
                // bitwise-identical to float32(math.Nan()) — using the default
                // formatting, which is equivalent to strconv.FormatFloat with format
                // 'g' and can be parsed by strconv.ParseFloat.
                //
                // For an ordinary floating-point number this format includes
                // sufficiently many digits to reconstruct the exact value. For positive
                // or negative infinity it is the string "+Inf" or "-Inf". For positive
                // or negative zero it is "0" or "-0". For NaN, it is the string "NaN".
                fmt.Fprintf(~b, "%T(%v)\n"u8, t, t);
            }
            break;
        }
        case float64 t: {
            if (math.IsNaN(t) && math.Float64bits(t) != math.Float64bits(math.NaN())){
                fmt.Fprintf(~b, "math.Float64frombits(0x%x)\n"u8, math.Float64bits(t));
            } else {
                fmt.Fprintf(~b, "%T(%v)\n"u8, t, t);
            }
            break;
        }
        case @string t: {
            fmt.Fprintf(~b, "string(%q)\n"u8, t);
            break;
        }
        case rune t: {
            if (utf8.ValidRune(t)){
                // int32
                // Although rune and int32 are represented by the same type, only a subset
                // of valid int32 values can be expressed as rune literals. Notably,
                // negative numbers, surrogate halves, and values above unicode.MaxRune
                // have no quoted representation.
                //
                // fmt with "%q" (and the corresponding functions in the strconv package)
                // would quote out-of-range values to the Unicode replacement character
                // instead of the original value (see https://go.dev/issue/51526), so
                // they must be treated as int32 instead.
                //
                // We arbitrarily draw the line at UTF-8 validity, which biases toward the
                // "rune" interpretation. (However, we accept either format as input.)
                fmt.Fprintf(~b, "rune(%q)\n"u8, t);
            } else {
                fmt.Fprintf(~b, "int32(%v)\n"u8, t);
            }
            break;
        }
        case byte t: {
            fmt.Fprintf(~b, // uint8
 // For bytes, we arbitrarily prefer the character interpretation.
 // (Every byte has a valid character encoding.)
 "byte(%q)\n"u8, t);
            break;
        }
        case slice<byte> t: {
            fmt.Fprintf(~b, // []uint8
 "[]byte(%q)\n"u8, t);
            break;
        }
        default: {
            var t = val.type();
            throw panic(fmt.Sprintf("unsupported type: %T"u8, t));
            break;
        }}
    }
    return b.Bytes();
}

// unmarshalCorpusFile decodes corpus bytes into their respective values.
internal static (slice<any>, error) unmarshalCorpusFile(slice<byte> b) {
    if (len(b) == 0) {
        return (default!, fmt.Errorf("cannot unmarshal empty string"u8));
    }
    var lines = bytes.Split(b, slice<byte>("\n"));
    if (len(lines) < 2) {
        return (default!, fmt.Errorf("must include version and at least one value"u8));
    }
    @string version = strings.TrimSuffix(((@string)lines[0]), "\r"u8);
    if (version != encVersion1) {
        return (default!, fmt.Errorf("unknown encoding version: %s"u8, version));
    }
    slice<any> vals = default!;
    foreach (var (_, line) in lines[1..]) {
        line = bytes.TrimSpace(line);
        if (len(line) == 0) {
            continue;
        }
        (v, err) = parseCorpusValue(line);
        if (err != default!) {
            return (default!, fmt.Errorf("malformed line %q: %v"u8, line, err));
        }
        vals = append(vals, v);
    }
    return (vals, default!);
}

internal static (any, error) parseCorpusValue(slice<byte> line) {
    var fs = token.NewFileSet();
    (expr, err) = parser.ParseExprFrom(fs, "(test)"u8, line, 0);
    if (err != default!) {
        return (default!, err);
    }
    var (call, ok) = expr._<ж<ast.CallExpr>>(ᐧ);
    if (!ok) {
        return (default!, fmt.Errorf("expected call expression"u8));
    }
    if (len((~call).Args) != 1) {
        return (default!, fmt.Errorf("expected call expression with 1 argument; got %d"u8, len((~call).Args)));
    }
    var arg = (~call).Args[0];
    {
        var (arrayType, okΔ1) = (~call).Fun._<ж<ast.ArrayType>>(ᐧ); if (okΔ1) {
            if ((~arrayType).Len != default!) {
                return (default!, fmt.Errorf("expected []byte or primitive type"u8));
            }
            var (elt, ok) = (~arrayType).Elt._<ж<ast.Ident>>(ᐧ);
            if (!ok || (~elt).Name != "byte"u8) {
                return (default!, fmt.Errorf("expected []byte"u8));
            }
            (lit, ok) = arg._<ж<ast.BasicLit>>(ᐧ);
            if (!ok || (~lit).Kind != token.STRING) {
                return (default!, fmt.Errorf("string literal required for type []byte"u8));
            }
            var (s, err) = strconv.Unquote((~lit).Value);
            if (err != default!) {
                return (default!, err);
            }
            return (slice<byte>(s), default!);
        }
    }
    ж<ast.Ident> idType = default!;
    {
        var (selector, okΔ2) = (~call).Fun._<ж<ast.SelectorExpr>>(ᐧ); if (okΔ2){
            var (xIdent, okΔ3) = (~selector).X._<ж<ast.Ident>>(ᐧ);
            if (!okΔ3 || (~xIdent).Name != "math"u8) {
                return (default!, fmt.Errorf("invalid selector type"u8));
            }
            var exprᴛ1 = (~(~selector).Sel).Name;
            if (exprᴛ1 == "Float64frombits"u8) {
                idType = Ꮡ(new ast.Ident(Name: "float64-bits"u8));
            }
            else if (exprᴛ1 == "Float32frombits"u8) {
                idType = Ꮡ(new ast.Ident(Name: "float32-bits"u8));
            }
            else { /* default: */
                return (default!, fmt.Errorf("invalid selector type"u8));
            }

        } else {
            (idType, ok) = (~call).Fun._<ж<ast.Ident>>(ᐧ);
            if (!okΔ2) {
                return (default!, fmt.Errorf("expected []byte or primitive type"u8));
            }
            if ((~idType).Name == "bool"u8) {
                var (id, okΔ4) = arg._<ж<ast.Ident>>(ᐧ);
                if (!okΔ4) {
                    return (default!, fmt.Errorf("malformed bool"u8));
                }
                if ((~id).Name == "true"u8){
                    return (true, default!);
                } else 
                if ((~id).Name == "false"u8){
                    return (false, default!);
                } else {
                    return (default!, fmt.Errorf("true or false required for type bool"u8));
                }
            }
        }
    }
    @string val = default!;
    token.Token kind = default!;
    {
        var (op, okΔ5) = arg._<ж<ast.UnaryExpr>>(ᐧ); if (okΔ5){
            switch ((~op).X.type()) {
            case ж<ast.BasicLit> lit: {
                if ((~op).Op != token.SUB) {
                    return (default!, fmt.Errorf("unsupported operation on int/float: %v"u8, (~op).Op));
                }
                val = (~op).Op.String() + (~lit).Value;
                kind = lit.val.Kind;
                break;
            }
            case ж<ast.Ident> lit: {
                if ((~lit).Name != "Inf"u8) {
                    // Special case for negative numbers.
                    // e.g. "-" + "124"
                    return (default!, fmt.Errorf("expected operation on int or float type"u8));
                }
                if ((~op).Op == token.SUB){
                    val = "-Inf"u8;
                } else {
                    val = "+Inf"u8;
                }
                kind = token.FLOAT;
                break;
            }
            default: {
                var lit = (~op).X.type();
                return (default!, fmt.Errorf("expected operation on int or float type"u8));
            }}
        } else {
            switch (arg.type()) {
            case ж<ast.BasicLit> lit: {
                (val, kind) = (lit.val.Value, lit.val.Kind);
                break;
            }
            case ж<ast.Ident> lit: {
                if ((~lit).Name != "NaN"u8) {
                    return (default!, fmt.Errorf("literal value required for primitive type"u8));
                }
                (val, kind) = ("NaN"u8, token.FLOAT);
                break;
            }
            default: {
                var lit = arg.type();
                return (default!, fmt.Errorf("literal value required for primitive type"u8));
            }}
        }
    }
    {
        @string typ = idType.val.Name;
        var exprᴛ2 = typ;
        if (exprᴛ2 == "string"u8) {
            if (kind != token.STRING) {
                return (default!, fmt.Errorf("string literal value required for type string"u8));
            }
            return strconv.Unquote(val);
        }
        if (exprᴛ2 == "byte"u8 || exprᴛ2 == "rune"u8) {
            if (kind == token.INT) {
                var exprᴛ3 = typ;
                if (exprᴛ3 == "rune"u8) {
                    return parseInt(val, typ);
                }
                if (exprᴛ3 == "byte"u8) {
                    return parseUint(val, typ);
                }

            }
            if (kind != token.CHAR) {
                return (default!, fmt.Errorf("character literal required for byte/rune types"u8));
            }
            nint n = len(val);
            if (n < 2) {
                return (default!, fmt.Errorf("malformed character literal, missing single quotes"u8));
            }
            var (code, _, _, err) = strconv.UnquoteChar(val[1..(int)(n - 1)], (rune)'\'');
            if (err != default!) {
                return (default!, err);
            }
            if (typ == "rune"u8) {
                return (code, default!);
            }
            if (code >= 256) {
                return (default!, fmt.Errorf("can only encode single byte to a byte type"u8));
            }
            return (((byte)code), default!);
        }
        if (exprᴛ2 == "int"u8 || exprᴛ2 == "int8"u8 || exprᴛ2 == "int16"u8 || exprᴛ2 == "int32"u8 || exprᴛ2 == "int64"u8) {
            if (kind != token.INT) {
                return (default!, fmt.Errorf("integer literal required for int types"u8));
            }
            return parseInt(val, typ);
        }
        if (exprᴛ2 == "uint"u8 || exprᴛ2 == "uint8"u8 || exprᴛ2 == "uint16"u8 || exprᴛ2 == "uint32"u8 || exprᴛ2 == "uint64"u8) {
            if (kind != token.INT) {
                return (default!, fmt.Errorf("integer literal required for uint types"u8));
            }
            return parseUint(val, typ);
        }
        if (exprᴛ2 == "float32"u8) {
            if (kind != token.FLOAT && kind != token.INT) {
                return (default!, fmt.Errorf("float or integer literal required for float32 type"u8));
            }
            var (v, err) = strconv.ParseFloat(val, 32);
            return (((float32)v), err);
        }
        if (exprᴛ2 == "float64"u8) {
            if (kind != token.FLOAT && kind != token.INT) {
                return (default!, fmt.Errorf("float or integer literal required for float64 type"u8));
            }
            return strconv.ParseFloat(val, 64);
        }
        if (exprᴛ2 == "float32-bits"u8) {
            if (kind != token.INT) {
                return (default!, fmt.Errorf("integer literal required for math.Float32frombits type"u8));
            }
            (bits, err) = parseUint(val, "uint32"u8);
            if (err != default!) {
                return (default!, err);
            }
            return (math.Float32frombits(bits._<uint32>()), default!);
        }
        if (exprᴛ2 == "float64-bits"u8) {
            if (kind != token.FLOAT && kind != token.INT) {
                return (default!, fmt.Errorf("integer literal required for math.Float64frombits type"u8));
            }
            (bits, err) = parseUint(val, "uint64"u8);
            if (err != default!) {
                return (default!, err);
            }
            return (math.Float64frombits(bits._<uint64>()), default!);
        }
        { /* default: */
            return (default!, fmt.Errorf("expected []byte or primitive type"u8));
        }
    }

}

// parseInt returns an integer of value val and type typ.
internal static (any, error) parseInt(@string val, @string typ) {
    var exprᴛ1 = typ;
    if (exprᴛ1 == "int"u8) {
        var (i, err) = strconv.ParseInt(val, // The int type may be either 32 or 64 bits. If 32, the fuzz tests in the
 // corpus may include 64-bit values produced by fuzzing runs on 64-bit
 // architectures. When running those tests, we implicitly wrap the values to
 // fit in a regular int. (The test case is still “interesting”, even if the
 // specific values of its inputs are platform-dependent.)
 0, 64);
        return (((nint)i), err);
    }
    if (exprᴛ1 == "int8"u8) {
        var (i, err) = strconv.ParseInt(val, 0, 8);
        return (((int8)i), err);
    }
    if (exprᴛ1 == "int16"u8) {
        var (i, err) = strconv.ParseInt(val, 0, 16);
        return (((int16)i), err);
    }
    if (exprᴛ1 == "int32"u8 || exprᴛ1 == "rune"u8) {
        var (i, err) = strconv.ParseInt(val, 0, 32);
        return (((int32)i), err);
    }
    if (exprᴛ1 == "int64"u8) {
        return strconv.ParseInt(val, 0, 64);
    }
    { /* default: */
        throw panic("unreachable");
    }

}

// parseUint returns an unsigned integer of value val and type typ.
internal static (any, error) parseUint(@string val, @string typ) {
    var exprᴛ1 = typ;
    if (exprᴛ1 == "uint"u8) {
        var (i, err) = strconv.ParseUint(val, 0, 64);
        return (((nuint)i), err);
    }
    if (exprᴛ1 == "uint8"u8 || exprᴛ1 == "byte"u8) {
        var (i, err) = strconv.ParseUint(val, 0, 8);
        return (((uint8)i), err);
    }
    if (exprᴛ1 == "uint16"u8) {
        var (i, err) = strconv.ParseUint(val, 0, 16);
        return (((uint16)i), err);
    }
    if (exprᴛ1 == "uint32"u8) {
        var (i, err) = strconv.ParseUint(val, 0, 32);
        return (((uint32)i), err);
    }
    if (exprᴛ1 == "uint64"u8) {
        return strconv.ParseUint(val, 0, 64);
    }
    { /* default: */
        throw panic("unreachable");
    }

}

} // end fuzz_package
