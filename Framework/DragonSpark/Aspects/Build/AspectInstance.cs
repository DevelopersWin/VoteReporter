using System;
using System.Reflection;
using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using PostSharp.Aspects;
using PostSharp.Extensibility;

namespace DragonSpark.Aspects.Build
{
	public sealed class AspectInstance<T> : ParameterizedSourceBase<MethodInfo, AspectInstance>
	{
		public static AspectInstance<T> Default { get; } = new AspectInstance<T>();
		AspectInstance() : this( AspectInstanceConstructor<T>.Default.Get ) {}

		readonly Func<MethodInfo, AspectInstance> constructorSource;

		public AspectInstance( Func<MethodInfo, AspectInstance> constructorSource )
		{
			this.constructorSource = constructorSource;
		}

		public override AspectInstance Get( MethodInfo parameter )
		{
			var method = parameter.AccountForGenericDefinition();
			var instance = constructorSource( method );
			var type = instance.Aspect?.GetType() ?? Type.GetType( instance.AspectConstruction.TypeName );
			var repository = PostSharpEnvironment.CurrentProject.GetService<IAspectRepositoryService>();
			var result = !repository.HasAspect( method, type ) ? instance : null;
			return result;
		}
	}
}