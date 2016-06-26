using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.TypeSystem;
using System.Linq;

namespace DragonSpark.ComponentModel
{
	public class HostedValueLocator<T> : FactoryBase<object, T[]> where T : class
	{
		public static HostedValueLocator<T> Instance { get; } = new HostedValueLocator<T>();

		public override T[] Create( object parameter ) => parameter.GetAttributes<HostingAttribute>().Select( attribute => attribute.Create( parameter ) ).OfType<T>().ToArray();
	}
}