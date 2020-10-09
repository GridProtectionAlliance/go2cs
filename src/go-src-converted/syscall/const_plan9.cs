// package syscall -- go2cs converted at 2020 October 09 04:45:21 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\const_plan9.go

using static go.builtin;

namespace go
{
    public static partial class syscall_package
    {
        // Plan 9 Constants

        // Open modes
        public static readonly long O_RDONLY = (long)0L;
        public static readonly long O_WRONLY = (long)1L;
        public static readonly long O_RDWR = (long)2L;
        public static readonly long O_TRUNC = (long)16L;
        public static readonly long O_CLOEXEC = (long)32L;
        public static readonly ulong O_EXCL = (ulong)0x1000UL;


        // Bind flags
        public static readonly ulong MORDER = (ulong)0x0003UL; // mask for bits defining order of mounting
        public static readonly ulong MREPL = (ulong)0x0000UL; // mount replaces object
        public static readonly ulong MBEFORE = (ulong)0x0001UL; // mount goes before others in union directory
        public static readonly ulong MAFTER = (ulong)0x0002UL; // mount goes after others in union directory
        public static readonly ulong MCREATE = (ulong)0x0004UL; // permit creation in mounted directory
        public static readonly ulong MCACHE = (ulong)0x0010UL; // cache some data
        public static readonly ulong MMASK = (ulong)0x0017UL; // all bits on

        // Rfork flags
        public static readonly long RFNAMEG = (long)1L << (int)(0L);
        public static readonly long RFENVG = (long)1L << (int)(1L);
        public static readonly long RFFDG = (long)1L << (int)(2L);
        public static readonly long RFNOTEG = (long)1L << (int)(3L);
        public static readonly long RFPROC = (long)1L << (int)(4L);
        public static readonly long RFMEM = (long)1L << (int)(5L);
        public static readonly long RFNOWAIT = (long)1L << (int)(6L);
        public static readonly long RFCNAMEG = (long)1L << (int)(10L);
        public static readonly long RFCENVG = (long)1L << (int)(11L);
        public static readonly long RFCFDG = (long)1L << (int)(12L);
        public static readonly long RFREND = (long)1L << (int)(13L);
        public static readonly long RFNOMNT = (long)1L << (int)(14L);


        // Qid.Type bits
        public static readonly ulong QTDIR = (ulong)0x80UL;
        public static readonly ulong QTAPPEND = (ulong)0x40UL;
        public static readonly ulong QTEXCL = (ulong)0x20UL;
        public static readonly ulong QTMOUNT = (ulong)0x10UL;
        public static readonly ulong QTAUTH = (ulong)0x08UL;
        public static readonly ulong QTTMP = (ulong)0x04UL;
        public static readonly ulong QTFILE = (ulong)0x00UL;


        // Dir.Mode bits
        public static readonly ulong DMDIR = (ulong)0x80000000UL;
        public static readonly ulong DMAPPEND = (ulong)0x40000000UL;
        public static readonly ulong DMEXCL = (ulong)0x20000000UL;
        public static readonly ulong DMMOUNT = (ulong)0x10000000UL;
        public static readonly ulong DMAUTH = (ulong)0x08000000UL;
        public static readonly ulong DMTMP = (ulong)0x04000000UL;
        public static readonly ulong DMREAD = (ulong)0x4UL;
        public static readonly ulong DMWRITE = (ulong)0x2UL;
        public static readonly ulong DMEXEC = (ulong)0x1UL;


        public static readonly long STATMAX = (long)65535L;
        public static readonly long ERRMAX = (long)128L;
        public static readonly long STATFIXLEN = (long)49L;

    }
}
