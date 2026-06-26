// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go.doc;

using bytes = bytes_package;
using fmt = fmt_package;
using sort = sort_package;
using strings = strings_package;
using utf8 = unicode.utf8_package;
using unicode;

partial class comment_package {

// A textPrinter holds the state needed for printing a Doc as plain text.
[GoType] partial struct textPrinter {
    public partial ref ж<Printer> Printer { get; }
    internal strings_package.Builder @long;
    internal @string prefix;
    internal @string codePrefix;
    internal nint width;
}

// Text returns a textual formatting of the [Doc].
// See the [Printer] documentation for ways to customize the text output.
[GoRecv] public static slice<byte> Text(this ref Printer p, ж<Doc> Ꮡd) {
    ref var d = ref Ꮡd.val;

    var tp = Ꮡ(new textPrinter(
        Printer: p,
        prefix: p.TextPrefix,
        codePrefix: p.TextCodePrefix,
        width: p.TextWidth
    ));
    if ((~tp).codePrefix == ""u8) {
        tp.val.codePrefix = p.TextPrefix + "\t"u8;
    }
    if ((~tp).width == 0) {
        tp.val.width = 80 - utf8.RuneCountInString((~tp).prefix);
    }
    ref var out = ref heap(new bytes_package.Buffer(), out var Ꮡout);
    foreach (var (i, x) in d.Content) {
        if (i > 0 && blankBefore(x)) {
            @out.WriteString((~tp).prefix);
            writeNL(Ꮡ@out);
        }
        tp.block(Ꮡ@out, x);
    }
    var anyUsed = false;
    foreach (var (_, def) in d.Links) {
        if ((~def).Used) {
            anyUsed = true;
            break;
        }
    }
    if (anyUsed) {
        writeNL(Ꮡ@out);
        foreach (var (_, def) in d.Links) {
            if ((~def).Used) {
                fmt.Fprintf(~Ꮡ@out, "[%s]: %s\n"u8, (~def).Text, (~def).URL);
            }
        }
    }
    return @out.Bytes();
}

// writeNL calls out.WriteByte('\n')
// but first trims trailing spaces on the previous line.
internal static void writeNL(ж<bytes.Buffer> Ꮡout) {
    ref var @out = ref Ꮡout.val;

    // Trim trailing spaces.
    var data = @out.Bytes();
    nint n = 0;
    while (n < len(data) && (data[len(data) - n - 1] == (rune)' ' || data[len(data) - n - 1] == (rune)'\t')) {
        n++;
    }
    if (n > 0) {
        @out.Truncate(len(data) - n);
    }
    @out.WriteByte((rune)'\n');
}

// block prints the block x to out.
[GoRecv] internal static void block(this ref textPrinter p, ж<bytes.Buffer> Ꮡout, Block x) {
    ref var @out = ref Ꮡout.val;

    switch (x.type()) {
    default: {
        var x = x.type();
        fmt.Fprintf(~@out, "?%T\n"u8, x);
        break;
    }
    case Paragraph.val x: {
        @out.WriteString(p.prefix);
        p.text(Ꮡout, ""u8, (~x).Text);
        break;
    }
    case Heading.val x: {
        @out.WriteString(p.prefix);
        @out.WriteString("# "u8);
        p.text(Ꮡout, ""u8, (~x).Text);
        break;
    }
    case Code.val x: {
        @string text = x.val.Text;
        while (text != ""u8) {
            @string line = default!;
            (line, text, _) = strings.Cut(text, "\n"u8);
            if (line != ""u8) {
                @out.WriteString(p.codePrefix);
                @out.WriteString(line);
            }
            writeNL(Ꮡout);
        }
        break;
    }
    case List.val x: {
        var loose = x.BlankBetween();
        foreach (var (i, item) in (~x).Items) {
            if (i > 0 && loose) {
                @out.WriteString(p.prefix);
                writeNL(Ꮡout);
            }
            @out.WriteString(p.prefix);
            @out.WriteString(" "u8);
            if ((~item).Number == ""u8){
                @out.WriteString(" - "u8);
            } else {
                @out.WriteString((~item).Number);
                @out.WriteString(". "u8);
            }
            foreach (var (iΔ1, blk) in (~item).Content) {
                @string fourSpace = "    "u8;
                if (iΔ1 > 0) {
                    writeNL(Ꮡout);
                    @out.WriteString(p.prefix);
                    @out.WriteString(fourSpace);
                }
                p.text(Ꮡout, fourSpace, blk._<Paragraph.val>().Text);
            }
        }
        break;
    }}
}

// text prints the text sequence x to out.
[GoRecv] internal static void text(this ref textPrinter p, ж<bytes.Buffer> Ꮡout, @string indent, slice<ΔText> x) {
    ref var @out = ref Ꮡout.val;

    p.oneLongLine(Ꮡ(p.@long), x);
    var words = strings.Fields(p.@long.String());
    p.@long.Reset();
    slice<nint> seq = default!;
    if (p.width < 0 || len(words) == 0){
        seq = new nint[]{0, len(words)}.slice();
    } else {
        // one long line
        seq = wrap(words, p.width - utf8.RuneCountInString(indent));
    }
    for (nint i = 0; i + 1 < len(seq); i++) {
        if (i > 0) {
            @out.WriteString(p.prefix);
            @out.WriteString(indent);
        }
        foreach (var (j, w) in words[(int)(seq[i])..(int)(seq[i + 1])]) {
            if (j > 0) {
                @out.WriteString(" "u8);
            }
            @out.WriteString(w);
        }
        writeNL(Ꮡout);
    }
}

// oneLongLine prints the text sequence x to out as one long line,
// without worrying about line wrapping.
// Explicit links have the [ ] dropped to improve readability.
[GoRecv] internal static void oneLongLine(this ref textPrinter p, ж<strings.Builder> Ꮡout, slice<ΔText> x) {
    ref var @out = ref Ꮡout.val;

    foreach (var (_, t) in x) {
        switch (t.type()) {
        case Plain t: {
            @out.WriteString(((@string)t));
            break;
        }
        case Italic t: {
            @out.WriteString(((@string)t));
            break;
        }
        case Link.val t: {
            p.oneLongLine(Ꮡout, (~t).Text);
            break;
        }
        case DocLink.val t: {
            p.oneLongLine(Ꮡout, (~t).Text);
            break;
        }}
    }
}

// A score is the score (also called weight) for a given line.
// add and cmp add and compare scores.
[GoType("dyn")] partial struct wrap_score {
    internal int64 hi;
    internal int64 lo;
}

// wrap wraps words into lines of at most max runes,
// minimizing the sum of the squares of the leftover lengths
// at the end of each line (except the last, of course),
// with a preference for ending lines at punctuation (.,:;).
//
// The returned slice gives the indexes of the first words
// on each line in the wrapped text with a final entry of len(words).
// Thus the lines are words[seq[0]:seq[1]], words[seq[1]:seq[2]],
// ..., words[seq[len(seq)-2]:seq[len(seq)-1]].
//
// The implementation runs in O(n log n) time, where n = len(words),
// using the algorithm described in D. S. Hirschberg and L. L. Larmore,
// “[The least weight subsequence problem],” FOCS 1985, pp. 137-143.
//
// [The least weight subsequence problem]: https://doi.org/10.1109/SFCS.1985.60
internal static slice<nint> /*seq*/ wrap(slice<@string> words, nint max) {
    slice<nint> seq = default!;

    // The algorithm requires that our scoring function be concave,
    // meaning that for all i₀ ≤ i₁ < j₀ ≤ j₁,
    // weight(i₀, j₀) + weight(i₁, j₁) ≤ weight(i₀, j₁) + weight(i₁, j₀).
    //
    // Our weights are two-element pairs [hi, lo]
    // ordered by elementwise comparison.
    // The hi entry counts the weight for lines that are longer than max,
    // and the lo entry counts the weight for lines that are not.
    // This forces the algorithm to first minimize the number of lines
    // that are longer than max, which correspond to lines with
    // single very long words. Having done that, it can move on to
    // minimizing the lo score, which is more interesting.
    //
    // The lo score is the sum for each line of the square of the
    // number of spaces remaining at the end of the line and a
    // penalty of 64 given out for not ending the line in a
    // punctuation character (.,:;).
    // The penalty is somewhat arbitrarily chosen by trying
    // different amounts and judging how nice the wrapped text looks.
    // Roughly speaking, using 64 means that we are willing to
    // end a line with eight blank spaces in order to end at a
    // punctuation character, even if the next word would fit in
    // those spaces.
    //
    // We care about ending in punctuation characters because
    // it makes the text easier to skim if not too many sentences
    // or phrases begin with a single word on the previous line.
    var add = (wrap_score s, wrap_score t) => new score(s.hi + t.hi, s.lo + t.lo);
    var cmp = (wrap_score s, wrap_score t) => {
        switch (ᐧ) {
        case {} when s.hi is < t.hi: {
            return -1;
        }
        case {} when s.hi is > t.hi: {
            return +1;
        }
        case {} when s.lo is < t.lo: {
            return -1;
        }
        case {} when s.lo is > t.lo: {
            return +1;
        }}

        return 0;
    };
    // total[j] is the total number of runes
    // (including separating spaces) in words[:j].
    var total = new slice<nint>(len(words) + 1);
    total[0] = 0;
    foreach (var (i, s) in words) {
        total[1 + i] = total[i] + utf8.RuneCountInString(s) + 1;
    }
    // weight returns weight(i, j).
    var weight = 
    var totalʗ1 = total;
    var wordsʗ1 = words;
    (nint i, nint j) => {
        // On the last line, there is zero weight for being too short.
        nint nΔ1 = totalʗ1[j] - 1 - totalʗ1[i];
        if (j == len(wordsʗ1) && nΔ1 <= max) {
            return new score(0, 0);
        }
        // Otherwise the weight is the penalty plus the square of the number of
        // characters remaining on the line or by which the line goes over.
        // In the latter case, that value goes in the hi part of the score.
        // (See note above.)
        var p = wrapPenalty(wordsʗ1[j - 1]);
        var v = ((int64)(max - nΔ1)) * ((int64)(max - nΔ1));
        if (nΔ1 > max) {
            return new score(v, p);
        }
        return new score(0, v + p);
    };
    // The rest of this function is “The Basic Algorithm” from
    // Hirschberg and Larmore's conference paper,
    // using the same names as in the paper.
    var f = new score[]{new(0, 0)}.slice();
    var g = 
    var addʗ1 = add;
    var fʗ1 = f;
    var weightʗ1 = weight;
    (nint i, nint j) => addʗ1(fʗ1[i], weightʗ1(i, j));
    var bridge = 
    var cmpʗ1 = cmp;
    var gʗ1 = g;
    var wordsʗ2 = words;
    (nint a, nint b, nint c) => {
        nint k = c + sort.Search(len(wordsʗ2) + 1 - c, 
        var cmpʗ2 = cmp;
        var gʗ2 = g;
        (nint k) => {
            kΔ1 += c;
            return cmpʗ2(gʗ2(a, kΔ1), gʗ2(b, kΔ1)) > 0;
        });
        if (k > len(words)) {
            return true;
        }
        return cmp(g(c, k), g(b, k)) <= 0;
    };
    // d is a one-ended deque implemented as a slice.
    var d = new slice<nint>(1, len(words));
    d[0] = 0;
    var bestleft = new slice<nint>(1, len(words));
    bestleft[0] = -1;
    for (nint m = 1; m < len(words); m++) {
        f = append(f, g(d[0], m));
        bestleft = append(bestleft, d[0]);
        while (len(d) > 1 && cmp(g(d[1], m + 1), g(d[0], m + 1)) <= 0) {
            d = d[1..];
        }
        // “Retire”
        while (len(d) > 1 && bridge(d[len(d) - 2], d[len(d) - 1], m)) {
            d = d[..(int)(len(d) - 1)];
        }
        // “Fire”
        if (cmp(g(m, len(words)), g(d[len(d) - 1], len(words))) < 0) {
            d = append(d, m);
            // “Hire”
            // The next few lines are not in the paper but are necessary
            // to handle two-word inputs correctly. It appears to be
            // just a bug in the paper's pseudocode.
            if (len(d) == 2 && cmp(g(d[1], m + 1), g(d[0], m + 1)) <= 0) {
                d = d[1..];
            }
        }
    }
    bestleft = append(bestleft, d[0]);
    // Recover least weight sequence from bestleft.
    nint n = 1;
    for (nint m = len(words); m > 0; m = bestleft[m]) {
        n++;
    }
    seq = new slice<nint>(n);
    for (nint m = len(words); m > 0; m = bestleft[m]) {
        n--;
        seq[n] = m;
    }
    return seq;
}

// wrapPenalty is the penalty for inserting a line break after word s.
internal static int64 wrapPenalty(@string s) {
    switch (s[len(s) - 1]) {
    case (rune)'.' or (rune)',' or (rune)':' or (rune)';': {
        return 0;
    }}

    return 64;
}

} // end comment_package
