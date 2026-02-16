using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using Bam.Data;

namespace Bam.Test.Tests.TestClasses.Dao
{
    public class TestDataCollection: DaoCollection<TestDataColumns, TestData>
    { 
		public TestDataCollection(){}
		public TestDataCollection(IDatabase db, DataTable table, IDao dao = null, string rc = null) : base(db, table, dao, rc) { }
		public TestDataCollection(DataTable table, IDao dao = null, string rc = null) : base(table, dao, rc) { }
		public TestDataCollection(IQuery<TestDataColumns, TestData> q, Bam.Data.Dao dao = null, string rc = null) : base(q, dao, rc) { }
		public TestDataCollection(IDatabase db, IQuery<TestDataColumns, TestData> q, bool load) : base(db, q, load) { }
		public TestDataCollection(IQuery<TestDataColumns, TestData> q, bool load) : base(q, load) { }
    }
}