<?php
//include('cache.php');
//Load();
return;
include('include.php');

$map = get('map');





$result=query("select user,time from (select * from tmreplays where mapN='$map' and time>5 ORDER BY date DESC limit 100) as T order by time limit 50") or trigger_error();

while($row = $result->fetch_object()) {
	echo $row->user.';'.$row->time. "\r\n";
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