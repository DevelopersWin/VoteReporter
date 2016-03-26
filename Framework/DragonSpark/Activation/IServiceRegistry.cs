using DragonSpark.Activation.IoC;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Specifications;
using Microsoft.Practices.Unity;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Generic;

namespace DragonSpark.Activation
{
	public interface IServiceRegistry
	{
		bool IsRegistered( Type type );

		void Register( MappingRegistrationParameter parameter );

		void Register( InstanceRegistrationParameter parameter );

		void RegisterFactory( FactoryRegistrationParameter parameter );
	}

	public class IsATypeSpecification : DecoratedSpecification<Type>
	{
		public static IsATypeSpecification Instance { get; } = new IsATypeSpecification();

		public IsATypeSpecification() : base( AlwaysSpecification.Instance ) { }
	}

	public class OnlyIfNotRegistered : DecoratedSpecification<Type>
	{
		public OnlyIfNotRegistered( IUnityContainer container ) : base( new IsRegisteredSpecification( container ).Inverse(), type => new LocateTypeRequest( type ) ) { }
	}

	public class RegisterInstanceByConventionCommand : RegisterInstanceByConventionCommand<IsATypeSpecification>
	{
		public RegisterInstanceByConventionCommand( IServiceRegistry registry, ImplementedInterfaceFromConventionLocator locator ) : base( registry, locator, IsATypeSpecification.Instance ) {}
	}

	public class RegisterInstanceByConventionCommand<T> : RegisterInstanceCommand<T> where T : ISpecification<Type>
	{
		readonly ImplementedInterfaceFromConventionLocator locator;

		public RegisterInstanceByConventionCommand( IServiceRegistry registry, [Required]ImplementedInterfaceFromConventionLocator locator, T specification ) : base( registry, specification )
		{
			this.locator = locator;
		}

		protected override void OnExecute( InstanceRegistrationParameter parameter ) => locator.Create( parameter.Instance.GetType() ).With( type =>
		{
			base.OnExecute( new InstanceRegistrationParameter( type, parameter.Instance, parameter.Name ) );
		} );
	}

	public class RegisterEntireHierarchyCommand : RegisterEntireHierarchyCommand<IsATypeSpecification>
	{
		public RegisterEntireHierarchyCommand( IServiceRegistry registry ) : base( registry, IsATypeSpecification.Instance ) {}
	}

	public class RegisterEntireHierarchyCommand<T> : RegisterHierarchyCommandBase<T> where T : ISpecification<Type>
	{
		public RegisterEntireHierarchyCommand( IServiceRegistry registry, T specification ) : base( registry, specification, parameter => parameter.Instance.Adapt().GetEntireHierarchy() ) {}
	}

	public class RegisterHierarchyCommand : RegisterHierarchyCommand<IsATypeSpecification>
	{
		public RegisterHierarchyCommand( IServiceRegistry registry ) : base( registry, IsATypeSpecification.Instance ) {}
	}

	public class RegisterHierarchyCommand<T> : RegisterHierarchyCommandBase<T> where T : ISpecification<Type>
	{
		public RegisterHierarchyCommand( IServiceRegistry registry, T specification ) : base( registry, specification, parameter => parameter.Instance.Adapt().GetHierarchy( false ) ) {}
	}

	public abstract class RegisterHierarchyCommandBase<T> : RegisterInstanceCommand<T> where T : ISpecification<Type>
	{
		readonly Func<InstanceRegistrationParameter, IEnumerable<Type>> typeResolver;

		protected RegisterHierarchyCommandBase( IServiceRegistry registry, T specification, [Required]Func<InstanceRegistrationParameter,IEnumerable<Type>> typeResolver ) : base( registry, specification )
		{
			this.typeResolver = typeResolver;
		}

		protected override void OnExecute( InstanceRegistrationParameter parameter ) => typeResolver( parameter ).Each( type =>
		{
			base.OnExecute( new InstanceRegistrationParameter( type, parameter.Instance, parameter.Name ) );
		} );
	}

	public abstract class RegistrationCommandBase<T, U> : Command<T> where T : RegistrationParameter where U : ISpecification<Type>
	{
		readonly Action<T> command;
		readonly U specification;

		protected RegistrationCommandBase( [Required]Action<T> command, [Required]U specification )
		{
			this.command = command;
			this.specification = specification;
		}

		public override bool CanExecute( T parameter ) => base.CanExecute( parameter ) && specification.IsSatisfiedBy( parameter.RequestedType );

		protected override void OnExecute( T parameter ) => command( parameter );
	}

	public class RegisterCommand : RegisterCommand<IsATypeSpecification>
	{
		public RegisterCommand( IServiceRegistry registry, IsATypeSpecification specification ) : base( registry, specification ) {}
	}

	public class RegisterCommand<T> : RegistrationCommandBase<MappingRegistrationParameter, T> where T : ISpecification<Type>
	{
		public RegisterCommand( [Required]IServiceRegistry registry, T specification ) : base( registry.Register, specification ) {}
	}

	public class RegisterInstanceCommand : RegisterInstanceCommand<IsATypeSpecification>
	{
		public RegisterInstanceCommand( IServiceRegistry registry, IsATypeSpecification specification ) : base( registry, specification ) {}
	}

	public class RegisterInstanceCommand<T> : RegistrationCommandBase<InstanceRegistrationParameter, T> where T : ISpecification<Type>
	{
		public RegisterInstanceCommand( [Required]IServiceRegistry registry, T specification ) : base( registry.Register, specification ) { }
	}

	public class RegisterFactoryCommand : RegisterFactoryCommand<IsATypeSpecification>
	{
		public RegisterFactoryCommand( IServiceRegistry registry, IsATypeSpecification specification ) : base( registry, specification ) {}
	}

	public class RegisterFactoryCommand<T> : RegistrationCommandBase<FactoryRegistrationParameter, T> where T : ISpecification<Type>
	{
		public RegisterFactoryCommand( [Required]IServiceRegistry registry, T specification ) : base( registry.RegisterFactory, specification ) { }
	}
	
	public abstract class RegistrationParameter : LocateTypeRequest
	{
		protected RegistrationParameter( Type type, string name = null ) : base( type, name ) {}
	}

	public class MappingRegistrationParameter : RegistrationParameter
	{
		public MappingRegistrationParameter( Type type, string name = null ) : this( type, type, name ) {}

		public MappingRegistrationParameter( Type type, [Required]Type mappedTo, string name = null ) : base( type, name )
		{
			MappedTo = mappedTo;
		}

		public Type MappedTo { get; }
	}

	public class InstanceRegistrationParameter<T> : InstanceRegistrationParameter
	{
		public InstanceRegistrationParameter( T instance, string name = null ) : base( typeof(T), instance, name ) {}
	}

	public class InstanceRegistrationParameter : RegistrationParameter
	{
		public InstanceRegistrationParameter( [Required]object instance, string name = null ) : this( instance.GetType(), instance, name )
		{}

		public InstanceRegistrationParameter( Type type, [Required]object instance, string name = null ) : base( type, name )
		{
			Instance = instance;
		}

		public object Instance { get; }
	}

	public class FactoryRegistrationParameter : RegistrationParameter
	{
		public FactoryRegistrationParameter( Type type, [Required]Func<object> factory, string name = null ) : base( type, name )
		{
			Factory = factory;
		}

		public Func<object> Factory { get; }
	}
}