using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using Bam.Data;

namespace Bam.Test.Tests.TestClasses.Dao
{
    public class RelatedDataCollection: DaoCollection<RelatedDataColumns, RelatedData>
    { 
		public RelatedDataCollection(){}
		public RelatedDataCollection(IDatabase db, DataTable table, IDao dao = null, string rc = null) : base(db, table, dao, rc) { }
		public RelatedDataCollection(DataTable table, IDao dao = null, string rc = null) : base(table, dao, rc) { }
		public RelatedDataCollection(IQuery<RelatedDataColumns, RelatedData> q, Bam.Data.Dao dao = null, string rc = null) : base(q, dao, rc) { }
		public RelatedDataCollection(IDatabase db, IQuery<RelatedDataColumns, RelatedData> q, bool load) : base(db, q, load) { }
		public RelatedDataCollection(IQuery<RelatedDataColumns, RelatedData> q, bool load) : base(q, load) { }
    }
}