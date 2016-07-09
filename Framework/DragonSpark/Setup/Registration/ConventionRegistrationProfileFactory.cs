using DragonSpark.Activation;
using DragonSpark.Extensions;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Reflection;
using Type = System.Type;

namespace DragonSpark.Setup.Registration
{
	[Persistent]
	public class IgnorableTypesLocator : CachedFactoryBase<ImmutableArray<Type>>
	{
		readonly ImmutableArray<Assembly> assemblies;
		

		public IgnorableTypesLocator( Assembly[] assemblies )
		{
			this.assemblies = assemblies.ToImmutableArray();
		}

		protected override ImmutableArray<Type> Cache() => assemblies.ToArray().SelectMany( assembly => assembly.From<RegistrationAttribute, IEnumerable<Type>>( attribute => attribute.IgnoreForRegistration ) ).ToImmutableArray();
		
	}

	public class ConventionTypesFactory : CachedFactoryBase<ImmutableArray<Type>>
	{
		readonly IgnorableTypesLocator ignorable;
		readonly ImmutableArray<TypeInfo> types;

		public ConventionTypesFactory( IgnorableTypesLocator ignorable, TypeInfo[] types )
		{
			this.ignorable = ignorable;
			this.types = types.ToImmutableArray();
		}

		protected override ImmutableArray<Type> Cache()
		{
			var result = types
						.Where( info => !info.IsAbstract && ( !info.IsNested || info.IsNestedPublic ) )
						.AsTypes()
						.Except( ignorable.Create().ToArray() )
						.Prioritize()
						.ToImmutableArray();
			return result;
		}
	}

	[Export, Shared]
	public class ConventionRegistrationProfileFactory : CachedFactoryBase<ConventionTypeContainer>
	{
		readonly Func<ImmutableArray<Type>> types;

		[ImportingConstructor]
		public ConventionRegistrationProfileFactory( [Required] ConventionTypesFactory factory ) : this( factory.Create ) {}

		public ConventionRegistrationProfileFactory( Func<ImmutableArray<Type>> types )
		{
			this.types = types;
		}

		protected override ConventionTypeContainer Cache() => new ConventionTypeContainer( types() );
	}
}