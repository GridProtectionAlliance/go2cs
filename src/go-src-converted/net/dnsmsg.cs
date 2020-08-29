// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// DNS packet assembly. See RFC 1035.
//
// This is intended to support name resolution during Dial.
// It doesn't have to be blazing fast.
//
// Each message structure has a Walk method that is used by
// a generic pack/unpack routine. Thus, if in the future we need
// to define new message structs, no new pack/unpack/printing code
// needs to be written.
//
// The first half of this file defines the DNS message formats.
// The second half implements the conversion to and from wire format.
// A few of the structure elements have string tags to aid the
// generic pack/unpack routines.
//
// TODO(rsc):  There are enough names defined in this file that they're all
// prefixed with dns. Perhaps put this in its own package later.

// package net -- go2cs converted at 2020 August 29 08:26:05 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\dnsmsg.go

using static go.builtin;
using System;

namespace go
{
    public static partial class net_package
    {
        // Packet formats

        // Wire constants.
 
        // valid dnsRR_Header.Rrtype and dnsQuestion.qtype
        private static readonly long dnsTypeA = 1L;
        private static readonly long dnsTypeNS = 2L;
        private static readonly long dnsTypeMD = 3L;
        private static readonly long dnsTypeMF = 4L;
        private static readonly long dnsTypeCNAME = 5L;
        private static readonly long dnsTypeSOA = 6L;
        private static readonly long dnsTypeMB = 7L;
        private static readonly long dnsTypeMG = 8L;
        private static readonly long dnsTypeMR = 9L;
        private static readonly long dnsTypeNULL = 10L;
        private static readonly long dnsTypeWKS = 11L;
        private static readonly long dnsTypePTR = 12L;
        private static readonly long dnsTypeHINFO = 13L;
        private static readonly long dnsTypeMINFO = 14L;
        private static readonly long dnsTypeMX = 15L;
        private static readonly long dnsTypeTXT = 16L;
        private static readonly long dnsTypeAAAA = 28L;
        private static readonly long dnsTypeSRV = 33L; 

        // valid dnsQuestion.qtype only
        private static readonly long dnsTypeAXFR = 252L;
        private static readonly long dnsTypeMAILB = 253L;
        private static readonly long dnsTypeMAILA = 254L;
        private static readonly long dnsTypeALL = 255L; 

        // valid dnsQuestion.qclass
        private static readonly long dnsClassINET = 1L;
        private static readonly long dnsClassCSNET = 2L;
        private static readonly long dnsClassCHAOS = 3L;
        private static readonly long dnsClassHESIOD = 4L;
        private static readonly long dnsClassANY = 255L; 

        // dnsMsg.rcode
        private static readonly long dnsRcodeSuccess = 0L;
        private static readonly long dnsRcodeFormatError = 1L;
        private static readonly long dnsRcodeServerFailure = 2L;
        private static readonly long dnsRcodeNameError = 3L;
        private static readonly long dnsRcodeNotImplemented = 4L;
        private static readonly long dnsRcodeRefused = 5L;

        // A dnsStruct describes how to iterate over its fields to emulate
        // reflective marshaling.
        private partial interface dnsStruct
        {
            bool Walk(Func<object, @string, @string, bool> f);
        }

        // The wire format for the DNS packet header.
        private partial struct dnsHeader
        {
            public ushort Id;
            public ushort Bits;
            public ushort Qdcount;
            public ushort Ancount;
            public ushort Nscount;
            public ushort Arcount;
        }

        private static bool Walk(this ref dnsHeader h, Func<object, @string, @string, bool> f)
        {
            return f(ref h.Id, "Id", "") && f(ref h.Bits, "Bits", "") && f(ref h.Qdcount, "Qdcount", "") && f(ref h.Ancount, "Ancount", "") && f(ref h.Nscount, "Nscount", "") && f(ref h.Arcount, "Arcount", "");
        }

 
        // dnsHeader.Bits
        private static readonly long _QR = 1L << (int)(15L); // query/response (response=1)
        private static readonly long _AA = 1L << (int)(10L); // authoritative
        private static readonly long _TC = 1L << (int)(9L); // truncated
        private static readonly long _RD = 1L << (int)(8L); // recursion desired
        private static readonly long _RA = 1L << (int)(7L); // recursion available

