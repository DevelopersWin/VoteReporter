using DragonSpark.Activation;
using DragonSpark.Aspects.Validation;
using DragonSpark.Composition;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Setup;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.TypeSystem;
using DragonSpark.Windows;
using Ploeh.AutoFixture;
using PostSharp.Aspects;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using System.Windows.Input;
using InitializeLocationCommand = DragonSpark.Composition.InitializeLocationCommand;

namespace DragonSpark.Testing.Framework.Setup
{
	[LinesOfCodeAvoided( 5 )]
	public class AutoDataAttribute : Ploeh.AutoFixture.Xunit2.AutoDataAttribute
	{
		readonly static Func<IFixture> DefaultFixtureFactory = FixtureFactory<AutoDataCustomization>.Instance.Get;
		
		public AutoDataAttribute() : this( DefaultFixtureFactory ) {}

		protected AutoDataAttribute( Func<IFixture> fixture ) : base( FixtureContext.Instance.WithInstance( fixture() ) ) {}

		protected virtual IApplication ApplicationSource( MethodBase method ) => ApplicationFactory.Instance.Get( method );

		public override IEnumerable<object[]> GetData( MethodInfo methodUnderTest )
		{
			var applicationSource = ApplicationSource( methodUnderTest );
			applicationSource.Run( new AutoData( Fixture, methodUnderTest ) );

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
		public static IScope<ApplicationInitializer> Instance { get; } = new Scope<ApplicationInitializer>( Factory.ForGlobalScope( () => new ApplicationInitializer() ) );
		ApplicationInitializer() {}

		public override void Execute( MethodBase parameter )
		{
			MethodContext.Instance.Assign( parameter );
			Disposables.Instance.Get().Add( ExecutionContext.Instance.Get() );
		}
	}

	public sealed class FixtureContext : Scope<IFixture>
	{
		public static FixtureContext Instance { get; } = new FixtureContext();
		FixtureContext() {}
	}

	public class FrameworkTypesAttribute : TypeProviderAttributeBase
	{
		public FrameworkTypesAttribute() : base( typeof(InitializationCommand), typeof(Configure), typeof(InitializeLocationCommand), typeof(MetadataCommand) ) {}
	}

	[AttributeUsage( AttributeTargets.Class | AttributeTargets.Method )]
	public abstract class TypeProviderAttributeBase : HostingAttribute
	{
		protected TypeProviderAttributeBase( params Type[] types ) : this( types.ToImmutableArray() ) {}
		protected TypeProviderAttributeBase( ImmutableArray<Type> additionalTypes ) : this( new Factory( additionalTypes ).Get ) {}

		protected TypeProviderAttributeBase( Func<MethodBase, ImmutableArray<Type>> factory ) : this( factory.Wrap() ) {}
		protected TypeProviderAttributeBase( Func<object, Func<MethodBase, ImmutableArray<Type>>> provider ) : base( provider ) {}

		protected class Factory : ParameterizedSourceBase<MethodBase, ImmutableArray<Type>>
		{
			readonly ImmutableArray<Type> additionalTypes;
			public Factory( ImmutableArray<Type> additionalTypes )
			{
				this.additionalTypes = additionalTypes;
			}

			public override ImmutableArray<Type> Get( MethodBase parameter ) => additionalTypes;
		}
	}

	public class AdditionalTypesAttribute : TypeProviderAttributeBase
	{
		public AdditionalTypesAttribute( params Type[] additionalTypes ) : base( additionalTypes.ToImmutableArray() ) {}
	}

	public sealed class ApplicationPartsAttribute : TypeProviderAttributeBase
	{
		public ApplicationPartsAttribute() : base( m => AllParts.Instance.Get( m.DeclaringType.Assembly ) ) {}
	}

	public sealed class ApplicationPublicPartsAttribute : TypeProviderAttributeBase
	{
		public ApplicationPublicPartsAttribute() : base( m => PublicParts.Instance.Get( m.DeclaringType.Assembly ) ) {}
	}

	[AttributeUsage( AttributeTargets.Method )]
	public class IncludeParameterTypesAttribute : TypeProviderAttributeBase
	{
		public IncludeParameterTypesAttribute( params Type[] additionalTypes ) : base( new Factory( additionalTypes ).Get ) {}

		new sealed class Factory : TypeProviderAttributeBase.Factory
		{
			public Factory( params Type[] additionalTypes ) : base( additionalTypes.ToImmutableArray() ) {}

			public override ImmutableArray<Type> Get( MethodBase parameter ) => base.Get( parameter ).Union( parameter.GetParameterTypes() ).ToImmutableArray();
		}
	}

	public class ContainingTypeAndNestedAttribute : TypeProviderAttributeBase
	{
		readonly static Func<MethodBase, ImmutableArray<Type>> Delegate = Factory.Instance.Get;
		public ContainingTypeAndNestedAttribute() : base( Delegate ) {}

		new sealed class Factory : ParameterizedSourceBase<MethodBase, ImmutableArray<Type>>
		{
			public static Factory Instance { get; } = new Factory();
			Factory() {}

			public override ImmutableArray<Type> Get( MethodBase parameter ) => SelfAndNestedTypes.Instance.Get( parameter.DeclaringType ).ToImmutableArray();
		}
	}

	public abstract class CommandAttributeBase : HostingAttribute
	{
		protected CommandAttributeBase( ICommand command ) : this( command.Cast<AutoData>() ) {}
		protected CommandAttributeBase( ICommand<AutoData> command ) : base( command.Wrap() ) {}
	}

	[ApplyAutoValidation]
	sealed class FixtureServiceProvider : ValidatedParameterizedSourceBase<Type, object>, IServiceProvider
	{
		readonly IFixture fixture;

		public FixtureServiceProvider( IFixture fixture ) : base( new Specification( fixture ) )
		{
			this.fixture = fixture;
		}

		public override object Get( Type parameter ) => fixture.Create<object>( parameter );

		public object GetService( Type serviceType ) => Get( serviceType );

		sealed class Specification : SpecificationBase<Type>
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