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

namespace Bam.Test.Tests.TestClasses.Dao
{
	// schema = _e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855_Dao
	// connection Name = _e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855_Dao
	[Serializable]
	[Bam.Data.Table("TestData", "_e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855_Dao")]
	public partial class TestData: Bam.Data.Dao
	{
		public TestData():base()
		{
			this.SetKeyColumnName();
			this.SetChildren();
		}

		public TestData(DataRow data)
			: base(data)
		{
			this.SetKeyColumnName();
			this.SetChildren();
		}

		public TestData(IDatabase db)
			: base(db)
		{
			this.SetKeyColumnName();
			this.SetChildren();
		}

		public TestData(IDatabase db, DataRow data)
			: base(db, data)
		{
			this.SetKeyColumnName();
			this.SetChildren();
		}

		[Bam.Exclude]
		public static implicit operator TestData(DataRow data)
		{
			return new TestData(data);
		}

		private void SetChildren()
		{


			if(_database != null)
			{
				this.ChildCollections.Add("RelatedData_TestDataId", new RelatedDataCollection(Database.GetQuery<RelatedDataColumns, RelatedData>((c) => c.TestDataId == GetULongValue("Id", false)), this, "TestDataId"));
			}


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

    // property:Name, columnName: Name	
    [Bam.Data.Column(Name="Name", DbDataType="VarChar", MaxLength="4000", AllowNull=true)]
    public string Name
    {
        get
        {
            return GetStringValue("Name");
        }
        set
        {
            SetValue("Name", value);
        }
    }

    // property:IntProperty, columnName: IntProperty	
    [Bam.Data.Column(Name="IntProperty", DbDataType="Int", MaxLength="10", AllowNull=true)]
    public int? IntProperty
    {
        get
        {
            return GetIntValue("IntProperty");
        }
        set
        {
            SetValue("IntProperty", value);
        }
    }

    // property:BooleanProperty, columnName: BooleanProperty	
    [Bam.Data.Column(Name="BooleanProperty", DbDataType="Bit", MaxLength="1", AllowNull=true)]
    public bool? BooleanProperty
    {
        get
        {
            return GetBooleanValue("BooleanProperty");
        }
        set
        {
            SetValue("BooleanProperty", value);
        }
    }

    // property:ULongProperty, columnName: ULongProperty	
    [Bam.Data.Column(Name="ULongProperty", DbDataType="BigInt", MaxLength="19", AllowNull=true)]
    public ulong? ULongProperty
    {
        get
        {
            return GetULongValue("ULongProperty");
        }
        set
        {
            SetValue("ULongProperty", value);
        }
    }

    // property:LongProperty, columnName: LongProperty	
    [Bam.Data.Column(Name="LongProperty", DbDataType="BigInt", MaxLength="19", AllowNull=true)]
    public long? LongProperty
    {
        get
        {
            return GetLongValue("LongProperty");
        }
        set
        {
            SetValue("LongProperty", value);
        }
    }

    // property:DecimalProperty, columnName: DecimalProperty	
    [Bam.Data.Column(Name="DecimalProperty", DbDataType="Decimal", MaxLength="28", AllowNull=true)]
    public decimal? DecimalProperty
    {
        get
        {
            return GetDecimalValue("DecimalProperty");
        }
        set
        {
            SetValue("DecimalProperty", value);
        }
    }

    // property:DateTimeProperty, columnName: DateTimeProperty	
    [Bam.Data.Column(Name="DateTimeProperty", DbDataType="DateTime", MaxLength="8", AllowNull=true)]
    public DateTime? DateTimeProperty
    {
        get
        {
            return GetDateTimeValue("DateTimeProperty");
        }
        set
        {
            SetValue("DateTimeProperty", value);
        }
    }



	[Bam.Exclude]
	public RelatedDataCollection RelatedDatasByTestDataId
	{
		get
		{
			if (this.IsNew)
			{
				throw new InvalidOperationException($"The current instance of type({this.GetType().Name}) hasn't been saved and will have no child collections, call Save() or Save(Database) first.");
			}

			if(!this.ChildCollections.ContainsKey("RelatedData_TestDataId"))
			{
				SetChildren();
			}

			var c = (RelatedDataCollection)this.ChildCollections["RelatedData_TestDataId"];
			if(!c.Loaded)
			{
				c.Load(Database);
			}
			return c;
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
				var colFilter = new TestDataColumns();
				return (colFilter.KeyColumn == GetDbId());
			}
		}

		/// <summary>
        /// Return every record in the TestData table.
        /// </summary>
		/// <param name="database">
		/// The database to load from or null
		/// </param>
		public static TestDataCollection LoadAll(IDatabase database = null)
		{
			IDatabase db = database ?? Db.For<TestData>();
            ISqlStringBuilder sql = db.GetSqlStringBuilder();
            sql.Select<TestData>();
            var results = new TestDataCollection(db, sql.ExecuteGetDataTable(db))
            {
                Database = db
            };
            return results;
        }

        /// <summary>
        /// Process all records in batches of the specified size
        /// </summary>
        [Bam.Exclude]
        public static async Task BatchAll(int batchSize, Action<IEnumerable<TestData>> batchProcessor, IDatabase database = null)
		{
			await Task.Run(async ()=>
			{
				TestDataColumns columns = new TestDataColumns();
				var orderBy = Bam.Data.Order.By<TestDataColumns>(c => c.KeyColumn, Bam.Data.SortOrder.Ascending);
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

		public static TestData GetById(uint? id, IDatabase database = null)
		{
			Args.ThrowIfNull(id, "id");
			Args.ThrowIf(!id.HasValue, "specified TestData.Id was null");
			return GetById(id.Value, database);
		}

		public static TestData GetById(uint id, IDatabase database = null)
		{
			return GetById((ulong)id, database);
		}

		public static TestData GetById(int? id, IDatabase database = null)
		{
			Args.ThrowIfNull(id, "id");
			Args.ThrowIf(!id.HasValue, "specified TestData.Id was null");
			return GetById(id.Value, database);
		}                                    
                                    
		public static TestData GetById(int id, IDatabase database = null)
		{
			return GetById((long)id, database);
		}

		public static TestData GetById(long? id, IDatabase database = null)
		{
			Args.ThrowIfNull(id, "id");
			Args.ThrowIf(!id.HasValue, "specified TestData.Id was null");
			return GetById(id.Value, database);
		}
                                    
		public static TestData GetById(long id, IDatabase database = null)
		{
			return OneWhere(c => c.KeyColumn == id, database);
		}

		public static TestData GetById(ulong? id, IDatabase database = null)
		{
			Args.ThrowIfNull(id, "id");
			Args.ThrowIf(!id.HasValue, "specified TestData.Id was null");
			return GetById(id.Value, database);
		}
                                    
		public static TestData GetById(ulong id, IDatabase database = null)
		{
			return OneWhere(c => c.KeyColumn == id, database);
		}

		public static TestData GetByUuid(string uuid, IDatabase database = null)
		{
			return OneWhere(c => Bam.Data.Query.Where("Uuid") == uuid, database);
		}

		public static TestData GetByCuid(string cuid, IDatabase database = null)
		{
			return OneWhere(c => Bam.Data.Query.Where("Cuid") == cuid, database);
		}

		[Bam.Exclude]
		public static TestDataCollection Query(QueryFilter filter, IDatabase database = null)
		{
			return Where(filter, database);
		}

		[Bam.Exclude]
		public static TestDataCollection Where(QueryFilter filter, IDatabase database = null)
		{
			WhereDelegate<TestDataColumns> whereDelegate = (c) => filter;
			return Where(whereDelegate, database);
		}

		/// <summary>
		/// Execute a query and return the results.
		/// </summary>
		/// <param name="where">A Func delegate that recieves a TestDataColumns
		/// and returns a QueryFilter which is the result of any comparisons
		/// between TestDataColumns and other values
		/// </param>
		/// <param name="db"></param>
		[Bam.Exclude]
		public static TestDataCollection Where(Func<TestDataColumns, QueryFilter<TestDataColumns>> where, OrderBy<TestDataColumns> orderBy = null, IDatabase database = null)
		{
			database = database ?? Db.For<TestData>();
			return new TestDataCollection(database.GetQuery<TestDataColumns, TestData>(where, orderBy), true);
		}

		/// <summary>
		/// Execute a query and return the results.
		/// </summary>
		/// <param name="where">A WhereDelegate that recieves a TestDataColumns
		/// and returns a IQueryFilter which is the result of any comparisons
		/// between TestDataColumns and other values
		/// </param>
		/// <param name="db"></param>
		[Bam.Exclude]
		public static TestDataCollection Where(WhereDelegate<TestDataColumns> where, IDatabase database = null)
		{
			database = database ?? Db.For<TestData>();
			var results = new TestDataCollection(database, database.GetQuery<TestDataColumns, TestData>(where), true);
			return results;
		}

		/// <summary>
		/// Execute a query and return the results.
		/// </summary>
		/// <param name="where">A WhereDelegate that recieves a TestDataColumns
		/// and returns a IQueryFilter which is the result of any comparisons
		/// between TestDataColumns and other values
		/// </param>
		/// <param name="orderBy">
		/// Specifies what column and direction to order the results by
		/// </param>
		/// <param name="database"></param>
		[Bam.Exclude]
		public static TestDataCollection Where(WhereDelegate<TestDataColumns> where, OrderBy<TestDataColumns> orderBy = null, IDatabase database = null)
		{
			database = database ?? Db.For<TestData>();
			var results = new TestDataCollection(database, database.GetQuery<TestDataColumns, TestData>(where, orderBy), true);
			return results;
		}

		/// <summary>
		/// This method is intended to respond to client side Qi queries.
		/// Use of this method from .Net should be avoided in favor of
		/// one of the methods that take a delegate of type
		/// WhereDelegate`TestDataColumns`.
		/// </summary>
		/// <param name="where"></param>
		/// <param name="database"></param>
		public static TestDataCollection Where(QiQuery where, IDatabase database = null)
		{
			var results = new TestDataCollection(database, Select<TestDataColumns>.From<TestData>().Where(where, database));
			return results;
		}

		/// <summary>
		/// Get one entry matching the specified filter.  If none exists
		/// one will be created; success will depend on the nullability
		/// of the specified columns.
		/// </summary>
		[Bam.Exclude]
		public static TestData GetOneWhere(QueryFilter where, IDatabase database = null)
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
		public static TestData OneWhere(QueryFilter where, IDatabase database = null)
		{
			WhereDelegate<TestDataColumns> whereDelegate = (c) => where;
			var result = Top(1, whereDelegate, database);
			return OneOrThrow(result);
		}

		/// <summary>
		/// Set one entry matching the specified filter.  If none exists
		/// one will be created; success will depend on the nullability
		/// of the specified columns.
		/// </summary>
		[Bam.Exclude]
		public static void SetOneWhere(WhereDelegate<TestDataColumns> where, IDatabase database = null)
		{
			SetOneWhere(where, out TestData ignore, database);
		}

		/// <summary>
		/// Set one entry matching the specified filter.  If none exists
		/// one will be created; success will depend on the nullability
		/// of the specified columns.
		/// </summary>
		[Bam.Exclude]
		public static void SetOneWhere(WhereDelegate<TestDataColumns> where, out TestData result, IDatabase database = null)
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
		public static TestData GetOneWhere(WhereDelegate<TestDataColumns> where, IDatabase database = null)
		{
			var result = OneWhere(where, database);
			if(result == null)
			{
				TestDataColumns c = new TestDataColumns();
				IQueryFilter filter = where(c);
				result = CreateFromFilter(filter, database);
			}

			return result;
		}

		/// <summary>
		/// Execute a query that should return only one result.  If more
		/// than one result is returned a MultipleEntriesFoundException will
		/// be thrown.  This method is most commonly used to retrieve a
		/// single TestData instance by its Id/Key value
		/// </summary>
		/// <param name="where">A WhereDelegate that recieves a TestDataColumns
		/// and returns a IQueryFilter which is the result of any comparisons
		/// between TestDataColumns and other values
		/// </param>
		/// <param name="database"></param>
		[Bam.Exclude]
		public static TestData OneWhere(WhereDelegate<TestDataColumns> where, IDatabase database = null)
		{
			var result = Top(1, where, database);
			return OneOrThrow(result);
		}

		/// <summary>
		/// This method is intended to respond to client side Qi queries.
		/// Use of this method from .Net should be avoided in favor of
		/// one of the methods that take a delegate of type
		/// WhereDelegate`TestDataColumns`.
		/// </summary>
		/// <param name="where"></param>
		/// <param name="database"></param>
		public static TestData OneWhere(QiQuery where, IDatabase database = null)
		{
			var results = Top(1, where, database);
			return OneOrThrow(results);
		}

		/// <summary>
		/// Execute a query and return the first result.  This method will issue a sql TOP clause so only the
		/// specified number of values will be returned.
		/// </summary>
		/// <param name="where">A WhereDelegate that recieves a TestDataColumns
		/// and returns a IQueryFilter which is the result of any comparisons
		/// between TestDataColumns and other values
		/// </param>
		/// <param name="database"></param>
		[Bam.Exclude]
		public static TestData FirstOneWhere(WhereDelegate<TestDataColumns> where, IDatabase database = null)
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
		/// <param name="where">A WhereDelegate that recieves a TestDataColumns
		/// and returns a IQueryFilter which is the result of any comparisons
		/// between TestDataColumns and other values
		/// </param>
		/// <param name="database"></param>
		[Bam.Exclude]
		public static TestData FirstOneWhere(WhereDelegate<TestDataColumns> where, OrderBy<TestDataColumns> orderBy, IDatabase database = null)
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
		/// <param name="where">A WhereDelegate that recieves a TestDataColumns
		/// and returns a IQueryFilter which is the result of any comparisons
		/// between TestDataColumns and other values
		/// </param>
		/// <param name="database"></param>
		[Bam.Exclude]
		public static TestData FirstOneWhere(QueryFilter where, OrderBy<TestDataColumns> orderBy = null, IDatabase database = null)
		{
			WhereDelegate<TestDataColumns> whereDelegate = (c) => where;
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
		/// <param name="where">A WhereDelegate that recieves a TestDataColumns
		/// and returns a IQueryFilter which is the result of any comparisons
		/// between TestDataColumns and other values
		/// </param>
		/// <param name="database"></param>
		[Bam.Exclude]
		public static TestDataCollection Top(int count, WhereDelegate<TestDataColumns> where, IDatabase database = null)
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
		/// <param name="where">A WhereDelegate that recieves a TestDataColumns
		/// and returns a IQueryFilter which is the result of any comparisons
		/// between TestDataColumns and other values
		/// </param>
		/// <param name="orderBy">
		/// Specifies what column and direction to order the results by
		/// </param>
		/// <param name="database">
		/// Which database to query or null to use the default
		/// </param>
		[Bam.Exclude]
		public static TestDataCollection Top(int count, WhereDelegate<TestDataColumns> where, OrderBy<TestDataColumns> orderBy, IDatabase database = null)
		{
			TestDataColumns c = new TestDataColumns();
			IQueryFilter filter = where(c);

			IDatabase db = database ?? Db.For<TestData>();
			IQuerySet query = GetQuerySet(db);
			query.Top<TestData>(count);
			query.Where(filter);

			if(orderBy != null)
			{
				query.OrderBy<TestDataColumns>(orderBy);
			}

			query.Execute(db);
			var results = query.Results.As<TestDataCollection>(0);
			results.Database = db;
			return results;
		}

		[Bam.Exclude]
		public static TestDataCollection Top(int count, QueryFilter where, IDatabase database)
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
		public static TestDataCollection Top(int count, QueryFilter where, OrderBy<TestDataColumns> orderBy = null, IDatabase database = null)
		{
			IDatabase db = database ?? Db.For<TestData>();
			IQuerySet query = GetQuerySet(db);
			query.Top<TestData>(count);
			query.Where(where);

			if(orderBy != null)
			{
				query.OrderBy<TestDataColumns>(orderBy);
			}

			query.Execute(db);
			var results = query.Results.As<TestDataCollection>(0);
			results.Database = db;
			return results;
		}

		[Bam.Exclude]
		public static TestDataCollection Top(int count, QueryFilter where, string orderBy = null, SortOrder sortOrder = SortOrder.Ascending, IDatabase database = null)
		{
			IDatabase db = database ?? Db.For<TestData>();
			IQuerySet query = GetQuerySet(db);
			query.Top<TestData>(count);
			query.Where(where);

			if(orderBy != null)
			{
				query.OrderBy(orderBy, sortOrder);
			}

			query.Execute(db);
			var results = query.Results.As<TestDataCollection>(0);
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
		public static TestDataCollection Top(int count, QiQuery where, IDatabase database = null)
		{
			IDatabase db = database ?? Db.For<TestData>();
			IQuerySet query = GetQuerySet(db);
			query.Top<TestData>(count);
			query.Where(where);
			query.Execute(db);
			var results = query.Results.As<TestDataCollection>(0);
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
			IDatabase db = database ?? Db.For<TestData>();
            IQuerySet query = GetQuerySet(db);
            query.Count<TestData>();
            query.Execute(db);
            return (long)query.Results[0].DataRow[0];
        }

		/// <summary>
		/// Execute a query and return the number of results
		/// </summary>
		/// <param name="where">A WhereDelegate that recieves a TestDataColumns
		/// and returns a IQueryFilter which is the result of any comparisons
		/// between TestDataColumns and other values
		/// </param>
		/// <param name="database">
		/// Which database to query or null to use the default
		/// </param>
		[Bam.Exclude]
		public static long Count(WhereDelegate<TestDataColumns> where, IDatabase database = null)
		{
			TestDataColumns c = new TestDataColumns();
			IQueryFilter filter = where(c) ;

			IDatabase db = database ?? Db.For<TestData>();
			IQuerySet query = GetQuerySet(db);
			query.Count<TestData>();
			query.Where(filter);
			query.Execute(db);
			return query.Results.As<CountResult>(0).Value;
		}

		public static long Count(QiQuery where, IDatabase database = null)
		{
		    IDatabase db = database ?? Db.For<TestData>();
			IQuerySet query = GetQuerySet(db);
			query.Count<TestData>();
			query.Where(where);
			query.Execute(db);
			return query.Results.As<CountResult>(0).Value;
		}

		private static TestData CreateFromFilter(IQueryFilter filter, IDatabase database = null)
		{
			IDatabase db = database ?? Db.For<TestData>();
			var dao = new TestData();
			filter.Parameters.Each(p=>
			{
				dao.Property(p.ColumnName, p.Value);
			});
			dao.Save(db);
			return dao;
		}

		private static TestData OneOrThrow(TestDataCollection c)
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
