// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package dnsmessage provides a mostly RFC 1035 compliant implementation of
// DNS message packing and unpacking.
//
// The package also supports messages with Extension Mechanisms for DNS
// (EDNS(0)) as defined in RFC 6891.
//
// This implementation is designed to minimize heap allocations and avoid
// unnecessary packing and unpacking as much as possible.
// package dnsmessage -- go2cs converted at 2020 October 08 03:31:37 UTC
// import "golang.org/x/net/dns/dnsmessage" ==> using dnsmessage = go.golang.org.x.net.dns.dnsmessage_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\net\dns\dnsmessage\message.go
using errors = go.errors_package;
using fmt = go.fmt_package;
using static go.builtin;

namespace go {
namespace golang.org {
namespace x {
namespace net {
namespace dns
{
    public static partial class dnsmessage_package
    {
        // Message formats

        // A Type is a type of DNS request and response.
        public partial struct Type // : ushort
        {
        }

 
        // ResourceHeader.Type and Question.Type
        public static readonly Type TypeA = (Type)1L;
        public static readonly Type TypeNS = (Type)2L;
        public static readonly Type TypeCNAME = (Type)5L;
        public static readonly Type TypeSOA = (Type)6L;
        public static readonly Type TypePTR = (Type)12L;
        public static readonly Type TypeMX = (Type)15L;
        public static readonly Type TypeTXT = (Type)16L;
        public static readonly Type TypeAAAA = (Type)28L;
        public static readonly Type TypeSRV = (Type)33L;
        public static readonly Type TypeOPT = (Type)41L; 

        // Question.Type
        public static readonly Type TypeWKS = (Type)11L;
        public static readonly Type TypeHINFO = (Type)13L;
        public static readonly Type TypeMINFO = (Type)14L;
        public static readonly Type TypeAXFR = (Type)252L;
        public static readonly Type TypeALL = (Type)255L;


        private static map typeNames = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<Type, @string>{TypeA:"TypeA",TypeNS:"TypeNS",TypeCNAME:"TypeCNAME",TypeSOA:"TypeSOA",TypePTR:"TypePTR",TypeMX:"TypeMX",TypeTXT:"TypeTXT",TypeAAAA:"TypeAAAA",TypeSRV:"TypeSRV",TypeOPT:"TypeOPT",TypeWKS:"TypeWKS",TypeHINFO:"TypeHINFO",TypeMINFO:"TypeMINFO",TypeAXFR:"TypeAXFR",TypeALL:"TypeALL",};

        // String implements fmt.Stringer.String.
        public static @string String(this Type t)
        {
            {
                var (n, ok) = typeNames[t];

                if (ok)
                {
                    return n;
                }

            }

            return printUint16(uint16(t));

        }

        // GoString implements fmt.GoStringer.GoString.
        public static @string GoString(this Type t)
        {
            {
                var (n, ok) = typeNames[t];

                if (ok)
                {
                    return "dnsmessage." + n;
                }

            }

            return printUint16(uint16(t));

        }

        // A Class is a type of network.
        public partial struct Class // : ushort
        {
        }

 
        // ResourceHeader.Class and Question.Class
        public static readonly Class ClassINET = (Class)1L;
        public static readonly Class ClassCSNET = (Class)2L;
        public static readonly Class ClassCHAOS = (Class)3L;
        public static readonly Class ClassHESIOD = (Class)4L; 

        // Question.Class
        public static readonly Class ClassANY = (Class)255L;


        private static map classNames = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<Class, @string>{ClassINET:"ClassINET",ClassCSNET:"ClassCSNET",ClassCHAOS:"ClassCHAOS",ClassHESIOD:"ClassHESIOD",ClassANY:"ClassANY",};

        // String implements fmt.Stringer.String.
        public static @string String(this Class c)
        {
            {
                var (n, ok) = classNames[c];

                if (ok)
                {
                    return n;
                }

            }

            return printUint16(uint16(c));

        }

        // GoString implements fmt.GoStringer.GoString.
        public static @string GoString(this Class c)
        {
            {
                var (n, ok) = classNames[c];

                if (ok)
                {
                    return "dnsmessage." + n;
                }

            }

            return printUint16(uint16(c));

        }

        // An OpCode is a DNS operation code.
        public partial struct OpCode // : ushort
        {
        }

        // GoString implements fmt.GoStringer.GoString.
        public static @string GoString(this OpCode o)
        {
            return printUint16(uint16(o));
        }

        // An RCode is a DNS response status code.
        public partial struct RCode // : ushort
        {
        }

 
        // Message.Rcode
        public static readonly RCode RCodeSuccess = (RCode)0L;
        public static readonly RCode RCodeFormatError = (RCode)1L;
        public static readonly RCode RCodeServerFailure = (RCode)2L;
        public static readonly RCode RCodeNameError = (RCode)3L;
        public static readonly RCode RCodeNotImplemented = (RCode)4L;
        public static readonly RCode RCodeRefused = (RCode)5L;


        private static map rCodeNames = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<RCode, @string>{RCodeSuccess:"RCodeSuccess",RCodeFormatError:"RCodeFormatError",RCodeServerFailure:"RCodeServerFailure",RCodeNameError:"RCodeNameError",RCodeNotImplemented:"RCodeNotImplemented",RCodeRefused:"RCodeRefused",};

        // String implements fmt.Stringer.String.
        public static @string String(this RCode r)
        {
            {
                var (n, ok) = rCodeNames[r];

                if (ok)
                {
                    return n;
                }

            }

            return printUint16(uint16(r));

        }

        // GoString implements fmt.GoStringer.GoString.
        public static @string GoString(this RCode r)
        {
            {
                var (n, ok) = rCodeNames[r];

                if (ok)
                {
                    return "dnsmessage." + n;
                }

            }

            return printUint16(uint16(r));

        }

        private static @string printPaddedUint8(byte i)
        {
            var b = byte(i);
            return string(new slice<byte>(new byte[] { b/100+'0', b/10%10+'0', b%10+'0' }));
        }

        private static slice<byte> printUint8Bytes(slice<byte> buf, byte i)
        {
            var b = byte(i);
            if (i >= 100L)
            {
                buf = append(buf, b / 100L + '0');
            }

            if (i >= 10L)
            {
                buf = append(buf, b / 10L % 10L + '0');
            }

            return append(buf, b % 10L + '0');

        }

        private static @string printByteSlice(slice<byte> b)
        {
            if (len(b) == 0L)
            {
                return "";
            }

            var buf = make_slice<byte>(0L, 5L * len(b));
            buf = printUint8Bytes(buf, uint8(b[0L]));
            foreach (var (_, n) in b[1L..])
            {
                buf = append(buf, ',', ' ');
                buf = printUint8Bytes(buf, uint8(n));
            }
            return string(buf);

        }

        private static readonly @string hexDigits = (@string)"0123456789abcdef";



        private static @string printString(slice<byte> str)
        {
            var buf = make_slice<byte>(0L, len(str));
            for (long i = 0L; i < len(str); i++)
            {
                var c = str[i];
                if (c == '.' || c == '-' || c == ' ' || 'A' <= c && c <= 'Z' || 'a' <= c && c <= 'z' || '0' <= c && c <= '9')
                {
                    buf = append(buf, c);
                    continue;
                }

                var upper = c >> (int)(4L);
                var lower = (c << (int)(4L)) >> (int)(4L);
                buf = append(buf, '\\', 'x', hexDigits[upper], hexDigits[lower]);

            }

            return string(buf);

        }

        private static @string printUint16(ushort i)
        {
            return printUint32(uint32(i));
        }

        private static @string printUint32(uint i)
        { 
            // Max value is 4294967295.
            var buf = make_slice<byte>(10L);
            {
                var b = buf;
                var d = uint32(1000000000L);

                while (d > 0L)
                {
                    b[0L] = byte(i / d % 10L + '0');
                    if (b[0L] == '0' && len(b) == len(buf) && len(buf) > 1L)
                    {
                        buf = buf[1L..];
                    d /= 10L;
                    }

                    b = b[1L..];
                    i %= d;

                }

            }
            return string(buf);

        }

        private static @string printBool(bool b)
        {
            if (b)
            {
                return "true";
            }

            return "false";

        }

 
        // ErrNotStarted indicates that the prerequisite information isn't
        // available yet because the previous records haven't been appropriately
        // parsed, skipped or finished.
        public static var ErrNotStarted = errors.New("parsing/packing of this type isn't available yet");        public static var ErrSectionDone = errors.New("parsing/packing of this section has completed");        private static var errBaseLen = errors.New("insufficient data for base length type");        private static var errCalcLen = errors.New("insufficient data for calculated length type");        private static var errReserved = errors.New("segment prefix is reserved");        private static var errTooManyPtr = errors.New("too many pointers (>10)");        private static var errInvalidPtr = errors.New("invalid pointer");        private static var errNilResouceBody = errors.New("nil resource body");        private static var errResourceLen = errors.New("insufficient data for resource body length");        private static var errSegTooLong = errors.New("segment length too long");        private static var errZeroSegLen = errors.New("zero length segment");        private static var errResTooLong = errors.New("resource length too long");        private static var errTooManyQuestions = errors.New("too many Questions to pack (>65535)");        private static var errTooManyAnswers = errors.New("too many Answers to pack (>65535)");        private static var errTooManyAuthorities = errors.New("too many Authorities to pack (>65535)");        private static var errTooManyAdditionals = errors.New("too many Additionals to pack (>65535)");        private static var errNonCanonicalName = errors.New("name is not in canonical format (it must end with a .)");        private static var errStringTooLong = errors.New("character string exceeds maximum length (255)");        private static var errCompressedSRV = errors.New("compressed name in SRV resource data");

        // Internal constants.
 
        // packStartingCap is the default initial buffer size allocated during
        // packing.
        //
        // The starting capacity doesn't matter too much, but most DNS responses
        // Will be <= 512 bytes as it is the limit for DNS over UDP.
        private static readonly long packStartingCap = (long)512L; 

        // uint16Len is the length (in bytes) of a uint16.
        private static readonly long uint16Len = (long)2L; 

        // uint32Len is the length (in bytes) of a uint32.
        private static readonly long uint32Len = (long)4L; 

        // headerLen is the length (in bytes) of a DNS header.
        //
        // A header is comprised of 6 uint16s and no padding.
        private static readonly long headerLen = (long)6L * uint16Len;


        private partial struct nestedError
        {
            public @string s; // err is the nested error.
            public error err;
        }

        // nestedError implements error.Error.
        private static @string Error(this ptr<nestedError> _addr_e)
        {
            ref nestedError e = ref _addr_e.val;

            return e.s + ": " + e.err.Error();
        }

        // Header is a representation of a DNS message header.
        public partial struct Header
        {
            public ushort ID;
            public bool Response;
            public OpCode OpCode;
            public bool Authoritative;
            public bool Truncated;
            public bool RecursionDesired;
            public bool RecursionAvailable;
            public RCode RCode;
        }

        private static (ushort, ushort) pack(this ptr<Header> _addr_m)
        {
            ushort id = default;
            ushort bits = default;
            ref Header m = ref _addr_m.val;

            id = m.ID;
            bits = uint16(m.OpCode) << (int)(11L) | uint16(m.RCode);
            if (m.RecursionAvailable)
            {
                bits |= headerBitRA;
            }

            if (m.RecursionDesired)
            {
                bits |= headerBitRD;
            }

            if (m.Truncated)
            {
                bits |= headerBitTC;
            }

            if (m.Authoritative)
            {
                bits |= headerBitAA;
            }

            if (m.Response)
            {
                bits |= headerBitQR;
            }

            return ;

        }

        // GoString implements fmt.GoStringer.GoString.
        private static @string GoString(this ptr<Header> _addr_m)
        {
            ref Header m = ref _addr_m.val;

            return "dnsmessage.Header{" + "ID: " + printUint16(m.ID) + ", " + "Response: " + printBool(m.Response) + ", " + "OpCode: " + m.OpCode.GoString() + ", " + "Authoritative: " + printBool(m.Authoritative) + ", " + "Truncated: " + printBool(m.Truncated) + ", " + "RecursionDesired: " + printBool(m.RecursionDesired) + ", " + "RecursionAvailable: " + printBool(m.RecursionAvailable) + ", " + "RCode: " + m.RCode.GoString() + "}";
        }

        // Message is a representation of a DNS message.
        public partial struct Message
        {
            public ref Header Header => ref Header_val;
            public slice<Question> Questions;
            public slice<Resource> Answers;
            public slice<Resource> Authorities;
            public slice<Resource> Additionals;
        }

        private partial struct section // : byte
        {
        }

        private static readonly section sectionNotStarted = (section)iota;
        private static readonly var sectionHeader = (var)0;
        private static readonly var sectionQuestions = (var)1;
        private static readonly var sectionAnswers = (var)2;
        private static readonly var sectionAuthorities = (var)3;
        private static readonly var sectionAdditionals = (var)4;
        private static readonly headerBitQR sectionDone = (headerBitQR)1L << (int)(15L); // query/response (response=1)
        private static readonly long headerBitAA = (long)1L << (int)(10L); // authoritative
        private static readonly long headerBitTC = (long)1L << (int)(9L); // truncated
        private static readonly long headerBitRD = (long)1L << (int)(8L); // recursion desired
        private static readonly long headerBitRA = (long)1L << (int)(7L); // recursion available

