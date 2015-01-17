using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace beastie
{
	public struct Species {
		public readonly string genus;
		public readonly string epithet;

		public Species(string binomial) {
			string[] parts = binomial.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length == 2) {
				this.genus = parts[0];
				this.epithet = parts[1];
			} else {
				Console.Error.WriteLine("no good species: " + binomial);
				this.genus = "";
				this.epithet = "";
				//TODO: throw error
			}
		}

		public Species(string genus, string epithet) {
			this.genus = genus;
			this.epithet = epithet;
		}

		override public string ToString() {
			return string.Format("{0} {1}", genus, epithet);
		}

		public override int GetHashCode ()
		{
			//return string.Format("{0},{1}", genus, epithet).GetHashCode();
			return genus.GetHashCode() ^ epithet.GetHashCode();
		}

		public override bool Equals(System.Object obj)
		{
			if (obj == null)
				return false;

			if (!(obj is Species))
				return false;

			Species sp = (Species) obj;
			return (genus == sp.genus) && (epithet == sp.epithet);
		}
	}

}