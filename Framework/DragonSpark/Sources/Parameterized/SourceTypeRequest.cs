using DragonSpark.Activation.Location;
using System;
using System.Runtime.InteropServices;

namespace DragonSpark.Sources.Parameterized
{
	public class SourceTypeRequest : LocateTypeRequest
	{
		public SourceTypeRequest( Type runtimeType, [Optional]string name, Type resultType ) :  base( runtimeType, name )
		{
			ResultType = resultType;
		}

		public Type ResultType { get; }
	}
}