using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.TypeSystem;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;

namespace DragonSpark.Application
{
	public struct SystemParts
	{
		[UsedImplicitly]
		public SystemParts( IEnumerable<Assembly> assemblies ) : this( assemblies.ToImmutableArray() ) {}

		SystemParts( ImmutableArray<Assembly> assemblies ) : this( assemblies, assemblies.SelectMany( TypesFactory.Default.GetEnumerable ).ToImmutableArray() ) {}

		public SystemParts( IEnumerable<Type> types ) : this( types.Fixed() ) {}

		SystemParts( Type[] types ) : this( types.Assemblies().ToImmutableArray(), types.ToImmutableArray() ) {}

		SystemParts( ImmutableArray<Assembly> assemblies, ImmutableArray<Type> types )
		{
			Assemblies = assemblies;
			Types = types;
		}

		public ImmutableArray<Assembly> Assemblies { get; }
		public ImmutableArray<Type> Types { get; }
	}
}