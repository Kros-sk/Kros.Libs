Kros.KORM

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

Supported Linq methods are ```Where, FirstOrDefault, Take, Sum, Max, Min, OrderBy, OrderByDescending, ThenBy, ThenByDescending, Count, Any.```

Other methods, such as ```Select, GroupBy, Join``` are not supported at this moment because of their complexity.

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

##### Redefining mapping conventions example
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

##### Custom model mapper
```c#
Database.DefaultModelMapper = new CustomModelMapper();
```
If your POCO class is defined in external library, you can redefine mapper, so it can map properties of the model to desired database names.

##### External class mapping example
```c#
var externalPersonMap = new Dictionary<string, string>() {
    { PropertyName<ExternalPerson>.GetPropertyName(p => p.oId), "Id" },
    { PropertyName<ExternalPerson>.GetPropertyName(p => p.Name), "FirstName" },
    { PropertyName<ExternalPerson>.GetPropertyName(p => p.SecondName), "LastName" }
};

Database.DefaultModelMapper.MapColumnName = (colInfo, modelType) =>
{
    if (modelType == typeof(ExternalPerson))
    {
        return externalPersonMap[colInfo.PropertyInfo.Name];
    }
    else
    {
        return colInfo.PropertyInfo.Name;
    }
};

using (var database = new Database(_connection))
{
    var people = database.Query<ExternalPerson>();

    foreach (var person in people)
    {
        Console.WriteLine($"{person.oId} : {person.Name}-{person.SecondName}");
    }
}
```

