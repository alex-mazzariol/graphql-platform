using System.Collections.Generic;
using System.Linq;
using ChilliCream.Testing;
using HotChocolate.Language;
using HotChocolate.Language.Utilities;
using HotChocolate.Resolvers;
using Snapshooter;
using Snapshooter.Xunit;
using Xunit;

namespace HotChocolate.Stitching.Merge;

public class SchemaMergerTests
{
    [Fact]
    public void MergeSimpleSchemaWithDefaultHandler()
    {
        // arrange
        var schema_a =
            Utf8GraphQLParser.Parse("union Foo = Bar | Baz union A = B | C");
        var schema_b =
            Utf8GraphQLParser.Parse("union Foo = Bar | Baz");

        // act
        var schema = SchemaMerger.New()
            .AddSchema("A", schema_a)
            .AddSchema("B", schema_b)
            .Merge();

        // assert
        schema.Print().MatchSnapshot();
    }

    [Fact]
    public void MergeDemoSchemaWithDefaultHandler()
    {
        // arrange
        var schema_a =
            Utf8GraphQLParser.Parse(
                FileResource.Open("Contract.graphql"));
        var schema_b =
            Utf8GraphQLParser.Parse(
                FileResource.Open("Customer.graphql"));

        // act
        var schema = SchemaMerger.New()
            .AddSchema("A", schema_a)
            .AddSchema("B", schema_b)
            .Merge();

        // assert
        schema.Print().MatchSnapshot();
    }

    [Fact]
    public void MergeDemoSchemaAndRemoveRootTypes()
    {
        // arrange
        var schema_a =
            Utf8GraphQLParser.Parse(
                FileResource.Open("Contract.graphql"));
        var schema_b =
            Utf8GraphQLParser.Parse(
                FileResource.Open("Customer.graphql"));

        // act
        var schema = SchemaMerger.New()
            .AddSchema("A", schema_a)
            .AddSchema("B", schema_b)
            .IgnoreRootTypes()
            .Merge();

        // assert
        schema.Print().MatchSnapshot();
    }

    [Fact]
    public void MergeDemoSchemaAndRemoveRootTypesOnSchemaA()
    {
        // arrange
        var schema_a =
            Utf8GraphQLParser.Parse(
                FileResource.Open("Contract.graphql"));
        var schema_b =
            Utf8GraphQLParser.Parse(
                FileResource.Open("Customer.graphql"));

        // act
        var schema = SchemaMerger.New()
            .AddSchema("A", schema_a)
            .AddSchema("B", schema_b)
            .IgnoreRootTypes("A")
            .Merge();

        // assert
        schema.Print().MatchSnapshot();
    }

    [Fact]
    public void MergeSchemaAndRenameTypeAtoXyzOnAllSchemas()
    {
        // arrange
        var schema_a =
            Utf8GraphQLParser.Parse(
                "type A { b1: String b2: String } type B { c: String }");
        var schema_b =
            Utf8GraphQLParser.Parse(
                "type A { b1: String b3: String } type B { c: String }");

        // act
        var schema = SchemaMerger.New()
            .AddSchema("A", schema_a)
            .AddSchema("B", schema_b)
            .RenameType("A", "Xyz")
            .Merge();

        // assert
        schema.Print().MatchSnapshot();
    }


    [Fact]
    public void MergeSchemaAndRenameTypeAtoXyzOnSchemaA()
    {
        // arrange
        var schema_a =
            Utf8GraphQLParser.Parse(
                "type A { b1: String b2: String } type B { c: String }");
        var schema_b =
            Utf8GraphQLParser.Parse(
                "type A { b1: String b3: String } type B { c: String }");

        // act
        var schema = SchemaMerger.New()
            .AddSchema("A", schema_a)
            .AddSchema("B", schema_b)
            .RenameType("A", "Xyz", "A")
            .Merge();

        // assert
        schema.Print().MatchSnapshot();
    }

