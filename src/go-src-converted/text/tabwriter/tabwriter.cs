// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package tabwriter implements a write filter (tabwriter.Writer) that
// translates tabbed columns in input into properly aligned text.
//
// The package is using the Elastic Tabstops algorithm described at
// http://nickgravgaard.com/elastictabstops/index.html.
//
// The text/tabwriter package is frozen and is not accepting new features.
// package tabwriter -- go2cs converted at 2020 October 09 04:49:59 UTC
// import "text/tabwriter" ==> using tabwriter = go.text.tabwriter_package
// Original source: C:\Go\src\text\tabwriter\tabwriter.go
using io = go.io_package;
using utf8 = go.unicode.utf8_package;
using static go.builtin;

namespace go {
namespace text
{
    public static partial class tabwriter_package
    {
        // ----------------------------------------------------------------------------
        // Filter implementation

        // A cell represents a segment of text terminated by tabs or line breaks.
        // The text itself is stored in a separate buffer; cell only describes the
        // segment's size in bytes, its width in runes, and whether it's an htab
        // ('\t') terminated cell.
        //
        private partial struct cell
        {
            public long size; // cell size in bytes
            public long width; // cell width in runes
            public bool htab; // true if the cell is terminated by an htab ('\t')
        }

        // A Writer is a filter that inserts padding around tab-delimited
        // columns in its input to align them in the output.
        //
        // The Writer treats incoming bytes as UTF-8-encoded text consisting
        // of cells terminated by horizontal ('\t') or vertical ('\v') tabs,
        // and newline ('\n') or formfeed ('\f') characters; both newline and
        // formfeed act as line breaks.
        //
        // Tab-terminated cells in contiguous lines constitute a column. The
        // Writer inserts padding as needed to make all cells in a column have
        // the same width, effectively aligning the columns. It assumes that
        // all characters have the same width, except for tabs for which a
        // tabwidth must be specified. Column cells must be tab-terminated, not
        // tab-separated: non-tab terminated trailing text at the end of a line
        // forms a cell but that cell is not part of an aligned column.
        // For instance, in this example (where | stands for a horizontal tab):
        //
        //    aaaa|bbb|d
        //    aa  |b  |dd
        //    a   |
        //    aa  |cccc|eee
        //
        // the b and c are in distinct columns (the b column is not contiguous
        // all the way). The d and e are not in a column at all (there's no
        // terminating tab, nor would the column be contiguous).
        //
        // The Writer assumes that all Unicode code points have the same width;
        // this may not be true in some fonts or if the string contains combining
        // characters.
        //
        // If DiscardEmptyColumns is set, empty columns that are terminated
        // entirely by vertical (or "soft") tabs are discarded. Columns
        // terminated by horizontal (or "hard") tabs are not affected by
        // this flag.
        //
        // If a Writer is configured to filter HTML, HTML tags and entities
        // are passed through. The widths of tags and entities are
        // assumed to be zero (tags) and one (entities) for formatting purposes.
        //
        // A segment of text may be escaped by bracketing it with Escape
        // characters. The tabwriter passes escaped text segments through
        // unchanged. In particular, it does not interpret any tabs or line
        // breaks within the segment. If the StripEscape flag is set, the
        // Escape characters are stripped from the output; otherwise they
        // are passed through as well. For the purpose of formatting, the
        // width of the escaped text is always computed excluding the Escape
        // characters.
        //
        // The formfeed character acts like a newline but it also terminates
        // all columns in the current line (effectively calling Flush). Tab-
        // terminated cells in the next line start new columns. Unless found
        // inside an HTML tag or inside an escaped text segment, formfeed
        // characters appear as newlines in the output.
        //
        // The Writer must buffer input internally, because proper spacing
        // of one line may depend on the cells in future lines. Clients must
        // call Flush when done calling Write.
        //
        public partial struct Writer
        {
            public io.Writer output;
            public long minwidth;
            public long tabwidth;
            public long padding;
            public array<byte> padbytes;
            public ulong flags; // current state
            public slice<byte> buf; // collected text excluding tabs or line breaks
            public long pos; // buffer position up to which cell.width of incomplete cell has been computed
            public cell cell; // current incomplete cell; cell.width is up to buf[pos] excluding ignored sections
            public byte endChar; // terminating char of escaped sequence (Escape for escapes, '>', ';' for HTML tags/entities, or 0)
            public slice<slice<cell>> lines; // list of lines; each line is a list of cells
            public slice<long> widths; // list of column widths in runes - re-used during formatting
        }

