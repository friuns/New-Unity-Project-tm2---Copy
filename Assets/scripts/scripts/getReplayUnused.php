<?php
include('cache.php');
Load();
include('include.php');

$map = get('map');
$count= get('count');
$hard = tryget('hard');
$version = tryget('version');
if(!$version)$version=0;
$inner = "select name ,time from tmreplays where name like '%.$map.rep' and version <= $version and fps>30 order by date desc";

$desk = $hard==0?'Desc':'';

if($hard==0 || $hard ==1)
    $q = "select name from ($inner limit 10) as table1 order by time $desk limit $count";
else
	$q = "$inner limit $count";

$result= $mysqli->query($q) or die("error");
while($row = $result->fetch_object()) {
	echo $row->name. "\r\n";
}
if($admin)
    echo $q;
//perf();
//Save();


//if(!$count) die();
//$a =glob("replays/*.$map.rep");
//$cnt = min($count,count($a));
//if($cnt==0) die();
//$k = array_rand($a,$cnt);
//if($cnt==1)
//	$k = array($k);
//foreach ($k as $key) 
//	echo "$a[$key]\r\n";

?>