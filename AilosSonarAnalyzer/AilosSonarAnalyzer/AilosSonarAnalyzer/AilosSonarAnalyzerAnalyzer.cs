using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace AilosSonarAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AilosSonarAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        private const string Category = "Naming";
        public const string DiagnosticId = "AilosSonarPlugin";

        private static readonly DiagnosticDescriptor AppSettingsRule = new DiagnosticDescriptor(
            "AppSettingsRule",
            "Avoid Literal Values in appsettings.json",
            "Avoid using literal values in appsettings.json. Consider using Azure Key Vault instead.",
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true
        );

        private static readonly DiagnosticDescriptor GetRule = new DiagnosticDescriptor(
            "GetRule",
            "Invalid Method Name (GET)",
            "Avoid using '{0}' in GET methods.",
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true
        );

        private static readonly DiagnosticDescriptor PostRule = new DiagnosticDescriptor(
            "PostRule",
            "Invalid Method Name (POST)",
            "Avoid using '{0}' in POST methods.",
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true
        );

        private static readonly DiagnosticDescriptor DeleteRule = new DiagnosticDescriptor(
            "DeleteRule",
            "Invalid Method Name (DELETE)",
            "Avoid using '{0}' in DELETE methods.",
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true
        );

        private static readonly DiagnosticDescriptor ExcludeFromCodeCoverageRule = new DiagnosticDescriptor(
            "ExcludeFromCodeCoverageRule",
            "Avoid using [ExcludeFromCodeCoverage]",
            "Avoid using [ExcludeFromCodeCoverage] attribute.",
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true
        );
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(AppSettingsRule, GetRule, PostRule, DeleteRule, ExcludeFromCodeCoverageRule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

            context.RegisterSyntaxNodeAction(AnalyzeMethodDeclaration, SyntaxKind.MethodDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeObjectCreationExpression, SyntaxKind.ObjectCreationExpression);
            context.RegisterSyntaxNodeAction(AnalyzeAttribute, SyntaxKind.Attribute);
        }

        private void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
        {
            var methodList = new List<string>
            {
                "HttpGet",
                "HttpDelete",
                "HttpPost"
            };
            var methodDeclaration = (MethodDeclarationSyntax)context.Node;

            var attributeListSyntax = methodDeclaration.AttributeLists.ToList();
            var attributes = attributeListSyntax
                .Select(d => d.Attributes)
                .ToList();
            var attributeList = attributes
                .Select(d => d.Select(c => c.Name.ToString())).ToList();

            var myAttrList = new List<string>();
            foreach ( var attr in attributeList)
            {
                myAttrList.Add(attr.First());
            }
            var w = attributeList
                .Where(e => methodList.Contains(""));

            var methodName = methodDeclaration.Identifier.Text;

            // Rule: Check if the method name contains "search" or "list" in GET methods
            if (myAttrList.Any(e => e.Contains("HttpGet")))
            {
                var keywordsToAvoid = new[] { "Search", "List" };
                if (keywordsToAvoid.Any(keyword => methodName.Contains(keyword)))
                {
                    var diagnostic = Diagnostic.Create(GetRule, methodDeclaration.Identifier.GetLocation(), methodName);
                    context.ReportDiagnostic(diagnostic);
                }
            }

            // Rule: Check if the method name contains "delete" or "exclude" in DELETE methods
            if (myAttrList.Any(e => e.Contains("HttpDelete")))
            {
                var keywordsToAvoid = new[] { "Delete", "Exclude" };
                if (keywordsToAvoid.Any(keyword => methodName.Contains(keyword)))
                {
                    var diagnostic = Diagnostic.Create(DeleteRule, methodDeclaration.Identifier.GetLocation(), methodName);
                    context.ReportDiagnostic(diagnostic);
                }
            }

            // Rule: Check if the method name contains verbs to avoid in POST methods
            if (myAttrList.Any(e => e.Contains("HttpPost")))
            {
                var keywordsToAvoid = new[] { "Include", "Register" };
                if (keywordsToAvoid.Any(keyword => methodName.Contains(keyword)))
                {
                    var diagnostic = Diagnostic.Create(PostRule, methodDeclaration.Identifier.GetLocation(), methodName);
                    context.ReportDiagnostic(diagnostic: diagnostic);
                }
            }
        }
        private void AnalyzeObjectCreationExpression(SyntaxNodeAnalysisContext context)
        {
            var objectCreationExpression = (ObjectCreationExpressionSyntax)context.Node;

            // Rule: Check if the object creation is for IConfigurationBuilder
            if (objectCreationExpression.Type.ToString() == "IConfigurationBuilder")
            {
                var argumentList = objectCreationExpression.ArgumentList;
                if (argumentList != null)
                {
                    // Check if the argument list contains any string literals
                    foreach (var argument in argumentList.Arguments)
                    {
                        if (argument.Expression.Kind() == SyntaxKind.StringLiteralExpression)
                        {
                            var diagnostic = Diagnostic.Create(AppSettingsRule, argument.GetLocation());
                            context.ReportDiagnostic(diagnostic);
                        }
                    }
                }
            }
        }
        private void AnalyzeAttribute(SyntaxNodeAnalysisContext context)
        {
            var attribute = (AttributeSyntax)context.Node;
            var attributeName = attribute.Name.ToString();

            // Rule: Check if the attribute is [ExcludeFromCodeCoverage]
            if (attributeName == "ExcludeFromCodeCoverage")
            {
                var diagnostic = Diagnostic.Create(ExcludeFromCodeCoverageRule, attribute.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