        // addLine adds a new line.
        // flushed is a hint indicating whether the underlying writer was just flushed.
        // If so, the previous line is not likely to be a good indicator of the new line's cells.
        private static void addLine(this ptr<Writer> _addr_b, bool flushed)
        {
            ref Writer b = ref _addr_b.val;
 
            // Grow slice instead of appending,
            // as that gives us an opportunity
            // to re-use an existing []cell.
            {
                var n__prev1 = n;

                var n = len(b.lines) + 1L;

                if (n <= cap(b.lines))
                {
                    b.lines = b.lines[..n];
                    b.lines[n - 1L] = b.lines[n - 1L][..0L];
                }
                else
                {
                    b.lines = append(b.lines, null);
                }

                n = n__prev1;

            }


            if (!flushed)
            { 
                // The previous line is probably a good indicator
                // of how many cells the current line will have.
                // If the current line's capacity is smaller than that,
                // abandon it and make a new one.
                {
                    var n__prev2 = n;

                    n = len(b.lines);

                    if (n >= 2L)
                    {
                        {
                            var prev = len(b.lines[n - 2L]);

                            if (prev > cap(b.lines[n - 1L]))
                            {
                                b.lines[n - 1L] = make_slice<cell>(0L, prev);
                            }

                        }

                    }

                    n = n__prev2;

                }

            }

        }

        // Reset the current state.
        private static void reset(this ptr<Writer> _addr_b)
        {
            ref Writer b = ref _addr_b.val;

            b.buf = b.buf[..0L];
            b.pos = 0L;
            b.cell = new cell();
            b.endChar = 0L;
            b.lines = b.lines[0L..0L];
            b.widths = b.widths[0L..0L];
            b.addLine(true);
        }

        // Internal representation (current state):
        //
        // - all text written is appended to buf; tabs and line breaks are stripped away
        // - at any given time there is a (possibly empty) incomplete cell at the end
        //   (the cell starts after a tab or line break)
        // - cell.size is the number of bytes belonging to the cell so far
        // - cell.width is text width in runes of that cell from the start of the cell to
        //   position pos; html tags and entities are excluded from this width if html
        //   filtering is enabled
        // - the sizes and widths of processed text are kept in the lines list
        //   which contains a list of cells for each line
        // - the widths list is a temporary list with current widths used during
        //   formatting; it is kept in Writer because it's re-used
        //
        //                    |<---------- size ---------->|
        //                    |                            |
        //                    |<- width ->|<- ignored ->|  |
        //                    |           |             |  |
        // [---processed---tab------------<tag>...</tag>...]
        // ^                  ^                         ^
        // |                  |                         |
        // buf                start of incomplete cell  pos

        // Formatting can be controlled with these flags.
 
        // Ignore html tags and treat entities (starting with '&'
        // and ending in ';') as single characters (width = 1).
        public static readonly ulong FilterHTML = (ulong)1L << (int)(iota); 

        // Strip Escape characters bracketing escaped text segments
        // instead of passing them through unchanged with the text.
        public static readonly var StripEscape = 0; 

        // Force right-alignment of cell content.
        // Default is left-alignment.
        public static readonly var AlignRight = 1; 

        // Handle empty columns as if they were not present in
        // the input in the first place.
        public static readonly var DiscardEmptyColumns = 2; 

