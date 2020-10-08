// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package bidi -- go2cs converted at 2020 October 08 05:01:56 UTC
// import "vendor/golang.org/x/text/unicode/bidi" ==> using bidi = go.vendor.golang.org.x.text.unicode.bidi_package
// Original source: C:\Go\src\vendor\golang.org\x\text\unicode\bidi\core.go
using log = go.log_package;
using static go.builtin;

namespace go {
namespace vendor {
namespace golang.org {
namespace x {
namespace text {
namespace unicode
{
    public static partial class bidi_package
    {
        // This implementation is a port based on the reference implementation found at:
        // https://www.unicode.org/Public/PROGRAMS/BidiReferenceJava/
        //
        // described in Unicode Bidirectional Algorithm (UAX #9).
        //
        // Input:
        // There are two levels of input to the algorithm, since clients may prefer to
        // supply some information from out-of-band sources rather than relying on the
        // default behavior.
        //
        // - Bidi class array
        // - Bidi class array, with externally supplied base line direction
        //
        // Output:
        // Output is separated into several stages:
        //
        //  - levels array over entire paragraph
        //  - reordering array over entire paragraph
        //  - levels array over line
        //  - reordering array over line
        //
        // Note that for conformance to the Unicode Bidirectional Algorithm,
        // implementations are only required to generate correct reordering and
        // character directionality (odd or even levels) over a line. Generating
        // identical level arrays over a line is not required. Bidi explicit format
        // codes (LRE, RLE, LRO, RLO, PDF) and BN can be assigned arbitrary levels and
        // positions as long as the rest of the input is properly reordered.
        //
        // As the algorithm is defined to operate on a single paragraph at a time, this
        // implementation is written to handle single paragraphs. Thus rule P1 is
        // presumed by this implementation-- the data provided to the implementation is
        // assumed to be a single paragraph, and either contains no 'B' codes, or a
        // single 'B' code at the end of the input. 'B' is allowed as input to
        // illustrate how the algorithm assigns it a level.
        //
        // Also note that rules L3 and L4 depend on the rendering engine that uses the
        // result of the bidi algorithm. This implementation assumes that the rendering
        // engine expects combining marks in visual order (e.g. to the left of their
        // base character in RTL runs) and that it adjusts the glyphs used to render
        // mirrored characters that are in RTL runs so that they render appropriately.

        // level is the embedding level of a character. Even embedding levels indicate
        // left-to-right order and odd levels indicate right-to-left order. The special
        // level of -1 is reserved for undefined order.
        private partial struct level // : sbyte
        {
        }

        private static readonly level implicitLevel = (level)-1L;

        // in returns if x is equal to any of the values in set.


        // in returns if x is equal to any of the values in set.
        public static bool @in(this Class c, params Class[] set)
        {
            set = set.Clone();

            foreach (var (_, s) in set)
            {
                if (c == s)
                {
                    return true;
                }

            }
            return false;

        }

        // A paragraph contains the state of a paragraph.
        private partial struct paragraph
        {
            public slice<Class> initialTypes; // Arrays of properties needed for paired bracket evaluation in N0
            public slice<bracketType> pairTypes; // paired Bracket types for paragraph
            public slice<int> pairValues; // rune for opening bracket or pbOpen and pbClose; 0 for pbNone

            public level embeddingLevel; // default: = implicitLevel;

// at the paragraph levels
            public slice<Class> resultTypes;
            public slice<level> resultLevels; // Index of matching PDI for isolate initiator characters. For other
// characters, the value of matchingPDI will be set to -1. For isolate
// initiators with no matching PDI, matchingPDI will be set to the length of
// the input string.
            public slice<long> matchingPDI; // Index of matching isolate initiator for PDI characters. For other
// characters, and for PDIs with no matching isolate initiator, the value of
// matchingIsolateInitiator will be set to -1.
            public slice<long> matchingIsolateInitiator;
        }

        // newParagraph initializes a paragraph. The user needs to supply a few arrays
        // corresponding to the preprocessed text input. The types correspond to the
        // Unicode BiDi classes for each rune. pairTypes indicates the bracket type for
        // each rune. pairValues provides a unique bracket class identifier for each
        // rune (suggested is the rune of the open bracket for opening and matching
        // close brackets, after normalization). The embedding levels are optional, but
        // may be supplied to encode embedding levels of styled text.
        //
        // TODO: return an error.
        private static ptr<paragraph> newParagraph(slice<Class> types, slice<bracketType> pairTypes, slice<int> pairValues, level levels)
        {
            validateTypes(types);
            validatePbTypes(pairTypes);
            validatePbValues(pairValues, pairTypes);
            validateParagraphEmbeddingLevel(levels);

            ptr<paragraph> p = addr(new paragraph(initialTypes:append([]Class(nil),types...),embeddingLevel:levels,pairTypes:pairTypes,pairValues:pairValues,resultTypes:append([]Class(nil),types...),));
            p.run();
            return _addr_p!;
        }

        private static long Len(this ptr<paragraph> _addr_p)
        {
            ref paragraph p = ref _addr_p.val;

            return len(p.initialTypes);
        }

