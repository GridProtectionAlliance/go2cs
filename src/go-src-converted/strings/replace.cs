// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package strings -- go2cs converted at 2020 August 29 08:42:41 UTC
// import "strings" ==> using strings = go.strings_package
// Original source: C:\Go\src\strings\replace.go
using io = go.io_package;
using static go.builtin;

namespace go
{
    public static partial class strings_package
    {
        // Replacer replaces a list of strings with replacements.
        // It is safe for concurrent use by multiple goroutines.
        public partial struct Replacer
        {
            public replacer r;
        }

        // replacer is the interface that a replacement algorithm needs to implement.
        private partial interface replacer
        {
            (long, error) Replace(@string s);
            (long, error) WriteString(io.Writer w, @string s);
        }

        // NewReplacer returns a new Replacer from a list of old, new string pairs.
        // Replacements are performed in order, without overlapping matches.
        public static ref Replacer NewReplacer(params @string[] oldnew) => func((_, panic, __) =>
        {
            oldnew = oldnew.Clone();

            if (len(oldnew) % 2L == 1L)
            {
                panic("strings.NewReplacer: odd argument count");
            }
            if (len(oldnew) == 2L && len(oldnew[0L]) > 1L)
            {
                return ref new Replacer(r:makeSingleStringReplacer(oldnew[0],oldnew[1]));
            }
            var allNewBytes = true;
            {
                long i__prev1 = i;

                long i = 0L;

                while (i < len(oldnew))
                {
                    if (len(oldnew[i]) != 1L)
                    {
                        return ref new Replacer(r:makeGenericReplacer(oldnew));
                    i += 2L;
                    }
                    if (len(oldnew[i + 1L]) != 1L)
                    {
                        allNewBytes = false;
                    }
                }


                i = i__prev1;
            }

            if (allNewBytes)
            {
                byteReplacer r = new byteReplacer();
                {
                    long i__prev1 = i;

                    foreach (var (__i) in r)
                    {
                        i = __i;
                        r[i] = byte(i);
                    } 
                    // The first occurrence of old->new map takes precedence
                    // over the others with the same old string.

                    i = i__prev1;
                }

                {
                    long i__prev1 = i;

                    i = len(oldnew) - 2L;

                    while (i >= 0L)
                    {
                        var o = oldnew[i][0L];
                        var n = oldnew[i + 1L][0L];
                        r[o] = n;
                        i -= 2L;
                    }


                    i = i__prev1;
                }
                return ref new Replacer(r:&r);
            }
            r = new byteStringReplacer(); 
            // The first occurrence of old->new map takes precedence
            // over the others with the same old string.
            {
                long i__prev1 = i;

                i = len(oldnew) - 2L;

                while (i >= 0L)
                {
                    o = oldnew[i][0L];
                    n = oldnew[i + 1L];
                    r[o] = (slice<byte>)n;
                    i -= 2L;
                }


                i = i__prev1;
            }
            return ref new Replacer(r:&r);
        });

        // Replace returns a copy of s with all replacements performed.
        private static @string Replace(this ref Replacer r, @string s)
        {
            return r.r.Replace(s);
        }

        // WriteString writes s to w with all replacements performed.
        private static (long, error) WriteString(this ref Replacer r, io.Writer w, @string s)
        {
            return r.r.WriteString(w, s);
        }

        // trieNode is a node in a lookup trie for prioritized key/value pairs. Keys
        // and values may be empty. For example, the trie containing keys "ax", "ay",
        // "bcbc", "x" and "xy" could have eight nodes:
        //
        //  n0  -
        //  n1  a-
        //  n2  .x+
        //  n3  .y+
        //  n4  b-
        //  n5  .cbc+
        //  n6  x+
        //  n7  .y+
        //
        // n0 is the root node, and its children are n1, n4 and n6; n1's children are
        // n2 and n3; n4's child is n5; n6's child is n7. Nodes n0, n1 and n4 (marked
        // with a trailing "-") are partial keys, and nodes n2, n3, n5, n6 and n7
        // (marked with a trailing "+") are complete keys.
        private partial struct trieNode
        {
            public @string value; // priority is the priority (higher is more important) of the trie node's
// key/value pair; keys are not necessarily matched shortest- or longest-
// first. Priority is positive if this node is a complete key, and zero
// otherwise. In the example above, positive/zero priorities are marked
// with a trailing "+" or "-".
            public long priority; // A trie node may have zero, one or more child nodes:
//  * if the remaining fields are zero, there are no children.
//  * if prefix and next are non-zero, there is one child in next.
//  * if table is non-zero, it defines all the children.
//
// Prefixes are preferred over tables when there is one child, but the
// root node always uses a table for lookup efficiency.

// prefix is the difference in keys between this trie node and the next.
// In the example above, node n4 has prefix "cbc" and n4's next node is n5.
// Node n5 has no children and so has zero prefix, next and table fields.
            public @string prefix;
            public ptr<trieNode> next; // table is a lookup table indexed by the next byte in the key, after
// remapping that byte through genericReplacer.mapping to create a dense
// index. In the example above, the keys only use 'a', 'b', 'c', 'x' and
// 'y', which remap to 0, 1, 2, 3 and 4. All other bytes remap to 5, and
// genericReplacer.tableSize will be 5. Node n0's table will be
// []*trieNode{ 0:n1, 1:n4, 3:n6 }, where the 0, 1 and 3 are the remapped
// 'a', 'b' and 'x'.
            public slice<ref trieNode> table;
        }

