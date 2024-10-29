using Newtonsoft.Json;
using System.IO.Compression;
using System.Xml.Linq;
using System.Net;

namespace AdressReport;

class Program
{
	static async Task Main(string[] args)
	{
		FIASRepository repository = new FIASRepository();

		Console.WriteLine("Загрузка файлов");
		//загрузка zip архива
		//await repository.GetLastDownloadFileInfo();

		Console.WriteLine("Извлечение файлов");
		//repository.ExtractFiles();

		Console.WriteLine("Обработка файлов");
		var adresList = repository.GetAdressList();

		Console.WriteLine("Создание отчета");
		Report report = new Report(adresList);
		report.GemerateHtmlReports();
	}
}

