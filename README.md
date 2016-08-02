# beastiebot
Threatened species related tools for Wikipedia

To generate Beastie Bot Wikipedia pages — listed at https://en.wikipedia.org/wiki/User:Beastie_Bot/table — run:

`% beastie wikipedia-redlist`

Getting it to run is non-trivial as Beastie Bot was originally created as a personal project. There are a large number of dependencies, many of the unnecessary or only for minor functions. There are several databases that are required and none are automatically or dynamically downloaded. There are the remnants of several other related projects also in the source code, such as to find commonly used binomial 2-grams that are missing on Wikipedia or Wiktionary. 

I have not attempted to run Beastie Bot from only the source code so I may be overlooking some requirements.

Some things you will need are:

* The English Wikipedia (en.wikipedia.org) database downloaded in Xowa's sqlite format: http://xowa.org/home/wiki/Dashboard/Image_databases.html (`images.*` not required)

* CSV of Wikipedia Redlist, including species, subspecies, stocks & subpopulations, but NOT regional data (regional entries are identical to regular entries so cannot be automatically excluded)

A saved serach is here: (example file name: `export-57234-04_July_2016-everything_but_regional.csv`)

http://www.iucnredlist.org/search/saved?id=57234

* You'll have to set paths for everything, which are currently hard coded. Mainly (but not only) in the file called `FileConfig.cs`. Search the source code for `C:` and `D:` to find other locations.

* You might also need the list of species tagged as "Possibly Extinct". These are available in a PDF which you'll have to clean up after copy-pasting into a text file. Available 

Table 9 of http://www.iucnredlist.org/about/summary-statistics

Example of start of what the file should look like: (`possiblyextinct_04_July_2016.txt`)

```
# IUCN Red List version 2016-1: Table 9
# Last Updated: 30 June 2016
# Table 9: Possibly Extinct and Possibly Extinct in the Wild Species
# https://cmsdocs.s3.amazonaws.com/summarystats/2016-1_Summary_Stats_Page_Documents/2016_1_RL_Stats_Table_9.pdf
# via: http://www.iucnredlist.org/about/summary-statistics#Table_9
# Scientific name Common name IUCN Red List (2016) Category Year of Assessment Date last recorded in the wild
Bos sauveli Kouprey CR(PE) 2008 1969/70
Capromys garridoi Garrido's Hutia CR(PE) 2008 1989
Crateromys australis Dinagat Crateromys CR(PE) 2008 1975
Crocidura trichura Christmas Island Shrew CR(PE) 2008 1985
Crocidura wimmeri Wimmer's Shrew CR(PE) 2008 1976
```

* Capitalization fixes `caps.txt`

Available from a file named `caps_backup.txt` in this repo

or from https://en.wikipedia.org/wiki/User:Beastie_Bot/caps

* You might need a `Defaults.dat` file that looks something like this:

```
https://en.wikipedia.org|Your username|Your password
```

It works without logging in (files are written locally), but it still probably needs the file to try anyway. It will probably fail to log in even if the details are correct. The program should work with a blank or dummy username and password.

* Some other Beastie Bot functions require Catalogue of Life database, or Google 2-gram data, but not for just running `wikipedia-redlist` (hopefully).

If you'd like to help with developing Beastie Bot, some obvious starting points might be to clean up and remove the hard coded file locations, or to set it up to cache downloaded Wikipedia pages (so as not to require a local copy of the entire Wikipedia database). Also it would be good to switch over from the IUCN Redlist's CSV export and over to their newer JSON interface.

Hopefully this gives the barest basic information here so if I become indisposed then someone who's motivated enough to pick up the reigns can do so. But if I am still around and you want to help with the project then you can probably just ask for further help or information. There's also plenty of comments in the source.

Read more about Beastie Bot at:

https://en.wikipedia.org/wiki/User:Beastie_Bot

Pengo.