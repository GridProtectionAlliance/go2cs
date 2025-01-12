using System.Collections.Generic;
using System.IO;
using Antlr4.Runtime;

// JRC: Code cleaned up compared to posted base class
// ReSharper disable InconsistentNaming
#pragma warning disable CA1050 // Declare types in namespaces

public abstract class GoParserBase : Parser
{
    protected GoParserBase(ITokenStream input)
        : base(input)
    {
    }

    protected GoParserBase(ITokenStream input, TextWriter output, TextWriter errorOutput)
        : base(input, output, errorOutput)
    {
    }

    /// <summary>
    /// Returns `true` if on the current index of the parser's
    /// token stream a token exists on the `HIDDEN` channel which
    /// either is a line terminator, or is a multi line comment that
    /// contains a line terminator.
    /// </summary>
    protected bool lineTerminatorAhead()
    {
        // Get the token ahead of the current index.
        int offset = 1;
        int possibleIndexEosToken = CurrentToken.TokenIndex - offset;

        if (possibleIndexEosToken == -1)
            return true;

        IToken ahead = tokenStream.Get(possibleIndexEosToken);

        // JRC: This deviates from posted code that fixes eos detection issues
        if (ahead.Channel == Lexer.Hidden)
        {
            while (ahead.Channel == Lexer.Hidden)
            {
                switch (ahead.Type)
                {
                    case GoLexer.TERMINATOR:
                        return true;
                    case GoLexer.WS:
                        possibleIndexEosToken = CurrentToken.TokenIndex - ++offset;
                        ahead = tokenStream.Get(possibleIndexEosToken);
                        break;
                    case GoLexer.COMMENT:
                    case GoLexer.LINE_COMMENT:
                    {
                        if (ahead.Text.Contains('\r') || ahead.Text.Contains('\n'))
                            return true;

                        possibleIndexEosToken = CurrentToken.TokenIndex - ++offset;
                        ahead = tokenStream.Get(possibleIndexEosToken);
                        break;
                    }
                }
            }
        }
        else
        { 
            return noTerminatorBetween(1);
        }

        return false;
    }

    /// <summary>
    /// Returns `true` if no line terminator exists between the specified
    /// token offset and the prior one on the `HIDDEN` channel.
    /// </summary>
    protected bool noTerminatorBetween(int tokenOffset)
    {
        BufferedTokenStream stream = (BufferedTokenStream)tokenStream;
        IList<IToken> tokens = stream.GetHiddenTokensToLeft(LT(stream, tokenOffset).TokenIndex);

        if (tokens == null)
            return true;

        foreach (IToken token in tokens)
        {
            if (token.Text.Contains('\n'))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Returns `true` if no line terminator exists after any encountered
    /// parameters beyond the specified token offset and the next on the
    /// `HIDDEN` channel.
    /// </summary>
    protected bool noTerminatorAfterParams(int tokenOffset)
    {
        BufferedTokenStream stream = (BufferedTokenStream)tokenStream;
        int leftParams = 1;
        int rightParams = 0;

        if (LT(stream, tokenOffset).Type == GoLexer.L_PAREN)
        {
            // Scan past parameters
            while (leftParams != rightParams)
            {
                tokenOffset++;
                int tokenType = LT(stream, tokenOffset).Type;

                switch (tokenType)
                {
                    case GoLexer.L_PAREN:
                        leftParams++;
                        break;
                    case GoLexer.R_PAREN:
                        rightParams++;
                        break;
                }
            }

            tokenOffset++;
            return noTerminatorBetween(tokenOffset);
        }

        return true;
    }

    protected bool checkPreviousTokenText(string text) => 
        LT(tokenStream, 1).Text?.Equals(text) ?? false;

    private static IToken LT(ITokenStream stream, int k) => 
        stream.LT(k);

    private ITokenStream tokenStream => TokenStream;
}
