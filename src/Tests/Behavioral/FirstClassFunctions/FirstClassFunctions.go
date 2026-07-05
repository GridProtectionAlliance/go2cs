// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

package main

import (
	"fmt"
	"math/rand"
)

const (
	win            = 100 // The winning score in a game of Pig
	gamesPerSeries = 10  // The number of games per series to simulate
)

// A score includes scores accumulated in previous turns for each player,
// as well as the points scored by the current player in this turn.
type score struct {
	player, opponent, thisTurn int
}

// An action transitions stochastically to a resulting score.
type action func(current score) (result score, turnIsOver bool)

// roll returns the (result, turnIsOver) outcome of simulating a die roll.
// If the roll value is 1, then thisTurn score is abandoned, and the players'
// roles swap.  Otherwise, the roll value is added to thisTurn.
func roll(s score) (score, bool) {
	outcome := rand.Intn(6) + 1 // A random int in [1, 6]
	if outcome == 1 {
		return score{s.opponent, s.player, 0}, true
	}
	return score{s.player, s.opponent, outcome + s.thisTurn}, false
}

// stay returns the (result, turnIsOver) outcome of staying.
// thisTurn score is added to the player's score, and the players' roles swap.
func stay(s score) (score, bool) {
	return score{s.opponent, s.player + s.thisTurn, 0}, true
}

// A strategy chooses an action for any given score.
type strategy func(score) action

// stayAtK returns a strategy that rolls until thisTurn is at least k, then stays.
func stayAtK(k int) strategy {
	return func(s score) action {
		if s.thisTurn >= k {
			return stay
		}
		return roll
	}
}

// play simulates a Pig game and returns the winner (0 or 1).
func play(strategy0, strategy1 strategy) int {
	strategies := []strategy{strategy0, strategy1}
	var s score
	var turnIsOver bool
	currentPlayer := rand.Intn(2) // Randomly decide who plays first
	for s.player+s.thisTurn < win {
		action := strategies[currentPlayer](s)
		s, turnIsOver = action(s)
		if turnIsOver {
			currentPlayer = (currentPlayer + 1) % 2
		}
	}
	return currentPlayer
}

// roundRobin simulates a series of games between every pair of strategies.
func roundRobin(strategies []strategy) ([]int, int) {
	wins := make([]int, len(strategies))
	for i := 0; i < len(strategies); i++ {
		for j := i + 1; j < len(strategies); j++ {
			for k := 0; k < gamesPerSeries; k++ {
				winner := play(strategies[i], strategies[j])
				if winner == 0 {
					wins[i]++
				} else {
					wins[j]++
				}
			}
		}
	}
	gamesPerStrategy := gamesPerSeries * (len(strategies) - 1) // no self play
	return wins, gamesPerStrategy
}

// ratioString takes a list of integer values and returns a string that lists
// each value and its percentage of the sum of all values.
// e.g., ratios(1, 2, 3) = "1/6 (16.7%), 2/6 (33.3%), 3/6 (50.0%)"
func ratioString(vals ...int) string {
	total := 0
	for _, val := range vals {
		total += val
	}
	s := ""
	for _, val := range vals {
		if s != "" {
			s += ", "
		}
		pct := 100 * float64(val) / float64(total)
		s += fmt.Sprintf("%d/%d (%0.1f%%)", val, total, pct)
	}
	return s
}

