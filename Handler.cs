using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Yandex.Cloud.Functions;

namespace SudRfParser
{
	public class Handler : YcFunction<object, Task<IEnumerable<CourtInfo>>>
	{
		private const string _courtQuery = "?id=300&act=ajax_search&searchtype=sp&court_subj={0}&suds_subj=&var=true";

		public Handler()
		{
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
		}
		
		public async Task<IEnumerable<CourtInfo>> FunctionHandler(object _, Context context)
		{
			using var client = new HttpClient();
			client.BaseAddress = new Uri("https://sudrf.ru/index.php");
			var subjectList = await GetSubjectList(client);

			if (!subjectList.Any()) throw new Exception("Subjects not found");

			var response = new List<CourtInfo>();

			foreach (var subject in subjectList)
			{
				response.Add(new CourtInfo
				{
					subject = subject,
					child_courts = await GetCourtList(client, subject.id)
				});
			}

			return response;
		}

		private async Task<List<Subject>> GetSubjectList(HttpClient client)
		{
			var doc = new HtmlDocument();
			doc.Load(await client.GetStreamAsync("?id=300&var=true"), Encoding.GetEncoding(1251));
			var courtNodes = GetSelectWithName(doc.DocumentNode, "court_subj")
				?.SelectNodes("option");

			if (courtNodes == null) return Enumerable.Empty<Subject>().ToList();
			
			return courtNodes.Select(e => new Subject
			{
				id = int.Parse(e.GetAttributeValue("value", "0")),
				name = e.InnerText
			}).Where(e => e.id != 0).ToList();
		}

		private HtmlNode GetSelectWithName(HtmlNode node, string name)
		{
			if (node == null || !node.ChildNodes.Any()) return null;

			var result = node.SelectNodes("select")?.FirstOrDefault(n => n.GetAttributeValue("name", null) == name);

			if (result != null) return result;

			foreach (var childNode in node.ChildNodes)
			{
				result = GetSelectWithName(childNode, name);
				if (result != null) return result;
			}

			return null;
		}
		
		private async Task<List<Court>> GetCourtList(HttpClient client, int id)
		{
			var doc = new HtmlDocument();
			doc.Load(await client.GetStreamAsync(string.Format(_courtQuery, id)), Encoding.GetEncoding(1251));
			var result = doc.DocumentNode.SelectNodes("option");
			
			return result.Select(e => new Court
			{
				code = e.GetAttributeValue("value", "0"),
				name = e.InnerText
			}).Where(e => e.code != "0").ToList();
		}
	}
}