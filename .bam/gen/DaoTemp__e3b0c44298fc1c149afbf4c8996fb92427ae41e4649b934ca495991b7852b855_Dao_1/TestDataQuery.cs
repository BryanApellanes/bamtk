/*
	This file was generated and should not be modified directly
*/
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using Bam.Data;

namespace Bam.Test.Tests.TestClasses.Dao
{
    public class TestDataQuery: Query<TestDataColumns, TestData>
    { 
		public TestDataQuery(){}
		public TestDataQuery(WhereDelegate<TestDataColumns> where, OrderBy<TestDataColumns> orderBy = null, Database db = null) : base(where, orderBy, db) { }
		public TestDataQuery(Func<TestDataColumns, QueryFilter<TestDataColumns>> where, OrderBy<TestDataColumns> orderBy = null, Database db = null) : base(where, orderBy, db) { }		
		public TestDataQuery(Delegate where, Database db = null) : base(where, db) { }
		
        public static TestDataQuery Where(WhereDelegate<TestDataColumns> where)
        {
            return Where(where, null, null);
        }

        public static TestDataQuery Where(WhereDelegate<TestDataColumns> where, OrderBy<TestDataColumns> orderBy = null, Database db = null)
        {
            return new TestDataQuery(where, orderBy, db);
        }

		public TestDataCollection Execute()
		{
			return new TestDataCollection(this, true);
		}
    }
}