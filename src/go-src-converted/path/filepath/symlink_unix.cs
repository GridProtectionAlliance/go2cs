// +build !windows

// package filepath -- go2cs converted at 2020 August 29 08:22:29 UTC
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
            return walkSymlinks(path);
        }
    }
}}
