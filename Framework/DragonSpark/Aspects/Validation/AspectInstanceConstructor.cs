using System.Reflection;
using DragonSpark.Sources.Parameterized;
using DragonSpark.TypeSystem;
using PostSharp.Aspects;
using PostSharp.Reflection;

namespace DragonSpark.Aspects.Validation
{
	sealed class AspectInstanceConstructor<T> : ParameterizedSourceBase<MethodInfo, AspectInstance>
	{
		public static AspectInstanceConstructor<T> Default { get; } = new AspectInstanceConstructor<T>();
		AspectInstanceConstructor() : this( new ObjectConstruction( typeof(T), Items<object>.Default ) ) {}

		readonly ObjectConstruction construction;

		public AspectInstanceConstructor( ObjectConstruction construction )
		{
			this.construction = construction;
		}

		public override AspectInstance Get( MethodInfo parameter ) => new AspectInstance( parameter, construction, null );
	}
}