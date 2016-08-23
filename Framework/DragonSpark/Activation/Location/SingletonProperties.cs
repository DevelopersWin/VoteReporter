using System;
using System.Reflection;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Activation.Location
{
	public class SingletonProperties : ParameterizedSourceBase<Type, PropertyInfo>
	{
		public static IParameterizedSource<Type, PropertyInfo> Default { get; } = new SingletonProperties().ToCache();
		SingletonProperties() : this( SingletonSpecification.Default ) {}

		readonly ISpecification<SingletonRequest> specification;

		public SingletonProperties( ISpecification<SingletonRequest> specification )
		{
			this.specification = specification;
		}

		public override PropertyInfo Get( Type parameter )
		{
			foreach ( var property in parameter.GetTypeInfo().DeclaredProperties.Fixed() )
			{
				if ( specification.IsSatisfiedBy( new SingletonRequest( parameter, property ) ) )
				{
					return property;
				}
			}
			return null;
		}
	}
}