        private static map sectionNames = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<section, @string>{sectionHeader:"header",sectionQuestions:"Question",sectionAnswers:"Answer",sectionAuthorities:"Authority",sectionAdditionals:"Additional",};

        // header is the wire format for a DNS message header.
        private partial struct header
        {
            public ushort id;
            public ushort bits;
            public ushort questions;
            public ushort answers;
            public ushort authorities;
            public ushort additionals;
        }

        private static ushort count(this ptr<header> _addr_h, section sec)
        {
            ref header h = ref _addr_h.val;


            if (sec == sectionQuestions) 
                return h.questions;
            else if (sec == sectionAnswers) 
                return h.answers;
            else if (sec == sectionAuthorities) 
                return h.authorities;
            else if (sec == sectionAdditionals) 
                return h.additionals;
                        return 0L;

        }

        // pack appends the wire format of the header to msg.
        private static slice<byte> pack(this ptr<header> _addr_h, slice<byte> msg)
        {
            ref header h = ref _addr_h.val;

            msg = packUint16(msg, h.id);
            msg = packUint16(msg, h.bits);
            msg = packUint16(msg, h.questions);
            msg = packUint16(msg, h.answers);
            msg = packUint16(msg, h.authorities);
            return packUint16(msg, h.additionals);
        }

        private static (long, error) unpack(this ptr<header> _addr_h, slice<byte> msg, long off)
        {
            long _p0 = default;
            error _p0 = default!;
            ref header h = ref _addr_h.val;

            var newOff = off;
            error err = default!;
            h.id, newOff, err = unpackUint16(msg, newOff);

            if (err != null)
            {
                return (off, error.As(addr(new nestedError("id",err))!)!);
            }

            h.bits, newOff, err = unpackUint16(msg, newOff);

            if (err != null)
            {
                return (off, error.As(addr(new nestedError("bits",err))!)!);
            }

            h.questions, newOff, err = unpackUint16(msg, newOff);

            if (err != null)
            {
                return (off, error.As(addr(new nestedError("questions",err))!)!);
            }

            h.answers, newOff, err = unpackUint16(msg, newOff);

            if (err != null)
            {
                return (off, error.As(addr(new nestedError("answers",err))!)!);
            }

            h.authorities, newOff, err = unpackUint16(msg, newOff);

            if (err != null)
            {
                return (off, error.As(addr(new nestedError("authorities",err))!)!);
            }

            h.additionals, newOff, err = unpackUint16(msg, newOff);

            if (err != null)
            {
                return (off, error.As(addr(new nestedError("additionals",err))!)!);
            }

            return (newOff, error.As(null!)!);

        }

        private static Header header(this ptr<header> _addr_h)
        {
            ref header h = ref _addr_h.val;

            return new Header(ID:h.id,Response:(h.bits&headerBitQR)!=0,OpCode:OpCode(h.bits>>11)&0xF,Authoritative:(h.bits&headerBitAA)!=0,Truncated:(h.bits&headerBitTC)!=0,RecursionDesired:(h.bits&headerBitRD)!=0,RecursionAvailable:(h.bits&headerBitRA)!=0,RCode:RCode(h.bits&0xF),);
        }

        // A Resource is a DNS resource record.
        public partial struct Resource
        {
            public ResourceHeader Header;
            public ResourceBody Body;
        }

        private static @string GoString(this ptr<Resource> _addr_r)
        {
            ref Resource r = ref _addr_r.val;

            return "dnsmessage.Resource{" + "Header: " + r.Header.GoString() + ", Body: &" + r.Body.GoString() + "}";
        }

        // A ResourceBody is a DNS resource record minus the header.
        public partial interface ResourceBody
        {
            @string pack(slice<byte> msg, map<@string, long> compression, long compressionOff); // realType returns the actual type of the Resource. This is used to
// fill in the header Type field.
            @string realType(); // GoString implements fmt.GoStringer.GoString.
            @string GoString();
        }

        // pack appends the wire format of the Resource to msg.
        private static (slice<byte>, error) pack(this ptr<Resource> _addr_r, slice<byte> msg, map<@string, long> compression, long compressionOff)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref Resource r = ref _addr_r.val;

            if (r.Body == null)
            {
                return (msg, error.As(errNilResouceBody)!);
            }

            var oldMsg = msg;
            r.Header.Type = r.Body.realType();
            var (msg, lenOff, err) = r.Header.pack(msg, compression, compressionOff);
            if (err != null)
            {
                return (msg, error.As(addr(new nestedError("ResourceHeader",err))!)!);
            }

            var preLen = len(msg);
            msg, err = r.Body.pack(msg, compression, compressionOff);
            if (err != null)
            {
                return (msg, error.As(addr(new nestedError("content",err))!)!);
            }

            {
                var err = r.Header.fixLen(msg, lenOff, preLen);

                if (err != null)
                {
                    return (oldMsg, error.As(err)!);
                }

            }

            return (msg, error.As(null!)!);

        }

        // A Parser allows incrementally parsing a DNS message.
        //
        // When parsing is started, the Header is parsed. Next, each Question can be
        // either parsed or skipped. Alternatively, all Questions can be skipped at
        // once. When all Questions have been parsed, attempting to parse Questions
        // will return (nil, nil) and attempting to skip Questions will return
        // (true, nil). After all Questions have been either parsed or skipped, all
        // Answers, Authorities and Additionals can be either parsed or skipped in the
        // same way, and each type of Resource must be fully parsed or skipped before
        // proceeding to the next type of Resource.
        //
        // Note that there is no requirement to fully skip or parse the message.
        public partial struct Parser
        {
            public slice<byte> msg;
            public header header;
            public section section;
            public long off;
            public long index;
            public bool resHeaderValid;
            public ResourceHeader resHeader;
        }

        // Start parses the header and enables the parsing of Questions.
        private static (Header, error) Start(this ptr<Parser> _addr_p, slice<byte> msg)
        {
            Header _p0 = default;
            error _p0 = default!;
            ref Parser p = ref _addr_p.val;

            if (p.msg != null)
            {
                p.val = new Parser();
            }

            p.msg = msg;
            error err = default!;
            p.off, err = p.header.unpack(msg, 0L);

            if (err != null)
            {
                return (new Header(), error.As(addr(new nestedError("unpacking header",err))!)!);
            }

            p.section = sectionQuestions;
            return (p.header.header(), error.As(null!)!);

        }

        private static error checkAdvance(this ptr<Parser> _addr_p, section sec)
        {
            ref Parser p = ref _addr_p.val;

            if (p.section < sec)
            {
                return error.As(ErrNotStarted)!;
            }

            if (p.section > sec)
            {
                return error.As(ErrSectionDone)!;
            }

            p.resHeaderValid = false;
            if (p.index == int(p.header.count(sec)))
            {
                p.index = 0L;
                p.section++;
                return error.As(ErrSectionDone)!;
            }

            return error.As(null!)!;

        }

        private static (Resource, error) resource(this ptr<Parser> _addr_p, section sec)
        {
            Resource _p0 = default;
            error _p0 = default!;
            ref Parser p = ref _addr_p.val;

            Resource r = default;
            error err = default!;
            r.Header, err = p.resourceHeader(sec);
            if (err != null)
            {
                return (r, error.As(err)!);
            }

            p.resHeaderValid = false;
            r.Body, p.off, err = unpackResourceBody(p.msg, p.off, r.Header);
            if (err != null)
            {
                return (new Resource(), error.As(addr(new nestedError("unpacking "+sectionNames[sec],err))!)!);
            }

            p.index++;
            return (r, error.As(null!)!);

        }

        private static (ResourceHeader, error) resourceHeader(this ptr<Parser> _addr_p, section sec)
        {
            ResourceHeader _p0 = default;
            error _p0 = default!;
            ref Parser p = ref _addr_p.val;

            if (p.resHeaderValid)
            {
                return (p.resHeader, error.As(null!)!);
            }

            {
                var err = p.checkAdvance(sec);

                if (err != null)
                {
                    return (new ResourceHeader(), error.As(err)!);
                }

            }

            ResourceHeader hdr = default;
            var (off, err) = hdr.unpack(p.msg, p.off);
            if (err != null)
            {
                return (new ResourceHeader(), error.As(err)!);
            }

            p.resHeaderValid = true;
            p.resHeader = hdr;
            p.off = off;
            return (hdr, error.As(null!)!);

        }

