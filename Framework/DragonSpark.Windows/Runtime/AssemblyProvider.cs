using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Setup;
using DragonSpark.TypeSystem;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;

namespace DragonSpark.Windows.Runtime
{
	/*public sealed class Assemblies : ExecutionScope<ITypeSource>
	{
		public static Assemblies Instance { get; } = new Assemblies();
		Assemblies() : base( ApplicationTypesBase.Instance.Get() ) {}
	}*/

	public sealed class FileSystemTypes : ApplicationTypesBase
	{
		public static FileSystemTypes Instance { get; } = new FileSystemTypes();
		FileSystemTypes() : base( FileSystemAssemblySource.Instance.Get ) {}
	}

	public abstract class ApplicationTypesBase : TypeSource
	{
		readonly Func<ImmutableArray<Assembly>> assemblySource;
		readonly Transform<IEnumerable<Assembly>> filter;
		readonly Func<IEnumerable<Assembly>, IEnumerable<Type>> partsSource;

		protected ApplicationTypesBase( Func<ImmutableArray<Assembly>> assemblySource ) : this( assemblySource, ApplicationAssemblyFilter.Instance.Get, PublicParts.Instance.Create ) {}

		protected ApplicationTypesBase( Func<ImmutableArray<Assembly>> assemblySource, Transform<IEnumerable<Assembly>> filter, Func<IEnumerable<Assembly>, IEnumerable<Type>> partsSource )
		{
			this.assemblySource = assemblySource;
			this.filter = filter;
			this.partsSource = partsSource;
		}

		protected override IEnumerable<Type> Yield()
		{
			var filtered = filter( assemblySource().AsEnumerable() ).Fixed();
			var result = new AssemblyBasedTypeSource( filtered ).Get().Union( partsSource( filtered ) );
			return result;
		}
	}

	public sealed class PublicParts : PartTypesBase
	{
		public static PublicParts Instance { get; } = new PublicParts();
		PublicParts() : base( DragonSpark.TypeSystem.PublicParts.Instance.Get ) {}
	}

	public sealed class AllParts : PartTypesBase
	{
		public static AllParts Instance { get; } = new AllParts();
		AllParts() : base( DragonSpark.TypeSystem.AllParts.Instance.Get ) {}
	}

	public abstract class PartTypesBase : FactoryBase<IEnumerable<Assembly>, IEnumerable<Type>>
	{
		readonly Func<Assembly, ImmutableArray<Type>> typeSource;
		readonly Func<IEnumerable<Assembly>, Assembly> assemblySource;

		protected PartTypesBase( Func<Assembly, ImmutableArray<Type>> typeSource ) : this( typeSource, ApplicationAssemblyLocator.Instance.Get ) {}

		protected PartTypesBase( Func<Assembly, ImmutableArray<Type>> typeSource, Func<IEnumerable<Assembly>, Assembly> assemblySource )
		{
			this.typeSource = typeSource;
			this.assemblySource = assemblySource;
		}

		public override IEnumerable<Type> Create( IEnumerable<Assembly> parameter ) => typeSource( assemblySource( parameter ) ).AsEnumerable();
	}
}