using System;
using System.Linq;
using System.Reflection;
using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;

namespace DragonSpark.Aspects.Validation
{
	public sealed class ValidationMethodLocator : TransformerBase<MethodInfo>
	{
		const string MethodName = nameof(ISpecification.IsSatisfiedBy);

		readonly Func<MethodLocator.Parameter, MethodInfo> source;

		public static ValidationMethodLocator Default { get; } = new ValidationMethodLocator();
		ValidationMethodLocator() : this( Defaults.Locator ) {}

		ValidationMethodLocator( Func<MethodLocator.Parameter, MethodInfo> source )
		{
			this.source = source;
		}

		public override MethodInfo Get( MethodInfo parameter )
		{
			var candidates = typeof(ISpecification<>).MakeGenericType( parameter.GetParameterTypes().Single() ).Append( typeof(ISpecification) );
			foreach ( var candidate in candidates )
			{
				var result = source( new MethodLocator.Parameter( candidate, MethodName, parameter.DeclaringType ) );
				if ( result != null )
				{
					return result;
				}
			}
			return null;
		}
	}
}