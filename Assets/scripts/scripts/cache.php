<?php
include_once('includeOnce.php');

function Load($time = 60,$skipQuery=false)
{
    return;
    global $fcache;
    $fcache =strrchr($_SERVER['PHP_SELF'],'/');
    if(!$skipQuery)
    {
        $fcache =$fcache.http_build_query($_REQUEST);
        $fcache = preg_replace("/[^a-zA-Z0-9]/", "", $fcache );
    }
    $fcache = "cache/$fcache.txt";

	if(file_exists2($fcache) && time() -filemtime($fcache)<$time)
    {
		echo(file_get_contents2($fcache));
        if(isset($_REQUEST['a']))
        {
            global $timeStart;
            echo "cache:".(microtime(true)-$timeStart)*1000;
        }
        die();
    }
    ob_start();
    register_shutdown_function('Save');
}
function Save()
{
    return;
	global $abs;
    global $fcache;
	file_put_contents2("$abs\\$fcache",ob_get_contents());
}
