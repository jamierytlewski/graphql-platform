using HotChocolate.Language;
using HotChocolate.Skimmed;
using HotChocolate.Utilities;
using DirectiveLocation = HotChocolate.Skimmed.DirectiveLocation;
using IHasDirectives = HotChocolate.Skimmed.IHasDirectives;
using IHasName = HotChocolate.Skimmed.IHasName;

namespace HotChocolate.Fusion.Composition;

internal static class DirectivesHelper
{
    public const string RefDirectiveName = "ref";
    public const string RemoveDirectiveName = "remove";
    public const string RenameDirectiveName = "rename";
    public const string CoordinateArg = "coordinate";
    public const string NewNameArg = "newName";
    public const string FieldArg = "field";

    public static bool ContainsRefDirective(this IHasDirectives member)
        => member.Directives.ContainsName(RefDirectiveName);

    public static RefDirective GetRefDirective(this IHasDirectives member)
    {
        var directive = member.Directives[RefDirectiveName].First();

        var arg = directive.Arguments.FirstOrDefault(t => t.Name.EqualsOrdinal(CoordinateArg));

        if (arg is { Value: StringValueNode coordinate })
        {
            return new RefDirective(SchemaCoordinate.Parse(coordinate.Value));
        }

        arg = directive.Arguments.FirstOrDefault(t => t.Name.EqualsOrdinal(FieldArg));

        if (arg is { Value: StringValueNode field })
        {
            return new RefDirective(Utf8GraphQLParser.Syntax.ParseField(field.Value));
        }

        throw new InvalidOperationException(
            "The ref argument must have a value for coordinate or field.");
    }
}