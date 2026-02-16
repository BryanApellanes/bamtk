using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Bam;
using Bam.Data;

namespace Bam.Test.Tests.TestClasses.Dao
{
    public class TestDataColumns: QueryFilter<TestDataColumns>, IFilterToken
    {
        public TestDataColumns() { }
        public TestDataColumns(string columnName, bool isForeignKey = false)
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
        
		public TestDataColumns KeyColumn => new TestDataColumns("Id");

        public TestDataColumns Id => new TestDataColumns("Id");
        public TestDataColumns Uuid => new TestDataColumns("Uuid");
        public TestDataColumns Cuid => new TestDataColumns("Cuid");
        public TestDataColumns Name => new TestDataColumns("Name");
        public TestDataColumns IntProperty => new TestDataColumns("IntProperty");
        public TestDataColumns BooleanProperty => new TestDataColumns("BooleanProperty");
        public TestDataColumns ULongProperty => new TestDataColumns("ULongProperty");
        public TestDataColumns LongProperty => new TestDataColumns("LongProperty");
        public TestDataColumns DecimalProperty => new TestDataColumns("DecimalProperty");
        public TestDataColumns DateTimeProperty => new TestDataColumns("DateTimeProperty");


		public Type DaoType => typeof(TestData);

		public string Operator { get; set; }

        public override string ToString()
        {
            return base.ColumnName;
        }
	}
}