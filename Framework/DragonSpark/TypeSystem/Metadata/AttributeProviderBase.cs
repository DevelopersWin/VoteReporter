using System;
using System.Collections.Generic;
using DragonSpark.Sources.Parameterized.Caching;

namespace DragonSpark.TypeSystem.Metadata
{
	public abstract class AttributeProviderBase : IAttributeProvider
	{
		readonly ICache<Type, bool> defined;
		readonly ICache<Type, IEnumerable<Attribute>> factory;

		protected AttributeProviderBase()
		{
			defined = new DecoratedSourceCache<Type, bool>( new WritableSourceCache<Type, bool>( new Func<Type, bool>( Contains ) ) );
			factory = new Cache<Type, IEnumerable<Attribute>>( GetAttributes );
		}

		public abstract bool Contains( Type attributeType );

		public abstract IEnumerable<Attribute> GetAttributes( Type attributeType );

		IEnumerable<Attribute> IAttributeProvider.GetAttributes( Type attributeType ) => defined.Get( attributeType ) ? factory.Get( attributeType ) : Items<Attribute>.Default;
	}
}