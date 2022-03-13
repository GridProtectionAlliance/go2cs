// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package strings -- go2cs converted at 2022 March 13 05:41:12 UTC
// import "strings" ==> using strings = go.strings_package
// Original source: C:\Program Files\Go\src\strings\search.go
namespace go;

public static partial class strings_package {

// stringFinder efficiently finds strings in a source text. It's implemented
// using the Boyer-Moore string search algorithm:
// https://en.wikipedia.org/wiki/Boyer-Moore_string_search_algorithm
// https://www.cs.utexas.edu/~moore/publications/fstrpos.pdf (note: this aged
// document uses 1-based indexing)
private partial struct stringFinder {
    public @string pattern; // badCharSkip[b] contains the distance between the last byte of pattern
// and the rightmost occurrence of b in pattern. If b is not in pattern,
// badCharSkip[b] is len(pattern).
//
// Whenever a mismatch is found with byte b in the text, we can safely
// shift the matching frame at least badCharSkip[b] until the next time
// the matching char could be in alignment.
    public array<nint> badCharSkip; // goodSuffixSkip[i] defines how far we can shift the matching frame given
// that the suffix pattern[i+1:] matches, but the byte pattern[i] does
// not. There are two cases to consider:
//
// 1. The matched suffix occurs elsewhere in pattern (with a different
// byte preceding it that we might possibly match). In this case, we can
// shift the matching frame to align with the next suffix chunk. For
// example, the pattern "mississi" has the suffix "issi" next occurring
// (in right-to-left order) at index 1, so goodSuffixSkip[3] ==
// shift+len(suffix) == 3+4 == 7.
//
// 2. If the matched suffix does not occur elsewhere in pattern, then the
// matching frame may share part of its prefix with the end of the
// matching suffix. In this case, goodSuffixSkip[i] will contain how far
// to shift the frame to align this portion of the prefix to the
// suffix. For example, in the pattern "abcxxxabc", when the first
// mismatch from the back is found to be in position 3, the matching
// suffix "xxabc" is not found elsewhere in the pattern. However, its
// rightmost "abc" (at position 6) is a prefix of the whole pattern, so
// goodSuffixSkip[3] == shift+len(suffix) == 6+5 == 11.
    public slice<nint> goodSuffixSkip;
}

private static ptr<stringFinder> makeStringFinder(@string pattern) {
    ptr<stringFinder> f = addr(new stringFinder(pattern:pattern,goodSuffixSkip:make([]int,len(pattern)),)); 
    // last is the index of the last character in the pattern.
    var last = len(pattern) - 1; 

    // Build bad character table.
    // Bytes not in the pattern can skip one pattern's length.
    {
        var i__prev1 = i;

        foreach (var (__i) in f.badCharSkip) {
            i = __i;
            f.badCharSkip[i] = len(pattern);
        }
        i = i__prev1;
    }

    {
        var i__prev1 = i;

        for (nint i = 0; i < last; i++) {
            f.badCharSkip[pattern[i]] = last - i;
        }

        i = i__prev1;
    } 

    // Build good suffix table.
    // First pass: set each value to the next index which starts a prefix of
    // pattern.
    var lastPrefix = last;
    {
        var i__prev1 = i;

        for (i = last; i >= 0; i--) {
            if (HasPrefix(pattern, pattern[(int)i + 1..])) {
                lastPrefix = i + 1;
            } 
            // lastPrefix is the shift, and (last-i) is len(suffix).
            f.goodSuffixSkip[i] = lastPrefix + last - i;
        }

        i = i__prev1;
    } 
    // Second pass: find repeats of pattern's suffix starting from the front.
    {
        var i__prev1 = i;

        for (i = 0; i < last; i++) {
            var lenSuffix = longestCommonSuffix(pattern, pattern[(int)1..(int)i + 1]);
            if (pattern[i - lenSuffix] != pattern[last - lenSuffix]) { 
                // (last-i) is the shift, and lenSuffix is len(suffix).
                f.goodSuffixSkip[last - lenSuffix] = lenSuffix + last - i;
            }
        }

        i = i__prev1;
    }

    return _addr_f!;
}

private static nint longestCommonSuffix(@string a, @string b) {
    nint i = default;

    while (i < len(a) && i < len(b)) {
        if (a[len(a) - 1 - i] != b[len(b) - 1 - i]) {
            break;
        i++;
        }
    }
    return ;
}

// next returns the index in text of the first occurrence of the pattern. If
// the pattern is not found, it returns -1.
private static nint next(this ptr<stringFinder> _addr_f, @string text) {
    ref stringFinder f = ref _addr_f.val;

    var i = len(f.pattern) - 1;
    while (i < len(text)) { 
        // Compare backwards from the end until the first unmatching character.
        var j = len(f.pattern) - 1;
        while (j >= 0 && text[i] == f.pattern[j]) {
            i--;
            j--;
        }
        if (j < 0) {
            return i + 1; // match
        }
        i += max(f.badCharSkip[text[i]], f.goodSuffixSkip[j]);
    }
    return -1;
}

private static nint max(nint a, nint b) {
    if (a > b) {
        return a;
    }
    return b;
}

} // end strings_package