        // The algorithm. Does not include line-based processing (Rules L1, L2).
        // These are applied later in the line-based phase of the algorithm.
        private static void run(this ptr<paragraph> _addr_p)
        {
            ref paragraph p = ref _addr_p.val;

            p.determineMatchingIsolates(); 

            // 1) determining the paragraph level
            // Rule P1 is the requirement for entering this algorithm.
            // Rules P2, P3.
            // If no externally supplied paragraph embedding level, use default.
            if (p.embeddingLevel == implicitLevel)
            {
                p.embeddingLevel = p.determineParagraphEmbeddingLevel(0L, p.Len());
            } 

            // Initialize result levels to paragraph embedding level.
            p.resultLevels = make_slice<level>(p.Len());
            setLevels(p.resultLevels, p.embeddingLevel); 

            // 2) Explicit levels and directions
            // Rules X1-X8.
            p.determineExplicitEmbeddingLevels(); 

            // Rule X9.
            // We do not remove the embeddings, the overrides, the PDFs, and the BNs
            // from the string explicitly. But they are not copied into isolating run
            // sequences when they are created, so they are removed for all
            // practical purposes.

            // Rule X10.
            // Run remainder of algorithm one isolating run sequence at a time
            foreach (var (_, seq) in p.determineIsolatingRunSequences())
            { 
                // 3) resolving weak types
                // Rules W1-W7.
                seq.resolveWeakTypes(); 

                // 4a) resolving paired brackets
                // Rule N0
                resolvePairedBrackets(seq); 

                // 4b) resolving neutral types
                // Rules N1-N3.
                seq.resolveNeutralTypes(); 

                // 5) resolving implicit embedding levels
                // Rules I1, I2.
                seq.resolveImplicitLevels(); 

                // Apply the computed levels and types
                seq.applyLevelsAndTypes();

            } 

            // Assign appropriate levels to 'hide' LREs, RLEs, LROs, RLOs, PDFs, and
            // BNs. This is for convenience, so the resulting level array will have
            // a value for every character.
            p.assignLevelsToCharactersRemovedByX9();

        }

        // determineMatchingIsolates determines the matching PDI for each isolate
        // initiator and vice versa.
        //
        // Definition BD9.
        //
        // At the end of this function:
        //
        //  - The member variable matchingPDI is set to point to the index of the
        //    matching PDI character for each isolate initiator character. If there is
        //    no matching PDI, it is set to the length of the input text. For other
        //    characters, it is set to -1.
        //  - The member variable matchingIsolateInitiator is set to point to the
        //    index of the matching isolate initiator character for each PDI character.
        //    If there is no matching isolate initiator, or the character is not a PDI,
        //    it is set to -1.
        private static void determineMatchingIsolates(this ptr<paragraph> _addr_p)
        {
            ref paragraph p = ref _addr_p.val;

            p.matchingPDI = make_slice<long>(p.Len());
            p.matchingIsolateInitiator = make_slice<long>(p.Len());

            {
                var i__prev1 = i;

                foreach (var (__i) in p.matchingIsolateInitiator)
                {
                    i = __i;
                    p.matchingIsolateInitiator[i] = -1L;
                }

                i = i__prev1;
            }

            {
                var i__prev1 = i;

                foreach (var (__i) in p.matchingPDI)
                {
                    i = __i;
                    p.matchingPDI[i] = -1L;

                    {
                        var t = p.resultTypes[i];

                        if (t.@in(LRI, RLI, FSI))
                        {
                            long depthCounter = 1L;
                            for (var j = i + 1L; j < p.Len(); j++)
                            {
                                {
                                    var u = p.resultTypes[j];

                                    if (u.@in(LRI, RLI, FSI))
                                    {
                                        depthCounter++;
                                    }
                                    else if (u == PDI)
                                    {
                                        depthCounter--;

                                        if (depthCounter == 0L)
                                        {
                                            p.matchingPDI[i] = j;
                                            p.matchingIsolateInitiator[j] = i;
                                            break;
                                        }

                                    }


                                }

                            }

                            if (p.matchingPDI[i] == -1L)
                            {
                                p.matchingPDI[i] = p.Len();
                            }

                        }

                    }

                }

                i = i__prev1;
            }
        }

        // determineParagraphEmbeddingLevel reports the resolved paragraph direction of
        // the substring limited by the given range [start, end).
        //
        // Determines the paragraph level based on rules P2, P3. This is also used
        // in rule X5c to find if an FSI should resolve to LRI or RLI.
        private static level determineParagraphEmbeddingLevel(this ptr<paragraph> _addr_p, long start, long end)
        {
            ref paragraph p = ref _addr_p.val;

            Class strongType = unknownClass; 

            // Rule P2.
            for (var i = start; i < end; i++)
            {
                {
                    var t = p.resultTypes[i];

                    if (t.@in(L, AL, R))
                    {
                        strongType = t;
                        break;
                    }
                    else if (t.@in(FSI, LRI, RLI))
                    {
                        i = p.matchingPDI[i]; // skip over to the matching PDI
                        if (i > end)
                        {
                            log.Panic("assert (i <= end)");
                        }

                    }


                }

            } 
            // Rule P3.
 
            // Rule P3.

            if (strongType == unknownClass) // none found
                // default embedding level when no strong types found is 0.
                return 0L;
            else if (strongType == L) 
                return 0L;
            else // AL, R
                return 1L;
            
        }

        private static readonly long maxDepth = (long)125L;

        // This stack will store the embedding levels and override and isolated
        // statuses


