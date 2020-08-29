// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package png -- go2cs converted at 2020 August 29 10:10:30 UTC
// import "image/png" ==> using png = go.image.png_package
// Original source: C:\Go\src\image\png\writer.go
using bufio = go.bufio_package;
using zlib = go.compress.zlib_package;
using crc32 = go.hash.crc32_package;
using image = go.image_package;
using color = go.image.color_package;
using io = go.io_package;
using strconv = go.strconv_package;
using static go.builtin;

namespace go {
namespace image
{
    public static partial class png_package
    {
        // Encoder configures encoding PNG images.
        public partial struct Encoder
        {
            public CompressionLevel CompressionLevel; // BufferPool optionally specifies a buffer pool to get temporary
// EncoderBuffers when encoding an image.
            public EncoderBufferPool BufferPool;
        }

        // EncoderBufferPool is an interface for getting and returning temporary
        // instances of the EncoderBuffer struct. This can be used to reuse buffers
        // when encoding multiple images.
        public partial interface EncoderBufferPool
        {
            ref EncoderBuffer Get();
            ref EncoderBuffer Put(ref EncoderBuffer _p0);
        }

        // EncoderBuffer holds the buffers used for encoding PNG images.
        public partial struct EncoderBuffer // : encoder
        {
        }

        private partial struct encoder
        {
            public ptr<Encoder> enc;
            public io.Writer w;
            public image.Image m;
            public long cb;
            public error err;
            public array<byte> header;
            public array<byte> footer;
            public array<byte> tmp;
            public array<slice<byte>> cr;
            public slice<byte> pr;
            public ptr<zlib.Writer> zw;
            public long zwLevel;
            public ptr<bufio.Writer> bw;
        }

        public partial struct CompressionLevel // : long
        {
        }

        public static readonly CompressionLevel DefaultCompression = 0L;
        public static readonly CompressionLevel NoCompression = -1L;
        public static readonly CompressionLevel BestSpeed = -2L;
        public static readonly CompressionLevel BestCompression = -3L; 

        // Positive CompressionLevel values are reserved to mean a numeric zlib
        // compression level, although that is not implemented yet.

        // Big-endian.
        private static void writeUint32(slice<byte> b, uint u)
        {
            b[0L] = uint8(u >> (int)(24L));
            b[1L] = uint8(u >> (int)(16L));
            b[2L] = uint8(u >> (int)(8L));
            b[3L] = uint8(u >> (int)(0L));
        }

        private partial interface opaquer
        {
            bool Opaque();
        }

        // Returns whether or not the image is fully opaque.
        private static bool opaque(image.Image m)
        {
            {
                opaquer (o, ok) = m._<opaquer>();

                if (ok)
                {
                    return o.Opaque();
                }

            }
            var b = m.Bounds();
            for (var y = b.Min.Y; y < b.Max.Y; y++)
            {
                for (var x = b.Min.X; x < b.Max.X; x++)
                {
                    var (_, _, _, a) = m.At(x, y).RGBA();
                    if (a != 0xffffUL)
                    {
                        return false;
                    }
                }

            }

            return true;
        }

        // The absolute value of a byte interpreted as a signed int8.
        private static long abs8(byte d)
        {
            if (d < 128L)
            {
                return int(d);
            }
            return 256L - int(d);
        }

        private static void writeChunk(this ref encoder e, slice<byte> b, @string name)
        {
            if (e.err != null)
            {
                return;
            }
            var n = uint32(len(b));
            if (int(n) != len(b))
            {
                e.err = UnsupportedError(name + " chunk is too large: " + strconv.Itoa(len(b)));
                return;
            }
            writeUint32(e.header[..4L], n);
            e.header[4L] = name[0L];
            e.header[5L] = name[1L];
            e.header[6L] = name[2L];
            e.header[7L] = name[3L];
            var crc = crc32.NewIEEE();
            crc.Write(e.header[4L..8L]);
            crc.Write(b);
            writeUint32(e.footer[..4L], crc.Sum32());

            _, e.err = e.w.Write(e.header[..8L]);
            if (e.err != null)
            {
                return;
            }
            _, e.err = e.w.Write(b);
            if (e.err != null)
            {
                return;
            }
            _, e.err = e.w.Write(e.footer[..4L]);
        }

