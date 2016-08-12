using DragonSpark.Aspects;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Sources;
using System;
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