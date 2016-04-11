using Ploeh.AutoFixture;
using PostSharp.Patterns.Contracts;
using System.Reflection;

namespace DragonSpark.Testing.Framework.Setup
{
	public class AutoData
	{
		public AutoData( [Required]IFixture fixture, [Required]MethodBase method )
		{
			Fixture = fixture;
			Method = method;
		}

		public IFixture Fixture { get; }

		public MethodBase Method { get; }
	}
}