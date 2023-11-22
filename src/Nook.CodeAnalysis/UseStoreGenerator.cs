using Microsoft.CodeAnalysis;

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
