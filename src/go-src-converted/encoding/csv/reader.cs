// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package csv reads and writes comma-separated values (CSV) files.
// There are many kinds of CSV files; this package supports the format
// described in RFC 4180.
//
// A csv file contains zero or more records of one or more fields per record.
// Each record is separated by the newline character. The final record may
// optionally be followed by a newline character.
//
//    field1,field2,field3
//
// White space is considered part of a field.
//
// Carriage returns before newline characters are silently removed.
//
// Blank lines are ignored. A line with only whitespace characters (excluding
// the ending newline character) is not considered a blank line.
//
// Fields which start and stop with the quote character " are called
// quoted-fields. The beginning and ending quote are not part of the
// field.
//
// The source:
//
//    normal string,"quoted-field"
//
// results in the fields
//
//    {`normal string`, `quoted-field`}
//
// Within a quoted-field a quote character followed by a second quote
// character is considered a single quote.
//
//    "the ""word"" is true","a ""quoted-field"""
//
// results in
//
//    {`the "word" is true`, `a "quoted-field"`}
//
// Newlines and commas may be included in a quoted-field
//
//    "Multi-line
//    field","comma is ,"
//
// results in
//
//    {`Multi-line
//    field`, `comma is ,`}
// package csv -- go2cs converted at 2020 October 09 04:59:45 UTC
// import "encoding/csv" ==> using csv = go.encoding.csv_package
// Original source: C:\Go\src\encoding\csv\reader.go
using bufio = go.bufio_package;
using bytes = go.bytes_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using io = go.io_package;
using unicode = go.unicode_package;
using utf8 = go.unicode.utf8_package;
using static go.builtin;

namespace go {
namespace encoding
{
    public static partial class csv_package
    {
        // A ParseError is returned for parsing errors.
        // Line numbers are 1-indexed and columns are 0-indexed.
        public partial struct ParseError
        {
            public long StartLine; // Line where the record starts
            public long Line; // Line where the error occurred
            public long Column; // Column (rune index) where the error occurred
            public error Err; // The actual error
        }

        private static @string Error(this ptr<ParseError> _addr_e)
        {
            ref ParseError e = ref _addr_e.val;

            if (e.Err == ErrFieldCount)
            {
                return fmt.Sprintf("record on line %d: %v", e.Line, e.Err);
            }

            if (e.StartLine != e.Line)
            {
                return fmt.Sprintf("record on line %d; parse error on line %d, column %d: %v", e.StartLine, e.Line, e.Column, e.Err);
            }

            return fmt.Sprintf("parse error on line %d, column %d: %v", e.Line, e.Column, e.Err);

        }

        private static error Unwrap(this ptr<ParseError> _addr_e)
        {
            ref ParseError e = ref _addr_e.val;

            return error.As(e.Err)!;
        }

        // These are the errors that can be returned in ParseError.Err.
        public static var ErrTrailingComma = errors.New("extra delimiter at end of line");        public static var ErrBareQuote = errors.New("bare \" in non-quoted-field");        public static var ErrQuote = errors.New("extraneous or missing \" in quoted-field");        public static var ErrFieldCount = errors.New("wrong number of fields");

        private static var errInvalidDelim = errors.New("csv: invalid field or comment delimiter");

        private static bool validDelim(int r)
        {
            return r != 0L && r != '"' && r != '\r' && r != '\n' && utf8.ValidRune(r) && r != utf8.RuneError;
        }

