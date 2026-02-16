/*
	This file was generated and should not be modified directly
*/
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using Bam.Data;

namespace Bam.Generators.Tests.TestClasses.Dao
{
    public class TestAnimalQuery: Query<TestAnimalColumns, TestAnimal>
    { 
		public TestAnimalQuery(){}
		public TestAnimalQuery(WhereDelegate<TestAnimalColumns> where, OrderBy<TestAnimalColumns> orderBy = null, Database db = null) : base(where, orderBy, db) { }
		public TestAnimalQuery(Func<TestAnimalColumns, QueryFilter<TestAnimalColumns>> where, OrderBy<TestAnimalColumns> orderBy = null, Database db = null) : base(where, orderBy, db) { }		
		public TestAnimalQuery(Delegate where, Database db = null) : base(where, db) { }
		
        public static TestAnimalQuery Where(WhereDelegate<TestAnimalColumns> where)
        {
            return Where(where, null, null);
        }

        public static TestAnimalQuery Where(WhereDelegate<TestAnimalColumns> where, OrderBy<TestAnimalColumns> orderBy = null, Database db = null)
        {
            return new TestAnimalQuery(where, orderBy, db);
        }

		public TestAnimalCollection Execute()
		{
			return new TestAnimalCollection(this, true);
		}
    }
}