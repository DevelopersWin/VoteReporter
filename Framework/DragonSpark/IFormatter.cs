using DragonSpark.Sources.Parameterized;

namespace DragonSpark
{
	public interface IFormatter : IParameterizedSource<FormatterParameter, string>, IParameterizedSource<object, string> {}
}