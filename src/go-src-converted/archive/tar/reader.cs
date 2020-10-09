// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package tar -- go2cs converted at 2020 October 09 05:08:03 UTC
// import "archive/tar" ==> using tar = go.archive.tar_package
// Original source: C:\Go\src\archive\tar\reader.go
using bytes = go.bytes_package;
using io = go.io_package;
using ioutil = go.io.ioutil_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using time = go.time_package;
using static go.builtin;
using System;

namespace go {
namespace archive
{
    public static partial class tar_package
    {
        // Reader provides sequential access to the contents of a tar archive.
        // Reader.Next advances to the next file in the archive (including the first),
        // and then Reader can be treated as an io.Reader to access the file's data.
        public partial struct Reader
        {
            public io.Reader r;
            public long pad; // Amount of padding (ignored) after current file entry
            public fileReader curr; // Reader for current file entry
            public block blk; // Buffer to use as temporary local storage

// err is a persistent error.
// It is only the responsibility of every exported method of Reader to
// ensure that this error is sticky.
            public error err;
        }

        private partial interface fileReader : io.Reader, fileState
        {
            (long, error) WriteTo(io.Writer _p0);
        }

        // NewReader creates a new Reader reading from r.
        public static ptr<Reader> NewReader(io.Reader r)
        {
            return addr(new Reader(r:r,curr:&regFileReader{r,0}));
        }

        // Next advances to the next entry in the tar archive.
        // The Header.Size determines how many bytes can be read for the next file.
        // Any remaining data in the current file is automatically discarded.
        //
        // io.EOF is returned at the end of the input.
        private static (ptr<Header>, error) Next(this ptr<Reader> _addr_tr)
        {
            ptr<Header> _p0 = default!;
            error _p0 = default!;
            ref Reader tr = ref _addr_tr.val;

            if (tr.err != null)
            {
                return (_addr_null!, error.As(tr.err)!);
            }

            var (hdr, err) = tr.next();
            tr.err = err;
            return (_addr_hdr!, error.As(err)!);

        }

        private static (ptr<Header>, error) next(this ptr<Reader> _addr_tr)
        {
            ptr<Header> _p0 = default!;
            error _p0 = default!;
            ref Reader tr = ref _addr_tr.val;

            map<@string, @string> paxHdrs = default;
            @string gnuLongName = default;            @string gnuLongLink = default; 

            // Externally, Next iterates through the tar archive as if it is a series of
            // files. Internally, the tar format often uses fake "files" to add meta
            // data that describes the next file. These meta data "files" should not
            // normally be visible to the outside. As such, this loop iterates through
            // one or more "header files" until it finds a "normal file".
 

            // Externally, Next iterates through the tar archive as if it is a series of
            // files. Internally, the tar format often uses fake "files" to add meta
            // data that describes the next file. These meta data "files" should not
            // normally be visible to the outside. As such, this loop iterates through
            // one or more "header files" until it finds a "normal file".
            var format = FormatUSTAR | FormatPAX | FormatGNU;
            while (true)
            { 
                // Discard the remainder of the file and any padding.
                {
                    var err__prev1 = err;

                    var err = discard(tr.r, tr.curr.PhysicalRemaining());

                    if (err != null)
                    {
                        return (_addr_null!, error.As(err)!);
                    }

                    err = err__prev1;

                }

                {
                    var err__prev1 = err;

                    var (_, err) = tryReadFull(tr.r, tr.blk[..tr.pad]);

                    if (err != null)
                    {
                        return (_addr_null!, error.As(err)!);
                    }

                    err = err__prev1;

                }

                tr.pad = 0L;

                var (hdr, rawHdr, err) = tr.readHeader();
                if (err != null)
                {
                    return (_addr_null!, error.As(err)!);
                }

                {
                    var err__prev1 = err;

                    err = tr.handleRegularFile(hdr);

                    if (err != null)
                    {
                        return (_addr_null!, error.As(err)!);
                    }

                    err = err__prev1;

                }

                format.mayOnlyBe(hdr.Format); 

                // Check for PAX/GNU special headers and files.

                if (hdr.Typeflag == TypeXHeader || hdr.Typeflag == TypeXGlobalHeader) 
                    format.mayOnlyBe(FormatPAX);
                    paxHdrs, err = parsePAX(tr);
                    if (err != null)
                    {
                        return (_addr_null!, error.As(err)!);
                    }

                    if (hdr.Typeflag == TypeXGlobalHeader)
                    {
                        mergePAX(_addr_hdr, paxHdrs);
                        return (addr(new Header(Name:hdr.Name,Typeflag:hdr.Typeflag,Xattrs:hdr.Xattrs,PAXRecords:hdr.PAXRecords,Format:format,)), error.As(null!)!);
                    }

                    continue; // This is a meta header affecting the next header
                else if (hdr.Typeflag == TypeGNULongName || hdr.Typeflag == TypeGNULongLink) 
                    format.mayOnlyBe(FormatGNU);
                    var (realname, err) = ioutil.ReadAll(tr);
                    if (err != null)
                    {
                        return (_addr_null!, error.As(err)!);
                    }

                    parser p = default;

                    if (hdr.Typeflag == TypeGNULongName) 
                        gnuLongName = p.parseString(realname);
                    else if (hdr.Typeflag == TypeGNULongLink) 
                        gnuLongLink = p.parseString(realname);
                                        continue; // This is a meta header affecting the next header
                else 
                    // The old GNU sparse format is handled here since it is technically
                    // just a regular file with additional attributes.

                    {
                        var err__prev1 = err;

                        err = mergePAX(_addr_hdr, paxHdrs);

                        if (err != null)
                        {
                            return (_addr_null!, error.As(err)!);
                        }

                        err = err__prev1;

                    }

                    if (gnuLongName != "")
                    {
                        hdr.Name = gnuLongName;
                    }

                    if (gnuLongLink != "")
                    {
                        hdr.Linkname = gnuLongLink;
                    }

                    if (hdr.Typeflag == TypeRegA)
                    {
                        if (strings.HasSuffix(hdr.Name, "/"))
                        {
                            hdr.Typeflag = TypeDir; // Legacy archives use trailing slash for directories
                        }
                        else
                        {
                            hdr.Typeflag = TypeReg;
                        }

                    } 

                    // The extended headers may have updated the size.
                    // Thus, setup the regFileReader again after merging PAX headers.
                    {
                        var err__prev1 = err;

                        err = tr.handleRegularFile(hdr);

                        if (err != null)
                        {
                            return (_addr_null!, error.As(err)!);
                        } 

                        // Sparse formats rely on being able to read from the logical data
                        // section; there must be a preceding call to handleRegularFile.

                        err = err__prev1;

                    } 

                    // Sparse formats rely on being able to read from the logical data
                    // section; there must be a preceding call to handleRegularFile.
                    {
                        var err__prev1 = err;

                        err = tr.handleSparseFile(hdr, rawHdr);

                        if (err != null)
                        {
                            return (_addr_null!, error.As(err)!);
                        } 

                        // Set the final guess at the format.

                        err = err__prev1;

                    } 

                    // Set the final guess at the format.
                    if (format.has(FormatUSTAR) && format.has(FormatPAX))
                    {
                        format.mayOnlyBe(FormatUSTAR);
                    }

                    hdr.Format = format;
                    return (_addr_hdr!, error.As(null!)!); // This is a file, so stop
                            }


        }

