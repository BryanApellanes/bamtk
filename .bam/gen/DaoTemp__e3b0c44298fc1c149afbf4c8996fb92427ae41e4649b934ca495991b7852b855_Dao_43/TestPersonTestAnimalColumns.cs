using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Bam;
using Bam.Data;

namespace Bam.Generators.Tests.TestClasses.Dao
{
    public class TestPersonTestAnimalColumns: QueryFilter<TestPersonTestAnimalColumns>, IFilterToken
    {
        public TestPersonTestAnimalColumns() { }
        public TestPersonTestAnimalColumns(string columnName, bool isForeignKey = false)
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
        
		public TestPersonTestAnimalColumns KeyColumn => new TestPersonTestAnimalColumns("Id");

        public TestPersonTestAnimalColumns Id => new TestPersonTestAnimalColumns("Id");
        public TestPersonTestAnimalColumns Uuid => new TestPersonTestAnimalColumns("Uuid");

        public TestPersonTestAnimalColumns TestPersonId => new TestPersonTestAnimalColumns("TestPersonId", true);
        public TestPersonTestAnimalColumns TestAnimalId => new TestPersonTestAnimalColumns("TestAnimalId", true);

		public Type DaoType => typeof(TestPersonTestAnimal);

		public string Operator { get; set; }

        public override string ToString()
        {
            return base.ColumnName;
        }
	}
}