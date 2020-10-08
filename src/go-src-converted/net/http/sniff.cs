// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package http -- go2cs converted at 2020 October 08 03:40:33 UTC
// import "net/http" ==> using http = go.net.http_package
// Original source: C:\Go\src\net\http\sniff.go
using bytes = go.bytes_package;
using binary = go.encoding.binary_package;
using static go.builtin;

namespace go {
namespace net
{
    public static partial class http_package
    {
        // The algorithm uses at most sniffLen bytes to make its decision.
        private static readonly long sniffLen = (long)512L;

        // DetectContentType implements the algorithm described
        // at https://mimesniff.spec.whatwg.org/ to determine the
        // Content-Type of the given data. It considers at most the
        // first 512 bytes of data. DetectContentType always returns
        // a valid MIME type: if it cannot determine a more specific one, it
        // returns "application/octet-stream".


        // DetectContentType implements the algorithm described
        // at https://mimesniff.spec.whatwg.org/ to determine the
        // Content-Type of the given data. It considers at most the
        // first 512 bytes of data. DetectContentType always returns
        // a valid MIME type: if it cannot determine a more specific one, it
        // returns "application/octet-stream".
        public static @string DetectContentType(slice<byte> data)
        {
            if (len(data) > sniffLen)
            {
                data = data[..sniffLen];
            } 

            // Index of the first non-whitespace byte in data.
            long firstNonWS = 0L;
            while (firstNonWS < len(data) && isWS(data[firstNonWS]))
            {
                firstNonWS++;
            }


            foreach (var (_, sig) in sniffSignatures)
            {
                {
                    var ct = sig.match(data, firstNonWS);

                    if (ct != "")
                    {
                        return ct;
                    }

                }

            }
            return "application/octet-stream"; // fallback
        }

        // isWS reports whether the provided byte is a whitespace byte (0xWS)
        // as defined in https://mimesniff.spec.whatwg.org/#terminology.
        private static bool isWS(byte b)
        {
            switch (b)
            {
                case '\t': 

                case '\n': 

                case '\x0c': 

                case '\r': 

                case ' ': 
                    return true;
                    break;
            }
            return false;

        }

        // isTT reports whether the provided byte is a tag-terminating byte (0xTT)
        // as defined in https://mimesniff.spec.whatwg.org/#terminology.
        private static bool isTT(byte b)
        {
            switch (b)
            {
                case ' ': 

                case '>': 
                    return true;
                    break;
            }
            return false;

        }

        private partial interface sniffSig
        {
            @string match(slice<byte> data, long firstNonWS);
        }

