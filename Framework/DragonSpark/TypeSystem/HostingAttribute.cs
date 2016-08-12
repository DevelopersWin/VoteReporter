using PostSharp.Patterns.Contracts;
using System;
using DragonSpark.Sources;

namespace DragonSpark.TypeSystem
{
	public abstract class HostingAttribute : Attribute, IParameterizedSource<object, object>
	{
		readonly Func<object, object> inner;

		protected HostingAttribute( [Required]Func<object, object> inner )
		{
			this.inner = inner;
		}

		public object Get( object parameter ) => inner( parameter );
	}
}