        // DNS queries.
        private partial struct dnsQuestion
        {
            public @string Name;
            public ushort Qtype;
            public ushort Qclass;
        }

        private static bool Walk(this ref dnsQuestion q, Func<object, @string, @string, bool> f)
        {
            return f(ref q.Name, "Name", "domain") && f(ref q.Qtype, "Qtype", "") && f(ref q.Qclass, "Qclass", "");
        }

        // DNS responses (resource records).
        // There are many types of messages,
        // but they all share the same header.
        private partial struct dnsRR_Header
        {
            public @string Name;
            public ushort Rrtype;
            public ushort Class;
            public uint Ttl;
            public ushort Rdlength; // length of data after header
        }

        private static ref dnsRR_Header Header(this ref dnsRR_Header h)
        {
            return h;
        }

        private static bool Walk(this ref dnsRR_Header h, Func<object, @string, @string, bool> f)
        {
            return f(ref h.Name, "Name", "domain") && f(ref h.Rrtype, "Rrtype", "") && f(ref h.Class, "Class", "") && f(ref h.Ttl, "Ttl", "") && f(ref h.Rdlength, "Rdlength", "");
        }

        private partial interface dnsRR : dnsStruct
        {
            ref dnsRR_Header Header();
        }

        // Specific DNS RR formats for each query type.

        private partial struct dnsRR_CNAME
        {
            public dnsRR_Header Hdr;
            public @string Cname;
        }

        private static ref dnsRR_Header Header(this ref dnsRR_CNAME rr)
        {
            return ref rr.Hdr;
        }

        private static bool Walk(this ref dnsRR_CNAME rr, Func<object, @string, @string, bool> f)
        {
            return rr.Hdr.Walk(f) && f(ref rr.Cname, "Cname", "domain");
        }

        private partial struct dnsRR_MX
        {
            public dnsRR_Header Hdr;
            public ushort Pref;
            public @string Mx;
        }

        private static ref dnsRR_Header Header(this ref dnsRR_MX rr)
        {
            return ref rr.Hdr;
        }

        private static bool Walk(this ref dnsRR_MX rr, Func<object, @string, @string, bool> f)
        {
            return rr.Hdr.Walk(f) && f(ref rr.Pref, "Pref", "") && f(ref rr.Mx, "Mx", "domain");
        }

        private partial struct dnsRR_NS
        {
            public dnsRR_Header Hdr;
            public @string Ns;
        }

        private static ref dnsRR_Header Header(this ref dnsRR_NS rr)
        {
            return ref rr.Hdr;
        }

        private static bool Walk(this ref dnsRR_NS rr, Func<object, @string, @string, bool> f)
        {
            return rr.Hdr.Walk(f) && f(ref rr.Ns, "Ns", "domain");
        }

        private partial struct dnsRR_PTR
        {
            public dnsRR_Header Hdr;
            public @string Ptr;
        }

        private static ref dnsRR_Header Header(this ref dnsRR_PTR rr)
        {
            return ref rr.Hdr;
        }

        private static bool Walk(this ref dnsRR_PTR rr, Func<object, @string, @string, bool> f)
        {
            return rr.Hdr.Walk(f) && f(ref rr.Ptr, "Ptr", "domain");
        }

        private partial struct dnsRR_SOA
        {
            public dnsRR_Header Hdr;
            public @string Ns;
            public @string Mbox;
            public uint Serial;
            public uint Refresh;
            public uint Retry;
            public uint Expire;
            public uint Minttl;
        }

        private static ref dnsRR_Header Header(this ref dnsRR_SOA rr)
        {
            return ref rr.Hdr;
        }

