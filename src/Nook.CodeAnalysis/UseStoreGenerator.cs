using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using RazorBlade.Analyzers;
using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;

namespace Nook.CodeAnalysis;

[Generator]
public partial class UseStoreGenerator : IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
        var source = context.SyntaxProvider.CreateSyntaxProvider(
                static (node, ct) => node.IsClassExtendingStore() || IsActionMethod(node),
                static (context, _) => GetStoreInfos(context) ?? GetActionInfos(context)
            )
            .WhereNotNull()
            .Collect()
            .GroupByStore();

        

        context.RegisterSourceOutput(source, GenerateCode);
	}
}

[Generator]
public partial class PlopGenerator : IIncrementalGenerator
{
    private record RazorFile(string ClassName, string Namespace, string ProjectDir, ISymbol State);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Récupérer tous les fichiers RAZOR avec
        // Le nom de classe
        // Le namespace
        //      Calculer le namespace à partir des règles
        
        // Récupérer tous les stores et leur state 

        // => Combine
        var input = context.AdditionalTextsProvider
            .Where(static i => i.Path.EndsWith(".razor", StringComparison.OrdinalIgnoreCase))
            .Combine(context.AnalyzerConfigOptionsProvider)
            .Select(static (tuple, ct) =>
            {
                var (i, optionsProvider) = tuple;

                var options = optionsProvider.GetOptions(i);
                if (!options.TryGetValue("build_property.rootnamespace", out var ns))
                {
                    ns = string.Join("#", options.Keys);
                }
                return new { i.Path, Namespace = ns, Content = i.GetText(ct)!.ToString() };
            })
            .WhereNotNull()
            .Collect();

        context.RegisterSourceOutput(input, (spc, input) =>
        {
            spc.AddSource($"Test.g.cs", $@"
    public static partial class ConstStrings
    {{
        /// <summary>
        /// {string.Join(" | ", input.Select(i => Path.GetFileNameWithoutExtension(i.Path)+"@"+i.Namespace))}
        /// </summary>
        public static string MyValue {{ get; }} = ""Hello3"";
    }}");
        });
    }
}

public partial class StatePropGeneratorI : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var globalOptions = context.ParseOptionsProvider
                                   .Combine(EmbeddedLibrarySourceGenerator.EmbeddedLibraryProvider(context))
                                   .Select(static (pair, _) =>
                                   {
                                       var (parseOptions, embeddedLibrary) = pair;

                                       return new GlobalOptions(
                                           (CSharpParseOptions)parseOptions,
                                           embeddedLibrary
                                       );
                                   });

        var inputFiles = context.AdditionalTextsProvider
                                .Where(static i => i.Path.EndsWith(".razor", StringComparison.OrdinalIgnoreCase))
                                .Combine(context.AnalyzerConfigOptionsProvider)
                                .Select(static (pair, _) => GetInputFile(pair.Left, pair.Right))
                                .WhereNotNull();

        context.RegisterSourceOutput(
            inputFiles.Combine(globalOptions)
                      .Combine(context.CompilationProvider)
                      .WithLambdaComparer((a, b) => a.Left.Equals(b.Left), pair => pair.Left.GetHashCode()), // Ignore the compilation for updates
            static (context, pair) =>
            {

                var ((inputFile, globalOptions), compilation) = pair;

                try
                {
                    context.AddSource($"{inputFile.ClassName}1.txt", "Hello"); // inputFile.AdditionalText.Path + " | " + inputFile.Namespace);
                    //Generate(context, inputFile, globalOptions, compilation);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                }
            }
        );
    }

    private static void Generate(SourceProductionContext context, InputFile file, GlobalOptions globalOptions, Compilation compilation)
    {
        var sourceText = file.AdditionalText.GetText();
        if (sourceText is null)
            return;

        var csharpDoc = GenerateRazorCode(sourceText, file, globalOptions);

    }


    private static RazorCSharpDocument GenerateRazorCode(SourceText sourceText, InputFile file, GlobalOptions globalOptions)
        => GenerateRazorCode(sourceText.ToString(), sourceText.Encoding ?? Encoding.UTF8, file.AdditionalText.Path);

    internal static RazorCSharpDocument GenerateRazorCode(string sourceText, Encoding encoding, string path)
    {
        var engine = RazorProjectEngine.Create(
            RazorConfiguration.Default,
            Invoker.EmptyRazorProjectFileSystem,
            cfg =>
            {
                /*
                ModelDirective.Register(cfg);

                cfg.SetCSharpLanguageVersion(globalOptions.ParseOptions.LanguageVersion);

                var configurationFeature = cfg.Features.Where(f => f.GetType().FullName == "Microsoft.AspNetCore.Razor.Language.DefaultDocumentClassifierPassFeature").Single();

                configurationFeature.ConfigureNamespace.Add((codeDoc, node) =>
                {
                    node.Content = NamespaceVisitor.GetNamespaceDirectiveContent(codeDoc)
                                   ?? file.Namespace
                                   ?? "Razor";
                });

                configurationFeature.ConfigureClass.Add((_, node) =>
                {
                    node.ClassName = file.ClassName;
                    node.BaseType = "global::RazorBlade.HtmlTemplate";

                    node.Modifiers.Clear();
                    node.Modifiers.Add("internal");
                    node.Modifiers.Add("partial");

                    // Enable nullable reference types for the class definition node, as they may be needed for the base class.
                    node.Annotations[CommonAnnotations.NullableContext] = CommonAnnotations.NullableContext;
                });

                configurationFeature.ConfigureMethod.Add((_, node) =>
                {
                    node.Modifiers.Clear();
                    node.Modifiers.Add("protected");
                    node.Modifiers.Add("async");
                    node.Modifiers.Add("override");
                });

                cfg.Features.Add(new ErrorOnTagHelperSyntaxTreePass());
                */
            }
        );

        var codeDoc = engine.Process(
            RazorSourceDocument.Create(sourceText, path, encoding),
            FileKinds.Legacy,
            Array.Empty<RazorSourceDocument>(),
            Array.Empty<TagHelperDescriptor>()
        );

        return codeDoc.GetCSharpDocument();
    }

    private static InputFile? GetInputFile(AdditionalText additionalText, AnalyzerConfigOptionsProvider optionsProvider)
    {
        var options = optionsProvider.GetOptions(additionalText);

        options.TryGetValue("build_metadata.AdditionalFiles.IsRazorBlade", out var isTargetFile);
        if (!string.Equals(isTargetFile, bool.TrueString, StringComparison.OrdinalIgnoreCase))
            return null;

        if (!options.TryGetValue("build_metadata.AdditionalFiles.Namespace", out var ns))
            ns = null;

        return new InputFile(
            additionalText,
            ns,
            Invoker.CSharpIdentifier_SanitizeIdentifier(Path.GetFileNameWithoutExtension(additionalText.Path))
        );
    }

    internal record InputFile(AdditionalText AdditionalText, string? Namespace, string ClassName);

    private record GlobalOptions(CSharpParseOptions ParseOptions, ImmutableArray<SyntaxTree> AdditionalSyntaxTrees);
}


