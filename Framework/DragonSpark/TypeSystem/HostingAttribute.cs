using PostSharp.Patterns.Contracts;
using System;
using DragonSpark.Activation;

namespace DragonSpark.TypeSystem
{
	public abstract class HostingAttribute : Attribute, IFactoryWithParameter
	{
		readonly Func<object, object> inner;

		protected HostingAttribute( [Required]Func<object, object> inner )
		{
			this.inner = inner;
		}

		public bool CanCreate( object parameter ) => true;

		public object Create( object parameter ) => inner( parameter );
	}
}