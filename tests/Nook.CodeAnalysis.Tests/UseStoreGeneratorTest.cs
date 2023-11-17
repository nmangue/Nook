using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Nook.SourceGenerators.Tests.TestCases;
using NFluent;
using Nook.Core;

namespace Nook.CodeAnalysis.Tests;

public class UseStoreGeneratorTest
{

    [Theory]
    [InlineData(MinimalStore.Input, MinimalStore.ExpectedOutput)]
    [InlineData(SimpleState.Input, SimpleState.ExpectedOutput)]
    [InlineData(AsyncAction.Input, AsyncAction.ExpectedOutput)]
    public void Test(string input, string expectedOutput)
    {
        Check.That(GetGeneratedOutput(input)).IsEqualTo(expectedOutput);
    }

    private static string? GetGeneratedOutput(string sourceCode)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        var references = AppDomain.CurrentDomain.GetAssemblies()
                                  .Concat(new[] { typeof(Store<>).Assembly })
                                  .Where(assembly => !assembly.IsDynamic)
                                  .Select(assembly => MetadataReference
                                                      .CreateFromFile(assembly.Location))
                                  .Cast<MetadataReference>();

        var compilation = CSharpCompilation.Create("SourceGeneratorTests",
                      new[] { syntaxTree },
                      references,
                      new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new UseStoreGenerator();

        CSharpGeneratorDriver.Create(generator)
                             .RunGeneratorsAndUpdateCompilation(compilation,
                                                                out var outputCompilation,
                                                                out var diagnostics);

        return outputCompilation.SyntaxTrees.Skip(1).LastOrDefault()?.ToString();
    }
}