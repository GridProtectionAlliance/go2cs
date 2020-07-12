/*
package main

import (
	"fmt"
	"strings"
)

func main() {
	// Create a tic-tac-toe board.
	board := [][]string{
		[]string{"_", "_", "_"},
		[]string{"_", "_", "_"},
		[]string{"_", "_", "_"},
	}

	// The players take turns.
	board[0][0] = "X"
	board[2][2] = "O"
	board[1][2] = "X"
	board[1][0] = "O"
	board[0][2] = "X"

	for i := 0; i < len(board); i++ {
		fmt.Printf("%s\n", strings.Join(board[i], " "))
	}
}
*/
#region source
using go;
using fmt = go.fmt_package;
using strings = go.strings_package;
using static go.builtin;

static class main_package
{
    static void Main() {
        // Create a tic-tac-toe board.
		var board = slice(new[]{
			slice(new @string[]{"_", "_", "_"}),
			slice(new @string[]{"_", "_", "_"}),
			slice(new @string[]{"_", "_", "_"}),
        });

		// The players take turns.
		board[0][0] = "X";
		board[2][2] = "O";
		board[1][2] = "X";
		board[1][0] = "O";
		board[0][2] = "X";
		
        for (var i = 0; i < len(board); i++) {
            fmt.Printf("{0}\n", strings.Join(board[i], " "));
        }
    }
}
#endregion