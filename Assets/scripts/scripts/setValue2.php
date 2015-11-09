<?php
return;
include('includeAcc.php');

parse_str($_REQUEST["values"],$values);
$str = "";
foreach($values as $key=>$val)
{
    $key = mysqli_real_escape_string($mysqli,$key);
    $val = mysqli_real_escape_string($mysqli,$val);
    $str = $str."$key='$val', ";
}
//die($str);
$r = "update tmusers set $str msgid='$msgid' where $check";
$mysqli->query($r);
if(!mysqli_affected_rows($mysqli)) die("failed set value");
$date=  date("Y-m-d H:i:s");

//file_put_contents("players/$name/reputation.txt","$date $key : $values\n",FILE_APPEND);
echo "success";