        private static error skipResource(this ptr<Parser> _addr_p, section sec)
        {
            ref Parser p = ref _addr_p.val;

            if (p.resHeaderValid)
            {
                var newOff = p.off + int(p.resHeader.Length);
                if (newOff > len(p.msg))
                {
                    return error.As(errResourceLen)!;
                }

                p.off = newOff;
                p.resHeaderValid = false;
                p.index++;
                return error.As(null!)!;

            }

            {
                var err__prev1 = err;

                var err = p.checkAdvance(sec);

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            err = default!;
            p.off, err = skipResource(p.msg, p.off);
            if (err != null)
            {
                return error.As(addr(new nestedError("skipping: "+sectionNames[sec],err))!)!;
            }

            p.index++;
            return error.As(null!)!;

        }

        // Question parses a single Question.
        private static (Question, error) Question(this ptr<Parser> _addr_p)
        {
            Question _p0 = default;
            error _p0 = default!;
            ref Parser p = ref _addr_p.val;

            {
                var err = p.checkAdvance(sectionQuestions);

                if (err != null)
                {
                    return (new Question(), error.As(err)!);
                }

            }

            Name name = default;
            var (off, err) = name.unpack(p.msg, p.off);
            if (err != null)
            {
                return (new Question(), error.As(addr(new nestedError("unpacking Question.Name",err))!)!);
            }

            var (typ, off, err) = unpackType(p.msg, off);
            if (err != null)
            {
                return (new Question(), error.As(addr(new nestedError("unpacking Question.Type",err))!)!);
            }

            var (class, off, err) = unpackClass(p.msg, off);
            if (err != null)
            {
                return (new Question(), error.As(addr(new nestedError("unpacking Question.Class",err))!)!);
            }

            p.off = off;
            p.index++;
            return (new Question(name,typ,class), error.As(null!)!);

        }

        // AllQuestions parses all Questions.
        private static (slice<Question>, error) AllQuestions(this ptr<Parser> _addr_p)
        {
            slice<Question> _p0 = default;
            error _p0 = default!;
            ref Parser p = ref _addr_p.val;
 
            // Multiple questions are valid according to the spec,
            // but servers don't actually support them. There will
            // be at most one question here.
            //
            // Do not pre-allocate based on info in p.header, since
            // the data is untrusted.
            Question qs = new slice<Question>(new Question[] {  });
            while (true)
            {
                var (q, err) = p.Question();
                if (err == ErrSectionDone)
                {
                    return (qs, error.As(null!)!);
                }

                if (err != null)
                {
                    return (null, error.As(err)!);
                }

                qs = append(qs, q);

            }


        }

        // SkipQuestion skips a single Question.
        private static error SkipQuestion(this ptr<Parser> _addr_p)
        {
            ref Parser p = ref _addr_p.val;

            {
                var err = p.checkAdvance(sectionQuestions);

                if (err != null)
                {
                    return error.As(err)!;
                }

            }

            var (off, err) = skipName(p.msg, p.off);
            if (err != null)
            {
                return error.As(addr(new nestedError("skipping Question Name",err))!)!;
            }

            off, err = skipType(p.msg, off);

            if (err != null)
            {
                return error.As(addr(new nestedError("skipping Question Type",err))!)!;
            }

            off, err = skipClass(p.msg, off);

            if (err != null)
            {
                return error.As(addr(new nestedError("skipping Question Class",err))!)!;
            }

            p.off = off;
            p.index++;
            return error.As(null!)!;

        }

        // SkipAllQuestions skips all Questions.
        private static error SkipAllQuestions(this ptr<Parser> _addr_p)
        {
            ref Parser p = ref _addr_p.val;

            while (true)
            {
                {
                    var err = p.SkipQuestion();

                    if (err == ErrSectionDone)
                    {
                        return error.As(null!)!;
                    }
                    else if (err != null)
                    {
                        return error.As(err)!;
                    }


                }

            }


        }

        // AnswerHeader parses a single Answer ResourceHeader.
        private static (ResourceHeader, error) AnswerHeader(this ptr<Parser> _addr_p)
        {
            ResourceHeader _p0 = default;
            error _p0 = default!;
            ref Parser p = ref _addr_p.val;

            return p.resourceHeader(sectionAnswers);
        }

        // Answer parses a single Answer Resource.
        private static (Resource, error) Answer(this ptr<Parser> _addr_p)
        {
            Resource _p0 = default;
            error _p0 = default!;
            ref Parser p = ref _addr_p.val;

            return p.resource(sectionAnswers);
        }

        // AllAnswers parses all Answer Resources.
        private static (slice<Resource>, error) AllAnswers(this ptr<Parser> _addr_p)
        {
            slice<Resource> _p0 = default;
            error _p0 = default!;
            ref Parser p = ref _addr_p.val;
 
            // The most common query is for A/AAAA, which usually returns
            // a handful of IPs.
            //
            // Pre-allocate up to a certain limit, since p.header is
            // untrusted data.
            var n = int(p.header.answers);
            if (n > 20L)
            {
                n = 20L;
            }

            var @as = make_slice<Resource>(0L, n);
            while (true)
            {
                var (a, err) = p.Answer();
                if (err == ErrSectionDone)
                {
                    return (as, error.As(null!)!);
                }

                if (err != null)
                {
                    return (null, error.As(err)!);
                }

                as = append(as, a);

            }


        }

        // SkipAnswer skips a single Answer Resource.
        private static error SkipAnswer(this ptr<Parser> _addr_p)
        {
            ref Parser p = ref _addr_p.val;

            return error.As(p.skipResource(sectionAnswers))!;
        }

        // SkipAllAnswers skips all Answer Resources.
        private static error SkipAllAnswers(this ptr<Parser> _addr_p)
        {
            ref Parser p = ref _addr_p.val;

            while (true)
            {
                {
                    var err = p.SkipAnswer();

                    if (err == ErrSectionDone)
                    {
                        return error.As(null!)!;
                    }
                    else if (err != null)
                    {
                        return error.As(err)!;
                    }


                }

            }


        }

        // AuthorityHeader parses a single Authority ResourceHeader.
        private static (ResourceHeader, error) AuthorityHeader(this ptr<Parser> _addr_p)
        {
            ResourceHeader _p0 = default;
            error _p0 = default!;
            ref Parser p = ref _addr_p.val;

            return p.resourceHeader(sectionAuthorities);
        }

        // Authority parses a single Authority Resource.
        private static (Resource, error) Authority(this ptr<Parser> _addr_p)
        {
            Resource _p0 = default;
            error _p0 = default!;
            ref Parser p = ref _addr_p.val;

            return p.resource(sectionAuthorities);
        }

        // AllAuthorities parses all Authority Resources.
        private static (slice<Resource>, error) AllAuthorities(this ptr<Parser> _addr_p)
        {
            slice<Resource> _p0 = default;
            error _p0 = default!;
            ref Parser p = ref _addr_p.val;
 
            // Authorities contains SOA in case of NXDOMAIN and friends,
            // otherwise it is empty.
            //
            // Pre-allocate up to a certain limit, since p.header is
            // untrusted data.
            var n = int(p.header.authorities);
            if (n > 10L)
            {
                n = 10L;
            }

            var @as = make_slice<Resource>(0L, n);
            while (true)
            {
                var (a, err) = p.Authority();
                if (err == ErrSectionDone)
                {
                    return (as, error.As(null!)!);
                }

                if (err != null)
                {
                    return (null, error.As(err)!);
                }

                as = append(as, a);

            }


        }

        // SkipAuthority skips a single Authority Resource.
        private static error SkipAuthority(this ptr<Parser> _addr_p)
        {
            ref Parser p = ref _addr_p.val;

            return error.As(p.skipResource(sectionAuthorities))!;
        }

        // SkipAllAuthorities skips all Authority Resources.
        private static error SkipAllAuthorities(this ptr<Parser> _addr_p)
        {
            ref Parser p = ref _addr_p.val;

            while (true)
            {
                {
                    var err = p.SkipAuthority();

                    if (err == ErrSectionDone)
                    {
                        return error.As(null!)!;
                    }
                    else if (err != null)
                    {
                        return error.As(err)!;
                    }


                }

            }


        }

        // AdditionalHeader parses a single Additional ResourceHeader.
        private static (ResourceHeader, error) AdditionalHeader(this ptr<Parser> _addr_p)
        {
            ResourceHeader _p0 = default;
            error _p0 = default!;
            ref Parser p = ref _addr_p.val;

            return p.resourceHeader(sectionAdditionals);
        }

        // Additional parses a single Additional Resource.
        private static (Resource, error) Additional(this ptr<Parser> _addr_p)
        {
            Resource _p0 = default;
            error _p0 = default!;
            ref Parser p = ref _addr_p.val;

            return p.resource(sectionAdditionals);
        }

        // AllAdditionals parses all Additional Resources.
        private static (slice<Resource>, error) AllAdditionals(this ptr<Parser> _addr_p)
        {
            slice<Resource> _p0 = default;
            error _p0 = default!;
            ref Parser p = ref _addr_p.val;
 
            // Additionals usually contain OPT, and sometimes A/AAAA
            // glue records.
            //
            // Pre-allocate up to a certain limit, since p.header is
            // untrusted data.
            var n = int(p.header.additionals);
            if (n > 10L)
            {
                n = 10L;
            }

            var @as = make_slice<Resource>(0L, n);
            while (true)
            {
                var (a, err) = p.Additional();
                if (err == ErrSectionDone)
                {
                    return (as, error.As(null!)!);
                }

                if (err != null)
                {
                    return (null, error.As(err)!);
                }

                as = append(as, a);

            }


        }

        // SkipAdditional skips a single Additional Resource.
        private static error SkipAdditional(this ptr<Parser> _addr_p)
        {
            ref Parser p = ref _addr_p.val;

            return error.As(p.skipResource(sectionAdditionals))!;
        }

        // SkipAllAdditionals skips all Additional Resources.
        private static error SkipAllAdditionals(this ptr<Parser> _addr_p)
        {
            ref Parser p = ref _addr_p.val;

            while (true)
            {
                {
                    var err = p.SkipAdditional();

                    if (err == ErrSectionDone)
                    {
                        return error.As(null!)!;
                    }
                    else if (err != null)
                    {
                        return error.As(err)!;
                    }


                }

            }


        }

        // CNAMEResource parses a single CNAMEResource.
        //
        // One of the XXXHeader methods must have been called before calling this
        // method.
        private static (CNAMEResource, error) CNAMEResource(this ptr<Parser> _addr_p)
        {
            CNAMEResource _p0 = default;
            error _p0 = default!;
            ref Parser p = ref _addr_p.val;

            if (!p.resHeaderValid || p.resHeader.Type != TypeCNAME)
            {
                return (new CNAMEResource(), error.As(ErrNotStarted)!);
            }

            var (r, err) = unpackCNAMEResource(p.msg, p.off);
            if (err != null)
            {
                return (new CNAMEResource(), error.As(err)!);
            }

            p.off += int(p.resHeader.Length);
            p.resHeaderValid = false;
            p.index++;
            return (r, error.As(null!)!);

        }

        // MXResource parses a single MXResource.
        //
        // One of the XXXHeader methods must have been called before calling this
        // method.
        private static (MXResource, error) MXResource(this ptr<Parser> _addr_p)
        {
            MXResource _p0 = default;
            error _p0 = default!;
            ref Parser p = ref _addr_p.val;

            if (!p.resHeaderValid || p.resHeader.Type != TypeMX)
            {
                return (new MXResource(), error.As(ErrNotStarted)!);
            }

            var (r, err) = unpackMXResource(p.msg, p.off);
            if (err != null)
            {
                return (new MXResource(), error.As(err)!);
            }

            p.off += int(p.resHeader.Length);
            p.resHeaderValid = false;
            p.index++;
            return (r, error.As(null!)!);

        }

        // NSResource parses a single NSResource.
        //
        // One of the XXXHeader methods must have been called before calling this
        // method.
        private static (NSResource, error) NSResource(this ptr<Parser> _addr_p)
        {
            NSResource _p0 = default;
            error _p0 = default!;
            ref Parser p = ref _addr_p.val;

            if (!p.resHeaderValid || p.resHeader.Type != TypeNS)
            {
                return (new NSResource(), error.As(ErrNotStarted)!);
            }

            var (r, err) = unpackNSResource(p.msg, p.off);
            if (err != null)
            {
                return (new NSResource(), error.As(err)!);
            }

            p.off += int(p.resHeader.Length);
            p.resHeaderValid = false;
            p.index++;
            return (r, error.As(null!)!);

        }

        // PTRResource parses a single PTRResource.
        //
        // One of the XXXHeader methods must have been called before calling this
        // method.
        private static (PTRResource, error) PTRResource(this ptr<Parser> _addr_p)
        {
            PTRResource _p0 = default;
            error _p0 = default!;
            ref Parser p = ref _addr_p.val;

            if (!p.resHeaderValid || p.resHeader.Type != TypePTR)
            {
                return (new PTRResource(), error.As(ErrNotStarted)!);
            }

            var (r, err) = unpackPTRResource(p.msg, p.off);
            if (err != null)
            {
                return (new PTRResource(), error.As(err)!);
            }

            p.off += int(p.resHeader.Length);
            p.resHeaderValid = false;
            p.index++;
            return (r, error.As(null!)!);

        }

        // SOAResource parses a single SOAResource.
        //
        // One of the XXXHeader methods must have been called before calling this
        // method.
        private static (SOAResource, error) SOAResource(this ptr<Parser> _addr_p)
        {
            SOAResource _p0 = default;
            error _p0 = default!;
            ref Parser p = ref _addr_p.val;

            if (!p.resHeaderValid || p.resHeader.Type != TypeSOA)
            {
                return (new SOAResource(), error.As(ErrNotStarted)!);
            }

            var (r, err) = unpackSOAResource(p.msg, p.off);
            if (err != null)
            {
                return (new SOAResource(), error.As(err)!);
            }

            p.off += int(p.resHeader.Length);
            p.resHeaderValid = false;
            p.index++;
            return (r, error.As(null!)!);

        }

        // TXTResource parses a single TXTResource.
        //
        // One of the XXXHeader methods must have been called before calling this
        // method.
        private static (TXTResource, error) TXTResource(this ptr<Parser> _addr_p)
        {
            TXTResource _p0 = default;
            error _p0 = default!;
            ref Parser p = ref _addr_p.val;

            if (!p.resHeaderValid || p.resHeader.Type != TypeTXT)
            {
                return (new TXTResource(), error.As(ErrNotStarted)!);
            }

            var (r, err) = unpackTXTResource(p.msg, p.off, p.resHeader.Length);
            if (err != null)
            {
                return (new TXTResource(), error.As(err)!);
            }

            p.off += int(p.resHeader.Length);
            p.resHeaderValid = false;
            p.index++;
            return (r, error.As(null!)!);

        }

        // SRVResource parses a single SRVResource.
        //
        // One of the XXXHeader methods must have been called before calling this
        // method.
        private static (SRVResource, error) SRVResource(this ptr<Parser> _addr_p)
        {
            SRVResource _p0 = default;
            error _p0 = default!;
            ref Parser p = ref _addr_p.val;

            if (!p.resHeaderValid || p.resHeader.Type != TypeSRV)
            {
                return (new SRVResource(), error.As(ErrNotStarted)!);
            }

            var (r, err) = unpackSRVResource(p.msg, p.off);
            if (err != null)
            {
                return (new SRVResource(), error.As(err)!);
            }

            p.off += int(p.resHeader.Length);
            p.resHeaderValid = false;
            p.index++;
            return (r, error.As(null!)!);

        }

        // AResource parses a single AResource.
        //
        // One of the XXXHeader methods must have been called before calling this
        // method.
        private static (AResource, error) AResource(this ptr<Parser> _addr_p)
        {
            AResource _p0 = default;
            error _p0 = default!;
            ref Parser p = ref _addr_p.val;

            if (!p.resHeaderValid || p.resHeader.Type != TypeA)
            {
                return (new AResource(), error.As(ErrNotStarted)!);
            }

            var (r, err) = unpackAResource(p.msg, p.off);
            if (err != null)
            {
                return (new AResource(), error.As(err)!);
            }

            p.off += int(p.resHeader.Length);
            p.resHeaderValid = false;
            p.index++;
            return (r, error.As(null!)!);

        }

        // AAAAResource parses a single AAAAResource.
        //
        // One of the XXXHeader methods must have been called before calling this
        // method.
        private static (AAAAResource, error) AAAAResource(this ptr<Parser> _addr_p)
        {
            AAAAResource _p0 = default;
            error _p0 = default!;
            ref Parser p = ref _addr_p.val;

            if (!p.resHeaderValid || p.resHeader.Type != TypeAAAA)
            {
                return (new AAAAResource(), error.As(ErrNotStarted)!);
            }

            var (r, err) = unpackAAAAResource(p.msg, p.off);
            if (err != null)
            {
                return (new AAAAResource(), error.As(err)!);
            }

            p.off += int(p.resHeader.Length);
            p.resHeaderValid = false;
            p.index++;
            return (r, error.As(null!)!);

        }

        // OPTResource parses a single OPTResource.
        //
        // One of the XXXHeader methods must have been called before calling this
        // method.
        private static (OPTResource, error) OPTResource(this ptr<Parser> _addr_p)
        {
            OPTResource _p0 = default;
            error _p0 = default!;
            ref Parser p = ref _addr_p.val;

            if (!p.resHeaderValid || p.resHeader.Type != TypeOPT)
            {
                return (new OPTResource(), error.As(ErrNotStarted)!);
            }

            var (r, err) = unpackOPTResource(p.msg, p.off, p.resHeader.Length);
            if (err != null)
            {
                return (new OPTResource(), error.As(err)!);
            }

            p.off += int(p.resHeader.Length);
            p.resHeaderValid = false;
            p.index++;
            return (r, error.As(null!)!);

        }

        // Unpack parses a full Message.
        private static error Unpack(this ptr<Message> _addr_m, slice<byte> msg)
        {
            ref Message m = ref _addr_m.val;

            Parser p = default;
            error err = default!;
            m.Header, err = p.Start(msg);

            if (err != null)
            {
                return error.As(err)!;
            }

            m.Questions, err = p.AllQuestions();

            if (err != null)
            {
                return error.As(err)!;
            }

            m.Answers, err = p.AllAnswers();

            if (err != null)
            {
                return error.As(err)!;
            }

            m.Authorities, err = p.AllAuthorities();

            if (err != null)
            {
                return error.As(err)!;
            }

            m.Additionals, err = p.AllAdditionals();

            if (err != null)
            {
                return error.As(err)!;
            }

            return error.As(null!)!;

        }

        // Pack packs a full Message.
        private static (slice<byte>, error) Pack(this ptr<Message> _addr_m)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref Message m = ref _addr_m.val;

            return m.AppendPack(make_slice<byte>(0L, packStartingCap));
        }

