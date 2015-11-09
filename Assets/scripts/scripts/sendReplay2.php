<?php
include('cache2.php');Load(60*30,$_REQUEST['map']);
include('include.php');

$map = get('map');
$user = get('user');
$flags = get('flags');
$playerName = get('playerName');
$mapName = get('mapName');
//$mapN = tryget('mapN');
//$info = tryget('info');
$time = tryget('time',5000,'0');
$time = str_replace(',','.',$time);
$retries= tryget('retries',5000,-1);
$fps = tryget('fps',5000,999);
if($fps<30)return;
$version = get('version');
$version2=9999;

if(!$time)$time=0;
$dm=tryget('scoreOnly');
if(isset($_FILES['file']))
{
    $thefile = $_FILES['file'];
    if(!empty($thefile) && $thefile["size"]>500)
    {

        $f = file_get_contents($thefile['tmp_name'],0,null,0,5);
        $p = unpack("cchar/iint",$f);
        if($p["char"]==9 && ($p["int"]==1241 || $p["int"]==1234))
        {
            file_put_contents("replayBusted.txt","$_SERVER[HTTP_REFERER]\n",FILE_APPEND);
            die("Please update your version");
        }
        $dir="replays2/$user/";
        crdir($dir);
        $file="$dir/$map.$flags.rep";
        move_uploaded_file2($thefile['tmp_name'], $file) or die('move failed');
        chmod2($file,0777);
        perf("move file");
        echo 'replay uploaded';
        $version2=$version;
//        file_put_contents("players2/$name.txt","");
//        if($info)
//            file_put_contents("$dir/$map.info",$info);
    }


}
if($version2==9999)
    die("version");

if($time >5 || $dm)
{
    $date = date('Y-m-d H:i:s',time());
    if($fps>30 && $fps !=999)
        query("insert into tmreplays2 (date,user,flags,map,time,version,fps,playtime,retries,playerName,mapName) values ('$date','$user','$flags','$map','$time','$version2','$fps','$date','$retries','$playerName','$mapName') on duplicate key update retries='$retries', playtime='$date',date='$date' , time = '$time', version='$version2',fps='$fps'") or die("replay upload failed");
    echo " Score Submited";
}
perf('query');


