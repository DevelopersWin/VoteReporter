using DragonSpark.Activation;
using DragonSpark.Extensions;
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
	public class IgnorableTypesLocator : FixedFactory<ImmutableArray<Type>>
	{
		public IgnorableTypesLocator( Assembly[] assemblies ) : 
			base( assemblies.ToArray().SelectMany( assembly => assembly.From<RegistrationAttribute, IEnumerable<Type>>( attribute => attribute.IgnoreForRegistration ) ).ToImmutableArray() ) {}
	}

	public class ConventionTypesFactory : FixedFactory<ImmutableArray<Type>>
	{
		public ConventionTypesFactory( IgnorableTypesLocator ignorable, TypeInfo[] types ) : base( Create( ignorable, types ) ) {}

		static ImmutableArray<Type> Create( IgnorableTypesLocator ignorable, TypeInfo[] types )
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
	public class ConventionRegistrationProfileFactory : FixedFactory<ConventionTypeContainer>
	{
		[ImportingConstructor]
		public ConventionRegistrationProfileFactory( [Required] ConventionTypesFactory factory ) : base( new ConventionTypeContainer( factory.Create() ) ) {}
	}
}