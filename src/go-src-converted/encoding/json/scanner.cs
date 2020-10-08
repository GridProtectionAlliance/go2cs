// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package json -- go2cs converted at 2020 October 08 03:42:54 UTC
// import "encoding/json" ==> using json = go.encoding.json_package
// Original source: C:\Go\src\encoding\json\scanner.go
// JSON value parser state machine.
// Just about at the limit of what is reasonable to write by hand.
// Some parts are a bit tedious, but overall it nicely factors out the
// otherwise common code from the multiple scanning functions
// in this package (Compact, Indent, checkValid, etc).
//
// This file starts with two simple examples using the scanner
// before diving into the scanner itself.

using strconv = go.strconv_package;
using sync = go.sync_package;
using static go.builtin;
using System;

namespace go {
namespace encoding
{
    public static partial class json_package
    {
        // Valid reports whether data is a valid JSON encoding.
        public static bool Valid(slice<byte> data) => func((defer, _, __) =>
        {
            var scan = newScanner();
            defer(freeScanner(_addr_scan));
            return checkValid(data, _addr_scan) == null;
        });

        // checkValid verifies that data is valid JSON-encoded data.
        // scan is passed in for use by checkValid to avoid an allocation.
        private static error checkValid(slice<byte> data, ptr<scanner> _addr_scan)
        {
            ref scanner scan = ref _addr_scan.val;

            scan.reset();
            foreach (var (_, c) in data)
            {
                scan.bytes++;
                if (scan.step(scan, c) == scanError)
                {
                    return error.As(scan.err)!;
                }

            }
            if (scan.eof() == scanError)
            {
                return error.As(scan.err)!;
            }

            return error.As(null!)!;

        }

        // A SyntaxError is a description of a JSON syntax error.
        public partial struct SyntaxError
        {
            public @string msg; // description of error
            public long Offset; // error occurred after reading Offset bytes
        }