        // This stack will store the embedding levels and override and isolated
        // statuses
        private partial struct directionalStatusStack
        {
            public long stackCounter;
            public array<level> embeddingLevelStack;
            public array<Class> overrideStatusStack;
            public array<bool> isolateStatusStack;
        }

        private static void empty(this ptr<directionalStatusStack> _addr_s)
        {
            ref directionalStatusStack s = ref _addr_s.val;

            s.stackCounter = 0L;
        }
        private static void pop(this ptr<directionalStatusStack> _addr_s)
        {
            ref directionalStatusStack s = ref _addr_s.val;

            s.stackCounter--;
        }
        private static long depth(this ptr<directionalStatusStack> _addr_s)
        {
            ref directionalStatusStack s = ref _addr_s.val;

            return s.stackCounter;
        }

        private static void push(this ptr<directionalStatusStack> _addr_s, level level, Class overrideStatus, bool isolateStatus)
        {
            ref directionalStatusStack s = ref _addr_s.val;

            s.embeddingLevelStack[s.stackCounter] = level;
            s.overrideStatusStack[s.stackCounter] = overrideStatus;
            s.isolateStatusStack[s.stackCounter] = isolateStatus;
            s.stackCounter++;
        }

        private static level lastEmbeddingLevel(this ptr<directionalStatusStack> _addr_s)
        {
            ref directionalStatusStack s = ref _addr_s.val;

            return s.embeddingLevelStack[s.stackCounter - 1L];
        }

        private static Class lastDirectionalOverrideStatus(this ptr<directionalStatusStack> _addr_s)
        {
            ref directionalStatusStack s = ref _addr_s.val;

            return s.overrideStatusStack[s.stackCounter - 1L];
        }

        private static bool lastDirectionalIsolateStatus(this ptr<directionalStatusStack> _addr_s)
        {
            ref directionalStatusStack s = ref _addr_s.val;

            return s.isolateStatusStack[s.stackCounter - 1L];
        }

        // Determine explicit levels using rules X1 - X8
        private static void determineExplicitEmbeddingLevels(this ptr<paragraph> _addr_p)
        {
            ref paragraph p = ref _addr_p.val;

            directionalStatusStack stack = default;
            long overflowIsolateCount = default;            long overflowEmbeddingCount = default;            long validIsolateCount = default; 

            // Rule X1.
 

            // Rule X1.
            stack.push(p.embeddingLevel, ON, false);

            foreach (var (i, t) in p.resultTypes)
            { 
                // Rules X2, X3, X4, X5, X5a, X5b, X5c

                if (t == RLE || t == LRE || t == RLO || t == LRO || t == RLI || t == LRI || t == FSI) 
                    var isIsolate = t.@in(RLI, LRI, FSI);
                    var isRTL = t.@in(RLE, RLO, RLI); 

                    // override if this is an FSI that resolves to RLI
                    if (t == FSI)
                    {
                        isRTL = (p.determineParagraphEmbeddingLevel(i + 1L, p.matchingPDI[i]) == 1L);
                    }

                    if (isIsolate)
                    {
                        p.resultLevels[i] = stack.lastEmbeddingLevel();
                        if (stack.lastDirectionalOverrideStatus() != ON)
                        {
                            p.resultTypes[i] = stack.lastDirectionalOverrideStatus();
                        }

                    }

                    level newLevel = default;
                    if (isRTL)
                    { 
                        // least greater odd
                        newLevel = (stack.lastEmbeddingLevel() + 1L) | 1L;

                    }
                    else
                    { 
                        // least greater even
                        newLevel = (stack.lastEmbeddingLevel() + 2L) & ~1L;

                    }

                    if (newLevel <= maxDepth && overflowIsolateCount == 0L && overflowEmbeddingCount == 0L)
                    {
                        if (isIsolate)
                        {
                            validIsolateCount++;
                        } 
                        // Push new embedding level, override status, and isolated
                        // status.
                        // No check for valid stack counter, since the level check
                        // suffices.

                        if (t == LRO) 
                            stack.push(newLevel, L, isIsolate);
                        else if (t == RLO) 
                            stack.push(newLevel, R, isIsolate);
                        else 
                            stack.push(newLevel, ON, isIsolate);
                        // Not really part of the spec
                        if (!isIsolate)
                        {
                            p.resultLevels[i] = newLevel;
                        }

                    }
                    else
                    { 
                        // This is an invalid explicit formatting character,
                        // so apply the "Otherwise" part of rules X2-X5b.
                        if (isIsolate)
                        {
                            overflowIsolateCount++;
                        }
                        else
                        { // !isIsolate
                            if (overflowIsolateCount == 0L)
                            {
                                overflowEmbeddingCount++;
                            }

                        }

                    } 

                    // Rule X6a
                else if (t == PDI) 
                    if (overflowIsolateCount > 0L)
                    {
                        overflowIsolateCount--;
                    }
                    else if (validIsolateCount == 0L)
                    { 
                        // do nothing
                    }
                    else
                    {
                        overflowEmbeddingCount = 0L;
                        while (!stack.lastDirectionalIsolateStatus())
                        {
                            stack.pop();
                        }

                        stack.pop();
                        validIsolateCount--;

                    }

                    p.resultLevels[i] = stack.lastEmbeddingLevel(); 

                    // Rule X7
                else if (t == PDF) 
                    // Not really part of the spec
                    p.resultLevels[i] = stack.lastEmbeddingLevel();

                    if (overflowIsolateCount > 0L)
                    { 
                        // do nothing
                    }
                    else if (overflowEmbeddingCount > 0L)
                    {
                        overflowEmbeddingCount--;
                    }
                    else if (!stack.lastDirectionalIsolateStatus() && stack.depth() >= 2L)
                    {
                        stack.pop();
                    }

                else if (t == B) // paragraph separator.
                    // Rule X8.

                    // These values are reset for clarity, in this implementation B
                    // can only occur as the last code in the array.
                    stack.empty();
                    overflowIsolateCount = 0L;
                    overflowEmbeddingCount = 0L;
                    validIsolateCount = 0L;
                    p.resultLevels[i] = p.embeddingLevel;
                else 
                    p.resultLevels[i] = stack.lastEmbeddingLevel();
                    if (stack.lastDirectionalOverrideStatus() != ON)
                    {
                        p.resultTypes[i] = stack.lastDirectionalOverrideStatus();
                    }

                            }

        }

