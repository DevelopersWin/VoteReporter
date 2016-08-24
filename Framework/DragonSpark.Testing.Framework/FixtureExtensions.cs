using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Kernel;
using System;

namespace DragonSpark.Testing.Framework
{
	public static class FixtureExtensions
	{
		public static T Create<T>( this IFixture @this, Type type ) => (T)new SpecimenContext( @this ).Resolve( type );

		/*public static T TryCreate<T>( this IFixture @this, Type type )
		{
			try
			{
				var result = @this.Create<T>( type );
				return result;
			}
			catch ( ObjectCreationException )
			{
				return default(T);
			}
		}*/
	}
}