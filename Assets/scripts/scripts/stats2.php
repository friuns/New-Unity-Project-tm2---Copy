<?php


include('include.php');
//file_put_contents("stats2Referer.txt",(isset($_SERVER["HTTP_REFERER"])? $_SERVER["HTTP_REFERER"]:"none")."\n",FILE_APPEND);
$source = $name= tryget('name');
$id = tryget('id');
$msg = tryget('msg');

if(tryget('csp'))
    die('0');
$table =tryget('csp')? 'csmh':'tmmh';
$version = tryget('version');

$q='';
if($id)
    $q .= "id ='$id'";
if($name)
    $q .= " or name='$name'";
if(!$name && !$id)
    die('0');
//$q .= " or ip='$_SERVER[REMOTE_ADDR]'";

if($msg)
{
	$msg = base64_decode($msg);
    file_put_contents2("clientLog.txt", "$_SERVER[REMOTE_ADDR]$msg\n",FILE_APPEND);
}
else
{
    $r = query("select * from $table where $q");
    $f = $r->fetch_assoc();

    if($f)
        die('1');
//    {
//        $source = $f['name'];
//        $msg = "busted".$msg;
    //}
}

if($msg)
    echo '1';

if($msg)
    query("insert into $table (id,name,ip,msg,createTime,source,version) values('$id','$name','$_SERVER[REMOTE_ADDR]','$msg',now(),'$source','$version') on duplicate key update msg='$msg', cnt=cnt+1,updateTime=now(), source= '$source',version='$version'");


