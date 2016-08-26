using DragonSpark.Extensions;
using DragonSpark.Specifications;
using System;
using System.Collections.Immutable;
using System.Reflection;

namespace DragonSpark.Activation.Location
{
	public class SpecifiedSingletonHostSpecification : SingletonSpecification
	{
		readonly Type host;
		public SpecifiedSingletonHostSpecification( Type host, params string[] candidates ) : base( candidates )
		{
			this.host = host;
		}

		public override bool IsSatisfiedBy( PropertyInfo parameter ) => parameter.DeclaringType == host && base.IsSatisfiedBy( parameter );
	}

	public class SingletonSpecification : SpecificationBase<PropertyInfo>
	{
		public static SingletonSpecification Default { get; } = new SingletonSpecification();
		SingletonSpecification() : this( "Instance", "Default" ) {}

		readonly ImmutableArray<string> candidates;

		public SingletonSpecification( params string[] candidates ) : this( candidates.ToImmutableArray() ) {}

		public SingletonSpecification( ImmutableArray<string> candidates )
		{
			this.candidates = candidates;
		}

		public override bool IsSatisfiedBy( PropertyInfo parameter )
		{
			var result = parameter.GetMethod.IsStatic && !parameter.GetMethod.ContainsGenericParameters 
				&& 
				( candidates.Contains( parameter.Name ) || parameter.Has<SingletonAttribute>() );
			return result;
		}
	}
}