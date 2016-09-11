using System;
using System.Reflection;
using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using PostSharp.Aspects;
using PostSharp.Extensibility;

namespace DragonSpark.Aspects.Validation
{
	public sealed class AspectInstanceFactory<T> : ParameterizedSourceBase<MethodInfo, AspectInstance>
	{
		public static AspectInstanceFactory<T> Default { get; } = new AspectInstanceFactory<T>();
		AspectInstanceFactory() : this( AspectInstanceConstructor<T>.Default.Get ) {}

		readonly Func<MethodInfo, AspectInstance> constructorSource;

		public AspectInstanceFactory( Func<MethodInfo, AspectInstance> constructorSource )
		{
			this.constructorSource = constructorSource;
		}

		public override AspectInstance Get( MethodInfo parameter )
		{
			var method = parameter.AccountForGenericDefinition();
			var repository = PostSharpEnvironment.CurrentProject.GetService<IAspectRepositoryService>();
			var instance = constructorSource( method );
			var type = instance.Aspect?.GetType() ?? Type.GetType( instance.AspectConstruction.TypeName );
			var result = !repository.HasAspect( method, type ) ? instance : null;
			return result;
		}
	}
}