using System;
using System.IO;
using System.IO.Compression;
using System.Net;


// sample code

namespace beastie
{
	class GzipReader
	{
		static byte[] Decompress(byte[] gzip)
		{
			using (GZipStream stream = new GZipStream(new MemoryStream(gzip),
			                                          CompressionMode.Decompress))
			{
				const int size = 4096;
				byte[] buffer = new byte[size];
				using (MemoryStream memory = new MemoryStream())
				{
					int count = 0;
					do
					{
						count = stream.Read(buffer, 0, size);
						if (count > 0)
						{
							memory.Write(buffer, 0, count);
						}
					}
					while (count > 0);
					return memory.ToArray();
				}
			}
		}
		
		static void Main(string[] args)
		{
			try
			{
				Console.WriteLine("*** Decompress web page ***");
				Console.WriteLine("    Specify file to download");
				Console.WriteLine("Downloading: {0}", args[0]);
				
				// Download url.
				using (WebClient client = new WebClient())
				{
					client.Headers[HttpRequestHeader.AcceptEncoding] = "gzip";
					byte[] data = client.DownloadData(args[0]);
					byte[] decompress = Decompress(data);
					string text = System.Text.ASCIIEncoding.ASCII.GetString(decompress);
					
					Console.WriteLine("Size from network: {0}", data.Length);
					Console.WriteLine("Size decompressed: {0}", decompress.Length);
					Console.WriteLine("First chars:       {0}", text.Substring(0, 5));
				}
			}
			finally
			{
				Console.WriteLine("[Done]");
				Console.ReadLine();
			}
		}
	}

}

