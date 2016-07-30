using DragonSpark.Activation;
using DragonSpark.Activation.IoC;
using DragonSpark.Aspects.Validation;
using DragonSpark.Configuration;
using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Runtime.Stores;
using DragonSpark.Setup;
using DragonSpark.TypeSystem;
using DragonSpark.Windows;
using Ploeh.AutoFixture;
using PostSharp.Aspects;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using System.Windows.Input;
using AssemblyPartLocator = DragonSpark.Windows.TypeSystem.AssemblyPartLocator;

namespace DragonSpark.Testing.Framework.Setup
{
	[LinesOfCodeAvoided( 5 )]
	public class AutoDataAttribute : Ploeh.AutoFixture.Xunit2.AutoDataAttribute
	{
		readonly static Func<IFixture> DefaultFixtureFactory = FixtureFactory<AutoDataCustomization>.Instance.Create;
		
		public AutoDataAttribute() : this( DefaultFixtureFactory ) {}

		protected AutoDataAttribute( Func<IFixture> fixture ) : base( FixtureContext.Instance.Assigned( fixture() ) ) {}

		protected virtual IApplication ApplicationSource( MethodBase method ) => ApplicationFactory.Instance.Create( method );

		public override IEnumerable<object[]> GetData( MethodInfo methodUnderTest )
		{
			ApplicationSource( methodUnderTest ).Run( new AutoData( Fixture, methodUnderTest ) );

			var result = base.GetData( methodUnderTest );
			return result;
		}
	}

	public sealed class ApplicationFactory : ConfiguringFactory<MethodBase, IApplication>
	{
		public static ApplicationFactory Instance { get; } = new ApplicationFactory();
		ApplicationFactory() : base( DefaultCreate, Initialize ) {}

		static void Initialize( MethodBase method ) => ApplicationInitializer.Instance.Get().Execute( method );

		static IApplication DefaultCreate( MethodBase _ ) => 
			ApplicationFactory<Application>.Instance.Create( MethodTypes.Instance, ApplicationCommandsSource.Instance );
	}

	public sealed class ApplicationInitializer : CommandBase<MethodBase>
	{
		public static IConfiguration<ApplicationInitializer> Instance { get; } = new Configuration<ApplicationInitializer>( () => new ApplicationInitializer() );
		ApplicationInitializer() {}

		public override void Execute( MethodBase parameter )
		{
			MethodContext.Instance.Assign( parameter );
			Disposables.Instance.Value.Add( ExecutionContextStore.Instance.Value );
		}
	}

	public class FrameworkTypesAttribute : TypeProviderAttributeBase
	{
		public FrameworkTypesAttribute() : base( typeof(InitializationCommand), typeof(Configure), typeof(MetadataCommand) ) {}
	}

	public sealed class FixtureContext : Configuration<IFixture>
	{
		public static FixtureContext Instance { get; } = new FixtureContext();
		FixtureContext() {}
	}

	public abstract class TypeProviderAttributeBase : HostingAttribute
	{
		protected TypeProviderAttributeBase( params Type[] types ) : this( types.ToImmutableArray() ) {}
		protected TypeProviderAttributeBase( ImmutableArray<Type> additionalTypes ) : this( new Factory( additionalTypes ).Create ) {}

		protected TypeProviderAttributeBase( Func<MethodBase, ImmutableArray<Type>> factory ) : this( factory.Wrap() ) {}
		protected TypeProviderAttributeBase( Func<object, Func<MethodBase, ImmutableArray<Type>>> provider ) : base( provider ) {}

		sealed class Factory : FactoryBase<MethodBase, ImmutableArray<Type>>
		{
			readonly ImmutableArray<Type> additionalTypes;
			public Factory( ImmutableArray<Type> additionalTypes )
			{
				this.additionalTypes = additionalTypes;
			}

			public override ImmutableArray<Type> Create( MethodBase parameter ) => additionalTypes;
		}
	}

	[AttributeUsage( AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = true )]
	public class ApplicationTypesAttribute : TypeProviderAttributeBase
	{
		public ApplicationTypesAttribute( params Type[] additionalTypes ) : base( additionalTypes.ToImmutableArray() ) {}
	}

