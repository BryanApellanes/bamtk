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
    public class TestDataPagedQuery: PagedQuery<TestDataColumns, TestData>
    { 
		public TestDataPagedQuery(TestDataColumns orderByColumn,TestDataQuery query, Database db = null) : base(orderByColumn, query, db) { }
    }
}