// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package mime implements parts of the MIME spec.
// package mime -- go2cs converted at 2022 March 06 22:21:17 UTC
// import "mime" ==> using mime = go.mime_package
// Original source: C:\Program Files\Go\src\mime\type.go
using fmt = go.fmt_package;
using sort = go.sort_package;
using strings = go.strings_package;
using sync = go.sync_package;
using System;


namespace go;

public static partial class mime_package {

private static sync.Map mimeTypes = default;private static sync.Map mimeTypesLower = default;private static sync.Mutex extensionsMu = default;private static sync.Map extensions = default;

private static void clearSyncMap(ptr<sync.Map> _addr_m) {
    ref sync.Map m = ref _addr_m.val;

    m.Range((k, _) => {
        m.Delete(k);
        return true;
    });
}

// setMimeTypes is used by initMime's non-test path, and by tests.
private static void setMimeTypes(map<@string, @string> lowerExt, map<@string, @string> mixExt) => func((defer, panic, _) => {
    clearSyncMap(_addr_mimeTypes);
    clearSyncMap(_addr_mimeTypesLower);
    clearSyncMap(_addr_extensions);

    {
        var k__prev1 = k;
        var v__prev1 = v;

        foreach (var (__k, __v) in lowerExt) {
            k = __k;
            v = __v;
            mimeTypesLower.Store(k, v);
        }
        k = k__prev1;
        v = v__prev1;
    }

    {
        var k__prev1 = k;
        var v__prev1 = v;

        foreach (var (__k, __v) in mixExt) {
            k = __k;
            v = __v;
            mimeTypes.Store(k, v);
        }
        k = k__prev1;
        v = v__prev1;
    }

    extensionsMu.Lock();
    defer(extensionsMu.Unlock());
    {
        var k__prev1 = k;
        var v__prev1 = v;

        foreach (var (__k, __v) in lowerExt) {
            k = __k;
            v = __v;
            var (justType, _, err) = ParseMediaType(v);
            if (err != null) {
                panic(err);
            }
            slice<@string> exts = default;
            {
                var (ei, ok) = extensions.Load(justType);

                if (ok) {
                    exts = ei._<slice<@string>>();
                }

            }

            extensions.Store(justType, append(exts, k));

        }
        k = k__prev1;
        v = v__prev1;
    }
});

private static map builtinTypesLower = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, @string>{".avif":"image/avif",".css":"text/css; charset=utf-8",".gif":"image/gif",".htm":"text/html; charset=utf-8",".html":"text/html; charset=utf-8",".jpeg":"image/jpeg",".jpg":"image/jpeg",".js":"text/javascript; charset=utf-8",".json":"application/json",".mjs":"text/javascript; charset=utf-8",".pdf":"application/pdf",".png":"image/png",".svg":"image/svg+xml",".wasm":"application/wasm",".webp":"image/webp",".xml":"text/xml; charset=utf-8",};

private static sync.Once once = default; // guards initMime

private static Action testInitMime = default;private static Action osInitMime = default;



private static void initMime() {
    {
        var fn = testInitMime;

        if (fn != null) {
            fn();
        }
        else
 {
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
//   /usr/local/share/mime/globs2
//   /usr/share/mime/globs2
//   /etc/mime.types
//   /etc/apache2/mime.types
//   /etc/apache/mime.types
//
// On Windows, MIME types are extracted from the registry.
//
// Text types have the charset parameter set to "utf-8" by default.
public static @string TypeByExtension(@string ext) {
    once.Do(initMime); 

    // Case-sensitive lookup.
    {
        var (v, ok) = mimeTypes.Load(ext);

        if (ok) {
            return v._<@string>();
        }
    } 

    // Case-insensitive lookup.
    // Optimistically assume a short ASCII extension and be
    // allocation-free in that case.
    array<byte> buf = new array<byte>(10);
    var lower = buf[..(int)0];
    const nuint utf8RuneSelf = 0x80; // from utf8 package, but not importing it.
 // from utf8 package, but not importing it.
    for (nint i = 0; i < len(ext); i++) {
        var c = ext[i];
        if (c >= utf8RuneSelf) { 
            // Slow path.
            var (si, _) = mimeTypesLower.Load(strings.ToLower(ext));
            @string (s, _) = si._<@string>();
            return s;

        }
        if ('A' <= c && c <= 'Z') {
            lower = append(lower, c + ('a' - 'A'));
        }
        else
 {
            lower = append(lower, c);
        }
    }
    (si, _) = mimeTypesLower.Load(string(lower));
    (s, _) = si._<@string>();
    return s;

}

// ExtensionsByType returns the extensions known to be associated with the MIME
// type typ. The returned extensions will each begin with a leading dot, as in
// ".html". When typ has no associated extensions, ExtensionsByType returns an
// nil slice.
public static (slice<@string>, error) ExtensionsByType(@string typ) {
    slice<@string> _p0 = default;
    error _p0 = default!;

    var (justType, _, err) = ParseMediaType(typ);
    if (err != null) {
        return (null, error.As(err)!);
    }
    once.Do(initMime);
    var (s, ok) = extensions.Load(justType);
    if (!ok) {
        return (null, error.As(null!)!);
    }
    var ret = append((slice<@string>)null, s._<slice<@string>>());
    sort.Strings(ret);
    return (ret, error.As(null!)!);

}

// AddExtensionType sets the MIME type associated with
// the extension ext to typ. The extension should begin with
// a leading dot, as in ".html".
public static error AddExtensionType(@string ext, @string typ) {
    if (!strings.HasPrefix(ext, ".")) {
        return error.As(fmt.Errorf("mime: extension %q missing leading dot", ext))!;
    }
    once.Do(initMime);
    return error.As(setExtensionType(ext, typ))!;

}

private static error setExtensionType(@string extension, @string mimeType) => func((defer, _, _) => {
    var (justType, param, err) = ParseMediaType(mimeType);
    if (err != null) {
        return error.As(err)!;
    }
    if (strings.HasPrefix(mimeType, "text/") && param["charset"] == "") {
        param["charset"] = "utf-8";
        mimeType = FormatMediaType(mimeType, param);
    }
    var extLower = strings.ToLower(extension);

    mimeTypes.Store(extension, mimeType);
    mimeTypesLower.Store(extLower, mimeType);

    extensionsMu.Lock();
    defer(extensionsMu.Unlock());
    slice<@string> exts = default;
    {
        var (ei, ok) = extensions.Load(justType);

        if (ok) {
            exts = ei._<slice<@string>>();
        }
    }

    foreach (var (_, v) in exts) {
        if (v == extLower) {
            return error.As(null!)!;
        }
    }    extensions.Store(justType, append(exts, extLower));
    return error.As(null!)!;

});

} // end mime_package