        private partial struct isolatingRunSequence
        {
            public ptr<paragraph> p;
            public slice<long> indexes; // indexes to the original string

            public slice<Class> types; // type of each character using the index
            public slice<level> resolvedLevels; // resolved levels after application of rules
            public level level;
            public Class sos;
            public Class eos;
        }

        private static long Len(this ptr<isolatingRunSequence> _addr_i)
        {
            ref isolatingRunSequence i = ref _addr_i.val;

            return len(i.indexes);
        }

        private static level maxLevel(level a, level b)
        {
            if (a > b)
            {
                return a;
            }

            return b;

        }

        // Rule X10, second bullet: Determine the start-of-sequence (sos) and end-of-sequence (eos) types,
        //              either L or R, for each isolating run sequence.
        private static ptr<isolatingRunSequence> isolatingRunSequence(this ptr<paragraph> _addr_p, slice<long> indexes)
        {
            ref paragraph p = ref _addr_p.val;

            var length = len(indexes);
            var types = make_slice<Class>(length);
            foreach (var (i, x) in indexes)
            {
                types[i] = p.resultTypes[x];
            } 

            // assign level, sos and eos
            var prevChar = indexes[0L] - 1L;
            while (prevChar >= 0L && isRemovedByX9(p.initialTypes[prevChar]))
            {
                prevChar--;
            }

            var prevLevel = p.embeddingLevel;
            if (prevChar >= 0L)
            {
                prevLevel = p.resultLevels[prevChar];
            }

            level succLevel = default;
            var lastType = types[length - 1L];
            if (lastType.@in(LRI, RLI, FSI))
            {
                succLevel = p.embeddingLevel;
            }
            else
            { 
                // the first character after the end of run sequence
                var limit = indexes[length - 1L] + 1L;
                while (limit < p.Len() && isRemovedByX9(p.initialTypes[limit]))
                {
                    limit++;
                }

                succLevel = p.embeddingLevel;
                if (limit < p.Len())
                {
                    succLevel = p.resultLevels[limit];
                }

            }

            var level = p.resultLevels[indexes[0L]];
            return addr(new isolatingRunSequence(p:p,indexes:indexes,types:types,level:level,sos:typeForLevel(maxLevel(prevLevel,level)),eos:typeForLevel(maxLevel(succLevel,level)),));

        }

