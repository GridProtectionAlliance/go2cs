// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package parse -- go2cs converted at 2022 March 13 05:38:54 UTC
// import "text/template/parse" ==> using parse = go.text.template.parse_package
// Original source: C:\Program Files\Go\src\text\template\parse\lex.go
namespace go.text.template;

using fmt = fmt_package;
using strings = strings_package;
using unicode = unicode_package;
using utf8 = unicode.utf8_package;


// item represents a token or text string returned from the scanner.

using System.Threading;
public static partial class parse_package {

private partial struct item {
    public itemType typ; // The type of this item.
    public Pos pos; // The starting position, in bytes, of this item in the input string.
    public @string val; // The value of this item.
    public nint line; // The line number at the start of this item.
}

private static @string String(this item i) {

    if (i.typ == itemEOF) 
        return "EOF";
    else if (i.typ == itemError) 
        return i.val;
    else if (i.typ > itemKeyword) 
        return fmt.Sprintf("<%s>", i.val);
    else if (len(i.val) > 10) 
        return fmt.Sprintf("%.10q...", i.val);
        return fmt.Sprintf("%q", i.val);
}

// itemType identifies the type of lex items.
private partial struct itemType { // : nint
}

private static readonly itemType itemError = iota; // error occurred; value is text of error
private static readonly var itemBool = 0; // boolean constant
private static readonly var itemChar = 1; // printable ASCII character; grab bag for comma etc.
private static readonly var itemCharConstant = 2; // character constant
private static readonly var itemComment = 3; // comment text
private static readonly var itemComplex = 4; // complex constant (1+2i); imaginary is just a number
private static readonly var itemAssign = 5; // equals ('=') introducing an assignment
private static readonly var itemDeclare = 6; // colon-equals (':=') introducing a declaration
private static readonly var itemEOF = 7;
private static readonly var itemField = 8; // alphanumeric identifier starting with '.'
private static readonly var itemIdentifier = 9; // alphanumeric identifier not starting with '.'
private static readonly var itemLeftDelim = 10; // left action delimiter
private static readonly var itemLeftParen = 11; // '(' inside action
private static readonly var itemNumber = 12; // simple number, including imaginary
private static readonly var itemPipe = 13; // pipe symbol
private static readonly var itemRawString = 14; // raw quoted string (includes quotes)
private static readonly var itemRightDelim = 15; // right action delimiter
private static readonly var itemRightParen = 16; // ')' inside action
private static readonly var itemSpace = 17; // run of spaces separating arguments
private static readonly var itemString = 18; // quoted string (includes quotes)
private static readonly var itemText = 19; // plain text
private static readonly var itemVariable = 20; // variable starting with '$', such as '$' or  '$1' or '$hello'
// Keywords appear after all the rest.
private static readonly var itemKeyword = 21; // used only to delimit the keywords
private static readonly var itemBlock = 22; // block keyword
private static readonly var itemDot = 23; // the cursor, spelled '.'
private static readonly var itemDefine = 24; // define keyword
private static readonly var itemElse = 25; // else keyword
private static readonly var itemEnd = 26; // end keyword
private static readonly var itemIf = 27; // if keyword
private static readonly var itemNil = 28; // the untyped nil constant, easiest to treat as a keyword
private static readonly var itemRange = 29; // range keyword
private static readonly var itemTemplate = 30; // template keyword
private static readonly var itemWith = 31; // with keyword

private static map key = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, itemType>{".":itemDot,"block":itemBlock,"define":itemDefine,"else":itemElse,"end":itemEnd,"if":itemIf,"range":itemRange,"nil":itemNil,"template":itemTemplate,"with":itemWith,};

private static readonly nint eof = -1;

// Trimming spaces.
// If the action begins "{{- " rather than "{{", then all space/tab/newlines
// preceding the action are trimmed; conversely if it ends " -}}" the
// leading spaces are trimmed. This is done entirely in the lexer; the
// parser never sees it happen. We require an ASCII space (' ', \t, \r, \n)
// to be present to avoid ambiguity with things like "{{-3}}". It reads
// better with the space present anyway. For simplicity, only ASCII
// does the job.


// Trimming spaces.
// If the action begins "{{- " rather than "{{", then all space/tab/newlines
// preceding the action are trimmed; conversely if it ends " -}}" the
// leading spaces are trimmed. This is done entirely in the lexer; the
// parser never sees it happen. We require an ASCII space (' ', \t, \r, \n)
// to be present to avoid ambiguity with things like "{{-3}}". It reads
// better with the space present anyway. For simplicity, only ASCII
// does the job.
private static readonly @string spaceChars = " \t\r\n"; // These are the space characters defined by Go itself.
private static readonly char trimMarker = '-'; // Attached to left/right delimiter, trims trailing spaces from preceding/following text.
private static readonly var trimMarkerLen = Pos(1 + 1); // marker plus space before or after

// stateFn represents the state of the scanner as a function that returns the next state.
public delegate  stateFn stateFn(ptr<lexer>);

// lexer holds the state of the scanner.
private partial struct lexer {
    public @string name; // the name of the input; used only for error reports
    public @string input; // the string being scanned
    public @string leftDelim; // start of action
    public @string rightDelim; // end of action
    public bool emitComment; // emit itemComment tokens.
    public Pos pos; // current position in the input
    public Pos start; // start position of this item
    public Pos width; // width of last rune read from input
    public channel<item> items; // channel of scanned items
    public nint parenDepth; // nesting depth of ( ) exprs
    public nint line; // 1+number of newlines seen
    public nint startLine; // start line of this item
}

// next returns the next rune in the input.
private static int next(this ptr<lexer> _addr_l) {
    ref lexer l = ref _addr_l.val;

    if (int(l.pos) >= len(l.input)) {
        l.width = 0;
        return eof;
    }
    var (r, w) = utf8.DecodeRuneInString(l.input[(int)l.pos..]);
    l.width = Pos(w);
    l.pos += l.width;
    if (r == '\n') {
        l.line++;
    }
    return r;
}

// peek returns but does not consume the next rune in the input.
private static int peek(this ptr<lexer> _addr_l) {
    ref lexer l = ref _addr_l.val;

    var r = l.next();
    l.backup();
    return r;
}

// backup steps back one rune. Can only be called once per call of next.
private static void backup(this ptr<lexer> _addr_l) {
    ref lexer l = ref _addr_l.val;

    l.pos -= l.width; 
    // Correct newline count.
    if (l.width == 1 && l.input[l.pos] == '\n') {
        l.line--;
    }
}

// emit passes an item back to the client.
private static void emit(this ptr<lexer> _addr_l, itemType t) {
    ref lexer l = ref _addr_l.val;

    l.items.Send(new item(t,l.start,l.input[l.start:l.pos],l.startLine));
    l.start = l.pos;
    l.startLine = l.line;
}

// ignore skips over the pending input before this point.
private static void ignore(this ptr<lexer> _addr_l) {
    ref lexer l = ref _addr_l.val;

    l.line += strings.Count(l.input[(int)l.start..(int)l.pos], "\n");
    l.start = l.pos;
    l.startLine = l.line;
}

// accept consumes the next rune if it's from the valid set.
private static bool accept(this ptr<lexer> _addr_l, @string valid) {
    ref lexer l = ref _addr_l.val;

    if (strings.ContainsRune(valid, l.next())) {
        return true;
    }
    l.backup();
    return false;
}

// acceptRun consumes a run of runes from the valid set.
private static void acceptRun(this ptr<lexer> _addr_l, @string valid) {
    ref lexer l = ref _addr_l.val;

    while (strings.ContainsRune(valid, l.next())) {
    }
    l.backup();
}

// errorf returns an error token and terminates the scan by passing
// back a nil pointer that will be the next state, terminating l.nextItem.
private static stateFn errorf(this ptr<lexer> _addr_l, @string format, params object[] args) {
    args = args.Clone();
    ref lexer l = ref _addr_l.val;

    l.items.Send(new item(itemError,l.start,fmt.Sprintf(format,args...),l.startLine));
    return null;
}

// nextItem returns the next item from the input.
// Called by the parser, not in the lexing goroutine.
private static item nextItem(this ptr<lexer> _addr_l) {
    ref lexer l = ref _addr_l.val;

    return l.items.Receive();
}

// drain drains the output so the lexing goroutine will exit.
// Called by the parser, not in the lexing goroutine.
private static void drain(this ptr<lexer> _addr_l) {
    ref lexer l = ref _addr_l.val;

    foreach (>>MARKER:FORRANGEEXPRESSIONS_LEVEL_1<< in l.items) {>>MARKER:FORRANGEMUTABLEEXPRESSIONS_LEVEL_1<<
    }
}

// lex creates a new scanner for the input string.
private static ptr<lexer> lex(@string name, @string input, @string left, @string right, bool emitComment) {
    if (left == "") {
        left = leftDelim;
    }
    if (right == "") {
        right = rightDelim;
    }
    ptr<lexer> l = addr(new lexer(name:name,input:input,leftDelim:left,rightDelim:right,emitComment:emitComment,items:make(chanitem),line:1,startLine:1,));
    go_(() => l.run());
    return _addr_l!;
}

// run runs the state machine for the lexer.
private static void run(this ptr<lexer> _addr_l) {
    ref lexer l = ref _addr_l.val;

    {
        var state = lexText;

        while (state != null) {
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
private static stateFn lexText(ptr<lexer> _addr_l) {
    ref lexer l = ref _addr_l.val;

    l.width = 0;
    {
        var x = strings.Index(l.input[(int)l.pos..], l.leftDelim);

        if (x >= 0) {
            var ldn = Pos(len(l.leftDelim));
            l.pos += Pos(x);
            var trimLength = Pos(0);
            if (hasLeftTrimMarker(l.input[(int)l.pos + ldn..])) {
                trimLength = rightTrimLength(l.input[(int)l.start..(int)l.pos]);
            }
            l.pos -= trimLength;
            if (l.pos > l.start) {
                l.line += strings.Count(l.input[(int)l.start..(int)l.pos], "\n");
                l.emit(itemText);
            }
            l.pos += trimLength;
            l.ignore();
            return lexLeftDelim;
        }
    }
    l.pos = Pos(len(l.input)); 
    // Correctly reached EOF.
    if (l.pos > l.start) {
        l.line += strings.Count(l.input[(int)l.start..(int)l.pos], "\n");
        l.emit(itemText);
    }
    l.emit(itemEOF);
    return null;
}

// rightTrimLength returns the length of the spaces at the end of the string.
private static Pos rightTrimLength(@string s) {
    return Pos(len(s) - len(strings.TrimRight(s, spaceChars)));
}

// atRightDelim reports whether the lexer is at a right delimiter, possibly preceded by a trim marker.
private static (bool, bool) atRightDelim(this ptr<lexer> _addr_l) {
    bool delim = default;
    bool trimSpaces = default;
    ref lexer l = ref _addr_l.val;

    if (hasRightTrimMarker(l.input[(int)l.pos..]) && strings.HasPrefix(l.input[(int)l.pos + trimMarkerLen..], l.rightDelim)) { // With trim marker.
        return (true, true);
    }
    if (strings.HasPrefix(l.input[(int)l.pos..], l.rightDelim)) { // Without trim marker.
        return (true, false);
    }
    return (false, false);
}

// leftTrimLength returns the length of the spaces at the beginning of the string.
private static Pos leftTrimLength(@string s) {
    return Pos(len(s) - len(strings.TrimLeft(s, spaceChars)));
}

// lexLeftDelim scans the left delimiter, which is known to be present, possibly with a trim marker.
private static stateFn lexLeftDelim(ptr<lexer> _addr_l) {
    ref lexer l = ref _addr_l.val;

    l.pos += Pos(len(l.leftDelim));
    var trimSpace = hasLeftTrimMarker(l.input[(int)l.pos..]);
    var afterMarker = Pos(0);
    if (trimSpace) {
        afterMarker = trimMarkerLen;
    }
    if (strings.HasPrefix(l.input[(int)l.pos + afterMarker..], leftComment)) {
        l.pos += afterMarker;
        l.ignore();
        return lexComment;
    }
    l.emit(itemLeftDelim);
    l.pos += afterMarker;
    l.ignore();
    l.parenDepth = 0;
    return lexInsideAction;
}

// lexComment scans a comment. The left comment marker is known to be present.
private static stateFn lexComment(ptr<lexer> _addr_l) {
    ref lexer l = ref _addr_l.val;

    l.pos += Pos(len(leftComment));
    var i = strings.Index(l.input[(int)l.pos..], rightComment);
    if (i < 0) {
        return l.errorf("unclosed comment");
    }
    l.pos += Pos(i + len(rightComment));
    var (delim, trimSpace) = l.atRightDelim();
    if (!delim) {
        return l.errorf("comment ends before closing delimiter");
    }
    if (l.emitComment) {
        l.emit(itemComment);
    }
    if (trimSpace) {
        l.pos += trimMarkerLen;
    }
    l.pos += Pos(len(l.rightDelim));
    if (trimSpace) {
        l.pos += leftTrimLength(l.input[(int)l.pos..]);
    }
    l.ignore();
    return lexText;
}

// lexRightDelim scans the right delimiter, which is known to be present, possibly with a trim marker.
private static stateFn lexRightDelim(ptr<lexer> _addr_l) {
    ref lexer l = ref _addr_l.val;

    var trimSpace = hasRightTrimMarker(l.input[(int)l.pos..]);
    if (trimSpace) {
        l.pos += trimMarkerLen;
        l.ignore();
    }
    l.pos += Pos(len(l.rightDelim));
    l.emit(itemRightDelim);
    if (trimSpace) {
        l.pos += leftTrimLength(l.input[(int)l.pos..]);
        l.ignore();
    }
    return lexText;
}

// lexInsideAction scans the elements inside action delimiters.
private static stateFn lexInsideAction(ptr<lexer> _addr_l) {
    ref lexer l = ref _addr_l.val;
 
    // Either number, quoted string, or identifier.
    // Spaces separate arguments; runs of spaces turn into itemSpace.
    // Pipe symbols separate and are emitted.
    var (delim, _) = l.atRightDelim();
    if (delim) {
        if (l.parenDepth == 0) {
            return lexRightDelim;
        }
        return l.errorf("unclosed left paren");
    }
    {
        var r__prev1 = r;

        var r = l.next();


        if (r == eof)
        {
            return l.errorf("unclosed action");
            goto __switch_break0;
        }
        if (isSpace(r))
        {
            l.backup(); // Put space back in case we have " -}}".
            return lexSpace;
            goto __switch_break0;
        }
        if (r == '=')
        {
            l.emit(itemAssign);
            goto __switch_break0;
        }
        if (r == ':')
        {
            if (l.next() != '=') {
                return l.errorf("expected :=");
            }
            l.emit(itemDeclare);
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
            if (l.pos < Pos(len(l.input))) {
                r = l.input[l.pos];
                if (r < '0' || '9' < r) {
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
            if (l.parenDepth < 0) {
                return l.errorf("unexpected right paren %#U", r);
            }
            goto __switch_break0;
        }
        if (r <= unicode.MaxASCII && unicode.IsPrint(r))
        {
            l.emit(itemChar);
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
// We have not consumed the first space, which is known to be present.
// Take care if there is a trim-marked right delimiter, which starts with a space.
private static stateFn lexSpace(ptr<lexer> _addr_l) {
    ref lexer l = ref _addr_l.val;

    int r = default;
    nint numSpaces = default;
    while (true) {
        r = l.peek();
        if (!isSpace(r)) {
            break;
        }
        l.next();
        numSpaces++;
    } 
    // Be careful about a trim-marked closing delimiter, which has a minus
    // after a space. We know there is a space, so check for the '-' that might follow.
    if (hasRightTrimMarker(l.input[(int)l.pos - 1..]) && strings.HasPrefix(l.input[(int)l.pos - 1 + trimMarkerLen..], l.rightDelim)) {
        l.backup(); // Before the space.
        if (numSpaces == 1) {
            return lexRightDelim; // On the delim, so go right to that.
        }
    }
    l.emit(itemSpace);
    return lexInsideAction;
}

// lexIdentifier scans an alphanumeric.
private static stateFn lexIdentifier(ptr<lexer> _addr_l) {
    ref lexer l = ref _addr_l.val;

Loop:
    while (true) {
        {
            var r = l.next();


            if (isAlphaNumeric(r))             else 
                l.backup();
                var word = l.input[(int)l.start..(int)l.pos];
                if (!l.atTerminator()) {
                    return l.errorf("bad character %#U", r);
                }

                if (key[word] > itemKeyword) 
                    l.emit(key[word]);
                else if (word[0] == '.') 
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
private static stateFn lexField(ptr<lexer> _addr_l) {
    ref lexer l = ref _addr_l.val;

    return lexFieldOrVariable(_addr_l, itemField);
}

// lexVariable scans a Variable: $Alphanumeric.
// The $ has been scanned.
private static stateFn lexVariable(ptr<lexer> _addr_l) {
    ref lexer l = ref _addr_l.val;

    if (l.atTerminator()) { // Nothing interesting follows -> "$".
        l.emit(itemVariable);
        return lexInsideAction;
    }
    return lexFieldOrVariable(_addr_l, itemVariable);
}

// lexVariable scans a field or variable: [.$]Alphanumeric.
// The . or $ has been scanned.
private static stateFn lexFieldOrVariable(ptr<lexer> _addr_l, itemType typ) {
    ref lexer l = ref _addr_l.val;

    if (l.atTerminator()) { // Nothing interesting follows -> "." or "$".
        if (typ == itemVariable) {
            l.emit(itemVariable);
        }
        else
 {
            l.emit(itemDot);
        }
        return lexInsideAction;
    }
    int r = default;
    while (true) {
        r = l.next();
        if (!isAlphaNumeric(r)) {
            l.backup();
            break;
        }
    }
    if (!l.atTerminator()) {
        return l.errorf("bad character %#U", r);
    }
    l.emit(typ);
    return lexInsideAction;
}

// atTerminator reports whether the input is at valid termination character to
// appear after an identifier. Breaks .X.Y into two pieces. Also catches cases
// like "$x+2" not being acceptable without a space, in case we decide one
// day to implement arithmetic.
private static bool atTerminator(this ptr<lexer> _addr_l) {
    ref lexer l = ref _addr_l.val;

    var r = l.peek();
    if (isSpace(r)) {
        return true;
    }

    if (r == eof || r == '.' || r == ',' || r == '|' || r == ':' || r == ')' || r == '(') 
        return true;
    // Does r start the delimiter? This can be ambiguous (with delim=="//", $x/2 will
    // succeed but should fail) but only in extremely rare cases caused by willfully
    // bad choice of delimiter.
    {
        var (rd, _) = utf8.DecodeRuneInString(l.rightDelim);

        if (rd == r) {
            return true;
        }
    }
    return false;
}

// lexChar scans a character constant. The initial quote is already
// scanned. Syntax checking is done by the parser.
private static stateFn lexChar(ptr<lexer> _addr_l) {
    ref lexer l = ref _addr_l.val;

Loop:
    while (true) {

        if (l.next() == '\\')
        {
            {
                var r = l.next();

                if (r != eof && r != '\n') {
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
private static stateFn lexNumber(ptr<lexer> _addr_l) {
    ref lexer l = ref _addr_l.val;

    if (!l.scanNumber()) {
        return l.errorf("bad number syntax: %q", l.input[(int)l.start..(int)l.pos]);
    }
    {
        var sign = l.peek();

        if (sign == '+' || sign == '-') { 
            // Complex: 1+2i. No spaces, must end in 'i'.
            if (!l.scanNumber() || l.input[l.pos - 1] != 'i') {
                return l.errorf("bad number syntax: %q", l.input[(int)l.start..(int)l.pos]);
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

private static bool scanNumber(this ptr<lexer> _addr_l) {
    ref lexer l = ref _addr_l.val;
 
    // Optional leading sign.
    l.accept("+-"); 
    // Is it hex?
    @string digits = "0123456789_";
    if (l.accept("0")) { 
        // Note: Leading 0 does not mean octal in floats.
        if (l.accept("xX")) {
            digits = "0123456789abcdefABCDEF_";
        }
        else if (l.accept("oO")) {
            digits = "01234567_";
        }
        else if (l.accept("bB")) {
            digits = "01_";
        }
    }
    l.acceptRun(digits);
    if (l.accept(".")) {
        l.acceptRun(digits);
    }
    if (len(digits) == 10 + 1 && l.accept("eE")) {
        l.accept("+-");
        l.acceptRun("0123456789_");
    }
    if (len(digits) == 16 + 6 + 1 && l.accept("pP")) {
        l.accept("+-");
        l.acceptRun("0123456789_");
    }
    l.accept("i"); 
    // Next thing mustn't be alphanumeric.
    if (isAlphaNumeric(l.peek())) {
        l.next();
        return false;
    }
    return true;
}

// lexQuote scans a quoted string.
private static stateFn lexQuote(ptr<lexer> _addr_l) {
    ref lexer l = ref _addr_l.val;

Loop:
    while (true) {

        if (l.next() == '\\')
        {
            {
                var r = l.next();

                if (r != eof && r != '\n') {
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
private static stateFn lexRawQuote(ptr<lexer> _addr_l) {
    ref lexer l = ref _addr_l.val;

Loop:
    while (true) {

        if (l.next() == eof) 
            return l.errorf("unterminated raw quoted string");
        else if (l.next() == '`') 
            _breakLoop = true;
            break;
            }
    l.emit(itemRawString);
    return lexInsideAction;
}

// isSpace reports whether r is a space character.
private static bool isSpace(int r) {
    return r == ' ' || r == '\t' || r == '\r' || r == '\n';
}

// isAlphaNumeric reports whether r is an alphabetic, digit, or underscore.
private static bool isAlphaNumeric(int r) {
    return r == '_' || unicode.IsLetter(r) || unicode.IsDigit(r);
}

private static bool hasLeftTrimMarker(@string s) {
    return len(s) >= 2 && s[0] == trimMarker && isSpace(rune(s[1]));
}

private static bool hasRightTrimMarker(@string s) {
    return len(s) >= 2 && isSpace(rune(s[0])) && s[1] == trimMarker;
}

} // end parse_package
