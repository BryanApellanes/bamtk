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
    public class TestCarPagedQuery: PagedQuery<TestCarColumns, TestCar>
    { 
		public TestCarPagedQuery(TestCarColumns orderByColumn,TestCarQuery query, Database db = null) : base(orderByColumn, query, db) { }
    }
}