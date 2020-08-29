// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package parse -- go2cs converted at 2020 August 29 08:34:35 UTC
// import "text/template/parse" ==> using parse = go.text.template.parse_package
// Original source: C:\Go\src\text\template\parse\lex.go
using fmt = go.fmt_package;
using strings = go.strings_package;
using unicode = go.unicode_package;
using utf8 = go.unicode.utf8_package;
using static go.builtin;
using System.Threading;

namespace go {
namespace text {
namespace template
{
    public static partial class parse_package
    {
        // item represents a token or text string returned from the scanner.
        private partial struct item
        {
            public itemType typ; // The type of this item.
            public Pos pos; // The starting position, in bytes, of this item in the input string.
            public @string val; // The value of this item.
            public long line; // The line number at the start of this item.
        }

        private static @string String(this item i)
        {

            if (i.typ == itemEOF) 
                return "EOF";
            else if (i.typ == itemError) 
                return i.val;
            else if (i.typ > itemKeyword) 
                return fmt.Sprintf("<%s>", i.val);
            else if (len(i.val) > 10L) 
                return fmt.Sprintf("%.10q...", i.val);
                        return fmt.Sprintf("%q", i.val);
        }

        // itemType identifies the type of lex items.
        private partial struct itemType // : long
        {
        }

        private static readonly itemType itemError = iota; // error occurred; value is text of error
        private static readonly var itemBool = 0; // boolean constant
        private static readonly var itemChar = 1; // printable ASCII character; grab bag for comma etc.
        private static readonly var itemCharConstant = 2; // character constant
        private static readonly var itemComplex = 3; // complex constant (1+2i); imaginary is just a number
        private static readonly var itemColonEquals = 4; // colon-equals (':=') introducing a declaration
        private static readonly var itemEOF = 5;
        private static readonly var itemField = 6; // alphanumeric identifier starting with '.'
        private static readonly var itemIdentifier = 7; // alphanumeric identifier not starting with '.'
        private static readonly var itemLeftDelim = 8; // left action delimiter
        private static readonly var itemLeftParen = 9; // '(' inside action
        private static readonly var itemNumber = 10; // simple number, including imaginary
        private static readonly var itemPipe = 11; // pipe symbol
        private static readonly var itemRawString = 12; // raw quoted string (includes quotes)
        private static readonly var itemRightDelim = 13; // right action delimiter
        private static readonly var itemRightParen = 14; // ')' inside action
        private static readonly var itemSpace = 15; // run of spaces separating arguments
        private static readonly var itemString = 16; // quoted string (includes quotes)
        private static readonly var itemText = 17; // plain text
        private static readonly var itemVariable = 18; // variable starting with '$', such as '$' or  '$1' or '$hello'
        // Keywords appear after all the rest.
        private static readonly var itemKeyword = 19; // used only to delimit the keywords
        private static readonly var itemBlock = 20; // block keyword
        private static readonly var itemDot = 21; // the cursor, spelled '.'
        private static readonly var itemDefine = 22; // define keyword
        private static readonly var itemElse = 23; // else keyword
        private static readonly var itemEnd = 24; // end keyword
        private static readonly var itemIf = 25; // if keyword
        private static readonly var itemNil = 26; // the untyped nil constant, easiest to treat as a keyword
        private static readonly var itemRange = 27; // range keyword
        private static readonly var itemTemplate = 28; // template keyword
        private static readonly var itemWith = 29; // with keyword

        private static map key = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, itemType>{".":itemDot,"block":itemBlock,"define":itemDefine,"else":itemElse,"end":itemEnd,"if":itemIf,"range":itemRange,"nil":itemNil,"template":itemTemplate,"with":itemWith,};

        private static readonly long eof = -1L;

        // Trimming spaces.
        // If the action begins "{{- " rather than "{{", then all space/tab/newlines
        // preceding the action are trimmed; conversely if it ends " -}}" the
        // leading spaces are trimmed. This is done entirely in the lexer; the
        // parser never sees it happen. We require an ASCII space to be
        // present to avoid ambiguity with things like "{{-3}}". It reads
        // better with the space present anyway. For simplicity, only ASCII
        // space does the job.


