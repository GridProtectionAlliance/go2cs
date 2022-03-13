// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package fuzzy -- go2cs converted at 2022 March 13 06:42:47 UTC
// import "cmd/vendor/golang.org/x/tools/internal/lsp/fuzzy" ==> using fuzzy = go.cmd.vendor.golang.org.x.tools.@internal.lsp.fuzzy_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\tools\internal\lsp\fuzzy\input.go
namespace go.cmd.vendor.golang.org.x.tools.@internal.lsp;

using unicode = unicode_package;


// RuneRole specifies the role of a rune in the context of an input.

public static partial class fuzzy_package {

public partial struct RuneRole { // : byte
}

 
// RNone specifies a rune without any role in the input (i.e., whitespace/non-ASCII).
public static readonly RuneRole RNone = iota; 
// RSep specifies a rune with the role of segment separator.
public static readonly var RSep = 0; 
// RTail specifies a rune which is a lower-case tail in a word in the input.
public static readonly var RTail = 1; 
// RUCTail specifies a rune which is an upper-case tail in a word in the input.
public static readonly var RUCTail = 2; 
// RHead specifies a rune which is the first character in a word in the input.
public static readonly var RHead = 3;

// RuneRoles detects the roles of each byte rune in an input string and stores it in the output
// slice. The rune role depends on the input type. Stops when it parsed all the runes in the string
// or when it filled the output. If output is nil, then it gets created.
public static slice<RuneRole> RuneRoles(@string str, slice<RuneRole> reuse) {
    slice<RuneRole> output = default;
    if (cap(reuse) < len(str)) {
        output = make_slice<RuneRole>(0, len(str));
    }
    else
 {
        output = reuse[..(int)0];
    }
    var prev = rtNone;
    var prev2 = rtNone;
    for (nint i = 0; i < len(str); i++) {
        var r = rune(str[i]);

        var role = RNone;

        var curr = rtLower;
        if (str[i] <= unicode.MaxASCII) {
            curr = runeType(rt[str[i]] - '0');
        }
        if (curr == rtLower) {
            if (prev == rtNone || prev == rtPunct) {
                role = RHead;
            }
            else
 {
                role = RTail;
            }
        }
        else if (curr == rtUpper) {
            role = RHead;

            if (prev == rtUpper) { 
                // This and previous characters are both upper case.

                if (i + 1 == len(str)) { 
                    // This is last character, previous was also uppercase -> this is UCTail
                    // i.e., (current char is C): aBC / BC / ABC
                    role = RUCTail;
                }
            }
        }
        else if (curr == rtPunct) {
            switch (r) {
                case '.': 

                case ':': 
                    role = RSep;
                    break;
            }
        }
        if (curr != rtLower) {
            if (i > 1 && output[i - 1] == RHead && prev2 == rtUpper && (output[i - 2] == RHead || output[i - 2] == RUCTail)) { 
                // The previous two characters were uppercase. The current one is not a lower case, so the
                // previous one can't be a HEAD. Make it a UCTail.
                // i.e., (last char is current char - B must be a UCTail): ABC / ZABC / AB.
                output[i - 1] = RUCTail;
            }
        }
        output = append(output, role);
        prev2 = prev;
        prev = curr;
    }
    return output;
}

private partial struct runeType { // : byte
}

private static readonly runeType rtNone = iota;
private static readonly var rtPunct = 0;
private static readonly var rtLower = 1;
private static readonly var rtUpper = 2;

private static readonly @string rt = "00000000000000000000000000000000000000000000001122222222221000000333333333333333333333333330000002222222222222222222222222200000";

// LastSegment returns the substring representing the last segment from the input, where each
// byte has an associated RuneRole in the roles slice. This makes sense only for inputs of Symbol
// or Filename type.


// LastSegment returns the substring representing the last segment from the input, where each
// byte has an associated RuneRole in the roles slice. This makes sense only for inputs of Symbol
// or Filename type.
public static @string LastSegment(@string input, slice<RuneRole> roles) { 
    // Exclude ending separators.
    var end = len(input) - 1;
    while (end >= 0 && roles[end] == RSep) {
        end--;
    }
    if (end < 0) {
        return "";
    }
    var start = end - 1;
    while (start >= 0 && roles[start] != RSep) {
        start--;
    }

    return input[(int)start + 1..(int)end + 1];
}

// ToLower transforms the input string to lower case, which is stored in the output byte slice.
// The lower casing considers only ASCII values - non ASCII values are left unmodified.
// Stops when parsed all input or when it filled the output slice. If output is nil, then it gets
// created.
public static slice<byte> ToLower(@string input, slice<byte> reuse) {
    var output = reuse;
    if (cap(reuse) < len(input)) {
        output = make_slice<byte>(len(input));
    }
    for (nint i = 0; i < len(input); i++) {
        var r = rune(input[i]);
        if (r <= unicode.MaxASCII) {
            if ('A' <= r && r <= 'Z') {
                r += 'a' - 'A';
            }
        }
        output[i] = byte(r);
    }
    return output[..(int)len(input)];
}

// WordConsumer defines a consumer for a word delimited by the [start,end) byte offsets in an input
// (start is inclusive, end is exclusive).
public delegate void WordConsumer(nint, nint);

// Words find word delimiters in an input based on its bytes' mappings to rune roles. The offset
// delimiters for each word are fed to the provided consumer function.
public static void Words(slice<RuneRole> roles, WordConsumer consume) {
    nint wordStart = default;
    foreach (var (i, r) in roles) {

        if (r == RUCTail || r == RTail)         else if (r == RHead || r == RNone || r == RSep) 
            if (i != wordStart) {
                consume(wordStart, i);
            }
            wordStart = i;
            if (r != RHead) { 
                // Skip this character.
                wordStart = i + 1;
            }
            }    if (wordStart != len(roles)) {
        consume(wordStart, len(roles));
    }
}

} // end fuzzy_package
