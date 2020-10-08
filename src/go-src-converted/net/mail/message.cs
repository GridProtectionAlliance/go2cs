// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

/*
Package mail implements parsing of mail messages.

For the most part, this package follows the syntax as specified by RFC 5322 and
extended by RFC 6532.
Notable divergences:
    * Obsolete address formats are not parsed, including addresses with
      embedded route information.
    * The full range of spacing (the CFWS syntax element) is not supported,
      such as breaking addresses across lines.
    * No unicode normalization is performed.
    * The special characters ()[]:;@\, are allowed to appear unquoted in names.
*/
// package mail -- go2cs converted at 2020 October 08 03:43:21 UTC
// import "net/mail" ==> using mail = go.net.mail_package
// Original source: C:\Go\src\net\mail\message.go
using bufio = go.bufio_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using io = go.io_package;
using log = go.log_package;
using mime = go.mime_package;
using textproto = go.net.textproto_package;
using strings = go.strings_package;
using sync = go.sync_package;
using time = go.time_package;
using utf8 = go.unicode.utf8_package;
using static go.builtin;
using System;

namespace go {
namespace net
{
    public static partial class mail_package
    {
        private static var debug = debugT(false);

        private partial struct debugT // : bool
        {
        }

        private static void Printf(this debugT d, @string format, params object[] args)
        {
            args = args.Clone();

            if (d)
            {
                log.Printf(format, args);
            }

        }

        // A Message represents a parsed mail message.
        public partial struct Message
        {
            public Header Header;
            public io.Reader Body;
        }

        // ReadMessage reads a message from r.
        // The headers are parsed, and the body of the message will be available
        // for reading from msg.Body.
        public static (ptr<Message>, error) ReadMessage(io.Reader r)
        {
            ptr<Message> msg = default!;
            error err = default!;

            var tp = textproto.NewReader(bufio.NewReader(r));

            var (hdr, err) = tp.ReadMIMEHeader();
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            return (addr(new Message(Header:Header(hdr),Body:tp.R,)), error.As(null!)!);

        }

        // Layouts suitable for passing to time.Parse.
        // These are tried in order.
        private static sync.Once dateLayoutsBuildOnce = default;        private static slice<@string> dateLayouts = default;

        private static void buildDateLayouts()
        { 
            // Generate layouts based on RFC 5322, section 3.3.

            array<@string> dows = new array<@string>(new @string[] { "", "Mon, " }); // day-of-week
            array<@string> days = new array<@string>(new @string[] { "2", "02" }); // day = 1*2DIGIT
            array<@string> years = new array<@string>(new @string[] { "2006", "06" }); // year = 4*DIGIT / 2*DIGIT
            array<@string> seconds = new array<@string>(new @string[] { ":05", "" }); // second
            // "-0700 (MST)" is not in RFC 5322, but is common.
            array<@string> zones = new array<@string>(new @string[] { "-0700", "MST" }); // zone = (("+" / "-") 4DIGIT) / "GMT" / ...

            foreach (var (_, dow) in dows)
            {
                foreach (var (_, day) in days)
                {
                    foreach (var (_, year) in years)
                    {
                        foreach (var (_, second) in seconds)
                        {
                            foreach (var (_, zone) in zones)
                            {
                                var s = dow + day + " Jan " + year + " 15:04" + second + " " + zone;
                                dateLayouts = append(dateLayouts, s);
                            }

                        }

                    }

                }

            }

        }

