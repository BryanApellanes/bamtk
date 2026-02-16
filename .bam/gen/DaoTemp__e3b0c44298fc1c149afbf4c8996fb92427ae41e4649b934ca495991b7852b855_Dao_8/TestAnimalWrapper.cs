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
	public class TestAnimalWrapper: Bam.Generators.Tests.TestClasses.TestAnimal, IHasUpdatedXrefCollectionProperties
	{
		public TestAnimalWrapper()
		{
			this.UpdatedXrefCollectionProperties = new Dictionary<string, PropertyInfo>();
		}

		public TestAnimalWrapper(DaoRepository repository) : this()
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




        // right xref

// Right Xref property: Left -> TestPerson ; Right -> TestAnimal
		List<Bam.Generators.Tests.TestClasses.TestPerson> _testPersons;
		public override List<Bam.Generators.Tests.TestClasses.TestPerson> Owners
		{
			get
			{
				if(_testPersons == null || _testPersons.Count == 0)
				{
					var xref = new XrefDaoCollection<Bam.Generators.Tests.TestClasses.Dao.TestPersonTestAnimal, Bam.Generators.Tests.TestClasses.Dao.TestPerson>(DaoRepository.GetDaoInstance(this), false);
					xref.Load(DaoRepository.Database);
					_testPersons = ((IEnumerable)xref).CopyAs<Bam.Generators.Tests.TestClasses.TestPerson>().ToList();
					SetUpdatedXrefCollectionProperty("TestPersons", this.GetType().GetProperty("Owners"));					
				}

				return _testPersons;
			}
			set
			{
				_testPersons = value;
				SetUpdatedXrefCollectionProperty("TestPersons", this.GetType().GetProperty("Owners"));
			}
		}

	}
	// -- generated
}																								
