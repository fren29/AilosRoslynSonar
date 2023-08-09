using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UnitTestAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class UnitTestAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticIdTrue = "TrueAnalyzerRule";
        public const string DiagnosticIdFalse = "FalseAnalyzerRule";

        private static readonly DiagnosticDescriptor RuleTrue = new DiagnosticDescriptor(
            DiagnosticIdTrue,
            "Avoid using Assert.True(true)",
            "Consider removing the redundant assertion.",
            "Testing",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Using Assert.True(true) is redundant in xUnit tests.");

        private static readonly DiagnosticDescriptor RuleFalse = new DiagnosticDescriptor(
            DiagnosticIdFalse,
            "Avoid using Assert.False(false)",
            "Consider removing the redundant assertion.",
            "Testing",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Using Assert.False(false) is redundant in xUnit tests.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(RuleTrue, RuleFalse);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();

            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;

            if (IsXUnitAssertMethod(invocation))
            {
                var methodSymbol = context.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;

                if (methodSymbol != null && methodSymbol.ContainingNamespace.ToString().StartsWith("Xunit"))
                {
                    if (IsRedundantTrueAssertion(invocation))
                    {
                        var diagnostic = Diagnostic.Create(RuleTrue, invocation.GetLocation());
                        context.ReportDiagnostic(diagnostic);
                    }
                    else if (IsRedundantFalseAssertion(invocation))
                    {
                        var diagnostic = Diagnostic.Create(RuleFalse, invocation.GetLocation());
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }

        private static bool IsXUnitAssertMethod(InvocationExpressionSyntax invocation)
        {
            var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;
            if (memberAccess != null)
            {
                var identifier = memberAccess.Name as IdentifierNameSyntax;
                if (identifier != null)
                {
                    var methodName = identifier.Identifier.ValueText;
                    return (methodName == "True" || methodName == "False") &&
                           memberAccess.Expression is IdentifierNameSyntax methodInvocation &&
                           methodInvocation.Identifier.ValueText == "Assert";
                }
            }
            return false;
        }

        private static bool IsRedundantTrueAssertion(InvocationExpressionSyntax invocation)
        {
            var argumentList = invocation.ArgumentList;
            if (argumentList.Arguments.Count != 1)
            {
                return false;
            }

            var argumentExpression = argumentList.Arguments[0].Expression;
            return argumentExpression.IsKind(SyntaxKind.TrueLiteralExpression);
        }

        private static bool IsRedundantFalseAssertion(InvocationExpressionSyntax invocation)
        {
            var argumentList = invocation.ArgumentList;
            if (argumentList.Arguments.Count != 1)
            {
                return false;
            }

            var argumentExpression = argumentList.Arguments[0].Expression;
            return argumentExpression.IsKind(SyntaxKind.FalseLiteralExpression);
        }
    }
}
