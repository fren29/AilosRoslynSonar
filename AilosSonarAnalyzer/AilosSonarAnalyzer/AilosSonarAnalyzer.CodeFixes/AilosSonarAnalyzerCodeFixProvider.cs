using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;


namespace AilosSonarAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AilosSonarAnalyzerCodeFixProvider)), Shared]
    public class AilosSonarAnalyzerCodeFixProvider : CodeFixProvider
    {
        public const string DiagnosticId = "ExcludeFromCodeCoverageRule";

        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            foreach (var diagnostic in context.Diagnostics)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: "Remove [ExcludeFromCodeCoverage]",
                        cancellationToken => RemoveAttributeAsync(context.Document, diagnostic.Location, cancellationToken),
                        nameof(AilosSonarAnalyzerCodeFixProvider)),
                    diagnostic);
            }

            return Task.CompletedTask;
        }

        private static async Task<Document> RemoveAttributeAsync(Document document, Location location, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var attributeNode = root.FindNode(location.SourceSpan).FirstAncestorOrSelf<AttributeSyntax>();

            var newRoot = root.RemoveNode(attributeNode, SyntaxRemoveOptions.KeepNoTrivia);

            return document.WithSyntaxRoot(newRoot);
        }
    }
}