        // Data matching the table in section 6.
        private static sniffSig sniffSignatures = new slice<sniffSig>(new sniffSig[] { sniffSig.As(htmlSig("<!DOCTYPE HTML"))!, sniffSig.As(htmlSig("<HTML"))!, sniffSig.As(htmlSig("<HEAD"))!, sniffSig.As(htmlSig("<SCRIPT"))!, sniffSig.As(htmlSig("<IFRAME"))!, sniffSig.As(htmlSig("<H1"))!, sniffSig.As(htmlSig("<DIV"))!, sniffSig.As(htmlSig("<FONT"))!, sniffSig.As(htmlSig("<TABLE"))!, sniffSig.As(htmlSig("<A"))!, sniffSig.As(htmlSig("<STYLE"))!, sniffSig.As(htmlSig("<TITLE"))!, sniffSig.As(htmlSig("<B"))!, sniffSig.As(htmlSig("<BODY"))!, sniffSig.As(htmlSig("<BR"))!, sniffSig.As(htmlSig("<P"))!, sniffSig.As(htmlSig("<!--"))!, sniffSig.As(&maskedSig{mask:[]byte("\xFF\xFF\xFF\xFF\xFF"),pat:[]byte("<?xml"),skipWS:true,ct:"text/xml; charset=utf-8"})!, sniffSig.As(&exactSig{[]byte("%PDF-"),"application/pdf"})!, sniffSig.As(&exactSig{[]byte("%!PS-Adobe-"),"application/postscript"})!, sniffSig.As(&maskedSig{mask:[]byte("\xFF\xFF\x00\x00"),pat:[]byte("\xFE\xFF\x00\x00"),ct:"text/plain; charset=utf-16be",})!, sniffSig.As(&maskedSig{mask:[]byte("\xFF\xFF\x00\x00"),pat:[]byte("\xFF\xFE\x00\x00"),ct:"text/plain; charset=utf-16le",})!, sniffSig.As(&maskedSig{mask:[]byte("\xFF\xFF\xFF\x00"),pat:[]byte("\xEF\xBB\xBF\x00"),ct:"text/plain; charset=utf-8",})!, sniffSig.As(&exactSig{[]byte("\x00\x00\x01\x00"),"image/x-icon"})!, sniffSig.As(&exactSig{[]byte("\x00\x00\x02\x00"),"image/x-icon"})!, sniffSig.As(&exactSig{[]byte("BM"),"image/bmp"})!, sniffSig.As(&exactSig{[]byte("GIF87a"),"image/gif"})!, sniffSig.As(&exactSig{[]byte("GIF89a"),"image/gif"})!, sniffSig.As(&maskedSig{mask:[]byte("\xFF\xFF\xFF\xFF\x00\x00\x00\x00\xFF\xFF\xFF\xFF\xFF\xFF"),pat:[]byte("RIFF\x00\x00\x00\x00WEBPVP"),ct:"image/webp",})!, sniffSig.As(&exactSig{[]byte("\x89PNG\x0D\x0A\x1A\x0A"),"image/png"})!, sniffSig.As(&exactSig{[]byte("\xFF\xD8\xFF"),"image/jpeg"})!, sniffSig.As(&maskedSig{mask:[]byte("\xFF\xFF\xFF\xFF"),pat:[]byte(".snd"),ct:"audio/basic",})!, sniffSig.As(&maskedSig{mask:[]byte("\xFF\xFF\xFF\xFF\x00\x00\x00\x00\xFF\xFF\xFF\xFF"),pat:[]byte("FORM\x00\x00\x00\x00AIFF"),ct:"audio/aiff",})!, sniffSig.As(&maskedSig{mask:[]byte("\xFF\xFF\xFF"),pat:[]byte("ID3"),ct:"audio/mpeg",})!, sniffSig.As(&maskedSig{mask:[]byte("\xFF\xFF\xFF\xFF\xFF"),pat:[]byte("OggS\x00"),ct:"application/ogg",})!, sniffSig.As(&maskedSig{mask:[]byte("\xFF\xFF\xFF\xFF\xFF\xFF\xFF\xFF"),pat:[]byte("MThd\x00\x00\x00\x06"),ct:"audio/midi",})!, sniffSig.As(&maskedSig{mask:[]byte("\xFF\xFF\xFF\xFF\x00\x00\x00\x00\xFF\xFF\xFF\xFF"),pat:[]byte("RIFF\x00\x00\x00\x00AVI "),ct:"video/avi",})!, sniffSig.As(&maskedSig{mask:[]byte("\xFF\xFF\xFF\xFF\x00\x00\x00\x00\xFF\xFF\xFF\xFF"),pat:[]byte("RIFF\x00\x00\x00\x00WAVE"),ct:"audio/wave",})!, sniffSig.As(mp4Sig{})!, sniffSig.As(&exactSig{[]byte("\x1A\x45\xDF\xA3"),"video/webm"})!, sniffSig.As(&maskedSig{pat:[]byte("\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00LP"),mask:[]byte("\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\xFF\xFF"),ct:"application/vnd.ms-fontobject",})!, sniffSig.As(&exactSig{[]byte("\x00\x01\x00\x00"),"font/ttf"})!, sniffSig.As(&exactSig{[]byte("OTTO"),"font/otf"})!, sniffSig.As(&exactSig{[]byte("ttcf"),"font/collection"})!, sniffSig.As(&exactSig{[]byte("wOFF"),"font/woff"})!, sniffSig.As(&exactSig{[]byte("wOF2"),"font/woff2"})!, sniffSig.As(&exactSig{[]byte("\x1F\x8B\x08"),"application/x-gzip"})!, sniffSig.As(&exactSig{[]byte("PK\x03\x04"),"application/zip"})!, sniffSig.As(&exactSig{[]byte("Rar!\x1A\x07\x00"),"application/x-rar-compressed"})!, sniffSig.As(&exactSig{[]byte("Rar!\x1A\x07\x01\x00"),"application/x-rar-compressed"})!, sniffSig.As(&exactSig{[]byte("\x00\x61\x73\x6D"),"application/wasm"})!, sniffSig.As(textSig{})! });

