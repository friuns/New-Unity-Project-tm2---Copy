<?php
include_once("include.php");
$name = get("name");
$md5 = get("md5");
$msgid = get("msgid");
$password = tryget("password");
$vkpassword = tryget("vkPassword");
$md = md5($password);
//$check = "name='$name' && msgid!='$msgid'";
$check = "name='$name' && msgid!='$msgid' && (password = '$md' || vkpassword = '$vkpassword')";
$cp="dsajhdjehfjhghvcxu";
foreach($_REQUEST as $key=>$val)
{
    if($key != "md5")
        $cp = $cp.$key.$val;
}
//echo "$cp\n";
if(md5("$cp")!= $md5)
    die("md5 check");

function CheckAcc()
{
    return;
    global $check;
    global $msgid;
    global $mysqli;
    $q = "update tmusers set msgid='$msgid' where $check limit 1";
    $mysqli->query($q);
    if($mysqli->affected_rows==0)
    {
        file_put_contents2("log.txt","account check fail $q\n",FILE_APPEND);
        die ("account check fail");
    }
}