        // ParseDate parses an RFC 5322 date string.
        public static (time.Time, error) ParseDate(@string date)
        {
            time.Time _p0 = default;
            error _p0 = default!;

            dateLayoutsBuildOnce.Do(buildDateLayouts); 
            // CR and LF must match and are tolerated anywhere in the date field.
            date = strings.ReplaceAll(date, "\r\n", "");
            if (strings.Index(date, "\r") != -1L)
            {
                return (new time.Time(), error.As(errors.New("mail: header has a CR without LF"))!);
            } 
            // Re-using some addrParser methods which support obsolete text, i.e. non-printable ASCII
            addrParser p = new addrParser(date,nil);
            p.skipSpace(); 

            // RFC 5322: zone = (FWS ( "+" / "-" ) 4DIGIT) / obs-zone
            // zone length is always 5 chars unless obsolete (obs-zone)
            {
                var ind__prev1 = ind;

                var ind = strings.IndexAny(p.s, "+-");

                if (ind != -1L && len(p.s) >= ind + 5L)
                {
                    date = p.s[..ind + 5L];
                    p.s = p.s[ind + 5L..];
                }                {
                    var ind__prev2 = ind;

                    ind = strings.Index(p.s, "T");


                    else if (ind != -1L && len(p.s) >= ind + 1L)
                    { 
                        // The last letter T of the obsolete time zone is checked when no standard time zone is found.
                        // If T is misplaced, the date to parse is garbage.
                        date = p.s[..ind + 1L];
                        p.s = p.s[ind + 1L..];

                    }

                    ind = ind__prev2;

                }


                ind = ind__prev1;

            }

            if (!p.skipCFWS())
            {
                return (new time.Time(), error.As(errors.New("mail: misformatted parenthetical comment"))!);
            }

            foreach (var (_, layout) in dateLayouts)
            {
                var (t, err) = time.Parse(layout, date);
                if (err == null)
                {
                    return (t, error.As(null!)!);
                }

            }
            return (new time.Time(), error.As(errors.New("mail: header could not be parsed"))!);

        }

        // A Header represents the key-value pairs in a mail message header.
        public partial struct Header // : map<@string, slice<@string>>
        {
        }

        // Get gets the first value associated with the given key.
        // It is case insensitive; CanonicalMIMEHeaderKey is used
        // to canonicalize the provided key.
        // If there are no values associated with the key, Get returns "".
        // To access multiple values of a key, or to use non-canonical keys,
        // access the map directly.
        public static @string Get(this Header h, @string key)
        {
            return textproto.MIMEHeader(h).Get(key);
        }

        public static var ErrHeaderNotPresent = errors.New("mail: header not in message");

        // Date parses the Date header field.
        public static (time.Time, error) Date(this Header h)
        {
            time.Time _p0 = default;
            error _p0 = default!;

            var hdr = h.Get("Date");
            if (hdr == "")
            {
                return (new time.Time(), error.As(ErrHeaderNotPresent)!);
            }

            return ParseDate(hdr);

        }

        // AddressList parses the named header field as a list of addresses.
        public static (slice<ptr<Address>>, error) AddressList(this Header h, @string key)
        {
            slice<ptr<Address>> _p0 = default;
            error _p0 = default!;

            var hdr = h.Get(key);
            if (hdr == "")
            {
                return (null, error.As(ErrHeaderNotPresent)!);
            }

            return ParseAddressList(hdr);

        }

        // Address represents a single mail address.
        // An address such as "Barry Gibbs <bg@example.com>" is represented
        // as Address{Name: "Barry Gibbs", Address: "bg@example.com"}.
        public partial struct Address
        {
            public @string Name; // Proper name; may be empty.
            public @string Address; // user@domain
        }

        // ParseAddress parses a single RFC 5322 address, e.g. "Barry Gibbs <bg@example.com>"
        public static (ptr<Address>, error) ParseAddress(@string address)
        {
            ptr<Address> _p0 = default!;
            error _p0 = default!;

            return (addr(new addrParser(s:address))).parseSingleAddress();
        }

        // ParseAddressList parses the given string as a list of addresses.
        public static (slice<ptr<Address>>, error) ParseAddressList(@string list)
        {
            slice<ptr<Address>> _p0 = default;
            error _p0 = default!;

            return (addr(new addrParser(s:list))).parseAddressList();
        }