func main() {
	strategies := make([]strategy, win)
	for k := range strategies {
		strategies[k] = stayAtK(k + 1)
	}
	wins, games := roundRobin(strategies)

    var k int
	for k = range strategies {
		fmt.Printf("Wins, losses staying at k =% 4d: %s\n",
			k+1, ratioString(wins[k], games-wins[k]))
	}

	// A conversion whose target is a bare func TYPE — `(func(...))(f)`, the shape of reflect
	// FuncOf's `(func())(nil)` prototype — exercises the *ast.FuncType expression-position case
	// in the converter (previously an unhandled convExpr node, silently dropping the file).
	converted := (func(score) (score, bool))(stay)
	result, turnIsOver := converted(score{player: 3, opponent: 5, thisTurn: 7})
	fmt.Println("converted func type call:", result.player, result.opponent, result.thisTurn, turnIsOver)

	// A call through a NAMED func-type FIELD deconstructs as one tuple call - the named
	// type's underlying signature carries the result count (bufio Scanner's split SplitFunc;
	// the shattered form emitted one full call per LHS element, tripling side effects).
	m := machine{split: func(s string) (int, string, error) { return len(s), s + "!", nil }}
	n, out, serr := m.split("hi")
	fmt.Println(n, out, serr == nil) // 2 hi! true

	p := provider{produce: func(x int) int { return x * 2 }}
	r := registry{h: p.produce}
	fmt.Println(r.h(21)) // 42

	var tg tagged = r.h
	fmt.Println(tg.tag(), r.h(1)) // handler 2

	wk := &worker{n: 3}
	if err := wk.initWorker(0); err == nil {
		fmt.Println(wk.run([]byte{1, 2}), *lastN) // 6 4
	}

	// Func-typed struct fields with NAMED tuple results (go/doc/comment's LookupPackage):
	// the string-based Func<> render must strip the Go-ordered names — one named result
	// unwraps to its bare type (a C# 1-tuple is CS8124), two become the C#-ordered named
	// tuple ((@string importPath, bool ok)).
	res := resolver{
		lookupPackage: func(name string) (importPath string, ok bool) {
			if name == "fmt" {
				return "pkg/fmt", true
			}
			return "", false
		},
		lookupSym: func(recv string, name string) (ok bool) {
			return recv == "" && name == "Printf"
		},
	}
	ip, found := res.lookupPackage("fmt")
	fmt.Println(ip, found, res.lookupSym("", "Printf"), res.lookupSym("T", "M")) // pkg/fmt true true false

	// A single-return literal whose returned tuple call needs a per-element interface
	// conversion hoists `var (ᴛ1, ᴛ2) = call;` ahead of the return — the expression-lambda
	// collapse must NOT fire there (net lookup.go's DoChan literal `func() (any, error) {
	// return testHookLookupIP(…) }` kept only the bare markers, CS0103 x2).
	fetch := func() (any, error) { return fetchPair() }
	got, gerr := fetch()
	fmt.Println("collapse hoist:", got, gerr == nil) // collapse hoist: pair true
}

func fetchPair() (string, error) { return "pair", nil }

type resolver struct {
	lookupPackage func(name string) (importPath string, ok bool)
	lookupSym     func(recv, name string) (ok bool)
}

type splitter func(s string) (int, string, error)

type machine struct{ split splitter }

// worker mirrors compress/flate's compressor: METHOD EXPRESSIONS assigned into func
// fields (`w.fill = (*worker).fillFast`) and invoked with the receiver as the first arg
// (`w.fill(w, b)`). The ref receiver must never take a box render (CS0103 x11 on Ꮡd),
// and the func-FIELD callee's signature drives the box form of the receiver arg
// (CS1503 x5).
type worker struct {
	fill func(*worker, []byte) int
	step func(*worker)
	n    int
}

var lastN *int

func (w *worker) fillFast(b []byte) int { lastN = &w.n; return len(b) + w.n }
func (w *worker) bump()                 { w.n++ }

func (w *worker) initWorker(level int) (err error) {
	switch {
	case level == 0:
		w.fill = (*worker).fillFast
		w.step = (*worker).bump
	default:
		return fmt.Errorf("bad level %d", level)
	}
	return nil
}

func (w *worker) run(b []byte) int {
	w.step(w)
	return w.fill(w, b)
}

// handler is a NAMED func type; registry's field takes a value of a DIFFERENT delegate
// type (provider's plain func-typed field), which C# cannot convert implicitly — the
// composite literal wraps it in the target delegate's constructor (internal/concurrent's
// `keyHash: mapType.Hasher`, CS1503 x3).
type handler func(int) int

// tag is a METHOD ON A FUNC TYPE - a C# delegate cannot be a partial struct, so the
// interface satisfaction routes through the generated VALUE adapter (flag's funcValue
// implementing Value; the struct-only throw had killed the package's whole generator run,
// CS0246 x19).
func (h handler) tag() string { return "handler" }

type tagged interface{ tag() string }

type provider struct{ produce func(int) int }

type registry struct{ h handler }
