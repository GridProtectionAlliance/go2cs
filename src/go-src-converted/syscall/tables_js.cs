// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build js,wasm

// package syscall -- go2cs converted at 2020 October 09 05:02:02 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\tables_js.go
using runtime = go.runtime_package;
using static go.builtin;

namespace go
{
    public static partial class syscall_package
    {
        // These were originally used by Nacl, then later also used by
        // js/wasm. Now that they're only used by js/wasm, these numbers are
        // just arbitrary.
        //
        // TODO: delete? replace with something meaningful?
        private static readonly long sys_null = (long)1L;
        private static readonly long sys_nameservice = (long)2L;
        private static readonly long sys_dup = (long)8L;
        private static readonly long sys_dup2 = (long)9L;
        private static readonly long sys_open = (long)10L;
        private static readonly long sys_close = (long)11L;
        private static readonly long sys_read = (long)12L;
        private static readonly long sys_write = (long)13L;
        private static readonly long sys_lseek = (long)14L;
        private static readonly long sys_stat = (long)16L;
        private static readonly long sys_fstat = (long)17L;
        private static readonly long sys_chmod = (long)18L;
        private static readonly long sys_isatty = (long)19L;
        private static readonly long sys_brk = (long)20L;
        private static readonly long sys_mmap = (long)21L;
        private static readonly long sys_munmap = (long)22L;
        private static readonly long sys_getdents = (long)23L;
        private static readonly long sys_mprotect = (long)24L;
        private static readonly long sys_list_mappings = (long)25L;
        private static readonly long sys_exit = (long)30L;
        private static readonly long sys_getpid = (long)31L;
        private static readonly long sys_sched_yield = (long)32L;
        private static readonly long sys_sysconf = (long)33L;
        private static readonly long sys_gettimeofday = (long)40L;
        private static readonly long sys_clock = (long)41L;
        private static readonly long sys_nanosleep = (long)42L;
        private static readonly long sys_clock_getres = (long)43L;
        private static readonly long sys_clock_gettime = (long)44L;
        private static readonly long sys_mkdir = (long)45L;
        private static readonly long sys_rmdir = (long)46L;
        private static readonly long sys_chdir = (long)47L;
        private static readonly long sys_getcwd = (long)48L;
        private static readonly long sys_unlink = (long)49L;
        private static readonly long sys_imc_makeboundsock = (long)60L;
        private static readonly long sys_imc_accept = (long)61L;
        private static readonly long sys_imc_connect = (long)62L;
        private static readonly long sys_imc_sendmsg = (long)63L;
        private static readonly long sys_imc_recvmsg = (long)64L;
        private static readonly long sys_imc_mem_obj_create = (long)65L;
        private static readonly long sys_imc_socketpair = (long)66L;
        private static readonly long sys_mutex_create = (long)70L;
        private static readonly long sys_mutex_lock = (long)71L;
        private static readonly long sys_mutex_trylock = (long)72L;
        private static readonly long sys_mutex_unlock = (long)73L;
        private static readonly long sys_cond_create = (long)74L;
        private static readonly long sys_cond_wait = (long)75L;
        private static readonly long sys_cond_signal = (long)76L;
        private static readonly long sys_cond_broadcast = (long)77L;
        private static readonly long sys_cond_timed_wait_abs = (long)79L;
        private static readonly long sys_thread_create = (long)80L;
        private static readonly long sys_thread_exit = (long)81L;
        private static readonly long sys_tls_init = (long)82L;
        private static readonly long sys_thread_nice = (long)83L;
        private static readonly long sys_tls_get = (long)84L;
        private static readonly long sys_second_tls_set = (long)85L;
        private static readonly long sys_second_tls_get = (long)86L;
        private static readonly long sys_exception_handler = (long)87L;
        private static readonly long sys_exception_stack = (long)88L;
        private static readonly long sys_exception_clear_flag = (long)89L;
        private static readonly long sys_sem_create = (long)100L;
        private static readonly long sys_sem_wait = (long)101L;
        private static readonly long sys_sem_post = (long)102L;
        private static readonly long sys_sem_get_value = (long)103L;
        private static readonly long sys_dyncode_create = (long)104L;
        private static readonly long sys_dyncode_modify = (long)105L;
        private static readonly long sys_dyncode_delete = (long)106L;
        private static readonly long sys_test_infoleak = (long)109L;
        private static readonly long sys_test_crash = (long)110L;
        private static readonly long sys_test_syscall_1 = (long)111L;
        private static readonly long sys_test_syscall_2 = (long)112L;
        private static readonly long sys_futex_wait_abs = (long)120L;
        private static readonly long sys_futex_wake = (long)121L;
        private static readonly long sys_pread = (long)130L;
        private static readonly long sys_pwrite = (long)131L;
        private static readonly long sys_truncate = (long)140L;
        private static readonly long sys_lstat = (long)141L;
        private static readonly long sys_link = (long)142L;
        private static readonly long sys_rename = (long)143L;
        private static readonly long sys_symlink = (long)144L;
        private static readonly long sys_access = (long)145L;
        private static readonly long sys_readlink = (long)146L;
        private static readonly long sys_utimes = (long)147L;
        private static readonly long sys_get_random_bytes = (long)150L;


