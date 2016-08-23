using DragonSpark.Application;
using DragonSpark.Commands;
using DragonSpark.Extensions;
using DragonSpark.Sources;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DragonSpark.Setup
{
	public class AssignSystemPartsCommand : DecoratedCommand
	{
		public AssignSystemPartsCommand( params Type[] types ) : this( types.AsEnumerable() ) {}
		//public AssignSystemPartsCommand( ITypeSource types ) : this( types.AsEnumerable() ) {}
		public AssignSystemPartsCommand( IEnumerable<Type> types ) : this( SystemPartsFactory.Default.Get( types ) ) {}
		AssignSystemPartsCommand( SystemParts value ) : base( ApplicationParts.Default.Configured( value ).Cast<object>() ) {}
	}
}