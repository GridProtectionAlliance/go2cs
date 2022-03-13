//go:build !windows
// +build !windows

// package filepath -- go2cs converted at 2022 March 13 05:28:17 UTC
// import "path/filepath" ==> using filepath = go.path.filepath_package
// Original source: C:\Program Files\Go\src\path\filepath\symlink_unix.go
namespace go.path;

public static partial class filepath_package {

private static (@string, error) evalSymlinks(@string path) {
    @string _p0 = default;
    error _p0 = default!;

    return walkSymlinks(path);
}

} // end filepath_package
