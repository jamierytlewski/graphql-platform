using System.Text;
using System.IO;
using System.Linq;
using HotChocolate;
using HotChocolate.Execution.Configuration;
using HotChocolate.Types;
using Microsoft.Extensions.DependencyInjection;

namespace StrawberryShake.CodeGeneration.CSharp.Integration.UploadScalar;

public static class UploadSchemaHelpers
{

    public static MemoryStream CreateStream(string str)
    {
        var data = new MemoryStream();
        data.Write(Encoding.UTF8.GetBytes(str));
        data.Position = 0;
        return data;
    }

    public static void Configure(IRequestExecutorBuilder builder)
    {
        builder.AddTypeExtension<UploadQueries>().AddType<UploadType>();
    }

    public static string ReadContents(this IFile file)
    {
        using var stream = file.OpenReadStream();
        using var read = new StreamReader(stream);
        return file.Name + ":" + read.ReadToEnd();
    }

    [ExtendObjectType("Query")]
    public class UploadQueries
    {
        public string Upload(
            string? nonUpload,
            IFile? single,
            IFile?[]? list,
            IFile?[]?[]? nested,
            [GraphQLName("object")] Test? objectSingle,
            Test?[]? objectList,
            Test?[]?[]? objectNested)
        {
            if (single is not null)
            {
                return single?.ReadContents() ?? "null";
            }

            if (list is not null)
            {
                return string.Join(",", list.Select(x => x?.ReadContents() ?? "null"));
            }

            if (nested is not null)
            {
                return string.Join(
                    ",",
                    nested.SelectMany(y => y.Select(x => x?.ReadContents() ?? "null")));
            }

            if (objectSingle is not null)
            {
                return objectSingle.Bar!.Baz!.File!.ReadContents() ?? "null";
            }

            if (objectList is not null)
            {
                return string.Join(
                    ",",
                    objectList.Select(x => x?.Bar!.Baz!.File!.ReadContents() ?? "null"));
            }

            if (objectNested is not null)
            {
                return string.Join(
                    ",",
                    objectNested.SelectMany(
                        y
                            => y.Select(x => x?.Bar!.Baz!.File!.ReadContents() ?? "null")));
            }

            return "error";
        }
    }

    public class Test
    {
        public Test(Bar? bar)
        {
            Bar = bar;
        }

        public Bar? Bar { get; }

        public void Deconstruct(out Bar? bar)
        {
            bar = Bar;
        }
    }

    public class Bar
    {
        public Bar(Baz? baz)
        {
            Baz = baz;
        }

        public Baz? Baz { get; }

        public void Deconstruct(out Baz? baz)
        {
            baz = Baz;
        }
    }

    public class Baz
    {
        public Baz(IFile? file)
        {
            File = file;
        }

        public IFile? File { get; }

        public void Deconstruct(out IFile? file)
        {
            file = File;
        }
    }
}
