﻿using Microsoft.CodeAnalysis;
using Nook.CodeAnalysis.Language;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Nook.CodeAnalysis;

[Generator]
public class RazorStatePropGenerator : IIncrementalGenerator
{
    private record RazorInfos(string ClassName, string Namespace, string? StoreName, string Debug);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var stores = context.SyntaxProvider.CreateSyntaxProvider(
               static (node, ct) => node.IsClassExtendingStore(),
               static (context, _) => UseStoreGenerator.GetStoreInfos(context) as StoreDeclaration
           )
           .WhereNotNull()
           .Collect();

        var razorFiles = context.AdditionalTextsProvider
            .Where(static file => file.Path.EndsWith(".razor", StringComparison.OrdinalIgnoreCase))
            .Combine(context.AnalyzerConfigOptionsProvider)
            .Select(static (tuple, ct) =>
            {
                var (file, optionsProvider) = tuple;

                var text = file.GetText(ct);

                var storeClass = GetInherits(text?.ToString());

                var className = GetClassNameFromPath(file.Path);

                var options = optionsProvider.GetOptions(file);
                if (!options.TryGetValue("build_property.rootnamespace", out var rootNamespace) ||
                    !options.TryGetValue("build_property.projectdir", out var projectDir))
                {
                    return null;
                }

                var debug = storeClass ?? "<null>";

                var ns = GetNamespaceFromPath(file.Path, projectDir, rootNamespace);


                return new RazorInfos(className, ns, storeClass, debug);
            })
            .WhereNotNull();

        var infos = razorFiles.Combine(stores);

        context.RegisterSourceOutput(infos, static (spc, pair) =>
        {
            var (razorFiles, stores) = pair;

            StoreDeclaration? storeDeclaration = null;
            if (!string.IsNullOrEmpty(razorFiles.StoreName))
            {
                storeDeclaration = stores.SingleOrDefault(sd => sd.StoreType.ToString().EndsWith(razorFiles.StoreName));
            }

            spc.AddSource($"{razorFiles.ClassName}.g.cs", $$"""
                // <auto-generated/>
                #nullable enable

                namespace {{razorFiles.Namespace}};

                public partial class {{razorFiles.ClassName}}
                {
                    /*
                    {{razorFiles.Debug}}
                    {{storeDeclaration?.StateType.GetFullyQualifiedName()}}
                    */
                    public const int Value = 42;

                    public {{storeDeclaration?.StateType.GetFullyQualifiedName()}} State => ((global::Nook.Core.Use<{{storeDeclaration?.StoreType.GetFullyQualifiedName()}}>)Store).Instance.CurrentState;
                }
                """);
        });
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

    private static readonly Regex InheritsRegex = new (@"^\s*@inherits\s+[\w:\.]*ComponentUsing<([\w:\.]+)>", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.CultureInvariant);

    internal static string? GetInherits(string? text)
    {
        if (string.IsNullOrEmpty(text)) return null;

        var match = InheritsRegex.Match(text);
        return match.Success ? match.Groups[1].Value : null;
    }
}