        private static bool Walk(this ref dnsRR_SOA rr, Func<object, @string, @string, bool> f)
        {
            return rr.Hdr.Walk(f) && f(ref rr.Ns, "Ns", "domain") && f(ref rr.Mbox, "Mbox", "domain") && f(ref rr.Serial, "Serial", "") && f(ref rr.Refresh, "Refresh", "") && f(ref rr.Retry, "Retry", "") && f(ref rr.Expire, "Expire", "") && f(ref rr.Minttl, "Minttl", "");
        }

        private partial struct dnsRR_TXT
        {
            public dnsRR_Header Hdr;
            public @string Txt; // not domain name
        }

        private static ref dnsRR_Header Header(this ref dnsRR_TXT rr)
        {
            return ref rr.Hdr;
        }

        private static bool Walk(this ref dnsRR_TXT rr, Func<object, @string, @string, bool> f)
        {
            if (!rr.Hdr.Walk(f))
            {
                return false;
            }
            ushort n = 0L;
            while (n < rr.Hdr.Rdlength)
            {
                @string txt = default;
                if (!f(ref txt, "Txt", ""))
                {
                    return false;
                } 
                // more bytes than rr.Hdr.Rdlength said there would be
                if (rr.Hdr.Rdlength - n < uint16(len(txt)) + 1L)
                {
                    return false;
                }
                n += uint16(len(txt)) + 1L;
                rr.Txt += txt;
            }

            return true;
        }

        private partial struct dnsRR_SRV
        {
            public dnsRR_Header Hdr;
            public ushort Priority;
            public ushort Weight;
            public ushort Port;
            public @string Target;
        }

        private static ref dnsRR_Header Header(this ref dnsRR_SRV rr)
        {
            return ref rr.Hdr;
        }

        private static bool Walk(this ref dnsRR_SRV rr, Func<object, @string, @string, bool> f)
        {
            return rr.Hdr.Walk(f) && f(ref rr.Priority, "Priority", "") && f(ref rr.Weight, "Weight", "") && f(ref rr.Port, "Port", "") && f(ref rr.Target, "Target", "domain");
        }

        private partial struct dnsRR_A
        {
            public dnsRR_Header Hdr;
            public uint A;
        }

        private static ref dnsRR_Header Header(this ref dnsRR_A rr)
        {
            return ref rr.Hdr;
        }

        private static bool Walk(this ref dnsRR_A rr, Func<object, @string, @string, bool> f)
        {
            return rr.Hdr.Walk(f) && f(ref rr.A, "A", "ipv4");
        }

        private partial struct dnsRR_AAAA
        {
            public dnsRR_Header Hdr;
            public array<byte> AAAA;
        }

        private static ref dnsRR_Header Header(this ref dnsRR_AAAA rr)
        {
            return ref rr.Hdr;
        }

        private static bool Walk(this ref dnsRR_AAAA rr, Func<object, @string, @string, bool> f)
        {
            return rr.Hdr.Walk(f) && f(rr.AAAA[..], "AAAA", "ipv6");
        }

        // Packing and unpacking.
        //
        // All the packers and unpackers take a (msg []byte, off int)
        // and return (off1 int, ok bool).  If they return ok==false, they
        // also return off1==len(msg), so that the next unpacker will
        // also fail. This lets us avoid checks of ok until the end of a
        // packing sequence.

        // Map of constructors for each RR wire type.
        private static map rr_mk = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<long, Func<dnsRR>>{dnsTypeCNAME:func()dnsRR{returnnew(dnsRR_CNAME)},dnsTypeMX:func()dnsRR{returnnew(dnsRR_MX)},dnsTypeNS:func()dnsRR{returnnew(dnsRR_NS)},dnsTypePTR:func()dnsRR{returnnew(dnsRR_PTR)},dnsTypeSOA:func()dnsRR{returnnew(dnsRR_SOA)},dnsTypeTXT:func()dnsRR{returnnew(dnsRR_TXT)},dnsTypeSRV:func()dnsRR{returnnew(dnsRR_SRV)},dnsTypeA:func()dnsRR{returnnew(dnsRR_A)},dnsTypeAAAA:func()dnsRR{returnnew(dnsRR_AAAA)},};

