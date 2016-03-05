using DragonSpark.Activation.FactoryModel;
using DragonSpark.Composition;
using PostSharp.Patterns.Contracts;
using System;

namespace DragonSpark.ComponentModel
{
	public sealed class ComposeAttribute : ActivateAttributeBase
	{
		static DelegatedCreator Creator { get; } = new DelegatedCreator( Composer.Compose );

		public ComposeAttribute( Type composedType = null ) : base( new ActivatedValueProvider.Converter( composedType, null ), Creator ) {}
	}

	public sealed class ComposeManyAttribute : ActivateAttributeBase
	{
		static DelegatedCreator Creator { get; } = new DelegatedCreator( Composer.ComposeMany );

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