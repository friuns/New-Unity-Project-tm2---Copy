<?php

include("inc/include.php");

//mysql_query("TRUNCATE maps")or trigger_error("");

$f = file_get_contents("http://206.190.128.180/server/maps/getMaps2.php")or trigger_error("Failed to open maplist");
//$f = file_get_contents("http://maps.cs-portable.com/maplist.php")or trigger_error("Failed to open maplist");
//$f = $f."\r\n".file_get_contents("http://maps2.cs-portable.com/maplist.php") or trigger_error("Failed to open maplist");
$exp = explode("\r\n",$f);
$maps = Array();
$i=0;
foreach($exp as $map)
{
	if($map)
	{		
		$exp2 = explode("#",$map);			
		$name= strrchr($exp2[0], "/")? substr(strrchr($exp2[0], "/"), 1):$exp2[0];
		$maps[$i++]=$name;
		$hidden=$name != $exp2[0]?1:0;
		if(mysql_query("insert into maps2 (map,date,hidden) values ('$name','$exp2[1]',$hidden)"))
			echo "added $map\r\n";
	}
}
$rs = mysql_query("select * from maps2") or trigger_error("");
while($row = mysql_fetch_array($rs))
{
	if(!in_array("$row[map]",$maps))
	{
		echo $row['map']." removed \r\n";
		mysql_query("delete from maps2 where map='$row[map]'") or trigger_error("");
	}
}	
echo "refreshed\r\n";