        // Resolving weak types Rules W1-W7.
        //
        // Note that some weak types (EN, AN) remain after this processing is
        // complete.
        private static void resolveWeakTypes(this ptr<isolatingRunSequence> _addr_s)
        {
            ref isolatingRunSequence s = ref _addr_s.val;

            // on entry, only these types remain
            s.assertOnly(L, R, AL, EN, ES, ET, AN, CS, B, S, WS, ON, NSM, LRI, RLI, FSI, PDI); 

            // Rule W1.
            // Changes all NSMs.
            var precedingCharacterType = s.sos;
            {
                var i__prev1 = i;
                var t__prev1 = t;

                foreach (var (__i, __t) in s.types)
                {
                    i = __i;
                    t = __t;
                    if (t == NSM)
                    {
                        s.types[i] = precedingCharacterType;
                    }
                    else
                    {
                        if (t.@in(LRI, RLI, FSI, PDI))
                        {
                            precedingCharacterType = ON;
                        }

                        precedingCharacterType = t;

                    }

                } 

                // Rule W2.
                // EN does not change at the start of the run, because sos != AL.

                i = i__prev1;
                t = t__prev1;
            }

            {
                var i__prev1 = i;
                var t__prev1 = t;

                foreach (var (__i, __t) in s.types)
                {
                    i = __i;
                    t = __t;
                    if (t == EN)
                    {
                        {
                            var j__prev2 = j;

                            for (var j = i - 1L; j >= 0L; j--)
                            {
                                {
                                    var t__prev2 = t;

                                    var t = s.types[j];

                                    if (t.@in(L, R, AL))
                                    {
                                        if (t == AL)
                                        {
                                            s.types[i] = AN;
                                        }

                                        break;

                                    }

                                    t = t__prev2;

                                }

                            }


                            j = j__prev2;
                        }

                    }

                } 

                // Rule W3.

                i = i__prev1;
                t = t__prev1;
            }

            {
                var i__prev1 = i;
                var t__prev1 = t;

                foreach (var (__i, __t) in s.types)
                {
                    i = __i;
                    t = __t;
                    if (t == AL)
                    {
                        s.types[i] = R;
                    }

                } 

                // Rule W4.
                // Since there must be values on both sides for this rule to have an
                // effect, the scan skips the first and last value.
                //
                // Although the scan proceeds left to right, and changes the type
                // values in a way that would appear to affect the computations
                // later in the scan, there is actually no problem. A change in the
                // current value can only affect the value to its immediate right,
                // and only affect it if it is ES or CS. But the current value can
                // only change if the value to its right is not ES or CS. Thus
                // either the current value will not change, or its change will have
                // no effect on the remainder of the analysis.

                i = i__prev1;
                t = t__prev1;
            }

            {
                var i__prev1 = i;

                for (long i = 1L; i < s.Len() - 1L; i++)
                {
                    t = s.types[i];
                    if (t == ES || t == CS)
                    {
                        var prevSepType = s.types[i - 1L];
                        var succSepType = s.types[i + 1L];
                        if (prevSepType == EN && succSepType == EN)
                        {
                            s.types[i] = EN;
                        }
                        else if (s.types[i] == CS && prevSepType == AN && succSepType == AN)
                        {
                            s.types[i] = AN;
                        }

                    }

                } 

                // Rule W5.


                i = i__prev1;
            } 

            // Rule W5.
            {
                var i__prev1 = i;
                var t__prev1 = t;

                foreach (var (__i, __t) in s.types)
                {
                    i = __i;
                    t = __t;
                    if (t == ET)
                    { 
                        // locate end of sequence
                        var runStart = i;
                        var runEnd = s.findRunLimit(runStart, ET); 

                        // check values at ends of sequence
                        t = s.sos;
                        if (runStart > 0L)
                        {
                            t = s.types[runStart - 1L];
                        }

                        if (t != EN)
                        {
                            t = s.eos;
                            if (runEnd < len(s.types))
                            {
                                t = s.types[runEnd];
                            }

                        }

                        if (t == EN)
                        {
                            setTypes(s.types[runStart..runEnd], EN);
                        } 
                        // continue at end of sequence
                        i = runEnd;

                    }

                } 

                // Rule W6.

                i = i__prev1;
                t = t__prev1;
            }

            {
                var i__prev1 = i;
                var t__prev1 = t;

                foreach (var (__i, __t) in s.types)
                {
                    i = __i;
                    t = __t;
                    if (t.@in(ES, ET, CS))
                    {
                        s.types[i] = ON;
                    }

                } 

                // Rule W7.

                i = i__prev1;
                t = t__prev1;
            }

            {
                var i__prev1 = i;
                var t__prev1 = t;

                foreach (var (__i, __t) in s.types)
                {
                    i = __i;
                    t = __t;
                    if (t == EN)
                    { 
                        // set default if we reach start of run
                        var prevStrongType = s.sos;
                        {
                            var j__prev2 = j;

                            for (j = i - 1L; j >= 0L; j--)
                            {
                                t = s.types[j];
                                if (t == L || t == R)
                                { // AL's have been changed to R
                                    prevStrongType = t;
                                    break;

                                }

                            }


                            j = j__prev2;
                        }
                        if (prevStrongType == L)
                        {
                            s.types[i] = L;
                        }

                    }

                }

                i = i__prev1;
                t = t__prev1;
            }
        }

        // 6) resolving neutral types Rules N1-N2.
        private static void resolveNeutralTypes(this ptr<isolatingRunSequence> _addr_s)
        {
            ref isolatingRunSequence s = ref _addr_s.val;

            // on entry, only these types can be in resultTypes
            s.assertOnly(L, R, EN, AN, B, S, WS, ON, RLI, LRI, FSI, PDI);

            foreach (var (i, t) in s.types)
            {

                if (t == WS || t == ON || t == B || t == S || t == RLI || t == LRI || t == FSI || t == PDI) 
                    // find bounds of run of neutrals
                    var runStart = i;
                    var runEnd = s.findRunLimit(runStart, B, S, WS, ON, RLI, LRI, FSI, PDI); 

                    // determine effective types at ends of run
                    Class leadType = default;                    Class trailType = default; 

                    // Note that the character found can only be L, R, AN, or
                    // EN.
 

                    // Note that the character found can only be L, R, AN, or
                    // EN.
                    if (runStart == 0L)
                    {
                        leadType = s.sos;
                    }
                    else
                    {
                        leadType = s.types[runStart - 1L];
                        if (leadType.@in(AN, EN))
                        {
                            leadType = R;
                        }

                    }

                    if (runEnd == len(s.types))
                    {
                        trailType = s.eos;
                    }
                    else
                    {
                        trailType = s.types[runEnd];
                        if (trailType.@in(AN, EN))
                        {
                            trailType = R;
                        }

                    }

                    Class resolvedType = default;
                    if (leadType == trailType)
                    { 
                        // Rule N1.
                        resolvedType = leadType;

                    }
                    else
                    { 
                        // Rule N2.
                        // Notice the embedding level of the run is used, not
                        // the paragraph embedding level.
                        resolvedType = typeForLevel(s.level);

                    }

                    setTypes(s.types[runStart..runEnd], resolvedType); 

                    // skip over run of (former) neutrals
                    i = runEnd;
                
            }

        }

        private static void setLevels(slice<level> levels, level newLevel)
        {
            foreach (var (i) in levels)
            {
                levels[i] = newLevel;
            }

        }

