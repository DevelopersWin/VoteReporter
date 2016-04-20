using DragonSpark.Activation.IoC;
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

		public static object Checked( this LifetimeContext @this, object instance )
		{
			instance.As<IDisposable>( @this.AddBoundInstance );
			return instance;
		}
	}

	public class InstanceExportDescriptorProvider<T> : ExportDescriptorProvider
	{
		readonly T instance;
		readonly CompositionContract[] contracts;

		public InstanceExportDescriptorProvider( [Required]T instance, string name = null )
		{
			this.instance = instance;
			contracts = new [] { typeof(T), instance.GetType() }.Distinct().Select( type => new CompositionContract( type, name ) ).ToArray();
		}

		public override IEnumerable<ExportDescriptorPromise> GetExportDescriptors( CompositionContract contract, DependencyAccessor descriptorAccessor )
		{
			if ( contracts.Contains( contract ) )
			{
				yield return new ExportDescriptorPromise( contract, GetType().FullName, true, NoDependencies, dependencies => ExportDescriptor.Create( ( context, operation ) => instance, NoMetadata ) );
			}
		}
	}

	// https://github.com/dotnet/corefx/issues/6857
	public class TypeInitializingExportDescriptorProvider : ExportDescriptorProvider
	{
		readonly BuildableTypeFromConventionLocator locator;

		public TypeInitializingExportDescriptorProvider( BuildableTypeFromConventionLocator locator )
		{
			this.locator = locator;
		}

		public override IEnumerable<ExportDescriptorPromise> GetExportDescriptors( CompositionContract contract, DependencyAccessor descriptorAccessor )
		{
			new[] { contract.ContractType, locator.Create( contract.ContractType ) }
				.NotNull()
				.Distinct()
				.Each( InitializeTypeCommand.Instance.ExecuteWith );
			yield break;
		}
	}
}