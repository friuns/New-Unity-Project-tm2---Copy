<?php
include('include.php');

//$_SERVER['HTTP_X_FORWARDED_FOR']? $_SERVER['HTTP_X_FORWARDED_FOR']:
$ip = $_SERVER['REMOTE_ADDR'];

$map = get('map');
$user = get('user');
$mapname = tryget('mapname');
$rate  = get('rate',1);

file_put_contents("rate.txt","$_SERVER[REMOTE_ADDR] $map $rate $user $mapname\n",FILE_APPEND);
//$name  = "usermaps/$map.map";
//echo "update tmusermaps set ip='$ip', rate= (rate*cnt +$rate)/(cnt+1) , cnt=cnt+1 where ip!='$ip' and name='usermaps/$map.map'";
//$r = $mysqli->query("update tmusermaps set ip='$ip', rate= (rate*LEAST(15,cnt) +$rate)/(LEAST(15,cnt)+1) , cnt=cnt+1 where name='$name' and ip != '$ip'");
$s = "update tmusermaps set ip='$ip', rate= (rate*cnt +$rate)/(cnt+1), rate2= (rate*cnt +1+$rate)/(cnt+2),cnt=LEAST(15,cnt+1) where id='$map' and ip != '$ip'";
$r = query($s);

if(mysqli_affected_rows($mysqli)==0)
    echo "Already voted";
    //$mysqli->query("Insert into tmusermaps (name, date, user, map, version, tested, cnt, rate, ip) VALUES ('$name', '2000-01-01','tmrace','$map','666',0,1,'$rate','$ip') ON DUPLICATE key update cnt=cnt");
