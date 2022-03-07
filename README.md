# jpdb-cli
#### A command line interface client for jpdb. Made by a「上手」programmer.

### Commands
+ login [username] [password] - log into your jpdb account; necessary to use other commands
+ deckfromtext [deckID] [filepath] - parse file `filepath` and add the parsed words to deck with id `deckID` (you must manually create the deck first)
+ coverage [content type] - calculates percentages of the database you have certain coverages for
+ review - review overdue words, this feature has lots of bugs; use at your own risk
+ statistics - shows your word statistics
+ logout - log out of your account
+ help - shows all commands and their syntax