    [Fact]
    public void MergeSchemaAndRemoveTypeAOnAllSchemas()
    {
        // arrange
        var schema_a =
            Utf8GraphQLParser.Parse(
                "type A { b1: String } type B { c: String }");
        var schema_b =
            Utf8GraphQLParser.Parse(
                "type A { b2: String } type B { c: String }");

        // act
        var schema = SchemaMerger.New()
            .AddSchema("A", schema_a)
            .AddSchema("B", schema_b)
            .IgnoreType("A")
            .Merge();

        // assert
        schema.Print().MatchSnapshot();
    }


    [Fact]
    public void MergeSchemaAndRemoveTypeAOnSchemaA()
    {
        // arrange
        var schema_a =
            Utf8GraphQLParser.Parse(
                "type A { b1: String } type B { c: String }");
        var schema_b =
            Utf8GraphQLParser.Parse(
                "type A { b2: String } type B { c: String }");

        // act
        var schema = SchemaMerger.New()
            .AddSchema("A", schema_a)
            .AddSchema("B", schema_b)
            .IgnoreType("A", "A")
            .Merge();

        // assert
        schema.Print().MatchSnapshot();
    }

    [Fact]
    public void MergeSchemaAndRemoveFieldB1OnAllSchemas()
    {
        // arrange
        var schema_a =
            Utf8GraphQLParser.Parse(
                "type A { b1: String b2: String } type B { c: String }");
        var schema_b =
            Utf8GraphQLParser.Parse(
                "type A { b1: String b3: String } type B { c: String }");

        // act
        var schema = SchemaMerger.New()
            .AddSchema("A", schema_a)
            .AddSchema("B", schema_b)
            .IgnoreField(new FieldReference("A", "b1"))
            .Merge();

        // assert
        schema.Print().MatchSnapshot();
    }


    [Fact]
    public void MergeSchemaAndRemoveFieldB1OnSchemaA()
    {
        // arrange
        var schema_a =
            Utf8GraphQLParser.Parse(
                "type A { b1: String b2: String } type B { c: String }");
        var schema_b =
            Utf8GraphQLParser.Parse(
                "type A { b1: String b3: String } type B { c: String }");

        // act
        var schema = SchemaMerger.New()
            .AddSchema("A", schema_a)
            .AddSchema("B", schema_b)
            .IgnoreField(new FieldReference("A", "b1"), "A")
            .Merge();

        // assert
        schema.Print().MatchSnapshot();
    }

    [Fact]
    public void MergeSchemaAndRenameFieldB1toB11OnAllSchemas()
    {
        // arrange
        var schema_a =
            Utf8GraphQLParser.Parse(
                "type A { b1: String b2: String } type B { c: String }");
        var schema_b =
            Utf8GraphQLParser.Parse(
                "type A { b1: String b3: String } type B { c: String }");

        // act
        var schema = SchemaMerger.New()
            .AddSchema("A", schema_a)
            .AddSchema("B", schema_b)
            .RenameField(new FieldReference("A", "b1"), "b11")
            .Merge();

        // assert
        schema.Print().MatchSnapshot();
    }


    [Fact]
    public void MergeSchemaAndRenameFieldB1toB11OnSchemaA()
    {
        // arrange
        var schema_a =
            Utf8GraphQLParser.Parse(
                "type A { b1: String b2: String } type B { c: String }");
        var schema_b =
            Utf8GraphQLParser.Parse(
                "type A { b1: String b3: String } type B { c: String }");

        // act
        var schema = SchemaMerger.New()
            .AddSchema("A", schema_a)
            .AddSchema("B", schema_b)
            .RenameField(new FieldReference("A", "b1"), "b11", "A")
            .Merge();

        // assert
        schema.Print().MatchSnapshot();
    }

    [Fact(Skip =  "Fix It")]
    public void RenameReferencingType()
    {
        // arrange
        var schema_a =
            Utf8GraphQLParser.Parse(
                "type A { b1: B } " +
                "type B implements C { c: String } " +
                "interface C { c: String }");

        var schema_b =
            Utf8GraphQLParser.Parse(
                "type B { b1: String b3: String } type C { c: String }");

        // act
        var a = SchemaMerger.New()
            .AddSchema("A", schema_a)
            .AddSchema("B", schema_b)
            .RenameType("A", "B", "Foo")
            .Merge();

        var b = SchemaMerger.New()
            .AddSchema("B", schema_b)
            .AddSchema("A", schema_a)
            .RenameType("A", "B", "Foo")
            .Merge();

        // assert
        a.Print().MatchSnapshot(SnapshotNameExtension.Create("A"));
        b.Print().MatchSnapshot(SnapshotNameExtension.Create("B"));
    }

