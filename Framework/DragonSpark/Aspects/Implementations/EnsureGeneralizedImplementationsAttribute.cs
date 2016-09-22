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
	public abstract class GeneralizedAspectBase : InstanceLevelAspect
	{
/*
		readonly IDescriptor descriptor;

		protected GeneralizedAspectBase( IDescriptor descriptor )
		{
			this.descriptor = descriptor;
		}
*/

	}

	[IntroduceInterface( typeof(IParameterizedSource<object, object>) )]
	public sealed class GeneralizedParameterizedSourceAspect : GeneralizedAspectBase, IParameterizedSource<object, object>
	{
		/*public GeneralizedParameterizedSourceAspect() : base( ParameterizedSourceDescriptor.Default ) {}*/

		public object Get( object parameter ) => null;
	}
	
	[IntroduceInterface( typeof(ISpecification<object>) )]
	public sealed class GeneralizedSpecificationAspect : GeneralizedAspectBase, ISpecification<object>
	{
		/*public GeneralizedSpecificationAspect() : base( SpecificationDescriptor.Default ) {}*/

		public bool IsSatisfiedBy( object parameter ) => false;
	}

	/*
	[IntroduceInterface( typeof(ICommandRelay), AncestorOverrideAction = InterfaceOverrideAction.Ignore )]
	public sealed class CommandRelayAspect : RelayAspectBase, ICommandRelay
	{
		readonly ICommandRelay relay;

		public CommandRelayAspect() {}

		public CommandRelayAspect( ICommandRelay relay )
		{
			this.relay = relay;
		}

		object IInvocation.Invoke( object parameter ) => relay.Invoke( parameter );
	}*/

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

		/*protected override bool Validate( Type parameter )
		{
			var validate = base.Validate( parameter );
			var one = TypeAssignableSpecification.Defaults.Get( typeof(ICommand) ).Inverse();
			var two = TypeAssignableSpecification.Defaults.Get( GeneralizedSpecificationTypeDefinition.Default.DeclaringType ).Inverse();
			var temp = new AllSpecification<Type>( one, two ).IsSatisfiedBy( parameter );
			MessageSource.MessageSink.Write( new Message( MessageLocation.Unknown, SeverityType.Error, "6776", $"YO: {this} {parameter}: {temp} | {validate}", null, null, null ));
			return validate;
		}*/
	}

	public sealed class Descriptors : ItemSource<IDescriptor>
	{
		public static Descriptors Default { get; } = new Descriptors();
		Descriptors() : base( ParameterizedSourceDescriptor.Default, SpecificationDescriptor.Default ) {}

		/*Descriptors( params IDescriptor[] descriptors ) : this( descriptors, descriptors.Select( definition => definition.Source.DeclaringType.Adapt() ).ToArray() ) {}
		Descriptors( IDescriptor[] descriptors, TypeAdapter[] adapters )
			: this( descriptors, new TypedPairs<IAspect>( adapters.Tuple( descriptors.Select( descriptor => new Func<object, IAspect>( descriptor.Get ) ).ToArray() ) ) ) {}

		Descriptors( IEnumerable<Relay.IDescriptor> descriptors, ITypedPairs<IAspect> instances ) : base( descriptors )
		{
			Aspects = instances;
		}

		public ITypedPairs<IAspect> Aspects { get; }*/
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
