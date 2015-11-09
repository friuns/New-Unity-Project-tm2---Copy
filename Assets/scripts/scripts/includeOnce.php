<?php
ini_set('display_errors','On');
header("Content-Type: text/plain");
chdir('../');
$lastQuery='';
$timeStart2= $timeStart1= microtime(true);
$abs = getcwd();
date_default_timezone_set('UTC');

$perfMsg="";
set_error_handler("myErrorHandler");


$admin = isset($_REQUEST['a']);


//$link = mysql_connect('127.0.0.1:3306', 'root', 'er54s44');


function myErrorHandler($errno, $errstr, $errfile, $errline) {

    global $mysqli;
    global $abs;
    $e = new Exception();
    $dump = http_build_query($_REQUEST);
    if(isset($mysqli))
        $errstr = $errstr.mysqli_error($mysqli);
    $mydt = date('Y-m-d H:i:s');
    $error = "$mydt $_SERVER[PHP_SELF]?$dump\r\n$errline:".basename($errfile)."\r\n$errstr\r\n".str_replace(str_replace('/','\\',$_SERVER['DOCUMENT_ROOT']), '',$e->getTraceAsString())."\r\n";
    //$er = substr($error,0,400);

    file_put_contents2("$abs/log.txt",$error,FILE_APPEND);
    die("<$error>");
}
$perfq = Array();
function perf($s='',$local=true)
{
    if($local)return;
    global $timeStart2;
    global $timeStart1;
    global $perfMsg;
    if($local)
        $timeStart= & $timeStart2;
    else
        $timeStart= & $timeStart1;
    global $lastQuery;
    global $mysqli;
    global $perfq;

    $ts = (microtime(true)-$timeStart)*1000;
    $timeStart=microtime(true);
    $mydt = date('Y-m-d H:0:0');

    if($ts>0 && isset($mysqli))
    {
        $dump = $ts>500?",max='$ts', ov=ov+1,dump='".mysqli_real_escape_string($mysqli,$lastQuery)."'":false?",dump='".mysqli_real_escape_string($mysqli,$lastQuery)."'":"";
        $s = ($local?count($perfq):'').$s;
        $q = "insert into tmperf (file,date,time) values('$_SERVER[SCRIPT_NAME] $s$perfMsg','$mydt','$ts')  on DUPLICATE key update requests = requests+1, avrg = ((avrg*10)+'$ts')/11, time = '$ts', score=score+'$ts' $dump";

        array_push($perfq,$q);
    }
    if(isset($_REQUEST['a']))
        echo "\nperf $s:$ts";
    if(!$local)
    {
        foreach ($perfq as $q) {
            $mysqli->query($q) or trigger_error("");
        }
    }

}



function crdir($name)
{
    if(!file_exists2($name))
    {
        mkdir2($name,0777,true);
        return true;
    }
    return false;
}


function tryget2($var,$default = "")
{
    tryget($var,50000,$default);
}

function tryget($var,$limit = 50000,$default = "")
{
    if(!isset($_REQUEST[$var]))
        return $default;
    else
        return get($var,$limit);
}
function get($var,$limit = 50000)
{
    if (!isset($_REQUEST[$var])) die("value not set");
    return check($_REQUEST[$var],$limit);
}
function check($val,$limit)
{
    //if(strlen($val)>$limit)
    global $mysqli;
    $val = substr($val,0,$limit);
    if(isset($mysqli))
        $val = mysqli_real_escape_string($mysqli,$val);
    return $val;
}

function move_uploaded_file2 ($filename, $destination) {
    return move_uploaded_file ($filename, strtolower($destination));
}
function mkdir2 ($pathname, $mode = 0777, $recursive = false) {
    return mkdir(strtolower($pathname), $mode, $recursive);
}
function file_exists2 ($filename) {
    return file_exists(strtolower($filename));
}
function file_put_contents2 ($filename, $data, $flags = null) {
    return file_put_contents(strtolower($filename), $data, $flags);
}
function file_get_contents2 ($filename, $flags = null) {
    return file_get_contents(strtolower($filename), $flags) ;
}
function filesize2 ($filename) {
    return filesize(strtolower($filename));
}
function chmod2 ($filename, $mode) {
    return chmod(strtolower($filename),$mode);
}