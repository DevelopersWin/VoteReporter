using DragonSpark.Sources;

namespace DragonSpark.Aspects.Extensibility
{
	class OriginInvocation : ThreadLocalStore<AspectInvocation>, IOriginInvocation
	{
		public object Invoke( object parameter ) => Get().Invoke( parameter );
	}
}