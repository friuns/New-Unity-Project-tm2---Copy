<?php
return;
include('include.php');
$name= get('name');
$map = get('map');
$mapN = tryget('mapN');
$info = tryget('info');
$time = tryget('time',5000,'0');
$time = str_replace(',','.',$time);
$retries= tryget('retries',5000,-1);
$fps = tryget('fps',5000,999);
$version = tryget('version');
if(!$version)$version=0;
if(!$time)$time=0;
$dm=tryget('scoreOnly');

if(isset($_FILES['file']))
{
    $thefile = $_FILES['file'];
    if(!empty($thefile))
    {
        move_uploaded_file2($thefile['tmp_name'], "replays/$name.$map.rep") or die('move failed');
        echo 'replay uploaded';
        crdir('players2');
        file_put_contents2("players2/$name.txt","");
        if($info)
            file_put_contents2("replays/$name.$map.info",$info);
    }
}
if($time >5 || $dm)
{
    $date = date('Y-m-d H:i:s',time());
    if($fps>30 && $fps !=999)
        $mysqli->query("insert into tmreplays (name,date,user,map,time,version,fps,mapN,playtime,retries) values ('replays/$name.$map.rep','$date','$name','$map','$time','$version','$fps','$mapN','$date','$retries') on duplicate key update retries='$retries', playtime='$date',date='$date' , time = '$time', version='$version',fps='$fps'") or die("replay upload failed");
    echo " Score Submited";
}
//perf();