        // Trimming spaces.
        // If the action begins "{{- " rather than "{{", then all space/tab/newlines
        // preceding the action are trimmed; conversely if it ends " -}}" the
        // leading spaces are trimmed. This is done entirely in the lexer; the
        // parser never sees it happen. We require an ASCII space to be
        // present to avoid ambiguity with things like "{{-3}}". It reads
        // better with the space present anyway. For simplicity, only ASCII
        // space does the job.
        private static readonly @string spaceChars = " \t\r\n"; // These are the space characters defined by Go itself.
        private static readonly @string leftTrimMarker = "- "; // Attached to left delimiter, trims trailing spaces from preceding text.
        private static readonly @string rightTrimMarker = " -"; // Attached to right delimiter, trims leading spaces from following text.
        private static readonly var trimMarkerLen = Pos(len(leftTrimMarker));

        // stateFn represents the state of the scanner as a function that returns the next state.
        public delegate  stateFn stateFn(ref lexer);

        // lexer holds the state of the scanner.
        private partial struct lexer
        {
            public @string name; // the name of the input; used only for error reports
            public @string input; // the string being scanned
            public @string leftDelim; // start of action
            public @string rightDelim; // end of action
            public Pos pos; // current position in the input
            public Pos start; // start position of this item
            public Pos width; // width of last rune read from input
            public channel<item> items; // channel of scanned items
            public long parenDepth; // nesting depth of ( ) exprs
            public long line; // 1+number of newlines seen
        }

        // next returns the next rune in the input.
        private static int next(this ref lexer l)
        {
            if (int(l.pos) >= len(l.input))
            {
                l.width = 0L;
                return eof;
            }
            var (r, w) = utf8.DecodeRuneInString(l.input[l.pos..]);
            l.width = Pos(w);
            l.pos += l.width;
            if (r == '\n')
            {
                l.line++;
            }
            return r;
        }

        // peek returns but does not consume the next rune in the input.
        private static int peek(this ref lexer l)
        {
            var r = l.next();
            l.backup();
            return r;
        }

        // backup steps back one rune. Can only be called once per call of next.
        private static void backup(this ref lexer l)
        {
            l.pos -= l.width; 
            // Correct newline count.
            if (l.width == 1L && l.input[l.pos] == '\n')
            {
                l.line--;
            }
        }

        // emit passes an item back to the client.
        private static void emit(this ref lexer l, itemType t)
        {
            l.items.Send(new item(t,l.start,l.input[l.start:l.pos],l.line)); 
            // Some items contain text internally. If so, count their newlines.

            if (t == itemText || t == itemRawString || t == itemLeftDelim || t == itemRightDelim) 
                l.line += strings.Count(l.input[l.start..l.pos], "\n");
                        l.start = l.pos;
        }

        // ignore skips over the pending input before this point.
        private static void ignore(this ref lexer l)
        {
            l.line += strings.Count(l.input[l.start..l.pos], "\n");
            l.start = l.pos;
        }

        // accept consumes the next rune if it's from the valid set.
        private static bool accept(this ref lexer l, @string valid)
        {
            if (strings.ContainsRune(valid, l.next()))
            {
                return true;
            }
            l.backup();
            return false;
        }

        // acceptRun consumes a run of runes from the valid set.
        private static void acceptRun(this ref lexer l, @string valid)
        {
            while (strings.ContainsRune(valid, l.next()))
            {
            }

            l.backup();
        }

        // errorf returns an error token and terminates the scan by passing
        // back a nil pointer that will be the next state, terminating l.nextItem.
        private static stateFn errorf(this ref lexer l, @string format, params object[] args)
        {
            l.items.Send(new item(itemError,l.start,fmt.Sprintf(format,args...),l.line));
            return null;
        }

