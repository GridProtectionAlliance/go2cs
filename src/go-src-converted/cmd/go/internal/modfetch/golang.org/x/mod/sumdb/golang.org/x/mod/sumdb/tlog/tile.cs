// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package tlog -- go2cs converted at 2020 October 08 04:36:20 UTC
// import "golang.org/x/mod/sumdb/tlog" ==> using tlog = go.golang.org.x.mod.sumdb.tlog_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\mod\sumdb\tlog\tile.go
using fmt = go.fmt_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using static go.builtin;

namespace go {
namespace golang.org {
namespace x {
namespace mod {
namespace sumdb
{
    public static partial class tlog_package
    {
        // A Tile is a description of a transparency log tile.
        // A tile of height H at level L offset N lists W consecutive hashes
        // at level H*L of the tree starting at offset N*(2**H).
        // A complete tile lists 2**H hashes; a partial tile lists fewer.
        // Note that a tile represents the entire subtree of height H
        // with those hashes as the leaves. The levels above H*L
        // can be reconstructed by hashing the leaves.
        //
        // Each Tile can be encoded as a “tile coordinate path”
        // of the form tile/H/L/NNN[.p/W].
        // The .p/W suffix is present only for partial tiles, meaning W < 2**H.
        // The NNN element is an encoding of N into 3-digit path elements.
        // All but the last path element begins with an "x".
        // For example,
        // Tile{H: 3, L: 4, N: 1234067, W: 1}'s path
        // is tile/3/4/x001/x234/067.p/1, and
        // Tile{H: 3, L: 4, N: 1234067, W: 8}'s path
        // is tile/3/4/x001/x234/067.
        // See Tile's Path method and the ParseTilePath function.
        //
        // The special level L=-1 holds raw record data instead of hashes.
        // In this case, the level encodes into a tile path as the path element
        // "data" instead of "-1".
        //
        // See also https://golang.org/design/25530-sumdb#checksum-database
        // and https://research.swtch.com/tlog#tiling_a_log.
        public partial struct Tile
        {
            public long H; // height of tile (1 ≤ H ≤ 30)
            public long L; // level in tiling (-1 ≤ L ≤ 63)
            public long N; // number within level (0 ≤ N, unbounded)
            public long W; // width of tile (1 ≤ W ≤ 2**H; 2**H is complete tile)
        }

        // TileForIndex returns the tile of fixed height h ≥ 1
        // and least width storing the given hash storage index.
        //
        // If h ≤ 0, TileForIndex panics.
        public static Tile TileForIndex(long h, long index) => func((_, panic, __) =>
        {
            if (h <= 0L)
            {
                panic(fmt.Sprintf("TileForIndex: invalid height %d", h));
            }
            var (t, _, _) = tileForIndex(h, index);
            return t;
        });

        // tileForIndex returns the tile of height h ≥ 1
        // storing the given hash index, which can be
        // reconstructed using tileHash(data[start:end]).
        private static (Tile, long, long) tileForIndex(long h, long index)
        {
            Tile t = default;
            long start = default;
            long end = default;

            var (level, n) = SplitStoredHashIndex(index);
            t.H = h;
            t.L = level / h;
            level -= t.L * h; // now level within tile
            t.N = n << (int)(uint(level)) >> (int)(uint(t.H));
            n -= t.N << (int)(uint(t.H)) >> (int)(uint(level)); // now n within tile at level
            t.W = int((n + 1L) << (int)(uint(level)));
            return (t, int(n << (int)(uint(level))) * HashSize, int((n + 1L) << (int)(uint(level))) * HashSize);
        }

        // HashFromTile returns the hash at the given storage index,
        // provided that t == TileForIndex(t.H, index) or a wider version,
        // and data is t's tile data (of length at least t.W*HashSize).
        public static (Hash, error) HashFromTile(Tile t, slice<byte> data, long index)
        {
            Hash _p0 = default;
            error _p0 = default!;

            if (t.H < 1L || t.H > 30L || t.L < 0L || t.L >= 64L || t.W < 1L || t.W > 1L << (int)(uint(t.H)))
            {
                return (new Hash(), error.As(fmt.Errorf("invalid tile %v", t.Path()))!);
            }
            if (len(data) < t.W * HashSize)
            {
                return (new Hash(), error.As(fmt.Errorf("data len %d too short for tile %v", len(data), t.Path()))!);
            }
            var (t1, start, end) = tileForIndex(t.H, index);
            if (t.L != t1.L || t.N != t1.N || t.W < t1.W)
            {
                return (new Hash(), error.As(fmt.Errorf("index %v is in %v not %v", index, t1.Path(), t.Path()))!);
            }
            return (tileHash(data[start..end]), error.As(null!)!);
        }