        private static void writeIHDR(this ref encoder e)
        {
            var b = e.m.Bounds();
            writeUint32(e.tmp[0L..4L], uint32(b.Dx()));
            writeUint32(e.tmp[4L..8L], uint32(b.Dy())); 
            // Set bit depth and color type.

            if (e.cb == cbG8) 
                e.tmp[8L] = 8L;
                e.tmp[9L] = ctGrayscale;
            else if (e.cb == cbTC8) 
                e.tmp[8L] = 8L;
                e.tmp[9L] = ctTrueColor;
            else if (e.cb == cbP8) 
                e.tmp[8L] = 8L;
                e.tmp[9L] = ctPaletted;
            else if (e.cb == cbTCA8) 
                e.tmp[8L] = 8L;
                e.tmp[9L] = ctTrueColorAlpha;
            else if (e.cb == cbG16) 
                e.tmp[8L] = 16L;
                e.tmp[9L] = ctGrayscale;
            else if (e.cb == cbTC16) 
                e.tmp[8L] = 16L;
                e.tmp[9L] = ctTrueColor;
            else if (e.cb == cbTCA16) 
                e.tmp[8L] = 16L;
                e.tmp[9L] = ctTrueColorAlpha;
                        e.tmp[10L] = 0L; // default compression method
            e.tmp[11L] = 0L; // default filter method
            e.tmp[12L] = 0L; // non-interlaced
            e.writeChunk(e.tmp[..13L], "IHDR");
        }

        private static void writePLTEAndTRNS(this ref encoder e, color.Palette p)
        {
            if (len(p) < 1L || len(p) > 256L)
            {
                e.err = FormatError("bad palette length: " + strconv.Itoa(len(p)));
                return;
            }
            long last = -1L;
            foreach (var (i, c) in p)
            {
                color.NRGBA c1 = color.NRGBAModel.Convert(c)._<color.NRGBA>();
                e.tmp[3L * i + 0L] = c1.R;
                e.tmp[3L * i + 1L] = c1.G;
                e.tmp[3L * i + 2L] = c1.B;
                if (c1.A != 0xffUL)
                {
                    last = i;
                }
                e.tmp[3L * 256L + i] = c1.A;
            }
            e.writeChunk(e.tmp[..3L * len(p)], "PLTE");
            if (last != -1L)
            {
                e.writeChunk(e.tmp[3L * 256L..3L * 256L + 1L + last], "tRNS");
            }
        }

        // An encoder is an io.Writer that satisfies writes by writing PNG IDAT chunks,
        // including an 8-byte header and 4-byte CRC checksum per Write call. Such calls
        // should be relatively infrequent, since writeIDATs uses a bufio.Writer.
        //
        // This method should only be called from writeIDATs (via writeImage).
        // No other code should treat an encoder as an io.Writer.
        private static (long, error) Write(this ref encoder e, slice<byte> b)
        {
            e.writeChunk(b, "IDAT");
            if (e.err != null)
            {
                return (0L, e.err);
            }
            return (len(b), null);
        }

