using DragonSpark.Sources.Parameterized;
using DragonSpark.TypeSystem;
using System;

namespace DragonSpark.Windows.Legacy.Markup
{
	public sealed class DesignTimeValueProvider : CompositeFactory<Type, object>
	{
		public static DesignTimeValueProvider Default { get; } = new DesignTimeValueProvider();
		DesignTimeValueProvider() : base( new DelegatedParameterizedSource<Type, object>( DefaultValues.Default.Get ), MockFactory.Default, StringDesignerValueFactory.Default ) {}
	}
}