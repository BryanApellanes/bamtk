/*
	This file was generated and should not be modified directly
*/
// model is SchemaDefinition
using System;
using System.Data;
using System.Data.Common;
using Bam;
using Bam.Data;
using Bam.Data.Qi;

namespace Bam.Test.Tests.TestClasses.Dao
{
	// schema = _e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855_Dao
    public static class _e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855_DaoContext
    {
		public static string ConnectionName
		{
			get
			{
				return "_e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855_Dao";
			}
		}

		public static IDatabase Db
		{
			get
			{
				return Bam.Data.Db.For(ConnectionName);
			}
		}


	public class TestDataQueryContext
	{
			public TestDataCollection Where(WhereDelegate<TestDataColumns> where, Database db = null)
			{
				return Bam.Test.Tests.TestClasses.Dao.TestData.Where(where, db);
			}
		   
			public TestDataCollection Where(WhereDelegate<TestDataColumns> where, OrderBy<TestDataColumns> orderBy = null, Database db = null)
			{
				return Bam.Test.Tests.TestClasses.Dao.TestData.Where(where, orderBy, db);
			}

			public TestData OneWhere(WhereDelegate<TestDataColumns> where, Database db = null)
			{
				return Bam.Test.Tests.TestClasses.Dao.TestData.OneWhere(where, db);
			}

			public static TestData GetOneWhere(WhereDelegate<TestDataColumns> where, Database db = null)
			{
				return Bam.Test.Tests.TestClasses.Dao.TestData.GetOneWhere(where, db);
			}
		
			public TestData FirstOneWhere(WhereDelegate<TestDataColumns> where, Database db = null)
			{
				return Bam.Test.Tests.TestClasses.Dao.TestData.FirstOneWhere(where, db);
			}

			public TestDataCollection Top(int count, WhereDelegate<TestDataColumns> where, Database db = null)
			{
				return Bam.Test.Tests.TestClasses.Dao.TestData.Top(count, where, db);
			}

			public TestDataCollection Top(int count, WhereDelegate<TestDataColumns> where, OrderBy<TestDataColumns> orderBy, Database db = null)
			{
				return Bam.Test.Tests.TestClasses.Dao.TestData.Top(count, where, orderBy, db);
			}

			public long Count(WhereDelegate<TestDataColumns> where, Database db = null)
			{
				return Bam.Test.Tests.TestClasses.Dao.TestData.Count(where, db);
			}
	}

	static TestDataQueryContext _testDatas;
	static object _testDatasLock = new object();
	public static TestDataQueryContext TestDatas
	{
		get
		{
			return _testDatasLock.DoubleCheckLock<TestDataQueryContext>(ref _testDatas, () => new TestDataQueryContext());
		}
	}
	public class RelatedDataQueryContext
	{
			public RelatedDataCollection Where(WhereDelegate<RelatedDataColumns> where, Database db = null)
			{
				return Bam.Test.Tests.TestClasses.Dao.RelatedData.Where(where, db);
			}
		   
			public RelatedDataCollection Where(WhereDelegate<RelatedDataColumns> where, OrderBy<RelatedDataColumns> orderBy = null, Database db = null)
			{
				return Bam.Test.Tests.TestClasses.Dao.RelatedData.Where(where, orderBy, db);
			}

			public RelatedData OneWhere(WhereDelegate<RelatedDataColumns> where, Database db = null)
			{
				return Bam.Test.Tests.TestClasses.Dao.RelatedData.OneWhere(where, db);
			}

			public static RelatedData GetOneWhere(WhereDelegate<RelatedDataColumns> where, Database db = null)
			{
				return Bam.Test.Tests.TestClasses.Dao.RelatedData.GetOneWhere(where, db);
			}
		
			public RelatedData FirstOneWhere(WhereDelegate<RelatedDataColumns> where, Database db = null)
			{
				return Bam.Test.Tests.TestClasses.Dao.RelatedData.FirstOneWhere(where, db);
			}

			public RelatedDataCollection Top(int count, WhereDelegate<RelatedDataColumns> where, Database db = null)
			{
				return Bam.Test.Tests.TestClasses.Dao.RelatedData.Top(count, where, db);
			}

			public RelatedDataCollection Top(int count, WhereDelegate<RelatedDataColumns> where, OrderBy<RelatedDataColumns> orderBy, Database db = null)
			{
				return Bam.Test.Tests.TestClasses.Dao.RelatedData.Top(count, where, orderBy, db);
			}

			public long Count(WhereDelegate<RelatedDataColumns> where, Database db = null)
			{
				return Bam.Test.Tests.TestClasses.Dao.RelatedData.Count(where, db);
			}
	}

	static RelatedDataQueryContext _relatedDatas;
	static object _relatedDatasLock = new object();
	public static RelatedDataQueryContext RelatedDatas
	{
		get
		{
			return _relatedDatasLock.DoubleCheckLock<RelatedDataQueryContext>(ref _relatedDatas, () => new RelatedDataQueryContext());
		}
	}
    }
}																								