        // TODO: Auto-generate some day. (Hard-coded in binaries so not likely to change.)
 
        // native_client/src/trusted/service_runtime/include/sys/errno.h
        // The errors are mainly copied from Linux.
        public static readonly Errno EPERM = (Errno)1L; /* Operation not permitted */
        public static readonly Errno ENOENT = (Errno)2L; /* No such file or directory */
        public static readonly Errno ESRCH = (Errno)3L; /* No such process */
        public static readonly Errno EINTR = (Errno)4L; /* Interrupted system call */
        public static readonly Errno EIO = (Errno)5L; /* I/O error */
        public static readonly Errno ENXIO = (Errno)6L; /* No such device or address */
        public static readonly Errno E2BIG = (Errno)7L; /* Argument list too long */
        public static readonly Errno ENOEXEC = (Errno)8L; /* Exec format error */
        public static readonly Errno EBADF = (Errno)9L; /* Bad file number */
        public static readonly Errno ECHILD = (Errno)10L; /* No child processes */
        public static readonly Errno EAGAIN = (Errno)11L; /* Try again */
        public static readonly Errno ENOMEM = (Errno)12L; /* Out of memory */
        public static readonly Errno EACCES = (Errno)13L; /* Permission denied */
        public static readonly Errno EFAULT = (Errno)14L; /* Bad address */
        public static readonly Errno EBUSY = (Errno)16L; /* Device or resource busy */
        public static readonly Errno EEXIST = (Errno)17L; /* File exists */
        public static readonly Errno EXDEV = (Errno)18L; /* Cross-device link */
        public static readonly Errno ENODEV = (Errno)19L; /* No such device */
        public static readonly Errno ENOTDIR = (Errno)20L; /* Not a directory */
        public static readonly Errno EISDIR = (Errno)21L; /* Is a directory */
        public static readonly Errno EINVAL = (Errno)22L; /* Invalid argument */
        public static readonly Errno ENFILE = (Errno)23L; /* File table overflow */
        public static readonly Errno EMFILE = (Errno)24L; /* Too many open files */
        public static readonly Errno ENOTTY = (Errno)25L; /* Not a typewriter */
        public static readonly Errno EFBIG = (Errno)27L; /* File too large */
        public static readonly Errno ENOSPC = (Errno)28L; /* No space left on device */
        public static readonly Errno ESPIPE = (Errno)29L; /* Illegal seek */
        public static readonly Errno EROFS = (Errno)30L; /* Read-only file system */
        public static readonly Errno EMLINK = (Errno)31L; /* Too many links */
        public static readonly Errno EPIPE = (Errno)32L; /* Broken pipe */
        public static readonly Errno ENAMETOOLONG = (Errno)36L; /* File name too long */
        public static readonly Errno ENOSYS = (Errno)38L; /* Function not implemented */
        public static readonly Errno EDQUOT = (Errno)122L; /* Quota exceeded */
        public static readonly Errno EDOM = (Errno)33L; /* Math arg out of domain of func */
        public static readonly Errno ERANGE = (Errno)34L; /* Math result not representable */
        public static readonly Errno EDEADLK = (Errno)35L; /* Deadlock condition */
        public static readonly Errno ENOLCK = (Errno)37L; /* No record locks available */
        public static readonly Errno ENOTEMPTY = (Errno)39L; /* Directory not empty */
        public static readonly Errno ELOOP = (Errno)40L; /* Too many symbolic links */
        public static readonly Errno ENOMSG = (Errno)42L; /* No message of desired type */
        public static readonly Errno EIDRM = (Errno)43L; /* Identifier removed */
        public static readonly Errno ECHRNG = (Errno)44L; /* Channel number out of range */
        public static readonly Errno EL2NSYNC = (Errno)45L; /* Level 2 not synchronized */
        public static readonly Errno EL3HLT = (Errno)46L; /* Level 3 halted */
        public static readonly Errno EL3RST = (Errno)47L; /* Level 3 reset */
        public static readonly Errno ELNRNG = (Errno)48L; /* Link number out of range */
        public static readonly Errno EUNATCH = (Errno)49L; /* Protocol driver not attached */
        public static readonly Errno ENOCSI = (Errno)50L; /* No CSI structure available */
        public static readonly Errno EL2HLT = (Errno)51L; /* Level 2 halted */
        public static readonly Errno EBADE = (Errno)52L; /* Invalid exchange */
        public static readonly Errno EBADR = (Errno)53L; /* Invalid request descriptor */
        public static readonly Errno EXFULL = (Errno)54L; /* Exchange full */
        public static readonly Errno ENOANO = (Errno)55L; /* No anode */
        public static readonly Errno EBADRQC = (Errno)56L; /* Invalid request code */
        public static readonly Errno EBADSLT = (Errno)57L; /* Invalid slot */
        public static readonly Errno EDEADLOCK = (Errno)EDEADLK; /* File locking deadlock error */
        public static readonly Errno EBFONT = (Errno)59L; /* Bad font file fmt */
        public static readonly Errno ENOSTR = (Errno)60L; /* Device not a stream */
        public static readonly Errno ENODATA = (Errno)61L; /* No data (for no delay io) */
        public static readonly Errno ETIME = (Errno)62L; /* Timer expired */
        public static readonly Errno ENOSR = (Errno)63L; /* Out of streams resources */
        public static readonly Errno ENONET = (Errno)64L; /* Machine is not on the network */
        public static readonly Errno ENOPKG = (Errno)65L; /* Package not installed */
        public static readonly Errno EREMOTE = (Errno)66L; /* The object is remote */
        public static readonly Errno ENOLINK = (Errno)67L; /* The link has been severed */
        public static readonly Errno EADV = (Errno)68L; /* Advertise error */
        public static readonly Errno ESRMNT = (Errno)69L; /* Srmount error */
        public static readonly Errno ECOMM = (Errno)70L; /* Communication error on send */
        public static readonly Errno EPROTO = (Errno)71L; /* Protocol error */
        public static readonly Errno EMULTIHOP = (Errno)72L; /* Multihop attempted */
        public static readonly Errno EDOTDOT = (Errno)73L; /* Cross mount point (not really error) */
        public static readonly Errno EBADMSG = (Errno)74L; /* Trying to read unreadable message */
        public static readonly Errno EOVERFLOW = (Errno)75L; /* Value too large for defined data type */
        public static readonly Errno ENOTUNIQ = (Errno)76L; /* Given log. name not unique */
        public static readonly Errno EBADFD = (Errno)77L; /* f.d. invalid for this operation */
        public static readonly Errno EREMCHG = (Errno)78L; /* Remote address changed */
        public static readonly Errno ELIBACC = (Errno)79L; /* Can't access a needed shared lib */
        public static readonly Errno ELIBBAD = (Errno)80L; /* Accessing a corrupted shared lib */
        public static readonly Errno ELIBSCN = (Errno)81L; /* .lib section in a.out corrupted */
        public static readonly Errno ELIBMAX = (Errno)82L; /* Attempting to link in too many libs */
        public static readonly Errno ELIBEXEC = (Errno)83L; /* Attempting to exec a shared library */
        public static readonly Errno EILSEQ = (Errno)84L;
        public static readonly Errno EUSERS = (Errno)87L;
        public static readonly Errno ENOTSOCK = (Errno)88L; /* Socket operation on non-socket */
        public static readonly Errno EDESTADDRREQ = (Errno)89L; /* Destination address required */
        public static readonly Errno EMSGSIZE = (Errno)90L; /* Message too long */
        public static readonly Errno EPROTOTYPE = (Errno)91L; /* Protocol wrong type for socket */
        public static readonly Errno ENOPROTOOPT = (Errno)92L; /* Protocol not available */
        public static readonly Errno EPROTONOSUPPORT = (Errno)93L; /* Unknown protocol */
        public static readonly Errno ESOCKTNOSUPPORT = (Errno)94L; /* Socket type not supported */
        public static readonly Errno EOPNOTSUPP = (Errno)95L; /* Operation not supported on transport endpoint */
        public static readonly Errno EPFNOSUPPORT = (Errno)96L; /* Protocol family not supported */
        public static readonly Errno EAFNOSUPPORT = (Errno)97L; /* Address family not supported by protocol family */
        public static readonly Errno EADDRINUSE = (Errno)98L; /* Address already in use */
        public static readonly Errno EADDRNOTAVAIL = (Errno)99L; /* Address not available */
        public static readonly Errno ENETDOWN = (Errno)100L; /* Network interface is not configured */
        public static readonly Errno ENETUNREACH = (Errno)101L; /* Network is unreachable */
        public static readonly Errno ENETRESET = (Errno)102L;
        public static readonly Errno ECONNABORTED = (Errno)103L; /* Connection aborted */
        public static readonly Errno ECONNRESET = (Errno)104L; /* Connection reset by peer */
        public static readonly Errno ENOBUFS = (Errno)105L; /* No buffer space available */
        public static readonly Errno EISCONN = (Errno)106L; /* Socket is already connected */
        public static readonly Errno ENOTCONN = (Errno)107L; /* Socket is not connected */
        public static readonly Errno ESHUTDOWN = (Errno)108L; /* Can't send after socket shutdown */
        public static readonly Errno ETOOMANYREFS = (Errno)109L;
        public static readonly Errno ETIMEDOUT = (Errno)110L; /* Connection timed out */
        public static readonly Errno ECONNREFUSED = (Errno)111L; /* Connection refused */
        public static readonly Errno EHOSTDOWN = (Errno)112L; /* Host is down */
        public static readonly Errno EHOSTUNREACH = (Errno)113L; /* Host is unreachable */
        public static readonly Errno EALREADY = (Errno)114L; /* Socket already connected */
        public static readonly Errno EINPROGRESS = (Errno)115L; /* Connection already in progress */
        public static readonly Errno ESTALE = (Errno)116L;
        public static readonly Errno ENOTSUP = (Errno)EOPNOTSUPP; /* Not supported */
        public static readonly Errno ENOMEDIUM = (Errno)123L; /* No medium (in tape drive) */
        public static readonly Errno ECANCELED = (Errno)125L; /* Operation canceled. */
        public static readonly Errno ELBIN = (Errno)2048L; /* Inode is remote (not really error) */
        public static readonly Errno EFTYPE = (Errno)2049L; /* Inappropriate file type or format */
        public static readonly Errno ENMFILE = (Errno)2050L; /* No more files */
        public static readonly Errno EPROCLIM = (Errno)2051L;
        public static readonly Errno ENOSHARE = (Errno)2052L; /* No such host or network path */
        public static readonly Errno ECASECLASH = (Errno)2053L; /* Filename exists with different case */
        public static readonly Errno EWOULDBLOCK = (Errno)EAGAIN; /* Operation would block */

