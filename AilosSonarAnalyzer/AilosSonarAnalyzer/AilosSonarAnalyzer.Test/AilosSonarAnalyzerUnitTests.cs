using System.Threading.Tasks;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = AilosSonarAnalyzer.Test.CSharpCodeFixVerifier<
    AilosSonarAnalyzer.AilosSonarAnalyzerAnalyzer,
    AilosSonarAnalyzer.AilosSonarAnalyzerCodeFixProvider>;

namespace AilosSonarAnalyzer.Test
{

    [TestClass]
    public class AilosSonarAnalyzerUnitTest
    {
        private static readonly DiagnosticDescriptor ExcludeFromCodeCoverageRule = new DiagnosticDescriptor(
            "ExcludeFromCodeCoverageRule",
            "Avoid using [ExcludeFromCodeCoverage]",
            "Avoid using [ExcludeFromCodeCoverage] attribute.",
            "Naming",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true
        );

        private static readonly DiagnosticDescriptor GetRule = new DiagnosticDescriptor(
            "GetRule",
            "Invalid Method Name (GET)",
            "Avoid using '{0}' in GET methods.",
            "Naming",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true
        );

        private static readonly DiagnosticDescriptor PostRule = new DiagnosticDescriptor(
            "PostRule",
            "Invalid Method Name (POST)",
            "Avoid using '{0}' in POST methods.",
            "Naming",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true
        );

        private static readonly DiagnosticDescriptor DeleteRule = new DiagnosticDescriptor(
            "DeleteRule",
            "Invalid Method Name (DELETE)",
            "Avoid using '{0}' in DELETE methods.",
            "Naming",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true
        );

        [TestMethod]
        public async Task TestExcludeFromCodeCoverageRule()
        {
            var a = new VerifyCS.Test();
            a.ReferenceAssemblies = a.ReferenceAssemblies.WithPackages(ImmutableArray.Create(new PackageIdentity[] { new PackageIdentity("Microsoft.AspNetCore.Mvc.Core", "2.2.5") }));

            var test = @"
using System.Diagnostics.CodeAnalysis;

public class TestClass
{
    [ExcludeFromCodeCoverage]
    public void Method()
    {
    }
}";

            var expected = new DiagnosticResult(ExcludeFromCodeCoverageRule)
                .WithLocation("/0/Test0.cs", 6, 6)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithMessage("Avoid using [ExcludeFromCodeCoverage] attribute.");

            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }

        [TestMethod]
        public async Task TestNonExcludedAttribute()
        {
            var test = @"
using System;

public class TestClass
{
    [Obsolete]
    public void Method()
    {
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [TestMethod]
        public async Task TestInvalidMethodNameInGetMethod()
        {
            var test = @"
using Microsoft.AspNetCore.Mvc;

public class TestClass
{
    [HttpGet]
    public void SearchData()
    {
    }
}";

            var expected = new DiagnosticResult(GetRule)
                .WithLocation("/0/Test0.cs", 7, 17) // Use the virtual file path
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithMessage("Avoid using 'SearchData' in GET methods.");

            await GenerateAnalyzerTest(test, expected).RunAsync();
        }

        [TestMethod]
        public async Task TestInvalidMethodNameInPostMethod()
        {
            var test = @"
using Microsoft.AspNetCore.Mvc;

public class TestClass
{
    [HttpPost]
    public void IncludeData()
    {
    }
}";

            var expected = new DiagnosticResult(PostRule)
                .WithLocation("/0/Test0.cs", 7, 17) // Use the virtual file path
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithMessage("Avoid using 'IncludeData' in POST methods.");

            await GenerateAnalyzerTest(test, expected).RunAsync();
        }

        [TestMethod]
        public async Task TestInvalidMethodNameInDeleteMethod()
        {

            var test = @"
using Microsoft.AspNetCore.Mvc;

public class TestClass
{
    [HttpDelete]
    public void ExcludeData()
    {
    }
}";
            var expected = new DiagnosticResult(DeleteRule)
                .WithLocation("/0/Test0.cs", 7, 17) // Use the virtual file path
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithMessage("Avoid using 'ExcludeData' in DELETE methods.");

            await GenerateAnalyzerTest(test, expected).RunAsync();
        }

        [TestMethod]
        public async Task TestValidMethodNameInGetMethod()
        {
            var test = @"
using Microsoft.AspNetCore.Mvc;

public class TestClass
{
    [HttpGet]
    public void GetData()
    {
    }
}";

            await GenerateAnalyzerTest(test).RunAsync();
        }

        [TestMethod]
        public async Task TestValidMethodNameInPostMethod()
        {
            var test = @"
using Microsoft.AspNetCore.Mvc;

public class TestClass
{
    [HttpPost]
    public void AddData()
    {
    }
}";

            await GenerateAnalyzerTest(test).RunAsync();
        }

        [TestMethod]
        public async Task TestValidMethodNameInDeleteMethod()
        {
            var test = @"
using Microsoft.AspNetCore.Mvc;

public class TestClass
{
    [HttpDelete]
    public void RemoveData()
    {
    }
}";

            await GenerateAnalyzerTest(test).RunAsync();
        }

        private static VerifyCS.Test GenerateAnalyzerTest(string sourceCode, DiagnosticResult expected)
        {
            var referenceAssemblies = ReferenceAssemblies.Default
                         .AddPackages(ImmutableArray.Create(
                             new PackageIdentity("Microsoft.AspNetCore.Mvc.Core", "2.1.3"))
                         );


            var httpRequestAnalyzerTest = new VerifyCS.Test
            {
                ReferenceAssemblies = referenceAssemblies,
                TestState =
                {
                    Sources = {sourceCode},
                    ExpectedDiagnostics = {expected}
                },
            };

            return httpRequestAnalyzerTest;
        }
        private static VerifyCS.Test GenerateAnalyzerTest(string sourceCode)
        {
            var referenceAssemblies = ReferenceAssemblies.Default
                         .AddPackages(ImmutableArray.Create(
                             new PackageIdentity("Microsoft.AspNetCore.Mvc.Core", "2.1.3"))
                         );


            var httpRequestAnalyzerTest = new VerifyCS.Test
            {
                ReferenceAssemblies = referenceAssemblies,
                TestState =
                {
                    Sources = {sourceCode},
                },
            };

            return httpRequestAnalyzerTest;
        }
    }
}
