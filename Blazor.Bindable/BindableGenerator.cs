using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Blazor.Bindable
{
    [Generator]
    internal class BindableGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxContextReceiver is not SyntaxReceiver receiver)
                return;

            GroupPropertiesByClassAndGenerateSource(context, receiver);
        }

        private void GroupPropertiesByClassAndGenerateSource(GeneratorExecutionContext context, SyntaxReceiver receiver)
        {
            foreach (var group in receiver.Properties.GroupBy<IPropertySymbol, INamedTypeSymbol>(f => f.ContainingType, SymbolEqualityComparer.Default))
            {
                var classSource = ProcessClass(group.Key, group.ToList());
                context.AddSource($"{group.Key.Name}.Bindable.g.cs", SourceText.From(classSource, Encoding.UTF8));
            }
        }

        private string ProcessClass(INamedTypeSymbol classSymbol, List<IPropertySymbol> properties)
        {
            var namespaceName = classSymbol.ContainingNamespace.ToDisplayString();

            var source = new StringBuilder($@"
// Do not modify generated file
namespace {namespaceName}
{{
    public partial class {classSymbol.Name}
    {{
");
            CreateEventCallbackAndCurrentInstanceForBindables(properties, source);

            source.Append(@"
    }
}");
            return source.ToString();
        }

        private void CreateEventCallbackAndCurrentInstanceForBindables(List<IPropertySymbol> properties, StringBuilder source)
        {
            foreach (var propertySymbol in properties)
                AddBindingPropertiesToSource(source, propertySymbol);
        }

        private void AddBindingPropertiesToSource(StringBuilder source, IPropertySymbol propertySymbol)
        {
            var propertyName = propertySymbol.Name;
            var propertyType = propertySymbol.Type;

            source.Append(BindableHelpers.EventCallbackText(propertyName, propertyType));
            source.Append(BindableHelpers.CurrentInstanceText(propertyName, propertyType));
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        class SyntaxReceiver : ISyntaxContextReceiver
        {
            public List<IPropertySymbol> Properties { get; } = new List<IPropertySymbol>();

            public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
            {
                if (context.Node is PropertyDeclarationSyntax propertyDeclarationSyntax
                    && propertyDeclarationSyntax.AttributeLists.Count > 0)
                {
                    var propertySymbol = propertyDeclarationSyntax.GetPropertySymbol(context);
                    if (propertySymbol.HasAttribute("Microsoft.AspNetCore.Components.BindableAttribute"))
                        Properties.Add(propertySymbol);
                }
            }
        }
    }
}