// +build !windows

// package filepath -- go2cs converted at 2020 October 09 04:49:48 UTC
// import "path/filepath" ==> using filepath = go.path.filepath_package
// Original source: C:\Go\src\path\filepath\symlink_unix.go

using static go.builtin;

namespace go {
namespace path
{
    public static partial class filepath_package
    {
        private static (@string, error) evalSymlinks(@string path)
        {
            @string _p0 = default;
            error _p0 = default!;

            return walkSymlinks(path);
        }
    }
}}
