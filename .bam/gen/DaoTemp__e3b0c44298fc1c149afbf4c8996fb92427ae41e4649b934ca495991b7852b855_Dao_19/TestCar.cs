/*
	This file was generated and should not be modified directly (handlebars template)
*/
// Model is Table
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Bam;
using Bam.Data;
using Bam.Data.Qi;

namespace Bam.Generators.Tests.TestClasses.Dao
{
	// schema = _e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855_Dao
	// connection Name = _e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855_Dao
	[Serializable]
	[Bam.Data.Table("TestCar", "_e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855_Dao")]
	public partial class TestCar: Bam.Data.Dao
	{
		public TestCar():base()
		{
			this.SetKeyColumnName();
			this.SetChildren();
		}

		public TestCar(DataRow data)
			: base(data)
		{
			this.SetKeyColumnName();
			this.SetChildren();
		}

		public TestCar(IDatabase db)
			: base(db)
		{
			this.SetKeyColumnName();
			this.SetChildren();
		}

		public TestCar(IDatabase db, DataRow data)
			: base(db, data)
		{
			this.SetKeyColumnName();
			this.SetChildren();
		}

		[Bam.Exclude]
		public static implicit operator TestCar(DataRow data)
		{
			return new TestCar(data);
		}

		private void SetChildren()
		{




		} // end SetChildren

	// property: Id, columnName: Id
	[Bam.Exclude]
	[Bam.Data.KeyColumn(Name="Id", DbDataType="BigInt", MaxLength="19")]
	public ulong? Id
	{
		get
		{
			return GetULongValue("Id");
		}
		set
		{
			SetValue("Id", value);
		}
	}
    // property:Uuid, columnName: Uuid	
    [Bam.Data.Column(Name="Uuid", DbDataType="VarChar", MaxLength="4000", AllowNull=false)]
    public string Uuid
    {
        get
        {
            return GetStringValue("Uuid");
        }
        set
        {
            SetValue("Uuid", value);
        }
    }

    // property:Cuid, columnName: Cuid	
    [Bam.Data.Column(Name="Cuid", DbDataType="VarChar", MaxLength="4000", AllowNull=true)]
    public string Cuid
    {
        get
        {
            return GetStringValue("Cuid");
        }
        set
        {
            SetValue("Cuid", value);
        }
    }

    // property:Make, columnName: Make	
    [Bam.Data.Column(Name="Make", DbDataType="VarChar", MaxLength="4000", AllowNull=true)]
    public string Make
    {
        get
        {
            return GetStringValue("Make");
        }
        set
        {
            SetValue("Make", value);
        }
    }

    // property:Model, columnName: Model	
    [Bam.Data.Column(Name="Model", DbDataType="VarChar", MaxLength="4000", AllowNull=true)]
    public string Model
    {
        get
        {
            return GetStringValue("Model");
        }
        set
        {
            SetValue("Model", value);
        }
    }


	// start TestPersonId -> TestPersonId
	[Bam.Data.ForeignKey(
        Table="TestCar",
		Name="TestPersonId",
		DbDataType="BigInt",
		MaxLength="",
		AllowNull=true,
		ReferencedKey="Id",
		ReferencedTable="TestPerson",
		Suffix="1")]
	public ulong? TestPersonId
	{
		get
		{
			return GetULongValue("TestPersonId", false);
		}
		set
		{
			SetValue("TestPersonId", value, false);
		}
	}

    TestPerson _testPersonOfTestPersonId;
	public TestPerson TestPersonOfTestPersonId
	{
		get
		{
			if(_testPersonOfTestPersonId == null)
			{
				_testPersonOfTestPersonId = Bam.Generators.Tests.TestClasses.Dao.TestPerson.OneWhere(c => c.KeyColumn == this.TestPersonId, this.Database);
			}
			return _testPersonOfTestPersonId;
		}
	}






		/// <summary>
        /// Gets a query filter that should uniquely identify
        /// the current instance.  The default implementation
        /// compares the Id/key field to the current instance's.
        /// </summary>
		[Bam.Exclude]
		public override IQueryFilter GetUniqueFilter()
		{
			if(UniqueFilterProvider != null)
			{
				return UniqueFilterProvider(this);
			}
			else
			{
				var colFilter = new TestCarColumns();
				return (colFilter.KeyColumn == GetDbId());
			}
		}

		/// <summary>
        /// Return every record in the TestCar table.
        /// </summary>
		/// <param name="database">
		/// The database to load from or null
		/// </param>
		public static TestCarCollection LoadAll(IDatabase database = null)
		{
			IDatabase db = database ?? Db.For<TestCar>();
            ISqlStringBuilder sql = db.GetSqlStringBuilder();
            sql.Select<TestCar>();
            var results = new TestCarCollection(db, sql.ExecuteGetDataTable(db))
            {
                Database = db
            };
            return results;
        }