        // nextItem returns the next item from the input.
        // Called by the parser, not in the lexing goroutine.
        private static item nextItem(this ref lexer l)
        {
            return l.items.Receive();
        }

        // drain drains the output so the lexing goroutine will exit.
        // Called by the parser, not in the lexing goroutine.
        private static void drain(this ref lexer l)
        {
            foreach (>>MARKER:FORRANGEEXPRESSIONS_LEVEL_1<< in l.items)
            {>>MARKER:FORRANGEMUTABLEEXPRESSIONS_LEVEL_1<<
            }
        }

        // lex creates a new scanner for the input string.
        private static ref lexer lex(@string name, @string input, @string left, @string right)
        {
            if (left == "")
            {
                left = leftDelim;
            }
            if (right == "")
            {
                right = rightDelim;
            }
            lexer l = ref new lexer(name:name,input:input,leftDelim:left,rightDelim:right,items:make(chanitem),line:1,);
            go_(() => l.run());
            return l;
        }

        // run runs the state machine for the lexer.
        private static void run(this ref lexer l)
        {
            {
                var state = lexText;

                while (state != null)
                {
                    state = state(l);
                }

            }
            close(l.items);
        }

        // state functions

        private static readonly @string leftDelim = "{{";
        private static readonly @string rightDelim = "}}";
        private static readonly @string leftComment = "/*";
        private static readonly @string rightComment = "*/";

        // lexText scans until an opening action delimiter, "{{".
        private static stateFn lexText(ref lexer l)
        {
            l.width = 0L;
            {
                var x = strings.Index(l.input[l.pos..], l.leftDelim);

                if (x >= 0L)
                {
                    var ldn = Pos(len(l.leftDelim));
                    l.pos += Pos(x);
                    var trimLength = Pos(0L);
                    if (strings.HasPrefix(l.input[l.pos + ldn..], leftTrimMarker))
                    {
                        trimLength = rightTrimLength(l.input[l.start..l.pos]);
                    }
                    l.pos -= trimLength;
                    if (l.pos > l.start)
                    {
                        l.emit(itemText);
                    }
                    l.pos += trimLength;
                    l.ignore();
                    return lexLeftDelim;
                }
                else
                {
                    l.pos = Pos(len(l.input));
                } 
                // Correctly reached EOF.

            } 
            // Correctly reached EOF.
            if (l.pos > l.start)
            {
                l.emit(itemText);
            }
            l.emit(itemEOF);
            return null;
        }

        // rightTrimLength returns the length of the spaces at the end of the string.
        private static Pos rightTrimLength(@string s)
        {
            return Pos(len(s) - len(strings.TrimRight(s, spaceChars)));
        }

        // atRightDelim reports whether the lexer is at a right delimiter, possibly preceded by a trim marker.
        private static (bool, bool) atRightDelim(this ref lexer l)
        {
            if (strings.HasPrefix(l.input[l.pos..], l.rightDelim))
            {
                return (true, false);
            } 
            // The right delim might have the marker before.
            if (strings.HasPrefix(l.input[l.pos..], rightTrimMarker) && strings.HasPrefix(l.input[l.pos + trimMarkerLen..], l.rightDelim))
            {
                return (true, true);
            }
            return (false, false);
        }

        // leftTrimLength returns the length of the spaces at the beginning of the string.
        private static Pos leftTrimLength(@string s)
        {
            return Pos(len(s) - len(strings.TrimLeft(s, spaceChars)));
        }

        // lexLeftDelim scans the left delimiter, which is known to be present, possibly with a trim marker.
        private static stateFn lexLeftDelim(ref lexer l)
        {
            l.pos += Pos(len(l.leftDelim));
            var trimSpace = strings.HasPrefix(l.input[l.pos..], leftTrimMarker);
            var afterMarker = Pos(0L);
            if (trimSpace)
            {
                afterMarker = trimMarkerLen;
            }
            if (strings.HasPrefix(l.input[l.pos + afterMarker..], leftComment))
            {
                l.pos += afterMarker;
                l.ignore();
                return lexComment;
            }
            l.emit(itemLeftDelim);
            l.pos += afterMarker;
            l.ignore();
            l.parenDepth = 0L;
            return lexInsideAction;
        }

