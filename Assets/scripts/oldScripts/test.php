<?php
include('include.php');
$clan = get("clan");
$player = get("playerName");

crdir("clans");
$clanPath="clans/$clan.txt";
if(file_exists($clanPath))
{
	$a=explode("\n",file_get_contents($clanPath));
	if(in_array($player,$a)) die("user Already exists");		
}

file_put_contents($clanPath,"$player\n",FILE_APPEND);


perf();