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

namespace Bam.Generators.Tests.TestClasses.Dao
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


	public class TestPersonQueryContext
	{
			public TestPersonCollection Where(WhereDelegate<TestPersonColumns> where, Database db = null)
			{
				return Bam.Generators.Tests.TestClasses.Dao.TestPerson.Where(where, db);
			}
		   
			public TestPersonCollection Where(WhereDelegate<TestPersonColumns> where, OrderBy<TestPersonColumns> orderBy = null, Database db = null)
			{
				return Bam.Generators.Tests.TestClasses.Dao.TestPerson.Where(where, orderBy, db);
			}

			public TestPerson OneWhere(WhereDelegate<TestPersonColumns> where, Database db = null)
			{
				return Bam.Generators.Tests.TestClasses.Dao.TestPerson.OneWhere(where, db);
			}

			public static TestPerson GetOneWhere(WhereDelegate<TestPersonColumns> where, Database db = null)
			{
				return Bam.Generators.Tests.TestClasses.Dao.TestPerson.GetOneWhere(where, db);
			}
		
			public TestPerson FirstOneWhere(WhereDelegate<TestPersonColumns> where, Database db = null)
			{
				return Bam.Generators.Tests.TestClasses.Dao.TestPerson.FirstOneWhere(where, db);
			}

			public TestPersonCollection Top(int count, WhereDelegate<TestPersonColumns> where, Database db = null)
			{
				return Bam.Generators.Tests.TestClasses.Dao.TestPerson.Top(count, where, db);
			}

			public TestPersonCollection Top(int count, WhereDelegate<TestPersonColumns> where, OrderBy<TestPersonColumns> orderBy, Database db = null)
			{
				return Bam.Generators.Tests.TestClasses.Dao.TestPerson.Top(count, where, orderBy, db);
			}

			public long Count(WhereDelegate<TestPersonColumns> where, Database db = null)
			{
				return Bam.Generators.Tests.TestClasses.Dao.TestPerson.Count(where, db);
			}
	}

	static TestPersonQueryContext _testPersons;
	static object _testPersonsLock = new object();
	public static TestPersonQueryContext TestPersons
	{
		get
		{
			return _testPersonsLock.DoubleCheckLock<TestPersonQueryContext>(ref _testPersons, () => new TestPersonQueryContext());
		}
	}
	public class TestCarQueryContext
	{
			public TestCarCollection Where(WhereDelegate<TestCarColumns> where, Database db = null)
			{
				return Bam.Generators.Tests.TestClasses.Dao.TestCar.Where(where, db);
			}
		   
			public TestCarCollection Where(WhereDelegate<TestCarColumns> where, OrderBy<TestCarColumns> orderBy = null, Database db = null)
			{
				return Bam.Generators.Tests.TestClasses.Dao.TestCar.Where(where, orderBy, db);
			}

			public TestCar OneWhere(WhereDelegate<TestCarColumns> where, Database db = null)
			{
				return Bam.Generators.Tests.TestClasses.Dao.TestCar.OneWhere(where, db);
			}

			public static TestCar GetOneWhere(WhereDelegate<TestCarColumns> where, Database db = null)
			{
				return Bam.Generators.Tests.TestClasses.Dao.TestCar.GetOneWhere(where, db);
			}
		
			public TestCar FirstOneWhere(WhereDelegate<TestCarColumns> where, Database db = null)
			{
				return Bam.Generators.Tests.TestClasses.Dao.TestCar.FirstOneWhere(where, db);
			}

			public TestCarCollection Top(int count, WhereDelegate<TestCarColumns> where, Database db = null)
			{
				return Bam.Generators.Tests.TestClasses.Dao.TestCar.Top(count, where, db);
			}

			public TestCarCollection Top(int count, WhereDelegate<TestCarColumns> where, OrderBy<TestCarColumns> orderBy, Database db = null)
			{
				return Bam.Generators.Tests.TestClasses.Dao.TestCar.Top(count, where, orderBy, db);
			}

			public long Count(WhereDelegate<TestCarColumns> where, Database db = null)
			{
				return Bam.Generators.Tests.TestClasses.Dao.TestCar.Count(where, db);
			}
	}

	static TestCarQueryContext _testCars;
	static object _testCarsLock = new object();
	public static TestCarQueryContext TestCars
	{
		get
		{
			return _testCarsLock.DoubleCheckLock<TestCarQueryContext>(ref _testCars, () => new TestCarQueryContext());
		}
	}
	public class TestAnimalQueryContext
	{
			public TestAnimalCollection Where(WhereDelegate<TestAnimalColumns> where, Database db = null)
			{
				return Bam.Generators.Tests.TestClasses.Dao.TestAnimal.Where(where, db);
			}
		   
			public TestAnimalCollection Where(WhereDelegate<TestAnimalColumns> where, OrderBy<TestAnimalColumns> orderBy = null, Database db = null)
			{
				return Bam.Generators.Tests.TestClasses.Dao.TestAnimal.Where(where, orderBy, db);
			}

			public TestAnimal OneWhere(WhereDelegate<TestAnimalColumns> where, Database db = null)
			{
				return Bam.Generators.Tests.TestClasses.Dao.TestAnimal.OneWhere(where, db);
			}

			public static TestAnimal GetOneWhere(WhereDelegate<TestAnimalColumns> where, Database db = null)
			{
				return Bam.Generators.Tests.TestClasses.Dao.TestAnimal.GetOneWhere(where, db);
			}
		
			public TestAnimal FirstOneWhere(WhereDelegate<TestAnimalColumns> where, Database db = null)
			{
				return Bam.Generators.Tests.TestClasses.Dao.TestAnimal.FirstOneWhere(where, db);
			}

			public TestAnimalCollection Top(int count, WhereDelegate<TestAnimalColumns> where, Database db = null)
			{
				return Bam.Generators.Tests.TestClasses.Dao.TestAnimal.Top(count, where, db);
			}

			public TestAnimalCollection Top(int count, WhereDelegate<TestAnimalColumns> where, OrderBy<TestAnimalColumns> orderBy, Database db = null)
			{
				return Bam.Generators.Tests.TestClasses.Dao.TestAnimal.Top(count, where, orderBy, db);
			}

			public long Count(WhereDelegate<TestAnimalColumns> where, Database db = null)
			{
				return Bam.Generators.Tests.TestClasses.Dao.TestAnimal.Count(where, db);
			}
	}

	static TestAnimalQueryContext _testAnimals;
	static object _testAnimalsLock = new object();
	public static TestAnimalQueryContext TestAnimals
	{
		get
		{
			return _testAnimalsLock.DoubleCheckLock<TestAnimalQueryContext>(ref _testAnimals, () => new TestAnimalQueryContext());
		}
	}
	public class TestPersonTestAnimalQueryContext
	{
			public TestPersonTestAnimalCollection Where(WhereDelegate<TestPersonTestAnimalColumns> where, Database db = null)
			{
				return Bam.Generators.Tests.TestClasses.Dao.TestPersonTestAnimal.Where(where, db);
			}
		   
			public TestPersonTestAnimalCollection Where(WhereDelegate<TestPersonTestAnimalColumns> where, OrderBy<TestPersonTestAnimalColumns> orderBy = null, Database db = null)
			{
				return Bam.Generators.Tests.TestClasses.Dao.TestPersonTestAnimal.Where(where, orderBy, db);
			}

			public TestPersonTestAnimal OneWhere(WhereDelegate<TestPersonTestAnimalColumns> where, Database db = null)
			{
				return Bam.Generators.Tests.TestClasses.Dao.TestPersonTestAnimal.OneWhere(where, db);
			}

			public static TestPersonTestAnimal GetOneWhere(WhereDelegate<TestPersonTestAnimalColumns> where, Database db = null)
			{
				return Bam.Generators.Tests.TestClasses.Dao.TestPersonTestAnimal.GetOneWhere(where, db);
			}
		
			public TestPersonTestAnimal FirstOneWhere(WhereDelegate<TestPersonTestAnimalColumns> where, Database db = null)
			{
				return Bam.Generators.Tests.TestClasses.Dao.TestPersonTestAnimal.FirstOneWhere(where, db);
			}

			public TestPersonTestAnimalCollection Top(int count, WhereDelegate<TestPersonTestAnimalColumns> where, Database db = null)
			{
				return Bam.Generators.Tests.TestClasses.Dao.TestPersonTestAnimal.Top(count, where, db);
			}

			public TestPersonTestAnimalCollection Top(int count, WhereDelegate<TestPersonTestAnimalColumns> where, OrderBy<TestPersonTestAnimalColumns> orderBy, Database db = null)
			{
				return Bam.Generators.Tests.TestClasses.Dao.TestPersonTestAnimal.Top(count, where, orderBy, db);
			}

			public long Count(WhereDelegate<TestPersonTestAnimalColumns> where, Database db = null)
			{
				return Bam.Generators.Tests.TestClasses.Dao.TestPersonTestAnimal.Count(where, db);
			}
	}

	static TestPersonTestAnimalQueryContext _testPersonTestAnimals;
	static object _testPersonTestAnimalsLock = new object();
	public static TestPersonTestAnimalQueryContext TestPersonTestAnimals
	{
		get
		{
			return _testPersonTestAnimalsLock.DoubleCheckLock<TestPersonTestAnimalQueryContext>(ref _testPersonTestAnimals, () => new TestPersonTestAnimalQueryContext());
		}
	}
    }
}																								
