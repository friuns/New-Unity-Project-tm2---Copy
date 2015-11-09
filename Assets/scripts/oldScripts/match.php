<?php

include('inc/include.php');
include('inc/includeAcc.php');

//$joinGame = tryget('joinGame');
//if($joinGame)
//{
//	mysql_query("update users set playing='$joinGame' where name='$name'") or trigger_error("");
//	return;
//}
//$match= get('match',100);
$date= date('Y-m-d H:i:s',time());
$matchType = get('matchType');
$kills = get('kills2');
$deaths = get('deaths2');
$clan = tryget('clan');
$captures = tryget('captures');
$gold = tryget('gold');
$money = tryget('money5');
$medal = "goldMedals= goldMedals+'$gold',money='$money',captures=captures+'$captures'";
//mysql_query("INSERT INTO `match` (user,matchName,timeStarted,timeUpdated,kills,deaths,matchType,info) VALUES ('$name','$match','$date','$date',$kills,$deaths,'$matchType','$info') 
//	ON DUPLICATE Key Update timeUpdated ='$date',kills=kills+$kills,deaths=deaths+$deaths,info='$info'") or trigger_error("could not update match");

$s ="LastTimeOnline=Now(),playing=''";
if($clan && ($matchType == "Classic" || $matchType == "TDM"))
	mysql_query("insert into clantable (clan,kills,date) VALUES('$clan',1,NOW()) ON DUPLICATE Key Update kills=kills+1,date=NOW()")or trigger_error("");

if($matchType == "Classic" || $matchType == "TDM")
	mysql_query("update users set $s,$medal,Kills = Kills+'$kills', Deaths = Deaths+'$deaths', KillsToday = KillsToday+'$kills', DeathsToday = DeathsToday+'$deaths', KillsWeek = KillsWeek + '$kills', DeathsWeek = DeathsWeek + '$deaths'  where name='$name' and password = '$password'")or trigger_error("");
elseif ($matchType == "Survival" || $matchType == "SinglePlayer" || $matchType=="ZombieMatch")
	mysql_query("update users set $s,$medal,SurvivalKills = SurvivalKills+'$kills' where name='$name' and password = '$password'") or trigger_error("");


if(tryget('android2') && ($matchType == "Classic" || $matchType == "TDM"))
	mysql_query("update users set AndroidKills = AndroidKills+'$kills', AndroidDeaths = AndroidDeaths+'$deaths' where name='$name' and password = '$password'") or trigger_error("");




//mysql_query("Update kicks set Kills = Kills+'$kills', Deaths = Deaths+'$deaths' where ip='$_SERVER[REMOTE_ADDR]'");

