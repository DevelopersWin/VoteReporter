using DragonSpark.Activation;
using System;
using System.Composition;

namespace DragonSpark.ComponentModel
{
	public sealed class ServiceAttribute : ServicesValueBase
	{
		//static DelegatedCreator Creator { get; } = new DelegatedCreator( Services.Get );

		public ServiceAttribute( Type serviceType = null ) : base( new ServicesValueProvider.Converter( serviceType ) ) {}
	}

	public sealed class ComposeAttribute : ServicesValueBase
	{
		// static DelegatedCreator Creator { get; } = new DelegatedCreator( Compose );

		static object Compose( Type arg ) => Services.Get<CompositionContext>().GetExport( arg );

		public ComposeAttribute( Type composedType = null ) : base( new ServicesValueProvider.Converter( composedType ), Compose ) {}
	}

	public sealed class ComposeManyAttribute : ServicesValueBase
	{
		// static DelegatedCreator Creator { get; } = new DelegatedCreator( Compose );

		static object Compose( Type arg ) => Services.Get<CompositionContext>().GetExports( arg );

		public ComposeManyAttribute( Type composedType = null ) : base( new ServicesValueProvider.Converter( composedType ), Compose ) {}
	}

	public sealed class FactoryAttribute : ServicesValueBase
	{
		// static DelegatedCreator Creator { get; } = new DelegatedCreator( Category );

		static object Factory( Type arg ) => Services.Get<InstanceFromFactoryTypeFactory>().Create( arg );

		public FactoryAttribute( Type factoryType = null ) : this( Services.Get<MemberInfoFactoryTypeLocator>, factoryType ) {}

		public FactoryAttribute( Func<MemberInfoFactoryTypeLocator> locator, Type factoryType = null ) : base( new ServicesValueProvider.Converter( p => factoryType ?? locator().Create( p ) ), Factory ) {}
	}

	/*public class DelegatedCreator : ServicesValueProvider.Category
	{
		readonly Func<Type, object> factory;

		public DelegatedCreator( [Required]Func<Type, object> factory )
		{
			this.factory = factory;
		}

		protected override object CreateItem( Tuple<LocateTypeRequest, DefaultValueParameter> parameter ) => factory( parameter.Item1.RequestedType );
	}*/
}