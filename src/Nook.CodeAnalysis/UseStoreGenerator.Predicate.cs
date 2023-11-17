using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nook.CodeAnalysis;

public partial class UseStoreGenerator
{
    private static bool IsActionMethod(SyntaxNode node)
        => node is MethodDeclarationSyntax mds &&
            mds.GetAllAttributes().Any(a => a.Name.ToString().EndsWith("Action"));

    public static bool IsClassExtendingStore(SyntaxNode syntaxNode)
        => GetStoreBaseNode(syntaxNode) != null;

    public static GenericNameSyntax? GetStoreBaseNode(SyntaxNode node)
    {
        if (node is ClassDeclarationSyntax cds && cds.BaseList != null)
        {
            foreach (var baseType in cds.BaseList.Types.OfType<SimpleBaseTypeSyntax>())
            {
                if (baseType.Type is GenericNameSyntax gns && gns.Identifier.Text.EndsWith("Store"))
                {
                    return gns;
                }
            }
        }
        return null;
    }
}