        // handleRegularFile sets up the current file reader and padding such that it
        // can only read the following logical data section. It will properly handle
        // special headers that contain no data section.
        private static error handleRegularFile(this ptr<Reader> _addr_tr, ptr<Header> _addr_hdr)
        {
            ref Reader tr = ref _addr_tr.val;
            ref Header hdr = ref _addr_hdr.val;

            var nb = hdr.Size;
            if (isHeaderOnlyType(hdr.Typeflag))
            {
                nb = 0L;
            }

            if (nb < 0L)
            {
                return error.As(ErrHeader)!;
            }

            tr.pad = blockPadding(nb);
            tr.curr = addr(new regFileReader(r:tr.r,nb:nb));
            return error.As(null!)!;

        }

        // handleSparseFile checks if the current file is a sparse format of any type
        // and sets the curr reader appropriately.
        private static error handleSparseFile(this ptr<Reader> _addr_tr, ptr<Header> _addr_hdr, ptr<block> _addr_rawHdr)
        {
            ref Reader tr = ref _addr_tr.val;
            ref Header hdr = ref _addr_hdr.val;
            ref block rawHdr = ref _addr_rawHdr.val;

            sparseDatas spd = default;
            error err = default!;
            if (hdr.Typeflag == TypeGNUSparse)
            {
                spd, err = tr.readOldGNUSparseMap(hdr, rawHdr);
            }
            else
            {
                spd, err = tr.readGNUSparsePAXHeaders(hdr);
            } 

            // If sp is non-nil, then this is a sparse file.
            // Note that it is possible for len(sp) == 0.
            if (err == null && spd != null)
            {
                if (isHeaderOnlyType(hdr.Typeflag) || !validateSparseEntries(spd, hdr.Size))
                {
                    return error.As(ErrHeader)!;
                }

                var sph = invertSparseEntries(spd, hdr.Size);
                tr.curr = addr(new sparseFileReader(tr.curr,sph,0));

            }

            return error.As(err)!;

        }

