package main

import "fmt"

type Row [3]int

type holder struct {
	arr [3]int
}

var leaked *[3]int
var leakedRow *Row

func leakLocal() [3]int {
	l := [3]int{1, 2, 3}
	leaked = &l
	return l
}

func leakRow() Row {
	r := Row{10, 20, 30}
	leakedRow = &r
	return r
}

func (h holder) get() [3]int {
	return h.arr
}

func modDirect(a [3]int) {
	a[0] = 99
}

func modNamed(r Row) {
	r[0] = 99
}

func modDeep(m [2][3]int) {
	m[0][0] = 99
}

func (r Row) mut() {
	r[0] = 77
}

// Range element values are per-iteration copies (gap class: range).
func rangeValues() {
	m := [2][3]int{{1, 2, 3}, {4, 5, 6}}

	for _, row := range m {
		row[0] = 99
	}

	fmt.Println("rangeValues:", m)

	s := [][3]int{{7, 8, 9}}

	for _, row := range s {
		row[1] = 99
	}

	fmt.Println("rangeSlice:", s)

	deep := [2][2][3]int{{{1, 2, 3}, {4, 5, 6}}, {{7, 8, 9}, {10, 11, 12}}}

	for _, plane := range deep {
		plane[0][0] = 99
	}

	fmt.Println("rangeDeep:", deep)
}

func rangeNamed() {
	rows := []Row{{1, 2, 3}, {4, 5, 6}}

	for _, r := range rows {
		r[0] = 99
	}

	fmt.Println("rangeNamed:", rows[0][0], rows[1][0])
}

func rangeHeapBoxed() {
	m := [2][3]int{{1, 2, 3}, {4, 5, 6}}

	for _, row := range m {
		p := &row
		p[0] = 88
	}

	fmt.Println("rangeHeapBoxed:", m)
}

func rangeAssignExisting() {
	m := [2][3]int{{1, 2, 3}, {4, 5, 6}}
	var row [3]int
	var i int

	for i, row = range m {
		row[2] = 99
	}

	fmt.Println("rangeAssignExisting:", m, row, i)
}

func mapKeyRange() {
	mk := map[[2]int]string{{1, 2}: "v"}

	for k := range mk {
		k[0] = 99
	}

	fmt.Println("mapKeyRange:", mk[[2]int{1, 2}])
}

// Composite-literal elements copy their array values in (gap class: composite literal).
func compositeElements() {
	a := [3]int{1, 2, 3}
	b := [3]int{4, 5, 6}

	m := [2][3]int{a, b}
	m[0][0] = 99
	fmt.Println("compositeArray:", a, m[0])

	s := [][3]int{a, b}
	s[1][0] = 99
	fmt.Println("compositeSlice:", b, s[1])
}

func compositeStructFields() {
	a := [3]int{1, 2, 3}

	s1 := holder{arr: a}
	s1.arr[0] = 99
	fmt.Println("structKeyed:", a, s1.arr)

	s2 := holder{a}
	a[1] = 88
	fmt.Println("structPositional:", a, s2.arr)
}

func compositeMapValueAndKey() {
	a := [3]int{1, 2, 3}
	mv := map[string][3]int{"x": a}
	a[0] = 99
	fmt.Println("mapValue:", mv["x"])

	k := [2]int{1, 2}
	mk := map[[2]int]string{k: "kv"}
	k[0] = 99
	fmt.Println("mapKeyLiteral:", mk[[2]int{1, 2}])
}

func compositeSparseAndAny() {
	a := [3]int{1, 2, 3}
	sp := [4][3]int{2: a}
	a[1] = 88
	fmt.Println("sparse:", sp[2])

	b := [3]int{7, 8, 9}
	lst := []any{b}
	b[0] = 99
	got := lst[0].([3]int)
	fmt.Println("anyBoxed:", got)
}

// Returned array values are copies (gap class: return).
func returnCopies() {
	r := leakLocal()
	(*leaked)[0] = 99
	fmt.Println("returnLeak:", r, *leaked)

	h := holder{arr: [3]int{5, 6, 7}}
	g := h.get()
	g[0] = 99
	fmt.Println("returnField:", h.arr, g)

	nr := leakRow()
	(*leakedRow)[1] = 99
	fmt.Println("returnNamed:", nr[0], nr[1], nr[2], (*leakedRow)[1])
}

// Array parameters are per-call copies — including NAMED arrays and value receivers
// (gap class: named array types), and nested arrays copy DEEP (multidimensional).
func paramCopies() {
	a := [3]int{1, 2, 3}
	modDirect(a)
	fmt.Println("paramDirect:", a)

	nr := Row{4, 5, 6}
	modNamed(nr)
	fmt.Println("paramNamed:", nr[0])

	m := [2][3]int{{1, 2, 3}, {4, 5, 6}}
	modDeep(m)
	fmt.Println("paramDeep:", m)

	nr.mut()
	fmt.Println("recvValue:", nr[0])
}

func funcLitParam() {
	a := [3]int{1, 2, 3}
	fl := func(x [3]int) {
		x[0] = 99
	}
	fl(a)
	fmt.Println("funcLitParam:", a)
}

// Named-array assignment/var-decl copies, and interface boxing of array values
// at declaration positions (gap classes: named arrays at assignment, any-boxing).
func namedAssignCopies() {
	nr := Row{1, 2, 3}
	d := nr
	d[0] = 99
	fmt.Println("namedAssign:", nr[0], d[0])

	var e Row = nr
	e[1] = 88
	fmt.Println("namedVarDecl:", nr[1], e[1])

	var x any = nr
	nr[2] = 77
	got := x.(Row)
	fmt.Println("namedAnyBoxed:", got[2], nr[2])

	a := [3]int{5, 6, 7}
	var y any = a
	a[0] = 99
	gotA := y.([3]int)
	fmt.Println("directAnyBoxed:", gotA[0], a[0])
}

// A channel send copies the array value at the send (audit: channel send).
func channelSend() {
	a := [3]int{1, 2, 3}
	ch := make(chan [3]int, 1)
	ch <- a
	a[0] = 99
	got := <-ch
	fmt.Println("channelSend:", got, a)
}

// append copies an array element into the new slot (audit: append).
func appendElement() {
	a := [3]int{1, 2, 3}
	var s [][3]int
	s = append(s, a)
	a[0] = 99
	fmt.Println("appendElement:", s[0], a)
}

func main() {
	rangeValues()
	rangeNamed()
	rangeHeapBoxed()
	rangeAssignExisting()
	mapKeyRange()
	compositeElements()
	compositeStructFields()
	compositeMapValueAndKey()
	compositeSparseAndAny()
	returnCopies()
	paramCopies()
	funcLitParam()
	namedAssignCopies()
	channelSend()
	appendElement()
}
