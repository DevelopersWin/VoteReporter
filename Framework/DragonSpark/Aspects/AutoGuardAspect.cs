using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.Specifications;
using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Serialization;
using PostSharp.Extensibility;
using PostSharp.Patterns.Contracts;
using PostSharp.Reflection;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Aspects
{
	[AspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	[AttributeUsage( AttributeTargets.Method ), MulticastAttributeUsage( MulticastTargets.Method | MulticastTargets.InstanceConstructor, TargetMemberAttributes = MulticastAttributes.NonAbstract ), LinesOfCodeAvoided( 1 )]
	public sealed class AutoGuardAspect : MethodLevelAspect, IAspectProvider
	{
		readonly static IValidatedParameterizedSource<Type, Type>[] DefaultSources = { DefaultSource.Default, ImmutableArraySource.Default };
		
		readonly IValidatedParameterizedSource<Type, Type>[] sources;

		public AutoGuardAspect() : this( DefaultSources ) {}

		public AutoGuardAspect( params IValidatedParameterizedSource<Type, Type>[] sources )
		{
			this.sources = sources;
		}

		public override bool CompileTimeValidate( MethodBase method ) => ( !method.IsSpecialName || method is ConstructorInfo ) && method.GetParameters().Any();

		public IEnumerable<AspectInstance> ProvideAspects( object targetElement )
		{
			var methodBase = (MethodBase)targetElement;
			foreach ( var parameter in methodBase.GetParameters().Where( info => !info.IsOptional ) )
			{
				var parameterType = parameter.ParameterType;
				foreach ( var source in sources )
				{
					if ( source.IsSatisfiedBy( parameterType ) )
					{
						var type = source.Get( parameterType );
						yield return new AspectInstance( parameter, new ObjectConstruction( type ), null ) { RepresentAsStandalone = true };
						break;
					}
				}
			}
		}

		sealed class DefaultSource : ValidatedParameterizedSourceBase<Type, Type>
		{
			public static IValidatedParameterizedSource<Type, Type> Default { get; } = new DefaultSource();
			DefaultSource() : base( Specification.DefaultNested ) {}

			public override Type Get( Type parameter ) => parameter == typeof(string) ? typeof(RequiredAttribute) : typeof(NotNullAttribute);

			sealed class Specification : SpecificationBase<Type>
			{
				public static ISpecification<Type> DefaultNested { get; } = new DelegatedSpecification<Type>( new DecoratedSourceCache<Type, bool>( new Specification().IsSatisfiedBy ).Get );
				Specification() {}

				public override bool IsSatisfiedBy( Type parameter ) => !parameter.IsByRef && Nullable.GetUnderlyingType( parameter ) == null && !parameter.GetTypeInfo().IsValueType;
			}
		}

		sealed class ImmutableArraySource : ValidatedParameterizedSourceBase<Type, Type>
		{
			public static ImmutableArraySource Default { get; } = new ImmutableArraySource();
			ImmutableArraySource() : base( GenericTypeAssignableSpecification.Defaults.Get( typeof(ImmutableArray<>) ) ) {}

			public override Type Get( Type parameter ) => typeof(AssignedAttribute);

			/*sealed class Specification : GenericTypeAssignableSpecification
			{
				public static ISpecification<Type> DefaultNested { get; } = new DelegatedSpecification<Type>( new DecoratedSourceCache<Type, bool>( new Specification().IsSatisfiedBy ).Get );
				Specification() : base( typeof(ImmutableArray<>) ) {}
			}*/
		}
	}
}