        private static void add(this ref trieNode t, @string key, @string val, long priority, ref genericReplacer r)
        {
            if (key == "")
            {
                if (t.priority == 0L)
                {
                    t.value = val;
                    t.priority = priority;
                }
                return;
            }
            if (t.prefix != "")
            { 
                // Need to split the prefix among multiple nodes.
                long n = default; // length of the longest common prefix
                while (n < len(t.prefix) && n < len(key))
                {
                    if (t.prefix[n] != key[n])
                    {
                        break;
                    n++;
                    }
                }

                if (n == len(t.prefix))
                {
                    t.next.add(key[n..], val, priority, r);
                }
                else if (n == 0L)
                { 
                    // First byte differs, start a new lookup table here. Looking up
                    // what is currently t.prefix[0] will lead to prefixNode, and
                    // looking up key[0] will lead to keyNode.
                    ref trieNode prefixNode = default;
                    if (len(t.prefix) == 1L)
                    {
                        prefixNode = t.next;
                    }
                    else
                    {
                        prefixNode = ref new trieNode(prefix:t.prefix[1:],next:t.next,);
                    }
                    ptr<trieNode> keyNode = @new<trieNode>();
                    t.table = make_slice<ref trieNode>(r.tableSize);
                    t.table[r.mapping[t.prefix[0L]]] = prefixNode;
                    t.table[r.mapping[key[0L]]] = keyNode;
                    t.prefix = "";
                    t.next = null;
                    keyNode.add(key[1L..], val, priority, r);
                }
                else
                { 
                    // Insert new node after the common section of the prefix.
                    trieNode next = ref new trieNode(prefix:t.prefix[n:],next:t.next,);
                    t.prefix = t.prefix[..n];
                    t.next = next;
                    next.add(key[n..], val, priority, r);
                }
            }
            else if (t.table != null)
            { 
                // Insert into existing table.
                var m = r.mapping[key[0L]];
                if (t.table[m] == null)
                {
                    t.table[m] = @new<trieNode>();
                }
                t.table[m].add(key[1L..], val, priority, r);
            }
            else
            {
                t.prefix = key;
                t.next = @new<trieNode>();
                t.next.add("", val, priority, r);
            }
        }

        private static (@string, long, bool) lookup(this ref genericReplacer r, @string s, bool ignoreRoot)
        { 
            // Iterate down the trie to the end, and grab the value and keylen with
            // the highest priority.
            long bestPriority = 0L;
            var node = ref r.root;
            long n = 0L;
            while (node != null)
            {
                if (node.priority > bestPriority && !(ignoreRoot && node == ref r.root))
                {
                    bestPriority = node.priority;
                    val = node.value;
                    keylen = n;
                    found = true;
                }
                if (s == "")
                {
                    break;
                }
                if (node.table != null)
                {
                    var index = r.mapping[s[0L]];
                    if (int(index) == r.tableSize)
                    {
                        break;
                    }
                    node = node.table[index];
                    s = s[1L..];
                    n++;
                }
                else if (node.prefix != "" && HasPrefix(s, node.prefix))
                {
                    n += len(node.prefix);
                    s = s[len(node.prefix)..];
                    node = node.next;
                }
                else
                {
                    break;
                }
            }

            return;
        }

        // genericReplacer is the fully generic algorithm.
        // It's used as a fallback when nothing faster can be used.
        private partial struct genericReplacer
        {
            public trieNode root; // tableSize is the size of a trie node's lookup table. It is the number
// of unique key bytes.
            public long tableSize; // mapping maps from key bytes to a dense index for trieNode.table.
            public array<byte> mapping;
        }

