using DragonSpark.Activation;
using DragonSpark.Activation.IoC;
using DragonSpark.Aspects.Validation;
using DragonSpark.Diagnostics.Logger;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Runtime.Stores;
using DragonSpark.Setup;
using DragonSpark.TypeSystem;
using Ploeh.AutoFixture;
using PostSharp.Aspects;
using PostSharp.Patterns.Contracts;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using Xunit.Sdk;
using AssemblyPartLocator = DragonSpark.Windows.TypeSystem.AssemblyPartLocator;

namespace DragonSpark.Testing.Framework.Setup
{
	[LinesOfCodeAvoided( 5 )]
	public class AutoDataAttribute : Ploeh.AutoFixture.Xunit2.AutoDataAttribute, IAspectProvider
	{
		readonly static Func<IFixture> DefaultFixtureFactory = FixtureFactory<AutoDataCustomization>.Instance.Create;
		readonly static Func<MethodBase, IApplication> DefaultSource = ApplicationFactory.Instance.Create;

		readonly Func<MethodBase, IApplication> applicationSource;

		public AutoDataAttribute() : this( DefaultFixtureFactory, DefaultSource ) {}

		protected AutoDataAttribute( Func<IFixture> fixture, Func<MethodBase, IApplication> applicationSource  ) : base( FixtureContext.Instance.Assigned( fixture() ) )
		{
			this.applicationSource = applicationSource;
		}

		public override IEnumerable<object[]> GetData( MethodInfo methodUnderTest )
		{
			applicationSource( methodUnderTest ).Run( new AutoData( Fixture, methodUnderTest ) );

			var result = base.GetData( methodUnderTest );
			return result;
		}

		IEnumerable<AspectInstance> IAspectProvider.ProvideAspects( object targetElement ) => targetElement.AsTo<MethodInfo, AspectInstance[]>( info => new AspectInstance( info, ExecuteMethodAspect.Instance ).ToItem() );
	}

	public sealed class ApplicationFactory : ConfiguringFactory<MethodBase, IApplication>
	{
		public static ApplicationFactory Instance { get; } = new ApplicationFactory();
		ApplicationFactory() : base( n => ApplicationConfiguration.Instance.Get(), MethodContext.Instance.Assign ) {}
	}

	public sealed class ApplicationConfiguration : Configuration<IApplication>
	{
		public static ApplicationConfiguration Instance { get; } = new ApplicationConfiguration();
		ApplicationConfiguration() : base( () => ApplicationFactory<Application>.Instance.Create( MethodTypes.Instance, ApplicationCommandsSource.Instance ) ) {}
	}

	public class FrameworkTypesAttribute : TypeProviderAttributeBase
	{
		public FrameworkTypesAttribute() : base( typeof(TestingApplicationInitializationCommand), typeof(Configure), typeof(MetadataCommand) ) {}
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

			public override ImmutableArray<Type> Create( MethodBase parameter ) => SelfAndNestedStrategy.Instance.Get( parameter.DeclaringType ).ToImmutableArray();
		}
	}

	

	public class MinimumLevel : BeforeAfterTestAttribute
	{
		readonly LogEventLevel level;

		public MinimumLevel( LogEventLevel level )
		{
			this.level = level;
		}

		public override void Before( MethodInfo methodUnderTest ) => LoggingController.Instance.Get( methodUnderTest ).MinimumLevel = level;
	}

	[ApplyAutoValidation]
	sealed class FixtureServiceProvider : FactoryBase<Type, object>, IServiceProvider
	{
		readonly IFixture fixture;

		public FixtureServiceProvider( [Required]IFixture fixture ) : base( new Specification( fixture ) )
		{
			this.fixture = fixture;
		}

		public override object Create( Type parameter ) => fixture.Create<object>( parameter );

		public object GetService( Type serviceType ) => Create( serviceType );

		sealed class Specification : GuardedSpecificationBase<Type>
		{
			readonly IServiceRegistry registry;

			public Specification( [Required] IFixture fixture ) : this( AssociatedRegistry.Default.Get( fixture ) ) {}

			Specification( [Required] IServiceRegistry registry )
			{
				this.registry = registry;
			}

			public override bool IsSatisfiedBy( Type parameter ) => registry.IsRegistered( parameter );
		}
	}
}