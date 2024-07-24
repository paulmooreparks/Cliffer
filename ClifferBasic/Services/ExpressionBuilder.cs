﻿using ClifferBasic.Model;

namespace ClifferBasic.Services;

internal class ExpressionBuilder {
    Tokenizer _tokenizer;
    ExpressionParser _expressionParser;
    VariableStore _variableStore;

    public ExpressionBuilder(
        Tokenizer tokenizer,
        ExpressionParser expressionParser,
        VariableStore variableStore) 
    { 
        _tokenizer = tokenizer;
        _expressionParser = expressionParser;
        _variableStore = variableStore;
    }

    internal Expression? BuildExpression(IEnumerable<string> args) {
        var tokens = _tokenizer.Tokenize(args);
        var expression = _expressionParser.Parse(tokens);
        return expression;
    }

    internal Expression? BuildExpression() {
        var expression = _expressionParser.Parse();
        return expression;
    }
}