using Microsoft.CodeAnalysis.CSharp;

namespace UIToolkitBinding.Core;

internal abstract record UITKBindableMemberContext
{
    public abstract string Type { get; init; }
    public abstract string FieldName { get; init; }
    public abstract string PropertyName { get; init; }
    public abstract DeclaredAccessibility DeclaredAccessibility { get; init; }
    public abstract SetterAccessibility SetterAccessibility { get; init; }

    public string GetFieldExpressionInGetAccessor()
    {
        if (FieldName == "value") return FieldName;

        if (SyntaxFacts.GetKeywordKind(FieldName) != SyntaxKind.None
            || SyntaxFacts.GetContextualKeywordKind(FieldName) != SyntaxKind.None)
        {
            return $"@{FieldName}";
        }
        return FieldName;
    }

    public string GetFieldExpressionInSetAccessor()
    {
        if (FieldName == "value") return $"this.{FieldName}";

        if (SyntaxFacts.GetKeywordKind(FieldName) != SyntaxKind.None
            || SyntaxFacts.GetContextualKeywordKind(FieldName) != SyntaxKind.None)
        {
            return $"@{FieldName}";
        }
        return FieldName;
    }

}
