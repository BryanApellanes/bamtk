using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using Bam.Data;

namespace Bam.Generators.Tests.TestClasses.Dao
{
    public class TestPersonCollection: DaoCollection<TestPersonColumns, TestPerson>
    { 
		public TestPersonCollection(){}
		public TestPersonCollection(IDatabase db, DataTable table, IDao dao = null, string rc = null) : base(db, table, dao, rc) { }
		public TestPersonCollection(DataTable table, IDao dao = null, string rc = null) : base(table, dao, rc) { }
		public TestPersonCollection(IQuery<TestPersonColumns, TestPerson> q, Bam.Data.Dao dao = null, string rc = null) : base(q, dao, rc) { }
		public TestPersonCollection(IDatabase db, IQuery<TestPersonColumns, TestPerson> q, bool load) : base(db, q, load) { }
		public TestPersonCollection(IQuery<TestPersonColumns, TestPerson> q, bool load) : base(q, load) { }
    }
}