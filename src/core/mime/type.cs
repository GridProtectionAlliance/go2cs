// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package mime implements parts of the MIME spec.
namespace go;

using fmt = fmt_package;
using slices = slices_package;
using strings = strings_package;
using sync = sync_package;

partial class mime_package {

internal static sync.Map mimeTypes; // map[string]string; ".Z" => "application/x-compress"
internal static sync.Map mimeTypesLower; // map[string]string; ".z" => "application/x-compress"
internal static sync.Mutex extensionsMu; // Guards stores (but not loads) on extensions.
internal static sync.Map extensions; // map[string][]string; slice values are append-only.

// setMimeTypes is used by initMime's non-test path, and by tests.
internal static void setMimeTypes(map<@string, @string> lowerExt, map<@string, @string> mixExt) => func((defer, _) => {
    mimeTypes.Clear();
    mimeTypesLower.Clear();
    extensions.Clear();
    foreach (var (k, v) in lowerExt) {
        mimeTypesLower.Store(k, v);
    }
    foreach (var (k, v) in mixExt) {
        mimeTypes.Store(k, v);
    }
    extensionsMu.Lock();
    var extensionsMuʗ1 = extensionsMu;
    defer(extensionsMuʗ1.Unlock);
    foreach (var (k, v) in lowerExt) {
        var (justType, _, err) = ParseMediaType(v);
        if (err != default!) {
            throw panic(err);
        }
        slice<@string> exts = default!;
        {
            var (ei, ok) = extensions.Load(justType); if (ok) {
                exts = ei._<slice<@string>>();
            }
        }
        extensions.Store(justType, append(exts, k));
    }
});

internal static map<@string, @string> builtinTypesLower = new map<@string, @string>{
    [".avif"u8] = "image/avif"u8,
    [".css"u8] = "text/css; charset=utf-8"u8,
    [".gif"u8] = "image/gif"u8,
    [".htm"u8] = "text/html; charset=utf-8"u8,
    [".html"u8] = "text/html; charset=utf-8"u8,
    [".jpeg"u8] = "image/jpeg"u8,
    [".jpg"u8] = "image/jpeg"u8,
    [".js"u8] = "text/javascript; charset=utf-8"u8,
    [".json"u8] = "application/json"u8,
    [".mjs"u8] = "text/javascript; charset=utf-8"u8,
    [".pdf"u8] = "application/pdf"u8,
    [".png"u8] = "image/png"u8,
    [".svg"u8] = "image/svg+xml"u8,
    [".wasm"u8] = "application/wasm"u8,
    [".webp"u8] = "image/webp"u8,
    [".xml"u8] = "text/xml; charset=utf-8"u8
};

internal static sync.Once once;  // guards initMime

internal static Action testInitMime;
internal static Action osInitMime;

internal static void initMime() {
    {
        var fn = testInitMime; if (fn != default!){
            fn();
        } else {
            setMimeTypes(builtinTypesLower, builtinTypesLower);
            osInitMime();
        }
    }
}

// TypeByExtension returns the MIME type associated with the file extension ext.
// The extension ext should begin with a leading dot, as in ".html".
// When ext has no associated type, TypeByExtension returns "".
//
// Extensions are looked up first case-sensitively, then case-insensitively.
//
// The built-in table is small but on unix it is augmented by the local
// system's MIME-info database or mime.types file(s) if available under one or
// more of these names:
//
//	/usr/local/share/mime/globs2
//	/usr/share/mime/globs2
//	/etc/mime.types
//	/etc/apache2/mime.types
//	/etc/apache/mime.types
//
// On Windows, MIME types are extracted from the registry.
//
// Text types have the charset parameter set to "utf-8" by default.
public static @string TypeByExtension(@string ext) {
    once.Do(initMime);
    // Case-sensitive lookup.
    {
        var (v, ok) = mimeTypes.Load(ext); if (ok) {
            return v._<@string>();
        }
    }
    // Case-insensitive lookup.
    // Optimistically assume a short ASCII extension and be
    // allocation-free in that case.
    array<byte> buf = new(10);
    var lower = buf[..0];
    static readonly UntypedInt utf8RuneSelf = /* 0x80 */ 128; // from utf8 package, but not importing it.
    for (nint i = 0; i < len(ext); i++) {
        var c = ext[i];
        if (c >= utf8RuneSelf) {
            // Slow path.
            var (siΔ1, _) = mimeTypesLower.Load(strings.ToLower(ext));
            var (sΔ1, _) = siΔ1._<@string>(ᐧ);
            return sΔ1;
        }
        if ((rune)'A' <= c && c <= (rune)'Z'){
            lower = append(lower, c + ((rune)'a' - (rune)'A'));
        } else {
            lower = append(lower, c);
        }
    }
    var (si, _) = mimeTypesLower.Load(((@string)lower));
    var (s, _) = si._<@string>(ᐧ);
    return s;
}

// ExtensionsByType returns the extensions known to be associated with the MIME
// type typ. The returned extensions will each begin with a leading dot, as in
// ".html". When typ has no associated extensions, ExtensionsByType returns an
// nil slice.
public static (slice<@string>, error) ExtensionsByType(@string typ) {
    var (justType, _, err) = ParseMediaType(typ);
    if (err != default!) {
        return (default!, err);
    }
    once.Do(initMime);
    var (s, ok) = extensions.Load(justType);
    if (!ok) {
        return (default!, default!);
    }
    var ret = append(slice<@string>(default!), s._<slice<@string>>().ꓸꓸꓸ);
    slices.Sort(ret);
    return (ret, default!);
}

// AddExtensionType sets the MIME type associated with
// the extension ext to typ. The extension should begin with
// a leading dot, as in ".html".
public static error AddExtensionType(@string ext, @string typ) {
    if (!strings.HasPrefix(ext, "."u8)) {
        return fmt.Errorf("mime: extension %q missing leading dot"u8, ext);
    }
    once.Do(initMime);
    return setExtensionType(ext, typ);
}

internal static error setExtensionType(@string extension, @string mimeType) => func((defer, _) => {
    var (justType, param, err) = ParseMediaType(mimeType);
    if (err != default!) {
        return err;
    }
    if (strings.HasPrefix(mimeType, "text/"u8) && param["charset"u8] == "") {
        param["charset"u8] = "utf-8"u8;
        mimeType = FormatMediaType(mimeType, param);
    }
    @string extLower = strings.ToLower(extension);
    mimeTypes.Store(extension, mimeType);
    mimeTypesLower.Store(extLower, mimeType);
    extensionsMu.Lock();
    var extensionsMuʗ1 = extensionsMu;
    defer(extensionsMuʗ1.Unlock);
    slice<@string> exts = default!;
    {
        var (ei, ok) = extensions.Load(justType); if (ok) {
            exts = ei._<slice<@string>>();
        }
    }
    foreach (var (_, v) in exts) {
        if (v == extLower) {
            return default!;
        }
    }
    extensions.Store(justType, append(exts, extLower));
    return default!;
});

} // end mime_package