        private static void setTypes(slice<Class> types, Class newType)
        {
            foreach (var (i) in types)
            {
                types[i] = newType;
            }

        }

        // 7) resolving implicit embedding levels Rules I1, I2.
        private static void resolveImplicitLevels(this ptr<isolatingRunSequence> _addr_s)
        {
            ref isolatingRunSequence s = ref _addr_s.val;

            // on entry, only these types can be in resultTypes
            s.assertOnly(L, R, EN, AN);

            s.resolvedLevels = make_slice<level>(len(s.types));
            setLevels(s.resolvedLevels, s.level);

            if ((s.level & 1L) == 0L)
            { // even level
                {
                    var i__prev1 = i;
                    var t__prev1 = t;

                    foreach (var (__i, __t) in s.types)
                    {
                        i = __i;
                        t = __t; 
                        // Rule I1.
                        if (t == L)
                        { 
                            // no change
                        }
                        else if (t == R)
                        {
                            s.resolvedLevels[i] += 1L;
                        }
                        else
                        { // t == AN || t == EN
                            s.resolvedLevels[i] += 2L;

                        }

                    }
            else

                    i = i__prev1;
                    t = t__prev1;
                }
            }            { // odd level
                {
                    var i__prev1 = i;
                    var t__prev1 = t;

                    foreach (var (__i, __t) in s.types)
                    {
                        i = __i;
                        t = __t; 
                        // Rule I2.
                        if (t == R)
                        { 
                            // no change
                        }
                        else
                        { // t == L || t == AN || t == EN
                            s.resolvedLevels[i] += 1L;

                        }

                    }

                    i = i__prev1;
                    t = t__prev1;
                }
            }

        }

        // Applies the levels and types resolved in rules W1-I2 to the
        // resultLevels array.
        private static void applyLevelsAndTypes(this ptr<isolatingRunSequence> _addr_s)
        {
            ref isolatingRunSequence s = ref _addr_s.val;

            foreach (var (i, x) in s.indexes)
            {
                s.p.resultTypes[x] = s.types[i];
                s.p.resultLevels[x] = s.resolvedLevels[i];
            }

        }

        // Return the limit of the run consisting only of the types in validSet
        // starting at index. This checks the value at index, and will return
        // index if that value is not in validSet.
        private static long findRunLimit(this ptr<isolatingRunSequence> _addr_s, long index, params Class[] validSet)
        {
            validSet = validSet.Clone();
            ref isolatingRunSequence s = ref _addr_s.val;

loop:
            while (index < len(s.types))
            {
                var t = s.types[index];
                foreach (var (_, valid) in validSet)
                {
                    if (t == valid)
                    {
                        _continueloop = true;
                        break;
                    }

                index++;
                }
                return index; // didn't find a match in validSet
            }
            return len(s.types);

        }

        // Algorithm validation. Assert that all values in types are in the
        // provided set.
        private static void assertOnly(this ptr<isolatingRunSequence> _addr_s, params Class[] codes)
        {
            codes = codes.Clone();
            ref isolatingRunSequence s = ref _addr_s.val;

loop:
            foreach (var (i, t) in s.types)
            {
                foreach (var (_, c) in codes)
                {
                    if (t == c)
                    {
                        _continueloop = true;
                        break;
                    }

                }
                log.Panicf("invalid bidi code %v present in assertOnly at position %d", t, s.indexes[i]);

            }

        }

        // determineLevelRuns returns an array of level runs. Each level run is
        // described as an array of indexes into the input string.
        //
        // Determines the level runs. Rule X9 will be applied in determining the
        // runs, in the way that makes sure the characters that are supposed to be
        // removed are not included in the runs.
        private static slice<slice<long>> determineLevelRuns(this ptr<paragraph> _addr_p)
        {
            ref paragraph p = ref _addr_p.val;

            long run = new slice<long>(new long[] {  });
            slice<long> allRuns = new slice<slice<long>>(new slice<long>[] {  });
            var currentLevel = implicitLevel;

            foreach (var (i) in p.initialTypes)
            {
                if (!isRemovedByX9(p.initialTypes[i]))
                {
                    if (p.resultLevels[i] != currentLevel)
                    { 
                        // we just encountered a new run; wrap up last run
                        if (currentLevel >= 0L)
                        { // only wrap it up if there was a run
                            allRuns = append(allRuns, run);
                            run = null;

                        } 
                        // Start new run
                        currentLevel = p.resultLevels[i];

                    }

                    run = append(run, i);

                }

            } 
            // Wrap up the final run, if any
            if (len(run) > 0L)
            {
                allRuns = append(allRuns, run);
            }

            return allRuns;

        }

