using DragonSpark.Activation;
using DragonSpark.Activation.FactoryModel;
using PostSharp.Patterns.Contracts;
using System;
using System.Composition;

namespace DragonSpark.ComponentModel
{
	public sealed class ServiceAttribute : ActivateAttributeBase
	{
		static DelegatedCreator Creator { get; } = new DelegatedCreator( Compose );

		static object Compose( Type arg ) => Services.Get( arg );

		public ServiceAttribute( Type composedType = null ) : base( new ActivatedValueProvider.Converter( composedType, null ), Creator ) {}
	}

	public sealed class ComposeAttribute : ActivateAttributeBase
	{
		static DelegatedCreator Creator { get; } = new DelegatedCreator( Compose );

		static object Compose( Type arg ) => Services.Get<CompositionContext>().GetExport( arg );

		public ComposeAttribute( Type composedType = null ) : base( new ActivatedValueProvider.Converter( composedType, null ), Creator ) {}
	}

	public sealed class ComposeManyAttribute : ActivateAttributeBase
	{
		static DelegatedCreator Creator { get; } = new DelegatedCreator( Compose );

		static object Compose( Type arg ) => Services.Get<CompositionContext>().GetExports( arg );

		public ComposeManyAttribute( Type composedType = null ) : base( new ActivatedValueProvider.Converter( composedType, null ), Creator ) {}
	}

	public sealed class FactoryAttribute : ActivateAttributeBase
	{
		static DelegatedCreator Creator { get; } = new DelegatedCreator( Factory );

		static object Factory( Type arg ) => Services.Get<InstanceFromFactoryTypeFactory>().Create( arg );

		public FactoryAttribute( Type factoryType = null, string name = null ) : this( Services.Get<MemberInfoFactoryTypeLocator>, factoryType, name ) {}

		public FactoryAttribute( Func<MemberInfoFactoryTypeLocator> locator, Type factoryType = null, string name = null ) : base( new ActivatedValueProvider.Converter( p => factoryType ?? locator().Create( p ), name ), Creator ) {}
	}

	public class DelegatedCreator : ActivatedValueProvider.Creator
	{
		readonly Func<Type, object> factory;

		public DelegatedCreator( [Required]Func<Type, object> factory )
		{
			this.factory = factory;
		}

		protected override object CreateItem( Tuple<ActivateParameter, DefaultValueParameter> parameter ) => factory( parameter.Item1.Type );
	}
}