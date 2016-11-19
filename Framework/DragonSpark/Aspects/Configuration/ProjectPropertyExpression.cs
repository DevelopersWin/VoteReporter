namespace DragonSpark.Aspects.Configuration
{
	public class ProjectPropertyExpression : ProjectExpression, IProjectProperty
	{
		readonly string propertyName;

		public ProjectPropertyExpression( string propertyName ) : base( $"{{${propertyName}}}" )
		{
			this.propertyName = propertyName;
		}

		public string Get() => propertyName;
	}
}