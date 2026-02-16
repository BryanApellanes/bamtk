using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using Bam.Data;

namespace Bam.Generators.Tests.TestClasses.Dao
{
    public class TestPersonTestAnimalCollection: DaoCollection<TestPersonTestAnimalColumns, TestPersonTestAnimal>
    { 
		public TestPersonTestAnimalCollection(){}
		public TestPersonTestAnimalCollection(IDatabase db, DataTable table, IDao dao = null, string rc = null) : base(db, table, dao, rc) { }
		public TestPersonTestAnimalCollection(DataTable table, IDao dao = null, string rc = null) : base(table, dao, rc) { }
		public TestPersonTestAnimalCollection(IQuery<TestPersonTestAnimalColumns, TestPersonTestAnimal> q, Bam.Data.Dao dao = null, string rc = null) : base(q, dao, rc) { }
		public TestPersonTestAnimalCollection(IDatabase db, IQuery<TestPersonTestAnimalColumns, TestPersonTestAnimal> q, bool load) : base(db, q, load) { }
		public TestPersonTestAnimalCollection(IQuery<TestPersonTestAnimalColumns, TestPersonTestAnimal> q, bool load) : base(q, load) { }
    }
}