        // lexComment scans a comment. The left comment marker is known to be present.
        private static stateFn lexComment(ref lexer l)
        {
            l.pos += Pos(len(leftComment));
            var i = strings.Index(l.input[l.pos..], rightComment);
            if (i < 0L)
            {
                return l.errorf("unclosed comment");
            }
            l.pos += Pos(i + len(rightComment));
            var (delim, trimSpace) = l.atRightDelim();
            if (!delim)
            {
                return l.errorf("comment ends before closing delimiter");
            }
            if (trimSpace)
            {
                l.pos += trimMarkerLen;
            }
            l.pos += Pos(len(l.rightDelim));
            if (trimSpace)
            {
                l.pos += leftTrimLength(l.input[l.pos..]);
            }
            l.ignore();
            return lexText;
        }

        // lexRightDelim scans the right delimiter, which is known to be present, possibly with a trim marker.
        private static stateFn lexRightDelim(ref lexer l)
        {
            var trimSpace = strings.HasPrefix(l.input[l.pos..], rightTrimMarker);
            if (trimSpace)
            {
                l.pos += trimMarkerLen;
                l.ignore();
            }
            l.pos += Pos(len(l.rightDelim));
            l.emit(itemRightDelim);
            if (trimSpace)
            {
                l.pos += leftTrimLength(l.input[l.pos..]);
                l.ignore();
            }
            return lexText;
        }

        // lexInsideAction scans the elements inside action delimiters.
        private static stateFn lexInsideAction(ref lexer l)
        { 
            // Either number, quoted string, or identifier.
            // Spaces separate arguments; runs of spaces turn into itemSpace.
            // Pipe symbols separate and are emitted.
            var (delim, _) = l.atRightDelim();
            if (delim)
            {
                if (l.parenDepth == 0L)
                {
                    return lexRightDelim;
                }
                return l.errorf("unclosed left paren");
            }
            {
                var r__prev1 = r;

                var r = l.next();


                if (r == eof || isEndOfLine(r))
                {
                    return l.errorf("unclosed action");
                    goto __switch_break0;
                }
                if (isSpace(r))
                {
                    return lexSpace;
                    goto __switch_break0;
                }
                if (r == ':')
                {
                    if (l.next() != '=')
                    {
                        return l.errorf("expected :=");
                    }
                    l.emit(itemColonEquals);
                    goto __switch_break0;
                }
                if (r == '|')
                {
                    l.emit(itemPipe);
                    goto __switch_break0;
                }
                if (r == '"')
                {
                    return lexQuote;
                    goto __switch_break0;
                }
                if (r == '`')
                {
                    return lexRawQuote;
                    goto __switch_break0;
                }
                if (r == '$')
                {
                    return lexVariable;
                    goto __switch_break0;
                }
                if (r == '\'')
                {
                    return lexChar;
                    goto __switch_break0;
                }
                if (r == '.') 
                {
                    // special look-ahead for ".field" so we don't break l.backup().
                    if (l.pos < Pos(len(l.input)))
                    {
                        r = l.input[l.pos];
                        if (r < '0' || '9' < r)
                        {
                            return lexField;
                        }
                    }
                    fallthrough = true; // '.' can start a number.
                }
                if (fallthrough || r == '+' || r == '-' || ('0' <= r && r <= '9'))
                {
                    l.backup();
                    return lexNumber;
                    goto __switch_break0;
                }
                if (isAlphaNumeric(r))
                {
                    l.backup();
                    return lexIdentifier;
                    goto __switch_break0;
                }
                if (r == '(')
                {
                    l.emit(itemLeftParen);
                    l.parenDepth++;
                    goto __switch_break0;
                }
                if (r == ')')
                {
                    l.emit(itemRightParen);
                    l.parenDepth--;
                    if (l.parenDepth < 0L)
                    {
                        return l.errorf("unexpected right paren %#U", r);
                    }
                    goto __switch_break0;
                }
                if (r <= unicode.MaxASCII && unicode.IsPrint(r))
                {
                    l.emit(itemChar);
                    return lexInsideAction;
                    goto __switch_break0;
                }
                // default: 
                    return l.errorf("unrecognized character in action: %#U", r);

                __switch_break0:;

                r = r__prev1;
            }
            return lexInsideAction;
        }

