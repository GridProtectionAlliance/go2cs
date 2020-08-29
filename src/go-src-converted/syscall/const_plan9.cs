// package syscall -- go2cs converted at 2020 August 29 08:16:15 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\const_plan9.go

using static go.builtin;

namespace go
{
    public static partial class syscall_package
    {
        // Plan 9 Constants

        // Open modes
        public static readonly long O_RDONLY = 0L;
        public static readonly long O_WRONLY = 1L;
        public static readonly long O_RDWR = 2L;
        public static readonly long O_TRUNC = 16L;
        public static readonly long O_CLOEXEC = 32L;
        public static readonly ulong O_EXCL = 0x1000UL;

        // Bind flags
        public static readonly ulong MORDER = 0x0003UL; // mask for bits defining order of mounting
        public static readonly ulong MREPL = 0x0000UL; // mount replaces object
        public static readonly ulong MBEFORE = 0x0001UL; // mount goes before others in union directory
        public static readonly ulong MAFTER = 0x0002UL; // mount goes after others in union directory
        public static readonly ulong MCREATE = 0x0004UL; // permit creation in mounted directory
        public static readonly ulong MCACHE = 0x0010UL; // cache some data
        public static readonly ulong MMASK = 0x0017UL; // all bits on

        // Rfork flags
        public static readonly long RFNAMEG = 1L << (int)(0L);
        public static readonly long RFENVG = 1L << (int)(1L);
        public static readonly long RFFDG = 1L << (int)(2L);
        public static readonly long RFNOTEG = 1L << (int)(3L);
        public static readonly long RFPROC = 1L << (int)(4L);
        public static readonly long RFMEM = 1L << (int)(5L);
        public static readonly long RFNOWAIT = 1L << (int)(6L);
        public static readonly long RFCNAMEG = 1L << (int)(10L);
        public static readonly long RFCENVG = 1L << (int)(11L);
        public static readonly long RFCFDG = 1L << (int)(12L);
        public static readonly long RFREND = 1L << (int)(13L);
        public static readonly long RFNOMNT = 1L << (int)(14L);

        // Qid.Type bits
        public static readonly ulong QTDIR = 0x80UL;
        public static readonly ulong QTAPPEND = 0x40UL;
        public static readonly ulong QTEXCL = 0x20UL;
        public static readonly ulong QTMOUNT = 0x10UL;
        public static readonly ulong QTAUTH = 0x08UL;
        public static readonly ulong QTTMP = 0x04UL;
        public static readonly ulong QTFILE = 0x00UL;

        // Dir.Mode bits
        public static readonly ulong DMDIR = 0x80000000UL;
        public static readonly ulong DMAPPEND = 0x40000000UL;
        public static readonly ulong DMEXCL = 0x20000000UL;
        public static readonly ulong DMMOUNT = 0x10000000UL;
        public static readonly ulong DMAUTH = 0x08000000UL;
        public static readonly ulong DMTMP = 0x04000000UL;
        public static readonly ulong DMREAD = 0x4UL;
        public static readonly ulong DMWRITE = 0x2UL;
        public static readonly ulong DMEXEC = 0x1UL;

        public static readonly long STATMAX = 65535L;
        public static readonly long ERRMAX = 128L;
        public static readonly long STATFIXLEN = 49L;
    }
}
