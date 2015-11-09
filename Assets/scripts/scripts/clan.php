<?php
include('include.php');
$clan = get("clan");
$player = get("playerName");

crdir("clans");
$clanPath="clans/$clan.txt";
if(file_exists2($clanPath))
{
	$a=explode("\n",file_get_contents2($clanPath));
	if(in_array($player,$a)) die("user Already exists");		
}

file_put_contents2($clanPath,"$player\n",FILE_APPEND);


