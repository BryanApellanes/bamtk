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
    public class TestCarQuery: Query<TestCarColumns, TestCar>
    { 
		public TestCarQuery(){}
		public TestCarQuery(WhereDelegate<TestCarColumns> where, OrderBy<TestCarColumns> orderBy = null, Database db = null) : base(where, orderBy, db) { }
		public TestCarQuery(Func<TestCarColumns, QueryFilter<TestCarColumns>> where, OrderBy<TestCarColumns> orderBy = null, Database db = null) : base(where, orderBy, db) { }		
		public TestCarQuery(Delegate where, Database db = null) : base(where, db) { }
		
        public static TestCarQuery Where(WhereDelegate<TestCarColumns> where)
        {
            return Where(where, null, null);
        }

        public static TestCarQuery Where(WhereDelegate<TestCarColumns> where, OrderBy<TestCarColumns> orderBy = null, Database db = null)
        {
            return new TestCarQuery(where, orderBy, db);
        }

		public TestCarCollection Execute()
		{
			return new TestCarCollection(this, true);
		}
    }
}