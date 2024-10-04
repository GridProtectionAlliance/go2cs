//******************************************************************************************************
//  Stack.go - Gbtc
//
//  Copyright © 2024, Grid Protection Alliance.	 All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  09/21/2024 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

package stack

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