        // A Reader reads records from a CSV-encoded file.
        //
        // As returned by NewReader, a Reader expects input conforming to RFC 4180.
        // The exported fields can be changed to customize the details before the
        // first call to Read or ReadAll.
        //
        // The Reader converts all \r\n sequences in its input to plain \n,
        // including in multiline field values, so that the returned data does
        // not depend on which line-ending convention an input file uses.
        public partial struct Reader
        {
            public int Comma; // Comment, if not 0, is the comment character. Lines beginning with the
// Comment character without preceding whitespace are ignored.
// With leading whitespace the Comment character becomes part of the
// field, even if TrimLeadingSpace is true.
// Comment must be a valid rune and must not be \r, \n,
// or the Unicode replacement character (0xFFFD).
// It must also not be equal to Comma.
            public int Comment; // FieldsPerRecord is the number of expected fields per record.
// If FieldsPerRecord is positive, Read requires each record to
// have the given number of fields. If FieldsPerRecord is 0, Read sets it to
// the number of fields in the first record, so that future records must
// have the same field count. If FieldsPerRecord is negative, no check is
// made and records may have a variable number of fields.
            public long FieldsPerRecord; // If LazyQuotes is true, a quote may appear in an unquoted field and a
// non-doubled quote may appear in a quoted field.
            public bool LazyQuotes; // If TrimLeadingSpace is true, leading white space in a field is ignored.
// This is done even if the field delimiter, Comma, is white space.
            public bool TrimLeadingSpace; // ReuseRecord controls whether calls to Read may return a slice sharing
// the backing array of the previous call's returned slice for performance.
// By default, each call to Read returns newly allocated memory owned by the caller.
            public bool ReuseRecord;
            public bool TrailingComma; // Deprecated: No longer used.

            public ptr<bufio.Reader> r; // numLine is the current line being read in the CSV file.
            public long numLine; // rawBuffer is a line buffer only used by the readLine method.
            public slice<byte> rawBuffer; // recordBuffer holds the unescaped fields, one after another.
// The fields can be accessed by using the indexes in fieldIndexes.
// E.g., For the row `a,"b","c""d",e`, recordBuffer will contain `abc"de`
// and fieldIndexes will contain the indexes [1, 2, 5, 6].
            public slice<byte> recordBuffer; // fieldIndexes is an index of fields inside recordBuffer.
// The i'th field ends at offset fieldIndexes[i] in recordBuffer.
            public slice<long> fieldIndexes; // lastRecord is a record cache and only used when ReuseRecord == true.
            public slice<@string> lastRecord;
        }

        // NewReader returns a new Reader that reads from r.
        public static ptr<Reader> NewReader(io.Reader r)
        {
            return addr(new Reader(Comma:',',r:bufio.NewReader(r),));
        }

        // Read reads one record (a slice of fields) from r.
        // If the record has an unexpected number of fields,
        // Read returns the record along with the error ErrFieldCount.
        // Except for that case, Read always returns either a non-nil
        // record or a non-nil error, but not both.
        // If there is no data left to be read, Read returns nil, io.EOF.
        // If ReuseRecord is true, the returned slice may be shared
        // between multiple calls to Read.
        private static (slice<@string>, error) Read(this ptr<Reader> _addr_r)
        {
            slice<@string> record = default;
            error err = default!;
            ref Reader r = ref _addr_r.val;

            if (r.ReuseRecord)
            {
                record, err = r.readRecord(r.lastRecord);
                r.lastRecord = record;
            }
            else
            {
                record, err = r.readRecord(null);
            }

            return (record, error.As(err)!);

        }

        // ReadAll reads all the remaining records from r.
        // Each record is a slice of fields.
        // A successful call returns err == nil, not err == io.EOF. Because ReadAll is
        // defined to read until EOF, it does not treat end of file as an error to be
        // reported.
        private static (slice<slice<@string>>, error) ReadAll(this ptr<Reader> _addr_r)
        {
            slice<slice<@string>> records = default;
            error err = default!;
            ref Reader r = ref _addr_r.val;

            while (true)
            {
                var (record, err) = r.readRecord(null);
                if (err == io.EOF)
                {
                    return (records, error.As(null!)!);
                }

                if (err != null)
                {
                    return (null, error.As(err)!);
                }

                records = append(records, record);

            }


        }

        // readLine reads the next line (with the trailing endline).
        // If EOF is hit without a trailing endline, it will be omitted.
        // If some bytes were read, then the error is never io.EOF.
        // The result is only valid until the next call to readLine.
        private static (slice<byte>, error) readLine(this ptr<Reader> _addr_r)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref Reader r = ref _addr_r.val;

