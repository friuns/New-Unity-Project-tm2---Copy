<?php
header("Content-Type: text/plain");
include('inc/include.php');

//if(!$admin)
//include('inc/includeAcc.php');
$web = tryget("web");
$filterDate ='';

$nick = tryget("nick");
if($nick)
{
	$rs = mysql_query("select regdate,friends from users where name='$nick'") or trigger_error("");
	$row = mysql_fetch_array($rs);
	if(tryget("friends"))
	{
		$filterDate ='where (';
		foreach(explode("|",$row['friends']) as $f)
		{						
			$filterDate .= "name='$f' or ";			
		}
		$filterDate .="name='$nick') and ";
	}
	elseif (tryget("sameDay"))
	{				
		$filterDate="where regdate='$row[regdate]' and ";
	}
	//$filterDate=" DATEDIFF(regdate,'$row[regdate]')<2 and ";
}


$scoreboard= $web?$web:tryget("board");
if($web)
	echo "
<a href ='getScoreBoard.php?web=Top Players'>Top Players</a>
<a href ='getScoreBoard.php?web=Top Players Today'>Top Players Today</a>
<a href ='getScoreBoard.php?web=Top Medals'>Top Medals</a>
<a href ='getScoreBoard.php?web=Top Zombie Kills'>Top Zombie Kills</a>
<a href ='getScoreBoard.php?web=Top Countries'>Top Countries</a>
<a href ='getScoreBoard.php?web=Top Android Players'>Top Android Players</a>
<a href ='getScoreBoard.php?web=Top Clans'>Top Clans</a>
<br/>
<textarea cols='100' rows='50'>
";
if(!$scoreboard)
	echo "Top Players,Top Medals,Top Zombie Kills,Top Android Players,Top Clans,Flag Captures,Achievement";
else
{
	echo "$scoreboard\r\n";
	if($scoreboard=="kd" || $scoreboard=="Top Players")
	{
		$q = "select name,Kills,Deaths from users $filterDate order by Kills Desc limit 0,100";
		$rs = mysql_query($q) or trigger_error("");
	}
	/*if($scoreboard=="kdToday" || $scoreboard =="Top Players Today")
		$rs = mysql_query("select name,KillsToday,DeathsToday from users $filterDate hour(timediff(NOW() , LastTimeOnline)) < 24 order by KillsToday Desc limit 0,100") or trigger_error("");*/
	if($scoreboard == "Top Medals")
	{
		echo "get first 50 kills in match and you get gold medal\r\n";
		$rs = mysql_query("select name,goldMedals from users $filterDate order by goldMedals Desc limit 0,100") or trigger_error("");
	}
	/*if($scoreboard=="kills" || $scoreboard == "Top kills")
		$rs = mysql_query("select name,Kills,Deaths from users where  order by Kills Desc limit 0,100") or trigger_error("");*/
	if($scoreboard=="zombie" || $scoreboard == "Top Zombie Kills")
		$rs = mysql_query("select name,SurvivalKills from users $filterDate  order by SurvivalKills Desc limit 0,100") or trigger_error("");
	/*if($scoreboard == "Top Countries")
		$rs = mysql_query("select country,sum(Kills) as Kills ,sum(Deaths) as Deaths, sum(Kills)/(sum(Deaths)+10000) as score from users where  GROUP BY country order by sum(Kills)/(sum(Deaths)+10000) desc") or trigger_error("");	*/
	if ($scoreboard =="Top Android Players")
		$rs = mysql_query("select name,AndroidKills,AndroidDeaths from users $filterDate  order by AndroidKills Desc limit 0,100") or trigger_error("");
	
	if ($scoreboard =="Flag Captures")	
		$rs = mysql_query("select name,captures from users $filterDate  order by captures Desc limit 0,100") or trigger_error("");
	
	if ($scoreboard =="Achievement")	
	{
		if($filterDate)
			$rs = mysql_query("select name,awardsCount as achievements from users $filterDate  order by awardsCount Desc limit 0,100") or trigger_error("");
		else
		{
			echo "how many people reached this Achievement\r\n";		
			$rs = mysql_query("select * from awards order by count Desc limit 0,100") or trigger_error("");
		}
	}
	
	if($scoreboard=="CW" || $scoreboard == "Top Clans")
	{
		echo "Top clans in week\r\n";
		$rs = mysql_query("select clan,kills from clantable order by kills Desc limit 0,300")or trigger_error("");				
	}
	if(!isset($rs)) die("table not found");
	printTable($rs);
}
if($web) echo " </textarea>";