        private static ref genericReplacer makeGenericReplacer(slice<@string> oldnew)
        {
            ptr<genericReplacer> r = @new<genericReplacer>(); 
            // Find each byte used, then assign them each an index.
            {
                long i__prev1 = i;

                long i = 0L;

                while (i < len(oldnew))
                {
                    var key = oldnew[i];
                    for (long j = 0L; j < len(key); j++)
                    {
                        r.mapping[key[j]] = 1L;
                    }

                    i += 2L;
                }


                i = i__prev1;
            }

            {
                var b__prev1 = b;

                foreach (var (_, __b) in r.mapping)
                {
                    b = __b;
                    r.tableSize += int(b);
                }

                b = b__prev1;
            }

            byte index = default;
            {
                long i__prev1 = i;
                var b__prev1 = b;

                foreach (var (__i, __b) in r.mapping)
                {
                    i = __i;
                    b = __b;
                    if (b == 0L)
                    {
                        r.mapping[i] = byte(r.tableSize);
                    }
                    else
                    {
                        r.mapping[i] = index;
                        index++;
                    }
                } 
                // Ensure root node uses a lookup table (for performance).

                i = i__prev1;
                b = b__prev1;
            }

            r.root.table = make_slice<ref trieNode>(r.tableSize);

            {
                long i__prev1 = i;

                i = 0L;

                while (i < len(oldnew))
                {
                    r.root.add(oldnew[i], oldnew[i + 1L], len(oldnew) - i, r);
                    i += 2L;
                }


                i = i__prev1;
            }
            return r;
        }

        private partial struct appendSliceWriter // : slice<byte>
        {
        }

        // Write writes to the buffer to satisfy io.Writer.
        private static (long, error) Write(this ref appendSliceWriter w, slice<byte> p)
        {
            w.Value = append(w.Value, p);
            return (len(p), null);
        }

        // WriteString writes to the buffer without string->[]byte->string allocations.
        private static (long, error) WriteString(this ref appendSliceWriter w, @string s)
        {
            w.Value = append(w.Value, s);
            return (len(s), null);
        }

        private partial interface stringWriterIface
        {
            (long, error) WriteString(@string _p0);
        }

        private partial struct stringWriter
        {
            public io.Writer w;
        }

        private static (long, error) WriteString(this stringWriter w, @string s)
        {
            return w.w.Write((slice<byte>)s);
        }

        private static stringWriterIface getStringWriter(io.Writer w)
        {
            stringWriterIface (sw, ok) = w._<stringWriterIface>();
            if (!ok)
            {
                sw = new stringWriter(w);
            }
            return sw;
        }

        private static @string Replace(this ref genericReplacer r, @string s)
        {
            var buf = make(appendSliceWriter, 0L, len(s));
            r.WriteString(ref buf, s);
            return string(buf);
        }

        private static (long, error) WriteString(this ref genericReplacer r, io.Writer w, @string s)
        {
            var sw = getStringWriter(w);
            long last = default;            long wn = default;

            bool prevMatchEmpty = default;
            {
                long i = 0L;

                while (i <= len(s))
                { 
                    // Fast path: s[i] is not a prefix of any pattern.
                    if (i != len(s) && r.root.priority == 0L)
                    {
                        var index = int(r.mapping[s[i]]);
                        if (index == r.tableSize || r.root.table[index] == null)
                        {
                            i++;
                            continue;
                        }
                    } 

                    // Ignore the empty match iff the previous loop found the empty match.
                    var (val, keylen, match) = r.lookup(s[i..], prevMatchEmpty);
                    prevMatchEmpty = match && keylen == 0L;
                    if (match)
                    {
                        wn, err = sw.WriteString(s[last..i]);
                        n += wn;
                        if (err != null)
                        {
                            return;
                        }
                        wn, err = sw.WriteString(val);
                        n += wn;
                        if (err != null)
                        {
                            return;
                        }
                        i += keylen;
                        last = i;
                        continue;
                    }
                    i++;
                }

            }
            if (last != len(s))
            {
                wn, err = sw.WriteString(s[last..]);
                n += wn;
            }
            return;
        }

        // singleStringReplacer is the implementation that's used when there is only
        // one string to replace (and that string has more than one byte).
        private partial struct singleStringReplacer
        {
            public ptr<stringFinder> finder; // value is the new string that replaces that pattern when it's found.
            public @string value;
        }

        private static ref singleStringReplacer makeSingleStringReplacer(@string pattern, @string value)
        {
            return ref new singleStringReplacer(finder:makeStringFinder(pattern),value:value);
        }

