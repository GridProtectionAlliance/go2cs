// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using os = os_package;
using filepath = path.filepath_package;
using sync = sync_package;
using Δsyscall = syscall_package;
using path;

partial class testenv_package {

internal static ж<sync.Once> ᏑsymlinkOnce = new(default(sync.Once));
internal static ref sync.Once symlinkOnce => ref ᏑsymlinkOnce.Value;

internal static error winSymlinkErr;

internal static void initWinHasSymlink() => func((defer, recover) => {
    var (tmpdir, err) = os.MkdirTemp(""u8, "symtest"u8);
    if (err != default!) {
        throw panic("failed to create temp directory: " + err.Error());
    }
    deferǃ(os.RemoveAll, tmpdir, defer);
    err = os.Symlink("target"u8, filepath.Join(tmpdir, "symlink"));
    if (err != default!) {
        err = err._<ж<os.LinkError>>().Value.Err;
        var exprᴛ1 = err;
        if (AreEqual(exprᴛ1, Δsyscall.EWINDOWS) || AreEqual(exprᴛ1, Δsyscall.ERROR_PRIVILEGE_NOT_HELD)) {
            winSymlinkErr = err;
        }

    }
});

internal static (bool ok, @string reason) hasSymlink() {
    bool ok = default!;
    @string reason = default!;

    ᏑsymlinkOnce.Do(initWinHasSymlink);
    var exprᴛ1 = winSymlinkErr;
    if (AreEqual(exprᴛ1, default!)) {
        return (true, "");
    }
    if (AreEqual(exprᴛ1, Δsyscall.EWINDOWS)) {
        return (false, ": symlinks are not supported on your version of Windows");
    }
    if (AreEqual(exprᴛ1, Δsyscall.ERROR_PRIVILEGE_NOT_HELD)) {
        return (false, ": you don't have enough privileges to create symlinks");
    }

    return (false, "");
}

} // end testenv_package
