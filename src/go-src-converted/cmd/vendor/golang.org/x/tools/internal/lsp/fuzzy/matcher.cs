// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package fuzzy implements a fuzzy matching algorithm.
// package fuzzy -- go2cs converted at 2022 March 06 23:35:15 UTC
// import "cmd/vendor/golang.org/x/tools/internal/lsp/fuzzy" ==> using fuzzy = go.cmd.vendor.golang.org.x.tools.@internal.lsp.fuzzy_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\tools\internal\lsp\fuzzy\matcher.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;

namespace go.cmd.vendor.golang.org.x.tools.@internal.lsp;

public static partial class fuzzy_package {

 
// MaxInputSize is the maximum size of the input scored against the fuzzy matcher. Longer inputs
// will be truncated to this size.
public static readonly nint MaxInputSize = 127; 
// MaxPatternSize is the maximum size of the pattern used to construct the fuzzy matcher. Longer
// inputs are truncated to this size.
public static readonly nint MaxPatternSize = 63;


private partial struct scoreVal { // : nint
}

private static nint val(this scoreVal s) {
    return int(s) >> 1;
}

private static nint prevK(this scoreVal s) {
    return int(s) & 1;
}

private static scoreVal score(nint val, nint prevK) {
    return scoreVal(val << 1 + prevK);
}

// Matcher implements a fuzzy matching algorithm for scoring candidates against a pattern.
// The matcher does not support parallel usage.
public partial struct Matcher {
    public @string pattern;
    public slice<byte> patternLower; // lower-case version of the pattern
    public slice<byte> patternShort; // first characters of the pattern
    public bool caseSensitive; // set if the pattern is mix-cased

    public slice<RuneRole> patternRoles; // the role of each character in the pattern
    public slice<RuneRole> roles; // the role of each character in the tested string

