// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using os = os_package;
using filepath = path.filepath_package;
using sync = sync_package;
using syscall = syscall_package;
using path;

partial class testenv_package {

internal static sync.Once symlinkOnce;

internal static error winSymlinkErr;

internal static void initWinHasSymlink() => func((defer, _) => {
    var (tmpdir, err) = os.MkdirTemp(""u8, "symtest"u8);
    if (err != default!) {
        throw panic("failed to create temp directory: "u8 + err.Error());
    }
    deferǃ(os.RemoveAll, tmpdir, defer);
    err = os.Symlink("target"u8, filepath.Join(tmpdir, "symlink"));
    if (err != default!) {
        err = err._<ж<os.LinkError>>().Err;
        var exprᴛ1 = err;
        if (exprᴛ1 == syscall.EWINDOWS || exprᴛ1 == syscall.ERROR_PRIVILEGE_NOT_HELD) {
            winSymlinkErr = err;
        }

    }
});

internal static (bool ok, @string reason) hasSymlink() {
    bool ok = default!;
    @string reason = default!;

    symlinkOnce.Do(initWinHasSymlink);
    var exprᴛ1 = winSymlinkErr;
    if (exprᴛ1 == default!) {
        return (true, "");
    }
    if (exprᴛ1 == syscall.EWINDOWS) {
        return (false, ": symlinks are not supported on your version of Windows");
    }
    if (exprᴛ1 == syscall.ERROR_PRIVILEGE_NOT_HELD) {
        return (false, ": you don't have enough privileges to create symlinks");
    }

    return (false, "");
}

} // end testenv_package
