using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AdressReport
{
	internal class FIASRepository
	{
		/// <summary>
		/// Загрузка архива.
		/// </summary>
		/// <param name="filePath">Путь для сохранения файла.</param>
		/// <returns></returns>
		public async Task GetLastDownloadFileInfo()
		{
			string filePath = @"адреса\adressBase.zip";

			string downloadZipUrl;

			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(
					"http://fias.nalog.ru/WebServices/Public/GetLastDownloadFileInfo");

			using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
			{
				using (Stream stream = response.GetResponseStream())
				using (StreamReader sr = new StreamReader(stream))
				{
					string data = sr.ReadToEnd();
					dynamic d = JsonConvert.DeserializeObject(data);
					downloadZipUrl = d.GarXMLFullURL;
				}
			}

			using (HttpClient client = new HttpClient())
			{
				client.Timeout = TimeSpan.FromMinutes(30);

				using (HttpResponseMessage response = await client.GetAsync(downloadZipUrl, HttpCompletionOption.ResponseHeadersRead))
				{
					response.EnsureSuccessStatusCode();

					using (Stream contentStream = await response.Content.ReadAsStreamAsync(),
												fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
					{
						await contentStream.CopyToAsync(fileStream);
					}
				}
			}
		}

		/// <summary>
		/// Извлечь файлы.
		/// </summary>
		/// <param name="filePath">Путь к архиву.</param>
		/// <param name="targetFolder">Дирректория для извлечения файлов.</param>
		public void ExtractFiles()
		{
			try
			{
				string filePath = @"адреса\adressBase.zip";
				string extractionPath = @"адреса";

				using (ZipArchive zipFile = ZipFile.OpenRead(filePath))
				{
					foreach (ZipArchiveEntry entry in zipFile.Entries)
					{
						if (entry.Name.StartsWith("AS_OBJECT_LEVELS") || entry.Name.StartsWith("AS_ADDR_OBJ") && !entry.Name.StartsWith("AS_ADDR_OBJ_DIVISION")
							&& !entry.Name.StartsWith("AS_ADDR_OBJ_PARAMS") && !entry.Name.StartsWith("AS_ADDR_OBJ_TYPES"))
						{
							string destinationPath = Path.Combine(extractionPath, entry.FullName);

							Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));

							entry.ExtractToFile(destinationPath, overwrite: true);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}

		/// <summary>
		/// Получить коллекцию адресов.
		/// </summary>
		/// <returns>Список адресов.</returns>
		public List<(string Level, string Name, string TypeName)> GetAdressList()
		{
			string levels = @"адреса\AS_OBJECT_LEVELS_20241024_0842958b-87a6-493c-9ced-ec8a5507e8e0.XML";
			string adress = @"адреса\";

			Dictionary<string, string> levelNameDictionary = new Dictionary<string, string>();

			try
			{
				XDocument xmlDoc = XDocument.Load(levels);

				foreach (var element in xmlDoc.Descendants("OBJECTLEVEL"))
				{
					string level = element.Attribute("LEVEL").Value;
					string name = element.Attribute("NAME").Value;

					if (element.Attribute("ISACTIVE").Value == "true")
						levelNameDictionary.Add(level, name);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}

			string rootDirectory = @"D:\repository\reports\адреса";

			List<(string Level, string Name, string TypeName)> adressList = new List<(string Level, string Name, string TypeName)>();

			try
			{
				string[] xmlFiles = Directory.GetFiles(rootDirectory, "*AS_ADDR_OBJ*.xml", SearchOption.AllDirectories);

				foreach (string xmlFilePath in xmlFiles)
				{
					try
					{

						XDocument xmlDoc = XDocument.Load(xmlFilePath);

						foreach (var element in xmlDoc.Descendants("OBJECT"))
						{
							string isActive = element.Attribute("ISACTIVE").Value;
							string level = element.Attribute("LEVEL").Value;
							string name = element.Attribute("NAME").Value;
							string typeName = element.Attribute("TYPENAME").Value;

							if (isActive == "1" && level != null && name != null)
							{
								string levelName = levelNameDictionary[level];
								adressList.Add((Level: levelName, Name: name, TypeName: typeName));
							}
						}
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex.Message);
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
			return adressList;
		}
	}
}
