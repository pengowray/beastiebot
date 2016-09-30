using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace beastie.beastieDB {
    class CreateBeastieDB {
//        string SqliteQuery = @"
/*
BEGIN TRANSACTION;
CREATE TABLE "WordsData" (
	`id`	INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	`dataimport`	INTEGER NOT NULL,
	`word`	TEXT NOT NULL,
	`wordRaw`	TEXT,
	FOREIGN KEY(`dataimport`) REFERENCES `DataImports`(`id`)
);
CREATE TABLE "WordList" (
	`id`	INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
	`word`	TEXT NOT NULL UNIQUE,
	`sources`	TEXT
);
CREATE TABLE "WordDistancesData" (
	`id`	INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	`dataimport`	INTEGER,
	`word`	TEXT NOT NULL,
	`data`	TEXT,
	FOREIGN KEY(`dataimport`) REFERENCES DataImports(id)
);
CREATE TABLE `NgramDepends` (
	`id`	INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
	`dataimport`	INTEGER,
	`lemma`	TEXT,
	`left`	TEXT,
	`right`	TEXT,
	`match_count`	INTEGER,
	`volume_count`	INTEGER,
	`broader_match_count`	INTEGER
);
CREATE TABLE "NaturesWindowData" (
	`id`	INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
	`dataimport`	INTEGER NOT NULL,
	`sci_name`	TEXT NOT NULL,
	`eng_common_name`	TEXT,
	`other_eng_names`	TEXT,
	FOREIGN KEY(`dataimport`) REFERENCES DataImports(id)
);
CREATE TABLE "DataImports" ( `id` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE, `fn` TEXT, `source` TEXT, `parameters` TEXT, `table` TEXT, `date_start` INTEGER, `date_complete` INTEGER, `date_del` INTEGER, `chksum` TEXT, `parent_dataimport` INTEGER, `log` TEXT, `continuing`	INTEGER NOT NULL DEFAULT 0, `last_item_done` TEXT, FOREIGN KEY(`parent_dataimport`) REFERENCES `DataImports`(`id`) );
CREATE INDEX `WordsData_word` ON `WordsData` (`word` ASC);
CREATE INDEX `WordsData_dataimport` ON `WordsData` (`dataimport` );
CREATE INDEX `WordDistanceData_word_index` ON `WordDistancesData` (`word` ASC);
COMMIT;
*/
    }
}
