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
	}

	[IntroduceInterface( typeof(IParameterizedSourceRelay), AncestorOverrideAction = InterfaceOverrideAction.Ignore )]
	public sealed class SourceRelayAspect : RelayAspectBase, IParameterizedSourceRelay
	{
		readonly IParameterizedSourceRelay relay;

		public SourceRelayAspect() {}

		public SourceRelayAspect( IParameterizedSourceRelay relay )
		{
			this.relay = relay;
		}

		public object Get( object parameter ) => relay.Get( parameter );
		object IInvocation.Invoke( object parameter ) => relay.Invoke( parameter );
	}
	
	[IntroduceInterface( typeof(ISpecificationRelay), AncestorOverrideAction = InterfaceOverrideAction.Ignore )]
	public sealed class SpecificationRelayAspect : RelayAspectBase, ISpecificationRelay
	{
		readonly ISpecificationRelay relay;

		public SpecificationRelayAspect() {}

		public SpecificationRelayAspect( ISpecificationRelay relay )
		{
			this.relay = relay;
		}

		public bool IsSatisfiedBy( object parameter ) => relay.IsSatisfiedBy( parameter );
		object IInvocation.Invoke( object parameter ) => relay.Invoke( parameter );
	}

	public sealed class InstanceAspects : AdapterLocatorBase<IAspect>
	{
		public static InstanceAspects Default { get; } = new InstanceAspects();
		InstanceAspects() : base( Descriptors.Default.Aspects ) {}	
	}

	public sealed class CommandValidationDescriptor : Descriptor<SpecificationRelayAspect, SpecificationMethodAspect>
	{
		public static CommandValidationDescriptor Default { get; } = new CommandValidationDescriptor();
		CommandValidationDescriptor() : base( CommandTypeDefinition.Default.Validation, GenericCommandTypeDefinition.Default.Validation, typeof(SpecificationRelay<>), typeof(ISpecificationRelay) ) {}
	}
	
	public sealed class CommandDescriptor : Descriptor<CommandRelayAspect, CommandMethodAspect>
	{
		public static CommandDescriptor Default { get; } = new CommandDescriptor();
		CommandDescriptor() : base( CommandTypeDefinition.Default.Execution, GenericCommandTypeDefinition.Default.Execution, typeof(CommandRelay<>), typeof(ICommandRelay) ) {}
	}

	public sealed class SourceDescriptor : Descriptor<SourceRelayAspect, ParameterizedSourceMethodAspect>
	{
		public static SourceDescriptor Default { get; } = new SourceDescriptor();
		SourceDescriptor() : base( GeneralizedParameterizedSourceTypeDefinition.Default.Method, ParameterizedSourceTypeDefinition.Default.Method, typeof(ParameterizedSourceRelay<,>), typeof(IParameterizedSourceRelay) ) {}
	}

	public sealed class SpecificationDescriptor : Descriptor<SpecificationRelayAspect, SpecificationMethodAspect>
	{
		public static SpecificationDescriptor Default { get; } = new SpecificationDescriptor();
		SpecificationDescriptor() : base( GeneralizedSpecificationTypeDefinition.Default.Method, GenericSpecificationTypeDefinition.Default.Method, typeof(SpecificationRelay<>), typeof(ISpecificationRelay) ) {}
	}

	public interface IDescriptor : IParameterizedSource<IAspect>, IParameterizedSource<Type, IEnumerable<AspectInstance>>
	{
		IMethodStore Source { get; }
	}

	public class Descriptor<TType, TMethod> : IDescriptor where TType : IAspect where TMethod : IAspect, new()
	{
		readonly Func<object, IInvocation> invocationSource;
		readonly Func<object, IAspect> aspectSource;
		readonly IAspectInstanceLocator typeLocator, methodLocator;

		public Descriptor( IMethodStore source, IMethodStore destination, Type adapterType, Type introducedInterface ) 
			: this( source, 
					new AdapterFactorySource<IInvocation>( destination.DeclaringType, adapterType ).Get, 
					ParameterConstructor<object, IAspect>.Make( introducedInterface, typeof(TType) ), 
					new TypeBasedAspectInstanceLocator<TType>( source ), 
					new MethodBasedAspectInstanceLocator<TMethod>( source ) ) {}

		Descriptor( IMethodStore source, Func<object, IInvocation> invocationSource, Func<object, IAspect> aspectSource, IAspectInstanceLocator typeLocator, IAspectInstanceLocator methodLocator ) 
		{
			Source = source;
			this.invocationSource = invocationSource;
			this.aspectSource = aspectSource;
			this.typeLocator = typeLocator;
			this.methodLocator = methodLocator;
		}

		public IMethodStore Source { get; }

		public IAspect Get( object source ) => aspectSource( invocationSource( source ) );
		public IEnumerable<AspectInstance> Get( Type parameter )
		{
			var method = methodLocator.Get( parameter );
			if ( method != null )
			{
				yield return typeLocator.Get( parameter );
				yield return method;
			}
		}
	}

	public sealed class Descriptors : ItemSource<IDescriptor>
	{
		public static Descriptors Default { get; } = new Descriptors();
		Descriptors() : this( CommandValidationDescriptor.Default, CommandDescriptor.Default, SourceDescriptor.Default, SpecificationDescriptor.Default ) {}

		Descriptors( params IDescriptor[] descriptors ) : this( descriptors, descriptors.Select( definition => definition.Source.DeclaringType.Adapt() ).ToArray() ) {}
		Descriptors( IDescriptor[] descriptors, TypeAdapter[] adapters ) 
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
		Support() : this( Descriptors.Default.ToArray() ) {}

		public Support( params IDescriptor[] descriptors ) 
			: this( SpecificationFactory.Default.Get( descriptors.Select( descriptor => descriptor.Source ).ToArray() ), descriptors.ToImmutableArray() ) {}

		readonly ImmutableArray<IDescriptor> descriptors;

		Support( Func<Type, bool> specification, ImmutableArray<IDescriptor> descriptors ) : base( specification )
		{
			this.descriptors = descriptors;
		}

		public IEnumerable<AspectInstance> Get( Type parameter )
		{
			foreach ( var descriptor in descriptors )
			{
				foreach ( var item in descriptor.Get( parameter ) )
				{
					if ( item != null )
					{
						yield return item;
					}
				}
			}
		}
	}

	[ProvideAspectRole( KnownRoles.InvocationWorkflow ), LinesOfCodeAvoided( 1 ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Validation )]
	public abstract class MethodAspectBase : AspectBase
	{
		readonly Func<object, IInvocation> invocationSource;

		protected MethodAspectBase( Func<object, IInvocation> invocationSource )
		{
			this.invocationSource = invocationSource;
		}

		public override void OnInvoke( MethodInterceptionArgs args )
		{
			var invocation = invocationSource( args.Instance );
			if ( invocation != null )
			{
				args.ReturnValue = invocation.Invoke( args.Arguments[0] );
			}
			else
			{
				args.Proceed();
			}
		}
	}

	public sealed class SpecificationMethodAspect : MethodAspectBase
	{
		readonly static Func<object, IInvocation> InvocationSource = InvocationLocator<ISpecificationRelay>.Default.Get;
		public SpecificationMethodAspect() : base( InvocationSource ) {}
	}

	public sealed class CommandMethodAspect : MethodAspectBase
	{
		readonly static Func<object, IInvocation> InvocationSource = InvocationLocator<ICommandRelay>.Default.Get;
		public CommandMethodAspect() : base( InvocationSource ) {}
	}

	public sealed class ParameterizedSourceMethodAspect : MethodAspectBase
	{
		readonly static Func<object, IInvocation> InvocationSource = InvocationLocator<IParameterizedSourceRelay>.Default.Get;
		public ParameterizedSourceMethodAspect() : base( InvocationSource ) {}
	}

	sealed class InvocationLocator<T> : IParameterizedSource<object, T> where T : class, IInvocation
	{
		public static InvocationLocator<T> Default { get; } = new InvocationLocator<T>();
		InvocationLocator() {}

		public T Get( object parameter ) => parameter as T;
	}

	public interface ICommandRelay : IInvocation {}
	public sealed class CommandRelay<T> : CommandInvocationBase<T>, ICommandRelay
	{
		readonly ICommand<T> command;

		public CommandRelay( ICommand<T> command )
		{
			this.command = command;
		}

		protected override void Execute( T parameter ) => command.Execute( parameter );
	}

	public interface ISpecificationRelay : ISpecification<object>, IInvocation {}
	public sealed class SpecificationRelay<T> : InvocationBase<T, bool>, ISpecificationRelay
	{
		readonly ISpecification<T> specification;
		public SpecificationRelay( ISpecification<T> specification )
		{
			this.specification = specification;
		}

		public override bool Invoke( T parameter ) => specification.IsSatisfiedBy( parameter );
		public bool IsSatisfiedBy( object parameter ) => parameter is T && specification.IsSatisfiedBy( (T)parameter );
	}

	public interface IParameterizedSourceRelay : IParameterizedSource<object, object>, IInvocation {}
	public sealed class ParameterizedSourceRelay<TParameter, TResult> : InvocationBase<TParameter, TResult>, IParameterizedSourceRelay
	{
		readonly IParameterizedSource<TParameter, TResult> source;

		public ParameterizedSourceRelay( IParameterizedSource<TParameter, TResult> source )
		{
			this.source = source;
		}

		public override TResult Invoke( TParameter parameter ) => source.Get( parameter );
		object IParameterizedSource<object, object>.Get( object parameter ) => source.Get( (TParameter)parameter );
	}
}
