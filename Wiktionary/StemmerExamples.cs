//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
using System;
namespace beastie
{
	public class StemmerExamples
	{
		public static void PrintStemmerExamples ()
		{
			// Alice's 
			StemPrint("croqueting"); // croquet
			StemPrint("uglifying"); // uglifi
			StemPrint("uglification"); // uglif :(
			StemPrint("skurried"); // skurri
			StemPrint("skurry"); // skurri
			StemPrint("scurry"); // scurri
			StemPrint("barrowful"); // barrow
			StemPrint("rosetree"); // rosetre
			StemPrint("rosetrees"); // rosetre
			StemPrint("muchness"); // much
			StemPrint("eaglet"); // eaglet
			StemPrint("Edgar Atheling"); // Edgar Athel
			StemPrint("flamingoes"); // flamingo
			StemPrint("flamingos"); // flamingo
			StemPrint("flamingo"); // flamingo
			StemPrint("needn't"); // needn't
			
			// satanic verses
			StemPrint("snappishly"); // snappish
			StemPrint("snappish"); // snappish
			StemPrint("disorderment"); // disorder
			StemPrint("chinkings"); // chink
			StemPrint("jingoistically"); // jingoist
			StemPrint("postcarded"); // postcard
			StemPrint("softbellied"); // softbelli
			StemPrint("upliftingly"); // uplift
			StemPrint("organical"); // organ
			StemPrint("soliloquist"); // soliloquist
			StemPrint("unsentimentality"); // unsentiment
			StemPrint("antiquestions"); // antiquest
			StemPrint("antiquest"); // antiquest
			StemPrint("stimulators"); // stimul
			StemPrint("unattainability"); // unattain
			// wormily -> wormili 
			
			// frankenstein 
			StemPrint("outstript"); // outstript :(
			StemPrint("outstripped"); // outstrip
			StemPrint("retrod"); // retrod
			StemPrint("trod"); // trod
			StemPrint("trodded"); // trod 
			StemPrint("trodden"); // trodden
			StemPrint("treads"); // tread
			StemPrint("drivest");  // drivest (archaic)
			StemPrint("overweigh"); // overweigh
			StemPrint("lovedst"); // lovedst
			StemPrint("remembrancers"); // remembranc
			StemPrint("wretchedly"); // wretch
			StemPrint("earlier"); // earlier
			StemPrint("earliest"); // earliest
			StemPrint("priest"); // priest
			StemPrint("abortive"); // abort
			StemPrint("unborrow'd"); // unborrow'd
			StemPrint("unborrowed"); // unborrow
			//blamabl,195450,blamable
			//hoarser,164101,hoarser
			//ceni,210614,Cenis
			// one among the schiavi ognor frementi
			// I have seen the mountains of La Valais, and the Pays de Vaud
			
			//wuthering
			StemPrint("overforwardly"); // overforward
			StemPrint("merchantibility"); // merchant
			StemPrint("unenforceability"); // unenforc
			StemPrint("o'ered"); // o'er
			
			//GULLIVERS
			// quadrangular -> quadrangular
			// stript -> stript
			
			//wikipedia on stemming
			StemPrint("catty"); // catti
			StemPrint("argument"); // argument
			StemPrint("friendlies"); // friend
			StemPrint("dries");
			StemPrint("axes");
			StemPrint("axis");
			StemPrint("axe");
			StemPrint("universal");
			StemPrint("university");
			StemPrint("universe");
			StemPrint("alumnus");
			StemPrint("alumni");
			StemPrint("alumna");
			StemPrint("alumnae");
			StemPrint("markets");
			StemPrint("marketing");
		}
		
		static void StemPrint(string word) {
			//string word = "Seated";
			//string word = "croqueting"; // "Listlessness";
			
			//least to most aggressive: Porter, Snowball(Porter2), and Lancaster (Paice-Husk), 
			
			SF.Snowball.Ext.EnglishStemmer eng = new SF.Snowball.Ext.EnglishStemmer(); // Porter2
			//SF.Snowball.Ext.KpStemmer eng = new SF.Snowball.Ext.KpStemmer();
			//SF.Snowball.Ext.LovinsStemmer eng = new SF.Snowball.Ext.LovinsStemmer(); // Lovins (1968)
			//SF.Snowball.Ext.PorterStemmer eng = new SF.Snowball.Ext.PorterStemmer(); // Porter (1980)
			//Paice/Husk Stemmer: modified Lancaster
			//SF.Snowball.Ext. eng = new SF.Snowball.Ext.PorterStemmer();
			
			
			eng.SetCurrent(word);
			bool success = eng.Stem();
			if (success == true) {
				Console.WriteLine("{0} -> {1}", word, eng.GetCurrent());
			} else {
				Console.WriteLine("{0} -> {1} ({2})", word, eng.GetCurrent(), success);
			}
			
		}
	}
}

