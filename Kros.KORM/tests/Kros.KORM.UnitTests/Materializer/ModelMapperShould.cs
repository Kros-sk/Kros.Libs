using FluentAssertions;
using Kros.KORM.Converter;
using Kros.KORM.Materializer;
using Kros.KORM.Metadata;
using Kros.KORM.Metadata.Attribute;
using System;
using System.Data;
using System.Linq;
using Xunit;

namespace Kros.KORM.UnitTests.Metadata
{
    public class ModelMapperShould : IDisposable
    {
        [Fact]
        public void UseAliasAttributeForGettingNames()
        {
            var modelMapper = new ConventionModelMapper();

            var tableInfo = modelMapper.GetTableInfo<Foo>();

            var columns = tableInfo.Columns.ToList();

            columns[1].Name.Should().Be("PostCode", "Becouse have alias.");
            columns[2].Name.Should().Be("FirstName", "Becouse have alias.");
        }

        [Fact]
        public void UseNamesFromConfigurationMap()
        {
            var modelMapper = new ConventionModelMapper();
            modelMapper.SetColumnName<Foo, string>(p => p.PropertyString, "Address");
            modelMapper.SetColumnName<Foo, double>(p => p.PropertyDouble, "Salary");

            var tableInfo = modelMapper.GetTableInfo<Foo>();

            var address = tableInfo.GetColumnInfoByPropertyName(nameof(Foo.PropertyString));
            var salary = tableInfo.GetColumnInfoByPropertyName(nameof(Foo.PropertyDouble));

            address.Name.Should().Be("Address");
            salary.Name.Should().Be("Salary");
        }

        [Fact]
        public void UseConventionForGettingNamesWhenAliasDoesNotExist()
        {
            var modelMapper = new ConventionModelMapper();

            var tableInfo = modelMapper.GetTableInfo<Foo>();

            var columns = tableInfo.Columns.ToList();

            columns[0].Name.Should().Be("Id");
            columns[3].Name.Should().Be("LastName");
            columns[4].Name.Should().Be("PropertyDouble");
        }

        [Fact]
        public void UseConventionForGettingTableNameWhenAliasDoesNotExist()
        {
            var modelMapper = new ConventionModelMapper();

            var tableInfo = modelMapper.GetTableInfo<Foo>();

            tableInfo.Name.Should().Be("Foo");
        }

        [Fact]
        public void UseAliasForGettingTableName()
        {
            var modelMapper = new ConventionModelMapper();

            var tableInfo = modelMapper.GetTableInfo<Foo1>();

            tableInfo.Name.Should().Be("Person", "Becouse have alias.");
        }

        [Fact]
        public void GetTableInfoWithPrimaryKey()
        {
            var modelMapper = new ConventionModelMapper();

            var tableInfo = modelMapper.GetTableInfo<Foo>();

            var key = tableInfo.PrimaryKey.ToList();

            key.Should().HaveCount(2, "Becouse class Foo have to property with Key attribute");
            key[0].Name.Should().Be("Id");
            key[0].IsPrimaryKey.Should().BeTrue();
            key[1].Name.Should().Be("PostCode");
            key[1].IsPrimaryKey.Should().BeTrue();
        }

        [Fact]
        public void GetTableInfoWithPrimaryKeyByConvention()
        {
            var modelMapper = new ConventionModelMapper();

            var tableInfo = modelMapper.GetTableInfo<Foo1>();

            var key = tableInfo.PrimaryKey.ToList();

            key.Should().HaveCount(1, "Becouse Foo1 dont have key attribute, but have one property which match Id convention");
            key[0].Name.Should().Be("Id");
            key[0].IsPrimaryKey.Should().BeTrue();
        }

        [Fact]
        public void GetTableInfoWithoutPrimarKeyWhenDontHaveKeyAttributeAndNoConventionMatch()
        {
            var modelMapper = new ConventionModelMapper();

            var tableInfo = modelMapper.GetTableInfo<Foo2>();

            var key = tableInfo.PrimaryKey.ToList();
            bool anyKey = key.Any(p => p.IsPrimaryKey);

            key.Should().HaveCount(0);
            anyKey.Should().BeFalse();
        }

        [Fact]
        public void GetTableInfoWithColumnConverter()
        {
            var modelMapper = new ConventionModelMapper();

            var tableInfo = modelMapper.GetTableInfo<Foo>();

            var columns = tableInfo.Columns.ToList();

            columns[5].Converter.Should().BeOfType<TestConverter>();
        }

        [Fact]
        public void GetTableInfoWithoutReadOnlyProperty()
        {
            var modelMapper = new ConventionModelMapper();

            var tableInfo = modelMapper.GetTableInfo<Foo>();

            var columns = tableInfo.Columns.ToList();

            tableInfo.GetColumnInfo("ReadOnlyProperty").Should().BeNull();
        }