        // lexSpace scans a run of space characters.
        // One space has already been seen.
        private static stateFn lexSpace(ref lexer l)
        {
            while (isSpace(l.peek()))
            {
                l.next();
            }

            l.emit(itemSpace);
            return lexInsideAction;
        }

        // lexIdentifier scans an alphanumeric.
        private static stateFn lexIdentifier(ref lexer l)
        {
Loop:
            while (true)
            {
                {
                    var r = l.next();


                    if (isAlphaNumeric(r))                     else 
                        l.backup();
                        var word = l.input[l.start..l.pos];
                        if (!l.atTerminator())
                        {
                            return l.errorf("bad character %#U", r);
                        }

                        if (key[word] > itemKeyword) 
                            l.emit(key[word]);
                        else if (word[0L] == '.') 
                            l.emit(itemField);
                        else if (word == "true" || word == "false") 
                            l.emit(itemBool);
                        else 
                            l.emit(itemIdentifier);
                                                _breakLoop = true;
                        break;

                }
            }
            return lexInsideAction;
        }

        // lexField scans a field: .Alphanumeric.
        // The . has been scanned.
        private static stateFn lexField(ref lexer l)
        {
            return lexFieldOrVariable(l, itemField);
        }

        // lexVariable scans a Variable: $Alphanumeric.
        // The $ has been scanned.
        private static stateFn lexVariable(ref lexer l)
        {
            if (l.atTerminator())
            { // Nothing interesting follows -> "$".
                l.emit(itemVariable);
                return lexInsideAction;
            }
            return lexFieldOrVariable(l, itemVariable);
        }

        // lexVariable scans a field or variable: [.$]Alphanumeric.
        // The . or $ has been scanned.
        private static stateFn lexFieldOrVariable(ref lexer l, itemType typ)
        {
            if (l.atTerminator())
            { // Nothing interesting follows -> "." or "$".
                if (typ == itemVariable)
                {
                    l.emit(itemVariable);
                }
                else
                {
                    l.emit(itemDot);
                }
                return lexInsideAction;
            }
            int r = default;
            while (true)
            {
                r = l.next();
                if (!isAlphaNumeric(r))
                {
                    l.backup();
                    break;
                }
            }

            if (!l.atTerminator())
            {
                return l.errorf("bad character %#U", r);
            }
            l.emit(typ);
            return lexInsideAction;
        }

        // atTerminator reports whether the input is at valid termination character to
        // appear after an identifier. Breaks .X.Y into two pieces. Also catches cases
        // like "$x+2" not being acceptable without a space, in case we decide one
        // day to implement arithmetic.
        private static bool atTerminator(this ref lexer l)
        {
            var r = l.peek();
            if (isSpace(r) || isEndOfLine(r))
            {
                return true;
            }

            if (r == eof || r == '.' || r == ',' || r == '|' || r == ':' || r == ')' || r == '(') 
                return true;
            // Does r start the delimiter? This can be ambiguous (with delim=="//", $x/2 will
            // succeed but should fail) but only in extremely rare cases caused by willfully
            // bad choice of delimiter.
            {
                var (rd, _) = utf8.DecodeRuneInString(l.rightDelim);

                if (rd == r)
                {
                    return true;
                }

            }
            return false;
        }

