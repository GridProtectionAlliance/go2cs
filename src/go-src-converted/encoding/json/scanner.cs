// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package json -- go2cs converted at 2020 August 29 08:35:52 UTC
// import "encoding/json" ==> using json = go.encoding.json_package
// Original source: C:\Go\src\encoding\json\scanner.go
// JSON value parser state machine.
// Just about at the limit of what is reasonable to write by hand.
// Some parts are a bit tedious, but overall it nicely factors out the
// otherwise common code from the multiple scanning functions
// in this package (Compact, Indent, checkValid, nextValue, etc).
//
// This file starts with two simple examples using the scanner
// before diving into the scanner itself.

using strconv = go.strconv_package;
using static go.builtin;
using System;

namespace go {
namespace encoding
{
    public static partial class json_package
    {
        // Valid reports whether data is a valid JSON encoding.
        public static bool Valid(slice<byte> data)
        {
            return checkValid(data, ref new scanner()) == null;
        }

        // checkValid verifies that data is valid JSON-encoded data.
        // scan is passed in for use by checkValid to avoid an allocation.
        private static error checkValid(slice<byte> data, ref scanner scan)
        {
            scan.reset();
            foreach (var (_, c) in data)
            {
                scan.bytes++;
                if (scan.step(scan, c) == scanError)
                {
                    return error.As(scan.err);
                }
            }
            if (scan.eof() == scanError)
            {
                return error.As(scan.err);
            }
            return error.As(null);
        }

        // nextValue splits data after the next whole JSON value,
        // returning that value and the bytes that follow it as separate slices.
        // scan is passed in for use by nextValue to avoid an allocation.
        private static (slice<byte>, slice<byte>, error) nextValue(slice<byte> data, ref scanner scan)
        {
            scan.reset();
            foreach (var (i, c) in data)
            {
                var v = scan.step(scan, c);
                if (v >= scanEndObject)
                {

                    // probe the scanner with a space to determine whether we will
                    // get scanEnd on the next character. Otherwise, if the next character
                    // is not a space, scanEndTop allocates a needless error.
                    if (v == scanEndObject || v == scanEndArray) 
                        if (scan.step(scan, ' ') == scanEnd)
                        {
                            return (data[..i + 1L], data[i + 1L..], null);
                        }
                    else if (v == scanError) 
                        return (null, null, scan.err);
                    else if (v == scanEnd) 
                        return (data[..i], data[i..], null);
                                    }
            }
            if (scan.eof() == scanError)
            {
                return (null, null, scan.err);
            }
            return (data, null, null);
        }

        // A SyntaxError is a description of a JSON syntax error.
        public partial struct SyntaxError
        {
            public @string msg; // description of error
            public long Offset; // error occurred after reading Offset bytes
        }

        private static @string Error(this ref SyntaxError e)
        {
            return e.msg;
        }

        // A scanner is a JSON scanning state machine.
        // Callers call scan.reset() and then pass bytes in one at a time
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
        private partial struct scanner
        {
            public Func<ref scanner, byte, long> step; // Reached end of top-level value.
            public bool endTop; // Stack of what we're in the middle of - array values, object keys, object values.
            public slice<long> parseState; // Error that happened, if any.
            public error err; // 1-byte redo (see undo method)
            public bool redo;
            public long redoCode;
            public Func<ref scanner, byte, long> redoState; // total bytes consumed, updated by decoder.Decode
            public long bytes;
        }

        // These values are returned by the state transition functions
        // assigned to scanner.state and the method scanner.eof.
        // They give details about the current state of the scan that
        // callers might be interested to know about.
        // It is okay to ignore the return value of any particular
        // call to scanner.state: if one call returns scanError,
        // every subsequent call will return scanError too.
 
        // Continue.
        private static readonly var scanContinue = iota; // uninteresting byte
        private static readonly var scanBeginLiteral = 0; // end implied by next result != scanContinue
        private static readonly var scanBeginObject = 1; // begin object
        private static readonly var scanObjectKey = 2; // just finished object key (string)
        private static readonly var scanObjectValue = 3; // just finished non-last object value
        private static readonly var scanEndObject = 4; // end object (implies scanObjectValue if possible)
        private static readonly var scanBeginArray = 5; // begin array
        private static readonly var scanArrayValue = 6; // just finished array value
        private static readonly var scanEndArray = 7; // end array (implies scanArrayValue if possible)
        private static readonly var scanSkipSpace = 8; // space byte; can skip; known to be last "continue" result

