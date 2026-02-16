using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Bam;
using Bam.Data;

namespace Bam.Generators.Tests.TestClasses.Dao
{
    public class TestPersonColumns: QueryFilter<TestPersonColumns>, IFilterToken
    {
        public TestPersonColumns() { }
        public TestPersonColumns(string columnName, bool isForeignKey = false)
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
        
		public TestPersonColumns KeyColumn => new TestPersonColumns("Id");

        public TestPersonColumns Id => new TestPersonColumns("Id");
        public TestPersonColumns Uuid => new TestPersonColumns("Uuid");
        public TestPersonColumns Cuid => new TestPersonColumns("Cuid");
        public TestPersonColumns Name => new TestPersonColumns("Name");
        public TestPersonColumns IntProperty => new TestPersonColumns("IntProperty");
        public TestPersonColumns BooleanProperty => new TestPersonColumns("BooleanProperty");
        public TestPersonColumns ULongProperty => new TestPersonColumns("ULongProperty");
        public TestPersonColumns LongProperty => new TestPersonColumns("LongProperty");
        public TestPersonColumns DecimalProperty => new TestPersonColumns("DecimalProperty");
        public TestPersonColumns DateTimeProperty => new TestPersonColumns("DateTimeProperty");


		public Type DaoType => typeof(TestPerson);

		public string Operator { get; set; }

        public override string ToString()
        {
            return base.ColumnName;
        }
	}
}