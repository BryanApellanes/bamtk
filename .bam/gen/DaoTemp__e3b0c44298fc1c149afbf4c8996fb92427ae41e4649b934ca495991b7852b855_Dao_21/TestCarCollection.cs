using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using Bam.Data;

namespace Bam.Generators.Tests.TestClasses.Dao
{
    public class TestCarCollection: DaoCollection<TestCarColumns, TestCar>
    { 
		public TestCarCollection(){}
		public TestCarCollection(IDatabase db, DataTable table, IDao dao = null, string rc = null) : base(db, table, dao, rc) { }
		public TestCarCollection(DataTable table, IDao dao = null, string rc = null) : base(table, dao, rc) { }
		public TestCarCollection(IQuery<TestCarColumns, TestCar> q, Bam.Data.Dao dao = null, string rc = null) : base(q, dao, rc) { }
		public TestCarCollection(IDatabase db, IQuery<TestCarColumns, TestCar> q, bool load) : base(db, q, load) { }
		public TestCarCollection(IQuery<TestCarColumns, TestCar> q, bool load) : base(q, load) { }
    }
}