	[AttributeUsage( AttributeTargets.Class | AttributeTargets.Assembly | AttributeTargets.Method )]
	public class ApplicationPartsAttribute : TypeProviderAttributeBase
	{
		public ApplicationPartsAttribute() : base( m => AssemblyPartLocator.All.Get( m.DeclaringType.Assembly ) ) {}
	}

	[AttributeUsage( AttributeTargets.Class | AttributeTargets.Assembly | AttributeTargets.Method )]
	public class ApplicationPublicPartsAttribute : TypeProviderAttributeBase
	{
		public ApplicationPublicPartsAttribute() : base( m => AssemblyPartLocator.Public.Get( m.DeclaringType.Assembly ) ) {}
	}

	[AttributeUsage( AttributeTargets.Method, AllowMultiple = true )]
	public class AdditionalTypesAttribute : TypeProviderAttributeBase
	{
		public AdditionalTypesAttribute( params Type[] additionalTypes ) : this( true, additionalTypes ) {}

		public AdditionalTypesAttribute( bool includeFromParameters = true, params Type[] additionalTypes ) : base( new Factory( includeFromParameters, additionalTypes ).Create ) {}

		sealed class Factory : FactoryBase<MethodBase, ImmutableArray<Type>>
		{
			readonly bool includeFromParameters;
			readonly ImmutableArray<Type> additionalTypes;

			public Factory( bool includeFromParameters, params Type[] additionalTypes )
			{
				this.includeFromParameters = includeFromParameters;
				this.additionalTypes = additionalTypes.ToImmutableArray();
			}

			public override ImmutableArray<Type> Create( MethodBase parameter ) => additionalTypes.Concat( includeFromParameters ? parameter.GetParameterTypes() : Items<Type>.Default ).ToImmutableArray();
		}
	}

	[AttributeUsage( AttributeTargets.Class | AttributeTargets.Assembly | AttributeTargets.Method )]
	public class ContainingTypeAndNestedAttribute : TypeProviderAttributeBase
	{
		readonly static Func<MethodBase, ImmutableArray<Type>> Delegate = Factory.Instance.Create;
		public ContainingTypeAndNestedAttribute() : base( Delegate ) {}

		sealed class Factory : FactoryBase<MethodBase, ImmutableArray<Type>>
		{
			public static Factory Instance { get; } = new Factory();
			Factory() {}

			public override ImmutableArray<Type> Create( MethodBase parameter ) => SelfAndNestedTypes.Instance.Get( parameter.DeclaringType ).ToImmutableArray();
		}
	}

	public abstract class CommandAttributeBase : HostingAttribute
	{
		protected CommandAttributeBase( ICommand command ) : this( command.Cast<AutoData>() ) {}
		protected CommandAttributeBase( ICommand<AutoData> command ) : base( command.Wrap() ) {}
	}

	public class MinimumLevel : CommandAttributeBase
	{
		public MinimumLevel( LogEventLevel level ) : base( MinimumLevelConfiguration.Instance.From( level ).Cast<AutoData>().WithPriority( Priority.BeforeNormal ) ) {}
	}

	[ApplyAutoValidation]
	sealed class FixtureServiceProvider : FactoryBase<Type, object>, IServiceProvider
	{
		readonly IFixture fixture;

		public FixtureServiceProvider( IFixture fixture ) : base( new Specification( fixture ) )
		{
			this.fixture = fixture;
		}

		public override object Create( Type parameter ) => fixture.Create<object>( parameter );

		public object GetService( Type serviceType ) => Create( serviceType );

		sealed class Specification : GuardedSpecificationBase<Type>
		{
			readonly IServiceRegistry registry;

			public Specification( IFixture fixture ) : this( AssociatedRegistry.Default.Get( fixture ) ) {}

			Specification( IServiceRegistry registry )
			{
				this.registry = registry;
			}

			public override bool IsSatisfiedBy( Type parameter ) => registry.IsRegistered( parameter );
		}
	}
}