        // Always use tabs for indentation columns (i.e., padding of
        // leading empty cells on the left) independent of padchar.
        public static readonly var TabIndent = 3; 

        // Print a vertical bar ('|') between columns (after formatting).
        // Discarded columns appear as zero-width columns ("||").
        public static readonly var Debug = 4;


        // A Writer must be initialized with a call to Init. The first parameter (output)
        // specifies the filter output. The remaining parameters control the formatting:
        //
        //    minwidth    minimal cell width including any padding
        //    tabwidth    width of tab characters (equivalent number of spaces)
        //    padding        padding added to a cell before computing its width
        //    padchar        ASCII char used for padding
        //            if padchar == '\t', the Writer will assume that the
        //            width of a '\t' in the formatted output is tabwidth,
        //            and cells are left-aligned independent of align_left
        //            (for correct-looking results, tabwidth must correspond
        //            to the tab width in the viewer displaying the result)
        //    flags        formatting control
        //
        private static ptr<Writer> Init(this ptr<Writer> _addr_b, io.Writer output, long minwidth, long tabwidth, long padding, byte padchar, ulong flags) => func((_, panic, __) =>
        {
            ref Writer b = ref _addr_b.val;

            if (minwidth < 0L || tabwidth < 0L || padding < 0L)
            {
                panic("negative minwidth, tabwidth, or padding");
            }

            b.output = output;
            b.minwidth = minwidth;
            b.tabwidth = tabwidth;
            b.padding = padding;
            foreach (var (i) in b.padbytes)
            {
                b.padbytes[i] = padchar;
            }
            if (padchar == '\t')
            { 
                // tab padding enforces left-alignment
                flags &= AlignRight;

            }

            b.flags = flags;

            b.reset();

            return _addr_b!;

        });

        // debugging support (keep code around)
        private static void dump(this ptr<Writer> _addr_b)
        {
            ref Writer b = ref _addr_b.val;

            long pos = 0L;
            foreach (var (i, line) in b.lines)
            {
                print("(", i, ") ");
                foreach (var (_, c) in line)
                {
                    print("[", string(b.buf[pos..pos + c.size]), "]");
                    pos += c.size;
                }
                print("\n");

            }
            print("\n");

        }

        // local error wrapper so we can distinguish errors we want to return
        // as errors from genuine panics (which we don't want to return as errors)
        private partial struct osError
        {
            public error err;
        }

        private static void write0(this ptr<Writer> _addr_b, slice<byte> buf) => func((_, panic, __) =>
        {
            ref Writer b = ref _addr_b.val;

            var (n, err) = b.output.Write(buf);
            if (n != len(buf) && err == null)
            {
                err = io.ErrShortWrite;
            }

            if (err != null)
            {
                panic(new osError(err));
            }

        });

        private static void writeN(this ptr<Writer> _addr_b, slice<byte> src, long n)
        {
            ref Writer b = ref _addr_b.val;

            while (n > len(src))
            {
                b.write0(src);
                n -= len(src);
            }

            b.write0(src[0L..n]);

        }

        private static byte newline = new slice<byte>(new byte[] { '\n' });        private static slice<byte> tabs = (slice<byte>)"\t\t\t\t\t\t\t\t";

        private static void writePadding(this ptr<Writer> _addr_b, long textw, long cellw, bool useTabs) => func((_, panic, __) =>
        {
            ref Writer b = ref _addr_b.val;

            if (b.padbytes[0L] == '\t' || useTabs)
            { 
                // padding is done with tabs
                if (b.tabwidth == 0L)
                {
                    return ; // tabs have no width - can't do any padding
                } 
                // make cellw the smallest multiple of b.tabwidth
                cellw = (cellw + b.tabwidth - 1L) / b.tabwidth * b.tabwidth;
                var n = cellw - textw; // amount of padding
                if (n < 0L)
                {
                    panic("internal error");
                }

                b.writeN(tabs, (n + b.tabwidth - 1L) / b.tabwidth);
                return ;

            } 

            // padding is done with non-tab characters
            b.writeN(b.padbytes[0L..], cellw - textw);

        });

