// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package syscall -- go2cs converted at 2020 August 29 08:38:31 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\tables_nacl.go

using static go.builtin;

namespace go
{
    public static partial class syscall_package
    {
        // TODO: generate with runtime/mknacl.sh, allow override with IRT.
        private static readonly long sys_null = 1L;
        private static readonly long sys_nameservice = 2L;
        private static readonly long sys_dup = 8L;
        private static readonly long sys_dup2 = 9L;
        private static readonly long sys_open = 10L;
        private static readonly long sys_close = 11L;
        private static readonly long sys_read = 12L;
        private static readonly long sys_write = 13L;
        private static readonly long sys_lseek = 14L;
        private static readonly long sys_stat = 16L;
        private static readonly long sys_fstat = 17L;
        private static readonly long sys_chmod = 18L;
        private static readonly long sys_isatty = 19L;
        private static readonly long sys_brk = 20L;
        private static readonly long sys_mmap = 21L;
        private static readonly long sys_munmap = 22L;
        private static readonly long sys_getdents = 23L;
        private static readonly long sys_mprotect = 24L;
        private static readonly long sys_list_mappings = 25L;
        private static readonly long sys_exit = 30L;
        private static readonly long sys_getpid = 31L;
        private static readonly long sys_sched_yield = 32L;
        private static readonly long sys_sysconf = 33L;
        private static readonly long sys_gettimeofday = 40L;
        private static readonly long sys_clock = 41L;
        private static readonly long sys_nanosleep = 42L;
        private static readonly long sys_clock_getres = 43L;
        private static readonly long sys_clock_gettime = 44L;
        private static readonly long sys_mkdir = 45L;
        private static readonly long sys_rmdir = 46L;
        private static readonly long sys_chdir = 47L;
        private static readonly long sys_getcwd = 48L;
        private static readonly long sys_unlink = 49L;
        private static readonly long sys_imc_makeboundsock = 60L;
        private static readonly long sys_imc_accept = 61L;
        private static readonly long sys_imc_connect = 62L;
        private static readonly long sys_imc_sendmsg = 63L;
        private static readonly long sys_imc_recvmsg = 64L;
        private static readonly long sys_imc_mem_obj_create = 65L;
        private static readonly long sys_imc_socketpair = 66L;
        private static readonly long sys_mutex_create = 70L;
        private static readonly long sys_mutex_lock = 71L;
        private static readonly long sys_mutex_trylock = 72L;
        private static readonly long sys_mutex_unlock = 73L;
        private static readonly long sys_cond_create = 74L;
        private static readonly long sys_cond_wait = 75L;
        private static readonly long sys_cond_signal = 76L;
        private static readonly long sys_cond_broadcast = 77L;
        private static readonly long sys_cond_timed_wait_abs = 79L;
        private static readonly long sys_thread_create = 80L;
        private static readonly long sys_thread_exit = 81L;
        private static readonly long sys_tls_init = 82L;
        private static readonly long sys_thread_nice = 83L;
        private static readonly long sys_tls_get = 84L;
        private static readonly long sys_second_tls_set = 85L;
        private static readonly long sys_second_tls_get = 86L;
        private static readonly long sys_exception_handler = 87L;
        private static readonly long sys_exception_stack = 88L;
        private static readonly long sys_exception_clear_flag = 89L;
        private static readonly long sys_sem_create = 100L;
        private static readonly long sys_sem_wait = 101L;
        private static readonly long sys_sem_post = 102L;
        private static readonly long sys_sem_get_value = 103L;
        private static readonly long sys_dyncode_create = 104L;
        private static readonly long sys_dyncode_modify = 105L;
        private static readonly long sys_dyncode_delete = 106L;
        private static readonly long sys_test_infoleak = 109L;
        private static readonly long sys_test_crash = 110L;
        private static readonly long sys_test_syscall_1 = 111L;
        private static readonly long sys_test_syscall_2 = 112L;
        private static readonly long sys_futex_wait_abs = 120L;
        private static readonly long sys_futex_wake = 121L;
        private static readonly long sys_pread = 130L;
        private static readonly long sys_pwrite = 131L;
        private static readonly long sys_truncate = 140L;
        private static readonly long sys_lstat = 141L;
        private static readonly long sys_link = 142L;
        private static readonly long sys_rename = 143L;
        private static readonly long sys_symlink = 144L;
        private static readonly long sys_access = 145L;
        private static readonly long sys_readlink = 146L;
        private static readonly long sys_utimes = 147L;
        private static readonly long sys_get_random_bytes = 150L;