        // Pack a domain name s into msg[off:].
        // Domain names are a sequence of counted strings
        // split at the dots. They end with a zero-length string.
        private static (long, bool) packDomainName(@string s, slice<byte> msg, long off)
        { 
            // Add trailing dot to canonicalize name.
            {
                var n = len(s);

                if (n == 0L || s[n - 1L] != '.')
                {
                    s += ".";
                } 

                // Allow root domain.

            } 

            // Allow root domain.
            if (s == ".")
            {
                msg[off] = 0L;
                off++;
                return (off, true);
            } 

            // Each dot ends a segment of the name.
            // We trade each dot byte for a length byte.
            // There is also a trailing zero.
            // Check that we have all the space we need.
            var tot = len(s) + 1L;
            if (off + tot > len(msg))
            {
                return (len(msg), false);
            } 

            // Emit sequence of counted strings, chopping at dots.
            long begin = 0L;
            for (long i = 0L; i < len(s); i++)
            {
                if (s[i] == '.')
                {
                    if (i - begin >= 1L << (int)(6L))
                    { // top two bits of length must be clear
                        return (len(msg), false);
                    }
                    if (i - begin == 0L)
                    {
                        return (len(msg), false);
                    }
                    msg[off] = byte(i - begin);
                    off++;

                    for (var j = begin; j < i; j++)
                    {
                        msg[off] = s[j];
                        off++;
                    }

                    begin = i + 1L;
                }
            }

            msg[off] = 0L;
            off++;
            return (off, true);
        }

        // Unpack a domain name.
        // In addition to the simple sequences of counted strings above,
        // domain names are allowed to refer to strings elsewhere in the
        // packet, to avoid repeating common suffixes when returning
        // many entries in a single domain. The pointers are marked
        // by a length byte with the top two bits set. Ignoring those
        // two bits, that byte and the next give a 14 bit offset from msg[0]
        // where we should pick up the trail.
        // Note that if we jump elsewhere in the packet,
        // we return off1 == the offset after the first pointer we found,
        // which is where the next record will start.
        // In theory, the pointers are only allowed to jump backward.
        // We let them jump anywhere and stop jumping after a while.
        private static (@string, long, bool) unpackDomainName(slice<byte> msg, long off)
        {
            s = "";
            long ptr = 0L; // number of pointers followed
Loop:
            while (true)
            {
                if (off >= len(msg))
                {
                    return ("", len(msg), false);
                }
                var c = int(msg[off]);
                off++;
                switch (c & 0xC0UL)
                {
                    case 0x00UL: 
                        if (c == 0x00UL)
                        { 
                            // end of name
                            _breakLoop = true;
                            break;
                        } 
                        // literal string
                        if (off + c > len(msg))
                        {
                            return ("", len(msg), false);
                        }
                        s += string(msg[off..off + c]) + ".";
                        off += c;
                        break;
                    case 0xC0UL: 
                        // pointer to somewhere else in msg.
                        // remember location after first ptr,
                        // since that's how many bytes we consumed.
                        // also, don't follow too many pointers --
                        // maybe there's a loop.
                        if (off >= len(msg))
                        {
                            return ("", len(msg), false);
                        }
                        var c1 = msg[off];
                        off++;
                        if (ptr == 0L)
                        {
                            off1 = off;
                        }
                        ptr++;

                        if (ptr > 10L)
                        {
                            return ("", len(msg), false);
                        }
                        off = (c ^ 0xC0UL) << (int)(8L) | int(c1);
                        break;
                    default: 
                        // 0x80 and 0x40 are reserved
                        return ("", len(msg), false);
                        break;
                }
            }
            if (len(s) == 0L)
            {
                s = ".";
            }
            if (ptr == 0L)
            {
                off1 = off;
            }
            return (s, off1, true);
        }

