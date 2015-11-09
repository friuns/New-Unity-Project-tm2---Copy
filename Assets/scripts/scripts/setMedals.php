<?php
include("include.php");
$user=get("user");
$medals=tryget("medals",4,0);
$reputation=tryget("reputation",4,0);
$xp=tryget("xp",5,0);
if(!$medals && !$reputation && !$xp) die("failed");
$s = '';
if($medals)
{
    $r = query("update tmusers set medals ='$medals' where name='$user' limit 1") or die("Error");
//    if(!mysqli_affected_rows($mysqli))
//        die("user not found");
    $s = "$user medals $medals\n";
}
if($reputation)
{
    $r = query("update tmusers set reputation ='$reputation' where name='$user' limit 1") or die("Error");
    //if(!mysqli_affected_rows($mysqli))
//        die("user not found");
    $s = $s."$user reputation $reputation\n";
}

if($xp)
{
    $r = query("update tmusers set xp ='$xp' where name='$user' limit 1") or die("Error");
    //if(!mysqli_affected_rows($mysqli))
//        die("user not found");
    $s = $s."$user xp $xp\n";
}

file_put_contents2("medalsChange.txt",$s,FILE_APPEND);
die($s);