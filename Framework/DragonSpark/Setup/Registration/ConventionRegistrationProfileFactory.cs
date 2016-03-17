using DragonSpark.Activation.FactoryModel;
using DragonSpark.Extensions;
using PostSharp.Patterns.Contracts;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Reflection;
using DragonSpark.Aspects;
using Type = System.Type;

namespace DragonSpark.Setup.Registration
{
	[Persistent]
	public class IgnorableTypesLocator : FactoryBase<Type[]>
	{
		readonly Assembly[] assemblies;

		public IgnorableTypesLocator( [Required]Assembly[] assemblies )
		{
			this.assemblies = assemblies;
		}

		[Freeze]
		protected override Type[] CreateItem() => assemblies.SelectMany( assembly => assembly.From<RegistrationAttribute, IEnumerable<Type>>( attribute => attribute.IgnoreForRegistration ) ).ToArray();
	}

	[Export]
	public class ConventionRegistrationProfileFactory : FactoryBase<ConventionTypeContainer>
	{
		readonly IgnorableTypesLocator ignorable;
		readonly TypeInfo[] types;

		[ImportingConstructor]
		public ConventionRegistrationProfileFactory( IgnorableTypesLocator ignorable, [Required]TypeInfo[] types )
		{
			this.ignorable = ignorable;
			this.types = types;
		}

		protected virtual Type[] DetermineCandidateTypes()
		{
			var result = types
						.Where( info => !info.IsAbstract && ( !info.IsNested || info.IsNestedPublic ) )
						.AsTypes()
						.Except( ignorable.Create() )
						.Prioritize()
						.ToArray();
			return result;
		}

		[Freeze]
		protected override ConventionTypeContainer CreateItem() => new ConventionTypeContainer( DetermineCandidateTypes() );
	}
}