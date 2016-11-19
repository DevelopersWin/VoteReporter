using DragonSpark.Aspects.Diagnostics;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Data;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.TypeSystem;
using JetBrains.Annotations;
using PostSharp.Extensibility;
using System;
using System.Linq;

namespace DragonSpark.Aspects.Configuration
{
	public sealed class Configuration<T> : SourceBase<T>
	{
		readonly static Type Type = typeof(T);
		
		public static Configuration<T> Default { get; } = new Configuration<T>();
		Configuration() : this( PostSharpEnvironment.CurrentProject ) {}

		readonly IProject project;
		readonly ISerializer serializer;
		
		[UsedImplicitly]
		public Configuration( IProject project )
			: this( project, 
					new Serializer( new DataContractSerializers( 
										new CompositeItemSource<Type>(
											KnownTypesForSerialization.Default.GetEnumerable( project ),
											new AssemblyBasedTypeSource( typeof(T), typeof(InitializeDiagnosticsCommand) ) ) 
									).Get
					) 
			) {}

		[UsedImplicitly]
		public Configuration( IProject project, ISerializer serializer )
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