        [Fact]
        public void GetTableInfoWithoutNoMappAttribute()
        {
            var modelMapper = new ConventionModelMapper();

            var tableInfo = modelMapper.GetTableInfo<Foo>();

            tableInfo.GetColumnInfo("Ignore").Should().BeNull();
        }

        [Fact]
        public void GetTableInfoWithAutoIncrementKey()
        {
            var modelMapper = new ConventionModelMapper();

            var tableInfo = modelMapper.GetTableInfo<FooWithAutoIncrement>();

            tableInfo.PrimaryKey.Single().AutoIncrementMethodType.Should().Be(AutoIncrementMethodType.Custom);
        }

        [Fact]
        public void GetTableInfoWithoutAutoIncrementKey()
        {
            var modelMapper = new ConventionModelMapper();

            var tableInfo = modelMapper.GetTableInfo<Foo>();

            tableInfo.PrimaryKey
                .Any(p => p.AutoIncrementMethodType != AutoIncrementMethodType.None)
                .Should().BeFalse();
        }

        [Fact]
        public void UseNewConventionForGettingTableInfo()
        {
            var modelMapper = new ConventionModelMapper
            {
                MapColumnName = (colInfo, modelType) =>
                {
                    return colInfo.PropertyInfo.Name.ToUpper();
                },

                MapTableName = (tInfo, type) =>
                {
                    return type.Name.ToLower();
                },

                MapPrimaryKey = (tInfo) =>
                {
                    var primaryKey = tInfo.Columns.Where(p => p.Name == "OID");

                    foreach (var key in primaryKey)
                    {
                        key.IsPrimaryKey = true;
                    }

                    return primaryKey;
                }
            };

            var tableInfo = modelMapper.GetTableInfo<Foo2>();

            var columns = tableInfo.Columns.ToList();

            tableInfo.Name.Should().Be("foo2");

            columns[1].Name.Should().Be("PROPERTYDOUBLE");

            tableInfo.PrimaryKey.Should().HaveCount(1);
            tableInfo.PrimaryKey.FirstOrDefault().Name.Should().Be("OID");
        }

        [Fact]
        public void HaveOnAfterMaterializeMethodInfo()
        {
            var modelMapper = new ConventionModelMapper();

            var tableInfo = modelMapper.GetTableInfo<Foo1>();
            tableInfo.OnAfterMaterialize.Name.Should().Be("OnAfterMaterialize");
        }

        [Fact]
        public void KnowConfigureInjection()
        {
            var modelMapper = new ConventionModelMapper();

            var configurator = modelMapper.InjectionConfigurator<Foo>()
                .FillProperty(p => p.PropertyString, () => "lorem")
                .FillProperty(p => p.PropertyDouble, () => 1);

            modelMapper.GetInjector<Foo>().Should().Be(configurator);
        }

        [Fact]
        public void DontThrowExceptionIfInjectionIsNotConfigured()
        {
            var modelMapper = new ConventionModelMapper();

            modelMapper.GetInjector<Foo>().Should().NotBeNull();
        }

        public void Dispose()
        {
            ConverterAttribute.ClearCache();
        }

        private class Foo
        {
            [Key()]
            public int Id { get; set; }

            [Key()]
            [Alias("PostCode")]
            public string Code { get; set; }

            [Alias("FirstName")]
            public string PropertyString { get; set; }

            public string LastName { get; set; }

            public double PropertyDouble { get; set; }

            [Converter(typeof(TestConverter))]
            public TestEnum PropertyEnum { get; set; }

            public int ReadOnlyProperty { get { return 5; } }

            public string DataTypeProperty { get; set; }

            [NoMap()]
            public int Ignore { get; set; }
        }

        [Alias("Person")]
        private class Foo1 : IMaterialize
        {
            public int Id { get; set; }

            public void OnAfterMaterialize(IDataRecord source)
            {
                throw new NotImplementedException();
            }
        }

        private class Foo2
        {
            public int OId { get; set; }

            public double PropertyDouble { get; set; }
        }

        private class FooWithAutoIncrement
        {
            [Key(AutoIncrementMethodType.Custom)]
            public int Id { get; set; }

            public double PropertyDouble { get; set; }
        }

        private enum TestEnum
        {
            Value1,
            Value2,
            Value3
        }

        private class TestConverter : IConverter
        {
            public object Convert(object value)
            {
                throw new NotImplementedException();
            }

            public object ConvertBack(object value)
            {
                throw new NotImplementedException();
            }
        }
    }
}