        // readGNUSparsePAXHeaders checks the PAX headers for GNU sparse headers.
        // If they are found, then this function reads the sparse map and returns it.
        // This assumes that 0.0 headers have already been converted to 0.1 headers
        // by the PAX header parsing logic.
        private static (sparseDatas, error) readGNUSparsePAXHeaders(this ptr<Reader> _addr_tr, ptr<Header> _addr_hdr)
        {
            sparseDatas _p0 = default;
            error _p0 = default!;
            ref Reader tr = ref _addr_tr.val;
            ref Header hdr = ref _addr_hdr.val;
 
            // Identify the version of GNU headers.
            bool is1x0 = default;
            var major = hdr.PAXRecords[paxGNUSparseMajor];
            var minor = hdr.PAXRecords[paxGNUSparseMinor];

            if (major == "0" && (minor == "0" || minor == "1")) 
                is1x0 = false;
            else if (major == "1" && minor == "0") 
                is1x0 = true;
            else if (major != "" || minor != "") 
                return (null, error.As(null!)!); // Unknown GNU sparse PAX version
            else if (hdr.PAXRecords[paxGNUSparseMap] != "") 
                is1x0 = false; // 0.0 and 0.1 did not have explicit version records, so guess
            else 
                return (null, error.As(null!)!); // Not a PAX format GNU sparse file.
                        hdr.Format.mayOnlyBe(FormatPAX); 

            // Update hdr from GNU sparse PAX headers.
            {
                var name = hdr.PAXRecords[paxGNUSparseName];

                if (name != "")
                {
                    hdr.Name = name;
                }

            }

            var size = hdr.PAXRecords[paxGNUSparseSize];
            if (size == "")
            {
                size = hdr.PAXRecords[paxGNUSparseRealSize];
            }

            if (size != "")
            {
                var (n, err) = strconv.ParseInt(size, 10L, 64L);
                if (err != null)
                {
                    return (null, error.As(ErrHeader)!);
                }

                hdr.Size = n;

            } 

            // Read the sparse map according to the appropriate format.
            if (is1x0)
            {
                return readGNUSparseMap1x0(tr.curr);
            }

            return readGNUSparseMap0x1(hdr.PAXRecords);

        }

        // mergePAX merges paxHdrs into hdr for all relevant fields of Header.
        private static error mergePAX(ptr<Header> _addr_hdr, map<@string, @string> paxHdrs)
        {
            error err = default!;
            ref Header hdr = ref _addr_hdr.val;

            foreach (var (k, v) in paxHdrs)
            {
                if (v == "")
                {
                    continue; // Keep the original USTAR value
                }

                long id64 = default;

                if (k == paxPath) 
                    hdr.Name = v;
                else if (k == paxLinkpath) 
                    hdr.Linkname = v;
                else if (k == paxUname) 
                    hdr.Uname = v;
                else if (k == paxGname) 
                    hdr.Gname = v;
                else if (k == paxUid) 
                    id64, err = strconv.ParseInt(v, 10L, 64L);
                    hdr.Uid = int(id64); // Integer overflow possible
                else if (k == paxGid) 
                    id64, err = strconv.ParseInt(v, 10L, 64L);
                    hdr.Gid = int(id64); // Integer overflow possible
                else if (k == paxAtime) 
                    hdr.AccessTime, err = parsePAXTime(v);
                else if (k == paxMtime) 
                    hdr.ModTime, err = parsePAXTime(v);
                else if (k == paxCtime) 
                    hdr.ChangeTime, err = parsePAXTime(v);
                else if (k == paxSize) 
                    hdr.Size, err = strconv.ParseInt(v, 10L, 64L);
                else 
                    if (strings.HasPrefix(k, paxSchilyXattr))
                    {
                        if (hdr.Xattrs == null)
                        {
                            hdr.Xattrs = make_map<@string, @string>();
                        }

                        hdr.Xattrs[k[len(paxSchilyXattr)..]] = v;

                    }

                                if (err != null)
                {
                    return error.As(ErrHeader)!;
                }

            }
            hdr.PAXRecords = paxHdrs;
            return error.As(null!)!;

        }

        // parsePAX parses PAX headers.
        // If an extended header (type 'x') is invalid, ErrHeader is returned
        private static (map<@string, @string>, error) parsePAX(io.Reader r)
        {
            map<@string, @string> _p0 = default;
            error _p0 = default!;

            var (buf, err) = ioutil.ReadAll(r);
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            var sbuf = string(buf); 

            // For GNU PAX sparse format 0.0 support.
            // This function transforms the sparse format 0.0 headers into format 0.1
            // headers since 0.0 headers were not PAX compliant.
            slice<@string> sparseMap = default;

            var paxHdrs = make_map<@string, @string>();
            while (len(sbuf) > 0L)
            {
                var (key, value, residual, err) = parsePAXRecord(sbuf);
                if (err != null)
                {
                    return (null, error.As(ErrHeader)!);
                }

                sbuf = residual;


                if (key == paxGNUSparseOffset || key == paxGNUSparseNumBytes) 
                    // Validate sparse header order and value.
                    if ((len(sparseMap) % 2L == 0L && key != paxGNUSparseOffset) || (len(sparseMap) % 2L == 1L && key != paxGNUSparseNumBytes) || strings.Contains(value, ","))
                    {
                        return (null, error.As(ErrHeader)!);
                    }

                    sparseMap = append(sparseMap, value);
                else 
                    paxHdrs[key] = value;
                
            }

            if (len(sparseMap) > 0L)
            {
                paxHdrs[paxGNUSparseMap] = strings.Join(sparseMap, ",");
            }

            return (paxHdrs, error.As(null!)!);

        }

