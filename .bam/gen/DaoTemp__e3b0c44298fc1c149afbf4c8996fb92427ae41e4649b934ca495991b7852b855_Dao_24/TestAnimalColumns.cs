using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Bam;
using Bam.Data;

namespace Bam.Generators.Tests.TestClasses.Dao
{
    public class TestAnimalColumns: QueryFilter<TestAnimalColumns>, IFilterToken
    {
        public TestAnimalColumns() { }
        public TestAnimalColumns(string columnName, bool isForeignKey = false)
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
        
		public TestAnimalColumns KeyColumn => new TestAnimalColumns("Id");

        public TestAnimalColumns Id => new TestAnimalColumns("Id");
        public TestAnimalColumns Uuid => new TestAnimalColumns("Uuid");
        public TestAnimalColumns Cuid => new TestAnimalColumns("Cuid");
        public TestAnimalColumns Name => new TestAnimalColumns("Name");


		public Type DaoType => typeof(TestAnimal);

		public string Operator { get; set; }

        public override string ToString()
        {
            return base.ColumnName;
        }
	}
}