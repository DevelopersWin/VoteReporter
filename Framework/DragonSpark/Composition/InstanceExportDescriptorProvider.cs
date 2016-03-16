using DragonSpark.Extensions;
using DragonSpark.TypeSystem;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Generic;
using System.Composition.Hosting;
using System.Composition.Hosting.Core;
using System.Linq;
using ExportDescriptorProvider = System.Composition.Hosting.Core.ExportDescriptorProvider;

namespace DragonSpark.Composition
{
	public static class CompositionHostExtensions
	{
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
				new ExportProperties.Instance( instance ).Assign( true );
				yield return new ExportDescriptorPromise( contract, GetType().FullName, true, NoDependencies, dependencies => ExportDescriptor.Create( ( context, operation ) => instance, NoMetadata ) );
			}
		}
	}

	public interface IExportDescriptorProviderRegistry
	{
		void Register( ExportDescriptorProvider provider );
	}

	// https://github.com/dotnet/corefx/issues/6857
	public class TypeInitializingExportDescriptorProvider : ExportDescriptorProvider
	{
		public static TypeInitializingExportDescriptorProvider Instance { get; } = new TypeInitializingExportDescriptorProvider();

		TypeInitializingExportDescriptorProvider() {}

		public override IEnumerable<ExportDescriptorPromise> GetExportDescriptors( CompositionContract contract, DependencyAccessor descriptorAccessor )
		{
			InitializeTypeCommand.Instance.ExecuteWith( contract.ContractType );
			yield break;
		}
	}

	public class RegisteredExportDescriptorProvider : ExportDescriptorProvider, IExportDescriptorProviderRegistry
	{
		readonly ICollection<ExportDescriptorProvider> providers = new List<ExportDescriptorProvider>();

		public override IEnumerable<ExportDescriptorPromise> GetExportDescriptors( CompositionContract contract, DependencyAccessor descriptorAccessor )
		{
			var result = contract.ContractType == typeof(IExportDescriptorProviderRegistry) ?
				new ExportDescriptorPromise( contract, GetType().FullName, true, NoDependencies, dependencies => ExportDescriptor.Create( ( context, operation ) => this, NoMetadata ) ).ToItem()
				:
				providers.SelectMany( provider => provider.GetExportDescriptors( contract, new Accessor( descriptorAccessor, providers.Except( provider.ToItem() ).ToArray() ) ) );
			return result;
		}

		class Accessor : DependencyAccessor
		{
			readonly DependencyAccessor accessor;
			readonly IEnumerable<ExportDescriptorProvider> providers;

			public Accessor( DependencyAccessor accessor, IEnumerable<ExportDescriptorProvider> providers )
			{
				this.accessor = accessor;
				this.providers = providers;
			}

			protected override IEnumerable<ExportDescriptorPromise> GetPromises( CompositionContract exportKey )
			{
				var result = providers.SelectMany( provider => provider.GetExportDescriptors( exportKey, this ) ).Concat( accessor.ResolveDependencies( GetType().Name, exportKey, true ).Select( dependency => dependency.Target ) ).ToArray();
				return result;
			}
		}

		public void Register( ExportDescriptorProvider provider ) => providers.Add( provider );
	}
}