        // packStruct packs a structure into msg at specified offset off, and
        // returns off1 such that msg[off:off1] is the encoded data.
        private static (long, bool) packStruct(dnsStruct any, slice<byte> msg, long off)
        {
            ok = any.Walk((field, name, tag) =>
            {
                switch (field.type())
                {
                    case ref ushort fv:
                        var i = fv.Value;
                        if (off + 2L > len(msg))
                        {
                            return false;
                        }
                        msg[off] = byte(i >> (int)(8L));
                        msg[off + 1L] = byte(i);
                        off += 2L;
                        break;
                    case ref uint fv:
                        i = fv.Value;
                        msg[off] = byte(i >> (int)(24L));
                        msg[off + 1L] = byte(i >> (int)(16L));
                        msg[off + 2L] = byte(i >> (int)(8L));
                        msg[off + 3L] = byte(i);
                        off += 4L;
                        break;
                    case slice<byte> fv:
                        var n = len(fv);
                        if (off + n > len(msg))
                        {
                            return false;
                        }
                        copy(msg[off..off + n], fv);
                        off += n;
                        break;
                    case ref @string fv:
                        var s = fv.Value;
                        switch (tag)
                        {
                            case "domain": 
                                off, ok = packDomainName(s, msg, off);
                                if (!ok)
                                {
                                    return false;
                                }
                                break;
                            case "": 
                                // Counted string: 1 byte length.
                                if (len(s) > 255L || off + 1L + len(s) > len(msg))
                                {
                                    return false;
                                }
                                msg[off] = byte(len(s));
                                off++;
                                off += copy(msg[off..], s);
                                break;
                            default: 
                                println("net: dns: unknown string tag", tag);
                                return false;
                                break;
                        }
                        break;
                    default:
                    {
                        var fv = field.type();
                        println("net: dns: unknown packing type");
                        return false;
                        break;
                    }
                }
                return true;
            });
            if (!ok)
            {
                return (len(msg), false);
            }
            return (off, true);
        }

        // unpackStruct decodes msg[off:] into the given structure, and
        // returns off1 such that msg[off:off1] is the encoded data.
        private static (long, bool) unpackStruct(dnsStruct any, slice<byte> msg, long off)
        {
            ok = any.Walk((field, name, tag) =>
            {
                switch (field.type())
                {
                    case ref ushort fv:
                        if (off + 2L > len(msg))
                        {
                            return false;
                        }
                        fv.Value = uint16(msg[off]) << (int)(8L) | uint16(msg[off + 1L]);
                        off += 2L;
                        break;
                    case ref uint fv:
                        if (off + 4L > len(msg))
                        {
                            return false;
                        }
                        fv.Value = uint32(msg[off]) << (int)(24L) | uint32(msg[off + 1L]) << (int)(16L) | uint32(msg[off + 2L]) << (int)(8L) | uint32(msg[off + 3L]);
                        off += 4L;
                        break;
                    case slice<byte> fv:
                        var n = len(fv);
                        if (off + n > len(msg))
                        {
                            return false;
                        }
                        copy(fv, msg[off..off + n]);
                        off += n;
                        break;
                    case ref @string fv:
                        @string s = default;
                        switch (tag)
                        {
                            case "domain": 
                                s, off, ok = unpackDomainName(msg, off);
                                if (!ok)
                                {
                                    return false;
                                }
                                break;
                            case "": 
                                if (off >= len(msg) || off + 1L + int(msg[off]) > len(msg))
                                {
                                    return false;
                                }
                                n = int(msg[off]);
                                off++;
                                var b = make_slice<byte>(n);
                                for (long i = 0L; i < n; i++)
                                {
                                    b[i] = msg[off + i];
                                }

                                off += n;
                                s = string(b);
                                break;
                            default: 
                                println("net: dns: unknown string tag", tag);
                                return false;
                                break;
                        }
                        fv.Value = s;
                        break;
                    default:
                    {
                        var fv = field.type();
                        println("net: dns: unknown packing type");
                        return false;
                        break;
                    }
                }
                return true;
            });
            if (!ok)
            {
                return (len(msg), false);
            }
            return (off, true);
        }

