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
	public class RelatedDataWrapper: Bam.Test.Tests.TestClasses.RelatedData, IHasUpdatedXrefCollectionProperties
	{
		public RelatedDataWrapper()
		{
			this.UpdatedXrefCollectionProperties = new Dictionary<string, PropertyInfo>();
		}

		public RelatedDataWrapper(DaoRepository repository) : this()
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


        Bam.Test.Tests.TestClasses.TestData _testData;
		public override Bam.Test.Tests.TestClasses.TestData TestData
		{
			get
			{
				if (_testData == null)
				{
					_testData = (Bam.Test.Tests.TestClasses.TestData)DaoRepository.GetParentPropertyOfChild(this, typeof(Bam.Test.Tests.TestClasses.TestData));
				}
				return _testData;
			}
			set
			{
				_testData = value;
			}
		}


	}
	// -- generated
}																								
