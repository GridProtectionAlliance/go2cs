// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package io -- go2cs converted at 2020 August 29 08:21:54 UTC
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
            return (0L, EOF);
        }

        private partial struct multiReader
        {
            public slice<Reader> readers;
        }

        private static (long, error) Read(this ref multiReader mr, slice<byte> p)
        {
            while (len(mr.readers) > 0L)
            { 
                // Optimization to flatten nested multiReaders (Issue 13558).
                if (len(mr.readers) == 1L)
                {
                    {
                        ref multiReader (r, ok) = mr.readers[0L]._<ref multiReader>();

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
                    return;
                }
            }

            return (0L, EOF);
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
            return ref new multiReader(r);
        }

        private partial struct multiWriter
        {
            public slice<Writer> writers;
        }

        private static (long, error) Write(this ref multiWriter t, slice<byte> p)
        {
            foreach (var (_, w) in t.writers)
            {
                n, err = w.Write(p);
                if (err != null)
                {
                    return;
                }
                if (n != len(p))
                {
                    err = ErrShortWrite;
                    return;
                }
            }
            return (len(p), null);
        }

        private static stringWriter _ = (multiWriter.Value)(null);

        private static (long, error) WriteString(this ref multiWriter t, @string s)
        {
            slice<byte> p = default; // lazily initialized if/when needed
            foreach (var (_, w) in t.writers)
            {
                {
                    stringWriter (sw, ok) = w._<stringWriter>();

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
                    return;
                }
                if (n != len(s))
                {
                    err = ErrShortWrite;
                    return;
                }
            }
            return (len(s), null);
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
                    ref multiWriter (mw, ok) = w._<ref multiWriter>();

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
            return ref new multiWriter(allWriters);
        }
    }
}
