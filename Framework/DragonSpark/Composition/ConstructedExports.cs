using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Composition
{
	public sealed class ConstructedExports : ExportSourceBase<ConstructorInfo>
	{
		readonly IDictionary<Type, ConstructorInfo> constructors;
		
		public ConstructedExports( IDictionary<Type, ConstructorInfo> constructors ) : base( constructors.Keys )
		{
			this.constructors = constructors;
		}

		public ConstructorInfo Get( IEnumerable<ConstructorInfo> parameter ) => Get( parameter.Select( info => info.DeclaringType ).Distinct().Single() );
		public override ConstructorInfo Get( Type parameter ) => constructors[ parameter ];
	}
}