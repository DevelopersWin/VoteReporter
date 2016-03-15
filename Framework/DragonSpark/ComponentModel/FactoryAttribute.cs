using DragonSpark.Activation.FactoryModel;
using DragonSpark.Composition;
using PostSharp.Patterns.Contracts;
using System;
using System.Composition.Hosting;
using DragonSpark.Extensions;
using DragonSpark.Setup;

namespace DragonSpark.ComponentModel
{
	public sealed class ApplicationServiceAttribute : ActivateAttributeBase
	{
		static DelegatedCreator Creator { get; } = new DelegatedCreator( Compose );

		static object Compose( Type arg ) => new CurrentApplication().Item.Context.GetService( arg );

		public ApplicationServiceAttribute( Type composedType = null ) : base( new ActivatedValueProvider.Converter( composedType, null ), Creator ) {}
	}

	public sealed class ComposeAttribute : ActivateAttributeBase
	{
		static DelegatedCreator Creator { get; } = new DelegatedCreator( Compose );

		static object Compose( Type arg ) => new CurrentApplication().Item.Context.Get<CompositionHost>().GetExport( arg );

		public ComposeAttribute( Type composedType = null ) : base( new ActivatedValueProvider.Converter( composedType, null ), Creator ) {}
	}

	public sealed class ComposeManyAttribute : ActivateAttributeBase
	{
		static DelegatedCreator Creator { get; } = new DelegatedCreator( Compose );

		static object Compose( Type arg ) => new CurrentApplication().Item.Context.Get<CompositionHost>().GetExports( arg );

		public ComposeManyAttribute( Type composedType = null ) : base( new ActivatedValueProvider.Converter( composedType, null ), Creator ) {}
	}

	public sealed class FactoryAttribute : ActivateAttributeBase
	{
		static DelegatedCreator Creator { get; } = new DelegatedCreator( Factory.From );

		public FactoryAttribute( Type factoryType = null, string name = null ) : base( new ActivatedValueProvider.Converter( p => factoryType ?? MemberInfoFactoryTypeLocator.Instance.Create( p ), name ), Creator ) {}
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