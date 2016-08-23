using DragonSpark.Aspects.Validation;
using DragonSpark.Runtime;
using DragonSpark.TypeSystem;
using System;
using System.Windows.Input;
using DragonSpark.Commands;
using DragonSpark.Specifications;

namespace DragonSpark.Setup
{
	/*public class InstanceRegistrationRequest<T> : InstanceRegistrationRequest
	{
		public InstanceRegistrationRequest( T instance, string name = null ) : base( typeof(T), instance, name ) {}
	}*/

	public interface IServiceRepository<T> : IServiceProvider, IRepository<T>, ISpecification<Type> {}

	/*public class ServiceProviderRegistry : RepositoryBase<IServiceProvider>
	{
		public static ISource<IRepository<IServiceProvider>> Default { get; } = new Scope<IRepository<IServiceProvider>>( Factory.ForGlobalScope( () => new ServiceProviderRegistry() ) );
		ServiceProviderRegistry() : base( DefaultServiceProvider.Default.Yield() ) {}

		protected override IEnumerable<IServiceProvider> Query() => base.Query().Reverse();
	}*/

	/*public abstract class InitializeServiceProviderCommandBase : Setup
	{
		protected InitializeServiceProviderCommandBase( Coerce<IServiceProvider> coercer ) : base( new DelegatedCommand<IServiceProvider>( RegisterServiceProviderCommand.Default.Execute, coercer ) ) {}
	}
*/

	/*[ApplyAutoValidation]
	public class RegisterServiceProviderCommand : CommandBase<IServiceProvider>
	{
		public static RegisterServiceProviderCommand Default { get; } = new RegisterServiceProviderCommand();
		RegisterServiceProviderCommand() : this( DependencyLocators.Default.Get, ServiceProviderRegistry.Default.Get ) {}

		readonly Func<IDependencyLocator> locatorSource;
		readonly Func<IRepository<IServiceProvider>> repositorySource;

		public RegisterServiceProviderCommand( Func<IDependencyLocator> locatorSource, Func<IRepository<IServiceProvider>> repositorySource )
		{
			this.locatorSource = locatorSource;
			this.repositorySource = repositorySource;
		}

		public override void Execute( IServiceProvider parameter )
		{
			var key = parameter.Get<IDependencyLocatorKey>();
			if ( key != null )
			{
				var locator = locatorSource();
				if ( !locator.Contains( key ) )
				{
					repositorySource().Add( locator.SetValue( key, parameter ) );
				}
			}
		}
	}

	public interface IDependencyLocator : ICache<IDependencyLocatorKey, IServiceProvider>
	{
		Func<Type, object> For( IDependencyLocatorKey locatorKey );
	}

	class DependencyLocators : Cache<IDependencyLocatorKey, IServiceProvider>, IDependencyLocator
	{
		public static ISource<IDependencyLocator> Default { get; } = new Scope<IDependencyLocator>( Factory.ForGlobalScope( () => new DependencyLocators() ) );
		DependencyLocators() {}

		public Func<Type, object> For( IDependencyLocatorKey locatorKey ) => Contains( locatorKey ) ? ActivatedServiceProvider.Sources.Get( Get( locatorKey ) ) : null;
	}

	

	public interface IDependencyLocatorKey {}*/

	/*	class ActivatedServiceProvider : IServiceProvider
	{
		readonly static Func<IServiceProvider, IValidatedParameterizedSource<Type, object>> Selector = ActivatedFactory.Default.Get;

		// public static IParameterizedSource<IServiceProvider, Func<Type, object>> Sources { get; } = new Cache<IServiceProvider, Func<Type, object>>( provider => new ActivatedServiceProvider( provider ).GetService );
		ActivatedServiceProvider( IServiceProvider provider ) : this( IsActive.Default.Get( provider ) ) {}

		readonly IsActive active;

		ActivatedServiceProvider( IsActive active )
		{
			this.active = active;
		}

		public object GetService( Type serviceType )
		{
			var stores = ServiceProviderRegistry.Default.Get().List().Select( Selector );
			var result = stores.Introduce( serviceType, tuple => tuple.Item1.IsSatisfiedBy( tuple.Item2 ), tuple => tuple.Item1.Get( tuple.Item2 ) ).FirstAssigned();
			return result;
		}
			
		// public bool CanProvide( Type serviceType ) => !active.Get( serviceType );
	}
*/

	public interface IApplication : ICommand, IDisposable {}

	[ApplyAutoValidation]
	public class Setup : CompositeCommand, ISetup
	{
		public Setup() : this( Items<ICommand>.Default ) {}

		public Setup( params ICommand[] commands ) : this( new OncePerScopeSpecification<object>(), commands ) {}
		public Setup( ISpecification<object> specification, params ICommand[] commands ) : base( specification, commands ) {}

		public DeclarativeCollection<object> Items { get; } = new DeclarativeCollection<object>();

		public Priority Priority { get; set; } = Priority.Normal;
	}
}