        // Stop.
        private static readonly var scanEnd = 9; // top-level value ended *before* this byte; known to be first "stop" result
        private static readonly var scanError = 10; // hit an error, scanner.err.

        // These values are stored in the parseState stack.
        // They give the current state of a composite value
        // being scanned. If the parser is inside a nested value
        // the parseState describes the nested state, outermost at entry 0.
        private static readonly var parseObjectKey = iota; // parsing object key (before colon)
        private static readonly var parseObjectValue = 0; // parsing object value (after colon)
        private static readonly var parseArrayValue = 1; // parsing array value

        // reset prepares the scanner for use.
        // It must be called before calling s.step.
        private static void reset(this ref scanner s)
        {
            s.step = stateBeginValue;
            s.parseState = s.parseState[0L..0L];
            s.err = null;
            s.redo = false;
            s.endTop = false;
        }

        // eof tells the scanner that the end of input has been reached.
        // It returns a scan status just as s.step does.
        private static long eof(this ref scanner s)
        {
            if (s.err != null)
            {
                return scanError;
            }
            if (s.endTop)
            {
                return scanEnd;
            }
            s.step(s, ' ');
            if (s.endTop)
            {
                return scanEnd;
            }
            if (s.err == null)
            {
                s.err = ref new SyntaxError("unexpected end of JSON input",s.bytes);
            }
            return scanError;
        }

        // pushParseState pushes a new parse state p onto the parse stack.
        private static void pushParseState(this ref scanner s, long p)
        {
            s.parseState = append(s.parseState, p);
        }

        // popParseState pops a parse state (already obtained) off the stack
        // and updates s.step accordingly.
        private static void popParseState(this ref scanner s)
        {
            var n = len(s.parseState) - 1L;
            s.parseState = s.parseState[0L..n];
            s.redo = false;
            if (n == 0L)
            {
                s.step = stateEndTop;
                s.endTop = true;
            }
            else
            {
                s.step = stateEndValue;
            }
        }

        private static bool isSpace(byte c)
        {
            return c == ' ' || c == '\t' || c == '\r' || c == '\n';
        }

        // stateBeginValueOrEmpty is the state after reading `[`.
        private static long stateBeginValueOrEmpty(ref scanner s, byte c)
        {
            if (c <= ' ' && isSpace(c))
            {
                return scanSkipSpace;
            }
            if (c == ']')
            {
                return stateEndValue(s, c);
            }
            return stateBeginValue(s, c);
        }

        // stateBeginValue is the state at the beginning of the input.
        private static long stateBeginValue(ref scanner s, byte c)
        {
            if (c <= ' ' && isSpace(c))
            {
                return scanSkipSpace;
            }
            switch (c)
            {
                case '{': 
                    s.step = stateBeginStringOrEmpty;
                    s.pushParseState(parseObjectKey);
                    return scanBeginObject;
                    break;
                case '[': 
                    s.step = stateBeginValueOrEmpty;
                    s.pushParseState(parseArrayValue);
                    return scanBeginArray;
                    break;
                case '"': 
                    s.step = stateInString;
                    return scanBeginLiteral;
                    break;
                case '-': 
                    s.step = stateNeg;
                    return scanBeginLiteral;
                    break;
                case '0': // beginning of 0.123
                    s.step = state0;
                    return scanBeginLiteral;
                    break;
                case 't': // beginning of true
                    s.step = stateT;
                    return scanBeginLiteral;
                    break;
                case 'f': // beginning of false
                    s.step = stateF;
                    return scanBeginLiteral;
                    break;
                case 'n': // beginning of null
                    s.step = stateN;
                    return scanBeginLiteral;
                    break;
            }
            if ('1' <= c && c <= '9')
            { // beginning of 1234.5
                s.step = state1;
                return scanBeginLiteral;
            }
            return s.error(c, "looking for beginning of value");
        }