        // Definition BD13. Determine isolating run sequences.
        private static slice<ptr<isolatingRunSequence>> determineIsolatingRunSequences(this ptr<paragraph> _addr_p)
        {
            ref paragraph p = ref _addr_p.val;

            var levelRuns = p.determineLevelRuns(); 

            // Compute the run that each character belongs to
            var runForCharacter = make_slice<long>(p.Len());
            {
                var run__prev1 = run;

                foreach (var (__i, __run) in levelRuns)
                {
                    i = __i;
                    run = __run;
                    foreach (var (_, index) in run)
                    {
                        runForCharacter[index] = i;
                    }

                }

                run = run__prev1;
            }

            ptr<isolatingRunSequence> sequences = new slice<ptr<isolatingRunSequence>>(new ptr<isolatingRunSequence>[] {  });

            slice<long> currentRunSequence = default;

            {
                var run__prev1 = run;

                foreach (var (_, __run) in levelRuns)
                {
                    run = __run;
                    var first = run[0L];
                    if (p.initialTypes[first] != PDI || p.matchingIsolateInitiator[first] == -1L)
                    {
                        currentRunSequence = null; 
                        // int run = i;
                        while (true)
                        { 
                            // Copy this level run into currentRunSequence
                            currentRunSequence = append(currentRunSequence, run);

                            var last = currentRunSequence[len(currentRunSequence) - 1L];
                            var lastT = p.initialTypes[last];
                            if (lastT.@in(LRI, RLI, FSI) && p.matchingPDI[last] != p.Len())
                            {
                                run = levelRuns[runForCharacter[p.matchingPDI[last]]];
                            }
                            else
                            {
                                break;
                            }

                        }

                        sequences = append(sequences, p.isolatingRunSequence(currentRunSequence));

                    }

                }

                run = run__prev1;
            }

            return sequences;

        }

        // Assign level information to characters removed by rule X9. This is for
        // ease of relating the level information to the original input data. Note
        // that the levels assigned to these codes are arbitrary, they're chosen so
        // as to avoid breaking level runs.
        private static void assignLevelsToCharactersRemovedByX9(this ptr<paragraph> _addr_p)
        {
            ref paragraph p = ref _addr_p.val;

            {
                var i__prev1 = i;

                foreach (var (__i, __t) in p.initialTypes)
                {
                    i = __i;
                    t = __t;
                    if (t.@in(LRE, RLE, LRO, RLO, PDF, BN))
                    {
                        p.resultTypes[i] = t;
                        p.resultLevels[i] = -1L;
                    }

                } 
                // now propagate forward the levels information (could have
                // propagated backward, the main thing is not to introduce a level
                // break where one doesn't already exist).

                i = i__prev1;
            }

            if (p.resultLevels[0L] == -1L)
            {
                p.resultLevels[0L] = p.embeddingLevel;
            }

            {
                var i__prev1 = i;

                for (long i = 1L; i < len(p.initialTypes); i++)
                {
                    if (p.resultLevels[i] == -1L)
                    {
                        p.resultLevels[i] = p.resultLevels[i - 1L];
                    }

                } 
                // Embedding information is for informational purposes only so need not be
                // adjusted.


                i = i__prev1;
            } 
            // Embedding information is for informational purposes only so need not be
            // adjusted.
        }

        //
        // Output
        //

        // getLevels computes levels array breaking lines at offsets in linebreaks.
        // Rule L1.
        //
        // The linebreaks array must include at least one value. The values must be
        // in strictly increasing order (no duplicates) between 1 and the length of
        // the text, inclusive. The last value must be the length of the text.
        private static slice<level> getLevels(this ptr<paragraph> _addr_p, slice<long> linebreaks)
        {
            ref paragraph p = ref _addr_p.val;
 
            // Note that since the previous processing has removed all
            // P, S, and WS values from resultTypes, the values referred to
            // in these rules are the initial types, before any processing
            // has been applied (including processing of overrides).
            //
            // This example implementation has reinserted explicit format codes
            // and BN, in order that the levels array correspond to the
            // initial text. Their final placement is not normative.
            // These codes are treated like WS in this implementation,
            // so they don't interrupt sequences of WS.

            validateLineBreaks(linebreaks, p.Len());

            var result = append((slice<level>)null, p.resultLevels); 

            // don't worry about linebreaks since if there is a break within
            // a series of WS values preceding S, the linebreak itself
            // causes the reset.
            foreach (var (i, t) in p.initialTypes)
            {
                if (t.@in(B, S))
                { 
                    // Rule L1, clauses one and two.
                    result[i] = p.embeddingLevel; 

                    // Rule L1, clause three.
                    {
                        var j__prev2 = j;

                        for (var j = i - 1L; j >= 0L; j--)
                        {
                            if (isWhitespace(p.initialTypes[j]))
                            { // including format codes
                                result[j] = p.embeddingLevel;

                            }
                            else
                            {
                                break;
                            }

                        }


                        j = j__prev2;
                    }

                }

            } 

            // Rule L1, clause four.
            long start = 0L;
            foreach (var (_, limit) in linebreaks)
            {
                {
                    var j__prev2 = j;

                    for (j = limit - 1L; j >= start; j--)
                    {
                        if (isWhitespace(p.initialTypes[j]))
                        { // including format codes
                            result[j] = p.embeddingLevel;

                        }
                        else
                        {
                            break;
                        }

                    }


                    j = j__prev2;
                }
                start = limit;

            }
            return result;

        }

        // getReordering returns the reordering of lines from a visual index to a
        // logical index for line breaks at the given offsets.
        //
        // Lines are concatenated from left to right. So for example, the fifth
        // character from the left on the third line is
        //
        //         getReordering(linebreaks)[linebreaks[1] + 4]
        //
        // (linebreaks[1] is the position after the last character of the second
        // line, which is also the index of the first character on the third line,
        // and adding four gets the fifth character from the left).
        //
        // The linebreaks array must include at least one value. The values must be
        // in strictly increasing order (no duplicates) between 1 and the length of
        // the text, inclusive. The last value must be the length of the text.
        private static slice<long> getReordering(this ptr<paragraph> _addr_p, slice<long> linebreaks)
        {
            ref paragraph p = ref _addr_p.val;

            validateLineBreaks(linebreaks, p.Len());

            return computeMultilineReordering(p.getLevels(linebreaks), linebreaks);
        }

