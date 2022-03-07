// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build js && wasm
// +build js,wasm

// package syscall -- go2cs converted at 2022 March 06 22:27:18 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Program Files\Go\src\syscall\tables_js.go
using runtime = go.runtime_package;

namespace go;

public static partial class syscall_package {

    // These were originally used by Nacl, then later also used by
    // js/wasm. Now that they're only used by js/wasm, these numbers are
    // just arbitrary.
    //
    // TODO: delete? replace with something meaningful?
private static readonly nint sys_null = 1;
private static readonly nint sys_nameservice = 2;
private static readonly nint sys_dup = 8;
private static readonly nint sys_dup2 = 9;
private static readonly nint sys_open = 10;
private static readonly nint sys_close = 11;
private static readonly nint sys_read = 12;
private static readonly nint sys_write = 13;
private static readonly nint sys_lseek = 14;
private static readonly nint sys_stat = 16;
private static readonly nint sys_fstat = 17;
private static readonly nint sys_chmod = 18;
private static readonly nint sys_isatty = 19;
private static readonly nint sys_brk = 20;
private static readonly nint sys_mmap = 21;
private static readonly nint sys_munmap = 22;
private static readonly nint sys_getdents = 23;
private static readonly nint sys_mprotect = 24;
private static readonly nint sys_list_mappings = 25;
private static readonly nint sys_exit = 30;
private static readonly nint sys_getpid = 31;
private static readonly nint sys_sched_yield = 32;
private static readonly nint sys_sysconf = 33;
private static readonly nint sys_gettimeofday = 40;
private static readonly nint sys_clock = 41;
private static readonly nint sys_nanosleep = 42;
private static readonly nint sys_clock_getres = 43;
private static readonly nint sys_clock_gettime = 44;
private static readonly nint sys_mkdir = 45;
private static readonly nint sys_rmdir = 46;
private static readonly nint sys_chdir = 47;
private static readonly nint sys_getcwd = 48;
private static readonly nint sys_unlink = 49;
private static readonly nint sys_imc_makeboundsock = 60;
private static readonly nint sys_imc_accept = 61;
private static readonly nint sys_imc_connect = 62;
private static readonly nint sys_imc_sendmsg = 63;
private static readonly nint sys_imc_recvmsg = 64;
private static readonly nint sys_imc_mem_obj_create = 65;
private static readonly nint sys_imc_socketpair = 66;
private static readonly nint sys_mutex_create = 70;
private static readonly nint sys_mutex_lock = 71;
private static readonly nint sys_mutex_trylock = 72;
private static readonly nint sys_mutex_unlock = 73;
private static readonly nint sys_cond_create = 74;
private static readonly nint sys_cond_wait = 75;
private static readonly nint sys_cond_signal = 76;
private static readonly nint sys_cond_broadcast = 77;
private static readonly nint sys_cond_timed_wait_abs = 79;
private static readonly nint sys_thread_create = 80;
private static readonly nint sys_thread_exit = 81;
private static readonly nint sys_tls_init = 82;
private static readonly nint sys_thread_nice = 83;
private static readonly nint sys_tls_get = 84;
private static readonly nint sys_second_tls_set = 85;
private static readonly nint sys_second_tls_get = 86;
private static readonly nint sys_exception_handler = 87;
private static readonly nint sys_exception_stack = 88;
private static readonly nint sys_exception_clear_flag = 89;
private static readonly nint sys_sem_create = 100;
private static readonly nint sys_sem_wait = 101;
private static readonly nint sys_sem_post = 102;
private static readonly nint sys_sem_get_value = 103;
private static readonly nint sys_dyncode_create = 104;
private static readonly nint sys_dyncode_modify = 105;
private static readonly nint sys_dyncode_delete = 106;
private static readonly nint sys_test_infoleak = 109;
private static readonly nint sys_test_crash = 110;
private static readonly nint sys_test_syscall_1 = 111;
private static readonly nint sys_test_syscall_2 = 112;
private static readonly nint sys_futex_wait_abs = 120;
private static readonly nint sys_futex_wake = 121;
private static readonly nint sys_pread = 130;
private static readonly nint sys_pwrite = 131;
private static readonly nint sys_truncate = 140;
private static readonly nint sys_lstat = 141;
private static readonly nint sys_link = 142;
private static readonly nint sys_rename = 143;
private static readonly nint sys_symlink = 144;
private static readonly nint sys_access = 145;
private static readonly nint sys_readlink = 146;
private static readonly nint sys_utimes = 147;
private static readonly nint sys_get_random_bytes = 150;


// TODO: Auto-generate some day. (Hard-coded in binaries so not likely to change.)
 
// native_client/src/trusted/service_runtime/include/sys/errno.h
// The errors are mainly copied from Linux.
public static readonly Errno EPERM = 1; /* Operation not permitted */
public static readonly Errno ENOENT = 2; /* No such file or directory */
public static readonly Errno ESRCH = 3; /* No such process */
public static readonly Errno EINTR = 4; /* Interrupted system call */
public static readonly Errno EIO = 5; /* I/O error */
public static readonly Errno ENXIO = 6; /* No such device or address */
public static readonly Errno E2BIG = 7; /* Argument list too long */
public static readonly Errno ENOEXEC = 8; /* Exec format error */
public static readonly Errno EBADF = 9; /* Bad file number */
public static readonly Errno ECHILD = 10; /* No child processes */
public static readonly Errno EAGAIN = 11; /* Try again */
public static readonly Errno ENOMEM = 12; /* Out of memory */
public static readonly Errno EACCES = 13; /* Permission denied */
public static readonly Errno EFAULT = 14; /* Bad address */
public static readonly Errno EBUSY = 16; /* Device or resource busy */
public static readonly Errno EEXIST = 17; /* File exists */
public static readonly Errno EXDEV = 18; /* Cross-device link */
public static readonly Errno ENODEV = 19; /* No such device */
public static readonly Errno ENOTDIR = 20; /* Not a directory */
public static readonly Errno EISDIR = 21; /* Is a directory */
public static readonly Errno EINVAL = 22; /* Invalid argument */
public static readonly Errno ENFILE = 23; /* File table overflow */
public static readonly Errno EMFILE = 24; /* Too many open files */
public static readonly Errno ENOTTY = 25; /* Not a typewriter */
public static readonly Errno EFBIG = 27; /* File too large */
public static readonly Errno ENOSPC = 28; /* No space left on device */
public static readonly Errno ESPIPE = 29; /* Illegal seek */
public static readonly Errno EROFS = 30; /* Read-only file system */
public static readonly Errno EMLINK = 31; /* Too many links */
public static readonly Errno EPIPE = 32; /* Broken pipe */
public static readonly Errno ENAMETOOLONG = 36; /* File name too long */
public static readonly Errno ENOSYS = 38; /* Function not implemented */
public static readonly Errno EDQUOT = 122; /* Quota exceeded */
public static readonly Errno EDOM = 33; /* Math arg out of domain of func */
public static readonly Errno ERANGE = 34; /* Math result not representable */
public static readonly Errno EDEADLK = 35; /* Deadlock condition */
public static readonly Errno ENOLCK = 37; /* No record locks available */
public static readonly Errno ENOTEMPTY = 39; /* Directory not empty */
public static readonly Errno ELOOP = 40; /* Too many symbolic links */
public static readonly Errno ENOMSG = 42; /* No message of desired type */
public static readonly Errno EIDRM = 43; /* Identifier removed */
public static readonly Errno ECHRNG = 44; /* Channel number out of range */
public static readonly Errno EL2NSYNC = 45; /* Level 2 not synchronized */
public static readonly Errno EL3HLT = 46; /* Level 3 halted */
public static readonly Errno EL3RST = 47; /* Level 3 reset */
public static readonly Errno ELNRNG = 48; /* Link number out of range */
public static readonly Errno EUNATCH = 49; /* Protocol driver not attached */
public static readonly Errno ENOCSI = 50; /* No CSI structure available */
public static readonly Errno EL2HLT = 51; /* Level 2 halted */
public static readonly Errno EBADE = 52; /* Invalid exchange */
public static readonly Errno EBADR = 53; /* Invalid request descriptor */
public static readonly Errno EXFULL = 54; /* Exchange full */
public static readonly Errno ENOANO = 55; /* No anode */
public static readonly Errno EBADRQC = 56; /* Invalid request code */
public static readonly Errno EBADSLT = 57; /* Invalid slot */
public static readonly Errno EDEADLOCK = EDEADLK; /* File locking deadlock error */
public static readonly Errno EBFONT = 59; /* Bad font file fmt */
public static readonly Errno ENOSTR = 60; /* Device not a stream */
public static readonly Errno ENODATA = 61; /* No data (for no delay io) */
public static readonly Errno ETIME = 62; /* Timer expired */
public static readonly Errno ENOSR = 63; /* Out of streams resources */
public static readonly Errno ENONET = 64; /* Machine is not on the network */
public static readonly Errno ENOPKG = 65; /* Package not installed */
public static readonly Errno EREMOTE = 66; /* The object is remote */
public static readonly Errno ENOLINK = 67; /* The link has been severed */
public static readonly Errno EADV = 68; /* Advertise error */
public static readonly Errno ESRMNT = 69; /* Srmount error */
public static readonly Errno ECOMM = 70; /* Communication error on send */
public static readonly Errno EPROTO = 71; /* Protocol error */
public static readonly Errno EMULTIHOP = 72; /* Multihop attempted */
public static readonly Errno EDOTDOT = 73; /* Cross mount point (not really error) */
public static readonly Errno EBADMSG = 74; /* Trying to read unreadable message */
public static readonly Errno EOVERFLOW = 75; /* Value too large for defined data type */
public static readonly Errno ENOTUNIQ = 76; /* Given log. name not unique */
public static readonly Errno EBADFD = 77; /* f.d. invalid for this operation */
public static readonly Errno EREMCHG = 78; /* Remote address changed */
public static readonly Errno ELIBACC = 79; /* Can't access a needed shared lib */
public static readonly Errno ELIBBAD = 80; /* Accessing a corrupted shared lib */
public static readonly Errno ELIBSCN = 81; /* .lib section in a.out corrupted */
public static readonly Errno ELIBMAX = 82; /* Attempting to link in too many libs */
public static readonly Errno ELIBEXEC = 83; /* Attempting to exec a shared library */
public static readonly Errno EILSEQ = 84;
public static readonly Errno EUSERS = 87;
public static readonly Errno ENOTSOCK = 88; /* Socket operation on non-socket */
public static readonly Errno EDESTADDRREQ = 89; /* Destination address required */
public static readonly Errno EMSGSIZE = 90; /* Message too long */
public static readonly Errno EPROTOTYPE = 91; /* Protocol wrong type for socket */
public static readonly Errno ENOPROTOOPT = 92; /* Protocol not available */
public static readonly Errno EPROTONOSUPPORT = 93; /* Unknown protocol */
public static readonly Errno ESOCKTNOSUPPORT = 94; /* Socket type not supported */
public static readonly Errno EOPNOTSUPP = 95; /* Operation not supported on transport endpoint */
public static readonly Errno EPFNOSUPPORT = 96; /* Protocol family not supported */
public static readonly Errno EAFNOSUPPORT = 97; /* Address family not supported by protocol family */
public static readonly Errno EADDRINUSE = 98; /* Address already in use */
public static readonly Errno EADDRNOTAVAIL = 99; /* Address not available */
public static readonly Errno ENETDOWN = 100; /* Network interface is not configured */
public static readonly Errno ENETUNREACH = 101; /* Network is unreachable */
public static readonly Errno ENETRESET = 102;
public static readonly Errno ECONNABORTED = 103; /* Connection aborted */
public static readonly Errno ECONNRESET = 104; /* Connection reset by peer */
public static readonly Errno ENOBUFS = 105; /* No buffer space available */
public static readonly Errno EISCONN = 106; /* Socket is already connected */
public static readonly Errno ENOTCONN = 107; /* Socket is not connected */
public static readonly Errno ESHUTDOWN = 108; /* Can't send after socket shutdown */
public static readonly Errno ETOOMANYREFS = 109;
public static readonly Errno ETIMEDOUT = 110; /* Connection timed out */
public static readonly Errno ECONNREFUSED = 111; /* Connection refused */
public static readonly Errno EHOSTDOWN = 112; /* Host is down */
public static readonly Errno EHOSTUNREACH = 113; /* Host is unreachable */
public static readonly Errno EALREADY = 114; /* Socket already connected */
public static readonly Errno EINPROGRESS = 115; /* Connection already in progress */
public static readonly Errno ESTALE = 116;
public static readonly Errno ENOTSUP = EOPNOTSUPP; /* Not supported */
public static readonly Errno ENOMEDIUM = 123; /* No medium (in tape drive) */
public static readonly Errno ECANCELED = 125; /* Operation canceled. */
public static readonly Errno ELBIN = 2048; /* Inode is remote (not really error) */
public static readonly Errno EFTYPE = 2049; /* Inappropriate file type or format */
public static readonly Errno ENMFILE = 2050; /* No more files */
public static readonly Errno EPROCLIM = 2051;
public static readonly Errno ENOSHARE = 2052; /* No such host or network path */
public static readonly Errno ECASECLASH = 2053; /* Filename exists with different case */
public static readonly Errno EWOULDBLOCK = EAGAIN; /* Operation would block */

// TODO: Auto-generate some day. (Hard-coded in binaries so not likely to change.)
private static array<@string> errorstr = new array<@string>(InitKeyedValues<@string>((EPERM, "Operation not permitted"), (ENOENT, "No such file or directory"), (ESRCH, "No such process"), (EINTR, "Interrupted system call"), (EIO, "I/O error"), (ENXIO, "No such device or address"), (E2BIG, "Argument list too long"), (ENOEXEC, "Exec format error"), (EBADF, "Bad file number"), (ECHILD, "No child processes"), (EAGAIN, "Try again"), (ENOMEM, "Out of memory"), (EACCES, "Permission denied"), (EFAULT, "Bad address"), (EBUSY, "Device or resource busy"), (EEXIST, "File exists"), (EXDEV, "Cross-device link"), (ENODEV, "No such device"), (ENOTDIR, "Not a directory"), (EISDIR, "Is a directory"), (EINVAL, "Invalid argument"), (ENFILE, "File table overflow"), (EMFILE, "Too many open files"), (ENOTTY, "Not a typewriter"), (EFBIG, "File too large"), (ENOSPC, "No space left on device"), (ESPIPE, "Illegal seek"), (EROFS, "Read-only file system"), (EMLINK, "Too many links"), (EPIPE, "Broken pipe"), (ENAMETOOLONG, "File name too long"), (ENOSYS, "not implemented on "+runtime.GOOS), (EDQUOT, "Quota exceeded"), (EDOM, "Math arg out of domain of func"), (ERANGE, "Math result not representable"), (EDEADLK, "Deadlock condition"), (ENOLCK, "No record locks available"), (ENOTEMPTY, "Directory not empty"), (ELOOP, "Too many symbolic links"), (ENOMSG, "No message of desired type"), (EIDRM, "Identifier removed"), (ECHRNG, "Channel number out of range"), (EL2NSYNC, "Level 2 not synchronized"), (EL3HLT, "Level 3 halted"), (EL3RST, "Level 3 reset"), (ELNRNG, "Link number out of range"), (EUNATCH, "Protocol driver not attached"), (ENOCSI, "No CSI structure available"), (EL2HLT, "Level 2 halted"), (EBADE, "Invalid exchange"), (EBADR, "Invalid request descriptor"), (EXFULL, "Exchange full"), (ENOANO, "No anode"), (EBADRQC, "Invalid request code"), (EBADSLT, "Invalid slot"), (EBFONT, "Bad font file fmt"), (ENOSTR, "Device not a stream"), (ENODATA, "No data (for no delay io)"), (ETIME, "Timer expired"), (ENOSR, "Out of streams resources"), (ENONET, "Machine is not on the network"), (ENOPKG, "Package not installed"), (EREMOTE, "The object is remote"), (ENOLINK, "The link has been severed"), (EADV, "Advertise error"), (ESRMNT, "Srmount error"), (ECOMM, "Communication error on send"), (EPROTO, "Protocol error"), (EMULTIHOP, "Multihop attempted"), (EDOTDOT, "Cross mount point (not really error)"), (EBADMSG, "Trying to read unreadable message"), (EOVERFLOW, "Value too large for defined data type"), (ENOTUNIQ, "Given log. name not unique"), (EBADFD, "f.d. invalid for this operation"), (EREMCHG, "Remote address changed"), (ELIBACC, "Can't access a needed shared lib"), (ELIBBAD, "Accessing a corrupted shared lib"), (ELIBSCN, ".lib section in a.out corrupted"), (ELIBMAX, "Attempting to link in too many libs"), (ELIBEXEC, "Attempting to exec a shared library"), (ENOTSOCK, "Socket operation on non-socket"), (EDESTADDRREQ, "Destination address required"), (EMSGSIZE, "Message too long"), (EPROTOTYPE, "Protocol wrong type for socket"), (ENOPROTOOPT, "Protocol not available"), (EPROTONOSUPPORT, "Unknown protocol"), (ESOCKTNOSUPPORT, "Socket type not supported"), (EOPNOTSUPP, "Operation not supported on transport endpoint"), (EPFNOSUPPORT, "Protocol family not supported"), (EAFNOSUPPORT, "Address family not supported by protocol family"), (EADDRINUSE, "Address already in use"), (EADDRNOTAVAIL, "Address not available"), (ENETDOWN, "Network interface is not configured"), (ENETUNREACH, "Network is unreachable"), (ECONNABORTED, "Connection aborted"), (ECONNRESET, "Connection reset by peer"), (ENOBUFS, "No buffer space available"), (EISCONN, "Socket is already connected"), (ENOTCONN, "Socket is not connected"), (ESHUTDOWN, "Can't send after socket shutdown"), (ETIMEDOUT, "Connection timed out"), (ECONNREFUSED, "Connection refused"), (EHOSTDOWN, "Host is down"), (EHOSTUNREACH, "Host is unreachable"), (EALREADY, "Socket already connected"), (EINPROGRESS, "Connection already in progress"), (ENOMEDIUM, "No medium (in tape drive)"), (ECANCELED, "Operation canceled."), (ELBIN, "Inode is remote (not really error)"), (EFTYPE, "Inappropriate file type or format"), (ENMFILE, "No more files"), (ENOSHARE, "No such host or network path"), (ECASECLASH, "Filename exists with different case")));

// Do the interface allocations only once for common
// Errno values.
private static error errEAGAIN = error.As(EAGAIN)!;private static error errEINVAL = error.As(EINVAL)!;private static error errENOENT = error.As(ENOENT)!;

// errnoErr returns common boxed Errno values, to prevent
// allocations at runtime.
private static error errnoErr(Errno e) {

    if (e == 0) 
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

} // end syscall_package
