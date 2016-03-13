namespace DragonSpark.Testing.Objects.Composition
{
	public interface IBasicService
	{
		string HelloWorld( string message );
	}

	public interface IParameterService
	{
		object Parameter { get; }
	}

	class ParameterService : IParameterService
	{
		public ParameterService( Parameter parameter )
		{
			Parameter = parameter;
			parameter.Message = "Assigned by ParameterService";
		}

		public object Parameter { get; }
	}
}