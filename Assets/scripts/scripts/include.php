<?php
include_once('includeOnce.php');
//file_put_contents("urlLog.txt","\nhttp://$_SERVER[HTTP_HOST]$_SERVER[REQUEST_URI]",FILE_APPEND);
//if(isset($_SERVER['HTTP_REFERER'])) file_put_contents("referer.txt","\n$_SERVER[HTTP_REFERER]",FILE_APPEND);

if(!isset($skipConnect))
{
    if($_SERVER["REMOTE_ADDR"] == "127.0.0.1")
        $mysqli = mysqli_connect('206.190.128.180', 'friuns2', 'er54s4', "friuns");
    else
        $mysqli= mysqli_connect('127.0.0.1', 'root', 'er54s44', 'friuns');

    if (!$mysqli)
        trigger_error( "Couldn't connect to MySQL" );

    register_shutdown_function('shutdown');
}

function shutdown()
{
	global $mysqli;
    if(isset($mysqli))
	{		 
		$er = mysqli_error($mysqli);
		if($er)
            trigger_error($er);
	}
	perf('',false);
}
function userfile($name, $check=false)
{
    $file = "players/$name/prefs.txt";
    if(!file_exists2($file))
    {
        crdir("players/$name/");
        file_put_contents2($file,'');
        if($check)
            file_put_contents("prefsNotFound.txt","$name\n",FILE_APPEND);
    }
}
function query($q)
{
    global $lastQuery;
    global $admin;
    global $mysqli;
    $lastQuery=$q;
    if ($admin)
        echo "$q\n";
    return $mysqli->query($q);
}
function query2($q)
{
    global $admin;
    global $mysqli;
    if ($admin)
        echo "$q\n";
    $timeStart= microtime(true);
    $r = $mysqli->query($q);

    $ts = (microtime(true)-$timeStart)*1000;
    $mydt = date('Y-m-d H:0:0');
    if($ts>0 && isset($mysqli))
        $mysqli->query("insert into tmperf (file,date,time) values('$q','$mydt',$ts)  on DUPLICATE key update requests = requests+1, avrg = ((avrg*10)+$ts)/11, time = $ts, score=score+$ts") or trigger_error("");
    return $r;
}

