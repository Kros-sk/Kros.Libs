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
