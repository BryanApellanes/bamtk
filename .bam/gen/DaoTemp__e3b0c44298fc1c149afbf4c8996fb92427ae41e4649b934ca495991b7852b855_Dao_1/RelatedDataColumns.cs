using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Bam;
using Bam.Data;

namespace Bam.Test.Tests.TestClasses.Dao
{
    public class RelatedDataColumns: QueryFilter<RelatedDataColumns>, IFilterToken
    {
        public RelatedDataColumns() { }
        public RelatedDataColumns(string columnName, bool isForeignKey = false)
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
        
		public RelatedDataColumns KeyColumn => new RelatedDataColumns("Id");

        public RelatedDataColumns Id => new RelatedDataColumns("Id");
        public RelatedDataColumns Uuid => new RelatedDataColumns("Uuid");
        public RelatedDataColumns Cuid => new RelatedDataColumns("Cuid");
        public RelatedDataColumns Name => new RelatedDataColumns("Name");
        public RelatedDataColumns IntProperty => new RelatedDataColumns("IntProperty");
        public RelatedDataColumns BooleanProperty => new RelatedDataColumns("BooleanProperty");
        public RelatedDataColumns ULongProperty => new RelatedDataColumns("ULongProperty");
        public RelatedDataColumns LongProperty => new RelatedDataColumns("LongProperty");
        public RelatedDataColumns DecimalProperty => new RelatedDataColumns("DecimalProperty");
        public RelatedDataColumns DateTimeProperty => new RelatedDataColumns("DateTimeProperty");

        public RelatedDataColumns TestDataId => new RelatedDataColumns("TestDataId", true);

		public Type DaoType => typeof(RelatedData);

		public string Operator { get; set; }

        public override string ToString()
        {
            return base.ColumnName;
        }
	}
}