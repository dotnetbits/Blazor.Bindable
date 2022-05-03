using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Blazor.Bindable
{
    internal static class SymbolHelper
    {
        public static IPropertySymbol GetPropertySymbol(this PropertyDeclarationSyntax propertyDeclarationSyntax, GeneratorSyntaxContext context)
            => context.SemanticModel.GetDeclaredSymbol(propertyDeclarationSyntax) as IPropertySymbol;

        public static bool HasAttribute(this ISymbol symbol, string attributeClass)
            => symbol.GetAttributes().Any(at => at.AttributeClass.ToDisplayString() == attributeClass);
    }
}