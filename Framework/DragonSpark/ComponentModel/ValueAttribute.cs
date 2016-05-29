using DragonSpark.Activation;
using DragonSpark.Aspects;
using DragonSpark.Runtime.Values;
using System;

namespace DragonSpark.ComponentModel
{
	public class AmbientValueAttribute : ServicesValueBase
	{
		public AmbientValueAttribute( Type valueType = null ) : base( new ServicesValueProvider.Converter( valueType ), AmbientStack.GetCurrentItem ) {}
	}

	public class ValueAttribute : ServicesValueBase
	{
		public ValueAttribute( [OfType( typeof(IStore) )]Type valueType ) : base( new ServicesValueProvider.Converter( valueType ), Create ) {}

		static object Create( Type type ) => Services.Get<IStore>( type ).Value;

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