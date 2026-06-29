package main

import "fmt"

type profBuf struct{ data []int }

// A capture-mode (direct-ж) method — it takes the address of its own field, so it is emitted with
// the box AS its receiver (`this ж<profBuf> Ꮡb`).
func (b *profBuf) push(v int) { appendInt(&b.data, v) }

func appendInt(s *[]int, v int) { *s = append(*s, v) }

func (b *profBuf) sum() int {
	t := 0
	for _, x := range b.data {
		t += x
	}
	return t
}

type cpuProfile struct {
	count int      // a VALUE field — taking its address heap-boxes the global (mirrors `lock mutex`)
	log   *profBuf // a POINTER field — its value is already a ж<profBuf>
}

// A package global whose address is taken (via &cpuprof.count below), so it is heap-boxed. Mirrors
// runtime's `var cpuprof cpuProfile`.
var cpuprof cpuProfile

func incr(p *int) { *p++ }

func run() {
	incr(&cpuprof.count) // address of a value field -> cpuprof is heap-boxed (Ꮡcpuprof)

	// A direct-ж method called on the POINTER field `log` of the boxed global. `cpuprof.log` is
	// already a ж<profBuf>, so the method binds to it directly — `cpuprof.log.push(...)`. The defect
	// took its address (`Ꮡcpuprof.of(cpuProfile.Ꮡlog)`), double-boxing to ж<ж<profBuf>> (CS1929).
	cpuprof.log.push(cpuprof.count)
	cpuprof.log.push(cpuprof.count)
}

func main() {
	cpuprof.log = &profBuf{}
	run()
	fmt.Println(cpuprof.count)     // 1
	fmt.Println(cpuprof.log.sum()) // 1 + 1 = 2
}