        // tileHash computes the subtree hash corresponding to the (2^K)-1 hashes in data.
        private static Hash tileHash(slice<byte> data) => func((_, panic, __) =>
        {
            if (len(data) == 0L)
            {
                panic("bad math in tileHash");
            }
            if (len(data) == HashSize)
            {
                Hash h = default;
                copy(h[..], data);
                return h;
            }
            var n = len(data) / 2L;
            return NodeHash(tileHash(data[..n]), tileHash(data[n..]));
        });

        // NewTiles returns the coordinates of the tiles of height h ≥ 1
        // that must be published when publishing from a tree of
        // size newTreeSize to replace a tree of size oldTreeSize.
        // (No tiles need to be published for a tree of size zero.)
        //
        // If h ≤ 0, TileForIndex panics.
        public static slice<Tile> NewTiles(long h, long oldTreeSize, long newTreeSize) => func((_, panic, __) =>
        {
            if (h <= 0L)
            {
                panic(fmt.Sprintf("NewTiles: invalid height %d", h));
            }
            var H = uint(h);
            slice<Tile> tiles = default;
            for (var level = uint(0L); newTreeSize >> (int)((H * level)) > 0L; level++)
            {
                var oldN = oldTreeSize >> (int)((H * level));
                var newN = newTreeSize >> (int)((H * level));
                {
                    var n__prev2 = n;

                    for (var n = oldN >> (int)(H); n < newN >> (int)(H); n++)
                    {
                        tiles = append(tiles, new Tile(H:h,L:int(level),N:n,W:1<<H));
                    }


                    n = n__prev2;
                }
                n = newN >> (int)(H);
                var maxW = int(newN - n << (int)(H));
                long minW = 1L;
                if (oldN > n << (int)(H))
                {
                    minW = int(oldN - n << (int)(H));
                }
                for (var w = minW; w <= maxW; w++)
                {
                    tiles = append(tiles, new Tile(H:h,L:int(level),N:n,W:w));
                }
            }

            return tiles;
        });

        // ReadTileData reads the hashes for tile t from r
        // and returns the corresponding tile data.
        public static (slice<byte>, error) ReadTileData(Tile t, HashReader r)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;

            var size = t.W;
            if (size == 0L)
            {
                size = 1L << (int)(uint(t.H));
            }
            var start = t.N << (int)(uint(t.H));
            var indexes = make_slice<long>(size);
            {
                long i__prev1 = i;

                for (long i = 0L; i < size; i++)
                {
                    indexes[i] = StoredHashIndex(t.H * t.L, start + int64(i));
                }


                i = i__prev1;
            }

            var (hashes, err) = r.ReadHashes(indexes);
            if (err != null)
            {
                return (null, error.As(err)!);
            }
            if (len(hashes) != len(indexes))
            {
                return (null, error.As(fmt.Errorf("tlog: ReadHashes(%d indexes) = %d hashes", len(indexes), len(hashes)))!);
            }
            var tile = make_slice<byte>(size * HashSize);
            {
                long i__prev1 = i;

                for (i = 0L; i < size; i++)
                {
                    copy(tile[i * HashSize..], hashes[i][..]);
                }


                i = i__prev1;
            }
            return (tile, error.As(null!)!);
        }

        // To limit the size of any particular directory listing,
        // we encode the (possibly very large) number N
        // by encoding three digits at a time.
        // For example, 123456789 encodes as x123/x456/789.
        // Each directory has at most 1000 each xNNN, NNN, and NNN.p children,
        // so there are at most 3000 entries in any one directory.
        private static readonly long pathBase = (long)1000L;

        // Path returns a tile coordinate path describing t.