        // stateBeginStringOrEmpty is the state after reading `{`.
        private static long stateBeginStringOrEmpty(ref scanner s, byte c)
        {
            if (c <= ' ' && isSpace(c))
            {
                return scanSkipSpace;
            }
            if (c == '}')
            {
                var n = len(s.parseState);
                s.parseState[n - 1L] = parseObjectValue;
                return stateEndValue(s, c);
            }
            return stateBeginString(s, c);
        }

        // stateBeginString is the state after reading `{"key": value,`.
        private static long stateBeginString(ref scanner s, byte c)
        {
            if (c <= ' ' && isSpace(c))
            {
                return scanSkipSpace;
            }
            if (c == '"')
            {
                s.step = stateInString;
                return scanBeginLiteral;
            }
            return s.error(c, "looking for beginning of object key string");
        }

        // stateEndValue is the state after completing a value,
        // such as after reading `{}` or `true` or `["x"`.
        private static long stateEndValue(ref scanner s, byte c)
        {
            var n = len(s.parseState);
            if (n == 0L)
            { 
                // Completed top-level before the current byte.
                s.step = stateEndTop;
                s.endTop = true;
                return stateEndTop(s, c);
            }
            if (c <= ' ' && isSpace(c))
            {
                s.step = stateEndValue;
                return scanSkipSpace;
            }
            var ps = s.parseState[n - 1L];

            if (ps == parseObjectKey) 
                if (c == ':')
                {
                    s.parseState[n - 1L] = parseObjectValue;
                    s.step = stateBeginValue;
                    return scanObjectKey;
                }
                return s.error(c, "after object key");
            else if (ps == parseObjectValue) 
                if (c == ',')
                {
                    s.parseState[n - 1L] = parseObjectKey;
                    s.step = stateBeginString;
                    return scanObjectValue;
                }
                if (c == '}')
                {
                    s.popParseState();
                    return scanEndObject;
                }
                return s.error(c, "after object key:value pair");
            else if (ps == parseArrayValue) 
                if (c == ',')
                {
                    s.step = stateBeginValue;
                    return scanArrayValue;
                }
                if (c == ']')
                {
                    s.popParseState();
                    return scanEndArray;
                }
                return s.error(c, "after array element");
                        return s.error(c, "");
        }

        // stateEndTop is the state after finishing the top-level value,
        // such as after reading `{}` or `[1,2,3]`.
        // Only space characters should be seen now.
        private static long stateEndTop(ref scanner s, byte c)
        {
            if (c != ' ' && c != '\t' && c != '\r' && c != '\n')
            { 
                // Complain about non-space byte on next call.
                s.error(c, "after top-level value");
            }
            return scanEnd;
        }

        // stateInString is the state after reading `"`.
        private static long stateInString(ref scanner s, byte c)
        {
            if (c == '"')
            {
                s.step = stateEndValue;
                return scanContinue;
            }
            if (c == '\\')
            {
                s.step = stateInStringEsc;
                return scanContinue;
            }
            if (c < 0x20UL)
            {
                return s.error(c, "in string literal");
            }
            return scanContinue;
        }

        // stateInStringEsc is the state after reading `"\` during a quoted string.
        private static long stateInStringEsc(ref scanner s, byte c)
        {
            switch (c)
            {
                case 'b': 

                case 'f': 

                case 'n': 

                case 'r': 

                case 't': 

                case '\\': 

                case '/': 

                case '"': 
                    s.step = stateInString;
                    return scanContinue;
                    break;
                case 'u': 
                    s.step = stateInStringEscU;
                    return scanContinue;
                    break;
            }
            return s.error(c, "in string escape code");
        }

        // stateInStringEscU is the state after reading `"\u` during a quoted string.
        private static long stateInStringEscU(ref scanner s, byte c)
        {
            if ('0' <= c && c <= '9' || 'a' <= c && c <= 'f' || 'A' <= c && c <= 'F')
            {
                s.step = stateInStringEscU1;
                return scanContinue;
            } 
            // numbers
            return s.error(c, "in \\u hexadecimal character escape");
        }