        // readHeader reads the next block header and assumes that the underlying reader
        // is already aligned to a block boundary. It returns the raw block of the
        // header in case further processing is required.
        //
        // The err will be set to io.EOF only when one of the following occurs:
        //    * Exactly 0 bytes are read and EOF is hit.
        //    * Exactly 1 block of zeros is read and EOF is hit.
        //    * At least 2 blocks of zeros are read.
        private static (ptr<Header>, ptr<block>, error) readHeader(this ptr<Reader> _addr_tr)
        {
            ptr<Header> _p0 = default!;
            ptr<block> _p0 = default!;
            error _p0 = default!;
            ref Reader tr = ref _addr_tr.val;
 
            // Two blocks of zero bytes marks the end of the archive.
            {
                var (_, err) = io.ReadFull(tr.r, tr.blk[..]);

                if (err != null)
                {
                    return (_addr_null!, _addr_null!, error.As(err)!); // EOF is okay here; exactly 0 bytes read
                }

            }

            if (bytes.Equal(tr.blk[..], zeroBlock[..]))
            {
                {
                    (_, err) = io.ReadFull(tr.r, tr.blk[..]);

                    if (err != null)
                    {
                        return (_addr_null!, _addr_null!, error.As(err)!); // EOF is okay here; exactly 1 block of zeros read
                    }

                }

                if (bytes.Equal(tr.blk[..], zeroBlock[..]))
                {
                    return (_addr_null!, _addr_null!, error.As(io.EOF)!); // normal EOF; exactly 2 block of zeros read
                }

                return (_addr_null!, _addr_null!, error.As(ErrHeader)!); // Zero block and then non-zero block
            } 

            // Verify the header matches a known format.
            var format = tr.blk.GetFormat();
            if (format == FormatUnknown)
            {
                return (_addr_null!, _addr_null!, error.As(ErrHeader)!);
            }

            parser p = default;
            ptr<Header> hdr = @new<Header>(); 

            // Unpack the V7 header.
            var v7 = tr.blk.V7();
            hdr.Typeflag = v7.TypeFlag()[0L];
            hdr.Name = p.parseString(v7.Name());
            hdr.Linkname = p.parseString(v7.LinkName());
            hdr.Size = p.parseNumeric(v7.Size());
            hdr.Mode = p.parseNumeric(v7.Mode());
            hdr.Uid = int(p.parseNumeric(v7.UID()));
            hdr.Gid = int(p.parseNumeric(v7.GID()));
            hdr.ModTime = time.Unix(p.parseNumeric(v7.ModTime()), 0L); 

            // Unpack format specific fields.
            if (format > formatV7)
            {
                var ustar = tr.blk.USTAR();
                hdr.Uname = p.parseString(ustar.UserName());
                hdr.Gname = p.parseString(ustar.GroupName());
                hdr.Devmajor = p.parseNumeric(ustar.DevMajor());
                hdr.Devminor = p.parseNumeric(ustar.DevMinor());

                @string prefix = default;

                if (format.has(FormatUSTAR | FormatPAX)) 
                    hdr.Format = format;
                    ustar = tr.blk.USTAR();
                    prefix = p.parseString(ustar.Prefix()); 

                    // For Format detection, check if block is properly formatted since
                    // the parser is more liberal than what USTAR actually permits.
                    Func<int, bool> notASCII = r => _addr_r >= 0x80UL!;
                    if (bytes.IndexFunc(tr.blk[..], notASCII) >= 0L)
                    {
                        hdr.Format = FormatUnknown; // Non-ASCII characters in block.
                    }

                    Func<slice<byte>, bool> nul = b => _addr_int(b[len(b) - 1L]) == 0L!;
                    if (!(nul(v7.Size()) && nul(v7.Mode()) && nul(v7.UID()) && nul(v7.GID()) && nul(v7.ModTime()) && nul(ustar.DevMajor()) && nul(ustar.DevMinor())))
                    {
                        hdr.Format = FormatUnknown; // Numeric fields must end in NUL
                    }

                else if (format.has(formatSTAR)) 
                    var star = tr.blk.STAR();
                    prefix = p.parseString(star.Prefix());
                    hdr.AccessTime = time.Unix(p.parseNumeric(star.AccessTime()), 0L);
                    hdr.ChangeTime = time.Unix(p.parseNumeric(star.ChangeTime()), 0L);
                else if (format.has(FormatGNU)) 
                    hdr.Format = format;
                    parser p2 = default;
                    var gnu = tr.blk.GNU();
                    {
                        var b__prev2 = b;

                        var b = gnu.AccessTime();

                        if (b[0L] != 0L)
                        {
                            hdr.AccessTime = time.Unix(p2.parseNumeric(b), 0L);
                        }

                        b = b__prev2;

                    }

                    {
                        var b__prev2 = b;

                        b = gnu.ChangeTime();

                        if (b[0L] != 0L)
                        {
                            hdr.ChangeTime = time.Unix(p2.parseNumeric(b), 0L);
                        } 

                        // Prior to Go1.8, the Writer had a bug where it would output
                        // an invalid tar file in certain rare situations because the logic
                        // incorrectly believed that the old GNU format had a prefix field.
                        // This is wrong and leads to an output file that mangles the
                        // atime and ctime fields, which are often left unused.
                        //
                        // In order to continue reading tar files created by former, buggy
                        // versions of Go, we skeptically parse the atime and ctime fields.
                        // If we are unable to parse them and the prefix field looks like
                        // an ASCII string, then we fallback on the pre-Go1.8 behavior
                        // of treating these fields as the USTAR prefix field.
                        //
                        // Note that this will not use the fallback logic for all possible
                        // files generated by a pre-Go1.8 toolchain. If the generated file
                        // happened to have a prefix field that parses as valid
                        // atime and ctime fields (e.g., when they are valid octal strings),
                        // then it is impossible to distinguish between a valid GNU file
                        // and an invalid pre-Go1.8 file.
                        //
                        // See https://golang.org/issues/12594
                        // See https://golang.org/issues/21005

                        b = b__prev2;

                    } 

                    // Prior to Go1.8, the Writer had a bug where it would output
                    // an invalid tar file in certain rare situations because the logic
                    // incorrectly believed that the old GNU format had a prefix field.
                    // This is wrong and leads to an output file that mangles the
                    // atime and ctime fields, which are often left unused.
                    //
                    // In order to continue reading tar files created by former, buggy
                    // versions of Go, we skeptically parse the atime and ctime fields.
                    // If we are unable to parse them and the prefix field looks like
                    // an ASCII string, then we fallback on the pre-Go1.8 behavior
                    // of treating these fields as the USTAR prefix field.
                    //
                    // Note that this will not use the fallback logic for all possible
                    // files generated by a pre-Go1.8 toolchain. If the generated file
                    // happened to have a prefix field that parses as valid
                    // atime and ctime fields (e.g., when they are valid octal strings),
                    // then it is impossible to distinguish between a valid GNU file
                    // and an invalid pre-Go1.8 file.
                    //
                    // See https://golang.org/issues/12594
                    // See https://golang.org/issues/21005
                    if (p2.err != null)
                    {
                        hdr.AccessTime = new time.Time();
                        hdr.ChangeTime = new time.Time();
                        ustar = tr.blk.USTAR();
                        {
                            var s = p.parseString(ustar.Prefix());

                            if (isASCII(s))
                            {
                                prefix = s;
                            }

                        }

                        hdr.Format = FormatUnknown; // Buggy file is not GNU
                    }

                                if (len(prefix) > 0L)
                {
                    hdr.Name = prefix + "/" + hdr.Name;
                }

            }

            return (_addr_hdr!, _addr__addr_tr.blk!, error.As(p.err)!);

        }