        private static byte vbar = new slice<byte>(new byte[] { '|' });

        private static long writeLines(this ptr<Writer> _addr_b, long pos0, long line0, long line1)
        {
            long pos = default;
            ref Writer b = ref _addr_b.val;

            pos = pos0;
            for (var i = line0; i < line1; i++)
            {
                var line = b.lines[i]; 

                // if TabIndent is set, use tabs to pad leading empty cells
                var useTabs = b.flags & TabIndent != 0L;

                foreach (var (j, c) in line)
                {
                    if (j > 0L && b.flags & Debug != 0L)
                    { 
                        // indicate column break
                        b.write0(vbar);

                    }

                    if (c.size == 0L)
                    { 
                        // empty cell
                        if (j < len(b.widths))
                        {
                            b.writePadding(c.width, b.widths[j], useTabs);
                        }

                    }
                    else
                    { 
                        // non-empty cell
                        useTabs = false;
                        if (b.flags & AlignRight == 0L)
                        { // align left
                            b.write0(b.buf[pos..pos + c.size]);
                            pos += c.size;
                            if (j < len(b.widths))
                            {
                                b.writePadding(c.width, b.widths[j], false);
                            }

                        }
                        else
                        { // align right
                            if (j < len(b.widths))
                            {
                                b.writePadding(c.width, b.widths[j], false);
                            }

                            b.write0(b.buf[pos..pos + c.size]);
                            pos += c.size;

                        }

                    }

                }
                if (i + 1L == len(b.lines))
                { 
                    // last buffered line - we don't have a newline, so just write
                    // any outstanding buffered data
                    b.write0(b.buf[pos..pos + b.cell.size]);
                    pos += b.cell.size;

                }
                else
                { 
                    // not the last line - write newline
                    b.write0(newline);

                }

            }

            return ;

        }

        // Format the text between line0 and line1 (excluding line1); pos
        // is the buffer position corresponding to the beginning of line0.
        // Returns the buffer position corresponding to the beginning of
        // line1 and an error, if any.
        //
        private static long format(this ptr<Writer> _addr_b, long pos0, long line0, long line1)
        {
            long pos = default;
            ref Writer b = ref _addr_b.val;

            pos = pos0;
            var column = len(b.widths);
            for (var @this = line0; this < line1; this++)
            {
                var line = b.lines[this];

                if (column >= len(line) - 1L)
                {
                    continue;
                } 
                // cell exists in this column => this line
                // has more cells than the previous line
                // (the last cell per line is ignored because cells are
                // tab-terminated; the last cell per line describes the
                // text before the newline/formfeed and does not belong
                // to a column)

                // print unprinted lines until beginning of block
                pos = b.writeLines(pos, line0, this);
                line0 = this; 

                // column block begin
                var width = b.minwidth; // minimal column width
                var discardable = true; // true if all cells in this column are empty and "soft"
                while (this < line1)
                {
                    line = b.lines[this];
                    if (column >= len(line) - 1L)
                    {
                        break;
                    this++;
                    } 
                    // cell exists in this column
                    var c = line[column]; 
                    // update width
                    {
                        var w = c.width + b.padding;

                        if (w > width)
                        {
                            width = w;
                        } 
                        // update discardable

                    } 
                    // update discardable
                    if (c.width > 0L || c.htab)
                    {
                        discardable = false;
                    }

                } 
                // column block end

                // discard empty columns if necessary
 
                // column block end

                // discard empty columns if necessary
                if (discardable && b.flags & DiscardEmptyColumns != 0L)
                {
                    width = 0L;
                } 

                // format and print all columns to the right of this column
                // (we know the widths of this column and all columns to the left)
                b.widths = append(b.widths, width); // push width
                pos = b.format(pos, line0, this);
                b.widths = b.widths[0L..len(b.widths) - 1L]; // pop width
                line0 = this;

            } 

            // print unprinted lines until end
 

            // print unprinted lines until end
            return b.writeLines(pos, line0, line1);

        }