        // AppendPack is like Pack but appends the full Message to b and returns the
        // extended buffer.
        private static (slice<byte>, error) AppendPack(this ptr<Message> _addr_m, slice<byte> b)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref Message m = ref _addr_m.val;
 
            // Validate the lengths. It is very unlikely that anyone will try to
            // pack more than 65535 of any particular type, but it is possible and
            // we should fail gracefully.
            if (len(m.Questions) > int(~uint16(0L)))
            {
                return (null, error.As(errTooManyQuestions)!);
            }

            if (len(m.Answers) > int(~uint16(0L)))
            {
                return (null, error.As(errTooManyAnswers)!);
            }

            if (len(m.Authorities) > int(~uint16(0L)))
            {
                return (null, error.As(errTooManyAuthorities)!);
            }

            if (len(m.Additionals) > int(~uint16(0L)))
            {
                return (null, error.As(errTooManyAdditionals)!);
            }

            header h = default;
            h.id, h.bits = m.Header.pack();

            h.questions = uint16(len(m.Questions));
            h.answers = uint16(len(m.Answers));
            h.authorities = uint16(len(m.Authorities));
            h.additionals = uint16(len(m.Additionals));

            var compressionOff = len(b);
            var msg = h.pack(b); 

            // RFC 1035 allows (but does not require) compression for packing. RFC
            // 1035 requires unpacking implementations to support compression, so
            // unconditionally enabling it is fine.
            //
            // DNS lookups are typically done over UDP, and RFC 1035 states that UDP
            // DNS messages can be a maximum of 512 bytes long. Without compression,
            // many DNS response messages are over this limit, so enabling
            // compression will help ensure compliance.
            map compression = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, long>{};

            {
                var i__prev1 = i;

                foreach (var (__i) in m.Questions)
                {
                    i = __i;
                    error err = default!;
                    msg, err = m.Questions[i].pack(msg, compression, compressionOff);

                    if (err != null)
                    {
                        return (null, error.As(addr(new nestedError("packing Question",err))!)!);
                    }

                }

                i = i__prev1;
            }

            {
                var i__prev1 = i;

                foreach (var (__i) in m.Answers)
                {
                    i = __i;
                    err = default!;
                    msg, err = m.Answers[i].pack(msg, compression, compressionOff);

                    if (err != null)
                    {
                        return (null, error.As(addr(new nestedError("packing Answer",err))!)!);
                    }

                }

                i = i__prev1;
            }

            {
                var i__prev1 = i;

                foreach (var (__i) in m.Authorities)
                {
                    i = __i;
                    err = default!;
                    msg, err = m.Authorities[i].pack(msg, compression, compressionOff);

                    if (err != null)
                    {
                        return (null, error.As(addr(new nestedError("packing Authority",err))!)!);
                    }

                }

                i = i__prev1;
            }

            {
                var i__prev1 = i;

                foreach (var (__i) in m.Additionals)
                {
                    i = __i;
                    err = default!;
                    msg, err = m.Additionals[i].pack(msg, compression, compressionOff);

                    if (err != null)
                    {
                        return (null, error.As(addr(new nestedError("packing Additional",err))!)!);
                    }

                }

                i = i__prev1;
            }

            return (msg, error.As(null!)!);

        }

        // GoString implements fmt.GoStringer.GoString.
        private static @string GoString(this ptr<Message> _addr_m)
        {
            ref Message m = ref _addr_m.val;

            @string s = "dnsmessage.Message{Header: " + m.Header.GoString() + ", " + "Questions: []dnsmessage.Question{";
            if (len(m.Questions) > 0L)
            {
                s += m.Questions[0L].GoString();
                foreach (var (_, q) in m.Questions[1L..])
                {
                    s += ", " + q.GoString();
                }

            }

            s += "}, Answers: []dnsmessage.Resource{";
            if (len(m.Answers) > 0L)
            {
                s += m.Answers[0L].GoString();
                {
                    var a__prev1 = a;

                    foreach (var (_, __a) in m.Answers[1L..])
                    {
                        a = __a;
                        s += ", " + a.GoString();
                    }

                    a = a__prev1;
                }
            }

            s += "}, Authorities: []dnsmessage.Resource{";
            if (len(m.Authorities) > 0L)
            {
                s += m.Authorities[0L].GoString();
                {
                    var a__prev1 = a;

                    foreach (var (_, __a) in m.Authorities[1L..])
                    {
                        a = __a;
                        s += ", " + a.GoString();
                    }

                    a = a__prev1;
                }
            }

            s += "}, Additionals: []dnsmessage.Resource{";
            if (len(m.Additionals) > 0L)
            {
                s += m.Additionals[0L].GoString();
                {
                    var a__prev1 = a;

                    foreach (var (_, __a) in m.Additionals[1L..])
                    {
                        a = __a;
                        s += ", " + a.GoString();
                    }

                    a = a__prev1;
                }
            }

            return s + "}}";

        }

        // A Builder allows incrementally packing a DNS message.
        //
        // Example usage:
        //    buf := make([]byte, 2, 514)
        //    b := NewBuilder(buf, Header{...})
        //    b.EnableCompression()
        //    // Optionally start a section and add things to that section.
        //    // Repeat adding sections as necessary.
        //    buf, err := b.Finish()
        //    // If err is nil, buf[2:] will contain the built bytes.
        public partial struct Builder
        {
            public slice<byte> msg; // section keeps track of the current section being built.
            public section section; // header keeps track of what should go in the header when Finish is
// called.
            public header header; // start is the starting index of the bytes allocated in msg for header.
            public long start; // compression is a mapping from name suffixes to their starting index
// in msg.
            public map<@string, long> compression;
        }

        // NewBuilder creates a new builder with compression disabled.
        //
        // Note: Most users will want to immediately enable compression with the
        // EnableCompression method. See that method's comment for why you may or may
        // not want to enable compression.
        //
        // The DNS message is appended to the provided initial buffer buf (which may be
        // nil) as it is built. The final message is returned by the (*Builder).Finish
        // method, which may return the same underlying array if there was sufficient
        // capacity in the slice.
        public static Builder NewBuilder(slice<byte> buf, Header h)
        {
            if (buf == null)
            {
                buf = make_slice<byte>(0L, packStartingCap);
            }

            Builder b = new Builder(msg:buf,start:len(buf));
            b.header.id, b.header.bits = h.pack();
            array<byte> hb = new array<byte>(headerLen);
            b.msg = append(b.msg, hb[..]);
            b.section = sectionHeader;
            return b;

        }

        // EnableCompression enables compression in the Builder.
        //
        // Leaving compression disabled avoids compression related allocations, but can
        // result in larger message sizes. Be careful with this mode as it can cause
        // messages to exceed the UDP size limit.
        //
        // According to RFC 1035, section 4.1.4, the use of compression is optional, but
        // all implementations must accept both compressed and uncompressed DNS
        // messages.
        //
        // Compression should be enabled before any sections are added for best results.
        private static void EnableCompression(this ptr<Builder> _addr_b)
        {
            ref Builder b = ref _addr_b.val;

            b.compression = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, long>{};
        }

        private static error startCheck(this ptr<Builder> _addr_b, section s)
        {
            ref Builder b = ref _addr_b.val;

            if (b.section <= sectionNotStarted)
            {
                return error.As(ErrNotStarted)!;
            }

            if (b.section > s)
            {
                return error.As(ErrSectionDone)!;
            }

            return error.As(null!)!;

        }

        // StartQuestions prepares the builder for packing Questions.
        private static error StartQuestions(this ptr<Builder> _addr_b)
        {
            ref Builder b = ref _addr_b.val;

            {
                var err = b.startCheck(sectionQuestions);

                if (err != null)
                {
                    return error.As(err)!;
                }

            }

            b.section = sectionQuestions;
            return error.As(null!)!;

        }

        // StartAnswers prepares the builder for packing Answers.
        private static error StartAnswers(this ptr<Builder> _addr_b)
        {
            ref Builder b = ref _addr_b.val;

            {
                var err = b.startCheck(sectionAnswers);

                if (err != null)
                {
                    return error.As(err)!;
                }

            }

            b.section = sectionAnswers;
            return error.As(null!)!;

        }

        // StartAuthorities prepares the builder for packing Authorities.
        private static error StartAuthorities(this ptr<Builder> _addr_b)
        {
            ref Builder b = ref _addr_b.val;

            {
                var err = b.startCheck(sectionAuthorities);

                if (err != null)
                {
                    return error.As(err)!;
                }

            }

            b.section = sectionAuthorities;
            return error.As(null!)!;

        }

        // StartAdditionals prepares the builder for packing Additionals.
        private static error StartAdditionals(this ptr<Builder> _addr_b)
        {
            ref Builder b = ref _addr_b.val;

            {
                var err = b.startCheck(sectionAdditionals);

                if (err != null)
                {
                    return error.As(err)!;
                }

            }

            b.section = sectionAdditionals;
            return error.As(null!)!;

        }

        private static error incrementSectionCount(this ptr<Builder> _addr_b)
        {
            ref Builder b = ref _addr_b.val;

            ptr<ushort> count;
            error err = default!;

            if (b.section == sectionQuestions) 
                count = _addr_b.header.questions;
                err = error.As(errTooManyQuestions)!;
            else if (b.section == sectionAnswers) 
                count = _addr_b.header.answers;
                err = error.As(errTooManyAnswers)!;
            else if (b.section == sectionAuthorities) 
                count = _addr_b.header.authorities;
                err = error.As(errTooManyAuthorities)!;
            else if (b.section == sectionAdditionals) 
                count = _addr_b.header.additionals;
                err = error.As(errTooManyAdditionals)!;
                        if (count == ~uint16(0L).val)
            {
                return error.As(err)!;
            }

            count.val++;
            return error.As(null!)!;

        }

        // Question adds a single Question.
        private static error Question(this ptr<Builder> _addr_b, Question q)
        {
            ref Builder b = ref _addr_b.val;

            if (b.section < sectionQuestions)
            {
                return error.As(ErrNotStarted)!;
            }

            if (b.section > sectionQuestions)
            {
                return error.As(ErrSectionDone)!;
            }

            var (msg, err) = q.pack(b.msg, b.compression, b.start);
            if (err != null)
            {
                return error.As(err)!;
            }

            {
                var err = b.incrementSectionCount();

                if (err != null)
                {
                    return error.As(err)!;
                }

            }

            b.msg = msg;
            return error.As(null!)!;

        }

        private static error checkResourceSection(this ptr<Builder> _addr_b)
        {
            ref Builder b = ref _addr_b.val;

            if (b.section < sectionAnswers)
            {
                return error.As(ErrNotStarted)!;
            }

            if (b.section > sectionAdditionals)
            {
                return error.As(ErrSectionDone)!;
            }

            return error.As(null!)!;

        }

