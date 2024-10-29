using System.Text;

namespace AdressReport
{
	internal class Report
	{
		public List<(string Level, string Name, string TypeName)> Entries { get; set; }
		private string reportPath = @"адреса\отчет\";

		public void GemerateHtmlReports()
		{
			var groupedRecords = this.Entries.GroupBy(r => r.Level);
			
			Directory.CreateDirectory(this.reportPath);

			foreach (var group in groupedRecords)
			{
				DateTime now = DateTime.Now;
				string level = group.Key;

				string sanitizedLevel = level.Replace("/", "_");

				string filePath = Path.Combine(this.reportPath, $"{sanitizedLevel}.html");

				if (File.Exists(filePath))
					File.Delete(filePath);


				StringBuilder htmlBuilder = new StringBuilder();
				htmlBuilder.AppendLine("<!DOCTYPE html>");
				htmlBuilder.AppendLine("<html lang=\"en\">");
				htmlBuilder.AppendLine("<head>");
				htmlBuilder.AppendLine("<meta charset=\"UTF-8\">");
				htmlBuilder.AppendLine($"<title>LEVEL {level}</title>");
				htmlBuilder.AppendLine("</head>");
				htmlBuilder.AppendLine("<body>");


				htmlBuilder.AppendLine($"<h1>Отчет по добавленным адресным объектам за {now.ToString("d")}</h1>");
				htmlBuilder.AppendLine($"<h2>{level}</h2>");


				htmlBuilder.AppendLine("<table border='1'>");
				htmlBuilder.AppendLine("<tr><th>Тип объекта</th><th>Наименование</th></tr>");


				foreach (var record in group)
				{
					htmlBuilder.AppendLine($"<tr><td>{record.TypeName}</td><td>{record.Name}</td></tr>");
				}


				htmlBuilder.AppendLine("</table>");
				htmlBuilder.AppendLine("</body>");
				htmlBuilder.AppendLine("</html>");


				File.WriteAllText(filePath, htmlBuilder.ToString(), Encoding.UTF8);
			}
		}

		public Report(List<(string Level, string Name, string TypeName)> entries)
		{
			this.Entries = entries;
			this.reportPath = reportPath;
		}

	}
}
