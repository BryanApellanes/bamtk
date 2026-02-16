using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using Bam.Data;

namespace Bam.Generators.Tests.TestClasses.Dao
{
    public class TestAnimalCollection: DaoCollection<TestAnimalColumns, TestAnimal>
    { 
		public TestAnimalCollection(){}
		public TestAnimalCollection(IDatabase db, DataTable table, IDao dao = null, string rc = null) : base(db, table, dao, rc) { }
		public TestAnimalCollection(DataTable table, IDao dao = null, string rc = null) : base(table, dao, rc) { }
		public TestAnimalCollection(IQuery<TestAnimalColumns, TestAnimal> q, Bam.Data.Dao dao = null, string rc = null) : base(q, dao, rc) { }
		public TestAnimalCollection(IDatabase db, IQuery<TestAnimalColumns, TestAnimal> q, bool load) : base(db, q, load) { }
		public TestAnimalCollection(IQuery<TestAnimalColumns, TestAnimal> q, bool load) : base(q, load) { }
    }
}