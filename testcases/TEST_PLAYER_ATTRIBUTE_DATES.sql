-- Check if trigger check for attribute start/end date validity works in PLAYER_ATTRIBUTE
-- Expected result: 'ERROR:  Attribute_End_Date has to be NULL or greater than Attribute_Start_Date'
INSERT INTO PLAYER_ATTRIBUTE
SELECT
	Player_Id,
	(SELECT MAX(Attribute_Value_Code) FROM DATA_ATTRIBUTE_VALUE WHERE Attribute_Type_Code = 1) AS Player_Attribute_Code,
	'hmmm',
	'2021-04-30',
	'2021-04-30'
FROM
	(SELECT
		Player_Id
	FROM
		PLAYER
	LIMIT 1) AS P;