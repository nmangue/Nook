using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using System.Linq;
using System.Collections.Immutable;

namespace Nook.CodeAnalysis;

public partial class UseStoreGenerator
{
    private const string FromServicesAttributeFqn = "global::Microsoft.AspNetCore.Mvc.FromServicesAttribute";
    private static readonly string ActionAttributeFqn = $"global::Nook.Core.ActionAttribute";
    private static readonly string InjectAttributeFqn = $"global::Nook.Core.InjectAttribute";

    private static IInfos? GetStoreInfos(GeneratorSyntaxContext context)
    {
        if (context.Node is ClassDeclarationSyntax targetStoreCds)
        {
            var stateIns = GetStateClass(targetStoreCds);
            if (stateIns != null &&
                context.SemanticModel.GetDeclaredSymbol(targetStoreCds) is INamedTypeSymbol classSymbol &&
                context.SemanticModel.GetSymbolInfo(stateIns).Symbol is INamedTypeSymbol stateSymbol)
            {
                return new StoreDeclaration(classSymbol, stateSymbol);
            }
        }
        return null;
    }

    private static IdentifierNameSyntax? GetStateClass(ClassDeclarationSyntax storeImplementationCds)
    {
        var storeBaseNode = GetStoreBaseNode(storeImplementationCds)!;
        return storeBaseNode.TypeArgumentList.Arguments.SingleOrDefault() is IdentifierNameSyntax stateIns ? stateIns : null;
    }

    private static IInfos? GetActionInfos(GeneratorSyntaxContext context)
    {
        if (context.Node is MethodDeclarationSyntax mds)
        {
            if (context.SemanticModel.GetDeclaredSymbol(mds) is IMethodSymbol ms &&
                ms.HasAttribute(ActionAttributeFqn))
            {
                var storeType = ms.ReceiverType as INamedTypeSymbol;
                var stateType = ms.ReturnType as INamedTypeSymbol;

                var isAsync = false;
                if (stateType?.TypeArguments.Length == 1 &&
                    stateType.GetFullyQualifiedName().StartsWith("Task<"))
                {
                    stateType = stateType.TypeArguments[0] as INamedTypeSymbol;
                    isAsync = true;
                }

                if (storeType != null && stateType != null)
                {
                    var parameters = ms.Parameters
                        .Select(p => new ActionParameterInfos(p, ShouldParameterBeBound(p)))
                        .ToImmutableList();

                    return new ActionDeclaration(ms, storeType, stateType, isAsync, parameters);
                }
            }
        }
        return null;
    }

    private static bool ShouldParameterBeBound(IParameterSymbol p)
        => p.HasAttribute(FromServicesAttributeFqn) ||
           p.HasAttribute(InjectAttributeFqn);
}
