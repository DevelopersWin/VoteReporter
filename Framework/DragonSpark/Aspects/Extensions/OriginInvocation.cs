using DragonSpark.Sources;

namespace DragonSpark.Aspects.Extensions
{
	class OriginInvocation : ThreadLocalStore<AspectInvocation>, IOriginInvocation
	{
		public object Invoke( object parameter ) => Get().Invoke( parameter );
	}
}