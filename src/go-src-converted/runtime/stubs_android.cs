// package runtime -- go2cs converted at 2020 August 29 08:21:02 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\stubs_android.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        // Return values of access/connect/socket are the return values of the syscall
        // (may encode error numbers).

        // int access(const char *, int)
        //go:noescape
        private static int access(ref byte name, int mode)
;

        // int connect(int, const struct sockaddr*, socklen_t)
        private static int connect(int fd, unsafe.Pointer addr, int len)
;

        // int socket(int, int, int)
        private static int socket(int domain, int typ, int prot)
;
    }
}
