// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package xml implements a simple XML 1.0 parser that
// understands XML name spaces.

// package xml -- go2cs converted at 2022 March 13 05:40:09 UTC
// import "encoding/xml" ==> using xml = go.encoding.xml_package
// Original source: C:\Program Files\Go\src\encoding\xml\xml.go
namespace go.encoding;
// References:
//    Annotated XML spec: https://www.xml.com/axml/testaxml.htm
//    XML name spaces: https://www.w3.org/TR/REC-xml-names/

// TODO(rsc):
//    Test error handling.


using bufio = bufio_package;
using bytes = bytes_package;
using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using strconv = strconv_package;
using strings = strings_package;
using unicode = unicode_package;
using utf8 = unicode.utf8_package;


// A SyntaxError represents a syntax error in the XML input stream.

using System;
public static partial class xml_package {

public partial struct SyntaxError {
    public @string Msg;
    public nint Line;
}

private static @string Error(this ptr<SyntaxError> _addr_e) {
    ref SyntaxError e = ref _addr_e.val;

    return "XML syntax error on line " + strconv.Itoa(e.Line) + ": " + e.Msg;
}

// A Name represents an XML name (Local) annotated
// with a name space identifier (Space).
// In tokens returned by Decoder.Token, the Space identifier
// is given as a canonical URL, not the short prefix used
// in the document being parsed.
public partial struct Name {
    public @string Space;
    public @string Local;
}

// An Attr represents an attribute in an XML element (Name=Value).
public partial struct Attr {
    public Name Name;
    public @string Value;
}

// A Token is an interface holding one of the token types:
// StartElement, EndElement, CharData, Comment, ProcInst, or Directive.
public partial interface Token {
}

// A StartElement represents an XML start element.
public partial struct StartElement {
    public Name Name;
    public slice<Attr> Attr;
}

// Copy creates a new copy of StartElement.
public static StartElement Copy(this StartElement e) {
    var attrs = make_slice<Attr>(len(e.Attr));
    copy(attrs, e.Attr);
    e.Attr = attrs;
    return e;
}

// End returns the corresponding XML end element.
public static EndElement End(this StartElement e) {
    return new EndElement(e.Name);
}

// An EndElement represents an XML end element.
public partial struct EndElement {
    public Name Name;
}

// A CharData represents XML character data (raw text),
// in which XML escape sequences have been replaced by
// the characters they represent.
public partial struct CharData { // : slice<byte>
}

private static slice<byte> makeCopy(slice<byte> b) {
    var b1 = make_slice<byte>(len(b));
    copy(b1, b);
    return b1;
}

// Copy creates a new copy of CharData.
public static CharData Copy(this CharData c) {
    return CharData(makeCopy(c));
}

// A Comment represents an XML comment of the form <!--comment-->.
// The bytes do not include the <!-- and --> comment markers.
public partial struct Comment { // : slice<byte>
}

// Copy creates a new copy of Comment.
public static Comment Copy(this Comment c) {
    return Comment(makeCopy(c));
}

// A ProcInst represents an XML processing instruction of the form <?target inst?>
public partial struct ProcInst {
    public @string Target;
    public slice<byte> Inst;
}

// Copy creates a new copy of ProcInst.
public static ProcInst Copy(this ProcInst p) {
    p.Inst = makeCopy(p.Inst);
    return p;
}

// A Directive represents an XML directive of the form <!text>.
// The bytes do not include the <! and > markers.
public partial struct Directive { // : slice<byte>
}

// Copy creates a new copy of Directive.
public static Directive Copy(this Directive d) {
    return Directive(makeCopy(d));
}

// CopyToken returns a copy of a Token.
public static Token CopyToken(Token t) {
    switch (t.type()) {
        case CharData v:
            return v.Copy();
            break;
        case Comment v:
            return v.Copy();
            break;
        case Directive v:
            return v.Copy();
            break;
        case ProcInst v:
            return v.Copy();
            break;
        case StartElement v:
            return v.Copy();
            break;
    }
    return t;
}

// A TokenReader is anything that can decode a stream of XML tokens, including a
// Decoder.
//
// When Token encounters an error or end-of-file condition after successfully
// reading a token, it returns the token. It may return the (non-nil) error from
// the same call or return the error (and a nil token) from a subsequent call.
// An instance of this general case is that a TokenReader returning a non-nil
// token at the end of the token stream may return either io.EOF or a nil error.
// The next Read should return nil, io.EOF.
//
// Implementations of Token are discouraged from returning a nil token with a
// nil error. Callers should treat a return of nil, nil as indicating that
// nothing happened; in particular it does not indicate EOF.
public partial interface TokenReader {
    (Token, error) Token();
}

// A Decoder represents an XML parser reading a particular input stream.
// The parser assumes that its input is encoded in UTF-8.
public partial struct Decoder {
    public bool Strict; // When Strict == false, AutoClose indicates a set of elements to
// consider closed immediately after they are opened, regardless
// of whether an end element is present.
    public slice<@string> AutoClose; // Entity can be used to map non-standard entity names to string replacements.
// The parser behaves as if these standard mappings are present in the map,
// regardless of the actual map content:
//
//    "lt": "<",
//    "gt": ">",
//    "amp": "&",
//    "apos": "'",
//    "quot": `"`,
    public map<@string, @string> Entity; // CharsetReader, if non-nil, defines a function to generate
// charset-conversion readers, converting from the provided
// non-UTF-8 charset into UTF-8. If CharsetReader is nil or
// returns an error, parsing stops with an error. One of the
// CharsetReader's result values must be non-nil.
    public Func<@string, io.Reader, (io.Reader, error)> CharsetReader; // DefaultSpace sets the default name space used for unadorned tags,
// as if the entire XML stream were wrapped in an element containing
// the attribute xmlns="DefaultSpace".
    public @string DefaultSpace;
    public io.ByteReader r;
    public TokenReader t;
    public bytes.Buffer buf;
    public ptr<bytes.Buffer> saved;
    public ptr<stack> stk;
    public ptr<stack> free;
    public bool needClose;
    public Name toClose;
    public Token nextToken;
    public nint nextByte;
    public map<@string, @string> ns;
    public error err;
    public nint line;
    public long offset;
    public nint unmarshalDepth;
}

// NewDecoder creates a new XML parser reading from r.
// If r does not implement io.ByteReader, NewDecoder will
// do its own buffering.
public static ptr<Decoder> NewDecoder(io.Reader r) {
    ptr<Decoder> d = addr(new Decoder(ns:make(map[string]string),nextByte:-1,line:1,Strict:true,));
    d.switchToReader(r);
    return _addr_d!;
}

// NewTokenDecoder creates a new XML parser using an underlying token stream.
public static ptr<Decoder> NewTokenDecoder(TokenReader t) { 
    // Is it already a Decoder?
    {
        ptr<Decoder> d__prev1 = d;

        ptr<Decoder> (d, ok) = t._<ptr<Decoder>>();

        if (ok) {
            return _addr_d!;
        }
        d = d__prev1;

    }
    ptr<Decoder> d = addr(new Decoder(ns:make(map[string]string),t:t,nextByte:-1,line:1,Strict:true,));
    return _addr_d!;
}

// Token returns the next XML token in the input stream.
// At the end of the input stream, Token returns nil, io.EOF.
//
// Slices of bytes in the returned token data refer to the
// parser's internal buffer and remain valid only until the next
// call to Token. To acquire a copy of the bytes, call CopyToken
// or the token's Copy method.
//
// Token expands self-closing elements such as <br>
// into separate start and end elements returned by successive calls.
//
// Token guarantees that the StartElement and EndElement
// tokens it returns are properly nested and matched:
// if Token encounters an unexpected end element
// or EOF before all expected end elements,
// it will return an error.
//
// Token implements XML name spaces as described by
// https://www.w3.org/TR/REC-xml-names/. Each of the
// Name structures contained in the Token has the Space
// set to the URL identifying its name space when known.
// If Token encounters an unrecognized name space prefix,
// it uses the prefix as the Space rather than report an error.
private static (Token, error) Token(this ptr<Decoder> _addr_d) {
    Token _p0 = default;
    error _p0 = default!;
    ref Decoder d = ref _addr_d.val;

    Token t = default!;
    error err = default!;
    if (d.stk != null && d.stk.kind == stkEOF) {
        return (null, error.As(io.EOF)!);
    }
    if (d.nextToken != null) {
        t = Token.As(d.nextToken)!;
        d.nextToken = null;
    }
    else
 {
        t, err = d.rawToken();

        if (t == null && err != null) {
            if (err == io.EOF && d.stk != null && d.stk.kind != stkEOF) {
                err = error.As(d.syntaxError("unexpected EOF"))!;
            }
            return (null, error.As(err)!);
        }
        err = error.As(null)!;
    }
    if (!d.Strict) {
        {
            var t1__prev2 = t1;

            var (t1, ok) = d.autoClose(t);

            if (ok) {
                d.nextToken = t;
                t = Token.As(t1)!;
            }

            t1 = t1__prev2;

        }
    }
    switch (t.type()) {
        case StartElement t1:
            foreach (var (_, a) in t1.Attr) {
                if (a.Name.Space == xmlnsPrefix) {
                    var (v, ok) = d.ns[a.Name.Local];
                    d.pushNs(a.Name.Local, v, ok);
                    d.ns[a.Name.Local] = a.Value;
                }
                if (a.Name.Space == "" && a.Name.Local == xmlnsPrefix) { 
                    // Default space for untagged names
                    (v, ok) = d.ns[""];
                    d.pushNs("", v, ok);
                    d.ns[""] = a.Value;
                }
            }
            d.translate(_addr_t1.Name, true);
            foreach (var (i) in t1.Attr) {
                d.translate(_addr_t1.Attr[i].Name, false);
            }
            d.pushElement(t1.Name);
            t = Token.As(t1)!;
            break;
        case EndElement t1:
            d.translate(_addr_t1.Name, true);
            if (!d.popElement(_addr_t1)) {
                return (null, error.As(d.err)!);
            }
            t = Token.As(t1)!;
            break;
    }
    return (t, error.As(err)!);
}

private static readonly @string xmlURL = "http://www.w3.org/XML/1998/namespace";
private static readonly @string xmlnsPrefix = "xmlns";
private static readonly @string xmlPrefix = "xml";

// Apply name space translation to name n.
// The default name space (for Space=="")
// applies only to element names, not to attribute names.
private static void translate(this ptr<Decoder> _addr_d, ptr<Name> _addr_n, bool isElementName) {
    ref Decoder d = ref _addr_d.val;
    ref Name n = ref _addr_n.val;


    if (n.Space == xmlnsPrefix) 
        return ;
    else if (n.Space == "" && !isElementName) 
        return ;
    else if (n.Space == xmlPrefix) 
        n.Space = xmlURL;
    else if (n.Space == "" && n.Local == xmlnsPrefix) 
        return ;
        {
        var (v, ok) = d.ns[n.Space];

        if (ok) {
            n.Space = v;
        }
        else if (n.Space == "") {
            n.Space = d.DefaultSpace;
        }

    }
}

private static void switchToReader(this ptr<Decoder> _addr_d, io.Reader r) {
    ref Decoder d = ref _addr_d.val;
 
    // Get efficient byte at a time reader.
    // Assume that if reader has its own
    // ReadByte, it's efficient enough.
    // Otherwise, use bufio.
    {
        io.ByteReader (rb, ok) = r._<io.ByteReader>();

        if (ok) {
            d.r = rb;
        }
        else
 {
            d.r = bufio.NewReader(r);
        }
    }
}

// Parsing state - stack holds old name space translations
// and the current set of open elements. The translations to pop when
// ending a given tag are *below* it on the stack, which is
// more work but forced on us by XML.
private partial struct stack {
    public ptr<stack> next;
    public nint kind;
    public Name name;
    public bool ok;
}

private static readonly var stkStart = iota;
private static readonly var stkNs = 0;
private static readonly var stkEOF = 1;

private static ptr<stack> push(this ptr<Decoder> _addr_d, nint kind) {
    ref Decoder d = ref _addr_d.val;

    var s = d.free;
    if (s != null) {
        d.free = s.next;
    }
    else
 {
        s = @new<stack>();
    }
    s.next = d.stk;
    s.kind = kind;
    d.stk = s;
    return _addr_s!;
}

private static ptr<stack> pop(this ptr<Decoder> _addr_d) {
    ref Decoder d = ref _addr_d.val;

    var s = d.stk;
    if (s != null) {
        d.stk = s.next;
        s.next = d.free;
        d.free = s;
    }
    return _addr_s!;
}

// Record that after the current element is finished
// (that element is already pushed on the stack)
// Token should return EOF until popEOF is called.
private static void pushEOF(this ptr<Decoder> _addr_d) {
    ref Decoder d = ref _addr_d.val;
 
    // Walk down stack to find Start.
    // It might not be the top, because there might be stkNs
    // entries above it.
    var start = d.stk;
    while (start.kind != stkStart) {
        start = start.next;
    } 
    // The stkNs entries below a start are associated with that
    // element too; skip over them.
    while (start.next != null && start.next.kind == stkNs) {
        start = start.next;
    }
    var s = d.free;
    if (s != null) {
        d.free = s.next;
    }
    else
 {
        s = @new<stack>();
    }
    s.kind = stkEOF;
    s.next = start.next;
    start.next = s;
}

// Undo a pushEOF.
// The element must have been finished, so the EOF should be at the top of the stack.
private static bool popEOF(this ptr<Decoder> _addr_d) {
    ref Decoder d = ref _addr_d.val;

    if (d.stk == null || d.stk.kind != stkEOF) {
        return false;
    }
    d.pop();
    return true;
}

// Record that we are starting an element with the given name.
private static void pushElement(this ptr<Decoder> _addr_d, Name name) {
    ref Decoder d = ref _addr_d.val;

    var s = d.push(stkStart);
    s.name = name;
}

// Record that we are changing the value of ns[local].
// The old value is url, ok.
private static void pushNs(this ptr<Decoder> _addr_d, @string local, @string url, bool ok) {
    ref Decoder d = ref _addr_d.val;

    var s = d.push(stkNs);
    s.name.Local = local;
    s.name.Space = url;
    s.ok = ok;
}

// Creates a SyntaxError with the current line number.
private static error syntaxError(this ptr<Decoder> _addr_d, @string msg) {
    ref Decoder d = ref _addr_d.val;

    return error.As(addr(new SyntaxError(Msg:msg,Line:d.line))!)!;
}

// Record that we are ending an element with the given name.
// The name must match the record at the top of the stack,
// which must be a pushElement record.
// After popping the element, apply any undo records from
// the stack to restore the name translations that existed
// before we saw this element.
private static bool popElement(this ptr<Decoder> _addr_d, ptr<EndElement> _addr_t) {
    ref Decoder d = ref _addr_d.val;
    ref EndElement t = ref _addr_t.val;

    var s = d.pop();
    var name = t.Name;

    if (s == null || s.kind != stkStart) 
        d.err = d.syntaxError("unexpected end element </" + name.Local + ">");
        return false;
    else if (s.name.Local != name.Local) 
        if (!d.Strict) {
            d.needClose = true;
            d.toClose = t.Name;
            t.Name = s.name;
            return true;
        }
        d.err = d.syntaxError("element <" + s.name.Local + "> closed by </" + name.Local + ">");
        return false;
    else if (s.name.Space != name.Space) 
        d.err = d.syntaxError("element <" + s.name.Local + "> in space " + s.name.Space + "closed by </" + name.Local + "> in space " + name.Space);
        return false;
    // Pop stack until a Start or EOF is on the top, undoing the
    // translations that were associated with the element we just closed.
    while (d.stk != null && d.stk.kind != stkStart && d.stk.kind != stkEOF) {
        s = d.pop();
        if (s.ok) {
            d.ns[s.name.Local] = s.name.Space;
        }
        else
 {
            delete(d.ns, s.name.Local);
        }
    }

    return true;
}

// If the top element on the stack is autoclosing and
// t is not the end tag, invent the end tag.
private static (Token, bool) autoClose(this ptr<Decoder> _addr_d, Token t) {
    Token _p0 = default;
    bool _p0 = default;
    ref Decoder d = ref _addr_d.val;

    if (d.stk == null || d.stk.kind != stkStart) {
        return (null, false);
    }
    var name = strings.ToLower(d.stk.name.Local);
    foreach (var (_, s) in d.AutoClose) {
        if (strings.ToLower(s) == name) { 
            // This one should be auto closed if t doesn't close it.
            EndElement (et, ok) = t._<EndElement>();
            if (!ok || et.Name.Local != name) {
                return (new EndElement(d.stk.name), true);
            }
            break;
        }
    }    return (null, false);
}

private static var errRawToken = errors.New("xml: cannot use RawToken from UnmarshalXML method");

// RawToken is like Token but does not verify that
// start and end elements match and does not translate
// name space prefixes to their corresponding URLs.
private static (Token, error) RawToken(this ptr<Decoder> _addr_d) {
    Token _p0 = default;
    error _p0 = default!;
    ref Decoder d = ref _addr_d.val;

    if (d.unmarshalDepth > 0) {
        return (null, error.As(errRawToken)!);
    }
    return d.rawToken();
}

private static (Token, error) rawToken(this ptr<Decoder> _addr_d) => func((_, panic, _) => {
    Token _p0 = default;
    error _p0 = default!;
    ref Decoder d = ref _addr_d.val;

    if (d.t != null) {
        return d.t.Token();
    }
    if (d.err != null) {
        return (null, error.As(d.err)!);
    }
    if (d.needClose) { 
        // The last element we read was self-closing and
        // we returned just the StartElement half.
        // Return the EndElement half now.
        d.needClose = false;
        return (new EndElement(d.toClose), error.As(null!)!);
    }
    var (b, ok) = d.getc();
    if (!ok) {
        return (null, error.As(d.err)!);
    }
    if (b != '<') { 
        // Text section.
        d.ungetc(b);
        var data = d.text(-1, false);
        if (data == null) {
            return (null, error.As(d.err)!);
        }
        return (CharData(data), error.As(null!)!);
    }
    b, ok = d.mustgetc();

    if (!ok) {
        return (null, error.As(d.err)!);
    }
    switch (b) {
        case '/': 
            // </: End element
            Name name = default;
            name, ok = d.nsname();

            if (!ok) {
                if (d.err == null) {
                    d.err = d.syntaxError("expected element name after </");
                }
                return (null, error.As(d.err)!);
            }
            d.space();
            b, ok = d.mustgetc();

            if (!ok) {
                return (null, error.As(d.err)!);
            }
            if (b != '>') {
                d.err = d.syntaxError("invalid characters between </" + name.Local + " and >");
                return (null, error.As(d.err)!);
            }
            return (new EndElement(name), error.As(null!)!);
            break;
        case '?': 
            // <?: Processing instruction.
            @string target = default;
            target, ok = d.name();

            if (!ok) {
                if (d.err == null) {
                    d.err = d.syntaxError("expected target name after <?");
                }
                return (null, error.As(d.err)!);
            }
            d.space();
            d.buf.Reset();
            byte b0 = default;
            while (true) {
                b, ok = d.mustgetc();

                if (!ok) {
                    return (null, error.As(d.err)!);
                }
                d.buf.WriteByte(b);
                if (b0 == '?' && b == '>') {
                    break;
                }
                b0 = b;
            }
            data = d.buf.Bytes();
            data = data[(int)0..(int)len(data) - 2]; // chop ?>

            if (target == "xml") {
                var content = string(data);
                var ver = procInst("version", content);
                if (ver != "" && ver != "1.0") {
                    d.err = fmt.Errorf("xml: unsupported version %q; only version 1.0 is supported", ver);
                    return (null, error.As(d.err)!);
                }
                var enc = procInst("encoding", content);
                if (enc != "" && enc != "utf-8" && enc != "UTF-8" && !strings.EqualFold(enc, "utf-8")) {
                    if (d.CharsetReader == null) {
                        d.err = fmt.Errorf("xml: encoding %q declared but Decoder.CharsetReader is nil", enc);
                        return (null, error.As(d.err)!);
                    }
                    var (newr, err) = d.CharsetReader(enc, d.r._<io.Reader>());
                    if (err != null) {
                        d.err = fmt.Errorf("xml: opening charset %q: %v", enc, err);
                        return (null, error.As(d.err)!);
                    }
                    if (newr == null) {
                        panic("CharsetReader returned a nil Reader for charset " + enc);
                    }
                    d.switchToReader(newr);
                }
            }
            return (new ProcInst(target,data), error.As(null!)!);
            break;
        case '!': 
            // <!: Maybe comment, maybe CDATA.
                    b, ok = d.mustgetc();

                    if (!ok) {
                        return (null, error.As(d.err)!);
                    }
                    switch (b) {
                        case '-': // <!-
                            // Probably <!-- for a comment.
                            b, ok = d.mustgetc();

                            if (!ok) {
                                return (null, error.As(d.err)!);
                            }
                            if (b != '-') {
                                d.err = d.syntaxError("invalid sequence <!- not part of <!--");
                                return (null, error.As(d.err)!);
                            } 
                            // Look for terminator.
                            d.buf.Reset();
                            b0 = default;            byte b1 = default;

                            while (true) {
                                b, ok = d.mustgetc();

                                if (!ok) {
                                    return (null, error.As(d.err)!);
                                }
                                d.buf.WriteByte(b);
                                if (b0 == '-' && b1 == '-') {
                                    if (b != '>') {
                                        d.err = d.syntaxError("invalid sequence \"--\" not allowed in comments");
                                        return (null, error.As(d.err)!);
                                    }
                                    break;
                                }
                                (b0, b1) = (b1, b);
                            }

                            data = d.buf.Bytes();
                            data = data[(int)0..(int)len(data) - 3]; // chop -->
                            return (Comment(data), error.As(null!)!);
                            break;
                        case '[': // <![
                            // Probably <![CDATA[.
                            {
                                nint i__prev1 = i;

                                for (nint i = 0; i < 6; i++) {
                                    b, ok = d.mustgetc();

                                    if (!ok) {
                                        return (null, error.As(d.err)!);
                                    }
                                    if (b != "CDATA["[i]) {
                                        d.err = d.syntaxError("invalid <![ sequence");
                                        return (null, error.As(d.err)!);
                                    }
                                } 
                                // Have <![CDATA[.  Read text until ]]>.


                                i = i__prev1;
                            } 
                            // Have <![CDATA[.  Read text until ]]>.
                            data = d.text(-1, true);
                            if (data == null) {
                                return (null, error.As(d.err)!);
                            }
                            return (CharData(data), error.As(null!)!);
                            break;
                    } 

                    // Probably a directive: <!DOCTYPE ...>, <!ENTITY ...>, etc.
                    // We don't care, but accumulate for caller. Quoted angle
                    // brackets do not count for nesting.
                    d.buf.Reset();
                    d.buf.WriteByte(b);
                    var inquote = uint8(0);
                    nint depth = 0;
                    while (true) {
                        b, ok = d.mustgetc();

                        if (!ok) {
                            return (null, error.As(d.err)!);
                        }
                        if (inquote == 0 && b == '>' && depth == 0) {
                            break;
                        }
            HandleB:
                        d.buf.WriteByte(b);

                        if (b == inquote) 
                            inquote = 0;
                        else if (inquote != 0)             else if (b == '\'' || b == '"') 
                            inquote = b;
                        else if (b == '>' && inquote == 0) 
                            depth--;
                        else if (b == '<' && inquote == 0) 
                            // Look for <!-- to begin comment.
                            @string s = "!--";
                            {
                                nint i__prev2 = i;

                                for (i = 0; i < len(s); i++) {
                                    b, ok = d.mustgetc();

                                    if (!ok) {
                                        return (null, error.As(d.err)!);
                                    }
                                    if (b != s[i]) {
                                        for (nint j = 0; j < i; j++) {
                                            d.buf.WriteByte(s[j]);
                                        }

                                        depth++;
                                        goto HandleB;
                                    }
                                } 

                                // Remove < that was written above.


                                i = i__prev2;
                            } 

                            // Remove < that was written above.
                            d.buf.Truncate(d.buf.Len() - 1); 

                            // Look for terminator.
                            b0 = default;                b1 = default;

                            while (true) {
                                b, ok = d.mustgetc();

                                if (!ok) {
                                    return (null, error.As(d.err)!);
                                }
                                if (b0 == '-' && b1 == '-' && b == '>') {
                                    break;
                                }
                                (b0, b1) = (b1, b);
                            } 

                            // Replace the comment with a space in the returned Directive
                            // body, so that markup parts that were separated by the comment
                            // (like a "<" and a "!") don't get joined when re-encoding the
                            // Directive, taking new semantic meaning.


                            // Replace the comment with a space in the returned Directive
                            // body, so that markup parts that were separated by the comment
                            // (like a "<" and a "!") don't get joined when re-encoding the
                            // Directive, taking new semantic meaning.
                            d.buf.WriteByte(' ');
                                }
                    return (Directive(d.buf.Bytes()), error.As(null!)!);
            break;
    } 

    // Must be an open element like <a href="foo">
    d.ungetc(b);

    name = default;    bool empty = default;    slice<Attr> attr = default;
    name, ok = d.nsname();

    if (!ok) {
        if (d.err == null) {
            d.err = d.syntaxError("expected element name after <");
        }
        return (null, error.As(d.err)!);
    }
    attr = new slice<Attr>(new Attr[] {  });
    while (true) {
        d.space();
        b, ok = d.mustgetc();

        if (!ok) {
            return (null, error.As(d.err)!);
        }
        if (b == '/') {
            empty = true;
            b, ok = d.mustgetc();

            if (!ok) {
                return (null, error.As(d.err)!);
            }
            if (b != '>') {
                d.err = d.syntaxError("expected /> in element");
                return (null, error.As(d.err)!);
            }
            break;
        }
        if (b == '>') {
            break;
        }
        d.ungetc(b);

        Attr a = new Attr();
        a.Name, ok = d.nsname();

        if (!ok) {
            if (d.err == null) {
                d.err = d.syntaxError("expected attribute name in element");
            }
            return (null, error.As(d.err)!);
        }
        d.space();
        b, ok = d.mustgetc();

        if (!ok) {
            return (null, error.As(d.err)!);
        }
        if (b != '=') {
            if (d.Strict) {
                d.err = d.syntaxError("attribute name without = in element");
                return (null, error.As(d.err)!);
            }
            d.ungetc(b);
            a.Value = a.Name.Local;
        }
        else
 {
            d.space();
            data = d.attrval();
            if (data == null) {
                return (null, error.As(d.err)!);
            }
            a.Value = string(data);
        }
        attr = append(attr, a);
    }
    if (empty) {
        d.needClose = true;
        d.toClose = name;
    }
    return (new StartElement(name,attr), error.As(null!)!);
});

private static slice<byte> attrval(this ptr<Decoder> _addr_d) {
    ref Decoder d = ref _addr_d.val;

    var (b, ok) = d.mustgetc();
    if (!ok) {
        return null;
    }
    if (b == '"' || b == '\'') {
        return d.text(int(b), false);
    }
    if (d.Strict) {
        d.err = d.syntaxError("unquoted or missing attribute value in element");
        return null;
    }
    d.ungetc(b);
    d.buf.Reset();
    while (true) {
        b, ok = d.mustgetc();
        if (!ok) {
            return null;
        }
        if ('a' <= b && b <= 'z' || 'A' <= b && b <= 'Z' || '0' <= b && b <= '9' || b == '_' || b == ':' || b == '-') {
            d.buf.WriteByte(b);
        }
        else
 {
            d.ungetc(b);
            break;
        }
    }
    return d.buf.Bytes();
}

// Skip spaces if any
private static void space(this ptr<Decoder> _addr_d) {
    ref Decoder d = ref _addr_d.val;

    while (true) {
        var (b, ok) = d.getc();
        if (!ok) {
            return ;
        }
        switch (b) {
            case ' ': 

            case '\r': 

            case '\n': 

            case '\t': 

                break;
            default: 
                d.ungetc(b);
                return ;
                break;
        }
    }
}

// Read a single byte.
// If there is no byte to read, return ok==false
// and leave the error in d.err.
// Maintain line number.
private static (byte, bool) getc(this ptr<Decoder> _addr_d) {
    byte b = default;
    bool ok = default;
    ref Decoder d = ref _addr_d.val;

    if (d.err != null) {
        return (0, false);
    }
    if (d.nextByte >= 0) {
        b = byte(d.nextByte);
        d.nextByte = -1;
    }
    else
 {
        b, d.err = d.r.ReadByte();
        if (d.err != null) {
            return (0, false);
        }
        if (d.saved != null) {
            d.saved.WriteByte(b);
        }
    }
    if (b == '\n') {
        d.line++;
    }
    d.offset++;
    return (b, true);
}

// InputOffset returns the input stream byte offset of the current decoder position.
// The offset gives the location of the end of the most recently returned token
// and the beginning of the next token.
private static long InputOffset(this ptr<Decoder> _addr_d) {
    ref Decoder d = ref _addr_d.val;

    return d.offset;
}

// Return saved offset.
// If we did ungetc (nextByte >= 0), have to back up one.
private static nint savedOffset(this ptr<Decoder> _addr_d) {
    ref Decoder d = ref _addr_d.val;

    var n = d.saved.Len();
    if (d.nextByte >= 0) {
        n--;
    }
    return n;
}

// Must read a single byte.
// If there is no byte to read,
// set d.err to SyntaxError("unexpected EOF")
// and return ok==false
private static (byte, bool) mustgetc(this ptr<Decoder> _addr_d) {
    byte b = default;
    bool ok = default;
    ref Decoder d = ref _addr_d.val;

    b, ok = d.getc();

    if (!ok) {
        if (d.err == io.EOF) {
            d.err = d.syntaxError("unexpected EOF");
        }
    }
    return ;
}

// Unread a single byte.
private static void ungetc(this ptr<Decoder> _addr_d, byte b) {
    ref Decoder d = ref _addr_d.val;

    if (b == '\n') {
        d.line--;
    }
    d.nextByte = int(b);
    d.offset--;
}

private static map entity = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, int>{"lt":'<',"gt":'>',"amp":'&',"apos":'\'',"quot":'"',};

// Read plain text section (XML calls it character data).
// If quote >= 0, we are in a quoted string and need to find the matching quote.
// If cdata == true, we are in a <![CDATA[ section and need to find ]]>.
// On failure return nil and leave the error in d.err.
private static slice<byte> text(this ptr<Decoder> _addr_d, nint quote, bool cdata) {
    ref Decoder d = ref _addr_d.val;

    byte b0 = default;    byte b1 = default;

    nint trunc = default;
    d.buf.Reset();
Input:
    while (true) {
        var (b, ok) = d.getc();
        if (!ok) {
            if (cdata) {
                if (d.err == io.EOF) {
                    d.err = d.syntaxError("unexpected EOF in CDATA section");
                }
                return null;
            }
            _breakInput = true;
            break;
        }
        if (b0 == ']' && b1 == ']' && b == '>') {
            if (cdata) {
                trunc = 2;
                _breakInput = true;
                break;
            }
            d.err = d.syntaxError("unescaped ]]> not in CDATA section");
            return null;
        }
        if (b == '<' && !cdata) {
            if (quote >= 0) {
                d.err = d.syntaxError("unescaped < inside quoted string");
                return null;
            }
            d.ungetc('<');
            _breakInput = true;
            break;
        }
        if (quote >= 0 && b == byte(quote)) {
            _breakInput = true;
            break;
        }
        if (b == '&' && !cdata) { 
            // Read escaped character expression up to semicolon.
            // XML in all its glory allows a document to define and use
            // its own character names with <!ENTITY ...> directives.
            // Parsers are required to recognize lt, gt, amp, apos, and quot
            // even if they have not been declared.
            var before = d.buf.Len();
            d.buf.WriteByte('&');
            bool ok = default;
            @string text = default;
            bool haveText = default;
            b, ok = d.mustgetc();

            if (!ok) {
                return null;
            }
            if (b == '#') {
                d.buf.WriteByte(b);
                b, ok = d.mustgetc();

                if (!ok) {
                    return null;
                }
                nint @base = 10;
                if (b == 'x') {
                    base = 16;
                    d.buf.WriteByte(b);
                    b, ok = d.mustgetc();

                    if (!ok) {
                        return null;
                    }
                }
                var start = d.buf.Len();
                while ('0' <= b && b <= '9' || base == 16 && 'a' <= b && b <= 'f' || base == 16 && 'A' <= b && b <= 'F') {
                    d.buf.WriteByte(b);
                    b, ok = d.mustgetc();

                    if (!ok) {
                        return null;
                    }
                }
            else

                if (b != ';') {
                    d.ungetc(b);
                }
                else
 {
                    var s = string(d.buf.Bytes()[(int)start..]);
                    d.buf.WriteByte(';');
                    var (n, err) = strconv.ParseUint(s, base, 64);
                    if (err == null && n <= unicode.MaxRune) {
                        text = string(rune(n));
                        haveText = true;
                    }
                }
            } {
                d.ungetc(b);
                if (!d.readName()) {
                    if (d.err != null) {
                        return null;
                    }
                }
                b, ok = d.mustgetc();

                if (!ok) {
                    return null;
                }
                if (b != ';') {
                    d.ungetc(b);
                }
                else
 {
                    var name = d.buf.Bytes()[(int)before + 1..];
                    d.buf.WriteByte(';');
                    if (isName(name)) {
                        s = string(name);
                        {
                            var r__prev5 = r;

                            var (r, ok) = entity[s];

                            if (ok) {
                                text = string(r);
                                haveText = true;
                            }
                            else if (d.Entity != null) {
                                text, haveText = d.Entity[s];
                            }

                            r = r__prev5;

                        }
                    }
                }
            }
            if (haveText) {
                d.buf.Truncate(before);
                d.buf.Write((slice<byte>)text);
                (b0, b1) = (0, 0);                _continueInput = true;
                break;
            }
            if (!d.Strict) {
                (b0, b1) = (0, 0);                _continueInput = true;
                break;
            }
            var ent = string(d.buf.Bytes()[(int)before..]);
            if (ent[len(ent) - 1] != ';') {
                ent += " (no semicolon)";
            }
            d.err = d.syntaxError("invalid character entity " + ent);
            return null;
        }
        if (b == '\r') {
            d.buf.WriteByte('\n');
        }
        else if (b1 == '\r' && b == '\n') { 
            // Skip \r\n--we already wrote \n.
        }
        else
 {
            d.buf.WriteByte(b);
        }
        (b0, b1) = (b1, b);
    }
    var data = d.buf.Bytes();
    data = data[(int)0..(int)len(data) - trunc]; 

    // Inspect each rune for being a disallowed character.
    var buf = data;
    while (len(buf) > 0) {
        var (r, size) = utf8.DecodeRune(buf);
        if (r == utf8.RuneError && size == 1) {
            d.err = d.syntaxError("invalid UTF-8");
            return null;
        }
        buf = buf[(int)size..];
        if (!isInCharacterRange(r)) {
            d.err = d.syntaxError(fmt.Sprintf("illegal character code %U", r));
            return null;
        }
    }

    return data;
}

// Decide whether the given rune is in the XML Character Range, per
// the Char production of https://www.xml.com/axml/testaxml.htm,
// Section 2.2 Characters.
private static bool isInCharacterRange(int r) {
    bool inrange = default;

    return r == 0x09 || r == 0x0A || r == 0x0D || r >= 0x20 && r <= 0xD7FF || r >= 0xE000 && r <= 0xFFFD || r >= 0x10000 && r <= 0x10FFFF;
}

// Get name space name: name with a : stuck in the middle.
// The part before the : is the name space identifier.
private static (Name, bool) nsname(this ptr<Decoder> _addr_d) {
    Name name = default;
    bool ok = default;
    ref Decoder d = ref _addr_d.val;

    var (s, ok) = d.name();
    if (!ok) {
        return ;
    }
    if (strings.Count(s, ":") > 1) {
        name.Local = s;
    }    {
        var i = strings.Index(s, ":");


        else if (i < 1 || i > len(s) - 2) {
            name.Local = s;
        }
        else
 {
            name.Space = s[(int)0..(int)i];
            name.Local = s[(int)i + 1..];
        }
    }
    return (name, true);
}

// Get name: /first(first|second)*/
// Do not set d.err if the name is missing (unless unexpected EOF is received):
// let the caller provide better context.
private static (@string, bool) name(this ptr<Decoder> _addr_d) {
    @string s = default;
    bool ok = default;
    ref Decoder d = ref _addr_d.val;

    d.buf.Reset();
    if (!d.readName()) {
        return ("", false);
    }
    var b = d.buf.Bytes();
    if (!isName(b)) {
        d.err = d.syntaxError("invalid XML name: " + string(b));
        return ("", false);
    }
    return (string(b), true);
}

// Read a name and append its bytes to d.buf.
// The name is delimited by any single-byte character not valid in names.
// All multi-byte characters are accepted; the caller must check their validity.
private static bool readName(this ptr<Decoder> _addr_d) {
    bool ok = default;
    ref Decoder d = ref _addr_d.val;

    byte b = default;
    b, ok = d.mustgetc();

    if (!ok) {
        return ;
    }
    if (b < utf8.RuneSelf && !isNameByte(b)) {
        d.ungetc(b);
        return false;
    }
    d.buf.WriteByte(b);

    while (true) {
        b, ok = d.mustgetc();

        if (!ok) {
            return ;
        }
        if (b < utf8.RuneSelf && !isNameByte(b)) {
            d.ungetc(b);
            break;
        }
        d.buf.WriteByte(b);
    }
    return true;
}

private static bool isNameByte(byte c) {
    return 'A' <= c && c <= 'Z' || 'a' <= c && c <= 'z' || '0' <= c && c <= '9' || c == '_' || c == ':' || c == '.' || c == '-';
}

private static bool isName(slice<byte> s) {
    if (len(s) == 0) {
        return false;
    }
    var (c, n) = utf8.DecodeRune(s);
    if (c == utf8.RuneError && n == 1) {
        return false;
    }
    if (!unicode.Is(first, c)) {
        return false;
    }
    while (n < len(s)) {
        s = s[(int)n..];
        c, n = utf8.DecodeRune(s);
        if (c == utf8.RuneError && n == 1) {
            return false;
        }
        if (!unicode.Is(first, c) && !unicode.Is(second, c)) {
            return false;
        }
    }
    return true;
}

private static bool isNameString(@string s) {
    if (len(s) == 0) {
        return false;
    }
    var (c, n) = utf8.DecodeRuneInString(s);
    if (c == utf8.RuneError && n == 1) {
        return false;
    }
    if (!unicode.Is(first, c)) {
        return false;
    }
    while (n < len(s)) {
        s = s[(int)n..];
        c, n = utf8.DecodeRuneInString(s);
        if (c == utf8.RuneError && n == 1) {
            return false;
        }
        if (!unicode.Is(first, c) && !unicode.Is(second, c)) {
            return false;
        }
    }
    return true;
}

// These tables were generated by cut and paste from Appendix B of
// the XML spec at https://www.xml.com/axml/testaxml.htm
// and then reformatting. First corresponds to (Letter | '_' | ':')
// and second corresponds to NameChar.

private static ptr<unicode.RangeTable> first = addr(new unicode.RangeTable(R16:[]unicode.Range16{{0x003A,0x003A,1},{0x0041,0x005A,1},{0x005F,0x005F,1},{0x0061,0x007A,1},{0x00C0,0x00D6,1},{0x00D8,0x00F6,1},{0x00F8,0x00FF,1},{0x0100,0x0131,1},{0x0134,0x013E,1},{0x0141,0x0148,1},{0x014A,0x017E,1},{0x0180,0x01C3,1},{0x01CD,0x01F0,1},{0x01F4,0x01F5,1},{0x01FA,0x0217,1},{0x0250,0x02A8,1},{0x02BB,0x02C1,1},{0x0386,0x0386,1},{0x0388,0x038A,1},{0x038C,0x038C,1},{0x038E,0x03A1,1},{0x03A3,0x03CE,1},{0x03D0,0x03D6,1},{0x03DA,0x03E0,2},{0x03E2,0x03F3,1},{0x0401,0x040C,1},{0x040E,0x044F,1},{0x0451,0x045C,1},{0x045E,0x0481,1},{0x0490,0x04C4,1},{0x04C7,0x04C8,1},{0x04CB,0x04CC,1},{0x04D0,0x04EB,1},{0x04EE,0x04F5,1},{0x04F8,0x04F9,1},{0x0531,0x0556,1},{0x0559,0x0559,1},{0x0561,0x0586,1},{0x05D0,0x05EA,1},{0x05F0,0x05F2,1},{0x0621,0x063A,1},{0x0641,0x064A,1},{0x0671,0x06B7,1},{0x06BA,0x06BE,1},{0x06C0,0x06CE,1},{0x06D0,0x06D3,1},{0x06D5,0x06D5,1},{0x06E5,0x06E6,1},{0x0905,0x0939,1},{0x093D,0x093D,1},{0x0958,0x0961,1},{0x0985,0x098C,1},{0x098F,0x0990,1},{0x0993,0x09A8,1},{0x09AA,0x09B0,1},{0x09B2,0x09B2,1},{0x09B6,0x09B9,1},{0x09DC,0x09DD,1},{0x09DF,0x09E1,1},{0x09F0,0x09F1,1},{0x0A05,0x0A0A,1},{0x0A0F,0x0A10,1},{0x0A13,0x0A28,1},{0x0A2A,0x0A30,1},{0x0A32,0x0A33,1},{0x0A35,0x0A36,1},{0x0A38,0x0A39,1},{0x0A59,0x0A5C,1},{0x0A5E,0x0A5E,1},{0x0A72,0x0A74,1},{0x0A85,0x0A8B,1},{0x0A8D,0x0A8D,1},{0x0A8F,0x0A91,1},{0x0A93,0x0AA8,1},{0x0AAA,0x0AB0,1},{0x0AB2,0x0AB3,1},{0x0AB5,0x0AB9,1},{0x0ABD,0x0AE0,0x23},{0x0B05,0x0B0C,1},{0x0B0F,0x0B10,1},{0x0B13,0x0B28,1},{0x0B2A,0x0B30,1},{0x0B32,0x0B33,1},{0x0B36,0x0B39,1},{0x0B3D,0x0B3D,1},{0x0B5C,0x0B5D,1},{0x0B5F,0x0B61,1},{0x0B85,0x0B8A,1},{0x0B8E,0x0B90,1},{0x0B92,0x0B95,1},{0x0B99,0x0B9A,1},{0x0B9C,0x0B9C,1},{0x0B9E,0x0B9F,1},{0x0BA3,0x0BA4,1},{0x0BA8,0x0BAA,1},{0x0BAE,0x0BB5,1},{0x0BB7,0x0BB9,1},{0x0C05,0x0C0C,1},{0x0C0E,0x0C10,1},{0x0C12,0x0C28,1},{0x0C2A,0x0C33,1},{0x0C35,0x0C39,1},{0x0C60,0x0C61,1},{0x0C85,0x0C8C,1},{0x0C8E,0x0C90,1},{0x0C92,0x0CA8,1},{0x0CAA,0x0CB3,1},{0x0CB5,0x0CB9,1},{0x0CDE,0x0CDE,1},{0x0CE0,0x0CE1,1},{0x0D05,0x0D0C,1},{0x0D0E,0x0D10,1},{0x0D12,0x0D28,1},{0x0D2A,0x0D39,1},{0x0D60,0x0D61,1},{0x0E01,0x0E2E,1},{0x0E30,0x0E30,1},{0x0E32,0x0E33,1},{0x0E40,0x0E45,1},{0x0E81,0x0E82,1},{0x0E84,0x0E84,1},{0x0E87,0x0E88,1},{0x0E8A,0x0E8D,3},{0x0E94,0x0E97,1},{0x0E99,0x0E9F,1},{0x0EA1,0x0EA3,1},{0x0EA5,0x0EA7,2},{0x0EAA,0x0EAB,1},{0x0EAD,0x0EAE,1},{0x0EB0,0x0EB0,1},{0x0EB2,0x0EB3,1},{0x0EBD,0x0EBD,1},{0x0EC0,0x0EC4,1},{0x0F40,0x0F47,1},{0x0F49,0x0F69,1},{0x10A0,0x10C5,1},{0x10D0,0x10F6,1},{0x1100,0x1100,1},{0x1102,0x1103,1},{0x1105,0x1107,1},{0x1109,0x1109,1},{0x110B,0x110C,1},{0x110E,0x1112,1},{0x113C,0x1140,2},{0x114C,0x1150,2},{0x1154,0x1155,1},{0x1159,0x1159,1},{0x115F,0x1161,1},{0x1163,0x1169,2},{0x116D,0x116E,1},{0x1172,0x1173,1},{0x1175,0x119E,0x119E-0x1175},{0x11A8,0x11AB,0x11AB-0x11A8},{0x11AE,0x11AF,1},{0x11B7,0x11B8,1},{0x11BA,0x11BA,1},{0x11BC,0x11C2,1},{0x11EB,0x11F0,0x11F0-0x11EB},{0x11F9,0x11F9,1},{0x1E00,0x1E9B,1},{0x1EA0,0x1EF9,1},{0x1F00,0x1F15,1},{0x1F18,0x1F1D,1},{0x1F20,0x1F45,1},{0x1F48,0x1F4D,1},{0x1F50,0x1F57,1},{0x1F59,0x1F5B,0x1F5B-0x1F59},{0x1F5D,0x1F5D,1},{0x1F5F,0x1F7D,1},{0x1F80,0x1FB4,1},{0x1FB6,0x1FBC,1},{0x1FBE,0x1FBE,1},{0x1FC2,0x1FC4,1},{0x1FC6,0x1FCC,1},{0x1FD0,0x1FD3,1},{0x1FD6,0x1FDB,1},{0x1FE0,0x1FEC,1},{0x1FF2,0x1FF4,1},{0x1FF6,0x1FFC,1},{0x2126,0x2126,1},{0x212A,0x212B,1},{0x212E,0x212E,1},{0x2180,0x2182,1},{0x3007,0x3007,1},{0x3021,0x3029,1},{0x3041,0x3094,1},{0x30A1,0x30FA,1},{0x3105,0x312C,1},{0x4E00,0x9FA5,1},{0xAC00,0xD7A3,1},},));

private static ptr<unicode.RangeTable> second = addr(new unicode.RangeTable(R16:[]unicode.Range16{{0x002D,0x002E,1},{0x0030,0x0039,1},{0x00B7,0x00B7,1},{0x02D0,0x02D1,1},{0x0300,0x0345,1},{0x0360,0x0361,1},{0x0387,0x0387,1},{0x0483,0x0486,1},{0x0591,0x05A1,1},{0x05A3,0x05B9,1},{0x05BB,0x05BD,1},{0x05BF,0x05BF,1},{0x05C1,0x05C2,1},{0x05C4,0x0640,0x0640-0x05C4},{0x064B,0x0652,1},{0x0660,0x0669,1},{0x0670,0x0670,1},{0x06D6,0x06DC,1},{0x06DD,0x06DF,1},{0x06E0,0x06E4,1},{0x06E7,0x06E8,1},{0x06EA,0x06ED,1},{0x06F0,0x06F9,1},{0x0901,0x0903,1},{0x093C,0x093C,1},{0x093E,0x094C,1},{0x094D,0x094D,1},{0x0951,0x0954,1},{0x0962,0x0963,1},{0x0966,0x096F,1},{0x0981,0x0983,1},{0x09BC,0x09BC,1},{0x09BE,0x09BF,1},{0x09C0,0x09C4,1},{0x09C7,0x09C8,1},{0x09CB,0x09CD,1},{0x09D7,0x09D7,1},{0x09E2,0x09E3,1},{0x09E6,0x09EF,1},{0x0A02,0x0A3C,0x3A},{0x0A3E,0x0A3F,1},{0x0A40,0x0A42,1},{0x0A47,0x0A48,1},{0x0A4B,0x0A4D,1},{0x0A66,0x0A6F,1},{0x0A70,0x0A71,1},{0x0A81,0x0A83,1},{0x0ABC,0x0ABC,1},{0x0ABE,0x0AC5,1},{0x0AC7,0x0AC9,1},{0x0ACB,0x0ACD,1},{0x0AE6,0x0AEF,1},{0x0B01,0x0B03,1},{0x0B3C,0x0B3C,1},{0x0B3E,0x0B43,1},{0x0B47,0x0B48,1},{0x0B4B,0x0B4D,1},{0x0B56,0x0B57,1},{0x0B66,0x0B6F,1},{0x0B82,0x0B83,1},{0x0BBE,0x0BC2,1},{0x0BC6,0x0BC8,1},{0x0BCA,0x0BCD,1},{0x0BD7,0x0BD7,1},{0x0BE7,0x0BEF,1},{0x0C01,0x0C03,1},{0x0C3E,0x0C44,1},{0x0C46,0x0C48,1},{0x0C4A,0x0C4D,1},{0x0C55,0x0C56,1},{0x0C66,0x0C6F,1},{0x0C82,0x0C83,1},{0x0CBE,0x0CC4,1},{0x0CC6,0x0CC8,1},{0x0CCA,0x0CCD,1},{0x0CD5,0x0CD6,1},{0x0CE6,0x0CEF,1},{0x0D02,0x0D03,1},{0x0D3E,0x0D43,1},{0x0D46,0x0D48,1},{0x0D4A,0x0D4D,1},{0x0D57,0x0D57,1},{0x0D66,0x0D6F,1},{0x0E31,0x0E31,1},{0x0E34,0x0E3A,1},{0x0E46,0x0E46,1},{0x0E47,0x0E4E,1},{0x0E50,0x0E59,1},{0x0EB1,0x0EB1,1},{0x0EB4,0x0EB9,1},{0x0EBB,0x0EBC,1},{0x0EC6,0x0EC6,1},{0x0EC8,0x0ECD,1},{0x0ED0,0x0ED9,1},{0x0F18,0x0F19,1},{0x0F20,0x0F29,1},{0x0F35,0x0F39,2},{0x0F3E,0x0F3F,1},{0x0F71,0x0F84,1},{0x0F86,0x0F8B,1},{0x0F90,0x0F95,1},{0x0F97,0x0F97,1},{0x0F99,0x0FAD,1},{0x0FB1,0x0FB7,1},{0x0FB9,0x0FB9,1},{0x20D0,0x20DC,1},{0x20E1,0x3005,0x3005-0x20E1},{0x302A,0x302F,1},{0x3031,0x3035,1},{0x3099,0x309A,1},{0x309D,0x309E,1},{0x30FC,0x30FE,1},},));

// HTMLEntity is an entity map containing translations for the
// standard HTML entity characters.
//
// See the Decoder.Strict and Decoder.Entity fields' documentation.
public static map<@string, @string> HTMLEntity = htmlEntity;

private static map htmlEntity = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, @string>{"nbsp":"\u00A0","iexcl":"\u00A1","cent":"\u00A2","pound":"\u00A3","curren":"\u00A4","yen":"\u00A5","brvbar":"\u00A6","sect":"\u00A7","uml":"\u00A8","copy":"\u00A9","ordf":"\u00AA","laquo":"\u00AB","not":"\u00AC","shy":"\u00AD","reg":"\u00AE","macr":"\u00AF","deg":"\u00B0","plusmn":"\u00B1","sup2":"\u00B2","sup3":"\u00B3","acute":"\u00B4","micro":"\u00B5","para":"\u00B6","middot":"\u00B7","cedil":"\u00B8","sup1":"\u00B9","ordm":"\u00BA","raquo":"\u00BB","frac14":"\u00BC","frac12":"\u00BD","frac34":"\u00BE","iquest":"\u00BF","Agrave":"\u00C0","Aacute":"\u00C1","Acirc":"\u00C2","Atilde":"\u00C3","Auml":"\u00C4","Aring":"\u00C5","AElig":"\u00C6","Ccedil":"\u00C7","Egrave":"\u00C8","Eacute":"\u00C9","Ecirc":"\u00CA","Euml":"\u00CB","Igrave":"\u00CC","Iacute":"\u00CD","Icirc":"\u00CE","Iuml":"\u00CF","ETH":"\u00D0","Ntilde":"\u00D1","Ograve":"\u00D2","Oacute":"\u00D3","Ocirc":"\u00D4","Otilde":"\u00D5","Ouml":"\u00D6","times":"\u00D7","Oslash":"\u00D8","Ugrave":"\u00D9","Uacute":"\u00DA","Ucirc":"\u00DB","Uuml":"\u00DC","Yacute":"\u00DD","THORN":"\u00DE","szlig":"\u00DF","agrave":"\u00E0","aacute":"\u00E1","acirc":"\u00E2","atilde":"\u00E3","auml":"\u00E4","aring":"\u00E5","aelig":"\u00E6","ccedil":"\u00E7","egrave":"\u00E8","eacute":"\u00E9","ecirc":"\u00EA","euml":"\u00EB","igrave":"\u00EC","iacute":"\u00ED","icirc":"\u00EE","iuml":"\u00EF","eth":"\u00F0","ntilde":"\u00F1","ograve":"\u00F2","oacute":"\u00F3","ocirc":"\u00F4","otilde":"\u00F5","ouml":"\u00F6","divide":"\u00F7","oslash":"\u00F8","ugrave":"\u00F9","uacute":"\u00FA","ucirc":"\u00FB","uuml":"\u00FC","yacute":"\u00FD","thorn":"\u00FE","yuml":"\u00FF","fnof":"\u0192","Alpha":"\u0391","Beta":"\u0392","Gamma":"\u0393","Delta":"\u0394","Epsilon":"\u0395","Zeta":"\u0396","Eta":"\u0397","Theta":"\u0398","Iota":"\u0399","Kappa":"\u039A","Lambda":"\u039B","Mu":"\u039C","Nu":"\u039D","Xi":"\u039E","Omicron":"\u039F","Pi":"\u03A0","Rho":"\u03A1","Sigma":"\u03A3","Tau":"\u03A4","Upsilon":"\u03A5","Phi":"\u03A6","Chi":"\u03A7","Psi":"\u03A8","Omega":"\u03A9","alpha":"\u03B1","beta":"\u03B2","gamma":"\u03B3","delta":"\u03B4","epsilon":"\u03B5","zeta":"\u03B6","eta":"\u03B7","theta":"\u03B8","iota":"\u03B9","kappa":"\u03BA","lambda":"\u03BB","mu":"\u03BC","nu":"\u03BD","xi":"\u03BE","omicron":"\u03BF","pi":"\u03C0","rho":"\u03C1","sigmaf":"\u03C2","sigma":"\u03C3","tau":"\u03C4","upsilon":"\u03C5","phi":"\u03C6","chi":"\u03C7","psi":"\u03C8","omega":"\u03C9","thetasym":"\u03D1","upsih":"\u03D2","piv":"\u03D6","bull":"\u2022","hellip":"\u2026","prime":"\u2032","Prime":"\u2033","oline":"\u203E","frasl":"\u2044","weierp":"\u2118","image":"\u2111","real":"\u211C","trade":"\u2122","alefsym":"\u2135","larr":"\u2190","uarr":"\u2191","rarr":"\u2192","darr":"\u2193","harr":"\u2194","crarr":"\u21B5","lArr":"\u21D0","uArr":"\u21D1","rArr":"\u21D2","dArr":"\u21D3","hArr":"\u21D4","forall":"\u2200","part":"\u2202","exist":"\u2203","empty":"\u2205","nabla":"\u2207","isin":"\u2208","notin":"\u2209","ni":"\u220B","prod":"\u220F","sum":"\u2211","minus":"\u2212","lowast":"\u2217","radic":"\u221A","prop":"\u221D","infin":"\u221E","ang":"\u2220","and":"\u2227","or":"\u2228","cap":"\u2229","cup":"\u222A","int":"\u222B","there4":"\u2234","sim":"\u223C","cong":"\u2245","asymp":"\u2248","ne":"\u2260","equiv":"\u2261","le":"\u2264","ge":"\u2265","sub":"\u2282","sup":"\u2283","nsub":"\u2284","sube":"\u2286","supe":"\u2287","oplus":"\u2295","otimes":"\u2297","perp":"\u22A5","sdot":"\u22C5","lceil":"\u2308","rceil":"\u2309","lfloor":"\u230A","rfloor":"\u230B","lang":"\u2329","rang":"\u232A","loz":"\u25CA","spades":"\u2660","clubs":"\u2663","hearts":"\u2665","diams":"\u2666","quot":"\u0022","amp":"\u0026","lt":"\u003C","gt":"\u003E","OElig":"\u0152","oelig":"\u0153","Scaron":"\u0160","scaron":"\u0161","Yuml":"\u0178","circ":"\u02C6","tilde":"\u02DC","ensp":"\u2002","emsp":"\u2003","thinsp":"\u2009","zwnj":"\u200C","zwj":"\u200D","lrm":"\u200E","rlm":"\u200F","ndash":"\u2013","mdash":"\u2014","lsquo":"\u2018","rsquo":"\u2019","sbquo":"\u201A","ldquo":"\u201C","rdquo":"\u201D","bdquo":"\u201E","dagger":"\u2020","Dagger":"\u2021","permil":"\u2030","lsaquo":"\u2039","rsaquo":"\u203A","euro":"\u20AC",};

// HTMLAutoClose is the set of HTML elements that
// should be considered to close automatically.
//
// See the Decoder.Strict and Decoder.Entity fields' documentation.
public static slice<@string> HTMLAutoClose = htmlAutoClose;

private static @string htmlAutoClose = new slice<@string>(new @string[] { "basefont", "br", "area", "link", "img", "param", "hr", "input", "col", "frame", "isindex", "base", "meta" });

private static slice<byte> escQuot = (slice<byte>)"&#34;";private static slice<byte> escApos = (slice<byte>)"&#39;";private static slice<byte> escAmp = (slice<byte>)"&amp;";private static slice<byte> escLT = (slice<byte>)"&lt;";private static slice<byte> escGT = (slice<byte>)"&gt;";private static slice<byte> escTab = (slice<byte>)"&#x9;";private static slice<byte> escNL = (slice<byte>)"&#xA;";private static slice<byte> escCR = (slice<byte>)"&#xD;";private static slice<byte> escFFFD = (slice<byte>)"\uFFFD";

// EscapeText writes to w the properly escaped XML equivalent
// of the plain text data s.
public static error EscapeText(io.Writer w, slice<byte> s) {
    return error.As(escapeText(w, s, true))!;
}

// escapeText writes to w the properly escaped XML equivalent
// of the plain text data s. If escapeNewline is true, newline
// characters will be escaped.
private static error escapeText(io.Writer w, slice<byte> s, bool escapeNewline) {
    slice<byte> esc = default;
    nint last = 0;
    {
        nint i = 0;

        while (i < len(s)) {
            var (r, width) = utf8.DecodeRune(s[(int)i..]);
            i += width;
            switch (r) {
                case '"': 
                    esc = escQuot;
                    break;
                case '\'': 
                    esc = escApos;
                    break;
                case '&': 
                    esc = escAmp;
                    break;
                case '<': 
                    esc = escLT;
                    break;
                case '>': 
                    esc = escGT;
                    break;
                case '\t': 
                    esc = escTab;
                    break;
                case '\n': 
                    if (!escapeNewline) {
                        continue;
                    }
                    esc = escNL;
                    break;
                case '\r': 
                    esc = escCR;
                    break;
                default: 
                    if (!isInCharacterRange(r) || (r == 0xFFFD && width == 1)) {
                        esc = escFFFD;
                        break;
                    }
                    continue;
                    break;
            }
            {
                var (_, err) = w.Write(s[(int)last..(int)i - width]);

                if (err != null) {
                    return error.As(err)!;
                }

            }
            {
                (_, err) = w.Write(esc);

                if (err != null) {
                    return error.As(err)!;
                }

            }
            last = i;
        }
    }
    (_, err) = w.Write(s[(int)last..]);
    return error.As(err)!;
}

// EscapeString writes to p the properly escaped XML equivalent
// of the plain text data s.
private static void EscapeString(this ptr<printer> _addr_p, @string s) {
    ref printer p = ref _addr_p.val;

    slice<byte> esc = default;
    nint last = 0;
    {
        nint i = 0;

        while (i < len(s)) {
            var (r, width) = utf8.DecodeRuneInString(s[(int)i..]);
            i += width;
            switch (r) {
                case '"': 
                    esc = escQuot;
                    break;
                case '\'': 
                    esc = escApos;
                    break;
                case '&': 
                    esc = escAmp;
                    break;
                case '<': 
                    esc = escLT;
                    break;
                case '>': 
                    esc = escGT;
                    break;
                case '\t': 
                    esc = escTab;
                    break;
                case '\n': 
                    esc = escNL;
                    break;
                case '\r': 
                    esc = escCR;
                    break;
                default: 
                    if (!isInCharacterRange(r) || (r == 0xFFFD && width == 1)) {
                        esc = escFFFD;
                        break;
                    }
                    continue;
                    break;
            }
            p.WriteString(s[(int)last..(int)i - width]);
            p.Write(esc);
            last = i;
        }
    }
    p.WriteString(s[(int)last..]);
}

// Escape is like EscapeText but omits the error return value.
// It is provided for backwards compatibility with Go 1.0.
// Code targeting Go 1.1 or later should use EscapeText.
public static void Escape(io.Writer w, slice<byte> s) {
    EscapeText(w, s);
}

private static slice<byte> cdataStart = (slice<byte>)"<![CDATA[";private static slice<byte> cdataEnd = (slice<byte>)"]]>";private static slice<byte> cdataEscape = (slice<byte>)"]]]]><![CDATA[>";

// emitCDATA writes to w the CDATA-wrapped plain text data s.
// It escapes CDATA directives nested in s.
private static error emitCDATA(io.Writer w, slice<byte> s) {
    if (len(s) == 0) {
        return error.As(null!)!;
    }
    {
        var (_, err) = w.Write(cdataStart);

        if (err != null) {
            return error.As(err)!;
        }
    }
    while (true) {
        var i = bytes.Index(s, cdataEnd);
        if (i >= 0 && i + len(cdataEnd) <= len(s)) { 
            // Found a nested CDATA directive end.
            {
                (_, err) = w.Write(s[..(int)i]);

                if (err != null) {
                    return error.As(err)!;
                }

            }
            {
                (_, err) = w.Write(cdataEscape);

                if (err != null) {
                    return error.As(err)!;
                }

            }
            i += len(cdataEnd);
        }
        else
 {
            {
                (_, err) = w.Write(s);

                if (err != null) {
                    return error.As(err)!;
                }

            }
            break;
        }
        s = s[(int)i..];
    }
    (_, err) = w.Write(cdataEnd);
    return error.As(err)!;
}

// procInst parses the `param="..."` or `param='...'`
// value out of the provided string, returning "" if not found.
private static @string procInst(@string param, @string s) { 
    // TODO: this parsing is somewhat lame and not exact.
    // It works for all actual cases, though.
    param = param + "=";
    var idx = strings.Index(s, param);
    if (idx == -1) {
        return "";
    }
    var v = s[(int)idx + len(param)..];
    if (v == "") {
        return "";
    }
    if (v[0] != '\'' && v[0] != '"') {
        return "";
    }
    idx = strings.IndexRune(v[(int)1..], rune(v[0]));
    if (idx == -1) {
        return "";
    }
    return v[(int)1..(int)idx + 1];
}

} // end xml_package
