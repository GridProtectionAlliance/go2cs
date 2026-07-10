package main

import "fmt"

func main() {
	g1()
	g2()
	g3()
	g4()
	g5()
	g6()
	g7()
	g8()
	g9()
}

// G1: basic read-only capture (the repro)
func g1() {
	var fs []func() int
	for i := 0; i < 3; i++ {
		fs = append(fs, func() int { return i })
	}
	fmt.Println("g1:", fs[0](), fs[1](), fs[2]())
}

// G2: capture + body write + unlabeled continue
func g2() {
	var fs []func() int
	for i := 0; i < 6; i++ {
		if i%2 == 0 {
			continue
		}
		i++
		fs = append(fs, func() int { return i })
	}
	fmt.Println("g2:", len(fs))
	for _, f := range fs {
		fmt.Println("g2v:", f())
	}
}

// G3: &i stored per iteration (boxed, no closure)
func g3() {
	var ps []*int
	for i := 0; i < 3; i++ {
		ps = append(ps, &i)
	}
	fmt.Println("g3:", *ps[0], *ps[1], *ps[2])
}

// G4: boxed + captured — closures observing per-iteration boxes
func g4() {
	var fs []func()
	for i := 0; i < 3; i++ {
		p := &i
		fs = append(fs, func() { fmt.Println("g4:", *p, i) })
	}
	for _, f := range fs {
		f()
	}
}

// G5: labeled continue with capture + write
func g5() {
	var fs []func() int
outer:
	for i := 0; i < 4; i++ {
		for j := 0; j < 2; j++ {
			if i%2 == 1 {
				i += 10
				continue outer
			}
			_ = j
		}
		fs = append(fs, func() int { return i })
	}
	for _, f := range fs {
		fmt.Println("g5:", f())
	}
}

// G6: nested same-name loops, both captured
func g6() {
	var fs []func() int
	for i := 0; i < 2; i++ {
		for i := 10; i < 12; i++ {
			fs = append(fs, func() int { return i })
		}
		fs = append(fs, func() int { return i })
	}
	for _, f := range fs {
		fmt.Println("g6:", f())
	}
}

// G7: multi-var init, only one captured
func g7() {
	var fs []func() int
	for i, j := 0, 100; i < 3; i, j = i+1, j+2 {
		_ = j
		fs = append(fs, func() int { return i })
	}
	fmt.Println("g7:", fs[0](), fs[1](), fs[2]())
}

type pt struct{ x, y int }

// G8: struct-typed loop var captured
func g8() {
	var fs []func() int
	for s := (pt{x: 1}); s.x < 4; s.x++ {
		fs = append(fs, func() int { return s.x })
	}
	fmt.Println("g8:", fs[0](), fs[1](), fs[2]())
}

// G9: write via immediately-invoked closure
func g9() {
	sum := 0
	for i := 0; i < 3; i++ {
		func() { i++ }()
		sum += i
	}
	fmt.Println("g9:", sum)
}
