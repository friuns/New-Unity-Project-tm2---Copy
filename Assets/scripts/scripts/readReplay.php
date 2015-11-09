<?php
/**
 * Created by JetBrains PhpStorm.
 * User: Administrator
 * Date: 23/02/14
 * Time: 3:25 AM
 * To change this template use File | Settings | File Templates.
 */
include("include.php");
$file = "replays2/269143/10000057.0.rep";
//$f = file_get_contents($file);
//$h = fopen($file,"r");$f=fread($h,5);fclose($h);
$f = file_get_contents($file,0,null,0,5);
$p = unpack("cchar/iint",$f);

echo $p["char"]==9 && $p["int"]==1241;

return;
if(file_exists2($file))
{
    $a = unpack("ci",file_get_contents2($file));
    file_put_contents("test.txt","$a[0]\n$a[1]\n",FILE_APPEND);
}
else
    file_put_contents("test.txt","not found\n",FILE_APPEND);