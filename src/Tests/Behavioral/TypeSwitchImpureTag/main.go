package main

import "fmt"

var calls int

func next(v any) any {
	calls++
	return v
}

// classify: an IMPURE tag (function call) with BOTH re-bind arms — a multi-type clause
// and a bound default. The tag must evaluate exactly ONCE per switch dispatch; the calls
// counter printed by main proves it against Go.
func classify(x any) string {
	switch v := next(x).(type) {
	case int:
		return fmt.Sprintf("int:%d", v)
	case string, bool:
		return fmt.Sprintf("strbool:%v", v)
	default:
		return fmt.Sprintf("other:%v", v)
	}
}

// recovered: the motivating shape — a recover() tag re-evaluated at a re-bind arm would
// return nil the second time, silently losing the recovered value.
func recovered(v any) (result string) {
	defer func() {
		switch p := recover().(type) {
		case string, int:
			result = fmt.Sprintf("caught:%v", p)
		case nil:
			result = "no panic?"
		default:
			result = fmt.Sprintf("caught-other:%v", p)
		}
	}()
	panic(v)
}

// fromChan: a channel-RECEIVE tag — re-evaluation would block forever on the drained
// buffered channel (the old emission hangs; the hoisted form completes).
func fromChan(ch chan any) string {
	switch v := (<-ch).(type) {
	case int, int64:
		return fmt.Sprintf("num:%v", v)
	default:
		return fmt.Sprintf("chan-other:%v", v)
	}
}

func main() {
	fmt.Println(classify(7))
	fmt.Println(classify("hi"))
	fmt.Println(classify(true))
	fmt.Println(classify(2.5))
	fmt.Println("calls:", calls)

	fmt.Println(recovered("boom"))
	fmt.Println(recovered(9))
	fmt.Println(recovered(1.25))

	ch := make(chan any, 1)
	ch <- 5
	fmt.Println(fromChan(ch))
	msg := "text"
	ch <- msg
	fmt.Println(fromChan(ch))
}
