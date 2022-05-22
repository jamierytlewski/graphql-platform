using HotChocolate.Configuration;
using HotChocolate.Data.Filters;
using HotChocolate.Language;
using HotChocolate.Types;

namespace HotChocolate.Data.ElasticSearch.Filters;

/// <summary>
/// This filter operation handler maps a StartsWith operation field to a
/// <see cref="ISearchOperation"/>
/// </summary>
public class ElasticSearchStringStartsWithOperationHandler
    : ElasticSearchOperationHandlerBase
{
    /// <summary>
    /// Initializes a new instance of
    /// <see cref="ElasticSearchStringStartsWithOperationHandler"/>
    /// </summary>
    public ElasticSearchStringStartsWithOperationHandler(InputParser inputParser)
        : base(inputParser)
    {
    }

    /// <inheritdoc />
    public override bool CanHandle(
        ITypeCompletionContext context,
        IFilterInputTypeDefinition typeDefinition,
        IFilterFieldDefinition fieldDefinition)
        => context.Type is StringOperationFilterInputType &&
            fieldDefinition is FilterOperationFieldDefinition
            {
                Id: DefaultFilterOperations.StartsWith
            };

    /// <inheritdoc />
    public override ISearchOperation HandleOperation(
        ElasticSearchFilterVisitorContext context,
        IFilterOperationField field,
        IValueNode value,
        object? parsedValue)
    {
        if (parsedValue is not string val)
        {
            throw ThrowHelper.Filtering_WrongValueProvided(field);
        }

        IElasticFilterMetadata metadata = field.GetElasticMetadata();

        return new TermOperation(
            context.GetPath(),
            metadata.Kind,
            val);
    }
}