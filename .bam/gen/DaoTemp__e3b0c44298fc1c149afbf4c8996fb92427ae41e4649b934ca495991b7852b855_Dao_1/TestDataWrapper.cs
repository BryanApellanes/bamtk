using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Data;
using System.Data.Common;
using System.Linq;
using Bam;
using Bam.Data;
using Bam.Data.Repositories;
using Newtonsoft.Json;
using Bam.Test.Tests.TestClasses;
using Bam.Test.Tests.TestClasses.Dao;

namespace ApplicationDataTypes.Wrappers
{
	// generated
	[Serializable]
	public class TestDataWrapper: Bam.Test.Tests.TestClasses.TestData, IHasUpdatedXrefCollectionProperties
	{
		public TestDataWrapper()
		{
			this.UpdatedXrefCollectionProperties = new Dictionary<string, PropertyInfo>();
		}

		public TestDataWrapper(DaoRepository repository) : this()
		{
			this.DaoRepository = repository;
		}

		[JsonIgnore]
		public DaoRepository DaoRepository { get; set; }

		[JsonIgnore]
		public Dictionary<string, PropertyInfo> UpdatedXrefCollectionProperties { get; set; }

		protected void SetUpdatedXrefCollectionProperty(string propertyName, PropertyInfo correspondingProperty)
		{
			if(UpdatedXrefCollectionProperties != null && !UpdatedXrefCollectionProperties.ContainsKey(propertyName))
			{
				UpdatedXrefCollectionProperties.Add(propertyName, correspondingProperty);				
			}
			else if(UpdatedXrefCollectionProperties != null)
			{
				UpdatedXrefCollectionProperties[propertyName] = correspondingProperty;				
			}
		}

        System.Collections.Generic.List<Bam.Test.Tests.TestClasses.RelatedData> _relatedData;
		public override System.Collections.Generic.List<Bam.Test.Tests.TestClasses.RelatedData> RelatedData
		{
			get
			{
				if (_relatedData == null)
				{
					_relatedData = DaoRepository.ForeignKeyCollectionLoader<Bam.Test.Tests.TestClasses.TestData, Bam.Test.Tests.TestClasses.RelatedData>(this).ToList();
				}
				return _relatedData;
			}
			set
			{
				_relatedData = value;
			}
		}



	}
	// -- generated
}																								
