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
    public class TestPersonQuery: Query<TestPersonColumns, TestPerson>
    { 
		public TestPersonQuery(){}
		public TestPersonQuery(WhereDelegate<TestPersonColumns> where, OrderBy<TestPersonColumns> orderBy = null, Database db = null) : base(where, orderBy, db) { }
		public TestPersonQuery(Func<TestPersonColumns, QueryFilter<TestPersonColumns>> where, OrderBy<TestPersonColumns> orderBy = null, Database db = null) : base(where, orderBy, db) { }		
		public TestPersonQuery(Delegate where, Database db = null) : base(where, db) { }
		
        public static TestPersonQuery Where(WhereDelegate<TestPersonColumns> where)
        {
            return Where(where, null, null);
        }

        public static TestPersonQuery Where(WhereDelegate<TestPersonColumns> where, OrderBy<TestPersonColumns> orderBy = null, Database db = null)
        {
            return new TestPersonQuery(where, orderBy, db);
        }

		public TestPersonCollection Execute()
		{
			return new TestPersonCollection(this, true);
		}
    }
}