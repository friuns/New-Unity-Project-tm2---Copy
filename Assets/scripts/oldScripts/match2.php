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

$kills = get('kills');
$deaths = get('deaths');
$androidKills = get('androidKills');
$androidDeaths = get('androidDeaths');
$botKills = get('botKills');
$money = tryget('money5');
$awards = tryget('awards');
$awardsCount = tryget('awardsCount');
$clan = tryget('clan');
$captures = tryget('captures');
$gold = tryget('gold');

if($awards) $awards =",awards = CONCAT_WS('\n',awards,'$awards')";

//$medal = "";
//mysql_query("INSERT INTO `match` (user,matchName,timeStarted,timeUpdated,kills,deaths,matchType,info) VALUES ('$name','$match','$date','$date',$kills,$deaths,'$matchType','$info') 
//	ON DUPLICATE Key Update timeUpdated ='$date',kills=kills+$kills,deaths=deaths+$deaths,info='$info'") or trigger_error("could not update match");


if($clan)
	mysql_query("insert into clantable (clan,kills,date) VALUES('$clan',1,NOW()) ON DUPLICATE Key Update kills=kills+$kills,date=NOW()")or trigger_error("");

$rq = "update users set LastTimeOnline=Now(),
playing='',
goldMedals= goldMedals+'$gold',
money= GREATEST(money, '$money'),
captures=captures+'$captures',
Kills = Kills+'$kills',
Deaths = Deaths+'$deaths',
KillsToday = KillsToday+'$kills',
DeathsToday = DeathsToday+'$deaths',
KillsWeek = KillsWeek + '$kills',
DeathsWeek = DeathsWeek + '$deaths' ,
SurvivalKills = SurvivalKills+'$botKills',
AndroidKills = AndroidKills+'$androidKills',
AndroidDeaths = AndroidDeaths+'$androidDeaths',
awardsCount= awardsCount+'$awardsCount'
$awards
where name='$name' and password = '$password'";
mysql_query($rq)or trigger_error("");

if($awards)
{
	foreach(explode('\n',tryget('awards')) as $award)
		if($award)
		{
			$award = str_replace(array('\n', '\r'),'',$award);
			mysql_query("insert into awards (award) VALUES('$award') ON DUPLICATE Key Update count=count+1")or trigger_error("");
		}
}