        // Chooses the filter to use for encoding the current row, and applies it.
        // The return value is the index of the filter and also of the row in cr that has had it applied.
        private static long filter(ref array<slice<byte>> cr, slice<byte> pr, long bpp)
        { 
            // We try all five filter types, and pick the one that minimizes the sum of absolute differences.
            // This is the same heuristic that libpng uses, although the filters are attempted in order of
            // estimated most likely to be minimal (ftUp, ftPaeth, ftNone, ftSub, ftAverage), rather than
            // in their enumeration order (ftNone, ftSub, ftUp, ftAverage, ftPaeth).
            var cdat0 = cr[0L][1L..];
            var cdat1 = cr[1L][1L..];
            var cdat2 = cr[2L][1L..];
            var cdat3 = cr[3L][1L..];
            var cdat4 = cr[4L][1L..];
            var pdat = pr[1L..];
            var n = len(cdat0); 

            // The up filter.
            long sum = 0L;
            {
                long i__prev1 = i;

                for (long i = 0L; i < n; i++)
                {
                    cdat2[i] = cdat0[i] - pdat[i];
                    sum += abs8(cdat2[i]);
                }


                i = i__prev1;
            }
            var best = sum;
            var filter = ftUp; 

            // The Paeth filter.
            sum = 0L;
            {
                long i__prev1 = i;

                for (i = 0L; i < bpp; i++)
                {
                    cdat4[i] = cdat0[i] - pdat[i];
                    sum += abs8(cdat4[i]);
                }


                i = i__prev1;
            }
            {
                long i__prev1 = i;

                for (i = bpp; i < n; i++)
                {
                    cdat4[i] = cdat0[i] - paeth(cdat0[i - bpp], pdat[i], pdat[i - bpp]);
                    sum += abs8(cdat4[i]);
                    if (sum >= best)
                    {
                        break;
                    }
                }


                i = i__prev1;
            }
            if (sum < best)
            {
                best = sum;
                filter = ftPaeth;
            } 

            // The none filter.
            sum = 0L;
            {
                long i__prev1 = i;

                for (i = 0L; i < n; i++)
                {
                    sum += abs8(cdat0[i]);
                    if (sum >= best)
                    {
                        break;
                    }
                }


                i = i__prev1;
            }
            if (sum < best)
            {
                best = sum;
                filter = ftNone;
            } 

            // The sub filter.
            sum = 0L;
            {
                long i__prev1 = i;

                for (i = 0L; i < bpp; i++)
                {
                    cdat1[i] = cdat0[i];
                    sum += abs8(cdat1[i]);
                }


                i = i__prev1;
            }
            {
                long i__prev1 = i;

                for (i = bpp; i < n; i++)
                {
                    cdat1[i] = cdat0[i] - cdat0[i - bpp];
                    sum += abs8(cdat1[i]);
                    if (sum >= best)
                    {
                        break;
                    }
                }


                i = i__prev1;
            }
            if (sum < best)
            {
                best = sum;
                filter = ftSub;
            } 

            // The average filter.
            sum = 0L;
            {
                long i__prev1 = i;

                for (i = 0L; i < bpp; i++)
                {
                    cdat3[i] = cdat0[i] - pdat[i] / 2L;
                    sum += abs8(cdat3[i]);
                }


                i = i__prev1;
            }
            {
                long i__prev1 = i;

                for (i = bpp; i < n; i++)
                {
                    cdat3[i] = cdat0[i] - uint8((int(cdat0[i - bpp]) + int(pdat[i])) / 2L);
                    sum += abs8(cdat3[i]);
                    if (sum >= best)
                    {
                        break;
                    }
                }


                i = i__prev1;
            }
            if (sum < best)
            {
                best = sum;
                filter = ftAverage;
            }
            return filter;
        }

        private static void zeroMemory(slice<byte> v)
        {
            foreach (var (i) in v)
            {
                v[i] = 0L;
            }
        }

