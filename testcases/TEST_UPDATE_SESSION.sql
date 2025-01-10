-- Check that column Mob_Kill_Count, Death_Count or Experience_Gained in table PLAYER_SESSION can't be UPDATEd to a lower value than currently existing one
-- Expected result: 'ERROR:  Invalid player session update! Mob_Kill_Count, Death_Count or Experience_Gained attepmted a decrement.'
UPDATE PLAYER_SESSION
SET
	Death_Count = PS1.Death_Count,
	Mob_Kill_Count = PS1.Mob_Kill_Count,
	Experience_Gained = PS1.Experience_Gained
FROM
	PLAYER_SESSION PS,
	(SELECT
		Session_Id,
		Death_Count - 1 AS Death_Count,
		Mob_Kill_Count -1 AS Mob_Kill_Count,
		Experience_Gained - 1 AS Experience_Gained
	FROM
		PLAYER_SESSION
	WHERE
		Session_Id = 1) PS1
WHERE
	PS.Session_Id = PS1.Session_Id;