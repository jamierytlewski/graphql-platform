using CookieCrumble;
using HotChocolate.Skimmed.Serialization;
using static HotChocolate.Fusion.Composition.ComposerFactory;

namespace HotChocolate.Fusion.Composition;

public sealed class DemoIntegrationTests
{
    [Fact]
    public async Task Accounts_And_Reviews()
    {
        var composer = CreateComposer();

        var context = await composer.ComposeAsync(
            new SubGraphConfiguration("Accounts", AccountsSdl),
            new SubGraphConfiguration("Reviews", ReviewsSdl));

        SchemaFormatter
            .FormatAsString(context.FusionGraph)
            .MatchSnapshot();
    }

    private const string AccountsSdl =
        """
        schema {
          query: Query
        }

        type Query {
          users: [User!]!
          userById(id: Int!): User!
        }

        type User {
          id: Int!
          name: String!
          birthdate: DateTime!
          username: String!
        }

        scalar DateTime

        directive @ref(coordinate: String, field: String) on FIELD_DEFINITION

        extend type Query {
          userById(id: Int! @ref(field: "id")): User!
        }
        """;

    private const string ReviewsSdl =
        """
        schema
          @rename(coordinate: "Author", newName: "User")
          @rename(coordinate: "Query.authorById", newName: "userById") {
          query: Query
        }

        type Query {
          reviews: [Review!]!
          authorById(id: Int!): Author
          productById(upc: Int!): Product
        }

        type Review {
          id: Int!
          author: Author!
          upc: Product!
          body: String!
        }

        type Author {
            id: Int!
            reviews: [Review!]!
        }

        type Product {
            upc: Int!
            reviews: [Review!]!
        }

        directive @ref(coordinate: String, field: String) on FIELD_DEFINITION
        directive @rename(coordinate: String! to: String!) on SCHEMA
        directive @remove(coordinate: String!) on SCHEMA

        extend type Query {
          authorById(id: Int! @ref(field: "id")): Author
          productById(upc: Int! @ref(field: "upc")): Product
        }
        """;
}