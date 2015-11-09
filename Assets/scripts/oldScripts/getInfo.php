<?php
header("Content-Type: text/plain");
include('inc/include.php');
//if(!$admin)
//include('inc/includeAcc.php');

$nick= get("nick");



if($nick=="/help")
	exit(file_get_contents('http://dl.dropbox.com/u/8543448/Game/help.txt'));



$rs = mysql_query("
SELECT
TIMEDIFF(NOW(),LastTimeOnline) as lastSeen,
users.`name`,
users.`password`,
users.clan,
users.msgID,
users.friends,
users.country,
users.regdate,
users.lastLogin,
users.ipaddress,
users.Kills,
users.Deaths,
users.KillsWeek,
users.DeathsWeek,
users.KillsToday,
users.DeathsToday,
users.SurvivalKills,
users.LastTimeOnline,
users.AndroidKills,
users.AndroidDeaths,
users.Device,
users.goldMedals,
users.playing,
users.email,
users.`mod`,
users.money,
users.captures,
users.rankKills,
users.rankKillsToday,
users.rankGoldMedals,
users.rankSurvivalKills,
users.rankCaptures,
users.rankAndroidKills,
users.rankKillsDeaths,
users.votesLeft,
users.awards,
users.awardsCount
FROM
users
where `name`='$nick'") or trigger_error("user not found");
$row = mysql_fetch_array($rs) or die("User Not Found");

$convert = array(
	0.4,0.1,0.1,0.1,0.1,0.02,0.02,0.02,0.02,0.02,0.02,0.02,0.02,0.02,0.02,0.02,0.02,0.02,0.02,0.02,0.02,0.02,0.02,0.02,0.02,0.02,0.02,0.02,0.02,0.02,0.02,0.02,0.02,0.02,0.02,0.02,0.02,0.02,0.02,0.02,0.02,0.02,0.02,0.02,0.02,0.02,0.02,0.02,0.02,0.02,0.02,0.02,0.02,0.02,0.02,0.02,0.02,0.02,0.02,0.02,0.02,0.02,0.02
);
$ttl=0;
$kd2 = round($row['Kills'] /($row['Deaths']+1),2);
$kd = ($row['Deaths']+50) /($row['Kills']+1);
$cou = count($convert);
for ($i = 0; $i < $cou; $i++) {
	$ttl+=$convert[$i];		
	if($kd<$ttl)
		break;	
}
$class = $i+1;
//$class = ceil($row['rankKills']/100);

$clan = $row['clan'];
$mod = $row['mod'];
$money = $row['money'];
echo "$row[name]#$row[friends]#$class#";
echo "\r\nPlayer:      $row[name]";
if($row["clan"])
{
	$clan = str_replace('#','',$row['clan']);
	echo "\r\nClan:        $clan";
}
echo "\r\nCountry:     $row[country]";
if($row["Device"] != "" && $row["Device"] != "WindowsWebPlayer")
	echo "\r\nDevice:      $row[Device]";
echo "\r\nRank:        $class (KD ratio $kd2)";
echo "\r\nKills:       $row[Kills]";
echo "\r\nDeaths:      $row[Deaths]";
echo "\r\nBot Kills:   $row[SurvivalKills]";
echo "\r\nGold Medals: $row[goldMedals]";
echo "\r\nCaptures   : $row[captures]";
echo "\r\nMoney      : $row[money]";

echo "\r\nLast Time Online: $row[lastSeen] ago";
$playing = str_replace('#','',$row['playing']);
echo "\r\nLast Game Played: $playing\r\n";


if($row['AndroidKills'])
{	
	echo "\r\nKills on Android: $row[AndroidKills]";
	echo "\r\nDeaths on Android: $row[AndroidDeaths]";
}
	

echo "
Achievements
$row[awards]
";

/* 
echo "

...::Your Place on scoreboard::...
[ScoreBoard]               [Place]
Kills                        $row[rankKills]";
if($row['AndroidKills']) echo "
Kills On Android             $row[rankAndroidKills]";

Kills/deaths+1000            $row[rankKillsDeaths]
Kills 24h                    $row[rankKillsToday]
zombie kills                 $row[rankSurvivalKills]
Gold medals                  $row[rankGoldMedals]
Flag Captures                $row[rankCaptures]
Kills On Android             $row[rankAndroidKills]
 */


echo "#$clan#$mod#$money#$row[awards]#$row[Kills]#$row[Deaths]";

