using DragonSpark.Aspects.Diagnostics;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Data;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Scopes;
using DragonSpark.TypeSystem;
using JetBrains.Annotations;
using PostSharp.Extensibility;
using System;
using System.Linq;

namespace DragonSpark.Aspects.Configuration
{
	public sealed class Configuration<T> : SingletonScope<T>
	{
		readonly static Type Type = typeof(T);

		public static Configuration<T> Default { get; } = new Configuration<T>();
		Configuration() : base( Implementation.Instance.Get ) {}

		public sealed class Implementation : SourceBase<T>
		{
			public static Implementation Instance { get; } = new Implementation();
			Implementation() : this( PostSharpEnvironment.CurrentProject ) {}

			readonly IProject project;
			readonly ISerializer serializer;

			public Implementation( IProject project )
				: this( project,
						new Serializer( new DataContractSerializers(
											new CompositeItemSource<Type>(
												DefaultKnownApplicationTypes.Default,
												KnownTypesForSerialization.Default.GetEnumerable( project ),
												new AssemblyBasedTypeSource( typeof(T), typeof(InitializeDiagnosticsCommand) ) 
											)
										).Get
						)
				) {}

			[UsedImplicitly]
			public Implementation( IProject project, ISerializer serializer )
			{
				this.project = project;
				this.serializer = serializer;
			}

			public override T Get()
			{
				var data = project.GetExtensionElements( Type.Name, $"clr-namespace:{Type.Namespace};assembly:{Type.Assembly().GetName().Name}" ).SingleOrDefault()?.Xml;
				var result = data != null ? serializer.Load<T>( data ) : default(T);
				return result;
			}
		}
	}
}