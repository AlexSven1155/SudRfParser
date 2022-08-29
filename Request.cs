namespace SudRfParser
{
	public class Request
	{
		public string login { get; set; }
		public string password { get; set; }
		public int limit { get; set; }
		public int offset { get; set; }
	}
}