        // stateInStringEscU1 is the state after reading `"\u1` during a quoted string.
        private static long stateInStringEscU1(ref scanner s, byte c)
        {
            if ('0' <= c && c <= '9' || 'a' <= c && c <= 'f' || 'A' <= c && c <= 'F')
            {
                s.step = stateInStringEscU12;
                return scanContinue;
            } 
            // numbers
            return s.error(c, "in \\u hexadecimal character escape");
        }

        // stateInStringEscU12 is the state after reading `"\u12` during a quoted string.
        private static long stateInStringEscU12(ref scanner s, byte c)
        {
            if ('0' <= c && c <= '9' || 'a' <= c && c <= 'f' || 'A' <= c && c <= 'F')
            {
                s.step = stateInStringEscU123;
                return scanContinue;
            } 
            // numbers
            return s.error(c, "in \\u hexadecimal character escape");
        }

        // stateInStringEscU123 is the state after reading `"\u123` during a quoted string.
        private static long stateInStringEscU123(ref scanner s, byte c)
        {
            if ('0' <= c && c <= '9' || 'a' <= c && c <= 'f' || 'A' <= c && c <= 'F')
            {
                s.step = stateInString;
                return scanContinue;
            } 
            // numbers
            return s.error(c, "in \\u hexadecimal character escape");
        }

        // stateNeg is the state after reading `-` during a number.
        private static long stateNeg(ref scanner s, byte c)
        {
            if (c == '0')
            {
                s.step = state0;
                return scanContinue;
            }
            if ('1' <= c && c <= '9')
            {
                s.step = state1;
                return scanContinue;
            }
            return s.error(c, "in numeric literal");
        }

        // state1 is the state after reading a non-zero integer during a number,
        // such as after reading `1` or `100` but not `0`.
        private static long state1(ref scanner s, byte c)
        {
            if ('0' <= c && c <= '9')
            {
                s.step = state1;
                return scanContinue;
            }
            return state0(s, c);
        }

        // state0 is the state after reading `0` during a number.
        private static long state0(ref scanner s, byte c)
        {
            if (c == '.')
            {
                s.step = stateDot;
                return scanContinue;
            }
            if (c == 'e' || c == 'E')
            {
                s.step = stateE;
                return scanContinue;
            }
            return stateEndValue(s, c);
        }

        // stateDot is the state after reading the integer and decimal point in a number,
        // such as after reading `1.`.
        private static long stateDot(ref scanner s, byte c)
        {
            if ('0' <= c && c <= '9')
            {
                s.step = stateDot0;
                return scanContinue;
            }
            return s.error(c, "after decimal point in numeric literal");
        }

        // stateDot0 is the state after reading the integer, decimal point, and subsequent
        // digits of a number, such as after reading `3.14`.
        private static long stateDot0(ref scanner s, byte c)
        {
            if ('0' <= c && c <= '9')
            {
                return scanContinue;
            }
            if (c == 'e' || c == 'E')
            {
                s.step = stateE;
                return scanContinue;
            }
            return stateEndValue(s, c);
        }

        // stateE is the state after reading the mantissa and e in a number,
        // such as after reading `314e` or `0.314e`.
        private static long stateE(ref scanner s, byte c)
        {
            if (c == '+' || c == '-')
            {
                s.step = stateESign;
                return scanContinue;
            }
            return stateESign(s, c);
        }

        // stateESign is the state after reading the mantissa, e, and sign in a number,
        // such as after reading `314e-` or `0.314e+`.
        private static long stateESign(ref scanner s, byte c)
        {
            if ('0' <= c && c <= '9')
            {
                s.step = stateE0;
                return scanContinue;
            }
            return s.error(c, "in exponent of numeric literal");
        }

        // stateE0 is the state after reading the mantissa, e, optional sign,
        // and at least one digit of the exponent in a number,
        // such as after reading `314e-2` or `0.314e+1` or `3.14e0`.
        private static long stateE0(ref scanner s, byte c)
        {
            if ('0' <= c && c <= '9')
            {
                return scanContinue;
            }
            return stateEndValue(s, c);
        }

