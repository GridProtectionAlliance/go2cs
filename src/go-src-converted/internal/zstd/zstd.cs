// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package zstd provides a decompressor for zstd streams,
// described in RFC 8878. It does not support dictionaries.
namespace go.@internal;

using binary = encoding.binary_package;
using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using encoding;

partial class zstd_package {

// fuzzing is a fuzzer hook set to true when fuzzing.
// This is used to reject cases where we don't match zstd.
internal static bool fuzzing = false;

// Reader implements [io.Reader] to read a zstd compressed stream.
[GoType] partial struct Reader {
    // The underlying Reader.
    internal io_package.Reader r;
    // Whether we have read the frame header.
    // This is of interest when buffer is empty.
    // If true we expect to see a new block.
    internal bool sawFrameHeader;
    // Whether the current frame expects a checksum.
    internal bool hasChecksum;
    // Whether we have read at least one frame.
    internal bool readOneFrame;
    // True if the frame size is not known.
    internal bool frameSizeUnknown;
    // The number of uncompressed bytes remaining in the current frame.
    // If frameSizeUnknown is true, this is not valid.
    internal uint64 remainingFrameSize;
    // The number of bytes read from r up to the start of the current
    // block, for error reporting.
    internal int64 blockOffset;
    // Buffered decompressed data.
    internal slice<byte> buffer;
    // Current read offset in buffer.
    internal nint off;
    // The current repeated offsets.
    internal uint32 repeatedOffset1;
    internal uint32 repeatedOffset2;
    internal uint32 repeatedOffset3;
    // The current Huffman tree used for compressing literals.
    internal slice<uint16> huffmanTable;
    internal nint huffmanTableBits;
    // The window for back references.
    internal window window;
    // A buffer available to hold a compressed block.
    internal slice<byte> compressedBuf;
    // A buffer for literals.
    internal slice<byte> literals;
    // Sequence decode FSE tables.
    internal array<slice<fseBaselineEntry>> seqTables = new(3);
    internal array<uint8> seqTableBits = new(3);
    // Buffers for sequence decode FSE tables.
    internal array<slice<fseBaselineEntry>> seqTableBuffers = new(3);
    // Scratch space used for small reads, to avoid allocation.
    internal array<byte> scratch = new(16);
    // A scratch table for reading an FSE. Only temporarily valid.
    internal slice<fseEntry> fseScratch;
    // For checksum computation.
    internal xxhash64 checksum;
}

// NewReader creates a new Reader that decompresses data from the given reader.
public static ж<Reader> NewReader(io.Reader input) {
    var r = @new<Reader>();
    r.Reset(input);
    return r;
}

// Reset discards the current state and starts reading a new stream from r.
// This permits reusing a Reader rather than allocating a new one.
[GoRecv] public static void Reset(this ref Reader r, io.Reader input) {
    r.r = input;
    // Several fields are preserved to avoid allocation.
    // Others are always set before they are used.
    r.sawFrameHeader = false;
    r.hasChecksum = false;
    r.readOneFrame = false;
    r.frameSizeUnknown = false;
    r.remainingFrameSize = 0;
    r.blockOffset = 0;
    r.buffer = r.buffer[..0];
    r.off = 0;
}

// repeatedOffset1
// repeatedOffset2
// repeatedOffset3
// huffmanTable
// huffmanTableBits
// window
// compressedBuf
// literals
// seqTables
// seqTableBits
// seqTableBuffers
// scratch
// fseScratch

// Read implements [io.Reader].
[GoRecv] public static (nint, error) Read(this ref Reader r, slice<byte> p) {
    {
        var err = r.refillIfNeeded(); if (err != default!) {
            return (0, err);
        }
    }
    nint n = copy(p, r.buffer[(int)(r.off)..]);
    r.off += n;
    return (n, default!);
}

// ReadByte implements [io.ByteReader].
[GoRecv] public static (byte, error) ReadByte(this ref Reader r) {
    {
        var err = r.refillIfNeeded(); if (err != default!) {
            return (0, err);
        }
    }
    var ret = r.buffer[r.off];
    r.off++;
    return (ret, default!);
}

// refillIfNeeded reads the next block if necessary.
[GoRecv] internal static error refillIfNeeded(this ref Reader r) {
    while (r.off >= len(r.buffer)) {
        {
            var err = r.refill(); if (err != default!) {
                return err;
            }
        }
        r.off = 0;
    }
    return default!;
}

// refill reads and decompresses the next block.
[GoRecv] internal static error refill(this ref Reader r) {
    if (!r.sawFrameHeader) {
        {
            var err = r.readFrameHeader(); if (err != default!) {
                return err;
            }
        }
    }
    return r.readBlock();
}

// readFrameHeader reads the frame header and prepares to read a block.
[GoRecv] internal static error readFrameHeader(this ref Reader r) {
retry:
    nint relativeOffset = 0;
    // Read magic number. RFC 3.1.1.
    {
        var (_, err) = io.ReadFull(r.r, r.scratch[..4]); if (err != default!) {
            // We require that the stream contains at least one frame.
            if (AreEqual(err, io.EOF) && !r.readOneFrame) {
                err = io.ErrUnexpectedEOF;
            }
            return r.wrapError(relativeOffset, err);
        }
    }
    {
        var magic = binary.LittleEndian.Uint32(r.scratch[..4]); if (magic != (nint)4247762216L) {
            if (magic >= 407710288 && magic <= 407710303) {
                // This is a skippable frame.
                r.blockOffset += ((int64)relativeOffset) + 4;
                {
                    var err = r.skipFrame(); if (err != default!) {
                        return err;
                    }
                }
                r.readOneFrame = true;
                goto retry;
            }
            return r.makeError(relativeOffset, "invalid magic number"u8);
        }
    }
    relativeOffset += 4;
    // Read Frame_Header_Descriptor. RFC 3.1.1.1.1.
    {
        var (_, err) = io.ReadFull(r.r, r.scratch[..1]); if (err != default!) {
            return r.wrapNonEOFError(relativeOffset, err);
        }
    }
    var descriptor = r.scratch[0];
    var singleSegment = (byte)(descriptor & (1 << (int)(5))) != 0;
    nint fcsFieldSize = 1 << (int)((descriptor >> (int)(6)));
    if (fcsFieldSize == 1 && !singleSegment) {
        fcsFieldSize = 0;
    }
    nint windowDescriptorSize = default!;
    if (singleSegment){
        windowDescriptorSize = 0;
    } else {
        windowDescriptorSize = 1;
    }
    if ((byte)(descriptor & (1 << (int)(3))) != 0) {
        return r.makeError(relativeOffset, "reserved bit set in frame header descriptor"u8);
    }
    r.hasChecksum = (byte)(descriptor & (1 << (int)(2))) != 0;
    if (r.hasChecksum) {
        r.checksum.reset();
    }
    // Dictionary_ID_Flag. RFC 3.1.1.1.1.6.
    nint dictionaryIdSize = 0;
    {
        var dictIdFlag = (byte)(descriptor & 3); if (dictIdFlag != 0) {
            dictionaryIdSize = 1 << (int)((dictIdFlag - 1));
        }
    }
    relativeOffset++;
    nint headerSize = windowDescriptorSize + dictionaryIdSize + fcsFieldSize;
    {
        var (_, err) = io.ReadFull(r.r, r.scratch[..(int)(headerSize)]); if (err != default!) {
            return r.wrapNonEOFError(relativeOffset, err);
        }
    }
    // Figure out the maximum amount of data we need to retain
    // for backreferences.
    uint64 windowSize = default!;
    if (!singleSegment) {
        // Window descriptor. RFC 3.1.1.1.2.
        var windowDescriptor = r.scratch[0];
        var exponent = ((uint64)(windowDescriptor >> (int)(3)));
        var mantissa = ((uint64)((byte)(windowDescriptor & 7)));
        var windowLog = exponent + 10;
        var windowBase = ((uint64)1) << (int)(windowLog);
        var windowAdd = (windowBase / 8) * mantissa;
        windowSize = windowBase + windowAdd;
        // Default zstd sets limits on the window size.
        if (fuzzing && (windowLog > 31 || windowSize > 1 << (int)(27))) {
            return r.makeError(relativeOffset, "windowSize too large"u8);
        }
    }
    // Dictionary_ID. RFC 3.1.1.1.3.
    if (dictionaryIdSize != 0) {
        var dictionaryId = r.scratch[(int)(windowDescriptorSize)..(int)(windowDescriptorSize + dictionaryIdSize)];
        // Allow only zero Dictionary ID.
        foreach (var (_, b) in dictionaryId) {
            if (b != 0) {
                return r.makeError(relativeOffset, "dictionaries are not supported"u8);
            }
        }
    }
    // Frame_Content_Size. RFC 3.1.1.1.4.
    r.frameSizeUnknown = false;
    r.remainingFrameSize = 0;
    var fb = r.scratch[(int)(windowDescriptorSize + dictionaryIdSize)..];
    switch (fcsFieldSize) {
    case 0: {
        r.frameSizeUnknown = true;
        break;
    }
    case 1: {
        r.remainingFrameSize = ((uint64)fb[0]);
        break;
    }
    case 2: {
        r.remainingFrameSize = 256 + ((uint64)binary.LittleEndian.Uint16(fb));
        break;
    }
    case 4: {
        r.remainingFrameSize = ((uint64)binary.LittleEndian.Uint32(fb));
        break;
    }
    case 8: {
        r.remainingFrameSize = binary.LittleEndian.Uint64(fb);
        break;
    }
    default: {
        throw panic("unreachable");
        break;
    }}

    // RFC 3.1.1.1.2.
    // When Single_Segment_Flag is set, Window_Descriptor is not present.
    // In this case, Window_Size is Frame_Content_Size.
    if (singleSegment) {
        windowSize = r.remainingFrameSize;
    }
    // RFC 8878 3.1.1.1.1.2. permits us to set an 8M max on window size.
    static readonly UntypedInt maxWindowSize = /* 8 << 20 */ 8388608;
    if (windowSize > maxWindowSize) {
        windowSize = maxWindowSize;
    }
    relativeOffset += headerSize;
    r.sawFrameHeader = true;
    r.readOneFrame = true;
    r.blockOffset += ((int64)relativeOffset);
    // Prepare to read blocks from the frame.
    r.repeatedOffset1 = 1;
    r.repeatedOffset2 = 4;
    r.repeatedOffset3 = 8;
    r.huffmanTableBits = 0;
    r.window.reset(((nint)windowSize));
    r.seqTables[0] = default!;
    r.seqTables[1] = default!;
    r.seqTables[2] = default!;
    return default!;
}

// skipFrame skips a skippable frame. RFC 3.1.2.
[GoRecv] internal static error skipFrame(this ref Reader r) {
    nint relativeOffset = 0;
    {
        var (_, err) = io.ReadFull(r.r, r.scratch[..4]); if (err != default!) {
            return r.wrapNonEOFError(relativeOffset, err);
        }
    }
    relativeOffset += 4;
    var size = binary.LittleEndian.Uint32(r.scratch[..4]);
    if (size == 0) {
        r.blockOffset += ((int64)relativeOffset);
        return default!;
    }
    {
        var (seeker, ok) = r.r._<io.Seeker>(ᐧ); if (ok) {
            r.blockOffset += ((int64)relativeOffset);
            // Implementations of Seeker do not always detect invalid offsets,
            // so check that the new offset is valid by comparing to the end.
            var (prev, err) = seeker.Seek(0, io.SeekCurrent);
            if (err != default!) {
                return r.wrapError(0, err);
            }
            var (end, err) = seeker.Seek(0, io.SeekEnd);
            if (err != default!) {
                return r.wrapError(0, err);
            }
            if (prev > end - ((int64)size)) {
                r.blockOffset += end - prev;
                return r.makeEOFError(0);
            }
            // The new offset is valid, so seek to it.
            (_, err) = seeker.Seek(prev + ((int64)size), io.SeekStart);
            if (err != default!) {
                return r.wrapError(0, err);
            }
            r.blockOffset += ((int64)size);
            return default!;
        }
    }
    slice<byte> skip = default!;
    static readonly UntypedInt chunk = /* 1 << 20 */ 1048576; // 1M
    while (size >= chunk) {
        if (len(skip) == 0) {
            skip = new slice<byte>(chunk);
        }
        {
            var (_, err) = io.ReadFull(r.r, skip); if (err != default!) {
                return r.wrapNonEOFError(relativeOffset, err);
            }
        }
        relativeOffset += chunk;
        size -= chunk;
    }
    if (size > 0) {
        if (len(skip) == 0) {
            skip = new slice<byte>(size);
        }
        {
            var (_, err) = io.ReadFull(r.r, skip); if (err != default!) {
                return r.wrapNonEOFError(relativeOffset, err);
            }
        }
        relativeOffset += ((nint)size);
    }
    r.blockOffset += ((int64)relativeOffset);
    return default!;
}

// readBlock reads the next block from a frame.
[GoRecv] internal static error readBlock(this ref Reader r) {
    nint relativeOffset = 0;
    // Read Block_Header. RFC 3.1.1.2.
    {
        var (_, err) = io.ReadFull(r.r, r.scratch[..3]); if (err != default!) {
            return r.wrapNonEOFError(relativeOffset, err);
        }
    }
    relativeOffset += 3;
    var header = (uint32)((uint32)(((uint32)r.scratch[0]) | (((uint32)r.scratch[1]) << (int)(8))) | (((uint32)r.scratch[2]) << (int)(16)));
    var lastBlock = (uint32)(header & 1) != 0;
    var blockType = (uint32)((header >> (int)(1)) & 3);
    nint blockSize = ((nint)(header >> (int)(3)));
    // Maximum block size is smaller of window size and 128K.
    // We don't record the window size for a single segment frame,
    // so just use 128K. RFC 3.1.1.2.3, 3.1.1.2.4.
    if (blockSize > 128 << (int)(10) || (r.window.size > 0 && blockSize > r.window.size)) {
        return r.makeError(relativeOffset, "block size too large"u8);
    }
    // Handle different block types. RFC 3.1.1.2.2.
    switch (blockType) {
    case 0: {
        r.setBufferSize(blockSize);
        {
            var (_, err) = io.ReadFull(r.r, r.buffer); if (err != default!) {
                return r.wrapNonEOFError(relativeOffset, err);
            }
        }
        relativeOffset += blockSize;
        r.blockOffset += ((int64)relativeOffset);
        break;
    }
    case 1: {
        r.setBufferSize(blockSize);
        {
            var (_, err) = io.ReadFull(r.r, r.scratch[..1]); if (err != default!) {
                return r.wrapNonEOFError(relativeOffset, err);
            }
        }
        relativeOffset++;
        var v = r.scratch[0];
        foreach (var (i, _) in r.buffer) {
            r.buffer[i] = v;
        }
        r.blockOffset += ((int64)relativeOffset);
        break;
    }
    case 2: {
        r.blockOffset += ((int64)relativeOffset);
        {
            var err = r.compressedBlock(blockSize); if (err != default!) {
                return err;
            }
        }
        r.blockOffset += ((int64)blockSize);
        break;
    }
    case 3: {
        return r.makeError(relativeOffset, "invalid block type"u8);
    }}

    if (!r.frameSizeUnknown) {
        if (((uint64)len(r.buffer)) > r.remainingFrameSize) {
            return r.makeError(relativeOffset, "too many uncompressed bytes in frame"u8);
        }
        r.remainingFrameSize -= ((uint64)len(r.buffer));
    }
    if (r.hasChecksum) {
        r.checksum.update(r.buffer);
    }
    if (!lastBlock){
        r.window.save(r.buffer);
    } else {
        if (!r.frameSizeUnknown && r.remainingFrameSize != 0) {
            return r.makeError(relativeOffset, "not enough uncompressed bytes for frame"u8);
        }
        // Check for checksum at end of frame. RFC 3.1.1.
        if (r.hasChecksum) {
            {
                var (_, err) = io.ReadFull(r.r, r.scratch[..4]); if (err != default!) {
                    return r.wrapNonEOFError(0, err);
                }
            }
            var inputChecksum = binary.LittleEndian.Uint32(r.scratch[..4]);
            var dataChecksum = ((uint32)r.checksum.digest());
            if (inputChecksum != dataChecksum) {
                return r.wrapError(0, fmt.Errorf("invalid checksum: got %#x want %#x"u8, dataChecksum, inputChecksum));
            }
            r.blockOffset += 4;
        }
        r.sawFrameHeader = false;
    }
    return default!;
}

// setBufferSize sets the decompressed buffer size.
// When this is called the buffer is empty.
[GoRecv] internal static void setBufferSize(this ref Reader r, nint size) {
    if (cap(r.buffer) < size) {
        nint need = size - cap(r.buffer);
        r.buffer = append(r.buffer[..(int)(cap(r.buffer))], new slice<byte>(need).ꓸꓸꓸ);
    }
    r.buffer = r.buffer[..(int)(size)];
}

// zstdError is an error while decompressing.
[GoType] partial struct zstdError {
    internal int64 offset;
    internal error err;
}

[GoRecv] internal static @string Error(this ref zstdError ze) {
    return fmt.Sprintf("zstd decompression error at %d: %v"u8, ze.offset, ze.err);
}

[GoRecv] internal static error Unwrap(this ref zstdError ze) {
    return ze.err;
}

[GoRecv] internal static error makeEOFError(this ref Reader r, nint off) {
    return r.wrapError(off, io.ErrUnexpectedEOF);
}

[GoRecv] internal static error wrapNonEOFError(this ref Reader r, nint off, error err) {
    if (AreEqual(err, io.EOF)) {
        err = io.ErrUnexpectedEOF;
    }
    return r.wrapError(off, err);
}

[GoRecv] internal static error makeError(this ref Reader r, nint off, @string msg) {
    return r.wrapError(off, errors.New(msg));
}

[GoRecv] internal static error wrapError(this ref Reader r, nint off, error err) {
    if (AreEqual(err, io.EOF)) {
        return err;
    }
    return new zstdError(r.blockOffset + ((int64)off), err);
}

} // end zstd_package
