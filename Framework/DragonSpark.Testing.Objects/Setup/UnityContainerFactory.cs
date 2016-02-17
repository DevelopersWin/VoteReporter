using DragonSpark.Activation.FactoryModel;
using DragonSpark.Diagnostics;
using DragonSpark.Testing.Framework;
using DragonSpark.TypeSystem;
using Microsoft.Practices.Unity;
using PostSharp.Patterns.Contracts;
using Serilog;
using Serilog.Core;
using System.Reflection;

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

	public abstract class UnityContainerFactory<TAssemblyProvider> : Activation.IoC.UnityContainerFactory
		where TAssemblyProvider : IAssemblyProvider, new()
	{
		protected UnityContainerFactory( ILogger logger ) : this( new TAssemblyProvider().Create(), logger ) {}

		protected UnityContainerFactory( [Required]Assembly[] assemblies, [Required]ILogger logger )
		{
			Assemblies = assemblies;
			Logger = logger;
		}
	}
}
