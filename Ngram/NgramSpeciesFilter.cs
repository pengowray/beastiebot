using System;
using System.Net;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using Amazon.S3.Model;

// TODO: Include both parts of binomial capitalized, as per pre-1950 for certain binoms. e.g. Panthera Leo, Centaurea Cyanus, Ardea Thula
// TODO: Include sp. and spp. (and subsp, ssp, sspp?) e.g. Canis spp. (dot usually counted as a third word so can be excluded)
// TODO: stem common endings?

// TODO: get Bing number of results: https://stackoverflow.com/questions/4231663/google-search-api-number-of-results
// TODO: get Google trends: https://stackoverflow.com/questions/7805711/javascript-json-google-trends-api

namespace beastie {
	public class NgramSpeciesFilter : NgramReader
	{
		//private string outputFilename;
		private StreamWriter output;

		public StringSpeciesSet species;

		Regex speciesRegex;

		public NgramSpeciesFilter() {
		}

		public void LoadSpeciesSet(string filename) {
			//species = new SpeciesSet();
			species = new StringSpeciesSet();
			species.ReadCsv(filename);
		}

		public void OutputToConsole() {
			//output = new StreamWriter(Console.Out, true, Encoding.Unicode);

			output = new StreamWriter(Console.OpenStandardOutput());
			output.AutoFlush = true;
			Console.SetOut(output); // is this needed?
		}

		public void SetOutputFile(string outputFile, bool append = false) {
			output = new StreamWriter(outputFile, append, Encoding.UTF8);
			//output = File.CreateText(outputFile);

		}

		/*
		public void SetOutputS3(string keyName, bool append = false) {
			AWSConfigs.AWSRegion = "us-east";
			AWSConfigs.Logging = LoggingOptions.Log4Net; 

			string existingBucketName = "species-twograms";

			TransferUtility fileTransferUtility = new
				TransferUtility(new AmazonS3Client(Amazon.RegionEndpoint.USEast1));

			//Stream stream = ;
			fileTransferUtility.Upload(Stream, existingBucketName, keyName);
			//Console.WriteLine("Upload completed");
		}
		*/

		protected override void ProcessLine(string line) {

			//var ngram = new Ngram(line);

			//Lemma lemma = ngram.lemma;
			//if (lemma.isBinomialCase) {


			if (speciesRegex.IsMatch(line)) {
				var match = speciesRegex.Match(line);

				//output.WriteLine("possible match: " + line);
				//output.WriteLine("possible groups: " + match.Groups[1] + "--" + match.Groups[2]);

				if (species.Contains(match.Groups[1].Value, match.Groups[2].Value)) {
					output.WriteLine(line);
					Console.WriteLine("Found: {0}", line);
				}

				//string[] words = line.Split(new char[]{ ' ' });
				//if (species.Contains(words[0], words[1])) {
				//	output.WriteLine(line);
				//}
			} 
		}

		protected override void Start() {
			// characters found via species.AllFirstChars() and species.AllOtherChars().. allowing literal ?'s anywhere for some reason. Needs to still match a species anyway.
			speciesRegex = new Regex(@"^([A-ZÆßŒ0\?][a-zÆßáäåæçèéëíïóôöøüŒœ\?\-]+) ([a-zÆßáäåæçèéëíïóôöøüŒœ\?\-\,\.]{2,})\t");

			if (output == null) {
				Console.WriteLine("Warning: No output set.");
				return;
			}

		}
		protected override void End() {
			//if (output != Console.Out)
				//output.Close();
		}

		public override void Close() {
			output.Close();
		}

		// could be a static function, but whatever
		public void CopyFileToS3(string file) {
			IAmazonS3 client;
			string accessKeyID = @"AKIAJFRF7HA2YAM4TTYQ";
			string secretAccessKey = @"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";

			//user: ngrams-s3
			client = new AmazonS3Client(accessKeyID, secretAccessKey, Amazon.RegionEndpoint.USEast1);

			PutObjectRequest request = new PutObjectRequest()
			{
				BucketName = "species-twograms",
				Key = "ngrams-output.txt",
				FilePath = file
			};
			PutObjectResponse response = client.PutObject(request); 
		}
	}
 }

