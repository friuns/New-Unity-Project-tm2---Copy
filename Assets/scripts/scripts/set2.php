<?php
include("include.php");
$user = get("user");
$key=get("key");
$value=tryget("value");

$r = query("update tmusers set $key ='$value' where name='$user' limit 1");
$a = "$user $key $value\n";
echo $a;
file_put_contents2("medalsChange.txt",$a,FILE_APPEND);