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
	[Bam.Data.Table("TestPersonTestAnimal", "_e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855_Dao")]
	public partial class TestPersonTestAnimal: Bam.Data.Dao
	{
		public TestPersonTestAnimal():base()
		{
			this.SetKeyColumnName();
			this.SetChildren();
		}

		public TestPersonTestAnimal(DataRow data)
			: base(data)
		{
			this.SetKeyColumnName();
			this.SetChildren();
		}

		public TestPersonTestAnimal(IDatabase db)
			: base(db)
		{
			this.SetKeyColumnName();
			this.SetChildren();
		}

		public TestPersonTestAnimal(IDatabase db, DataRow data)
			: base(db, data)
		{
			this.SetKeyColumnName();
			this.SetChildren();
		}

		[Bam.Exclude]
		public static implicit operator TestPersonTestAnimal(DataRow data)
		{
			return new TestPersonTestAnimal(data);
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


	// start TestPersonId -> TestPersonId
	[Bam.Data.ForeignKey(
        Table="TestPersonTestAnimal",
		Name="TestPersonId",
		DbDataType="BigInt",
		MaxLength="",
		AllowNull=false,
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

	// start TestAnimalId -> TestAnimalId
	[Bam.Data.ForeignKey(
        Table="TestPersonTestAnimal",
		Name="TestAnimalId",
		DbDataType="BigInt",
		MaxLength="",
		AllowNull=false,
		ReferencedKey="Id",
		ReferencedTable="TestAnimal",
		Suffix="2")]
	public ulong? TestAnimalId
	{
		get
		{
			return GetULongValue("TestAnimalId", false);
		}
		set
		{
			SetValue("TestAnimalId", value, false);
		}
	}

    TestAnimal _testAnimalOfTestAnimalId;
	public TestAnimal TestAnimalOfTestAnimalId
	{
		get
		{
			if(_testAnimalOfTestAnimalId == null)
			{
				_testAnimalOfTestAnimalId = Bam.Generators.Tests.TestClasses.Dao.TestAnimal.OneWhere(c => c.KeyColumn == this.TestAnimalId, this.Database);
			}
			return _testAnimalOfTestAnimalId;
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
				var colFilter = new TestPersonTestAnimalColumns();
				return (colFilter.KeyColumn == GetDbId());
			}
		}

		/// <summary>
        /// Return every record in the TestPersonTestAnimal table.
        /// </summary>
		/// <param name="database">
		/// The database to load from or null
		/// </param>
		public static TestPersonTestAnimalCollection LoadAll(IDatabase database = null)
		{
			IDatabase db = database ?? Db.For<TestPersonTestAnimal>();
            ISqlStringBuilder sql = db.GetSqlStringBuilder();
            sql.Select<TestPersonTestAnimal>();
            var results = new TestPersonTestAnimalCollection(db, sql.ExecuteGetDataTable(db))
            {
                Database = db
            };
            return results;
        }

        /// <summary>
        /// Process all records in batches of the specified size
        /// </summary>
        [Bam.Exclude]
        public static async Task BatchAll(int batchSize, Action<IEnumerable<TestPersonTestAnimal>> batchProcessor, IDatabase database = null)
		{
			await Task.Run(async ()=>
			{
				TestPersonTestAnimalColumns columns = new TestPersonTestAnimalColumns();
				var orderBy = Bam.Data.Order.By<TestPersonTestAnimalColumns>(c => c.KeyColumn, Bam.Data.SortOrder.Ascending);
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

		public static TestPersonTestAnimal GetById(uint? id, IDatabase database = null)
		{
			Args.ThrowIfNull(id, "id");
			Args.ThrowIf(!id.HasValue, "specified TestPersonTestAnimal.Id was null");
			return GetById(id.Value, database);
		}

		public static TestPersonTestAnimal GetById(uint id, IDatabase database = null)
		{
			return GetById((ulong)id, database);
		}

		public static TestPersonTestAnimal GetById(int? id, IDatabase database = null)
		{
			Args.ThrowIfNull(id, "id");
			Args.ThrowIf(!id.HasValue, "specified TestPersonTestAnimal.Id was null");
			return GetById(id.Value, database);
		}                                    
                                    
		public static TestPersonTestAnimal GetById(int id, IDatabase database = null)
		{
			return GetById((long)id, database);
		}

		public static TestPersonTestAnimal GetById(long? id, IDatabase database = null)
		{
			Args.ThrowIfNull(id, "id");
			Args.ThrowIf(!id.HasValue, "specified TestPersonTestAnimal.Id was null");
			return GetById(id.Value, database);
		}
                                    
		public static TestPersonTestAnimal GetById(long id, IDatabase database = null)
		{
			return OneWhere(c => c.KeyColumn == id, database);
		}

		public static TestPersonTestAnimal GetById(ulong? id, IDatabase database = null)
		{
			Args.ThrowIfNull(id, "id");
			Args.ThrowIf(!id.HasValue, "specified TestPersonTestAnimal.Id was null");
			return GetById(id.Value, database);
		}
                                    
		public static TestPersonTestAnimal GetById(ulong id, IDatabase database = null)
		{
			return OneWhere(c => c.KeyColumn == id, database);
		}

		public static TestPersonTestAnimal GetByUuid(string uuid, IDatabase database = null)
		{
			return OneWhere(c => Bam.Data.Query.Where("Uuid") == uuid, database);
		}

		public static TestPersonTestAnimal GetByCuid(string cuid, IDatabase database = null)
		{
			return OneWhere(c => Bam.Data.Query.Where("Cuid") == cuid, database);
		}

		[Bam.Exclude]
		public static TestPersonTestAnimalCollection Query(QueryFilter filter, IDatabase database = null)
		{
			return Where(filter, database);
		}

		[Bam.Exclude]
		public static TestPersonTestAnimalCollection Where(QueryFilter filter, IDatabase database = null)
		{
			WhereDelegate<TestPersonTestAnimalColumns> whereDelegate = (c) => filter;
			return Where(whereDelegate, database);
		}

		/// <summary>
		/// Execute a query and return the results.
		/// </summary>
		/// <param name="where">A Func delegate that recieves a TestPersonTestAnimalColumns
		/// and returns a QueryFilter which is the result of any comparisons
		/// between TestPersonTestAnimalColumns and other values
		/// </param>
		/// <param name="db"></param>
		[Bam.Exclude]
		public static TestPersonTestAnimalCollection Where(Func<TestPersonTestAnimalColumns, QueryFilter<TestPersonTestAnimalColumns>> where, OrderBy<TestPersonTestAnimalColumns> orderBy = null, IDatabase database = null)
		{
			database = database ?? Db.For<TestPersonTestAnimal>();
			return new TestPersonTestAnimalCollection(database.GetQuery<TestPersonTestAnimalColumns, TestPersonTestAnimal>(where, orderBy), true);
		}

		/// <summary>
		/// Execute a query and return the results.
		/// </summary>
		/// <param name="where">A WhereDelegate that recieves a TestPersonTestAnimalColumns
		/// and returns a IQueryFilter which is the result of any comparisons
		/// between TestPersonTestAnimalColumns and other values
		/// </param>
		/// <param name="db"></param>
		[Bam.Exclude]
		public static TestPersonTestAnimalCollection Where(WhereDelegate<TestPersonTestAnimalColumns> where, IDatabase database = null)
		{
			database = database ?? Db.For<TestPersonTestAnimal>();
			var results = new TestPersonTestAnimalCollection(database, database.GetQuery<TestPersonTestAnimalColumns, TestPersonTestAnimal>(where), true);
			return results;
		}

		/// <summary>
		/// Execute a query and return the results.
		/// </summary>
		/// <param name="where">A WhereDelegate that recieves a TestPersonTestAnimalColumns
		/// and returns a IQueryFilter which is the result of any comparisons
		/// between TestPersonTestAnimalColumns and other values
		/// </param>
		/// <param name="orderBy">
		/// Specifies what column and direction to order the results by
		/// </param>
		/// <param name="database"></param>
		[Bam.Exclude]
		public static TestPersonTestAnimalCollection Where(WhereDelegate<TestPersonTestAnimalColumns> where, OrderBy<TestPersonTestAnimalColumns> orderBy = null, IDatabase database = null)
		{
			database = database ?? Db.For<TestPersonTestAnimal>();
			var results = new TestPersonTestAnimalCollection(database, database.GetQuery<TestPersonTestAnimalColumns, TestPersonTestAnimal>(where, orderBy), true);
			return results;
		}

		/// <summary>
		/// This method is intended to respond to client side Qi queries.
		/// Use of this method from .Net should be avoided in favor of
		/// one of the methods that take a delegate of type
		/// WhereDelegate`TestPersonTestAnimalColumns`.
		/// </summary>
		/// <param name="where"></param>
		/// <param name="database"></param>
		public static TestPersonTestAnimalCollection Where(QiQuery where, IDatabase database = null)
		{
			var results = new TestPersonTestAnimalCollection(database, Select<TestPersonTestAnimalColumns>.From<TestPersonTestAnimal>().Where(where, database));
			return results;
		}

		/// <summary>
		/// Get one entry matching the specified filter.  If none exists
		/// one will be created; success will depend on the nullability
		/// of the specified columns.
		/// </summary>
		[Bam.Exclude]
		public static TestPersonTestAnimal GetOneWhere(QueryFilter where, IDatabase database = null)
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
		public static TestPersonTestAnimal OneWhere(QueryFilter where, IDatabase database = null)
		{
			WhereDelegate<TestPersonTestAnimalColumns> whereDelegate = (c) => where;
			var result = Top(1, whereDelegate, database);
			return OneOrThrow(result);
		}

		/// <summary>
		/// Set one entry matching the specified filter.  If none exists
		/// one will be created; success will depend on the nullability
		/// of the specified columns.
		/// </summary>
		[Bam.Exclude]
		public static void SetOneWhere(WhereDelegate<TestPersonTestAnimalColumns> where, IDatabase database = null)
		{
			SetOneWhere(where, out TestPersonTestAnimal ignore, database);
		}

		/// <summary>
		/// Set one entry matching the specified filter.  If none exists
		/// one will be created; success will depend on the nullability
		/// of the specified columns.
		/// </summary>
		[Bam.Exclude]
		public static void SetOneWhere(WhereDelegate<TestPersonTestAnimalColumns> where, out TestPersonTestAnimal result, IDatabase database = null)
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
		public static TestPersonTestAnimal GetOneWhere(WhereDelegate<TestPersonTestAnimalColumns> where, IDatabase database = null)
		{
			var result = OneWhere(where, database);
			if(result == null)
			{
				TestPersonTestAnimalColumns c = new TestPersonTestAnimalColumns();
				IQueryFilter filter = where(c);
				result = CreateFromFilter(filter, database);
			}

			return result;
		}

		/// <summary>
		/// Execute a query that should return only one result.  If more
		/// than one result is returned a MultipleEntriesFoundException will
		/// be thrown.  This method is most commonly used to retrieve a
		/// single TestPersonTestAnimal instance by its Id/Key value
		/// </summary>
		/// <param name="where">A WhereDelegate that recieves a TestPersonTestAnimalColumns
		/// and returns a IQueryFilter which is the result of any comparisons
		/// between TestPersonTestAnimalColumns and other values
		/// </param>
		/// <param name="database"></param>
		[Bam.Exclude]
		public static TestPersonTestAnimal OneWhere(WhereDelegate<TestPersonTestAnimalColumns> where, IDatabase database = null)
		{
			var result = Top(1, where, database);
			return OneOrThrow(result);
		}

		/// <summary>
		/// This method is intended to respond to client side Qi queries.
		/// Use of this method from .Net should be avoided in favor of
		/// one of the methods that take a delegate of type
		/// WhereDelegate`TestPersonTestAnimalColumns`.
		/// </summary>
		/// <param name="where"></param>
		/// <param name="database"></param>
		public static TestPersonTestAnimal OneWhere(QiQuery where, IDatabase database = null)
		{
			var results = Top(1, where, database);
			return OneOrThrow(results);
		}

		/// <summary>
		/// Execute a query and return the first result.  This method will issue a sql TOP clause so only the
		/// specified number of values will be returned.
		/// </summary>
		/// <param name="where">A WhereDelegate that recieves a TestPersonTestAnimalColumns
		/// and returns a IQueryFilter which is the result of any comparisons
		/// between TestPersonTestAnimalColumns and other values
		/// </param>
		/// <param name="database"></param>
		[Bam.Exclude]
		public static TestPersonTestAnimal FirstOneWhere(WhereDelegate<TestPersonTestAnimalColumns> where, IDatabase database = null)
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
		/// <param name="where">A WhereDelegate that recieves a TestPersonTestAnimalColumns
		/// and returns a IQueryFilter which is the result of any comparisons
		/// between TestPersonTestAnimalColumns and other values
		/// </param>
		/// <param name="database"></param>
		[Bam.Exclude]
		public static TestPersonTestAnimal FirstOneWhere(WhereDelegate<TestPersonTestAnimalColumns> where, OrderBy<TestPersonTestAnimalColumns> orderBy, IDatabase database = null)
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
		/// <param name="where">A WhereDelegate that recieves a TestPersonTestAnimalColumns
		/// and returns a IQueryFilter which is the result of any comparisons
		/// between TestPersonTestAnimalColumns and other values
		/// </param>
		/// <param name="database"></param>
		[Bam.Exclude]
		public static TestPersonTestAnimal FirstOneWhere(QueryFilter where, OrderBy<TestPersonTestAnimalColumns> orderBy = null, IDatabase database = null)
		{
			WhereDelegate<TestPersonTestAnimalColumns> whereDelegate = (c) => where;
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
		/// <param name="where">A WhereDelegate that recieves a TestPersonTestAnimalColumns
		/// and returns a IQueryFilter which is the result of any comparisons
		/// between TestPersonTestAnimalColumns and other values
		/// </param>
		/// <param name="database"></param>
		[Bam.Exclude]
		public static TestPersonTestAnimalCollection Top(int count, WhereDelegate<TestPersonTestAnimalColumns> where, IDatabase database = null)
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
		/// <param name="where">A WhereDelegate that recieves a TestPersonTestAnimalColumns
		/// and returns a IQueryFilter which is the result of any comparisons
		/// between TestPersonTestAnimalColumns and other values
		/// </param>
		/// <param name="orderBy">
		/// Specifies what column and direction to order the results by
		/// </param>
		/// <param name="database">
		/// Which database to query or null to use the default
		/// </param>
		[Bam.Exclude]
		public static TestPersonTestAnimalCollection Top(int count, WhereDelegate<TestPersonTestAnimalColumns> where, OrderBy<TestPersonTestAnimalColumns> orderBy, IDatabase database = null)
		{
			TestPersonTestAnimalColumns c = new TestPersonTestAnimalColumns();
			IQueryFilter filter = where(c);

			IDatabase db = database ?? Db.For<TestPersonTestAnimal>();
			IQuerySet query = GetQuerySet(db);
			query.Top<TestPersonTestAnimal>(count);
			query.Where(filter);

			if(orderBy != null)
			{
				query.OrderBy<TestPersonTestAnimalColumns>(orderBy);
			}

			query.Execute(db);
			var results = query.Results.As<TestPersonTestAnimalCollection>(0);
			results.Database = db;
			return results;
		}

		[Bam.Exclude]
		public static TestPersonTestAnimalCollection Top(int count, QueryFilter where, IDatabase database)
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
		public static TestPersonTestAnimalCollection Top(int count, QueryFilter where, OrderBy<TestPersonTestAnimalColumns> orderBy = null, IDatabase database = null)
		{
			IDatabase db = database ?? Db.For<TestPersonTestAnimal>();
			IQuerySet query = GetQuerySet(db);
			query.Top<TestPersonTestAnimal>(count);
			query.Where(where);

			if(orderBy != null)
			{
				query.OrderBy<TestPersonTestAnimalColumns>(orderBy);
			}

			query.Execute(db);
			var results = query.Results.As<TestPersonTestAnimalCollection>(0);
			results.Database = db;
			return results;
		}

		[Bam.Exclude]
		public static TestPersonTestAnimalCollection Top(int count, QueryFilter where, string orderBy = null, SortOrder sortOrder = SortOrder.Ascending, IDatabase database = null)
		{
			IDatabase db = database ?? Db.For<TestPersonTestAnimal>();
			IQuerySet query = GetQuerySet(db);
			query.Top<TestPersonTestAnimal>(count);
			query.Where(where);

			if(orderBy != null)
			{
				query.OrderBy(orderBy, sortOrder);
			}

			query.Execute(db);
			var results = query.Results.As<TestPersonTestAnimalCollection>(0);
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
		public static TestPersonTestAnimalCollection Top(int count, QiQuery where, IDatabase database = null)
		{
			IDatabase db = database ?? Db.For<TestPersonTestAnimal>();
			IQuerySet query = GetQuerySet(db);
			query.Top<TestPersonTestAnimal>(count);
			query.Where(where);
			query.Execute(db);
			var results = query.Results.As<TestPersonTestAnimalCollection>(0);
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
			IDatabase db = database ?? Db.For<TestPersonTestAnimal>();
            IQuerySet query = GetQuerySet(db);
            query.Count<TestPersonTestAnimal>();
            query.Execute(db);
            return (long)query.Results[0].DataRow[0];
        }

		/// <summary>
		/// Execute a query and return the number of results
		/// </summary>
		/// <param name="where">A WhereDelegate that recieves a TestPersonTestAnimalColumns
		/// and returns a IQueryFilter which is the result of any comparisons
		/// between TestPersonTestAnimalColumns and other values
		/// </param>
		/// <param name="database">
		/// Which database to query or null to use the default
		/// </param>
		[Bam.Exclude]
		public static long Count(WhereDelegate<TestPersonTestAnimalColumns> where, IDatabase database = null)
		{
			TestPersonTestAnimalColumns c = new TestPersonTestAnimalColumns();
			IQueryFilter filter = where(c) ;

			IDatabase db = database ?? Db.For<TestPersonTestAnimal>();
			IQuerySet query = GetQuerySet(db);
			query.Count<TestPersonTestAnimal>();
			query.Where(filter);
			query.Execute(db);
			return query.Results.As<CountResult>(0).Value;
		}

		public static long Count(QiQuery where, IDatabase database = null)
		{
		    IDatabase db = database ?? Db.For<TestPersonTestAnimal>();
			IQuerySet query = GetQuerySet(db);
			query.Count<TestPersonTestAnimal>();
			query.Where(where);
			query.Execute(db);
			return query.Results.As<CountResult>(0).Value;
		}

		private static TestPersonTestAnimal CreateFromFilter(IQueryFilter filter, IDatabase database = null)
		{
			IDatabase db = database ?? Db.For<TestPersonTestAnimal>();
			var dao = new TestPersonTestAnimal();
			filter.Parameters.Each(p=>
			{
				dao.Property(p.ColumnName, p.Value);
			});
			dao.Save(db);
			return dao;
		}

		private static TestPersonTestAnimal OneOrThrow(TestPersonTestAnimalCollection c)
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