        // Path returns a tile coordinate path describing t.
        public static @string Path(this Tile t)
        {
            var n = t.N;
            var nStr = fmt.Sprintf("%03d", n % pathBase);
            while (n >= pathBase)
            {
                n /= pathBase;
                nStr = fmt.Sprintf("x%03d/%s", n % pathBase, nStr);
            }

            @string pStr = "";
            if (t.W != 1L << (int)(uint(t.H)))
            {
                pStr = fmt.Sprintf(".p/%d", t.W);
            }
            @string L = default;
            if (t.L == -1L)
            {
                L = "data";
            }
            else
            {
                L = fmt.Sprintf("%d", t.L);
            }
            return fmt.Sprintf("tile/%d/%s/%s%s", t.H, L, nStr, pStr);
        }

        // ParseTilePath parses a tile coordinate path.
        public static (Tile, error) ParseTilePath(@string path)
        {
            Tile _p0 = default;
            error _p0 = default!;

            var f = strings.Split(path, "/");
            if (len(f) < 4L || f[0L] != "tile")
            {
                return (new Tile(), error.As(addr(new badPathError(path))!)!);
            }
            var (h, err1) = strconv.Atoi(f[1L]);
            var isData = false;
            if (f[2L] == "data")
            {
                isData = true;
                f[2L] = "0";
            }
            var (l, err2) = strconv.Atoi(f[2L]);
            if (err1 != null || err2 != null || h < 1L || l < 0L || h > 30L)
            {
                return (new Tile(), error.As(addr(new badPathError(path))!)!);
            }
            long w = 1L << (int)(uint(h));
            {
                var dotP = f[len(f) - 2L];

                if (strings.HasSuffix(dotP, ".p"))
                {
                    var (ww, err) = strconv.Atoi(f[len(f) - 1L]);
                    if (err != null || ww <= 0L || ww >= w)
                    {
                        return (new Tile(), error.As(addr(new badPathError(path))!)!);
                    }
                    w = ww;
                    f[len(f) - 2L] = dotP[..len(dotP) - len(".p")];
                    f = f[..len(f) - 1L];
                }

            }
            f = f[3L..];
            var n = int64(0L);
            foreach (var (_, s) in f)
            {
                var (nn, err) = strconv.Atoi(strings.TrimPrefix(s, "x"));
                if (err != null || nn < 0L || nn >= pathBase)
                {
                    return (new Tile(), error.As(addr(new badPathError(path))!)!);
                }
                n = n * pathBase + int64(nn);
            }
            if (isData)
            {
                l = -1L;
            }
            Tile t = new Tile(H:h,L:l,N:n,W:w);
            if (path != t.Path())
            {
                return (new Tile(), error.As(addr(new badPathError(path))!)!);
            }
            return (t, error.As(null!)!);
        }

        private partial struct badPathError
        {
            public @string path;
        }

        private static @string Error(this ptr<badPathError> _addr_e)
        {
            ref badPathError e = ref _addr_e.val;

            return fmt.Sprintf("malformed tile path %q", e.path);
        }

        // A TileReader reads tiles from a go.sum database log.
        public partial interface TileReader
        {
            (slice<slice<byte>>, error) Height(); // ReadTiles returns the data for each requested tile.
// If ReadTiles returns err == nil, it must also return
// a data record for each tile (len(data) == len(tiles))
// and each data record must be the correct length
// (len(data[i]) == tiles[i].W*HashSize).
//
// An implementation of ReadTiles typically reads
// them from an on-disk cache or else from a remote
// tile server. Tile data downloaded from a server should
// be considered suspect and not saved into a persistent
// on-disk cache before returning from ReadTiles.
// When the client confirms the validity of the tile data,
// it will call SaveTiles to signal that they can be safely
// written to persistent storage.
// See also https://research.swtch.com/tlog#authenticating_tiles.
            (slice<slice<byte>>, error) ReadTiles(slice<Tile> tiles); // SaveTiles informs the TileReader that the tile data
// returned by ReadTiles has been confirmed as valid
// and can be saved in persistent storage (on disk).
            (slice<slice<byte>>, error) SaveTiles(slice<Tile> tiles, slice<slice<byte>> data);
        }

