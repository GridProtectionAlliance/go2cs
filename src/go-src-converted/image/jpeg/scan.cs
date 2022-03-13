// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package jpeg -- go2cs converted at 2022 March 13 06:44:12 UTC
// import "image/jpeg" ==> using jpeg = go.image.jpeg_package
// Original source: C:\Program Files\Go\src\image\jpeg\scan.go
namespace go.image;

using image = image_package;


// makeImg allocates and initializes the destination image.

public static partial class jpeg_package {

private static void makeImg(this ptr<decoder> _addr_d, nint mxx, nint myy) => func((_, panic, _) => {
    ref decoder d = ref _addr_d.val;

    if (d.nComp == 1) {
        var m = image.NewGray(image.Rect(0, 0, 8 * mxx, 8 * myy));
        d.img1 = m.SubImage(image.Rect(0, 0, d.width, d.height))._<ptr<image.Gray>>();
        return ;
    }
    var h0 = d.comp[0].h;
    var v0 = d.comp[0].v;
    var hRatio = h0 / d.comp[1].h;
    var vRatio = v0 / d.comp[1].v;
    image.YCbCrSubsampleRatio subsampleRatio = default;
    switch (hRatio << 4 | vRatio) {
        case 0x11: 
            subsampleRatio = image.YCbCrSubsampleRatio444;
            break;
        case 0x12: 
            subsampleRatio = image.YCbCrSubsampleRatio440;
            break;
        case 0x21: 
            subsampleRatio = image.YCbCrSubsampleRatio422;
            break;
        case 0x22: 
            subsampleRatio = image.YCbCrSubsampleRatio420;
            break;
        case 0x41: 
            subsampleRatio = image.YCbCrSubsampleRatio411;
            break;
        case 0x42: 
            subsampleRatio = image.YCbCrSubsampleRatio410;
            break;
        default: 
            panic("unreachable");
            break;
    }
    m = image.NewYCbCr(image.Rect(0, 0, 8 * h0 * mxx, 8 * v0 * myy), subsampleRatio);
    d.img3 = m.SubImage(image.Rect(0, 0, d.width, d.height))._<ptr<image.YCbCr>>();

    if (d.nComp == 4) {
        var h3 = d.comp[3].h;
        var v3 = d.comp[3].v;
        d.blackPix = make_slice<byte>(8 * h3 * mxx * 8 * v3 * myy);
        d.blackStride = 8 * h3 * mxx;
    }
});

// Specified in section B.2.3.
private static error processSOS(this ptr<decoder> _addr_d, nint n) {
    ref decoder d = ref _addr_d.val;

    if (d.nComp == 0) {
        return error.As(FormatError("missing SOF marker"))!;
    }
    if (n < 6 || 4 + 2 * d.nComp < n || n % 2 != 0) {
        return error.As(FormatError("SOS has wrong length"))!;
    }
    {
        var err__prev1 = err;

        var err = d.readFull(d.tmp[..(int)n]);

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    }
    var nComp = int(d.tmp[0]);
    if (n != 4 + 2 * nComp) {
        return error.As(FormatError("SOS length inconsistent with number of components"))!;
    }
    var scan = default;
    nint totalHV = 0;
    {
        nint i__prev1 = i;

        for (nint i = 0; i < nComp; i++) {
            var cs = d.tmp[1 + 2 * i]; // Component selector.
            nint compIndex = -1;
            {
                var j__prev2 = j;

                foreach (var (__j, __comp) in d.comp[..(int)d.nComp]) {
                    j = __j;
                    comp = __comp;
                    if (cs == comp.c) {
                        compIndex = j;
                    }
                }

                j = j__prev2;
            }

            if (compIndex < 0) {
                return error.As(FormatError("unknown component selector"))!;
            }
            scan[i].compIndex = uint8(compIndex); 
            // Section B.2.3 states that "the value of Cs_j shall be different from
            // the values of Cs_1 through Cs_(j-1)". Since we have previously
            // verified that a frame's component identifiers (C_i values in section
            // B.2.2) are unique, it suffices to check that the implicit indexes
            // into d.comp are unique.
            {
                var j__prev2 = j;

                for (nint j = 0; j < i; j++) {
                    if (scan[i].compIndex == scan[j].compIndex) {
                        return error.As(FormatError("repeated component selector"))!;
                    }
                }


                j = j__prev2;
            }
            totalHV += d.comp[compIndex].h * d.comp[compIndex].v; 

            // The baseline t <= 1 restriction is specified in table B.3.
            scan[i].td = d.tmp[2 + 2 * i] >> 4;
            {
                var t__prev1 = t;

                var t = scan[i].td;

                if (t > maxTh || (d.baseline && t > 1)) {
                    return error.As(FormatError("bad Td value"))!;
                }

                t = t__prev1;

            }
            scan[i].ta = d.tmp[2 + 2 * i] & 0x0f;
            {
                var t__prev1 = t;

                t = scan[i].ta;

                if (t > maxTh || (d.baseline && t > 1)) {
                    return error.As(FormatError("bad Ta value"))!;
                }

                t = t__prev1;

            }
        }

        i = i__prev1;
    } 
    // Section B.2.3 states that if there is more than one component then the
    // total H*V values in a scan must be <= 10.
    if (d.nComp > 1 && totalHV > 10) {
        return error.As(FormatError("total sampling factors too large"))!;
    }
    var zigStart = int32(0);
    var zigEnd = int32(blockSize - 1);
    var ah = uint32(0);
    var al = uint32(0);
    if (d.progressive) {
        zigStart = int32(d.tmp[1 + 2 * nComp]);
        zigEnd = int32(d.tmp[2 + 2 * nComp]);
        ah = uint32(d.tmp[3 + 2 * nComp] >> 4);
        al = uint32(d.tmp[3 + 2 * nComp] & 0x0f);
        if ((zigStart == 0 && zigEnd != 0) || zigStart > zigEnd || blockSize <= zigEnd) {
            return error.As(FormatError("bad spectral selection bounds"))!;
        }
        if (zigStart != 0 && nComp != 1) {
            return error.As(FormatError("progressive AC coefficients for more than one component"))!;
        }
        if (ah != 0 && ah != al + 1) {
            return error.As(FormatError("bad successive approximation values"))!;
        }
    }
    var h0 = d.comp[0].h;
    var v0 = d.comp[0].v; // The h and v values from the Y components.
    var mxx = (d.width + 8 * h0 - 1) / (8 * h0);
    var myy = (d.height + 8 * v0 - 1) / (8 * v0);
    if (d.img1 == null && d.img3 == null) {
        d.makeImg(mxx, myy);
    }
    if (d.progressive) {
        {
            nint i__prev1 = i;

            for (i = 0; i < nComp; i++) {
                compIndex = scan[i].compIndex;
                if (d.progCoeffs[compIndex] == null) {
                    d.progCoeffs[compIndex] = make_slice<block>(mxx * myy * d.comp[compIndex].h * d.comp[compIndex].v);
                }
            }


            i = i__prev1;
        }
    }
    d.bits = new bits();
    nint mcu = 0;
    var expectedRST = uint8(rst0Marker);
 
    // b is the decoded coefficients, in natural (not zig-zag) order.
    ref block b = ref heap(out ptr<block> _addr_b);    array<int> dc = new array<int>(maxComponents);    nint bx = default;    nint by = default;
    nint blockCount = default;
    for (nint my = 0; my < myy; my++) {
        for (nint mx = 0; mx < mxx; mx++) {
            {
                nint i__prev3 = i;

                for (i = 0; i < nComp; i++) {
                    compIndex = scan[i].compIndex;
                    var hi = d.comp[compIndex].h;
                    var vi = d.comp[compIndex].v;
                    {
                        var j__prev4 = j;

                        for (j = 0; j < hi * vi; j++) { 
                            // The blocks are traversed one MCU at a time. For 4:2:0 chroma
                            // subsampling, there are four Y 8x8 blocks in every 16x16 MCU.
                            //
                            // For a sequential 32x16 pixel image, the Y blocks visiting order is:
                            //    0 1 4 5
                            //    2 3 6 7
                            //
                            // For progressive images, the interleaved scans (those with nComp > 1)
                            // are traversed as above, but non-interleaved scans are traversed left
                            // to right, top to bottom:
                            //    0 1 2 3
                            //    4 5 6 7
                            // Only DC scans (zigStart == 0) can be interleaved. AC scans must have
                            // only one component.
                            //
                            // To further complicate matters, for non-interleaved scans, there is no
                            // data for any blocks that are inside the image at the MCU level but
                            // outside the image at the pixel level. For example, a 24x16 pixel 4:2:0
                            // progressive image consists of two 16x16 MCUs. The interleaved scans
                            // will process 8 Y blocks:
                            //    0 1 4 5
                            //    2 3 6 7
                            // The non-interleaved scans will process only 6 Y blocks:
                            //    0 1 2
                            //    3 4 5
                            if (nComp != 1) {
                                bx = hi * mx + j % hi;
                                by = vi * my + j / hi;
                            }
                            else
 {
                                var q = mxx * hi;
                                bx = blockCount % q;
                                by = blockCount / q;
                                blockCount++;
                                if (bx * 8 >= d.width || by * 8 >= d.height) {
                                    continue;
                                }
                            } 

                            // Load the previous partially decoded coefficients, if applicable.
                            if (d.progressive) {
                                b = d.progCoeffs[compIndex][by * mxx * hi + bx];
                            }
                            else
 {
                                b = new block();
                            }
                            if (ah != 0) {
                                {
                                    var err__prev2 = err;

                                    err = d.refine(_addr_b, _addr_d.huff[acTable][scan[i].ta], zigStart, zigEnd, 1 << (int)(al));

                                    if (err != null) {
                                        return error.As(err)!;
                                    }

                                    err = err__prev2;

                                }
                            }
                            else
 {
                                var zig = zigStart;
                                if (zig == 0) {
                                    zig++; 
                                    // Decode the DC coefficient, as specified in section F.2.2.1.
                                    var (value, err) = d.decodeHuffman(_addr_d.huff[dcTable][scan[i].td]);
                                    if (err != null) {
                                        return error.As(err)!;
                                    }
                                    if (value > 16) {
                                        return error.As(UnsupportedError("excessive DC component"))!;
                                    }
                                    var (dcDelta, err) = d.receiveExtend(value);
                                    if (err != null) {
                                        return error.As(err)!;
                                    }
                                    dc[compIndex] += dcDelta;
                                    b[0] = dc[compIndex] << (int)(al);
                                }
                                if (zig <= zigEnd && d.eobRun > 0) {
                                    d.eobRun--;
                                }
                                else
 { 
                                    // Decode the AC coefficients, as specified in section F.2.2.2.
                                    var huff = _addr_d.huff[acTable][scan[i].ta];
                                    while (zig <= zigEnd) {
                                        (value, err) = d.decodeHuffman(huff);
                                        if (err != null) {
                                            return error.As(err)!;
                                        zig++;
                                        }
                                        var val0 = value >> 4;
                                        var val1 = value & 0x0f;
                                        if (val1 != 0) {
                                            zig += int32(val0);
                                            if (zig > zigEnd) {
                                                break;
                                            }
                                            var (ac, err) = d.receiveExtend(val1);
                                            if (err != null) {
                                                return error.As(err)!;
                                            }
                                            b[unzig[zig]] = ac << (int)(al);
                                        }
                                        else
 {
                                            if (val0 != 0x0f) {
                                                d.eobRun = uint16(1 << (int)(val0));
                                                if (val0 != 0) {
                                                    var (bits, err) = d.decodeBits(int32(val0));
                                                    if (err != null) {
                                                        return error.As(err)!;
                                                    }
                                                    d.eobRun |= uint16(bits);
                                                }
                                                d.eobRun--;
                                                break;
                                            }
                                            zig += 0x0f;
                                        }
                                    }
                                }
                            }
                            if (d.progressive) { 
                                // Save the coefficients.
                                d.progCoeffs[compIndex][by * mxx * hi + bx] = b; 
                                // At this point, we could call reconstructBlock to dequantize and perform the
                                // inverse DCT, to save early stages of a progressive image to the *image.YCbCr
                                // buffers (the whole point of progressive encoding), but in Go, the jpeg.Decode
                                // function does not return until the entire image is decoded, so we "continue"
                                // here to avoid wasted computation. Instead, reconstructBlock is called on each
                                // accumulated block by the reconstructProgressiveImage method after all of the
                                // SOS markers are processed.
                                continue;
                            }
                            {
                                var err__prev1 = err;

                                err = d.reconstructBlock(_addr_b, bx, by, int(compIndex));

                                if (err != null) {
                                    return error.As(err)!;
                                }

                                err = err__prev1;

                            }
                        } // for j


                        j = j__prev4;
                    } // for j
                } // for i


                i = i__prev3;
            } // for i
            mcu++;
            if (d.ri > 0 && mcu % d.ri == 0 && mcu < mxx * myy) { 
                // A more sophisticated decoder could use RST[0-7] markers to resynchronize from corrupt input,
                // but this one assumes well-formed input, and hence the restart marker follows immediately.
                {
                    var err__prev2 = err;

                    err = d.readFull(d.tmp[..(int)2]);

                    if (err != null) {
                        return error.As(err)!;
                    } 

                    // Section F.1.2.3 says that "Byte alignment of markers is
                    // achieved by padding incomplete bytes with 1-bits. If padding
                    // with 1-bits creates a X’FF’ value, a zero byte is stuffed
                    // before adding the marker."
                    //
                    // Seeing "\xff\x00" here is not spec compliant, as we are not
                    // expecting an *incomplete* byte (that needed padding). Still,
                    // some real world encoders (see golang.org/issue/28717) insert
                    // it, so we accept it and re-try the 2 byte read.
                    //
                    // libjpeg issues a warning (but not an error) for this:
                    // https://github.com/LuaDist/libjpeg/blob/6c0fcb8ddee365e7abc4d332662b06900612e923/jdmarker.c#L1041-L1046

                    err = err__prev2;

                } 

                // Section F.1.2.3 says that "Byte alignment of markers is
                // achieved by padding incomplete bytes with 1-bits. If padding
                // with 1-bits creates a X’FF’ value, a zero byte is stuffed
                // before adding the marker."
                //
                // Seeing "\xff\x00" here is not spec compliant, as we are not
                // expecting an *incomplete* byte (that needed padding). Still,
                // some real world encoders (see golang.org/issue/28717) insert
                // it, so we accept it and re-try the 2 byte read.
                //
                // libjpeg issues a warning (but not an error) for this:
                // https://github.com/LuaDist/libjpeg/blob/6c0fcb8ddee365e7abc4d332662b06900612e923/jdmarker.c#L1041-L1046
                if (d.tmp[0] == 0xff && d.tmp[1] == 0x00) {
                    {
                        var err__prev3 = err;

                        err = d.readFull(d.tmp[..(int)2]);

                        if (err != null) {
                            return error.As(err)!;
                        }

                        err = err__prev3;

                    }
                }
                if (d.tmp[0] != 0xff || d.tmp[1] != expectedRST) {
                    return error.As(FormatError("bad RST marker"))!;
                }
                expectedRST++;
                if (expectedRST == rst7Marker + 1) {
                    expectedRST = rst0Marker;
                } 
                // Reset the Huffman decoder.
                d.bits = new bits(); 
                // Reset the DC components, as per section F.2.1.3.1.
                dc = new array<int>(new int[] {  }); 
                // Reset the progressive decoder state, as per section G.1.2.2.
                d.eobRun = 0;
            }
        } // for mx
    } // for my

    return error.As(null!)!;
}

// refine decodes a successive approximation refinement block, as specified in
// section G.1.2.
private static error refine(this ptr<decoder> _addr_d, ptr<block> _addr_b, ptr<huffman> _addr_h, int zigStart, int zigEnd, int delta) => func((_, panic, _) => {
    ref decoder d = ref _addr_d.val;
    ref block b = ref _addr_b.val;
    ref huffman h = ref _addr_h.val;
 
    // Refining a DC component is trivial.
    if (zigStart == 0) {
        if (zigEnd != 0) {
            panic("unreachable");
        }
        var (bit, err) = d.decodeBit();
        if (err != null) {
            return error.As(err)!;
        }
        if (bit) {
            b[0] |= delta;
        }
        return error.As(null!)!;
    }
    var zig = zigStart;
    if (d.eobRun == 0) {
loop:
        while (zig <= zigEnd) {
            var z = int32(0);
            var (value, err) = d.decodeHuffman(h);
            if (err != null) {
                return error.As(err)!;
            zig++;
            }
            var val0 = value >> 4;
            var val1 = value & 0x0f;

            switch (val1) {
                case 0: 
                    if (val0 != 0x0f) {
                        d.eobRun = uint16(1 << (int)(val0));
                        if (val0 != 0) {
                            var (bits, err) = d.decodeBits(int32(val0));
                            if (err != null) {
                                return error.As(err)!;
                            }
                            d.eobRun |= uint16(bits);
                        }
                        _breakloop = true;
                        break;
                    }
                    break;
                case 1: 
                    z = delta;
                    (bit, err) = d.decodeBit();
                    if (err != null) {
                        return error.As(err)!;
                    }
                    if (!bit) {
                        z = -z;
                    }
                    break;
                default: 
                    return error.As(FormatError("unexpected Huffman code"))!;
                    break;
            }

            zig, err = d.refineNonZeroes(b, zig, zigEnd, int32(val0), delta);
            if (err != null) {
                return error.As(err)!;
            }
            if (zig > zigEnd) {
                return error.As(FormatError("too many coefficients"))!;
            }
            if (z != 0) {
                b[unzig[zig]] = z;
            }
        }
    }
    if (d.eobRun > 0) {
        d.eobRun--;
        {
            var (_, err) = d.refineNonZeroes(b, zig, zigEnd, -1, delta);

            if (err != null) {
                return error.As(err)!;
            }

        }
    }
    return error.As(null!)!;
});

// refineNonZeroes refines non-zero entries of b in zig-zag order. If nz >= 0,
// the first nz zero entries are skipped over.
private static (int, error) refineNonZeroes(this ptr<decoder> _addr_d, ptr<block> _addr_b, int zig, int zigEnd, int nz, int delta) {
    int _p0 = default;
    error _p0 = default!;
    ref decoder d = ref _addr_d.val;
    ref block b = ref _addr_b.val;

    while (zig <= zigEnd) {
        var u = unzig[zig];
        if (b[u] == 0) {
            if (nz == 0) {
                break;
        zig++;
            }
            nz--;
            continue;
        }
        var (bit, err) = d.decodeBit();
        if (err != null) {
            return (0, error.As(err)!);
        }
        if (!bit) {
            continue;
        }
        if (b[u] >= 0) {
            b[u] += delta;
        }
        else
 {
            b[u] -= delta;
        }
    }
    return (zig, error.As(null!)!);
}

private static error reconstructProgressiveImage(this ptr<decoder> _addr_d) {
    ref decoder d = ref _addr_d.val;
 
    // The h0, mxx, by and bx variables have the same meaning as in the
    // processSOS method.
    var h0 = d.comp[0].h;
    var mxx = (d.width + 8 * h0 - 1) / (8 * h0);
    for (nint i = 0; i < d.nComp; i++) {
        if (d.progCoeffs[i] == null) {
            continue;
        }
        nint v = 8 * d.comp[0].v / d.comp[i].v;
        nint h = 8 * d.comp[0].h / d.comp[i].h;
        var stride = mxx * d.comp[i].h;
        for (nint by = 0; by * v < d.height; by++) {
            for (nint bx = 0; bx * h < d.width; bx++) {
                {
                    var err = d.reconstructBlock(_addr_d.progCoeffs[i][by * stride + bx], bx, by, i);

                    if (err != null) {
                        return error.As(err)!;
                    }

                }
            }
        }
    }
    return error.As(null!)!;
}

// reconstructBlock dequantizes, performs the inverse DCT and stores the block
// to the image.
private static error reconstructBlock(this ptr<decoder> _addr_d, ptr<block> _addr_b, nint bx, nint by, nint compIndex) {
    ref decoder d = ref _addr_d.val;
    ref block b = ref _addr_b.val;

    var qt = _addr_d.quant[d.comp[compIndex].tq];
    for (nint zig = 0; zig < blockSize; zig++) {
        b[unzig[zig]] *= qt[zig];
    }
    idct(b);
    slice<byte> dst = (slice<byte>)null;
    nint stride = 0;
    if (d.nComp == 1) {
        (dst, stride) = (d.img1.Pix[(int)8 * (by * d.img1.Stride + bx)..], d.img1.Stride);
    }
    else
 {
        switch (compIndex) {
            case 0: 
                (dst, stride) = (d.img3.Y[(int)8 * (by * d.img3.YStride + bx)..], d.img3.YStride);
                break;
            case 1: 
                (dst, stride) = (d.img3.Cb[(int)8 * (by * d.img3.CStride + bx)..], d.img3.CStride);
                break;
            case 2: 
                (dst, stride) = (d.img3.Cr[(int)8 * (by * d.img3.CStride + bx)..], d.img3.CStride);
                break;
            case 3: 
                (dst, stride) = (d.blackPix[(int)8 * (by * d.blackStride + bx)..], d.blackStride);
                break;
            default: 
                return error.As(UnsupportedError("too many components"))!;
                break;
        }
    }
    for (nint y = 0; y < 8; y++) {
        var y8 = y * 8;
        var yStride = y * stride;
        for (nint x = 0; x < 8; x++) {
            var c = b[y8 + x];
            if (c < -128) {
                c = 0;
            }
            else if (c > 127) {
                c = 255;
            }
            else
 {
                c += 128;
            }
            dst[yStride + x] = uint8(c);
        }
    }
    return error.As(null!)!;
}

} // end jpeg_package
