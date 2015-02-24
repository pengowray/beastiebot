using System;
using LsonLib;

namespace beastie {
	public class LuaStyleStemmer
	{

		// https://en.wiktionary.org/wiki/Module:la-utilities
		string ModuleLaUtilities = @"
	suffix = {
		['1st'] = { 
			['title']='[[Appendix:Latin first declension|First declension]]',
			['nom_sg']='Xa', ['gen_sg']='Xae', ['dat_sg']='Xae', ['acc_sg']='Xam', ['abl_sg']='Xā', ['voc_sg']='Xa', 
			['nom_pl']='Xae', ['gen_pl']='Xārum', ['dat_pl']='Xīs', ['acc_pl']='Xās', ['abl_pl']='Xīs', ['voc_pl']='Xae',
			['if_loc'] = { ['title'] = '[[Appendix:Latin first declension|First declension]] with locative.', 	
				['loc_sg']='Xae', ['loc_pl']='Xīs' }  },

		['1st-abus'] = { ['stem']='X',
			['title']='[[Appendix:Latin first declension|First declension]] with dative/ablative plural in {{term||-ābus|lang=la}}.',
			['nom_sg']='Xa', ['gen_sg']='Xae', ['dat_sg']='Xae', ['acc_sg']='Xam', ['abl_sg']='Xā', ['voc_sg']='Xa', 
			['nom_pl']='Xae', ['gen_pl']='Xārum', ['dat_pl']='Xābus', ['acc_pl']='Xās', ['abl_pl']='Xābus', ['voc_pl']='Xae' },

		['1st-Greek'] = { ['stem']='X',
			['title']='[[Appendix:Latin first declension|First declension]], Greek type.',
			['nom_sg']='Xē', ['gen_sg']='Xēs', ['dat_sg']='Xae', ['acc_sg']='Xēn', ['abl_sg']='Xē', ['voc_sg']='Xē', 
			['nom_pl']='Xae', ['gen_pl']='Xārum', ['dat_pl']='Xīs', ['acc_pl']='Xās', ['abl_pl']='Xīs', ['voc_pl']='Xae' },

		['1st-Greek-Ma'] = { 
			['title']='[[Appendix:Latin first declension|First declension]], Greek type masculine in {{term||-ās|lang=la}}.',
			['nom_sg']='Xās', ['gen_sg']='Xae', ['dat_sg']='Xae', ['acc_sg']='Xān', ['abl_sg']='Xā', ['voc_sg']='Xā', 
			['nom_pl']='Xae', ['gen_pl']='Xārum', ['dat_pl']='Xīs', ['acc_pl']='Xās', ['abl_pl']='Xīs', ['voc_pl']='Xae' },

		['1st-Greek-Me'] = { 
			['title']='[[Appendix:Latin first declension|First declension]], Greek type masculine in {{term||-ēs|lang=la}}.',
			['nom_sg']='Xēs', ['gen_sg']='Xae', ['dat_sg']='Xae', ['acc_sg']='Xēn', ['abl_sg']='Xē', ['voc_sg']='Xē', 
			['nom_pl']='Xae', ['gen_pl']='Xārum', ['dat_pl']='Xīs', ['acc_pl']='Xās', ['abl_pl']='Xīs', ['voc_pl']='Xae' },

		['1st-am'] = { 
			['title']='[[Appendix:Latin first declension|First declension]] with nominative singular in {{term||-am|lang=la}}.',
			['nom_sg']='Xam', ['gen_sg']='Xae', ['dat_sg']='Xae', ['acc_sg']='Xam', ['abl_sg']='Xā', ['voc_sg']='Xam', 
			['nom_pl']='Xae', ['gen_pl']='Xārum', ['dat_pl']='Xīs', ['acc_pl']='Xās', ['abl_pl']='Xīs', ['voc_pl']='Xae' },

		['2nd'] = {
			['title']='[[Appendix:Latin second declension|Second declension]]',
			['nom_sg']='Xus', ['gen_sg']='Xī', ['dat_sg']='Xō', ['acc_sg']='Xum', ['abl_sg']='Xō',  ['voc_sg']='Xe', 
			['nom_pl']='Xī', ['gen_pl']='Xōrum', ['dat_pl']='Xīs', ['acc_pl']='Xōs', ['abl_pl']='Xīs', ['voc_pl']='Xī',
			['if_loc'] = { ['if_loc'] = { ['title'] = '[[Appendix:Latin second declension|Second declension]] with locative.', 
				['voc_sg']='Xos', ['loc_sg']='Xī', ['loc_pl']='Xīs' }  },

		['2nd-N'] = {
			['title']='[[Appendix:Latin second declension|Second declension]] neuter.',
			['nom_sg']='Xum', ['gen_sg']='Xī', ['dat_sg']='Xō', ['acc_sg']='Xum', ['abl_sg']='Xō',  ['voc_sg']='Xum', 
			['nom_pl']='Xa', ['gen_pl']='Xōrum', ['dat_pl']='Xīs', ['acc_pl']='Xa', ['abl_pl']='Xīs', ['voc_pl']='Xa',
			['if_loc'] = { ['title'] = '[[Appendix:Latin second declension|Second declension]] neuter with locative.', 
				['loc_sg']='Xī', ['loc_pl']='Xīs' } },

		['2nd-er'] = {
			['title']='[[Appendix:Latin second declension|Second declension]], nominative singular in {{term||-er|lang=la}}.',
			['nom_sg']='W', ['gen_sg']='Xī', ['dat_sg']='Xō', ['acc_sg']='Xum', ['abl_sg']='Xō',  ['voc_sg']='W', 
			['nom_pl']='Xī', ['gen_pl']='Xōrum', ['dat_pl']='Xīs', ['acc_pl']='Xōs', ['abl_pl']='Xīs', ['voc_pl']='Xī' },

		['2nd-Greek'] = {
			['title']='[[Appendix:Latin second declension|Second declension]], Greek type',
			['nom_sg']='Xos', ['gen_sg']='Xī', ['dat_sg']='Xō', ['acc_sg']='Xon', ['acc_sg2']= 'Xum', ['abl_sg']='Xō', ['voc_sg']='Xe', 
			['nom_pl']='Xī', ['gen_pl']='Xōrum', ['dat_pl']='Xīs', ['acc_pl']='Xōs', ['abl_pl']='Xīs', ['voc_pl']='Xī' },
		
		['2nd-N-Greek'] = {
			['title']='[[Appendix:Latin second declension|Second declension]] neuter, Greek type',
			['nom_sg']='Xon', ['gen_sg']='Xī', ['dat_sg']='Xō', ['acc_sg']='Xon', ['abl_sg']='Xō', ['voc_sg']='Xon', 
			['nom_pl']='Xa', ['gen_pl']='Xōrum', ['dat_pl']='Xīs', ['acc_pl']='Xa', ['abl_pl']='Xīs', ['voc_pl']='Xa' },
		
		['2nd-ius'] = { -- todo: footnote for gen_sg
			['title']='[[Appendix:Latin second declension|Second declension]], nominative singular in {{term||-ius|lang=la}}.',
			['nom_sg']='Xïus', ['gen_sg']='Xïī', ['dat_sg']='Xiō', ['acc_sg']='Xium', ['abl_sg']='Xiō',  ['voc_sg']='Xī', 
			['nom_pl']='Xiī', ['gen_pl']='Xiōrum', ['dat_pl']='Xiīs', ['acc_pl']='Xiōs', ['abl_pl']='Xiīs', ['voc_pl']='Xiī'},
		
		['2nd-N-us'] = {
			['title']='[[Appendix:Latin second declension|Second declension]] neuter with nominative/accusative/vocative in {{term||-us|lang=la}}.',
			['nom_sg']='Xus', ['gen_sg']='Xī', ['dat_sg']='Xō', ['acc_sg']='Xus', ['abl_sg']='Xō', ['voc_sg']='Xus' } }
	}
	

 
";
		//			-- ['ref'] = ['1', 'acc_sg', 'or Xum']

		public LuaStyleStemmer() {
		}

		public void Test() {
			var lson = LsonVars.Parse(ModuleLaUtilities);
			Console.WriteLine(lson["suffix"]["1st"]);

			foreach (var s in lson["suffix"].Keys) {
				Console.WriteLine(s);
			}
		}
	}
}

