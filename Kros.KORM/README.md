# Kros.KORM

Kros.KORM is simple, fast and easy to use micro-ORM framework for .NETStandard created by Kros a.s. from Slovakia.

## Why to use Kros.KORM

* You can easily create query builder for creating queries returning IEnumerable of your POCO objects
* Linq support
* Saving changes to your data (Insert / Update / Delete)
* Korm supports bulk operations for fast inserting and updating large amounts of data (BulkInsert, BulkDelete)

## This topic contains following sections:

* [Kros.KORM.dll](#kroskormdll)
* [Query](#query)
* [Linq to KORM](#linq-to-korm)
* [DataAnnotation attributes](#dataannotation-attributes)
* [Convention model mapper](#convention-model-mapper)
* [Converters](#converters)
* [OnAfterMaterialize](#onaftermaterialize)
* [Property injection](#property-injection)
* [Model builder](#model-builder)
* [Changes committing](#changes-committing)
* [Bulk operations](#bulk-operations)
* [SQL commands executing](#sql-commands-executing)
* [Logging](#logging)
* [Supported database types](#supported-database-types)
* [Unit and performance tests](#unit-and-performance-tests)

### Kros.KORM.dll

What should you do when you want to append Kros.KORM to your project?

##### Package manager

```
Install-Package Kros.KORM
Install-Package Kros.KORM.MsAccess
```

##### .NET CLI

```
dotnet add package Kros.KORM
```

After that you are ready to use benefits of Kros.KORM.

### Query

You can use Kros.KORM for creating queries and their materialization. Kros.KORM helps you put together desired query, that can return instances of objects populated from database by using foreach or linq.
 
##### Query for obtaining data

```c#
var people = database.Query<Person>()
    .Select("p.Id", "FirstName", "LastName", "PostCode")
    .From("Person JOIN Address ON (Person.AddressId = Address.Id)")
    .Where("Age > @1", 18);

foreach (var person in people)
{
    Console.WriteLine(person.FirstName);
}
```

For more information take a look at definition of [IQuery](https://kros-sk.github.io/Kros.Libs/api/Kros.KORM.Query.IQuery-1.html).

### Linq to KORM

Kros.KORM allows you to use Linq for creating queries. Basic queries are translated to SQL language.

##### Example

```c#
var people = database.Query<Person>()
    .From("Person JOIN Address ON (Person.AddressId = Address.Id)")
    .Where(p => p.LastName.EndsWith("ová"))
    .OrderByDescending(p => p.Id)
    .Take(5);

foreach (var person in people)
{
    Console.WriteLine(person.FirstName);
}
```

##### Supported Linq methods
```c#
Where, FirstOrDefault, Take, Sum, Max, Min, OrderBy, OrderByDescending, ThenBy, ThenByDescending, Count, Any.
```

Other methods, such as Select, GroupBy, Join are not supported at this moment because of their complexity.

You can use also some string functions in Linq queries:

| String function | Example                                               | Translation to T-SQL                          |
|-----------------|-------------------------------------------------------|-----------------------------------------------|
| StartWith       | Where(p => p.FirstName.StartWith("Mi"))               | WHERE (FirstName LIKE @1 + '%')               |
| EndWith         | Where(p => p.LastName.EndWith("ová"))                 | WHERE (LastName LIKE '%' + @1)                |
| Contains        | Where(p => p.LastName.Contains("ia"))                 | WHERE (LastName LIKE '%' + @1 + '%')          |
| IsNullOrEmpty   | Where(p => String.IsNullOrEmpty(p.LastName))          | WHERE (LastName IS NULL OR LastName = '')     |
| ToUpper         | Where(p => p.LastName.ToUpper() == "Smith")           | WHERE (UPPER(LastName) = @1)                  |
| ToLower         | Where(p => p.LastName.ToLower() == "Smith")           | WHERE (LOWER(LastName) = @1)                  |
| Replace         | Where(p => p.FirstName.Replace("hn", "zo") == "Jozo") | WHERE (REPLACE(FirstName, @1, @2) = @3)       |
| Substring       | Where(p => p.FirstName.Substring(1, 2) == "oh")       | WHERE (SUBSTRING(FirstName, @1 + 1, @2) = @3) |
| Trim            | Where(p => p.FirstName.Trim() == "John")              | WHERE (RTRIM(LTRIM(FirstName)) = @1)          |

Translation is provided by implementation of [ISqlExpressionVisitor](https://kros-sk.github.io/Kros.Libs/api/Kros.KORM.Query.Sql.ISqlExpressionVisitor.html).

### DataAnnotation attributes

Properties (not readonly or writeonly properties) are implicitly mapped to database fields with same name. When you want to map property to database field with different name use AliasAttribute. The same works for mapping POCO classes with database tables.

```c#
Copy
[Alias("Workers")]
private class Staff
{
    [Alias("PK")]
    public int Id { get; set; }

    [Alias("Name")]
    public string FirstName { get; set; }

    [Alias("SecondName")]
    public string LastName { get; set; }
}

private void StaffExample()
{
    using (var database = new Database(_connection))
    {
        _command.CommandText = "SELECT PK, Name, SecondName from Workers";

        using (var reader = _command.ExecuteReader())
        {
            var staff = database.ModelBuilder.Materialize<Staff>(reader);
        }
    }
}
```

When you need to have read-write properties independent of the database use NoMapAttribute.

```c#
[NoMap]
public int Computed { get; set; }
```

### Convention model mapper

If you have different conventions for naming properties in POCO classes and fields in database, you can redefine behaviour of ModelMapper, which serves mapping POCO classes to database tables and vice versa.

Redefine convention for mapping example
```c#
Database.DefaultModelMapper.MapColumnName = (colInfo, modelType) =>
{
    return string.Format("COL_{0}", colInfo.PropertyInfo.Name.ToUpper());
};

Database.DefaultModelMapper.MapTableName = (tInfo, type) =>
{
    return string.Format("TABLE_{0}", type.Name.ToUpper());
};

using (var database = new Database(_connection))
{

    _command.CommandText = "SELECT COL_ID, COL_FIRSTNAME from TABLE_WORKERS";

    using (var reader = _command.ExecuteReader())
    {
        var people = database.ModelBuilder.Materialize<Person>(reader);

        foreach (var person in people)
        {
            Console.WriteLine(person.FirstName);
        }
    }
}
```

Alternatively you can write your own implementation of [IModelMapper](https://kros-sk.github.io/Kros.Libs/api/Kros.KORM.Metadata.IModelMapper.html).

### Converters

### OnAfterMaterialize

### Property injection

### Model builder

### Changes committing

### Bulk operations

### SQL commands executing

### Logging

### Supported database types

### Unit and performance tests