        // TODO: Auto-generate some day. (Hard-coded in binaries so not likely to change.)
 
        // native_client/src/trusted/service_runtime/include/sys/errno.h
        // The errors are mainly copied from Linux.
        public static readonly Errno EPERM = 1L; /* Operation not permitted */
        public static readonly Errno ENOENT = 2L; /* No such file or directory */
        public static readonly Errno ESRCH = 3L; /* No such process */
        public static readonly Errno EINTR = 4L; /* Interrupted system call */
        public static readonly Errno EIO = 5L; /* I/O error */
        public static readonly Errno ENXIO = 6L; /* No such device or address */
        public static readonly Errno E2BIG = 7L; /* Argument list too long */
        public static readonly Errno ENOEXEC = 8L; /* Exec format error */
        public static readonly Errno EBADF = 9L; /* Bad file number */
        public static readonly Errno ECHILD = 10L; /* No child processes */
        public static readonly Errno EAGAIN = 11L; /* Try again */
        public static readonly Errno ENOMEM = 12L; /* Out of memory */
        public static readonly Errno EACCES = 13L; /* Permission denied */
        public static readonly Errno EFAULT = 14L; /* Bad address */
        public static readonly Errno EBUSY = 16L; /* Device or resource busy */
        public static readonly Errno EEXIST = 17L; /* File exists */
        public static readonly Errno EXDEV = 18L; /* Cross-device link */
        public static readonly Errno ENODEV = 19L; /* No such device */
        public static readonly Errno ENOTDIR = 20L; /* Not a directory */
        public static readonly Errno EISDIR = 21L; /* Is a directory */
        public static readonly Errno EINVAL = 22L; /* Invalid argument */
        public static readonly Errno ENFILE = 23L; /* File table overflow */
        public static readonly Errno EMFILE = 24L; /* Too many open files */
        public static readonly Errno ENOTTY = 25L; /* Not a typewriter */
        public static readonly Errno EFBIG = 27L; /* File too large */
        public static readonly Errno ENOSPC = 28L; /* No space left on device */
        public static readonly Errno ESPIPE = 29L; /* Illegal seek */
        public static readonly Errno EROFS = 30L; /* Read-only file system */
        public static readonly Errno EMLINK = 31L; /* Too many links */
        public static readonly Errno EPIPE = 32L; /* Broken pipe */
        public static readonly Errno ENAMETOOLONG = 36L; /* File name too long */
        public static readonly Errno ENOSYS = 38L; /* Function not implemented */
        public static readonly Errno EDQUOT = 122L; /* Quota exceeded */
        public static readonly Errno EDOM = 33L; /* Math arg out of domain of func */
        public static readonly Errno ERANGE = 34L; /* Math result not representable */
        public static readonly Errno EDEADLK = 35L; /* Deadlock condition */
        public static readonly Errno ENOLCK = 37L; /* No record locks available */
        public static readonly Errno ENOTEMPTY = 39L; /* Directory not empty */
        public static readonly Errno ELOOP = 40L; /* Too many symbolic links */
        public static readonly Errno ENOMSG = 42L; /* No message of desired type */
        public static readonly Errno EIDRM = 43L; /* Identifier removed */
        public static readonly Errno ECHRNG = 44L; /* Channel number out of range */
        public static readonly Errno EL2NSYNC = 45L; /* Level 2 not synchronized */
        public static readonly Errno EL3HLT = 46L; /* Level 3 halted */
        public static readonly Errno EL3RST = 47L; /* Level 3 reset */
        public static readonly Errno ELNRNG = 48L; /* Link number out of range */
        public static readonly Errno EUNATCH = 49L; /* Protocol driver not attached */
        public static readonly Errno ENOCSI = 50L; /* No CSI structure available */
        public static readonly Errno EL2HLT = 51L; /* Level 2 halted */
        public static readonly Errno EBADE = 52L; /* Invalid exchange */
        public static readonly Errno EBADR = 53L; /* Invalid request descriptor */
        public static readonly Errno EXFULL = 54L; /* Exchange full */
        public static readonly Errno ENOANO = 55L; /* No anode */
        public static readonly Errno EBADRQC = 56L; /* Invalid request code */
        public static readonly Errno EBADSLT = 57L; /* Invalid slot */
        public static readonly Errno EDEADLOCK = EDEADLK; /* File locking deadlock error */
        public static readonly Errno EBFONT = 59L; /* Bad font file fmt */
        public static readonly Errno ENOSTR = 60L; /* Device not a stream */
        public static readonly Errno ENODATA = 61L; /* No data (for no delay io) */
        public static readonly Errno ETIME = 62L; /* Timer expired */
        public static readonly Errno ENOSR = 63L; /* Out of streams resources */
        public static readonly Errno ENONET = 64L; /* Machine is not on the network */
        public static readonly Errno ENOPKG = 65L; /* Package not installed */
        public static readonly Errno EREMOTE = 66L; /* The object is remote */
        public static readonly Errno ENOLINK = 67L; /* The link has been severed */
        public static readonly Errno EADV = 68L; /* Advertise error */
        public static readonly Errno ESRMNT = 69L; /* Srmount error */
        public static readonly Errno ECOMM = 70L; /* Communication error on send */
        public static readonly Errno EPROTO = 71L; /* Protocol error */
        public static readonly Errno EMULTIHOP = 72L; /* Multihop attempted */
        public static readonly Errno EDOTDOT = 73L; /* Cross mount point (not really error) */
        public static readonly Errno EBADMSG = 74L; /* Trying to read unreadable message */
        public static readonly Errno EOVERFLOW = 75L; /* Value too large for defined data type */
        public static readonly Errno ENOTUNIQ = 76L; /* Given log. name not unique */
        public static readonly Errno EBADFD = 77L; /* f.d. invalid for this operation */
        public static readonly Errno EREMCHG = 78L; /* Remote address changed */
        public static readonly Errno ELIBACC = 79L; /* Can't access a needed shared lib */
        public static readonly Errno ELIBBAD = 80L; /* Accessing a corrupted shared lib */
        public static readonly Errno ELIBSCN = 81L; /* .lib section in a.out corrupted */
        public static readonly Errno ELIBMAX = 82L; /* Attempting to link in too many libs */
        public static readonly Errno ELIBEXEC = 83L; /* Attempting to exec a shared library */
        public static readonly Errno EILSEQ = 84L;
        public static readonly Errno EUSERS = 87L;
        public static readonly Errno ENOTSOCK = 88L; /* Socket operation on non-socket */
        public static readonly Errno EDESTADDRREQ = 89L; /* Destination address required */
        public static readonly Errno EMSGSIZE = 90L; /* Message too long */
        public static readonly Errno EPROTOTYPE = 91L; /* Protocol wrong type for socket */
        public static readonly Errno ENOPROTOOPT = 92L; /* Protocol not available */
        public static readonly Errno EPROTONOSUPPORT = 93L; /* Unknown protocol */
        public static readonly Errno ESOCKTNOSUPPORT = 94L; /* Socket type not supported */
        public static readonly Errno EOPNOTSUPP = 95L; /* Operation not supported on transport endpoint */
        public static readonly Errno EPFNOSUPPORT = 96L; /* Protocol family not supported */
        public static readonly Errno EAFNOSUPPORT = 97L; /* Address family not supported by protocol family */
        public static readonly Errno EADDRINUSE = 98L; /* Address already in use */
        public static readonly Errno EADDRNOTAVAIL = 99L; /* Address not available */
        public static readonly Errno ENETDOWN = 100L; /* Network interface is not configured */
        public static readonly Errno ENETUNREACH = 101L; /* Network is unreachable */
        public static readonly Errno ENETRESET = 102L;
        public static readonly Errno ECONNABORTED = 103L; /* Connection aborted */
        public static readonly Errno ECONNRESET = 104L; /* Connection reset by peer */
        public static readonly Errno ENOBUFS = 105L; /* No buffer space available */
        public static readonly Errno EISCONN = 106L; /* Socket is already connected */
        public static readonly Errno ENOTCONN = 107L; /* Socket is not connected */
        public static readonly Errno ESHUTDOWN = 108L; /* Can't send after socket shutdown */
        public static readonly Errno ETOOMANYREFS = 109L;
        public static readonly Errno ETIMEDOUT = 110L; /* Connection timed out */
        public static readonly Errno ECONNREFUSED = 111L; /* Connection refused */
        public static readonly Errno EHOSTDOWN = 112L; /* Host is down */
        public static readonly Errno EHOSTUNREACH = 113L; /* Host is unreachable */
        public static readonly Errno EALREADY = 114L; /* Socket already connected */
        public static readonly Errno EINPROGRESS = 115L; /* Connection already in progress */
        public static readonly Errno ESTALE = 116L;
        public static readonly Errno ENOTSUP = EOPNOTSUPP; /* Not supported */
        public static readonly Errno ENOMEDIUM = 123L; /* No medium (in tape drive) */
        public static readonly Errno ECANCELED = 125L; /* Operation canceled. */
        public static readonly Errno ELBIN = 2048L; /* Inode is remote (not really error) */
        public static readonly Errno EFTYPE = 2049L; /* Inappropriate file type or format */
        public static readonly Errno ENMFILE = 2050L; /* No more files */
        public static readonly Errno EPROCLIM = 2051L;
        public static readonly Errno ENOSHARE = 2052L; /* No such host or network path */
        public static readonly Errno ECASECLASH = 2053L; /* Filename exists with different case */
        public static readonly Errno EWOULDBLOCK = EAGAIN; /* Operation would block */

