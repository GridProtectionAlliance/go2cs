// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package io -- go2cs converted at 2020 October 09 04:49:26 UTC
// import "io" ==> using io = go.io_package
// Original source: C:\Go\src\io\multi.go

using static go.builtin;

namespace go
{
    public static partial class io_package
    {
        private partial struct eofReader
        {
        }

        private static (long, error) Read(this eofReader _p0, slice<byte> _p0)
        {
            long _p0 = default;
            error _p0 = default!;

            return (0L, error.As(EOF)!);
        }

        private partial struct multiReader
        {
            public slice<Reader> readers;
        }

        private static (long, error) Read(this ptr<multiReader> _addr_mr, slice<byte> p)
        {
            long n = default;
            error err = default!;
            ref multiReader mr = ref _addr_mr.val;

            while (len(mr.readers) > 0L)
            { 
                // Optimization to flatten nested multiReaders (Issue 13558).
                if (len(mr.readers) == 1L)
                {
                    {
                        ptr<multiReader> (r, ok) = mr.readers[0L]._<ptr<multiReader>>();

                        if (ok)
                        {
                            mr.readers = r.readers;
                            continue;
                        }

                    }

                }

                n, err = mr.readers[0L].Read(p);
                if (err == EOF)
                { 
                    // Use eofReader instead of nil to avoid nil panic
                    // after performing flatten (Issue 18232).
                    mr.readers[0L] = new eofReader(); // permit earlier GC
                    mr.readers = mr.readers[1L..];

                }

                if (n > 0L || err != EOF)
                {
                    if (err == EOF && len(mr.readers) > 0L)
                    { 
                        // Don't return EOF yet. More readers remain.
                        err = null;

                    }

                    return ;

                }

            }

            return (0L, error.As(EOF)!);

        }

        // MultiReader returns a Reader that's the logical concatenation of
        // the provided input readers. They're read sequentially. Once all
        // inputs have returned EOF, Read will return EOF.  If any of the readers
        // return a non-nil, non-EOF error, Read will return that error.
        public static Reader MultiReader(params Reader[] readers)
        {
            readers = readers.Clone();

            var r = make_slice<Reader>(len(readers));
            copy(r, readers);
            return addr(new multiReader(r));
        }

        private partial struct multiWriter
        {
            public slice<Writer> writers;
        }

        private static (long, error) Write(this ptr<multiWriter> _addr_t, slice<byte> p)
        {
            long n = default;
            error err = default!;
            ref multiWriter t = ref _addr_t.val;

            foreach (var (_, w) in t.writers)
            {
                n, err = w.Write(p);
                if (err != null)
                {
                    return ;
                }

                if (n != len(p))
                {
                    err = ErrShortWrite;
                    return ;
                }

            }
            return (len(p), error.As(null!)!);

        }

        private static StringWriter _ = (multiWriter.val)(null);

        private static (long, error) WriteString(this ptr<multiWriter> _addr_t, @string s)
        {
            long n = default;
            error err = default!;
            ref multiWriter t = ref _addr_t.val;

            slice<byte> p = default; // lazily initialized if/when needed
            foreach (var (_, w) in t.writers)
            {
                {
                    StringWriter (sw, ok) = w._<StringWriter>();

                    if (ok)
                    {
                        n, err = sw.WriteString(s);
                    }
                    else
                    {
                        if (p == null)
                        {
                            p = (slice<byte>)s;
                        }

                        n, err = w.Write(p);

                    }

                }

                if (err != null)
                {
                    return ;
                }

                if (n != len(s))
                {
                    err = ErrShortWrite;
                    return ;
                }

            }
            return (len(s), error.As(null!)!);

        }

        // MultiWriter creates a writer that duplicates its writes to all the
        // provided writers, similar to the Unix tee(1) command.
        //
        // Each write is written to each listed writer, one at a time.
        // If a listed writer returns an error, that overall write operation
        // stops and returns the error; it does not continue down the list.
        public static Writer MultiWriter(params Writer[] writers)
        {
            writers = writers.Clone();

            var allWriters = make_slice<Writer>(0L, len(writers));
            foreach (var (_, w) in writers)
            {
                {
                    ptr<multiWriter> (mw, ok) = w._<ptr<multiWriter>>();

                    if (ok)
                    {
                        allWriters = append(allWriters, mw.writers);
                    }
                    else
                    {
                        allWriters = append(allWriters, w);
                    }

                }

            }
            return addr(new multiWriter(allWriters));

        }
    }
}
