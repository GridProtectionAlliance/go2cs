// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package pem implements the PEM data encoding, which originated in Privacy
// Enhanced Mail. The most common use of PEM encoding today is in TLS keys and
// certificates. See RFC 1421.
// package pem -- go2cs converted at 2020 October 09 04:54:43 UTC
// import "encoding/pem" ==> using pem = go.encoding.pem_package
// Original source: C:\Go\src\encoding\pem\pem.go
using bytes = go.bytes_package;
using base64 = go.encoding.base64_package;
using errors = go.errors_package;
using io = go.io_package;
using sort = go.sort_package;
using strings = go.strings_package;
using static go.builtin;

namespace go {
namespace encoding
{
    public static partial class pem_package
    {
        // A Block represents a PEM encoded structure.
        //
        // The encoded form is:
        //    -----BEGIN Type-----
        //    Headers
        //    base64-encoded Bytes
        //    -----END Type-----
        // where Headers is a possibly empty sequence of Key: Value lines.
        public partial struct Block
        {
            public @string Type; // The type, taken from the preamble (i.e. "RSA PRIVATE KEY").
            public map<@string, @string> Headers; // Optional headers.
            public slice<byte> Bytes; // The decoded bytes of the contents. Typically a DER encoded ASN.1 structure.
        }

        // getLine results the first \r\n or \n delineated line from the given byte
        // array. The line does not include trailing whitespace or the trailing new
        // line bytes. The remainder of the byte array (also not including the new line
        // bytes) is also returned and this will always be smaller than the original
        // argument.
        private static (slice<byte>, slice<byte>) getLine(slice<byte> data)
        {
            slice<byte> line = default;
            slice<byte> rest = default;

            var i = bytes.IndexByte(data, '\n');
            long j = default;
            if (i < 0L)
            {
                i = len(data);
                j = i;
            }
            else
            {
                j = i + 1L;
                if (i > 0L && data[i - 1L] == '\r')
                {
                    i--;
                }

            }

            return (bytes.TrimRight(data[0L..i], " \t"), data[j..]);

        }

        // removeSpacesAndTabs returns a copy of its input with all spaces and tabs
        // removed, if there were any. Otherwise, the input is returned unchanged.
        //
        // The base64 decoder already skips newline characters, so we don't need to
        // filter them out here.
        private static slice<byte> removeSpacesAndTabs(slice<byte> data)
        {
            if (!bytes.ContainsAny(data, " \t"))
            { 
                // Fast path; most base64 data within PEM contains newlines, but
                // no spaces nor tabs. Skip the extra alloc and work.
                return data;

            }

            var result = make_slice<byte>(len(data));
            long n = 0L;

            foreach (var (_, b) in data)
            {
                if (b == ' ' || b == '\t')
                {
                    continue;
                }

                result[n] = b;
                n++;

            }
            return result[0L..n];

        }

        private static slice<byte> pemStart = (slice<byte>)"\n-----BEGIN ";
        private static slice<byte> pemEnd = (slice<byte>)"\n-----END ";
        private static slice<byte> pemEndOfLine = (slice<byte>)"-----";