    [Fact]
    public void Rename_Type_With_Various_Variants()
    {
        // arrange
        var initial =
            Utf8GraphQLParser.Parse(
                "type A { b1: B! b2: [B!] b3: [B] b4: [B!]! } " +
                "type B implements C { c: String } " +
                "interface C { c: String }");

        // act
        var merged = SchemaMerger.New()
            .AddSchema("A", initial)
            .RenameType("B", "Foo", "A")
            .Merge();

        // assert
        merged.Print().MatchSnapshot();
    }

    [Fact]
    public void FieldDefinitionDoesNotHaveSameTypeShape()
    {
        // arrange
        var schema_a =
            Utf8GraphQLParser.Parse(
                "type A { b1: String b2: String } type B { c: String! }");
        var schema_b =
            Utf8GraphQLParser.Parse(
                "type A { b1: String b3: String } type B { c: String }");

        // act
        var merged = SchemaMerger.New()
            .AddSchema("A", schema_a)
            .AddSchema("B", schema_b)
            .Merge();

        // assert
        merged.Print().MatchSnapshot();
    }

    [Fact]
    public void RenameObjectFieldThatImplementsInterface()
    {
        // arrange
        var schema_a =
            Utf8GraphQLParser.Parse(
                "type A { b1: B } " +
                "type B implements D { c: String } " +
                "type C implements D { c: String } " +
                "interface D { c: String }");

        var schema_b =
            Utf8GraphQLParser.Parse(
                "type B { b1: String b3: String } type C { c: String }");

        // act
        var a = SchemaMerger.New()
            .AddSchema("A", schema_a)
            .AddSchema("B", schema_b)
            .RenameField(new FieldReference("B", "c"), "c123", "A")
            .Merge();

        // assert
        a.Print().MatchSnapshot();
    }

    [Fact]
    public void RenameObjectField()
    {
        // arrange
        var schema_a =
            Utf8GraphQLParser.Parse(
                "type A { b1: B } " +
                "type B { c: String } " +
                "type C implements D { c: String } " +
                "interface D { c: String }");

        var schema_b =
            Utf8GraphQLParser.Parse(
                "type B { b1: String b3: String } type C { c: String }");

        // act
        var a = SchemaMerger.New()
            .AddSchema("A", schema_a)
            .AddSchema("B", schema_b)
            .RenameField(new FieldReference("B", "c"), "c123", "A")
            .Merge();

        // assert
        a.Print().MatchSnapshot();
    }

    [Fact]
    public void RenameInterfaceField()
    {
        // arrange
        var schema_a =
            Utf8GraphQLParser.Parse(
                "type A { b1: B } " +
                "type B implements D { c: String } " +
                "type C implements D { c: String } " +
                "interface D { c: String }");

        var schema_b =
            Utf8GraphQLParser.Parse(
                "type B { b1: String b3: String } type C { c: String }");

        // act
        var a = SchemaMerger.New()
            .AddSchema("A", schema_a)
            .AddSchema("B", schema_b)
            .RenameField(new FieldReference("D", "c"), "c123", "A")
            .Merge();

        // assert
        a.Print().MatchSnapshot();
    }

    [Fact]
    public void LastFieldRenameWins()
    {
        // arrange
        var schema_a =
            Utf8GraphQLParser.Parse(
                "type A { b1: B } " +
                "type B implements D { c: String } " +
                "type C implements D { c: String } " +
                "interface D { c: String }");

        var schema_b =
            Utf8GraphQLParser.Parse(
                "type B { b1: String b3: String } type C { c: String }");

        // act
        var a = SchemaMerger.New()
            .AddSchema("A", schema_a)
            .AddSchema("B", schema_b)
            .RenameField(new FieldReference("B", "c"), "c123", "A")
            .RenameField(new FieldReference("C", "c"), "c456", "A")
            .RenameField(new FieldReference("D", "c"), "c789", "A")
            .Merge();

        var b = SchemaMerger.New()
            .AddSchema("A", schema_a)
            .AddSchema("B", schema_b)
            .RenameField(new FieldReference("B", "c"), "c123", "A")
            .RenameField(new FieldReference("D", "c"), "c789", "A")
            .RenameField(new FieldReference("C", "c"), "c456", "A")
            .Merge();

        // assert
         a.Print().MatchSnapshot(SnapshotNameExtension.Create("A"));
        b.Print().MatchSnapshot(SnapshotNameExtension.Create("B"));
    }