        // lexChar scans a character constant. The initial quote is already
        // scanned. Syntax checking is done by the parser.
        private static stateFn lexChar(ref lexer l)
        {
Loop:
            while (true)
            {

                if (l.next() == '\\')
                {
                    {
                        var r = l.next();

                        if (r != eof && r != '\n')
                        {
                            break;
                        }

                    }
                    fallthrough = true;
                }
                if (fallthrough || l.next() == eof || l.next() == '\n')
                {
                    return l.errorf("unterminated character constant");
                    goto __switch_break1;
                }
                if (l.next() == '\'')
                {
                    _breakLoop = true;
                    break;
                    goto __switch_break1;
                }

                __switch_break1:;
            }
            l.emit(itemCharConstant);
            return lexInsideAction;
        }

        // lexNumber scans a number: decimal, octal, hex, float, or imaginary. This
        // isn't a perfect number scanner - for instance it accepts "." and "0x0.2"
        // and "089" - but when it's wrong the input is invalid and the parser (via
        // strconv) will notice.
        private static stateFn lexNumber(ref lexer l)
        {
            if (!l.scanNumber())
            {
                return l.errorf("bad number syntax: %q", l.input[l.start..l.pos]);
            }
            {
                var sign = l.peek();

                if (sign == '+' || sign == '-')
                { 
                    // Complex: 1+2i. No spaces, must end in 'i'.
                    if (!l.scanNumber() || l.input[l.pos - 1L] != 'i')
                    {
                        return l.errorf("bad number syntax: %q", l.input[l.start..l.pos]);
                    }
                    l.emit(itemComplex);
                }
                else
                {
                    l.emit(itemNumber);
                }

            }
            return lexInsideAction;
        }

        private static bool scanNumber(this ref lexer l)
        { 
            // Optional leading sign.
            l.accept("+-"); 
            // Is it hex?
            @string digits = "0123456789";
            if (l.accept("0") && l.accept("xX"))
            {
                digits = "0123456789abcdefABCDEF";
            }
            l.acceptRun(digits);
            if (l.accept("."))
            {
                l.acceptRun(digits);
            }
            if (l.accept("eE"))
            {
                l.accept("+-");
                l.acceptRun("0123456789");
            } 
            // Is it imaginary?
            l.accept("i"); 
            // Next thing mustn't be alphanumeric.
            if (isAlphaNumeric(l.peek()))
            {
                l.next();
                return false;
            }
            return true;
        }

        // lexQuote scans a quoted string.
        private static stateFn lexQuote(ref lexer l)
        {
Loop:
            while (true)
            {

                if (l.next() == '\\')
                {
                    {
                        var r = l.next();

                        if (r != eof && r != '\n')
                        {
                            break;
                        }

                    }
                    fallthrough = true;
                }
                if (fallthrough || l.next() == eof || l.next() == '\n')
                {
                    return l.errorf("unterminated quoted string");
                    goto __switch_break2;
                }
                if (l.next() == '"')
                {
                    _breakLoop = true;
                    break;
                    goto __switch_break2;
                }

                __switch_break2:;
            }
            l.emit(itemString);
            return lexInsideAction;
        }

        // lexRawQuote scans a raw quoted string.
        private static stateFn lexRawQuote(ref lexer l)
        {
            var startLine = l.line;
Loop:
            while (true)
            {

                if (l.next() == eof) 
                    // Restore line number to location of opening quote.
                    // We will error out so it's ok just to overwrite the field.
                    l.line = startLine;
                    return l.errorf("unterminated raw quoted string");
                else if (l.next() == '`') 
                    _breakLoop = true;
                    break;
                            }
            l.emit(itemRawString);
            return lexInsideAction;
        }

        // isSpace reports whether r is a space character.
        private static bool isSpace(int r)
        {
            return r == ' ' || r == '\t';
        }

        // isEndOfLine reports whether r is an end-of-line character.
        private static bool isEndOfLine(int r)
        {
            return r == '\r' || r == '\n';
        }

        // isAlphaNumeric reports whether r is an alphabetic, digit, or underscore.
        private static bool isAlphaNumeric(int r)
        {
            return r == '_' || unicode.IsLetter(r) || unicode.IsDigit(r);
        }
    }
}}}