        // CNAMEResource adds a single CNAMEResource.
        private static error CNAMEResource(this ptr<Builder> _addr_b, ResourceHeader h, CNAMEResource r)
        {
            ref Builder b = ref _addr_b.val;

            {
                var err__prev1 = err;

                var err = b.checkResourceSection();

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            h.Type = r.realType();
            var (msg, lenOff, err) = h.pack(b.msg, b.compression, b.start);
            if (err != null)
            {
                return error.As(addr(new nestedError("ResourceHeader",err))!)!;
            }

            var preLen = len(msg);
            msg, err = r.pack(msg, b.compression, b.start);

            if (err != null)
            {
                return error.As(addr(new nestedError("CNAMEResource body",err))!)!;
            }

            {
                var err__prev1 = err;

                err = h.fixLen(msg, lenOff, preLen);

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            {
                var err__prev1 = err;

                err = b.incrementSectionCount();

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            b.msg = msg;
            return error.As(null!)!;

        }

        // MXResource adds a single MXResource.
        private static error MXResource(this ptr<Builder> _addr_b, ResourceHeader h, MXResource r)
        {
            ref Builder b = ref _addr_b.val;

            {
                var err__prev1 = err;

                var err = b.checkResourceSection();

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            h.Type = r.realType();
            var (msg, lenOff, err) = h.pack(b.msg, b.compression, b.start);
            if (err != null)
            {
                return error.As(addr(new nestedError("ResourceHeader",err))!)!;
            }

            var preLen = len(msg);
            msg, err = r.pack(msg, b.compression, b.start);

            if (err != null)
            {
                return error.As(addr(new nestedError("MXResource body",err))!)!;
            }

            {
                var err__prev1 = err;

                err = h.fixLen(msg, lenOff, preLen);

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            {
                var err__prev1 = err;

                err = b.incrementSectionCount();

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            b.msg = msg;
            return error.As(null!)!;

        }

        // NSResource adds a single NSResource.
        private static error NSResource(this ptr<Builder> _addr_b, ResourceHeader h, NSResource r)
        {
            ref Builder b = ref _addr_b.val;

            {
                var err__prev1 = err;

                var err = b.checkResourceSection();

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            h.Type = r.realType();
            var (msg, lenOff, err) = h.pack(b.msg, b.compression, b.start);
            if (err != null)
            {
                return error.As(addr(new nestedError("ResourceHeader",err))!)!;
            }

            var preLen = len(msg);
            msg, err = r.pack(msg, b.compression, b.start);

            if (err != null)
            {
                return error.As(addr(new nestedError("NSResource body",err))!)!;
            }

            {
                var err__prev1 = err;

                err = h.fixLen(msg, lenOff, preLen);

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            {
                var err__prev1 = err;

                err = b.incrementSectionCount();

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            b.msg = msg;
            return error.As(null!)!;

        }

        // PTRResource adds a single PTRResource.
        private static error PTRResource(this ptr<Builder> _addr_b, ResourceHeader h, PTRResource r)
        {
            ref Builder b = ref _addr_b.val;

            {
                var err__prev1 = err;

                var err = b.checkResourceSection();

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            h.Type = r.realType();
            var (msg, lenOff, err) = h.pack(b.msg, b.compression, b.start);
            if (err != null)
            {
                return error.As(addr(new nestedError("ResourceHeader",err))!)!;
            }

            var preLen = len(msg);
            msg, err = r.pack(msg, b.compression, b.start);

            if (err != null)
            {
                return error.As(addr(new nestedError("PTRResource body",err))!)!;
            }

            {
                var err__prev1 = err;

                err = h.fixLen(msg, lenOff, preLen);

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            {
                var err__prev1 = err;

                err = b.incrementSectionCount();

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            b.msg = msg;
            return error.As(null!)!;

        }

        // SOAResource adds a single SOAResource.
        private static error SOAResource(this ptr<Builder> _addr_b, ResourceHeader h, SOAResource r)
        {
            ref Builder b = ref _addr_b.val;

            {
                var err__prev1 = err;

                var err = b.checkResourceSection();

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            h.Type = r.realType();
            var (msg, lenOff, err) = h.pack(b.msg, b.compression, b.start);
            if (err != null)
            {
                return error.As(addr(new nestedError("ResourceHeader",err))!)!;
            }

            var preLen = len(msg);
            msg, err = r.pack(msg, b.compression, b.start);

            if (err != null)
            {
                return error.As(addr(new nestedError("SOAResource body",err))!)!;
            }

            {
                var err__prev1 = err;

                err = h.fixLen(msg, lenOff, preLen);

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            {
                var err__prev1 = err;

                err = b.incrementSectionCount();

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            b.msg = msg;
            return error.As(null!)!;

        }

        // TXTResource adds a single TXTResource.
        private static error TXTResource(this ptr<Builder> _addr_b, ResourceHeader h, TXTResource r)
        {
            ref Builder b = ref _addr_b.val;

            {
                var err__prev1 = err;

                var err = b.checkResourceSection();

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            h.Type = r.realType();
            var (msg, lenOff, err) = h.pack(b.msg, b.compression, b.start);
            if (err != null)
            {
                return error.As(addr(new nestedError("ResourceHeader",err))!)!;
            }

            var preLen = len(msg);
            msg, err = r.pack(msg, b.compression, b.start);

            if (err != null)
            {
                return error.As(addr(new nestedError("TXTResource body",err))!)!;
            }

            {
                var err__prev1 = err;

                err = h.fixLen(msg, lenOff, preLen);

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            {
                var err__prev1 = err;

                err = b.incrementSectionCount();

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            b.msg = msg;
            return error.As(null!)!;

        }

        // SRVResource adds a single SRVResource.
        private static error SRVResource(this ptr<Builder> _addr_b, ResourceHeader h, SRVResource r)
        {
            ref Builder b = ref _addr_b.val;

            {
                var err__prev1 = err;

                var err = b.checkResourceSection();

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            h.Type = r.realType();
            var (msg, lenOff, err) = h.pack(b.msg, b.compression, b.start);
            if (err != null)
            {
                return error.As(addr(new nestedError("ResourceHeader",err))!)!;
            }

            var preLen = len(msg);
            msg, err = r.pack(msg, b.compression, b.start);

            if (err != null)
            {
                return error.As(addr(new nestedError("SRVResource body",err))!)!;
            }

            {
                var err__prev1 = err;

                err = h.fixLen(msg, lenOff, preLen);

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            {
                var err__prev1 = err;

                err = b.incrementSectionCount();

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            b.msg = msg;
            return error.As(null!)!;

        }

        // AResource adds a single AResource.
        private static error AResource(this ptr<Builder> _addr_b, ResourceHeader h, AResource r)
        {
            ref Builder b = ref _addr_b.val;

            {
                var err__prev1 = err;

                var err = b.checkResourceSection();

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            h.Type = r.realType();
            var (msg, lenOff, err) = h.pack(b.msg, b.compression, b.start);
            if (err != null)
            {
                return error.As(addr(new nestedError("ResourceHeader",err))!)!;
            }

            var preLen = len(msg);
            msg, err = r.pack(msg, b.compression, b.start);

            if (err != null)
            {
                return error.As(addr(new nestedError("AResource body",err))!)!;
            }

            {
                var err__prev1 = err;

                err = h.fixLen(msg, lenOff, preLen);

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            {
                var err__prev1 = err;

                err = b.incrementSectionCount();

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            b.msg = msg;
            return error.As(null!)!;

        }

        // AAAAResource adds a single AAAAResource.
        private static error AAAAResource(this ptr<Builder> _addr_b, ResourceHeader h, AAAAResource r)
        {
            ref Builder b = ref _addr_b.val;

            {
                var err__prev1 = err;

                var err = b.checkResourceSection();

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            h.Type = r.realType();
            var (msg, lenOff, err) = h.pack(b.msg, b.compression, b.start);
            if (err != null)
            {
                return error.As(addr(new nestedError("ResourceHeader",err))!)!;
            }

            var preLen = len(msg);
            msg, err = r.pack(msg, b.compression, b.start);

            if (err != null)
            {
                return error.As(addr(new nestedError("AAAAResource body",err))!)!;
            }

            {
                var err__prev1 = err;

                err = h.fixLen(msg, lenOff, preLen);

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            {
                var err__prev1 = err;

                err = b.incrementSectionCount();

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            b.msg = msg;
            return error.As(null!)!;

        }

        // OPTResource adds a single OPTResource.
        private static error OPTResource(this ptr<Builder> _addr_b, ResourceHeader h, OPTResource r)
        {
            ref Builder b = ref _addr_b.val;

            {
                var err__prev1 = err;

                var err = b.checkResourceSection();

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            h.Type = r.realType();
            var (msg, lenOff, err) = h.pack(b.msg, b.compression, b.start);
            if (err != null)
            {
                return error.As(addr(new nestedError("ResourceHeader",err))!)!;
            }

            var preLen = len(msg);
            msg, err = r.pack(msg, b.compression, b.start);

            if (err != null)
            {
                return error.As(addr(new nestedError("OPTResource body",err))!)!;
            }

            {
                var err__prev1 = err;

                err = h.fixLen(msg, lenOff, preLen);

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            {
                var err__prev1 = err;

                err = b.incrementSectionCount();

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            b.msg = msg;
            return error.As(null!)!;

        }

        // Finish ends message building and generates a binary message.
        private static (slice<byte>, error) Finish(this ptr<Builder> _addr_b)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref Builder b = ref _addr_b.val;

            if (b.section < sectionHeader)
            {
                return (null, error.As(ErrNotStarted)!);
            }

            b.section = sectionDone; 
            // Space for the header was allocated in NewBuilder.
            b.header.pack(b.msg[b.start..b.start]);
            return (b.msg, error.As(null!)!);

        }

        // A ResourceHeader is the header of a DNS resource record. There are
        // many types of DNS resource records, but they all share the same header.
        public partial struct ResourceHeader
        {
            public Name Name; // Type is the type of DNS resource record.
//
// This field will be set automatically during packing.
            public Type Type; // Class is the class of network to which this DNS resource record
// pertains.
            public Class Class; // TTL is the length of time (measured in seconds) which this resource
// record is valid for (time to live). All Resources in a set should
// have the same TTL (RFC 2181 Section 5.2).
            public uint TTL; // Length is the length of data in the resource record after the header.
//
// This field will be set automatically during packing.
            public ushort Length;
        }

        // GoString implements fmt.GoStringer.GoString.
        private static @string GoString(this ptr<ResourceHeader> _addr_h)
        {
            ref ResourceHeader h = ref _addr_h.val;

            return "dnsmessage.ResourceHeader{" + "Name: " + h.Name.GoString() + ", " + "Type: " + h.Type.GoString() + ", " + "Class: " + h.Class.GoString() + ", " + "TTL: " + printUint32(h.TTL) + ", " + "Length: " + printUint16(h.Length) + "}";
        }

        // pack appends the wire format of the ResourceHeader to oldMsg.
        //
        // lenOff is the offset in msg where the Length field was packed.
        private static (slice<byte>, long, error) pack(this ptr<ResourceHeader> _addr_h, slice<byte> oldMsg, map<@string, long> compression, long compressionOff)
        {
            slice<byte> msg = default;
            long lenOff = default;
            error err = default!;
            ref ResourceHeader h = ref _addr_h.val;

            msg = oldMsg;
            msg, err = h.Name.pack(msg, compression, compressionOff);

            if (err != null)
            {
                return (oldMsg, 0L, error.As(addr(new nestedError("Name",err))!)!);
            }

            msg = packType(msg, h.Type);
            msg = packClass(msg, h.Class);
            msg = packUint32(msg, h.TTL);
            lenOff = len(msg);
            msg = packUint16(msg, h.Length);
            return (msg, lenOff, error.As(null!)!);

        }

        private static (long, error) unpack(this ptr<ResourceHeader> _addr_h, slice<byte> msg, long off)
        {
            long _p0 = default;
            error _p0 = default!;
            ref ResourceHeader h = ref _addr_h.val;

            var newOff = off;
            error err = default!;
            newOff, err = h.Name.unpack(msg, newOff);

            if (err != null)
            {
                return (off, error.As(addr(new nestedError("Name",err))!)!);
            }

            h.Type, newOff, err = unpackType(msg, newOff);

            if (err != null)
            {
                return (off, error.As(addr(new nestedError("Type",err))!)!);
            }

            h.Class, newOff, err = unpackClass(msg, newOff);

            if (err != null)
            {
                return (off, error.As(addr(new nestedError("Class",err))!)!);
            }

            h.TTL, newOff, err = unpackUint32(msg, newOff);

            if (err != null)
            {
                return (off, error.As(addr(new nestedError("TTL",err))!)!);
            }

            h.Length, newOff, err = unpackUint16(msg, newOff);

            if (err != null)
            {
                return (off, error.As(addr(new nestedError("Length",err))!)!);
            }

            return (newOff, error.As(null!)!);

        }

        // fixLen updates a packed ResourceHeader to include the length of the
        // ResourceBody.
        //
        // lenOff is the offset of the ResourceHeader.Length field in msg.
        //
        // preLen is the length that msg was before the ResourceBody was packed.
        private static error fixLen(this ptr<ResourceHeader> _addr_h, slice<byte> msg, long lenOff, long preLen)
        {
            ref ResourceHeader h = ref _addr_h.val;

            var conLen = len(msg) - preLen;
            if (conLen > int(~uint16(0L)))
            {
                return error.As(errResTooLong)!;
            } 

            // Fill in the length now that we know how long the content is.
            packUint16(msg[lenOff..lenOff], uint16(conLen));
            h.Length = uint16(conLen);

            return error.As(null!)!;

        }

        // EDNS(0) wire constants.
        private static readonly long edns0Version = (long)0L;

        private static readonly ulong edns0DNSSECOK = (ulong)0x00008000UL;
        private static readonly ulong ednsVersionMask = (ulong)0x00ff0000UL;
        private static readonly ulong edns0DNSSECOKMask = (ulong)0x00ff8000UL;


        // SetEDNS0 configures h for EDNS(0).
        //
        // The provided extRCode must be an extedned RCode.
        private static error SetEDNS0(this ptr<ResourceHeader> _addr_h, long udpPayloadLen, RCode extRCode, bool dnssecOK)
        {
            ref ResourceHeader h = ref _addr_h.val;

            h.Name = new Name(Data:[nameLen]byte{'.'},Length:1); // RFC 6891 section 6.1.2
            h.Type = TypeOPT;
            h.Class = Class(udpPayloadLen);
            h.TTL = uint32(extRCode) >> (int)(4L) << (int)(24L);
            if (dnssecOK)
            {
                h.TTL |= edns0DNSSECOK;
            }

            return error.As(null!)!;

        }

        // DNSSECAllowed reports whether the DNSSEC OK bit is set.
        private static bool DNSSECAllowed(this ptr<ResourceHeader> _addr_h)
        {
            ref ResourceHeader h = ref _addr_h.val;

            return h.TTL & edns0DNSSECOKMask == edns0DNSSECOK; // RFC 6891 section 6.1.3
        }

        // ExtendedRCode returns an extended RCode.
        //
        // The provided rcode must be the RCode in DNS message header.
        private static RCode ExtendedRCode(this ptr<ResourceHeader> _addr_h, RCode rcode)
        {
            ref ResourceHeader h = ref _addr_h.val;

            if (h.TTL & ednsVersionMask == edns0Version)
            { // RFC 6891 section 6.1.3
                return RCode(h.TTL >> (int)(24L) << (int)(4L)) | rcode;

            }

            return rcode;

        }

        private static (long, error) skipResource(slice<byte> msg, long off)
        {
            long _p0 = default;
            error _p0 = default!;

            var (newOff, err) = skipName(msg, off);
            if (err != null)
            {
                return (off, error.As(addr(new nestedError("Name",err))!)!);
            }

            newOff, err = skipType(msg, newOff);

            if (err != null)
            {
                return (off, error.As(addr(new nestedError("Type",err))!)!);
            }

            newOff, err = skipClass(msg, newOff);

            if (err != null)
            {
                return (off, error.As(addr(new nestedError("Class",err))!)!);
            }

            newOff, err = skipUint32(msg, newOff);

            if (err != null)
            {
                return (off, error.As(addr(new nestedError("TTL",err))!)!);
            }

            var (length, newOff, err) = unpackUint16(msg, newOff);
            if (err != null)
            {
                return (off, error.As(addr(new nestedError("Length",err))!)!);
            }

            newOff += int(length);

            if (newOff > len(msg))
            {
                return (off, error.As(errResourceLen)!);
            }

            return (newOff, error.As(null!)!);

        }

        // packUint16 appends the wire format of field to msg.
        private static slice<byte> packUint16(slice<byte> msg, ushort field)
        {
            return append(msg, byte(field >> (int)(8L)), byte(field));
        }

        private static (ushort, long, error) unpackUint16(slice<byte> msg, long off)
        {
            ushort _p0 = default;
            long _p0 = default;
            error _p0 = default!;

            if (off + uint16Len > len(msg))
            {
                return (0L, off, error.As(errBaseLen)!);
            }

            return (uint16(msg[off]) << (int)(8L) | uint16(msg[off + 1L]), off + uint16Len, error.As(null!)!);

        }

        private static (long, error) skipUint16(slice<byte> msg, long off)
        {
            long _p0 = default;
            error _p0 = default!;

            if (off + uint16Len > len(msg))
            {
                return (off, error.As(errBaseLen)!);
            }

            return (off + uint16Len, error.As(null!)!);

        }

        // packType appends the wire format of field to msg.
        private static slice<byte> packType(slice<byte> msg, Type field)
        {
            return packUint16(msg, uint16(field));
        }

        private static (Type, long, error) unpackType(slice<byte> msg, long off)
        {
            Type _p0 = default;
            long _p0 = default;
            error _p0 = default!;

            var (t, o, err) = unpackUint16(msg, off);
            return (Type(t), o, error.As(err)!);
        }

        private static (long, error) skipType(slice<byte> msg, long off)
        {
            long _p0 = default;
            error _p0 = default!;

            return skipUint16(msg, off);
        }

        // packClass appends the wire format of field to msg.
        private static slice<byte> packClass(slice<byte> msg, Class field)
        {
            return packUint16(msg, uint16(field));
        }

        private static (Class, long, error) unpackClass(slice<byte> msg, long off)
        {
            Class _p0 = default;
            long _p0 = default;
            error _p0 = default!;

            var (c, o, err) = unpackUint16(msg, off);
            return (Class(c), o, error.As(err)!);
        }

        private static (long, error) skipClass(slice<byte> msg, long off)
        {
            long _p0 = default;
            error _p0 = default!;

            return skipUint16(msg, off);
        }

        // packUint32 appends the wire format of field to msg.
        private static slice<byte> packUint32(slice<byte> msg, uint field)
        {
            return append(msg, byte(field >> (int)(24L)), byte(field >> (int)(16L)), byte(field >> (int)(8L)), byte(field));
        }

        private static (uint, long, error) unpackUint32(slice<byte> msg, long off)
        {
            uint _p0 = default;
            long _p0 = default;
            error _p0 = default!;

            if (off + uint32Len > len(msg))
            {
                return (0L, off, error.As(errBaseLen)!);
            }

            var v = uint32(msg[off]) << (int)(24L) | uint32(msg[off + 1L]) << (int)(16L) | uint32(msg[off + 2L]) << (int)(8L) | uint32(msg[off + 3L]);
            return (v, off + uint32Len, error.As(null!)!);

        }

        private static (long, error) skipUint32(slice<byte> msg, long off)
        {
            long _p0 = default;
            error _p0 = default!;

            if (off + uint32Len > len(msg))
            {
                return (off, error.As(errBaseLen)!);
            }

            return (off + uint32Len, error.As(null!)!);

        }

        // packText appends the wire format of field to msg.
        private static (slice<byte>, error) packText(slice<byte> msg, @string field)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;

            var l = len(field);
            if (l > 255L)
            {
                return (null, error.As(errStringTooLong)!);
            }

            msg = append(msg, byte(l));
            msg = append(msg, field);

            return (msg, error.As(null!)!);

        }

        private static (@string, long, error) unpackText(slice<byte> msg, long off)
        {
            @string _p0 = default;
            long _p0 = default;
            error _p0 = default!;

            if (off >= len(msg))
            {
                return ("", off, error.As(errBaseLen)!);
            }

            var beginOff = off + 1L;
            var endOff = beginOff + int(msg[off]);
            if (endOff > len(msg))
            {
                return ("", off, error.As(errCalcLen)!);
            }

            return (string(msg[beginOff..endOff]), endOff, error.As(null!)!);

        }

        // packBytes appends the wire format of field to msg.
        private static slice<byte> packBytes(slice<byte> msg, slice<byte> field)
        {
            return append(msg, field);
        }

        private static (long, error) unpackBytes(slice<byte> msg, long off, slice<byte> field)
        {
            long _p0 = default;
            error _p0 = default!;

            var newOff = off + len(field);
            if (newOff > len(msg))
            {
                return (off, error.As(errBaseLen)!);
            }

            copy(field, msg[off..newOff]);
            return (newOff, error.As(null!)!);

        }

        private static readonly long nameLen = (long)255L;

        // A Name is a non-encoded domain name. It is used instead of strings to avoid
        // allocations.


        // A Name is a non-encoded domain name. It is used instead of strings to avoid
        // allocations.
        public partial struct Name
        {
            public array<byte> Data;
            public byte Length;
        }

        // NewName creates a new Name from a string.
        public static (Name, error) NewName(@string name)
        {
            Name _p0 = default;
            error _p0 = default!;

            if (len((slice<byte>)name) > nameLen)
            {
                return (new Name(), error.As(errCalcLen)!);
            }

            Name n = new Name(Length:uint8(len(name)));
            copy(n.Data[..], (slice<byte>)name);
            return (n, error.As(null!)!);

        }

        // MustNewName creates a new Name from a string and panics on error.
        public static Name MustNewName(@string name) => func((_, panic, __) =>
        {
            var (n, err) = NewName(name);
            if (err != null)
            {
                panic("creating name: " + err.Error());
            }

            return n;

        });

        // String implements fmt.Stringer.String.
        public static @string String(this Name n)
        {
            return string(n.Data[..n.Length]);
        }

        // GoString implements fmt.GoStringer.GoString.
        private static @string GoString(this ptr<Name> _addr_n)
        {
            ref Name n = ref _addr_n.val;

            return "dnsmessage.MustNewName(\"" + printString(n.Data[..n.Length]) + "\")";
        }

        // pack appends the wire format of the Name to msg.
        //
        // Domain names are a sequence of counted strings split at the dots. They end
        // with a zero-length string. Compression can be used to reuse domain suffixes.
        //
        // The compression map will be updated with new domain suffixes. If compression
        // is nil, compression will not be used.
        private static (slice<byte>, error) pack(this ptr<Name> _addr_n, slice<byte> msg, map<@string, long> compression, long compressionOff)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref Name n = ref _addr_n.val;

            var oldMsg = msg; 

            // Add a trailing dot to canonicalize name.
            if (n.Length == 0L || n.Data[n.Length - 1L] != '.')
            {
                return (oldMsg, error.As(errNonCanonicalName)!);
            } 

            // Allow root domain.
            if (n.Data[0L] == '.' && n.Length == 1L)
            {
                return (append(msg, 0L), error.As(null!)!);
            } 

            // Emit sequence of counted strings, chopping at dots.
            for (long i = 0L;
            long begin = 0L; i < int(n.Length); i++)
            { 
                // Check for the end of the segment.
                if (n.Data[i] == '.')
                { 
                    // The two most significant bits have special meaning.
                    // It isn't allowed for segments to be long enough to
                    // need them.
                    if (i - begin >= 1L << (int)(6L))
                    {
                        return (oldMsg, error.As(errSegTooLong)!);
                    } 

                    // Segments must have a non-zero length.
                    if (i - begin == 0L)
                    {
                        return (oldMsg, error.As(errZeroSegLen)!);
                    }

                    msg = append(msg, byte(i - begin));

                    for (var j = begin; j < i; j++)
                    {
                        msg = append(msg, n.Data[j]);
                    }


                    begin = i + 1L;
                    continue;

                } 

                // We can only compress domain suffixes starting with a new
                // segment. A pointer is two bytes with the two most significant
                // bits set to 1 to indicate that it is a pointer.
                if ((i == 0L || n.Data[i - 1L] == '.') && compression != null)
                {
                    {
                        var (ptr, ok) = compression[string(n.Data[i..])];

                        if (ok)
                        { 
                            // Hit. Emit a pointer instead of the rest of
                            // the domain.
                            return (append(msg, byte(ptr >> (int)(8L) | 0xC0UL), byte(ptr)), error.As(null!)!);

                        } 

                        // Miss. Add the suffix to the compression table if the
                        // offset can be stored in the available 14 bytes.

                    } 

                    // Miss. Add the suffix to the compression table if the
                    // offset can be stored in the available 14 bytes.
                    if (len(msg) <= int(~uint16(0L) >> (int)(2L)))
                    {
                        compression[string(n.Data[i..])] = len(msg) - compressionOff;
                    }

                }

            }

            return (append(msg, 0L), error.As(null!)!);

        }

        // unpack unpacks a domain name.
        private static (long, error) unpack(this ptr<Name> _addr_n, slice<byte> msg, long off)
        {
            long _p0 = default;
            error _p0 = default!;
            ref Name n = ref _addr_n.val;

            return n.unpackCompressed(msg, off, true);
        }

        private static (long, error) unpackCompressed(this ptr<Name> _addr_n, slice<byte> msg, long off, bool allowCompression)
        {
            long _p0 = default;
            error _p0 = default!;
            ref Name n = ref _addr_n.val;
 
            // currOff is the current working offset.
            var currOff = off; 

            // newOff is the offset where the next record will start. Pointers lead
            // to data that belongs to other names and thus doesn't count towards to
            // the usage of this name.
            var newOff = off; 

            // ptr is the number of pointers followed.
            long ptr = default; 

            // Name is a slice representation of the name data.
            var name = n.Data[..0L];

Loop:
            while (true)
            {
                if (currOff >= len(msg))
                {
                    return (off, error.As(errBaseLen)!);
                }

                var c = int(msg[currOff]);
                currOff++;
                switch (c & 0xC0UL)
                {
                    case 0x00UL: // String segment
                        if (c == 0x00UL)
                        { 
                            // A zero length signals the end of the name.
                            _breakLoop = true;
                            break;
                        }

                        var endOff = currOff + c;
                        if (endOff > len(msg))
                        {
                            return (off, error.As(errCalcLen)!);
                        }

                        name = append(name, msg[currOff..endOff]);
                        name = append(name, '.');
                        currOff = endOff;
                        break;
                    case 0xC0UL: // Pointer
                        if (!allowCompression)
                        {
                            return (off, error.As(errCompressedSRV)!);
                        }

                        if (currOff >= len(msg))
                        {
                            return (off, error.As(errInvalidPtr)!);
                        }

                        var c1 = msg[currOff];
                        currOff++;
                        if (ptr == 0L)
                        {
                            newOff = currOff;
                        } 
                        // Don't follow too many pointers, maybe there's a loop.
                        ptr++;

                        if (ptr > 10L)
                        {
                            return (off, error.As(errTooManyPtr)!);
                        }

                        currOff = (c ^ 0xC0UL) << (int)(8L) | int(c1);
                        break;
                    default: 
                        // Prefixes 0x80 and 0x40 are reserved.
                        return (off, error.As(errReserved)!);
                        break;
                }

            }
            if (len(name) == 0L)
            {
                name = append(name, '.');
            }

            if (len(name) > len(n.Data))
            {
                return (off, error.As(errCalcLen)!);
            }

            n.Length = uint8(len(name));
            if (ptr == 0L)
            {
                newOff = currOff;
            }

            return (newOff, error.As(null!)!);

        }

        private static (long, error) skipName(slice<byte> msg, long off)
        {
            long _p0 = default;
            error _p0 = default!;
 
            // newOff is the offset where the next record will start. Pointers lead
            // to data that belongs to other names and thus doesn't count towards to
            // the usage of this name.
            var newOff = off;

Loop:

            while (true)
            {
                if (newOff >= len(msg))
                {
                    return (off, error.As(errBaseLen)!);
                }

                var c = int(msg[newOff]);
                newOff++;
                switch (c & 0xC0UL)
                {
                    case 0x00UL: 
                        if (c == 0x00UL)
                        { 
                            // A zero length signals the end of the name.
                            _breakLoop = true;
                            break;
                        } 
                        // literal string
                        newOff += c;
                        if (newOff > len(msg))
                        {
                            return (off, error.As(errCalcLen)!);
                        }

                        break;
                    case 0xC0UL: 
                        // Pointer to somewhere else in msg.

                        // Pointers are two bytes.
                        newOff++; 

                        // Don't follow the pointer as the data here has ended.
                        _breakLoop = true;
                        break;
                        break;
                    default: 
                        // Prefixes 0x80 and 0x40 are reserved.
                        return (off, error.As(errReserved)!);
                        break;
                }

            }

            return (newOff, error.As(null!)!);

        }

        // A Question is a DNS query.
        public partial struct Question
        {
            public Name Name;
            public Type Type;
            public Class Class;
        }

        // pack appends the wire format of the Question to msg.
        private static (slice<byte>, error) pack(this ptr<Question> _addr_q, slice<byte> msg, map<@string, long> compression, long compressionOff)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref Question q = ref _addr_q.val;

            var (msg, err) = q.Name.pack(msg, compression, compressionOff);
            if (err != null)
            {
                return (msg, error.As(addr(new nestedError("Name",err))!)!);
            }

            msg = packType(msg, q.Type);
            return (packClass(msg, q.Class), error.As(null!)!);

        }

        // GoString implements fmt.GoStringer.GoString.
        private static @string GoString(this ptr<Question> _addr_q)
        {
            ref Question q = ref _addr_q.val;

            return "dnsmessage.Question{" + "Name: " + q.Name.GoString() + ", " + "Type: " + q.Type.GoString() + ", " + "Class: " + q.Class.GoString() + "}";
        }

        private static (ResourceBody, long, error) unpackResourceBody(slice<byte> msg, long off, ResourceHeader hdr)
        {
            ResourceBody _p0 = default;
            long _p0 = default;
            error _p0 = default!;

            ResourceBody r = default!;            error err = default!;            @string name = default;

            if (hdr.Type == TypeA) 
                ref AResource rb = ref heap(out ptr<AResource> _addr_rb);
                rb, err = unpackAResource(msg, off);
                r = ResourceBody.As(_addr_rb)!;
                name = "A";
            else if (hdr.Type == TypeNS) 
                rb = default;
                rb, err = unpackNSResource(msg, off);
                r = ResourceBody.As(_addr_rb)!;
                name = "NS";
            else if (hdr.Type == TypeCNAME) 
                rb = default;
                rb, err = unpackCNAMEResource(msg, off);
                r = ResourceBody.As(_addr_rb)!;
                name = "CNAME";
            else if (hdr.Type == TypeSOA) 
                rb = default;
                rb, err = unpackSOAResource(msg, off);
                r = ResourceBody.As(_addr_rb)!;
                name = "SOA";
            else if (hdr.Type == TypePTR) 
                rb = default;
                rb, err = unpackPTRResource(msg, off);
                r = ResourceBody.As(_addr_rb)!;
                name = "PTR";
            else if (hdr.Type == TypeMX) 
                rb = default;
                rb, err = unpackMXResource(msg, off);
                r = ResourceBody.As(_addr_rb)!;
                name = "MX";
            else if (hdr.Type == TypeTXT) 
                rb = default;
                rb, err = unpackTXTResource(msg, off, hdr.Length);
                r = ResourceBody.As(_addr_rb)!;
                name = "TXT";
            else if (hdr.Type == TypeAAAA) 
                rb = default;
                rb, err = unpackAAAAResource(msg, off);
                r = ResourceBody.As(_addr_rb)!;
                name = "AAAA";
            else if (hdr.Type == TypeSRV) 
                rb = default;
                rb, err = unpackSRVResource(msg, off);
                r = ResourceBody.As(_addr_rb)!;
                name = "SRV";
            else if (hdr.Type == TypeOPT) 
                rb = default;
                rb, err = unpackOPTResource(msg, off, hdr.Length);
                r = ResourceBody.As(_addr_rb)!;
                name = "OPT";
                        if (err != null)
            {
                return (null, off, error.As(addr(new nestedError(name+" record",err))!)!);
            }

            if (r == null)
            {
                return (null, off, error.As(fmt.Errorf("invalid resource type: %d", hdr.Type))!);
            }

            return (r, off + int(hdr.Length), error.As(null!)!);

        }

        // A CNAMEResource is a CNAME Resource record.
        public partial struct CNAMEResource
        {
            public Name CNAME;
        }

        private static Type realType(this ptr<CNAMEResource> _addr_r)
        {
            ref CNAMEResource r = ref _addr_r.val;

            return TypeCNAME;
        }

        // pack appends the wire format of the CNAMEResource to msg.
        private static (slice<byte>, error) pack(this ptr<CNAMEResource> _addr_r, slice<byte> msg, map<@string, long> compression, long compressionOff)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref CNAMEResource r = ref _addr_r.val;

            return r.CNAME.pack(msg, compression, compressionOff);
        }

        // GoString implements fmt.GoStringer.GoString.
        private static @string GoString(this ptr<CNAMEResource> _addr_r)
        {
            ref CNAMEResource r = ref _addr_r.val;

            return "dnsmessage.CNAMEResource{CNAME: " + r.CNAME.GoString() + "}";
        }

        private static (CNAMEResource, error) unpackCNAMEResource(slice<byte> msg, long off)
        {
            CNAMEResource _p0 = default;
            error _p0 = default!;

            Name cname = default;
            {
                var (_, err) = cname.unpack(msg, off);

                if (err != null)
                {
                    return (new CNAMEResource(), error.As(err)!);
                }

            }

            return (new CNAMEResource(cname), error.As(null!)!);

        }

        // An MXResource is an MX Resource record.
        public partial struct MXResource
        {
            public ushort Pref;
            public Name MX;
        }

        private static Type realType(this ptr<MXResource> _addr_r)
        {
            ref MXResource r = ref _addr_r.val;

            return TypeMX;
        }

        // pack appends the wire format of the MXResource to msg.
        private static (slice<byte>, error) pack(this ptr<MXResource> _addr_r, slice<byte> msg, map<@string, long> compression, long compressionOff)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref MXResource r = ref _addr_r.val;

            var oldMsg = msg;
            msg = packUint16(msg, r.Pref);
            var (msg, err) = r.MX.pack(msg, compression, compressionOff);
            if (err != null)
            {
                return (oldMsg, error.As(addr(new nestedError("MXResource.MX",err))!)!);
            }

            return (msg, error.As(null!)!);

        }

        // GoString implements fmt.GoStringer.GoString.
        private static @string GoString(this ptr<MXResource> _addr_r)
        {
            ref MXResource r = ref _addr_r.val;

            return "dnsmessage.MXResource{" + "Pref: " + printUint16(r.Pref) + ", " + "MX: " + r.MX.GoString() + "}";
        }

        private static (MXResource, error) unpackMXResource(slice<byte> msg, long off)
        {
            MXResource _p0 = default;
            error _p0 = default!;

            var (pref, off, err) = unpackUint16(msg, off);
            if (err != null)
            {
                return (new MXResource(), error.As(addr(new nestedError("Pref",err))!)!);
            }

            Name mx = default;
            {
                var (_, err) = mx.unpack(msg, off);

                if (err != null)
                {
                    return (new MXResource(), error.As(addr(new nestedError("MX",err))!)!);
                }

            }

            return (new MXResource(pref,mx), error.As(null!)!);

        }

        // An NSResource is an NS Resource record.
        public partial struct NSResource
        {
            public Name NS;
        }

        private static Type realType(this ptr<NSResource> _addr_r)
        {
            ref NSResource r = ref _addr_r.val;

            return TypeNS;
        }

        // pack appends the wire format of the NSResource to msg.
        private static (slice<byte>, error) pack(this ptr<NSResource> _addr_r, slice<byte> msg, map<@string, long> compression, long compressionOff)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref NSResource r = ref _addr_r.val;

            return r.NS.pack(msg, compression, compressionOff);
        }

        // GoString implements fmt.GoStringer.GoString.
        private static @string GoString(this ptr<NSResource> _addr_r)
        {
            ref NSResource r = ref _addr_r.val;

            return "dnsmessage.NSResource{NS: " + r.NS.GoString() + "}";
        }

        private static (NSResource, error) unpackNSResource(slice<byte> msg, long off)
        {
            NSResource _p0 = default;
            error _p0 = default!;

            Name ns = default;
            {
                var (_, err) = ns.unpack(msg, off);

                if (err != null)
                {
                    return (new NSResource(), error.As(err)!);
                }

            }

            return (new NSResource(ns), error.As(null!)!);

        }

        // A PTRResource is a PTR Resource record.
        public partial struct PTRResource
        {
            public Name PTR;
        }

        private static Type realType(this ptr<PTRResource> _addr_r)
        {
            ref PTRResource r = ref _addr_r.val;

            return TypePTR;
        }

        // pack appends the wire format of the PTRResource to msg.
        private static (slice<byte>, error) pack(this ptr<PTRResource> _addr_r, slice<byte> msg, map<@string, long> compression, long compressionOff)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref PTRResource r = ref _addr_r.val;

            return r.PTR.pack(msg, compression, compressionOff);
        }

        // GoString implements fmt.GoStringer.GoString.
        private static @string GoString(this ptr<PTRResource> _addr_r)
        {
            ref PTRResource r = ref _addr_r.val;

            return "dnsmessage.PTRResource{PTR: " + r.PTR.GoString() + "}";
        }

        private static (PTRResource, error) unpackPTRResource(slice<byte> msg, long off)
        {
            PTRResource _p0 = default;
            error _p0 = default!;

            Name ptr = default;
            {
                var (_, err) = ptr.unpack(msg, off);

                if (err != null)
                {
                    return (new PTRResource(), error.As(err)!);
                }

            }

            return (new PTRResource(ptr), error.As(null!)!);

        }

        // An SOAResource is an SOA Resource record.
        public partial struct SOAResource
        {
            public Name NS;
            public Name MBox;
            public uint Serial;
            public uint Refresh;
            public uint Retry;
            public uint Expire; // MinTTL the is the default TTL of Resources records which did not
// contain a TTL value and the TTL of negative responses. (RFC 2308
// Section 4)
            public uint MinTTL;
        }

        private static Type realType(this ptr<SOAResource> _addr_r)
        {
            ref SOAResource r = ref _addr_r.val;

            return TypeSOA;
        }

        // pack appends the wire format of the SOAResource to msg.
        private static (slice<byte>, error) pack(this ptr<SOAResource> _addr_r, slice<byte> msg, map<@string, long> compression, long compressionOff)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref SOAResource r = ref _addr_r.val;

            var oldMsg = msg;
            var (msg, err) = r.NS.pack(msg, compression, compressionOff);
            if (err != null)
            {
                return (oldMsg, error.As(addr(new nestedError("SOAResource.NS",err))!)!);
            }

            msg, err = r.MBox.pack(msg, compression, compressionOff);
            if (err != null)
            {
                return (oldMsg, error.As(addr(new nestedError("SOAResource.MBox",err))!)!);
            }

            msg = packUint32(msg, r.Serial);
            msg = packUint32(msg, r.Refresh);
            msg = packUint32(msg, r.Retry);
            msg = packUint32(msg, r.Expire);
            return (packUint32(msg, r.MinTTL), error.As(null!)!);

        }

        // GoString implements fmt.GoStringer.GoString.
        private static @string GoString(this ptr<SOAResource> _addr_r)
        {
            ref SOAResource r = ref _addr_r.val;

            return "dnsmessage.SOAResource{" + "NS: " + r.NS.GoString() + ", " + "MBox: " + r.MBox.GoString() + ", " + "Serial: " + printUint32(r.Serial) + ", " + "Refresh: " + printUint32(r.Refresh) + ", " + "Retry: " + printUint32(r.Retry) + ", " + "Expire: " + printUint32(r.Expire) + ", " + "MinTTL: " + printUint32(r.MinTTL) + "}";
        }

        private static (SOAResource, error) unpackSOAResource(slice<byte> msg, long off)
        {
            SOAResource _p0 = default;
            error _p0 = default!;

            Name ns = default;
            var (off, err) = ns.unpack(msg, off);
            if (err != null)
            {
                return (new SOAResource(), error.As(addr(new nestedError("NS",err))!)!);
            }

            Name mbox = default;
            off, err = mbox.unpack(msg, off);

            if (err != null)
            {
                return (new SOAResource(), error.As(addr(new nestedError("MBox",err))!)!);
            }

            var (serial, off, err) = unpackUint32(msg, off);
            if (err != null)
            {
                return (new SOAResource(), error.As(addr(new nestedError("Serial",err))!)!);
            }

            var (refresh, off, err) = unpackUint32(msg, off);
            if (err != null)
            {
                return (new SOAResource(), error.As(addr(new nestedError("Refresh",err))!)!);
            }

            var (retry, off, err) = unpackUint32(msg, off);
            if (err != null)
            {
                return (new SOAResource(), error.As(addr(new nestedError("Retry",err))!)!);
            }

            var (expire, off, err) = unpackUint32(msg, off);
            if (err != null)
            {
                return (new SOAResource(), error.As(addr(new nestedError("Expire",err))!)!);
            }

            var (minTTL, _, err) = unpackUint32(msg, off);
            if (err != null)
            {
                return (new SOAResource(), error.As(addr(new nestedError("MinTTL",err))!)!);
            }

            return (new SOAResource(ns,mbox,serial,refresh,retry,expire,minTTL), error.As(null!)!);

        }

        // A TXTResource is a TXT Resource record.
        public partial struct TXTResource
        {
            public slice<@string> TXT;
        }

        private static Type realType(this ptr<TXTResource> _addr_r)
        {
            ref TXTResource r = ref _addr_r.val;

            return TypeTXT;
        }

        // pack appends the wire format of the TXTResource to msg.
        private static (slice<byte>, error) pack(this ptr<TXTResource> _addr_r, slice<byte> msg, map<@string, long> compression, long compressionOff)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref TXTResource r = ref _addr_r.val;

            var oldMsg = msg;
            foreach (var (_, s) in r.TXT)
            {
                error err = default!;
                msg, err = packText(msg, s);
                if (err != null)
                {
                    return (oldMsg, error.As(err)!);
                }

            }
            return (msg, error.As(null!)!);

        }

        // GoString implements fmt.GoStringer.GoString.
        private static @string GoString(this ptr<TXTResource> _addr_r)
        {
            ref TXTResource r = ref _addr_r.val;

            @string s = "dnsmessage.TXTResource{TXT: []string{";
            if (len(r.TXT) == 0L)
            {
                return s + "}}";
            }

            s += "\"" + printString((slice<byte>)r.TXT[0L]);
            foreach (var (_, t) in r.TXT[1L..])
            {
                s += "\", \"" + printString((slice<byte>)t);
            }
            return s + "\"}}";

        }

        private static (TXTResource, error) unpackTXTResource(slice<byte> msg, long off, ushort length)
        {
            TXTResource _p0 = default;
            error _p0 = default!;

            var txts = make_slice<@string>(0L, 1L);
            {
                var n = uint16(0L);

                while (n < length)
                {
                    @string t = default;
                    error err = default!;
                    t, off, err = unpackText(msg, off);

                    if (err != null)
                    {
                        return (new TXTResource(), error.As(addr(new nestedError("text",err))!)!);
                    } 
                    // Check if we got too many bytes.
                    if (length - n < uint16(len(t)) + 1L)
                    {
                        return (new TXTResource(), error.As(errCalcLen)!);
                    }

                    n += uint16(len(t)) + 1L;
                    txts = append(txts, t);

                }

            }
            return (new TXTResource(txts), error.As(null!)!);

        }

        // An SRVResource is an SRV Resource record.
        public partial struct SRVResource
        {
            public ushort Priority;
            public ushort Weight;
            public ushort Port;
            public Name Target; // Not compressed as per RFC 2782.
        }

        private static Type realType(this ptr<SRVResource> _addr_r)
        {
            ref SRVResource r = ref _addr_r.val;

            return TypeSRV;
        }

        // pack appends the wire format of the SRVResource to msg.
        private static (slice<byte>, error) pack(this ptr<SRVResource> _addr_r, slice<byte> msg, map<@string, long> compression, long compressionOff)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref SRVResource r = ref _addr_r.val;

            var oldMsg = msg;
            msg = packUint16(msg, r.Priority);
            msg = packUint16(msg, r.Weight);
            msg = packUint16(msg, r.Port);
            var (msg, err) = r.Target.pack(msg, null, compressionOff);
            if (err != null)
            {
                return (oldMsg, error.As(addr(new nestedError("SRVResource.Target",err))!)!);
            }

            return (msg, error.As(null!)!);

        }

        // GoString implements fmt.GoStringer.GoString.
        private static @string GoString(this ptr<SRVResource> _addr_r)
        {
            ref SRVResource r = ref _addr_r.val;

            return "dnsmessage.SRVResource{" + "Priority: " + printUint16(r.Priority) + ", " + "Weight: " + printUint16(r.Weight) + ", " + "Port: " + printUint16(r.Port) + ", " + "Target: " + r.Target.GoString() + "}";
        }

        private static (SRVResource, error) unpackSRVResource(slice<byte> msg, long off)
        {
            SRVResource _p0 = default;
            error _p0 = default!;

            var (priority, off, err) = unpackUint16(msg, off);
            if (err != null)
            {
                return (new SRVResource(), error.As(addr(new nestedError("Priority",err))!)!);
            }

            var (weight, off, err) = unpackUint16(msg, off);
            if (err != null)
            {
                return (new SRVResource(), error.As(addr(new nestedError("Weight",err))!)!);
            }

            var (port, off, err) = unpackUint16(msg, off);
            if (err != null)
            {
                return (new SRVResource(), error.As(addr(new nestedError("Port",err))!)!);
            }

            Name target = default;
            {
                var (_, err) = target.unpackCompressed(msg, off, false);

                if (err != null)
                {
                    return (new SRVResource(), error.As(addr(new nestedError("Target",err))!)!);
                }

            }

            return (new SRVResource(priority,weight,port,target), error.As(null!)!);

        }

        // An AResource is an A Resource record.
        public partial struct AResource
        {
            public array<byte> A;
        }

        private static Type realType(this ptr<AResource> _addr_r)
        {
            ref AResource r = ref _addr_r.val;

            return TypeA;
        }

        // pack appends the wire format of the AResource to msg.
        private static (slice<byte>, error) pack(this ptr<AResource> _addr_r, slice<byte> msg, map<@string, long> compression, long compressionOff)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref AResource r = ref _addr_r.val;

            return (packBytes(msg, r.A[..]), error.As(null!)!);
        }

        // GoString implements fmt.GoStringer.GoString.
        private static @string GoString(this ptr<AResource> _addr_r)
        {
            ref AResource r = ref _addr_r.val;

            return "dnsmessage.AResource{" + "A: [4]byte{" + printByteSlice(r.A[..]) + "}}";
        }

        private static (AResource, error) unpackAResource(slice<byte> msg, long off)
        {
            AResource _p0 = default;
            error _p0 = default!;

            array<byte> a = new array<byte>(4L);
            {
                var (_, err) = unpackBytes(msg, off, a[..]);

                if (err != null)
                {
                    return (new AResource(), error.As(err)!);
                }

            }

            return (new AResource(a), error.As(null!)!);

        }

        // An AAAAResource is an AAAA Resource record.
        public partial struct AAAAResource
        {
            public array<byte> AAAA;
        }

        private static Type realType(this ptr<AAAAResource> _addr_r)
        {
            ref AAAAResource r = ref _addr_r.val;

            return TypeAAAA;
        }

        // GoString implements fmt.GoStringer.GoString.
        private static @string GoString(this ptr<AAAAResource> _addr_r)
        {
            ref AAAAResource r = ref _addr_r.val;

            return "dnsmessage.AAAAResource{" + "AAAA: [16]byte{" + printByteSlice(r.AAAA[..]) + "}}";
        }

        // pack appends the wire format of the AAAAResource to msg.
        private static (slice<byte>, error) pack(this ptr<AAAAResource> _addr_r, slice<byte> msg, map<@string, long> compression, long compressionOff)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref AAAAResource r = ref _addr_r.val;

            return (packBytes(msg, r.AAAA[..]), error.As(null!)!);
        }

        private static (AAAAResource, error) unpackAAAAResource(slice<byte> msg, long off)
        {
            AAAAResource _p0 = default;
            error _p0 = default!;

            array<byte> aaaa = new array<byte>(16L);
            {
                var (_, err) = unpackBytes(msg, off, aaaa[..]);

                if (err != null)
                {
                    return (new AAAAResource(), error.As(err)!);
                }

            }

            return (new AAAAResource(aaaa), error.As(null!)!);

        }

        // An OPTResource is an OPT pseudo Resource record.
        //
        // The pseudo resource record is part of the extension mechanisms for DNS
        // as defined in RFC 6891.
        public partial struct OPTResource
        {
            public slice<Option> Options;
        }

        // An Option represents a DNS message option within OPTResource.
        //
        // The message option is part of the extension mechanisms for DNS as
        // defined in RFC 6891.
        public partial struct Option
        {
            public ushort Code; // option code
            public slice<byte> Data;
        }

        // GoString implements fmt.GoStringer.GoString.
        private static @string GoString(this ptr<Option> _addr_o)
        {
            ref Option o = ref _addr_o.val;

            return "dnsmessage.Option{" + "Code: " + printUint16(o.Code) + ", " + "Data: []byte{" + printByteSlice(o.Data) + "}}";
        }

        private static Type realType(this ptr<OPTResource> _addr_r)
        {
            ref OPTResource r = ref _addr_r.val;

            return TypeOPT;
        }

        private static (slice<byte>, error) pack(this ptr<OPTResource> _addr_r, slice<byte> msg, map<@string, long> compression, long compressionOff)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref OPTResource r = ref _addr_r.val;

            foreach (var (_, opt) in r.Options)
            {
                msg = packUint16(msg, opt.Code);
                var l = uint16(len(opt.Data));
                msg = packUint16(msg, l);
                msg = packBytes(msg, opt.Data);
            }
            return (msg, error.As(null!)!);

        }

        // GoString implements fmt.GoStringer.GoString.
        private static @string GoString(this ptr<OPTResource> _addr_r)
        {
            ref OPTResource r = ref _addr_r.val;

            @string s = "dnsmessage.OPTResource{Options: []dnsmessage.Option{";
            if (len(r.Options) == 0L)
            {
                return s + "}}";
            }

            s += r.Options[0L].GoString();
            foreach (var (_, o) in r.Options[1L..])
            {
                s += ", " + o.GoString();
            }
            return s + "}}";

        }

        private static (OPTResource, error) unpackOPTResource(slice<byte> msg, long off, ushort length)
        {
            OPTResource _p0 = default;
            error _p0 = default!;

            slice<Option> opts = default;
            {
                var oldOff = off;

                while (off < oldOff + int(length))
                {
                    error err = default!;
                    Option o = default;
                    o.Code, off, err = unpackUint16(msg, off);
                    if (err != null)
                    {
                        return (new OPTResource(), error.As(addr(new nestedError("Code",err))!)!);
                    }

                    ushort l = default;
                    l, off, err = unpackUint16(msg, off);
                    if (err != null)
                    {
                        return (new OPTResource(), error.As(addr(new nestedError("Data",err))!)!);
                    }

                    o.Data = make_slice<byte>(l);
                    if (copy(o.Data, msg[off..]) != int(l))
                    {
                        return (new OPTResource(), error.As(addr(new nestedError("Data",errCalcLen))!)!);
                    }

                    off += int(l);
                    opts = append(opts, o);

                }

            }
            return (new OPTResource(opts), error.As(null!)!);

        }
    }
}}}}}