        // TileHashReader returns a HashReader that satisfies requests
        // by loading tiles of the given tree.
        //
        // The returned HashReader checks that loaded tiles are
        // valid for the given tree. Therefore, any hashes returned
        // by the HashReader are already proven to be in the tree.
        public static HashReader TileHashReader(Tree tree, TileReader tr)
        {
            return addr(new tileHashReader(tree:tree,tr:tr));
        }

        private partial struct tileHashReader
        {
            public Tree tree;
            public TileReader tr;
        }

        // tileParent returns t's k'th tile parent in the tiles for a tree of size n.
        // If there is no such parent, tileParent returns Tile{}.
        private static Tile tileParent(Tile t, long k, long n)
        {
            t.L += k;
            t.N >>= uint(k * t.H);
            t.W = 1L << (int)(uint(t.H));
            {
                var max = n >> (int)(uint(t.L * t.H));

                if (t.N << (int)(uint(t.H)) + int64(t.W) >= max)
                {
                    if (t.N << (int)(uint(t.H)) >= max)
                    {
                        return new Tile();
                    }
                    t.W = int(max - t.N << (int)(uint(t.H)));
                }

            }
            return t;
        }

        private static (slice<Hash>, error) ReadHashes(this ptr<tileHashReader> _addr_r, slice<long> indexes)
        {
            slice<Hash> _p0 = default;
            error _p0 = default!;
            ref tileHashReader r = ref _addr_r.val;

            var h = r.tr.Height();

            var tileOrder = make_map<Tile, long>(); // tileOrder[tileKey(tiles[i])] = i
            slice<Tile> tiles = default; 

            // Plan to fetch tiles necessary to recompute tree hash.
            // If it matches, those tiles are authenticated.
            var stx = subTreeIndex(0L, r.tree.N, null);
            var stxTileOrder = make_slice<long>(len(stx));
            {
                var i__prev1 = i;
                var x__prev1 = x;

                foreach (var (__i, __x) in stx)
                {
                    i = __i;
                    x = __x;
                    var (tile, _, _) = tileForIndex(h, x);
                    tile = tileParent(tile, 0L, r.tree.N);
                    {
                        var j__prev1 = j;

                        var (j, ok) = tileOrder[tile];

                        if (ok)
                        {
                            stxTileOrder[i] = j;
                            continue;
                        }

                        j = j__prev1;

                    }
                    stxTileOrder[i] = len(tiles);
                    tileOrder[tile] = len(tiles);
                    tiles = append(tiles, tile);
                } 

                // Plan to fetch tiles containing the indexes,
                // along with any parent tiles needed
                // for authentication. For most calls,
                // the parents are being fetched anyway.

                i = i__prev1;
                x = x__prev1;
            }

            var indexTileOrder = make_slice<long>(len(indexes));
            {
                var i__prev1 = i;
                var x__prev1 = x;

                foreach (var (__i, __x) in indexes)
                {
                    i = __i;
                    x = __x;
                    if (x >= StoredHashIndex(0L, r.tree.N))
                    {
                        return (null, error.As(fmt.Errorf("indexes not in tree"))!);
                    }
                    (tile, _, _) = tileForIndex(h, x); 

                    // Walk up parent tiles until we find one we've requested.
                    // That one will be authenticated.
                    long k = 0L;
                    while (k >= 0L)
                    {
                        var p = tileParent(tile, k, r.tree.N);
                        {
                            var j__prev1 = j;

                            (j, ok) = tileOrder[p];

                            if (ok)
                            {
                                if (k == 0L)
                                {
                                    indexTileOrder[i] = j;
                        k++;
                                }
                                break;
                            }

                            j = j__prev1;

                        }
                    } 

                    // Walk back down recording child tiles after parents.
                    // This loop ends by revisiting the tile for this index
                    // (tileParent(tile, 0, r.tree.N)) unless k == 0, in which
                    // case the previous loop did it.
 

                    // Walk back down recording child tiles after parents.
                    // This loop ends by revisiting the tile for this index
                    // (tileParent(tile, 0, r.tree.N)) unless k == 0, in which
                    // case the previous loop did it.
                    k--;

                    while (k >= 0L)
                    {
                        p = tileParent(tile, k, r.tree.N);
                        if (p.W != 1L << (int)(uint(p.H)))
                        { 
                            // Only full tiles have parents.
                            // This tile has a parent, so it must be full.
                            return (null, error.As(fmt.Errorf("bad math in tileHashReader: %d %d %v", r.tree.N, x, p))!);
                        k--;
                        }
                        tileOrder[p] = len(tiles);
                        if (k == 0L)
                        {
                            indexTileOrder[i] = len(tiles);
                        }
                        tiles = append(tiles, p);
                    }
                } 

                // Fetch all the tile data.

                i = i__prev1;
                x = x__prev1;
            }

            var (data, err) = r.tr.ReadTiles(tiles);
            if (err != null)
            {
                return (null, error.As(err)!);
            }
            if (len(data) != len(tiles))
            {
                return (null, error.As(fmt.Errorf("TileReader returned bad result slice (len=%d, want %d)", len(data), len(tiles)))!);
            }
            {
                var i__prev1 = i;
                var tile__prev1 = tile;

                foreach (var (__i, __tile) in tiles)
                {
                    i = __i;
                    tile = __tile;
                    if (len(data[i]) != tile.W * HashSize)
                    {
                        return (null, error.As(fmt.Errorf("TileReader returned bad result slice (%v len=%d, want %d)", tile.Path(), len(data[i]), tile.W * HashSize))!);
                    }
                } 

                // Authenticate the initial tiles against the tree hash.
                // They are arranged so that parents are authenticated before children.
                // First the tiles needed for the tree hash.

                i = i__prev1;
                tile = tile__prev1;
            }

            var (th, err) = HashFromTile(tiles[stxTileOrder[len(stx) - 1L]], data[stxTileOrder[len(stx) - 1L]], stx[len(stx) - 1L]);
            if (err != null)
            {
                return (null, error.As(err)!);
            }
            {
                var i__prev1 = i;

                for (var i = len(stx) - 2L; i >= 0L; i--)
                {
                    var (h, err) = HashFromTile(tiles[stxTileOrder[i]], data[stxTileOrder[i]], stx[i]);
                    if (err != null)
                    {
                        return (null, error.As(err)!);
                    }
                    th = NodeHash(h, th);
                }


                i = i__prev1;
            }
            if (th != r.tree.Hash)
            { 
                // The tiles do not support the tree hash.
                // We know at least one is wrong, but not which one.
                return (null, error.As(fmt.Errorf("downloaded inconsistent tile"))!);
            } 

            // Authenticate full tiles against their parents.
            {
                var i__prev1 = i;

                for (i = len(stx); i < len(tiles); i++)
                {
                    var tile = tiles[i];
                    p = tileParent(tile, 1L, r.tree.N);
                    (j, ok) = tileOrder[p];
                    if (!ok)
                    {
                        return (null, error.As(fmt.Errorf("bad math in tileHashReader %d %v: lost parent of %v", r.tree.N, indexes, tile))!);
                    }
                    (h, err) = HashFromTile(p, data[j], StoredHashIndex(p.L * p.H, tile.N));
                    if (err != null)
                    {
                        return (null, error.As(fmt.Errorf("bad math in tileHashReader %d %v: lost hash of %v: %v", r.tree.N, indexes, tile, err))!);
                    }
                    if (h != tileHash(data[i]))
                    {
                        return (null, error.As(fmt.Errorf("downloaded inconsistent tile"))!);
                    }
                } 

                // Now we have all the tiles needed for the requested hashes,
                // and we've authenticated the full tile set against the trusted tree hash.


                i = i__prev1;
            } 

            // Now we have all the tiles needed for the requested hashes,
            // and we've authenticated the full tile set against the trusted tree hash.
            r.tr.SaveTiles(tiles, data); 

            // Pull out the requested hashes.
            var hashes = make_slice<Hash>(len(indexes));
            {
                var i__prev1 = i;
                var x__prev1 = x;

                foreach (var (__i, __x) in indexes)
                {
                    i = __i;
                    x = __x;
                    var j = indexTileOrder[i];
                    (h, err) = HashFromTile(tiles[j], data[j], x);
                    if (err != null)
                    {
                        return (null, error.As(fmt.Errorf("bad math in tileHashReader %d %v: lost hash %v: %v", r.tree.N, indexes, x, err))!);
                    }
                    hashes[i] = h;
                }

                i = i__prev1;
                x = x__prev1;
            }

            return (hashes, error.As(null!)!);
        }
    }
}}}}}