        // readOldGNUSparseMap reads the sparse map from the old GNU sparse format.
        // The sparse map is stored in the tar header if it's small enough.
        // If it's larger than four entries, then one or more extension headers are used
        // to store the rest of the sparse map.
        //
        // The Header.Size does not reflect the size of any extended headers used.
        // Thus, this function will read from the raw io.Reader to fetch extra headers.
        // This method mutates blk in the process.
        private static (sparseDatas, error) readOldGNUSparseMap(this ptr<Reader> _addr_tr, ptr<Header> _addr_hdr, ptr<block> _addr_blk)
        {
            sparseDatas _p0 = default;
            error _p0 = default!;
            ref Reader tr = ref _addr_tr.val;
            ref Header hdr = ref _addr_hdr.val;
            ref block blk = ref _addr_blk.val;
 
            // Make sure that the input format is GNU.
            // Unfortunately, the STAR format also has a sparse header format that uses
            // the same type flag but has a completely different layout.
            if (blk.GetFormat() != FormatGNU)
            {
                return (null, error.As(ErrHeader)!);
            }

            hdr.Format.mayOnlyBe(FormatGNU);

            parser p = default;
            hdr.Size = p.parseNumeric(blk.GNU().RealSize());
            if (p.err != null)
            {
                return (null, error.As(p.err)!);
            }

            var s = blk.GNU().Sparse();
            var spd = make(sparseDatas, 0L, s.MaxEntries());
            while (true)
            {
                for (long i = 0L; i < s.MaxEntries(); i++)
                { 
                    // This termination condition is identical to GNU and BSD tar.
                    if (s.Entry(i).Offset()[0L] == 0x00UL)
                    {
                        break; // Don't return, need to process extended headers (even if empty)
                    }

                    var offset = p.parseNumeric(s.Entry(i).Offset());
                    var length = p.parseNumeric(s.Entry(i).Length());
                    if (p.err != null)
                    {
                        return (null, error.As(p.err)!);
                    }

                    spd = append(spd, new sparseEntry(Offset:offset,Length:length));

                }


                if (s.IsExtended()[0L] > 0L)
                { 
                    // There are more entries. Read an extension header and parse its entries.
                    {
                        var (_, err) = mustReadFull(tr.r, blk[..]);

                        if (err != null)
                        {
                            return (null, error.As(err)!);
                        }

                    }

                    s = blk.Sparse();
                    continue;

                }

                return (spd, error.As(null!)!); // Done
            }


        }

