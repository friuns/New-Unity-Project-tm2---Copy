
<?php
/*

include('include.php');
include('AESPost.php');
$source = $name= tryget('name');
$id = tryget('id');
$msg = tryget('msg');
$table =tryget('csp')? 'csmh':'tmmh';
$version = tryget('version');

$q='';
if($id)
    $q .= "id ='$id'";
//if($name)
//    $q .= " or  name='$name'";

//$q .= " or ip='$_SERVER[REMOTE_ADDR]'";

if($msg)
    file_put_contents2("clientLog2.txt", "$_SERVER[REMOTE_ADDR]$msg\n",FILE_APPEND);
else
{
    $r = $mysqli->query("select * from $table where $q limit 1");
    $f = $r->fetch_assoc();
    if($f)
    {
        //$source = $f['name'];
        //$msg = "busted".$msg;
        return 1;
    }
}

echo $msg?'1':'0';


if($msg)
    $mysqli->query("insert into $table (id,name,ip,msg,createTime,source,version) values('$id','$name','$_SERVER[REMOTE_ADDR]','$msg',now(),'$source','$version') on duplicate key update msg='$msg', cnt=cnt+1,updateTime=now(), source= '$source',version='$version'");



