using DragonSpark.Activation.FactoryModel;
using DragonSpark.Aspects;
using DragonSpark.Extensions;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Reflection;
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

	public class ConventionTypesFactory : FactoryBase<Type[]>
	{
		readonly IgnorableTypesLocator ignorable;
		readonly TypeInfo[] types;

		public ConventionTypesFactory( [Required] IgnorableTypesLocator ignorable, [Required]TypeInfo[] types )
		{
			this.ignorable = ignorable;
			this.types = types;
		}

		[Freeze]
		protected override Type[] CreateItem()
		{
			var result = types
						.Where( info => !info.IsAbstract && ( !info.IsNested || info.IsNestedPublic ) )
						.AsTypes()
						.Except( ignorable.Create() )
						.Prioritize()
						.ToArray();
			return result;
		}
	}

	[Export, Shared]
	public class ConventionRegistrationProfileFactory : FactoryBase<ConventionTypeContainer>
	{
		readonly Func<Type[]> types;

		[ImportingConstructor]
		public ConventionRegistrationProfileFactory( [Required] ConventionTypesFactory factory ) : this( factory.Create ) {}

		public ConventionRegistrationProfileFactory( Func<Type[]> types )
		{
			this.types = types;
		}

		[Freeze]
		protected override ConventionTypeContainer CreateItem() => new ConventionTypeContainer( types() );
	}
}