using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using DragonSpark.TypeSystem;
using PostSharp.Aspects;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Aspects.Validation
{
	public sealed class AspectInstances : ParameterizedSourceBase<Type, IEnumerable<AspectInstance>>
	{
		public static AspectInstances Default { get; } = new AspectInstances();
		AspectInstances() : this( Defaults.AspectProfiles ) {}

		readonly ImmutableArray<IAspectProfile> profiles;
		readonly Func<MethodInfo, MethodInfo> specificationSource;
		readonly Func<MethodInfo, AspectInstance> validatorSource;
		readonly Func<MethodInfo, AspectInstance> executionSource;

		public AspectInstances( ImmutableArray<IAspectProfile> profiles ) : this( profiles, ValidationMethodLocator.Default.Get, AspectInstanceFactory<AutoValidationValidationAspect>.Default.Get, AspectInstanceFactory<AutoValidationExecuteAspect>.Default.Get ) {}

		public AspectInstances( ImmutableArray<IAspectProfile> profiles, Func<MethodInfo, MethodInfo> specificationSource, Func<MethodInfo, AspectInstance> validatorSource, Func<MethodInfo, AspectInstance> executionSource )
		{
			this.profiles = profiles;
			this.specificationSource = specificationSource;
			this.validatorSource = validatorSource;
			this.executionSource = executionSource;
		}

		public override IEnumerable<AspectInstance> Get( Type parameter ) => Yield( parameter ).WhereAssigned();

		IEnumerable<AspectInstance> Yield( Type parameter )
		{
			foreach ( var profile in profiles )
			{
				var method = profile.Get( parameter );
				if ( method != null )
				{
					var validator = specificationSource( method );
					if ( validator != null )
					{
						yield return validatorSource( validator );
						yield return executionSource( method );
					}
				}
			}
		}
	}

	sealed class AutoValidationTypeSpecification : SpecificationWithContextBase<Type, ImmutableArray<TypeAdapter>>
	{
		public static AutoValidationTypeSpecification Default { get; } = new AutoValidationTypeSpecification();
		AutoValidationTypeSpecification() : this( Defaults.AspectProfiles.Select( profile => profile.Method.DeclaringType.Adapt() ).ToImmutableArray() ) {}

		public AutoValidationTypeSpecification( ImmutableArray<TypeAdapter> context ) : base( context ) {}

		public override bool IsSatisfiedBy( Type parameter )
		{
			if ( !Context.IsAssignableFrom( parameter ) )
			{
				throw new InvalidOperationException( $"{parameter} does not implement any of the types defined in {GetType()}, which are: {string.Join( ",", Context.Select( t => t.Type.FullName ) )}" );
			}
			return true;
		}
	}
}