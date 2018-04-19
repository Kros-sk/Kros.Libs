# Kros.KORM

Kros.KORM is simple, fast and easy to use micro-ORM framework for .NETStandard created by Kros a.s. from Slovakia.

## Why to use Kros.KORM

* You can easily create query builder for creating queries returning IEnumerable of your POCO objects
* Linq
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

Kros.KORM allow you to use Linq for creating queries. Basic queries are translated to SQL language.

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
Where, FirstOrDefault, Take, Sum, Max, Min, OrderBy, OrderByDescending, ThenBy, ThenByDescending, Count, Any.

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

### Convention model mapper

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
