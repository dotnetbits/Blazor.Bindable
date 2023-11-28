using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Collections.Immutable;
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
            var typeString = classSymbol.TypeParameters.Any() ? BuildTypeParameterString(classSymbol.TypeParameters) : "";

            var source = new StringBuilder($@"#nullable enable
// Do not modify generated file
namespace {namespaceName}
{{
    public partial class {classSymbol.Name}{typeString}
    {{
");
            CreateEventCallbackAndCurrentInstanceForBindables(properties, source);

            source.Append(@"
    }
}
#nullable restore");
            return source.ToString();
        }

        private string BuildTypeParameterString(ImmutableArray<ITypeParameterSymbol> typeParameters)
        {
            var sb = new StringBuilder();
            sb.Append("<");
            foreach (var typeParameter in typeParameters)
            {
                sb.Append(typeParameter.Name);
                sb.Append(", ");
            }
            sb.Remove(sb.Length - 2, 2);
            sb.Append(">");
            return sb.ToString();
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