        // Append text to current cell.
        private static void append(this ptr<Writer> _addr_b, slice<byte> text)
        {
            ref Writer b = ref _addr_b.val;

            b.buf = append(b.buf, text);
            b.cell.size += len(text);
        }

        // Update the cell width.
        private static void updateWidth(this ptr<Writer> _addr_b)
        {
            ref Writer b = ref _addr_b.val;

            b.cell.width += utf8.RuneCount(b.buf[b.pos..]);
            b.pos = len(b.buf);
        }

        // To escape a text segment, bracket it with Escape characters.
        // For instance, the tab in this string "Ignore this tab: \xff\t\xff"
        // does not terminate a cell and constitutes a single character of
        // width one for formatting purposes.
        //
        // The value 0xff was chosen because it cannot appear in a valid UTF-8 sequence.
        //
        public static readonly char Escape = (char)'\xff';

        // Start escaped mode.


        // Start escaped mode.
        private static void startEscape(this ptr<Writer> _addr_b, byte ch)
        {
            ref Writer b = ref _addr_b.val;


            if (ch == Escape) 
                b.endChar = Escape;
            else if (ch == '<') 
                b.endChar = '>';
            else if (ch == '&') 
                b.endChar = ';';
            
        }

        // Terminate escaped mode. If the escaped text was an HTML tag, its width
        // is assumed to be zero for formatting purposes; if it was an HTML entity,
        // its width is assumed to be one. In all other cases, the width is the
        // unicode width of the text.
        //
        private static void endEscape(this ptr<Writer> _addr_b)
        {
            ref Writer b = ref _addr_b.val;


            if (b.endChar == Escape) 
                b.updateWidth();
                if (b.flags & StripEscape == 0L)
                {
                    b.cell.width -= 2L; // don't count the Escape chars
                }

            else if (b.endChar == '>')             else if (b.endChar == ';') 
                b.cell.width++; // entity, count as one rune
                        b.pos = len(b.buf);
            b.endChar = 0L;

        }

        // Terminate the current cell by adding it to the list of cells of the
        // current line. Returns the number of cells in that line.
        //
        private static long terminateCell(this ptr<Writer> _addr_b, bool htab)
        {
            ref Writer b = ref _addr_b.val;

            b.cell.htab = htab;
            var line = _addr_b.lines[len(b.lines) - 1L];
            line.val = append(line.val, b.cell);
            b.cell = new cell();
            return len(line.val);
        }

        private static void handlePanic(this ptr<Writer> _addr_b, ptr<error> _addr_err, @string op) => func((_, panic, __) =>
        {
            ref Writer b = ref _addr_b.val;
            ref error err = ref _addr_err.val;

            {
                var e = recover();

                if (e != null)
                {
                    if (op == "Flush")
                    { 
                        // If Flush ran into a panic, we still need to reset.
                        b.reset();

                    }

                    {
                        osError (nerr, ok) = e._<osError>();

                        if (ok)
                        {
                            err = error.As(nerr.err)!;
                            return ;
                        }

                    }

                    panic("tabwriter: panic during " + op);

                }

            }

        });

        // Flush should be called after the last call to Write to ensure
        // that any data buffered in the Writer is written to output. Any
        // incomplete escape sequence at the end is considered
        // complete for formatting purposes.
        private static error Flush(this ptr<Writer> _addr_b)
        {
            ref Writer b = ref _addr_b.val;

            return error.As(b.flush())!;
        }

