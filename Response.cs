using System.Collections.Generic;

namespace SudRfParser
{
	public class CourtInfo
	{
		public Subject subject { get; set; }
		public List<Court> child_courts { get; set; }
	}
	
	public class Subject
	{
		public int id { get; set; }
		public string name { get; set; }
	}

	public class Court
	{
		public string code { get; set; }
		public string name { get; set; }
	}
}