        // readGNUSparseMap1x0 reads the sparse map as stored in GNU's PAX sparse format
        // version 1.0. The format of the sparse map consists of a series of
        // newline-terminated numeric fields. The first field is the number of entries
        // and is always present. Following this are the entries, consisting of two
        // fields (offset, length). This function must stop reading at the end
        // boundary of the block containing the last newline.
        //
        // Note that the GNU manual says that numeric values should be encoded in octal
        // format. However, the GNU tar utility itself outputs these values in decimal.
        // As such, this library treats values as being encoded in decimal.
        private static (sparseDatas, error) readGNUSparseMap1x0(io.Reader r)
        {
            sparseDatas _p0 = default;
            error _p0 = default!;

            long cntNewline = default;            bytes.Buffer buf = default;            block blk = default; 

            // feedTokens copies data in blocks from r into buf until there are
            // at least cnt newlines in buf. It will not read more blocks than needed.
            Func<long, error> feedTokens = n =>
            {
                while (cntNewline < n)
                {
                    {
                        var err__prev1 = err;

                        var (_, err) = mustReadFull(r, blk[..]);

                        if (err != null)
                        {
                            return err;
                        }

                        err = err__prev1;

                    }

                    buf.Write(blk[..]);
                    foreach (var (_, c) in blk)
                    {
                        if (c == '\n')
                        {
                            cntNewline++;
                        }

                    }

                }

                return null;

            } 

            // nextToken gets the next token delimited by a newline. This assumes that
            // at least one newline exists in the buffer.
; 

            // nextToken gets the next token delimited by a newline. This assumes that
            // at least one newline exists in the buffer.
            Func<@string> nextToken = () =>
            {
                cntNewline--;
                var (tok, _) = buf.ReadString('\n');
                return strings.TrimRight(tok, "\n");
            } 

            // Parse for the number of entries.
            // Use integer overflow resistant math to check this.
; 

            // Parse for the number of entries.
            // Use integer overflow resistant math to check this.
            {
                var err__prev1 = err;

                var err = feedTokens(1L);

                if (err != null)
                {
                    return (null, error.As(err)!);
                }

                err = err__prev1;

            }

            var (numEntries, err) = strconv.ParseInt(nextToken(), 10L, 0L); // Intentionally parse as native int
            if (err != null || numEntries < 0L || int(2L * numEntries) < int(numEntries))
            {
                return (null, error.As(ErrHeader)!);
            } 

            // Parse for all member entries.
            // numEntries is trusted after this since a potential attacker must have
            // committed resources proportional to what this library used.
            {
                var err__prev1 = err;

                err = feedTokens(2L * numEntries);

                if (err != null)
                {
                    return (null, error.As(err)!);
                }

                err = err__prev1;

            }

            var spd = make(sparseDatas, 0L, numEntries);
            for (var i = int64(0L); i < numEntries; i++)
            {
                var (offset, err1) = strconv.ParseInt(nextToken(), 10L, 64L);
                var (length, err2) = strconv.ParseInt(nextToken(), 10L, 64L);
                if (err1 != null || err2 != null)
                {
                    return (null, error.As(ErrHeader)!);
                }

                spd = append(spd, new sparseEntry(Offset:offset,Length:length));

            }

            return (spd, error.As(null!)!);

        }

        // readGNUSparseMap0x1 reads the sparse map as stored in GNU's PAX sparse format
        // version 0.1. The sparse map is stored in the PAX headers.
        private static (sparseDatas, error) readGNUSparseMap0x1(map<@string, @string> paxHdrs)
        {
            sparseDatas _p0 = default;
            error _p0 = default!;
 
            // Get number of entries.
            // Use integer overflow resistant math to check this.
            var numEntriesStr = paxHdrs[paxGNUSparseNumBlocks];
            var (numEntries, err) = strconv.ParseInt(numEntriesStr, 10L, 0L); // Intentionally parse as native int
            if (err != null || numEntries < 0L || int(2L * numEntries) < int(numEntries))
            {
                return (null, error.As(ErrHeader)!);
            } 

            // There should be two numbers in sparseMap for each entry.
            var sparseMap = strings.Split(paxHdrs[paxGNUSparseMap], ",");
            if (len(sparseMap) == 1L && sparseMap[0L] == "")
            {
                sparseMap = sparseMap[..0L];
            }

            if (int64(len(sparseMap)) != 2L * numEntries)
            {
                return (null, error.As(ErrHeader)!);
            } 

            // Loop through the entries in the sparse map.
            // numEntries is trusted now.
            var spd = make(sparseDatas, 0L, numEntries);
            while (len(sparseMap) >= 2L)
            {
                var (offset, err1) = strconv.ParseInt(sparseMap[0L], 10L, 64L);
                var (length, err2) = strconv.ParseInt(sparseMap[1L], 10L, 64L);
                if (err1 != null || err2 != null)
                {
                    return (null, error.As(ErrHeader)!);
                }

                spd = append(spd, new sparseEntry(Offset:offset,Length:length));
                sparseMap = sparseMap[2L..];

            }

            return (spd, error.As(null!)!);

        }

        // Read reads from the current file in the tar archive.
        // It returns (0, io.EOF) when it reaches the end of that file,
        // until Next is called to advance to the next file.
        //
        // If the current file is sparse, then the regions marked as a hole
        // are read back as NUL-bytes.
        //
        // Calling Read on special types like TypeLink, TypeSymlink, TypeChar,
        // TypeBlock, TypeDir, and TypeFifo returns (0, io.EOF) regardless of what
        // the Header.Size claims.
        private static (long, error) Read(this ptr<Reader> _addr_tr, slice<byte> b)
        {
            long _p0 = default;
            error _p0 = default!;
            ref Reader tr = ref _addr_tr.val;

            if (tr.err != null)
            {
                return (0L, error.As(tr.err)!);
            }

            var (n, err) = tr.curr.Read(b);
            if (err != null && err != io.EOF)
            {
                tr.err = err;
            }

            return (n, error.As(err)!);

        }