        // stateT is the state after reading `t`.
        private static long stateT(ref scanner s, byte c)
        {
            if (c == 'r')
            {
                s.step = stateTr;
                return scanContinue;
            }
            return s.error(c, "in literal true (expecting 'r')");
        }

        // stateTr is the state after reading `tr`.
        private static long stateTr(ref scanner s, byte c)
        {
            if (c == 'u')
            {
                s.step = stateTru;
                return scanContinue;
            }
            return s.error(c, "in literal true (expecting 'u')");
        }

        // stateTru is the state after reading `tru`.
        private static long stateTru(ref scanner s, byte c)
        {
            if (c == 'e')
            {
                s.step = stateEndValue;
                return scanContinue;
            }
            return s.error(c, "in literal true (expecting 'e')");
        }

        // stateF is the state after reading `f`.
        private static long stateF(ref scanner s, byte c)
        {
            if (c == 'a')
            {
                s.step = stateFa;
                return scanContinue;
            }
            return s.error(c, "in literal false (expecting 'a')");
        }

        // stateFa is the state after reading `fa`.
        private static long stateFa(ref scanner s, byte c)
        {
            if (c == 'l')
            {
                s.step = stateFal;
                return scanContinue;
            }
            return s.error(c, "in literal false (expecting 'l')");
        }

        // stateFal is the state after reading `fal`.
        private static long stateFal(ref scanner s, byte c)
        {
            if (c == 's')
            {
                s.step = stateFals;
                return scanContinue;
            }
            return s.error(c, "in literal false (expecting 's')");
        }

        // stateFals is the state after reading `fals`.
        private static long stateFals(ref scanner s, byte c)
        {
            if (c == 'e')
            {
                s.step = stateEndValue;
                return scanContinue;
            }
            return s.error(c, "in literal false (expecting 'e')");
        }

        // stateN is the state after reading `n`.
        private static long stateN(ref scanner s, byte c)
        {
            if (c == 'u')
            {
                s.step = stateNu;
                return scanContinue;
            }
            return s.error(c, "in literal null (expecting 'u')");
        }

        // stateNu is the state after reading `nu`.
        private static long stateNu(ref scanner s, byte c)
        {
            if (c == 'l')
            {
                s.step = stateNul;
                return scanContinue;
            }
            return s.error(c, "in literal null (expecting 'l')");
        }

        // stateNul is the state after reading `nul`.
        private static long stateNul(ref scanner s, byte c)
        {
            if (c == 'l')
            {
                s.step = stateEndValue;
                return scanContinue;
            }
            return s.error(c, "in literal null (expecting 'l')");
        }

        // stateError is the state after reaching a syntax error,
        // such as after reading `[1}` or `5.1.2`.
        private static long stateError(ref scanner s, byte c)
        {
            return scanError;
        }

        // error records an error and switches to the error state.
        private static long error(this ref scanner s, byte c, @string context)
        {
            s.step = stateError;
            s.err = ref new SyntaxError("invalid character "+quoteChar(c)+" "+context,s.bytes);
            return scanError;
        }

        // quoteChar formats c as a quoted character literal
        private static @string quoteChar(byte c)
        { 
            // special cases - different from quoted strings
            if (c == '\'')
            {
                return "\'\\\'\'";
            }
            if (c == '"')
            {
                return "\'\"\'";
            } 

            // use quoted string with different quotation marks
            var s = strconv.Quote(string(c));
            return "'" + s[1L..len(s) - 1L] + "'";
        }

        // undo causes the scanner to return scanCode from the next state transition.
        // This gives callers a simple 1-byte undo mechanism.
        private static void undo(this ref scanner _s, long scanCode) => func(_s, (ref scanner s, Defer _, Panic panic, Recover __) =>
        {
            if (s.redo)
            {
                panic("json: invalid use of scanner");
            }
            s.redoCode = scanCode;
            s.redoState = s.step;
            s.step = stateRedo;
            s.redo = true;
        });

        // stateRedo helps implement the scanner's 1-byte undo.
        private static long stateRedo(ref scanner s, byte c)
        {
            s.redo = false;
            s.step = s.redoState;
            return s.redoCode;
        }
    }
}}
