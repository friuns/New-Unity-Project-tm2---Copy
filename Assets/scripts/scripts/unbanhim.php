<?php
include("include.php");
$user = get("user");
$r = query("delete from tmmh where name='$user'");
$a = "$user unbanned\n";
echo $a;
file_put_contents2("unban.txt",$a,FILE_APPEND);