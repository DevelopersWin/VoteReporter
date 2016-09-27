using DragonSpark.Activation;
using DragonSpark.Aspects.Build;
using DragonSpark.Commands;
using DragonSpark.Extensions;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using DragonSpark.TypeSystem;
using PostSharp.Aspects;
using PostSharp.Aspects.Advices;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Dependencies;
using PostSharp.Aspects.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace DragonSpark.Aspects.Relay
{
	[ProvideAspectRole( KnownRoles.InvocationWorkflow ), LinesOfCodeAvoided( 1 ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Validation )]
	public sealed class ApplyRelayAttribute : ApplyAspectBase
	{
		public ApplyRelayAttribute() : base( Support.Default ) {}
	}

	[AttributeUsage( AttributeTargets.Class ), AspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	public abstract class RelayAspectBase : InstanceLevelAspect
	{
		public override object CreateInstance( AdviceArgs adviceArgs ) => InstanceAspects.Default.Get( adviceArgs.Instance );
	}

	[IntroduceInterface( typeof(ICommandRelay) )]
	public sealed class CommandRelayAspect : SpecificationRelayAspectBase, ICommandRelay
	{
		readonly ICommandRelay relay;

		public CommandRelayAspect() {}

		public CommandRelayAspect( ICommandRelay relay ) : base( relay )
		{
			this.relay = relay;
		}

		public void Execute( object parameter ) => relay.Execute( parameter );
	}

	[IntroduceInterface( typeof(IParameterizedSourceRelay) )]
	public sealed class SourceRelayAspect : RelayAspectBase, IParameterizedSourceRelay
	{
		readonly IParameterizedSourceRelay relay;

		public SourceRelayAspect() {}

		public SourceRelayAspect( IParameterizedSourceRelay relay )
		{
			this.relay = relay;
		}

		public object Get( object parameter ) => relay.Get( parameter );
	}
	
	[IntroduceInterface( typeof(ISpecificationRelay) )]
	public sealed class SpecificationRelayAspect : SpecificationRelayAspectBase
	{
		public SpecificationRelayAspect() {}
		public SpecificationRelayAspect( ISpecificationRelay relay ) : base( relay ) {}
	}

	public abstract class SpecificationRelayAspectBase : RelayAspectBase, ISpecificationRelay
	{
		readonly ISpecificationRelay relay;

		protected SpecificationRelayAspectBase() {}

		protected SpecificationRelayAspectBase( ISpecificationRelay relay )
		{
			this.relay = relay;
		}

		public bool IsSatisfiedBy( object parameter ) => relay.IsSatisfiedBy( parameter );
	}

	public sealed class InstanceAspects : AdapterLocatorBase<IAspect>
	{
		public static InstanceAspects Default { get; } = new InstanceAspects();
		InstanceAspects() : base( Descriptors.Default.Aspects ) {}	
	}

	/*public sealed class CommandValidationDescriptor : Descriptor<SpecificationRelayAspect, >
	{
		public static CommandValidationDescriptor Default { get; } = new CommandValidationDescriptor();
		CommandValidationDescriptor() : base( CommandTypeDefinition.Default.Validation, GenericCommandTypeDefinition.Default.Validation, typeof(SpecificationRelay<>), typeof(ISpecificationRelay) ) {}
	}*/
	
	public sealed class CommandDescriptor : Descriptor<CommandRelayAspect>
	{
		public static CommandDescriptor Default { get; } = new CommandDescriptor();
		CommandDescriptor() : base( CommandTypeDefinition.Default, GenericCommandTypeDefinition.Default, typeof(CommandRelay<>), typeof(ICommandRelay),
			new MethodBasedAspectInstanceLocator<SpecificationMethodAspect>( CommandTypeDefinition.Default.Validation ),
			new MethodBasedAspectInstanceLocator<CommandMethodAspect>( CommandTypeDefinition.Default.Execution )
			) {}
	}

	public sealed class SourceDescriptor : Descriptor<SourceRelayAspect>
	{
		public static SourceDescriptor Default { get; } = new SourceDescriptor();
		SourceDescriptor() : base( GeneralizedParameterizedSourceTypeDefinition.Default, ParameterizedSourceTypeDefinition.Default, typeof(ParameterizedSourceRelay<,>), typeof(IParameterizedSourceRelay),
			new MethodBasedAspectInstanceLocator<ParameterizedSourceMethodAspect>( GeneralizedParameterizedSourceTypeDefinition.Default.Method )
			) {}
	}

	public sealed class SpecificationDescriptor : Descriptor<SpecificationRelayAspect>
	{
		public static SpecificationDescriptor Default { get; } = new SpecificationDescriptor();
		SpecificationDescriptor() : base( GeneralizedSpecificationTypeDefinition.Default, GenericSpecificationTypeDefinition.Default, typeof(SpecificationRelay<>), typeof(ISpecificationRelay),
			new MethodBasedAspectInstanceLocator<SpecificationMethodAspect>( GeneralizedSpecificationTypeDefinition.Default.Method )
			) {}
	}

	public interface IDescriptor : ITypeAware, IParameterizedSource<IAspect>, IParameterizedSource<Type, IEnumerable<AspectInstance>> {}

	public class Descriptor<T> : TypeBasedAspectInstanceLocator<T>, IDescriptor where T : IAspect
	{
		readonly Func<object, object> adapterSource;
		readonly Func<object, IAspect> aspectSource;
		readonly ImmutableArray<IAspectInstanceLocator> locators;
		
		public Descriptor( ITypeAware source, ITypeAware destination, Type adapterType, Type introducedInterface, params IAspectInstanceLocator[] locators ) 
			: this( source,
					new AdapterFactorySource( destination.DeclaringType, adapterType ).Get, 
					ParameterConstructor<object, IAspect>.Make( introducedInterface, typeof(T) ), 
					locators.ToImmutableArray()
				  ) {}

		Descriptor( ITypeAware source, Func<object, object> adapterSource, Func<object, IAspect> aspectSource, ImmutableArray<IAspectInstanceLocator> locators ) : base( source )
		{
			DeclaringType = source.DeclaringType;
			this.adapterSource = adapterSource;
			this.aspectSource = aspectSource;
			this.locators = locators;
		}

		public Type DeclaringType { get; }

		public IAspect Get( object source ) => aspectSource( adapterSource( source ) );

		IEnumerable<AspectInstance> IParameterizedSource<Type, IEnumerable<AspectInstance>>.Get( Type parameter )
		{
			var methods = GetMappings( parameter ).ToArray();
			var result = methods.Length == locators.Length ? base.Get( parameter ).Append( methods ).Fixed() : Items<AspectInstance>.Default;
			return result;
		}

		IEnumerable<AspectInstance> GetMappings( Type parameter )
		{
			foreach ( var locator in locators )
			{
				var instance = locator.Get( parameter );
				if ( instance != null )
				{
					yield return instance;
				}
			}
		}
	}

	public sealed class Descriptors : ItemSource<IDescriptor>
	{
		public static Descriptors Default { get; } = new Descriptors();
		Descriptors() : this( CommandDescriptor.Default, SourceDescriptor.Default, SpecificationDescriptor.Default ) {}

		Descriptors( params IDescriptor[] descriptors ) : this( descriptors.Select( definition => definition.DeclaringType.Adapt() ).ToArray(), descriptors ) {}
		Descriptors( TypeAdapter[] adapters, IDescriptor[] descriptors ) 
			: this( descriptors, new TypedPairs<IAspect>( adapters.Tuple( descriptors.Select( descriptor => new Func<object, IAspect>( descriptor.Get ) ).ToArray() ) ) ) {}

		Descriptors( IEnumerable<IDescriptor> descriptors, ITypedPairs<IAspect> instances ) : base( descriptors )
		{
			Aspects = instances;
		}

		public ITypedPairs<IAspect> Aspects { get; }
	}
	
	sealed class Support : DelegatedSpecification<Type>, ISupportDefinition
	{
		public static Support Default { get; } = new Support();
		Support() : this( Descriptors.Default.ToImmutableArray() ) {}

		readonly ImmutableArray<IDescriptor> descriptors;

		Support( ImmutableArray<IDescriptor> descriptors ) : base( SpecificationFactory.Default.Get( descriptors.ToArray() ) )
		{
			this.descriptors = descriptors;
		}

		public IEnumerable<AspectInstance> Get( Type parameter )
		{
			foreach ( var descriptor in descriptors )
			{
				var instances = descriptor.Get( parameter ).Fixed();
				if ( instances.Any() )
				{
					return instances;
				}
			}
			return Items<AspectInstance>.Default;
		}
	}

	[ProvideAspectRole( KnownRoles.InvocationWorkflow ), LinesOfCodeAvoided( 1 ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Validation )]
	public abstract class MethodAspectBase : AspectBase {}

	public sealed class SpecificationMethodAspect : MethodAspectBase
	{
		public override void OnInvoke( MethodInterceptionArgs args )
		{
			var invocation = args.Instance as ISpecificationRelay;
			if ( invocation != null )
			{
				args.ReturnValue = invocation.IsSatisfiedBy( args.Arguments[0] );
			}
			else
			{
				args.Proceed();
			}
		}
	}

	public sealed class CommandMethodAspect : MethodAspectBase
	{
		public override void OnInvoke( MethodInterceptionArgs args )
		{
			var invocation = args.Instance as ICommandRelay;
			if ( invocation != null )
			{
				invocation.Execute( args.Arguments[0] );
			}
			else
			{
				args.Proceed();
			}
		}
	}

	public sealed class ParameterizedSourceMethodAspect : MethodAspectBase
	{
		public override void OnInvoke( MethodInterceptionArgs args )
		{
			var invocation = args.Instance as IParameterizedSourceRelay;
			if ( invocation != null )
			{
				args.ReturnValue = invocation.Get( args.Arguments[0] );
			}
			else
			{
				args.Proceed();
			}
		}
	}

	/*sealed class InvocationLocator<T> : IParameterizedSource<object, T> where T : class, IInvocation
	{
		public static InvocationLocator<T> Default { get; } = new InvocationLocator<T>();
		InvocationLocator() {}

		public T Get( object parameter ) => parameter as T;
	}*/

	public interface ICommandRelay : ISpecificationRelay
	{
		void Execute( object parameter );
	}
	public sealed class CommandRelay<T> : SpecificationRelay<T>, ICommandRelay
	{
		readonly ICommand<T> command;

		public CommandRelay( ICommand<T> command ) : base( command )
		{
			this.command = command;
		}

		public void Execute( object parameter ) => command.Execute( (T)parameter );
	}

	public interface ISpecificationRelay
	{
		bool IsSatisfiedBy( object parameter );
	}
	public class SpecificationRelay<T> : ISpecificationRelay
	{
		readonly ISpecification<T> specification;
		public SpecificationRelay( ISpecification<T> specification )
		{
			this.specification = specification;
		}

		public bool IsSatisfiedBy( object parameter ) => parameter is T && specification.IsSatisfiedBy( (T)parameter );
	}

	public interface IParameterizedSourceRelay
	{
		object Get( object parameter );
	}
	public sealed class ParameterizedSourceRelay<TParameter, TResult> : IParameterizedSourceRelay
	{
		readonly IParameterizedSource<TParameter, TResult> source;

		public ParameterizedSourceRelay( IParameterizedSource<TParameter, TResult> source )
		{
			this.source = source;
		}

		public object Get( object parameter ) => source.Get( (TParameter)parameter );
	}
}