        private static error writeImage(this ref encoder _e, io.Writer w, image.Image m, long cb, long level) => func(_e, (ref encoder e, Defer defer, Panic _, Recover __) =>
        {
            if (e.zw == null || e.zwLevel != level)
            {
                var (zw, err) = zlib.NewWriterLevel(w, level);
                if (err != null)
                {
                    return error.As(err);
                }
                e.zw = zw;
                e.zwLevel = level;
            }
            else
            {
                e.zw.Reset(w);
            }
            defer(e.zw.Close());

            long bpp = 0L; // Bytes per pixel.


            if (cb == cbG8) 
                bpp = 1L;
            else if (cb == cbTC8) 
                bpp = 3L;
            else if (cb == cbP8) 
                bpp = 1L;
            else if (cb == cbTCA8) 
                bpp = 4L;
            else if (cb == cbTC16) 
                bpp = 6L;
            else if (cb == cbTCA16) 
                bpp = 8L;
            else if (cb == cbG16) 
                bpp = 2L;
            // cr[*] and pr are the bytes for the current and previous row.
            // cr[0] is unfiltered (or equivalently, filtered with the ftNone filter).
            // cr[ft], for non-zero filter types ft, are buffers for transforming cr[0] under the
            // other PNG filter types. These buffers are allocated once and re-used for each row.
            // The +1 is for the per-row filter type, which is at cr[*][0].
            var b = m.Bounds();
            long sz = 1L + bpp * b.Dx();
            {
                var i__prev1 = i;

                foreach (var (__i) in e.cr)
                {
                    i = __i;
                    if (cap(e.cr[i]) < sz)
                    {
                        e.cr[i] = make_slice<byte>(sz);
                    }
                    else
                    {
                        e.cr[i] = e.cr[i][..sz];
                    }
                    e.cr[i][0L] = uint8(i);
                }

                i = i__prev1;
            }

            var cr = e.cr;
            if (cap(e.pr) < sz)
            {
                e.pr = make_slice<byte>(sz);
            }
            else
            {
                e.pr = e.pr[..sz];
                zeroMemory(e.pr);
            }
            var pr = e.pr;

            ref image.Gray (gray, _) = m._<ref image.Gray>();
            ref image.RGBA (rgba, _) = m._<ref image.RGBA>();
            ref image.Paletted (paletted, _) = m._<ref image.Paletted>();
            ref image.NRGBA (nrgba, _) = m._<ref image.NRGBA>();

            for (var y = b.Min.Y; y < b.Max.Y; y++)
            { 
                // Convert from colors to bytes.
                long i = 1L;

                if (cb == cbG8) 
                    if (gray != null)
                    {
                        var offset = (y - b.Min.Y) * gray.Stride;
                        copy(cr[0L][1L..], gray.Pix[offset..offset + b.Dx()]);
                    }
                    else
                    {
                        {
                            var x__prev2 = x;

                            for (var x = b.Min.X; x < b.Max.X; x++)
                            {
                                color.Gray c = color.GrayModel.Convert(m.At(x, y))._<color.Gray>();
                                cr[0L][i] = c.Y;
                                i++;
                            }


                            x = x__prev2;
                        }
                    }
                else if (cb == cbTC8) 
                    // We have previously verified that the alpha value is fully opaque.
                    var cr0 = cr[0L];
                    long stride = 0L;
                    slice<byte> pix = (slice<byte>)null;
                    if (rgba != null)
                    {
                        stride = rgba.Stride;
                        pix = rgba.Pix;
                    }
                    else if (nrgba != null)
                    {
                        stride = nrgba.Stride;
                        pix = nrgba.Pix;
                    }
                    if (stride != 0L)
                    {
                        var j0 = (y - b.Min.Y) * stride;
                        var j1 = j0 + b.Dx() * 4L;
                        {
                            var j = j0;

                            while (j < j1)
                            {
                                cr0[i + 0L] = pix[j + 0L];
                                cr0[i + 1L] = pix[j + 1L];
                                cr0[i + 2L] = pix[j + 2L];
                                i += 3L;
                                j += 4L;
                            }
                    else

                        }
                    }                    {
                        {
                            var x__prev2 = x;

                            for (x = b.Min.X; x < b.Max.X; x++)
                            {
                                var (r, g, b, _) = m.At(x, y).RGBA();
                                cr0[i + 0L] = uint8(r >> (int)(8L));
                                cr0[i + 1L] = uint8(g >> (int)(8L));
                                cr0[i + 2L] = uint8(b >> (int)(8L));
                                i += 3L;
                            }


                            x = x__prev2;
                        }
                    }
                else if (cb == cbP8) 
                    if (paletted != null)
                    {
                        offset = (y - b.Min.Y) * paletted.Stride;
                        copy(cr[0L][1L..], paletted.Pix[offset..offset + b.Dx()]);
                    }
                    else
                    {
                        image.PalettedImage pi = m._<image.PalettedImage>();
                        {
                            var x__prev2 = x;

                            for (x = b.Min.X; x < b.Max.X; x++)
                            {
                                cr[0L][i] = pi.ColorIndexAt(x, y);
                                i += 1L;
                            }


                            x = x__prev2;
                        }
                    }
                else if (cb == cbTCA8) 
                    if (nrgba != null)
                    {
                        offset = (y - b.Min.Y) * nrgba.Stride;
                        copy(cr[0L][1L..], nrgba.Pix[offset..offset + b.Dx() * 4L]);
                    }
                    else
                    { 
                        // Convert from image.Image (which is alpha-premultiplied) to PNG's non-alpha-premultiplied.
                        {
                            var x__prev2 = x;

                            for (x = b.Min.X; x < b.Max.X; x++)
                            {
                                c = color.NRGBAModel.Convert(m.At(x, y))._<color.NRGBA>();
                                cr[0L][i + 0L] = c.R;
                                cr[0L][i + 1L] = c.G;
                                cr[0L][i + 2L] = c.B;
                                cr[0L][i + 3L] = c.A;
                                i += 4L;
                            }


                            x = x__prev2;
                        }
                    }
                else if (cb == cbG16) 
                    {
                        var x__prev2 = x;

                        for (x = b.Min.X; x < b.Max.X; x++)
                        {
                            c = color.Gray16Model.Convert(m.At(x, y))._<color.Gray16>();
                            cr[0L][i + 0L] = uint8(c.Y >> (int)(8L));
                            cr[0L][i + 1L] = uint8(c.Y);
                            i += 2L;
                        }


                        x = x__prev2;
                    }
                else if (cb == cbTC16) 
                    // We have previously verified that the alpha value is fully opaque.
                    {
                        var x__prev2 = x;

                        for (x = b.Min.X; x < b.Max.X; x++)
                        {
                            (r, g, b, _) = m.At(x, y).RGBA();
                            cr[0L][i + 0L] = uint8(r >> (int)(8L));
                            cr[0L][i + 1L] = uint8(r);
                            cr[0L][i + 2L] = uint8(g >> (int)(8L));
                            cr[0L][i + 3L] = uint8(g);
                            cr[0L][i + 4L] = uint8(b >> (int)(8L));
                            cr[0L][i + 5L] = uint8(b);
                            i += 6L;
                        }


                        x = x__prev2;
                    }
                else if (cb == cbTCA16) 
                    // Convert from image.Image (which is alpha-premultiplied) to PNG's non-alpha-premultiplied.
                    {
                        var x__prev2 = x;

                        for (x = b.Min.X; x < b.Max.X; x++)
                        {
                            c = color.NRGBA64Model.Convert(m.At(x, y))._<color.NRGBA64>();
                            cr[0L][i + 0L] = uint8(c.R >> (int)(8L));
                            cr[0L][i + 1L] = uint8(c.R);
                            cr[0L][i + 2L] = uint8(c.G >> (int)(8L));
                            cr[0L][i + 3L] = uint8(c.G);
                            cr[0L][i + 4L] = uint8(c.B >> (int)(8L));
                            cr[0L][i + 5L] = uint8(c.B);
                            cr[0L][i + 6L] = uint8(c.A >> (int)(8L));
                            cr[0L][i + 7L] = uint8(c.A);
                            i += 8L;
                        }


                        x = x__prev2;
                    }
                // Apply the filter.
                // Skip filter for NoCompression and paletted images (cbP8) as
                // "filters are rarely useful on palette images" and will result
                // in larger files (see http://www.libpng.org/pub/png/book/chapter09.html).
                var f = ftNone;
                if (level != zlib.NoCompression && cb != cbP8)
                {
                    f = filter(ref cr, pr, bpp);
                } 

                // Write the compressed bytes.
                {
                    var (_, err) = e.zw.Write(cr[f]);

                    if (err != null)
                    {
                        return error.As(err);
                    } 

                    // The current row for y is the previous row for y+1.

                } 

                // The current row for y is the previous row for y+1.
                pr = cr[0L];
                cr[0L] = pr;
            }

            return error.As(null);
        });