        // An AddressParser is an RFC 5322 address parser.
        public partial struct AddressParser
        {
            public ptr<mime.WordDecoder> WordDecoder;
        }

        // Parse parses a single RFC 5322 address of the
        // form "Gogh Fir <gf@example.com>" or "foo@example.com".
        private static (ptr<Address>, error) Parse(this ptr<AddressParser> _addr_p, @string address)
        {
            ptr<Address> _p0 = default!;
            error _p0 = default!;
            ref AddressParser p = ref _addr_p.val;

            return (addr(new addrParser(s:address,dec:p.WordDecoder))).parseSingleAddress();
        }

        // ParseList parses the given string as a list of comma-separated addresses
        // of the form "Gogh Fir <gf@example.com>" or "foo@example.com".
        private static (slice<ptr<Address>>, error) ParseList(this ptr<AddressParser> _addr_p, @string list)
        {
            slice<ptr<Address>> _p0 = default;
            error _p0 = default!;
            ref AddressParser p = ref _addr_p.val;

            return (addr(new addrParser(s:list,dec:p.WordDecoder))).parseAddressList();
        }

        // String formats the address as a valid RFC 5322 address.
        // If the address's name contains non-ASCII characters
        // the name will be rendered according to RFC 2047.
        private static @string String(this ptr<Address> _addr_a)
        {
            ref Address a = ref _addr_a.val;
 
            // Format address local@domain
            var at = strings.LastIndex(a.Address, "@");
            @string local = default;            @string domain = default;

            if (at < 0L)
            { 
                // This is a malformed address ("@" is required in addr-spec);
                // treat the whole address as local-part.
                local = a.Address;

            }
            else
            {
                local = a.Address[..at];
                domain = a.Address[at + 1L..];

            } 

            // Add quotes if needed
            var quoteLocal = false;
            {
                var r__prev1 = r;

                foreach (var (__i, __r) in local)
                {
                    i = __i;
                    r = __r;
                    if (isAtext(r, false, false))
                    {
                        continue;
                    }

                    if (r == '.')
                    { 
                        // Dots are okay if they are surrounded by atext.
                        // We only need to check that the previous byte is
                        // not a dot, and this isn't the end of the string.
                        if (i > 0L && local[i - 1L] != '.' && i < len(local) - 1L)
                        {
                            continue;
                        }

                    }

                    quoteLocal = true;
                    break;

                }

                r = r__prev1;
            }

            if (quoteLocal)
            {
                local = quoteString(local);
            }

            @string s = "<" + local + "@" + domain + ">";

            if (a.Name == "")
            {
                return s;
            } 

            // If every character is printable ASCII, quoting is simple.
            var allPrintable = true;
            {
                var r__prev1 = r;

                foreach (var (_, __r) in a.Name)
                {
                    r = __r; 
                    // isWSP here should actually be isFWS,
                    // but we don't support folding yet.
                    if (!isVchar(r) && !isWSP(r) || isMultibyte(r))
                    {
                        allPrintable = false;
                        break;
                    }

                }

                r = r__prev1;
            }

            if (allPrintable)
            {
                return quoteString(a.Name) + " " + s;
            } 

            // Text in an encoded-word in a display-name must not contain certain
            // characters like quotes or parentheses (see RFC 2047 section 5.3).
            // When this is the case encode the name using base64 encoding.
            if (strings.ContainsAny(a.Name, "\"#$%&'(),.:;<>@[]^`{|}~"))
            {
                return mime.BEncoding.Encode("utf-8", a.Name) + " " + s;
            }

            return mime.QEncoding.Encode("utf-8", a.Name) + " " + s;

        }

        private partial struct addrParser
        {
            public @string s;
            public ptr<mime.WordDecoder> dec; // may be nil
        }

