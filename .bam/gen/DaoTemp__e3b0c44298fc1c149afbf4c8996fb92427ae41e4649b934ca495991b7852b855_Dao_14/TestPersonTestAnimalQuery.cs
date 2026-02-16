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
    public class TestPersonTestAnimalQuery: Query<TestPersonTestAnimalColumns, TestPersonTestAnimal>
    { 
		public TestPersonTestAnimalQuery(){}
		public TestPersonTestAnimalQuery(WhereDelegate<TestPersonTestAnimalColumns> where, OrderBy<TestPersonTestAnimalColumns> orderBy = null, Database db = null) : base(where, orderBy, db) { }
		public TestPersonTestAnimalQuery(Func<TestPersonTestAnimalColumns, QueryFilter<TestPersonTestAnimalColumns>> where, OrderBy<TestPersonTestAnimalColumns> orderBy = null, Database db = null) : base(where, orderBy, db) { }		
		public TestPersonTestAnimalQuery(Delegate where, Database db = null) : base(where, db) { }
		
        public static TestPersonTestAnimalQuery Where(WhereDelegate<TestPersonTestAnimalColumns> where)
        {
            return Where(where, null, null);
        }

        public static TestPersonTestAnimalQuery Where(WhereDelegate<TestPersonTestAnimalColumns> where, OrderBy<TestPersonTestAnimalColumns> orderBy = null, Database db = null)
        {
            return new TestPersonTestAnimalQuery(where, orderBy, db);
        }

		public TestPersonTestAnimalCollection Execute()
		{
			return new TestPersonTestAnimalCollection(this, true);
		}
    }
}