        // writeTo writes the content of the current file to w.
        // The bytes written matches the number of remaining bytes in the current file.
        //
        // If the current file is sparse and w is an io.WriteSeeker,
        // then writeTo uses Seek to skip past holes defined in Header.SparseHoles,
        // assuming that skipped regions are filled with NULs.
        // This always writes the last byte to ensure w is the right size.
        //
        // TODO(dsnet): Re-export this when adding sparse file support.
        // See https://golang.org/issue/22735
        private static (long, error) writeTo(this ptr<Reader> _addr_tr, io.Writer w)
        {
            long _p0 = default;
            error _p0 = default!;
            ref Reader tr = ref _addr_tr.val;

            if (tr.err != null)
            {
                return (0L, error.As(tr.err)!);
            }

            var (n, err) = tr.curr.WriteTo(w);
            if (err != null)
            {
                tr.err = err;
            }

            return (n, error.As(err)!);

        }

        // regFileReader is a fileReader for reading data from a regular file entry.
        private partial struct regFileReader
        {
            public io.Reader r; // Underlying Reader
            public long nb; // Number of remaining bytes to read
        }

        private static (long, error) Read(this ptr<regFileReader> _addr_fr, slice<byte> b)
        {
            long n = default;
            error err = default!;
            ref regFileReader fr = ref _addr_fr.val;

            if (int64(len(b)) > fr.nb)
            {
                b = b[..fr.nb];
            }

            if (len(b) > 0L)
            {
                n, err = fr.r.Read(b);
                fr.nb -= int64(n);
            }


            if (err == io.EOF && fr.nb > 0L) 
                return (n, error.As(io.ErrUnexpectedEOF)!);
            else if (err == null && fr.nb == 0L) 
                return (n, error.As(io.EOF)!);
            else 
                return (n, error.As(err)!);
            
        }

        private static (long, error) WriteTo(this ptr<regFileReader> _addr_fr, io.Writer w)
        {
            long _p0 = default;
            error _p0 = default!;
            ref regFileReader fr = ref _addr_fr.val;

            return io.Copy(w, /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{io.Reader}{fr});
        }

        private static long LogicalRemaining(this regFileReader fr)
        {
            return fr.nb;
        }

        private static long PhysicalRemaining(this regFileReader fr)
        {
            return fr.nb;
        }

        // sparseFileReader is a fileReader for reading data from a sparse file entry.
        private partial struct sparseFileReader
        {
            public fileReader fr; // Underlying fileReader
            public sparseHoles sp; // Normalized list of sparse holes
            public long pos; // Current position in sparse file
        }

        private static (long, error) Read(this ptr<sparseFileReader> _addr_sr, slice<byte> b)
        {
            long n = default;
            error err = default!;
            ref sparseFileReader sr = ref _addr_sr.val;

            var finished = int64(len(b)) >= sr.LogicalRemaining();
            if (finished)
            {
                b = b[..sr.LogicalRemaining()];
            }

            var b0 = b;
            var endPos = sr.pos + int64(len(b));
            while (endPos > sr.pos && err == null)
            {
                long nf = default; // Bytes read in fragment
                var holeStart = sr.sp[0L].Offset;
                var holeEnd = sr.sp[0L].endOffset();
                if (sr.pos < holeStart)
                { // In a data fragment
                    var bf = b[..min(int64(len(b)), holeStart - sr.pos)];
                    nf, err = tryReadFull(sr.fr, bf);

                }
                else
                { // In a hole fragment
                    bf = b[..min(int64(len(b)), holeEnd - sr.pos)];
                    nf, err = tryReadFull(new zeroReader(), bf);

                }

                b = b[nf..];
                sr.pos += int64(nf);
                if (sr.pos >= holeEnd && len(sr.sp) > 1L)
                {
                    sr.sp = sr.sp[1L..]; // Ensure last fragment always remains
                }

            }


            n = len(b0) - len(b);

            if (err == io.EOF) 
                return (n, error.As(errMissData)!); // Less data in dense file than sparse file
            else if (err != null) 
                return (n, error.As(err)!);
            else if (sr.LogicalRemaining() == 0L && sr.PhysicalRemaining() > 0L) 
                return (n, error.As(errUnrefData)!); // More data in dense file than sparse file
            else if (finished) 
                return (n, error.As(io.EOF)!);
            else 
                return (n, error.As(null!)!);
            
        }

