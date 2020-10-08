// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package quotedprintable -- go2cs converted at 2020 October 08 03:38:32 UTC
// import "mime/quotedprintable" ==> using quotedprintable = go.mime.quotedprintable_package
// Original source: C:\Go\src\mime\quotedprintable\writer.go
using io = go.io_package;
using static go.builtin;

namespace go {
namespace mime
{
    public static partial class quotedprintable_package
    {
        private static readonly long lineMaxLen = (long)76L;

        // A Writer is a quoted-printable writer that implements io.WriteCloser.


        // A Writer is a quoted-printable writer that implements io.WriteCloser.
        public partial struct Writer
        {
            public bool Binary;
            public io.Writer w;
            public long i;
            public array<byte> line;
            public bool cr;
        }

        // NewWriter returns a new Writer that writes to w.
        public static ptr<Writer> NewWriter(io.Writer w)
        {
            return addr(new Writer(w:w));
        }

        // Write encodes p using quoted-printable encoding and writes it to the
        // underlying io.Writer. It limits line length to 76 characters. The encoded
        // bytes are not necessarily flushed until the Writer is closed.
        private static (long, error) Write(this ptr<Writer> _addr_w, slice<byte> p)
        {
            long n = default;
            error err = default!;
            ref Writer w = ref _addr_w.val;

            foreach (var (i, b) in p)
            {

                // Simple writes are done in batch.
                if (b >= '!' && b <= '~' && b != '=') 
                    continue;
                else if (isWhitespace(b) || !w.Binary && (b == '\n' || b == '\r')) 
                    continue;
                                if (i > n)
                {
                    {
                        var err__prev2 = err;

                        var err = w.write(p[n..i]);

                        if (err != null)
                        {
                            return (n, error.As(err)!);
                        }

                        err = err__prev2;

                    }

                    n = i;

                }

                {
                    var err__prev1 = err;

                    err = w.encode(b);

                    if (err != null)
                    {
                        return (n, error.As(err)!);
                    }

                    err = err__prev1;

                }

                n++;

            }
            if (n == len(p))
            {
                return (n, error.As(null!)!);
            }

            {
                var err__prev1 = err;

                err = w.write(p[n..]);

                if (err != null)
                {
                    return (n, error.As(err)!);
                }

                err = err__prev1;

            }


            return (len(p), error.As(null!)!);

        }

        // Close closes the Writer, flushing any unwritten data to the underlying
        // io.Writer, but does not close the underlying io.Writer.
        private static error Close(this ptr<Writer> _addr_w)
        {
            ref Writer w = ref _addr_w.val;

            {
                var err = w.checkLastByte();

                if (err != null)
                {
                    return error.As(err)!;
                }

            }


            return error.As(w.flush())!;

        }

        // write limits text encoded in quoted-printable to 76 characters per line.
        private static error write(this ptr<Writer> _addr_w, slice<byte> p)
        {
            ref Writer w = ref _addr_w.val;

            foreach (var (_, b) in p)
            {
                if (b == '\n' || b == '\r')
                { 
                    // If the previous byte was \r, the CRLF has already been inserted.
                    if (w.cr && b == '\n')
                    {
                        w.cr = false;
                        continue;
                    }

                    if (b == '\r')
                    {
                        w.cr = true;
                    }

                    {
                        var err__prev2 = err;

                        var err = w.checkLastByte();

                        if (err != null)
                        {
                            return error.As(err)!;
                        }

                        err = err__prev2;

                    }

                    {
                        var err__prev2 = err;

                        err = w.insertCRLF();

                        if (err != null)
                        {
                            return error.As(err)!;
                        }

                        err = err__prev2;

                    }

                    continue;

                }

                if (w.i == lineMaxLen - 1L)
                {
                    {
                        var err__prev2 = err;

                        err = w.insertSoftLineBreak();

                        if (err != null)
                        {
                            return error.As(err)!;
                        }

                        err = err__prev2;

                    }

                }

                w.line[w.i] = b;
                w.i++;
                w.cr = false;

            }
            return error.As(null!)!;

        }

        private static error encode(this ptr<Writer> _addr_w, byte b)
        {
            ref Writer w = ref _addr_w.val;

            if (lineMaxLen - 1L - w.i < 3L)
            {
                {
                    var err = w.insertSoftLineBreak();

                    if (err != null)
                    {
                        return error.As(err)!;
                    }

                }

            }

            w.line[w.i] = '=';
            w.line[w.i + 1L] = upperhex[b >> (int)(4L)];
            w.line[w.i + 2L] = upperhex[b & 0x0fUL];
            w.i += 3L;

            return error.As(null!)!;

        }

        private static readonly @string upperhex = (@string)"0123456789ABCDEF";

        // checkLastByte encodes the last buffered byte if it is a space or a tab.


        // checkLastByte encodes the last buffered byte if it is a space or a tab.
        private static error checkLastByte(this ptr<Writer> _addr_w)
        {
            ref Writer w = ref _addr_w.val;

            if (w.i == 0L)
            {
                return error.As(null!)!;
            }

            var b = w.line[w.i - 1L];
            if (isWhitespace(b))
            {
                w.i--;
                {
                    var err = w.encode(b);

                    if (err != null)
                    {
                        return error.As(err)!;
                    }

                }

            }

            return error.As(null!)!;

        }

        private static error insertSoftLineBreak(this ptr<Writer> _addr_w)
        {
            ref Writer w = ref _addr_w.val;

            w.line[w.i] = '=';
            w.i++;

            return error.As(w.insertCRLF())!;
        }

        private static error insertCRLF(this ptr<Writer> _addr_w)
        {
            ref Writer w = ref _addr_w.val;

            w.line[w.i] = '\r';
            w.line[w.i + 1L] = '\n';
            w.i += 2L;

            return error.As(w.flush())!;
        }

        private static error flush(this ptr<Writer> _addr_w)
        {
            ref Writer w = ref _addr_w.val;

            {
                var (_, err) = w.w.Write(w.line[..w.i]);

                if (err != null)
                {
                    return error.As(err)!;
                }

            }


            w.i = 0L;
            return error.As(null!)!;

        }

        private static bool isWhitespace(byte b)
        {
            return b == ' ' || b == '\t';
        }
    }
}}
