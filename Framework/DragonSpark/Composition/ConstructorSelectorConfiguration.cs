using DragonSpark.Activation.Location;
using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using DragonSpark.TypeSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Composition
{
	public sealed class ConstructorSelector : ParameterizedSourceBase<Type, ConstructorInfo>
	{
		readonly Func<ConstructorInfo, bool> specification;

		/*public static IParameterizedSource<IEnumerable<ConstructorInfo>, ConstructorInfo> Default { get; } = new ParameterizedScope<IEnumerable<ConstructorInfo>, ConstructorInfo>( Factory.Global( () => new ConstructorSelector().ToSourceDelegate() ) );
		ConstructorSelector() : this( IsValidConstructorSpecification.Default.Get() ) {}*/

		public ConstructorSelector( Func<ConstructorInfo, bool> specification )
		{
			this.specification = specification;
		}

		public override ConstructorInfo Get( Type parameter ) => 
			InstanceConstructors.Default.Get( parameter.GetTypeInfo() ).AsEnumerable().OrderByDescending( info => info.GetParameters().Length ).FirstOrDefault( specification );
	}

	sealed class IsValidConstructorSpecification : SpecificationBase<ConstructorInfo>
	{
		readonly Func<Type, bool> validate;

		/*public static ISource<Func<ConstructorInfo, bool>> Default { get; } = new Scope<Func<ConstructorInfo, bool>>( Factory.Global( () => new IsValidConstructorSpecification().ToCachedSpecification().ToSpecificationDelegate() ) );
		IsValidConstructorSpecification() : this( IsValidTypeSpecification.Default.Get() ) {}*/

		public IsValidConstructorSpecification( Func<Type, bool> validate )
		{
			this.validate = validate;
		}

		public override bool IsSatisfiedBy( ConstructorInfo parameter )
		{
			var types = parameter.GetParameterTypes();
			var result = !types.Any() || types.All( validate );
			return result;
		}
	}

	sealed class IsValidTypeSpecification : AnySpecification<Type>
	{
		// public static ISource<Func<Type, bool>> Default { get; } = new Scope<Func<Type, bool>>( Factory.Global( () => new IsValidTypeSpecification().ToCachedSpecification().ToSpecificationDelegate() ) );
		public IsValidTypeSpecification( ICollection<Type> types ) : base( new DelegatedSpecification<Type>( types.Contains ), DefaultServiceProvider.Default ) {}

		/*public override bool IsSatisfiedBy( Type parameter = null )
		{
			var result = base.IsSatisfiedBy( parameter );
			return result;
		}*/
	}
}