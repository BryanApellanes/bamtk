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
    public class RelatedDataQuery: Query<RelatedDataColumns, RelatedData>
    { 
		public RelatedDataQuery(){}
		public RelatedDataQuery(WhereDelegate<RelatedDataColumns> where, OrderBy<RelatedDataColumns> orderBy = null, Database db = null) : base(where, orderBy, db) { }
		public RelatedDataQuery(Func<RelatedDataColumns, QueryFilter<RelatedDataColumns>> where, OrderBy<RelatedDataColumns> orderBy = null, Database db = null) : base(where, orderBy, db) { }		
		public RelatedDataQuery(Delegate where, Database db = null) : base(where, db) { }
		
        public static RelatedDataQuery Where(WhereDelegate<RelatedDataColumns> where)
        {
            return Where(where, null, null);
        }

        public static RelatedDataQuery Where(WhereDelegate<RelatedDataColumns> where, OrderBy<RelatedDataColumns> orderBy = null, Database db = null)
        {
            return new RelatedDataQuery(where, orderBy, db);
        }

		public RelatedDataCollection Execute()
		{
			return new RelatedDataCollection(this, true);
		}
    }
}