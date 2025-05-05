// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using fmt = fmt_package;
using os = os_package;
using exec = os.exec_package;
using syscall = syscall_package;
using @unsafe = unsafe_package;
using os;

partial class fuzz_package {

[GoType] partial struct sharedMemSys {
    internal syscall_package.ΔHandle mapObj;
}

internal static (ж<sharedMem> mem, error err) sharedMemMapFile(ж<os.File> Ꮡf, nint size, bool removeOnClose) => func((defer, _) => {
    ж<sharedMem> mem = default!;
    error err = default!;

    ref var f = ref Ꮡf.val;
    var errʗ1 = err;
    defer(() => {
        if (errʗ1 != default!) {
            errʗ1 = fmt.Errorf("mapping temporary file %s: %w"u8, f.Name(), errʗ1);
        }
    });
    // Create a file mapping object. The object itself is not shared.
    var (mapObj, err) = syscall.CreateFileMapping(
        ((syscallꓸHandle)f.Fd()), // fhandle

        nil, // sa

        syscall.PAGE_READWRITE, // prot

        0, // maxSizeHigh

        0, // maxSizeLow

        nil);
    // name
    if (err != default!) {
        return (default!, err);
    }
    // Create a view from the file mapping object.
    var access = ((uint32)((uint32)(syscall.FILE_MAP_READ | syscall.FILE_MAP_WRITE)));
    var (addr, err) = syscall.MapViewOfFile(
        mapObj, // handle

        access, // access

        0, // offsetHigh

        0, // offsetLow

        ((uintptr)size));
    // length
    if (err != default!) {
        syscall.CloseHandle(mapObj);
        return (default!, err);
    }
    var region = @unsafe.Slice((ж<byte>)(uintptr)(((@unsafe.Pointer)addr)), size);
    return (Ꮡ(new sharedMem(
        f: f,
        region: region,
        removeOnClose: removeOnClose,
        sys: new sharedMemSys(mapObj: mapObj)
    )), default!);
});

// Close unmaps the shared memory and closes the temporary file. If this
// sharedMem was created with sharedMemTempFile, Close also removes the file.
[GoRecv] internal static error Close(this ref sharedMem m) {
    // Attempt all operations, even if we get an error for an earlier operation.
    // os.File.Close may fail due to I/O errors, but we still want to delete
    // the temporary file.
    slice<error> errs = default!;
    errs = append(errs,
        syscall.UnmapViewOfFile(((uintptr)new @unsafe.Pointer(Ꮡ(m.region[0])))),
        syscall.CloseHandle(m.sys.mapObj),
        m.f.Close());
    if (m.removeOnClose) {
        errs = append(errs, os.Remove(m.f.Name()));
    }
    foreach (var (_, err) in errs) {
        if (err != default!) {
            return err;
        }
    }
    return default!;
}

// setWorkerComm configures communication channels on the cmd that will
// run a worker process.
internal static void setWorkerComm(ж<exec.Cmd> Ꮡcmd, workerComm comm) {
    ref var cmd = ref Ꮡcmd.val;

    var mem = ᐸꟷ(comm.memMu);
    var memFD = (~mem).f.Fd();
    comm.memMu.ᐸꟷ(mem);
    syscall.SetHandleInformation(((syscallꓸHandle)comm.fuzzIn.Fd()), syscall.HANDLE_FLAG_INHERIT, 1);
    syscall.SetHandleInformation(((syscallꓸHandle)comm.fuzzOut.Fd()), syscall.HANDLE_FLAG_INHERIT, 1);
    syscall.SetHandleInformation(((syscallꓸHandle)memFD), syscall.HANDLE_FLAG_INHERIT, 1);
    cmd.Env = append(cmd.Env, fmt.Sprintf("GO_TEST_FUZZ_WORKER_HANDLES=%x,%x,%x"u8, comm.fuzzIn.Fd(), comm.fuzzOut.Fd(), memFD));
    cmd.SysProcAttr = Ꮡ(new syscall.SysProcAttr(AdditionalInheritedHandles: new syscallꓸHandle[]{((syscallꓸHandle)comm.fuzzIn.Fd()), ((syscallꓸHandle)comm.fuzzOut.Fd()), ((syscallꓸHandle)memFD)}.slice()));
}

// getWorkerComm returns communication channels in the worker process.
internal static (workerComm comm, error err) getWorkerComm() {
    workerComm comm = default!;
    error err = default!;

    @string v = os.Getenv("GO_TEST_FUZZ_WORKER_HANDLES"u8);
    if (v == ""u8) {
        return (new workerComm(nil), fmt.Errorf("GO_TEST_FUZZ_WORKER_HANDLES not set"u8));
    }
    ref var fuzzInFD = ref heap(new uintptr(), out var ᏑfuzzInFD);
    ref var fuzzOutFD = ref heap(new uintptr(), out var ᏑfuzzOutFD);
    ref var memFileFD = ref heap(new uintptr(), out var ᏑmemFileFD);
    {
        var (_, errΔ1) = fmt.Sscanf(v, "%x,%x,%x"u8, ᏑfuzzInFD, ᏑfuzzOutFD, ᏑmemFileFD); if (errΔ1 != default!) {
            return (new workerComm(nil), fmt.Errorf("parsing GO_TEST_FUZZ_WORKER_HANDLES=%s: %v"u8, v, errΔ1));
        }
    }
    var fuzzIn = os.NewFile(fuzzInFD, "fuzz_in"u8);
    var fuzzOut = os.NewFile(fuzzOutFD, "fuzz_out"u8);
    var memFile = os.NewFile(memFileFD, "fuzz_mem"u8);
    (fi, err) = memFile.Stat();
    if (err != default!) {
        return (new workerComm(nil), fmt.Errorf("worker checking temp file size: %w"u8, err));
    }
    nint size = ((nint)fi.Size());
    if (((int64)size) != fi.Size()) {
        return (new workerComm(nil), fmt.Errorf("fuzz temp file exceeds maximum size"u8));
    }
    var removeOnClose = false;
    (mem, err) = sharedMemMapFile(memFile, size, removeOnClose);
    if (err != default!) {
        return (new workerComm(nil), err);
    }
    var memMu = new channel<ж<sharedMem>>(1);
    memMu.ᐸꟷ(mem);
    return (new workerComm(fuzzIn: fuzzIn, fuzzOut: fuzzOut, memMu: memMu), default!);
}

internal static bool isInterruptError(error err) {
    // On Windows, we can't tell whether the process was interrupted by the error
    // returned by Wait. It looks like an ExitError with status 1.
    return false;
}

// terminationSignal returns -1 and false because Windows doesn't have signals.
internal static (osꓸSignal, bool) terminationSignal(error err) {
    return (((syscallꓸSignal)(-1)), false);
}

// isCrashSignal is not implemented because Windows doesn't have signals.
internal static bool isCrashSignal(osꓸSignal signal) {
    throw panic("not implemented: no signals on windows");
}

} // end fuzz_package
