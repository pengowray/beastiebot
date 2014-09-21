using System;
using System.IO;
using System.Diagnostics; 
//using MySql.Data.MySqlClient;
using Ini;
using Microsoft.Win32;
using CommandLine;

namespace beastie
{
	public class RunMysqld {
		string _location = null; // if not found, defaults to: @"C:\Program Files (x86)\Catalogue of Life\2014 Annual Checklist"
		public string location {
			get {
				return ColLocation();
			}
		}

		string myiniFilename {
			get {
				return location + @"\server\mysql\my.ini";
			}
		}

		const string dquote = "\"";

		// TODO: auto try 2014 and 2013.
		public string year = "2014"; // should work with 2013 and 2014. Other years not tested.

		string _port = null;
		public string port {
			get {
				if (_port == null) {
					IniFile myIni = new IniFile(myiniFilename);
					_port = myIni.IniReadValue("mysqld", "port");
				}
				return _port;
			}
		}

		// test
		public static void Main (string[] args) {
			Console.OutputEncoding = System.Text.Encoding.Unicode;
			Console.OutputEncoding = System.Text.Encoding.UTF8;

			RunMysqld me = new RunMysqld ();
			Console.WriteLine("location: " + me.ColLocation() );
			me.StartDatabase();
		}

		public string ColLocation() {
			//TODO: if fail, try to find it in the start menu:
			// -- "C:\ProgramData\Microsoft\Windows\Start Menu\Programs\Catalogue of Life\2014 Annual Checklist.lnk"
			// -- use Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu)
			// -- and https://stackoverflow.com/questions/139010/how-to-resolve-a-lnk-in-c-sharp

			if (_location != null)
				return _location;

			string keyName1 = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Catalogue of Life "+ year + @" Annual Checklist";
			string keyName2 = @"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Catalogue of Life " + year + @" Annual Checklist";
			string valueName = "UninstallString";
			string defaultValue = @"C:\Windows\unvise32.exe C:\Program Files (x86)\Catalogue of Life\" + year + @" Annual Checklist\uninstal.log";

			string seperator1 = "unvise32.exe\" "; // in case it's quoted (not actually seen in the wild)
			string seperator2 = "unvise32.exe ";

			string regValue = Registry.GetValue(keyName1, valueName, null) as string;
			if (regValue == null) {
				regValue = Registry.GetValue(keyName2, valueName, null) as string;
			}

			if (regValue == null) {
				//TODO: How to allow user override?
				Console.WriteLine("Warning: Catalogue of Life installation not found. Using default location.");
				regValue = defaultValue;
			}

			int start = 0;
			int sepIndex = regValue.IndexOf(seperator1);
			start = sepIndex + seperator1.Length;
			if (sepIndex == -1) {
				sepIndex = regValue.IndexOf(seperator2);
				start = sepIndex + seperator2.Length;
			}

			if (sepIndex == -1) {
				Console.WriteLine("an error (sepIndex not found)");
				return null;
			}

			int endIndex = regValue.LastIndexOf(@"\uninstal.log");

			_location = regValue.Substring(start, endIndex - start);
			return _location;
		}

		// showCmdWindow = true is meant to show the cmd run in a cmd.exe window, but it doesn't work.
		public void StartDatabase(bool showCmdWindow = false) {

			string mysqldFile = location + @"\server\mysql\bin\mysqld";

			IniFile myIni = new IniFile(myiniFilename);

			string tmpdir = myIni.IniReadValue("mysqld", "tmpdir");

			if (tmpdir != null && tmpdir != "") {
				Directory.CreateDirectory(tmpdir); // If the directory already exists, this method does nothing.
			}

			string param = "--defaults-file=" + dquote + myiniFilename + dquote;

			ProcessStartInfo cmdsi = new ProcessStartInfo();
			if (showCmdWindow) {
				// Doesn't work
				cmdsi.FileName = "cmd.exe";
				cmdsi.Arguments = "/SK " + dquote + mysqldFile + dquote + " " + param;
			} else {

				cmdsi.FileName = mysqldFile;
				cmdsi.Arguments = param;
			}
			
			Console.WriteLine("Running: " + cmdsi.FileName + " " + cmdsi.Arguments);
			Process cmd = Process.Start(cmdsi);

			//TODO: show errors and ouput somewhere (cmd.StandardOutput, etc)
		}

		public void ShutdownDatabase() {
			string mysqldFile = location + @"\server\mysql\bin\mysqladmin";
			//string myiniFilename = location + @"\server\mysql\my.ini";

			string param = "-u root" +
				" --port=" + port +
				" shutdown "; 

			ProcessStartInfo cmdsi = new ProcessStartInfo ();
			cmdsi.FileName = mysqldFile;
			cmdsi.Arguments = param;
			Console.WriteLine("Running: " + cmdsi.FileName + " " + cmdsi.Arguments);
			Process cmd = Process.Start(cmdsi);
		}
	}

}
