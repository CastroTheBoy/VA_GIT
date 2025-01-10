-- Check if trigger check for existing attribute value works in PLAYER_ATTRIBUTE
-- Expected result: 'ERROR:  Attribute_Value_Code = _ does not exist for Attribute_Type_Code = 1!'
INSERT INTO PLAYER_ATTRIBUTE
SELECT
	Player_Id,
	(SELECT MAX(Attribute_Value_Code) FROM DATA_ATTRIBUTE_VALUE WHERE Attribute_Type_Code = 1) + 1 AS Player_Attribute_Code,
	'hmmm',
	'2021-04-30',
	NULL
FROM
	(SELECT
		Player_Id
	FROM
		PLAYER
	LIMIT 1) AS P;