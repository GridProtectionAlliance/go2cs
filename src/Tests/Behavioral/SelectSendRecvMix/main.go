// Mixed send and receive cases: only the single committed op mutates state, including
// send and receive cases on the SAME channel (the pending-slot hardening: a send-case
// win must not leave a stale receive value behind for a later guard to consume).
package main

import "fmt"

func main() {
	// One recv-ready and one send-ready case across two channels: two selects commit
	// the two ops in SOME order (uniformly random), never both at once.
	data := make(chan int, 1)
	out := make(chan int, 1)
	data <- 5
	recvGot, sentTo, rounds := 0, 0, 0
	for len(data) > 0 || len(out) == 0 {
		rounds++
		select {
		case v := <-data:
			recvGot = v
		case out <- 7:
			sentTo++
		}
	}
	fmt.Println("rounds:", rounds, "recvGot:", recvGot, "sentTo:", sentTo, "out:", <-out)

	// Send and recv cases on the SAME channel. Full: only the recv case is ready.
	ch := make(chan int, 1)
	ch <- 3
	took := 0
	select {
	case ch <- 8:
		fmt.Println("send fired on full channel (wrong)")
	case took = <-ch:
	}
	fmt.Println("took:", took, "len:", len(ch))

	// Empty: only the send case is ready.
	select {
	case ch <- 8:
	case took = <-ch:
		fmt.Println("recv fired on empty channel (wrong)")
	}
	fmt.Println("len:", len(ch), "drained:", <-ch)
}
