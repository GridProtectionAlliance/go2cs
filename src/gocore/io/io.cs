//******************************************************************************************************
//  io_package.cs - Gbtc
//
//  Copyright © 2020, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  06/29/2020 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using errors = go.errors_package;

namespace go
{
    // Copyright 2009 The Go Authors. All rights reserved.
    // Use of this source code is governed by a BSD-style
    // license that can be found in the LICENSE file.

    // Package io provides basic interfaces to I/O primitives.
    // Its primary job is to wrap existing implementations of such primitives,
    // such as those in package os, into shared public interfaces that
    // abstract the functionality, plus some other related primitives.
    //
    // Because these interfaces and primitives wrap lower-level operations with
    // various implementations, unless otherwise informed clients should not
    // assume they are safe for parallel execution.
    public static partial class io_package
    {
        // Seek whence values.
        public const int SeekStart = 0;     // seek relative to the origin of the file
        public const int SeekCurrent = 1;   // seek relative to the current offset
        public const int SeekEnd = 2;       // seek relative to the end

        // ErrShortWrite means that a write accepted fewer bytes than requested
        // but failed to return an explicit error.
        public static readonly error ErrShortWrite = errors.New("short write");

        // ErrShortBuffer means that a read required a longer buffer than was provided.
        public static readonly error ErrShortBuffer = errors.New("short buffer");

        // EOF is the error returned by Read when no more input is available.
        // Functions should return EOF only to signal a graceful end of input.
        // If the EOF occurs unexpectedly in a structured data stream,
        // the appropriate error is either ErrUnexpectedEOF or some other error
        // giving more detail.
        public static readonly error EOF = errors.New("EOF");

        // ErrUnexpectedEOF means that EOF was encountered in the
        // middle of reading a fixed-size block or data structure.
        public static readonly error ErrUnexpectedEOF = errors.New("unexpected EOF");

        // ErrNoProgress is returned by some clients of an io.Reader when
        // many calls to Read have failed to return any data or error,
        // usually the sign of a broken io.Reader implementation.
        public static readonly error ErrNoProgress = errors.New("multiple Read calls return no data or error");
        
        // Reader is the interface that wraps the basic Read method.
        //
        // Read reads up to len(p) bytes into p. It returns the number of bytes
        // read (0 <= n <= len(p)) and any error encountered. Even if Read
        // returns n < len(p), it may use all of p as scratch space during the call.
        // If some data is available but not len(p) bytes, Read conventionally
        // returns what is available instead of waiting for more.
        //
        // When Read encounters an error or end-of-file condition after
        // successfully reading n > 0 bytes, it returns the number of
        // bytes read. It may return the (non-nil) error from the same call
        // or return the error (and n == 0) from a subsequent call.
        // An instance of this general case is that a Reader returning
        // a non-zero number of bytes at the end of the input stream may
        // return either err == EOF or err == nil. The next Read should
        // return 0, EOF.
        //
        // Callers should always process the n > 0 bytes returned before
        // considering the error err. Doing so correctly handles I/O errors
        // that happen after reading some bytes and also both of the
        // allowed EOF behaviors.
        //
        // Implementations of Read are discouraged from returning a
        // zero byte count with a nil error, except when len(p) == 0.
        // Callers should treat a return of 0 and nil as indicating that
        // nothing happened; in particular it does not indicate EOF.
        //
        // Implementations must not retain p.
        public partial interface Reader {
            (int n, error err) Read(in slice<byte> p);
        }
    }
}
