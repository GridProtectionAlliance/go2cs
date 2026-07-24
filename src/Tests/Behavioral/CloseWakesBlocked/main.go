// close() must wake every parked waiter with the right outcome: parked receivers (plain
// and select) get (zero, false); parked senders (plain and select) panic "send on
// closed channel" on wake. Every scenario is interleaving-independent: whether the
// waiter parks before the close or arrives after it, Go specifies the SAME observable
// result, so the output is deterministic without sleeps.
package main

import "fmt"

func expectPanic(name string, f func()) {
	defer func() {
		fmt.Println(name, "->", recover())
	}()
	f()
}

func main() {
	// close wakes blocked receivers: three goroutines park in <-ch (unbuffered, no
	// sender ever); close hands each (0, false).
	ch := make(chan int)
	res := make(chan string, 3)
	for i := 0; i < 3; i++ {
		go func() {
			v, ok := <-ch
			res <- fmt.Sprintf("recv %d %t", v, ok)
		}()
	}
	close(ch)
	for i := 0; i < 3; i++ {
		fmt.Println(<-res)
	}

	// close wakes a blocked sender: no receiver ever exists, so the send can only end
	// in the close-induced panic (parked-then-woken or send-after-close alike).
	ch2 := make(chan int)
	res2 := make(chan string, 1)
	go func() {
		defer func() {
			if r := recover(); r != nil {
				res2 <- fmt.Sprintf("sender panicked: %v", r)
			}
		}()
		ch2 <- 1
		res2 <- "sender completed (wrong)"
	}()
	close(ch2)
	fmt.Println(<-res2)

	// close during a blocked select, receive direction: the select's ch3 case wakes
	// with (0, false); the other channel is never ready.
	ch3 := make(chan int)
	other := make(chan int)
	res3 := make(chan string, 1)
	go func() {
		select {
		case v, ok := <-ch3:
			res3 <- fmt.Sprintf("select recv %d %t", v, ok)
		case v := <-other:
			res3 <- fmt.Sprintf("other %d (wrong)", v)
		}
	}()
	close(ch3)
	fmt.Println(<-res3)

	// close during a blocked select, send direction: the woken (or arriving) select
	// send panics.
	ch4 := make(chan int)
	other2 := make(chan int)
	res4 := make(chan string, 1)
	go func() {
		defer func() {
			if r := recover(); r != nil {
				res4 <- fmt.Sprintf("select send panicked: %v", r)
			}
		}()
		select {
		case ch4 <- 99:
			res4 <- "select send completed (wrong)"
		case v := <-other2:
			res4 <- fmt.Sprintf("other %d (wrong)", v)
		}
	}()
	close(ch4)
	fmt.Println(<-res4)

	// The close/send panic family (all recovered).
	expectPanic("close of closed", func() {
		cc := make(chan int)
		close(cc)
		close(cc)
	})
	expectPanic("close of nil", func() {
		var nc chan int
		close(nc)
	})
	expectPanic("send on closed", func() {
		sc := make(chan int, 1)
		close(sc)
		sc <- 1
	})
	expectPanic("select send on closed with default", func() {
		sd := make(chan int, 1)
		close(sd)
		select {
		case sd <- 1:
			fmt.Println("sent (wrong)")
		default:
			fmt.Println("default (wrong)")
		}
	})
}
