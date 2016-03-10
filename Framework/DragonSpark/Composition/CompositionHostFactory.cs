using DragonSpark.Activation.FactoryModel;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Values;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Generic;
using System.Composition.Hosting;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Composition
{
	public static class Composer
	{
		// public static CompositionHost Create( [Required] Assembly[] assemblies ) => new CompositionHostFactory().Create( assemblies );

		public static CompositionHost Current => new CompositionHostContext().Item;

		public static T Compose<T>() => (T)Compose( typeof(T) );

		public static object Compose( [Required] Type type ) => Current.GetExport( type );

		public static object ComposeMany( [Required] Type type ) => Current.GetExports( type );
	}

	public class CompositionHostContext : ExecutionContextValue<CompositionHost> {}

	public class FactoryTypeContainer : TypeContainer<IFactory>
	{
		public FactoryTypeContainer( IEnumerable<Type> types ) : base( types ) {}
	}

	public class FactoryWithParameterTypeContainer : TypeContainer<IFactoryWithParameter>
	{
		public FactoryWithParameterTypeContainer( IEnumerable<Type> types ) : base( types ) {}
	}

	public abstract class TypeContainer<T>
	{
		protected TypeContainer( [Required]IEnumerable<Type> types )
		{
			Types = types.Where( typeof(T).Adapt().IsAssignableFrom ).ToArray();
		}

		public Type[] Types { get; }
	}

	public class CompositionHostFactory : FactoryBase<Assembly[], CompositionHost>
	{
		public static CompositionHostFactory Instance { get; } = new CompositionHostFactory();

		CompositionHostFactory() {}

		protected override CompositionHost CreateItem( Assembly[] parameter )
		{
			var types = parameter.SelectMany( assembly => assembly.DefinedTypes ).AsTypes().ToArray();
			var result = new ContainerConfiguration()
				.WithParts( types )
				.WithProvider( new RegisteredExportDescriptorProvider() )
				.WithProvider( new InstanceExportDescriptorProvider( parameter ) )
				.WithProvider( new InstanceExportDescriptorProvider( types ) )
				.WithProvider( new InstanceExportDescriptorProvider( new FactoryTypeContainer( types ) ) )
				.WithProvider( new InstanceExportDescriptorProvider( new FactoryWithParameterTypeContainer( types ) ) )
				.WithProvider( new FactoryExportDescriptorProvider( parameter ) )
				.WithProvider( new FactoryDelegateExportDescriptorProvider( parameter ) )
				.CreateContainer();
			return result;
		}
	}
}