    [Fact]
    public void MergeDirectivesWithCustomRule()
    {
        // arrange
        var schema_a =
            Utf8GraphQLParser.Parse("directive @foo on FIELD");
        var schema_b =
            Utf8GraphQLParser.Parse("directive @foo(a: String) on FIELD");

        // act
        var a = SchemaMerger.New()
            .AddSchema("A", schema_a)
            .AddSchema("B", schema_b)
            .AddDirectiveMergeRule(next => (context, directives) =>
            {
                context.AddDirective(
                    directives.First(t =>
                        t.Definition.Arguments.Any()).Definition);
            })
            .Merge();

        // assert
        a.Print().MatchSnapshot();
    }

    [Fact]
    public void MergeDirectivesWithCustomHandler()
    {
        // arrange
        var schema_a =
            Utf8GraphQLParser.Parse("directive @foo on FIELD");
        var schema_b =
            Utf8GraphQLParser.Parse("directive @foo(a: String) on FIELD");

        // act
        var a = SchemaMerger.New()
            .AddSchema("A", schema_a)
            .AddSchema("B", schema_b)
            .AddDirectiveMergeHandler<CustomDirectiveMergeHandler>()
            .Merge();

        // assert
        a.Print().MatchSnapshot();
    }

    [Fact]
    public void MergeTypeWithCustomRule()
    {
        // arrange
        var schema_a =
            Utf8GraphQLParser.Parse("type Foo { a: String }");
        var schema_b =
            Utf8GraphQLParser.Parse("type Foo { b: String }");

        // act
        var a = SchemaMerger.New()
            .AddSchema("A", schema_a)
            .AddSchema("B", schema_b)
            .AddTypeMergeRule(next => (context, types) =>
            {
                var typeInfos = types.OfType<ObjectTypeInfo>().ToArray();
                var fields = typeInfos[0].Definition.Fields.ToList();
                fields.AddRange(typeInfos[1].Definition.Fields);
                context.AddType(
                    typeInfos[0].Definition.WithFields(fields));
            })
            .Merge();

        // assert
        a.Print().MatchSnapshot();
    }

    [Fact]
    public void MergeTypeWithCustomHandler()
    {
        // arrange
        var schema_a =
            Utf8GraphQLParser.Parse("type Foo { a: String }");
        var schema_b =
            Utf8GraphQLParser.Parse("type Foo { b: String }");

        // act
        var a = SchemaMerger.New()
            .AddSchema("A", schema_a)
            .AddSchema("B", schema_b)
            .AddTypeMergeHandler<CustomTypeMergeHandler>()
            .Merge();

        // assert
        a.Print().MatchSnapshot();
    }

    public class CustomDirectiveMergeHandler
        : IDirectiveMergeHandler
    {
        public CustomDirectiveMergeHandler(MergeDirectiveRuleDelegate next)
        {
        }

        public void Merge(
            ISchemaMergeContext context,
            IReadOnlyList<IDirectiveTypeInfo> directives)
        {
            context.AddDirective(
                directives.First(t =>
                    t.Definition.Arguments.Any()).Definition);
        }
    }

    public class CustomTypeMergeHandler
        : ITypeMergeHandler
    {
        public CustomTypeMergeHandler(MergeTypeRuleDelegate next)
        {
        }

        public void Merge(
            ISchemaMergeContext context,
            IReadOnlyList<ITypeInfo> types)
        {
            var typeInfos = types.OfType<ObjectTypeInfo>().ToArray();
            var fields = typeInfos[0].Definition.Fields.ToList();
            fields.AddRange(typeInfos[1].Definition.Fields);
            context.AddType(
                typeInfos[0].Definition.WithFields(fields));
        }
    }
}