        // Generic struct printer. Prints fields with tag "ipv4" or "ipv6"
        // as IP addresses.
        private static @string printStruct(dnsStruct any)
        {
            @string s = "{";
            long i = 0L;
            any.Walk((val, name, tag) =>
            {
                i++;
                if (i > 1L)
                {
                    s += ", ";
                }
                s += name + "=";
                switch (tag)
                {
                    case "ipv4": 
                        i = val._<ref uint>().Value;
                        s += IPv4(byte(i >> (int)(24L)), byte(i >> (int)(16L)), byte(i >> (int)(8L)), byte(i)).String();
                        break;
                    case "ipv6": 
                        i = val._<slice<byte>>();
                        s += IP(i).String();
                        break;
                    default: 
                        i = default;
                        switch (val.type())
                        {
                            case ref @string v:
                                s += v.Value;
                                return true;
                                break;
                            case slice<byte> v:
                                s += string(v);
                                return true;
                                break;
                            case ref bool v:
                                if (v.Value)
                                {
                                    s += "true";
                                }
                                else
                                {
                                    s += "false";
                                }
                                return true;
                                break;
                            case ref long v:
                                i = int64(v.Value);
                                break;
                            case ref ulong v:
                                i = int64(v.Value);
                                break;
                            case ref byte v:
                                i = int64(v.Value);
                                break;
                            case ref ushort v:
                                i = int64(v.Value);
                                break;
                            case ref uint v:
                                i = int64(v.Value);
                                break;
                            case ref ulong v:
                                i = int64(v.Value);
                                break;
                            case ref System.UIntPtr v:
                                i = int64(v.Value);
                                break;
                            default:
                            {
                                var v = val.type();
                                s += "<unknown type>";
                                return true;
                                break;
                            }
                        }
                        s += itoa(int(i));
                        break;
                }
                return true;
            });
            s += "}";
            return s;
        }

        // Resource record packer.
        private static (long, bool) packRR(dnsRR rr, slice<byte> msg, long off)
        {
            long off1 = default; 
            // pack twice, once to find end of header
            // and again to find end of packet.
            // a bit inefficient but this doesn't need to be fast.
            // off1 is end of header
            // off2 is end of rr
            off1, ok = packStruct(rr.Header(), msg, off);
            if (!ok)
            {
                return (len(msg), false);
            }
            off2, ok = packStruct(rr, msg, off);
            if (!ok)
            {
                return (len(msg), false);
            } 
            // pack a third time; redo header with correct data length
            rr.Header().Rdlength = uint16(off2 - off1);
            packStruct(rr.Header(), msg, off);
            return (off2, true);
        }

        // Resource record unpacker.
        private static (dnsRR, long, bool) unpackRR(slice<byte> msg, long off)
        { 
            // unpack just the header, to find the rr type and length
            dnsRR_Header h = default;
            var off0 = off;
            off, ok = unpackStruct(ref h, msg, off);

            if (!ok)
            {
                return (null, len(msg), false);
            }
            var end = off + int(h.Rdlength); 

            // make an rr of that type and re-unpack.
            // again inefficient but doesn't need to be fast.
            var (mk, known) = rr_mk[int(h.Rrtype)];
            if (!known)
            {
                return (ref h, end, true);
            }
            rr = mk();
            off, ok = unpackStruct(rr, msg, off0);
            if (off != end)
            {
                return (ref h, end, true);
            }
            return (rr, off, ok);
        }

        // Usable representation of a DNS packet.

        // A manually-unpacked version of (id, bits).
        // This is in its own struct for easy printing.
        private partial struct dnsMsgHdr
        {
            public ushort id;
            public bool response;
            public long opcode;
            public bool authoritative;
            public bool truncated;
            public bool recursion_desired;
            public bool recursion_available;
            public long rcode;
        }

