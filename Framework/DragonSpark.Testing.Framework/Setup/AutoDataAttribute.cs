using DragonSpark.Activation;
using DragonSpark.Aspects.Validation;
using DragonSpark.Diagnostics.Logger;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
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
using System.Windows.Input;
using Xunit.Sdk;

namespace DragonSpark.Testing.Framework.Setup
{
	[LinesOfCodeAvoided( 5 )]
	public class AutoDataAttribute : Ploeh.AutoFixture.Xunit2.AutoDataAttribute, IAspectProvider
	{
		readonly static Func<IFixture> DefaultFixtureFactory = FixtureFactory<AutoDataCustomization>.Instance.Create;
		readonly static Func<MethodBase, IApplication> DefaultSource = ApplicationFactory.Instance.Create;

		readonly Func<MethodBase, IApplication> applicationSource;

		public AutoDataAttribute() : this( DefaultFixtureFactory, DefaultSource ) {}

		// protected AutoDataAttribute( Func<MethodBase, IApplication> applicationSource  ) : this( DefaultFixtureFactory, applicationSource ) {}

		protected AutoDataAttribute( Func<IFixture> fixture, Func<MethodBase, IApplication> applicationSource  ) : base( FixtureContext.Instance.Assigned( fixture() ).Value )
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

	public class ApplicationFactory : FactoryBase<MethodBase, IApplication>
	{
		public static ApplicationFactory Instance { get; } = new ApplicationFactory();
		ApplicationFactory() : this( m => Items<ICommand>.Default ) {}

		readonly Func<MethodBase, IEnumerable<ICommand>> commandSource;

		public ApplicationFactory( Func<MethodBase, IEnumerable<ICommand>> commandSource )
		{
			this.commandSource = commandSource;
		}

		public override IApplication Create( MethodBase parameter )
		{
			new CompositeCommand( commandSource( parameter ).Fixed() ).Run();
			var result = ActiveApplication.Instance.Create<Application>();
			return result;
		}
	}

	public sealed class FixtureContext : Configuration<IFixture>
	{
		public static FixtureContext Instance { get; } = new FixtureContext();
		FixtureContext() {}
	}

	[AttributeUsage( AttributeTargets.Class | AttributeTargets.Assembly )]
	public class ApplicationTypesAttribute : Attribute
	{
		public ApplicationTypesAttribute( params Type[] additionalTypes )
		{
			AdditionalTypes = additionalTypes.ToImmutableArray();
		}

		public ImmutableArray<Type> AdditionalTypes { get; }
	}

	[AttributeUsage( AttributeTargets.Method )]
	public class AdditionalTypesAttribute : ApplicationTypesAttribute
	{
		public AdditionalTypesAttribute( params Type[] additionalTypes ) : this( true, additionalTypes ) {}

		public AdditionalTypesAttribute( bool includeFromParameters = true, params Type[] additionalTypes ) : base( additionalTypes )
		{
			IncludeFromParameters = includeFromParameters;
		}

		public bool IncludeFromParameters { get; }
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