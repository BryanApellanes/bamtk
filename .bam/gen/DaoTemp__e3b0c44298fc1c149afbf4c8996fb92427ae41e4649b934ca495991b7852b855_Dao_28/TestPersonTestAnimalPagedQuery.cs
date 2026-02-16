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
    public class TestPersonTestAnimalPagedQuery: PagedQuery<TestPersonTestAnimalColumns, TestPersonTestAnimal>
    { 
		public TestPersonTestAnimalPagedQuery(TestPersonTestAnimalColumns orderByColumn,TestPersonTestAnimalQuery query, Database db = null) : base(orderByColumn, query, db) { }
    }
}