        private static bool Walk(this ref dnsMsgHdr h, Func<object, @string, @string, bool> f)
        {
            return f(ref h.id, "id", "") && f(ref h.response, "response", "") && f(ref h.opcode, "opcode", "") && f(ref h.authoritative, "authoritative", "") && f(ref h.truncated, "truncated", "") && f(ref h.recursion_desired, "recursion_desired", "") && f(ref h.recursion_available, "recursion_available", "") && f(ref h.rcode, "rcode", "");
        }

        private partial struct dnsMsg
        {
            public ref dnsMsgHdr dnsMsgHdr => ref dnsMsgHdr_val;
            public slice<dnsQuestion> question;
            public slice<dnsRR> answer;
            public slice<dnsRR> ns;
            public slice<dnsRR> extra;
        }

        private static (slice<byte>, bool) Pack(this ref dnsMsg dns)
        {
            dnsHeader dh = default; 

            // Convert convenient dnsMsg into wire-like dnsHeader.
            dh.Id = dns.id;
            dh.Bits = uint16(dns.opcode) << (int)(11L) | uint16(dns.rcode);
            if (dns.recursion_available)
            {
                dh.Bits |= _RA;
            }
            if (dns.recursion_desired)
            {
                dh.Bits |= _RD;
            }
            if (dns.truncated)
            {
                dh.Bits |= _TC;
            }
            if (dns.authoritative)
            {
                dh.Bits |= _AA;
            }
            if (dns.response)
            {
                dh.Bits |= _QR;
            } 

            // Prepare variable sized arrays.
            var question = dns.question;
            var answer = dns.answer;
            var ns = dns.ns;
            var extra = dns.extra;

            dh.Qdcount = uint16(len(question));
            dh.Ancount = uint16(len(answer));
            dh.Nscount = uint16(len(ns));
            dh.Arcount = uint16(len(extra)); 

            // Could work harder to calculate message size,
            // but this is far more than we need and not
            // big enough to hurt the allocator.
            msg = make_slice<byte>(2000L); 

            // Pack it in: header and then the pieces.
            long off = 0L;
            off, ok = packStruct(ref dh, msg, off);
            if (!ok)
            {
                return (null, false);
            }
            {
                long i__prev1 = i;

                for (long i = 0L; i < len(question); i++)
                {
                    off, ok = packStruct(ref question[i], msg, off);
                    if (!ok)
                    {
                        return (null, false);
                    }
                }


                i = i__prev1;
            }
            {
                long i__prev1 = i;

                for (i = 0L; i < len(answer); i++)
                {
                    off, ok = packRR(answer[i], msg, off);
                    if (!ok)
                    {
                        return (null, false);
                    }
                }


                i = i__prev1;
            }
            {
                long i__prev1 = i;

                for (i = 0L; i < len(ns); i++)
                {
                    off, ok = packRR(ns[i], msg, off);
                    if (!ok)
                    {
                        return (null, false);
                    }
                }


                i = i__prev1;
            }
            {
                long i__prev1 = i;

                for (i = 0L; i < len(extra); i++)
                {
                    off, ok = packRR(extra[i], msg, off);
                    if (!ok)
                    {
                        return (null, false);
                    }
                }


                i = i__prev1;
            }
            return (msg[0L..off], true);
        }

