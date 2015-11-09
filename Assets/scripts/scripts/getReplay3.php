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
$a["replays"] = Array();
$i=0;
$lastTime=99999;
for (; $row = $result->fetch_assoc();$i++ )
{
    if($i<$count)
    {

        $row["path"] = "replays2/$row[user]/$row[map].$row[flags].rep";
        $lastTime = min($lastTime, time() - strtotime($row["date"]));
        $a["replays"][$i] =$row;
    }
}
if($admin)
    echo "\nlt:$lastTime\n";
$a["dontUploadReplay"] = $i>=10 && $lastTime<50*60;
$a["count"]=$i;
echo json_encode($a);
//while($row = $result->fetch_assoc()) {
//    echo "replays2/$row[user]/$row[map].$row[flags].rep\r\n";
//}
