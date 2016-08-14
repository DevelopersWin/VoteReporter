using DragonSpark.Extensions;
using DragonSpark.TypeSystem;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Composition.Hosting;
using System.Composition.Hosting.Core;
using System.Linq;

namespace DragonSpark.Composition
{
	public static class CompositionHostExtensions
	{
		public static T TryGet<T>( this CompositionContext @this, string name = null ) => TryGet<T>( @this, typeof(T), name );

		public static T TryGet<T>( this CompositionContext @this, Type type, string name = null )
		{
			object existing;
			var result = @this.TryGetExport( type, name, out existing ) ? (T)existing : default(T);
			return result;
		}

		public static ContainerConfiguration WithInstance<T>( [Required] this ContainerConfiguration @this, T instance, string name = null ) => @this.WithProvider( new InstanceExportDescriptorProvider<T>( instance, name ) );

		public static object Registered( this LifetimeContext @this, object instance )
		{
			instance.As<IDisposable>( @this.AddBoundInstance );
			return instance;
		}
	}

	public class InstanceExportDescriptorProvider<T> : ExportDescriptorProvider
	{
		readonly T instance;
		readonly CompositionContract[] contracts;
		readonly CompositeActivator activate;

		public InstanceExportDescriptorProvider( T instance, string name = null )
		{
			this.instance = instance;
			contracts = new [] { typeof(T), instance.GetType() }.Distinct().Introduce( name, tuple => new CompositionContract( tuple.Item1, tuple.Item2 ) ).ToArray();
			activate = Activator;
		}

		public override IEnumerable<ExportDescriptorPromise> GetExportDescriptors( CompositionContract contract, DependencyAccessor descriptorAccessor )
		{
			if ( contracts.Contains( contract ) )
			{
				// ActivationProperties.Instance.Set( instance, true );
				yield return new ExportDescriptorPromise( contract, GetType().FullName, true, NoDependencies, GetDescriptor );
			}
		}

		ExportDescriptor GetDescriptor( IEnumerable<CompositionDependency> dependencies ) => ExportDescriptor.Create( activate, NoMetadata );

		object Activator( LifetimeContext context, CompositionOperation operation ) => instance;
	}

	// https://github.com/dotnet/corefx/issues/6857
	public class TypeInitializingExportDescriptorProvider : ExportDescriptorProvider
	{
		readonly Func<Type, Type> types;
		readonly static Action<Type> Initializer = InitializeTypeCommand.Instance.ToDelegate();

		public TypeInitializingExportDescriptorProvider() : this( ConventionTypes.Instance.Get ) {}

		TypeInitializingExportDescriptorProvider( Func<Type, Type> types )
		{
			this.types = types;
		}

		public override IEnumerable<ExportDescriptorPromise> GetExportDescriptors( CompositionContract contract, DependencyAccessor descriptorAccessor )
		{
			contract.ContractType.Yield()
				.Append( types( contract.ContractType ) )
				.WhereAssigned()
				.Distinct()
				.Each( Initializer );
			yield break;
		}
	}
}