        private static (slice<ptr<Address>>, error) parseAddressList(this ptr<addrParser> _addr_p)
        {
            slice<ptr<Address>> _p0 = default;
            error _p0 = default!;
            ref addrParser p = ref _addr_p.val;

            slice<ptr<Address>> list = default;
            while (true)
            {
                p.skipSpace(); 

                // allow skipping empty entries (RFC5322 obs-addr-list)
                if (p.consume(','))
                {
                    continue;
                }

                var (addrs, err) = p.parseAddress(true);
                if (err != null)
                {
                    return (null, error.As(err)!);
                }

                list = append(list, addrs);

                if (!p.skipCFWS())
                {
                    return (null, error.As(errors.New("mail: misformatted parenthetical comment"))!);
                }

                if (p.empty())
                {
                    break;
                }

                if (p.peek() != ',')
                {
                    return (null, error.As(errors.New("mail: expected comma"))!);
                } 

                // Skip empty entries for obs-addr-list.
                while (p.consume(','))
                {
                    p.skipSpace();
                }

                if (p.empty())
                {
                    break;
                }

            }

            return (list, error.As(null!)!);

        }

        private static (ptr<Address>, error) parseSingleAddress(this ptr<addrParser> _addr_p)
        {
            ptr<Address> _p0 = default!;
            error _p0 = default!;
            ref addrParser p = ref _addr_p.val;

            var (addrs, err) = p.parseAddress(true);
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            if (!p.skipCFWS())
            {
                return (_addr_null!, error.As(errors.New("mail: misformatted parenthetical comment"))!);
            }

            if (!p.empty())
            {
                return (_addr_null!, error.As(fmt.Errorf("mail: expected single address, got %q", p.s))!);
            }

            if (len(addrs) == 0L)
            {
                return (_addr_null!, error.As(errors.New("mail: empty group"))!);
            }

            if (len(addrs) > 1L)
            {
                return (_addr_null!, error.As(errors.New("mail: group with multiple addresses"))!);
            }

            return (_addr_addrs[0L]!, error.As(null!)!);

        }

        // parseAddress parses a single RFC 5322 address at the start of p.
        private static (slice<ptr<Address>>, error) parseAddress(this ptr<addrParser> _addr_p, bool handleGroup)
        {
            slice<ptr<Address>> _p0 = default;
            error _p0 = default!;
            ref addrParser p = ref _addr_p.val;

            debug.Printf("parseAddress: %q", p.s);
            p.skipSpace();
            if (p.empty())
            {
                return (null, error.As(errors.New("mail: no address"))!);
            } 

            // address = mailbox / group
            // mailbox = name-addr / addr-spec
            // group = display-name ":" [group-list] ";" [CFWS]

            // addr-spec has a more restricted grammar than name-addr,
            // so try parsing it first, and fallback to name-addr.
            // TODO(dsymonds): Is this really correct?
            var (spec, err) = p.consumeAddrSpec();
            if (err == null)
            {
                @string displayName = default;
                p.skipSpace();
                if (!p.empty() && p.peek() == '(')
                {
                    displayName, err = p.consumeDisplayNameComment();
                    if (err != null)
                    {
                        return (null, error.As(err)!);
                    }

                }

                return (new slice<ptr<Address>>(new ptr<Address>[] { {Name:displayName,Address:spec,} }), error.As(err)!);

            }

            debug.Printf("parseAddress: not an addr-spec: %v", err);
            debug.Printf("parseAddress: state is now %q", p.s); 

            // display-name
            displayName = default;
            if (p.peek() != '<')
            {
                displayName, err = p.consumePhrase();
                if (err != null)
                {
                    return (null, error.As(err)!);
                }

            }

            debug.Printf("parseAddress: displayName=%q", displayName);

            p.skipSpace();
            if (handleGroup)
            {
                if (p.consume(':'))
                {
                    return p.consumeGroupList();
                }

            } 
            // angle-addr = "<" addr-spec ">"
            if (!p.consume('<'))
            {
                var atext = true;
                foreach (var (_, r) in displayName)
                {
                    if (!isAtext(r, true, false))
                    {
                        atext = false;
                        break;
                    }

                }
                if (atext)
                { 
                    // The input is like "foo.bar"; it's possible the input
                    // meant to be "foo.bar@domain", or "foo.bar <...>".
                    return (null, error.As(errors.New("mail: missing '@' or angle-addr"))!);

                } 
                // The input is like "Full Name", which couldn't possibly be a
                // valid email address if followed by "@domain"; the input
                // likely meant to be "Full Name <...>".
                return (null, error.As(errors.New("mail: no angle-addr"))!);

            }

            spec, err = p.consumeAddrSpec();
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            if (!p.consume('>'))
            {
                return (null, error.As(errors.New("mail: unclosed angle-addr"))!);
            }

            debug.Printf("parseAddress: spec=%q", spec);

            return (new slice<ptr<Address>>(new ptr<Address>[] { {Name:displayName,Address:spec,} }), error.As(null!)!);

        }

