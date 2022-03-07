// Copyright 2014 Google Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

// package binutils -- go2cs converted at 2022 March 06 23:23:16 UTC
// import "cmd/vendor/github.com/google/pprof/internal/binutils" ==> using binutils = go.cmd.vendor.github.com.google.pprof.@internal.binutils_package
// Original source: C:\Program Files\Go\src\cmd\vendor\github.com\google\pprof\internal\binutils\addr2liner_nm.go
using bufio = go.bufio_package;
using bytes = go.bytes_package;
using io = go.io_package;
using exec = go.os.exec_package;
using strconv = go.strconv_package;
using strings = go.strings_package;

using plugin = go.github.com.google.pprof.@internal.plugin_package;

namespace go.cmd.vendor.github.com.google.pprof.@internal;

public static partial class binutils_package {

private static readonly @string defaultNM = "nm";


// addr2LinerNM is a connection to an nm command for obtaining symbol
// information from a binary.
private partial struct addr2LinerNM {
    public slice<symbolInfo> m; // Sorted list of symbol addresses from binary.
}

private partial struct symbolInfo {
    public ulong address;
    public ulong size;
    public @string name;
    public @string symType;
}

// isData returns if the symbol has a known data object symbol type.
private static bool isData(this ptr<symbolInfo> _addr_s) {
    ref symbolInfo s = ref _addr_s.val;
 
    // The following symbol types are taken from https://linux.die.net/man/1/nm:
    // Lowercase letter means local symbol, uppercase denotes a global symbol.
    // - b or B: the symbol is in the uninitialized data section, e.g. .bss;
    // - d or D: the symbol is in the initialized data section;
    // - r or R: the symbol is in a read only data section;
    // - v or V: the symbol is a weak object;
    // - W: the symbol is a weak symbol that has not been specifically tagged as a
    //      weak object symbol. Experiments with some binaries, showed these to be
    //      mostly data objects.
    return strings.ContainsAny(s.symType, "bBdDrRvVW");

}

// newAddr2LinerNM starts the given nm command reporting information about the
// given executable file. If file is a shared library, base should be the
// address at which it was mapped in the program under consideration.
private static (ptr<addr2LinerNM>, error) newAddr2LinerNM(@string cmd, @string file, ulong @base) {
    ptr<addr2LinerNM> _p0 = default!;
    error _p0 = default!;

    if (cmd == "") {
        cmd = defaultNM;
    }
    ref bytes.Buffer b = ref heap(out ptr<bytes.Buffer> _addr_b);
    var c = exec.Command(cmd, "--numeric-sort", "--print-size", "--format=posix", file);
    _addr_c.Stdout = _addr_b;
    c.Stdout = ref _addr_c.Stdout.val;
    {
        var err = c.Run();

        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
    }

    return _addr_parseAddr2LinerNM(base, _addr_b)!;

}

private static (ptr<addr2LinerNM>, error) parseAddr2LinerNM(ulong @base, io.Reader nm) {
    ptr<addr2LinerNM> _p0 = default!;
    error _p0 = default!;

    ptr<addr2LinerNM> a = addr(new addr2LinerNM(m:[]symbolInfo{},)); 

    // Parse nm output and populate symbol map.
    // Skip lines we fail to parse.
    var buf = bufio.NewReader(nm);
    while (true) {
        var (line, err) = buf.ReadString('\n');
        if (line == "" && err != null) {
            if (err == io.EOF) {
                break;
            }
            return (_addr_null!, error.As(err)!);
        }
        line = strings.TrimSpace(line);
        var fields = strings.Split(line, " ");
        if (len(fields) != 4) {
            continue;
        }
        var (address, err) = strconv.ParseUint(fields[2], 16, 64);
        if (err != null) {
            continue;
        }
        var (size, err) = strconv.ParseUint(fields[3], 16, 64);
        if (err != null) {
            continue;
        }
        a.m = append(a.m, new symbolInfo(address:address+base,size:size,name:fields[0],symType:fields[1],));

    }

    return (_addr_a!, error.As(null!)!);

}

// addrInfo returns the stack frame information for a specific program
// address. It returns nil if the address could not be identified.
private static (slice<plugin.Frame>, error) addrInfo(this ptr<addr2LinerNM> _addr_a, ulong addr) {
    slice<plugin.Frame> _p0 = default;
    error _p0 = default!;
    ref addr2LinerNM a = ref _addr_a.val;

    if (len(a.m) == 0 || addr < a.m[0].address || addr >= (a.m[len(a.m) - 1].address + a.m[len(a.m) - 1].size)) {
        return (null, error.As(null!)!);
    }
    nint low = 0;
    var high = len(a.m);
    while (low + 1 < high) {
        var mid = (low + high) / 2;
        var v = a.m[mid].address;
        if (addr == v) {
            low = mid;
            break;
        }
        else if (addr > v) {
            low = mid;
        }
        else
 {
            high = mid;
        }
    } 

    // Address is between a.m[low] and a.m[high]. Pick low, as it represents
    // [low, high). For data symbols, we use a strict check that the address is in
    // the [start, start + size) range of a.m[low].
    if (a.m[low].isData() && addr >= (a.m[low].address + a.m[low].size)) {
        return (null, error.As(null!)!);
    }
    return (new slice<plugin.Frame>(new plugin.Frame[] { {Func:a.m[low].name} }), error.As(null!)!);

}

} // end binutils_package
