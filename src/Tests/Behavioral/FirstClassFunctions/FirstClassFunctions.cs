namespace go;

using fmt = fmt_package;
using rand = math.rand_package;

public static partial class main_package {

private const nint win = 100;

private const nint gamesPerSeries = 10;

private static nint totalGames;

[GoType("struct")]
private partial struct score {
    public nint player;
}

private delegate (score result, bool turnIsOver) action(score current);

private static (score, bool) roll(score s) {
    nint outcome = rand.Intn(6) + 1;
    if (outcome == 1) {
        return (new score(s.opponent, s.player, 0), true);
    }

    return (new score(s.player, s.opponent, outcome + s.thisTurn), false);
}

private static (score, bool) stay(score s) {
    return (new score(s.opponent, s.player + s.thisTurn, 0), true);
}

private delegate action strategy();

private static strategy stayAtK(nint k) {
    return (score s) => {
        if (s.thisTurn >= k) {
            return stay;
        }

        return roll;
    };
}

private static nint play(strategy strategy0, strategy strategy1) {
    var strategies = new strategy[] {strategy0, strategy1 }.slice();
    score s;

    bool turnIsOver;

    nint currentPlayer = rand.Intn(2);
    while (s.player + s.thisTurn < win) {
        var action = strategies[currentPlayer](s);        /* visitAssignStmt mismatch: s, turnIsOver = action(s) */

        if (turnIsOver) {
            currentPlayer = (currentPlayer + 1) % 2;
        }

    }

    return currentPlayer;
}

private static (slice<nint>, nint) roundRobin(slice<strategy> strategies) => func((defer, recover) => {
    var wins = make(nint[], len(strategies));
    for (nint i = 0; i < len(strategies); i++) {
        defer(() => {
            {
                var r = recover(); if (r != null) {
                    fmt.Println("Recovered in roundRobin", r);
                }
            }
            totalGames++;
        });
        for (nint j = i + 1; j < len(strategies); j++) {
            for (nint k = 0; k < gamesPerSeries; k++) {
                nint winner = play(strategies[i], strategies[j]);
                if (winner == 0){
                    wins[i]++;
                } else {
                    wins[j]++;
                }

            }

        }

    }

    nint gamesPerStrategy = gamesPerSeries * (len(strategies) - 1);
    return (wins, gamesPerStrategy);
}
);
private static @string ratioString(params nint[] vals) {
    nint total = 0;    /* visitRangeStmt: for _, val := range vals {
	total += val
} */

    @string s = ""u8;    /* visitRangeStmt: for _, val := range vals {
	if s != "" {
		s += ", "
	}
	pct := 100 * float64(val) / float64(total)
	s += fmt.Sprintf("%!d(MISSING)/%!d(MISSING) (%!f(MISSING)%)", val, total, pct)
} */

    return s;
}

private static void Main() {
    var strategies = make(strategy[], win);    /* visitRangeStmt: for k := range strategies {
	strategies[k] = stayAtK(k + 1)
} */
    /* visitAssignStmt mismatch: wins, games := roundRobin(strategies) */
    /* visitRangeStmt: for k := range strategies {
	fmt.Printf("Total ganes=%!d(MISSING), Wins, losses staying at k =%!d(MISSING): %!s(MISSING)\n",
		totalGames, k+1, ratioString(wins[k], games-wins[k]))
} */

}

} // end main_package
