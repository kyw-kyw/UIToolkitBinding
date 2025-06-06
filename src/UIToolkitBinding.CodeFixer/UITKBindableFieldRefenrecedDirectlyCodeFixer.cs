using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Composition;
using UIToolkitBinding.Analyzers;

namespace UIToolkitBinding.CodeFixer;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UITKBindableFieldRefenrecedDirectlyCodeFixer)), Shared]
public sealed class UITKBindableFieldRefenrecedDirectlyCodeFixer : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
        DiagnosticDescriptors.BindableFieldReferencedDirectlyId);

    public override FixAllProvider? GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root == null) return;

        foreach (Diagnostic diagnostic in context.Diagnostics)
        {
            SyntaxNode? diagnosticTargetNode = root.FindNode(diagnostic.Location.SourceSpan);
            if (diagnostic.Id != DiagnosticDescriptors.BindableFieldReferencedDirectlyId) continue;

            if (diagnostic.Properties[UITKBindableFieldRefenrecedDirectlyAnalyzer.fieldNameKey] is not string fieldName
                || diagnostic.Properties[UITKBindableFieldRefenrecedDirectlyAnalyzer.propertyNameKey] is not string propertyName) continue;

            if (diagnosticTargetNode is IdentifierNameSyntax { Identifier.Text: string identifierName } identifierNameSyntax
                && identifierName == fieldName)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        "Reference property",
                        ct => UpdateReference(context.Document, identifierNameSyntax, propertyName, ct),
                        "UITKBindableFieldRefenrecedDirectlyAnalyzer.ReferenceProperty"),
                    diagnostic);
            }
        }
    }

    static async Task<Document> UpdateReference(Document document, IdentifierNameSyntax fieldReference, string propertyName, CancellationToken cancellationToken)
    {
        IdentifierNameSyntax propertyReference = SyntaxFactory.IdentifierName(propertyName);
        SyntaxNode originalRoot = await fieldReference.SyntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(false);
        SyntaxTree updatedTree = originalRoot.ReplaceNode(fieldReference, propertyReference).SyntaxTree;

        return document.WithSyntaxRoot(await updatedTree.GetRootAsync(cancellationToken).ConfigureAwait(false));
    }
}
