using DragonSpark.Activation.FactoryModel;
using DragonSpark.Activation.IoC;
using DragonSpark.Diagnostics;
using DragonSpark.Testing.Framework;
using Microsoft.Practices.Unity;
using PostSharp.Patterns.Contracts;
using Serilog.Core;

namespace DragonSpark.Testing.Objects.Setup
{
	[Discoverable]
	public class UnityContainerFactory : UnityContainerFactory<AssemblyProvider>
	{
		readonly RecordingLogEventSink sink;

		public class Register : RegisterFactoryAttribute
		{
			public Register() : base( typeof(UnityContainerFactory) ) {}
		}

		public UnityContainerFactory() : this( new RecordingLogEventSink() ) {}

		public UnityContainerFactory( [Required]RecordingLogEventSink sink ) : base( new RecordingSinkFactory( sink ).Create() )
		{
			this.sink = sink;
		}

		public static UnityContainerFactory Instance { get; } = new UnityContainerFactory();

		protected override IUnityContainer CreateItem() => base.CreateItem().RegisterInstance<ILogEventSink>( sink );
	}
}
