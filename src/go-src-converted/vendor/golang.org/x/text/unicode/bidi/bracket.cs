// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package bidi -- go2cs converted at 2022 March 06 23:38:25 UTC
// import "vendor/golang.org/x/text/unicode/bidi" ==> using bidi = go.vendor.golang.org.x.text.unicode.bidi_package
// Original source: C:\Program Files\Go\src\vendor\golang.org\x\text\unicode\bidi\bracket.go
using list = go.container.list_package;
using fmt = go.fmt_package;
using sort = go.sort_package;

namespace go.vendor.golang.org.x.text.unicode;

public static partial class bidi_package {

    // This file contains a port of the reference implementation of the
    // Bidi Parentheses Algorithm:
    // https://www.unicode.org/Public/PROGRAMS/BidiReferenceJava/BidiPBAReference.java
    //
    // The implementation in this file covers definitions BD14-BD16 and rule N0
    // of UAX#9.
    //
    // Some preprocessing is done for each rune before data is passed to this
    // algorithm:
    //  - opening and closing brackets are identified
    //  - a bracket pair type, like '(' and ')' is assigned a unique identifier that
    //    is identical for the opening and closing bracket. It is left to do these
    //    mappings.
    //  - The BPA algorithm requires that bracket characters that are canonical
    //    equivalents of each other be able to be substituted for each other.
    //    It is the responsibility of the caller to do this canonicalization.
    //
    // In implementing BD16, this implementation departs slightly from the "logical"
    // algorithm defined in UAX#9. In particular, the stack referenced there
    // supports operations that go beyond a "basic" stack. An equivalent
    // implementation based on a linked list is used here.

    // Bidi_Paired_Bracket_Type
    // BD14. An opening paired bracket is a character whose
    // Bidi_Paired_Bracket_Type property value is Open.
    //
    // BD15. A closing paired bracket is a character whose
    // Bidi_Paired_Bracket_Type property value is Close.
private partial struct bracketType { // : byte
}

private static readonly bracketType bpNone = iota;
private static readonly var bpOpen = 0;
private static readonly var bpClose = 1;


// bracketPair holds a pair of index values for opening and closing bracket
// location of a bracket pair.
private partial struct bracketPair {
    public nint opener;
    public nint closer;
}

private static @string String(this ptr<bracketPair> _addr_b) {
    ref bracketPair b = ref _addr_b.val;

    return fmt.Sprintf("(%v, %v)", b.opener, b.closer);
}

// bracketPairs is a slice of bracketPairs with a sort.Interface implementation.
private partial struct bracketPairs { // : slice<bracketPair>
}

private static nint Len(this bracketPairs b) {
    return len(b);
}
private static void Swap(this bracketPairs b, nint i, nint j) {
    (b[i], b[j]) = (b[j], b[i]);
}
private static bool Less(this bracketPairs b, nint i, nint j) {
    return b[i].opener < b[j].opener;
}

// resolvePairedBrackets runs the paired bracket part of the UBA algorithm.
//
// For each rune, it takes the indexes into the original string, the class the
// bracket type (in pairTypes) and the bracket identifier (pairValues). It also
// takes the direction type for the start-of-sentence and the embedding level.
//
// The identifiers for bracket types are the rune of the canonicalized opening
// bracket for brackets (open or close) or 0 for runes that are not brackets.
private static void resolvePairedBrackets(ptr<isolatingRunSequence> _addr_s) {
    ref isolatingRunSequence s = ref _addr_s.val;

    bracketPairer p = new bracketPairer(sos:s.sos,openers:list.New(),codesIsolatedRun:s.types,indexes:s.indexes,);
    var dirEmbed = L;
    if (s.level & 1 != 0) {
        dirEmbed = R;
    }
    p.locateBrackets(s.p.pairTypes, s.p.pairValues);
    p.resolveBrackets(dirEmbed, s.p.initialTypes);

}

private partial struct bracketPairer {
    public Class sos; // direction corresponding to start of sequence

// The following is a restatement of BD 16 using non-algorithmic language.
//
// A bracket pair is a pair of characters consisting of an opening
// paired bracket and a closing paired bracket such that the
// Bidi_Paired_Bracket property value of the former equals the latter,
// subject to the following constraints.
// - both characters of a pair occur in the same isolating run sequence
// - the closing character of a pair follows the opening character
// - any bracket character can belong at most to one pair, the earliest possible one
// - any bracket character not part of a pair is treated like an ordinary character
// - pairs may nest properly, but their spans may not overlap otherwise

// Bracket characters with canonical decompositions are supposed to be
// treated as if they had been normalized, to allow normalized and non-
// normalized text to give the same result. In this implementation that step
// is pushed out to the caller. The caller has to ensure that the pairValue
// slices contain the rune of the opening bracket after normalization for
// any opening or closing bracket.