        // TODO: Auto-generate some day. (Hard-coded in binaries so not likely to change.)
        private static array<@string> errorstr = new array<@string>(InitKeyedValues<@string>((EPERM, "Operation not permitted"), (ENOENT, "No such file or directory"), (ESRCH, "No such process"), (EINTR, "Interrupted system call"), (EIO, "I/O error"), (ENXIO, "No such device or address"), (E2BIG, "Argument list too long"), (ENOEXEC, "Exec format error"), (EBADF, "Bad file number"), (ECHILD, "No child processes"), (EAGAIN, "Try again"), (ENOMEM, "Out of memory"), (EACCES, "Permission denied"), (EFAULT, "Bad address"), (EBUSY, "Device or resource busy"), (EEXIST, "File exists"), (EXDEV, "Cross-device link"), (ENODEV, "No such device"), (ENOTDIR, "Not a directory"), (EISDIR, "Is a directory"), (EINVAL, "Invalid argument"), (ENFILE, "File table overflow"), (EMFILE, "Too many open files"), (ENOTTY, "Not a typewriter"), (EFBIG, "File too large"), (ENOSPC, "No space left on device"), (ESPIPE, "Illegal seek"), (EROFS, "Read-only file system"), (EMLINK, "Too many links"), (EPIPE, "Broken pipe"), (ENAMETOOLONG, "File name too long"), (ENOSYS, "not implemented on Native Client"), (EDQUOT, "Quota exceeded"), (EDOM, "Math arg out of domain of func"), (ERANGE, "Math result not representable"), (EDEADLK, "Deadlock condition"), (ENOLCK, "No record locks available"), (ENOTEMPTY, "Directory not empty"), (ELOOP, "Too many symbolic links"), (ENOMSG, "No message of desired type"), (EIDRM, "Identifier removed"), (ECHRNG, "Channel number out of range"), (EL2NSYNC, "Level 2 not synchronized"), (EL3HLT, "Level 3 halted"), (EL3RST, "Level 3 reset"), (ELNRNG, "Link number out of range"), (EUNATCH, "Protocol driver not attached"), (ENOCSI, "No CSI structure available"), (EL2HLT, "Level 2 halted"), (EBADE, "Invalid exchange"), (EBADR, "Invalid request descriptor"), (EXFULL, "Exchange full"), (ENOANO, "No anode"), (EBADRQC, "Invalid request code"), (EBADSLT, "Invalid slot"), (EBFONT, "Bad font file fmt"), (ENOSTR, "Device not a stream"), (ENODATA, "No data (for no delay io)"), (ETIME, "Timer expired"), (ENOSR, "Out of streams resources"), (ENONET, "Machine is not on the network"), (ENOPKG, "Package not installed"), (EREMOTE, "The object is remote"), (ENOLINK, "The link has been severed"), (EADV, "Advertise error"), (ESRMNT, "Srmount error"), (ECOMM, "Communication error on send"), (EPROTO, "Protocol error"), (EMULTIHOP, "Multihop attempted"), (EDOTDOT, "Cross mount point (not really error)"), (EBADMSG, "Trying to read unreadable message"), (EOVERFLOW, "Value too large for defined data type"), (ENOTUNIQ, "Given log. name not unique"), (EBADFD, "f.d. invalid for this operation"), (EREMCHG, "Remote address changed"), (ELIBACC, "Can't access a needed shared lib"), (ELIBBAD, "Accessing a corrupted shared lib"), (ELIBSCN, ".lib section in a.out corrupted"), (ELIBMAX, "Attempting to link in too many libs"), (ELIBEXEC, "Attempting to exec a shared library"), (ENOTSOCK, "Socket operation on non-socket"), (EDESTADDRREQ, "Destination address required"), (EMSGSIZE, "Message too long"), (EPROTOTYPE, "Protocol wrong type for socket"), (ENOPROTOOPT, "Protocol not available"), (EPROTONOSUPPORT, "Unknown protocol"), (ESOCKTNOSUPPORT, "Socket type not supported"), (EOPNOTSUPP, "Operation not supported on transport endpoint"), (EPFNOSUPPORT, "Protocol family not supported"), (EAFNOSUPPORT, "Address family not supported by protocol family"), (EADDRINUSE, "Address already in use"), (EADDRNOTAVAIL, "Address not available"), (ENETDOWN, "Network interface is not configured"), (ENETUNREACH, "Network is unreachable"), (ECONNABORTED, "Connection aborted"), (ECONNRESET, "Connection reset by peer"), (ENOBUFS, "No buffer space available"), (EISCONN, "Socket is already connected"), (ENOTCONN, "Socket is not connected"), (ESHUTDOWN, "Can't send after socket shutdown"), (ETIMEDOUT, "Connection timed out"), (ECONNREFUSED, "Connection refused"), (EHOSTDOWN, "Host is down"), (EHOSTUNREACH, "Host is unreachable"), (EALREADY, "Socket already connected"), (EINPROGRESS, "Connection already in progress"), (ENOMEDIUM, "No medium (in tape drive)"), (ECANCELED, "Operation canceled."), (ELBIN, "Inode is remote (not really error)"), (EFTYPE, "Inappropriate file type or format"), (ENMFILE, "No more files"), (ENOSHARE, "No such host or network path"), (ECASECLASH, "Filename exists with different case")));

        // Do the interface allocations only once for common
        // Errno values.
        private static error errEAGAIN = error.As(EAGAIN);        private static error errEINVAL = error.As(EINVAL);        private static error errENOENT = error.As(ENOENT);

        // errnoErr returns common boxed Errno values, to prevent
        // allocations at runtime.
        private static error errnoErr(Errno e)
        {

            if (e == 0L) 
                return error.As(null);
            else if (e == EAGAIN) 
                return error.As(errEAGAIN);
            else if (e == EINVAL) 
                return error.As(errEINVAL);
            else if (e == ENOENT) 
                return error.As(errENOENT);
                        return error.As(e);
        }
    }
}
