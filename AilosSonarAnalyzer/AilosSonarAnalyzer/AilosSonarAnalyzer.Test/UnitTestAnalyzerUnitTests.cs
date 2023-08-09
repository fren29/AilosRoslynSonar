using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using VerifyCS = AilosSonarAnalyzer.Test.CSharpCodeFixVerifier<
    AilosSonarAnalyzer.AilosSonarAnalyzerAnalyzer,
    AilosSonarAnalyzer.AilosSonarAnalyzerCodeFixProvider>;

namespace UnitTestAnalyzer.Test
{
    [TestClass]
    public class UnitTestAnalyzerUnitTests
    {
        // Diagnostic Ids
        private const string TrueDiagnosticId = "TrueAnalyzerRule";
        private const string FalseDiagnosticId = "FalseAnalyzerRule";

        private static readonly DiagnosticDescriptor RuleTrue = new DiagnosticDescriptor(
            TrueDiagnosticId,
            "Avoid using Assert.True(true)",
            "Consider removing the redundant assertion.",
            "Testing",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Using Assert.True(true) is redundant in xUnit tests.");

        private static readonly DiagnosticDescriptor RuleFalse = new DiagnosticDescriptor(
            FalseDiagnosticId,
            "Avoid using Assert.False(false)",
            "Consider removing the redundant assertion.",
            "Testing",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Using Assert.False(false) is redundant in xUnit tests.");

        [TestMethod]
        public async Task NoDiagnosticWhenNoRedundantAssertTrue()
        {
            var test = @"
using Xunit;

public class TestClass
{
    [Fact]
    public void TestMethod()
    {
        Assert.True(true);
        Assert.True(true);
        Assert.True(false);
    }
}";
            var expected = new DiagnosticResult(RuleTrue)
                .WithLocation("/0/Test0.cs", 6, 6)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithMessage("Avoid using [ExcludeFromCodeCoverage] attribute.");

            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }

        [TestMethod]
        public async Task RedundantAssertTrueProducesDiagnostic()
        {
            var test = @"
using Xunit;

public class TestClass
{
    [Fact]
    public void TestMethod()
    {
        Assert.True(true);
    }
}";

            var expected = new DiagnosticResult(RuleTrue)
                .WithLocation("/0/Test0.cs", 6, 6)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithMessage("Avoid using [ExcludeFromCodeCoverage] attribute.");

            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }

        [TestMethod]
        public async Task RedundantAssertTrueGetsFixed()
        {
            var test = @"
using Xunit;

public class TestClass
{
    [Fact]
    public void TestMethod()
    {
        Assert.False(false);
    }
}";

            var expected = new DiagnosticResult(RuleFalse)
                .WithLocation("/0/Test0.cs", 6, 6)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithMessage("Avoid using [ExcludeFromCodeCoverage] attribute.");

            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }
    }
}