    public array<array<array<scoreVal>>> scores;
    public float scoreScale;
    public nint lastCandidateLen; // in bytes
    public bool lastCandidateMatched; // Here we save the last candidate in lower-case. This is basically a byte slice we reuse for
// performance reasons, so the slice is not reallocated for every candidate.
    public array<byte> lowerBuf;
    public array<RuneRole> rolesBuf;
}

private static nint bestK(this ptr<Matcher> _addr_m, nint i, nint j) {
    ref Matcher m = ref _addr_m.val;

    if (m.scores[i][j][0].val() < m.scores[i][j][1].val()) {
        return 1;
    }
    return 0;

}

// NewMatcher returns a new fuzzy matcher for scoring candidates against the provided pattern.
public static ptr<Matcher> NewMatcher(@string pattern) {
    if (len(pattern) > MaxPatternSize) {
        pattern = pattern[..(int)MaxPatternSize];
    }
    ptr<Matcher> m = addr(new Matcher(pattern:pattern,patternLower:ToLower(pattern,nil),));

    foreach (var (i, c) in m.patternLower) {
        if (pattern[i] != c) {
            m.caseSensitive = true;
            break;
        }
    }    if (len(pattern) > 3) {
        m.patternShort = m.patternLower[..(int)3];
    }
    else
 {
        m.patternShort = m.patternLower;
    }
    m.patternRoles = RuneRoles(pattern, null);

    if (len(pattern) > 0) {
        nint maxCharScore = 4;
        m.scoreScale = 1 / float32(maxCharScore * len(pattern));
    }
    return _addr_m!;

}

// Score returns the score returned by matching the candidate to the pattern.
// This is not designed for parallel use. Multiple candidates must be scored sequentially.
// Returns a score between 0 and 1 (0 - no match, 1 - perfect match).
private static float Score(this ptr<Matcher> _addr_m, @string candidate) {
    ref Matcher m = ref _addr_m.val;

    if (len(candidate) > MaxInputSize) {
        candidate = candidate[..(int)MaxInputSize];
    }
    var lower = ToLower(candidate, m.lowerBuf[..]);
    m.lastCandidateLen = len(candidate);

    if (len(m.pattern) == 0) { 
        // Empty patterns perfectly match candidates.
        return 1;

    }
    if (m.match(candidate, lower)) {
        var sc = m.computeScore(candidate, lower);
        if (sc > minScore / 2 && !m.poorMatch()) {
            m.lastCandidateMatched = true;
            if (len(m.pattern) == len(candidate)) { 
                // Perfect match.
                return 1;

            }

            if (sc < 0) {
                sc = 0;
            }

            var normalizedScore = float32(sc) * m.scoreScale;
            if (normalizedScore > 1) {
                normalizedScore = 1;
            }

            return normalizedScore;

        }
    }
    m.lastCandidateMatched = false;
    return 0;

}

private static readonly nint minScore = -10000;

// MatchedRanges returns matches ranges for the last scored string as a flattened array of
// [begin, end) byte offset pairs.


// MatchedRanges returns matches ranges for the last scored string as a flattened array of
// [begin, end) byte offset pairs.
private static slice<nint> MatchedRanges(this ptr<Matcher> _addr_m) {
    ref Matcher m = ref _addr_m.val;

    if (len(m.pattern) == 0 || !m.lastCandidateMatched) {
        return null;
    }
    var i = m.lastCandidateLen;
    var j = len(m.pattern);
    if (m.scores[i][j][0].val() < minScore / 2 && m.scores[i][j][1].val() < minScore / 2) {
        return null;
    }
    slice<nint> ret = default;
    var k = m.bestK(i, j);
    while (i > 0) {
        var take = (k == 1);
        k = m.scores[i][j][k].prevK();
        if (take) {
            if (len(ret) == 0 || ret[len(ret) - 1] != i) {
                ret = append(ret, i);
                ret = append(ret, i - 1);
            }
            else
 {
                ret[len(ret) - 1] = i - 1;
            }

            j--;

        }
        i--;

    } 
    // Reverse slice.
    {
        var i__prev1 = i;

        for (i = 0; i < len(ret) / 2; i++) {
            (ret[i], ret[len(ret) - 1 - i]) = (ret[len(ret) - 1 - i], ret[i]);
        }

        i = i__prev1;
    }
    return ret;

}

private static bool match(this ptr<Matcher> _addr_m, @string candidate, slice<byte> candidateLower) {
    ref Matcher m = ref _addr_m.val;

    nint i = 0;
    nint j = 0;
    while (i < len(candidateLower) && j < len(m.patternLower)) {
        if (candidateLower[i] == m.patternLower[j]) {
            j++;
        i++;
        }
    }
    if (j != len(m.patternLower)) {
        return false;
    }
    m.roles = RuneRoles(candidate, m.rolesBuf[..]);

    return true;

}

private static nint computeScore(this ptr<Matcher> _addr_m, @string candidate, slice<byte> candidateLower) {
    ref Matcher m = ref _addr_m.val;

    var pattLen = len(m.pattern);
    var candLen = len(candidate);

    {
        nint j__prev1 = j;

        for (nint j = 0; j <= len(m.pattern); j++) {
            m.scores[0][j][0] = minScore << 1;
            m.scores[0][j][1] = minScore << 1;
        }

        j = j__prev1;
    }
    m.scores[0][0][0] = score(0, 0); // Start with 0.

    nint segmentsLeft = 1;
    nint lastSegStart = 0;
    {
        nint i__prev1 = i;

        for (nint i = 0; i < candLen; i++) {
            if (m.roles[i] == RSep) {
                segmentsLeft++;
                lastSegStart = i + 1;
            }
        }

        i = i__prev1;
    } 

    // A per-character bonus for a consecutive match.
    nint consecutiveBonus = 2;
    nint wordIdx = 0; // Word count within segment.
    {
        nint i__prev1 = i;

        for (i = 1; i <= candLen; i++) {
            var role = m.roles[i - 1];
            var isHead = role == RHead;

            if (isHead) {
                wordIdx++;
            }
            else if (role == RSep && segmentsLeft > 1) {
                wordIdx = 0;
                segmentsLeft--;
            }

            nint skipPenalty = default;
            if (i == 1 || (i - 1) == lastSegStart) { 
                // Skipping the start of first or last segment.
                skipPenalty++;

            }

            {
                nint j__prev2 = j;

                for (j = 0; j <= pattLen; j++) { 
                    // By default, we don't have a match. Fill in the skip data.
                    m.scores[i][j][1] = minScore << 1; 

                    // Compute the skip score.
                    nint k = 0;
                    if (m.scores[i - 1][j][0].val() < m.scores[i - 1][j][1].val()) {
                        k = 1;
                    }

                    var skipScore = m.scores[i - 1][j][k].val(); 
                    // Do not penalize missing characters after the last matched segment.
                    if (j != pattLen) {
                        skipScore -= skipPenalty;
                    }

                    m.scores[i][j][0] = score(skipScore, k);

                    if (j == 0 || candidateLower[i - 1] != m.patternLower[j - 1]) { 
                        // Not a match.
                        continue;

                    }

                    var pRole = m.patternRoles[j - 1];

                    if (role == RTail && pRole == RHead) {
                        if (j > 1) { 
                            // Not a match: a head in the pattern matches a tail character in the candidate.
                            continue;

                        } 
                        // Special treatment for the first character of the pattern. We allow
                        // matches in the middle of a word if they are long enough, at least
                        // min(3, pattern.length) characters.
                        if (!bytes.HasPrefix(candidateLower[(int)i - 1..], m.patternShort)) {
                            continue;
                        }

                    } 

                    // Compute the char score.
                    nint charScore = default; 
                    // Bonus 1: the char is in the candidate's last segment.
                    if (segmentsLeft <= 1) {
                        charScore++;
                    } 
                    // Bonus 2: Case match or a Head in the pattern aligns with one in the word.
                    // Single-case patterns lack segmentation signals and we assume any character
                    // can be a head of a segment.
                    if (candidate[i - 1] == m.pattern[j - 1] || role == RHead && (!m.caseSensitive || pRole == RHead)) {
                        charScore++;
                    } 

                    // Penalty 1: pattern char is Head, candidate char is Tail.
                    if (role == RTail && pRole == RHead) {
                        charScore--;
                    } 
                    // Penalty 2: first pattern character matched in the middle of a word.
                    if (j == 1 && role == RTail) {
                        charScore -= 4;
                    } 

                    // Third dimension encodes whether there is a gap between the previous match and the current
                    // one.
                    {
                        nint k__prev3 = k;

                        for (k = 0; k < 2; k++) {
                            var sc = m.scores[i - 1][j - 1][k].val() + charScore;

                            var isConsecutive = k == 1 || i - 1 == 0 || i - 1 == lastSegStart;
                            if (isConsecutive) { 
                                // Bonus 3: a consecutive match. First character match also gets a bonus to
                                // ensure prefix final match score normalizes to 1.0.
                                // Logically, this is a part of charScore, but we have to compute it here because it
                                // only applies for consecutive matches (k == 1).
                                sc += consecutiveBonus;

                            }

                            if (k == 0) { 
                                // Penalty 3: Matching inside a segment (and previous char wasn't matched). Penalize for the lack
                                // of alignment.
                                if (role == RTail || role == RUCTail) {
                                    sc -= 3;
                                }

                            }

                            if (sc > m.scores[i][j][1].val()) {
                                m.scores[i][j][1] = score(sc, k);
                            }

                        }


                        k = k__prev3;
                    }

                }


                j = j__prev2;
            }

        }

        i = i__prev1;
    }

    var result = m.scores[len(candidate)][len(m.pattern)][m.bestK(len(candidate), len(m.pattern))].val();

    return result;

}

// ScoreTable returns the score table computed for the provided candidate. Used only for debugging.
private static @string ScoreTable(this ptr<Matcher> _addr_m, @string candidate) {
    ref Matcher m = ref _addr_m.val;

    bytes.Buffer buf = default;

    bytes.Buffer line1 = default;    bytes.Buffer line2 = default;    bytes.Buffer separator = default;

    line1.WriteString("\t");
    line2.WriteString("\t");
    {
        nint j__prev1 = j;

        for (nint j = 0; j < len(m.pattern); j++) {
            line1.WriteString(fmt.Sprintf("%c\t\t", m.pattern[j]));
            separator.WriteString("----------------");
        }

        j = j__prev1;
    }

    buf.WriteString(line1.String());
    buf.WriteString("\n");
    buf.WriteString(separator.String());
    buf.WriteString("\n");

    for (nint i = 1; i <= len(candidate); i++) {
        line1.Reset();
        line2.Reset();

        line1.WriteString(fmt.Sprintf("%c\t", candidate[i - 1]));
        line2.WriteString("\t");

        {
            nint j__prev2 = j;

            for (j = 1; j <= len(m.pattern); j++) {
                line1.WriteString(fmt.Sprintf("M%6d(%c)\t", m.scores[i][j][0].val(), dir(m.scores[i][j][0].prevK())));
                line2.WriteString(fmt.Sprintf("H%6d(%c)\t", m.scores[i][j][1].val(), dir(m.scores[i][j][1].prevK())));
            }


            j = j__prev2;
        }
        buf.WriteString(line1.String());
        buf.WriteString("\n");
        buf.WriteString(line2.String());
        buf.WriteString("\n");
        buf.WriteString(separator.String());
        buf.WriteString("\n");

    }

    return buf.String();

}

private static int dir(nint prevK) {
    if (prevK == 0) {
        return 'M';
    }
    return 'H';

}

private static bool poorMatch(this ptr<Matcher> _addr_m) {
    ref Matcher m = ref _addr_m.val;

    if (len(m.pattern) < 2) {
        return false;
    }
    var i = m.lastCandidateLen;
    var j = len(m.pattern);
    var k = m.bestK(i, j);

    nint counter = default;    nint len = default;

    while (i > 0) {
        var take = (k == 1);
        k = m.scores[i][j][k].prevK();
        if (take) {
            len++;
            if (k == 0 && len < 3 && m.roles[i - 1] == RTail) { 
                // Short match in the middle of a word
                counter++;
                if (counter > 1) {
                    return true;
                }

            }

            j--;

        }
        else
 {
            len = 0;
        }
        i--;

    }
    return false;

}

} // end fuzzy_package
