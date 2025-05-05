// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package xml implements a simple XML 1.0 parser that
// understands XML name spaces.
namespace go.encoding;

// References:
//    Annotated XML spec: https://www.xml.com/axml/testaxml.htm
//    XML name spaces: https://www.w3.org/TR/REC-xml-names/
using bufio = bufio_package;
using bytes = bytes_package;
using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using strconv = strconv_package;
using strings = strings_package;
using unicode = unicode_package;
using utf8 = unicode.utf8_package;
using unicode;

partial class xml_package {

// A SyntaxError represents a syntax error in the XML input stream.
[GoType] partial struct SyntaxError {
    public @string Msg;
    public nint Line;
}

[GoRecv] public static @string Error(this ref SyntaxError e) {
    return "XML syntax error on line "u8 + strconv.Itoa(e.Line) + ": "u8 + e.Msg;
}

// A Name represents an XML name (Local) annotated
// with a name space identifier (Space).
// In tokens returned by [Decoder.Token], the Space identifier
// is given as a canonical URL, not the short prefix used
// in the document being parsed.
[GoType] partial struct Name {
    public @string Space;
    public @string Local;
}

// An Attr represents an attribute in an XML element (Name=Value).
[GoType] partial struct Attr {
    public Name Name;
    public @string Value;
}

[GoType("any")] partial struct ΔToken;

// A StartElement represents an XML start element.
[GoType] partial struct StartElement {
    public Name Name;
    public slice<Attr> Attr;
}

// Copy creates a new copy of StartElement.
public static StartElement Copy(this StartElement e) {
    var attrs = new slice<Attr>(len(e.Attr));
    copy(attrs, e.Attr);
    e.Attr = attrs;
    return e;
}

// End returns the corresponding XML end element.
public static EndElement End(this StartElement e) {
    return new EndElement(e.Name);
}

// An EndElement represents an XML end element.
[GoType] partial struct EndElement {
    public Name Name;
}

[GoType("[]byte")] partial struct CharData;

// Copy creates a new copy of CharData.
public static CharData Copy(this CharData c) {
    return ((CharData)bytes.Clone(c));
}

[GoType("[]byte")] partial struct Comment;

// Copy creates a new copy of Comment.
public static Comment Copy(this Comment c) {
    return ((Comment)bytes.Clone(c));
}

// A ProcInst represents an XML processing instruction of the form <?target inst?>
[GoType] partial struct ProcInst {
    public @string Target;
    public slice<byte> Inst;
}

// Copy creates a new copy of ProcInst.
public static ProcInst Copy(this ProcInst p) {
    p.Inst = bytes.Clone(p.Inst);
    return p;
}

[GoType("[]byte")] partial struct Directive;

// Copy creates a new copy of Directive.
public static Directive Copy(this Directive d) {
    return ((Directive)bytes.Clone(d));
}

// CopyToken returns a copy of a Token.
public static ΔToken CopyToken(ΔToken t) {
    switch (t.type()) {
    case CharData v: {
        return v.Copy();
    }
    case Comment v: {
        return v.Copy();
    }
    case Directive v: {
        return v.Copy();
    }
    case ProcInst v: {
        return v.Copy();
    }
    case StartElement v: {
        return v.Copy();
    }}
    return t;
}

// A TokenReader is anything that can decode a stream of XML tokens, including a
// [Decoder].
//
// When Token encounters an error or end-of-file condition after successfully
// reading a token, it returns the token. It may return the (non-nil) error from
// the same call or return the error (and a nil token) from a subsequent call.
// An instance of this general case is that a TokenReader returning a non-nil
// token at the end of the token stream may return either io.EOF or a nil error.
// The next Read should return nil, [io.EOF].
//
// Implementations of Token are discouraged from returning a nil token with a
// nil error. Callers should treat a return of nil, nil as indicating that
// nothing happened; in particular it does not indicate EOF.
[GoType] partial interface TokenReader {
    (ΔToken, error) Token();
}

// A Decoder represents an XML parser reading a particular input stream.
// The parser assumes that its input is encoded in UTF-8.
[GoType] partial struct Decoder {
    // Strict defaults to true, enforcing the requirements
    // of the XML specification.
    // If set to false, the parser allows input containing common
    // mistakes:
    //	* If an element is missing an end tag, the parser invents
    //	  end tags as necessary to keep the return values from Token
    //	  properly balanced.
    //	* In attribute values and character data, unknown or malformed
    //	  character entities (sequences beginning with &) are left alone.
    //
    // Setting:
    //
    //	d.Strict = false
    //	d.AutoClose = xml.HTMLAutoClose
    //	d.Entity = xml.HTMLEntity
    //
    // creates a parser that can handle typical HTML.
    //
    // Strict mode does not enforce the requirements of the XML name spaces TR.
    // In particular it does not reject name space tags using undefined prefixes.
    // Such tags are recorded with the unknown prefix as the name space URL.
    public bool Strict;
    // When Strict == false, AutoClose indicates a set of elements to
    // consider closed immediately after they are opened, regardless
    // of whether an end element is present.
    public slice<@string> AutoClose;
    // Entity can be used to map non-standard entity names to string replacements.
    // The parser behaves as if these standard mappings are present in the map,
    // regardless of the actual map content:
    //
    //	"lt": "<",
    //	"gt": ">",
    //	"amp": "&",
    //	"apos": "'",
    //	"quot": `"`,
    public map<@string, @string> Entity;
    // CharsetReader, if non-nil, defines a function to generate
    // charset-conversion readers, converting from the provided
    // non-UTF-8 charset into UTF-8. If CharsetReader is nil or
    // returns an error, parsing stops with an error. One of the
    // CharsetReader's result values must be non-nil.
    public Func<@string, io.Reader, (io.Reader, error)> CharsetReader;
    // DefaultSpace sets the default name space used for unadorned tags,
    // as if the entire XML stream were wrapped in an element containing
    // the attribute xmlns="DefaultSpace".
    public @string DefaultSpace;
    internal io_package.ByteReader r;
    internal TokenReader t;
    internal bytes_package.Buffer buf;
    internal ж<bytes_package.Buffer> saved;
    internal ж<stack> stk;
    internal ж<stack> free;
    internal bool needClose;
    internal Name toClose;
    internal ΔToken nextToken;
    internal nint nextByte;
    internal map<@string, @string> ns;
    internal error err;
    internal nint line;
    internal int64 linestart;
    internal int64 offset;
    internal nint unmarshalDepth;
}

// NewDecoder creates a new XML parser reading from r.
// If r does not implement [io.ByteReader], NewDecoder will
// do its own buffering.
public static ж<Decoder> NewDecoder(io.Reader r) {
    var d = Ꮡ(new Decoder(
        ns: new map<@string, @string>(),
        nextByte: -1,
        line: 1,
        Strict: true
    ));
    d.switchToReader(r);
    return d;
}

// NewTokenDecoder creates a new XML parser using an underlying token stream.
public static ж<Decoder> NewTokenDecoder(TokenReader t) {
    // Is it already a Decoder?
    {
        var (dΔ1, ok) = t._<Decoder.val>(ᐧ); if (ok) {
            return dΔ1;
        }
    }
    var d = Ꮡ(new Decoder(
        ns: new map<@string, @string>(),
        t: t,
        nextByte: -1,
        line: 1,
        Strict: true
    ));
    return d;
}

// Token returns the next XML token in the input stream.
// At the end of the input stream, Token returns nil, [io.EOF].
//
// Slices of bytes in the returned token data refer to the
// parser's internal buffer and remain valid only until the next
// call to Token. To acquire a copy of the bytes, call [CopyToken]
// or the token's Copy method.
//
// Token expands self-closing elements such as <br>
// into separate start and end elements returned by successive calls.
//
// Token guarantees that the [StartElement] and [EndElement]
// tokens it returns are properly nested and matched:
// if Token encounters an unexpected end element
// or EOF before all expected end elements,
// it will return an error.
//
// If [Decoder.CharsetReader] is called and returns an error,
// the error is wrapped and returned.
//
// Token implements XML name spaces as described by
// https://www.w3.org/TR/REC-xml-names/. Each of the
// [Name] structures contained in the Token has the Space
// set to the URL identifying its name space when known.
// If Token encounters an unrecognized name space prefix,
// it uses the prefix as the Space rather than report an error.
[GoRecv] public static (ΔToken, error) Token(this ref Decoder d) {
    ΔToken t = default!;
    error err = default!;
    if (d.stk != nil && d.stk.kind == stkEOF) {
        return (default!, io.EOF);
    }
    if (d.nextToken != default!){
        t = d.nextToken;
        d.nextToken = default!;
    } else {
        {
            (t, err) = d.rawToken(); if (t == default! && err != default!) {
                if (AreEqual(err, io.EOF) && d.stk != nil && d.stk.kind != stkEOF) {
                    err = d.syntaxError("unexpected EOF"u8);
                }
                return (default!, err);
            }
        }
        // We still have a token to process, so clear any
        // errors (e.g. EOF) and proceed.
        err = default!;
    }
    if (!d.Strict) {
        {
            var (t1, ok) = d.autoClose(t); if (ok) {
                d.nextToken = t;
                t = t1;
            }
        }
    }
    switch (t.type()) {
    case StartElement t1: {
        foreach (var (_, a) in t1.Attr) {
            // In XML name spaces, the translations listed in the
            // attributes apply to the element name and
            // to the other attribute names, so process
            // the translations first.
            if (a.Name.Space == xmlnsPrefix) {
                @string v = d.ns[a.Name.Local];
                var ok = d.ns[a.Name.Local];
                d.pushNs(a.Name.Local, v, ok);
                d.ns[a.Name.Local] = a.Value;
            }
            if (a.Name.Space == ""u8 && a.Name.Local == xmlnsPrefix) {
                // Default space for untagged names
                @string v = d.ns[""u8];
                var ok = d.ns[""u8];
                d.pushNs(""u8, v, ok);
                d.ns[""u8] = a.Value;
            }
        }
        d.pushElement(t1.Name);
        d.translate(Ꮡt1.of(StartElement.ᏑName), true);
        ref var i = ref heap(new nint(), out var Ꮡi);

        foreach (var (i, _) in t1.Attr) {
            d.translate(Ꮡt1.Attr[i].of(Attr.ᏑName), false);
        }
        t = t1;
        break;
    }
    case EndElement t1: {
        if (!d.popElement(Ꮡ(t1))) {
            return (default!, d.err);
        }
        t = t1;
        break;
    }}
    return (t, err);
}

internal static readonly @string xmlURL = "http://www.w3.org/XML/1998/namespace"u8;
internal static readonly @string xmlnsPrefix = "xmlns"u8;
internal static readonly @string xmlPrefix = "xml"u8;

// Apply name space translation to name n.
// The default name space (for Space=="")
// applies only to element names, not to attribute names.
[GoRecv] public static void translate(this ref Decoder d, ж<Name> Ꮡn, bool isElementName) {
    ref var n = ref Ꮡn.val;

    switch (ᐧ) {
    case {} when n.Space == xmlnsPrefix: {
        return;
    }
    case {} when n.Space == ""u8 && !isElementName: {
        return;
    }
    case {} when n.Space == xmlPrefix: {
        n.Space = xmlURL;
        break;
    }
    case {} when n.Space == ""u8 && n.Local == xmlnsPrefix: {
        return;
    }}

    {
        @string v = d.ns[n.Space];
        var ok = d.ns[n.Space]; if (ok){
            n.Space = v;
        } else 
        if (n.Space == ""u8) {
            n.Space = d.DefaultSpace;
        }
    }
}

[GoRecv] internal static void switchToReader(this ref Decoder d, io.Reader r) {
    // Get efficient byte at a time reader.
    // Assume that if reader has its own
    // ReadByte, it's efficient enough.
    // Otherwise, use bufio.
    {
        var (rb, ok) = r._<io.ByteReader>(ᐧ); if (ok){
            d.r = rb;
        } else {
            d.r = bufio.NewReader(r);
        }
    }
}

// Parsing state - stack holds old name space translations
// and the current set of open elements. The translations to pop when
// ending a given tag are *below* it on the stack, which is
// more work but forced on us by XML.
[GoType] partial struct stack {
    internal ж<stack> next;
    internal nint kind;
    internal Name name;
    internal bool ok;
}

internal static readonly UntypedInt stkStart = iota;
internal static readonly UntypedInt stkNs = 1;
internal static readonly UntypedInt stkEOF = 2;

[GoRecv] internal static ж<stack> push(this ref Decoder d, nint kind) {
    var s = d.free;
    if (s != nil){
        d.free = s.val.next;
    } else {
        s = @new<stack>();
    }
    s.val.next = d.stk;
    s.val.kind = kind;
    d.stk = s;
    return s;
}

[GoRecv] internal static ж<stack> pop(this ref Decoder d) {
    var s = d.stk;
    if (s != nil) {
        d.stk = s.val.next;
        s.val.next = d.free;
        d.free = s;
    }
    return s;
}

// Record that after the current element is finished
// (that element is already pushed on the stack)
// Token should return EOF until popEOF is called.
[GoRecv] internal static void pushEOF(this ref Decoder d) {
    // Walk down stack to find Start.
    // It might not be the top, because there might be stkNs
    // entries above it.
    var start = d.stk;
    while ((~start).kind != stkStart) {
        start = start.val.next;
    }
    // The stkNs entries below a start are associated with that
    // element too; skip over them.
    while ((~start).next != nil && (~(~start).next).kind == stkNs) {
        start = start.val.next;
    }
    var s = d.free;
    if (s != nil){
        d.free = s.val.next;
    } else {
        s = @new<stack>();
    }
    s.val.kind = stkEOF;
    s.val.next = start.val.next;
    start.val.next = s;
}

// Undo a pushEOF.
// The element must have been finished, so the EOF should be at the top of the stack.
[GoRecv] internal static bool popEOF(this ref Decoder d) {
    if (d.stk == nil || d.stk.kind != stkEOF) {
        return false;
    }
    d.pop();
    return true;
}

// Record that we are starting an element with the given name.
[GoRecv] internal static void pushElement(this ref Decoder d, Name name) {
    var s = d.push(stkStart);
    s.val.name = name;
}

// Record that we are changing the value of ns[local].
// The old value is url, ok.
[GoRecv] internal static void pushNs(this ref Decoder d, @string local, @string url, bool ok) {
    var s = d.push(stkNs);
    (~s).name.Local = local;
    (~s).name.Space = url;
    s.val.ok = ok;
}

// Creates a SyntaxError with the current line number.
[GoRecv] internal static error syntaxError(this ref Decoder d, @string msg) {
    return new SyntaxError(Msg: msg, Line: d.line);
}

// Record that we are ending an element with the given name.
// The name must match the record at the top of the stack,
// which must be a pushElement record.
// After popping the element, apply any undo records from
// the stack to restore the name translations that existed
// before we saw this element.
[GoRecv] public static bool popElement(this ref Decoder d, ж<EndElement> Ꮡt) {
    ref var t = ref Ꮡt.val;

    var s = d.pop();
    var name = t.Name;
    switch (ᐧ) {
    case {} when s == nil || (~s).kind != stkStart: {
        d.err = d.syntaxError("unexpected end element </"u8 + name.Local + ">"u8);
        return false;
    }
    case {} when (~s).name.Local is != name.Local: {
        if (!d.Strict) {
            d.needClose = true;
            d.toClose = t.Name;
            t.Name = s.val.name;
            return true;
        }
        d.err = d.syntaxError("element <"u8 + (~s).name.Local + "> closed by </"u8 + name.Local + ">"u8);
        return false;
    }
    case {} when (~s).name.Space is != name.Space: {
        @string ns = name.Space;
        if (name.Space == ""u8) {
            ns = @""""""u8;
        }
        d.err = d.syntaxError("element <"u8 + (~s).name.Local + "> in space "u8 + (~s).name.Space + " closed by </"u8 + name.Local + "> in space "u8 + ns);
        return false;
    }}

    d.translate(Ꮡ(t.Name), true);
    // Pop stack until a Start or EOF is on the top, undoing the
    // translations that were associated with the element we just closed.
    while (d.stk != nil && d.stk.kind != stkStart && d.stk.kind != stkEOF) {
        var sΔ1 = d.pop();
        if ((~sΔ1).ok){
            d.ns[(~s).name.Local] = (~sΔ1).name.Space;
        } else {
            delete(d.ns, (~sΔ1).name.Local);
        }
    }
    return true;
}

// If the top element on the stack is autoclosing and
// t is not the end tag, invent the end tag.
[GoRecv] internal static (ΔToken, bool) autoClose(this ref Decoder d, ΔToken t) {
    if (d.stk == nil || d.stk.kind != stkStart) {
        return (default!, false);
    }
    foreach (var (_, s) in d.AutoClose) {
        if (strings.EqualFold(s, d.stk.name.Local)) {
            // This one should be auto closed if t doesn't close it.
            var (et, ok) = t._<EndElement>(ᐧ);
            if (!ok || !strings.EqualFold(et.Name.Local, d.stk.name.Local)) {
                return (new EndElement(d.stk.name), true);
            }
            break;
        }
    }
    return (default!, false);
}

internal static error errRawToken = errors.New("xml: cannot use RawToken from UnmarshalXML method"u8);

// RawToken is like [Decoder.Token] but does not verify that
// start and end elements match and does not translate
// name space prefixes to their corresponding URLs.
[GoRecv] public static (ΔToken, error) RawToken(this ref Decoder d) {
    if (d.unmarshalDepth > 0) {
        return (default!, errRawToken);
    }
    return d.rawToken();
}

[GoRecv] internal static (ΔToken, error) rawToken(this ref Decoder d) {
    if (d.t != default!) {
        return d.t.Token();
    }
    if (d.err != default!) {
        return (default!, d.err);
    }
    if (d.needClose) {
        // The last element we read was self-closing and
        // we returned just the StartElement half.
        // Return the EndElement half now.
        d.needClose = false;
        return (new EndElement(d.toClose), default!);
    }
    var (b, ok) = d.getc();
    if (!ok) {
        return (default!, d.err);
    }
    if (b != (rune)'<') {
        // Text section.
        d.ungetc(b);
        var data = d.text(-1, false);
        if (data == default!) {
            return (default!, d.err);
        }
        return (((CharData)data), default!);
    }
    {
        (b, ok) = d.mustgetc(); if (!ok) {
            return (default!, d.err);
        }
    }
    switch (b) {
    case (rune)'/': {
        // </: End element
        Name nameΔ2 = default!;
        {
            (nameΔ2, ok) = d.nsname(); if (!ok) {
                if (d.err == default!) {
                    d.err = d.syntaxError("expected element name after </"u8);
                }
                return (default!, d.err);
            }
        }
        d.space();
        {
            (b, ok) = d.mustgetc(); if (!ok) {
                return (default!, d.err);
            }
        }
        if (b != (rune)'>') {
            d.err = d.syntaxError("invalid characters between </"u8 + nameΔ2.Local + " and >"u8);
            return (default!, d.err);
        }
        return (new EndElement(nameΔ2), default!);
    }
    case (rune)'?': {
        // <?: Processing instruction.
        @string targetΔ1 = default!;
        {
            (targetΔ1, ok) = d.name(); if (!ok) {
                if (d.err == default!) {
                    d.err = d.syntaxError("expected target name after <?"u8);
                }
                return (default!, d.err);
            }
        }
        d.space();
        d.buf.Reset();
        byte b0Δ4 = default!;
        while (ᐧ) {
            {
                (b, ok) = d.mustgetc(); if (!ok) {
                    return (default!, d.err);
                }
            }
            d.buf.WriteByte(b);
            if (b0Δ4 == (rune)'?' && b == (rune)'>') {
                break;
            }
            b0 = b;
        }
        var data = d.buf.Bytes();
        data = data[0..(int)(len(data) - 2)];
        if (targetΔ1 == "xml"u8) {
            // chop ?>
            @string content = ((@string)data);
            @string ver = procInst("version"u8, content);
            if (ver != ""u8 && ver != "1.0"u8) {
                d.err = fmt.Errorf("xml: unsupported version %q; only version 1.0 is supported"u8, ver);
                return (default!, d.err);
            }
            @string enc = procInst("encoding"u8, content);
            if (enc != ""u8 && enc != "utf-8"u8 && enc != "UTF-8"u8 && !strings.EqualFold(enc, "utf-8"u8)) {
                if (d.CharsetReader == default!) {
                    d.err = fmt.Errorf("xml: encoding %q declared but Decoder.CharsetReader is nil"u8, enc);
                    return (default!, d.err);
                }
                (newr, err) = d.CharsetReader(enc, d.r._<io.Reader>());
                if (err != default!) {
                    d.err = fmt.Errorf("xml: opening charset %q: %w"u8, enc, err);
                    return (default!, d.err);
                }
                if (newr == default!) {
                    throw panic("CharsetReader returned a nil Reader for charset "u8 + enc);
                }
                d.switchToReader(newr);
            }
        }
        return (new ProcInst(targetΔ1, data), default!);
    }
    case (rune)'!': {
        {
            (b, ok) = d.mustgetc(); if (!ok) {
                // <!: Maybe comment, maybe CDATA.
                return (default!, d.err);
            }
        }
        switch (b) {
        case (rune)'-': {
            {
                (b, ok) = d.mustgetc(); if (!ok) {
                    // <!-
                    // Probably <!-- for a comment.
                    return (default!, d.err);
                }
            }
            if (b != (rune)'-') {
                d.err = d.syntaxError("invalid sequence <!- not part of <!--"u8);
                return (default!, d.err);
            }
            d.buf.Reset();
// Look for terminator.
            byte b0Δ6 = default!;
            byte b1Δ4 = default!;
            while (ᐧ) {
                {
                    (b, ok) = d.mustgetc(); if (!ok) {
                        return (default!, d.err);
                    }
                }
                d.buf.WriteByte(b);
                if (b0Δ6 == (rune)'-' && b1Δ4 == (rune)'-') {
                    if (b != (rune)'>') {
                        d.err = d.syntaxError(
                            @"invalid sequence ""--"" not allowed in comments"u8);
                        return (default!, d.err);
                    }
                    break;
                }
                (b0, b1) = (b1Δ4, b);
            }
            var data = d.buf.Bytes();
            data = data[0..(int)(len(data) - 3)];
            return (((Comment)data), default!);
        }
        case (rune)'[': {
            for (nint i = 0; i < 6; i++) {
                // chop -->
                // <![
                // Probably <![CDATA[.
                {
                    (b, ok) = d.mustgetc(); if (!ok) {
                        return (default!, d.err);
                    }
                }
                if (b != "CDATA["u8[i]) {
                    d.err = d.syntaxError("invalid <![ sequence"u8);
                    return (default!, d.err);
                }
            }
            var data = d.text(-1, // Have <![CDATA[.  Read text until ]]>.
 true);
            if (data == default!) {
                return (default!, d.err);
            }
            return (((CharData)data), default!);
        }}

        d.buf.Reset();
        d.buf.WriteByte(b);
        var inquote = ((uint8)0);
        nint depth = 0;
        while (ᐧ) {
            // Probably a directive: <!DOCTYPE ...>, <!ENTITY ...>, etc.
            // We don't care, but accumulate for caller. Quoted angle
            // brackets do not count for nesting.
            {
                (b, ok) = d.mustgetc(); if (!ok) {
                    return (default!, d.err);
                }
            }
            if (inquote == 0 && b == (rune)'>' && depth == 0) {
                break;
            }
HandleB:
            d.buf.WriteByte(b);
            switch (ᐧ) {
            case {} when b is inquote: {
                inquote = 0;
                break;
            }
            case {} when inquote is != 0: {
                break;
            }
            case {} when b == (rune)'\'' || b == (rune)'"': {
                inquote = b;
                break;
            }
            case {} when b == (rune)'>' && inquote == 0: {
                depth--;
                break;
            }
            case {} when b == (rune)'<' && inquote == 0: {
                @string s = "!--"u8;
                for (nint i = 0; i < len(s); i++) {
                    // in quotes, no special action
                    // Look for <!-- to begin comment.
                    {
                        (b, ok) = d.mustgetc(); if (!ok) {
                            return (default!, d.err);
                        }
                    }
                    if (b != s[i]) {
                        for (nint j = 0; j < i; j++) {
                            d.buf.WriteByte(s[j]);
                        }
                        depth++;
                        goto HandleB;
                    }
                }
                d.buf.Truncate(d.buf.Len() - 1);
// Remove < that was written above.

                // Look for terminator.
                byte b0 = default!;
                byte b1 = default!;
                while (ᐧ) {
                    {
                        (b, ok) = d.mustgetc(); if (!ok) {
                            return (default!, d.err);
                        }
                    }
                    if (b0 == (rune)'-' && b1 == (rune)'-' && b == (rune)'>') {
                        break;
                    }
                    (b0, b1) = (b1, b);
                }
                d.buf.WriteByte((rune)' ');
                break;
            }}

        }
        return (((Directive)d.buf.Bytes()), default!);
    }}

    // Replace the comment with a space in the returned Directive
    // body, so that markup parts that were separated by the comment
    // (like a "<" and a "!") don't get joined when re-encoding the
    // Directive, taking new semantic meaning.
    // Must be an open element like <a href="foo">
    d.ungetc(b);
    Name name = default!;
    bool empty = default!;
    slice<Attr> attr = default!;
    {
        (name, ok) = d.nsname(); if (!ok) {
            if (d.err == default!) {
                d.err = d.syntaxError("expected element name after <"u8);
            }
            return (default!, d.err);
        }
    }
    attr = new Attr[]{}.slice();
    while (ᐧ) {
        d.space();
        {
            (b, ok) = d.mustgetc(); if (!ok) {
                return (default!, d.err);
            }
        }
        if (b == (rune)'/') {
            empty = true;
            {
                (b, ok) = d.mustgetc(); if (!ok) {
                    return (default!, d.err);
                }
            }
            if (b != (rune)'>') {
                d.err = d.syntaxError("expected /> in element"u8);
                return (default!, d.err);
            }
            break;
        }
        if (b == (rune)'>') {
            break;
        }
        d.ungetc(b);
        var a = new Attr(nil);
        {
            var (a.Name, ok) = d.nsname(); if (!ok) {
                if (d.err == default!) {
                    d.err = d.syntaxError("expected attribute name in element"u8);
                }
                return (default!, d.err);
            }
        }
        d.space();
        {
            (b, ok) = d.mustgetc(); if (!ok) {
                return (default!, d.err);
            }
        }
        if (b != (rune)'='){
            if (d.Strict) {
                d.err = d.syntaxError("attribute name without = in element"u8);
                return (default!, d.err);
            }
            d.ungetc(b);
            a.Value = a.Name.Local;
        } else {
            d.space();
            var data = d.attrval();
            if (data == default!) {
                return (default!, d.err);
            }
            a.Value = ((@string)data);
        }
        attr = append(attr, a);
    }
    if (empty) {
        d.needClose = true;
        d.toClose = name;
    }
    return (new StartElement(name, attr), default!);
}

[GoRecv] internal static slice<byte> attrval(this ref Decoder d) {
    var (b, ok) = d.mustgetc();
    if (!ok) {
        return default!;
    }
    // Handle quoted attribute values
    if (b == (rune)'"' || b == (rune)'\'') {
        return d.text(((nint)b), false);
    }
    // Handle unquoted attribute values for strict parsers
    if (d.Strict) {
        d.err = d.syntaxError("unquoted or missing attribute value in element"u8);
        return default!;
    }
    // Handle unquoted attribute values for unstrict parsers
    d.ungetc(b);
    d.buf.Reset();
    while (ᐧ) {
        (b, ok) = d.mustgetc();
        if (!ok) {
            return default!;
        }
        // https://www.w3.org/TR/REC-html40/intro/sgmltut.html#h-3.2.2
        if ((rune)'a' <= b && b <= (rune)'z' || (rune)'A' <= b && b <= (rune)'Z' || (rune)'0' <= b && b <= (rune)'9' || b == (rune)'_' || b == (rune)':' || b == (rune)'-'){
            d.buf.WriteByte(b);
        } else {
            d.ungetc(b);
            break;
        }
    }
    return d.buf.Bytes();
}

// Skip spaces if any
[GoRecv] internal static void space(this ref Decoder d) {
    while (ᐧ) {
        var (b, ok) = d.getc();
        if (!ok) {
            return;
        }
        switch (b) {
        case (rune)' ' or (rune)'\r' or (rune)'\n' or (rune)'\t': {
            break;
        }
        default: {
            d.ungetc(b);
            return;
        }}

    }
}

// Read a single byte.
// If there is no byte to read, return ok==false
// and leave the error in d.err.
// Maintain line number.
[GoRecv] internal static (byte b, bool ok) getc(this ref Decoder d) {
    byte b = default!;
    bool ok = default!;

    if (d.err != default!) {
        return (0, false);
    }
    if (d.nextByte >= 0){
        b = ((byte)d.nextByte);
        d.nextByte = -1;
    } else {
        (b, d.err) = d.r.ReadByte();
        if (d.err != default!) {
            return (0, false);
        }
        if (d.saved != nil) {
            d.saved.WriteByte(b);
        }
    }
    if (b == (rune)'\n') {
        d.line++;
        d.linestart = d.offset + 1;
    }
    d.offset++;
    return (b, true);
}

// InputOffset returns the input stream byte offset of the current decoder position.
// The offset gives the location of the end of the most recently returned token
// and the beginning of the next token.
[GoRecv] public static int64 InputOffset(this ref Decoder d) {
    return d.offset;
}

// InputPos returns the line of the current decoder position and the 1 based
// input position of the line. The position gives the location of the end of the
// most recently returned token.
[GoRecv] public static (nint line, nint column) InputPos(this ref Decoder d) {
    nint line = default!;
    nint column = default!;

    return (d.line, ((nint)(d.offset - d.linestart)) + 1);
}

// Return saved offset.
// If we did ungetc (nextByte >= 0), have to back up one.
[GoRecv] internal static nint savedOffset(this ref Decoder d) {
    nint n = d.saved.Len();
    if (d.nextByte >= 0) {
        n--;
    }
    return n;
}

// Must read a single byte.
// If there is no byte to read,
// set d.err to SyntaxError("unexpected EOF")
// and return ok==false
[GoRecv] internal static (byte b, bool ok) mustgetc(this ref Decoder d) {
    byte b = default!;
    bool ok = default!;

    {
        (b, ok) = d.getc(); if (!ok) {
            if (AreEqual(d.err, io.EOF)) {
                d.err = d.syntaxError("unexpected EOF"u8);
            }
        }
    }
    return (b, ok);
}

// Unread a single byte.
[GoRecv] internal static void ungetc(this ref Decoder d, byte b) {
    if (b == (rune)'\n') {
        d.line--;
    }
    d.nextByte = ((nint)b);
    d.offset--;
}

internal static map<@string, rune> entity = new map<@string, rune>{
    ["lt"u8] = (rune)'<',
    ["gt"u8] = (rune)'>',
    ["amp"u8] = (rune)'&',
    ["apos"u8] = (rune)'\'',
    ["quot"u8] = (rune)'"'
};

// Read plain text section (XML calls it character data).
// If quote >= 0, we are in a quoted string and need to find the matching quote.
// If cdata == true, we are in a <![CDATA[ section and need to find ]]>.
// On failure return nil and leave the error in d.err.
[GoRecv] internal static slice<byte> text(this ref Decoder d, nint quote, bool cdata) {
    byte b0 = default!;
    byte b1 = default!;
    nint trunc = default!;
    d.buf.Reset();
Input:
    while (ᐧ) {
        var (b, okΔ1) = d.getc();
        if (!okΔ1) {
            if (cdata) {
                if (AreEqual(d.err, io.EOF)) {
                    d.err = d.syntaxError("unexpected EOF in CDATA section"u8);
                }
                return default!;
            }
            goto break_Input;
        }
        // <![CDATA[ section ends with ]]>.
        // It is an error for ]]> to appear in ordinary text.
        if (b0 == (rune)']' && b1 == (rune)']' && b == (rune)'>') {
            if (cdata) {
                trunc = 2;
                goto break_Input;
            }
            d.err = d.syntaxError("unescaped ]]> not in CDATA section"u8);
            return default!;
        }
        // Stop reading text if we see a <.
        if (b == (rune)'<' && !cdata) {
            if (quote >= 0) {
                d.err = d.syntaxError("unescaped < inside quoted string"u8);
                return default!;
            }
            d.ungetc((rune)'<');
            goto break_Input;
        }
        if (quote >= 0 && b == ((byte)quote)) {
            goto break_Input;
        }
        if (b == (rune)'&' && !cdata) {
            // Read escaped character expression up to semicolon.
            // XML in all its glory allows a document to define and use
            // its own character names with <!ENTITY ...> directives.
            // Parsers are required to recognize lt, gt, amp, apos, and quot
            // even if they have not been declared.
            nint before = d.buf.Len();
            d.buf.WriteByte((rune)'&');
            bool okΔ2 = default!;
            @string text = default!;
            bool haveText = default!;
            {
                (b, okΔ2) = d.mustgetc(); if (!okΔ2) {
                    return default!;
                }
            }
            if (b == (rune)'#'){
                d.buf.WriteByte(b);
                {
                    (b, okΔ2) = d.mustgetc(); if (!okΔ2) {
                        return default!;
                    }
                }
                nint @base = 10;
                if (b == (rune)'x') {
                    @base = 16;
                    d.buf.WriteByte(b);
                    {
                        (b, okΔ2) = d.mustgetc(); if (!okΔ2) {
                            return default!;
                        }
                    }
                }
                nint start = d.buf.Len();
                while ((rune)'0' <= b && b <= (rune)'9' || @base == 16 && (rune)'a' <= b && b <= (rune)'f' || @base == 16 && (rune)'A' <= b && b <= (rune)'F') {
                    d.buf.WriteByte(b);
                    {
                        (b, okΔ2) = d.mustgetc(); if (!okΔ2) {
                            return default!;
                        }
                    }
                }
                if (b != (rune)';'){
                    d.ungetc(b);
                } else {
                    @string s = ((@string)(d.buf.Bytes()[(int)(start)..]));
                    d.buf.WriteByte((rune)';');
                    var (n, err) = strconv.ParseUint(s, @base, 64);
                    if (err == default! && n <= unicode.MaxRune) {
                        text = ((@string)((rune)n));
                        haveText = true;
                    }
                }
            } else {
                d.ungetc(b);
                if (!d.readName()) {
                    if (d.err != default!) {
                        return default!;
                    }
                }
                {
                    (b, okΔ2) = d.mustgetc(); if (!okΔ2) {
                        return default!;
                    }
                }
                if (b != (rune)';'){
                    d.ungetc(b);
                } else {
                    var name = d.buf.Bytes()[(int)(before + 1)..];
                    d.buf.WriteByte((rune)';');
                    if (isName(name)) {
                        @string s = ((@string)name);
                        {
                            var (r, okΔ3) = entity[s]; if (okΔ3){
                                text = ((@string)r);
                                haveText = true;
                            } else 
                            if (d.Entity != default!) {
                                (text, haveText) = d.Entity[s];
                            }
                        }
                    }
                }
            }
            if (haveText) {
                d.buf.Truncate(before);
                d.buf.WriteString(text);
                (b0, b1) = (0, 0);
                goto continue_Input;
            }
            if (!d.Strict) {
                (b0, b1) = (0, 0);
                goto continue_Input;
            }
            @string ent = ((@string)(d.buf.Bytes()[(int)(before)..]));
            if (ent[len(ent) - 1] != (rune)';') {
                ent += " (no semicolon)"u8;
            }
            d.err = d.syntaxError("invalid character entity "u8 + ent);
            return default!;
        }
        // We must rewrite unescaped \r and \r\n into \n.
        if (b == (rune)'\r'){
            d.buf.WriteByte((rune)'\n');
        } else 
        if (b1 == (rune)'\r' && b == (rune)'\n'){
        } else {
            // Skip \r\n--we already wrote \n.
            d.buf.WriteByte(b);
        }
        (b0, b1) = (b1, b);
continue_Input:;
    }
break_Input:;
    var data = d.buf.Bytes();
    data = data[0..(int)(len(data) - trunc)];
    // Inspect each rune for being a disallowed character.
    var buf = data;
    while (len(buf) > 0) {
        var (r, size) = utf8.DecodeRune(buf);
        if (r == utf8.RuneError && size == 1) {
            d.err = d.syntaxError("invalid UTF-8"u8);
            return default!;
        }
        buf = buf[(int)(size)..];
        if (!isInCharacterRange(r)) {
            d.err = d.syntaxError(fmt.Sprintf("illegal character code %U"u8, r));
            return default!;
        }
    }
    return data;
}

// Decide whether the given rune is in the XML Character Range, per
// the Char production of https://www.xml.com/axml/testaxml.htm,
// Section 2.2 Characters.
internal static bool /*inrange*/ isInCharacterRange(rune r) {
    bool inrange = default!;

    return r == 9 || r == 10 || r == 13 || r >= 32 && r <= 55295 || r >= 57344 && r <= 65533 || r >= 65536 && r <= 1114111;
}

// Get name space name: name with a : stuck in the middle.
// The part before the : is the name space identifier.
[GoRecv] internal static (Name name, bool ok) nsname(this ref Decoder d) {
    Name name = default!;
    bool ok = default!;

    var (s, ok) = d.name();
    if (!ok) {
        return (name, ok);
    }
    if (strings.Count(s, ":"u8) > 1){
        return (name, false);
    } else 
    {
        var (space, local, okΔ1) = strings.Cut(s, ":"u8); if (!okΔ1 || space == ""u8 || local == ""u8){
            name.Local = s;
        } else {
            name.Space = space;
            name.Local = local;
        }
    }
    return (name, true);
}

// Get name: /first(first|second)*/
// Do not set d.err if the name is missing (unless unexpected EOF is received):
// let the caller provide better context.
[GoRecv] internal static (@string s, bool ok) name(this ref Decoder d) {
    @string s = default!;
    bool ok = default!;

    d.buf.Reset();
    if (!d.readName()) {
        return ("", false);
    }
    // Now we check the characters.
    var b = d.buf.Bytes();
    if (!isName(b)) {
        d.err = d.syntaxError("invalid XML name: "u8 + ((@string)b));
        return ("", false);
    }
    return (((@string)b), true);
}

// Read a name and append its bytes to d.buf.
// The name is delimited by any single-byte character not valid in names.
// All multi-byte characters are accepted; the caller must check their validity.
[GoRecv] internal static bool /*ok*/ readName(this ref Decoder d) {
    bool ok = default!;

    byte b = default!;
    {
        (b, ok) = d.mustgetc(); if (!ok) {
            return ok;
        }
    }
    if (b < utf8.RuneSelf && !isNameByte(b)) {
        d.ungetc(b);
        return false;
    }
    d.buf.WriteByte(b);
    while (ᐧ) {
        {
            (b, ok) = d.mustgetc(); if (!ok) {
                return ok;
            }
        }
        if (b < utf8.RuneSelf && !isNameByte(b)) {
            d.ungetc(b);
            break;
        }
        d.buf.WriteByte(b);
    }
    return true;
}

internal static bool isNameByte(byte c) {
    return (rune)'A' <= c && c <= (rune)'Z' || (rune)'a' <= c && c <= (rune)'z' || (rune)'0' <= c && c <= (rune)'9' || c == (rune)'_' || c == (rune)':' || c == (rune)'.' || c == (rune)'-';
}

internal static bool isName(slice<byte> s) {
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
        s = s[(int)(n)..];
        (c, n) = utf8.DecodeRune(s);
        if (c == utf8.RuneError && n == 1) {
            return false;
        }
        if (!unicode.Is(first, c) && !unicode.Is(second, c)) {
            return false;
        }
    }
    return true;
}

