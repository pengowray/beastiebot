using System;

namespace beastie {
	public class DescendantsRequest
	{
		WiktionaryBot wikt = null;
		bool quick;

		public DescendantsRequest(string requestEpithet, bool rigorous = false) {
			quick = !rigorous;

			string searchStem = LatinStemmer.stemAsNoun(requestEpithet, false);

			wikt = WiktionaryBot.Instance();

			SpeciesSet speciesSet = new SpeciesSet();
			speciesSet.ReadCol();

			LatinStemBall ball = new LatinStemBall();

			foreach (Species sp in speciesSet.AllSpecies()) {

				string stem = LatinStemmer.stemAsNoun(sp.epithet, false);
				if (stem == searchStem) {
					bool found = wikt.ExistsMulLa(sp.ToString(), quick);
					ball.Add(sp, 1, !found);
				}

			}

			long score = ball.FirstDeclScore(false);
			Console.WriteLine("score: " + score);
			Console.WriteLine("things: " + ball.PrettyExamples());
			Console.WriteLine(ball.Descendants());

		}
	}
}