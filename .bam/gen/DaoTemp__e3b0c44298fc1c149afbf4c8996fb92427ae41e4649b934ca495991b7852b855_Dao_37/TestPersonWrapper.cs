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
	public class TestPersonWrapper: Bam.Generators.Tests.TestClasses.TestPerson, IHasUpdatedXrefCollectionProperties
	{
		public TestPersonWrapper()
		{
			this.UpdatedXrefCollectionProperties = new Dictionary<string, PropertyInfo>();
		}

		public TestPersonWrapper(DaoRepository repository) : this()
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

        System.Collections.Generic.List<Bam.Generators.Tests.TestClasses.TestCar> _testCars;
		public override System.Collections.Generic.List<Bam.Generators.Tests.TestClasses.TestCar> TestCars
		{
			get
			{
				if (_testCars == null)
				{
					_testCars = DaoRepository.ForeignKeyCollectionLoader<Bam.Generators.Tests.TestClasses.TestPerson, Bam.Generators.Tests.TestClasses.TestCar>(this).ToList();
				}
				return _testCars;
			}
			set
			{
				_testCars = value;
			}
		}

        // left xref

// Left Xref property: Left -> TestPerson ; Right -> TestAnimal
		List<Bam.Generators.Tests.TestClasses.TestAnimal> _testAnimals;
		public override List<Bam.Generators.Tests.TestClasses.TestAnimal> Pets
		{
			get
			{
				if(_testAnimals == null || _testAnimals.Count == 0)
				{
					var xref = new XrefDaoCollection<Bam.Generators.Tests.TestClasses.Dao.TestPersonTestAnimal, Bam.Generators.Tests.TestClasses.Dao.TestAnimal>(DaoRepository.GetDaoInstance(this), false);
					xref.Load(DaoRepository.Database);
					_testAnimals = ((IEnumerable)xref).CopyAs<Bam.Generators.Tests.TestClasses.TestAnimal>().ToList();
					SetUpdatedXrefCollectionProperty("TestAnimals", this.GetType().GetProperty("Pets"));					
				}

				return _testAnimals;
			}
			set
			{
				_testAnimals = value;
				SetUpdatedXrefCollectionProperty("TestAnimals", this.GetType().GetProperty("Pets"));
			}
		}


	}
	// -- generated
}																								
