using DragonSpark.Aspects.Build;
using DragonSpark.Extensions;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using PostSharp.Aspects;
using PostSharp.Aspects.Advices;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Dependencies;
using PostSharp.Aspects.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace DragonSpark.Aspects.Implementations
{
	[ProvideAspectRole( KnownRoles.Implementations ), LinesOfCodeAvoided( 1 ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.Before, KnownRoles.ValueConversion )]
	public sealed class EnsureGeneralizedImplementationsAttribute : ApplyAspectBase
	{
		public EnsureGeneralizedImplementationsAttribute() : base( Support.Default ) {}
	}

	[AttributeUsage( AttributeTargets.Class ), AspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	public abstract class GeneralizedAspectBase : InstanceLevelAspect {}

	[IntroduceInterface( typeof(IParameterizedSource<object, object>) )]
	public sealed class GeneralizedParameterizedSourceAspect : GeneralizedAspectBase, IParameterizedSource<object, object>
	{
		public object Get( object parameter ) => null;
	}
	
	[IntroduceInterface( typeof(ISpecification<object>) )]
	public sealed class GeneralizedSpecificationAspect : GeneralizedAspectBase, ISpecification<object>
	{
		public bool IsSatisfiedBy( object parameter ) => false;
	}

	public interface IDescriptor : ITypeAware, IAspectInstanceLocator {}

	public sealed class SpecificationDescriptor : Descriptor<GeneralizedSpecificationAspect>
	{
		public static SpecificationDescriptor Default { get; } = new SpecificationDescriptor();
		SpecificationDescriptor() : base( GenericSpecificationTypeDefinition.Default.DeclaringType, GeneralizedSpecificationTypeDefinition.Default.DeclaringType, CommandTypeDefinition.Default.DeclaringType ) {}
	}

	public sealed class ParameterizedSourceDescriptor : Descriptor<GeneralizedParameterizedSourceAspect>
	{
		public static ParameterizedSourceDescriptor Default { get; } = new ParameterizedSourceDescriptor();
		ParameterizedSourceDescriptor() : base( ParameterizedSourceTypeDefinition.Default.DeclaringType, GeneralizedParameterizedSourceTypeDefinition.Default.DeclaringType ) {}
	}

	public class Descriptor<T> : TypeBasedAspectInstanceLocator<T>, IDescriptor where T : IAspect
	{
		public Descriptor( Type declaringType, params Type[] implementedTypes ) : base( TypeAssignableSpecification.Defaults.Get( declaringType ).And( new AllSpecification<Type>( implementedTypes.Select( type => TypeAssignableSpecification.Defaults.Get( type ).Inverse() ).Fixed() ) ) )
		{
			DeclaringType = declaringType;
		}

		public Type DeclaringType { get; }
	}

	public sealed class Descriptors : ItemSource<IDescriptor>
	{
		public static Descriptors Default { get; } = new Descriptors();
		Descriptors() : base( ParameterizedSourceDescriptor.Default, SpecificationDescriptor.Default ) {}
	}

	sealed class Support : DelegatedSpecification<Type>, ISupportDefinition
	{
		public static Support Default { get; } = new Support();
		Support() : this( Descriptors.Default.ToArray() ) {}

		readonly ImmutableArray<IDescriptor> descriptors;

		public Support( params IDescriptor[] descriptors ) : base( SpecificationFactory.Default.Get( descriptors ) )
		{
			this.descriptors = descriptors.ToImmutableArray();
		}

		public IEnumerable<AspectInstance> Get( Type parameter )
		{
			foreach ( var descriptor in descriptors )
			{
				var instance = descriptor.Get( parameter );
				if ( instance != null )
				{
					yield return instance;
				}
			}
		}
	}
}