        // Write the actual image data to one or more IDAT chunks.
        private static void writeIDATs(this ref encoder e)
        {
            if (e.err != null)
            {
                return;
            }
            if (e.bw == null)
            {
                e.bw = bufio.NewWriterSize(e, 1L << (int)(15L));
            }
            else
            {
                e.bw.Reset(e);
            }
            e.err = e.writeImage(e.bw, e.m, e.cb, levelToZlib(e.enc.CompressionLevel));
            if (e.err != null)
            {
                return;
            }
            e.err = e.bw.Flush();
        }

        // This function is required because we want the zero value of
        // Encoder.CompressionLevel to map to zlib.DefaultCompression.
        private static long levelToZlib(CompressionLevel l)
        {

            if (l == DefaultCompression) 
                return zlib.DefaultCompression;
            else if (l == NoCompression) 
                return zlib.NoCompression;
            else if (l == BestSpeed) 
                return zlib.BestSpeed;
            else if (l == BestCompression) 
                return zlib.BestCompression;
            else 
                return zlib.DefaultCompression;
                    }

        private static void writeIEND(this ref encoder e)
        {
            e.writeChunk(null, "IEND");

        }

        // Encode writes the Image m to w in PNG format. Any Image may be
        // encoded, but images that are not image.NRGBA might be encoded lossily.
        public static error Encode(io.Writer w, image.Image m)
        {
            Encoder e = default;
            return error.As(e.Encode(w, m));
        }

