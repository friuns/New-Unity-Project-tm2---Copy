<?php
include('cache.php');
Load();
include('include.php');

$clan  = get("clan");
$map = get("map");
$txt = file_get_contents2("clans/$clan.txt");
$ar = explode("\n",$txt);
foreach ($ar as $a)
{	
	$p="replays/$a.$map.info";
	if(file_exists2($p))
		echo "$a\n";
}
//Save();
//perf();
?>