            var (line, err) = r.r.ReadSlice('\n');
            if (err == bufio.ErrBufferFull)
            {
                r.rawBuffer = append(r.rawBuffer[..0L], line);
                while (err == bufio.ErrBufferFull)
                {
                    line, err = r.r.ReadSlice('\n');
                    r.rawBuffer = append(r.rawBuffer, line);
                }

                line = r.rawBuffer;

            }

            if (len(line) > 0L && err == io.EOF)
            {
                err = null; 
                // For backwards compatibility, drop trailing \r before EOF.
                if (line[len(line) - 1L] == '\r')
                {
                    line = line[..len(line) - 1L];
                }

            }

            r.numLine++; 
            // Normalize \r\n to \n on all input lines.
            {
                var n = len(line);

                if (n >= 2L && line[n - 2L] == '\r' && line[n - 1L] == '\n')
                {
                    line[n - 2L] = '\n';
                    line = line[..n - 1L];
                }

            }

            return (line, error.As(err)!);

        }

        // lengthNL reports the number of bytes for the trailing \n.
        private static long lengthNL(slice<byte> b)
        {
            if (len(b) > 0L && b[len(b) - 1L] == '\n')
            {
                return 1L;
            }

            return 0L;

        }

        // nextRune returns the next rune in b or utf8.RuneError.
        private static int nextRune(slice<byte> b)
        {
            var (r, _) = utf8.DecodeRune(b);
            return r;
        }

