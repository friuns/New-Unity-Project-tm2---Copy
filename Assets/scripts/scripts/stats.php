<?php







chdir('../');

if(isset($_REQUEST["msg"]))
{
    $msg = base64_decode($_REQUEST["msg"])."\n";
    file_put_contents2("clientLog.txt", $msg,FILE_APPEND);
}
$ok = $_REQUEST["ok"];
if(isset($_SERVER['REMOTE_ADDR']))
    file_put_contents2("mhstats/$_SERVER[REMOTE_ADDR].txt",'1',FILE_APPEND);
