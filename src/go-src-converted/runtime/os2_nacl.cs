// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 August 29 08:18:41 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\os2_nacl.go

using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static readonly long _NSIG = 32L;
        private static readonly long _SI_USER = 1L; 

        // native_client/src/trusted/service_runtime/include/sys/errno.h
        // The errors are mainly copied from Linux.
        private static readonly long _EPERM = 1L; /* Operation not permitted */
        private static readonly long _ENOENT = 2L; /* No such file or directory */
        private static readonly long _ESRCH = 3L; /* No such process */
        private static readonly long _EINTR = 4L; /* Interrupted system call */
        private static readonly long _EIO = 5L; /* I/O error */
        private static readonly long _ENXIO = 6L; /* No such device or address */
        private static readonly long _E2BIG = 7L; /* Argument list too long */
        private static readonly long _ENOEXEC = 8L; /* Exec format error */
        private static readonly long _EBADF = 9L; /* Bad file number */
        private static readonly long _ECHILD = 10L; /* No child processes */
        private static readonly long _EAGAIN = 11L;        /* Try again */
        // _ENOMEM is defined in mem_bsd.go for nacl.
        // _ENOMEM          = 12       /* Out of memory */
        private static readonly long _EACCES = 13L; /* Permission denied */
        private static readonly long _EFAULT = 14L; /* Bad address */
        private static readonly long _EBUSY = 16L; /* Device or resource busy */
        private static readonly long _EEXIST = 17L; /* File exists */
        private static readonly long _EXDEV = 18L; /* Cross-device link */
        private static readonly long _ENODEV = 19L; /* No such device */
        private static readonly long _ENOTDIR = 20L; /* Not a directory */
        private static readonly long _EISDIR = 21L; /* Is a directory */
        private static readonly long _EINVAL = 22L; /* Invalid argument */
        private static readonly long _ENFILE = 23L; /* File table overflow */
        private static readonly long _EMFILE = 24L; /* Too many open files */
        private static readonly long _ENOTTY = 25L; /* Not a typewriter */
        private static readonly long _EFBIG = 27L; /* File too large */
        private static readonly long _ENOSPC = 28L; /* No space left on device */
        private static readonly long _ESPIPE = 29L; /* Illegal seek */
        private static readonly long _EROFS = 30L; /* Read-only file system */
        private static readonly long _EMLINK = 31L; /* Too many links */
        private static readonly long _EPIPE = 32L; /* Broken pipe */
        private static readonly long _ENAMETOOLONG = 36L; /* File name too long */
        private static readonly long _ENOSYS = 38L; /* Function not implemented */
        private static readonly long _EDQUOT = 122L; /* Quota exceeded */
        private static readonly long _EDOM = 33L; /* Math arg out of domain of func */
        private static readonly long _ERANGE = 34L; /* Math result not representable */
        private static readonly long _EDEADLK = 35L; /* Deadlock condition */
        private static readonly long _ENOLCK = 37L; /* No record locks available */
        private static readonly long _ENOTEMPTY = 39L; /* Directory not empty */
        private static readonly long _ELOOP = 40L; /* Too many symbolic links */
        private static readonly long _ENOMSG = 42L; /* No message of desired type */
        private static readonly long _EIDRM = 43L; /* Identifier removed */
        private static readonly long _ECHRNG = 44L; /* Channel number out of range */
        private static readonly long _EL2NSYNC = 45L; /* Level 2 not synchronized */
        private static readonly long _EL3HLT = 46L; /* Level 3 halted */
        private static readonly long _EL3RST = 47L; /* Level 3 reset */
        private static readonly long _ELNRNG = 48L; /* Link number out of range */
        private static readonly long _EUNATCH = 49L; /* Protocol driver not attached */
        private static readonly long _ENOCSI = 50L; /* No CSI structure available */
        private static readonly long _EL2HLT = 51L; /* Level 2 halted */
        private static readonly long _EBADE = 52L; /* Invalid exchange */
        private static readonly long _EBADR = 53L; /* Invalid request descriptor */
        private static readonly long _EXFULL = 54L; /* Exchange full */
        private static readonly long _ENOANO = 55L; /* No anode */
        private static readonly long _EBADRQC = 56L; /* Invalid request code */
        private static readonly long _EBADSLT = 57L; /* Invalid slot */
        private static readonly var _EDEADLOCK = _EDEADLK; /* File locking deadlock error */
        private static readonly long _EBFONT = 59L; /* Bad font file fmt */
        private static readonly long _ENOSTR = 60L; /* Device not a stream */
        private static readonly long _ENODATA = 61L; /* No data (for no delay io) */
        private static readonly long _ETIME = 62L; /* Timer expired */
        private static readonly long _ENOSR = 63L; /* Out of streams resources */
        private static readonly long _ENONET = 64L; /* Machine is not on the network */
        private static readonly long _ENOPKG = 65L; /* Package not installed */
        private static readonly long _EREMOTE = 66L; /* The object is remote */
        private static readonly long _ENOLINK = 67L; /* The link has been severed */
        private static readonly long _EADV = 68L; /* Advertise error */
        private static readonly long _ESRMNT = 69L; /* Srmount error */
        private static readonly long _ECOMM = 70L; /* Communication error on send */
        private static readonly long _EPROTO = 71L; /* Protocol error */
        private static readonly long _EMULTIHOP = 72L; /* Multihop attempted */
        private static readonly long _EDOTDOT = 73L; /* Cross mount point (not really error) */
        private static readonly long _EBADMSG = 74L; /* Trying to read unreadable message */
        private static readonly long _EOVERFLOW = 75L; /* Value too large for defined data type */
        private static readonly long _ENOTUNIQ = 76L; /* Given log. name not unique */
        private static readonly long _EBADFD = 77L; /* f.d. invalid for this operation */
        private static readonly long _EREMCHG = 78L; /* Remote address changed */
        private static readonly long _ELIBACC = 79L; /* Can't access a needed shared lib */
        private static readonly long _ELIBBAD = 80L; /* Accessing a corrupted shared lib */
        private static readonly long _ELIBSCN = 81L; /* .lib section in a.out corrupted */
        private static readonly long _ELIBMAX = 82L; /* Attempting to link in too many libs */
        private static readonly long _ELIBEXEC = 83L; /* Attempting to exec a shared library */
        private static readonly long _EILSEQ = 84L;
        private static readonly long _EUSERS = 87L;
        private static readonly long _ENOTSOCK = 88L; /* Socket operation on non-socket */
        private static readonly long _EDESTADDRREQ = 89L; /* Destination address required */
        private static readonly long _EMSGSIZE = 90L; /* Message too long */
        private static readonly long _EPROTOTYPE = 91L; /* Protocol wrong type for socket */
        private static readonly long _ENOPROTOOPT = 92L; /* Protocol not available */
        private static readonly long _EPROTONOSUPPORT = 93L; /* Unknown protocol */
        private static readonly long _ESOCKTNOSUPPORT = 94L; /* Socket type not supported */
        private static readonly long _EOPNOTSUPP = 95L; /* Operation not supported on transport endpoint */
        private static readonly long _EPFNOSUPPORT = 96L; /* Protocol family not supported */
        private static readonly long _EAFNOSUPPORT = 97L; /* Address family not supported by protocol family */
        private static readonly long _EADDRINUSE = 98L; /* Address already in use */
        private static readonly long _EADDRNOTAVAIL = 99L; /* Address not available */
        private static readonly long _ENETDOWN = 100L; /* Network interface is not configured */
        private static readonly long _ENETUNREACH = 101L; /* Network is unreachable */
        private static readonly long _ENETRESET = 102L;
        private static readonly long _ECONNABORTED = 103L; /* Connection aborted */
        private static readonly long _ECONNRESET = 104L; /* Connection reset by peer */
        private static readonly long _ENOBUFS = 105L; /* No buffer space available */
        private static readonly long _EISCONN = 106L; /* Socket is already connected */
        private static readonly long _ENOTCONN = 107L; /* Socket is not connected */
        private static readonly long _ESHUTDOWN = 108L; /* Can't send after socket shutdown */
        private static readonly long _ETOOMANYREFS = 109L;
        private static readonly long _ETIMEDOUT = 110L; /* Connection timed out */
        private static readonly long _ECONNREFUSED = 111L; /* Connection refused */
        private static readonly long _EHOSTDOWN = 112L; /* Host is down */
        private static readonly long _EHOSTUNREACH = 113L; /* Host is unreachable */
        private static readonly long _EALREADY = 114L; /* Socket already connected */
        private static readonly long _EINPROGRESS = 115L; /* Connection already in progress */
        private static readonly long _ESTALE = 116L;
        private static readonly var _ENOTSUP = _EOPNOTSUPP; /* Not supported */
        private static readonly long _ENOMEDIUM = 123L; /* No medium (in tape drive) */
        private static readonly long _ECANCELED = 125L; /* Operation canceled. */
        private static readonly long _ELBIN = 2048L; /* Inode is remote (not really error) */
        private static readonly long _EFTYPE = 2049L; /* Inappropriate file type or format */
        private static readonly long _ENMFILE = 2050L; /* No more files */
        private static readonly long _EPROCLIM = 2051L;
        private static readonly long _ENOSHARE = 2052L; /* No such host or network path */
        private static readonly long _ECASECLASH = 2053L; /* Filename exists with different case */
        private static readonly var _EWOULDBLOCK = _EAGAIN;        /* Operation would block */

        // native_client/src/trusted/service_runtime/include/bits/mman.h.
        // NOTE: DO NOT USE native_client/src/shared/imc/nacl_imc_c.h.
        // Those MAP_*values are different from these.
        private static readonly ulong _PROT_NONE = 0x0UL;
        private static readonly ulong _PROT_READ = 0x1UL;
        private static readonly ulong _PROT_WRITE = 0x2UL;
        private static readonly ulong _PROT_EXEC = 0x4UL;

        private static readonly ulong _MAP_SHARED = 0x1UL;
        private static readonly ulong _MAP_PRIVATE = 0x2UL;
        private static readonly ulong _MAP_FIXED = 0x10UL;
        private static readonly ulong _MAP_ANON = 0x20UL;

        private static readonly long _MADV_FREE = 0L;
        private static readonly long _SIGFPE = 8L;
        private static readonly long _FPE_INTDIV = 0L;

        private partial struct siginfo
        {
        }
    }
}
