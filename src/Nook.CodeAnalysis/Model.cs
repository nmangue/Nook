using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Nook.CodeAnalysis;

public interface IInfos
{
    INamedTypeSymbol StoreType { get; }
    INamedTypeSymbol StateType { get; }
}

public record StoreDeclaration(
    INamedTypeSymbol StoreType,
    INamedTypeSymbol StateType) : IInfos;

public record ActionDeclaration(
    IMethodSymbol MethodSymbol,
    INamedTypeSymbol StoreType,
    INamedTypeSymbol StateType,
    bool IsAsync,
    IReadOnlyList<ActionParameterInfos> Parameters) : IInfos;

public record ActionParameterInfos(
    IParameterSymbol ParameterSymbol,
    bool BindFromService);
