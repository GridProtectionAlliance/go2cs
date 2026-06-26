// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.coverage;

using bufio = bufio_package;
using binary = encoding.binary_package;
using fmt = fmt_package;
using coverage = @internal.coverage_package;
using slicewriter = @internal.coverage.slicewriter_package;
using stringtab = @internal.coverage.stringtab_package;
using uleb128 = @internal.coverage.uleb128_package;
using io = io_package;
using os = os_package;
using slices = slices_package;
using @internal;
using encoding;

partial class encodecounter_package {

// This package contains APIs and helpers for encoding initial portions
// of the counter data files emitted at runtime when coverage instrumentation
// is enabled.  Counter data files may contain multiple segments; the file
// header and first segment are written via the "Write" method below, and
// additional segments can then be added using "AddSegment".
[GoType] partial struct CoverageDataWriter {
    internal ж<@internal.coverage.stringtab_package.Writer> stab;
    internal ж<bufio_package.Writer> w;
    internal @internal.coverage_package.CounterSegmentHeader csh;
    internal slice<byte> tmp;
    internal @internal.coverage_package.CounterFlavor cflavor;
    internal uint32 segs;
    internal bool debug;
}

public static ж<CoverageDataWriter> NewCoverageDataWriter(io.Writer w, coverage.CounterFlavor flav) {
    var r = Ꮡ(new CoverageDataWriter(
        stab: Ꮡ(new stringtab.Writer(nil)),
        w: bufio.NewWriter(w),
        tmp: new slice<byte>(64),
        cflavor: flav
    ));
    (~r).stab.InitWriter();
    (~r).stab.Lookup(""u8);
    return r;
}

// CounterVisitor describes a helper object used during counter file
// writing; when writing counter data files, clients pass a
// CounterVisitor to the write/emit routines, then the expectation is
// that the VisitFuncs method will then invoke the callback "f" with
// data for each function to emit to the file.
[GoType] partial interface CounterVisitor {
    error VisitFuncs(CounterVisitorFn f);
}

public delegate error CounterVisitorFn(uint32 pkid, uint32 funcid, slice<uint32> counters);

// Write writes the contents of the count-data file to the writer
// previously supplied to NewCoverageDataWriter. Returns an error
// if something went wrong somewhere with the write.
[GoRecv] public static error Write(this ref CoverageDataWriter cfw, array<byte> metaFileHash, map<@string, @string> args, CounterVisitor visitor) {
    metaFileHash = metaFileHash.Clone();

    {
        var err = cfw.writeHeader(metaFileHash); if (err != default!) {
            return err;
        }
    }
    return cfw.AppendSegment(args, visitor);
}

internal static error padToFourByteBoundary(ж<slicewriter.WriteSeeker> Ꮡws) {
    ref var ws = ref Ꮡws.val;

    nint sz = len(ws.BytesWritten());
    var zeros = new byte[]{0, 0, 0, 0}.slice();
    var rem = ((uint32)sz) % 4;
    if (rem != 0) {
        var pad = zeros[..(int)((4 - rem))];
        {
            var (nw, err) = ws.Write(pad); if (err != default!){
                return err;
            } else 
            if (nw != len(pad)) {
                return fmt.Errorf("error: short write"u8);
            }
        }
    }
    return default!;
}

[GoRecv] public static error patchSegmentHeader(this ref CoverageDataWriter cfw, ж<slicewriter.WriteSeeker> Ꮡws) {
    ref var ws = ref Ꮡws.val;

    // record position
    var (off, err) = ws.Seek(0, io.SeekCurrent);
    if (err != default!) {
        return fmt.Errorf("error seeking in patchSegmentHeader: %v"u8, err);
    }
    // seek back to start so that we can update the segment header
    {
        var (_, errΔ1) = ws.Seek(0, io.SeekStart); if (errΔ1 != default!) {
            return fmt.Errorf("error seeking in patchSegmentHeader: %v"u8, errΔ1);
        }
    }
    if (cfw.debug) {
        fmt.Fprintf(~os.Stderr, "=-= writing counter segment header: %+v"u8, cfw.csh);
    }
    {
        var errΔ2 = binary.Write(~ws, binary.LittleEndian, cfw.csh); if (errΔ2 != default!) {
            return errΔ2;
        }
    }
    // ... and finally return to the original offset.
    {
        var (_, errΔ3) = ws.Seek(off, io.SeekStart); if (errΔ3 != default!) {
            return fmt.Errorf("error seeking in patchSegmentHeader: %v"u8, errΔ3);
        }
    }
    return default!;
}

[GoRecv] public static error writeSegmentPreamble(this ref CoverageDataWriter cfw, map<@string, @string> args, ж<slicewriter.WriteSeeker> Ꮡws) {
    ref var ws = ref Ꮡws.val;

    {
        var err = binary.Write(~ws, binary.LittleEndian, cfw.csh); if (err != default!) {
            return err;
        }
    }
    var hdrsz = ((uint32)len(ws.BytesWritten()));
    // Write string table and args to a byte slice (since we need
    // to capture offsets at various points), then emit the slice
    // once we are done.
    cfw.stab.Freeze();
    {
        var err = cfw.stab.Write(~ws); if (err != default!) {
            return err;
        }
    }
    cfw.csh.StrTabLen = ((uint32)len(ws.BytesWritten())) - hdrsz;
    var akeys = new slice<@string>(0, len(args));
    foreach (var (k, _) in args) {
        akeys = append(akeys, k);
    }
    slices.Sort(akeys);
    var wrULEB128 = (nuint v) => {
        cfw.tmp = cfw.tmp[..0];
        cfw.tmp = uleb128.AppendUleb128(cfw.tmp, v);
        {
            var (_, err) = ws.Write(cfw.tmp); if (err != default!) {
                return err;
            }
        }
        return default!;
    };
    // Count of arg pairs.
    {
        var err = wrULEB128(((nuint)len(args))); if (err != default!) {
            return err;
        }
    }
    // Arg pairs themselves.
    foreach (var (_, k) in akeys) {
        nuint ki = ((nuint)cfw.stab.Lookup(k));
        {
            var err = wrULEB128(ki); if (err != default!) {
                return err;
            }
        }
        @string v = args[k];
        nuint vi = ((nuint)cfw.stab.Lookup(v));
        {
            var err = wrULEB128(vi); if (err != default!) {
                return err;
            }
        }
    }
    {
        var err = padToFourByteBoundary(Ꮡws); if (err != default!) {
            return err;
        }
    }
    cfw.csh.ArgsLen = ((uint32)len(ws.BytesWritten())) - (cfw.csh.StrTabLen + hdrsz);
    return default!;
}

// AppendSegment appends a new segment to a counter data, with a new
// args section followed by a payload of counter data clauses.
[GoRecv] public static error AppendSegment(this ref CoverageDataWriter cfw, map<@string, @string> args, CounterVisitor visitor) {
    cfw.stab = Ꮡ(new stringtab.Writer(nil));
    cfw.stab.InitWriter();
    cfw.stab.Lookup(""u8);
    error err = default!;
    foreach (var (k, v) in args) {
        cfw.stab.Lookup(k);
        cfw.stab.Lookup(v);
    }
    var ws = Ꮡ(new slicewriter.WriteSeeker(nil));
    {
        err = cfw.writeSegmentPreamble(args, ws); if (err != default!) {
            return err;
        }
    }
    {
        err = cfw.writeCounters(visitor, ws); if (err != default!) {
            return err;
        }
    }
    {
        err = cfw.patchSegmentHeader(ws); if (err != default!) {
            return err;
        }
    }
    {
        var errΔ1 = cfw.writeBytes(ws.BytesWritten()); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    {
        err = cfw.writeFooter(); if (err != default!) {
            return err;
        }
    }
    {
        var errΔ2 = cfw.w.Flush(); if (errΔ2 != default!) {
            return fmt.Errorf("write error: %v"u8, errΔ2);
        }
    }
    cfw.stab = default!;
    return default!;
}

[GoRecv] internal static error writeHeader(this ref CoverageDataWriter cfw, array<byte> metaFileHash) {
    metaFileHash = metaFileHash.Clone();

    // Emit file header.
    var ch = new coverage.CounterFileHeader(
        Magic: coverage.CovCounterMagic,
        Version: coverage.CounterFileVersion,
        MetaHash: metaFileHash,
        CFlavor: cfw.cflavor,
        BigEndian: false
    );
    {
        var err = binary.Write(~cfw.w, binary.LittleEndian, ch); if (err != default!) {
            return err;
        }
    }
    return default!;
}

[GoRecv] internal static error writeBytes(this ref CoverageDataWriter cfw, slice<byte> b) {
    if (len(b) == 0) {
        return default!;
    }
    var (nw, err) = cfw.w.Write(b);
    if (err != default!) {
        return fmt.Errorf("error writing counter data: %v"u8, err);
    }
    if (len(b) != nw) {
        return fmt.Errorf("error writing counter data: short write"u8);
    }
    return default!;
}

[GoRecv] public static error writeCounters(this ref CoverageDataWriter cfw, CounterVisitor visitor, ж<slicewriter.WriteSeeker> Ꮡws) {
    ref var ws = ref Ꮡws.val;

    // Notes:
    // - this version writes everything little-endian, which means
    //   a call is needed to encode every value (expensive)
    // - we may want to move to a model in which we just blast out
    //   all counters, or possibly mmap the file and do the write
    //   implicitly.
    var ctrb = new slice<byte>(4);
    var wrval = 
    var ctrbʗ1 = ctrb;
    (uint32 val) => {
        slice<byte> buf = default!;
        nint towr = default!;
        if (cfw.cflavor == coverage.CtrRaw){
            binary.LittleEndian.PutUint32(ctrbʗ1, val);
            buf = ctrbʗ1;
            towr = 4;
        } else 
        if (cfw.cflavor == coverage.CtrULeb128){
            cfw.tmp = cfw.tmp[..0];
            cfw.tmp = uleb128.AppendUleb128(cfw.tmp, ((nuint)val));
            buf = cfw.tmp;
            towr = len(buf);
        } else {
            throw panic("internal error: bad counter flavor");
        }
        {
            var (sz, err) = ws.Write(buf); if (err != default!){
                return err;
            } else 
            if (sz != towr) {
                return fmt.Errorf("writing counters: short write"u8);
            }
        }
        return default!;
    };
    // Write out entries for each live function.
    var emitter = 
    var wrvalʗ1 = wrval;
    (uint32 pkid, uint32 funcid, slice<uint32> counters) => {
        cfw.csh.FcnEntries++;
        {
            var err = wrvalʗ1(((uint32)len(counters))); if (err != default!) {
                return err;
            }
        }
        {
            var err = wrvalʗ1(pkid); if (err != default!) {
                return err;
            }
        }
        {
            var err = wrvalʗ1(funcid); if (err != default!) {
                return err;
            }
        }
        foreach (var (_, val) in counters) {
            {
                var err = wrvalʗ1(val); if (err != default!) {
                    return err;
                }
            }
        }
        return default!;
    };
    {
        var err = visitor.VisitFuncs(emitter); if (err != default!) {
            return err;
        }
    }
    return default!;
}

[GoRecv] internal static error writeFooter(this ref CoverageDataWriter cfw) {
    cfw.segs++;
    var cf = new coverage.CounterFileFooter(
        Magic: coverage.CovCounterMagic,
        NumSegments: cfw.segs
    );
    {
        var err = binary.Write(~cfw.w, binary.LittleEndian, cf); if (err != default!) {
            return err;
        }
    }
    return default!;
}

} // end encodecounter_package