        private static (slice<ptr<Address>>, error) consumeGroupList(this ptr<addrParser> _addr_p)
        {
            slice<ptr<Address>> _p0 = default;
            error _p0 = default!;
            ref addrParser p = ref _addr_p.val;

            slice<ptr<Address>> group = default; 
            // handle empty group.
            p.skipSpace();
            if (p.consume(';'))
            {
                p.skipCFWS();
                return (group, error.As(null!)!);
            }

            while (true)
            {
                p.skipSpace(); 
                // embedded groups not allowed.
                var (addrs, err) = p.parseAddress(false);
                if (err != null)
                {
                    return (null, error.As(err)!);
                }

                group = append(group, addrs);

                if (!p.skipCFWS())
                {
                    return (null, error.As(errors.New("mail: misformatted parenthetical comment"))!);
                }

                if (p.consume(';'))
                {
                    p.skipCFWS();
                    break;
                }

                if (!p.consume(','))
                {
                    return (null, error.As(errors.New("mail: expected comma"))!);
                }

            }

            return (group, error.As(null!)!);

        }

        // consumeAddrSpec parses a single RFC 5322 addr-spec at the start of p.
        private static (@string, error) consumeAddrSpec(this ptr<addrParser> _addr_p) => func((defer, _, __) =>
        {
            @string spec = default;
            error err = default!;
            ref addrParser p = ref _addr_p.val;

            debug.Printf("consumeAddrSpec: %q", p.s);

            var orig = p.val;
            defer(() =>
            {
                if (err != null)
                {
                    p.val = orig;
                }

            }()); 

            // local-part = dot-atom / quoted-string
            @string localPart = default;
            p.skipSpace();
            if (p.empty())
            {
                return ("", error.As(errors.New("mail: no addr-spec"))!);
            }

            if (p.peek() == '"')
            { 
                // quoted-string
                debug.Printf("consumeAddrSpec: parsing quoted-string");
                localPart, err = p.consumeQuotedString();
                if (localPart == "")
                {
                    err = errors.New("mail: empty quoted string in addr-spec");
                }

            }
            else
            { 
                // dot-atom
                debug.Printf("consumeAddrSpec: parsing dot-atom");
                localPart, err = p.consumeAtom(true, false);

            }

            if (err != null)
            {
                debug.Printf("consumeAddrSpec: failed: %v", err);
                return ("", error.As(err)!);
            }

            if (!p.consume('@'))
            {
                return ("", error.As(errors.New("mail: missing @ in addr-spec"))!);
            } 

            // domain = dot-atom / domain-literal
            @string domain = default;
            p.skipSpace();
            if (p.empty())
            {
                return ("", error.As(errors.New("mail: no domain in addr-spec"))!);
            } 
            // TODO(dsymonds): Handle domain-literal
            domain, err = p.consumeAtom(true, false);
            if (err != null)
            {
                return ("", error.As(err)!);
            }

            return (localPart + "@" + domain, error.As(null!)!);

        });

