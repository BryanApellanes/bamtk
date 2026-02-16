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
using Bam.Generators.Tests.TestClasses;
using Bam.Generators.Tests.TestClasses.Dao;

namespace ApplicationDataTypes.Wrappers
{
	// generated
	[Serializable]
	public class TestCarWrapper: Bam.Generators.Tests.TestClasses.TestCar, IHasUpdatedXrefCollectionProperties
	{
		public TestCarWrapper()
		{
			this.UpdatedXrefCollectionProperties = new Dictionary<string, PropertyInfo>();
		}

		public TestCarWrapper(DaoRepository repository) : this()
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


        Bam.Generators.Tests.TestClasses.TestPerson _testPerson;
		public override Bam.Generators.Tests.TestClasses.TestPerson TestPerson
		{
			get
			{
				if (_testPerson == null)
				{
					_testPerson = (Bam.Generators.Tests.TestClasses.TestPerson)DaoRepository.GetParentPropertyOfChild(this, typeof(Bam.Generators.Tests.TestClasses.TestPerson));
				}
				return _testPerson;
			}
			set
			{
				_testPerson = value;
			}
		}


	}
	// -- generated
}																								