        // TODO: Auto-generate some day. (Hard-coded in binaries so not likely to change.)
        private static array<@string> errorstr = new array<@string>(InitKeyedValues<@string>((EPERM, "Operation not permitted"), (ENOENT, "No such file or directory"), (ESRCH, "No such process"), (EINTR, "Interrupted system call"), (EIO, "I/O error"), (ENXIO, "No such device or address"), (E2BIG, "Argument list too long"), (ENOEXEC, "Exec format error"), (EBADF, "Bad file number"), (ECHILD, "No child processes"), (EAGAIN, "Try again"), (ENOMEM, "Out of memory"), (EACCES, "Permission denied"), (EFAULT, "Bad address"), (EBUSY, "Device or resource busy"), (EEXIST, "File exists"), (EXDEV, "Cross-device link"), (ENODEV, "No such device"), (ENOTDIR, "Not a directory"), (EISDIR, "Is a directory"), (EINVAL, "Invalid argument"), (ENFILE, "File table overflow"), (EMFILE, "Too many open files"), (ENOTTY, "Not a typewriter"), (EFBIG, "File too large"), (ENOSPC, "No space left on device"), (ESPIPE, "Illegal seek"), (EROFS, "Read-only file system"), (EMLINK, "Too many links"), (EPIPE, "Broken pipe"), (ENAMETOOLONG, "File name too long"), (ENOSYS, "not implemented on "+runtime.GOOS), (EDQUOT, "Quota exceeded"), (EDOM, "Math arg out of domain of func"), (ERANGE, "Math result not representable"), (EDEADLK, "Deadlock condition"), (ENOLCK, "No record locks available"), (ENOTEMPTY, "Directory not empty"), (ELOOP, "Too many symbolic links"), (ENOMSG, "No message of desired type"), (EIDRM, "Identifier removed"), (ECHRNG, "Channel number out of range"), (EL2NSYNC, "Level 2 not synchronized"), (EL3HLT, "Level 3 halted"), (EL3RST, "Level 3 reset"), (ELNRNG, "Link number out of range"), (EUNATCH, "Protocol driver not attached"), (ENOCSI, "No CSI structure available"), (EL2HLT, "Level 2 halted"), (EBADE, "Invalid exchange"), (EBADR, "Invalid request descriptor"), (EXFULL, "Exchange full"), (ENOANO, "No anode"), (EBADRQC, "Invalid request code"), (EBADSLT, "Invalid slot"), (EBFONT, "Bad font file fmt"), (ENOSTR, "Device not a stream"), (ENODATA, "No data (for no delay io)"), (ETIME, "Timer expired"), (ENOSR, "Out of streams resources"), (ENONET, "Machine is not on the network"), (ENOPKG, "Package not installed"), (EREMOTE, "The object is remote"), (ENOLINK, "The link has been severed"), (EADV, "Advertise error"), (ESRMNT, "Srmount error"), (ECOMM, "Communication error on send"), (EPROTO, "Protocol error"), (EMULTIHOP, "Multihop attempted"), (EDOTDOT, "Cross mount point (not really error)"), (EBADMSG, "Trying to read unreadable message"), (EOVERFLOW, "Value too large for defined data type"), (ENOTUNIQ, "Given log. name not unique"), (EBADFD, "f.d. invalid for this operation"), (EREMCHG, "Remote address changed"), (ELIBACC, "Can't access a needed shared lib"), (ELIBBAD, "Accessing a corrupted shared lib"), (ELIBSCN, ".lib section in a.out corrupted"), (ELIBMAX, "Attempting to link in too many libs"), (ELIBEXEC, "Attempting to exec a shared library"), (ENOTSOCK, "Socket operation on non-socket"), (EDESTADDRREQ, "Destination address required"), (EMSGSIZE, "Message too long"), (EPROTOTYPE, "Protocol wrong type for socket"), (ENOPROTOOPT, "Protocol not available"), (EPROTONOSUPPORT, "Unknown protocol"), (ESOCKTNOSUPPORT, "Socket type not supported"), (EOPNOTSUPP, "Operation not supported on transport endpoint"), (EPFNOSUPPORT, "Protocol family not supported"), (EAFNOSUPPORT, "Address family not supported by protocol family"), (EADDRINUSE, "Address already in use"), (EADDRNOTAVAIL, "Address not available"), (ENETDOWN, "Network interface is not configured"), (ENETUNREACH, "Network is unreachable"), (ECONNABORTED, "Connection aborted"), (ECONNRESET, "Connection reset by peer"), (ENOBUFS, "No buffer space available"), (EISCONN, "Socket is already connected"), (ENOTCONN, "Socket is not connected"), (ESHUTDOWN, "Can't send after socket shutdown"), (ETIMEDOUT, "Connection timed out"), (ECONNREFUSED, "Connection refused"), (EHOSTDOWN, "Host is down"), (EHOSTUNREACH, "Host is unreachable"), (EALREADY, "Socket already connected"), (EINPROGRESS, "Connection already in progress"), (ENOMEDIUM, "No medium (in tape drive)"), (ECANCELED, "Operation canceled."), (ELBIN, "Inode is remote (not really error)"), (EFTYPE, "Inappropriate file type or format"), (ENMFILE, "No more files"), (ENOSHARE, "No such host or network path"), (ECASECLASH, "Filename exists with different case")));

