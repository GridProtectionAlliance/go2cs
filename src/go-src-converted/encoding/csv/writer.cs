// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package csv -- go2cs converted at 2020 August 29 08:35:18 UTC
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
        // A Writer writes records to a CSV encoded file.
        //
        // As returned by NewWriter, a Writer writes records terminated by a
        // newline and uses ',' as the field delimiter. The exported fields can be
        // changed to customize the details before the first call to Write or WriteAll.
        //
        // Comma is the field delimiter.
        //
        // If UseCRLF is true, the Writer ends each output line with \r\n instead of \n.
        public partial struct Writer
        {
            public int Comma; // Field delimiter (set to ',' by NewWriter)
            public bool UseCRLF; // True to use \r\n as the line terminator
            public ptr<bufio.Writer> w;
        }

        // NewWriter returns a new Writer that writes to w.
        public static ref Writer NewWriter(io.Writer w)
        {
            return ref new Writer(Comma:',',w:bufio.NewWriter(w),);
        }

        // Writer writes a single CSV record to w along with any necessary quoting.
        // A record is a slice of strings with each string being one field.
        private static error Write(this ref Writer w, slice<@string> record)
        {
            if (!validDelim(w.Comma))
            {
                return error.As(errInvalidDelim);
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
                            return error.As(err);
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
                            return error.As(err);
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
                        return error.As(err);
                    }

                    err = err__prev1;

                }

                foreach (var (_, r1) in field)
                {
                    err = default;
                    switch (r1)
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
                        default: 
                            _, err = w.w.WriteRune(r1);
                            break;
                    }
                    if (err != null)
                    {
                        return error.As(err);
                    }
                }
                {
                    var err__prev1 = err;

                    err = w.w.WriteByte('"');

                    if (err != null)
                    {
                        return error.As(err);
                    }

                    err = err__prev1;

                }
            }
            err = default;
            if (w.UseCRLF)
            {
                _, err = w.w.WriteString("\r\n");
            }
            else
            {
                err = w.w.WriteByte('\n');
            }
            return error.As(err);
        }

        // Flush writes any buffered data to the underlying io.Writer.
        // To check if an error occurred during the Flush, call Error.
        private static void Flush(this ref Writer w)
        {
            w.w.Flush();
        }

        // Error reports any error that has occurred during a previous Write or Flush.
        private static error Error(this ref Writer w)
        {
            var (_, err) = w.w.Write(null);
            return error.As(err);
        }

        // WriteAll writes multiple CSV records to w using Write and then calls Flush.
        private static error WriteAll(this ref Writer w, slice<slice<@string>> records)
        {
            foreach (var (_, record) in records)
            {
                var err = w.Write(record);
                if (err != null)
                {
                    return error.As(err);
                }
            }
            return error.As(w.w.Flush());
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
        private static bool fieldNeedsQuotes(this ref Writer w, @string field)
        {
            if (field == "")
            {
                return false;
            }
            if (field == "\\." || strings.ContainsRune(field, w.Comma) || strings.ContainsAny(field, "\"\r\n"))
            {
                return true;
            }
            var (r1, _) = utf8.DecodeRuneInString(field);
            return unicode.IsSpace(r1);
        }
    }
}}
