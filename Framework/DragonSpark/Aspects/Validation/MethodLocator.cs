using System;
using System.Linq;
using System.Reflection;
using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Aspects.Validation
{
	public sealed class MethodLocator : ParameterizedSourceBase<MethodLocator.Parameter, MethodInfo>
	{
		public static MethodLocator Default { get; } = new MethodLocator();
		MethodLocator() {}

		public override MethodInfo Get( Parameter parameter )
		{
			var mappings = parameter.Candidate.Adapt().GetMappedMethods( parameter.DeclaringType );
			var mapping = mappings.Introduce( parameter.MethodName, pair => pair.Item1.InterfaceMethod.Name == pair.Item2 && ( pair.Item1.MappedMethod.IsFinal || pair.Item1.MappedMethod.IsVirtual ) && !pair.Item1.MappedMethod.IsAbstract ).SingleOrDefault();
			var result = mapping.IsAssigned() ? mapping.MappedMethod : null;
			return result;
		}

		public struct Parameter
		{
			public Parameter( Type declaringType, string methodName, Type candidate )
			{
				DeclaringType = declaringType;
				MethodName = methodName;
				Candidate = candidate;
			}

			public Type DeclaringType { get; }
			public string MethodName { get; }
			public Type Candidate { get; }
		}
	}
}