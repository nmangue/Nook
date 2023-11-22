﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Nook.CodeAnalysis.Language;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Nook.CodeAnalysis;

[Generator]
public class RazorStatePropGenerator : IIncrementalGenerator
{
    private record RazorInfos(string ClassName, string Namespace, string? StoreName);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Retrieves all store declarations as reference
        var stores = context.SyntaxProvider.CreateSyntaxProvider(
               static (node, ct) => node.IsClassExtendingStore(),
               static (context, _) => UseStoreGenerator.GetStoreInfos(context) as StoreDeclaration
           )
           .WhereNotNull()
           .Collect();

        // Retrieves all razor components that inherits from Nook.AspNetCore.ComponentUsing
        var razorFiles = context.AdditionalTextsProvider
            .Where(static file => file.Path.EndsWith(".razor", StringComparison.OrdinalIgnoreCase))
            .Combine(context.AnalyzerConfigOptionsProvider)
            .Select(static (tuple, ct) =>
            {
                var (file, optionsProvider) = tuple;

                var text = file.GetText(ct);

                var storeClass = GetInherits(text?.Lines);

                if (string.IsNullOrEmpty(storeClass))
                {
                    return null;
                }

                var className = GetClassNameFromPath(file.Path);

                var options = optionsProvider.GetOptions(file);
                if (!options.TryGetValue("build_property.rootnamespace", out var rootNamespace) ||
                    !options.TryGetValue("build_property.projectdir", out var projectDir))
                {
                    return null;
                }

                var ns = GetNamespaceFromPath(file.Path, projectDir, rootNamespace);

                return new RazorInfos(className, ns, storeClass);
            })
            .WhereNotNull();

        var input = razorFiles.Combine(stores);

        context.RegisterSourceOutput(input, static (spc, pair) =>
        {
            var (razorFile, stores) = pair;
            var storeDeclaration = Find(stores, razorFile.StoreName);

            if (storeDeclaration != null)
            {
                var stateTypeFqn = storeDeclaration.StateType.GetFullyQualifiedName();
                var storeTypeFqn = storeDeclaration?.StoreType.GetFullyQualifiedName();

                spc.AddSource($"{razorFile.Namespace}.{razorFile.ClassName}.g.cs", $$"""
                    // <auto-generated/>
                    #nullable enable

                    namespace {{razorFile.Namespace}};

                    public partial class {{razorFile.ClassName}}
                    {
                        public {{stateTypeFqn}} State => ((global::Nook.Core.Use<{{storeTypeFqn}}>)Store).Instance.CurrentState;
                    }
                    """);
            }
        });
    }

    private static StoreDeclaration? Find(ImmutableArray<StoreDeclaration> stores, string? storeNameSuffix)
    {
        StoreDeclaration? storeDeclaration = null;
        if (!string.IsNullOrEmpty(storeNameSuffix))
        {
            storeDeclaration = stores.SingleOrDefault(sd => sd.StoreType.ToString().EndsWith(storeNameSuffix));
        }

        return storeDeclaration;
    }

    internal static string GetClassNameFromPath(string path)
    {
        var className = Path.GetFileNameWithoutExtension(path);
        className = CSharpIdentifier.SanitizeIdentifier(className);
        return className;
    }

    internal static string GetNamespaceFromPath(string filePath, string projectDir, string? rootNamespace)
    {
        var fileDir = Path.GetDirectoryName(filePath);
        var relativeDir = PathUtils.GetRelativePath(projectDir, fileDir);

        var nsBuilder = new StringBuilder(rootNamespace ?? "Razor");

        // Check if the relativeDir is not simply the current dir "."
        // Or a parent direction ".." (the latter should not happen)
        if (!string.IsNullOrEmpty(relativeDir) && relativeDir[0] != '.')
        {
            const char namespaceSeparatorChar = '.';

            var nsIdentifiers = relativeDir
                .TrimEnd(Path.DirectorySeparatorChar)
                .Split(Path.DirectorySeparatorChar)
                .Select(CSharpIdentifier.SanitizeIdentifier);

            foreach (var identifier in nsIdentifiers)
            {
                nsBuilder.Append(namespaceSeparatorChar);
                nsBuilder.Append(identifier);
            }
        }

        return nsBuilder.ToString();
    }

    private static readonly Regex InheritsRegex = new (@"^\s*@inherits\s+[\w:\.]*ComponentUsing<([\w:\.]+)>", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    internal static string? GetInherits(IEnumerable<TextLine>? lines)
    {
        if (lines != null)
        {
            foreach (var line in lines)
            {
                var storeType = GetInherits(line.ToString());
                if (storeType != null)
                {
                    return storeType;
                }
            }
        }
        return null;
    }

    internal static string? GetInherits(string? text)
    {
        if (string.IsNullOrEmpty(text)) return null;

        var match = InheritsRegex.Match(text);
        return match.Success ? match.Groups[1].Value : null;
    }
}
