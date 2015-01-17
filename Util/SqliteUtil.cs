using System;
using System.Data;
using System.Data.SQLite;
using System.IO;

public static class SqliteUtil
{

	public static byte[] GetBytes(this SQLiteDataReader reader, string indexName)
	{
		return GetBytes(reader, reader.GetOrdinal(indexName));
	}

	public static byte[] GetBytes(this SQLiteDataReader reader, int i)
	{
		const int CHUNK_SIZE = 2 * 1024;
		byte[] buffer = new byte[CHUNK_SIZE];
		long bytesRead;
		long fieldOffset = 0;
		using (MemoryStream stream = new MemoryStream())
		{
			while ((bytesRead = reader.GetBytes(i, fieldOffset, buffer, 0, buffer.Length)) > 0)
			{
				stream.Write(buffer, 0, (int)bytesRead);
				fieldOffset += bytesRead;
			}
			return stream.ToArray();
		}
	}
}