        /// <summary>
        /// Process all records in batches of the specified size
        /// </summary>
        [Bam.Exclude]
        public static async Task BatchAll(int batchSize, Action<IEnumerable<TestCar>> batchProcessor, IDatabase database = null)
		{
			await Task.Run(async ()=>
			{
				TestCarColumns columns = new TestCarColumns();
				var orderBy = Bam.Data.Order.By<TestCarColumns>(c => c.KeyColumn, Bam.Data.SortOrder.Ascending);
				var results = Top(batchSize, (c) => c.KeyColumn > 0, orderBy, database);
				while(results.Count > 0)
				{
					await Task.Run(()=>
					{
						batchProcessor(results);
					});
					long topId = results.Select(d => d.Property<long>(columns.KeyColumn.ToString())).ToArray().Largest();
					results = Top(batchSize, (c) => c.KeyColumn > topId, orderBy, database);
				}
			});
		}

		public static TestCar GetById(uint? id, IDatabase database = null)
		{
			Args.ThrowIfNull(id, "id");
			Args.ThrowIf(!id.HasValue, "specified TestCar.Id was null");
			return GetById(id.Value, database);
		}

		public static TestCar GetById(uint id, IDatabase database = null)
		{
			return GetById((ulong)id, database);
		}

		public static TestCar GetById(int? id, IDatabase database = null)
		{
			Args.ThrowIfNull(id, "id");
			Args.ThrowIf(!id.HasValue, "specified TestCar.Id was null");
			return GetById(id.Value, database);
		}                                    
                                    
		public static TestCar GetById(int id, IDatabase database = null)
		{
			return GetById((long)id, database);
		}

		public static TestCar GetById(long? id, IDatabase database = null)
		{
			Args.ThrowIfNull(id, "id");
			Args.ThrowIf(!id.HasValue, "specified TestCar.Id was null");
			return GetById(id.Value, database);
		}
                                    
		public static TestCar GetById(long id, IDatabase database = null)
		{
			return OneWhere(c => c.KeyColumn == id, database);
		}

		public static TestCar GetById(ulong? id, IDatabase database = null)
		{
			Args.ThrowIfNull(id, "id");
			Args.ThrowIf(!id.HasValue, "specified TestCar.Id was null");
			return GetById(id.Value, database);
		}
                                    
		public static TestCar GetById(ulong id, IDatabase database = null)
		{
			return OneWhere(c => c.KeyColumn == id, database);
		}

		public static TestCar GetByUuid(string uuid, IDatabase database = null)
		{
			return OneWhere(c => Bam.Data.Query.Where("Uuid") == uuid, database);
		}

		public static TestCar GetByCuid(string cuid, IDatabase database = null)
		{
			return OneWhere(c => Bam.Data.Query.Where("Cuid") == cuid, database);
		}

		[Bam.Exclude]
		public static TestCarCollection Query(QueryFilter filter, IDatabase database = null)
		{
			return Where(filter, database);
		}

		[Bam.Exclude]
		public static TestCarCollection Where(QueryFilter filter, IDatabase database = null)
		{
			WhereDelegate<TestCarColumns> whereDelegate = (c) => filter;
			return Where(whereDelegate, database);
		}

		/// <summary>
		/// Execute a query and return the results.
		/// </summary>
		/// <param name="where">A Func delegate that recieves a TestCarColumns
		/// and returns a QueryFilter which is the result of any comparisons
		/// between TestCarColumns and other values
		/// </param>
		/// <param name="db"></param>
		[Bam.Exclude]
		public static TestCarCollection Where(Func<TestCarColumns, QueryFilter<TestCarColumns>> where, OrderBy<TestCarColumns> orderBy = null, IDatabase database = null)
		{
			database = database ?? Db.For<TestCar>();
			return new TestCarCollection(database.GetQuery<TestCarColumns, TestCar>(where, orderBy), true);
		}

		/// <summary>
		/// Execute a query and return the results.
		/// </summary>
		/// <param name="where">A WhereDelegate that recieves a TestCarColumns
		/// and returns a IQueryFilter which is the result of any comparisons
		/// between TestCarColumns and other values
		/// </param>
		/// <param name="db"></param>
		[Bam.Exclude]
		public static TestCarCollection Where(WhereDelegate<TestCarColumns> where, IDatabase database = null)
		{
			database = database ?? Db.For<TestCar>();
			var results = new TestCarCollection(database, database.GetQuery<TestCarColumns, TestCar>(where), true);
			return results;
		}

