namespace go;

partial class main_package {

public const nint N = /* 11 + 1 */ 12;
internal static slice<rune> board = slice<rune>(
    (@string)"""
...........
...........
....●●●....
....●●●....
..●●●●●●●..
..●●●○●●●..
..●●●●●●●..
....●●●....
....●●●....
...........
...........

""");
internal static nint center;
[GoInit] internal static void init() {
    println("init fn 1");
    nint n = 0;
    foreach (var (pos, field) in board) {
        if (field == '○') {
            center = pos;
            n++;
        }
    }
    if (n != 1) {
        center = -1;
    }
}

[GoInit] internal static void initΔ1() {
    println("init fn 2");
}

internal static nint moves;
internal static bool move(nint pos, nint dir) {
    moves++;
    if (board[pos] == '●' && board[pos + dir] == '●' && board[pos + 2 * dir] == '○') {
        board[pos] = '○';
        board[pos + dir] = '○';
        board[pos + 2 * dir] = '●';
        return true;
    }
    return false;
}

internal static void unmove(nint pos, nint dir) {
    board[pos] = '●';
    board[pos + dir] = '●';
    board[pos + 2 * dir] = '○';
}

internal static bool solve() {
    nint last = default!;
    nint n = default!;
    foreach (var (pos, field) in board) {
        if (field == '●') {
            foreach (var (_, dir) in new nint[]{-1, -N, +1, +N}.array()) {
                if (move(pos, dir)) {
                    if (solve()) {
                        unmove(pos, dir);
                        println(((@string)board));
                        return true;
                    }
                    unmove(pos, dir);
                }
            }
            last = pos;
            n++;
        }
    }
    if (n == 1 && (center < 0 || last == center)) {
        println(((@string)board));
        return true;
    }
    return false;
}

internal static void Main() {
    if (!solve()) {
        println("no solution found");
    }
    println(moves, "moves tried");
}

} // end main_package