        private static @string Error(this ptr<SyntaxError> _addr_e)
        {
            ref SyntaxError e = ref _addr_e.val;

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
        private partial struct scanner
        {
            public Func<ptr<scanner>, byte, long> step; // Reached end of top-level value.
            public bool endTop; // Stack of what we're in the middle of - array values, object keys, object values.
            public slice<long> parseState; // Error that happened, if any.
            public error err; // total bytes consumed, updated by decoder.Decode (and deliberately
// not set to zero by scan.reset)
            public long bytes;
        }

        private static sync.Pool scannerPool = new sync.Pool(New:func()interface{}{return&scanner{}},);

        private static ptr<scanner> newScanner()
        {
            ptr<scanner> scan = scannerPool.Get()._<ptr<scanner>>(); 
            // scan.reset by design doesn't set bytes to zero
            scan.bytes = 0L;
            scan.reset();
            return _addr_scan!;

        }

        private static void freeScanner(ptr<scanner> _addr_scan)
        {
            ref scanner scan = ref _addr_scan.val;
 
            // Avoid hanging on to too much memory in extreme cases.
            if (len(scan.parseState) > 1024L)
            {
                scan.parseState = null;
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
 
        // Continue.
        private static readonly var scanContinue = (var)iota; // uninteresting byte
        private static readonly var scanBeginLiteral = (var)0; // end implied by next result != scanContinue
        private static readonly var scanBeginObject = (var)1; // begin object
        private static readonly var scanObjectKey = (var)2; // just finished object key (string)
        private static readonly var scanObjectValue = (var)3; // just finished non-last object value
        private static readonly var scanEndObject = (var)4; // end object (implies scanObjectValue if possible)
        private static readonly var scanBeginArray = (var)5; // begin array
        private static readonly var scanArrayValue = (var)6; // just finished array value
        private static readonly var scanEndArray = (var)7; // end array (implies scanArrayValue if possible)
        private static readonly var scanSkipSpace = (var)8; // space byte; can skip; known to be last "continue" result

        // Stop.
        private static readonly var scanEnd = (var)9; // top-level value ended *before* this byte; known to be first "stop" result
        private static readonly var scanError = (var)10; // hit an error, scanner.err.

        // These values are stored in the parseState stack.
        // They give the current state of a composite value
        // being scanned. If the parser is inside a nested value
        // the parseState describes the nested state, outermost at entry 0.
        private static readonly var parseObjectKey = (var)iota; // parsing object key (before colon)
        private static readonly var parseObjectValue = (var)0; // parsing object value (after colon)
        private static readonly var parseArrayValue = (var)1; // parsing array value

        // This limits the max nesting depth to prevent stack overflow.
        // This is permitted by https://tools.ietf.org/html/rfc7159#section-9
        private static readonly long maxNestingDepth = (long)10000L;

        // reset prepares the scanner for use.
        // It must be called before calling s.step.


        // reset prepares the scanner for use.
        // It must be called before calling s.step.
        private static void reset(this ptr<scanner> _addr_s)
        {
            ref scanner s = ref _addr_s.val;

            s.step = stateBeginValue;
            s.parseState = s.parseState[0L..0L];
            s.err = null;
            s.endTop = false;
        }

        // eof tells the scanner that the end of input has been reached.
        // It returns a scan status just as s.step does.
        private static long eof(this ptr<scanner> _addr_s)
        {
            ref scanner s = ref _addr_s.val;

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
                s.err = addr(new SyntaxError("unexpected end of JSON input",s.bytes));
            }

            return scanError;

        }

        // pushParseState pushes a new parse state p onto the parse stack.
        // an error state is returned if maxNestingDepth was exceeded, otherwise successState is returned.
        private static long pushParseState(this ptr<scanner> _addr_s, byte c, long newParseState, long successState)
        {
            ref scanner s = ref _addr_s.val;

            s.parseState = append(s.parseState, newParseState);
            if (len(s.parseState) <= maxNestingDepth)
            {
                return successState;
            }

            return s.error(c, "exceeded max depth");

        }

        // popParseState pops a parse state (already obtained) off the stack
        // and updates s.step accordingly.
        private static void popParseState(this ptr<scanner> _addr_s)
        {
            ref scanner s = ref _addr_s.val;

            var n = len(s.parseState) - 1L;
            s.parseState = s.parseState[0L..n];
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
            return c <= ' ' && (c == ' ' || c == '\t' || c == '\r' || c == '\n');
        }

        // stateBeginValueOrEmpty is the state after reading `[`.
        private static long stateBeginValueOrEmpty(ptr<scanner> _addr_s, byte c)
        {
            ref scanner s = ref _addr_s.val;

            if (isSpace(c))
            {
                return scanSkipSpace;
            }

            if (c == ']')
            {
                return stateEndValue(_addr_s, c);
            }

            return stateBeginValue(_addr_s, c);

        }

        // stateBeginValue is the state at the beginning of the input.
        private static long stateBeginValue(ptr<scanner> _addr_s, byte c)
        {
            ref scanner s = ref _addr_s.val;

            if (isSpace(c))
            {
                return scanSkipSpace;
            }

            switch (c)
            {
                case '{': 
                    s.step = stateBeginStringOrEmpty;
                    return s.pushParseState(c, parseObjectKey, scanBeginObject);
                    break;
                case '[': 
                    s.step = stateBeginValueOrEmpty;
                    return s.pushParseState(c, parseArrayValue, scanBeginArray);
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
        private static long stateBeginStringOrEmpty(ptr<scanner> _addr_s, byte c)
        {
            ref scanner s = ref _addr_s.val;

            if (isSpace(c))
            {
                return scanSkipSpace;
            }

            if (c == '}')
            {
                var n = len(s.parseState);
                s.parseState[n - 1L] = parseObjectValue;
                return stateEndValue(_addr_s, c);
            }

            return stateBeginString(_addr_s, c);

        }

        // stateBeginString is the state after reading `{"key": value,`.
        private static long stateBeginString(ptr<scanner> _addr_s, byte c)
        {
            ref scanner s = ref _addr_s.val;

            if (isSpace(c))
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
        private static long stateEndValue(ptr<scanner> _addr_s, byte c)
        {
            ref scanner s = ref _addr_s.val;

            var n = len(s.parseState);
            if (n == 0L)
            { 
                // Completed top-level before the current byte.
                s.step = stateEndTop;
                s.endTop = true;
                return stateEndTop(_addr_s, c);

            }

            if (isSpace(c))
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
        private static long stateEndTop(ptr<scanner> _addr_s, byte c)
        {
            ref scanner s = ref _addr_s.val;

            if (!isSpace(c))
            { 
                // Complain about non-space byte on next call.
                s.error(c, "after top-level value");

            }

            return scanEnd;

        }

        // stateInString is the state after reading `"`.
        private static long stateInString(ptr<scanner> _addr_s, byte c)
        {
            ref scanner s = ref _addr_s.val;

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
        private static long stateInStringEsc(ptr<scanner> _addr_s, byte c)
        {
            ref scanner s = ref _addr_s.val;

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
        private static long stateInStringEscU(ptr<scanner> _addr_s, byte c)
        {
            ref scanner s = ref _addr_s.val;

            if ('0' <= c && c <= '9' || 'a' <= c && c <= 'f' || 'A' <= c && c <= 'F')
            {
                s.step = stateInStringEscU1;
                return scanContinue;
            } 
            // numbers
            return s.error(c, "in \\u hexadecimal character escape");

        }

        // stateInStringEscU1 is the state after reading `"\u1` during a quoted string.
        private static long stateInStringEscU1(ptr<scanner> _addr_s, byte c)
        {
            ref scanner s = ref _addr_s.val;

            if ('0' <= c && c <= '9' || 'a' <= c && c <= 'f' || 'A' <= c && c <= 'F')
            {
                s.step = stateInStringEscU12;
                return scanContinue;
            } 
            // numbers
            return s.error(c, "in \\u hexadecimal character escape");

        }

        // stateInStringEscU12 is the state after reading `"\u12` during a quoted string.
        private static long stateInStringEscU12(ptr<scanner> _addr_s, byte c)
        {
            ref scanner s = ref _addr_s.val;

            if ('0' <= c && c <= '9' || 'a' <= c && c <= 'f' || 'A' <= c && c <= 'F')
            {
                s.step = stateInStringEscU123;
                return scanContinue;
            } 
            // numbers
            return s.error(c, "in \\u hexadecimal character escape");

        }

        // stateInStringEscU123 is the state after reading `"\u123` during a quoted string.
        private static long stateInStringEscU123(ptr<scanner> _addr_s, byte c)
        {
            ref scanner s = ref _addr_s.val;

            if ('0' <= c && c <= '9' || 'a' <= c && c <= 'f' || 'A' <= c && c <= 'F')
            {
                s.step = stateInString;
                return scanContinue;
            } 
            // numbers
            return s.error(c, "in \\u hexadecimal character escape");

        }

        // stateNeg is the state after reading `-` during a number.
        private static long stateNeg(ptr<scanner> _addr_s, byte c)
        {
            ref scanner s = ref _addr_s.val;

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
        private static long state1(ptr<scanner> _addr_s, byte c)
        {
            ref scanner s = ref _addr_s.val;

            if ('0' <= c && c <= '9')
            {
                s.step = state1;
                return scanContinue;
            }

            return state0(_addr_s, c);

        }

        // state0 is the state after reading `0` during a number.
        private static long state0(ptr<scanner> _addr_s, byte c)
        {
            ref scanner s = ref _addr_s.val;

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

            return stateEndValue(_addr_s, c);

        }

        // stateDot is the state after reading the integer and decimal point in a number,
        // such as after reading `1.`.
        private static long stateDot(ptr<scanner> _addr_s, byte c)
        {
            ref scanner s = ref _addr_s.val;

            if ('0' <= c && c <= '9')
            {
                s.step = stateDot0;
                return scanContinue;
            }

            return s.error(c, "after decimal point in numeric literal");

        }

        // stateDot0 is the state after reading the integer, decimal point, and subsequent
        // digits of a number, such as after reading `3.14`.
        private static long stateDot0(ptr<scanner> _addr_s, byte c)
        {
            ref scanner s = ref _addr_s.val;

            if ('0' <= c && c <= '9')
            {
                return scanContinue;
            }

            if (c == 'e' || c == 'E')
            {
                s.step = stateE;
                return scanContinue;
            }

            return stateEndValue(_addr_s, c);

        }

        // stateE is the state after reading the mantissa and e in a number,
        // such as after reading `314e` or `0.314e`.
        private static long stateE(ptr<scanner> _addr_s, byte c)
        {
            ref scanner s = ref _addr_s.val;

            if (c == '+' || c == '-')
            {
                s.step = stateESign;
                return scanContinue;
            }

            return stateESign(_addr_s, c);

        }

        // stateESign is the state after reading the mantissa, e, and sign in a number,
        // such as after reading `314e-` or `0.314e+`.
        private static long stateESign(ptr<scanner> _addr_s, byte c)
        {
            ref scanner s = ref _addr_s.val;

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
        private static long stateE0(ptr<scanner> _addr_s, byte c)
        {
            ref scanner s = ref _addr_s.val;

            if ('0' <= c && c <= '9')
            {
                return scanContinue;
            }

            return stateEndValue(_addr_s, c);

        }

        // stateT is the state after reading `t`.
        private static long stateT(ptr<scanner> _addr_s, byte c)
        {
            ref scanner s = ref _addr_s.val;

            if (c == 'r')
            {
                s.step = stateTr;
                return scanContinue;
            }

            return s.error(c, "in literal true (expecting 'r')");

        }

        // stateTr is the state after reading `tr`.
        private static long stateTr(ptr<scanner> _addr_s, byte c)
        {
            ref scanner s = ref _addr_s.val;

            if (c == 'u')
            {
                s.step = stateTru;
                return scanContinue;
            }

            return s.error(c, "in literal true (expecting 'u')");

        }

        // stateTru is the state after reading `tru`.
        private static long stateTru(ptr<scanner> _addr_s, byte c)
        {
            ref scanner s = ref _addr_s.val;

            if (c == 'e')
            {
                s.step = stateEndValue;
                return scanContinue;
            }

            return s.error(c, "in literal true (expecting 'e')");

        }

        // stateF is the state after reading `f`.
        private static long stateF(ptr<scanner> _addr_s, byte c)
        {
            ref scanner s = ref _addr_s.val;

            if (c == 'a')
            {
                s.step = stateFa;
                return scanContinue;
            }

            return s.error(c, "in literal false (expecting 'a')");

        }

        // stateFa is the state after reading `fa`.
        private static long stateFa(ptr<scanner> _addr_s, byte c)
        {
            ref scanner s = ref _addr_s.val;

            if (c == 'l')
            {
                s.step = stateFal;
                return scanContinue;
            }

            return s.error(c, "in literal false (expecting 'l')");

        }

        // stateFal is the state after reading `fal`.
        private static long stateFal(ptr<scanner> _addr_s, byte c)
        {
            ref scanner s = ref _addr_s.val;

            if (c == 's')
            {
                s.step = stateFals;
                return scanContinue;
            }

            return s.error(c, "in literal false (expecting 's')");

        }

        // stateFals is the state after reading `fals`.
        private static long stateFals(ptr<scanner> _addr_s, byte c)
        {
            ref scanner s = ref _addr_s.val;

            if (c == 'e')
            {
                s.step = stateEndValue;
                return scanContinue;
            }

            return s.error(c, "in literal false (expecting 'e')");

        }

        // stateN is the state after reading `n`.
        private static long stateN(ptr<scanner> _addr_s, byte c)
        {
            ref scanner s = ref _addr_s.val;

            if (c == 'u')
            {
                s.step = stateNu;
                return scanContinue;
            }

            return s.error(c, "in literal null (expecting 'u')");

        }

        // stateNu is the state after reading `nu`.
        private static long stateNu(ptr<scanner> _addr_s, byte c)
        {
            ref scanner s = ref _addr_s.val;

            if (c == 'l')
            {
                s.step = stateNul;
                return scanContinue;
            }

            return s.error(c, "in literal null (expecting 'l')");

        }

        // stateNul is the state after reading `nul`.
        private static long stateNul(ptr<scanner> _addr_s, byte c)
        {
            ref scanner s = ref _addr_s.val;

            if (c == 'l')
            {
                s.step = stateEndValue;
                return scanContinue;
            }

            return s.error(c, "in literal null (expecting 'l')");

        }

        // stateError is the state after reaching a syntax error,
        // such as after reading `[1}` or `5.1.2`.
        private static long stateError(ptr<scanner> _addr_s, byte c)
        {
            ref scanner s = ref _addr_s.val;

            return scanError;
        }

        // error records an error and switches to the error state.
        private static long error(this ptr<scanner> _addr_s, byte c, @string context)
        {
            ref scanner s = ref _addr_s.val;

            s.step = stateError;
            s.err = addr(new SyntaxError("invalid character "+quoteChar(c)+" "+context,s.bytes));
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
    }
}}
