using DragonSpark.Activation.FactoryModel;
using DragonSpark.Activation.IoC;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Values;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Composition.Hosting;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Composition
{
	public static class Composer
	{
		public static CompositionHost Current => new CompositionHostContext().Item;

		public static T Compose<T>() => (T)Compose( typeof(T) );

		public static object Compose( [Required] Type type ) => Current.GetExport( type );

		public static object ComposeMany( [Required] Type type ) => Current.GetExports( type );
	}

	public class CompositionHostContext : ExecutionContextValue<CompositionHost> {}

	public class FactoryTypeProfile
	{
		public FactoryTypeProfile( [Required]Type factoryType, string name, [Required]Type resultType )
		{
			FactoryType = factoryType;
			Name = name;
			ResultType = resultType;
		}

		public Type FactoryType { get; }
		public string Name { get; }
		public Type ResultType { get; }
	}

	public class FactoryTypeContainer : List<FactoryTypeProfile>
	{
		public FactoryTypeContainer( IEnumerable<Type> types ) : base( 
			types
				.Where( Factory.IsFactory )
				.Where( x => x.IsDefined<ExportAttribute>( true ) )
				.Where( CanBuildSpecification.Instance.IsSatisfiedBy )
				.Select( type => new FactoryTypeProfile( type, type.From<ExportAttribute, string>( attribute => attribute.ContractName ), Factory.GetResultType( type ) ) ) )
		{}
	}

	/*public class FactoryTypeContainer : TypeOfContainer<IFactory>
	{
		public FactoryTypeContainer( IEnumerable<Type> types ) : base( types ) {}
	}

	public class FactoryWithParameterTypeContainer : TypeOfContainer<IFactoryWithParameter>
	{
		public FactoryWithParameterTypeContainer( IEnumerable<Type> types ) : base( types ) {}
	}*/

	/*public abstract class TypeOfContainer<T> : TypeContainer
	{
		protected TypeOfContainer( [Required]IEnumerable<Type> types ) : base( types.Where( typeof(T).Adapt().IsAssignableFrom ) ) {}
	}*/

	public abstract class TypeContainer : List<Type>
	{
		protected TypeContainer( [Required]IEnumerable<Type> types ) : base( types ) {}
	}

	public class CompositionHostFactory : FactoryBase<Assembly[], CompositionHost>
	{
		public static CompositionHostFactory Instance { get; } = new CompositionHostFactory();

		CompositionHostFactory() {}

		protected override CompositionHost CreateItem( Assembly[] parameter )
		{
			var types = parameter.SelectMany( assembly => assembly.DefinedTypes ).AsTypes().ToArray();
			var container = new FactoryTypeContainer( types );
			var locator = new DiscoverableFactoryTypeLocator( container );
			var result = new ContainerConfiguration()
				.WithProvider( new RegisteredExportDescriptorProvider() )
				.WithProvider( new InstanceExportDescriptorProvider( parameter ) )
				.WithProvider( new InstanceExportDescriptorProvider( types ) )
				.WithProvider( new InstanceExportDescriptorProvider( container ) )
				.WithProvider( new InstanceExportDescriptorProvider( locator ) )
				/*.WithProvider( new InstanceExportDescriptorProvider( new FactoryTypeContainer( types ) ) )
				.WithProvider( new InstanceExportDescriptorProvider( new FactoryWithParameterTypeContainer( types ) ) )*/
				.WithProvider( new FactoryExportDescriptorProvider( locator ) )
				.WithParts( types )
				.WithProvider( new FactoryDelegateExportDescriptorProvider( locator ) )
				.CreateContainer();
			return result;
		}
	}
}
