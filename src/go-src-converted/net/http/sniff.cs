// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

using bytes = bytes_package;
using binary = encoding.binary_package;
using encoding;

partial class http_package {

// The algorithm uses at most sniffLen bytes to make its decision.
internal static readonly UntypedInt sniffLen = 512;

// DetectContentType implements the algorithm described
// at https://mimesniff.spec.whatwg.org/ to determine the
// Content-Type of the given data. It considers at most the
// first 512 bytes of data. DetectContentType always returns
// a valid MIME type: if it cannot determine a more specific one, it
// returns "application/octet-stream".
public static @string DetectContentType(slice<byte> data) {
    if (len(data) > sniffLen) {
        data = data[..(int)(sniffLen)];
    }
    // Index of the first non-whitespace byte in data.
    nint firstNonWS = 0;
    for (; firstNonWS < len(data) && isWS(data[firstNonWS]); firstNonWS++) {
    }
    foreach (var (_, sig) in sniffSignatures) {
        {
            @string ct = sig.match(data, firstNonWS); if (ct != ""u8) {
                return ct;
            }
        }
    }
    return "application/octet-stream"u8;
}

// fallback

// isWS reports whether the provided byte is a whitespace byte (0xWS)
// as defined in https://mimesniff.spec.whatwg.org/#terminology.
internal static bool isWS(byte b) {
    switch (b) {
    case (rune)'\t' or (rune)'\n' or (rune)'\x0c' or (rune)'\r' or (rune)' ': {
        return true;
    }}

    return false;
}

// isTT reports whether the provided byte is a tag-terminating byte (0xTT)
// as defined in https://mimesniff.spec.whatwg.org/#terminology.
internal static bool isTT(byte b) {
    switch (b) {
    case (rune)' ' or (rune)'>': {
        return true;
    }}

    return false;
}

[GoType] partial interface sniffSig {
    // match returns the MIME type of the data, or "" if unknown.
    @string match(slice<byte> data, nint firstNonWS);
}

// UTF BOMs.
// Image types
// For posterity, we originally returned "image/vnd.microsoft.icon" from
// https://tools.ietf.org/html/draft-ietf-websec-mime-sniff-03#section-7
// https://codereview.appspot.com/4746042
// but that has since been replaced with "image/x-icon" in Section 6.2
// of https://mimesniff.spec.whatwg.org/#matching-an-image-type-pattern
// Audio and Video types
// Enforce the pattern match ordering as prescribed in
// https://mimesniff.spec.whatwg.org/#matching-an-audio-or-video-type-pattern
// 6.2.0.2. video/mp4
// 6.2.0.3. video/webm
// Font types
// 34 NULL bytes followed by the string "LP"
// 34 NULL bytes followed by \xF\xF
// Archive types
// RAR's signatures are incorrectly defined by the MIME spec as per
//    https://github.com/whatwg/mimesniff/issues/63
// However, RAR Labs correctly defines it at:
//    https://www.rarlab.com/technote.htm#rarsign
// so we use the definition from RAR Labs.
// TODO: do whatever the spec ends up doing.
// RAR v1.5-v4.0
// RAR v5+
// should be last
// Data matching the table in section 6.
internal static slice<sniffSig> sniffSignatures = new sniffSig[]{((htmlSig)"<!DOCTYPE HTML"u8), ((htmlSig)"<HTML"u8), ((htmlSig)"<HEAD"u8), ((htmlSig)"<SCRIPT"u8), ((htmlSig)"<IFRAME"u8), ((htmlSig)"<H1"u8), ((htmlSig)"<DIV"u8), ((htmlSig)"<FONT"u8), ((htmlSig)"<TABLE"u8), ((htmlSig)"<A"u8), ((htmlSig)"<STYLE"u8), ((htmlSig)"<TITLE"u8), ((htmlSig)"<B"u8), ((htmlSig)"<BODY"u8), ((htmlSig)"<BR"u8), ((htmlSig)"<P"u8), ((htmlSig)"<!--"u8), new maskedSig(
    mask: slice<byte>("\xFF\xFF\xFF\xFF\xFF"),
    pat: slice<byte>("<?xml"),
    skipWS: true,
    ct: "text/xml; charset=utf-8"u8), new exactSig(slice<byte>("%PDF-"), "application/pdf"), new exactSig(slice<byte>("%!PS-Adobe-"), "application/postscript"), new maskedSig(
    mask: slice<byte>("\xFF\xFF\x00\x00"),
    pat: slice<byte>("\xFE\xFF\x00\x00"),
    ct: "text/plain; charset=utf-16be"u8
), new maskedSig(
    mask: slice<byte>("\xFF\xFF\x00\x00"),
    pat: slice<byte>("\xFF\xFE\x00\x00"),
    ct: "text/plain; charset=utf-16le"u8
), new maskedSig(
    mask: slice<byte>("\xFF\xFF\xFF\x00"),
    pat: slice<byte>("\xEF\xBB\xBF\x00"),
    ct: "text/plain; charset=utf-8"u8
), new exactSig(slice<byte>("\x00\x00\x01\x00"), "image/x-icon"), new exactSig(slice<byte>("\x00\x00\x02\x00"), "image/x-icon"), new exactSig(slice<byte>("BM"), "image/bmp"), new exactSig(slice<byte>("GIF87a"), "image/gif"), new exactSig(slice<byte>("GIF89a"), "image/gif"), new maskedSig(
    mask: slice<byte>("\xFF\xFF\xFF\xFF\x00\x00\x00\x00\xFF\xFF\xFF\xFF\xFF\xFF"),
    pat: slice<byte>("RIFF\x00\x00\x00\x00WEBPVP"),
    ct: "image/webp"u8
), new exactSig(slice<byte>("\x89PNG\x0D\x0A\x1A\x0A"), "image/png"), new exactSig(slice<byte>("\xFF\xD8\xFF"), "image/jpeg"), new maskedSig(
    mask: slice<byte>("\xFF\xFF\xFF\xFF\x00\x00\x00\x00\xFF\xFF\xFF\xFF"),
    pat: slice<byte>("FORM\x00\x00\x00\x00AIFF"),
    ct: "audio/aiff"u8
), new maskedSig(
    mask: slice<byte>("\xFF\xFF\xFF"),
    pat: slice<byte>("ID3"),
    ct: "audio/mpeg"u8
), new maskedSig(
    mask: slice<byte>("\xFF\xFF\xFF\xFF\xFF"),
    pat: slice<byte>("OggS\x00"),
    ct: "application/ogg"u8
), new maskedSig(
    mask: slice<byte>("\xFF\xFF\xFF\xFF\xFF\xFF\xFF\xFF"),
    pat: slice<byte>("MThd\x00\x00\x00\x06"),
    ct: "audio/midi"u8
), new maskedSig(
    mask: slice<byte>("\xFF\xFF\xFF\xFF\x00\x00\x00\x00\xFF\xFF\xFF\xFF"),
    pat: slice<byte>("RIFF\x00\x00\x00\x00AVI "),
    ct: "video/avi"u8
), new maskedSig(
    mask: slice<byte>("\xFF\xFF\xFF\xFF\x00\x00\x00\x00\xFF\xFF\xFF\xFF"),
    pat: slice<byte>("RIFF\x00\x00\x00\x00WAVE"),
    ct: "audio/wave"u8
), new mp4Sig(nil), new exactSig(slice<byte>("\x1A\x45\xDF\xA3"), "video/webm"), new maskedSig(
    pat: slice<byte>("\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00LP"),
    mask: slice<byte>("\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\xFF\xFF"),
    ct: "application/vnd.ms-fontobject"u8
), new exactSig(slice<byte>("\x00\x01\x00\x00"), "font/ttf"), new exactSig(slice<byte>("OTTO"), "font/otf"), new exactSig(slice<byte>("ttcf"), "font/collection"), new exactSig(slice<byte>("wOFF"), "font/woff"), new exactSig(slice<byte>("wOF2"), "font/woff2"), new exactSig(slice<byte>("\x1F\x8B\x08"), "application/x-gzip"), new exactSig(slice<byte>("PK\x03\x04"), "application/zip"), new exactSig(slice<byte>("Rar!\x1A\x07\x00"), "application/x-rar-compressed"), new exactSig(slice<byte>("Rar!\x1A\x07\x01\x00"), "application/x-rar-compressed"), new exactSig(slice<byte>("\x00\x61\x73\x6D"), "application/wasm"), new textSig(nil)
}.slice();

[GoType] partial struct exactSig {
    internal slice<byte> sig;
    internal @string ct;
}

[GoRecv] internal static @string match(this ref exactSig e, slice<byte> data, nint firstNonWS) {
    if (bytes.HasPrefix(data, e.sig)) {
        return e.ct;
    }
    return ""u8;
}

[GoType] partial struct maskedSig {
    internal slice<byte> mask;
    internal slice<byte> pat;
    internal bool skipWS;
    internal @string ct;
}

[GoRecv] internal static @string match(this ref maskedSig m, slice<byte> data, nint firstNonWS) {
    // pattern matching algorithm section 6
    // https://mimesniff.spec.whatwg.org/#pattern-matching-algorithm
    if (m.skipWS) {
        data = data[(int)(firstNonWS)..];
    }
    if (len(m.pat) != len(m.mask)) {
        return ""u8;
    }
    if (len(data) < len(m.pat)) {
        return ""u8;
    }
    foreach (var (i, pb) in m.pat) {
        var maskedData = (byte)(data[i] & m.mask[i]);
        if (maskedData != pb) {
            return ""u8;
        }
    }
    return m.ct;
}

[GoType("[]byte")] partial struct htmlSig;

internal static @string match(this htmlSig h, slice<byte> data, nint firstNonWS) {
    data = data[(int)(firstNonWS)..];
    if (len(data) < len(h) + 1) {
        return ""u8;
    }
    foreach (var (i, b) in h) {
        var db = data[i];
        if ((rune)'A' <= b && b <= (rune)'Z') {
            db &= (byte)(223);
        }
        if (b != db) {
            return ""u8;
        }
    }
    // Next byte must be a tag-terminating byte(0xTT).
    if (!isTT(data[len(h)])) {
        return ""u8;
    }
    return "text/html; charset=utf-8"u8;
}

internal static slice<byte> mp4ftype = slice<byte>("ftyp");

internal static slice<byte> mp4 = slice<byte>("mp4");

[GoType] partial struct mp4Sig {
}

internal static @string match(this mp4Sig _, slice<byte> data, nint firstNonWS) {
    // https://mimesniff.spec.whatwg.org/#signature-for-mp4
    // c.f. section 6.2.1
    if (len(data) < 12) {
        return ""u8;
    }
    nint boxSize = ((nint)binary.BigEndian.Uint32(data[..4]));
    if (len(data) < boxSize || boxSize % 4 != 0) {
        return ""u8;
    }
    if (!bytes.Equal(data[4..8], mp4ftype)) {
        return ""u8;
    }
    for (nint st = 8; st < boxSize; st += 4) {
        if (st == 12) {
            // Ignores the four bytes that correspond to the version number of the "major brand".
            continue;
        }
        if (bytes.Equal(data[(int)(st)..(int)(st + 3)], mp4)) {
            return "video/mp4"u8;
        }
    }
    return ""u8;
}

[GoType] partial struct textSig {
}

internal static @string match(this textSig _, slice<byte> data, nint firstNonWS) {
    // c.f. section 5, step 4.
    foreach (var (_, b) in data[(int)(firstNonWS)..]) {
        switch (ᐧ) {
        case {} when (b <= 8) || (b == 11) || (14 <= b && b <= 26) || (28 <= b && b <= 31): {
            return ""u8;
        }}

    }
    return "text/plain; charset=utf-8"u8;
}

} // end http_package
