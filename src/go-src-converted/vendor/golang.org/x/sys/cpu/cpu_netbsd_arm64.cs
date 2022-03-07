// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package cpu -- go2cs converted at 2022 March 06 23:38:20 UTC
// import "vendor/golang.org/x/sys/cpu" ==> using cpu = go.vendor.golang.org.x.sys.cpu_package
// Original source: C:\Program Files\Go\src\vendor\golang.org\x\sys\cpu\cpu_netbsd_arm64.go
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;

namespace go.vendor.golang.org.x.sys;

public static partial class cpu_package {

    // Minimal copy of functionality from x/sys/unix so the cpu package can call
    // sysctl without depending on x/sys/unix.
private static readonly nint _CTL_QUERY = -2;

private static readonly nuint _SYSCTL_VERS_1 = 0x1000000;


private static System.UIntPtr _zero = default;

private static error sysctl(slice<int> mib, ptr<byte> _addr_old, ptr<System.UIntPtr> _addr_oldlen, ptr<byte> _addr_@new, System.UIntPtr newlen) {
    error err = default!;
    ref byte old = ref _addr_old.val;
    ref System.UIntPtr oldlen = ref _addr_oldlen.val;
    ref byte @new = ref _addr_@new.val;

    unsafe.Pointer _p0 = default;
    if (len(mib) > 0) {
        _p0 = @unsafe.Pointer(_addr_mib[0]);
    }
    else
 {
        _p0 = @unsafe.Pointer(_addr__zero);
    }
    var (_, _, errno) = syscall.Syscall6(syscall.SYS___SYSCTL, uintptr(_p0), uintptr(len(mib)), uintptr(@unsafe.Pointer(old)), uintptr(@unsafe.Pointer(oldlen)), uintptr(@unsafe.Pointer(new)), uintptr(newlen));
    if (errno != 0) {
        return error.As(errno)!;
    }
    return error.As(null!)!;

}

private partial struct sysctlNode {
    public uint Flags;
    public int Num;
    public array<sbyte> Name;
    public uint Ver;
    public uint __rsvd;
    public array<byte> Un;
    public array<byte> _sysctl_size;
    public array<byte> _sysctl_func;
    public array<byte> _sysctl_parent;
    public array<byte> _sysctl_desc;
}

private static (slice<sysctlNode>, error) sysctlNodes(slice<int> mib) {
    slice<sysctlNode> _p0 = default;
    error _p0 = default!;

    ref System.UIntPtr olen = ref heap(out ptr<System.UIntPtr> _addr_olen); 

    // Get a list of all sysctl nodes below the given MIB by performing
    // a sysctl for the given MIB with CTL_QUERY appended.
    mib = append(mib, _CTL_QUERY);
    ref sysctlNode qnode = ref heap(new sysctlNode(Flags:_SYSCTL_VERS_1), out ptr<sysctlNode> _addr_qnode);
    var qp = (byte.val)(@unsafe.Pointer(_addr_qnode));
    var sz = @unsafe.Sizeof(qnode);
    {
        var err__prev1 = err;

        var err = sysctl(mib, _addr_null, _addr_olen, _addr_qp, sz);

        if (err != null) {
            return (null, error.As(err)!);
        }
        err = err__prev1;

    } 

    // Now that we know the size, get the actual nodes.
    var nodes = make_slice<sysctlNode>(olen / sz);
    var np = (byte.val)(@unsafe.Pointer(_addr_nodes[0]));
    {
        var err__prev1 = err;

        err = sysctl(mib, _addr_np, _addr_olen, _addr_qp, sz);

        if (err != null) {
            return (null, error.As(err)!);
        }
        err = err__prev1;

    }


    return (nodes, error.As(null!)!);

}

private static (slice<int>, error) nametomib(@string name) {
    slice<int> _p0 = default;
    error _p0 = default!;
 
    // Split name into components.
    slice<@string> parts = default;
    nint last = 0;
    {
        nint i__prev1 = i;

        for (nint i = 0; i < len(name); i++) {
            if (name[i] == '.') {
                parts = append(parts, name[(int)last..(int)i]);
                last = i + 1;
            }
        }

        i = i__prev1;
    }
    parts = append(parts, name[(int)last..]);

    int mib = new slice<int>(new int[] {  }); 
    // Discover the nodes and construct the MIB OID.
    foreach (var (partno, part) in parts) {
        var (nodes, err) = sysctlNodes(mib);
        if (err != null) {
            return (null, error.As(err)!);
        }
        foreach (var (_, node) in nodes) {
            var n = make_slice<byte>(0);
            {
                nint i__prev3 = i;

                foreach (var (__i) in node.Name) {
                    i = __i;
                    if (node.Name[i] != 0) {
                        n = append(n, byte(node.Name[i]));
                    }
                }

                i = i__prev3;
            }

            if (string(n) == part) {
                mib = append(mib, int32(node.Num));
                break;
            }

        }        if (len(mib) != partno + 1) {
            return (null, error.As(err)!);
        }
    }    return (mib, error.As(null!)!);

}

// aarch64SysctlCPUID is struct aarch64_sysctl_cpu_id from NetBSD's <aarch64/armreg.h>
private partial struct aarch64SysctlCPUID {
    public ulong midr; /* Main ID Register */
    public ulong revidr; /* Revision ID Register */
    public ulong mpidr; /* Multiprocessor Affinity Register */
    public ulong aa64dfr0; /* A64 Debug Feature Register 0 */
    public ulong aa64dfr1; /* A64 Debug Feature Register 1 */
    public ulong aa64isar0; /* A64 Instruction Set Attribute Register 0 */
    public ulong aa64isar1; /* A64 Instruction Set Attribute Register 1 */
    public ulong aa64mmfr0; /* A64 Memory Model Feature Register 0 */
    public ulong aa64mmfr1; /* A64 Memory Model Feature Register 1 */
    public ulong aa64mmfr2; /* A64 Memory Model Feature Register 2 */
    public ulong aa64pfr0; /* A64 Processor Feature Register 0 */
    public ulong aa64pfr1; /* A64 Processor Feature Register 1 */
    public ulong aa64zfr0; /* A64 SVE Feature ID Register 0 */
    public uint mvfr0; /* Media and VFP Feature Register 0 */
    public uint mvfr1; /* Media and VFP Feature Register 1 */
    public uint mvfr2; /* Media and VFP Feature Register 2 */
    public uint pad;
    public ulong clidr; /* Cache Level ID Register */
    public ulong ctr; /* Cache Type Register */
}

private static (ptr<aarch64SysctlCPUID>, error) sysctlCPUID(@string name) {
    ptr<aarch64SysctlCPUID> _p0 = default!;
    error _p0 = default!;

    var (mib, err) = nametomib(name);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    ref aarch64SysctlCPUID @out = ref heap(new aarch64SysctlCPUID(), out ptr<aarch64SysctlCPUID> _addr_@out);
    ref var n = ref heap(@unsafe.Sizeof(out), out ptr<var> _addr_n);
    var (_, _, errno) = syscall.Syscall6(syscall.SYS___SYSCTL, uintptr(@unsafe.Pointer(_addr_mib[0])), uintptr(len(mib)), uintptr(@unsafe.Pointer(_addr_out)), uintptr(@unsafe.Pointer(_addr_n)), uintptr(0), uintptr(0));
    if (errno != 0) {
        return (_addr_null!, error.As(errno)!);
    }
    return (_addr__addr_out!, error.As(null!)!);

}

private static void doinit() {
    var (cpuid, err) = sysctlCPUID("machdep.cpu0.cpu_id");
    if (err != null) {
        setMinimalFeatures();
        return ;
    }
    parseARM64SystemRegisters(cpuid.aa64isar0, cpuid.aa64isar1, cpuid.aa64pfr0);

    Initialized = true;

}

} // end cpu_package
