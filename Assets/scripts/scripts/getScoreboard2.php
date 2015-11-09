<?php
//include('cache.php');
//Load();
include('include.php');

$map = get('map');


if(tryget('dm'))
    $q = "select user,time,retries from tmreplays where map='$map' order by time DESC limit 100";
else
    $q = "select user,time,retries from (select * from tmreplays where map='$map' ORDER BY playtime DESC limit 1000) as T order by time ASC limit 100";

$result=query($q);
if($admin)
    echo $q;
while($row = $result->fetch_object()) {
	echo $row->user.';'.$row->time.';'.$row->retries. "\r\n";
}

//perf();
//Save();


//if(!$count) die();
//$a =glob("replays/*.$map.rep");
//$cnt = min($count,count($a));
//if($cnt==0) die();
//$k = array_rand($a,$cnt);
//if($cnt==1)
//	$k = array($k);


?>