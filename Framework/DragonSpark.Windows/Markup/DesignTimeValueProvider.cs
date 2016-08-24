using DragonSpark.Sources.Parameterized;
using DragonSpark.TypeSystem;
using System;

namespace DragonSpark.Windows.Markup
{
	public sealed class DesignTimeValueProvider : CompositeFactory<Type, object>
	{
		public static DesignTimeValueProvider Default { get; } = new DesignTimeValueProvider();
		DesignTimeValueProvider() : base( SpecialValues.DefaultOrEmpty, MockFactory.Default.ToSourceDelegate(), StringDesignerValueFactory.Default.ToSourceDelegate() ) {}
	}
}