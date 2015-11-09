<?php
//include('cache.php');
//Load();
return;
include('cache2.php');Load(60*60);
include('include.php');
$map = get('map');
$flags = get('flags');

if(tryget('dm'))
    $q = "select * from tmreplays2 where map='$map' and flags ='$flags' order by time DESC limit 100";
else
    $q = "select * from (select * from tmreplays2 where map='$map' and flags='$flags' ORDER BY playtime DESC limit 1000) as T order by time ASC limit 100";

$result=query($q);
if($admin)
    echo $q;
while($row = $result->fetch_object()) {
    echo $row->playerName.';'.$row->time.';'.$row->retries. "\r\n";
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