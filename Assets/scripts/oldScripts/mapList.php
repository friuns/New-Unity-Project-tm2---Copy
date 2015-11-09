<?php
include("inc/include.php");

if(tryget("list"))
{
	$rs = mysql_query("select * from maps");
	while($row = mysql_fetch_array($rs,MYSQL_ASSOC))
	{
		echo "$row[map],$row[tdmPlays],$row[classicPlays],$row[survivalPlays],$row[singlePlays]";
		echo "\r\n";
	}		
}

