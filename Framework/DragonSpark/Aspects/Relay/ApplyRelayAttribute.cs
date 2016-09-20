using DragonSpark.Aspects.Build;
using DragonSpark.Commands;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using DragonSpark.TypeSystem;
using PostSharp.Aspects;
using PostSharp.Aspects.Advices;
using PostSharp.Aspects.Dependencies;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace DragonSpark.Aspects.Relay
{
	[ProvideAspectRole( KnownRoles.InvocationWorkflow ), LinesOfCodeAvoided( 1 ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Validation )]
	public class ApplyRelayAttribute : ApplyAspectBase, IInvocation
	{
		readonly IInvocation invocation;
		
		public ApplyRelayAttribute() : base( Support.Default ) {}

		public ApplyRelayAttribute( IInvocation invocation ) : this()
		{
			this.invocation = invocation;
		}

		public override object CreateInstance( AdviceArgs adviceArgs ) => Adapters.Default.Get( adviceArgs.Instance );

		object IInvocation.Invoke( object parameter ) => invocation.Invoke( parameter );
	}

	[IntroduceInterface( typeof(ICommandRelay) )]
	public sealed class ApplyCommandRelayAttribute : ApplyRelayAttribute, ICommandRelay
	{
		readonly ICommandRelay relay;
		
		public ApplyCommandRelayAttribute( ICommandRelay relay ) : base( relay )
		{
			this.relay = relay;
		}

		public bool IsSatisfiedBy( object parameter ) => relay.IsSatisfiedBy( parameter );
	}

	[IntroduceInterface( typeof(IParameterizedSourceRelay) )]
	public sealed class ApplySourceRelayAttribute : ApplyRelayAttribute, IParameterizedSourceRelay
	{
		readonly IParameterizedSourceRelay relay;
		
		public ApplySourceRelayAttribute( IParameterizedSourceRelay relay ) : base( relay )
		{
			this.relay = relay;
		}

		public object Get( object parameter ) => relay.Get( parameter );
	}
	
	[IntroduceInterface( typeof(ISpecificationRelay) )]
	public sealed class ApplySpecificationRelayAttribute : ApplyRelayAttribute, ISpecificationRelay
	{
		readonly ISpecification<object> specification;
		public ApplySpecificationRelayAttribute( ISpecificationRelay specification ) : base( specification )
		{
			this.specification = specification;
		}

		public bool IsSatisfiedBy( object parameter ) => specification.IsSatisfiedBy( parameter );
	}

	/*public sealed class Invocations : AdapterLocatorBase<IInvocation>
	{
		public static Invocations Default { get; } = new Invocations();
		Invocations() : base( Descriptors.Default.Invocations ) {}	
	}*/
	public sealed class Adapters : AdapterLocatorBase<IAspect>
	{
		public static Adapters Default { get; } = new Adapters();
		Adapters() : base( Descriptors.Default.Aspects ) {}	
	}

	public sealed class CommandDescriptor : Descriptor
	{
		public static CommandDescriptor Default { get; } = new CommandDescriptor();
		CommandDescriptor() : base( CommandDefinition.Default, GenericCommandDefinition.Default, typeof(CommandRelay<>), typeof(ICommandRelay), typeof(ApplyCommandRelayAttribute) ) {}
	}

	public sealed class SpecificationDescriptor : Descriptor
	{
		public static SpecificationDescriptor Default { get; } = new SpecificationDescriptor();
		SpecificationDescriptor() : base( GeneralizedSpecificationDefinition.Default, GenericSpecificationDefinition.Default, typeof(SpecificationRelay<>), typeof(ISpecificationRelay), typeof(ApplySpecificationRelayAttribute) ) {}
	}

	public sealed class SourceDescriptor : Descriptor
	{
		public static SourceDescriptor Default { get; } = new SourceDescriptor();
		SourceDescriptor() : base( GeneralizedParameterizedSourceDefinition.Default, ParameterizedSourceDefinition.Default, typeof(ParameterizedSourceRelay<,>), typeof(IParameterizedSourceRelay), typeof(ApplySourceRelayAttribute) ) {}
	}

	public class Descriptor
	{
		readonly Func<object, IInvocation> invocationSource;
		readonly Func<object, IAspect> aspectSource;

		public Descriptor( IDefinition source, IDefinition destination, Type adapterType, Type interfaceType, Type attributeType ) 
			: this( source, source.DeclaringType.Adapt(), new AdapterFactorySource<IInvocation>( destination.DeclaringType, adapterType ).Get, new AdapterFactorySource<IAspect>( interfaceType, attributeType ).Get ) {}

		Descriptor( IDefinition definition, TypeAdapter adapter, Func<object, IInvocation> invocationSource, Func<object, IAspect> aspectSource )
		{
			this.invocationSource = invocationSource;
			this.aspectSource = aspectSource;
			Definition = definition;
			Adapter = adapter;
		}

		public IDefinition Definition { get; }
		public TypeAdapter Adapter { get; }

		public IInvocation GetInvocation( object source ) => invocationSource( source );

		public IAspect GetAspect( object source ) => aspectSource( source );
	}

	public sealed class Descriptors
	{
		public static Descriptors Default { get; } = new Descriptors();
		Descriptors() : this( CommandDescriptor.Default, SourceDescriptor.Default, SpecificationDescriptor.Default ) {}

		public Descriptors( params Descriptor[] descriptors ) : this( descriptors, descriptors.Select( descriptor => descriptor.Definition ).ToImmutableArray(), descriptors.Select( definition => definition.Adapter ).ToArray() ) {}
		Descriptors( IEnumerable<Descriptor> descriptors, ImmutableArray<IDefinition> definitions, IEnumerable<TypeAdapter> adapters ) 
			: this( definitions, adapters.Tuple( descriptors.Select( descriptor => new Func<object, IAspect>( descriptor.GetAspect ) ).ToArray() ).Hide() ) {}
		Descriptors( ImmutableArray<IDefinition> definitions, IEnumerable<ValueTuple<TypeAdapter, Func<object, IAspect>>> aspects )
		{
			Definitions = definitions;
			Aspects = aspects;
		}

		public ImmutableArray<IDefinition> Definitions { get; }
		public IEnumerable<ValueTuple<TypeAdapter, Func<object, IAspect>>> Aspects { get; }
	}

	sealed class Support : SupportDefinition<Aspect>
	{
		public static Support Default { get; } = new Support();
		Support() : base( Descriptors.Default.Definitions.ToArray() ) {}
	}

	[IntroduceInterface( typeof(IInvocation) )]
	[ProvideAspectRole( KnownRoles.InvocationWorkflow ), LinesOfCodeAvoided( 1 ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Validation )]
	public sealed class Aspect : AspectBase
	{
		public override void OnInvoke( MethodInterceptionArgs args )
		{
			var invocation = args.Instance as IInvocation;
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

	public interface ICommandRelay : IInvocation, ISpecification<object> {}
	public sealed class CommandRelay<T> : CommandInvocationBase<T>, ICommandRelay
	{
		readonly ICommand<T> command;

		public CommandRelay( ICommand<T> command )
		{
			this.command = command;
		}

		protected override void Execute( T parameter ) => command.Execute( parameter );
		public bool IsSatisfiedBy( object parameter ) => command.IsSatisfiedBy( (T)parameter );
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
		public bool IsSatisfiedBy( object parameter ) => specification.IsSatisfiedBy( (T)parameter );
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
