using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.TypeSystem;
using System.Collections.Immutable;
using System.Linq;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.ComponentModel
{
	public sealed class HostedValueLocator<T> : FactoryBase<object, ImmutableArray<T>> where T : class
	{
		public static HostedValueLocator<T> Instance { get; } = new HostedValueLocator<T>();
		HostedValueLocator() {}

		public override ImmutableArray<T> Create( object parameter ) => parameter.GetAttributes<HostingAttribute>().Introduce( parameter, tuple => tuple.Item1.Get( tuple.Item2 ) ).OfType<T>().ToImmutableArray();
	}
}