using DragonSpark.Setup;
using Ploeh.AutoFixture;
using PostSharp.Patterns.Contracts;
using System.Reflection;

namespace DragonSpark.Testing.Framework.Setup
{
	public class AutoData : CompositeServiceProvider
	{
		public AutoData( [Required]IFixture fixture, [Required]MethodBase method/*, [Required] IProfiler profiler*/ ) : base(  )
		{
			Fixture = fixture;
			Method = method;
			// Profiler = profiler;
		}

		/*public AutoData Initialize()
		{
			Profiler.Start();
			return this;
		}*/

		public IFixture Fixture { get; }

		public MethodBase Method { get; }

		// public IProfiler Profiler { get; set; }

		public void Dispose()
		{
			// Profiler.Dispose();
		}
	}
}