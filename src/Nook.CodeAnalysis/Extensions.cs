using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Nook.CodeAnalysis;

internal static class Extensions
{
    public static IncrementalValuesProvider<T> WhereNotNull<T>(this IncrementalValuesProvider<T?> o)
        => o.Where(x => x != null)!;

    public static IncrementalValuesProvider<IGrouping<ISymbol, IInfos>> GroupByStore(this IncrementalValueProvider<ImmutableArray<IInfos>> source)
      => source.SelectMany(
          (all, _) => all.GroupBy<IInfos, ISymbol>(
                  infos => infos.StoreType,
                  SymbolEqualityComparer.Default
              )
            .ToImmutableList()
      );

    public static IEnumerable<AttributeSyntax> GetAllAttributes(this MethodDeclarationSyntax mds)
        => mds.AttributeLists.SelectMany(al => al.Attributes);

    public static IEnumerable<IMethodSymbol> GetMethods(this INamedTypeSymbol namedTypeSymbol) 
        => namedTypeSymbol.GetMembers().OfType<IMethodSymbol>();

    public static bool SymbolEquals(this ITypeSymbol self, ITypeSymbol other) 
        => SymbolEqualityComparer.Default.Equals(self, other);

    public static string GetFullyQualifiedName(this ITypeSymbol nameTypeSymbol)
        => nameTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

    public static void WriteBlankLine(this IndentedTextWriter writer)
        => writer.WriteLineNoTabs(null!);

    public static bool HasFromServiceAttribute(this IParameterSymbol parameterSymbol)
        => parameterSymbol.HasAttribute("global::Microsoft.AspNetCore.Mvc.FromServicesAttribute");

    public static bool HasBindFromServicesAttribute(this IParameterSymbol parameterSymbol)
        => parameterSymbol.HasAttribute("global::Pinia.Core.BindFromServicesAttribute");

    public static bool HasActionAttribute(this IMethodSymbol method)
        => method.HasAttribute("global::Pinia.Core.ActionAttribute");

    public static bool HasAttribute(this ISymbol parameterSymbol, string attributeClassFqn) 
        => parameterSymbol.GetAttributes().Any(a => a.AttributeClass?.GetFullyQualifiedName() == attributeClassFqn);

    public static IDisposable WriteBlock(this IndentedTextWriter writer, string? s)
        => new TextWriteCodeBlock(writer, s);

    private class TextWriteCodeBlock : IDisposable
    {

        public IndentedTextWriter Writer { get; }

        public TextWriteCodeBlock(IndentedTextWriter writer, string? s)
        {
            Writer = writer;
            
            if (s != null)
            {
                writer.WriteLine(s);
            }
            writer.WriteLine('{');
            writer.Indent++;
        }

        public void Dispose()
        {
            Writer.Indent--;
            Writer.WriteLine('}');
        }
    }
}
