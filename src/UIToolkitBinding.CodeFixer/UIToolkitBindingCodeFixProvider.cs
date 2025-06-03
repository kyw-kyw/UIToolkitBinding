using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using System.Collections.Immutable;
using System.Composition;
using UIToolkitBinding.Analyzers;

namespace UIToolkitBinding;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UIToolkitBindingCodeFixProvider)), Shared]
public sealed class UIToolkitBindingCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            DiagnosticDescriptors.MustBePartialId,
            DiagnosticDescriptors.UnnecessaryDataSourceAttributeId,
            DiagnosticDescriptors.UnnecessaryBindableFieldAttributeId,
            DiagnosticDescriptors.DontCreatePropertyAttributeShouldBeGivenId);

    public override FixAllProvider? GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false) as CompilationUnitSyntax;
        if (root is null) return;

        var model = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
        if (model is null) return;

        foreach (Diagnostic diagnostic in context.Diagnostics)
        {
            SyntaxNode? diagnosticTargetNode = root.FindNode(diagnostic.Location.SourceSpan);
            switch (diagnostic.Id)
            {
                case DiagnosticDescriptors.DontCreatePropertyAttributeShouldBeGivenId:
                    if (diagnosticTargetNode?.AncestorsAndSelf().FirstOrDefault(n => n is FieldDeclarationSyntax) is FieldDeclarationSyntax fieldDeclaration)
                    {
                        context.RegisterCodeFix(
                            CodeAction.Create(
                                "Add DontCreatePropertyAttribute",
                                ct => AddDontCreatePropertyAttributeAsync(context.Document, fieldDeclaration, ct),
                                "UIToolkitBindingAnalyzer.AddDontCreatePropertyAttribute"),
                            diagnostic);
                    }
                    break;
                case DiagnosticDescriptors.MustBePartialId when diagnosticTargetNode is BaseTypeDeclarationSyntax typeDeclarationSyntax:
                    context.RegisterCodeFix(
                            CodeAction.Create(
                                "Add partial modifier",
                                ct => AddPartialModifierAsync(context.Document, root, typeDeclarationSyntax, diagnostic, ct),
                                "UIToolkitBindingAnalyzer.AddPartialModifier"),
                            diagnostic);
                    break;
                case DiagnosticDescriptors.UnnecessaryDataSourceAttributeId when diagnosticTargetNode is BaseTypeDeclarationSyntax typeDeclarationSyntax:
                    context.RegisterCodeFix(
                            CodeAction.Create(
                                "Remove RemoveUITKDataSourceObjectAttribute",
                                ct => RemoveUITKDataSourceObjectAttributeAsync(context.Document, typeDeclarationSyntax, ct),
                                "UIToolkitBindingAnalyzer.RemoveUITKDataSourceObjectAttribute"),
                            diagnostic);
                    break;
                case DiagnosticDescriptors.UnnecessaryBindableFieldAttributeId when diagnosticTargetNode is AttributeSyntax attributeSyntax:
                    context.RegisterCodeFix(
                            CodeAction.Create(
                                "Remove UITKBindableFieldAttribute",
                                ct => RemoveUITKBindableFieldAttributeAsync(context.Document, attributeSyntax, ct),
                                "UIToolkitBindingAnalyzer.RemoveUITKBindableFieldAttribute"),
                            diagnostic);
                    break;
            }
        }
    }

    static async Task<Solution> AddDontCreatePropertyAttributeAsync(Document document, FieldDeclarationSyntax fieldDecl, CancellationToken cancellationToken)
    {
        SolutionEditor solutionEditor = new(document.Project.Solution);
        DocumentEditor documentEditor = await solutionEditor.GetDocumentEditorAsync(document.Id, cancellationToken);
        SyntaxGenerator syntaxGenerator = SyntaxGenerator.GetGenerator(documentEditor.OriginalDocument);
        documentEditor.AddAttribute(fieldDecl, syntaxGenerator.Attribute(AttributeConstants.DontCreatePropertyAttribute));
        return solutionEditor.GetChangedSolution();
    }

    static Task<Document> AddPartialModifierAsync(Document document, SyntaxNode syntaxRoot, BaseTypeDeclarationSyntax typeDecl, Diagnostic diagnostic, CancellationToken cancellationToken)
    {
        SyntaxNode? modifiedSyntax = syntaxRoot;
        if (TryAddPartialModifier(typeDecl, out BaseTypeDeclarationSyntax? modified))
        {
            modifiedSyntax = modifiedSyntax.ReplaceNode(typeDecl, modified!);
        }

        foreach (Location addlLocation in diagnostic.AdditionalLocations)
        {
            BaseTypeDeclarationSyntax? addlType = modifiedSyntax.FindNode(addlLocation.SourceSpan) as BaseTypeDeclarationSyntax;
            if (addlType is not null && TryAddPartialModifier(addlType, out modified))
            {
                modifiedSyntax = modifiedSyntax.ReplaceNode(addlType, modified!);
            }
        }

        document = document.WithSyntaxRoot(modifiedSyntax);
        return Task.FromResult(document);

        static bool TryAddPartialModifier(BaseTypeDeclarationSyntax typeDeclaration, out BaseTypeDeclarationSyntax? modified)
        {
            if (typeDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword))
            {
                modified = null;
                return false;
            }

            modified = typeDeclaration.AddModifiers(SyntaxFactory.Token(SyntaxKind.PartialKeyword));
            return true;
        }
    }

    static async Task<Document> RemoveUITKDataSourceObjectAttributeAsync(Document document, BaseTypeDeclarationSyntax typeDeclaration, CancellationToken cancellationToken)
    {
        SolutionEditor solutionEditor = new(document.Project.Solution);
        DocumentEditor documentEditor = await solutionEditor.GetDocumentEditorAsync(document.Id, cancellationToken);
        AttributeSyntax? dataSourceAttribute = null;
        foreach (var attributeList in typeDeclaration.AttributeLists)
        {
            dataSourceAttribute = attributeList.Attributes.FirstOrDefault(static x => x.Name.ToFullString() is "UIToolkitBinding.UITKDataSourceObject" or "UITKDataSourceObject");
            if (dataSourceAttribute != null) break;
        }
        if (dataSourceAttribute != null)
        {
            if (dataSourceAttribute.Parent is AttributeListSyntax attributeListSyntax)
            {
                if (attributeListSyntax.Attributes.Count == 1)
                {
                    documentEditor.RemoveNode(attributeListSyntax);
                }
                else
                {
                    documentEditor.RemoveNode(dataSourceAttribute);
                }
            }
        }
        return documentEditor.GetChangedDocument();
    }

    static async Task<Document> RemoveUITKBindableFieldAttributeAsync(Document document, AttributeSyntax attributeSyntax, CancellationToken cancellationToken)
    {
        SolutionEditor solutionEditor = new(document.Project.Solution);
        DocumentEditor documentEditor = await solutionEditor.GetDocumentEditorAsync(document.Id, cancellationToken);
        if (attributeSyntax.Parent is AttributeListSyntax attributeListSyntax)
        {
            if (attributeListSyntax.Attributes.Count == 1)
            {
                documentEditor.RemoveNode(attributeListSyntax);
            }
            else
            {
                documentEditor.RemoveNode(attributeSyntax);
            }
        }

        return documentEditor.GetChangedDocument();
    }
}
