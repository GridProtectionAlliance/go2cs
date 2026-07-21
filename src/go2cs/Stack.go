// Stack.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

package main

// Stack represents a stack data structure
type Stack[T any] struct {
	items []T
}

func NewStack[T any](items ...T) *Stack[T] {
	return &Stack[T]{items: items}
}

func (s *Stack[T]) Len() int {
	return len(s.items)
}

func (s *Stack[T]) IsEmpty() bool {
	return s.Len() == 0
}

func (s *Stack[T]) At(index int) T {
	return s.items[index]
}

// Push adds an item to the to the top of the stack
func (s *Stack[T]) Push(item T) {
	// For simplicity in implementation, the bottom is the first element
	s.items = append(s.items, item)
}

// Pop removes and returns the item from the top of the stack
func (s *Stack[T]) Pop() T {
	item, ok := s.TryPop()

	if !ok {
		panic("stack is empty")
	}

	return item
}

// TryPop attempts to remove and return the item from the top of the stack
func (s *Stack[T]) TryPop() (T, bool) {
	if s.IsEmpty() {
		var zero T
		return zero, false
	}

	index := len(s.items) - 1
	item := s.items[index]
	s.items = s.items[:index]

	return item, true
}

// Peek returns the item from the top of the stack without removing it
func (s *Stack[T]) Peek() (T, bool) {
	if s.IsEmpty() {
		var zero T
		return zero, false
	}

	return s.items[len(s.items)-1], true
}