        // consumePhrase parses the RFC 5322 phrase at the start of p.
        private static (@string, error) consumePhrase(this ptr<addrParser> _addr_p)
        {
            @string phrase = default;
            error err = default!;
            ref addrParser p = ref _addr_p.val;

            debug.Printf("consumePhrase: [%s]", p.s); 
            // phrase = 1*word
            slice<@string> words = default;
            bool isPrevEncoded = default;
            while (true)
            { 
                // word = atom / quoted-string
                @string word = default;
                p.skipSpace();
                if (p.empty())
                {
                    break;
                }

                var isEncoded = false;
                if (p.peek() == '"')
                { 
                    // quoted-string
                    word, err = p.consumeQuotedString();

                }
                else
                { 
                    // atom
                    // We actually parse dot-atom here to be more permissive
                    // than what RFC 5322 specifies.
                    word, err = p.consumeAtom(true, true);
                    if (err == null)
                    {
                        word, isEncoded, err = p.decodeRFC2047Word(word);
                    }

                }

                if (err != null)
                {
                    break;
                }

                debug.Printf("consumePhrase: consumed %q", word);
                if (isPrevEncoded && isEncoded)
                {
                    words[len(words) - 1L] += word;
                }
                else
                {
                    words = append(words, word);
                }

                isPrevEncoded = isEncoded;

            } 
            // Ignore any error if we got at least one word.
 
            // Ignore any error if we got at least one word.
            if (err != null && len(words) == 0L)
            {
                debug.Printf("consumePhrase: hit err: %v", err);
                return ("", error.As(fmt.Errorf("mail: missing word in phrase: %v", err))!);
            }

            phrase = strings.Join(words, " ");
            return (phrase, error.As(null!)!);

        }

        // consumeQuotedString parses the quoted string at the start of p.
        private static (@string, error) consumeQuotedString(this ptr<addrParser> _addr_p)
        {
            @string qs = default;
            error err = default!;
            ref addrParser p = ref _addr_p.val;
 
            // Assume first byte is '"'.
            long i = 1L;
            var qsb = make_slice<int>(0L, 10L);

            var escaped = false;

Loop:
            while (true)
            {
                var (r, size) = utf8.DecodeRuneInString(p.s[i..]);


                if (size == 0L) 
                    return ("", error.As(errors.New("mail: unclosed quoted-string"))!);
                else if (size == 1L && r == utf8.RuneError) 
                    return ("", error.As(fmt.Errorf("mail: invalid utf-8 in quoted-string: %q", p.s))!);
                else if (escaped) 
                    //  quoted-pair = ("\" (VCHAR / WSP))

                    if (!isVchar(r) && !isWSP(r))
                    {
                        return ("", error.As(fmt.Errorf("mail: bad character in quoted-string: %q", r))!);
                    }

                    qsb = append(qsb, r);
                    escaped = false;
                else if (isQtext(r) || isWSP(r)) 
                    // qtext (printable US-ASCII excluding " and \), or
                    // FWS (almost; we're ignoring CRLF)
                    qsb = append(qsb, r);
                else if (r == '"') 
                    _breakLoop = true;

                    break;
                else if (r == '\\') 
                    escaped = true;
                else 
                    return ("", error.As(fmt.Errorf("mail: bad character in quoted-string: %q", r))!);
                                i += size;

            }
            p.s = p.s[i + 1L..];
            return (string(qsb), error.As(null!)!);

        }