        private static (slice<@string>, error) readRecord(this ptr<Reader> _addr_r, slice<@string> dst)
        {
            slice<@string> _p0 = default;
            error _p0 = default!;
            ref Reader r = ref _addr_r.val;

            if (r.Comma == r.Comment || !validDelim(r.Comma) || (r.Comment != 0L && !validDelim(r.Comment)))
            {
                return (null, error.As(errInvalidDelim)!);
            } 

            // Read line (automatically skipping past empty lines and any comments).
            slice<byte> line = default;            slice<byte> fullLine = default;

            error errRead = default!;
            while (errRead == null)
            {
                line, errRead = r.readLine();
                if (r.Comment != 0L && nextRune(line) == r.Comment)
                {
                    line = null;
                    continue; // Skip comment lines
                }

                if (errRead == null && len(line) == lengthNL(line))
                {
                    line = null;
                    continue; // Skip empty lines
                }

                fullLine = line;
                break;

            }

            if (errRead == io.EOF)
            {
                return (null, error.As(errRead)!);
            } 

            // Parse each field in the record.
            error err = default!;
            const var quoteLen = len("\"");

            var commaLen = utf8.RuneLen(r.Comma);
            var recLine = r.numLine; // Starting line for record
            r.recordBuffer = r.recordBuffer[..0L];
            r.fieldIndexes = r.fieldIndexes[..0L];
parseField:
            while (true)
            {
                if (r.TrimLeadingSpace)
                {
                    line = bytes.TrimLeftFunc(line, unicode.IsSpace);
                }

                if (len(line) == 0L || line[0L] != '"')
                { 
                    // Non-quoted string field
                    var i = bytes.IndexRune(line, r.Comma);
                    var field = line;
                    if (i >= 0L)
                    {
                        field = field[..i];
                    }
                    else
                    {
                        field = field[..len(field) - lengthNL(field)];
                    } 
                    // Check to make sure a quote does not appear in field.
                    if (!r.LazyQuotes)
                    {
                        {
                            var j = bytes.IndexByte(field, '"');

                            if (j >= 0L)
                            {
                                var col = utf8.RuneCount(fullLine[..len(fullLine) - len(line[j..])]);
                                err = error.As(addr(new ParseError(StartLine:recLine,Line:r.numLine,Column:col,Err:ErrBareQuote)))!;
                                _breakparseField = true;
                                break;
                            }

                        }

                    }

                    r.recordBuffer = append(r.recordBuffer, field);
                    r.fieldIndexes = append(r.fieldIndexes, len(r.recordBuffer));
                    if (i >= 0L)
                    {
                        line = line[i + commaLen..];
                        _continueparseField = true;
                        break;
                    }

                    _breakparseField = true;
                    break;
                }
                else
                { 
                    // Quoted string field
                    line = line[quoteLen..];
                    while (true)
                    {
                        i = bytes.IndexByte(line, '"');
                        if (i >= 0L)
                        { 
                            // Hit next quote.
                            r.recordBuffer = append(r.recordBuffer, line[..i]);
                            line = line[i + quoteLen..];
                            {
                                var rn = nextRune(line);


                                if (rn == '"') 
                                    // `""` sequence (append quote).
                                    r.recordBuffer = append(r.recordBuffer, '"');
                                    line = line[quoteLen..];
                                else if (rn == r.Comma) 
                                    // `",` sequence (end of field).
                                    line = line[commaLen..];
                                    r.fieldIndexes = append(r.fieldIndexes, len(r.recordBuffer));
                                    _continueparseField = true;
                                    break;
                                else if (lengthNL(line) == len(line)) 
                                    // `"\n` sequence (end of line).
                                    r.fieldIndexes = append(r.fieldIndexes, len(r.recordBuffer));
                                    _breakparseField = true;
                                    break;
                                else if (r.LazyQuotes) 
                                    // `"` sequence (bare quote).
                                    r.recordBuffer = append(r.recordBuffer, '"');
                                else 
                                    // `"*` sequence (invalid non-escaped quote).
                                    col = utf8.RuneCount(fullLine[..len(fullLine) - len(line) - quoteLen]);
                                    err = error.As(addr(new ParseError(StartLine:recLine,Line:r.numLine,Column:col,Err:ErrQuote)))!;
                                    _breakparseField = true;
                                    break;

                            }

                        }
                        else if (len(line) > 0L)
                        { 
                            // Hit end of line (copy all data so far).
                            r.recordBuffer = append(r.recordBuffer, line);
                            if (errRead != null)
                            {
                                _breakparseField = true;
                                break;
                            }

                            line, errRead = r.readLine();
                            if (errRead == io.EOF)
                            {
                                errRead = error.As(null)!;
                            }

                            fullLine = line;

                        }
                        else
                        { 
                            // Abrupt end of file (EOF or error).
                            if (!r.LazyQuotes && errRead == null)
                            {
                                col = utf8.RuneCount(fullLine);
                                err = error.As(addr(new ParseError(StartLine:recLine,Line:r.numLine,Column:col,Err:ErrQuote)))!;
                                _breakparseField = true;
                                break;
                            }

                            r.fieldIndexes = append(r.fieldIndexes, len(r.recordBuffer));
                            _breakparseField = true;
                            break;
                        }

                    }


                }

            }
            if (err == null)
            {
                err = error.As(errRead)!;
            } 

            // Create a single string and create slices out of it.
            // This pins the memory of the fields together, but allocates once.
            var str = string(r.recordBuffer); // Convert to string once to batch allocations
            dst = dst[..0L];
            if (cap(dst) < len(r.fieldIndexes))
            {
                dst = make_slice<@string>(len(r.fieldIndexes));
            }

            dst = dst[..len(r.fieldIndexes)];
            long preIdx = default;
            {
                var i__prev1 = i;

                foreach (var (__i, __idx) in r.fieldIndexes)
                {
                    i = __i;
                    idx = __idx;
                    dst[i] = str[preIdx..idx];
                    preIdx = idx;
                } 

                // Check or update the expected fields per record.

                i = i__prev1;
            }

            if (r.FieldsPerRecord > 0L)
            {
                if (len(dst) != r.FieldsPerRecord && err == null)
                {
                    err = error.As(addr(new ParseError(StartLine:recLine,Line:recLine,Err:ErrFieldCount)))!;
                }

            }
            else if (r.FieldsPerRecord == 0L)
            {
                r.FieldsPerRecord = len(dst);
            }

            return (dst, error.As(err)!);

        }
    }
}}
