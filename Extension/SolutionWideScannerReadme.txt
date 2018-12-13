Rules how to works solution-wide scanner

1) Scanner requires ALL items (tables, columns) to be exists in the query.
2) Every item is a wild card (? means only ONE any symbol, * means from zero to infinity any symbols).
3) Every wild card should contains at least one regular char, so wild cards '**', '??', '?*' etc are not allowed.
4) For column names wild card '*' IS ALLOWED and means 'Require using a star in a select query'. But it's better to check an appropriate checkbox.
5) For table names parenthesis [] does not have any influence on the results. You may or may not put your table name wild cards in these parenthesis.
6) SQL SERVER: any table name item may contains 4 parts: server name, database name, schema name, table name.
7) If your query contains 'raw_data' table name, and your wild card contains 'somescheme.raw_data', this query DOES match.
8) If your query contains 'somescheme.raw_data' table name, and your wild card contains 'raw_data', this query DOES match.
9) If your query contains 'dbo.raw_data' table name, and your wild card contains 'somescheme.raw_data', this query DOES NOT match.
10) Rules 7,8,9 are applied for server name and database name as usual.
11) Symbol case (upper case, lower case) does not have any influence on the results.
12) Scanner is able to find a temp table (create table #t (id int), wild card: #t), and table variable (declare @t table (id int), wild card: @t)
13) Scanner DOES NOT process UNSAVED changes! Please save all changes before process.

'Require using a star in a select query' means that SQL query MUST contain * in a column list, otherwise this query will removed from the results.

Examples

Suppose you have a query: select id from [dbo].[raw_data]
These table name wild cards will find this table:
	[dbo].[raw_data]
	[DBO].[RAW_DATA]
	dbo.[raw_data]
	[dbo].RAW_DATA
	dbo.raw_data
	DBO.RAW_DATA
	raw_data
	RAW_DATA
	r?w_da*
	R?W_DA*
	d?o.raw_da*
	[d*].[r?w_d?ta]
	*
	etc...

These column name wild cards will find this table:
	id
	[id]
	ID
	iD
	i?
	[i?]
	?D
	[?D]
	I*
	[I*]
	etc...

These table name wild cards will NOT find this table:
	[dbo.raw_data]
	dbo].[raw_data
	[dbo]?[raw_data]
	etc...

These column name wild cards will NOT find this table:
	[id
	id]
	?? (invalid wild card)
	*  (NOT A VALID WILD CARD, it means that query MUST contain a star symbol)


eof
