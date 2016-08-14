using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Sources;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Composition;
using Activator = DragonSpark.Activation.Activator;

namespace DragonSpark.Windows.Entity
{
	[Shared]
	class ActivationSource : IActivationSource
	{
		public static ISource<IActivationSource> Default { get; } = new Scope<IActivationSource>( Factory.ForGlobalScope( () => new ActivationSource( Activator.Instance.Get() ) ) );

		readonly IActivator activator;

		readonly Collection<Type> watching = new Collection<Type>();

		public ActivationSource( [Required]IActivator activator )
		{
			this.activator = activator;
		}

		public void Apply( object item )
		{
			var current = activator;
			var type = item.GetType();
			if ( !watching.Contains( type ) )
			{
				using ( new Context( watching, type ) )
				{
					var instance = current.Activate<object>( type );
					if ( instance != item )
					{
						instance.MapInto( item, Mappings.OnlyProvidedValues() );
					}
				}
			}
		}

		class Context : IDisposable
		{
			readonly IList items;
			readonly Type item;

			public Context( IList items, Type item )
			{
				this.items = items;
				this.item = item;
				items.Add( item );
			}

			public void Dispose() => items.Remove( item );
		}
	}
}