using DragonSpark.Extensions;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Composition
{
	public sealed class ConstructorSelector : ParameterizedSourceBase<IEnumerable<ConstructorInfo>, ConstructorInfo>
	{
		readonly Func<ConstructorInfo, bool> specification;

		public static IParameterizedSource<IEnumerable<ConstructorInfo>, ConstructorInfo> Default { get; } = new ParameterizedScope<IEnumerable<ConstructorInfo>, ConstructorInfo>( Factory.Global( () => new ConstructorSelector().ToSourceDelegate() ) );
		ConstructorSelector() : this( IsValidConstructorSpecification.Default.Get() ) {}

		ConstructorSelector( Func<ConstructorInfo, bool> specification )
		{
			this.specification = specification;
		}

		public override ConstructorInfo Get( IEnumerable<ConstructorInfo> parameter ) => parameter.OrderByDescending( info => info.GetParameters().Length ).FirstOrDefault( specification );
	}

	sealed class IsValidConstructorSpecification : SpecificationBase<ConstructorInfo>
	{
		readonly Func<Type, bool> validate;

		public static ISource<Func<ConstructorInfo, bool>> Default { get; } = new Scope<Func<ConstructorInfo, bool>>( Factory.Global( () => new IsValidConstructorSpecification().ToCachedSpecification().ToSpecificationDelegate() ) );
		IsValidConstructorSpecification() : this( IsValidTypeSpecification.Default.Get() ) {}

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

	sealed class IsValidTypeSpecification : DelegatedSpecification<Type>
	{
		public static ISource<Func<Type, bool>> Default { get; } = new Scope<Func<Type, bool>>( Factory.Global( () => new IsValidTypeSpecification().ToCachedSpecification().ToSpecificationDelegate() ) );
		IsValidTypeSpecification() : base( ExportsProfileFactory.Default.Get().All.Contains ) {}

		public override bool IsSatisfiedBy( Type parameter )
		{
			var contains = base.IsSatisfiedBy( parameter );
			// var canActivate = Defaults.ActivateSpecification.IsSatisfiedBy( parameter );
			// var constructor = ConstructorSelector.Default.Get().Invoke( InstanceConstructors.Default.Get( parameter.GetTypeInfo() ).AsEnumerable() );
			var result = contains /*|| canActivate*/;
			return result;
		}
	}
}