        // flush is the internal version of Flush, with a named return value which we
        // don't want to expose.
        private static error flush(this ptr<Writer> _addr_b) => func((defer, _, __) =>
        {
            error err = default!;
            ref Writer b = ref _addr_b.val;

            defer(b.handlePanic(_addr_err, "Flush"));
            b.flushNoDefers();
            return error.As(null!)!;
        });

        // flushNoDefers is like flush, but without a deferred handlePanic call. This
        // can be called from other methods which already have their own deferred
        // handlePanic calls, such as Write, and avoid the extra defer work.
        private static void flushNoDefers(this ptr<Writer> _addr_b)
        {
            ref Writer b = ref _addr_b.val;
 
            // add current cell if not empty
            if (b.cell.size > 0L)
            {
                if (b.endChar != 0L)
                { 
                    // inside escape - terminate it even if incomplete
                    b.endEscape();

                }

                b.terminateCell(false);

            } 

            // format contents of buffer
            b.format(0L, 0L, len(b.lines));
            b.reset();

        }

        private static slice<byte> hbar = (slice<byte>)"---\n";

        // Write writes buf to the writer b.
        // The only errors returned are ones encountered
        // while writing to the underlying output stream.
        //
        private static (long, error) Write(this ptr<Writer> _addr_b, slice<byte> buf) => func((defer, _, __) =>
        {
            long n = default;
            error err = default!;
            ref Writer b = ref _addr_b.val;

            defer(b.handlePanic(_addr_err, "Write")); 

            // split text into cells
            n = 0L;
            foreach (var (i, ch) in buf)
            {
                if (b.endChar == 0L)
                { 
                    // outside escape

                    if (ch == '\t' || ch == '\v' || ch == '\n' || ch == '\f') 
                        // end of cell
                        b.append(buf[n..i]);
                        b.updateWidth();
                        n = i + 1L; // ch consumed
                        var ncells = b.terminateCell(ch == '\t');
                        if (ch == '\n' || ch == '\f')
                        { 
                            // terminate line
                            b.addLine(ch == '\f');
                            if (ch == '\f' || ncells == 1L)
                            { 
                                // A '\f' always forces a flush. Otherwise, if the previous
                                // line has only one cell which does not have an impact on
                                // the formatting of the following lines (the last cell per
                                // line is ignored by format()), thus we can flush the
                                // Writer contents.
                                b.flushNoDefers();
                                if (ch == '\f' && b.flags & Debug != 0L)
                                { 
                                    // indicate section break
                                    b.write0(hbar);

                                }

                            }

                        }

                    else if (ch == Escape) 
                        // start of escaped sequence
                        b.append(buf[n..i]);
                        b.updateWidth();
                        n = i;
                        if (b.flags & StripEscape != 0L)
                        {
                            n++; // strip Escape
                        }

                        b.startEscape(Escape);
                    else if (ch == '<' || ch == '&') 
                        // possibly an html tag/entity
                        if (b.flags & FilterHTML != 0L)
                        { 
                            // begin of tag/entity
                            b.append(buf[n..i]);
                            b.updateWidth();
                            n = i;
                            b.startEscape(ch);

                        }

                                    }
                else
                { 
                    // inside escape
                    if (ch == b.endChar)
                    { 
                        // end of tag/entity
                        var j = i + 1L;
                        if (ch == Escape && b.flags & StripEscape != 0L)
                        {
                            j = i; // strip Escape
                        }

                        b.append(buf[n..j]);
                        n = i + 1L; // ch consumed
                        b.endEscape();

                    }

                }

            } 

            // append leftover text
            b.append(buf[n..]);
            n = len(buf);
            return ;

        });

        // NewWriter allocates and initializes a new tabwriter.Writer.
        // The parameters are the same as for the Init function.
        //
        public static ptr<Writer> NewWriter(io.Writer output, long minwidth, long tabwidth, long padding, byte padchar, ulong flags)
        {
            return @new<Writer>().Init(output, minwidth, tabwidth, padding, padchar, flags);
        }
    }
}}
