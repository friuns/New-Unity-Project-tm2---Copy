<?php
return;
include("inc/include.php");
//$dump = str_replace(array("\r", "\r\n", "\n"),'',var_export($_GET,true).var_export($_POST,true));
//file_put_contents("mapScore.txt","$date $_SERVER[REQUEST_URI] $dump\r\n", FILE_APPEND);
$map = tryget("map",300);
if($map)
{
	$plays = get("plays");
	$botPlays= get("botPlays");	
	if($plays)
		mysql_query("update maps2 set plays=plays+'$plays', botPlays=botPlays+'$botPlays' where map = '$map'") or trigger_error("");
}

$nick = tryget("nick");
if($nick)
{
	$game = get("gameName",1000);
	mysql_query("update users set playing='$game',LastTimeOnline=Now() where name = '$nick'");
}
$server = tryget("server");
if($server)
	mysql_query("update servers set count=count+1, lastPlayed = Now() where ip = '$server'")or trigger_error("");
