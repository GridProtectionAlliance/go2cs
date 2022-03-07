// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package version implements the ``go version'' command.
// package version -- go2cs converted at 2022 March 06 23:17:47 UTC
// import "cmd/go/internal/version" ==> using version = go.cmd.go.@internal.version_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\version\version.go
using bytes = go.bytes_package;
using context = go.context_package;
using binary = go.encoding.binary_package;
using fmt = go.fmt_package;
using fs = go.io.fs_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using runtime = go.runtime_package;
using strings = go.strings_package;

using @base = go.cmd.go.@internal.@base_package;
using System;


namespace go.cmd.go.@internal;

public static partial class version_package {

public static ptr<base.Command> CmdVersion = addr(new base.Command(UsageLine:"go version [-m] [-v] [file ...]",Short:"print Go version",Long:`Version prints the build information for Go executables.

Go version reports the Go version used to build each of the named
executable files.

If no files are named on the command line, go version prints its own
version information.

If a directory is named, go version walks that directory, recursively,
looking for recognized Go binaries and reporting their versions.
By default, go version does not report unrecognized files found
during a directory scan. The -v flag causes it to report unrecognized files.

The -m flag causes go version to print each executable's embedded
module version information, when available. In the output, the module
information consists of multiple lines following the version line, each
indented by a leading tab character.

See also: go doc runtime/debug.BuildInfo.
`,));

private static void init() {
    CmdVersion.Run = runVersion; // break init cycle
}

private static var versionM = CmdVersion.Flag.Bool("m", false, "");private static var versionV = CmdVersion.Flag.Bool("v", false, "");

private static void runVersion(context.Context ctx, ptr<base.Command> _addr_cmd, slice<@string> args) {
    ref base.Command cmd = ref _addr_cmd.val;

    if (len(args) == 0) { 
        // If any of this command's flags were passed explicitly, error
        // out, because they only make sense with arguments.
        //
        // Don't error if the flags came from GOFLAGS, since that can be
        // a reasonable use case. For example, imagine GOFLAGS=-v to
        // turn "verbose mode" on for all Go commands, which should not
        // break "go version".
        if ((!@base.InGOFLAGS("-m") && versionM.val) || (!@base.InGOFLAGS("-v") && versionV.val)) {
            fmt.Fprintf(os.Stderr, "go version: flags can only be used with arguments\n");
            @base.SetExitStatus(2);
            return ;
        }
        fmt.Printf("go version %s %s/%s\n", runtime.Version(), runtime.GOOS, runtime.GOARCH);
        return ;

    }
    foreach (var (_, arg) in args) {
        var (info, err) = os.Stat(arg);
        if (err != null) {
            fmt.Fprintf(os.Stderr, "%v\n", err);
            @base.SetExitStatus(1);
            continue;
        }
        if (info.IsDir()) {
            scanDir(arg);
        }
        else
 {
            scanFile(arg, info, true);
        }
    }
}

// scanDir scans a directory for executables to run scanFile on.
private static void scanDir(@string dir) {
    filepath.WalkDir(dir, (path, d, err) => {
        if (d.Type().IsRegular() || d.Type() & fs.ModeSymlink != 0) {
            var (info, err) = d.Info();
            if (err != null) {
                if (versionV.val) {
                    fmt.Fprintf(os.Stderr, "%s: %v\n", path, err);
                }
                return null;
            }
            scanFile(path, info, versionV.val);
        }
        return null;

    });

}

// isExe reports whether the file should be considered executable.
private static bool isExe(@string file, fs.FileInfo info) {
    if (runtime.GOOS == "windows") {
        return strings.HasSuffix(strings.ToLower(file), ".exe");
    }
    return info.Mode().IsRegular() && info.Mode() & 0111 != 0;

}

// scanFile scans file to try to report the Go and module versions.
// If mustPrint is true, scanFile will report any error reading file.
// Otherwise (mustPrint is false, because scanFile is being called
// by scanDir) scanFile prints nothing for non-Go executables.
private static void scanFile(@string file, fs.FileInfo info, bool mustPrint) => func((defer, _, _) => {
    if (info.Mode() & fs.ModeSymlink != 0) { 
        // Accept file symlinks only.
        var (i, err) = os.Stat(file);
        if (err != null || !i.Mode().IsRegular()) {
            if (mustPrint) {
                fmt.Fprintf(os.Stderr, "%s: symlink\n", file);
            }
            return ;
        }
        info = i;

    }
    if (!isExe(file, info)) {
        if (mustPrint) {
            fmt.Fprintf(os.Stderr, "%s: not executable file\n", file);
        }
        return ;

    }
    var (x, err) = openExe(file);
    if (err != null) {
        if (mustPrint) {
            fmt.Fprintf(os.Stderr, "%s: %v\n", file, err);
        }
        return ;

    }
    defer(x.Close());

    var (vers, mod) = findVers(x);
    if (vers == "") {
        if (mustPrint) {
            fmt.Fprintf(os.Stderr, "%s: go version not found\n", file);
        }
        return ;

    }
    fmt.Printf("%s: %s\n", file, vers);
    if (versionM && mod != "".val) {
        fmt.Printf("\t%s\n", strings.ReplaceAll(mod[..(int)len(mod) - 1], "\n", "\n\t"));
    }
});

// The build info blob left by the linker is identified by
// a 16-byte header, consisting of buildInfoMagic (14 bytes),
// the binary's pointer size (1 byte),
// and whether the binary is big endian (1 byte).
private static slice<byte> buildInfoMagic = (slice<byte>)"\xff Go buildinf:";

// findVers finds and returns the Go version and module version information
// in the executable x.
private static (@string, @string) findVers(exe x) {
    @string vers = default;
    @string mod = default;
 
    // Read the first 64kB of text to find the build info blob.
    var text = x.DataStart();
    var (data, err) = x.ReadData(text, 64 * 1024);
    if (err != null) {
        return ;
    }
    while (!bytes.HasPrefix(data, buildInfoMagic)) {
        if (len(data) < 32) {
            return ;
        data = data[(int)32..];
        }
    } 

    // Decode the blob.
    var ptrSize = int(data[14]);
    var bigEndian = data[15] != 0;
    binary.ByteOrder bo = default;
    if (bigEndian) {
        bo = binary.BigEndian;
    }
    else
 {
        bo = binary.LittleEndian;
    }
    Func<slice<byte>, ulong> readPtr = default;
    if (ptrSize == 4) {
        readPtr = b => uint64(bo.Uint32(b));
        }
    else;

    } {
        readPtr = bo.Uint64;
    }
    vers = readString(x, ptrSize, readPtr, readPtr(data[(int)16..]));
    if (vers == "") {
        return ;
    }
    mod = readString(x, ptrSize, readPtr, readPtr(data[(int)16 + ptrSize..]));
    if (len(mod) >= 33 && mod[len(mod) - 17] == '\n') { 
        // Strip module framing.
        mod = mod[(int)16..(int)len(mod) - 16];

    }
    else
 {
        mod = "";
    }
    return ;

}

// readString returns the string at address addr in the executable x.
private static @string readString(exe x, nint ptrSize, Func<slice<byte>, ulong> readPtr, ulong addr) {
    var (hdr, err) = x.ReadData(addr, uint64(2 * ptrSize));
    if (err != null || len(hdr) < 2 * ptrSize) {
        return "";
    }
    var dataAddr = readPtr(hdr);
    var dataLen = readPtr(hdr[(int)ptrSize..]);
    var (data, err) = x.ReadData(dataAddr, dataLen);
    if (err != null || uint64(len(data)) < dataLen) {
        return "";
    }
    return string(data);

}

} // end version_package
