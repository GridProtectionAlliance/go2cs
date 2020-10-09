// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package csv -- go2cs converted at 2020 October 09 04:59:46 UTC
// import "encoding/csv" ==> using csv = go.encoding.csv_package
// Original source: C:\Go\src\encoding\csv\writer.go
using bufio = go.bufio_package;
using io = go.io_package;
using strings = go.strings_package;
using unicode = go.unicode_package;
using utf8 = go.unicode.utf8_package;
using static go.builtin;

namespace go {
namespace encoding
{
    public static partial class csv_package
    {
        // A Writer writes records using CSV encoding.
        //
        // As returned by NewWriter, a Writer writes records terminated by a
        // newline and uses ',' as the field delimiter. The exported fields can be
        // changed to customize the details before the first call to Write or WriteAll.
        //
        // Comma is the field delimiter.
        //
        // If UseCRLF is true, the Writer ends each output line with \r\n instead of \n.
        //
        // The writes of individual records are buffered.
        // After all data has been written, the client should call the
        // Flush method to guarantee all data has been forwarded to
        // the underlying io.Writer.  Any errors that occurred should
        // be checked by calling the Error method.
        public partial struct Writer
        {
            public int Comma; // Field delimiter (set to ',' by NewWriter)
            public bool UseCRLF; // True to use \r\n as the line terminator
            public ptr<bufio.Writer> w;
        }

        // NewWriter returns a new Writer that writes to w.
        public static ptr<Writer> NewWriter(io.Writer w)
        {
            return addr(new Writer(Comma:',',w:bufio.NewWriter(w),));
        }

        // Write writes a single CSV record to w along with any necessary quoting.
        // A record is a slice of strings with each string being one field.
        // Writes are buffered, so Flush must eventually be called to ensure
        // that the record is written to the underlying io.Writer.
        private static error Write(this ptr<Writer> _addr_w, slice<@string> record)
        {
            ref Writer w = ref _addr_w.val;

            if (!validDelim(w.Comma))
            {
                return error.As(errInvalidDelim)!;
            }

            foreach (var (n, field) in record)
            {
                if (n > 0L)
                {
                    {
                        var err__prev2 = err;

                        var (_, err) = w.w.WriteRune(w.Comma);

                        if (err != null)
                        {
                            return error.As(err)!;
                        }

                        err = err__prev2;

                    }

                } 

                // If we don't have to have a quoted field then just
                // write out the field and continue to the next field.
                if (!w.fieldNeedsQuotes(field))
                {
                    {
                        var err__prev2 = err;

                        (_, err) = w.w.WriteString(field);

                        if (err != null)
                        {
                            return error.As(err)!;
                        }

                        err = err__prev2;

                    }

                    continue;

                }

                {
                    var err__prev1 = err;

                    var err = w.w.WriteByte('"');

                    if (err != null)
                    {
                        return error.As(err)!;
                    }

                    err = err__prev1;

                }

                while (len(field) > 0L)
                { 
                    // Search for special characters.
                    var i = strings.IndexAny(field, "\"\r\n");
                    if (i < 0L)
                    {
                        i = len(field);
                    } 

                    // Copy verbatim everything before the special character.
                    {
                        var err__prev1 = err;

                        (_, err) = w.w.WriteString(field[..i]);

                        if (err != null)
                        {
                            return error.As(err)!;
                        }

                        err = err__prev1;

                    }

                    field = field[i..]; 

                    // Encode the special character.
                    if (len(field) > 0L)
                    {
                        err = default!;
                        switch (field[0L])
                        {
                            case '"': 
                                _, err = w.w.WriteString("\"\"");
                                break;
                            case '\r': 
                                if (!w.UseCRLF)
                                {
                                    err = w.w.WriteByte('\r');
                                }

                                break;
                            case '\n': 
                                if (w.UseCRLF)
                                {
                                    _, err = w.w.WriteString("\r\n");
                                }
                                else
                                {
                                    err = w.w.WriteByte('\n');
                                }

                                break;
                        }
                        field = field[1L..];
                        if (err != null)
                        {
                            return error.As(err)!;
                        }

                    }

                }

                {
                    var err__prev1 = err;

                    err = w.w.WriteByte('"');

                    if (err != null)
                    {
                        return error.As(err)!;
                    }

                    err = err__prev1;

                }

            }
            err = default!;
            if (w.UseCRLF)
            {
                _, err = w.w.WriteString("\r\n");
            }
            else
            {
                err = w.w.WriteByte('\n');
            }

            return error.As(err)!;

        }

        // Flush writes any buffered data to the underlying io.Writer.
        // To check if an error occurred during the Flush, call Error.
        private static void Flush(this ptr<Writer> _addr_w)
        {
            ref Writer w = ref _addr_w.val;

            w.w.Flush();
        }

        // Error reports any error that has occurred during a previous Write or Flush.
        private static error Error(this ptr<Writer> _addr_w)
        {
            ref Writer w = ref _addr_w.val;

            var (_, err) = w.w.Write(null);
            return error.As(err)!;
        }

        // WriteAll writes multiple CSV records to w using Write and then calls Flush,
        // returning any error from the Flush.
        private static error WriteAll(this ptr<Writer> _addr_w, slice<slice<@string>> records)
        {
            ref Writer w = ref _addr_w.val;

            foreach (var (_, record) in records)
            {
                var err = w.Write(record);
                if (err != null)
                {
                    return error.As(err)!;
                }

            }
            return error.As(w.w.Flush())!;

        }

        // fieldNeedsQuotes reports whether our field must be enclosed in quotes.
        // Fields with a Comma, fields with a quote or newline, and
        // fields which start with a space must be enclosed in quotes.
        // We used to quote empty strings, but we do not anymore (as of Go 1.4).
        // The two representations should be equivalent, but Postgres distinguishes
        // quoted vs non-quoted empty string during database imports, and it has
        // an option to force the quoted behavior for non-quoted CSV but it has
        // no option to force the non-quoted behavior for quoted CSV, making
        // CSV with quoted empty strings strictly less useful.
        // Not quoting the empty string also makes this package match the behavior
        // of Microsoft Excel and Google Drive.
        // For Postgres, quote the data terminating string `\.`.
        private static bool fieldNeedsQuotes(this ptr<Writer> _addr_w, @string field)
        {
            ref Writer w = ref _addr_w.val;

            if (field == "")
            {
                return false;
            }

            if (field == "\\.")
            {
                return true;
            }

            if (w.Comma < utf8.RuneSelf)
            {
                for (long i = 0L; i < len(field); i++)
                {
                    var c = field[i];
                    if (c == '\n' || c == '\r' || c == '"' || c == byte(w.Comma))
                    {
                        return true;
                    }

                }
            else


            }            {
                if (strings.ContainsRune(field, w.Comma) || strings.ContainsAny(field, "\"\r\n"))
                {
                    return true;
                }

            }

            var (r1, _) = utf8.DecodeRuneInString(field);
            return unicode.IsSpace(r1);

        }
    }
}}