        private static bool Unpack(this ref dnsMsg dns, slice<byte> msg)
        { 
            // Header.
            dnsHeader dh = default;
            long off = 0L;
            bool ok = default;
            off, ok = unpackStruct(ref dh, msg, off);

            if (!ok)
            {
                return false;
            }
            dns.id = dh.Id;
            dns.response = (dh.Bits & _QR) != 0L;
            dns.opcode = int(dh.Bits >> (int)(11L)) & 0xFUL;
            dns.authoritative = (dh.Bits & _AA) != 0L;
            dns.truncated = (dh.Bits & _TC) != 0L;
            dns.recursion_desired = (dh.Bits & _RD) != 0L;
            dns.recursion_available = (dh.Bits & _RA) != 0L;
            dns.rcode = int(dh.Bits & 0xFUL); 

            // Arrays.
            dns.question = make_slice<dnsQuestion>(dh.Qdcount);
            dns.answer = make_slice<dnsRR>(0L, dh.Ancount);
            dns.ns = make_slice<dnsRR>(0L, dh.Nscount);
            dns.extra = make_slice<dnsRR>(0L, dh.Arcount);

            dnsRR rec = default;

            {
                long i__prev1 = i;

                for (long i = 0L; i < len(dns.question); i++)
                {
                    off, ok = unpackStruct(ref dns.question[i], msg, off);
                    if (!ok)
                    {
                        return false;
                    }
                }


                i = i__prev1;
            }
            {
                long i__prev1 = i;

                for (i = 0L; i < int(dh.Ancount); i++)
                {
                    rec, off, ok = unpackRR(msg, off);
                    if (!ok)
                    {
                        return false;
                    }
                    dns.answer = append(dns.answer, rec);
                }


                i = i__prev1;
            }
            {
                long i__prev1 = i;

                for (i = 0L; i < int(dh.Nscount); i++)
                {
                    rec, off, ok = unpackRR(msg, off);
                    if (!ok)
                    {
                        return false;
                    }
                    dns.ns = append(dns.ns, rec);
                }


                i = i__prev1;
            }
            {
                long i__prev1 = i;

                for (i = 0L; i < int(dh.Arcount); i++)
                {
                    rec, off, ok = unpackRR(msg, off);
                    if (!ok)
                    {
                        return false;
                    }
                    dns.extra = append(dns.extra, rec);
                } 
                //    if off != len(msg) {
                //        println("extra bytes in dns packet", off, "<", len(msg));
                //    }


                i = i__prev1;
            } 
            //    if off != len(msg) {
            //        println("extra bytes in dns packet", off, "<", len(msg));
            //    }
            return true;
        }

        private static @string String(this ref dnsMsg dns)
        {
            @string s = "DNS: " + printStruct(ref dns.dnsMsgHdr) + "\n";
            if (len(dns.question) > 0L)
            {
                s += "-- Questions\n";
                {
                    long i__prev1 = i;

                    for (long i = 0L; i < len(dns.question); i++)
                    {
                        s += printStruct(ref dns.question[i]) + "\n";
                    }


                    i = i__prev1;
                }
            }
            if (len(dns.answer) > 0L)
            {
                s += "-- Answers\n";
                {
                    long i__prev1 = i;

                    for (i = 0L; i < len(dns.answer); i++)
                    {
                        s += printStruct(dns.answer[i]) + "\n";
                    }


                    i = i__prev1;
                }
            }
            if (len(dns.ns) > 0L)
            {
                s += "-- Name servers\n";
                {
                    long i__prev1 = i;

                    for (i = 0L; i < len(dns.ns); i++)
                    {
                        s += printStruct(dns.ns[i]) + "\n";
                    }


                    i = i__prev1;
                }
            }
            if (len(dns.extra) > 0L)
            {
                s += "-- Extra\n";
                {
                    long i__prev1 = i;

                    for (i = 0L; i < len(dns.extra); i++)
                    {
                        s += printStruct(dns.extra[i]) + "\n";
                    }


                    i = i__prev1;
                }
            }
            return s;
        }

        // IsResponseTo reports whether m is an acceptable response to query.
        private static bool IsResponseTo(this ref dnsMsg m, ref dnsMsg query)
        {
            if (!m.response)
            {
                return false;
            }
            if (m.id != query.id)
            {
                return false;
            }
            if (len(m.question) != len(query.question))
            {
                return false;
            }
            foreach (var (i, q) in m.question)
            {
                var q2 = query.question[i];
                if (!equalASCIILabel(q.Name, q2.Name) || q.Qtype != q2.Qtype || q.Qclass != q2.Qclass)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
