namespace go;

using fmt = fmt_package;
using rand = math.rand_package;
using ꓸꓸꓸnint = System.Span<nint>;

partial class main_package {

private const nint win = 100;
private const nint gamesPerSeries = 10;
[GoType] partial struct score {
    public nint player;
    public nint opponent;
    public nint thisTurn;
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

private delegate action strategy(score _);

private static strategy stayAtK(nint k) {
    return (score s) => {
        if (s.thisTurn >= k) {
            return stay;
        }
        return roll;
    };
}

private static nint play(strategy strategy0, strategy strategy1) {
    var strategies = new strategy[]{strategy0, strategy1}.slice();
    score s = default!;
    bool turnIsOver = default!;
    nint currentPlayer = rand.Intn(2);
    while (s.player + s.thisTurn < win) {
        var action = strategies[currentPlayer](s);
        (s, turnIsOver) = action(s);
        if (turnIsOver) {
            currentPlayer = (currentPlayer + 1) % 2;
        }
    }
    return currentPlayer;
}

private static (slice<nint>, nint) roundRobin(slice<strategy> strategies) {
    var wins = new slice<nint>(len(strategies));
    for (nint i = 0; i < len(strategies); i++) {
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

private static @string ratioString(params ꓸꓸꓸnint valsʗp) {
    var vals = valsʗp.slice();

    nint total = 0;
    foreach (var (_, val) in vals) {
        total += val;
    }
    @string s = ""u8;
    foreach (var (_, val) in vals) {
        if (s != ""u8) {
            s += ", "u8;
        }
        var pct = 100 * ((float64)val) / ((float64)total);
        s += fmt.Sprintf("%d/%d (%0.1f%%)"u8, val, total, pct);
    }
    return s;
}

private static void Main() {
    var strategies = new slice<strategy>(win);
    foreach (var (kΔ1, _) in strategies) {
        strategies[kΔ1] = stayAtK(kΔ1 + 1);
    }
    var (wins, games) = roundRobin(strategies);
    nint k = default!;
    foreach (var (iᴛ1, _) in strategies) {
        k = iᴛ1;

        fmt.Printf("Wins, losses staying at k =% 4d: %s\n"u8,
            k + 1, ratioString(wins[k], games - wins[k]));
    }
}

} // end main_package
