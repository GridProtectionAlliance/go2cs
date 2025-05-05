// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

/*
Package mail implements parsing of mail messages.

For the most part, this package follows the syntax as specified by RFC 5322 and
extended by RFC 6532.
Notable divergences:
  - Obsolete address formats are not parsed, including addresses with
    embedded route information.
  - The full range of spacing (the CFWS syntax element) is not supported,
    such as breaking addresses across lines.
  - No unicode normalization is performed.
  - A leading From line is permitted, as in mbox format (RFC 4155).
*/
namespace go.net;

using bufio = bufio_package;
using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using log = log_package;
using mime = mime_package;
using net = net_package;
using textproto = net.textproto_package;
using strings = strings_package;
using sync = sync_package;
using time = time_package;
using utf8 = unicode.utf8_package;
using unicode;
using ꓸꓸꓸany = Span<any>;

partial class mail_package {

internal static debugT debug = ((debugT)false);

[GoType("bool")] partial struct debugT;

internal static void Printf(this debugT d, @string format, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    if (d) {
        log.Printf(format, args.ꓸꓸꓸ);
    }
}

// A Message represents a parsed mail message.
[GoType] partial struct Message {
    public Header Header;
    public io_package.Reader Body;
}

// ReadMessage reads a message from r.
// The headers are parsed, and the body of the message will be available
// for reading from msg.Body.
public static (ж<Message> msg, error err) ReadMessage(io.Reader r) {
    ж<Message> msg = default!;
    error err = default!;

    var tp = textproto.NewReader(bufio.NewReader(r));
    (hdr, err) = readHeader(tp);
    if (err != default! && (!AreEqual(err, io.EOF) || len(hdr) == 0)) {
        return (default!, err);
    }
    return (Ꮡ(new Message(
        Header: ((Header)hdr),
        Body: (~tp).R
    )), default!);
}

// readHeader reads the message headers from r.
// This is like textproto.ReadMIMEHeader, but doesn't validate.
// The fix for issue #53188 tightened up net/textproto to enforce
// restrictions of RFC 7230.
// This package implements RFC 5322, which does not have those restrictions.
// This function copies the relevant code from net/textproto,
// simplified for RFC 5322.
internal static (map<@string, slice<@string>>, error) readHeader(ж<textproto.Reader> Ꮡr) {
    ref var r = ref Ꮡr.val;

    var m = new map<@string, slice<@string>>();
    // The first line cannot start with a leading space.
    {
        (buf, err) = r.R.Peek(1); if (err == default! && (buf[0] == (rune)' ' || buf[0] == (rune)'\t')) {
            var (line, errΔ1) = r.ReadLine();
            if (errΔ1 != default!) {
                return (m, errΔ1);
            }
            return (m, errors.New("malformed initial line: "u8 + line));
        }
    }
    while (ᐧ) {
        var (kv, err) = r.ReadContinuedLine();
        if (kv == ""u8) {
            return (m, err);
        }
        // Key ends at first colon.
        var (k, v, ok) = strings.Cut(kv, ":"u8);
        if (!ok) {
            return (m, errors.New("malformed header line: "u8 + kv));
        }
        @string key = textproto.CanonicalMIMEHeaderKey(k);
        // Permit empty key, because that is what we did in the past.
        if (key == ""u8) {
            continue;
        }
        // Skip initial spaces in value.
        @string value = strings.TrimLeft(v, " \t"u8);
        m[key] = append(m[key], value);
        if (err != default!) {
            return (m, err);
        }
    }
}

// Layouts suitable for passing to time.Parse.
// These are tried in order.
internal static sync.Once dateLayoutsBuildOnce;

internal static slice<@string> dateLayouts;

internal static void buildDateLayouts() {
    // Generate layouts based on RFC 5322, section 3.3.
    var dows = new @string[]{"", "Mon, "}.array();
    // day-of-week
    var days = new @string[]{"2", "02"}.array();
    // day = 1*2DIGIT
    var years = new @string[]{"2006", "06"}.array();
    // year = 4*DIGIT / 2*DIGIT
    var seconds = new @string[]{":05", ""}.array();
    // second
    // "-0700 (MST)" is not in RFC 5322, but is common.
    var zones = new @string[]{"-0700", "MST", "UT"}.array();
    // zone = (("+" / "-") 4DIGIT) / "UT" / "GMT" / ...
    foreach (var (_, dow) in dows) {
        foreach (var (_, day) in days) {
            foreach (var (_, year) in years) {
                foreach (var (_, second) in seconds) {
                    foreach (var (_, zone) in zones) {
                        @string s = dow + day + " Jan "u8 + year + " 15:04"u8 + second + " "u8 + zone;
                        dateLayouts = append(dateLayouts, s);
                    }
                }
            }
        }
    }
}

// ParseDate parses an RFC 5322 date string.
public static (time.Time, error) ParseDate(@string date) {
    dateLayoutsBuildOnce.Do(buildDateLayouts);
    // CR and LF must match and are tolerated anywhere in the date field.
    date = strings.ReplaceAll(date, "\r\n"u8, ""u8);
    if (strings.Contains(date, "\r"u8)) {
        return (new time.Time(nil), errors.New("mail: header has a CR without LF"u8));
    }
    // Re-using some addrParser methods which support obsolete text, i.e. non-printable ASCII
    var p = new addrParser(date, nil);
    p.skipSpace();
    // RFC 5322: zone = (FWS ( "+" / "-" ) 4DIGIT) / obs-zone
    // zone length is always 5 chars unless obsolete (obs-zone)
    {
        nint ind = strings.IndexAny(p.s, "+-"u8); if (ind != -1 && len(p.s) >= ind + 5){
            date = p.s[..(int)(ind + 5)];
            p.s = p.s[(int)(ind + 5)..];
        } else {
            nint indΔ1 = strings.Index(p.s, "T"u8);
            if (indΔ1 == 0) {
                // In this case we have the following date formats:
                // * Thu, 20 Nov 1997 09:55:06 MDT
                // * Thu, 20 Nov 1997 09:55:06 MDT (MDT)
                // * Thu, 20 Nov 1997 09:55:06 MDT (This comment)
                indΔ1 = strings.Index(p.s[1..], "T"u8);
                if (indΔ1 != -1) {
                    indΔ1++;
                }
            }
            if (indΔ1 != -1 && len(p.s) >= indΔ1 + 5) {
                // The last letter T of the obsolete time zone is checked when no standard time zone is found.
                // If T is misplaced, the date to parse is garbage.
                date = p.s[..(int)(indΔ1 + 1)];
                p.s = p.s[(int)(indΔ1 + 1)..];
            }
        }
    }
    if (!p.skipCFWS()) {
        return (new time.Time(nil), errors.New("mail: misformatted parenthetical comment"u8));
    }
    foreach (var (_, layout) in dateLayouts) {
        var (t, err) = time.Parse(layout, date);
        if (err == default!) {
            return (t, default!);
        }
    }
    return (new time.Time(nil), errors.New("mail: header could not be parsed"u8));
}
/* visitMapType: map[string][]string */

// Get gets the first value associated with the given key.
// It is case insensitive; CanonicalMIMEHeaderKey is used
// to canonicalize the provided key.
// If there are no values associated with the key, Get returns "".
// To access multiple values of a key, or to use non-canonical keys,
// access the map directly.
public static @string Get(this Header h, @string key) {
    return ((textproto.MIMEHeader)h).Get(key);
}

public static error ErrHeaderNotPresent = errors.New("mail: header not in message"u8);

// Date parses the Date header field.
public static (time.Time, error) Date(this Header h) {
    @string hdr = h.Get("Date"u8);
    if (hdr == ""u8) {
        return (new time.Time(nil), ErrHeaderNotPresent);
    }
    return ParseDate(hdr);
}

// AddressList parses the named header field as a list of addresses.
public static (slice<ж<Address>>, error) AddressList(this Header h, @string key) {
    @string hdr = h.Get(key);
    if (hdr == ""u8) {
        return (default!, ErrHeaderNotPresent);
    }
    return ParseAddressList(hdr);
}

// Address represents a single mail address.
// An address such as "Barry Gibbs <bg@example.com>" is represented
// as Address{Name: "Barry Gibbs", Address: "bg@example.com"}.
[GoType] partial struct Address {
    public @string Name; // Proper name; may be empty.
    public @string Address; // user@domain
}

// ParseAddress parses a single RFC 5322 address, e.g. "Barry Gibbs <bg@example.com>"
public static (ж<Address>, error) ParseAddress(@string address) {
    return (Ꮡ(new addrParser(s: address))).parseSingleAddress();
}

// ParseAddressList parses the given string as a list of addresses.
public static (slice<ж<Address>>, error) ParseAddressList(@string list) {
    return (Ꮡ(new addrParser(s: list))).parseAddressList();
}

// An AddressParser is an RFC 5322 address parser.
[GoType] partial struct AddressParser {
    // WordDecoder optionally specifies a decoder for RFC 2047 encoded-words.
    public ж<mime_package.WordDecoder> WordDecoder;
}

// Parse parses a single RFC 5322 address of the
// form "Gogh Fir <gf@example.com>" or "foo@example.com".
[GoRecv] public static (ж<Address>, error) Parse(this ref AddressParser p, @string address) {
    return (Ꮡ(new addrParser(s: address, dec: p.WordDecoder))).parseSingleAddress();
}

// ParseList parses the given string as a list of comma-separated addresses
// of the form "Gogh Fir <gf@example.com>" or "foo@example.com".
[GoRecv] public static (slice<ж<Address>>, error) ParseList(this ref AddressParser p, @string list) {
    return (Ꮡ(new addrParser(s: list, dec: p.WordDecoder))).parseAddressList();
}

// String formats the address as a valid RFC 5322 address.
// If the address's name contains non-ASCII characters
// the name will be rendered according to RFC 2047.
[GoRecv] public static @string String(this ref Address a) {
    // Format address local@domain
    nint at = strings.LastIndex(a.Address, "@"u8);
    @string local = default!;
    @string domain = default!;
    if (at < 0){
        // This is a malformed address ("@" is required in addr-spec);
        // treat the whole address as local-part.
        local = a.Address;
    } else {
        (local, domain) = (a.Address[..(int)(at)], a.Address[(int)(at + 1)..]);
    }
    // Add quotes if needed
    var quoteLocal = false;
    foreach (var (i, r) in local) {
        if (isAtext(r, false)) {
            continue;
        }
        if (r == (rune)'.') {
            // Dots are okay if they are surrounded by atext.
            // We only need to check that the previous byte is
            // not a dot, and this isn't the end of the string.
            if (i > 0 && local[i - 1] != (rune)'.' && i < len(local) - 1) {
                continue;
            }
        }
        quoteLocal = true;
        break;
    }
    if (quoteLocal) {
        local = quoteString(local);
    }
    @string s = "<"u8 + local + "@"u8 + domain + ">"u8;
    if (a.Name == ""u8) {
        return s;
    }
    // If every character is printable ASCII, quoting is simple.
    var allPrintable = true;
    foreach (var (_, r) in a.Name) {
        // isWSP here should actually be isFWS,
        // but we don't support folding yet.
        if (!isVchar(r) && !isWSP(r) || isMultibyte(r)) {
            allPrintable = false;
            break;
        }
    }
    if (allPrintable) {
        return quoteString(a.Name) + " "u8 + s;
    }
    // Text in an encoded-word in a display-name must not contain certain
    // characters like quotes or parentheses (see RFC 2047 section 5.3).
    // When this is the case encode the name using base64 encoding.
    if (strings.ContainsAny(a.Name, "\"#$%&'(),.:;<>@[]^`{|}~"u8)) {
        return mime.BEncoding.Encode("utf-8"u8, a.Name) + " "u8 + s;
    }
    return mime.QEncoding.Encode("utf-8"u8, a.Name) + " "u8 + s;
}

[GoType] partial struct addrParser {
    internal @string s;
    internal ж<mime_package.WordDecoder> dec; // may be nil
}

[GoRecv] internal static (slice<ж<Address>>, error) parseAddressList(this ref addrParser p) {
    slice<ж<Address>> list = default!;
    while (ᐧ) {
        p.skipSpace();
        // allow skipping empty entries (RFC5322 obs-addr-list)
        if (p.consume((rune)',')) {
            continue;
        }
        (addrs, err) = p.parseAddress(true);
        if (err != default!) {
            return (default!, err);
        }
        list = append(list, Ꮡaddrs.ꓸꓸꓸ);
        if (!p.skipCFWS()) {
            return (default!, errors.New("mail: misformatted parenthetical comment"u8));
        }
        if (p.empty()) {
            break;
        }
        if (p.peek() != (rune)',') {
            return (default!, errors.New("mail: expected comma"u8));
        }
        // Skip empty entries for obs-addr-list.
        while (p.consume((rune)',')) {
            p.skipSpace();
        }
        if (p.empty()) {
            break;
        }
    }
    return (list, default!);
}

[GoRecv] internal static (ж<Address>, error) parseSingleAddress(this ref addrParser p) {
    (addrs, err) = p.parseAddress(true);
    if (err != default!) {
        return (default!, err);
    }
    if (!p.skipCFWS()) {
        return (default!, errors.New("mail: misformatted parenthetical comment"u8));
    }
    if (!p.empty()) {
        return (default!, fmt.Errorf("mail: expected single address, got %q"u8, p.s));
    }
    if (len(addrs) == 0) {
        return (default!, errors.New("mail: empty group"u8));
    }
    if (len(addrs) > 1) {
        return (default!, errors.New("mail: group with multiple addresses"u8));
    }
    return (addrs[0], default!);
}

// parseAddress parses a single RFC 5322 address at the start of p.
[GoRecv] internal static (slice<ж<Address>>, error) parseAddress(this ref addrParser p, bool handleGroup) {
    debug.Printf("parseAddress: %q"u8, p.s);
    p.skipSpace();
    if (p.empty()) {
        return (default!, errors.New("mail: no address"u8));
    }
    // address = mailbox / group
    // mailbox = name-addr / addr-spec
    // group = display-name ":" [group-list] ";" [CFWS]
    // addr-spec has a more restricted grammar than name-addr,
    // so try parsing it first, and fallback to name-addr.
    // TODO(dsymonds): Is this really correct?
    var (spec, err) = p.consumeAddrSpec();
    if (err == default!) {
        @string displayNameΔ1 = default!;
        p.skipSpace();
        if (!p.empty() && p.peek() == (rune)'(') {
            (, err) = p.consumeDisplayNameComment();
            if (err != default!) {
                return (default!, err);
            }
        }
        return (new ж<Address>[]{new(
            Name: displayNameΔ1,
            Address: spec
        )
        }.slice(), err);
    }
    debug.Printf("parseAddress: not an addr-spec: %v"u8, err);
    debug.Printf("parseAddress: state is now %q"u8, p.s);
    // display-name
    @string displayName = default!;
    if (p.peek() != (rune)'<') {
        (displayName, err) = p.consumePhrase();
        if (err != default!) {
            return (default!, err);
        }
    }
    debug.Printf("parseAddress: displayName=%q"u8, displayName);
    p.skipSpace();
    if (handleGroup) {
        if (p.consume((rune)':')) {
            return p.consumeGroupList();
        }
    }
    // angle-addr = "<" addr-spec ">"
    if (!p.consume((rune)'<')) {
        var atext = true;
        foreach (var (_, r) in displayName) {
            if (!isAtext(r, true)) {
                atext = false;
                break;
            }
        }
        if (atext) {
            // The input is like "foo.bar"; it's possible the input
            // meant to be "foo.bar@domain", or "foo.bar <...>".
            return (default!, errors.New("mail: missing '@' or angle-addr"u8));
        }
        // The input is like "Full Name", which couldn't possibly be a
        // valid email address if followed by "@domain"; the input
        // likely meant to be "Full Name <...>".
        return (default!, errors.New("mail: no angle-addr"u8));
    }
    (spec, err) = p.consumeAddrSpec();
    if (err != default!) {
        return (default!, err);
    }
    if (!p.consume((rune)'>')) {
        return (default!, errors.New("mail: unclosed angle-addr"u8));
    }
    debug.Printf("parseAddress: spec=%q"u8, spec);
    return (new ж<Address>[]{new(
        Name: displayName,
        Address: spec
    )
    }.slice(), default!);
}

[GoRecv] internal static (slice<ж<Address>>, error) consumeGroupList(this ref addrParser p) {
    slice<ж<Address>> group = default!;
    // handle empty group.
    p.skipSpace();
    if (p.consume((rune)';')) {
        if (!p.skipCFWS()) {
            return (default!, errors.New("mail: misformatted parenthetical comment"u8));
        }
        return (group, default!);
    }
    while (ᐧ) {
        p.skipSpace();
        // embedded groups not allowed.
        (addrs, err) = p.parseAddress(false);
        if (err != default!) {
            return (default!, err);
        }
        group = append(group, Ꮡaddrs.ꓸꓸꓸ);
        if (!p.skipCFWS()) {
            return (default!, errors.New("mail: misformatted parenthetical comment"u8));
        }
        if (p.consume((rune)';')) {
            if (!p.skipCFWS()) {
                return (default!, errors.New("mail: misformatted parenthetical comment"u8));
            }
            break;
        }
        if (!p.consume((rune)',')) {
            return (default!, errors.New("mail: expected comma"u8));
        }
    }
    return (group, default!);
}

// consumeAddrSpec parses a single RFC 5322 addr-spec at the start of p.
[GoRecv] internal static (@string spec, error err) consumeAddrSpec(this ref addrParser p) => func((defer, _) => {
    @string spec = default!;
    error err = default!;

    debug.Printf("consumeAddrSpec: %q"u8, p.s);
    ref var orig = ref heap<addrParser>(out var Ꮡorig);
    orig = p;
    var origʗ1 = orig;
    defer(() => {
        if (err != default!) {
            p = origʗ1;
        }
    });
    // local-part = dot-atom / quoted-string
    @string localPart = default!;
    p.skipSpace();
    if (p.empty()) {
        return ("", errors.New("mail: no addr-spec"u8));
    }
    if (p.peek() == (rune)'"'){
        // quoted-string
        debug.Printf("consumeAddrSpec: parsing quoted-string"u8);
        (localPart, err) = p.consumeQuotedString();
        if (localPart == ""u8) {
            err = errors.New("mail: empty quoted string in addr-spec"u8);
        }
    } else {
        // dot-atom
        debug.Printf("consumeAddrSpec: parsing dot-atom"u8);
        (localPart, err) = p.consumeAtom(true, false);
    }
    if (err != default!) {
        debug.Printf("consumeAddrSpec: failed: %v"u8, err);
        return ("", err);
    }
    if (!p.consume((rune)'@')) {
        return ("", errors.New("mail: missing @ in addr-spec"u8));
    }
    // domain = dot-atom / domain-literal
    @string domain = default!;
    p.skipSpace();
    if (p.empty()) {
        return ("", errors.New("mail: no domain in addr-spec"u8));
    }
    if (p.peek() == (rune)'['){
        // domain-literal
        (domain, err) = p.consumeDomainLiteral();
        if (err != default!) {
            return ("", err);
        }
    } else {
        // dot-atom
        (domain, err) = p.consumeAtom(true, false);
        if (err != default!) {
            return ("", err);
        }
    }
    return (localPart + "@"u8 + domain, default!);
});

// consumePhrase parses the RFC 5322 phrase at the start of p.
[GoRecv] internal static (@string phrase, error err) consumePhrase(this ref addrParser p) {
    @string phrase = default!;
    error err = default!;

    debug.Printf("consumePhrase: [%s]"u8, p.s);
    // phrase = 1*word
    slice<@string> words = default!;
    bool isPrevEncoded = default!;
    while (ᐧ) {
        // obs-phrase allows CFWS after one word
        if (len(words) > 0) {
            if (!p.skipCFWS()) {
                return ("", errors.New("mail: misformatted parenthetical comment"u8));
            }
        }
        // word = atom / quoted-string
        @string word = default!;
        p.skipSpace();
        if (p.empty()) {
            break;
        }
        var isEncoded = false;
        if (p.peek() == (rune)'"'){
            // quoted-string
            (word, err) = p.consumeQuotedString();
        } else {
            // atom
            // We actually parse dot-atom here to be more permissive
            // than what RFC 5322 specifies.
            (word, err) = p.consumeAtom(true, true);
            if (err == default!) {
                (word, isEncoded, err) = p.decodeRFC2047Word(word);
            }
        }
        if (err != default!) {
            break;
        }
        debug.Printf("consumePhrase: consumed %q"u8, word);
        if (isPrevEncoded && isEncoded){
            words[len(words) - 1] += word;
        } else {
            words = append(words, word);
        }
        isPrevEncoded = isEncoded;
    }
    // Ignore any error if we got at least one word.
    if (err != default! && len(words) == 0) {
        debug.Printf("consumePhrase: hit err: %v"u8, err);
        return ("", fmt.Errorf("mail: missing word in phrase: %v"u8, err));
    }
    phrase = strings.Join(words, " "u8);
    return (phrase, default!);
}

// consumeQuotedString parses the quoted string at the start of p.
[GoRecv] internal static (@string qs, error err) consumeQuotedString(this ref addrParser p) {
    @string qs = default!;
    error err = default!;

    // Assume first byte is '"'.
    nint i = 1;
    var qsb = new slice<rune>(0, 10);
    var escaped = false;
Loop:
    while (ᐧ) {
        var (r, size) = utf8.DecodeRuneInString(p.s[(int)(i)..]);
        switch (ᐧ) {
        case {} when size is 0: {
            return ("", errors.New("mail: unclosed quoted-string"u8));
        }
        case {} when size == 1 && r == utf8.RuneError: {
            return ("", fmt.Errorf("mail: invalid utf-8 in quoted-string: %q"u8, p.s));
        }
        case {} when escaped: {
            if (!isVchar(r) && !isWSP(r)) {
                //  quoted-pair = ("\" (VCHAR / WSP))
                return ("", fmt.Errorf("mail: bad character in quoted-string: %q"u8, r));
            }
            qsb = append(qsb, r);
            escaped = false;
            break;
        }
        case {} when isQtext(r) || isWSP(r): {
            qsb = append(qsb, // qtext (printable US-ASCII excluding " and \), or
 // FWS (almost; we're ignoring CRLF)
 r);
            break;
        }
        case {} when r is (rune)'"': {
            goto break_Loop;
            break;
        }
        case {} when r is (rune)'\\': {
            escaped = true;
            break;
        }
        default: {
            return ("", fmt.Errorf("mail: bad character in quoted-string: %q"u8, r));
        }}

        i += size;
continue_Loop:;
    }
break_Loop:;
    p.s = p.s[(int)(i + 1)..];
    return (((@string)qsb), default!);
}

// consumeAtom parses an RFC 5322 atom at the start of p.
// If dot is true, consumeAtom parses an RFC 5322 dot-atom instead.
// If permissive is true, consumeAtom will not fail on:
// - leading/trailing/double dots in the atom (see golang.org/issue/4938)
[GoRecv] internal static (@string atom, error err) consumeAtom(this ref addrParser p, bool dot, bool permissive) {
    @string atom = default!;
    error err = default!;

    nint i = 0;
Loop:
    while (ᐧ) {
        var (r, size) = utf8.DecodeRuneInString(p.s[(int)(i)..]);
        switch (ᐧ) {
        case {} when size == 1 && r == utf8.RuneError: {
            return ("", fmt.Errorf("mail: invalid utf-8 in address: %q"u8, p.s));
        }
        case {} when size == 0 || !isAtext(r, dot): {
            goto break_Loop;
            break;
        }
        default: {
            i += size;
            break;
        }}

continue_Loop:;
    }
break_Loop:;
    if (i == 0) {
        return ("", errors.New("mail: invalid string"u8));
    }
    (atom, p.s) = (p.s[..(int)(i)], p.s[(int)(i)..]);
    if (!permissive) {
        if (strings.HasPrefix(atom, "."u8)) {
            return ("", errors.New("mail: leading dot in atom"u8));
        }
        if (strings.Contains(atom, ".."u8)) {
            return ("", errors.New("mail: double dot in atom"u8));
        }
        if (strings.HasSuffix(atom, "."u8)) {
            return ("", errors.New("mail: trailing dot in atom"u8));
        }
    }
    return (atom, default!);
}

// consumeDomainLiteral parses an RFC 5322 domain-literal at the start of p.
[GoRecv] internal static (@string, error) consumeDomainLiteral(this ref addrParser p) {
    // Skip the leading [
    if (!p.consume((rune)'[')) {
        return ("", errors.New(@"mail: missing ""["" in domain-literal"u8));
    }
    // Parse the dtext
    @string dtext = default!;
    while (ᐧ) {
        if (p.empty()) {
            return ("", errors.New("mail: unclosed domain-literal"u8));
        }
        if (p.peek() == (rune)']') {
            break;
        }
        var (r, size) = utf8.DecodeRuneInString(p.s);
        if (size == 1 && r == utf8.RuneError) {
            return ("", fmt.Errorf("mail: invalid utf-8 in domain-literal: %q"u8, p.s));
        }
        if (!isDtext(r)) {
            return ("", fmt.Errorf("mail: bad character in domain-literal: %q"u8, r));
        }
        dtext += p.s[..(int)(size)];
        p.s = p.s[(int)(size)..];
    }
    // Skip the trailing ]
    if (!p.consume((rune)']')) {
        return ("", errors.New("mail: unclosed domain-literal"u8));
    }
    // Check if the domain literal is an IP address
    if (net.ParseIP(dtext) == default!) {
        return ("", fmt.Errorf("mail: invalid IP address in domain-literal: %q"u8, dtext));
    }
    return ("["u8 + dtext + "]"u8, default!);
}

[GoRecv] internal static (@string, error) consumeDisplayNameComment(this ref addrParser p) {
    if (!p.consume((rune)'(')) {
        return ("", errors.New("mail: comment does not start with ("u8));
    }
    var (comment, ok) = p.consumeComment();
    if (!ok) {
        return ("", errors.New("mail: misformatted parenthetical comment"u8));
    }
    // TODO(stapelberg): parse quoted-string within comment
    var words = strings.FieldsFunc(comment, (rune r) => r == (rune)' ' || r == (rune)'\t');
    foreach (var (idx, word) in words) {
        var (decoded, isEncoded, err) = p.decodeRFC2047Word(word);
        if (err != default!) {
            return ("", err);
        }
        if (isEncoded) {
            words[idx] = decoded;
        }
    }
    return (strings.Join(words, " "u8), default!);
}

[GoRecv] internal static bool consume(this ref addrParser p, byte c) {
    if (p.empty() || p.peek() != c) {
        return false;
    }
    p.s = p.s[1..];
    return true;
}

// skipSpace skips the leading space and tab characters.
[GoRecv] internal static void skipSpace(this ref addrParser p) {
    p.s = strings.TrimLeft(p.s, " \t"u8);
}

[GoRecv] internal static byte peek(this ref addrParser p) {
    return p.s[0];
}

[GoRecv] internal static bool empty(this ref addrParser p) {
    return p.len() == 0;
}

[GoRecv] internal static nint len(this ref addrParser p) {
    return len(p.s);
}

// skipCFWS skips CFWS as defined in RFC5322.
[GoRecv] internal static bool skipCFWS(this ref addrParser p) {
    p.skipSpace();
    while (ᐧ) {
        if (!p.consume((rune)'(')) {
            break;
        }
        {
            var (_, ok) = p.consumeComment(); if (!ok) {
                return false;
            }
        }
        p.skipSpace();
    }
    return true;
}

[GoRecv] internal static (@string, bool) consumeComment(this ref addrParser p) {
    // '(' already consumed.
    nint depth = 1;
    @string comment = default!;
    while (ᐧ) {
        if (p.empty() || depth == 0) {
            break;
        }
        if (p.peek() == (rune)'\\' && p.len() > 1){
            p.s = p.s[1..];
        } else 
        if (p.peek() == (rune)'('){
            depth++;
        } else 
        if (p.peek() == (rune)')') {
            depth--;
        }
        if (depth > 0) {
            comment += p.s[..1];
        }
        p.s = p.s[1..];
    }
    return (comment, depth == 0);
}

[GoRecv] internal static (@string word, bool isEncoded, error err) decodeRFC2047Word(this ref addrParser p, @string s) {
    @string word = default!;
    bool isEncoded = default!;
    error err = default!;

    var dec = p.dec;
    if (dec == nil) {
        dec = Ꮡ(rfc2047Decoder);
    }
    // Substitute our own CharsetReader function so that we can tell
    // whether an error from the Decode method was due to the
    // CharsetReader (meaning the charset is invalid).
    // We used to look for the charsetError type in the error result,
    // but that behaves badly with CharsetReaders other than the
    // one in rfc2047Decoder.
    var adec = dec.val;
    var charsetReaderError = false;
    adec.CharsetReader = 
    var decʗ1 = dec;
    (@string charset, io.Reader input) => {
        if ((~decʗ1).CharsetReader == default!) {
            charsetReaderError = true;
            return (default!, ((charsetError)charset));
        }
        (r, errΔ1) = (~decʗ1).CharsetReader(charset, input);
        if (errΔ1 != default!) {
            charsetReaderError = true;
        }
        return (r, errΔ1);
    };
    (word, err) = adec.Decode(s);
    if (err == default!) {
        return (word, true, default!);
    }
    // If the error came from the character set reader
    // (meaning the character set itself is invalid
    // but the decoding worked fine until then),
    // return the original text and the error,
    // with isEncoded=true.
    if (charsetReaderError) {
        return (s, true, err);
    }
    // Ignore invalid RFC 2047 encoded-word errors.
    return (s, false, default!);
}

internal static mime.WordDecoder rfc2047Decoder = new mime.WordDecoder(
    CharsetReader: 
    (@string charset, io.Reader input) => (default!, ((charsetError)charset))
);

[GoType("@string")] partial struct charsetError;

internal static @string Error(this charsetError e) {
    return fmt.Sprintf("charset not supported: %q"u8, ((@string)e));
}

// isAtext reports whether r is an RFC 5322 atext character.
// If dot is true, period is included.
internal static bool isAtext(rune r, bool dot) {
    switch (r) {
    case (rune)'.': {
        return dot;
    }
    case (rune)'(' or (rune)')' or (rune)'<' or (rune)'>' or (rune)'[' or (rune)']' or (rune)':' or (rune)';' or (rune)'@' or (rune)'\\' or (rune)',' or (rune)'"': {
        return false;
    }}

    // RFC 5322 3.2.3. specials
    // RFC 5322 3.2.3. specials
    return isVchar(r);
}

// isQtext reports whether r is an RFC 5322 qtext character.
internal static bool isQtext(rune r) {
    // Printable US-ASCII, excluding backslash or quote.
    if (r == (rune)'\\' || r == (rune)'"') {
        return false;
    }
    return isVchar(r);
}

// quoteString renders a string as an RFC 5322 quoted-string.
internal static @string quoteString(@string s) {
    strings.Builder b = default!;
    b.WriteByte((rune)'"');
    foreach (var (_, r) in s) {
        if (isQtext(r) || isWSP(r)){
            b.WriteRune(r);
        } else 
        if (isVchar(r)) {
            b.WriteByte((rune)'\\');
            b.WriteRune(r);
        }
    }
    b.WriteByte((rune)'"');
    return b.String();
}

// isVchar reports whether r is an RFC 5322 VCHAR character.
internal static bool isVchar(rune r) {
    // Visible (printing) characters.
    return (rune)'!' <= r && r <= (rune)'~' || isMultibyte(r);
}

// isMultibyte reports whether r is a multi-byte UTF-8 character
// as supported by RFC 6532.
internal static bool isMultibyte(rune r) {
    return r >= utf8.RuneSelf;
}

// isWSP reports whether r is a WSP (white space).
// WSP is a space or horizontal tab (RFC 5234 Appendix B).
internal static bool isWSP(rune r) {
    return r == (rune)' ' || r == (rune)'\t';
}

// isDtext reports whether r is an RFC 5322 dtext character.
internal static bool isDtext(rune r) {
    // Printable US-ASCII, excluding "[", "]", or "\".
    if (r == (rune)'[' || r == (rune)']' || r == (rune)'\\') {
        return false;
    }
    return isVchar(r);
}

} // end mail_package