        // Encode writes the Image m to w in PNG format.
        private static error Encode(this ref Encoder _enc, io.Writer w, image.Image m) => func(_enc, (ref Encoder enc, Defer defer, Panic _, Recover __) =>
        { 
            // Obviously, negative widths and heights are invalid. Furthermore, the PNG
            // spec section 11.2.2 says that zero is invalid. Excessively large images are
            // also rejected.
            var mw = int64(m.Bounds().Dx());
            var mh = int64(m.Bounds().Dy());
            if (mw <= 0L || mh <= 0L || mw >= 1L << (int)(32L) || mh >= 1L << (int)(32L))
            {
                return error.As(FormatError("invalid image size: " + strconv.FormatInt(mw, 10L) + "x" + strconv.FormatInt(mh, 10L)));
            }
            ref encoder e = default;
            if (enc.BufferPool != null)
            {
                var buffer = enc.BufferPool.Get();
                e = (encoder.Value)(buffer);

            }
            if (e == null)
            {
                e = ref new encoder();
            }
            if (enc.BufferPool != null)
            {
                defer(enc.BufferPool.Put((EncoderBuffer.Value)(e)));
            }
            e.enc = enc;
            e.w = w;
            e.m = m;

            color.Palette pal = default; 
            // cbP8 encoding needs PalettedImage's ColorIndexAt method.
            {
                image.PalettedImage (_, ok) = m._<image.PalettedImage>();

                if (ok)
                {
                    pal, _ = m.ColorModel()._<color.Palette>();
                }

            }
            if (pal != null)
            {
                e.cb = cbP8;
            }
            else
            {

                if (m.ColorModel() == color.GrayModel) 
                    e.cb = cbG8;
                else if (m.ColorModel() == color.Gray16Model) 
                    e.cb = cbG16;
                else if (m.ColorModel() == color.RGBAModel || m.ColorModel() == color.NRGBAModel || m.ColorModel() == color.AlphaModel) 
                    if (opaque(m))
                    {
                        e.cb = cbTC8;
                    }
                    else
                    {
                        e.cb = cbTCA8;
                    }
                else 
                    if (opaque(m))
                    {
                        e.cb = cbTC16;
                    }
                    else
                    {
                        e.cb = cbTCA16;
                    }
                            }
            _, e.err = io.WriteString(w, pngHeader);
            e.writeIHDR();
            if (pal != null)
            {
                e.writePLTEAndTRNS(pal);
            }
            e.writeIDATs();
            e.writeIEND();
            return error.As(e.err);
        });
    }
}}