        // Decode will find the next PEM formatted block (certificate, private key
        // etc) in the input. It returns that block and the remainder of the input. If
        // no PEM data is found, p is nil and the whole of the input is returned in
        // rest.
        public static (ptr<Block>, slice<byte>) Decode(slice<byte> data)
        {
            ptr<Block> p = default!;
            slice<byte> rest = default;
 
            // pemStart begins with a newline. However, at the very beginning of
            // the byte array, we'll accept the start string without it.
            rest = data;
            if (bytes.HasPrefix(data, pemStart[1L..]))
            {
                rest = rest[len(pemStart) - 1L..len(data)];
            }            {
                var i__prev2 = i;

                var i = bytes.Index(data, pemStart);


                else if (i >= 0L)
                {
                    rest = rest[i + len(pemStart)..len(data)];
                }
                else
                {
                    return (_addr_null!, data);
                }

                i = i__prev2;

            }


            var (typeLine, rest) = getLine(rest);
            if (!bytes.HasSuffix(typeLine, pemEndOfLine))
            {
                return _addr_decodeError(data, rest)!;
            }

            typeLine = typeLine[0L..len(typeLine) - len(pemEndOfLine)];

            p = addr(new Block(Headers:make(map[string]string),Type:string(typeLine),));

            while (true)
            { 
                // This loop terminates because getLine's second result is
                // always smaller than its argument.
                if (len(rest) == 0L)
                {
                    return (_addr_null!, data);
                }

                var (line, next) = getLine(rest);

                i = bytes.IndexByte(line, ':');
                if (i == -1L)
                {
                    break;
                } 

                // TODO(agl): need to cope with values that spread across lines.
                var key = line[..i];
                var val = line[i + 1L..];
                key = bytes.TrimSpace(key);
                val = bytes.TrimSpace(val);
                p.Headers[string(key)] = string(val);
                rest = next;

            }


            long endIndex = default;            long endTrailerIndex = default; 

            // If there were no headers, the END line might occur
            // immediately, without a leading newline.
 

            // If there were no headers, the END line might occur
            // immediately, without a leading newline.
            if (len(p.Headers) == 0L && bytes.HasPrefix(rest, pemEnd[1L..]))
            {
                endIndex = 0L;
                endTrailerIndex = len(pemEnd) - 1L;
            }
            else
            {
                endIndex = bytes.Index(rest, pemEnd);
                endTrailerIndex = endIndex + len(pemEnd);
            }

            if (endIndex < 0L)
            {
                return _addr_decodeError(data, rest)!;
            } 

            // After the "-----" of the ending line, there should be the same type
            // and then a final five dashes.
            var endTrailer = rest[endTrailerIndex..];
            var endTrailerLen = len(typeLine) + len(pemEndOfLine);
            if (len(endTrailer) < endTrailerLen)
            {
                return _addr_decodeError(data, rest)!;
            }

            var restOfEndLine = endTrailer[endTrailerLen..];
            endTrailer = endTrailer[..endTrailerLen];
            if (!bytes.HasPrefix(endTrailer, typeLine) || !bytes.HasSuffix(endTrailer, pemEndOfLine))
            {
                return _addr_decodeError(data, rest)!;
            } 

            // The line must end with only whitespace.
            {
                var (s, _) = getLine(restOfEndLine);

                if (len(s) != 0L)
                {
                    return _addr_decodeError(data, rest)!;
                }

            }


            var base64Data = removeSpacesAndTabs(rest[..endIndex]);
            p.Bytes = make_slice<byte>(base64.StdEncoding.DecodedLen(len(base64Data)));
            var (n, err) = base64.StdEncoding.Decode(p.Bytes, base64Data);
            if (err != null)
            {
                return _addr_decodeError(data, rest)!;
            }

            p.Bytes = p.Bytes[..n]; 

            // the -1 is because we might have only matched pemEnd without the
            // leading newline if the PEM block was empty.
            _, rest = getLine(rest[endIndex + len(pemEnd) - 1L..]);

            return ;

        }

        private static (ptr<Block>, slice<byte>) decodeError(slice<byte> data, slice<byte> rest)
        {
            ptr<Block> _p0 = default!;
            slice<byte> _p0 = default;
 
            // If we get here then we have rejected a likely looking, but
            // ultimately invalid PEM block. We need to start over from a new
            // position. We have consumed the preamble line and will have consumed
            // any lines which could be header lines. However, a valid preamble
            // line is not a valid header line, therefore we cannot have consumed
            // the preamble line for the any subsequent block. Thus, we will always
            // find any valid block, no matter what bytes precede it.
            //
            // For example, if the input is
            //
            //    -----BEGIN MALFORMED BLOCK-----
            //    junk that may look like header lines
            //   or data lines, but no END line
            //
            //    -----BEGIN ACTUAL BLOCK-----
            //    realdata
            //    -----END ACTUAL BLOCK-----
            //
            // we've failed to parse using the first BEGIN line
            // and now will try again, using the second BEGIN line.
            var (p, rest) = Decode(rest);
            if (p == null)
            {
                rest = data;
            }

            return (_addr_p!, rest);

        }

        private static readonly long pemLineLength = (long)64L;



        private partial struct lineBreaker
        {
            public array<byte> line;
            public long used;
            public io.Writer @out;
        }

        private static byte nl = new slice<byte>(new byte[] { '\n' });

        private static (long, error) Write(this ptr<lineBreaker> _addr_l, slice<byte> b)
        {
            long n = default;
            error err = default!;
            ref lineBreaker l = ref _addr_l.val;

            if (l.used + len(b) < pemLineLength)
            {
                copy(l.line[l.used..], b);
                l.used += len(b);
                return (len(b), error.As(null!)!);
            }

            n, err = l.@out.Write(l.line[0L..l.used]);
            if (err != null)
            {
                return ;
            }

            var excess = pemLineLength - l.used;
            l.used = 0L;

            n, err = l.@out.Write(b[0L..excess]);
            if (err != null)
            {
                return ;
            }

            n, err = l.@out.Write(nl);
            if (err != null)
            {
                return ;
            }

            return l.Write(b[excess..]);

        }

