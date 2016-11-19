using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.TypeSystem;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Application
{
	public struct SystemParts
	{
		[UsedImplicitly]
		public SystemParts( IEnumerable<Assembly> assemblies ) : this( assemblies.Fixed() ) {}

		SystemParts( Assembly[] assemblies ) : this( assemblies, assemblies.SelectMany( TypesFactory.Default.GetEnumerable ) ) {}

		public SystemParts( IEnumerable<Type> types ) : this( types.Fixed() ) {}

		SystemParts( Type[] types ) : this( types.Assemblies(), types ) {}

		public SystemParts( IEnumerable<Assembly> assemblies, IEnumerable<Type> types ) : this( assemblies.Distinct().ToImmutableArray(), types.Distinct().ToImmutableArray() ) {}

		SystemParts( ImmutableArray<Assembly> assemblies, ImmutableArray<Type> types )
		{
			Assemblies = assemblies;
			Types = types;
		}

		public ImmutableArray<Assembly> Assemblies { get; }
		public ImmutableArray<Type> Types { get; }
	}
}