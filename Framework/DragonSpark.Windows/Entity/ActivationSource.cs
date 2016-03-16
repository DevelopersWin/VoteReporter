using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Setup.Registration;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections;
using System.Collections.ObjectModel;

namespace DragonSpark.Windows.Entity
{
	[Persistent]
	class ActivationSource : IActivationSource
	{
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
			var canActivate = current.CanActivate( type );
			if ( canActivate && !watching.Contains( type ) )
			{
				using ( new Context( watching, type ) )
				{
					var instance = current.Activate( type );
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

			public void Dispose()
			{
				items.Remove( item );
			}
		}
	}
}