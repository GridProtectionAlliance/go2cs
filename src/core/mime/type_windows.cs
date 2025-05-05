// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using registry = @internal.syscall.windows.registry_package;
using @internal.syscall.windows;

partial class mime_package {

[GoInit] internal static void init() {
    osInitMime = initMimeWindows;
}

internal static void initMimeWindows() {
    (names, err) = registry.CLASSES_ROOT.ReadSubKeyNames();
    if (err != default!) {
        return;
    }
    foreach (var (_, name) in names) {
        if (len(name) < 2 || name[0] != (rune)'.') {
            // looking for extensions only
            continue;
        }
        var (k, err) = registry.OpenKey(registry.CLASSES_ROOT, name, registry.READ);
        if (err != default!) {
            continue;
        }
        var (v, _, err) = k.GetStringValue("Content Type"u8);
        k.Close();
        if (err != default!) {
            continue;
        }
        // There is a long-standing problem on Windows: the
        // registry sometimes records that the ".js" extension
        // should be "text/plain". See issue #32350. While
        // normally local configuration should override
        // defaults, this problem is common enough that we
        // handle it here by ignoring that registry setting.
        if (name == ".js"u8 && (v == "text/plain"u8 || v == "text/plain; charset=utf-8"u8)) {
            continue;
        }
        setExtensionType(name, v);
    }
}

internal static map<@string, @string> initMimeForTests() {
    return new map<@string, @string>{
        [".PnG"u8] = "image/png"u8
    };
}

} // end mime_package
