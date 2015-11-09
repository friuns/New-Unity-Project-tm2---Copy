<?php

include('include.php');
include('cache2.php');Load(60*60);
$map = get('map');
$flags = get('flags');
$count= get('count');
$hard = tryget('hard');
$version = tryget('version');
if(!$version)$version=0;
$inner = "select * from tmreplays2 where map = '$map' and flags = '$flags' order by date desc";

$desk = $hard==0?'Desc':'';
perf('start');
if($hard==0 || $hard ==1)
    $result= query($q="select * from ($inner limit 30) as asd order by time $desk limit 10") or die("error");
else
    $result= query($q="$inner limit 10") or die("error");

if($admin)
    echo "$q";
//$a = $result->fetch_all(MYSQLI_ASSOC);
//foreach ($a as $row) {
//    $row["path"] = "replays2/$row[user]/$row[map].$row[flags].rep\r\n";
//}
$a= Array();
for ($i=0; $row = $result->fetch_assoc();$i++ )
{
    $row["path"] = "replays2/$row[user]/$row[map].$row[flags].rep";
    $a[$i] =$row;
}
echo json_encode($a);
perf('end');
//while($row = $result->fetch_assoc()) {
//    echo "replays2/$row[user]/$row[map].$row[flags].rep\r\n";
//}