        // Return multiline reordering array for a given level array. Reordering
        // does not occur across a line break.
        private static slice<long> computeMultilineReordering(slice<level> levels, slice<long> linebreaks)
        {
            var result = make_slice<long>(len(levels));

            long start = 0L;
            foreach (var (_, limit) in linebreaks)
            {
                var tempLevels = make_slice<level>(limit - start);
                copy(tempLevels, levels[start..]);

                foreach (var (j, order) in computeReordering(tempLevels))
                {
                    result[start + j] = order + start;
                }
                start = limit;

            }
            return result;

        }

        // Return reordering array for a given level array. This reorders a single
        // line. The reordering is a visual to logical map. For example, the
        // leftmost char is string.charAt(order[0]). Rule L2.
        private static slice<long> computeReordering(slice<level> levels)
        {
            var result = make_slice<long>(len(levels)); 
            // initialize order
            {
                var i__prev1 = i;

                foreach (var (__i) in result)
                {
                    i = __i;
                    result[i] = i;
                } 

                // locate highest level found on line.
                // Note the rules say text, but no reordering across line bounds is
                // performed, so this is sufficient.

                i = i__prev1;
            }

            var highestLevel = level(0L);
            var lowestOddLevel = level(maxDepth + 2L);
            {
                var level__prev1 = level;

                foreach (var (_, __level) in levels)
                {
                    level = __level;
                    if (level > highestLevel)
                    {
                        highestLevel = level;
                    }

                    if (level & 1L != 0L && level < lowestOddLevel)
                    {
                        lowestOddLevel = level;
                    }

                }

                level = level__prev1;
            }

            {
                var level__prev1 = level;

                for (var level = highestLevel; level >= lowestOddLevel; level--)
                {
                    {
                        var i__prev2 = i;

                        for (long i = 0L; i < len(levels); i++)
                        {
                            if (levels[i] >= level)
                            { 
                                // find range of text at or above this level
                                var start = i;
                                var limit = i + 1L;
                                while (limit < len(levels) && levels[limit] >= level)
                                {
                                    limit++;
                                }


                                {
                                    var j = start;
                                    var k = limit - 1L;

                                    while (j < k)
                                    {
                                        result[j] = result[k];
                                        result[k] = result[j];
                                        j = j + 1L;
                                    k = k - 1L;
                                    } 
                                    // skip to end of level run

                                } 
                                // skip to end of level run
                                i = limit;

                            }

                        }


                        i = i__prev2;
                    }

                }


                level = level__prev1;
            }

            return result;

        }

        // isWhitespace reports whether the type is considered a whitespace type for the
        // line break rules.
        private static bool isWhitespace(Class c)
        {

            if (c == LRE || c == RLE || c == LRO || c == RLO || c == PDF || c == LRI || c == RLI || c == FSI || c == PDI || c == BN || c == WS) 
                return true;
                        return false;

        }

        // isRemovedByX9 reports whether the type is one of the types removed in X9.
        private static bool isRemovedByX9(Class c)
        {

            if (c == LRE || c == RLE || c == LRO || c == RLO || c == PDF || c == BN) 
                return true;
                        return false;

        }

        // typeForLevel reports the strong type (L or R) corresponding to the level.
        private static Class typeForLevel(level level)
        {
            if ((level & 0x1UL) == 0L)
            {
                return L;
            }

            return R;

        }

        // TODO: change validation to not panic

        private static void validateTypes(slice<Class> types)
        {
            if (len(types) == 0L)
            {
                log.Panic("types is null");
            }

            foreach (var (i, t) in types[..len(types) - 1L])
            {
                if (t == B)
                {
                    log.Panicf("B type before end of paragraph at index: %d", i);
                }

            }

        }

        private static void validateParagraphEmbeddingLevel(level embeddingLevel)
        {
            if (embeddingLevel != implicitLevel && embeddingLevel != 0L && embeddingLevel != 1L)
            {
                log.Panicf("illegal paragraph embedding level: %d", embeddingLevel);
            }

        }

        private static void validateLineBreaks(slice<long> linebreaks, long textLength)
        {
            long prev = 0L;
            foreach (var (i, next) in linebreaks)
            {
                if (next <= prev)
                {
                    log.Panicf("bad linebreak: %d at index: %d", next, i);
                }

                prev = next;

            }
            if (prev != textLength)
            {
                log.Panicf("last linebreak was %d, want %d", prev, textLength);
            }

        }

        private static void validatePbTypes(slice<bracketType> pairTypes)
        {
            if (len(pairTypes) == 0L)
            {
                log.Panic("pairTypes is null");
            }

            foreach (var (i, pt) in pairTypes)
            {

                if (pt == bpNone || pt == bpOpen || pt == bpClose)                 else 
                    log.Panicf("illegal pairType value at %d: %v", i, pairTypes[i]);
                
            }

        }

        private static void validatePbValues(slice<int> pairValues, slice<bracketType> pairTypes)
        {
            if (pairValues == null)
            {
                log.Panic("pairValues is null");
            }

            if (len(pairTypes) != len(pairValues))
            {
                log.Panic("pairTypes is different length from pairValues");
            }

        }
    }
}}}}}}