        // Do the interface allocations only once for common
        // Errno values.
        private static error errEAGAIN = error.As(EAGAIN)!;        private static error errEINVAL = error.As(EINVAL)!;        private static error errENOENT = error.As(ENOENT)!;

        // errnoErr returns common boxed Errno values, to prevent
        // allocations at runtime.
        private static error errnoErr(Errno e)
        {

            if (e == 0L) 
                return error.As(null!)!;
            else if (e == EAGAIN) 
                return error.As(errEAGAIN)!;
            else if (e == EINVAL) 
                return error.As(errEINVAL)!;
            else if (e == ENOENT) 
                return error.As(errENOENT)!;
                        return error.As(e)!;

        }

        private static map errnoByCode = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, Errno>{"EPERM":EPERM,"ENOENT":ENOENT,"ESRCH":ESRCH,"EINTR":EINTR,"EIO":EIO,"ENXIO":ENXIO,"E2BIG":E2BIG,"ENOEXEC":ENOEXEC,"EBADF":EBADF,"ECHILD":ECHILD,"EAGAIN":EAGAIN,"ENOMEM":ENOMEM,"EACCES":EACCES,"EFAULT":EFAULT,"EBUSY":EBUSY,"EEXIST":EEXIST,"EXDEV":EXDEV,"ENODEV":ENODEV,"ENOTDIR":ENOTDIR,"EISDIR":EISDIR,"EINVAL":EINVAL,"ENFILE":ENFILE,"EMFILE":EMFILE,"ENOTTY":ENOTTY,"EFBIG":EFBIG,"ENOSPC":ENOSPC,"ESPIPE":ESPIPE,"EROFS":EROFS,"EMLINK":EMLINK,"EPIPE":EPIPE,"ENAMETOOLONG":ENAMETOOLONG,"ENOSYS":ENOSYS,"EDQUOT":EDQUOT,"EDOM":EDOM,"ERANGE":ERANGE,"EDEADLK":EDEADLK,"ENOLCK":ENOLCK,"ENOTEMPTY":ENOTEMPTY,"ELOOP":ELOOP,"ENOMSG":ENOMSG,"EIDRM":EIDRM,"ECHRNG":ECHRNG,"EL2NSYNC":EL2NSYNC,"EL3HLT":EL3HLT,"EL3RST":EL3RST,"ELNRNG":ELNRNG,"EUNATCH":EUNATCH,"ENOCSI":ENOCSI,"EL2HLT":EL2HLT,"EBADE":EBADE,"EBADR":EBADR,"EXFULL":EXFULL,"ENOANO":ENOANO,"EBADRQC":EBADRQC,"EBADSLT":EBADSLT,"EDEADLOCK":EDEADLOCK,"EBFONT":EBFONT,"ENOSTR":ENOSTR,"ENODATA":ENODATA,"ETIME":ETIME,"ENOSR":ENOSR,"ENONET":ENONET,"ENOPKG":ENOPKG,"EREMOTE":EREMOTE,"ENOLINK":ENOLINK,"EADV":EADV,"ESRMNT":ESRMNT,"ECOMM":ECOMM,"EPROTO":EPROTO,"EMULTIHOP":EMULTIHOP,"EDOTDOT":EDOTDOT,"EBADMSG":EBADMSG,"EOVERFLOW":EOVERFLOW,"ENOTUNIQ":ENOTUNIQ,"EBADFD":EBADFD,"EREMCHG":EREMCHG,"ELIBACC":ELIBACC,"ELIBBAD":ELIBBAD,"ELIBSCN":ELIBSCN,"ELIBMAX":ELIBMAX,"ELIBEXEC":ELIBEXEC,"EILSEQ":EILSEQ,"EUSERS":EUSERS,"ENOTSOCK":ENOTSOCK,"EDESTADDRREQ":EDESTADDRREQ,"EMSGSIZE":EMSGSIZE,"EPROTOTYPE":EPROTOTYPE,"ENOPROTOOPT":ENOPROTOOPT,"EPROTONOSUPPORT":EPROTONOSUPPORT,"ESOCKTNOSUPPORT":ESOCKTNOSUPPORT,"EOPNOTSUPP":EOPNOTSUPP,"EPFNOSUPPORT":EPFNOSUPPORT,"EAFNOSUPPORT":EAFNOSUPPORT,"EADDRINUSE":EADDRINUSE,"EADDRNOTAVAIL":EADDRNOTAVAIL,"ENETDOWN":ENETDOWN,"ENETUNREACH":ENETUNREACH,"ENETRESET":ENETRESET,"ECONNABORTED":ECONNABORTED,"ECONNRESET":ECONNRESET,"ENOBUFS":ENOBUFS,"EISCONN":EISCONN,"ENOTCONN":ENOTCONN,"ESHUTDOWN":ESHUTDOWN,"ETOOMANYREFS":ETOOMANYREFS,"ETIMEDOUT":ETIMEDOUT,"ECONNREFUSED":ECONNREFUSED,"EHOSTDOWN":EHOSTDOWN,"EHOSTUNREACH":EHOSTUNREACH,"EALREADY":EALREADY,"EINPROGRESS":EINPROGRESS,"ESTALE":ESTALE,"ENOTSUP":ENOTSUP,"ENOMEDIUM":ENOMEDIUM,"ECANCELED":ECANCELED,"ELBIN":ELBIN,"EFTYPE":EFTYPE,"ENMFILE":ENMFILE,"EPROCLIM":EPROCLIM,"ENOSHARE":ENOSHARE,"ECASECLASH":ECASECLASH,"EWOULDBLOCK":EWOULDBLOCK,};
    }
}
