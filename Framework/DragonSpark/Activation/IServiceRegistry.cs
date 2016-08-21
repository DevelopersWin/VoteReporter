using DragonSpark.Runtime.Specifications;
using System;

namespace DragonSpark.Activation
{
	/*public interface IServiceRegistry
	{
		bool IsRegistered( Type type );

		void Register( MappingRegistrationParameter parameter );

		void Register( InstanceRegistrationParameter parameter );

		void RegisterFactory( FactoryRegistrationParameter parameter );
	}*/

	public class IsATypeSpecification : DecoratedSpecification<Type>
	{
		public static IsATypeSpecification Default { get; } = new IsATypeSpecification();
		IsATypeSpecification() : base( Specifications<Type>.Assigned ) { }
	}

	/*public class OnlyIfNotRegistered : DecoratedSpecification<Type>
	{
		public OnlyIfNotRegistered( IUnityContainer container ) : base( new IsRegisteredSpecification( container ).Inverse().Project<Type, TypeRequest>( LocatorBase.Coercer.Default.Coerce ) ) { }
	}*/

	/*public class RegisterInstanceByConventionCommand : RegisterInstanceByConventionCommand<IsATypeSpecification>
	{
		public RegisterInstanceByConventionCommand( IServiceRegistry registry, ConventionImplementedInterfaces locator ) : base( registry, locator, IsATypeSpecification.Default ) {}
	}

	public class RegisterInstanceByConventionCommand<T> : RegisterInstanceCommand<T> where T : ISpecification<Type>
	{
		readonly ConventionImplementedInterfaces locator;

		public RegisterInstanceByConventionCommand( IServiceRegistry registry, [Required]ConventionImplementedInterfaces locator, T specification ) : base( registry, specification )
		{
			this.locator = locator;
		}

		public override void Execute( InstanceRegistrationParameter parameter )
		{
			var located = locator.Get( parameter.Default.GetType() );
			if ( located != null )
			{
				base.Execute( new InstanceRegistrationParameter( located, parameter.Default, parameter.Name ) );
			}
		}
	}

	public class RegisterEntireHierarchyCommand : RegisterEntireHierarchyCommand<IsATypeSpecification>
	{
		public RegisterEntireHierarchyCommand( IServiceRegistry registry ) : base( registry, IsATypeSpecification.Default ) {}
	}

	public class RegisterEntireHierarchyCommand<T> : RegisterHierarchyCommandBase<T> where T : ISpecification<Type>
	{
		public RegisterEntireHierarchyCommand( IServiceRegistry registry, T specification ) : base( registry, specification, parameter => parameter.Default.Adapt().GetEntireHierarchy() ) {}
	}

	public class RegisterHierarchyCommand : RegisterHierarchyCommand<IsATypeSpecification>
	{
		public RegisterHierarchyCommand( IServiceRegistry registry ) : base( registry, IsATypeSpecification.Default ) {}
	}

	public class RegisterHierarchyCommand<T> : RegisterHierarchyCommandBase<T> where T : ISpecification<Type>
	{
		public RegisterHierarchyCommand( IServiceRegistry registry, T specification ) : base( registry, specification, parameter => parameter.Default.Adapt().GetHierarchy( false ) ) {}
	}

	public abstract class RegisterHierarchyCommandBase<T> : RegisterInstanceCommand<T> where T : ISpecification<Type>
	{
		readonly Func<InstanceRegistrationParameter, IEnumerable<Type>> typeResolver;

		protected RegisterHierarchyCommandBase( IServiceRegistry registry, T specification, [Required]Func<InstanceRegistrationParameter,IEnumerable<Type>> typeResolver ) : base( registry, specification )
		{
			this.typeResolver = typeResolver;
		}

		public override void Execute( InstanceRegistrationParameter parameter )
		{
			foreach ( var type in typeResolver( parameter ) )
			{
				base.Execute( new InstanceRegistrationParameter( type, parameter.Default, parameter.Name ) );
			}
		}
	}

	public abstract class RegistrationCommandBase<TParameter, TSpecification> : DelegatedCommand<TParameter> where TParameter : RegistrationParameter where TSpecification : ISpecification<Type>
	{
		protected RegistrationCommandBase( [Required]Action<TParameter> command, [Required]TSpecification specification ) 
			: base( command, specification.Project<TypeRequest, Type>( request => request.RequestedType ) ) {}
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

		public object Default { get; }
	}

	public class FactoryRegistrationParameter : RegistrationParameter
	{
		public FactoryRegistrationParameter( Type type, [Required]Func<object> factory, string name = null ) : base( type, name )
		{
			Factory = factory;
		}

		public Func<object> Factory { get; }
	}*/
}