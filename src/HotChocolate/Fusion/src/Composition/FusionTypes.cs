using HotChocolate.Fusion.Composition.Properties;
using HotChocolate.Language;
using HotChocolate.Skimmed;
using HotChocolate.Utilities;
using static HotChocolate.Fusion.Composition.DirectiveArguments;
using DirectiveLocation = HotChocolate.Skimmed.DirectiveLocation;

namespace HotChocolate.Fusion.Composition;

public sealed class FusionTypes
{
    private const string _prefixFormat = "{0}_{1}";
    private const string _typeFormat = "_{0}";
    private const string _variable = "variable";
    private const string _resolver = "resolver";
    private const string _selection = "Selection";
    private const string _selectionSet = "SelectionSet";
    private const string _typeName = "TypeName";
    private const string _type = "Type";
    private const string _source = "source";
    private readonly Schema _fusionGraph;

    public FusionTypes(Schema fusionGraph, string? prefix = null)
    {
        _fusionGraph = fusionGraph;

        Prefix = prefix ?? string.Empty;

        var resolver = string.IsNullOrEmpty(prefix)
            ? _resolver
            : string.Format(_prefixFormat, prefix, _resolver);

        var variable = string.IsNullOrEmpty(prefix)
            ? _variable
            : string.Format(_prefixFormat, prefix, _variable);

        var selection = string.IsNullOrEmpty(prefix)
            ? string.Format(_typeFormat, _selection)
            : string.Format(_prefixFormat, prefix, _selection);

        var selectionSet = string.IsNullOrEmpty(prefix)
            ? string.Format(_typeFormat, _selectionSet)
            : string.Format(_prefixFormat, prefix, _selectionSet);

        var typeName = string.IsNullOrEmpty(prefix)
            ? string.Format(_typeFormat, _typeName)
            : string.Format(_prefixFormat, prefix, _typeName);

        var type = string.IsNullOrEmpty(prefix)
            ? string.Format(_typeFormat, _type)
            : string.Format(_prefixFormat, prefix, _type);

        var source = string.IsNullOrEmpty(prefix)
            ? _source
            : string.Format(_prefixFormat, prefix, _source);

        if (_fusionGraph.ContextData.TryGetValue(nameof(FusionTypes), out var value) &&
            (value is not string prefixValue || !Prefix.EqualsOrdinal(prefixValue)))
        {
            throw new ArgumentException(
                CompositionResources.FusionTypes_EnsureInitialized_Failed,
                nameof(fusionGraph));
        }

        Selection = RegisterScalarType(selection);
        SelectionSet = RegisterScalarType(selectionSet);
        TypeName = RegisterScalarType(typeName);
        Type = RegisterScalarType(type);
        Resolver = RegisterResolverDirectiveType(resolver, SelectionSet, TypeName);
        Variable = RegisterVariableDirectiveType(variable, TypeName, Selection, Type);
        Source = RegisterSourceDirectiveType(source, TypeName);
    }

    private string Prefix { get; }

    public ScalarType Selection { get; }

    public ScalarType SelectionSet { get; }

    public ScalarType TypeName { get; }

    public ScalarType Type { get; }

    public DirectiveType Resolver { get; }

    public DirectiveType Variable { get; }

    public DirectiveType Source { get; }

    private ScalarType RegisterScalarType(string name)
    {
        var selection = new ScalarType(name);
        _fusionGraph.Types.Add(selection);
        return selection;
    }

    public Directive CreateVariableDirective(
        string subGraphName,
        string variableName,
        FieldNode select,
        ITypeNode type)
        => new Directive(
            Variable,
            new Argument(SubGraphArg, subGraphName),
            new Argument(NameArg, variableName),
            new Argument(SelectArg, select.ToString()),
            new Argument(TypeArg, type.ToString()));

    private DirectiveType RegisterVariableDirectiveType(
        string name,
        ScalarType typeName,
        ScalarType selection,
        ScalarType type)
    {
        var directiveType = new DirectiveType(name);
        directiveType.Arguments.Add(new InputField(NameArg, new NonNullType(typeName)));
        directiveType.Arguments.Add(new InputField(SelectArg, new NonNullType(selection)));
        directiveType.Arguments.Add(new InputField(SubGraphArg, new NonNullType(typeName)));
        directiveType.Arguments.Add(new InputField(TypeArg, new NonNullType(type)));
        directiveType.Locations |= DirectiveLocation.Object;
        directiveType.Locations |= DirectiveLocation.FieldDefinition;
        _fusionGraph.DirectiveTypes.Add(directiveType);
        return directiveType;
    }

    public Directive CreateResolverDirective(
        string subGraphName,
        SelectionSetNode select)
        => new Directive(
            Resolver,
            new Argument(SubGraphArg, subGraphName),
            new Argument(SelectArg, select.ToString()));

    private DirectiveType RegisterResolverDirectiveType(
        string name,
        ScalarType typeName,
        ScalarType selectionSet)
    {
        var directiveType = new DirectiveType(name);
        directiveType.Arguments.Add(new InputField(SelectArg, new NonNullType(selectionSet)));
        directiveType.Arguments.Add(new InputField(SubGraphArg, new NonNullType(typeName)));
        directiveType.Locations |= DirectiveLocation.Object;
        _fusionGraph.DirectiveTypes.Add(directiveType);
        return directiveType;
    }

    public Directive CreateSourceDirective(string subGraphName, string? originalName = null)
        => originalName is null
            ? new Directive(
                Source,
                new Argument(SubGraphArg, subGraphName))
            : new Directive(
                Source,
                new Argument(SubGraphArg, subGraphName),
                new Argument(NameArg, originalName));

    private DirectiveType RegisterSourceDirectiveType(string name, ScalarType typeName)
    {
        var directiveType = new DirectiveType(name);
        directiveType.Locations = DirectiveLocation.FieldDefinition;
        directiveType.Arguments.Add(new InputField(SubGraphArg, new NonNullType(typeName)));
        directiveType.Arguments.Add(new InputField(NameArg, typeName));
        _fusionGraph.DirectiveTypes.Add(directiveType);
        return directiveType;
    }
}