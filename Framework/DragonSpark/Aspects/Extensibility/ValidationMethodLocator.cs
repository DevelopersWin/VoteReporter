using DragonSpark.Aspects.Extensibility.Validation;
using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using System;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Aspects.Extensions.Build
{
	public sealed class ValidationMethodLocator : AlterationBase<MethodInfo>
	{
		const string MethodName = nameof(ISpecification<object>.IsSatisfiedBy);

		readonly Func<MethodLocator.Parameter, MethodInfo> source;

		public static ValidationMethodLocator Default { get; } = new ValidationMethodLocator();
		ValidationMethodLocator() : this( Extensibility.Validation.Defaults.Locator ) {}

		ValidationMethodLocator( Func<MethodLocator.Parameter, MethodInfo> source )
		{
			this.source = source;
		}

		public override MethodInfo Get( MethodInfo parameter )
		{
			var candidates = typeof(ISpecification<>).MakeGenericType( parameter.GetParameterTypes().Single() ).Append( typeof(ISpecification<object>) );
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