internal static bool isNameString(@string s) {
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
        s = s[(int)(n)..];
        (c, n) = utf8.DecodeRuneInString(s);
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
internal static ж<unicode.RangeTable> first = Ꮡ(new unicode.RangeTable(
    R16: new unicode.Range16[]{
        new(58, 58, 1),
        new(65, 90, 1),
        new(95, 95, 1),
        new(97, 122, 1),
        new(192, 214, 1),
        new(216, 246, 1),
        new(248, 255, 1),
        new(256, 305, 1),
        new(308, 318, 1),
        new(321, 328, 1),
        new(330, 382, 1),
        new(384, 451, 1),
        new(461, 496, 1),
        new(500, 501, 1),
        new(506, 535, 1),
        new(592, 680, 1),
        new(699, 705, 1),
        new(902, 902, 1),
        new(904, 906, 1),
        new(908, 908, 1),
        new(910, 929, 1),
        new(931, 974, 1),
        new(976, 982, 1),
        new(986, 992, 2),
        new(994, 1011, 1),
        new(1025, 1036, 1),
        new(1038, 1103, 1),
        new(1105, 1116, 1),
        new(1118, 1153, 1),
        new(1168, 1220, 1),
        new(1223, 1224, 1),
        new(1227, 1228, 1),
        new(1232, 1259, 1),
        new(1262, 1269, 1),
        new(1272, 1273, 1),
        new(1329, 1366, 1),
        new(1369, 1369, 1),
        new(1377, 1414, 1),
        new(1488, 1514, 1),
        new(1520, 1522, 1),
        new(1569, 1594, 1),
        new(1601, 1610, 1),
        new(1649, 1719, 1),
        new(1722, 1726, 1),
        new(1728, 1742, 1),
        new(1744, 1747, 1),
        new(1749, 1749, 1),
        new(1765, 1766, 1),
        new(2309, 2361, 1),
        new(2365, 2365, 1),
        new(2392, 2401, 1),
        new(2437, 2444, 1),
        new(2447, 2448, 1),
        new(2451, 2472, 1),
        new(2474, 2480, 1),
        new(2482, 2482, 1),
        new(2486, 2489, 1),
        new(2524, 2525, 1),
        new(2527, 2529, 1),
        new(2544, 2545, 1),
        new(2565, 2570, 1),
        new(2575, 2576, 1),
        new(2579, 2600, 1),
        new(2602, 2608, 1),
        new(2610, 2611, 1),
        new(2613, 2614, 1),
        new(2616, 2617, 1),
        new(2649, 2652, 1),
        new(2654, 2654, 1),
        new(2674, 2676, 1),
        new(2693, 2699, 1),
        new(2701, 2701, 1),
        new(2703, 2705, 1),
        new(2707, 2728, 1),
        new(2730, 2736, 1),
        new(2738, 2739, 1),
        new(2741, 2745, 1),
        new(2749, 2784, 35),
        new(2821, 2828, 1),
        new(2831, 2832, 1),
        new(2835, 2856, 1),
        new(2858, 2864, 1),
        new(2866, 2867, 1),
        new(2870, 2873, 1),
        new(2877, 2877, 1),
        new(2908, 2909, 1),
        new(2911, 2913, 1),
        new(2949, 2954, 1),
        new(2958, 2960, 1),
        new(2962, 2965, 1),
        new(2969, 2970, 1),
        new(2972, 2972, 1),
        new(2974, 2975, 1),
        new(2979, 2980, 1),
        new(2984, 2986, 1),
        new(2990, 2997, 1),
        new(2999, 3001, 1),
        new(3077, 3084, 1),
        new(3086, 3088, 1),
        new(3090, 3112, 1),
        new(3114, 3123, 1),
        new(3125, 3129, 1),
        new(3168, 3169, 1),
        new(3205, 3212, 1),
        new(3214, 3216, 1),
        new(3218, 3240, 1),
        new(3242, 3251, 1),
        new(3253, 3257, 1),
        new(3294, 3294, 1),
        new(3296, 3297, 1),
        new(3333, 3340, 1),
        new(3342, 3344, 1),
        new(3346, 3368, 1),
        new(3370, 3385, 1),
        new(3424, 3425, 1),
        new(3585, 3630, 1),
        new(3632, 3632, 1),
        new(3634, 3635, 1),
        new(3648, 3653, 1),
        new(3713, 3714, 1),
        new(3716, 3716, 1),
        new(3719, 3720, 1),
        new(3722, 3725, 3),
        new(3732, 3735, 1),
        new(3737, 3743, 1),
        new(3745, 3747, 1),
        new(3749, 3751, 2),
        new(3754, 3755, 1),
        new(3757, 3758, 1),
        new(3760, 3760, 1),
        new(3762, 3763, 1),
        new(3773, 3773, 1),
        new(3776, 3780, 1),
        new(3904, 3911, 1),
        new(3913, 3945, 1),
        new(4256, 4293, 1),
        new(4304, 4342, 1),
        new(4352, 4352, 1),
        new(4354, 4355, 1),
        new(4357, 4359, 1),
        new(4361, 4361, 1),
        new(4363, 4364, 1),
        new(4366, 4370, 1),
        new(4412, 4416, 2),
        new(4428, 4432, 2),
        new(4436, 4437, 1),
        new(4441, 4441, 1),
        new(4447, 4449, 1),
        new(4451, 4457, 2),
        new(4461, 4462, 1),
        new(4466, 4467, 1),
        new(4469, 4510, 4510 - 4469),
        new(4520, 4523, 4523 - 4520),
        new(4526, 4527, 1),
        new(4535, 4536, 1),
        new(4538, 4538, 1),
        new(4540, 4546, 1),
        new(4587, 4592, 4592 - 4587),
        new(4601, 4601, 1),
        new(7680, 7835, 1),
        new(7840, 7929, 1),
        new(7936, 7957, 1),
        new(7960, 7965, 1),
        new(7968, 8005, 1),
        new(8008, 8013, 1),
        new(8016, 8023, 1),
        new(8025, 8027, 8027 - 8025),
        new(8029, 8029, 1),
        new(8031, 8061, 1),
        new(8064, 8116, 1),
        new(8118, 8124, 1),
        new(8126, 8126, 1),
        new(8130, 8132, 1),
        new(8134, 8140, 1),
        new(8144, 8147, 1),
        new(8150, 8155, 1),
        new(8160, 8172, 1),
        new(8178, 8180, 1),
        new(8182, 8188, 1),
        new(8486, 8486, 1),
        new(8490, 8491, 1),
        new(8494, 8494, 1),
        new(8576, 8578, 1),
        new(12295, 12295, 1),
        new(12321, 12329, 1),
        new(12353, 12436, 1),
        new(12449, 12538, 1),
        new(12549, 12588, 1),
        new(19968, 40869, 1),
        new(44032, 55203, 1)
    }.slice()
));

internal static ж<unicode.RangeTable> second = Ꮡ(new unicode.RangeTable(
    R16: new unicode.Range16[]{
        new(45, 46, 1),
        new(48, 57, 1),
        new(183, 183, 1),
        new(720, 721, 1),
        new(768, 837, 1),
        new(864, 865, 1),
        new(903, 903, 1),
        new(1155, 1158, 1),
        new(1425, 1441, 1),
        new(1443, 1465, 1),
        new(1467, 1469, 1),
        new(1471, 1471, 1),
        new(1473, 1474, 1),
        new(1476, 1600, 1600 - 1476),
        new(1611, 1618, 1),
        new(1632, 1641, 1),
        new(1648, 1648, 1),
        new(1750, 1756, 1),
        new(1757, 1759, 1),
        new(1760, 1764, 1),
        new(1767, 1768, 1),
        new(1770, 1773, 1),
        new(1776, 1785, 1),
        new(2305, 2307, 1),
        new(2364, 2364, 1),
        new(2366, 2380, 1),
        new(2381, 2381, 1),
        new(2385, 2388, 1),
        new(2402, 2403, 1),
        new(2406, 2415, 1),
        new(2433, 2435, 1),
        new(2492, 2492, 1),
        new(2494, 2495, 1),
        new(2496, 2500, 1),
        new(2503, 2504, 1),
        new(2507, 2509, 1),
        new(2519, 2519, 1),
        new(2530, 2531, 1),
        new(2534, 2543, 1),
        new(2562, 2620, 58),
        new(2622, 2623, 1),
        new(2624, 2626, 1),
        new(2631, 2632, 1),
        new(2635, 2637, 1),
        new(2662, 2671, 1),
        new(2672, 2673, 1),
        new(2689, 2691, 1),
        new(2748, 2748, 1),
        new(2750, 2757, 1),
        new(2759, 2761, 1),
        new(2763, 2765, 1),
        new(2790, 2799, 1),
        new(2817, 2819, 1),
        new(2876, 2876, 1),
        new(2878, 2883, 1),
        new(2887, 2888, 1),
        new(2891, 2893, 1),
        new(2902, 2903, 1),
        new(2918, 2927, 1),
        new(2946, 2947, 1),
        new(3006, 3010, 1),
        new(3014, 3016, 1),
        new(3018, 3021, 1),
        new(3031, 3031, 1),
        new(3047, 3055, 1),
        new(3073, 3075, 1),
        new(3134, 3140, 1),
        new(3142, 3144, 1),
        new(3146, 3149, 1),
        new(3157, 3158, 1),
        new(3174, 3183, 1),
        new(3202, 3203, 1),
        new(3262, 3268, 1),
        new(3270, 3272, 1),
        new(3274, 3277, 1),
        new(3285, 3286, 1),
        new(3302, 3311, 1),
        new(3330, 3331, 1),
        new(3390, 3395, 1),
        new(3398, 3400, 1),
        new(3402, 3405, 1),
        new(3415, 3415, 1),
        new(3430, 3439, 1),
        new(3633, 3633, 1),
        new(3636, 3642, 1),
        new(3654, 3654, 1),
        new(3655, 3662, 1),
        new(3664, 3673, 1),
        new(3761, 3761, 1),
        new(3764, 3769, 1),
        new(3771, 3772, 1),
        new(3782, 3782, 1),
        new(3784, 3789, 1),
        new(3792, 3801, 1),
        new(3864, 3865, 1),
        new(3872, 3881, 1),
        new(3893, 3897, 2),
        new(3902, 3903, 1),
        new(3953, 3972, 1),
        new(3974, 3979, 1),
        new(3984, 3989, 1),
        new(3991, 3991, 1),
        new(3993, 4013, 1),
        new(4017, 4023, 1),
        new(4025, 4025, 1),
        new(8400, 8412, 1),
        new(8417, 12293, 12293 - 8417),
        new(12330, 12335, 1),
        new(12337, 12341, 1),
        new(12441, 12442, 1),
        new(12445, 12446, 1),
        new(12540, 12542, 1)
    }.slice()
));

// HTMLEntity is an entity map containing translations for the
// standard HTML entity characters.
//
// See the [Decoder.Strict] and [Decoder.Entity] fields' documentation.
public static map<@string, @string> HTMLEntity = htmlEntity;

/*
		hget http://www.w3.org/TR/html4/sgml/entities.html |
		ssam '
			,y /\&gt;/ x/\&lt;(.|\n)+/ s/\n/ /g
			,x v/^\&lt;!ENTITY/d
			,s/\&lt;!ENTITY ([^ ]+) .*U\+([0-9A-F][0-9A-F][0-9A-F][0-9A-F]) .+/	"\1": "\\u\2",/g
		'
	*/
internal static map<@string, @string> htmlEntity = new map<@string, @string>{
    ["nbsp"u8] = "\u00A0"u8,
    ["iexcl"u8] = "\u00A1"u8,
    ["cent"u8] = "\u00A2"u8,
    ["pound"u8] = "\u00A3"u8,
    ["curren"u8] = "\u00A4"u8,
    ["yen"u8] = "\u00A5"u8,
    ["brvbar"u8] = "\u00A6"u8,
    ["sect"u8] = "\u00A7"u8,
    ["uml"u8] = "\u00A8"u8,
    ["copy"u8] = "\u00A9"u8,
    ["ordf"u8] = "\u00AA"u8,
    ["laquo"u8] = "\u00AB"u8,
    ["not"u8] = "\u00AC"u8,
    ["shy"u8] = "\u00AD"u8,
    ["reg"u8] = "\u00AE"u8,
    ["macr"u8] = "\u00AF"u8,
    ["deg"u8] = "\u00B0"u8,
    ["plusmn"u8] = "\u00B1"u8,
    ["sup2"u8] = "\u00B2"u8,
    ["sup3"u8] = "\u00B3"u8,
    ["acute"u8] = "\u00B4"u8,
    ["micro"u8] = "\u00B5"u8,
    ["para"u8] = "\u00B6"u8,
    ["middot"u8] = "\u00B7"u8,
    ["cedil"u8] = "\u00B8"u8,
    ["sup1"u8] = "\u00B9"u8,
    ["ordm"u8] = "\u00BA"u8,
    ["raquo"u8] = "\u00BB"u8,
    ["frac14"u8] = "\u00BC"u8,
    ["frac12"u8] = "\u00BD"u8,
    ["frac34"u8] = "\u00BE"u8,
    ["iquest"u8] = "\u00BF"u8,
    ["Agrave"u8] = "\u00C0"u8,
    ["Aacute"u8] = "\u00C1"u8,
    ["Acirc"u8] = "\u00C2"u8,
    ["Atilde"u8] = "\u00C3"u8,
    ["Auml"u8] = "\u00C4"u8,
    ["Aring"u8] = "\u00C5"u8,
    ["AElig"u8] = "\u00C6"u8,
    ["Ccedil"u8] = "\u00C7"u8,
    ["Egrave"u8] = "\u00C8"u8,
    ["Eacute"u8] = "\u00C9"u8,
    ["Ecirc"u8] = "\u00CA"u8,
    ["Euml"u8] = "\u00CB"u8,
    ["Igrave"u8] = "\u00CC"u8,
    ["Iacute"u8] = "\u00CD"u8,
    ["Icirc"u8] = "\u00CE"u8,
    ["Iuml"u8] = "\u00CF"u8,
    ["ETH"u8] = "\u00D0"u8,
    ["Ntilde"u8] = "\u00D1"u8,
    ["Ograve"u8] = "\u00D2"u8,
    ["Oacute"u8] = "\u00D3"u8,
    ["Ocirc"u8] = "\u00D4"u8,
    ["Otilde"u8] = "\u00D5"u8,
    ["Ouml"u8] = "\u00D6"u8,
    ["times"u8] = "\u00D7"u8,
    ["Oslash"u8] = "\u00D8"u8,
    ["Ugrave"u8] = "\u00D9"u8,
    ["Uacute"u8] = "\u00DA"u8,
    ["Ucirc"u8] = "\u00DB"u8,
    ["Uuml"u8] = "\u00DC"u8,
    ["Yacute"u8] = "\u00DD"u8,
    ["THORN"u8] = "\u00DE"u8,
    ["szlig"u8] = "\u00DF"u8,
    ["agrave"u8] = "\u00E0"u8,
    ["aacute"u8] = "\u00E1"u8,
    ["acirc"u8] = "\u00E2"u8,
    ["atilde"u8] = "\u00E3"u8,
    ["auml"u8] = "\u00E4"u8,
    ["aring"u8] = "\u00E5"u8,
    ["aelig"u8] = "\u00E6"u8,
    ["ccedil"u8] = "\u00E7"u8,
    ["egrave"u8] = "\u00E8"u8,
    ["eacute"u8] = "\u00E9"u8,
    ["ecirc"u8] = "\u00EA"u8,
    ["euml"u8] = "\u00EB"u8,
    ["igrave"u8] = "\u00EC"u8,
    ["iacute"u8] = "\u00ED"u8,
    ["icirc"u8] = "\u00EE"u8,
    ["iuml"u8] = "\u00EF"u8,
    ["eth"u8] = "\u00F0"u8,
    ["ntilde"u8] = "\u00F1"u8,
    ["ograve"u8] = "\u00F2"u8,
    ["oacute"u8] = "\u00F3"u8,
    ["ocirc"u8] = "\u00F4"u8,
    ["otilde"u8] = "\u00F5"u8,
    ["ouml"u8] = "\u00F6"u8,
    ["divide"u8] = "\u00F7"u8,
    ["oslash"u8] = "\u00F8"u8,
    ["ugrave"u8] = "\u00F9"u8,
    ["uacute"u8] = "\u00FA"u8,
    ["ucirc"u8] = "\u00FB"u8,
    ["uuml"u8] = "\u00FC"u8,
    ["yacute"u8] = "\u00FD"u8,
    ["thorn"u8] = "\u00FE"u8,
    ["yuml"u8] = "\u00FF"u8,
    ["fnof"u8] = "\u0192"u8,
    ["Alpha"u8] = "\u0391"u8,
    ["Beta"u8] = "\u0392"u8,
    ["Gamma"u8] = "\u0393"u8,
    ["Delta"u8] = "\u0394"u8,
    ["Epsilon"u8] = "\u0395"u8,
    ["Zeta"u8] = "\u0396"u8,
    ["Eta"u8] = "\u0397"u8,
    ["Theta"u8] = "\u0398"u8,
    ["Iota"u8] = "\u0399"u8,
    ["Kappa"u8] = "\u039A"u8,
    ["Lambda"u8] = "\u039B"u8,
    ["Mu"u8] = "\u039C"u8,
    ["Nu"u8] = "\u039D"u8,
    ["Xi"u8] = "\u039E"u8,
    ["Omicron"u8] = "\u039F"u8,
    ["Pi"u8] = "\u03A0"u8,
    ["Rho"u8] = "\u03A1"u8,
    ["Sigma"u8] = "\u03A3"u8,
    ["Tau"u8] = "\u03A4"u8,
    ["Upsilon"u8] = "\u03A5"u8,
    ["Phi"u8] = "\u03A6"u8,
    ["Chi"u8] = "\u03A7"u8,
    ["Psi"u8] = "\u03A8"u8,
    ["Omega"u8] = "\u03A9"u8,
    ["alpha"u8] = "\u03B1"u8,
    ["beta"u8] = "\u03B2"u8,
    ["gamma"u8] = "\u03B3"u8,
    ["delta"u8] = "\u03B4"u8,
    ["epsilon"u8] = "\u03B5"u8,
    ["zeta"u8] = "\u03B6"u8,
    ["eta"u8] = "\u03B7"u8,
    ["theta"u8] = "\u03B8"u8,
    ["iota"u8] = "\u03B9"u8,
    ["kappa"u8] = "\u03BA"u8,
    ["lambda"u8] = "\u03BB"u8,
    ["mu"u8] = "\u03BC"u8,
    ["nu"u8] = "\u03BD"u8,
    ["xi"u8] = "\u03BE"u8,
    ["omicron"u8] = "\u03BF"u8,
    ["pi"u8] = "\u03C0"u8,
    ["rho"u8] = "\u03C1"u8,
    ["sigmaf"u8] = "\u03C2"u8,
    ["sigma"u8] = "\u03C3"u8,
    ["tau"u8] = "\u03C4"u8,
    ["upsilon"u8] = "\u03C5"u8,
    ["phi"u8] = "\u03C6"u8,
    ["chi"u8] = "\u03C7"u8,
    ["psi"u8] = "\u03C8"u8,
    ["omega"u8] = "\u03C9"u8,
    ["thetasym"u8] = "\u03D1"u8,
    ["upsih"u8] = "\u03D2"u8,
    ["piv"u8] = "\u03D6"u8,
    ["bull"u8] = "\u2022"u8,
    ["hellip"u8] = "\u2026"u8,
    ["prime"u8] = "\u2032"u8,
    ["Prime"u8] = "\u2033"u8,
    ["oline"u8] = "\u203E"u8,
    ["frasl"u8] = "\u2044"u8,
    ["weierp"u8] = "\u2118"u8,
    ["image"u8] = "\u2111"u8,
    ["real"u8] = "\u211C"u8,
    ["trade"u8] = "\u2122"u8,
    ["alefsym"u8] = "\u2135"u8,
    ["larr"u8] = "\u2190"u8,
    ["uarr"u8] = "\u2191"u8,
    ["rarr"u8] = "\u2192"u8,
    ["darr"u8] = "\u2193"u8,
    ["harr"u8] = "\u2194"u8,
    ["crarr"u8] = "\u21B5"u8,
    ["lArr"u8] = "\u21D0"u8,
    ["uArr"u8] = "\u21D1"u8,
    ["rArr"u8] = "\u21D2"u8,
    ["dArr"u8] = "\u21D3"u8,
    ["hArr"u8] = "\u21D4"u8,
    ["forall"u8] = "\u2200"u8,
    ["part"u8] = "\u2202"u8,
    ["exist"u8] = "\u2203"u8,
    ["empty"u8] = "\u2205"u8,
    ["nabla"u8] = "\u2207"u8,
    ["isin"u8] = "\u2208"u8,
    ["notin"u8] = "\u2209"u8,
    ["ni"u8] = "\u220B"u8,
    ["prod"u8] = "\u220F"u8,
    ["sum"u8] = "\u2211"u8,
    ["minus"u8] = "\u2212"u8,
    ["lowast"u8] = "\u2217"u8,
    ["radic"u8] = "\u221A"u8,
    ["prop"u8] = "\u221D"u8,
    ["infin"u8] = "\u221E"u8,
    ["ang"u8] = "\u2220"u8,
    ["and"u8] = "\u2227"u8,
    ["or"u8] = "\u2228"u8,
    ["cap"u8] = "\u2229"u8,
    ["cup"u8] = "\u222A"u8,
    ["int"u8] = "\u222B"u8,
    ["there4"u8] = "\u2234"u8,
    ["sim"u8] = "\u223C"u8,
    ["cong"u8] = "\u2245"u8,
    ["asymp"u8] = "\u2248"u8,
    ["ne"u8] = "\u2260"u8,
    ["equiv"u8] = "\u2261"u8,
    ["le"u8] = "\u2264"u8,
    ["ge"u8] = "\u2265"u8,
    ["sub"u8] = "\u2282"u8,
    ["sup"u8] = "\u2283"u8,
    ["nsub"u8] = "\u2284"u8,
    ["sube"u8] = "\u2286"u8,
    ["supe"u8] = "\u2287"u8,
    ["oplus"u8] = "\u2295"u8,
    ["otimes"u8] = "\u2297"u8,
    ["perp"u8] = "\u22A5"u8,
    ["sdot"u8] = "\u22C5"u8,
    ["lceil"u8] = "\u2308"u8,
    ["rceil"u8] = "\u2309"u8,
    ["lfloor"u8] = "\u230A"u8,
    ["rfloor"u8] = "\u230B"u8,
    ["lang"u8] = "\u2329"u8,
    ["rang"u8] = "\u232A"u8,
    ["loz"u8] = "\u25CA"u8,
    ["spades"u8] = "\u2660"u8,
    ["clubs"u8] = "\u2663"u8,
    ["hearts"u8] = "\u2665"u8,
    ["diams"u8] = "\u2666"u8,
    ["quot"u8] = "\u0022"u8,
    ["amp"u8] = "\u0026"u8,
    ["lt"u8] = "\u003C"u8,
    ["gt"u8] = "\u003E"u8,
    ["OElig"u8] = "\u0152"u8,
    ["oelig"u8] = "\u0153"u8,
    ["Scaron"u8] = "\u0160"u8,
    ["scaron"u8] = "\u0161"u8,
    ["Yuml"u8] = "\u0178"u8,
    ["circ"u8] = "\u02C6"u8,
    ["tilde"u8] = "\u02DC"u8,
    ["ensp"u8] = "\u2002"u8,
    ["emsp"u8] = "\u2003"u8,
    ["thinsp"u8] = "\u2009"u8,
    ["zwnj"u8] = "\u200C"u8,
    ["zwj"u8] = "\u200D"u8,
    ["lrm"u8] = "\u200E"u8,
    ["rlm"u8] = "\u200F"u8,
    ["ndash"u8] = "\u2013"u8,
    ["mdash"u8] = "\u2014"u8,
    ["lsquo"u8] = "\u2018"u8,
    ["rsquo"u8] = "\u2019"u8,
    ["sbquo"u8] = "\u201A"u8,
    ["ldquo"u8] = "\u201C"u8,
    ["rdquo"u8] = "\u201D"u8,
    ["bdquo"u8] = "\u201E"u8,
    ["dagger"u8] = "\u2020"u8,
    ["Dagger"u8] = "\u2021"u8,
    ["permil"u8] = "\u2030"u8,
    ["lsaquo"u8] = "\u2039"u8,
    ["rsaquo"u8] = "\u203A"u8,
    ["euro"u8] = "\u20AC"u8
};

// HTMLAutoClose is the set of HTML elements that
// should be considered to close automatically.
//
// See the [Decoder.Strict] and [Decoder.Entity] fields' documentation.
public static slice<@string> HTMLAutoClose = htmlAutoClose;

/*
		hget http://www.w3.org/TR/html4/loose.dtd |
		9 sed -n 's/<!ELEMENT ([^ ]*) +- O EMPTY.+/	"\1",/p' | tr A-Z a-z
	*/
internal static slice<@string> htmlAutoClose = new @string[]{
    "basefont",
    "br",
    "area",
    "link",
    "img",
    "param",
    "hr",
    "input",
    "col",
    "frame",
    "isindex",
    "base",
    "meta"
}.slice();

internal static slice<byte> escQuot = slice<byte>("&#34;"); // shorter than "&quot;"
internal static slice<byte> escApos = slice<byte>("&#39;"); // shorter than "&apos;"
internal static slice<byte> escAmp = slice<byte>("&amp;");
internal static slice<byte> escLT = slice<byte>("&lt;");
internal static slice<byte> escGT = slice<byte>("&gt;");
internal static slice<byte> escTab = slice<byte>("&#x9;");
internal static slice<byte> escNL = slice<byte>("&#xA;");
internal static slice<byte> escCR = slice<byte>("&#xD;");
internal static slice<byte> escFFFD = slice<byte>("\uFFFD"); // Unicode replacement character

// EscapeText writes to w the properly escaped XML equivalent
// of the plain text data s.
public static error EscapeText(io.Writer w, slice<byte> s) {
    return escapeText(w, s, true);
}

// escapeText writes to w the properly escaped XML equivalent
// of the plain text data s. If escapeNewline is true, newline
// characters will be escaped.
internal static error escapeText(io.Writer w, slice<byte> s, bool escapeNewline) {
    slice<byte> esc = default!;
    nint last = 0;
    for (nint i = 0; i < len(s); ) {
        var (r, width) = utf8.DecodeRune(s[(int)(i)..]);
        i += width;
        switch (r) {
        case (rune)'"': {
            esc = escQuot;
            break;
        }
        case (rune)'\'': {
            esc = escApos;
            break;
        }
        case (rune)'&': {
            esc = escAmp;
            break;
        }
        case (rune)'<': {
            esc = escLT;
            break;
        }
        case (rune)'>': {
            esc = escGT;
            break;
        }
        case (rune)'\t': {
            esc = escTab;
            break;
        }
        case (rune)'\n': {
            if (!escapeNewline) {
                continue;
            }
            esc = escNL;
            break;
        }
        case (rune)'\r': {
            esc = escCR;
            break;
        }
        default: {
            if (!isInCharacterRange(r) || (r == 65533 && width == 1)) {
                esc = escFFFD;
                break;
            }
            continue;
            break;
        }}

        {
            var (_, errΔ1) = w.Write(s[(int)(last)..(int)(i - width)]); if (errΔ1 != default!) {
                return errΔ1;
            }
        }
        {
            var (_, errΔ2) = w.Write(esc); if (errΔ2 != default!) {
                return errΔ2;
            }
        }
        last = i;
    }
    var (_, err) = w.Write(s[(int)(last)..]);
    return err;
}

// EscapeString writes to p the properly escaped XML equivalent
// of the plain text data s.
[GoRecv] internal static void EscapeString(this ref printer p, @string s) {
    slice<byte> esc = default!;
    nint last = 0;
    for (nint i = 0; i < len(s); ) {
        var (r, width) = utf8.DecodeRuneInString(s[(int)(i)..]);
        i += width;
        switch (r) {
        case (rune)'"': {
            esc = escQuot;
            break;
        }
        case (rune)'\'': {
            esc = escApos;
            break;
        }
        case (rune)'&': {
            esc = escAmp;
            break;
        }
        case (rune)'<': {
            esc = escLT;
            break;
        }
        case (rune)'>': {
            esc = escGT;
            break;
        }
        case (rune)'\t': {
            esc = escTab;
            break;
        }
        case (rune)'\n': {
            esc = escNL;
            break;
        }
        case (rune)'\r': {
            esc = escCR;
            break;
        }
        default: {
            if (!isInCharacterRange(r) || (r == 65533 && width == 1)) {
                esc = escFFFD;
                break;
            }
            continue;
            break;
        }}

        p.WriteString(s[(int)(last)..(int)(i - width)]);
        p.Write(esc);
        last = i;
    }
    p.WriteString(s[(int)(last)..]);
}

// Escape is like [EscapeText] but omits the error return value.
// It is provided for backwards compatibility with Go 1.0.
// Code targeting Go 1.1 or later should use [EscapeText].
public static void Escape(io.Writer w, slice<byte> s) {
    EscapeText(w, s);
}

internal static slice<byte> cdataStart = slice<byte>("<![CDATA[");
internal static slice<byte> cdataEnd = slice<byte>("]]>");
internal static slice<byte> cdataEscape = slice<byte>("]]]]><![CDATA[>");

// emitCDATA writes to w the CDATA-wrapped plain text data s.
// It escapes CDATA directives nested in s.
internal static error emitCDATA(io.Writer w, slice<byte> s) {
    if (len(s) == 0) {
        return default!;
    }
    {
        var (_, errΔ1) = w.Write(cdataStart); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    while (ᐧ) {
        var (before, after, ok) = bytes.Cut(s, cdataEnd);
        if (!ok) {
            break;
        }
        // Found a nested CDATA directive end.
        {
            var (_, errΔ2) = w.Write(before); if (errΔ2 != default!) {
                return errΔ2;
            }
        }
        {
            var (_, errΔ3) = w.Write(cdataEscape); if (errΔ3 != default!) {
                return errΔ3;
            }
        }
        s = after;
    }
    {
        var (_, errΔ4) = w.Write(s); if (errΔ4 != default!) {
            return errΔ4;
        }
    }
    var (_, err) = w.Write(cdataEnd);
    return err;
}

// procInst parses the `param="..."` or `param='...'`
// value out of the provided string, returning "" if not found.
internal static @string procInst(@string param, @string s) {
    // TODO: this parsing is somewhat lame and not exact.
    // It works for all actual cases, though.
    param = param + "="u8;
    nint lenp = len(param);
    nint i = 0;
    byte sep = default!;
    while (i < len(s)) {
        @string sub = s[(int)(i)..];
        nint k = strings.Index(sub, param);
        if (k < 0 || lenp + k >= len(sub)) {
            return ""u8;
        }
        i += lenp + k + 1;
        {
            var c = sub[lenp + k]; if (c == (rune)'\'' || c == (rune)'"') {
                sep = c;
                break;
            }
        }
    }
    if (sep == 0) {
        return ""u8;
    }
    nint j = strings.IndexByte(s[(int)(i)..], sep);
    if (j < 0) {
        return ""u8;
    }
    return s[(int)(i)..(int)(i + j)];
}

} // end xml_package
