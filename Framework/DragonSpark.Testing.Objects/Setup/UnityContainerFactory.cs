using DragonSpark.Testing.Framework;
using System.Composition;

namespace DragonSpark.Testing.Objects.Setup
{
	[Export]
	public class UnityContainerFactory : Activation.IoC.UnityContainerFactory
	{
		public static UnityContainerFactory Instance { get; } = new UnityContainerFactory();

		public class Register : RegisterFactoryAttribute
		{
			public Register() : base( typeof(UnityContainerFactory) ) {}
		}
	}

	[Export]
	public class RecordingSinkFactory : Diagnostics.RecordingSinkFactory
	{}
}