        private partial struct exactSig
        {
            public slice<byte> sig;
            public @string ct;
        }

        private static @string match(this ptr<exactSig> _addr_e, slice<byte> data, long firstNonWS)
        {
            ref exactSig e = ref _addr_e.val;

            if (bytes.HasPrefix(data, e.sig))
            {
                return e.ct;
            }

            return "";

        }

        private partial struct maskedSig
        {
            public slice<byte> mask;
            public slice<byte> pat;
            public bool skipWS;
            public @string ct;
        }

        private static @string match(this ptr<maskedSig> _addr_m, slice<byte> data, long firstNonWS)
        {
            ref maskedSig m = ref _addr_m.val;
 
            // pattern matching algorithm section 6
            // https://mimesniff.spec.whatwg.org/#pattern-matching-algorithm

            if (m.skipWS)
            {
                data = data[firstNonWS..];
            }

            if (len(m.pat) != len(m.mask))
            {
                return "";
            }

            if (len(data) < len(m.pat))
            {
                return "";
            }

            foreach (var (i, pb) in m.pat)
            {
                var maskedData = data[i] & m.mask[i];
                if (maskedData != pb)
                {
                    return "";
                }

            }
            return m.ct;

        }

        private partial struct htmlSig // : slice<byte>
        {
        }

        private static @string match(this htmlSig h, slice<byte> data, long firstNonWS)
        {
            data = data[firstNonWS..];
            if (len(data) < len(h) + 1L)
            {
                return "";
            }

            foreach (var (i, b) in h)
            {
                var db = data[i];
                if ('A' <= b && b <= 'Z')
                {
                    db &= 0xDFUL;
                }

                if (b != db)
                {
                    return "";
                }

            } 
            // Next byte must be a tag-terminating byte(0xTT).
            if (!isTT(data[len(h)]))
            {
                return "";
            }

            return "text/html; charset=utf-8";

        }

        private static slice<byte> mp4ftype = (slice<byte>)"ftyp";
        private static slice<byte> mp4 = (slice<byte>)"mp4";

        private partial struct mp4Sig
        {
        }

        private static @string match(this mp4Sig _p0, slice<byte> data, long firstNonWS)
        { 
            // https://mimesniff.spec.whatwg.org/#signature-for-mp4
            // c.f. section 6.2.1
            if (len(data) < 12L)
            {
                return "";
            }

            var boxSize = int(binary.BigEndian.Uint32(data[..4L]));
            if (len(data) < boxSize || boxSize % 4L != 0L)
            {
                return "";
            }

            if (!bytes.Equal(data[4L..8L], mp4ftype))
            {
                return "";
            }

            {
                long st = 8L;

                while (st < boxSize)
                {
                    if (st == 12L)
                    { 
                        // Ignores the four bytes that correspond to the version number of the "major brand".
                        continue;
                    st += 4L;
                    }

                    if (bytes.Equal(data[st..st + 3L], mp4))
                    {
                        return "video/mp4";
                    }

                }

            }
            return "";

        }

        private partial struct textSig
        {
        }

        private static @string match(this textSig _p0, slice<byte> data, long firstNonWS)
        { 
            // c.f. section 5, step 4.
            foreach (var (_, b) in data[firstNonWS..])
            {

                if (b <= 0x08UL || b == 0x0BUL || 0x0EUL <= b && b <= 0x1AUL || 0x1CUL <= b && b <= 0x1FUL) 
                    return "";
                
            }
            return "text/plain; charset=utf-8";

        }
    }
}}
