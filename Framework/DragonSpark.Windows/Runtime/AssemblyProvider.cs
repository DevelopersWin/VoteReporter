using DragonSpark.Application;
using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.TypeSystem;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Windows.Runtime
{
	public sealed class FileSystemTypes : ApplicationTypesBase
	{
		public static FileSystemTypes Default { get; } = new FileSystemTypes();
		FileSystemTypes() : base( FileSystemAssemblySource.Default.Get ) {}
	}

	public abstract class ApplicationTypesBase : TypeSource
	{
		readonly Func<ImmutableArray<Assembly>> assemblySource;
		readonly Transform<IEnumerable<Assembly>> filter;
		readonly Func<IEnumerable<Assembly>, IEnumerable<Type>> partsSource;

		protected ApplicationTypesBase( Func<ImmutableArray<Assembly>> assemblySource ) : this( assemblySource, ApplicationAssemblyFilter.Default.Get, PublicParts.Default.Get ) {}

		protected ApplicationTypesBase( Func<ImmutableArray<Assembly>> assemblySource, Transform<IEnumerable<Assembly>> filter, Func<IEnumerable<Assembly>, IEnumerable<Type>> partsSource )
		{
			this.assemblySource = assemblySource;
			this.filter = filter;
			this.partsSource = partsSource;
		}

		protected override IEnumerable<Type> Yield()
		{
			var filtered = filter( assemblySource().AsEnumerable() ).Fixed();
			var result = new AssemblyBasedTypeSource( filtered ).Union( partsSource( filtered ) );
			return result;
		}
	}

	public sealed class PublicParts : PartTypesBase
	{
		public static PublicParts Default { get; } = new PublicParts();
		PublicParts() : base( DragonSpark.TypeSystem.PublicParts.Default.Get ) {}
	}

	public sealed class AllParts : PartTypesBase
	{
		public static AllParts Default { get; } = new AllParts();
		AllParts() : base( DragonSpark.TypeSystem.AllParts.Default.Get ) {}
	}

	public abstract class PartTypesBase : ParameterizedSourceBase<IEnumerable<Assembly>, IEnumerable<Type>>
	{
		readonly Func<Assembly, ImmutableArray<Type>> typeSource;
		readonly Func<IEnumerable<Assembly>, Assembly> assemblySource;

		protected PartTypesBase( Func<Assembly, ImmutableArray<Type>> typeSource ) : this( typeSource, ApplicationAssemblyLocator.Default.Get ) {}

		protected PartTypesBase( Func<Assembly, ImmutableArray<Type>> typeSource, Func<IEnumerable<Assembly>, Assembly> assemblySource )
		{
			this.typeSource = typeSource;
			this.assemblySource = assemblySource;
		}

		public override IEnumerable<Type> Get( IEnumerable<Assembly> parameter ) => typeSource( assemblySource( parameter ) ).AsEnumerable();
	}
}