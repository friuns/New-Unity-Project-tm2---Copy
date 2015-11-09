<?php
include('include.php');
include('cache2.php');Load(60*60);
$map = get('map');
$count= get('count');
$hard = tryget('hard');
$version = tryget('version');
if(!$version)$version=0;
$inner = "select name ,time from tmreplays where map = '$map' and version <= $version and fps>30 order by date desc";

$desk = $hard==0?'Desc':'';

if($hard==0 || $hard ==1)	
	$result= query($q="select name from ($inner limit 10) as asd order by time $desk limit $count") or die("error");
else
	$result= query($q="$inner limit $count") or die("error");

while($row = $result->fetch_object()) {
	echo $row->name. "\r\n";
}
if($admin)
    echo "$q";
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