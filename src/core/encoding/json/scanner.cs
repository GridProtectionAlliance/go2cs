// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.encoding;

// JSON value parser state machine.
// Just about at the limit of what is reasonable to write by hand.
// Some parts are a bit tedious, but overall it nicely factors out the
// otherwise common code from the multiple scanning functions
// in this package (Compact, Indent, checkValid, etc).
//
// This file starts with two simple examples using the scanner
// before diving into the scanner itself.
using strconv = strconv_package;
using sync = sync_package;

partial class json_package {

// Valid reports whether data is a valid JSON encoding.
public static bool Valid(slice<byte> data) => func((defer, _) => {
    var scan = newScanner();
    deferǃ(freeScanner, scan, defer);
    return checkValid(data, scan) == default!;
});

// checkValid verifies that data is valid JSON-encoded data.
// scan is passed in for use by checkValid to avoid an allocation.
// checkValid returns nil or a SyntaxError.
internal static error checkValid(slice<byte> data, ж<scanner> Ꮡscan) {
    ref var scan = ref Ꮡscan.val;

    scan.reset();
    foreach (var (_, c) in data) {
        scan.bytes++;
        if (scan.step(scan, c) == scanError) {
            return scan.err;
        }
    }
    if (scan.eof() == scanError) {
        return scan.err;
    }
    return default!;
}

// A SyntaxError is a description of a JSON syntax error.
// [Unmarshal] will return a SyntaxError if the JSON can't be parsed.
[GoType] partial struct SyntaxError {
    internal @string msg; // description of error
    public int64 Offset;  // error occurred after reading Offset bytes
}

[GoRecv] public static @string Error(this ref SyntaxError e) {
    return e.msg;
}

// A scanner is a JSON scanning state machine.
// Callers call scan.reset and then pass bytes in one at a time
// by calling scan.step(&scan, c) for each byte.
// The return value, referred to as an opcode, tells the
// caller about significant parsing events like beginning
// and ending literals, objects, and arrays, so that the
// caller can follow along if it wishes.
// The return value scanEnd indicates that a single top-level
// JSON value has been completed, *before* the byte that
// just got passed in.  (The indication must be delayed in order
// to recognize the end of numbers: is 123 a whole value or
// the beginning of 12345e+6?).
[GoType] partial struct scanner {
    // The step is a func to be called to execute the next transition.
    // Also tried using an integer constant and a single func
    // with a switch, but using the func directly was 10% faster
    // on a 64-bit Mac Mini, and it's nicer to read.
    internal Func<ж<scanner>, byte, nint> step;
    // Reached end of top-level value.
    internal bool endTop;
    // Stack of what we're in the middle of - array values, object keys, object values.
    internal slice<nint> parseState;
    // Error that happened, if any.
    internal error err;
    // total bytes consumed, updated by decoder.Decode (and deliberately
    // not set to zero by scan.reset)
    internal int64 bytes;
}

internal static sync.Pool scannerPool = new sync.Pool(
    New: () => Ꮡ(new scanner(nil))
);

internal static ж<scanner> newScanner() {
    var scan = scannerPool.Get()._<scanner.val>();
    // scan.reset by design doesn't set bytes to zero
    scan.val.bytes = 0;
    scan.reset();
    return scan;
}

internal static void freeScanner(ж<scanner> Ꮡscan) {
    ref var scan = ref Ꮡscan.val;

    // Avoid hanging on to too much memory in extreme cases.
    if (len(scan.parseState) > 1024) {
        scan.parseState = default!;
    }
    scannerPool.Put(scan);
}

// These values are returned by the state transition functions
// assigned to scanner.state and the method scanner.eof.
// They give details about the current state of the scan that
// callers might be interested to know about.
// It is okay to ignore the return value of any particular
// call to scanner.state: if one call returns scanError,
// every subsequent call will return scanError too.
internal static readonly UntypedInt scanContinue = iota; // uninteresting byte

internal static readonly UntypedInt scanBeginLiteral = 1; // end implied by next result != scanContinue

internal static readonly UntypedInt scanBeginObject = 2; // begin object

internal static readonly UntypedInt scanObjectKey = 3; // just finished object key (string)

internal static readonly UntypedInt scanObjectValue = 4; // just finished non-last object value

internal static readonly UntypedInt scanEndObject = 5; // end object (implies scanObjectValue if possible)

internal static readonly UntypedInt scanBeginArray = 6; // begin array

internal static readonly UntypedInt scanArrayValue = 7; // just finished array value

internal static readonly UntypedInt scanEndArray = 8; // end array (implies scanArrayValue if possible)

internal static readonly UntypedInt scanSkipSpace = 9; // space byte; can skip; known to be last "continue" result

internal static readonly UntypedInt scanEnd = 10; // top-level value ended *before* this byte; known to be first "stop" result

internal static readonly UntypedInt scanError = 11; // hit an error, scanner.err.

// These values are stored in the parseState stack.
// They give the current state of a composite value
// being scanned. If the parser is inside a nested value
// the parseState describes the nested state, outermost at entry 0.
internal static readonly UntypedInt parseObjectKey = iota; // parsing object key (before colon)

internal static readonly UntypedInt parseObjectValue = 1; // parsing object value (after colon)

internal static readonly UntypedInt parseArrayValue = 2; // parsing array value

// This limits the max nesting depth to prevent stack overflow.
// This is permitted by https://tools.ietf.org/html/rfc7159#section-9
internal static readonly UntypedInt maxNestingDepth = 10000;

// reset prepares the scanner for use.
// It must be called before calling s.step.
[GoRecv] internal static void reset(this ref scanner s) {
    s.step = stateBeginValue;
    s.parseState = s.parseState[0..0];
    s.err = default!;
    s.endTop = false;
}

// eof tells the scanner that the end of input has been reached.
// It returns a scan status just as s.step does.
[GoRecv] internal static nint eof(this ref scanner s) {
    if (s.err != default!) {
        return scanError;
    }
    if (s.endTop) {
        return scanEnd;
    }
    s.step(s, (rune)' ');
    if (s.endTop) {
        return scanEnd;
    }
    if (s.err == default!) {
        s.err = Ꮡ(new SyntaxError("unexpected end of JSON input", s.bytes));
    }
    return scanError;
}

// pushParseState pushes a new parse state p onto the parse stack.
// an error state is returned if maxNestingDepth was exceeded, otherwise successState is returned.
[GoRecv] internal static nint pushParseState(this ref scanner s, byte c, nint newParseState, nint successState) {
    s.parseState = append(s.parseState, newParseState);
    if (len(s.parseState) <= maxNestingDepth) {
        return successState;
    }
    return s.error(c, "exceeded max depth"u8);
}

// popParseState pops a parse state (already obtained) off the stack
// and updates s.step accordingly.
[GoRecv] internal static void popParseState(this ref scanner s) {
    nint n = len(s.parseState) - 1;
    s.parseState = s.parseState[0..(int)(n)];
    if (n == 0){
        s.step = stateEndTop;
        s.endTop = true;
    } else {
        s.step = stateEndValue;
    }
}

internal static bool isSpace(byte c) {
    return c <= (rune)' ' && (c == (rune)' ' || c == (rune)'\t' || c == (rune)'\r' || c == (rune)'\n');
}

// stateBeginValueOrEmpty is the state after reading `[`.
internal static nint stateBeginValueOrEmpty(ж<scanner> Ꮡs, byte c) {
    ref var s = ref Ꮡs.val;

    if (isSpace(c)) {
        return scanSkipSpace;
    }
    if (c == (rune)']') {
        return stateEndValue(Ꮡs, c);
    }
    return stateBeginValue(Ꮡs, c);
}

// stateBeginValue is the state at the beginning of the input.
internal static nint stateBeginValue(ж<scanner> Ꮡs, byte c) {
    ref var s = ref Ꮡs.val;

    if (isSpace(c)) {
        return scanSkipSpace;
    }
    switch (c) {
    case (rune)'{': {
        s.step = stateBeginStringOrEmpty;
        return s.pushParseState(c, parseObjectKey, scanBeginObject);
    }
    case (rune)'[': {
        s.step = stateBeginValueOrEmpty;
        return s.pushParseState(c, parseArrayValue, scanBeginArray);
    }
    case (rune)'"': {
        s.step = stateInString;
        return scanBeginLiteral;
    }
    case (rune)'-': {
        s.step = stateNeg;
        return scanBeginLiteral;
    }
    case (rune)'0': {
        s.step = state0;
        return scanBeginLiteral;
    }
    case (rune)'t': {
        s.step = stateT;
        return scanBeginLiteral;
    }
    case (rune)'f': {
        s.step = stateF;
        return scanBeginLiteral;
    }
    case (rune)'n': {
        s.step = stateN;
        return scanBeginLiteral;
    }}

    // beginning of 0.123
    // beginning of true
    // beginning of false
    // beginning of null
    if ((rune)'1' <= c && c <= (rune)'9') {
        // beginning of 1234.5
        s.step = state1;
        return scanBeginLiteral;
    }
    return s.error(c, "looking for beginning of value"u8);
}

// stateBeginStringOrEmpty is the state after reading `{`.
internal static nint stateBeginStringOrEmpty(ж<scanner> Ꮡs, byte c) {
    ref var s = ref Ꮡs.val;

    if (isSpace(c)) {
        return scanSkipSpace;
    }
    if (c == (rune)'}') {
        nint n = len(s.parseState);
        s.parseState[n - 1] = parseObjectValue;
        return stateEndValue(Ꮡs, c);
    }
    return stateBeginString(Ꮡs, c);
}

// stateBeginString is the state after reading `{"key": value,`.
internal static nint stateBeginString(ж<scanner> Ꮡs, byte c) {
    ref var s = ref Ꮡs.val;

    if (isSpace(c)) {
        return scanSkipSpace;
    }
    if (c == (rune)'"') {
        s.step = stateInString;
        return scanBeginLiteral;
    }
    return s.error(c, "looking for beginning of object key string"u8);
}

// stateEndValue is the state after completing a value,
// such as after reading `{}` or `true` or `["x"`.
internal static nint stateEndValue(ж<scanner> Ꮡs, byte c) {
    ref var s = ref Ꮡs.val;

    nint n = len(s.parseState);
    if (n == 0) {
        // Completed top-level before the current byte.
        s.step = stateEndTop;
        s.endTop = true;
        return stateEndTop(Ꮡs, c);
    }
    if (isSpace(c)) {
        s.step = stateEndValue;
        return scanSkipSpace;
    }
    nint ps = s.parseState[n - 1];
    var exprᴛ1 = ps;
    if (exprᴛ1 == parseObjectKey) {
        if (c == (rune)':') {
            s.parseState[n - 1] = parseObjectValue;
            s.step = stateBeginValue;
            return scanObjectKey;
        }
        return s.error(c, "after object key"u8);
    }
    if (exprᴛ1 == parseObjectValue) {
        if (c == (rune)',') {
            s.parseState[n - 1] = parseObjectKey;
            s.step = stateBeginString;
            return scanObjectValue;
        }
        if (c == (rune)'}') {
            s.popParseState();
            return scanEndObject;
        }
        return s.error(c, "after object key:value pair"u8);
    }
    if (exprᴛ1 == parseArrayValue) {
        if (c == (rune)',') {
            s.step = stateBeginValue;
            return scanArrayValue;
        }
        if (c == (rune)']') {
            s.popParseState();
            return scanEndArray;
        }
        return s.error(c, "after array element"u8);
    }

    return s.error(c, ""u8);
}

// stateEndTop is the state after finishing the top-level value,
// such as after reading `{}` or `[1,2,3]`.
// Only space characters should be seen now.
internal static nint stateEndTop(ж<scanner> Ꮡs, byte c) {
    ref var s = ref Ꮡs.val;

    if (!isSpace(c)) {
        // Complain about non-space byte on next call.
        s.error(c, "after top-level value"u8);
    }
    return scanEnd;
}

// stateInString is the state after reading `"`.
internal static nint stateInString(ж<scanner> Ꮡs, byte c) {
    ref var s = ref Ꮡs.val;

    if (c == (rune)'"') {
        s.step = stateEndValue;
        return scanContinue;
    }
    if (c == (rune)'\\') {
        s.step = stateInStringEsc;
        return scanContinue;
    }
    if (c < 32) {
        return s.error(c, "in string literal"u8);
    }
    return scanContinue;
}

// stateInStringEsc is the state after reading `"\` during a quoted string.
internal static nint stateInStringEsc(ж<scanner> Ꮡs, byte c) {
    ref var s = ref Ꮡs.val;

    switch (c) {
    case (rune)'b' or (rune)'f' or (rune)'n' or (rune)'r' or (rune)'t' or (rune)'\\' or (rune)'/' or (rune)'"': {
        s.step = stateInString;
        return scanContinue;
    }
    case (rune)'u': {
        s.step = stateInStringEscU;
        return scanContinue;
    }}

    return s.error(c, "in string escape code"u8);
}

// stateInStringEscU is the state after reading `"\u` during a quoted string.
internal static nint stateInStringEscU(ж<scanner> Ꮡs, byte c) {
    ref var s = ref Ꮡs.val;

    if ((rune)'0' <= c && c <= (rune)'9' || (rune)'a' <= c && c <= (rune)'f' || (rune)'A' <= c && c <= (rune)'F') {
        s.step = stateInStringEscU1;
        return scanContinue;
    }
    // numbers
    return s.error(c, "in \\u hexadecimal character escape"u8);
}

// stateInStringEscU1 is the state after reading `"\u1` during a quoted string.
internal static nint stateInStringEscU1(ж<scanner> Ꮡs, byte c) {
    ref var s = ref Ꮡs.val;

    if ((rune)'0' <= c && c <= (rune)'9' || (rune)'a' <= c && c <= (rune)'f' || (rune)'A' <= c && c <= (rune)'F') {
        s.step = stateInStringEscU12;
        return scanContinue;
    }
    // numbers
    return s.error(c, "in \\u hexadecimal character escape"u8);
}

// stateInStringEscU12 is the state after reading `"\u12` during a quoted string.
internal static nint stateInStringEscU12(ж<scanner> Ꮡs, byte c) {
    ref var s = ref Ꮡs.val;

    if ((rune)'0' <= c && c <= (rune)'9' || (rune)'a' <= c && c <= (rune)'f' || (rune)'A' <= c && c <= (rune)'F') {
        s.step = stateInStringEscU123;
        return scanContinue;
    }
    // numbers
    return s.error(c, "in \\u hexadecimal character escape"u8);
}

// stateInStringEscU123 is the state after reading `"\u123` during a quoted string.
internal static nint stateInStringEscU123(ж<scanner> Ꮡs, byte c) {
    ref var s = ref Ꮡs.val;

    if ((rune)'0' <= c && c <= (rune)'9' || (rune)'a' <= c && c <= (rune)'f' || (rune)'A' <= c && c <= (rune)'F') {
        s.step = stateInString;
        return scanContinue;
    }
    // numbers
    return s.error(c, "in \\u hexadecimal character escape"u8);
}

// stateNeg is the state after reading `-` during a number.
internal static nint stateNeg(ж<scanner> Ꮡs, byte c) {
    ref var s = ref Ꮡs.val;

    if (c == (rune)'0') {
        s.step = state0;
        return scanContinue;
    }
    if ((rune)'1' <= c && c <= (rune)'9') {
        s.step = state1;
        return scanContinue;
    }
    return s.error(c, "in numeric literal"u8);
}

// state1 is the state after reading a non-zero integer during a number,
// such as after reading `1` or `100` but not `0`.
internal static nint state1(ж<scanner> Ꮡs, byte c) {
    ref var s = ref Ꮡs.val;

    if ((rune)'0' <= c && c <= (rune)'9') {
        s.step = state1;
        return scanContinue;
    }
    return state0(Ꮡs, c);
}

// state0 is the state after reading `0` during a number.
internal static nint state0(ж<scanner> Ꮡs, byte c) {
    ref var s = ref Ꮡs.val;

    if (c == (rune)'.') {
        s.step = stateDot;
        return scanContinue;
    }
    if (c == (rune)'e' || c == (rune)'E') {
        s.step = stateE;
        return scanContinue;
    }
    return stateEndValue(Ꮡs, c);
}

// stateDot is the state after reading the integer and decimal point in a number,
// such as after reading `1.`.
internal static nint stateDot(ж<scanner> Ꮡs, byte c) {
    ref var s = ref Ꮡs.val;

    if ((rune)'0' <= c && c <= (rune)'9') {
        s.step = stateDot0;
        return scanContinue;
    }
    return s.error(c, "after decimal point in numeric literal"u8);
}

// stateDot0 is the state after reading the integer, decimal point, and subsequent
// digits of a number, such as after reading `3.14`.
internal static nint stateDot0(ж<scanner> Ꮡs, byte c) {
    ref var s = ref Ꮡs.val;

    if ((rune)'0' <= c && c <= (rune)'9') {
        return scanContinue;
    }
    if (c == (rune)'e' || c == (rune)'E') {
        s.step = stateE;
        return scanContinue;
    }
    return stateEndValue(Ꮡs, c);
}

// stateE is the state after reading the mantissa and e in a number,
// such as after reading `314e` or `0.314e`.
internal static nint stateE(ж<scanner> Ꮡs, byte c) {
    ref var s = ref Ꮡs.val;

    if (c == (rune)'+' || c == (rune)'-') {
        s.step = stateESign;
        return scanContinue;
    }
    return stateESign(Ꮡs, c);
}

// stateESign is the state after reading the mantissa, e, and sign in a number,
// such as after reading `314e-` or `0.314e+`.
internal static nint stateESign(ж<scanner> Ꮡs, byte c) {
    ref var s = ref Ꮡs.val;

    if ((rune)'0' <= c && c <= (rune)'9') {
        s.step = stateE0;
        return scanContinue;
    }
    return s.error(c, "in exponent of numeric literal"u8);
}

// stateE0 is the state after reading the mantissa, e, optional sign,
// and at least one digit of the exponent in a number,
// such as after reading `314e-2` or `0.314e+1` or `3.14e0`.
internal static nint stateE0(ж<scanner> Ꮡs, byte c) {
    ref var s = ref Ꮡs.val;

    if ((rune)'0' <= c && c <= (rune)'9') {
        return scanContinue;
    }
    return stateEndValue(Ꮡs, c);
}

// stateT is the state after reading `t`.
internal static nint stateT(ж<scanner> Ꮡs, byte c) {
    ref var s = ref Ꮡs.val;

    if (c == (rune)'r') {
        s.step = stateTr;
        return scanContinue;
    }
    return s.error(c, "in literal true (expecting 'r')"u8);
}

// stateTr is the state after reading `tr`.
internal static nint stateTr(ж<scanner> Ꮡs, byte c) {
    ref var s = ref Ꮡs.val;

    if (c == (rune)'u') {
        s.step = stateTru;
        return scanContinue;
    }
    return s.error(c, "in literal true (expecting 'u')"u8);
}

// stateTru is the state after reading `tru`.
internal static nint stateTru(ж<scanner> Ꮡs, byte c) {
    ref var s = ref Ꮡs.val;

    if (c == (rune)'e') {
        s.step = stateEndValue;
        return scanContinue;
    }
    return s.error(c, "in literal true (expecting 'e')"u8);
}

// stateF is the state after reading `f`.
internal static nint stateF(ж<scanner> Ꮡs, byte c) {
    ref var s = ref Ꮡs.val;

    if (c == (rune)'a') {
        s.step = stateFa;
        return scanContinue;
    }
    return s.error(c, "in literal false (expecting 'a')"u8);
}

// stateFa is the state after reading `fa`.
internal static nint stateFa(ж<scanner> Ꮡs, byte c) {
    ref var s = ref Ꮡs.val;

    if (c == (rune)'l') {
        s.step = stateFal;
        return scanContinue;
    }
    return s.error(c, "in literal false (expecting 'l')"u8);
}

// stateFal is the state after reading `fal`.
internal static nint stateFal(ж<scanner> Ꮡs, byte c) {
    ref var s = ref Ꮡs.val;

    if (c == (rune)'s') {
        s.step = stateFals;
        return scanContinue;
    }
    return s.error(c, "in literal false (expecting 's')"u8);
}

// stateFals is the state after reading `fals`.
internal static nint stateFals(ж<scanner> Ꮡs, byte c) {
    ref var s = ref Ꮡs.val;

    if (c == (rune)'e') {
        s.step = stateEndValue;
        return scanContinue;
    }
    return s.error(c, "in literal false (expecting 'e')"u8);
}

// stateN is the state after reading `n`.
internal static nint stateN(ж<scanner> Ꮡs, byte c) {
    ref var s = ref Ꮡs.val;

    if (c == (rune)'u') {
        s.step = stateNu;
        return scanContinue;
    }
    return s.error(c, "in literal null (expecting 'u')"u8);
}

// stateNu is the state after reading `nu`.
internal static nint stateNu(ж<scanner> Ꮡs, byte c) {
    ref var s = ref Ꮡs.val;

    if (c == (rune)'l') {
        s.step = stateNul;
        return scanContinue;
    }
    return s.error(c, "in literal null (expecting 'l')"u8);
}

// stateNul is the state after reading `nul`.
internal static nint stateNul(ж<scanner> Ꮡs, byte c) {
    ref var s = ref Ꮡs.val;

    if (c == (rune)'l') {
        s.step = stateEndValue;
        return scanContinue;
    }
    return s.error(c, "in literal null (expecting 'l')"u8);
}

// stateError is the state after reaching a syntax error,
// such as after reading `[1}` or `5.1.2`.
internal static nint stateError(ж<scanner> Ꮡs, byte c) {
    ref var s = ref Ꮡs.val;

    return scanError;
}

// error records an error and switches to the error state.
[GoRecv] internal static nint error(this ref scanner s, byte c, @string context) {
    s.step = stateError;
    s.err = Ꮡ(new SyntaxError("invalid character "u8 + quoteChar(c) + " "u8 + context, s.bytes));
    return scanError;
}

// quoteChar formats c as a quoted character literal.
internal static @string quoteChar(byte c) {
    // special cases - different from quoted strings
    if (c == (rune)'\'') {
        return @"'\''"u8;
    }
    if (c == (rune)'"') {
        return @"'""'"u8;
    }
    // use quoted string with different quotation marks
    @string s = strconv.Quote(((@string)c));
    return "'" + s[1..(int)(len(s) - 1)] + "'";
}

} // end json_package