        // consumeAtom parses an RFC 5322 atom at the start of p.
        // If dot is true, consumeAtom parses an RFC 5322 dot-atom instead.
        // If permissive is true, consumeAtom will not fail on:
        // - leading/trailing/double dots in the atom (see golang.org/issue/4938)
        // - special characters (RFC 5322 3.2.3) except '<', '>', ':' and '"' (see golang.org/issue/21018)
        private static (@string, error) consumeAtom(this ptr<addrParser> _addr_p, bool dot, bool permissive)
        {
            @string atom = default;
            error err = default!;
            ref addrParser p = ref _addr_p.val;

            long i = 0L;

Loop:

            while (true)
            {
                var (r, size) = utf8.DecodeRuneInString(p.s[i..]);

                if (size == 1L && r == utf8.RuneError) 
                    return ("", error.As(fmt.Errorf("mail: invalid utf-8 in address: %q", p.s))!);
                else if (size == 0L || !isAtext(r, dot, permissive)) 
                    _breakLoop = true;

                    break;
                else 
                    i += size;
                
            }

            if (i == 0L)
            {
                return ("", error.As(errors.New("mail: invalid string"))!);
            }

            atom = p.s[..i];
            p.s = p.s[i..];
            if (!permissive)
            {
                if (strings.HasPrefix(atom, "."))
                {
                    return ("", error.As(errors.New("mail: leading dot in atom"))!);
                }

                if (strings.Contains(atom, ".."))
                {
                    return ("", error.As(errors.New("mail: double dot in atom"))!);
                }

                if (strings.HasSuffix(atom, "."))
                {
                    return ("", error.As(errors.New("mail: trailing dot in atom"))!);
                }

            }

            return (atom, error.As(null!)!);

        }

        private static (@string, error) consumeDisplayNameComment(this ptr<addrParser> _addr_p)
        {
            @string _p0 = default;
            error _p0 = default!;
            ref addrParser p = ref _addr_p.val;

            if (!p.consume('('))
            {
                return ("", error.As(errors.New("mail: comment does not start with ("))!);
            }

            var (comment, ok) = p.consumeComment();
            if (!ok)
            {
                return ("", error.As(errors.New("mail: misformatted parenthetical comment"))!);
            } 

            // TODO(stapelberg): parse quoted-string within comment
            var words = strings.FieldsFunc(comment, r => r == ' ' || r == '\t');
            foreach (var (idx, word) in words)
            {
                var (decoded, isEncoded, err) = p.decodeRFC2047Word(word);
                if (err != null)
                {
                    return ("", error.As(err)!);
                }

                if (isEncoded)
                {
                    words[idx] = decoded;
                }

            }
            return (strings.Join(words, " "), error.As(null!)!);

        }

        private static bool consume(this ptr<addrParser> _addr_p, byte c)
        {
            ref addrParser p = ref _addr_p.val;

            if (p.empty() || p.peek() != c)
            {
                return false;
            }

            p.s = p.s[1L..];
            return true;

        }

        // skipSpace skips the leading space and tab characters.
        private static void skipSpace(this ptr<addrParser> _addr_p)
        {
            ref addrParser p = ref _addr_p.val;

            p.s = strings.TrimLeft(p.s, " \t");
        }

        private static byte peek(this ptr<addrParser> _addr_p)
        {
            ref addrParser p = ref _addr_p.val;

            return p.s[0L];
        }

        private static bool empty(this ptr<addrParser> _addr_p)
        {
            ref addrParser p = ref _addr_p.val;

            return p.len() == 0L;
        }

        private static long len(this ptr<addrParser> _addr_p)
        {
            ref addrParser p = ref _addr_p.val;

            return len(p.s);
        }

        // skipCFWS skips CFWS as defined in RFC5322.
        private static bool skipCFWS(this ptr<addrParser> _addr_p)
        {
            ref addrParser p = ref _addr_p.val;

            p.skipSpace();

            while (true)
            {
                if (!p.consume('('))
                {
                    break;
                }

                {
                    var (_, ok) = p.consumeComment();

                    if (!ok)
                    {
                        return false;
                    }

                }


                p.skipSpace();

            }


            return true;

        }

