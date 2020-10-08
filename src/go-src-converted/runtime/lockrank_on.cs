// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build goexperiment.staticlockranking

// package runtime -- go2cs converted at 2020 October 08 03:19:59 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\lockrank_on.go
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class runtime_package
    {
        // lockRankStruct is embedded in mutex
        private partial struct lockRankStruct
        {
            public lockRank rank; // pad field to make sure lockRankStruct is a multiple of 8 bytes, even on
// 32-bit systems.
            public long pad;
        }

        // init checks that the partial order in lockPartialOrder fits within the total
        // order determined by the order of the lockRank constants.
        private static void init()
        {
            foreach (var (rank, list) in lockPartialOrder)
            {
                foreach (var (_, entry) in list)
                {
                    if (entry > lockRank(rank))
                    {
                        println("lockPartial order row", lockRank(rank).String(), "entry", entry.String());
                        throw("lockPartialOrder table is inconsistent with total lock ranking order");
                    }

                }

            }

        }

        private static void lockInit(ptr<mutex> _addr_l, lockRank rank)
        {
            ref mutex l = ref _addr_l.val;

            l.rank = rank;
        }

        private static lockRank getLockRank(ptr<mutex> _addr_l)
        {
            ref mutex l = ref _addr_l.val;

            return l.rank;
        }

        // The following functions are the entry-points to record lock
        // operations.
        // All of these are nosplit and switch to the system stack immediately
        // to avoid stack growths. Since a stack growth could itself have lock
        // operations, this prevents re-entrant calls.

        // lockWithRank is like lock(l), but allows the caller to specify a lock rank
        // when acquiring a non-static lock.
        //go:nosplit
        private static void lockWithRank(ptr<mutex> _addr_l, lockRank rank)
        {
            ref mutex l = ref _addr_l.val;

            if (l == _addr_debuglock || l == _addr_paniclk)
            { 
                // debuglock is only used for println/printlock(). Don't do lock
                // rank recording for it, since print/println are used when
                // printing out a lock ordering problem below.
                //
                // paniclk has an ordering problem, since it can be acquired
                // during a panic with any other locks held (especially if the
                // panic is because of a directed segv), and yet also allg is
                // acquired after paniclk in tracebackothers()). This is a genuine
                // problem, so for now we don't do lock rank recording for paniclk
                // either.
                lock2(l);
                return ;

            }

            if (rank == 0L)
            {
                rank = lockRankLeafRank;
            }

            var gp = getg(); 
            // Log the new class.
            systemstack(() =>
            {
                var i = gp.m.locksHeldLen;
                if (i >= len(gp.m.locksHeld))
                {
                    throw("too many locks held concurrently for rank checking");
                }

                gp.m.locksHeld[i].rank = rank;
                gp.m.locksHeld[i].lockAddr = uintptr(@unsafe.Pointer(l));
                gp.m.locksHeldLen++; 

                // i is the index of the lock being acquired
                if (i > 0L)
                {
                    checkRanks(_addr_gp, gp.m.locksHeld[i - 1L].rank, rank);
                }

                lock2(l);

            });

        }

        // acquireLockRank acquires a rank which is not associated with a mutex lock
        //go:nosplit
        private static void acquireLockRank(lockRank rank)
        {
            var gp = getg(); 
            // Log the new class.
            systemstack(() =>
            {
                var i = gp.m.locksHeldLen;
                if (i >= len(gp.m.locksHeld))
                {
                    throw("too many locks held concurrently for rank checking");
                }

                gp.m.locksHeld[i].rank = rank;
                gp.m.locksHeld[i].lockAddr = 0L;
                gp.m.locksHeldLen++; 

                // i is the index of the lock being acquired
                if (i > 0L)
                {
                    checkRanks(_addr_gp, gp.m.locksHeld[i - 1L].rank, rank);
                }

            });

        }

        // checkRanks checks if goroutine g, which has mostly recently acquired a lock
        // with rank 'prevRank', can now acquire a lock with rank 'rank'.
        private static void checkRanks(ptr<g> _addr_gp, lockRank prevRank, lockRank rank)
        {
            ref g gp = ref _addr_gp.val;

            var rankOK = false;
            if (rank < prevRank)
            { 
                // If rank < prevRank, then we definitely have a rank error
                rankOK = false;

            }
            else if (rank == lockRankLeafRank)
            { 
                // If new lock is a leaf lock, then the preceding lock can
                // be anything except another leaf lock.
                rankOK = prevRank < lockRankLeafRank;

            }
            else
            { 
                // We've now verified the total lock ranking, but we
                // also enforce the partial ordering specified by
                // lockPartialOrder as well. Two locks with the same rank
                // can only be acquired at the same time if explicitly
                // listed in the lockPartialOrder table.
                var list = lockPartialOrder[rank];
                foreach (var (_, entry) in list)
                {
                    if (entry == prevRank)
                    {
                        rankOK = true;
                        break;
                    }

                }

            }

            if (!rankOK)
            {
                printlock();
                println(gp.m.procid, " ======");
                foreach (var (j, held) in gp.m.locksHeld[..gp.m.locksHeldLen])
                {
                    println(j, ":", held.rank.String(), held.rank, @unsafe.Pointer(gp.m.locksHeld[j].lockAddr));
                }
                throw("lock ordering problem");

            }

        }

        //go:nosplit
        private static void unlockWithRank(ptr<mutex> _addr_l)
        {
            ref mutex l = ref _addr_l.val;

            if (l == _addr_debuglock || l == _addr_paniclk)
            { 
                // See comment at beginning of lockWithRank.
                unlock2(l);
                return ;

            }

            var gp = getg();
            systemstack(() =>
            {
                var found = false;
                for (var i = gp.m.locksHeldLen - 1L; i >= 0L; i--)
                {
                    if (gp.m.locksHeld[i].lockAddr == uintptr(@unsafe.Pointer(l)))
                    {
                        found = true;
                        copy(gp.m.locksHeld[i..gp.m.locksHeldLen - 1L], gp.m.locksHeld[i + 1L..gp.m.locksHeldLen]);
                        gp.m.locksHeldLen--;
                        break;
                    }

                }

                if (!found)
                {
                    println(gp.m.procid, ":", l.rank.String(), l.rank, l);
                    throw("unlock without matching lock acquire");
                }

                unlock2(l);

            });

        }

        // releaseLockRank releases a rank which is not associated with a mutex lock
        //go:nosplit
        private static void releaseLockRank(lockRank rank)
        {
            var gp = getg();
            systemstack(() =>
            {
                var found = false;
                for (var i = gp.m.locksHeldLen - 1L; i >= 0L; i--)
                {
                    if (gp.m.locksHeld[i].rank == rank && gp.m.locksHeld[i].lockAddr == 0L)
                    {
                        found = true;
                        copy(gp.m.locksHeld[i..gp.m.locksHeldLen - 1L], gp.m.locksHeld[i + 1L..gp.m.locksHeldLen]);
                        gp.m.locksHeldLen--;
                        break;
                    }

                }

                if (!found)
                {
                    println(gp.m.procid, ":", rank.String(), rank);
                    throw("lockRank release without matching lockRank acquire");
                }

            });

        }

        //go:nosplit
        private static void lockWithRankMayAcquire(ptr<mutex> _addr_l, lockRank rank)
        {
            ref mutex l = ref _addr_l.val;

            var gp = getg();
            if (gp.m.locksHeldLen == 0L)
            { 
                // No possibilty of lock ordering problem if no other locks held
                return ;

            }

            systemstack(() =>
            {
                var i = gp.m.locksHeldLen;
                if (i >= len(gp.m.locksHeld))
                {
                    throw("too many locks held concurrently for rank checking");
                } 
                // Temporarily add this lock to the locksHeld list, so
                // checkRanks() will print out list, including this lock, if there
                // is a lock ordering problem.
                gp.m.locksHeld[i].rank = rank;
                gp.m.locksHeld[i].lockAddr = uintptr(@unsafe.Pointer(l));
                gp.m.locksHeldLen++;
                checkRanks(_addr_gp, gp.m.locksHeld[i - 1L].rank, rank);
                gp.m.locksHeldLen--;

            });

        }
    }
}
