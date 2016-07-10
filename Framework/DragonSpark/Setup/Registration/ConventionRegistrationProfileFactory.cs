using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Stores;
using PostSharp.Patterns.Contracts;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Reflection;
using Type = System.Type;

namespace DragonSpark.Setup.Registration
{
	[Persistent]
	public class IgnorableTypesLocator : StoreBase<ImmutableArray<Type>>
	{
		readonly ImmutableArray<Assembly> assemblies;
		
		public IgnorableTypesLocator( IEnumerable<Assembly> assemblies )
		{
			this.assemblies = assemblies.ToImmutableArray();
		}

		protected override ImmutableArray<Type> Get() => assemblies.ToArray().SelectMany( assembly => assembly.From<RegistrationAttribute, IEnumerable<Type>>( attribute => attribute.IgnoreForRegistration ) ).ToImmutableArray();
	}

	public class ConventionTypesFactory : StoreBase<ImmutableArray<Type>>
	{
		readonly IgnorableTypesLocator ignorable;
		readonly ImmutableArray<TypeInfo> types;

		public ConventionTypesFactory( IgnorableTypesLocator ignorable, TypeInfo[] types )
		{
			this.ignorable = ignorable;
			this.types = types.ToImmutableArray();
		}

		protected override ImmutableArray<Type> Get()
		{
			var result = types
						.Where( info => !info.IsAbstract && ( !info.IsNested || info.IsNestedPublic ) )
						.AsTypes()
						.Except( ignorable.Value.ToArray() )
						.Prioritize()
						.ToImmutableArray();
			return result;
		}
	}

	[Export, Shared]
	public class ConventionRegistrationProfileFactory : FactoryBase<ConventionTypeContainer>
	{
		readonly ConventionTypeContainer container;

		[ImportingConstructor]
		public ConventionRegistrationProfileFactory( [Required] ConventionTypesFactory factory ) : this( new ConventionTypeContainer( factory.Value ) ) {}

		ConventionRegistrationProfileFactory( ConventionTypeContainer container )
		{
			this.container = container;
		}

		public override ConventionTypeContainer Create() => container;
	}
}