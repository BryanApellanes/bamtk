using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Bam;
using Bam.Data;

namespace Bam.Generators.Tests.TestClasses.Dao
{
    public class TestCarColumns: QueryFilter<TestCarColumns>, IFilterToken
    {
        public TestCarColumns() { }
        public TestCarColumns(string columnName, bool isForeignKey = false)
            : base(columnName)
        { 
            _isForeignKey = isForeignKey;
        }
        
        public bool IsKey()
        {
            return (bool)ColumnName?.Equals(KeyColumn.ColumnName);
        }

        private bool? _isForeignKey;
        public bool IsForeignKey
        {
            get
            {
                if (_isForeignKey == null)
                {
                    PropertyInfo prop = DaoType
                        .GetProperties()
                        .FirstOrDefault(pi => ((MemberInfo) pi)
                            .HasCustomAttributeOfType<ForeignKeyAttribute>(out ForeignKeyAttribute foreignKeyAttribute)
                                && foreignKeyAttribute.Name.Equals(ColumnName));
                        _isForeignKey = prop != null;
                }

                return _isForeignKey.Value;
            }
            set => _isForeignKey = value;
        }
        
		public TestCarColumns KeyColumn => new TestCarColumns("Id");

        public TestCarColumns Id => new TestCarColumns("Id");
        public TestCarColumns Uuid => new TestCarColumns("Uuid");
        public TestCarColumns Cuid => new TestCarColumns("Cuid");
        public TestCarColumns Make => new TestCarColumns("Make");
        public TestCarColumns Model => new TestCarColumns("Model");

        public TestCarColumns TestPersonId => new TestCarColumns("TestPersonId", true);

		public Type DaoType => typeof(TestCar);

		public string Operator { get; set; }

        public override string ToString()
        {
            return base.ColumnName;
        }
	}
}