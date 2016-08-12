using DragonSpark.Aspects;
using DragonSpark.Runtime.Properties;
using System;
using DragonSpark.Runtime.Sources;
using Activator = DragonSpark.Activation.Activator;

namespace DragonSpark.ComponentModel
{
	public class AmbientValueAttribute : ServicesValueBase
	{
		readonly static Func<Type, object> GetCurrentItem = AmbientStack.GetCurrentItem;
		public AmbientValueAttribute( Type valueType = null ) : base( new ServicesValueProvider.Converter( valueType ), GetCurrentItem ) {}
	}

	public class ValueAttribute : ServicesValueBase
	{
		readonly static Func<Type, object> Creator = Create;
		public ValueAttribute( [OfType( typeof(IStore) )]Type valueType ) : base( new ServicesValueProvider.Converter( valueType ), Creator ) {}

		static object Create( Type type ) => Activator.Activate<IStore>( type ).Value;

		/*public class Creator : ServicesValueProvider.Category
		{
			public new static Creator Instance { get; } = new Creator();

			readonly Func<Type, IValue> factory;

			public Creator() : this( new ServicesValueProvider.Creator<IValue>().Create ) { }

			protected Creator( [Required]Func<Type, IValue> factory )
			{
				this.factory = factory;
			}

			protected override object CreateItem( Type parameter ) => factory( parameter ).Item;
		}*/
	}
}