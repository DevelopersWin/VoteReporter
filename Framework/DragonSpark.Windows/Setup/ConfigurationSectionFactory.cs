using DragonSpark.Extensions;
using PostSharp.Patterns.Contracts;
using System;
using System.Configuration;
using System.Linq;
using DragonSpark.Activation.Sources;

namespace DragonSpark.Windows.Setup
{
	public class ConfigurationSectionFactory<T> : SourceBase<T> where T : ConfigurationSection
	{
		readonly Func<string, object> factory;

		public ConfigurationSectionFactory() : this( ConfigurationManager.GetSection ) {}

		public ConfigurationSectionFactory( [Required]Func<string, object> factory )
		{
			this.factory = factory;
		}

		public override T Get()
		{
			var name = typeof(T).Name.SplitCamelCase().First().ToLower();
			var result = factory( name ) as T;
			return result;
		}
	}
}