		/// <summary>
		/// Execute a query and return the results.
		/// </summary>
		/// <param name="where">A WhereDelegate that recieves a TestCarColumns
		/// and returns a IQueryFilter which is the result of any comparisons
		/// between TestCarColumns and other values
		/// </param>
		/// <param name="orderBy">
		/// Specifies what column and direction to order the results by
		/// </param>
		/// <param name="database"></param>
		[Bam.Exclude]
		public static TestCarCollection Where(WhereDelegate<TestCarColumns> where, OrderBy<TestCarColumns> orderBy = null, IDatabase database = null)
		{
			database = database ?? Db.For<TestCar>();
			var results = new TestCarCollection(database, database.GetQuery<TestCarColumns, TestCar>(where, orderBy), true);
			return results;
		}

		/// <summary>
		/// This method is intended to respond to client side Qi queries.
		/// Use of this method from .Net should be avoided in favor of
		/// one of the methods that take a delegate of type
		/// WhereDelegate`TestCarColumns`.
		/// </summary>
		/// <param name="where"></param>
		/// <param name="database"></param>
		public static TestCarCollection Where(QiQuery where, IDatabase database = null)
		{
			var results = new TestCarCollection(database, Select<TestCarColumns>.From<TestCar>().Where(where, database));
			return results;
		}

		/// <summary>
		/// Get one entry matching the specified filter.  If none exists
		/// one will be created; success will depend on the nullability
		/// of the specified columns.
		/// </summary>
		[Bam.Exclude]
		public static TestCar GetOneWhere(QueryFilter where, IDatabase database = null)
		{
			var result = OneWhere(where, database);
			if(result == null)
			{
				result = CreateFromFilter(where, database);
			}

			return result;
		}

		/// <summary>
		/// Execute a query that should return only one result.  If more
		/// than one result is returned a MultipleEntriesFoundException will
		/// be thrown.
		/// </summary>
		/// <param name="where"></param>
		/// <param name="database"></param>
		[Bam.Exclude]
		public static TestCar OneWhere(QueryFilter where, IDatabase database = null)
		{
			WhereDelegate<TestCarColumns> whereDelegate = (c) => where;
			var result = Top(1, whereDelegate, database);
			return OneOrThrow(result);
		}

		/// <summary>
		/// Set one entry matching the specified filter.  If none exists
		/// one will be created; success will depend on the nullability
		/// of the specified columns.
		/// </summary>
		[Bam.Exclude]
		public static void SetOneWhere(WhereDelegate<TestCarColumns> where, IDatabase database = null)
		{
			SetOneWhere(where, out TestCar ignore, database);
		}

		/// <summary>
		/// Set one entry matching the specified filter.  If none exists
		/// one will be created; success will depend on the nullability
		/// of the specified columns.
		/// </summary>
		[Bam.Exclude]
		public static void SetOneWhere(WhereDelegate<TestCarColumns> where, out TestCar result, IDatabase database = null)
		{
			result = GetOneWhere(where, database);
		}

		/// <summary>
		/// Get one entry matching the specified filter.  If none exists
		/// one will be created; success will depend on the nullability
		/// of the specified columns.
		/// </summary>
		/// <param name="where"></param>
		/// <param name="database"></param>
		[Bam.Exclude]
		public static TestCar GetOneWhere(WhereDelegate<TestCarColumns> where, IDatabase database = null)
		{
			var result = OneWhere(where, database);
			if(result == null)
			{
				TestCarColumns c = new TestCarColumns();
				IQueryFilter filter = where(c);
				result = CreateFromFilter(filter, database);
			}

			return result;
		}

		/// <summary>
		/// Execute a query that should return only one result.  If more
		/// than one result is returned a MultipleEntriesFoundException will
		/// be thrown.  This method is most commonly used to retrieve a
		/// single TestCar instance by its Id/Key value
		/// </summary>
		/// <param name="where">A WhereDelegate that recieves a TestCarColumns
		/// and returns a IQueryFilter which is the result of any comparisons
		/// between TestCarColumns and other values
		/// </param>
		/// <param name="database"></param>
		[Bam.Exclude]
		public static TestCar OneWhere(WhereDelegate<TestCarColumns> where, IDatabase database = null)
		{
			var result = Top(1, where, database);
			return OneOrThrow(result);
		}

		/// <summary>
		/// This method is intended to respond to client side Qi queries.
		/// Use of this method from .Net should be avoided in favor of
		/// one of the methods that take a delegate of type
		/// WhereDelegate`TestCarColumns`.
		/// </summary>
		/// <param name="where"></param>
		/// <param name="database"></param>
		public static TestCar OneWhere(QiQuery where, IDatabase database = null)
		{
			var results = Top(1, where, database);
			return OneOrThrow(results);
		}