        private static (long, error) WriteTo(this ptr<sparseFileReader> _addr_sr, io.Writer w)
        {
            long n = default;
            error err = default!;
            ref sparseFileReader sr = ref _addr_sr.val;

            io.WriteSeeker (ws, ok) = w._<io.WriteSeeker>();
            if (ok)
            {
                {
                    var (_, err) = ws.Seek(0L, io.SeekCurrent);

                    if (err != null)
                    {
                        ok = false; // Not all io.Seeker can really seek
                    }

                }

            }

            if (!ok)
            {
                return io.Copy(w, /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{io.Reader}{sr});
            }

            bool writeLastByte = default;
            var pos0 = sr.pos;
            while (sr.LogicalRemaining() > 0L && !writeLastByte && err == null)
            {
                long nf = default; // Size of fragment
                var holeStart = sr.sp[0L].Offset;
                var holeEnd = sr.sp[0L].endOffset();
                if (sr.pos < holeStart)
                { // In a data fragment
                    nf = holeStart - sr.pos;
                    nf, err = io.CopyN(ws, sr.fr, nf);

                }
                else
                { // In a hole fragment
                    nf = holeEnd - sr.pos;
                    if (sr.PhysicalRemaining() == 0L)
                    {
                        writeLastByte = true;
                        nf--;
                    }

                    _, err = ws.Seek(nf, io.SeekCurrent);

                }

                sr.pos += nf;
                if (sr.pos >= holeEnd && len(sr.sp) > 1L)
                {
                    sr.sp = sr.sp[1L..]; // Ensure last fragment always remains
                }

            } 

            // If the last fragment is a hole, then seek to 1-byte before EOF, and
            // write a single byte to ensure the file is the right size.
 

            // If the last fragment is a hole, then seek to 1-byte before EOF, and
            // write a single byte to ensure the file is the right size.
            if (writeLastByte && err == null)
            {
                _, err = ws.Write(new slice<byte>(new byte[] { 0 }));
                sr.pos++;
            }

            n = sr.pos - pos0;

            if (err == io.EOF) 
                return (n, error.As(errMissData)!); // Less data in dense file than sparse file
            else if (err != null) 
                return (n, error.As(err)!);
            else if (sr.LogicalRemaining() == 0L && sr.PhysicalRemaining() > 0L) 
                return (n, error.As(errUnrefData)!); // More data in dense file than sparse file
            else 
                return (n, error.As(null!)!);
            
        }

        private static long LogicalRemaining(this sparseFileReader sr)
        {
            return sr.sp[len(sr.sp) - 1L].endOffset() - sr.pos;
        }
        private static long PhysicalRemaining(this sparseFileReader sr)
        {
            return sr.fr.PhysicalRemaining();
        }

        private partial struct zeroReader
        {
        }

        private static (long, error) Read(this zeroReader _p0, slice<byte> b)
        {
            long _p0 = default;
            error _p0 = default!;

            foreach (var (i) in b)
            {
                b[i] = 0L;
            }
            return (len(b), error.As(null!)!);

        }

        // mustReadFull is like io.ReadFull except it returns
        // io.ErrUnexpectedEOF when io.EOF is hit before len(b) bytes are read.
        private static (long, error) mustReadFull(io.Reader r, slice<byte> b)
        {
            long _p0 = default;
            error _p0 = default!;

            var (n, err) = tryReadFull(r, b);
            if (err == io.EOF)
            {
                err = io.ErrUnexpectedEOF;
            }

            return (n, error.As(err)!);

        }

        // tryReadFull is like io.ReadFull except it returns
        // io.EOF when it is hit before len(b) bytes are read.
        private static (long, error) tryReadFull(io.Reader r, slice<byte> b)
        {
            long n = default;
            error err = default!;

            while (len(b) > n && err == null)
            {
                long nn = default;
                nn, err = r.Read(b[n..]);
                n += nn;
            }

            if (len(b) == n && err == io.EOF)
            {
                err = null;
            }

            return (n, error.As(err)!);

        }

        // discard skips n bytes in r, reporting an error if unable to do so.
        private static error discard(io.Reader r, long n)
        { 
            // If possible, Seek to the last byte before the end of the data section.
            // Do this because Seek is often lazy about reporting errors; this will mask
            // the fact that the stream may be truncated. We can rely on the
            // io.CopyN done shortly afterwards to trigger any IO errors.
            long seekSkipped = default; // Number of bytes skipped via Seek
            {
                io.Seeker (sr, ok) = r._<io.Seeker>();

                if (ok && n > 1L)
                { 
                    // Not all io.Seeker can actually Seek. For example, os.Stdin implements
                    // io.Seeker, but calling Seek always returns an error and performs
                    // no action. Thus, we try an innocent seek to the current position
                    // to see if Seek is really supported.
                    var (pos1, err) = sr.Seek(0L, io.SeekCurrent);
                    if (pos1 >= 0L && err == null)
                    { 
                        // Seek seems supported, so perform the real Seek.
                        var (pos2, err) = sr.Seek(n - 1L, io.SeekCurrent);
                        if (pos2 < 0L || err != null)
                        {
                            return error.As(err)!;
                        }

                        seekSkipped = pos2 - pos1;

                    }

                }

            }


            var (copySkipped, err) = io.CopyN(ioutil.Discard, r, n - seekSkipped);
            if (err == io.EOF && seekSkipped + copySkipped < n)
            {
                err = io.ErrUnexpectedEOF;
            }

            return error.As(err)!;

        }
    }
}}