        private static (@string, bool) consumeComment(this ptr<addrParser> _addr_p)
        {
            @string _p0 = default;
            bool _p0 = default;
            ref addrParser p = ref _addr_p.val;
 
            // '(' already consumed.
            long depth = 1L;

            @string comment = default;
            while (true)
            {
                if (p.empty() || depth == 0L)
                {
                    break;
                }

                if (p.peek() == '\\' && p.len() > 1L)
                {
                    p.s = p.s[1L..];
                }
                else if (p.peek() == '(')
                {
                    depth++;
                }
                else if (p.peek() == ')')
                {
                    depth--;
                }

                if (depth > 0L)
                {
                    comment += p.s[..1L];
                }

                p.s = p.s[1L..];

            }


            return (comment, depth == 0L);

        }

        private static (@string, bool, error) decodeRFC2047Word(this ptr<addrParser> _addr_p, @string s)
        {
            @string word = default;
            bool isEncoded = default;
            error err = default!;
            ref addrParser p = ref _addr_p.val;

            if (p.dec != null)
            {
                word, err = p.dec.Decode(s);
            }
            else
            {
                word, err = rfc2047Decoder.Decode(s);
            }

            if (err == null)
            {
                return (word, true, error.As(null!)!);
            }

            {
                charsetError (_, ok) = err._<charsetError>();

                if (ok)
                {
                    return (s, true, error.As(err)!);
                } 

                // Ignore invalid RFC 2047 encoded-word errors.

            } 

            // Ignore invalid RFC 2047 encoded-word errors.
            return (s, false, error.As(null!)!);

        }

        private static mime.WordDecoder rfc2047Decoder = new mime.WordDecoder(CharsetReader:func(charsetstring,inputio.Reader)(io.Reader,error){returnnil,charsetError(charset)},);

        private partial struct charsetError // : @string
        {
        }

        private static @string Error(this charsetError e)
        {
            return fmt.Sprintf("charset not supported: %q", string(e));
        }

        // isAtext reports whether r is an RFC 5322 atext character.
        // If dot is true, period is included.
        // If permissive is true, RFC 5322 3.2.3 specials is included,
        // except '<', '>', ':' and '"'.
        private static bool isAtext(int r, bool dot, bool permissive)
        {
            switch (r)
            {
                case '.': 
                    return dot; 

                    // RFC 5322 3.2.3. specials
                    break;
                case '(': 

                case ')': 

                case '[': 

                case ']': 

                case ';': 

                case '@': 

                case '\\': 

                case ',': 
                    return permissive;
                    break;
                case '<': 

                case '>': 

                case '"': 

                case ':': 
                    return false;
                    break;
            }
            return isVchar(r);

        }

        // isQtext reports whether r is an RFC 5322 qtext character.
        private static bool isQtext(int r)
        { 
            // Printable US-ASCII, excluding backslash or quote.
            if (r == '\\' || r == '"')
            {
                return false;
            }

            return isVchar(r);

        }

        // quoteString renders a string as an RFC 5322 quoted-string.
        private static @string quoteString(@string s)
        {
            strings.Builder buf = default;
            buf.WriteByte('"');
            foreach (var (_, r) in s)
            {
                if (isQtext(r) || isWSP(r))
                {
                    buf.WriteRune(r);
                }
                else if (isVchar(r))
                {
                    buf.WriteByte('\\');
                    buf.WriteRune(r);
                }

            }
            buf.WriteByte('"');
            return buf.String();

        }

        // isVchar reports whether r is an RFC 5322 VCHAR character.
        private static bool isVchar(int r)
        { 
            // Visible (printing) characters.
            return '!' <= r && r <= '~' || isMultibyte(r);

        }

        // isMultibyte reports whether r is a multi-byte UTF-8 character
        // as supported by RFC 6532
        private static bool isMultibyte(int r)
        {
            return r >= utf8.RuneSelf;
        }

        // isWSP reports whether r is a WSP (white space).
        // WSP is a space or horizontal tab (RFC 5234 Appendix B).
        private static bool isWSP(int r)
        {
            return r == ' ' || r == '\t';
        }
    }
}}
