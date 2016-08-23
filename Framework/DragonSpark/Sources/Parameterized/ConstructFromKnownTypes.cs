using System;
using System.Linq;
using DragonSpark.TypeSystem;

namespace DragonSpark.Sources.Parameterized
{
	public class ConstructFromKnownTypes<T> : ParameterConstructedCompositeFactory<object>, IParameterizedSource<object, T>
	{
		public static ISource<IParameterizedSource<object, T>> Default { get; } = new Scope<ConstructFromKnownTypes<T>>( Factory.Global( () => new ConstructFromKnownTypes<T>( KnownTypes.Default.Get<T>().ToArray() ) ) );
		ConstructFromKnownTypes( params Type[] types ) : base( types ) {}

		T IParameterizedSource<object, T>.Get( object parameter ) => (T)Get( parameter );
	}
}