        private static @string Replace(this ref singleStringReplacer r, @string s)
        {
            slice<byte> buf = default;
            long i = 0L;
            var matched = false;
            while (true)
            {
                var match = r.finder.next(s[i..]);
                if (match == -1L)
                {
                    break;
                }
                matched = true;
                buf = append(buf, s[i..i + match]);
                buf = append(buf, r.value);
                i += match + len(r.finder.pattern);
            }

            if (!matched)
            {
                return s;
            }
            buf = append(buf, s[i..]);
            return string(buf);
        }

        private static (long, error) WriteString(this ref singleStringReplacer r, io.Writer w, @string s)
        {
            var sw = getStringWriter(w);
            long i = default;            long wn = default;

            while (true)
            {
                var match = r.finder.next(s[i..]);
                if (match == -1L)
                {
                    break;
                }
                wn, err = sw.WriteString(s[i..i + match]);
                n += wn;
                if (err != null)
                {
                    return;
                }
                wn, err = sw.WriteString(r.value);
                n += wn;
                if (err != null)
                {
                    return;
                }
                i += match + len(r.finder.pattern);
            }

            wn, err = sw.WriteString(s[i..]);
            n += wn;
            return;
        }

        // byteReplacer is the implementation that's used when all the "old"
        // and "new" values are single ASCII bytes.
        // The array contains replacement bytes indexed by old byte.
        private partial struct byteReplacer // : array<byte>
        {
        }

        private static @string Replace(this ref byteReplacer r, @string s)
        {
            slice<byte> buf = default; // lazily allocated
            for (long i = 0L; i < len(s); i++)
            {
                var b = s[i];
                if (r[b] != b)
                {
                    if (buf == null)
                    {
                        buf = (slice<byte>)s;
                    }
                    buf[i] = r[b];
                }
            }

            if (buf == null)
            {
                return s;
            }
            return string(buf);
        }

        private static (long, error) WriteString(this ref byteReplacer r, io.Writer w, @string s)
        { 
            // TODO(bradfitz): use io.WriteString with slices of s, avoiding allocation.
            long bufsize = 32L << (int)(10L);
            if (len(s) < bufsize)
            {
                bufsize = len(s);
            }
            var buf = make_slice<byte>(bufsize);

            while (len(s) > 0L)
            {
                var ncopy = copy(buf, s[..]);
                s = s[ncopy..];
                foreach (var (i, b) in buf[..ncopy])
                {
                    buf[i] = r[b];
                }
                var (wn, err) = w.Write(buf[..ncopy]);
                n += wn;
                if (err != null)
                {
                    return (n, err);
                }
            }

            return (n, null);
        }

        // byteStringReplacer is the implementation that's used when all the
        // "old" values are single ASCII bytes but the "new" values vary in size.
        // The array contains replacement byte slices indexed by old byte.
        // A nil []byte means that the old byte should not be replaced.
        private partial struct byteStringReplacer // : array<slice<byte>>
        {
        }

        private static @string Replace(this ref byteStringReplacer r, @string s)
        {
            var newSize = len(s);
            var anyChanges = false;
            {
                long i__prev1 = i;

                for (long i = 0L; i < len(s); i++)
                {
                    var b = s[i];
                    if (r[b] != null)
                    {
                        anyChanges = true; 
                        // The -1 is because we are replacing 1 byte with len(r[b]) bytes.
                        newSize += len(r[b]) - 1L;
                    }
                }


                i = i__prev1;
            }
            if (!anyChanges)
            {
                return s;
            }
            var buf = make_slice<byte>(newSize);
            var bi = buf;
            {
                long i__prev1 = i;

                for (i = 0L; i < len(s); i++)
                {
                    b = s[i];
                    if (r[b] != null)
                    {
                        var n = copy(bi, r[b]);
                        bi = bi[n..];
                    }
                    else
                    {
                        bi[0L] = b;
                        bi = bi[1L..];
                    }
                }


                i = i__prev1;
            }
            return string(buf);
        }

        private static (long, error) WriteString(this ref byteStringReplacer r, io.Writer w, @string s)
        {
            var sw = getStringWriter(w);
            long last = 0L;
            for (long i = 0L; i < len(s); i++)
            {
                var b = s[i];
                if (r[b] == null)
                {
                    continue;
                }
                if (last != i)
                {
                    var (nw, err) = sw.WriteString(s[last..i]);
                    n += nw;
                    if (err != null)
                    {
                        return (n, err);
                    }
                }
                last = i + 1L;
                (nw, err) = w.Write(r[b]);
                n += nw;
                if (err != null)
                {
                    return (n, err);
                }
            }

            if (last != len(s))
            {
                long nw = default;
                nw, err = sw.WriteString(s[last..]);
                n += nw;
            }
            return;
        }
    }
}
