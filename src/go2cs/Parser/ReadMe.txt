The Golang.g4 Antlr grammar file, from https://github.com/antlr/grammars-v4/tree/master/golang, has been
modified from the original to compile with C#: java -jar antlr-4.7.1-complete.jar -Dlanguage=CSharp

Specific grammar and embedded code changes:

@parser::members {
    /// <summary>
    /// Determines if on the current index of the parser's token stream a token exists on the
    /// <c>HiddenChannel</c> which either is a line terminator, or is a multi line comment that
    /// contains a line terminator. Also checks for <c>LineCommentChannel</c> which will always
	/// indicate that a line terminator exists.
    /// </summary>
    /// <returns>
    /// <c>true</c> if on the current index of the parser's token stream a token exists on the
    /// <c>HiddenChannel</c> which either is a line terminator, or is a multi line comment that
    /// contains a line terminator; otherwise, <c>false</c>.
    /// </returns>
    private bool LineTerminatorAhead()
    {
        // Get the token ahead of the current index.
        int possibleIndexEosToken = CurrentToken.TokenIndex - 1;

        IToken ahead = TokenStream.Get(possibleIndexEosToken);

        if (ahead == null)
            return false;

        if (ahead.Channel != TokenConstants.HiddenChannel && ahead.Channel != GolangLexer.LineCommentChannel)
        {
            // We're only interested in tokens on the HIDDEN channels.
            return false;
        }

        if (ahead.Type == TERMINATOR || ahead.Type == LINE_COMMENT)
        {
            // There is definitely a line terminator ahead.
            return true;
        }

        if (ahead.Type == WS)
        {
            // Get the token ahead of the current whitespace.
            possibleIndexEosToken = CurrentToken.TokenIndex - 2;
            ahead = TokenStream.Get(possibleIndexEosToken);
        }

        // Get the token's text and type.
        String text = ahead.Text;
        int type = ahead.Type;

        // Check if the token is, or contains a line terminator
        return type == LINE_COMMENT || (type == COMMENT && text.IndexOfAny(new[] { '\r', '\n' }) >= 0) || type == TERMINATOR;
    }

    /// <summary>
    /// Determines if no line terminator exists between the specified
    /// <paramref name="tokenOffset"/> and the prior one on the hidden
    /// channel.
    /// </summary>
    /// <param name="tokenOffset">Starting token offset.</param>
    /// <returns>
    /// <c>true</c>  if no line terminator exists between the specified
    /// <paramref name="tokenOffset"/> and the prior one on the hidden
    /// channel; otherwise, <c>false</c>.
    /// </returns>
	private bool noTerminatorBetween(int tokenOffset)
	{
		BufferedTokenStream stream = TokenStream as BufferedTokenStream;		
        IList<IToken> tokens = stream.GetHiddenTokensToLeft(stream.LT(tokenOffset).TokenIndex);

        if (tokens == null)
            return true;

		foreach (IToken token in tokens)
		{
            if (token.Text.Contains("\n"))
                return false;
		}

		return true;
	}

    /// <summary>
    /// Determines if no line terminator exists after any encountered
    /// parameters beyond the specified <paramref name="tokenOffset"/>
    /// and the next on the hidden channel.
    /// </summary>
    /// <param name="tokenOffset">Starting token offset.</param>
    /// <returns>
    /// <c>true</c> if no line terminator exists after any encountered
    /// parameters beyond the specified <paramref name="tokenOffset"/>
    /// and the next on the hidden channel; otherwise, <c>false</c>.
    /// </returns>
    private bool noTerminatorAfterParams(int tokenOffset)
    {
		BufferedTokenStream stream = TokenStream as BufferedTokenStream;
        int leftParams = 1;
        int rightParams = 0;

        if (stream.LT(tokenOffset).Text == "(")
        {
            // Scan past parameters
            while (leftParams != rightParams)
            {
                tokenOffset++;
                string value = stream.LT(tokenOffset).Text;

                if (value == "(")
                    leftParams++;
                else if (value == ")")
                    rightParams++;
            }

            tokenOffset++;
            return noTerminatorBetween(tokenOffset);
        }
        
        return true;
    }
}

@lexer::members {
	// Line comment channel
	public const int LineCommentChannel = 2;

    // The most recently produced token
    private IToken lastToken = null;

    /// <summary>
    /// Return the next token from the character stream and records this last token
    /// in case it resides on the default channel. This recorded token is used to
    /// determine when the lexer could possibly match a regex literal.
    /// </summary>
    /// <remarks>
    /// Return a token from this source; i.e., match a token on the char stream.
    /// </remarks>
    public override IToken NextToken()
    {
        // Get the next token.
        IToken next = base.NextToken();

        // Keep track of the last token on the default channel
        if (next.Channel == TokenConstants.DefaultChannel)
            this.lastToken = next;

        return next;
    }
}

//raw_string_lit         = "`" { unicode_char | newline } "`" .
fragment RAW_STRING_LIT
    : '`' ( UNICODE_CHAR | NEWLINE | [~`] )*? '`'
    ;

eos
    : ';'
    | EOF
    | {LineTerminatorAhead()}?
    | {TokenStream.LT(1).Text.Equals("}", StringComparison.Ordinal)}?
    ;

COMMENT
	:   '/*' .*? '*/' [\r\n]* -> channel(HIDDEN)
	;

LINE_COMMENT
    :   [ \t]* '//' ~[\r\n]* [\r\n]+ -> channel(2)
    ;