<?php


include('inc/include.php');
include('AESPost.php');
$source = $name= tryget('name');
$id = tryget('id');
$msg = tryget('msg');
$mhversion =tryget('csp');
$version = tryget('version');

$q='';
if($id)
    $q .= "id ='$id'";
//if($name)
//    $q .= " or name='$name'";

//$q .= "or ip='$_SERVER[REMOTE_ADDR]'";
$date=date('Y-m-d');


    $r = mysql_query("select * from csmh where $q limit 1");//and createTime < CURRENT_DATE - INTERVAL 2 DAY
    $f = mysql_fetch_array($r);

//    if($f)
//    {
//        $source = $f['name'];
//        $msg = "busted $msg $f[name] $f[id] $f[msg]";
//        $date = $f['createTime'];
//    }


echo $f?'one':'zero';

if($mhversion> 2 && $msg)
    mysql_query("insert into csmh (id,name,ip,msg,createTime,source,version,updateTime) values('$id','$name','$_SERVER[REMOTE_ADDR]','$msg','$date','$source','$version',now()) on duplicate key update msg='$msg', cnt=cnt+1,updateTime=now(), source= '$source',version='$version'");