		/// <summary>
		/// Execute a query and return the first result.  This method will issue a sql TOP clause so only the
		/// specified number of values will be returned.
		/// </summary>
		/// <param name="where">A WhereDelegate that recieves a TestCarColumns
		/// and returns a IQueryFilter which is the result of any comparisons
		/// between TestCarColumns and other values
		/// </param>
		/// <param name="database"></param>
		[Bam.Exclude]
		public static TestCar FirstOneWhere(WhereDelegate<TestCarColumns> where, IDatabase database = null)
		{
			var results = Top(1, where, database);
			if(results.Count > 0)
			{
				return results[0];
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Execute a query and return the first result.  This method will issue a sql TOP clause so only the
		/// specified number of values will be returned.
		/// </summary>
		/// <param name="where">A WhereDelegate that recieves a TestCarColumns
		/// and returns a IQueryFilter which is the result of any comparisons
		/// between TestCarColumns and other values
		/// </param>
		/// <param name="database"></param>
		[Bam.Exclude]
		public static TestCar FirstOneWhere(WhereDelegate<TestCarColumns> where, OrderBy<TestCarColumns> orderBy, IDatabase database = null)
		{
			var results = Top(1, where, orderBy, database);
			if(results.Count > 0)
			{
				return results[0];
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Shortcut for Top(1, where, orderBy, database)
		/// </summary>
		/// <param name="where">A WhereDelegate that recieves a TestCarColumns
		/// and returns a IQueryFilter which is the result of any comparisons
		/// between TestCarColumns and other values
		/// </param>
		/// <param name="database"></param>
		[Bam.Exclude]
		public static TestCar FirstOneWhere(QueryFilter where, OrderBy<TestCarColumns> orderBy = null, IDatabase database = null)
		{
			WhereDelegate<TestCarColumns> whereDelegate = (c) => where;
			var results = Top(1, whereDelegate, orderBy, database);
			if(results.Count > 0)
			{
				return results[0];
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Execute a query and return the specified number
		/// of values. This method will issue a sql TOP clause so only the
		/// specified number of values will be returned.
		/// </summary>
		/// <param name="count">The number of values to return.
		/// This value is used in the sql query so no more than this
		/// number of values will be returned by the database.
		/// </param>
		/// <param name="where">A WhereDelegate that recieves a TestCarColumns
		/// and returns a IQueryFilter which is the result of any comparisons
		/// between TestCarColumns and other values
		/// </param>
		/// <param name="database"></param>
		[Bam.Exclude]
		public static TestCarCollection Top(int count, WhereDelegate<TestCarColumns> where, IDatabase database = null)
		{
			return Top(count, where, null, database);
		}

		/// <summary>
		/// Execute a query and return the specified number of values.  This method
		/// will issue a sql TOP clause so only the specified number of values
		/// will be returned.
		/// </summary>
		/// <param name="count">The number of values to return.
		/// This value is used in the sql query so no more than this
		/// number of values will be returned by the database.
		/// </param>
		/// <param name="where">A WhereDelegate that recieves a TestCarColumns
		/// and returns a IQueryFilter which is the result of any comparisons
		/// between TestCarColumns and other values
		/// </param>
		/// <param name="orderBy">
		/// Specifies what column and direction to order the results by
		/// </param>
		/// <param name="database">
		/// Which database to query or null to use the default
		/// </param>
		[Bam.Exclude]
		public static TestCarCollection Top(int count, WhereDelegate<TestCarColumns> where, OrderBy<TestCarColumns> orderBy, IDatabase database = null)
		{
			TestCarColumns c = new TestCarColumns();
			IQueryFilter filter = where(c);

			IDatabase db = database ?? Db.For<TestCar>();
			IQuerySet query = GetQuerySet(db);
			query.Top<TestCar>(count);
			query.Where(filter);

			if(orderBy != null)
			{
				query.OrderBy<TestCarColumns>(orderBy);
			}

			query.Execute(db);
			var results = query.Results.As<TestCarCollection>(0);
			results.Database = db;
			return results;
		}

		[Bam.Exclude]
		public static TestCarCollection Top(int count, QueryFilter where, IDatabase database)
		{
			return Top(count, where, null, database);
		}
		/// <summary>
		/// Execute a query and return the specified number of values.  This method
		/// will issue a sql TOP clause so only the specified number of values
		/// will be returned.
		/// of values
		/// </summary>
		/// <param name="count">The number of values to return.
		/// This value is used in the sql query so no more than this
		/// number of values will be returned by the database.
		/// </param>
		/// <param name="where">A QueryFilter used to filter the
		/// results
		/// </param>
		/// <param name="orderBy">
		/// Specifies what column and direction to order the results by
		/// </param>
		/// <param name="database">
		/// Which database to query or null to use the default
		/// </param>
		[Bam.Exclude]
		public static TestCarCollection Top(int count, QueryFilter where, OrderBy<TestCarColumns> orderBy = null, IDatabase database = null)
		{
			IDatabase db = database ?? Db.For<TestCar>();
			IQuerySet query = GetQuerySet(db);
			query.Top<TestCar>(count);
			query.Where(where);

			if(orderBy != null)
			{
				query.OrderBy<TestCarColumns>(orderBy);
			}

			query.Execute(db);
			var results = query.Results.As<TestCarCollection>(0);
			results.Database = db;
			return results;
		}

		[Bam.Exclude]
		public static TestCarCollection Top(int count, QueryFilter where, string orderBy = null, SortOrder sortOrder = SortOrder.Ascending, IDatabase database = null)
		{
			IDatabase db = database ?? Db.For<TestCar>();
			IQuerySet query = GetQuerySet(db);
			query.Top<TestCar>(count);
			query.Where(where);

			if(orderBy != null)
			{
				query.OrderBy(orderBy, sortOrder);
			}

			query.Execute(db);
			var results = query.Results.As<TestCarCollection>(0);
			results.Database = db;
			return results;
		}

		/// <summary>
		/// Execute a query and return the specified number of values.  This method
		/// will issue a sql TOP clause so only the specified number of values
		/// will be returned.
		/// of values
		/// </summary>
		/// <param name="count">The number of values to return.
		/// This value is used in the sql query so no more than this
		/// number of values will be returned by the database.
		/// </param>
		/// <param name="where">A QueryFilter used to filter the
		/// results
		/// </param>
		/// <param name="database">
		/// Which database to query or null to use the default
		/// </param>
		public static TestCarCollection Top(int count, QiQuery where, IDatabase database = null)
		{
			IDatabase db = database ?? Db.For<TestCar>();
			IQuerySet query = GetQuerySet(db);
			query.Top<TestCar>(count);
			query.Where(where);
			query.Execute(db);
			var results = query.Results.As<TestCarCollection>(0);
			results.Database = db;
			return results;
		}

		/// <summary>
		/// Return the count of @(Model.ClassName.Pluralize())
		/// </summary>
		/// <param name="database">
		/// Which database to query or null to use the default
		/// </param>
		public static long Count(IDatabase database = null)
        {
			IDatabase db = database ?? Db.For<TestCar>();
            IQuerySet query = GetQuerySet(db);
            query.Count<TestCar>();
            query.Execute(db);
            return (long)query.Results[0].DataRow[0];
        }

		/// <summary>
		/// Execute a query and return the number of results
		/// </summary>
		/// <param name="where">A WhereDelegate that recieves a TestCarColumns
		/// and returns a IQueryFilter which is the result of any comparisons
		/// between TestCarColumns and other values
		/// </param>
		/// <param name="database">
		/// Which database to query or null to use the default
		/// </param>
		[Bam.Exclude]
		public static long Count(WhereDelegate<TestCarColumns> where, IDatabase database = null)
		{
			TestCarColumns c = new TestCarColumns();
			IQueryFilter filter = where(c) ;

			IDatabase db = database ?? Db.For<TestCar>();
			IQuerySet query = GetQuerySet(db);
			query.Count<TestCar>();
			query.Where(filter);
			query.Execute(db);
			return query.Results.As<CountResult>(0).Value;
		}

		public static long Count(QiQuery where, IDatabase database = null)
		{
		    IDatabase db = database ?? Db.For<TestCar>();
			IQuerySet query = GetQuerySet(db);
			query.Count<TestCar>();
			query.Where(where);
			query.Execute(db);
			return query.Results.As<CountResult>(0).Value;
		}

		private static TestCar CreateFromFilter(IQueryFilter filter, IDatabase database = null)
		{
			IDatabase db = database ?? Db.For<TestCar>();
			var dao = new TestCar();
			filter.Parameters.Each(p=>
			{
				dao.Property(p.ColumnName, p.Value);
			});
			dao.Save(db);
			return dao;
		}

		private static TestCar OneOrThrow(TestCarCollection c)
		{
			if(c.Count == 1)
			{
				return c[0];
			}
			else if(c.Count > 1)
			{
				throw new MultipleEntriesFoundException();
			}

			return null;
		}

	}
}
