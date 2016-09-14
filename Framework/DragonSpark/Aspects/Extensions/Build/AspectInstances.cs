using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using PostSharp.Aspects;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace DragonSpark.Aspects.Extensions.Build
{
	public sealed class AspectInstances : ParameterizedSourceBase<Type, IEnumerable<AspectInstance>>
	{
		public static AspectInstances Default { get; } = new AspectInstances();
		AspectInstances() : this( AutoValidation.DefaultProfiles.AsEnumerable() ) {}

		readonly ImmutableArray<IAspectSource> sources;
		/*readonly Func<MethodInfo, MethodInfo> specificationSource;
		readonly Func<MethodInfo, AspectInstance> validatorSource;
		readonly Func<MethodInfo, AspectInstance> executionSource;*/

		public AspectInstances( IEnumerable<IProfile> sources ) : 
			this( sources.Concat()/*, ValidationMethodLocator.Default.Get, AspectInstance<AutoValidationValidationAspect>.Default.Get, AspectInstance<AutoValidationExecuteAspect>.Default.Get*/ ) {}

		public AspectInstances( IEnumerable<IAspectSource> sources/*, Func<MethodInfo, MethodInfo> specificationSource, Func<MethodInfo, AspectInstance> validatorSource, Func<MethodInfo, AspectInstance> executionSource*/ )
		{
			this.sources = sources.ToImmutableArray();
			/*this.specificationSource = specificationSource;
			this.validatorSource = validatorSource;
			this.executionSource = executionSource;*/
		}

		public override IEnumerable<AspectInstance> Get( Type parameter ) => Yield( parameter ).WhereAssigned();

		IEnumerable<AspectInstance> Yield( Type parameter )
		{
			foreach ( var source in sources )
			{
				var instance = source.Get( parameter );
				if ( instance != null )
				{
					/*var validator = specificationSource( method );
					if ( validator != null )
					{
						yield return validatorSource( validator );
						yield return executionSource( method );
					}*/
					yield return instance;
				}
			}
		}
	}
}