    public ptr<list.List> openers; // list of positions for opening brackets

// bracket pair positions sorted by location of opening bracket
    public bracketPairs pairPositions;
    public slice<Class> codesIsolatedRun; // directional bidi codes for an isolated run
    public slice<nint> indexes; // array of index values into the original string

}

// matchOpener reports whether characters at given positions form a matching
// bracket pair.
private static bool matchOpener(this ptr<bracketPairer> _addr_p, slice<int> pairValues, nint opener, nint closer) {
    ref bracketPairer p = ref _addr_p.val;

    return pairValues[p.indexes[opener]] == pairValues[p.indexes[closer]];
}

private static readonly nint maxPairingDepth = 63;

// locateBrackets locates matching bracket pairs according to BD16.
//
// This implementation uses a linked list instead of a stack, because, while
// elements are added at the front (like a push) they are not generally removed
// in atomic 'pop' operations, reducing the benefit of the stack archetype.


// locateBrackets locates matching bracket pairs according to BD16.
//
// This implementation uses a linked list instead of a stack, because, while
// elements are added at the front (like a push) they are not generally removed
// in atomic 'pop' operations, reducing the benefit of the stack archetype.
private static void locateBrackets(this ptr<bracketPairer> _addr_p, slice<bracketType> pairTypes, slice<int> pairValues) {
    ref bracketPairer p = ref _addr_p.val;
 
    // traverse the run
    // do that explicitly (not in a for-each) so we can record position
    foreach (var (i, index) in p.indexes) {
        // look at the bracket type for each character
        if (pairTypes[index] == bpNone || p.codesIsolatedRun[i] != ON) { 
            // continue scanning
            continue;

        }

        if (pairTypes[index] == bpOpen) 
            // check if maximum pairing depth reached
            if (p.openers.Len() == maxPairingDepth) {
                p.openers.Init();
                return ;
            } 
            // remember opener location, most recent first
            p.openers.PushFront(i);
        else if (pairTypes[index] == bpClose) 
            // see if there is a match
            nint count = 0;
            {
                var elem = p.openers.Front();

                while (elem != null) {
                    count++;
                    nint opener = elem.Value._<nint>();
                    if (p.matchOpener(pairValues, opener, i)) { 
                        // if the opener matches, add nested pair to the ordered list
                        p.pairPositions = append(p.pairPositions, new bracketPair(opener,i)); 
                        // remove up to and including matched opener
                        while (count > 0) {
                            p.openers.Remove(p.openers.Front());
                            count--;
                        }

                        break;
                    elem = elem.Next();
                    }

                }

            }
            sort.Sort(p.pairPositions); 
            // if we get here, the closing bracket matched no openers
            // and gets ignored
            }
}

// Bracket pairs within an isolating run sequence are processed as units so
// that both the opening and the closing paired bracket in a pair resolve to
// the same direction.
//
// N0. Process bracket pairs in an isolating run sequence sequentially in
// the logical order of the text positions of the opening paired brackets
// using the logic given below. Within this scope, bidirectional types EN
// and AN are treated as R.
//
// Identify the bracket pairs in the current isolating run sequence
// according to BD16. For each bracket-pair element in the list of pairs of
// text positions:
//
// a Inspect the bidirectional types of the characters enclosed within the
// bracket pair.
//
// b If any strong type (either L or R) matching the embedding direction is
// found, set the type for both brackets in the pair to match the embedding
// direction.
//
// o [ e ] o -> o e e e o
//
// o [ o e ] -> o e o e e
//
// o [ NI e ] -> o e NI e e
//
// c Otherwise, if a strong type (opposite the embedding direction) is
// found, test for adjacent strong types as follows: 1 First, check
// backwards before the opening paired bracket until the first strong type
// (L, R, or sos) is found. If that first preceding strong type is opposite
// the embedding direction, then set the type for both brackets in the pair
// to that type. 2 Otherwise, set the type for both brackets in the pair to
// the embedding direction.
//
// o [ o ] e -> o o o o e
//
// o [ o NI ] o -> o o o NI o o
//
// e [ o ] o -> e e o e o
//
// e [ o ] e -> e e o e e
//
// e ( o [ o ] NI ) e -> e e o o o o NI e e
//
// d Otherwise, do not set the type for the current bracket pair. Note that
// if the enclosed text contains no strong types the paired brackets will
// both resolve to the same level when resolved individually using rules N1
// and N2.
//
// e ( NI ) o -> e ( NI ) o

// getStrongTypeN0 maps character's directional code to strong type as required
// by rule N0.
//
// TODO: have separate type for "strong" directionality.
private static Class getStrongTypeN0(this ptr<bracketPairer> _addr_p, nint index) {
    ref bracketPairer p = ref _addr_p.val;


    // in the scope of N0, number types are treated as R
    if (p.codesIsolatedRun[index] == EN || p.codesIsolatedRun[index] == AN || p.codesIsolatedRun[index] == AL || p.codesIsolatedRun[index] == R) 
        return R;
    else if (p.codesIsolatedRun[index] == L) 
        return L;
    else 
        return ON;
    
}

// classifyPairContent reports the strong types contained inside a Bracket Pair,
// assuming the given embedding direction.
//
// It returns ON if no strong type is found. If a single strong type is found,
// it returns this type. Otherwise it returns the embedding direction.
//
// TODO: use separate type for "strong" directionality.
private static Class classifyPairContent(this ptr<bracketPairer> _addr_p, bracketPair loc, Class dirEmbed) {
    ref bracketPairer p = ref _addr_p.val;

    var dirOpposite = ON;
    for (var i = loc.opener + 1; i < loc.closer; i++) {
        var dir = p.getStrongTypeN0(i);
        if (dir == ON) {
            continue;
        }
        if (dir == dirEmbed) {
            return dir; // type matching embedding direction found
        }
        dirOpposite = dir;

    } 
    // return ON if no strong type found, or class opposite to dirEmbed
    return dirOpposite;

}

// classBeforePair determines which strong types are present before a Bracket
// Pair. Return R or L if strong type found, otherwise ON.
private static Class classBeforePair(this ptr<bracketPairer> _addr_p, bracketPair loc) {
    ref bracketPairer p = ref _addr_p.val;

    for (var i = loc.opener - 1; i >= 0; i--) {
        {
            var dir = p.getStrongTypeN0(i);

            if (dir != ON) {
                return dir;
            }

        }

    } 
    // no strong types found, return sos
    return p.sos;

}

// assignBracketType implements rule N0 for a single bracket pair.
private static void assignBracketType(this ptr<bracketPairer> _addr_p, bracketPair loc, Class dirEmbed, slice<Class> initialTypes) {
    ref bracketPairer p = ref _addr_p.val;
 
    // rule "N0, a", inspect contents of pair
    var dirPair = p.classifyPairContent(loc, dirEmbed); 

    // dirPair is now L, R, or N (no strong type found)

    // the following logical tests are performed out of order compared to
    // the statement of the rules but yield the same results
    if (dirPair == ON) {
        return ; // case "d" - nothing to do
    }
    if (dirPair != dirEmbed) { 
        // case "c": strong type found, opposite - check before (c.1)
        dirPair = p.classBeforePair(loc);
        if (dirPair == dirEmbed || dirPair == ON) { 
            // no strong opposite type found before - use embedding (c.2)
            dirPair = dirEmbed;

        }
    }
    p.setBracketsToType(loc, dirPair, initialTypes);

}

private static void setBracketsToType(this ptr<bracketPairer> _addr_p, bracketPair loc, Class dirPair, slice<Class> initialTypes) {
    ref bracketPairer p = ref _addr_p.val;

    p.codesIsolatedRun[loc.opener] = dirPair;
    p.codesIsolatedRun[loc.closer] = dirPair;

    {
        var i__prev1 = i;

        for (var i = loc.opener + 1; i < loc.closer; i++) {
            var index = p.indexes[i];
            if (initialTypes[index] != NSM) {
                break;
            }
            p.codesIsolatedRun[i] = dirPair;
        }

        i = i__prev1;
    }

    {
        var i__prev1 = i;

        for (i = loc.closer + 1; i < len(p.indexes); i++) {
            index = p.indexes[i];
            if (initialTypes[index] != NSM) {
                break;
            }
            p.codesIsolatedRun[i] = dirPair;
        }

        i = i__prev1;
    }

}

// resolveBrackets implements rule N0 for a list of pairs.
private static void resolveBrackets(this ptr<bracketPairer> _addr_p, Class dirEmbed, slice<Class> initialTypes) {
    ref bracketPairer p = ref _addr_p.val;

    foreach (var (_, loc) in p.pairPositions) {
        p.assignBracketType(loc, dirEmbed, initialTypes);
    }
}

} // end bidi_package