        private static error Close(this ptr<lineBreaker> _addr_l)
        {
            error err = default!;
            ref lineBreaker l = ref _addr_l.val;

            if (l.used > 0L)
            {
                _, err = l.@out.Write(l.line[0L..l.used]);
                if (err != null)
                {
                    return ;
                }

                _, err = l.@out.Write(nl);

            }

            return ;

        }

        private static error writeHeader(io.Writer @out, @string k, @string v)
        {
            var (_, err) = @out.Write((slice<byte>)k + ": " + v + "\n");
            return error.As(err)!;
        }

        // Encode writes the PEM encoding of b to out.
        public static error Encode(io.Writer @out, ptr<Block> _addr_b)
        {
            ref Block b = ref _addr_b.val;
 
            // Check for invalid block before writing any output.
            {
                var k__prev1 = k;

                foreach (var (__k) in b.Headers)
                {
                    k = __k;
                    if (strings.Contains(k, ":"))
                    {
                        return error.As(errors.New("pem: cannot encode a header key that contains a colon"))!;
                    }

                } 

                // All errors below are relayed from underlying io.Writer,
                // so it is now safe to write data.

                k = k__prev1;
            }

            {
                var err__prev1 = err;

                var (_, err) = @out.Write(pemStart[1L..]);

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            {
                var err__prev1 = err;

                (_, err) = @out.Write((slice<byte>)b.Type + "-----\n");

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }


            if (len(b.Headers) > 0L)
            {
                const @string procType = (@string)"Proc-Type";

                var h = make_slice<@string>(0L, len(b.Headers));
                var hasProcType = false;
                {
                    var k__prev1 = k;

                    foreach (var (__k) in b.Headers)
                    {
                        k = __k;
                        if (k == procType)
                        {
                            hasProcType = true;
                            continue;
                        }

                        h = append(h, k);

                    } 
                    // The Proc-Type header must be written first.
                    // See RFC 1421, section 4.6.1.1

                    k = k__prev1;
                }

                if (hasProcType)
                {
                    {
                        var err__prev3 = err;

                        var err = writeHeader(out, procType, b.Headers[procType]);

                        if (err != null)
                        {
                            return error.As(err)!;
                        }

                        err = err__prev3;

                    }

                } 
                // For consistency of output, write other headers sorted by key.
                sort.Strings(h);
                {
                    var k__prev1 = k;

                    foreach (var (_, __k) in h)
                    {
                        k = __k;
                        {
                            var err__prev2 = err;

                            err = writeHeader(out, k, b.Headers[k]);

                            if (err != null)
                            {
                                return error.As(err)!;
                            }

                            err = err__prev2;

                        }

                    }

                    k = k__prev1;
                }

                {
                    var err__prev2 = err;

                    (_, err) = @out.Write(nl);

                    if (err != null)
                    {
                        return error.As(err)!;
                    }

                    err = err__prev2;

                }

            }

            ref lineBreaker breaker = ref heap(out ptr<lineBreaker> _addr_breaker);
            breaker.@out = out;

            var b64 = base64.NewEncoder(base64.StdEncoding, _addr_breaker);
            {
                var err__prev1 = err;

                (_, err) = b64.Write(b.Bytes);

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            b64.Close();
            breaker.Close();

            {
                var err__prev1 = err;

                (_, err) = @out.Write(pemEnd[1L..]);

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            (_, err) = @out.Write((slice<byte>)b.Type + "-----\n");
            return error.As(err)!;

        }

        // EncodeToMemory returns the PEM encoding of b.
        //
        // If b has invalid headers and cannot be encoded,
        // EncodeToMemory returns nil. If it is important to
        // report details about this error case, use Encode instead.
        public static slice<byte> EncodeToMemory(ptr<Block> _addr_b)
        {
            ref Block b = ref _addr_b.val;

            ref bytes.Buffer buf = ref heap(out ptr<bytes.Buffer> _addr_buf);
            {
                var err = Encode(_addr_buf, _addr_b);

                if (err != null)
                {
                    return null;
                }

            }

            return buf.Bytes();

        }
    }
}}