For dynamic mapping you can use method [SetColumnName<TModel, TValue>](https://kros-sk.github.io/Kros.Libs/api/Kros.KORM.Metadata.IModelMapper.html#Kros_KORM_Metadata_IModelMapper_SetColumnName__2_System_Linq_Expressions_Expression_System_Func___0___1___System_String_)

```c#
Database.DefaultModelMapper.SetColumnName<Person, string>(p => p.Name, "FirstName");
```

### Converters

Data type of column in database and data type of property in your POCO class may differ. Some of these differences are automatically solved by Kros.KORM, for example double in database is converted to int in your model, same as int in database to enum in model, etc.

For more complicated conversion Kros.KORM offers possibility similar to data binding in WPF, where IValueConverter is used.

Imagine you store a list of addresses separated by some special character (for example #) in one long text column, but the property in your POCO class is list of strings.

Let's define a converter that can convert string to list of strings.

```c#
public class AddressesConverter : IConverter
{
    public object Convert(object value)
    {
        var ret = new List<string>();
        if (value != null)
        {
            var address = value.ToString();
            var addresses = address.Split('#');

            ret.AddRange(addresses);
        }

        return ret;
    }

    public object ConvertBack(object value)
    {
        var addresses = string.Join("#", (value as List<string>));

        return addresses;
    }
}
```

And now you can set this converter for your property.

```c#
[Converter(typeof(AddressesConverter))]
public List<string> Addresses { get; set; }
```

### OnAfterMaterialize

If you want to do some special action right after materialisation is done (for example to do some calculations) or you want to get some other values from source reader, that can not by processed automatically, your class should implement interface [IMaterialize](https://kros-sk.github.io/Kros.Libs/api/Kros.KORM.Materializer.IMaterialize.html).

You can do whatever you need in method ```OnAfterMaterialize```.

For example, if you have three int columns for date in database (Year, Month and Day) but in your POCO class you have only one date property, you can solve it as follows:

```c#
[NoMap]
public DateTime Date { get; set; }

public void OnAfterMaterialize(IDataRecord source)
{
    var year = source.GetInt32(source.GetOrdinal("Year"));
    var month = source.GetInt32(source.GetOrdinal("Month"));
    var day = source.GetInt32(source.GetOrdinal("Day"));

    this.Date = new DateTime(year, month, day);
}
```

### Property injection

Sometimes you might need to inject some service to your model, for example calculator or logger. For these purposes KORM offers IInjectionConfigurator, that can help you with injection configuration.

Let's have properties in model

```c#
[NoMap]
public ICalculationService CalculationService { get; set; }

[NoMap]
public ILogger Logger { get; set; }
```

And that is how you can configure them.

```c#
Database.DefaultModelMapper
    .InjectionConfigurator<Person>()
        .FillProperty(p => p.CalculationService, () => new CalculationService())
        .FillProperty(p => p.Logger, () => ServiceContainer.Instance.Resolve<ILogger>());
```        

### Model builder

For materialisation KORM uses IModelFactory, that creates factory for creating and filling your POCO objects.

By default DynamicMethodModelFactory is implemented, which uses dynamic method for creating delegates.

If you want to try some other implementation (for example based on reflexion) you can redefine property Database.DefaultModelFactory.

```c#
Database.DefaultModelFactory = new ReflectionModelfactory();
```

### Changes committing

You can use KORM also for editing, adding or deleting records from database. [IdDbSet](https://kros-sk.github.io/Kros.Libs/api/Kros.KORM/Kros.KORM.Query.IDbSet-1.html) is designed for that.

Records to edit or delete are identified by the primary key. You can set primary key to your POCO class by using ```Key``` attribute.

```c#
[Key()]
public int Id { get; set; }

public string FirstName { get; set; }

public string LastName { get; set; }
```

##### Inserting records to database

```c#
public void Insert()
{
    using (var database = new Database(_connection))
    {
        var people = database.Query<Person>().AsDbSet();

        people.Add(new Person() { Id = 1, FirstName = "Jean Claude", LastName = "Van Damme" });
        people.Add(new Person() { Id = 2, FirstName = "Sylvester", LastName = "Stallone" });

        people.CommitChanges();
    }
}
```

KORM supports bulk inserting, which is one of its best features. You add records to DbSet standardly by method ```Add```, but for committing to database use method ```BulkInsert``` instead of ```CommitChanges```.

```c#
var people = database.Query<Person>().AsDbSet();

foreach (var person in dataForImport)
{
    people.Add(person);
}

people.BulkInsert();
```

KORM supports also bulk update of records, you can use ```BulkUpdate``` method.

```c#
var people = database.Query<Person>().AsDbSet();

foreach (var person in dataForUpdate)
{
    people.Edit(person);
}

people.BulkUpdate();
```

This bulk way of inserting or updating data is several times faster than standard inserts or updates.

For both of bulk operations you can provide data as an argument of method. The advantage is that if we have a specific enumerator, we do not need to spill data into memory.

##### Primary key generating.

KORM supports generating of primary keys for inserted records. Primary key must be simple Int32 column. Primary key property in POCO class must be decorated by ```Key``` attribute and its property ```AutoIncrementMethodType``` must be set to ```Custom```.

```c#
[Key(autoIncrementMethodType: AutoIncrementMethodType.Custom)]
public int Id { get; set; }
```

KORM generates primary key for every inserted record, that does not have value for primary key property. For generating primary keys implementations of [IIdGenerator](https://kros-sk.github.io/Kros.Libs/api/Kros.Utils/Kros.Data.IIdGenerator.html) are used.

##### Editing records in database

```c#
public void Edit()
{
    using (var database = new Database(_connection))
    {
        var people = database.Query<Person>().AsDbSet();

        foreach (var person in people)
        {
            person.LastName += "ová";
            people.Edit(person);
        }

        people.CommitChanges();
    }
}
```

### Deleting records from database

```c#
public void Delete()
{
    using (var database = new Database(_connection))
    {
        var people = database.Query<Person>().AsDbSet();

        people.Delete(people.FirstOrDefault(x => x.Id == 1));
        people.Delete(people.FirstOrDefault(x => x.Id == 2));

        people.CommitChanges();
    }
}
```

##### Explicit transactions

By default, changes of a DbSet are committed to database in a transaction. If committing of one record fails, rollback of transaction is executed.

Sometimes you might come to situation, when such implicit transaction would not meet your requirements. For example you need to commit changes to two tables as an atomic operation. When saving changes to first of tables is not successful, you want to discard changes to the other table. Solution of that task is easy with explicit transactions supported by KORM. See the documentation of [BeginTransaction](https://kros-sk.github.io/Kros.Libs/api/Kros.KORM/Kros.KORM.IDatabase.html#Kros_KORM_IDatabase_BeginTransaction).

```c#
using (var transaction = database.BeginTransaction())
{
    var invoicesDbSet = database.Query<Invoice>().AsDbSet();
    var itemsDbSet = database.Query<Item>().AsDbSet();

    try
    {
        invoicesDbSet.Add(invoices);
        invoicesDbSet.CommitChanges();

        itemsDbSet.Add(items);
        itemsDbSet.CommitChanges();

        transaction.Commit();
    }
    catch
    {
        transaction.Rollback();
    }
}
```

### SQL commands executing

KORM supports SQL commands execution. There are three types of commands:

* ```ExecuteNonQuery``` for commands that do not return value (DELETE, UPDATE, ...)
* ```ExecuteScalar``` for commands that return only one value (SELECT)
* ```ExecuteStoredProcedure``` for executing of stored procedures. Stored procedure may return scalar value or list of values or 
it can return data in output parameters.

##### Execution of stored procedure example

```c#
public class Person
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime BDay { get; set; }
}

private Database _database = new Database(new SqlConnection("connection string"));

// Stored procedure returns a scalar value.
int intResult = _database.ExecuteStoredProcedure<int>("ProcedureName");
DateTime dateResult = _database.ExecuteStoredProcedure<DateTime>("ProcedureName");

// Stored procedure sets the value of output parameter.
var parameters = new CommandParameterCollection();
parameters.Add("@param1", 10);
parameters.Add("@param2", DateTime.Now);
parameters.Add("@outputParam", null, DbType.String, ParameterDirection.Output);

_database.ExecuteStoredProcedure<string>("ProcedureName", parameters);

Console.WriteLine(parameters["@outputParam"].Value);


// Stored procedure returns complex object.
Person person = _database.ExecuteStoredProcedure<Person>("ProcedureName");


// Stored procedure returns list of complex objects.
IEnumerable<Person> persons = _database.ExecuteStoredProcedure<IEnumerable<Person>>("ProcedureName");
```

##### CommandTimeout support.

If you want to execute time-consuming command, you will definitely appreciate CommandTimeout property of transaction. See the documentation of [BeginTransaction](https://kros-sk.github.io/Kros.Libs/api/Kros.KORM/Kros.KORM.IDatabase.html#Kros_KORM_IDatabase_BeginTransaction).

Warning: You can set CommandTimeout only for main transaction, not for nested transactions. In that case CommandTimout of main transaction will be used.

```c#
IEnumerable<Person> persons = null;

using (var transaction = database.BeginTransaction(IsolationLevel.Chaos))
{
    transaction.CommandTimeout = 150;

    try
    {
        persons = database.ExecuteStoredProcedure<IEnumerable<Person>>("LongRunningProcedure_GetPersons");
        transaction.Commit();
    }
    catch
    {
        transaction.Rollback();
    }
}
```

### Logging

### Supported database types

### Unit and performance tests
