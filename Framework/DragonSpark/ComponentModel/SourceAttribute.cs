using DragonSpark.Aspects;
using DragonSpark.Runtime.Sources;
using System;
using DragonSpark.Runtime.Sources.Caching;
using Activator = DragonSpark.Activation.Activator;

namespace DragonSpark.ComponentModel
{
	public class AmbientValueAttribute : ServicesValueBase
	{
		readonly static Func<Type, object> GetCurrentItem = AmbientStack.GetCurrentItem;
		public AmbientValueAttribute( Type valueType = null ) : base( new ServicesValueProvider.Converter( valueType ), GetCurrentItem ) {}
	}

	public class SourceAttribute : ServicesValueBase
	{
		readonly static Func<Type, object> Creator = Create;
		public SourceAttribute( [OfType( typeof(ISource) )]Type valueType ) : base( new ServicesValueProvider.Converter( valueType ), Creator ) {}

		static object Create( Type type ) => Activator.Activate<ISource>( type ).Get();

		/*public class Origin : ServicesValueProvider.Category
		{
			public new static Origin Instance { get; } = new Origin();

			readonly Func<Type, IValue> factory;

			public Origin() : this( new ServicesValueProvider.Origin<IValue>().Create ) { }

			protected Origin( [Required]Func<Type, IValue> factory )
			{
				this.factory = factory;
			}

			protected override object CreateItem( Type parameter ) => factory( parameter ).Item;
		}*/
	}
}