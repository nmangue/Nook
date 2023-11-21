using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Nook.CodeAnalysis;

public partial class UseStoreGenerator
{
    private static bool IsActionMethod(SyntaxNode node)
        => node is MethodDeclarationSyntax mds &&
            mds.GetAllAttributes().Any(a => a.Name.ToString().EndsWith("Action"));
}
